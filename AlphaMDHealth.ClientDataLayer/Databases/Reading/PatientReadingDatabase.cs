using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public partial class ReadingDatabase : BaseDatabase
	{
		/// <summary>
		/// Gets reading for add/edit page
		/// </summary>
		/// <param name="readingsData">Reference object containing the readingID to be fetched as well as to return reading to be edited and metadata</param>
		/// <returns>Reading to be edited and its metadata</returns>
		public async Task GetPatientReadingAsync(PatientReadingDTO readingsData)
		{
			SQLiteAsyncConnection sqlConnection = SqlConnection;
			TaskModel taskData = await new PatientTaskDatabase().GetTaskByPatientTaskIDAsync(readingsData.PatientTaskID);
			if (readingsData.PatientReadingID == Guid.Empty)
			{
				readingsData.ListData = new List<PatientReadingUIModel>();
				foreach (var metadata in readingsData.ChartMetaData)
				{
					var existingReading = await sqlConnection.FindWithQueryAsync<PatientReadingUIModel>(
						$"{GetPatientReadingsQuery(string.Empty)} ORDER BY A.ReadingDateTime DESC, SequenceNo ASC LIMIT 1"
						, metadata.ReadingID, readingsData.SelectedUserID);
					if (existingReading != null)
					{
						readingsData.ListData.Add(existingReading);
					}
				}
			}
			else
			{
				short readingID = readingsData.ReadingID;	
			    readingID = await FetchDataForQuestionnaireQuestionAnswerAsync(readingsData, sqlConnection, taskData, readingsData.ReadingID);	
				var readingParentID = readingsData.ChartMetaData.FirstOrDefault(x => x.ReadingID == readingID)?.ReadingParentID ?? 0;
				var metadata = readingsData.ChartMetaData.Where(x =>
					(readingParentID > 0 && x.ReadingParentID == readingParentID) ||
					(x.ReadingID == readingID)
				)?.ToList();

				string readingIDs = GenericMethods.IsListNotEmpty(metadata)
					? string.Join(",", metadata.Select(x => x.ReadingID))
					: $"{readingID}";
				if (taskData?.TaskType == ResourceConstants.R_QUESTIONNAIRE_KEY)
				{
					var readingParentIdForCheck = readingsData.ChartMetaData.FirstOrDefault(x => x.ReadingID == readingsData.ReadingID)?.ReadingParentID ?? 0;
					if (readingParentIdForCheck == 0)// For grouped reading get all chiled data on next and prev button
					{
						readingsData.ListData = await sqlConnection.QueryAsync<PatientReadingUIModel>(
						$"{GetPatientReadingsQuery(readingIDs)} " +
						"AND A.ReadingDateTime = (SELECT ReadingDateTime FROM PatientReadingModel WHERE PatientReadingID = ?) " +
						"ORDER BY A.ReadingDateTime DESC, SequenceNo ASC "
						, readingsData.SelectedUserID, readingsData.PatientReadingID);
					}
					else // For chiled reading get only that chiled data 
					{
						readingsData.ListData = await sqlConnection.QueryAsync<PatientReadingUIModel>(
					  $"{GetPatientReadingsQuery(readingID.ToString())} " +
					   "AND A.ReadingDateTime = (SELECT ReadingDateTime FROM PatientReadingModel WHERE PatientReadingID = ?) " +
					  "ORDER BY A.ReadingDateTime DESC, SequenceNo ASC "
					   , readingsData.SelectedUserID, readingsData.PatientReadingID);
					}
				}
				else
				{
					readingsData.ListData = await sqlConnection.QueryAsync<PatientReadingUIModel>(
					$"{GetPatientReadingsQuery(readingIDs)} " +
					"AND A.ReadingDateTime = (SELECT ReadingDateTime FROM PatientReadingModel WHERE PatientReadingID = ?) " +
					"ORDER BY A.ReadingDateTime DESC, SequenceNo ASC "
					, readingsData.SelectedUserID, readingsData.PatientReadingID);
				}
			}
			if (taskData?.TaskType == ResourceConstants.R_MEASUREMENT_KEY)
			{
				await UpdateTaskStatusAsync(readingsData, ResourceConstants.R_INPROGRESS_STATUS_KEY).ConfigureAwait(false);
			}
		}

		private async Task<short> FetchDataForQuestionnaireQuestionAnswerAsync(PatientReadingDTO readingsData, SQLiteAsyncConnection sqlConnection, TaskModel taskData, short readingID)
		{
			if (taskData?.TaskType == ResourceConstants.R_QUESTIONNAIRE_KEY)
			{
					readingsData.ListData = await sqlConnection.QueryAsync<PatientReadingUIModel>(
					"SELECT  1 AS IsTwoRowList, A.PatientReadingID, A.ReadingValue, A.ReadingDateTime, A.ReadingNotes, A.SourceName, " +
					"A.SourceQuantity, A.PatientTaskID, A.ReadingID, A.ReadingSourceType, A.IsActive, A.AddedByID " +
					"FROM PatientReadingModel A " +
					"WHERE A.PatientReadingID = ?",
					readingsData.PatientReadingID);
					readingID = readingsData.ListData.FirstOrDefault().ReadingID;
			}
			return readingID;
		}

		/// <summary>
		/// Fetch latest reading data
		/// </summary>
		/// <param name="readingsData"></param>
		/// <returns>latest reading data</returns>
		public async Task GetLatestPatientReadingsAsync(PatientReadingDTO readingsData)
		{
			foreach (var type in readingsData.ChartMetaData)
			{
				var latestReading = await SqlConnection.FindWithQueryAsync<PatientReadingUIModel>
					($"{GetPatientReadingsQuery(string.Empty)} ORDER BY A.ReadingDateTime DESC, SequenceNo ASC LIMIT 1", type.ReadingID, readingsData.SelectedUserID);
				if (latestReading != null)
				{
					if (!GenericMethods.IsListNotEmpty(readingsData.ListData))
					{
						readingsData.ListData = new List<PatientReadingUIModel>();
					}
					readingsData.ListData.Add(latestReading);
				}
			}
		}

		/// <summary>
		/// Gets readings within the given range for detail page
		/// </summary>
		/// <param name="readingsData">Reference object to return list of readings and metadata</param>
		/// <param name="startDate">Start date from which the readings are to be fetched</param>
		/// <param name="endDate">End data till which the readings are to be fetched</param>
		/// <returns>List of readings and metadata within the given start and end date</returns>
		public async Task GetPatientReadingsAsync(PatientReadingDTO readingsData, DateTimeOffset startDate, DateTimeOffset endDate)
		{
			await SqlConnection.RunInTransactionAsync(transaction =>
			{
				// IsActive is used to identify if for this VitalType, the given user has any data
				readingsData.IsActive = transaction.FindWithQuery<PatientReadingUIModel>(
					"SELECT 1 FROM PatientReadingModel A " +
					"JOIN ReadingModel C ON C.ReadingID = A.ReadingID AND A.UserID = C.UserID " +
					"WHERE A.UserID = ? AND A.IsActive = 1"
					, readingsData.SelectedUserID
				) != null;
				string readingIDs = GetCommaSeperatedReadingIDs(readingsData);
				readingsData.ListData = transaction.Query<PatientReadingUIModel>(
					$"{GetPatientReadingsQuery(readingIDs)} AND A.ReadingDateTime >= ? AND A.ReadingDateTime <= ? " +
					"ORDER BY A.ReadingDateTime DESC, SequenceNo ASC ",
					readingsData.SelectedUserID, startDate, endDate);
			}).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets top records based on the given record count and reading type
		/// </summary>
		/// <param name="readingsData">Reference object for record count input as well as reading list as output</param>
		/// <returns>List of readings based on the given input filter</returns>
		public async Task GetPatientReadingsByReadingIDAsync(PatientReadingDTO readingsData)
		{
			await SqlConnection.RunInTransactionAsync(transaction =>
			{
				readingsData.ListData = new List<PatientReadingUIModel>();
				foreach (var readingSubType in readingsData.ChartMetaData)
				{
					var readingData = transaction.Query<PatientReadingUIModel>(
						$"{GetPatientReadingsQuery(string.Empty)} ORDER BY A.ReadingDateTime DESC, SequenceNo ASC LIMIT ?",
						readingSubType.ReadingID, readingsData.SelectedUserID, readingSubType.SummaryRecordCount);
					if (GenericMethods.IsListNotEmpty(readingData))
					{
						readingsData.ListData.AddRange(readingData);
					}
				}
			}).ConfigureAwait(false);
		}

		private string GetCommaSeperatedReadingIDs(PatientReadingDTO readingsData)
		{
			var metadata = readingsData.ChartMetaData.Where(x => x.ReadingID == readingsData.ReadingID || x.ReadingParentID == readingsData.ReadingID)?.ToList();
			string readingIDs = $"{readingsData.ReadingID}";
			if (GenericMethods.IsListNotEmpty(metadata))
			{
				readingIDs = string.Join(",", metadata.Select(x => x.ReadingID));
			}
			return readingIDs;
		}

		private string GetPatientReadingsQuery(string readingIDs)
		{
			var typeCheck = string.IsNullOrWhiteSpace(readingIDs) ? "=? " : $"IN ({readingIDs})";
			return "SELECT DISTINCT 1 AS IsTwoRowList, A.PatientReadingID, A.ReadingValue, A.ReadingDateTime, A.ReadingNotes, A.SourceName, " +
				"A.SourceQuantity, A.PatientTaskID, A.ReadingID, A.ReadingSourceType, A.IsActive, A.AddedByID, " +
				"(SELECT SequenceNo FROM ReadingModel WHERE ReadingID = A.ReadingID AND UserID = A.UserID LIMIT 1) AS SequenceNo " +
				"FROM PatientReadingModel A " +
				$"WHERE A.ReadingID {typeCheck} AND A.UserID = ? AND A.IsActive = 1 ";
		}

        private async Task UpdateTaskStatusAsync(PatientReadingDTO readingData, string status)
        {
            //// When Saving reading data of a Task, Update task status as well 
            if (readingData.RecordCount == -1 && readingData.PatientTaskID > 0)
            {
                await new PatientTaskDatabase().UpdateTaskStatusAsync(readingData.PatientTaskID, status).ConfigureAwait(false);
            }
        }


        public async Task GetPatientOverviewReadingAsync(PatientReadingDTO readingData)
		{
			readingData.PatientReadings = await SqlConnection.QueryAsync<PatientReadingModel>("SELECT * FROM PatientReadingModel WHERE IsActive = 1 AND UserID = ?", readingData.SelectedUserID).ConfigureAwait(false);
		}

		private async Task GetPatientReadingsToSyncWithServerAsync(PatientReadingDTO readingData)
		{
			readingData.PatientReadings = await SqlConnection.QueryAsync<PatientReadingModel>(
				"SELECT * FROM PatientReadingModel WHERE IsSynced = 0"
			).ConfigureAwait(false);
		}

		/// <summary>
		/// Update reading sync status
		/// </summary>
		/// <param name="readingData">data to update sync status</param>
		public async Task UpdatePatientReadingsSyncStatusAsync(PatientReadingDTO readingData)
		{
			await SqlConnection.RunInTransactionAsync(transaction =>
			{
				foreach (PatientReadingModel reading in readingData.PatientReadings)
				{
					SaveResultModel result = readingData.SaveReadings?.FirstOrDefault(x => x.ClientGuid == reading.PatientReadingID);
					reading.ErrCode = result == null ? ErrorCode.OK : result.ErrCode;
					switch (reading.ErrCode)
					{
						case ErrorCode.OK:
							if (result == null || result.ClientGuid == result.ServerGuid)
							{
								// Data is successfully synced, so only update sync flag and EmpID received from server
								transaction.Execute("UPDATE PatientReadingModel SET IsSynced = 1, ErrCode = ? WHERE PatientReadingID = ?", reading.ErrCode, reading.PatientReadingID);
							}
							else
							{
								// When Data is not present for both IDs (ClientGUID & ServerGUID), then UPDATE ClientGUID with ServerGUID in Local DB
								// When Data is present for both IDs (ClientGUID & ServerGUID), then DELETE data of ServerGuid from local DB, and UPDATE ClientGUID with ServerGUID in Local DB
								if (transaction.FindWithQuery<PatientReadingModel>("SELECT 1 FROM PatientReadingModel WHERE PatientReadingID = ?", result.ServerGuid) != null)
								{
									// DELETE data of ServerGuid from local DB
									transaction.Execute("DELETE FROM PatientReadingModel WHERE PatientReadingID = ?", result.ServerGuid);
								}
								//// UPDATE ClientGUID with ServerGUID in Local DB
								transaction.Execute("UPDATE PatientReadingModel SET PatientReadingID = ?, IsSynced = 1, ErrCode = ? WHERE PatientReadingID = ?", result.ServerGuid, reading.ErrCode, reading.PatientReadingID);
							}
							break;
						case ErrorCode.DuplicateGuid:
							// UPDATE ClientGUID with new Guid
							Guid newGuid = GenerateNewGuid(transaction);
							transaction.Execute("UPDATE PatientReadingModel SET PatientReadingID = ?, IsSynced = 0 WHERE PatientReadingID = ?", newGuid, reading.PatientReadingID);
							readingData.ErrCode = reading.ErrCode;
							break;
						default:
							// Mark record with the received error code
							transaction.Execute("UPDATE PatientReadingModel SET ErrCode = ? WHERE PatientReadingID = ?", reading.ErrCode, reading.PatientReadingID);
							break;
					}
				}
			}).ConfigureAwait(false);
		}

		/// <summary>
		/// Save patient reading
		/// </summary>
		/// <param name="readingData">object to save patient reading data</param>
		/// <returns>List of patient readings</returns>
		public async Task SavePatientReadingsAsync(PatientReadingDTO readingData)
		{
			// Check for Duplicate Data
			await SavePatientReadings(readingData).ConfigureAwait(false);

			TaskModel taskData = await new PatientTaskDatabase().GetTaskByPatientTaskIDAsync(readingData.PatientTaskID);
			if (taskData?.TaskType == ResourceConstants.R_MEASUREMENT_KEY)
			{
				await UpdateTaskStatusAsync(readingData, ResourceConstants.R_COMPLETED_STATUS_KEY).ConfigureAwait(false);
			}
		}

		private async Task SavePatientReadings(PatientReadingDTO readingData)
		{
			await SqlConnection.RunInTransactionAsync(transaction =>
			{
				foreach (PatientReadingUIModel pReading in readingData.ListData)
				{
					if (pReading.IsActive)
					{
						PatientReadingModel existingReading = transaction.FindWithQuery<PatientReadingModel>(
							"SELECT * FROM PatientReadingModel WHERE PatientReadingID = ?", pReading.PatientReadingID
						);
						if (existingReading == null)
						{
							existingReading = transaction.Query<PatientReadingModel>(
								"SELECT * FROM PatientReadingModel WHERE ReadingID=? AND UserID=?"
								, pReading.ReadingID, readingData.SelectedUserID)?.FirstOrDefault(p =>
									p.ReadingID == pReading.ReadingID && p.IsActive &&
									IsDateTimeMatched(pReading.ReadingFrequency, pReading.ReadingDateTime.Value, p.ReadingDateTime.Value)
							);
						}
						if (existingReading == null)
						{
							transaction.Execute("INSERT INTO PatientReadingModel" +
                                "(PatientReadingID, ReadingID, ReadingDateTime, ReadingFrequency, SourceName, SourceQuantity, ReadingValue, ReadingValue2, " +
								"ReadingNotes, PatientTaskID, ReadingSourceType, IsActive, IsSynced, UserID, AddedON, LastModifiedON, AddedByID) " +
								"VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?) ",
								pReading.PatientReadingID, pReading.ReadingID, pReading.ReadingDateTime, pReading.ReadingFrequency, pReading.SourceName, pReading.SourceQuantity,
								pReading.ReadingValue.Value, pReading.ReadingValue2, pReading.ReadingNotes, pReading.PatientTaskID, pReading.ReadingSourceType, true, false,
								readingData.SelectedUserID, GenericMethods.GetUtcDateTime, GenericMethods.GetUtcDateTime, pReading.AddedByID);
						}
						else
						{
							pReading.PatientReadingID = existingReading.PatientReadingID;
							transaction.Execute("UPDATE PatientReadingModel SET ReadingID=?, ReadingDateTime=?, ReadingFrequency=?, SourceName=?, SourceQuantity=?, ReadingValue=?, ReadingValue2=?" +
								"ReadingNotes=?, ReadingSourceType=?, IsActive=1, IsSynced=0, UserID=?, LastModifiedON=?  WHERE PatientReadingID=? ",
								pReading.ReadingID, pReading.ReadingDateTime, pReading.ReadingFrequency, pReading.SourceName, pReading.SourceQuantity, pReading.ReadingValue.Value, pReading.ReadingValue2,
                                pReading.ReadingNotes, pReading.ReadingSourceType, readingData.SelectedUserID, GenericMethods.GetUtcDateTime, pReading.PatientReadingID);
						};
					}
					else
					{
						transaction.Execute("UPDATE PatientReadingModel SET IsSynced = 0, IsActive = 0,  LastModifiedON = ? WHERE PatientReadingID = ?", GenericMethods.GetUtcDateTime, pReading.PatientReadingID);
					}
				}
			}).ConfigureAwait(false);

		}
        public async Task<PatientReadingModel> GetPatientReadingAsync(string patientReadingID)
        {
            return await SqlConnection.FindWithQueryAsync<PatientReadingModel>(
                "SELECT * FROM PatientReadingModel WHERE PatientReadingID = ?", patientReadingID
            );
        }

        private bool IsDateTimeMatched(short frequency, DateTimeOffset dt, DateTimeOffset dbDT)
		{
			if (frequency == ResourceConstants.R_DAILY_SUM_KEY_ID
				|| frequency == ResourceConstants.R_DAILY_AVG_KEY_ID)
			{
				return new DateTimeOffset(dbDT.Year, dbDT.Month, dbDT.Day, 00, 00, 00, dbDT.Offset) ==
					new DateTimeOffset(dt.Year, dt.Month, dt.Day, 00, 00, 00, dt.Offset);
			}
			else if (frequency == ResourceConstants.R_HOURLY_SUM_KEY_ID
				|| frequency == ResourceConstants.R_HOURLY_AVG_KEY_ID)
			{
				return new DateTimeOffset(dbDT.Year, dbDT.Month, dbDT.Day, dbDT.Hour, 00, 00, dbDT.Offset) ==
					new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, 00, 00, dt.Offset);
			}
			else
			{
				return new DateTimeOffset(dbDT.Year, dbDT.Month, dbDT.Day, dbDT.Hour, dbDT.Minute, 00, dbDT.Offset) ==
					new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 00, dt.Offset);
			}
		}

		private void SavePatientReadings(PatientReadingDTO readingData, SQLiteConnection transaction)
		{
			if (GenericMethods.IsListNotEmpty(readingData.PatientReadings))
			{
				foreach (var patientReading in readingData.PatientReadings.OrderBy(x => x.IsActive))
				{
					transaction.InsertOrReplace(patientReading);
				}
			}
		}
	}
}