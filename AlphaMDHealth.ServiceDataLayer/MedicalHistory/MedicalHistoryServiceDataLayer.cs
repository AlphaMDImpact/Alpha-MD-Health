using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    /// <summary>
    /// Medical history service data layer operation
    /// </summary>
    public class MedicalHistoryServiceDataLayer : BaseServiceDataLayer
	{
		/// <summary>
		/// Get Medical History data for patient/program
		/// </summary>
		/// <param name="medicalHistoryData">Reference object to return list of Medical History data</param>
		/// <returns>Medical History data with operation status</returns>
		public async Task GetMedicalHistoryAsync(MedicalHistoryDTO medicalHistoryData, bool showAllData)
		{
			using var connection = ConnectDatabase();
			connection.Open();
			DynamicParameters parameter = new DynamicParameters();
			parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), medicalHistoryData.FeatureFor, DbType.Byte, ParameterDirection.Input);
			AddDateTimeParameter(nameof(BaseDTO.FromDate), medicalHistoryData.FromDate, parameter, ParameterDirection.Input);
			AddDateTimeParameter(nameof(BaseDTO.ToDate), medicalHistoryData.ToDate, parameter, ParameterDirection.Input);
			parameter.Add(ConcateAt(nameof(MedicalHistoryDTO.DateTimeDifference)), medicalHistoryData.DateTimeDifference, DbType.Double, ParameterDirection.Input);
			parameter.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), medicalHistoryData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
			parameter.Add(ConcateAt(nameof(MedicalHistoryViewModel.ShowAllData)), showAllData, DbType.Byte, ParameterDirection.Input);
			MapCommonSPParameters(medicalHistoryData, parameter, AppPermissions.MedicalHistoryView.ToString(), $"{AppPermissions.MedicalHistoryAddEdit},{AppPermissions.MedicalHistoryPrint},{AppPermissions.MedicalHistoryShare}");
			SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_MEDICAL_HISTORY, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
			if (result.HasRows())
			{
				medicalHistoryData.OrganisationAddress = (await MapTableDataAsync<ContactModel>(result).ConfigureAwait(false))?.FirstOrDefault()?.ContactValue;
				medicalHistoryData.OrganisationContact = (await MapTableDataAsync<ContactModel>(result).ConfigureAwait(false))?.FirstOrDefault()?.ContactValue;
				medicalHistoryData.MedicalHistory = await MapTableDataAsync<MedicalHistoryModel>(result).ConfigureAwait(false);
				var medicalHistoryViews = await MapTableDataAsync<MedicalHistoryViewModel>(result).ConfigureAwait(false);
				if (GenericMethods.IsListNotEmpty(medicalHistoryViews))
				{
					foreach (var view in medicalHistoryViews)
					{
						await MapHistryViewsDataAsync(medicalHistoryData, result, view);
					}
				}
				medicalHistoryData.AllMedicalHistoryViews = await MapTableDataAsync<MedicalHistoryViewModel>(result).ConfigureAwait(false);
				medicalHistoryData.AllMedicalHistoryViews?.ForEach(x =>
					x.PageData = medicalHistoryViews?.FirstOrDefault(v => v.FeatureCode == x.FeatureCode)?.PageData
				);
				medicalHistoryData.AddHistoryFor = await MapTableDataAsync<OptionModel>(result).ConfigureAwait(false);
				await MapReturnPermissionsAsync(medicalHistoryData, result).ConfigureAwait(false);
			}
			medicalHistoryData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
		}

		private async Task MapHistryViewsDataAsync(MedicalHistoryDTO medicalHistoryData, SqlMapper.GridReader result, MedicalHistoryViewModel view)
		{
			switch (view.FeatureCode)
			{
				case AppPermissions.PatientReadingsView:
					PatientReadingDTO readingData = new PatientReadingDTO { RecordCount = medicalHistoryData.RecordCount, SelectedUserID = medicalHistoryData.SelectedUserID };
					await new PatientReadingServiceDataLayer().MapPatientReadingsViewDataAsync(readingData, result);
					view.PageData = readingData;
					break;
				case AppPermissions.PatientProviderNotesView:
					PatientProviderNoteDTO patientProviderNoteData = new PatientProviderNoteDTO { RecordCount = medicalHistoryData.RecordCount, SelectedUserID = medicalHistoryData.SelectedUserID };
					await new PatientProviderNoteDataLayer().MapPatientProviderNotesViewDataAsync(patientProviderNoteData, result);
					view.PageData = patientProviderNoteData;
					break;
				case AppPermissions.PatientTasksView:
					ProgramDTO taskData = new ProgramDTO { RecordCount = medicalHistoryData.RecordCount, SelectedUserID = medicalHistoryData.SelectedUserID };
					await new PatientTaskServiceDataLayer().MapPatientTasksViewDataAsync(taskData, result);
					view.PageData = taskData;
					break;
				case AppPermissions.PatientFilesView:
					FileDTO fileData = new FileDTO { RecordCount = medicalHistoryData.RecordCount, SelectedUserID = medicalHistoryData.SelectedUserID };
					await new FilesServiceDataLayer().MapPatientFilesViewDataAsync(fileData, result);
					view.PageData = fileData;
					break;
				case AppPermissions.PatientMedicationsView:
					PatientMedicationDTO medicationData = new PatientMedicationDTO { RecordCount = medicalHistoryData.RecordCount, SelectedUserID = medicalHistoryData.SelectedUserID };
					await new MedicationServiceDataLayer().MapPatientMedicationsViewDataAsync(medicationData, result);
					view.PageData = medicationData;
					break;
				case AppPermissions.PrescriptionView:
					PatientMedicationDTO prescriptionData = new PatientMedicationDTO { RecordCount = -2, SelectedUserID = medicalHistoryData.SelectedUserID };
					await new MedicationServiceDataLayer().MapPatientMedicationsViewDataAsync(prescriptionData, result);
					view.PageData = prescriptionData;
					break;
				case AppPermissions.PatientEducationsView:
					ContentPageDTO contentPageData = new ContentPageDTO { RecordCount = medicalHistoryData.RecordCount, SelectedUserID = medicalHistoryData.SelectedUserID };
					await new ContentPageServiceDataLayer().MapContentPagesViewDataAsync(contentPageData, result);
					view.PageData = contentPageData;
					break;
				case AppPermissions.PatientTrackersView:
					TrackerDTO trackerData = new TrackerDTO { RecordCount = medicalHistoryData.RecordCount, SelectedUserID = medicalHistoryData.SelectedUserID };
					await new TrackerServiceDataLayer().MapPatientTrackersViewDataAsync(trackerData, result);
					view.PageData = trackerData;
					break;
			}
		}


		/// <summary>
		/// Save Medical Report Forwards Data
		/// </summary>
		/// <param name="medicalHistoryData">Reference object which holds Data</param>
		/// <returns>Operation Status Code</returns>
		public async Task SaveMedicalReportForwardsAsync(MedicalHistoryDTO medicalHistoryData)
		{
			using var connection = ConnectDatabase();
			DynamicParameters parameter = new DynamicParameters();
			parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), medicalHistoryData.FeatureFor, DbType.Byte, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MedicalReportForwards.PatientID), medicalHistoryData.MedicalReportForwards.PatientID, DbType.Int64, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MedicalReportForwards.DoctorName), medicalHistoryData.MedicalReportForwards.DoctorName, DbType.String, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MedicalReportForwards.MobileNo), medicalHistoryData.MedicalReportForwards.MobileNo, DbType.String, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MedicalReportForwards.EmailID), medicalHistoryData.MedicalReportForwards.EmailID, DbType.String, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MedicalReportForwards.ReportForDate), medicalHistoryData.MedicalReportForwards.ReportForDate, DbType.DateTimeOffset, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), medicalHistoryData.MedicalReportForwards.IsActive, DbType.Boolean, ParameterDirection.Input);
			MapCommonSPParameters(medicalHistoryData, parameter, AppPermissions.MedicalHistoryShare.ToString());
			await connection.ExecuteAsync(SPNameConstants.USP_SAVE_MEDICAL_REPORT_FORWARDS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
			medicalHistoryData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
		}
	}
}