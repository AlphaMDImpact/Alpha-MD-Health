using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class QuestionnaireService : BaseService
{

    /// <summary>
    /// Sync questionnaire(s) from service
    /// </summary>
    /// <param name="questionnaires">questionnaires reference object to return data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>questionnaires received from server with operation status</returns>
    public async Task SyncQuestionnairesFromServerAsync(QuestionnaireDTO questionnaires, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_QUESTIONNAIRES_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_QUESTIONNAIRE_ID_KEY, Convert.ToString(questionnaires.Questionnaire.QuestionnaireID, CultureInfo.InvariantCulture) },
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(questionnaires.RecordCount, CultureInfo.InvariantCulture) }
                }
            };
            await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
            questionnaires.ErrCode = httpData.ErrCode;
            if (questionnaires.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(questionnaires, data);
                    MapQuestionnairesData(data, questionnaires);
                }
            }
        }
        catch (Exception ex)
        {
            questionnaires.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Questions from service
    /// </summary>
    /// <param name="questionnaires">reference object to return data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Questions received from service</returns>
    public async Task SyncQuestionnaireQuestionsFromServerAsync(QuestionnaireDTO questionnaires, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_QUESTIONNAIRE_QUESTIONS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(questionnaires.RecordCount, CultureInfo.InvariantCulture) },
                    { Constants.SE_QUESTIONNAIRE_ID_KEY, Convert.ToString(questionnaires.Question.QuestionnaireID, CultureInfo.InvariantCulture) },
                    { Constants.SE_QUESTION_ID_KEY, Convert.ToString(questionnaires.Question.QuestionID, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
            questionnaires.ErrCode = httpData.ErrCode;
            if (questionnaires.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(questionnaires, data);
                    MapQuestionnaireQuestionData(questionnaires, data);
                }
            }
        }
        catch (Exception ex)
        {
            questionnaires.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Questions To service
    /// </summary>
    /// <param name="requestData">object to send data and return status</param>
    /// <returns>Operation status</returns>
    public async Task SyncQuestionaireQuestionToServerAsync(QuestionnaireDTO requestData)
    {
        try
        {
            var httpData = new HttpServiceModel<QuestionnaireDTO>
            {
                CancellationToken = new CancellationToken(),
                PathWithoutBasePath = UrlConstants.SAVE_QUESTIONNAIRE_QUESTIONS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = requestData,
            };
            await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
            requestData.ErrCode = httpData.ErrCode;
        }
        catch (Exception ex)
        {
            requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Questionnaire data to server
    /// </summary>
    /// <param name="questionnaire">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status</returns>
    public async Task SyncQuestionnaireToServerAsync(QuestionnaireDTO questionnaire, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<QuestionnaireDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_QUESTIONNAIRE_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = questionnaire,
            };
            await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
            questionnaire.ErrCode = httpData.ErrCode;
            if (questionnaire.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data?.HasValues == true)
                {
                    questionnaire.Questionnaire.QuestionnaireID = (long)data[nameof(QuestionnaireDTO.Questionnaire)][nameof(QuestionnaireModel.QuestionnaireID)];
                }
            }
        }
        catch (Exception ex)
        {
            questionnaire.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Questionnaire status to server
    /// </summary>
    /// <param name="questionnaire">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status</returns>
    public async Task SyncPublishQuestionnaireToServerAsync(QuestionnaireDTO questionnaire, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<QuestionnaireDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.PUBLISH_QUESTIONNAIRE_ASYNC,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { nameof(QuestionnaireModel.IsPublished), questionnaire.Questionnaire.IsPublished.ToString(CultureInfo.InvariantCulture) },
                    { nameof(QuestionnaireModel.QuestionnaireID), questionnaire.Questionnaire.QuestionnaireID.ToString(CultureInfo.InvariantCulture) }
                }
            };
            await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
            questionnaire.ErrCode = httpData.ErrCode;
        }
        catch (Exception ex)
        {
            questionnaire.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync questionnaires subscale data from service
    /// </summary>
    /// <param name="questionnaires">questionnaire reference object to return data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>questionnaires subscale data retrieved from server in questionnaires</returns>
    public async Task SyncQuestionnaireSubScaleFromServerAsync(QuestionnaireDTO questionnaires, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_QUESTIONNAIRE_SUBSCALE_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_QUESTIONNAIRE_ID_KEY, Convert.ToString(questionnaires.Questionnaire?.QuestionnaireID, CultureInfo.InvariantCulture) },
                    { nameof(BaseDTO.RecordCount), Convert.ToString(questionnaires.RecordCount, CultureInfo.InvariantCulture) },
                    { nameof(QuestionnaireSubscaleModel.SubscaleID), Convert.ToString(questionnaires.QuestionnaireSubscaleData?.SubscaleID, CultureInfo.InvariantCulture) },
                    { nameof(QuestionnaireSubscaleRangesModel.SubScaleRangeID), Convert.ToString(questionnaires.QuestionnaireSubscaleRange?.SubScaleRangeID, CultureInfo.InvariantCulture) }
                }
            };
            await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
            questionnaires.ErrCode = httpData.ErrCode;
            if (questionnaires.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(questionnaires, data);
                    MapQuestionnaireSubscaleData(data, questionnaires);
                }
            }
        }
        catch (Exception ex)
        {
            questionnaires.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync questionnaires subscale data to service
    /// </summary>
    /// <param name="questionaire">Questionnaire reference object with subscale data</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status code</returns>
    public async Task SyncQuestionnaireSubscaleToServerAsync(QuestionnaireDTO questionaire, CancellationToken cancellationToken)
    {
        try
        {
            if (questionaire?.QuestionnaireSubscaleData == null || questionaire.QuestionnaireSubscaleData.QuestionnaireID < 1)
            {
                questionaire = questionaire ?? new QuestionnaireDTO();
                questionaire.ErrCode = ErrorCode.InvalidData;
                return;
            }
            var httpData = new HttpServiceModel<QuestionnaireDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_QUESTIONNAIRE_SUBSCALE_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { nameof(QuestionnaireModel.QuestionnaireID), questionaire.QuestionnaireSubscaleData.QuestionnaireID.ToString(CultureInfo.InvariantCulture) },
                    { nameof(QuestionnaireSubscaleModel.ScoreTypeID), questionaire.QuestionnaireSubscaleData.ScoreTypeID.ToString() }
                }
            };
            await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
            questionaire.ErrCode = httpData.ErrCode;
            if (questionaire.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data?.HasValues == true)
                {
                    questionaire.QuestionnaireSubscaleData.SubscaleID = (long)data[nameof(QuestionnaireDTO.RecordCount)];
                }
            }
        }
        catch (Exception ex)
        {
            questionaire.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync questionnaires subscale ranges to server
    /// </summary>
    /// <param name="questionnaire">Questionnaire reference object with subscale ranges</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status code</returns>
    public async Task SyncQuestionnaireSubscaleRangesToServerAsync(QuestionnaireDTO questionnaire, CancellationToken cancellationToken)
    {
        try
        {
            if (questionnaire?.QuestionnaireSubscaleRange == null)
            {
                questionnaire = questionnaire ?? new QuestionnaireDTO();
                questionnaire.ErrCode = ErrorCode.InvalidData;
                return;
            }
            var httpData = new HttpServiceModel<QuestionnaireDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_QUESTIONNAIRE_SUBSCALE_RANGES_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = questionnaire,
            };
            await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
            questionnaire.ErrCode = httpData.ErrCode;
        }
        catch (Exception ex)
        {
            questionnaire.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Question conditions from service
    /// </summary>
    /// <param name="questionnaires">reference object to return data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Questions received from service</returns>
    public async Task SyncQuestionConditionsFromServerAsync(QuestionnaireDTO questionnaires, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_QUESTION_CONDITIONS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(questionnaires.RecordCount, CultureInfo.InvariantCulture) },
                    { Constants.SE_QUESTIONNAIRE_ID_KEY, Convert.ToString(questionnaires.Question.QuestionnaireID, CultureInfo.InvariantCulture) },
                    { Constants.SE_QUESTION_ID_KEY, Convert.ToString(questionnaires.Question.QuestionID, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
            questionnaires.ErrCode = httpData.ErrCode;
            if (questionnaires.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(questionnaires, data);
                    MapQuestionConditionsData(questionnaires, data);
                }
            }
        }
        catch (Exception ex)
        {
            questionnaires.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Question score(s) from service
    /// </summary>
    /// <param name="questionnaires">reference object to return data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Question score(s) received from service</returns>
    public async Task SyncQuestionScoreFromServerAsync(QuestionnaireDTO questionnaires, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_QUESTION_SCORE_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(questionnaires.RecordCount, CultureInfo.InvariantCulture) },
                    { Constants.SE_QUESTIONNAIRE_ID_KEY, Convert.ToString(questionnaires.Question.QuestionnaireID, CultureInfo.InvariantCulture) },
                    { Constants.SE_QUESTION_ID_KEY, Convert.ToString(questionnaires.Question.QuestionID, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
            questionnaires.ErrCode = httpData.ErrCode;
            if (questionnaires.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(questionnaires, data);
                    MapQuestionScoresData(questionnaires, data);
                }
            }
        }
        catch (Exception ex)
        {
            questionnaires.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void MapQuestionnairesData(JToken data, QuestionnaireDTO questionnaires)
    {
        if (questionnaires.RecordCount == -1)
        {
            SetPageResources(questionnaires.Resources);
            if (data[nameof(QuestionnaireDTO.Questionnaire)]?.Count() > 0)
            {
                questionnaires.Questionnaire = MapQuestionnaire(data[nameof(QuestionnaireDTO.Questionnaire)]);
            }
            questionnaires.DropDownOptions = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_QUESTIONNAIRE_TYPE_GROUP, string.Empty, false, questionnaires.Questionnaire?.QuestionnaireTypeID ?? -1);
            questionnaires.DefaultRespondants = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_USER_TYPE_GROUP, string.Empty, false, questionnaires.Questionnaire?.DefaultRespondentID ?? -1);
            questionnaires.PageDetails = MapPageDetails(data);
        }
        else
        {
            SetResourcesAndSettings(questionnaires);
            LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
            questionnaires.Questionnaires = MapQuestionnaires(data, nameof(QuestionnaireDTO.Questionnaires));
            foreach (var questionaireData in questionnaires.Questionnaires)
            {
                questionaireData.PublisheUnpublishText = questionaireData.IsPublished ? LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_PUBLISHED_KEY) : LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_UNPUBLISHED_KEY);
                questionaireData.StatusStyle = GetStatus(questionaireData.PublisheUnpublishText);
                questionaireData.CreatedOn = GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(questionaireData.AddedOn), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
            }
        }
    }

    private FieldTypes GetStatus(string IsPublished)
    {
        FieldTypes statusStyle;
        if (IsPublished == LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_PUBLISHED_KEY))
        {
            statusStyle = FieldTypes.SuccessBadgeControl;
        }
        else
        {
            statusStyle = FieldTypes.DangerBadgeControl;
        }
        return statusStyle;
    }

    private List<QuestionnaireModel> MapQuestionnaires(JToken data, string nameOfToken)
    {
        return data[nameOfToken].Any()
            ? (from dataItem in data[nameOfToken]
               select MapQuestionnaire(dataItem)).ToList()
            : new List<QuestionnaireModel>();
    }

    private void MapQuestionnaireQuestionData(QuestionnaireDTO questionnaires, JToken data)
    {
        if (questionnaires.RecordCount == -1)
        {
            SetPageResources(questionnaires.Resources);
            var questionJData = data[nameof(QuestionnaireDTO.Question)];
            if (questionJData.HasValues)
            {
                questionnaires.Question = MapQuestionnaireQuestion(questionJData);
            }
            questionnaires.QuestionDetails = MapQuestionDetails(data, nameof(QuestionnaireDTO.QuestionDetails));
            questionnaires.QuestionOptions = MapQuestionOptions(data, nameof(QuestionnaireDTO.QuestionOptions));
            questionnaires.QuestionnaireQuestionOptionDetails = MapQuetionnaireQuestionOptions(data);

            List<ResourceModel> dataSource = questionnaires.Resources.Where(x => x.GroupName == GroupConstants.RS_QUESTIONNAIRE_QUESTION_TYPE_GROUP).OrderBy(x => x.ResourceValue).ToList();
            long selectedID = dataSource.FirstOrDefault(x => x.ResourceKey == questionnaires.Question.QuestionTypeID.ToString())?.ResourceKeyID ?? 0;
            questionnaires.DropDownOptions = GetPickerSource(dataSource, nameof(ResourceModel.ResourceKeyID), Constants.CNT_RESOURCE_VALUE_TEXT, selectedID, false, null);
            questionnaires.DefaultRespondants = GetPickerSource(data, nameof(QuestionnaireDTO.DefaultRespondants), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), (long)questionnaires.Question?.CategoryID,false,null);
            questionnaires.ReadingsOptions= GetPickerSource(data, nameof(QuestionnaireDTO.ReadingsOptions), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), (long)questionnaires.Question?.CategoryID, false, null);
        }
        else
        {
            questionnaires.Questions = MapQuestionnaireQuestions(data, nameof(QuestionnaireDTO.Questions));
        }
    }

    private void MapQuestionnaireSubscaleData(JToken data, QuestionnaireDTO questionnaires)
    {
        if (questionnaires.RecordCount == -2)
        {
            SetPageResources(questionnaires.Resources);
            questionnaires.DropDownOptions = GetPickerSource(data, nameof(QuestionnaireDTO.DropDownOptions), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), 0, false, nameof(OptionModel.ParentOptionID));
        }
        else if (questionnaires.RecordCount == -1)
        {
            questionnaires.QuestionnaireSubscaleRange = new QuestionnaireSubscaleRangesModel
            {
                SubScaleID = (long)data[nameof(QuestionnaireDTO.QuestionnaireSubscaleRange)][nameof(QuestionnaireSubscaleRangesModel.SubScaleID)],
                SubScaleRangeID = (long)data[nameof(QuestionnaireDTO.QuestionnaireSubscaleRange)][nameof(QuestionnaireSubscaleRangesModel.SubScaleRangeID)],
                MinValue = (float)data[nameof(QuestionnaireDTO.QuestionnaireSubscaleRange)][nameof(QuestionnaireSubscaleRangesModel.MinValue)],
                MaxValue = (float)data[nameof(QuestionnaireDTO.QuestionnaireSubscaleRange)][nameof(QuestionnaireSubscaleRangesModel.MaxValue)],
            };
            questionnaires.PageDetails = MapPageDetails(data);
        }
        else
        {
            questionnaires.QuestionnaireSubscaleRanges = MapQuesionnaireSubscaleRanges(data, nameof(QuestionnaireDTO.QuestionnaireSubscaleRanges));
        }
    }

    private List<ContentDetailModel> MapPageDetails(JToken data)
    {
        return data[nameof(QuestionnaireDTO.PageDetails)].Any() ?
                                                        (from dataItem in data[nameof(QuestionnaireDTO.PageDetails)]
                                                         select new ContentDetailModel
                                                         {
                                                             PageID = (long)dataItem[nameof(ContentDetailModel.PageID)],
                                                             LanguageID = (byte)dataItem[nameof(ContentDetailModel.LanguageID)],
                                                             LanguageName = (string)dataItem[nameof(ContentDetailModel.LanguageName)],
                                                             PageHeading = (string)dataItem[nameof(ContentDetailModel.PageHeading)],
                                                             PageData = (string)dataItem[nameof(ContentDetailModel.PageData)],
                                                             IsActive = true,
                                                         }).ToList() : new List<ContentDetailModel>();
    }

    private List<QuestionnaireSubscaleRangesModel> MapQuesionnaireSubscaleRanges(JToken data, string nameOfToken)
    {
        return data[nameOfToken].Any() ?
               (from dataItem in data[nameOfToken]
                select new QuestionnaireSubscaleRangesModel
                {
                    SubScaleRangeID = GetDataItem<long>(dataItem, nameof(QuestionnaireSubscaleRangesModel.SubScaleRangeID)),
                    SubScaleID = GetDataItem<long>(dataItem, nameof(QuestionnaireSubscaleRangesModel.SubScaleID)),
                    MinValue = GetDataItem<float>(dataItem, nameof(QuestionnaireSubscaleRangesModel.MinValue)),
                    MaxValue = GetDataItem<float>(dataItem, nameof(QuestionnaireSubscaleRangesModel.MaxValue)),
                    PageHeading = GetDataItem<string>(dataItem, nameof(QuestionnaireSubscaleRangesModel.PageHeading)),
                    IsActive = GetDataItem<bool>(dataItem, nameof(QuestionnaireSubscaleRangesModel.IsActive)),
                }).ToList() : new List<QuestionnaireSubscaleRangesModel>();
    }

    private QuestionnaireModel MapQuestionnaire(JToken dataItem)
    {
        return new QuestionnaireModel
        {
            QuestionnaireID = GetDataItem<long>(dataItem, nameof(QuestionnaireModel.QuestionnaireID)),
            CaptionText = GetDataItem<string>(dataItem, nameof(QuestionnaireModel.CaptionText)),
            InstructionsText = GetDataItem<string>(dataItem, nameof(QuestionnaireModel.InstructionsText)),
            QuestionnaireCode = GetDataItem<string>(dataItem, nameof(QuestionnaireModel.QuestionnaireCode)),
            AddedOn = GetDataItem<DateTimeOffset>(dataItem, nameof(QuestionnaireModel.AddedOn)),
            NoOfQuestions = GetDataItem<int>(dataItem, nameof(QuestionnaireModel.NoOfQuestions)),
            NoOfSubscales = GetDataItem<byte>(dataItem, nameof(QuestionnaireModel.NoOfSubscales)),
            QuestionnaireTypeID = GetDataItem<short>(dataItem, nameof(QuestionnaireModel.QuestionnaireTypeID)),
            DefaultRespondentID = GetDataItem<short>(dataItem, nameof(QuestionnaireModel.DefaultRespondentID)),
            IsPublished = GetDataItem<bool>(dataItem, nameof(QuestionnaireModel.IsPublished)),
            SubscaleID = GetDataItem<long>(dataItem, nameof(QuestionnaireModel.SubscaleID)),
            QuestionnaireAction = GetDataItem<QuestionnaireAction>(dataItem, nameof(QuestionnaireModel.QuestionnaireAction)),
        };
    }

    private List<QuestionnaireQuestionDetailsModel> MapQuestionDetails(JToken data, string nameOfToken)
    {
        return data[nameOfToken].Any() ?
                            (from dataItem in data[nameOfToken]
                             select new QuestionnaireQuestionDetailsModel
                             {
                                 QuestionID = GetDataItem<long>(dataItem, nameof(QuestionnaireQuestionDetailsModel.QuestionID)),
                                 CaptionText = GetDataItem<string>(dataItem, nameof(QuestionnaireQuestionDetailsModel.CaptionText)),
                                 AnswerPlaceHolder = GetDataItem<string>(dataItem, nameof(QuestionnaireQuestionDetailsModel.AnswerPlaceHolder)),
                                 InstructionsText = GetDataItem<string>(dataItem, nameof(QuestionnaireQuestionDetailsModel.InstructionsText)),
                                 LanguageID = GetDataItem<byte>(dataItem, nameof(QuestionnaireQuestionDetailsModel.LanguageID)),
                                 LanguageName = GetDataItem<string>(dataItem, nameof(QuestionnaireQuestionDetailsModel.LanguageName)),
                                 IsActive = GetDataItem<bool>(dataItem, nameof(QuestionnaireQuestionDetailsModel.IsActive)),
                             }).ToList() : new List<QuestionnaireQuestionDetailsModel>();
    }

    private List<QuestionnaireQuestionOptionModel> MapQuetionnaireQuestionOptions(JToken data)
    {
        return data[nameof(QuestionnaireDTO.QuestionnaireQuestionOptionDetails)].Any() ?
                            (from dataItem in data[nameof(QuestionnaireDTO.QuestionnaireQuestionOptionDetails)]
                             select new QuestionnaireQuestionOptionModel
                             {
                                 QuestionID = GetDataItem<long>(dataItem, nameof(QuestionnaireQuestionOptionModel.QuestionID)),
                                 QuestionOptionID = GetDataItem<long>(dataItem, nameof(QuestionnaireQuestionOptionModel.QuestionOptionID)),
                                 LanguageID = GetDataItem<byte>(dataItem, nameof(QuestionnaireQuestionOptionModel.LanguageID)),
                                 CaptionText = GetDataItem<string>(dataItem, nameof(QuestionnaireQuestionOptionModel.CaptionText)),
                                 LanguageName = GetDataItem<string>(dataItem, nameof(QuestionnaireQuestionOptionModel.LanguageName)),
                                 IsActive = true
                             }).ToList() : new List<QuestionnaireQuestionOptionModel>();
    }

    private QuestionnaireDetailsModel MapQuestionnaireDetails(JToken dataItem)
    {
        return new QuestionnaireDetailsModel
        {
            QuestionnaireID = GetDataItem<long>(dataItem, nameof(QuestionnaireDetailsModel.QuestionnaireID)),
            CaptionText = GetDataItem<string>(dataItem, nameof(QuestionnaireDetailsModel.CaptionText)),
            InstructionsText = GetDataItem<string>(dataItem, nameof(QuestionnaireDetailsModel.InstructionsText)),
        };
    }


    private void MapQuestionConditionsData(QuestionnaireDTO questionnaires, JToken data)
    {
        if (questionnaires.RecordCount == -1)
        {
            var questionJData = data[nameof(QuestionnaireDTO.Question)];
            if (questionJData.HasValues)
            {
                questionnaires.Question = MapQuestionnaireQuestion(questionJData);
            }
            questionnaires.QuestionConditions = MapQuestionsConditions(data, nameof(QuestionnaireDTO.QuestionConditions));
            
            // select question options 
            questionnaires.DropDownOptions = (from dataItem in data[nameof(QuestionnaireDTO.DropDownOptions)]
                                              select new OptionModel
                                              {
                                                  OptionID = GetDataItem<int>(dataItem, nameof(OptionModel.OptionID)),
                                                  OptionText = Regex.Replace(GetDataItem<string>(dataItem, nameof(OptionModel.OptionText)), Constants.HTML_TAGS, String.Empty),
                                                  IsSelected = GetDataItem<int>(dataItem, nameof(OptionModel.OptionID)) == questionnaires.Question.QuestionID
                                              }).ToList();

            questionnaires.Resources.Find(x => x.ResourceKey == ResourceConstants.R_CONDITION_VALUE_1_KEY).MinLength = questionnaires.Question.MinValue;
            questionnaires.Resources.Find(x => x.ResourceKey == ResourceConstants.R_CONDITION_VALUE_1_KEY).MaxLength = questionnaires.Question.MaxValue;
            questionnaires.Resources.Find(x => x.ResourceKey == ResourceConstants.R_CONDITION_VALUE_2_KEY).MinLength = questionnaires.Question.MinValue;
            questionnaires.Resources.Find(x => x.ResourceKey == ResourceConstants.R_CONDITION_VALUE_2_KEY).MaxLength = questionnaires.Question.MaxValue;
        }
        else
        {
            questionnaires.Questions = data[nameof(QuestionnaireDTO.Questions)].Any() ?
            (from question in data[nameof(QuestionnaireDTO.Questions)]
             select MapQuestionnaireQuestion(question)).ToList() : new List<QuestionnaireQuestionModel>();
        }
    }

    private void MapQuestionScoresData(QuestionnaireDTO questionnaires, JToken data)
    {
        if (questionnaires.RecordCount == -1)
        {
            var questionJData = data[nameof(QuestionnaireDTO.Question)];
            if (questionJData.HasValues)
            {
                questionnaires.Question = MapQuestionnaireQuestion(questionJData);
            }
            questionnaires.QuestionScores = data[nameof(QuestionnaireDTO.QuestionScores)].Any() ?
            (from question in data[nameof(QuestionnaireDTO.QuestionScores)]
             select MapQuestionsScores(question)).ToList() : new List<QuestionScoreModel>();

            // select question options 
            questionnaires.DropDownOptions = (from dataItem in data[nameof(QuestionnaireDTO.DropDownOptions)]
                                              select new OptionModel
                                              {
                                                  OptionID = GetDataItem<int>(dataItem, nameof(OptionModel.OptionID)),
                                                  OptionText = Regex.Replace(GetDataItem<string>(dataItem, nameof(OptionModel.OptionText)), Constants.HTML_TAGS, String.Empty),
                                                  IsSelected = GetDataItem<int>(dataItem, nameof(OptionModel.OptionID)) == questionnaires.Question.QuestionID
                                              }).ToList();

            questionnaires.Resources.Find(x => x.ResourceKey == ResourceConstants.R_CONDITION_VALUE_1_KEY).MinLength = questionnaires.Question.MinValue;
            questionnaires.Resources.Find(x => x.ResourceKey == ResourceConstants.R_CONDITION_VALUE_1_KEY).MaxLength = questionnaires.Question.MaxValue;
            questionnaires.Resources.Find(x => x.ResourceKey == ResourceConstants.R_CONDITION_VALUE_2_KEY).MinLength = questionnaires.Question.MinValue;
            questionnaires.Resources.Find(x => x.ResourceKey == ResourceConstants.R_CONDITION_VALUE_2_KEY).MaxLength = questionnaires.Question.MaxValue;
        }
        else
        {
            questionnaires.QuestionnaireQuestionScores = data[nameof(QuestionnaireDTO.QuestionnaireQuestionScores)].Any() ?
            (from question in data[nameof(QuestionnaireDTO.QuestionnaireQuestionScores)]
             select MapQuestionsScores(question)).ToList() : new List<QuestionScoreModel>();
        }
    }
    private QuestionScoreModel MapQuestionsScores(JToken dataItem)
    {
        return new QuestionScoreModel
        {
            OptionID = GetDataItem<long>(dataItem, nameof(QuestionScoreModel.OptionID)),
            QuestionID = GetDataItem<long>(dataItem, nameof(QuestionScoreModel.QuestionID)),
            Value1 = GetDataItem<double>(dataItem, nameof(QuestionScoreModel.Value1)),
            Value2 = GetDataItem<double>(dataItem, nameof(QuestionScoreModel.Value2)),
            OptionText = GetDataItem<string>(dataItem, nameof(QuestionScoreModel.OptionText)),
            InstructionText = GetDataItem<string>(dataItem, nameof(QuestionScoreModel.InstructionText)),
            ScoreValue = GetDataItem<double>(dataItem, nameof(QuestionScoreModel.ScoreValue)),
        };
    }

    /// <summary>
    /// Sync Questionnaire Linked Questions To Server
    /// </summary>
    /// <param name="requestData">object to send data and return status</param>
    /// <returns>Operation status</returns>
    public async Task SyncQuestionnaireConditionsToServerAsync(QuestionnaireDTO requestData)
    {
        try
        {
            var httpData = new HttpServiceModel<QuestionnaireDTO>
            {
                CancellationToken = new CancellationToken(),
                PathWithoutBasePath = UrlConstants.SAVE_QUESTIONNAIRE_CONDITIONS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = requestData,
            };
            await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
            requestData.ErrCode = httpData.ErrCode;
        }
        catch (Exception ex)
        {
            requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Questionnaire  Question Score To Server
    /// </summary>
    /// <param name="requestData">object to send data and return status</param>
    /// <returns>Operation status</returns>
    public async Task SyncQuestionnaireQuestionScoresToServerAsync(QuestionnaireDTO requestData)
    {
        try
        {
            var httpData = new HttpServiceModel<QuestionnaireDTO>
            {
                CancellationToken = new CancellationToken(),
                PathWithoutBasePath = UrlConstants.SAVE_QUESTION_SCORE_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = requestData,
            };
            await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
            requestData.ErrCode = httpData.ErrCode;
        }
        catch (Exception ex)
        {
            requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }
}