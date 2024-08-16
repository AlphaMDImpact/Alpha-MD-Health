using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public partial class QuestionnaireService : BaseService
    {
        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveQuestionnaireAsync(DataSyncModel result, JToken data)
        {
            try
            {
                QuestionnaireDTO questionnaireData = new QuestionnaireDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    Questionnaires = MapQuestionnaires(data, nameof(DataSyncDTO.Questionnaires)),
                    Questions = MapQuestionnaireQuestions(data, nameof(DataSyncDTO.QuestionnaireQuestions)),
                    QuestionOptions = MapQuestionOptions(data, nameof(DataSyncDTO.QuestionnaireQuestionOptions)),
                    QuestionnaireSubscales = MapQuestionnaireSubscales(data, nameof(DataSyncDTO.QuestionnaireSubscales)),
                    QuestionnaireSubscaleRanges = MapQuesionnaireSubscaleRanges(data, nameof(DataSyncDTO.QuestionnaireSubscaleRanges)),
                    QuestionnaireQuestionAnswers = MapPatientQuestionnaireQuestionAnswers(data, nameof(DataSyncDTO.PatientQuestionnaireQuestionAnswers)),
                    QuestionnaireScores = MapQuestionScores(data, nameof(DataSyncDTO.PatientQuestionnaireScores)),
                    QuestionConditions = MapQuestionsConditions(data, nameof(DataSyncDTO.QuestionConditions)),
                    QuestionScores = MapQuestionnaireQuestionScores(data, nameof(DataSyncDTO.QuestionScores))
                };
                if (GenericMethods.IsListNotEmpty(questionnaireData.Questionnaires) || GenericMethods.IsListNotEmpty(questionnaireData.Questions)
                    || GenericMethods.IsListNotEmpty(questionnaireData.QuestionOptions) || GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireSubscales)
                    || GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireSubscaleRanges) || GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireQuestionAnswers)
                    || GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireScores) || GenericMethods.IsListNotEmpty(questionnaireData.QuestionConditions)
                    || GenericMethods.IsListNotEmpty(questionnaireData.QuestionScores))
                {
                    await new QuestionnaireDatabase().SaveQuetionnairesAsync(questionnaireData).ConfigureAwait(false);
                    result.RecordCount = 1;
                }
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
        }

        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveQuestionnaireI18NAsync(DataSyncModel result, JToken data)
        {
            try
            {
                QuestionnaireDTO questionnaireData = new QuestionnaireDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    QuestionnaireDetails = MapQuestionnaireDetails(data, nameof(DataSyncDTO.QuestionnaireDetails)),
                    QuestionDetails = MapQuestionDetails(data, nameof(DataSyncDTO.QuestionnaireQuestionDetails)),
                    QuestionOptionDetails = MapQuestionOptionDetails(data, nameof(DataSyncDTO.QuestionnaireQuestionOptionDetails)),
                    QuestionnaireSubscaleRangeDetails = MapQuestionnaireSubscaleRangeDetails(data, nameof(DataSyncDTO.QuestionnaireRecommendations)),
                };
                if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireDetails) || GenericMethods.IsListNotEmpty(questionnaireData.QuestionDetails)
                    || GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireQuestionOptionDetails) || GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireSubscaleRangeDetails))
                {
                    await new QuestionnaireDatabase().SaveQuetionnairesAsync(questionnaireData).ConfigureAwait(false);
                    result.RecordCount = 1;
                }
                _ = ImageMappingAsync().ConfigureAwait(false);
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
        }

        /// <summary>
        /// Sync Patient Program results to server
        /// </summary>
        /// <param name="requestData">patient Questionnaire data to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        internal async Task SyncQuestionnaireResultsToServerAsync(QuestionnaireDTO requestData, CancellationToken cancellationToken)
        {
            try
            {
                await new QuestionnaireDatabase().GetPatientQuestionnaireResultsForSyncAsync(requestData).ConfigureAwait(false);
                if (GenericMethods.IsListNotEmpty(requestData.QuestionnaireQuestionAnswers) || GenericMethods.IsListNotEmpty(requestData.QuestionnaireScores))
                {
                    var httpData = new HttpServiceModel<QuestionnaireDTO>
                    {
                        CancellationToken = cancellationToken,
                        PathWithoutBasePath = UrlConstants.SAVE_PATIENT_QUESTIONNAIRE_RESULTS_ASYNC_PATH,
                        ContentToSend = requestData,
                        QueryParameters = new NameValueCollection
                        {
                            { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        }
                    };
                    await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                    requestData.ErrCode = httpData.ErrCode;
                    if (requestData.ErrCode == ErrorCode.OK)
                    {
                        JToken data = JToken.Parse(httpData.Response);
                        if (data?.HasValues == true)
                        {
                            requestData.QuestionnaireQuestionAnswerResult = MapSaveResponse(data, nameof(QuestionnaireDTO.QuestionnaireQuestionAnswerResult));
                            requestData.QuestionnaireScoreResult = MapSaveResponse(data, nameof(QuestionnaireDTO.QuestionnaireScoreResult));
                            await new QuestionnaireDatabase().UpdatePatientQuestionnaireResultsStatusAsync(requestData).ConfigureAwait(false);
                            if (requestData.ErrCode == ErrorCode.DuplicateGuid)
                            {
                                requestData.ErrCode = ErrorCode.OK;
                                await SyncQuestionnaireResultsToServerAsync(requestData, cancellationToken).ConfigureAwait(false);
                            }
                            requestData.RecordCount = 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get questionnaire page data
        /// </summary>
        /// <param name="questionnaireData">Reference object to hold questionnaire page data</param>
        /// <returns>Questionnaire page data with operation status</returns>
        public async Task GetQuestionnaireAsync(QuestionnaireDTO questionnaireData, short readingCategoryID = 0)
        {
            try
            {
                questionnaireData.SelectedUserID = GetUserID();
                if (MobileConstants.IsMobilePlatform)
                {
                    await Task.WhenAll(
                        GetResourcesAsync(GroupConstants.RS_USER_TYPE_GROUP, GroupConstants.RS_TASK_GROUP, GroupConstants.RS_PATIENT_DOCUMNET_GROUP),
                        GetSettingsAsync(GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, GroupConstants.RS_TASK_GROUP, GroupConstants.RS_COMMON_GROUP),
                        new QuestionnaireDatabase().GetQuestionnaireAsync(questionnaireData)
                    ).ConfigureAwait(false);
                    // Map resources and next question
                    MapQuestionnaireDynamicResource(questionnaireData);
                    questionnaireData.ErrCode = ErrorCode.OK;
                }
                else
                {
                    await SyncNextQuestionFromServerAsync(questionnaireData, readingCategoryID).ConfigureAwait(false);
                }
                if (questionnaireData.ErrCode == ErrorCode.OK)
                {
                    MapDataIntoResource(questionnaireData);
                    MapFileDocuments(questionnaireData);
                }
            }
            catch (Exception ex)
            {
                questionnaireData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get previous/next patient question for the selected Questionnaire
        /// </summary>
        /// <param name="questionnaireData">Questionnaire data reference object</param>
        /// <param name="action">Action to get data for next page</param>
        /// <returns>Operation Status</returns>
        public async Task GetQuestionAsync(QuestionnaireDTO questionnaireData, string action)
        {
            QuestionnaireDTO questionnaire = new QuestionnaireDTO
            {
                Questionnaire = questionnaireData.Questionnaire,
                QuestionnaireDetail = questionnaireData.QuestionnaireDetail,
                Question = questionnaireData.Question,
                QuestionDetail = questionnaireData.QuestionDetail,
                QuestionOptions = questionnaireData.QuestionOptions,
                QuestionnaireQuestionAnswer = questionnaireData.QuestionnaireQuestionAnswer,
                QuestionnaireSubscaleRangeDetails = questionnaireData.QuestionnaireSubscaleRangeDetails,
            };
            try
            {
                questionnaireData.SelectedUserID = GetUserID();
                await new QuestionnaireDatabase().GetQuestionAsync(questionnaireData, action).ConfigureAwait(false);
                MapFileDocuments(questionnaireData);
                MapQuestionnaireDynamicResource(questionnaireData);
                MapDataIntoResource(questionnaireData);
                questionnaireData.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                questionnaireData.Questionnaire = questionnaire.Questionnaire;
                questionnaireData.QuestionnaireDetail = questionnaire.QuestionnaireDetail;
                questionnaireData.Question = questionnaire.Question;
                questionnaireData.QuestionDetail = questionnaire.QuestionDetail;
                questionnaireData.QuestionOptions = questionnaire.QuestionOptions;
                questionnaireData.QuestionnaireQuestionAnswer = questionnaire.QuestionnaireQuestionAnswer;
                questionnaireData.QuestionnaireSubscaleRangeDetails = questionnaire.QuestionnaireSubscaleRangeDetails;
                questionnaireData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get next/previous question from service
        /// </summary>
        /// <param name="questionnaireData">taskData reference to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Next/previous question and answer received from server</returns>
        public async Task SyncNextQuestionFromServerAsync(QuestionnaireDTO questionnaireData, short readingCategoryID)
        {
            try
            {
                MapQuestionnaireInputData(questionnaireData);
                var httpData = new HttpServiceModel<QuestionnaireDTO>
                {
                    CancellationToken = CancellationToken.None,
                    PathWithoutBasePath = UrlConstants.GET_NEXT_QUESTION_ASYNC_PATH,
                    ContentToSend = questionnaireData,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { nameof(TaskModel.PatientTaskID), Convert.ToString(questionnaireData.PatientTaskID, CultureInfo.InvariantCulture) },
                        { nameof(ReadingModel.ReadingCategoryID),Convert.ToString(readingCategoryID, CultureInfo.InvariantCulture) }
                    }
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                questionnaireData.ErrCode = httpData.ErrCode;
                if (questionnaireData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        if (!MobileConstants.IsMobilePlatform)
                        {

                            MapCommonData(questionnaireData, data);
                            MapQuestionnaireTaskData(data, questionnaireData);

                            if (questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.StartQuestionnaire)
                            {
                                ResourceModel startResource = new ResourceModel
                                {
                                    ResourceKey = ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_START_KEY,
                                    ResourceValue = LibResources.GetResourceValueByKey(questionnaireData.Resources, ResourceConstants.R_START_ACTION_KEY),
                                    KeyDescription = ImageConstants.I_QUESTIONNAIRE_TASK_ICON_SVG,
                                    GroupName = GroupConstants.RS_PROVIDER_NOTE_QUESTIONNAIRE_GROUP,
                                    InfoValue = questionnaireData.Questionnaire.InstructionsText,
                                    LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0)
                                };
                                questionnaireData.Resources.Add(startResource);
                            }
                            else if (questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.Finish
                                && GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireSubscaleRangeDetails))
                            {
                                var doneResource = LibResources.GetResourceByKey(questionnaireData.Resources, ResourceConstants.R_TASK_COMPLETED_MESSAGE_KEY);
                                doneResource.InfoValue = questionnaireData.QuestionnaireSubscaleRangeDetails[0].CaptionText;
                            }
                            if (questionnaireData.Question?.QuestionID > 0)
                            {
                                AddResource(questionnaireData, questionnaireData.Question, ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY);
                            }
                            SetResourcesAndSettings(questionnaireData);
                            questionnaireData.ReadingCategoryID = (short)data[nameof(QuestionnaireDTO.ReadingCategoryID)];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                questionnaireData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }


        private void MapQuestionnaireInputData(QuestionnaireDTO questionnaireData)
        {
            if (questionnaireData.QuestionnaireQuestionAnswer.QuestionID > 0 && questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID == Guid.Empty)
            {
                questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID = GenericMethods.GenerateGuid();
                if (GenericMethods.IsListNotEmpty(questionnaireData.FileDocuments))
                {
                    questionnaireData.FileDocuments[0].DocumentSourceID = questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID.ToString();
                }
            }
        }

        private void MapQuestionnaireTaskData(JToken data, QuestionnaireDTO questionnaireData)
        {
            questionnaireData.Questionnaire = MapQuestionnaire(data[nameof(QuestionnaireDTO.Questionnaire)]);
            if (questionnaireData.Questionnaire.QuestionnaireID > 0)
            {
                questionnaireData.Question = MapQuestionnaireQuestion(data[nameof(QuestionnaireDTO.Question)]);
                questionnaireData.QuestionOptions = MapQuestionOptions(data, nameof(QuestionnaireDTO.QuestionOptions));
                questionnaireData.QuestionnaireQuestionAnswer = data[nameof(QuestionnaireDTO.QuestionnaireQuestionAnswer)].Any() ? MapQuestionAnswer(data[nameof(QuestionnaireDTO.QuestionnaireQuestionAnswer)]) : null;
                questionnaireData.FileDocuments = new FileService(_essentials).MapFileDocuments(data, nameof(QuestionnaireDTO.FileDocuments));
            }
            else
            {
                questionnaireData.Question = new QuestionnaireQuestionModel();
                questionnaireData.QuestionOptions = null;
                questionnaireData.QuestionnaireQuestionAnswer = null;
                questionnaireData.QuestionnaireQuestionAnswers = null;
                questionnaireData.FileDocument = null;
            }
            if (questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.Finish)
            {
                questionnaireData.QuestionnaireSubscaleRangeDetails = MapQuestionnaireSubscaleRangeDetails(data, nameof(QuestionnaireDTO.QuestionnaireSubscaleRangeDetails));
            }
        }

        private void MapFileDocuments(QuestionnaireDTO questionnaireData)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.FileDocuments))
            {
                LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                string deleteValue = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DELETE_ACTION_KEY);
                questionnaireData.FileDocuments.ForEach(document =>
                {
                    FileService fileService = new FileService(_essentials)
                    {
                        PageData = PageData
                    };
                    fileService.FormatDocumentData(dayFormat, monthFormat, yearFormat, deleteValue, document);
                });
            }
        }

        private void MapDataIntoResource(QuestionnaireDTO questionnaireData)
        {
            if (questionnaireData.Questionnaire.QuestionnaireAction != QuestionnaireAction.StartQuestionnaire
                && questionnaireData.Questionnaire.QuestionnaireAction != QuestionnaireAction.Done)
            {
                MapQuestionnaireQuestionAnswer(questionnaireData);
            }
        }

        private async Task ImageMappingAsync()
        {
            QuestionnaireDTO questionnaireData = new QuestionnaireDTO();
            using (QuestionnaireDatabase dbLayerObject = new QuestionnaireDatabase())
            {
                await dbLayerObject.GetQuestionnaireDetailsToDownloadImageAsync(questionnaireData).ConfigureAwait(false);
                await Task.WhenAll(
                    GetQuestionnaireDetailImagesAsync(questionnaireData),
                    GetQuestionnaireSubscaleRangeDetailsImagesAsync(questionnaireData)
                ).ConfigureAwait(false);
                await dbLayerObject.SaveQuetionnairesAsync(questionnaireData).ConfigureAwait(false);
            }
        }

        private async Task GetQuestionnaireDetailImagesAsync(QuestionnaireDTO questionnaireData)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireDetails))
            {
                foreach (var item in questionnaireData.QuestionnaireDetails)
                {
                    item.InstructionsText = await GetImageContentAsync(item.InstructionsText).ConfigureAwait(false);
                    item.IsDataDownloaded = true;
                }
            }
        }

        private async Task GetQuestionnaireSubscaleRangeDetailsImagesAsync(QuestionnaireDTO questionnaireData)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireSubscaleRangeDetails))
            {
                foreach (var item in questionnaireData.QuestionnaireSubscaleRangeDetails)
                {
                    item.InstructionsText = await GetImageContentAsync(item.InstructionsText).ConfigureAwait(false);
                    item.IsDataDownloaded = true;
                }
            }
        }

        private void MapQuestionnaireDynamicResource(QuestionnaireDTO questionnaireData)
        {
            if (questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.StartQuestionnaire || questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.Done)
            {
                GetDataForStartOrDoneView(questionnaireData);
            }
            else
            {
                if (questionnaireData.Question != null)
                {
                    ResourceModel dynamicQuestionResource = LibResources.GetResourceByKey(PageData?.Resources, ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY);
                    if (dynamicQuestionResource == null)
                    {
                        dynamicQuestionResource = new ResourceModel
                        {
                            ResourceKey = ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY
                        };
                        PageData.Resources.Add(dynamicQuestionResource);
                    }
                    dynamicQuestionResource.ResourceValue = dynamicQuestionResource.PlaceHolderValue = questionnaireData.QuestionDetail.AnswerPlaceHolder;
                    dynamicQuestionResource.MaxLength = questionnaireData.Question.MaxValue;
                    dynamicQuestionResource.MinLength = questionnaireData.Question.MinValue;
                    dynamicQuestionResource.IsRequired = questionnaireData.Question.IsRequired;
                    // Update min and max length for time picker as min max lenght is not supported in time picker 
                    if (questionnaireData.Question.QuestionTypeID == QuestionType.TimeQuestionKey)
                    {
                        dynamicQuestionResource.MinLength = dynamicQuestionResource.MaxLength = 0;
                    }
                }
            }
        }

        private void GetDataForStartOrDoneView(QuestionnaireDTO questionnaireData)
        {
            ResourceModel dynamicQuestionResource;
            dynamicQuestionResource = questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.StartQuestionnaire ?
                new ResourceModel
                {
                    ResourceKey = QuestionnaireAction.StartQuestionnaire.ToString(),
                    ResourceValue = questionnaireData.QuestionnaireDetail.CaptionText,
                    InfoValue = questionnaireData.QuestionnaireDetail.InstructionsText,
                    KeyDescription = ImageConstants.I_QUESTIONNAIRE_TASK_ICON_SVG
                }
                : new ResourceModel
                {
                    ResourceKey = QuestionnaireAction.Done.ToString(),
                    ResourceValue = GetResourceValueForStartOrDoneView(questionnaireData),
                    InfoValue = questionnaireData.QuestionnaireSubscaleRangeDetails?.FirstOrDefault()?.InstructionsText ?? string.Empty,
                    KeyDescription = ImageConstants.I_TASK_COMPLETE_ICON_SVG
                };
            PageData.Resources.Add(dynamicQuestionResource);
        }

        private string GetResourceValueForStartOrDoneView(QuestionnaireDTO questionnaireData)
        {
            return string.IsNullOrWhiteSpace(questionnaireData.QuestionnaireSubscaleRangeDetails?.FirstOrDefault()?.CaptionText) ?
                                            LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_TASK_COMPLETED_MESSAGE_KEY) :
                                            questionnaireData.QuestionnaireSubscaleRangeDetails.FirstOrDefault().CaptionText;
        }

        /// <summary>
        /// Checks if questionnaire answers is patientReadingID
        /// </summary>
        /// <param name="patientReadingID"> patientReadingID </param>
        /// <returns>true if answer value is patientReadingID else false </returns>
        public async Task<bool> CheckIfAnsValueIsReadingIDAsync(string patientReadingID)
        {
            return await new QuestionnaireDatabase().CheckIfAnsValueIsReadingIDAsync(patientReadingID).ConfigureAwait(false);
        }
        private void MapQuestionnaireQuestionAnswer(QuestionnaireDTO questionnaireData)
        {
            if (questionnaireData.QuestionnaireQuestionAnswer == null)
            {
                questionnaireData.QuestionnaireQuestionAnswer = new PatientQuestionnaireQuestionAnswersModel
                {
                    PatientTaskID = questionnaireData.PatientTaskID.ToString(),
                    QuestionID = questionnaireData.Question.QuestionID,
                    AnswerValue = string.Empty,
                    ErrCode = ErrorCode.OK
                };
            }
            if (questionnaireData.Question != null
                && (questionnaireData.Question.QuestionTypeID == QuestionType.DropDownQuestionKey
                || questionnaireData.Question.QuestionTypeID == QuestionType.MultiSelectQuestionKey
                || questionnaireData.Question.QuestionTypeID == QuestionType.SingleSelectQuestionKey))
            {
                questionnaireData.DropDownOptions = (from item in questionnaireData.QuestionOptions select CreateDropDownOption(questionnaireData, item)).ToList();
                if (questionnaireData.Question.QuestionTypeID == QuestionType.DropDownQuestionKey)
                {
                    questionnaireData.DropDownOptions.Insert(0, new OptionModel
                    {
                        OptionID = -1,
                        OptionText = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DROP_DOWN_PLACE_HOLDER_KEY)
                    });
                }
                // Set default value for dropdown question when question is not answered
                if (string.IsNullOrWhiteSpace(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue)
                    && questionnaireData.Question.QuestionTypeID == QuestionType.DropDownQuestionKey)
                {
                    questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = Constants.CONSTANT_NEG_ONE;
                    questionnaireData.DropDownOptions.FirstOrDefault(x => x.OptionID == -1).IsSelected = true;
                }
            }
        }

        private OptionModel CreateDropDownOption(QuestionnaireDTO questionnaireData, QuestionnaireQuestionOptionModel item)
        {
            //Update selected value of current question options
            item.IsSelected = questionnaireData.QuestionnaireQuestionAnswer.AnswerValue?.Contains(item.QuestionOptionID.ToString(CultureInfo.InvariantCulture)) ?? false;
            return new OptionModel
            {
                OptionID = item.QuestionOptionID,
                OptionText = item.CaptionText,
                SequenceNo = item.SequenceNo,
                IsSelected = item.IsSelected
            };
        }

        private List<PatientQuestionnaireQuestionAnswersModel> MapQuestionAnswers(JToken data, string nameOfToken)
        {
            return data[nameOfToken].Any() ?
                                (from dataItem in data[nameOfToken]
                                 select new PatientQuestionnaireQuestionAnswersModel
                                 {
                                     PatientAnswerID = GetDataItem<Guid>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.PatientAnswerID)),
                                     PatientTaskID = GetDataItem<string>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.PatientTaskID)),
                                     QuestionID = GetDataItem<long>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.QuestionID)),
                                     AnswerValue = GetDataItem<string>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.AnswerValue)),
                                     // CaptionText = GetDataItem<string>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.CaptionText)),
                                     TaskType = GetDataItem<byte>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.TaskType)),
                                     ErrCode = ErrorCode.OK,
                                     IsSynced = true
                                 }).ToList() : new List<PatientQuestionnaireQuestionAnswersModel>();
        }

        private List<QuestionnaireSubscaleModel> MapQuestionnaireSubscales(JToken data, string nameOfToken)
        {
            return data[nameOfToken].Any() ?
                                (from dataItem in data[nameOfToken]
                                 select new QuestionnaireSubscaleModel
                                 {
                                     QuestionnaireID = GetDataItem<long>(dataItem, nameof(QuestionnaireSubscaleModel.QuestionnaireID)),
                                     SubscaleID = GetDataItem<long>(dataItem, nameof(QuestionnaireSubscaleModel.SubscaleID)),
                                     ScoreTypeID = GetDataItem<string>(dataItem, nameof(QuestionnaireSubscaleModel.ScoreTypeID)).ToEnum<QuestionnaireSubscaleScoreType>(),
                                     IsActive = GetDataItem<bool>(dataItem, nameof(QuestionnaireSubscaleModel.IsActive)),
                                 }).ToList() : new List<QuestionnaireSubscaleModel>();
        }

        private List<PatientQuestionnaireScoresModel> MapQuestionScores(JToken data, string nameOfToken)
        {
            return data[nameOfToken].Any() ?
                                (from dataItem in data[nameOfToken]
                                 select new PatientQuestionnaireScoresModel
                                 {
                                     PatientScoreID = GetDataItem<Guid>(dataItem, nameof(PatientQuestionnaireScoresModel.PatientScoreID)),
                                     SubScaleRangeID = GetDataItem<long>(dataItem, nameof(PatientQuestionnaireScoresModel.SubScaleRangeID)),
                                     PatientTaskID = GetDataItem<string>(dataItem, nameof(PatientQuestionnaireScoresModel.PatientTaskID)),
                                     UserScore = GetDataItem<double>(dataItem, nameof(PatientQuestionnaireScoresModel.UserScore)),
                                     ErrCode = ErrorCode.OK,
                                     IsSynced = true
                                 }).ToList() : new List<PatientQuestionnaireScoresModel>();
        }

        private List<QuestionnaireDetailsModel> MapQuestionnaireDetails(JToken data, string nameOfToken)
        {
            return data[nameOfToken].Any() ?
                                (from dataItem in data[nameOfToken]
                                 select new QuestionnaireDetailsModel
                                 {
                                     QuestionnaireID = GetDataItem<long>(dataItem, nameof(QuestionnaireDetailsModel.QuestionnaireID)),
                                     CaptionText = GetDataItem<string>(dataItem, nameof(QuestionnaireDetailsModel.CaptionText)),
                                     InstructionsText = GetDataItem<string>(dataItem, nameof(QuestionnaireDetailsModel.InstructionsText)),
                                     LanguageID = GetDataItem<byte>(dataItem, nameof(QuestionnaireDetailsModel.LanguageID)),
                                     IsActive = GetDataItem<bool>(dataItem, nameof(QuestionnaireDetailsModel.IsActive)),
                                     IsDataDownloaded = false
                                 }).ToList() : new List<QuestionnaireDetailsModel>();
        }

        private List<QuestionnaireSubscaleRangeDetailsModel> MapQuestionnaireSubscaleRangeDetails(JToken data, string nameOfToken)
        {
            return data[nameOfToken].Any() ?
                                (from dataItem in data[nameOfToken]
                                 select new QuestionnaireSubscaleRangeDetailsModel
                                 {
                                     SubScaleRangeID = GetDataItem<long>(dataItem, nameof(QuestionnaireSubscaleRangeDetailsModel.SubScaleRangeID)),
                                     CaptionText = GetDataItem<string>(dataItem, nameof(QuestionnaireSubscaleRangeDetailsModel.CaptionText)),
                                     InstructionsText = GetDataItem<string>(dataItem, nameof(QuestionnaireSubscaleRangeDetailsModel.InstructionsText)),
                                     LanguageID = GetDataItem<byte>(dataItem, nameof(QuestionnaireSubscaleRangeDetailsModel.LanguageID)),
                                     IsActive = GetDataItem<bool>(dataItem, nameof(QuestionnaireSubscaleRangeDetailsModel.IsActive)),
                                     IsDataDownloaded = false
                                 }).ToList() : new List<QuestionnaireSubscaleRangeDetailsModel>();
        }

        private List<QuestionnaireQuestionOptionDetailsModel> MapQuestionOptionDetails(JToken data, string nameOfToken)
        {
            return data[nameOfToken].Any() ?
                                (from dataItem in data[nameOfToken]
                                 select new QuestionnaireQuestionOptionDetailsModel
                                 {
                                     QuestionOptionID = GetDataItem<long>(dataItem, nameof(QuestionnaireQuestionOptionDetailsModel.QuestionOptionID)),
                                     CaptionText = GetDataItem<string>(dataItem, nameof(QuestionnaireQuestionOptionDetailsModel.CaptionText)),
                                     LanguageID = GetDataItem<byte>(dataItem, nameof(QuestionnaireQuestionOptionDetailsModel.LanguageID)),
                                     IsActive = GetDataItem<bool>(dataItem, nameof(QuestionnaireQuestionOptionDetailsModel.IsActive)),
                                 }).ToList() : new List<QuestionnaireQuestionOptionDetailsModel>();
        }

        private List<QuestionScoreModel> MapQuestionnaireQuestionScores(JToken data, string nameOfToken)
        {
            return data[nameOfToken].Any() ?
                                (from dataItem in data[nameOfToken]
                                 select new QuestionScoreModel
                                 {
                                     QuestionID = GetDataItem<long>(dataItem, nameof(QuestionScoreModel.QuestionID)),
                                     Value1 = GetDataItem<double>(dataItem, nameof(QuestionScoreModel.Value1)),
                                     Value2 = GetDataItem<double>(dataItem, nameof(QuestionScoreModel.Value2)),
                                     ScoreValue = GetDataItem<double>(dataItem, nameof(QuestionScoreModel.ScoreValue)),
                                     IsActive = GetDataItem<bool>(dataItem, nameof(QuestionScoreModel.IsActive)),
                                 }).ToList() : new List<QuestionScoreModel>();
        }
    }
}
