using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using PhantomJs.NetCore;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class MedicalHistoryServiceBusinessLayer : BaseServiceBusinessLayer
	{
		/// <summary>
		/// MedicalHistory service
		/// </summary>
		/// <param name="httpContext">Instance of HttpContext</param>
		public MedicalHistoryServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
		{
		}

		/// <summary>
		/// Get Medical History data for patient/program
		/// </summary>
		/// <param name="languageID">User's language ID</param>
		/// <param name="organisationID">User's Organisation ID</param>
		/// <param name="permissionAtLevelID">User's permission level</param>
		/// <param name="selectedUserID">Selected user's ID for whoes data needs to be retrived</param>
		/// <param name="fromDate">Start date from where data needs to fetch</param>
		/// <param name="toDate">End date, till data needs to fetched</param>
		/// <param name="dateTimeDifference">utc and local date time difference</param>
		/// <returns>Medical History data with operation status</returns>
		public async Task<MedicalHistoryDTO> GetMedicalHistoryAsync(byte languageID, long organisationID, long permissionAtLevelID, long selectedUserID, string fromDate, string toDate, double dateTimeDifference, bool showAllData)
		{
			MedicalHistoryDTO medicalHistoryData = new MedicalHistoryDTO();
			try
			{
				if (permissionAtLevelID < 1 || organisationID < 1 || languageID < 1 || selectedUserID < 1)
				{
					medicalHistoryData.ErrCode = ErrorCode.InvalidData;
					return medicalHistoryData;
				}
				if (AccountID < 1)
				{
					medicalHistoryData.ErrCode = ErrorCode.Unauthorized;
					return medicalHistoryData;
				}
				medicalHistoryData.OrganisationID = organisationID;
				medicalHistoryData.LanguageID = languageID;
				medicalHistoryData.AccountID = AccountID;
				if (await GetSettingsResourcesAsync(medicalHistoryData, true, GroupConstants.RS_COMMON_GROUP, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_MENU_ACTION_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP}").ConfigureAwait(false))
				{
					medicalHistoryData.SelectedUserID = selectedUserID;
					medicalHistoryData.PermissionAtLevelID = permissionAtLevelID;
					medicalHistoryData.FromDate = fromDate;
					medicalHistoryData.ToDate = toDate;
					medicalHistoryData.DateTimeDifference = dateTimeDifference;
					medicalHistoryData.FeatureFor = FeatureFor;
					await new MedicalHistoryServiceDataLayer().GetMedicalHistoryAsync(medicalHistoryData, showAllData).ConfigureAwait(false);
					if (medicalHistoryData.ErrCode == ErrorCode.OK)
					{
						// Load Views Dependancy data here (like settings, resources, replace blob storage link etc.) 
						if (GenericMethods.IsListNotEmpty(medicalHistoryData.AllMedicalHistoryViews))
						{
							foreach (var view in medicalHistoryData.AllMedicalHistoryViews)
							{
								if (view.PageData != null)
								{
									await LoadViewsDependanciesAsync(medicalHistoryData, view).ConfigureAwait(false);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				medicalHistoryData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
			}
			return medicalHistoryData;
		}

		private async Task LoadViewsDependanciesAsync(MedicalHistoryDTO medicalHistoryData, MedicalHistoryViewModel view)
		{
			switch (view.FeatureCode)
			{
				case AppPermissions.PatientReadingsView:
					PatientReadingDTO readingData = view.PageData as PatientReadingDTO;
					await new PatientReadingServiceBusinessLayer(_httpContext).GetReadingsResourceAndSettingsAsync(readingData, medicalHistoryData.OrganisationID, medicalHistoryData.LanguageID).ConfigureAwait(false);
					view.PageData = readingData;
					break;
				case AppPermissions.PatientProviderNotesView:
					PatientProviderNoteDTO patientProviderNoteData = view.PageData as PatientProviderNoteDTO;
					var notesService = new PatientProviderNoteServiceBusinessLayer(_httpContext);
					await notesService.GetPatientProviderNotesResourceAndSettingsAsync(patientProviderNoteData, medicalHistoryData.OrganisationID, medicalHistoryData.LanguageID).ConfigureAwait(false);
					await notesService.MapPatientProviderNotesDependancyAsync(patientProviderNoteData).ConfigureAwait(false);
					view.PageData = patientProviderNoteData;
					break;
				case AppPermissions.PatientTasksView:
					ProgramDTO taskData = view.PageData as ProgramDTO;
					await new PatientTaskServiceBussinessLayer(_httpContext).GetPatientTasksResourceAndSettingsAsync(taskData, medicalHistoryData.OrganisationID, medicalHistoryData.LanguageID).ConfigureAwait(false);
					view.PageData = taskData;
					break;
				case AppPermissions.PatientFilesView:
					FileDTO fileData = view.PageData as FileDTO;
					var fileService = new FilesServiceBusinessLayer(_httpContext);
					await fileService.GetFilesAndDocumentsResourceAndSettingsAsync(fileData, medicalHistoryData.OrganisationID, medicalHistoryData.LanguageID).ConfigureAwait(false);
					await fileService.MapFilesAndDocumentsDependancyAsync(fileData).ConfigureAwait(false);
					view.PageData = fileData;
					break;
				case AppPermissions.PatientMedicationsView:
				case AppPermissions.PrescriptionView:
					PatientMedicationDTO medicationData = view.PageData as PatientMedicationDTO;
					await new MedicationServiceBusinessLayer(_httpContext).GetMedicationsResourceAndSettingsAsync(medicationData, medicalHistoryData.OrganisationID, medicalHistoryData.LanguageID).ConfigureAwait(false);
					view.PageData = medicationData;
					break;
				case AppPermissions.PatientEducationsView:
					ContentPageDTO contentPageData = view.PageData as ContentPageDTO;
					var contentPageService = new ContentPageServiceBusinessLayer(_httpContext);
					await contentPageService.GetContentPagesResourceAndSettingsAsync(contentPageData, medicalHistoryData.OrganisationID, medicalHistoryData.LanguageID).ConfigureAwait(false);
					await contentPageService.MapContentPagesDependancyAsync(contentPageData).ConfigureAwait(false);
					view.PageData = contentPageData;
					break;
				case AppPermissions.PatientTrackersView:
					TrackerDTO trackerData = view.PageData as TrackerDTO;
					var trackerService = new TrackerServiceBusinessLayer(_httpContext);
					await trackerService.GetTrackersResourceAndSettingsAsync(trackerData, medicalHistoryData.OrganisationID, medicalHistoryData.LanguageID).ConfigureAwait(false);
					await trackerService.MapTrackersDependancyAsync(trackerData).ConfigureAwait(false);
					view.PageData = trackerData;
					break;
			}
		}

		/// <summary>
		/// Get Medical Report Forwards data
		/// </summary>
		/// <param name="permissionAtLevelID">permission at level id</param>
		/// <param name="organiationID">organisation id</param>
		/// <param name="languageID">language id</param>
		/// <returns>Return data and operation status</returns>
		public async Task<MedicalHistoryDTO> GetMedicalReportForwardsAsync(long permissionAtLevelID, long organiationID, byte languageID)
		{
			MedicalHistoryDTO patientMedicationData = new MedicalHistoryDTO();
			try
			{
				if (permissionAtLevelID < 1 || languageID < 1 || organiationID < 1)
				{
					patientMedicationData.ErrCode = ErrorCode.InvalidData;
					return patientMedicationData;
				}
				patientMedicationData.AccountID = AccountID;
				if (patientMedicationData.AccountID < 1)
				{
					patientMedicationData.ErrCode = ErrorCode.Unauthorized;
					return patientMedicationData;
				}
				patientMedicationData.ErrCode = ErrorCode.OK;
				patientMedicationData.OrganisationID = organiationID;
				patientMedicationData.LanguageID = languageID;
				await GetConfigurationDataAsync(patientMedicationData, languageID, organiationID).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				patientMedicationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
			}
			return patientMedicationData;
		}

		private async Task<bool> GetConfigurationDataAsync(MedicalHistoryDTO medicalHistoryData, byte languageID, long organisationID)
		{
			medicalHistoryData.CountryCodes = (await GetDataFromCacheAsync(CachedDataType.Countries, string.Empty, languageID, default, 0, 0, false).ConfigureAwait(false)).CountryCodes;

			medicalHistoryData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_APPOINTMENT_PAGE_GROUP},{GroupConstants.RS_MENU_ACTION_GROUP}", languageID, default, medicalHistoryData.AccountID, 0, false).ConfigureAwait(false)).Resources;
			if (GenericMethods.IsListNotEmpty(medicalHistoryData.Resources))
			{
				var orgSettings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, $"{GroupConstants.RS_ORGANISATION_SETTINGS_GROUP}",
					languageID, default, 0, organisationID, false).ConfigureAwait(false)).Settings;

				var settings = (await GetDataFromCacheAsync(CachedDataType.Settings, $"{GroupConstants.RS_COMMON_GROUP}",
				   languageID, default, 0, organisationID, false).ConfigureAwait(false)).Settings;

				medicalHistoryData.Settings = new List<SettingModel>();
				medicalHistoryData.Settings.AddRange(orgSettings);
				medicalHistoryData.Settings.AddRange(settings);

				if (GenericMethods.IsListNotEmpty(medicalHistoryData.Settings))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Save Medical Report Forwards
		/// </summary>
		/// <param name="medicalHistoryData">Reference object which holds Data</param>
		/// <param name="permissionAtLevelID">permission At Level ID</param>
		/// <param name="organisationID">organisation ID</param>
		/// <returns>Result of operation</returns>
		public async Task<MedicalHistoryDTO> SaveMedicalReportForwardsAsync(MedicalHistoryDTO medicalHistoryData, long permissionAtLevelID, long organisationID)
		{
			try
			{
				if (permissionAtLevelID < 1 || organisationID < 1)
				{
					medicalHistoryData.ErrCode = ErrorCode.InvalidData;
					return medicalHistoryData;
				}
				if (AccountID < 1)
				{
					medicalHistoryData.ErrCode = ErrorCode.Unauthorized;
					return medicalHistoryData;
				}
				medicalHistoryData.AccountID = AccountID;
				medicalHistoryData.PermissionAtLevelID = permissionAtLevelID;
				medicalHistoryData.FeatureFor = FeatureFor;
				await new MedicalHistoryServiceDataLayer().SaveMedicalReportForwardsAsync(medicalHistoryData).ConfigureAwait(false);
				if (medicalHistoryData.ErrCode == ErrorCode.OK)
				{
					await ConvertHtmlStringToByteArrayAsync(medicalHistoryData);
					if (medicalHistoryData.ErrCode == ErrorCode.OK)
					{
						TemplateDTO myData = new TemplateDTO
						{
							AccountID = AccountID,
							OrganisationID = organisationID,
							TemplateData = new TemplateModel
							{
								TemplateName = TemplateName.EShareMedicalProfile,
								IsExternal = true,
								ExternalEmailID = medicalHistoryData.MedicalReportForwards.EmailID,
								ExternalMobileNo = medicalHistoryData.MedicalReportForwards.MobileNo,
								ExternalUserName = medicalHistoryData.MedicalReportForwards.DoctorName,
								DataRecordPrimaryKey = medicalHistoryData.MedicalReportForwards.PatientID.ToString()
							},
							Attachments = new List<FileDataModel>
							{
								new FileDataModel { RecordID = GenericMethods.GenerateGuid().ToString() + ".pdf", Base64File = Convert.ToBase64String(medicalHistoryData.PdfBytes) }
							}
						};
						await SendCommunicationAsync(myData).ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				medicalHistoryData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
			}
			return medicalHistoryData;
		}

		/// <summary>
		/// Convert HTML String To PDF Byte Array Using PhantomJs.NetCore
		/// </summary>
		/// <param name="medicalHistoryData">Reference object which holds Data</param>
		private async Task ConvertHtmlStringToByteArrayAsync(MedicalHistoryDTO medicalHistoryData)
		{
			try
			{
				// create instance of PdfGenerator:
				PdfGenerator generator = new();

				// call method GeneratePdf with the HTML string and the output directory:
				string pathOfGeneratedPdf = generator.GeneratePdf(medicalHistoryData.HtmlString);

				medicalHistoryData.PdfBytes = await File.ReadAllBytesAsync(pathOfGeneratedPdf);

				// delete file
				File.Delete(pathOfGeneratedPdf);
			}
			catch (Exception ex)
			{
				medicalHistoryData.ErrCode = ErrorCode.InternalServerError;
				LogError(ex.Message, ex);
			}
		}
	}
}