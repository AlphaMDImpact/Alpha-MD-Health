using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class PatientProviderNoteServiceBusinessLayer : BaseServiceBusinessLayer
	{
		/// <summary>
		/// Provider Note service
		/// </summary>
		/// <param name="httpContext">Instance of HttpContext</param>
		public PatientProviderNoteServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
		{
		}

		/// <summary>
		/// Get patient provider note(s)
		/// </summary>
		/// <param name="languageID">language ID</param>
		/// <param name="selectedUserID">patient ID </param>
		/// <param name="permissionAtLevelID">level at which permission is required </param>
		/// <param name="organisationID">Id of organization</param>
		/// <param name="recordCount">number of record count to fetch</param>
		/// <param name="providerNoteID">providerNote ID</param>
		/// <param name="programID">Program ID</param>
		/// <param name="questionnaireID">Questionnaire ID</param>
		/// <param name="fromDate">Start date from where data needs to fetch</param>
		/// <param name="toDate">End date, till data needs to fetched</param>
		/// <returns>provider note(s) with operation status</returns>
		public async Task<PatientProviderNoteDTO> GetPatientProviderNotesAsync(byte languageID, long selectedUserID, long permissionAtLevelID, long organisationID, long recordCount, Guid providerNoteID, long programID, long questionnaireID, string fromDate, string toDate)
		{
			PatientProviderNoteDTO patientProviderNoteData = new PatientProviderNoteDTO();
			try
			{
				if (permissionAtLevelID < 1 || languageID < 1 || selectedUserID < 1)
				{
					patientProviderNoteData.ErrCode = ErrorCode.InvalidData;
					return patientProviderNoteData;
				}
				if (AccountID < 1)
				{
					patientProviderNoteData.ErrCode = ErrorCode.Unauthorized;
					return patientProviderNoteData;
				}
				if (await GetPatientProviderNotesResourceAndSettingsAsync(patientProviderNoteData, organisationID, languageID).ConfigureAwait(false))
				{
					patientProviderNoteData.PermissionAtLevelID = permissionAtLevelID;
					patientProviderNoteData.SelectedUserID = selectedUserID;
					patientProviderNoteData.RecordCount = recordCount;
					patientProviderNoteData.FromDate = fromDate;
					patientProviderNoteData.ToDate = toDate;
					patientProviderNoteData.PatientProviderNote = new PatientProviderNoteModel { ProviderNoteID = providerNoteID, ProgramID = programID, QuestionnaireID = questionnaireID };
					patientProviderNoteData.FeatureFor = FeatureFor;
					await new PatientProviderNoteDataLayer().GetPatientProviderNotesAsync(patientProviderNoteData).ConfigureAwait(false);
					await MapPatientProviderNotesDependancyAsync(patientProviderNoteData);
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				patientProviderNoteData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
			}
			return patientProviderNoteData;
		}

		internal async Task MapPatientProviderNotesDependancyAsync(PatientProviderNoteDTO patientProviderNoteDTO)
		{
			if (patientProviderNoteDTO.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(patientProviderNoteDTO.FileDocuments))
			{
				await new FilesServiceBusinessLayer(_httpContext).ReplaceFileAttachmentImageCdnLinkAsync(patientProviderNoteDTO.FileDocuments);
			}
		}

		internal async Task<bool> GetPatientProviderNotesResourceAndSettingsAsync(PatientProviderNoteDTO patientProviderNoteData, long organisationID, byte languageID)
		{
			patientProviderNoteData.AccountID = AccountID;
			patientProviderNoteData.OrganisationID = organisationID;
			patientProviderNoteData.LanguageID = languageID;
			if (await GetSettingsResourcesAsync(patientProviderNoteData, true,
				$"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROVIDER_NOTE_GROUP}",
				$"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROVIDER_NOTE_GROUP},{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_PATIENT_DOCUMNET_GROUP}"
			).ConfigureAwait(false))
			{
				patientProviderNoteData.ErrCode = ErrorCode.OK;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Save Provider Note Data
		/// </summary>
		/// <param name="permissionAtLevelID">level at which permission is required </param>
		/// <param name="noteData">Reference object which holds Patient Provider Note Data</param>
		/// <returns>Operation status<</returns>
		public async Task<PatientProviderNoteDTO> SavePatientProviderNoteAsync(long permissionAtLevelID, PatientProviderNoteDTO noteData)
		{
			try
			{
				if (permissionAtLevelID < 1 || noteData.PatientProviderNote == null || AccountID < 1 || IsAnswersInValid(noteData))
				{
					noteData.ErrCode = ErrorCode.InvalidData;
					return noteData;
				}
                if (noteData.PatientProviderNote.IsActive)
                {
                    noteData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(noteData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROVIDER_NOTE_GROUP},{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_PATIENT_DOCUMNET_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(noteData.PatientProviderNote, noteData.Resources))
                        {
                            noteData.ErrCode = ErrorCode.InvalidData;
                            return noteData;
                        }
                    }
                    else
                    {
                        return noteData;
                    }
                }
                noteData.AccountID = AccountID;
				noteData.PermissionAtLevelID = permissionAtLevelID;
				noteData.FeatureFor = FeatureFor;
				await SaveProviderNoteAsync(noteData).ConfigureAwait(false);
				if (noteData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(noteData?.FileDocuments))
				{
					await SaveNoteDocumentsAsync(noteData).ConfigureAwait(false);
				}
				else
				{
					noteData.ErrCode = ErrorCode.OK;
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				noteData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
			}
			return noteData;
		}

		private async Task SaveNoteDocumentsAsync(PatientProviderNoteDTO noteData)
		{
			FileDTO fileData = new FileDTO()
			{
				AccountID = noteData.AccountID,
				PermissionAtLevelID = noteData.PermissionAtLevelID,
				FileDocuments = noteData.FileDocuments,
				File = new FileModel()
				{
					UserID = noteData.PatientProviderNote.PatientID
				}
			};
			await new FilesServiceBusinessLayer(_httpContext).SaveNoteDocumentsAsyncs(fileData).ConfigureAwait(false);
		}

		private static async Task SaveProviderNoteAsync(PatientProviderNoteDTO patientProviderNoteData)
		{
			await new PatientProviderNoteDataLayer().SavePatientProviderNoteAsync(patientProviderNoteData).ConfigureAwait(false);
			if (patientProviderNoteData.ErrCode == ErrorCode.DuplicateGuid)
			{
				patientProviderNoteData.PatientProviderNote.ProviderNoteID = GenericMethods.GenerateGuid();
				patientProviderNoteData?.QuestionnaireQuestionAnswers.ForEach(x => x.PatientTaskID = patientProviderNoteData.PatientProviderNote.ProviderNoteID.ToString());
				patientProviderNoteData.ErrCode = ErrorCode.OK;
				await SaveProviderNoteAsync(patientProviderNoteData);
			}
		}

		private bool IsAnswersInValid(PatientProviderNoteDTO patientProviderNoteData)
		{
			return patientProviderNoteData.QuestionnaireQuestionAnswers?.Any(x => x.TaskType < 2 || x.QuestionID < 1) ?? false;
		}

		private async Task<bool> GetConfigurationDataAsync(BaseDTO baseDTO, byte languageID, string groupNames)
		{
			baseDTO.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
			var ProviderNOteGroupSetting = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_PROVIDER_NOTE_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
			var OrganizationSettings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP,
				  languageID, default, 0, baseDTO.OrganisationID, false).ConfigureAwait(false)).Settings;
			baseDTO.Settings.AddRange(OrganizationSettings);
			baseDTO.Settings.AddRange(ProviderNOteGroupSetting);
			baseDTO.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, groupNames
					, languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
			if (baseDTO.Resources != null)
			{
				return true;
			}
			return false;
		}
	}
}