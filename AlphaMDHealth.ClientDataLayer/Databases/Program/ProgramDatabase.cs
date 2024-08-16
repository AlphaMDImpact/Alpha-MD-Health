using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public class ProgramDatabase : BaseDatabase
	{
		/// <summary>
		/// Insert or update record in the database
		/// </summary>
		/// <param name="programData">Program data for save into DB</param>
		/// <returns>Operation Status</returns>
		public async Task SaveProgramsAsync(PatientProgramDTO programData)
		{
			await SqlConnection.RunInTransactionAsync(transaction =>
			{
				foreach (ProgramModel program in programData.Programs)
				{
					transaction.InsertOrReplace(program);
				}
			}).ConfigureAwait(false);
		}

		/// <summary>
		/// Insert or update record in the database
		/// </summary>
		/// <param name="programData">Program data for save into DB</param>
		/// <returns>Operation Status</returns>
		public async Task SavePatientProgramsAsync(PatientProgramDTO programData)
		{
			var patientPrograms = programData.PatientPrograms?.OrderBy(x => x.LastModifiedON)?.ToList();
			await SqlConnection.RunInTransactionAsync(transaction =>
			{
				foreach (PatientProgramModel program in patientPrograms)
				{
					SavePatientProgram(program, transaction);
				}
			}).ConfigureAwait(false);
		}

		/// <summary>
		/// Get Programs which are not synced to server
		/// </summary>
		/// <param name="programData">Program object </param>
		/// <returns>Program data</returns>
		public async Task GetPatientProgramsToSyncWithServerAsync(PatientProgramDTO programData)
		{
			programData.PatientPrograms = await SqlConnection.QueryAsync<PatientProgramModel>(
				"SELECT * FROM PatientProgramModel WHERE IsSynced = 0"
			).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets list of programs
		/// </summary>
		/// <param name="programData">Object for program list </param>
		/// <returns>List of program with operation status reference object</returns>
		public async Task GetPatientProgramsAsync(PatientProgramDTO programData)
		{
			bool IsPatient = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0) == (int)RoleName.Patient;
			string limit = programData.RecordCount > 0 ? $" LIMIT {programData.RecordCount}" : "";
			string condition = programData.IsPatientAllowedForProgramSelection ? "LEFT " : "";
			string pColor = IsPatient
				? "IFNULL(B.ProgramGroupIdentifier, A.ProgramGroupIdentifier)"
				: "A.ProgramGroupIdentifier";

            programData.OrganizationPrograms = await SqlConnection.QueryAsync<OptionModel>("SELECT  ProgramID AS OptionID, " +
                "Name AS OptionText FROM " +
                "ProgramModel WHERE IsActive = 1 AND IsPublished = 1 "
                ).ConfigureAwait(false);

            programData.Programs = await SqlConnection.QueryAsync<ProgramModel>(
				"SELECT DISTINCT  B.PatientProgramID, A.ProgramID, A.Name, A.AllowSelfRegistration, A.IsPublished, " +
				$"B.PatientID, B.ProgramEntryPoint AS ProgramEntryPoint, B.IsActive, B.AddedON, {pColor} AS ProgramGroupIdentifier " +
				"FROM ProgramModel A " +
				$"{condition}JOIN PatientProgramModel B ON A.ProgramID = B.ProgramID AND B.PatientID = ? " +
				"WHERE A.IsActive = 1 AND A.IsPublished = 1 " +
				$"ORDER BY A.Name COLLATE NOCASE ASC {limit}", programData.SelectedUserID
			).ConfigureAwait(false);

			programData.Programs = programData.Programs?.GroupBy(x => x.ProgramID)
									.Select(x => x.FirstOrDefault(y => y.IsActive) ?? x.FirstOrDefault())?.ToList();

			if (!IsPatient)
			{
				programData.Programs = programData.Programs.Where(x => x.IsActive).ToList();
			}

			programData.TrackerTypes = await SqlConnection.QueryAsync<OptionModel>(
				"SELECT DISTINCT B.TrackerID AS OptionID, B.TrackerName AS OptionText, A.ProgramID AS ParentOptionID " +
				"FROM ProgramTrackerModel A " +
				"LEFT JOIN TrackersModel B ON  A.TrackerID= B.TrackerID " +
				"WHERE B.TrackerID > 0 AND A.ProgramID > 0 AND A.IsActive = 1 "
			);

			if (programData.PatientProgram?.PatientProgramID > 0)
			{
				programData.PatientProgram = await SqlConnection.FindWithQueryAsync<PatientProgramModel>(
					"SELECT A.ProgramID, A.ProgramEntryPoint, A.PatientProgramID, A.PatientID, A.TrackerID, A.EntryTypeID, A.EntryDate, " +
					"IFNULL(B.ProgramGroupIdentifier, A.ProgramGroupIdentifier) AS ProgramGroupIdentifier, B.Name " +
					"FROM PatientProgramModel A " +
					"LEFT JOIN ProgramModel B ON A.ProgramID = B.ProgramID " +
					"WHERE A.PatientProgramID = ?",
					programData.PatientProgram.PatientProgramID
				).ConfigureAwait(false);
			}
		}

		private void SavePatientProgram(PatientProgramModel program, SQLiteConnection transaction)
		{
			string setPatientProgramID = "";
			string condition = $"WHERE PatientProgramID = {program.PatientProgramID} AND ProgramID = {program.ProgramID} AND PatientID = {program.PatientID}";
			var existingData = transaction.FindWithQuery<PatientProgramModel>($"SELECT * FROM PatientProgramModel {condition}");
			if (existingData == null && program.PatientProgramID > 0)
			{
				condition = $"WHERE PatientProgramID < 1 AND ProgramID = {program.ProgramID} AND PatientID = {program.PatientID}";
				existingData = transaction.FindWithQuery<PatientProgramModel>($"SELECT * FROM PatientProgramModel {condition}");
				setPatientProgramID = $"PatientProgramID = {program.PatientProgramID},";
			}
			if (existingData == null)
			{
				if (program.PatientProgramID < 1)
				{
					long temPatientProgrampID = GetPatientPrograms(transaction);
					program.PatientProgramID = temPatientProgrampID < 0 ? temPatientProgrampID - 1 : -1;
				}
				//LibGenericMethods.LogData($"INSERT PatientProgramModel== PatientProgramID:{program.PatientProgramID}, ProgramID:{program.ProgramID}, PatientID:{program.PatientID}, ProgramGroupIdentifier:{program.ProgramGroupIdentifier}, ProgramEntryPoint:{program.ProgramEntryPoint}, IsActive:{program.IsActive}, IsSynced:{program.IsSynced}, AddedON:{program.AddedON}, EntryTypeID:{program.EntryTypeID}, EntryDate:{program.EntryDate}, TrackerID:{program.TrackerID}");
				if (Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0) == (int)RoleName.Patient)
				{
				    SaveProgramFromPatientLogin(true, program, setPatientProgramID, condition, transaction);
				}
				else
				{
					transaction.Execute(
						"INSERT INTO PatientProgramModel " +
						"(PatientProgramID, ProgramID, PatientID, ProgramGroupIdentifier, ProgramEntryPoint, EntryTypeID, EntryDate, TrackerID, IsActive, IsSynced, AddedON, LastModifiedON) " +
						"VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"
						, program.PatientProgramID, program.ProgramID, program.PatientID, program.ProgramGroupIdentifier, program.ProgramEntryPoint, program.EntryTypeID
						, program.EntryDate, program.TrackerID, program.IsActive, program.IsSynced, program.AddedON, program.LastModifiedON
					);
				}
			}
			else if (program.LastModifiedON == null || existingData.LastModifiedON <= program.LastModifiedON)
			{
				//LibGenericMethods.LogData($"UPDATE PatientProgramModel== PatientProgramID:{program.PatientProgramID}, ProgramID:{program.ProgramID}, PatientID:{program.PatientID}, ProgramGroupIdentifier:{program.ProgramGroupIdentifier}, ProgramEntryPoint:{program.ProgramEntryPoint}, IsActive:{program.IsActive}, IsSynced:{program.IsSynced}, AddedON:{program.AddedON}, EntryTypeID:{program.EntryTypeID}, EntryDate:{program.EntryDate}, TrackerID:{program.TrackerID}");
				if (Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0) == (int)RoleName.Patient)
				{
					SaveProgramFromPatientLogin(false, program, setPatientProgramID, condition, transaction);
				}
				else
				{
					transaction.Execute(
					  "UPDATE PatientProgramModel " +
					  $"SET {setPatientProgramID} ProgramGroupIdentifier = ?, ProgramEntryPoint = ?, EntryTypeID = ?, " +
					  $"EntryDate = ?, TrackerID = ?, IsActive = ?, IsSynced = ?, AddedON = ?, LastModifiedON = ? {condition}"
					  , program.ProgramGroupIdentifier, program.ProgramEntryPoint, program.EntryTypeID
					  , program.EntryDate, program.TrackerID, program.IsActive, program.IsSynced, program.AddedON, program.LastModifiedON);
				}
			}
			else if (!string.IsNullOrWhiteSpace(setPatientProgramID))
			{
				//LibGenericMethods.LogData($"UPDATE2 PatientProgramModel== PatientProgramID:{program.PatientProgramID}, ProgramID:{program.ProgramID}, PatientID:{program.PatientID}, ProgramGroupIdentifier:{program.ProgramGroupIdentifier}, ProgramEntryPoint:{program.ProgramEntryPoint}, IsActive:{program.IsActive}, IsSynced:{program.IsSynced}, AddedON:{program.AddedON}, EntryTypeID:{program.EntryTypeID}, EntryDate:{program.EntryDate}, TrackerID:{program.TrackerID}");
				transaction.Execute(
				  $"UPDATE PatientProgramModel SET {setPatientProgramID} {condition}");
			}
		}

		/// <summary>
		/// Saves a program from a patient login.
		/// </summary>
		/// <param name="IsInsert">Whether the program is being inserted or updated.</param>
		/// <param name="patientProgramModel">The patient program model.</param>
		/// <param name="setPatientProgramID">The set patient program identifier.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="transaction">The transaction.</param>
		public void SaveProgramFromPatientLogin(bool IsInsert, PatientProgramModel patientProgramModel, string setPatientProgramID, string condition, SQLiteConnection transaction)
		{
			if (IsInsert)
			{
				transaction.Execute(
					"INSERT INTO PatientProgramModel " +
					"(PatientProgramID, ProgramID, PatientID, ProgramGroupIdentifier, ProgramEntryPoint, EntryTypeID, EntryDate, TrackerID, IsActive, IsSynced, AddedON, LastModifiedON) " +
					"VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"
					, patientProgramModel.PatientProgramID, patientProgramModel.ProgramID, patientProgramModel.PatientID, patientProgramModel.ProgramGroupIdentifier, 0, 814
					, null, null, patientProgramModel.IsActive, patientProgramModel.IsSynced, patientProgramModel.AddedON, patientProgramModel.LastModifiedON
					);
			}
			else
            {
				transaction.Execute(
				  "UPDATE PatientProgramModel " +
				  $"SET {setPatientProgramID} ProgramGroupIdentifier = ?, " +
				  $"IsActive = ?, IsSynced = ?, AddedON = ?, LastModifiedON = ? {condition}"
				  , patientProgramModel.ProgramGroupIdentifier, patientProgramModel.IsActive, patientProgramModel.IsSynced, patientProgramModel.AddedON, patientProgramModel.LastModifiedON);
			}
		}

		/// <summary>
		/// save Patients SharedPrograms
		/// </summary>
		/// <param name="programData">Object for program list </param>
		/// <returns>List of program with operation status reference object</returns>
		public async Task SavePatientsSharedProgramsAsync(ProgramDTO programData)
		{
			var sharedPrograms = programData.PatientsSharedPrograms?.OrderBy(x => x.LastModifiedON)?.ToList();
			await SqlConnection.RunInTransactionAsync(transaction =>
			{
				foreach (PatientsSharedProgramsModel program in sharedPrograms)
				{
					//LibGenericMethods.LogData($"PatientsSharedProgramsModel== PatientCareGiverID:{program.PatientCareGiverID}, PatientProgramID:{program.PatientProgramID}, IsActive:{program.IsActive}, LastModifiedON:{program.LastModifiedON}");
					if (transaction.FindWithQuery<PatientsSharedProgramsModel>(
						"SELECT 1 FROM PatientsSharedProgramsModel WHERE  PatientCareGiverID=? AND PatientProgramID=?"
						, program.PatientCareGiverID, program.PatientProgramID) == null)
					{
						transaction.Execute("INSERT INTO PatientsSharedProgramsModel(PatientCareGiverID, PatientProgramID, IsActive, LastModifiedON) VALUES(?, ?, ?, ?) "
						 , program.PatientCareGiverID, program.PatientProgramID, program.IsActive, program.LastModifiedON);
					}
					else
					{
						transaction.Execute("UPDATE PatientsSharedProgramsModel SET IsActive=?, LastModifiedON = ? WHERE PatientCareGiverID=? AND PatientProgramID=?"
							, program.IsActive, program.LastModifiedON, program.PatientCareGiverID, program.PatientProgramID);
					}
				}
			}).ConfigureAwait(false);
		}

		/// <summary>
		/// update is sync status and PatientProgramID
		/// </summary>
		/// <param name="oldPatientProgramID">temprarry patient Program id</param>
		/// <param name="newPatientProgramID">actual patient Program id</param>
		/// <returns><update issynced and Patient bill id/returns>
		public async Task UpdatePatientProgramIDAsync(long oldPatientProgramID, long newPatientProgramID)
		{
			if (oldPatientProgramID > 0)
			{
				await SqlConnection.RunInTransactionAsync(transaction =>
				{
					transaction.Execute("UPDATE PatientProgramModel SET IsSynced = 1 WHERE PatientProgramID = ?", newPatientProgramID);
				}).ConfigureAwait(false);
			}
			else
			{
				//If patient program with same newPatientProgramID exist and marked as isActive = 0 then mark it IsActive = 1 
				bool IfProgramWithPatientProgramIDExist = false;
				await SqlConnection.RunInTransactionAsync(transaction =>
				{
					IfProgramWithPatientProgramIDExist = transaction.ExecuteScalar<bool>("SELECT 1 FROM PatientProgramModel WHERE  PatientProgramID = ? AND IsActive = 0", newPatientProgramID);
				}).ConfigureAwait(false);
				if (IfProgramWithPatientProgramIDExist)
				{
					await SqlConnection.RunInTransactionAsync(transaction =>
					{
						transaction.Execute("UPDATE PatientProgramModel SET IsSynced = 1, IsActive = 1  WHERE PatientProgramID = ?", newPatientProgramID);
						transaction.Execute("DELETE FROM PatientProgramModel WHERE PatientProgramID = ?", oldPatientProgramID);
					}).ConfigureAwait(false);
				}
				else
				{
					await SqlConnection.RunInTransactionAsync(transaction =>
					{
						transaction.Execute("UPDATE PatientProgramModel SET IsSynced = 1, PatientProgramID = ? WHERE PatientProgramID = ?", newPatientProgramID, oldPatientProgramID);
					}).ConfigureAwait(false);
				}
			}
		}

		/// <summary>
		/// Save PatientPrograms to local database 
		/// </summary>
		/// <param name="programData">Reference object of PatientProgram</param>
		/// <returns>operation status</returns>
		public async Task SavePatientProgramDataAsync(PatientProgramDTO programData)
		{
			if (GenericMethods.IsListNotEmpty(programData.PatientPrograms))
			{
				PatientProgramModel patientprogram = programData.PatientPrograms.FirstOrDefault();
				if (await SqlConnection.FindWithQueryAsync<PatientProgramModel>(
					"SELECT 1 FROM PatientProgramModel " +
					"WHERE IsActive = 1 AND PatientID = ? AND ProgramID = ? " + " AND PatientProgramID <> ?"
					, patientprogram.PatientID, patientprogram.ProgramID, patientprogram.PatientProgramID
				).ConfigureAwait(false) != null)
				{
					programData.ErrCode = ErrorCode.DuplicateData;
				}
				else
				{
					await SavePatientProgramsAsync(programData).ConfigureAwait(false);
				}
			}
		}

		private long GetPatientPrograms(SQLiteConnection transaction)
		{
			return transaction.ExecuteScalar<long>("SELECT PatientProgramID FROM PatientProgramModel WHERE IsSynced = 0 ORDER BY PatientProgramID ASC LIMIT 1");
		}

		/// <summary>
		/// delete file from database
		/// </summary>
		/// <param name="fileID">ID to delete files and documents </param>
		/// <returns>Operation status in FilesDTO</returns>
		public async Task DeletePatientProgramAsync(long patientProgramID)
		{
			await SqlConnection.RunInTransactionAsync(transaction =>
			{
				transaction.Execute("UPDATE PatientProgramModel SET IsActive = 0, IsSynced = 0 WHERE PatientProgramID = ?", patientProgramID);
			}).ConfigureAwait(false);
		}
	}
}