using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public partial class QuestionnaireService : BaseService
    {
        /// <summary>
        ///  Get patient provider notes from server
        /// </summary>
        /// <param name="patientProviderNoteData">Reference object contains data</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Provider notes data with operation status</returns>
        private async Task SyncPatientProviderNotesFromServerAsync(PatientProviderNoteDTO patientProviderNoteData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PATIENT_PROVIDER_NOTE_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY,GetPermissionAtLevelID()},
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(patientProviderNoteData.RecordCount, CultureInfo.InvariantCulture) },
                        { nameof(PatientProviderNoteModel.ProviderNoteID), Convert.ToString(patientProviderNoteData.PatientProviderNote?.ProviderNoteID, CultureInfo.InvariantCulture) },
                        { nameof(PatientProviderNoteModel.ProgramID), Convert.ToString(patientProviderNoteData.PatientProviderNote?.ProgramID, CultureInfo.InvariantCulture) },
                        { nameof(PatientProviderNoteModel.QuestionnaireID), Convert.ToString(patientProviderNoteData.PatientProviderNote?.QuestionnaireID, CultureInfo.InvariantCulture) },
                        { nameof(BaseDTO.SelectedUserID), Convert.ToString(patientProviderNoteData.SelectedUserID, CultureInfo.InvariantCulture)},
                        { nameof(BaseDTO.FromDate), patientProviderNoteData.FromDate },
                        { nameof(BaseDTO.ToDate), patientProviderNoteData.ToDate },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                patientProviderNoteData.ErrCode = httpData.ErrCode;
                if (patientProviderNoteData.ErrCode == ErrorCode.OK)
                {
                    MapGetPatientProviderNoteServiceResponse(patientProviderNoteData, httpData.Response);
                }
            }
            catch (Exception ex)
            {
                patientProviderNoteData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapGetPatientProviderNoteServiceResponse(PatientProviderNoteDTO patientProviderNoteData, string jsonResponse)
        {
            JToken data = JToken.Parse(jsonResponse);
            if (data != null && data.HasValues)
            {
                if (!MobileConstants.IsMobilePlatform)
                {
                    MapCommonData(patientProviderNoteData, data);
                    SetResourcesAndSettings(patientProviderNoteData);
                }
                MapPatientProviderNotes(data, patientProviderNoteData);
            }
        }

        internal object MapPatientProviderNotesHistoryData(MedicalHistoryDTO medicalHistoryData, MedicalHistoryViewModel historyView, string jsonResponse)
        {
            PatientProviderNoteDTO patientProviderNoteData = new PatientProviderNoteDTO
            {
                FromDate = medicalHistoryData.FromDate,
                ToDate = medicalHistoryData.ToDate,
                RecordCount = medicalHistoryData.RecordCount,
                ErrCode = historyView.ErrorCode
            };
            MapGetPatientProviderNoteServiceResponse(patientProviderNoteData, jsonResponse);
            patientProviderNoteData.FeaturePermissions = medicalHistoryData.FeaturePermissions;
            GetProviderListUI(patientProviderNoteData);
            historyView.HasData = GenericMethods.IsListNotEmpty(patientProviderNoteData.PatientProviderNotes);
            return patientProviderNoteData;
        }

        /// <summary>
        /// Save patient provider note data to server
        /// </summary>
        /// <param name="requestData">object to sync patient provider note data and return operation status</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncPatientProviderNoteToServerAsync(PatientProviderNoteDTO requestData, CancellationToken cancellationToken)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    await new PatientProviderNoteDatabase().GetPatientProviderNotesForSyncAsync(requestData).ConfigureAwait(false);
                    if (GenericMethods.IsListNotEmpty(requestData.PatientProviderNotes))
                    {
                        foreach (var note in requestData.PatientProviderNotes)
                        {
                            requestData.PatientProviderNote = note;
                            requestData.AddedBy = note.ProviderNoteID.ToString();
                            requestData.QuestionnaireQuestionAnswers = note.IsActive ? requestData.QuestionnaireQuestionAnswers?.Where(x => x.PatientTaskID == note.ProviderNoteID.ToString())?.ToList() : new List<PatientQuestionnaireQuestionAnswersModel>();
                            await SyncToServerAsync(requestData, cancellationToken).ConfigureAwait(false);
                            if (requestData.ErrCode == ErrorCode.OK)
                            {
                                JToken data = JToken.Parse(requestData.Response);
                                requestData.PatientProviderNote.ProviderNoteID = (Guid)data[nameof(PatientProviderNoteDTO.PatientProviderNote)][nameof(PatientProviderNoteModel.ProviderNoteID)];
                                if (data?.HasValues == true && MobileConstants.IsMobilePlatform)
                                {
                                    //Update issynced = true
                                    await new PatientProviderNoteDatabase().UpdateProviderNoteIDAsync(requestData.AddedBy, requestData.PatientProviderNote.ProviderNoteID.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    await SyncToServerAsync(requestData, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                requestData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get patient provider notes data
        /// </summary>
        /// <param name="providerNoteData">patient provider notes data</param>
        /// <returns>patient provider notes data with operation status</returns>
        public async Task GetProviderNotesAsync(PatientProviderNoteDTO providerNoteData)
        {
            try
            {
                providerNoteData.SelectedUserID = GetUserID();
                if (MobileConstants.IsMobilePlatform)
                {
                    providerNoteData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                    providerNoteData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                    await Task.WhenAll(
                         GetResourcesAsync(GroupConstants.RS_PROVIDER_NOTE_GROUP, GroupConstants.RS_PATIENT_DOCUMNET_GROUP, GroupConstants.RS_PROGRAMS_GROUP),
                         GetSettingsAsync(GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                         GetFeaturesAsync(AppPermissions.PatientProviderNoteAddEdit.ToString(), AppPermissions.PatientProviderNotesView.ToString()),
                         new PatientProviderNoteDatabase().GetPatientProviderNotesAsync(providerNoteData)
                     ).ConfigureAwait(false);
                    MapQuestionDetails(providerNoteData.QuestionnaireQuestions, providerNoteData.QuestionnaireQuestiosDetails);
                    providerNoteData.ErrCode = ErrorCode.OK;
                }
                else
                {
                    await SyncPatientProviderNotesFromServerAsync(providerNoteData, CancellationToken.None).ConfigureAwait(false);
                }
                if (GenericMethods.IsListNotEmpty(providerNoteData.PatientProviderNotes))
                {
                    GetDateFormats(out string dayFormat, out string monthFormat, out string yearFormat);
                    foreach (var providerNote in providerNoteData.PatientProviderNotes)
                    {
                        providerNote.NoteDateTimeString = GenericMethods.GetDateTimeBasedOnCulture(providerNote.NoteDateTime.Value, DateTimeType.Date,dayFormat,monthFormat,yearFormat);
                    }
                }
                GetProviderListUI(providerNoteData);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                providerNoteData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// Get provider note data
        /// </summary>
        /// <param name="providerNoteData">patient provider note data</param>
        /// <returns>patient provider note data with operation status</returns>
        public async Task GetProviderNoteAsync(PatientProviderNoteDTO providerNoteData)
        {
            try
            {
                providerNoteData.SelectedUserID = GetUserID();
                if (MobileConstants.IsMobilePlatform)
                {
                    await Task.WhenAll(
                         GetResourcesAsync(GroupConstants.RS_PROVIDER_NOTE_GROUP, GroupConstants.RS_PATIENT_DOCUMNET_GROUP),
                         GetSettingsAsync(GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_PROVIDER_NOTE_GROUP),
                         GetFeaturesAsync(AppPermissions.PatientProviderNoteAddEdit.ToString(), AppPermissions.PatientProviderNoteDelete.ToString()),
                         new PatientProviderNoteDatabase().GetPatientProviderNoteAsync(providerNoteData)
                     ).ConfigureAwait(false);
                    if (GenericMethods.IsListNotEmpty(providerNoteData.QuestionnaireQuestions))
                    {
                        providerNoteData.QuestionnaireQuestions?.ForEach(item =>
                        {
                            item.CaptionText = providerNoteData.QuestionnaireQuestiosDetails?.FirstOrDefault(x => x.QuestionID == item.QuestionID)?.CaptionText;
                            item.InstructionsText = providerNoteData.QuestionnaireQuestiosDetails?.FirstOrDefault(x => x.QuestionID == item.QuestionID)?.InstructionsText;
                            item.AnswerPlaceHolder = providerNoteData.QuestionnaireQuestiosDetails?.FirstOrDefault(x => x.QuestionID == item.QuestionID)?.AnswerPlaceHolder;
                        });
                    }
                    providerNoteData.ErrCode = ErrorCode.OK;
                }
                else
                {
                    await SyncPatientProviderNotesFromServerAsync(providerNoteData, CancellationToken.None).ConfigureAwait(false);
                }
                if (providerNoteData.ErrCode == ErrorCode.OK)
                {
                    AssignData(providerNoteData);
                    if (providerNoteData.RecordCount == -3 || providerNoteData.PatientProviderNote.ProviderNoteID != Guid.Empty)
                    {
                        AddResourcesDynamically(providerNoteData);
                        GetProviderQuestionsInOrder(providerNoteData);
                        AddResourcesForMobile(providerNoteData);
                        MapDocuments(providerNoteData);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                providerNoteData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        private void AssignData(PatientProviderNoteDTO providerNoteData)
        {
            if (providerNoteData.PatientProviderNote.ProgramNoteID == 0)
            {
                providerNoteData.PatientProviderNote.NoteDateTime = DateTime.Now;

                if (providerNoteData.PatientPrograms?.Count == 1)
                {
                    providerNoteData.PatientPrograms.FirstOrDefault().IsSelected = true;
                }
                if (providerNoteData.RecordCount == -2)
                {
                    if (providerNoteData.Providers?.Count == 1)
                    {
                        providerNoteData.Providers.FirstOrDefault().IsSelected = true;
                    }
                    if (GenericMethods.IsListNotEmpty(providerNoteData.ProgramNotes))
                    {
                        providerNoteData.ProgramNotes.RemoveAll(x => x.IsDefault == false);
                    }
                    if (providerNoteData.ProgramNotes?.Count == 1)
                    {
                        providerNoteData.ProgramNotes.FirstOrDefault().IsSelected = true;
                    }
                }
            }
        }

        private void MapDocuments(PatientProviderNoteDTO providerNoteData)
        {
            if (providerNoteData.FileDocuments?.Count > 0)
            {
                LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                string deleteValue = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DELETE_ACTION_KEY);
                providerNoteData.FileDocuments.ForEach(document =>
                {
                    FileService fileService = new FileService(_essentials);
                    fileService.PageData = PageData;
                    fileService.FormatDocumentData(dayFormat, monthFormat, yearFormat, deleteValue, document);
                });
            }
        }

        private void AddResourcesForMobile(PatientProviderNoteDTO providerNoteData)
        {
            if (MobileConstants.IsMobilePlatform && GenericMethods.IsListNotEmpty(providerNoteData.Resources))
            {
                var resourcesToRemovet = PageData.Resources.Where(x => x.GroupName == GroupConstants.RS_PROVIDER_NOTE_QUESTIONNAIRE_GROUP)?.ToList();
                if (resourcesToRemovet?.Count > 0)
                {
                    var indexOfFirst = PageData.Resources.IndexOf(PageData.Resources.FirstOrDefault(x => x.GroupName == GroupConstants.RS_PROVIDER_NOTE_QUESTIONNAIRE_GROUP));
                    if (indexOfFirst > 0)
                    {
                        PageData.Resources.RemoveRange(indexOfFirst, resourcesToRemovet.Count);
                    }
                }
                PageData.Resources?.AddRange(providerNoteData.Resources?.Where(x => x.GroupName == GroupConstants.RS_PROVIDER_NOTE_QUESTIONNAIRE_GROUP)?.ToList());
            }
        }

        /// <summary>
        /// Generates GUID in case of add and handles all save cases Save the PatientProviderNote 
        /// </summary>
        /// <param name="patientProviderNoteData">Data to be saved</param>
        /// <returns>Operation status</returns>
        public async Task SavePatientProviderAsync(PatientProviderNoteDTO patientProviderNoteData)
        {
            try
            {
                patientProviderNoteData.PatientProviderNote.AddedOn = GenericMethods.GetUtcDateTime;
                if (patientProviderNoteData.PatientProviderNote.ProviderNoteID == Guid.Empty)
                {
                    patientProviderNoteData.PatientProviderNote.ProviderNoteID = GenericMethods.GenerateGuid();
                }
                if (GenericMethods.IsListNotEmpty(patientProviderNoteData.QuestionnaireQuestionAnswers) && patientProviderNoteData.PatientProviderNote.IsActive)
                {
                    foreach (var questionAnswer in patientProviderNoteData.QuestionnaireQuestionAnswers)
                    {
                        questionAnswer.LastModifiedON = DateTimeOffset.UtcNow;
                        if (questionAnswer.PatientAnswerID == Guid.Empty)
                        {
                            questionAnswer.PatientAnswerID = GenericMethods.GenerateGuid();
                            questionAnswer.IsActive = true;
                            questionAnswer.IsSynced = false;
                            questionAnswer.PatientTaskID = patientProviderNoteData.PatientProviderNote.ProviderNoteID.ToString();
                            questionAnswer.TaskType = 2;
                        }
                    }
                    //Map file Data SourceID
                    MapAnswetIDToDataSourceID(patientProviderNoteData);
                }
                if (MobileConstants.IsMobilePlatform)
                {
                    await new PatientProviderNoteDatabase().SaveProviderNoteAsync(patientProviderNoteData).ConfigureAwait(false);
                    if (patientProviderNoteData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(patientProviderNoteData?.FileDocuments) && patientProviderNoteData.PatientProviderNote.IsActive)
                    {
                        await new FilesDatabase().SaveQuestionFileAndDocumentAsync(patientProviderNoteData.FileDocuments, GetUserID()).ConfigureAwait(false);
                    }
                    patientProviderNoteData.ErrCode = ErrorCode.OK;
                }
                else
                {
                    await SyncPatientProviderNoteToServerAsync(patientProviderNoteData, CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                patientProviderNoteData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapAnswetIDToDataSourceID(PatientProviderNoteDTO patientProviderNoteData)
        {
            if (GenericMethods.IsListNotEmpty(patientProviderNoteData?.FileDocuments))
            {
                foreach (var item in patientProviderNoteData.ProviderQuestions.Select((question, i) => new { i, question }))
                {
                    var question = item.question;
                    string answerValue = string.Empty;

                    string key = MobileConstants.IsMobilePlatform ? string.Concat("FileList", Constants.SYMBOL_DASH, question.QuestionID, item.i)
                        : string.Concat(question.QuestionTypeID, question.QuestionID.ToString(), item.i);

                    if (question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey)
                    {
                        if (patientProviderNoteData.FileDocuments.Any(x => x.UniqueKey == key))
                        {
                            patientProviderNoteData.FileDocuments.FirstOrDefault(x => x.UniqueKey == key).DocumentSourceID = patientProviderNoteData.QuestionnaireQuestionAnswers.FirstOrDefault(x => x.QuestionID == question.QuestionID).PatientAnswerID.ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Map and Save patient provider notes From Sever
        /// </summary>
        /// <param name="result">Data Sync Result</param>
        /// <param name="data">Jtoken Data From Sync call</param>
        /// <returns>Operation Status and No of records saved</returns>
        internal async Task MapAndSavePatientProviderNotesAsync(DataSyncModel result, JToken data)
        {
            try
            {
                PatientProviderNoteDTO noteData = new PatientProviderNoteDTO();
                noteData.PatientProviderNotes = MapProviderNotes(data, nameof(DataSyncDTO.PatientProviderNotes));
                await new PatientProviderNoteDatabase().SavePatientProviderNotesAsync(noteData).ConfigureAwait(false);
                result.RecordCount = noteData.PatientProviderNotes?.Count ?? 0 + noteData.PatientProviderNotes?.Count ?? 0;
            }
            catch (Exception ex)
            {
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Map and Save program notes From Sever
        /// </summary>
        /// <param name="result">Data Sync Result</param>
        /// <param name="data">Jtoken Data From Sync call</param>
        /// <returns>Operation Status and No of records saved</returns>
        internal async Task MapAndSaveProgramNotesAsync(DataSyncModel result, JToken data)
        {
            try
            {
                ProgramDTO noteData = new ProgramDTO();
                MapProgramNotes(data, noteData);
                if (GenericMethods.IsListNotEmpty(noteData.ProgramNotes))
                {
                    await new PatientProviderNoteDatabase().SaveProgramNotesAsync(noteData).ConfigureAwait(false);
                    result.RecordCount = 1;
                }
            }
            catch (Exception ex)
            {
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Map and Save program notes language data From Sever
        /// </summary>
        /// <param name="result">Data Sync Result</param>
        /// <param name="data">Jtoken Data From Sync call</param>
        /// <returns>Operation Status and No of records saved</returns>
        internal async Task MapAndSaveProgramNotes18NAsync(DataSyncModel result, JToken data)
        {
            try
            {
                ProgramDTO noteData = new ProgramDTO();
                MapProgramNotesI18N(data, noteData);
                if (GenericMethods.IsListNotEmpty(noteData.ProgramNotesI18N))
                {
                    await new PatientProviderNoteDatabase().SaveProgramNotesI18NAsync(noteData).ConfigureAwait(false);
                    result.RecordCount = noteData.ProgramNotesI18N?.Count ?? 0 + noteData.ProgramNotesI18N?.Count ?? 0;
                }
            }
            catch (Exception ex)
            {
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private async Task SyncToServerAsync(PatientProviderNoteDTO requestData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<PatientProviderNoteDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PATIENT_PROVIDER_NOTE_ASYNC_PATH,
                    ContentToSend = requestData,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    }
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                requestData.ErrCode = httpData.ErrCode;
                requestData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapPatientProviderNotes(JToken data, PatientProviderNoteDTO patientProviderNoteDTO)
        {
            if (patientProviderNoteDTO.RecordCount == -1)
            {
                if (patientProviderNoteDTO.PatientProviderNote?.ProviderNoteID != Guid.Empty)
                {
                    var patientProviderNoteJData = data[nameof(PatientProviderNoteDTO.PatientProviderNote)];
                    if (patientProviderNoteJData.HasValues)
                    {
                        patientProviderNoteDTO.PatientProviderNote = MapPatientProviderNote(patientProviderNoteJData);
                    }

                    patientProviderNoteDTO.PatientProviderNote.NoteDateTime = _essentials.ConvertToLocalTime(patientProviderNoteDTO.PatientProviderNote.NoteDateTime.Value);
                    patientProviderNoteDTO.QuestionnaireQuestionAnswers = MapPatientQuestionnaireQuestionAnswers(data, nameof(patientProviderNoteDTO.QuestionnaireQuestionAnswers));
                    patientProviderNoteDTO.FileDocuments = new FileService(_essentials).MapFileDocuments(data, nameof(patientProviderNoteDTO.FileDocuments));
                }
                patientProviderNoteDTO.PatientPrograms = GetPickerSource(data, nameof(PatientProviderNoteDTO.PatientPrograms), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), patientProviderNoteDTO.PatientProviderNote?.ProgramID ?? -1, false, null);

            }
            if (patientProviderNoteDTO.RecordCount == -2 || patientProviderNoteDTO.PatientProviderNote?.ProviderNoteID != Guid.Empty)
            {
                patientProviderNoteDTO.Providers = GetPickerSource(data, nameof(PatientProviderNoteDTO.Providers), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), patientProviderNoteDTO.PatientProviderNote?.CareGiverID ?? -1, false, null);
                patientProviderNoteDTO.ProgramNotes = GetPickerSource(data, nameof(PatientProviderNoteDTO.ProgramNotes), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), patientProviderNoteDTO.PatientProviderNote?.ProgramNoteID ?? -1, false, nameof(OptionModel.ParentOptionID));
            }
            if (patientProviderNoteDTO.RecordCount == -3 || patientProviderNoteDTO.PatientProviderNote?.ProviderNoteID != Guid.Empty)
            {
                patientProviderNoteDTO.QuestionConditions = MapQuestionsConditions(data, nameof(patientProviderNoteDTO.QuestionConditions));
                patientProviderNoteDTO.QuestionnaireQuestions = MapQuestionnaireQuestions(data, nameof(patientProviderNoteDTO.QuestionnaireQuestions));
                patientProviderNoteDTO.QuestionnaireQuestionOptions = MapQuestionOptions(data, nameof(patientProviderNoteDTO.QuestionnaireQuestionOptions));
                patientProviderNoteDTO.Files = new FileService(_essentials).MapFiles(data, nameof(patientProviderNoteDTO.Files));
            }
            if (patientProviderNoteDTO.RecordCount >= 0)
            {
                patientProviderNoteDTO.PatientProviderNotes = MapProviderNotes(data, nameof(patientProviderNoteDTO.PatientProviderNotes));
                patientProviderNoteDTO.QuestionnaireQuestions = MapQuestionnaireQuestions(data, nameof(patientProviderNoteDTO.QuestionnaireQuestions));
                patientProviderNoteDTO.QuestionnaireQuestionOptions = MapQuestionOptions(data, nameof(patientProviderNoteDTO.QuestionnaireQuestionOptions));
            }
        }

        private void AddResourcesDynamically(PatientProviderNoteDTO patientProviderNoteData)
        {
            if (GenericMethods.IsListNotEmpty(patientProviderNoteData?.QuestionnaireQuestions))
            {
                if (GenericMethods.IsListNotEmpty(patientProviderNoteData.Resources))
                {
                    patientProviderNoteData.Resources?.OrderBy(x => x.GroupName);
                    var resourcesToRemoved = patientProviderNoteData.Resources.Where(x => x.GroupName == GroupConstants.RS_PROVIDER_NOTE_QUESTIONNAIRE_GROUP).ToList();
                    var indexOfFirst = patientProviderNoteData.Resources.IndexOf(patientProviderNoteData.Resources.FirstOrDefault(x => x.GroupName == GroupConstants.RS_PROVIDER_NOTE_QUESTIONNAIRE_GROUP));
                    if (indexOfFirst > 0)
                    {
                        patientProviderNoteData.Resources.RemoveRange(indexOfFirst, resourcesToRemoved.Count);
                    }
                }
                else
                {
                    patientProviderNoteData.Resources = new List<ResourceModel>();
                }
                foreach (var question in patientProviderNoteData.QuestionnaireQuestions)
                {
                    AddResource(patientProviderNoteData, question, string.Concat(question.QuestionTypeID, question.QuestionID.ToString()));
                }
            }
        }

        private FieldTypes GetFieldType(QuestionType questionType)
        {
            return questionType switch
            {
                QuestionType.TextQuestionKey => FieldTypes.TextEntryControl,
                QuestionType.NumericQuestionKey => FieldTypes.NumericEntryControl,
                QuestionType.MultilineTextQuestionKey => FieldTypes.MultiLineEntryControl,
                QuestionType.DateQuestionKey => FieldTypes.DateControl,
                QuestionType.TimeQuestionKey => FieldTypes.TimeControl,
                QuestionType.DateTimeQuestionKey => FieldTypes.DateTimeControl,
                QuestionType.SingleSelectQuestionKey => FieldTypes.VerticalRadioButtonControl,
                QuestionType.DropDownQuestionKey => FieldTypes.SingleSelectDropdownControl,
                QuestionType.MultiSelectQuestionKey => FieldTypes.VerticalCheckBoxControl,
                QuestionType.VerticalSliderQuestionKey => FieldTypes.VerticalSliderControl,
                QuestionType.HorizontalSliderQuestionKey => FieldTypes.HorizontalSliderControl,
                QuestionType.FilesAndDocumentQuestionKey => FieldTypes.UploadControl,
                _ => default,
            };
        }

        private void AddResource(BaseDTO patientProviderNoteData, QuestionnaireQuestionModel question, string resourceKey)
        {
            ResourceModel dynamicResource = new ResourceModel
            {
                KeyDescription = question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey ? LibSettings.GetSettingValueByKey(patientProviderNoteData.Settings, SettingsConstants.S_UPLOAD_SUPPORTED_FILE_TYPE_KEY) : null,
                ResourceKey = resourceKey,
                FieldType = GetFieldType(question.QuestionTypeID).ToString(),
                GroupName = GroupConstants.RS_PROVIDER_NOTE_QUESTIONNAIRE_GROUP,
                ResourceValue = question?.CaptionText,
                PlaceHolderValue = question.AnswerPlaceHolder,
                MinLength = question.MinValue,
                MaxLength = question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey ? 1 : question.MaxValue,
                IsRequired = question.IsRequired,
                LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0)
            };
            if(question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey)
            {
                dynamicResource.ResourceValue = GenericMethods.HtmlToPlainText(Regex.Replace(question?.CaptionText, Constants.HTML_TAGS, String.Empty));
                dynamicResource.PlaceHolderValue = LibResources.GetResourceByKey(patientProviderNoteData.Resources, ResourceConstants.R_DOCUMENT_TEXT_KEY).PlaceHolderValue;
                dynamicResource.ResourceKey = resourceKey;
                dynamicResource.MinLength = question.MinValue;
                dynamicResource.MaxLength = 1;
                dynamicResource.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            }
            patientProviderNoteData.Resources.Add(dynamicResource);
        }

        /// <summary>
        /// Maps the question text and answers values to questionnaireQuestion
        /// </summary>
        /// <param name="questionnaireQuestions">Questions in which question text and answer value to be mapped</param>
        /// <param name="questionnaireQuestionDetails">Question Details that contains language specific question and answer value</param>
        public void MapQuestionDetails(List<QuestionnaireQuestionModel> questionnaireQuestions, List<QuestionnaireQuestionDetailsModel> questionnaireQuestionDetails)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireQuestions))
            {
                foreach (var ques in questionnaireQuestions)
                {
                    ques.CaptionText = questionnaireQuestionDetails.FirstOrDefault(x => x.QuestionID == ques.QuestionID && x.AnswerPlaceHolder == ques.ProviderNoteID)?.CaptionText ?? string.Empty;
                    ques.InstructionsText = questionnaireQuestionDetails.FirstOrDefault(x => x.QuestionID == ques.QuestionID && x.AnswerPlaceHolder == ques.ProviderNoteID)?.InstructionsText ?? string.Empty;
                }
            }
        }

        private void GetProviderListUI(PatientProviderNoteDTO providerNoteData)
        {
            if (providerNoteData.ErrCode == ErrorCode.OK)
            {
                if (providerNoteData.RecordCount >= 0)
                {
                    providerNoteData.Providers = new List<OptionModel>();
                    LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                    string childrenUI = string.Empty;
                    if (GenericMethods.IsListNotEmpty(providerNoteData.PatientProviderNotes))
                    {
                        foreach (var note in providerNoteData.PatientProviderNotes)
                        {
                            string noteFormattedDate = GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(note.NoteDateTime.Value), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                            var questionAnswers = providerNoteData.QuestionnaireQuestions.Where(x => x.ProviderNoteID == note.ProviderNoteID.ToString()).ToList();
                            if (GenericMethods.IsListNotEmpty(questionAnswers))
                            {
                                childrenUI = GetQuestionAnswerList(questionAnswers, providerNoteData.QuestionnaireQuestionOptions, dayFormat, monthFormat, yearFormat, false);
                            }
                            else
                            {
                                continue;
                            }
                            var finalDiv = $"<div style='{StyleConstants.WIDTH_WEBKIT_FILL_AVAILABLE}'>" +
                                $"<div style= 'border-left:{note.ProgramGroupIdentifier} 4px solid; border-right:unset;'>" +
                                $"<div style= 'display: -webkit-box !important;display: flex !important; padding: 10px 0px'> " +
                                $"<table style='width: 100%;'>" +
                                "<tbody>" +
                                "<tr>" +
                                $"<td style = '{StyleConstants.WIDTH_WEBKIT_FILL_AVAILABLE} padding-left: 10px;'> " +
                                $"<label style='{StyleConstants.WIDTH_WEBKIT_FILL_AVAILABLE} max-width: 500px; color:#212121; font:-size:14px;  font-weight:600;'><b>{LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_BY_KEY)} : {note.UserName} </b> </label> <br>" +
                                $"<label style='{StyleConstants.WIDTH_WEBKIT_FILL_AVAILABLE} font-size:13px; font-weight:300'>{LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_PROGRAM_TITLE_KEY)} : {note.ProgramName} </label> " +
                                $"</td> " +
                                $"<td style='text-align: right;'>" +
                                $"<label style='font-size:13px; font-weight:300; color:black; width: max-content;'>{LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DATE_KEY)} {noteFormattedDate} </label> " +
                                $"</td> " +
                                "</tr>" +
                                "</tbody>" +
                                $"</table> " +
                                $"</div> " +
                                $"</div> " +
                                $"<hr {StyleConstants.HR_TAG_STYLE}></hr>" +
                                $"{childrenUI}" +
                                $" </div> ";
                            providerNoteData.Providers.Add(new OptionModel { ParentOptionText = note.ProviderNoteID.ToString(), OptionText = finalDiv });
                            childrenUI = string.Empty;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Format questions and answers data of  for UI
        /// </summary>
        /// <param name="questionAnswers">List questions and answers</param>
        /// <param name="questionnaireQuestionOption"> List of Answer Options</param>
        /// <param name="dayFormat">Day format</param>
        /// <param name="monthFormat">Month Format</param>
        /// <param name="yearFormat">Year Foramt</param>
        /// <param name="isTaskPage"> IF the request is coming from Task Page</param>
        public string GetQuestionAnswerList(List<QuestionnaireQuestionModel> questionAnswers, List<QuestionnaireQuestionOptionModel> questionnaireQuestionOption, string dayFormat, string monthFormat, string yearFormat, bool isTaskPage)
        {
            string childrenUI = string.Empty;
            if (GenericMethods.IsListNotEmpty(questionAnswers))
            {
                foreach (var question in questionAnswers)
                {

                    if (question.QuestionTypeID != QuestionType.RichTextQuestionKey && !string.IsNullOrWhiteSpace(question.InstructionsText))
                    {
                        question.CaptionText = GenericMethods.HtmlToPlainText(Regex.Replace(question.CaptionText, Constants.HTML_TAGS, String.Empty));
                        if (isTaskPage)
                        {
                            childrenUI += $"<div  style = '{StyleConstants.WIDTH_WEBKIT_FILL_AVAILABLE} padding-top: 5px;'>" +
                                $"<div style = 'padding:5px 0px'>" +
                            $"<label {StyleConstants.QUESTION_LABEL_STYLE}>{question.CaptionText}</label><br>";
                        }
                        else
                        {
                            childrenUI += $"<div  style = '{StyleConstants.WIDTH_WEBKIT_FILL_AVAILABLE} padding-left: 15px;'> <div style = 'padding:5px 10px'>" +
                                  $"<label  {StyleConstants.QUESTION_LABEL_STYLE}>{question.CaptionText}</label><br>";
                        }
                        switch (question.QuestionTypeID)
                        {
                            case QuestionType.SingleSelectQuestionKey:
                            case QuestionType.DropDownQuestionKey:
                                if (!string.IsNullOrWhiteSpace(question.InstructionsText))
                                {
                                    Int64 value = 0;
                                    if (Int64.TryParse(question.InstructionsText, out value))
                                    {
                                        question.InstructionsText = questionnaireQuestionOption.FirstOrDefault(x => x.QuestionOptionID == value)?.CaptionText;
                                    }
                                    childrenUI += $"<label {StyleConstants.ANS_LABEL_STYLE}>{question.InstructionsText}</label>";
                                }
                                else
                                {
                                    childrenUI += $"<label {StyleConstants.ANS_LABEL_STYLE}>{Constants.SYMBOL_DOUBLE_HYPHEN}</label>";
                                }
                                break;
                            case QuestionType.MultiSelectQuestionKey:
                                if (!string.IsNullOrWhiteSpace(question.InstructionsText))
                                {
                                    String[] result = question.InstructionsText.Split(Constants.SYMBOL_PIPE_SEPERATOR);
                                    for (int i = 0; i < result.Length; i++)
                                    {
                                        Int64 value = 0;
                                        if (Int64.TryParse(result[i], out value))
                                        {
                                            question.InstructionsText = questionnaireQuestionOption.FirstOrDefault(x => x.QuestionOptionID == value)?.CaptionText;
                                        }
                                        childrenUI +=
                                        $"<li {StyleConstants.ANS_LABEL_STYLE}>{question.InstructionsText}</li>";
                                    }
                                }
                                else
                                {
                                    childrenUI += $"<label {StyleConstants.ANS_LABEL_STYLE}>{Constants.SYMBOL_DOUBLE_HYPHEN}</label>";
                                }
                                break;
                            case QuestionType.TimeQuestionKey:
                                if (!string.IsNullOrWhiteSpace(question.InstructionsText))
                                {
                                    question.InstructionsText = GenericMethods.GetLocalDateTimeBasedOnCulture(Convert.ToDateTime(question.InstructionsText, CultureInfo.InvariantCulture), DateTimeType.Time, dayFormat, monthFormat, yearFormat);
                                    childrenUI += $"<label {StyleConstants.ANS_LABEL_STYLE}>{question.InstructionsText}</label>";
                                }
                                else
                                {
                                    childrenUI += $"<label {StyleConstants.ANS_LABEL_STYLE}>{Constants.SYMBOL_DOUBLE_HYPHEN}</label>";
                                }
                                break;
                            case QuestionType.DateQuestionKey:
                                if (!string.IsNullOrWhiteSpace(question.InstructionsText))
                                {
                                    question.InstructionsText = GenericMethods.GetLocalDateTimeBasedOnCulture(Convert.ToDateTime(question.InstructionsText, CultureInfo.InvariantCulture), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                                    childrenUI += $"<label  {StyleConstants.ANS_LABEL_STYLE} >{question.InstructionsText}</label>";
                                }
                                else
                                {
                                    childrenUI += $"<label  {StyleConstants.ANS_LABEL_STYLE} >{Constants.SYMBOL_DOUBLE_HYPHEN}</label>";
                                }
                                break;
                            case QuestionType.DateTimeQuestionKey:
                                if (!string.IsNullOrWhiteSpace(question.InstructionsText))
                                {
                                    question.InstructionsText = GenericMethods.GetLocalDateTimeBasedOnCulture(Convert.ToDateTime(question.InstructionsText, CultureInfo.InvariantCulture), default, dayFormat, monthFormat, yearFormat);
                                    childrenUI += $"<label  {StyleConstants.ANS_LABEL_STYLE} >{question.InstructionsText}</label>";
                                }
                                else
                                {
                                    childrenUI += $"<label  {StyleConstants.ANS_LABEL_STYLE} >{Constants.SYMBOL_DOUBLE_HYPHEN}</label>";
                                }
                                break;
                            case QuestionType.MeasurementQuestionKey:
                                if (string.IsNullOrEmpty(question.InstructionsText))
                                {
                                    childrenUI += $"<label  {StyleConstants.ANS_LABEL_STYLE} >{Constants.SYMBOL_DOUBLE_HYPHEN}</label>";
                                }
                                else
                                {
                                    childrenUI += $"<label  {StyleConstants.ANS_LABEL_STYLE} >{(string.IsNullOrEmpty(question.InstructionsText) ? string.Empty : ((int)float.Parse(question.InstructionsText)).ToString())}</label>";
                                }
                                break;
                            default:
                                if (string.IsNullOrEmpty(question.InstructionsText))
                                {
                                    childrenUI += $"<label  {StyleConstants.ANS_LABEL_STYLE} >{Constants.SYMBOL_DOUBLE_HYPHEN}</label>";
                                }
                                else
                                {
                                    childrenUI += $"<label  {StyleConstants.ANS_LABEL_STYLE} >{question.InstructionsText}</label>";
                                }
                                break;
                        }
                        childrenUI += $"</div><hr  {StyleConstants.HR_TAG_STYLE}></hr>" +
                                    $"</div>";
                    }
                }
            }
            return childrenUI;
        }


        private void GetProviderQuestionsInOrder(PatientProviderNoteDTO providerNoteData)
        {
            if (GenericMethods.IsListNotEmpty(providerNoteData?.QuestionnaireQuestions))
            {
                long? nextQuestionID = providerNoteData.QuestionnaireQuestions.FirstOrDefault(x => x.IsStartingQuestion)?.QuestionID;
                bool isNotLastQuestion = nextQuestionID > 0;
                providerNoteData.ProviderQuestions = new List<QuestionnaireQuestionModel>();
                while (isNotLastQuestion)
                {
                    QuestionnaireQuestionModel question = providerNoteData.QuestionnaireQuestions.FirstOrDefault(x => x.QuestionID == nextQuestionID);
                    providerNoteData.ProviderQuestions.Add(new QuestionnaireQuestionModel
                    {
                        QuestionID = question.QuestionID,
                        QuestionTypeID = question.QuestionTypeID,
                        SliderSteps = question.SliderSteps,
                        IsRequired = question.IsRequired,
                        CaptionText = question.CaptionText
                    });
                    switch (question.QuestionTypeID)
                    {
                        case QuestionType.TextQuestionKey:
                        case QuestionType.MultilineTextQuestionKey:
                        case QuestionType.FilesAndDocumentQuestionKey:
                        case QuestionType.DateQuestionKey:
                        case QuestionType.DateTimeQuestionKey:
                        case QuestionType.TimeQuestionKey:
                        case QuestionType.NumericQuestionKey:
                        case QuestionType.HorizontalSliderQuestionKey:
                        case QuestionType.VerticalSliderQuestionKey:
                            nextQuestionID = question.IsRequired ?
                             providerNoteData.QuestionConditions.FirstOrDefault(x => x.QuestionID == question.QuestionID && x.Value1.ToString() == Constants.FIFTEENTHOUSAND)?.TargetQuestionID :
                             providerNoteData.QuestionConditions.FirstOrDefault(x => x.QuestionID == question.QuestionID && x.Value1.ToString() == Constants.TENTHOUSAND)?.TargetQuestionID;
                            break;
                        case QuestionType.SingleSelectQuestionKey:
                        case QuestionType.MultiSelectQuestionKey:
                        case QuestionType.RichTextQuestionKey:
                        case QuestionType.DropDownQuestionKey:
                            nextQuestionID = providerNoteData.QuestionConditions.FirstOrDefault(x => x.QuestionID == question.QuestionID)?.TargetQuestionID;
                            break;
                    }
                    if (nextQuestionID == null || nextQuestionID == 0 || providerNoteData.ProviderQuestions?.Count > Convert.ToInt32(LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_MAX_QUESTIONS_ALLOWED_KEY)))
                    {
                        isNotLastQuestion = false;
                    }
                }
            }
        }
        /// <summary>
        /// Formats DateTime to DateTime String 
        /// </summary>
        /// <param name="dayFormat">Day format</param>
        /// <param name="monthFormat">Month Format</param>
        /// <param name="yearFormat">Year Foramt</param>
        private void GetDateFormats(out string dayFormat, out string monthFormat, out string yearFormat)
        {
            dayFormat = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_DATE_DAY_FORMAT_KEY);
            monthFormat = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_MONTH_FORMAT_KEY);
            yearFormat = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_YEAR_FORMAT_KEY);
        }
        #region Notes Data Mapping

        private void MapProgramNotes(JToken data, ProgramDTO noteData)
        {
            noteData.ProgramNotes = (data[nameof(noteData.ProgramNotes)].Any()) ?
                 (from dataItem in data[nameof(PatientProviderNoteDTO.ProgramNotes)]
                  select new ProgramNoteModel
                  {
                      ProgramNoteID = GetDataItem<long>(dataItem, nameof(ProgramNoteModel.ProgramNoteID)),
                      ProgramID = GetDataItem<long>(dataItem, nameof(ProgramNoteModel.ProgramID)),
                      QuestionnaireID = GetDataItem<long>(dataItem, nameof(ProgramNoteModel.QuestionnaireID)),
                      IsActive = GetDataItem<bool>(dataItem, nameof(ProgramNoteModel.IsActive))
                  }).ToList() : new List<ProgramNoteModel>();
        }

        private void MapProgramNotesI18N(JToken data, ProgramDTO noteData)
        {
            noteData.ProgramNotesI18N = (data[nameof(noteData.ProgramNotesI18N)].Any()) ?
                 (from dataItem in data[nameof(ProgramDTO.ProgramNotesI18N)]
                  select new ProgramNoteI18NModel
                  {
                      ProgramNoteID = GetDataItem<long>(dataItem, nameof(ProgramNoteI18NModel.ProgramNoteID)),
                      LanguageID = GetDataItem<byte>(dataItem, nameof(ProgramNoteI18NModel.LanguageID)),
                      NoteText = GetDataItem<string>(dataItem, nameof(ProgramNoteI18NModel.NoteText)),
                      IsActive = GetDataItem<bool>(dataItem, nameof(ProgramNoteI18NModel.IsActive))
                  }).ToList() : new List<ProgramNoteI18NModel>();
        }
        #endregion
    }
}
