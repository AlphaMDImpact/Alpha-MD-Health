using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class QuestionnaireServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Questionnaire service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public QuestionnaireServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get Questionnaire(s)
        /// </summary>
        /// <param name="questionnaireID">questionnaire Id</param>
        /// <param name="organisationID">organisation Id</param>
        /// <param name="languageID">language Id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="recordCount">record count</param>
        /// <returns>Questionnaires an operation status</returns>
        public async Task<QuestionnaireDTO> GetQuestionnairesAsync(long questionnaireID, long organisationID, byte languageID, long permissionAtLevelID, long recordCount)
        {
            QuestionnaireDTO questionnaires = new QuestionnaireDTO();
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1 || organisationID < 1)
                {
                    questionnaires.ErrCode = ErrorCode.InvalidData;
                    return questionnaires;
                }
                questionnaires.AccountID = AccountID;
                if (questionnaires.AccountID < 1)
                {
                    questionnaires.ErrCode = ErrorCode.Unauthorized;
                    return questionnaires;
                }
                questionnaires.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                questionnaires.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources,
                    $"{GroupConstants.RS_QUESTIONNAIRE_TYPE_GROUP},{GroupConstants.RS_USER_TYPE_GROUP},{GroupConstants.RS_QUESTIONNAIRE_PAGE_GROUP},{GroupConstants.RS_COMMON_GROUP}",
                    languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(questionnaires.Resources))
                {
                    questionnaires.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP,
                        languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
                    questionnaires.Settings.AddRange((await GetDataFromCacheAsync(CachedDataType.OrganisationSettings,
                        $"{GroupConstants.RS_ORGANISATION_SETTINGS_GROUP}",
                        languageID, default, 0, organisationID, false).ConfigureAwait(false)).Settings);
                    if (GenericMethods.IsListNotEmpty(questionnaires.Settings))
                    {
                        questionnaires.LanguageID = languageID;
                        questionnaires.PermissionAtLevelID = permissionAtLevelID;
                        questionnaires.RecordCount = recordCount;
                        questionnaires.Questionnaire = new QuestionnaireModel
                        {
                            QuestionnaireID = questionnaireID
                        };
                        questionnaires.FeatureFor = FeatureFor;
                        await new QuestionnaireServiceDataLayer().GetQuestionnairesAsync(questionnaires).ConfigureAwait(false);
                        if (questionnaires.ErrCode == ErrorCode.OK)
                        {
                            await new ContentPageServiceBusinessLayer(_httpContext).ReplaceContentDetailImageCdnLinkAsync(questionnaires.PageDetails, new BaseDTO());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                questionnaires.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return questionnaires;
        }

        /// <summary>
        /// Saves Questionnaire 
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="questionnaire">reference object which holds questionnaire data</param>
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SaveQuestionnaireAsync(byte languageID, long permissionAtLevelID, long organisationID, QuestionnaireDTO questionnaire)
        {
            try
            {
                IsQuestionnaireValid(questionnaire, permissionAtLevelID, languageID, organisationID);
                if (questionnaire.ErrCode == ErrorCode.OK)
                {
                    if (AccountID < 1)
                    {
                        questionnaire.ErrCode = ErrorCode.Unauthorized;
                        return questionnaire;
                    }
                    questionnaire.AccountID = AccountID;
                    questionnaire.LanguageID = languageID;
                    questionnaire.PermissionAtLevelID = permissionAtLevelID;
                    questionnaire.OrganisationID = organisationID;
                    questionnaire.FeatureFor = FeatureFor;
                    if (questionnaire.Questionnaire.QuestionnaireID == 0)
                    {
                        await new QuestionnaireServiceDataLayer().SaveQuestionnaireAsync(questionnaire, false).ConfigureAwait(false);
                    }
                    if (questionnaire.ErrCode == ErrorCode.OK)
                    {
                        if (questionnaire.PageDetails.FirstOrDefault(x => x.PageData != string.Empty) != null)
                        {
                            await UploadImagesAsync(questionnaire, GetQuestionnaireFolderID(questionnaire)).ConfigureAwait(false);
                        }
                        if (questionnaire.ErrCode == ErrorCode.OK)
                        {
                            await new QuestionnaireServiceDataLayer().SaveQuestionnaireAsync(questionnaire, true).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                questionnaire.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return questionnaire;
        }

        /// <summary>
        /// Saves questionnaire status
        /// </summary>
        /// <param name="languageID">Language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="questionnaireID">The questionaire to Publish or unpulish</param>
        /// <param name="isPublished">Publish= True, Unpublish= False</param>
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> PublishQuestionnaireAsync(byte languageID, long permissionAtLevelID, long questionnaireID, bool isPublished)
        {
            BaseDTO resultDTO = new BaseDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || questionnaireID < 1)
                {
                    resultDTO.ErrCode = ErrorCode.InvalidData;
                    return resultDTO;
                }
                if (AccountID < 1)
                {
                    resultDTO.ErrCode = ErrorCode.Unauthorized;
                    return resultDTO;
                }
                resultDTO.AccountID = AccountID;
                resultDTO.LanguageID = languageID;
                resultDTO.PermissionAtLevelID = permissionAtLevelID;
                resultDTO.RecordCount = questionnaireID;
                resultDTO.IsActive = isPublished;
                resultDTO.FeatureFor = FeatureFor;
                await new QuestionnaireServiceDataLayer().PublishQuestionnaireAsync(resultDTO).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                resultDTO.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return resultDTO;
        }

        /// <summary>
        /// Get Sub scale data for the selected questionnaire
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="questionnaireID">Questionnaire ID</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <param name="subscaleID">subscale id to fetch</param>
        /// <param name="subscaleRangeID">subscale range id to fetch</param>
        /// <returns>Questionnaire subscale Data with operation status</returns>
        public async Task<QuestionnaireDTO> GetQuestionnaireSubscaleAsync(byte languageID, long permissionAtLevelID, long questionnaireID, long recordCount, long subscaleID, long subscaleRangeID)
        {
            QuestionnaireDTO questionnaire = new QuestionnaireDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || questionnaireID < 1)
                {
                    questionnaire.ErrCode = ErrorCode.InvalidData;
                    return questionnaire;
                }
                if (AccountID < 1)
                {
                    questionnaire.ErrCode = ErrorCode.Unauthorized;
                    return questionnaire;
                }
                questionnaire.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                questionnaire.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_QUESTIONNAIRE_PAGE_GROUP}",
                    languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(questionnaire.Resources))
                {
                    questionnaire.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
                    if (GenericMethods.IsListNotEmpty(questionnaire.Settings))
                    {
                        questionnaire.AccountID = AccountID;
                        questionnaire.LanguageID = languageID;
                        questionnaire.PermissionAtLevelID = permissionAtLevelID;
                        questionnaire.RecordCount = recordCount;
                        questionnaire.Questionnaire = new QuestionnaireModel { QuestionnaireID = questionnaireID };
                        questionnaire.QuestionnaireSubscaleData = new QuestionnaireSubscaleModel { SubscaleID = subscaleID };
                        questionnaire.QuestionnaireSubscaleRange = new QuestionnaireSubscaleRangesModel { SubScaleRangeID = subscaleRangeID };
                        questionnaire.FeatureFor = FeatureFor;
                        await new QuestionnaireServiceDataLayer().GetQuestionnaireSubscaleAsync(questionnaire).ConfigureAwait(false);
                        if (questionnaire.ErrCode == ErrorCode.OK)
                        {
                            await new ContentPageServiceBusinessLayer(_httpContext).ReplaceContentDetailImageCdnLinkAsync(questionnaire.PageDetails, new BaseDTO());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                questionnaire.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return questionnaire;
        }

        /// <summary>
        /// Save questionnaire subscale data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="questionnaireID">Selected Questionnaire ID</param>
        /// <param name="questionnaireSubscale">Selected subscale value</param>
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SaveQuestionnaireSubscaleAsync(byte languageID, long permissionAtLevelID, long questionnaireID, string questionnaireSubscale)
        {
            BaseDTO resultData = new BaseDTO();
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1 || questionnaireID < 1 || string.IsNullOrWhiteSpace(questionnaireSubscale))
                {
                    resultData.ErrCode = ErrorCode.InvalidData;
                    return resultData;
                }
                if (AccountID < 1)
                {
                    resultData.ErrCode = ErrorCode.Unauthorized;
                    return resultData;
                }
                resultData.AccountID = AccountID;
                resultData.LanguageID = languageID;
                resultData.PermissionAtLevelID = permissionAtLevelID;
                resultData.RecordCount = questionnaireID;
                resultData.ErrorDescription = questionnaireSubscale;
                resultData.FeatureFor = FeatureFor;
                await new QuestionnaireServiceDataLayer().SaveQuestionnaireSubscaleAsync(resultData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                resultData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return resultData;
        }

        /// <summary>
        /// Save questionnaire subscale ranges
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organizationID">organization id</param>
        /// <param name="questionnaire">object which holds questionnaire subscale ranges to store in db</param>
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SaveQuestionnaireSubscaleRangesAsync(byte languageID, long permissionAtLevelID, long organizationID, QuestionnaireDTO questionnaire)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1 || questionnaire == null)
                {
                    questionnaire = new QuestionnaireDTO
                    {
                        ErrCode = ErrorCode.InvalidData
                    };
                    return questionnaire;
                }
                IsRangeValid(questionnaire, organizationID);
                if (questionnaire.ErrCode == ErrorCode.OK)
                {
                    questionnaire.LanguageID = languageID;
                    if (await GetSettingsResourcesAsync(questionnaire, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_QUESTIONNAIRE_PAGE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(questionnaire.QuestionnaireSubscaleRange, questionnaire.Resources))
                        {
                            questionnaire.ErrCode = ErrorCode.InvalidData;
                            return questionnaire;
                        }
                    }
                    else
                    {
                        return questionnaire;
                    }
                    questionnaire.AccountID = AccountID;
                    questionnaire.PermissionAtLevelID = permissionAtLevelID;
                    questionnaire.OrganisationID = organizationID;
                    questionnaire.FeatureFor = FeatureFor;
                    if (questionnaire.QuestionnaireSubscaleRange.SubScaleRangeID == 0)
                    {
                        await new QuestionnaireServiceDataLayer().SaveQuestionnaireSubscaleRangesAsync(questionnaire, false).ConfigureAwait(false);
                    }
                    if (questionnaire.ErrCode == ErrorCode.OK)
                    {
                        if (questionnaire.PageDetails.FirstOrDefault(x => x.PageData != string.Empty) != null && questionnaire.IsActive)
                        {
                            await UploadImagesAsync(questionnaire, $"{GetQuestionnaireFolderID(questionnaire)}_subscalerange_{questionnaire.QuestionnaireSubscaleRange.SubScaleRangeID}").ConfigureAwait(false);
                        }
                        if (questionnaire.ErrCode == ErrorCode.OK)
                        {
                            await new QuestionnaireServiceDataLayer().SaveQuestionnaireSubscaleRangesAsync(questionnaire, true).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                questionnaire.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return questionnaire;
        }

        /// <summary>
        /// Gets Questionnaire question data
        /// </summary>
        /// <param name="languageID">Users language ID</param>
        /// <param name="permissionAtLevelID">Users permission at ID</param>
        /// <param name="recordCount">Record count for retriving list or single data</param>
        /// <param name="questionnaireID">Questionnaire ID</param>
        /// <param name="questionID">Question ID</param>
        /// <returns>Questionnaire questions with operation status</returns>
        public async Task<QuestionnaireDTO> GetQuestionnaireQuestionsAsync(byte languageID, long permissionAtLevelID, long recordCount, long questionnaireID, long questionID)
        {
            QuestionnaireDTO quesstionaire = new QuestionnaireDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || questionnaireID < 1)
                {
                    quesstionaire.ErrCode = ErrorCode.InvalidData;
                    return quesstionaire;
                }
                if (AccountID < 1)
                {
                    quesstionaire.ErrCode = ErrorCode.Unauthorized;
                    return quesstionaire;
                }
                quesstionaire.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                quesstionaire.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_QUESTIONNAIRE_PAGE_GROUP},{GroupConstants.RS_QUESTIONNAIRE_QUESTION_TYPE_GROUP},{GroupConstants.RS_PATIENT_DOCUMNET_GROUP}",
                    languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(quesstionaire.Resources))
                {
                    quesstionaire.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, $"{GroupConstants.RS_COMMON_GROUP}",
                        languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
                    if (GenericMethods.IsListNotEmpty(quesstionaire.Settings))
                    {
                        quesstionaire.AccountID = AccountID;
                        quesstionaire.LanguageID = languageID;
                        quesstionaire.PermissionAtLevelID = permissionAtLevelID;
                        quesstionaire.RecordCount = recordCount;
                        quesstionaire.Question = new QuestionnaireQuestionModel { QuestionnaireID = questionnaireID, QuestionID = questionID };
                        quesstionaire.FeatureFor = FeatureFor;
                        await new QuestionnaireServiceDataLayer().GetQuestionnaireQuestionsAsync(quesstionaire).ConfigureAwait(false);
                        if (quesstionaire.ErrCode == ErrorCode.OK)
                        {
                            await ReplaceQuestionImageCdnLinkAsync(quesstionaire.QuestionDetails);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                quesstionaire.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return quesstionaire;
        }

        /// <summary>
        /// Saves questionnaire question data
        /// </summary>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="questionData">reference object which holds question data</param>
        /// <returns>Operation Status</returns>
        public async Task<BaseDTO> SaveQuestionnaireQuestionAsync(long permissionAtLevelID, long organisationID, QuestionnaireDTO questionData)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1 || questionData?.Question == null || questionData.Question.QuestionnaireID < 1 || (questionData.Question.IsActive && (!GenericMethods.IsListNotEmpty(questionData.QuestionDetails) || IsOptionsNotProvided(questionData))))
                {
                    questionData ??= new QuestionnaireDTO();
                    questionData.ErrCode = ErrorCode.InvalidData;
                    return questionData;
                }
                if (questionData.IsActive)
                {
                    questionData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(questionData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_QUESTIONNAIRE_PAGE_GROUP},{GroupConstants.RS_QUESTIONNAIRE_QUESTION_TYPE_GROUP},{GroupConstants.RS_PATIENT_DOCUMNET_GROUP}").ConfigureAwait(false))
                    {
                        if (questionData.Question.QuestionTypeID == QuestionType.HorizontalSliderQuestionKey || questionData.Question.QuestionTypeID == QuestionType.VerticalSliderQuestionKey)
                        {
                            if (!(questionData.Question.SliderSteps >= (int)questionData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SLIDER_STEPS_KEY).MinLength) &&
                                (questionData.Question.SliderSteps <= (int)questionData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SLIDER_STEPS_KEY).MaxLength))
                            {
                                questionData.ErrCode = ErrorCode.InvalidData;
                                return questionData;
                            }
                        }
                        if (!(questionData.Question.QuestionTypeID == QuestionType.SingleSelectQuestionKey || questionData.Question.QuestionTypeID == QuestionType.MultiSelectQuestionKey || questionData.Question.QuestionTypeID == QuestionType.DropDownQuestionKey
                            || questionData.Question.QuestionTypeID == QuestionType.RichTextQuestionKey || questionData.Question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey || questionData.Question.QuestionTypeID == QuestionType.MeasurementQuestionKey
                            || questionData.Question.QuestionTypeID == 0))
                        {
                            if (!(questionData.Question.MinValue >= (int)questionData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_MIN_TEXT_KEY).MinLength) &&
                                (questionData.Question.MaxValue >= (int)questionData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_MAX_TEXT_KEY).MaxLength))
                            {
                                questionData.ErrCode = ErrorCode.InvalidData;
                                return questionData;
                            }
                        }
                        if (questionData.Question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey)
                        {
                            if (!(questionData.Question.CategoryID >= (int)questionData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SELECT_File_CATEGORY_KEY).MinLength) &&
                               (questionData.Question.CategoryID <= (int)questionData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SELECT_File_CATEGORY_KEY).MaxLength))
                            {
                                questionData.ErrCode = ErrorCode.InvalidData;
                                return questionData;
                            }
                        }
                        if (questionData.Question.QuestionTypeID == QuestionType.MeasurementQuestionKey)
                        {
                            if (!(questionData.Question.CategoryID >= (int)questionData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SELECT_MEASURMENT_KEY).MinLength) &&
                               (questionData.Question.CategoryID <= (int)questionData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SELECT_MEASURMENT_KEY).MaxLength))
                            {
                                questionData.ErrCode = ErrorCode.InvalidData;
                                return questionData;
                            }
                        }
                        if (!await ValidateDataAsync(questionData.Question, questionData.Resources))
                        {
                            questionData.ErrCode = ErrorCode.InvalidData;
                            return questionData;
                        }
                    }
                    else
                    {
                        return questionData;
                    }
                }
                questionData.AccountID = AccountID;
                questionData.PermissionAtLevelID = permissionAtLevelID;
                questionData.OrganisationID = organisationID;
                await SaveQuestionAsync(questionData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                questionData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return questionData;
        }

        private async Task SaveQuestionAsync(QuestionnaireDTO questionData)
        {
            questionData.FeatureFor = FeatureFor;
            await UploadQuestionImagesAsync(questionData, $"{GetQuestionnaireFolderID(questionData)}_question_{questionData.Question.QuestionID}").ConfigureAwait(false);
            if (questionData.ErrCode == ErrorCode.OK)
            {
                await new QuestionnaireServiceDataLayer().SaveQuestionnaireQuestionAsync(questionData).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get Question conditions
        /// </summary>
        /// <param name="questionnaireID">questionnaire Id</param>
        /// <param name="questionID">question Id</param>
        /// <param name="organisationID">organisation Id</param>
        /// <param name="languageID">language Id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="recordCount">record count</param>
        /// <returns>Question conditions with operation status</returns>
        public async Task<QuestionnaireDTO> GetQuestionConditionsAsync(long questionnaireID, long questionID, long organisationID, byte languageID, long permissionAtLevelID, long recordCount)
        {
            QuestionnaireDTO questionnaires = new QuestionnaireDTO();
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1 || organisationID < 1 || questionnaireID < 1)
                {
                    questionnaires.ErrCode = ErrorCode.InvalidData;
                    return questionnaires;
                }
                questionnaires.AccountID = AccountID;
                if (questionnaires.AccountID < 1)
                {
                    questionnaires.ErrCode = ErrorCode.Unauthorized;
                    return questionnaires;
                }
                questionnaires.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                questionnaires.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_QUESTIONNAIRE_PAGE_GROUP},{GroupConstants.RS_QUESTIONNAIRE_QUESTION_TYPE_GROUP}",
                    languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(questionnaires.Resources))
                {
                    questionnaires.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
                    if (GenericMethods.IsListNotEmpty(questionnaires.Settings))
                    {
                        questionnaires.LanguageID = languageID;
                        questionnaires.PermissionAtLevelID = permissionAtLevelID;
                        questionnaires.RecordCount = recordCount;
                        questionnaires.Question = new QuestionnaireQuestionModel { QuestionnaireID = questionnaireID, QuestionID = questionID };
                        questionnaires.FeatureFor = FeatureFor;
                        await new QuestionnaireServiceDataLayer().GetQuestionConditionsAsync(questionnaires).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                questionnaires.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return questionnaires;
        }

        /// <summary>
        /// Saves Questionnaire question condition data
        /// </summary>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="questionData">reference object which holds question data</param>
        /// <returns>Operation Status</returns>
        public async Task<BaseDTO> SaveQuestionConditionsAsync(long permissionAtLevelID, long organisationID, QuestionnaireDTO questionData)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1 || questionData?.Question == null || questionData.Question.QuestionnaireID < 1 || questionData.Question.QuestionID < 1 || (questionData.Question.IsActive && (!GenericMethods.IsListNotEmpty(questionData.QuestionConditions))))
                {
                    questionData ??= new QuestionnaireDTO();
                    questionData.ErrCode = ErrorCode.InvalidData;
                    return questionData;
                }
                questionData.LanguageID = 1;
                if (await GetSettingsResourcesAsync(questionData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_QUESTIONNAIRE_PAGE_GROUP},{GroupConstants.RS_QUESTIONNAIRE_QUESTION_TYPE_GROUP}").ConfigureAwait(false))
                {
                    if (!await ValidateDataAsync(questionData.Question, questionData.Resources))
                    {
                        questionData.ErrCode = ErrorCode.InvalidData;
                        return questionData;
                    }
                }
                else
                {
                    return questionData;
                }   
                questionData.AccountID = AccountID;
                questionData.PermissionAtLevelID = permissionAtLevelID;
                questionData.OrganisationID = organisationID;
                questionData.FeatureFor = FeatureFor;
                await new QuestionnaireServiceDataLayer().SaveQuestionConditionsAsync(questionData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                questionData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return questionData;
        }

        /// <summary>
        /// Saves Patient Questionnaire Results
        /// </summary>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="questionnaireData">reference object which holds patient questionnaire results</param>
        /// <returns>ErrorCode and New Server GUID in case of duplicate GUID</returns>
        public async Task<QuestionnaireDTO> SavePatientQuestionnaireResultsAsync(long permissionAtLevelID, QuestionnaireDTO questionnaireData)
        {
            try
            {
                if (permissionAtLevelID < 1
                    || (!GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireQuestionAnswers) && !GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireScores))
                    || IsAnswersInValid(questionnaireData) || IsScoresInValid(questionnaireData))
                {
                    questionnaireData.ErrCode = ErrorCode.InvalidData;
                    return questionnaireData;
                }
                if (AccountID < 1)
                {
                    questionnaireData.ErrCode = ErrorCode.Unauthorized;
                    return questionnaireData;
                }
                questionnaireData.AccountID = AccountID;
                questionnaireData.FeatureFor = FeatureFor;
                await new QuestionnaireServiceDataLayer().SavePatientQuestionnaireResultsAsync(questionnaireData).ConfigureAwait(false);
                questionnaireData.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                questionnaireData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return questionnaireData;
        }

        /// <summary>
        /// Get Question scores
        /// </summary>
        /// <param name="questionnaireID">questionnaire Id</param>
        /// <param name="questionID">question ID</param>
        /// <param name="organisationID">organisation Id</param>
        /// <param name="languageID">language Id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="recordCount">record count</param>
        /// <returns>Questionnaire scores with operation status</returns>
        public async Task<QuestionnaireDTO> GetQuestionScoreAsync(long questionnaireID, long questionID, long organisationID, byte languageID, long permissionAtLevelID, long recordCount)
        {
            QuestionnaireDTO questionnaires = new QuestionnaireDTO();
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1 || organisationID < 1 || questionnaireID < 1)
                {
                    questionnaires.ErrCode = ErrorCode.InvalidData;
                    return questionnaires;
                }
                questionnaires.AccountID = AccountID;
                if (questionnaires.AccountID < 1)
                {
                    questionnaires.ErrCode = ErrorCode.Unauthorized;
                    return questionnaires;
                }
                questionnaires.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                questionnaires.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_QUESTIONNAIRE_PAGE_GROUP},{GroupConstants.RS_QUESTIONNAIRE_QUESTION_TYPE_GROUP}",
                    languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(questionnaires.Resources))
                {
                    questionnaires.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
                    if (GenericMethods.IsListNotEmpty(questionnaires.Settings))
                    {
                        questionnaires.LanguageID = languageID;
                        questionnaires.PermissionAtLevelID = permissionAtLevelID;
                        questionnaires.RecordCount = recordCount;
                        questionnaires.Question = new QuestionnaireQuestionModel { QuestionnaireID = questionnaireID, QuestionID = questionID };
                        questionnaires.FeatureFor = FeatureFor;
                        await new QuestionnaireServiceDataLayer().GetQuestionScoreAsync(questionnaires).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                questionnaires.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return questionnaires;
        }

        /// <summary>
        /// Saves question score data
        /// </summary>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="questionData">reference object which holds question data</param>
        /// <returns>Operation Status</returns>
        public async Task<BaseDTO> SaveQuestionScoreAsync(long permissionAtLevelID, long organisationID, QuestionnaireDTO questionData)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1 || questionData?.Question == null || questionData.Question.QuestionnaireID < 1 || questionData.Question.QuestionID < 1 || (questionData.Question.IsActive && (!GenericMethods.IsListNotEmpty(questionData.QuestionScores))))
                {
                    questionData ??= new QuestionnaireDTO();
                    questionData.ErrCode = ErrorCode.InvalidData;
                    return questionData;
                }
                questionData.LanguageID = 1;
                if (await GetSettingsResourcesAsync(questionData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_QUESTIONNAIRE_PAGE_GROUP},{GroupConstants.RS_QUESTIONNAIRE_QUESTION_TYPE_GROUP}").ConfigureAwait(false))
                {
                    if (!await ValidateDataAsync(questionData.Question, questionData.Resources))
                    {
                        questionData.ErrCode = ErrorCode.InvalidData;
                        return questionData;
                    }
                }
                else
                {
                    return questionData;
                }
                questionData.AccountID = AccountID;
                questionData.PermissionAtLevelID = permissionAtLevelID;
                questionData.OrganisationID = organisationID;
                questionData.FeatureFor = FeatureFor;
                await new QuestionnaireServiceDataLayer().SaveQuestionScoresAsync(questionData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                questionData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return questionData;
        }

        private bool IsScoresInValid(QuestionnaireDTO questionnaireData)
        {
            return questionnaireData.QuestionnaireScores?.Any(x => string.IsNullOrEmpty(x.PatientTaskID) || x.SubScaleRangeID < 1) ?? false;
        }

        private bool IsAnswersInValid(QuestionnaireDTO questionnaireData)
        {
            return questionnaireData.QuestionnaireQuestionAnswers?.Any(x => x.TaskType < 1 || x.QuestionID < 1 || string.IsNullOrWhiteSpace(x.PatientTaskID)) ?? false;
        }

        private void IsQuestionnaireValid(QuestionnaireDTO questionnaire, long permissionAtLevelID, byte languageID, long organisationID)
        {
            if (permissionAtLevelID < 1 || languageID < 1 || organisationID < 1 || questionnaire?.Questionnaire == null
                || string.IsNullOrWhiteSpace(questionnaire.Questionnaire.QuestionnaireCode)
                || questionnaire.Questionnaire.QuestionnaireTypeID < 1 || questionnaire.Questionnaire.DefaultRespondentID < 1
                || !GenericMethods.IsListNotEmpty(questionnaire.PageDetails))
            {
                questionnaire.ErrCode = ErrorCode.InvalidData;
            }
        }

        private void IsRangeValid(QuestionnaireDTO questionnaire, long organisationID)
        {
            if (organisationID < 1 || questionnaire.QuestionnaireSubscaleRange == null 
                || (questionnaire.QuestionnaireSubscaleRange.SubScaleRangeID < 1 && questionnaire.QuestionnaireSubscaleRange.SubScaleID < 1))
            {
                questionnaire.ErrCode = ErrorCode.InvalidData;
            }
        }

        //private bool IsRecomendationValid(QuestionnaireDTO questionnaire)
        //{
        //    if (questionnaire.PageDetails?.FirstOrDefault(x => x.PageData != string.Empty || x.PageHeading != string.Empty) != null)
        //    {
        //        return questionnaire.PageDetails.FirstOrDefault(x => (x.PageData == string.Empty || x.PageHeading == string.Empty)) == null;
        //    }
        //    return true;
        //}

        private bool IsOptionsNotProvided(QuestionnaireDTO questionData)
        {
            if ((questionData.Question.QuestionTypeID == QuestionType.SingleSelectQuestionKey || questionData.Question.QuestionTypeID == QuestionType.MultiSelectQuestionKey || questionData.Question.QuestionTypeID == QuestionType.DropDownQuestionKey) && (!GenericMethods.IsListNotEmpty(questionData.QuestionOptions) || !GenericMethods.IsListNotEmpty(questionData.QuestionnaireQuestionOptionDetails)))
            {
                return true;
            }
            return false;
        }

        internal async Task ReplaceQuestionnaireDetailImageCdnLinkAsync(List<QuestionnaireDetailsModel> pageDetails)
        {
            BaseDTO cdnCacheData = new BaseDTO();
            if (GenericMethods.IsListNotEmpty(pageDetails))
            {
                foreach (var detail in pageDetails)
                {
                    if (!string.IsNullOrWhiteSpace(detail.InstructionsText))
                    {
                        detail.InstructionsText = await ReplaceCDNLinkAsync(detail.InstructionsText, cdnCacheData);
                    }
                }
            }
        }

        internal async Task ReplaceQuestionnaireRecommendationsImageCdnLinkAsync(List<QuestionnaireSubscaleRangeDetailsModel> pageDetails)
        {
            BaseDTO cdnCacheData = new BaseDTO();
            if (GenericMethods.IsListNotEmpty(pageDetails))
            {
                foreach (var detail in pageDetails)
                {
                    if (!string.IsNullOrWhiteSpace(detail.InstructionsText))
                    {
                        detail.InstructionsText = await ReplaceCDNLinkAsync(detail.InstructionsText, cdnCacheData);
                    }
                }
            }
        }

        private async Task UploadImagesAsync(QuestionnaireDTO questionnaireData, string ID)
        {
            FileUploadDTO files = CreateFileDataObject(FileTypeToUpload.QuestionnairesImages, ID);
            files.FileContainers[0].FileData.AddRange(from detail in questionnaireData.PageDetails
                                                      select new FileDataModel
                                                      {
                                                          HasMultiple = true,
                                                          Base64File = detail.PageData,
                                                          RecordID = $"{detail.LanguageID}_{nameof(detail.PageData)}",
                                                      });
            files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
            questionnaireData.ErrCode = files.ErrCode;
            if (questionnaireData.ErrCode == ErrorCode.OK)
            {
                foreach (var detail in questionnaireData.PageDetails)
                {
                    detail.PageData = GetBase64FileFromFirstContainer(files, $"{detail.LanguageID}_{nameof(detail.PageData)}");
                }
            }
        }

        internal async Task ReplaceQuestionImageCdnLinkAsync(List<QuestionnaireQuestionDetailsModel> questionDetails)
        {
            BaseDTO cdnCacheData = new BaseDTO();
            if (GenericMethods.IsListNotEmpty(questionDetails))
            {
                foreach (var detail in questionDetails)
                {
                    if (!string.IsNullOrWhiteSpace(detail.InstructionsText))
                    {
                        detail.InstructionsText = await ReplaceCDNLinkAsync(detail.InstructionsText, cdnCacheData);
                    }
                    if (!string.IsNullOrWhiteSpace(detail.CaptionText))
                    {
                        detail.CaptionText = await ReplaceCDNLinkAsync(detail.CaptionText, cdnCacheData);
                    }
                }
            }
        }

        private async Task UploadQuestionImagesAsync(QuestionnaireDTO questionData, string ID)
        {
            FileUploadDTO files = CreateFileDataObject(FileTypeToUpload.QuestionnairesImages, ID);
            files.FileContainers[0].FileData.AddRange(from detail in questionData.QuestionDetails
                                                      select new FileDataModel
                                                      {
                                                          HasMultiple = true,
                                                          Base64File = detail.InstructionsText,
                                                          RecordID = $"{detail.LanguageID}_{nameof(detail.InstructionsText)}",
                                                      });
            files.FileContainers[0].FileData.AddRange(from detail in questionData.QuestionDetails
                                                      select new FileDataModel
                                                      {
                                                          HasMultiple = true,
                                                          Base64File = detail.CaptionText,
                                                          RecordID = $"{detail.LanguageID}_{nameof(detail.CaptionText)}",
                                                      });
            files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
            questionData.ErrCode = files.ErrCode;
            if (questionData.ErrCode == ErrorCode.OK)
            {
                foreach (var detail in questionData.QuestionDetails)
                {
                    detail.InstructionsText = GetBase64FileFromFirstContainer(files, $"{detail.LanguageID}_{nameof(detail.InstructionsText)}");
                    detail.CaptionText = GetBase64FileFromFirstContainer(files, $"{detail.LanguageID}_{nameof(detail.CaptionText)}");
                }
            }
        }


        private string GetQuestionnaireFolderID(QuestionnaireDTO questionnaire)
        {
            return $"questionnaire_{questionnaire.Questionnaire.QuestionnaireID}";
        }
    }
}