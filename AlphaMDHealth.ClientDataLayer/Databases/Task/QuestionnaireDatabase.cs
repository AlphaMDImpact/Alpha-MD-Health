using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;
using System.Globalization;

namespace AlphaMDHealth.ClientDataLayer
{
    public class QuestionnaireDatabase : BaseDatabase
    {
        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="questionnaireData">Questionnaire data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveQuetionnairesAsync(QuestionnaireDTO questionnaireData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveQuestionnaires(questionnaireData, transaction);
                SaveQuestionnaireDetails(questionnaireData, transaction);
                SaveQuestionnaireQuestions(questionnaireData, transaction);
                SaveQuestionnaireQuestionOptions(questionnaireData, transaction);
                SaveQuestionnaireQuestionDetails(questionnaireData, transaction);
                SaveQuestionOptionDetails(questionnaireData, transaction);
                SaveQuestionnaireSubscales(questionnaireData, transaction);
                SaveQuestionnaireSubscaleRanges(questionnaireData, transaction);
                SaveQuestionnaireSubscaleRangeDetails(questionnaireData, transaction);
                SaveQuestionnaireConditions(questionnaireData, transaction);
                SaveQuestionnaireQuestionAnswers(questionnaireData, transaction);
                SaveQuestionnaireScores(questionnaireData, transaction);
                SaveQuestionScores(questionnaireData, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get Document List and Files List to sync
        /// </summary>
        /// <param name="questionnaireData">Reference object to return questionnaire results records</param>
        /// <returns>operation status</returns>
        public async Task GetPatientQuestionnaireResultsForSyncAsync(QuestionnaireDTO questionnaireData)
        {
            questionnaireData.QuestionnaireQuestionAnswers = await SqlConnection.QueryAsync<PatientQuestionnaireQuestionAnswersModel>
                ($"SELECT * FROM PatientQuestionnaireQuestionAnswersModel WHERE IsSynced = 0 AND TaskType = 1");
            questionnaireData.QuestionnaireScores = await SqlConnection.QueryAsync<PatientQuestionnaireScoresModel>
                ($"SELECT * FROM PatientQuestionnaireScoresModel WHERE IsSynced = 0");
        }

        /// <summary>
        /// Updates Patient Questionnaire results status
        /// </summary>
        /// <param name="questionnaireData">Response received from server</param>
        /// <returns>Operation status</returns>
        public async Task UpdatePatientQuestionnaireResultsStatusAsync(QuestionnaireDTO questionnaireData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireQuestionAnswers))
                {
                    foreach (var item in questionnaireData.QuestionnaireQuestionAnswers)
                    {
                        SaveResultModel result = questionnaireData.QuestionnaireQuestionAnswerResult?.FirstOrDefault(x => x.ClientGuid == item.PatientAnswerID);
                        item.ErrCode = result == null ? ErrorCode.OK : result.ErrCode;
                        switch (item.ErrCode)
                        {
                            case ErrorCode.OK:
                                transaction.Execute(
                                    $"UPDATE PatientQuestionnaireQuestionAnswersModel SET AnswerValue = ?, IsSynced = 1, ErrCode = ? WHERE PatientAnswerID = ?"
                                    , item.AnswerValue, item.ErrCode, item.PatientAnswerID);
                                break;
                            case ErrorCode.DuplicateGuid:
                                transaction.Execute(
                                    $"UPDATE PatientQuestionnaireQuestionAnswersModel SET PatientAnswerID = ? WHERE PatientAnswerID = ?"
                                    , GenerateNewGuid(transaction, false), item.PatientAnswerID);
                                questionnaireData.ErrCode = item.ErrCode;
                                break;
                            default:
                                transaction.Execute(
                                    "UPDATE PatientQuestionnaireQuestionAnswersModel SET ErrCode = ? WHERE PatientAnswerID = ?"
                                    , item.ErrCode, item.PatientAnswerID);
                                break;
                        }
                    }
                }
                UpdateQuestionnaireScoreSyncStatus(questionnaireData, transaction);
            });
        }

        private void UpdateQuestionnaireScoreSyncStatus(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireScores))
            {
                foreach (var item in questionnaireData.QuestionnaireScores)
                {
                    SaveResultModel result = questionnaireData.QuestionnaireScoreResult?.FirstOrDefault(x => x.ClientGuid == item.PatientScoreID);
                    item.ErrCode = result == null ? ErrorCode.OK : result.ErrCode;
                    switch (item.ErrCode)
                    {
                        case ErrorCode.OK:
                            transaction.Execute(
                                $"UPDATE PatientQuestionnaireScoresModel SET SubScaleRangeID = ?, UserScore = ?, IsSynced = 1, ErrCode = ? WHERE PatientScoreID = ?"
                                , item.SubScaleRangeID, item.UserScore, item.ErrCode, item.PatientScoreID);
                            break;
                        case ErrorCode.DuplicateGuid:
                            transaction.Execute(
                                $"UPDATE PatientQuestionnaireScoresModel SET PatientScoreID = ? WHERE PatientScoreID = ?"
                                , GenerateNewGuid(transaction, true), item.PatientScoreID);
                            questionnaireData.ErrCode = item.ErrCode;
                            break;
                        default:
                            transaction.Execute(
                                "UPDATE PatientQuestionnaireScoresModel SET ErrCode = ? WHERE PatientScoreID = ?"
                                , item.ErrCode, item.PatientScoreID);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Retrives Next/Previous question of the selected questionnaire
        /// </summary>
        /// <param name="questionnaireData">Questionnaire data reference object</param>
        /// <param name="isNextAction">To check action for next page</param>
        /// <param name="isPreviousAction">To check action for previous page</param>
        /// <param name="shouldSaveAnswer">To check answer should be saved or not</param>
        /// <param name="answerQuestionCondition">Condition to get answer data</param>
        /// <returns>Operation status and records in data object</returns>
        private async Task GetQuestionAsync(QuestionnaireDTO questionnaireData, bool isStartingQuestion, bool isNextAction, bool isPreviousAction, bool shouldSaveAnswer, string answerQuestionCondition, FileDTO fileData = null)
        {
            var numberOfQuestions = await GetPatientQuestionnaireQuestionAnswersAsync(questionnaireData.PatientTaskID).ConfigureAwait(false);
            questionnaireData.NumberOfQuestionAnswer = numberOfQuestions?.Count;

            // if questionObject is null then find out which question (sequence) was answerd previously else get sequence of question from same object
            bool alreadyQuestionValue = false;
            if (isStartingQuestion)
            {
                questionnaireData.Question = await SqlConnection.FindWithQueryAsync<QuestionnaireQuestionModel>(
                    "SELECT * FROM QuestionnaireQuestionModel WHERE QuestionnaireID = ? AND IsStartingQuestion = 1 AND IsActive = 1"
                    , questionnaireData.Questionnaire.QuestionnaireID
                ).ConfigureAwait(false);
            }

            if (questionnaireData.Question == null)
            {
                //Latest answered quetion of current task
                questionnaireData.QuestionnaireQuestionAnswer = await SqlConnection.FindWithQueryAsync<PatientQuestionnaireQuestionAnswersModel>(
                    "SELECT * FROM PatientQuestionnaireQuestionAnswersModel WHERE PatientTaskID = ? AND TaskType = 1 ORDER BY LastModifiedON DESC LIMIT 1"
                    , questionnaireData.PatientTaskID
                ).ConfigureAwait(false);
                if (questionnaireData.QuestionnaireQuestionAnswer != null && questionnaireData.QuestionnaireQuestionAnswer.NextQuestionID > 0)
                {
                    questionnaireData.Question = await GetQuestionBasedOnQuestionIDAsync(questionnaireData.Questionnaire.QuestionnaireID, questionnaireData.QuestionnaireQuestionAnswer.NextQuestionID).ConfigureAwait(false);
                    isNextAction = questionnaireData.Question.IsStartingQuestion;
                    if (questionnaireData.Question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey)
                    {
                        /* questionnaireData.QuestionnaireQuestionAnswer = await CommonDataBase.SqlConnection.FindWithQueryAsync<PatientQuestionnaireQuestionAnswersModel>(
                            "SELECT * FROM PatientQuestionnaireQuestionAnswersModel " +
                            $"WHERE QuestionID = ? AND PreviousQuestionID = ? AND PatientTaskID = ? AND TaskType = 1 ",
                           questionnaireData.Question.QuestionID, questionnaireData.QuestionnaireQuestionAnswer.QuestionID, questionnaireData.PatientTaskID).ConfigureAwait(false);
                        if (questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID != Guid.Empty)
                        {
                            questionnaireData.FileDocuments = await CommonDataBase.SqlConnection.QueryAsync<FileDocumentModel>
                                                ($"SELECT * FROM FileDocumentModel WHERE DocumentSourceID = ? AND IsActive = 1",
                                                questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID).ConfigureAwait(false);
                        }*/
                    }
                    alreadyQuestionValue = true;
                }
            }

            if (!alreadyQuestionValue)
            {
                //Get First Question
                if (isNextAction)
                {
                    questionnaireData.Question = await SqlConnection.FindWithQueryAsync<QuestionnaireQuestionModel>(
                        $"SELECT * FROM QuestionnaireQuestionModel WHERE QuestionnaireID = ? AND IsStartingQuestion = 1 AND IsActive = 1"
                        , questionnaireData.Questionnaire.QuestionnaireID
                    ).ConfigureAwait(false);
                }
                else if (isPreviousAction)
                {
                    if (questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID > 0)
                    {
                        questionnaireData.Question = await GetQuestionBasedOnQuestionIDAsync(questionnaireData.Questionnaire.QuestionnaireID, questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID).ConfigureAwait(false);
                        isNextAction = questionnaireData.Question.IsStartingQuestion;
                    }
                    else
                    {
                        questionnaireData.Questionnaire.QuestionnaireAction = await GetQuestionnaireActionAsync(questionnaireData.PatientTaskID, isNextAction, false).ConfigureAwait(false);
                        return;
                    }
                }
                else
                {
                    await GetQuestionFromConditionsAsync(questionnaireData);
                }
            }

            //Saving answer after getting NextQuestionID
            if (shouldSaveAnswer)
            {
                questionnaireData.QuestionnaireQuestionAnswer.NextQuestionID = questionnaireData.Question == null
                    ? 0
                    : questionnaireData.Question.QuestionID;
                await SavePatientQuestionAnswersAsync(questionnaireData, true, fileData).ConfigureAwait(false);
            }

            if (questionnaireData.Question != null)
            {
                questionnaireData.QuestionDetail = await SqlConnection.FindWithQueryAsync<QuestionnaireQuestionDetailsModel>(
                    "SELECT * FROM QuestionnaireQuestionDetailsModel WHERE QuestionID = ? AND IsActive = 1 AND LanguageID =  ?"
                    , questionnaireData.Question.QuestionID, questionnaireData.LanguageID
                ).ConfigureAwait(false);

                long previousQuestionID = 0;
                if (!isPreviousAction)
                {
                    previousQuestionID = questionnaireData.QuestionnaireQuestionAnswer?.QuestionID ?? 0;
                };
                questionnaireData.QuestionnaireQuestionAnswer = null;

                if (!alreadyQuestionValue)
                {
                    questionnaireData.QuestionnaireQuestionAnswer = (await SqlConnection.QueryAsync<PatientQuestionnaireQuestionAnswersModel>(
                        $"SELECT * FROM PatientQuestionnaireQuestionAnswersModel WHERE QuestionID = ? AND PatientTaskID = ? AND TaskType = 1 {answerQuestionCondition}"
                        , questionnaireData.Question.QuestionID, questionnaireData.PatientTaskID
                    ).ConfigureAwait(false)).FirstOrDefault();
                }

                if (questionnaireData.Question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey)
                {
                    if (questionnaireData.QuestionnaireQuestionAnswer != default)
                    {
                        questionnaireData.FileDocuments = await SqlConnection.QueryAsync<FileDocumentModel>(
                            $"SELECT * FROM FileDocumentModel WHERE DocumentSourceID = ? AND IsActive = 1"
                            , questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID
                        ).ConfigureAwait(false);
                    }
                }

                if (previousQuestionID > 0 && questionnaireData.QuestionnaireQuestionAnswer == null)
                {
                    questionnaireData.QuestionnaireQuestionAnswer = new PatientQuestionnaireQuestionAnswersModel
                    {
                        QuestionID = questionnaireData.Question.QuestionID,
                        PreviousQuestionID = previousQuestionID
                    };
                }

                questionnaireData.QuestionOptions = await SqlConnection.QueryAsync<QuestionnaireQuestionOptionModel>(
                    "SELECT A.*, B.CaptionText FROM QuestionnaireQuestionOptionModel A " +
                   "LEFT JOIN QuestionnaireQuestionOptionDetailsModel B ON A.QuestionOptionID = B.QuestionOptionID " +
                   "WHERE A.QuestionID = ? AND A.IsActive = 1 AND B.LanguageID = ?"
                   , questionnaireData.Question.QuestionID, questionnaireData.LanguageID
                ).ConfigureAwait(false);

                questionnaireData.Questionnaire.QuestionnaireAction = await GetQuestionnaireActionAsync(questionnaireData.PatientTaskID, isNextAction, false).ConfigureAwait(false);
            }
            else
            {
                await CalculateAndSaveQuestionnaireScoreAsync(questionnaireData).ConfigureAwait(false);
                questionnaireData.Questionnaire.QuestionnaireAction = QuestionnaireAction.Done;
            }
        }


        private async Task<QuestionnaireQuestionModel> GetQuestionBasedOnQuestionIDAsync(long questionnaireID, long? QuestionID)
        {
            return await SqlConnection.FindWithQueryAsync<QuestionnaireQuestionModel>(
                "SELECT * FROM QuestionnaireQuestionModel WHERE QuestionnaireID = ? AND QuestionID = ? AND IsActive = 1"
                , questionnaireID, QuestionID
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Calculate questionnaire score and save in DB
        /// </summary>
        /// <param name="questionnaireData">Reference object to hold questionnare data</param>
        /// <returns>Operation status</returns>
        private async Task CalculateAndSaveQuestionnaireScoreAsync(QuestionnaireDTO questionnaireData)
        {
            List<PatientQuestionnaireQuestionAnswersModel> questionAnswers = await GetActivePatientQuestionnaireQuestionAnswersAsync(questionnaireData.PatientTaskID).ConfigureAwait(false);
            QuestionnaireSubscaleModel subscaleInfo = await GetQuestionnaireSubscaleAsync(questionnaireData.Questionnaire.QuestionnaireID).ConfigureAwait(false);
            if (subscaleInfo != null)
            {
                decimal? questionnaireScore = GetQuestionAnswerScoreAsync(questionAnswers, subscaleInfo);

                // Get Subscale range for calculated score
                var subscaleRange = await GetSubscaleRangeAsync(subscaleInfo, questionnaireScore).ConfigureAwait(false);
                // if there is no subscale range then no PatientQuestionnaireScore will be saved as it will break on server because of Foreign Key.
                // Also as discussed with Balvvant this case will not be handeled here as questionnaire was configured wrong
                if (subscaleRange != null)
                {
                    // Get subscale recommendation and Insert data in PatientQuestionnaireScores Table
                    //var subscaleRangeRecommendation = await GetQuestionnaireSubscaleRangeDetailsAsync(questionnaireData, subscaleRange).ConfigureAwait(false);
                    await SavePatientQuestionnaireScoreAsync(questionnaireData, questionnaireScore, subscaleRange).ConfigureAwait(false);

                    await GetQuestionnaireScoreAndRecommendationAsync(questionnaireData).ConfigureAwait(false);
                }
            }
            // Update Task Status as Completed
            await new PatientTaskDatabase().UpdateTaskStatusAsync(Convert.ToInt64(questionnaireData.PatientTaskID, CultureInfo.InvariantCulture), ResourceConstants.R_COMPLETED_STATUS_KEY).ConfigureAwait(true);
        }

        //private async Task<QuestionnaireSubscaleRangeDetailsModel> GetQuestionnaireSubscaleRangeDetailsAsync(QuestionnaireDTO questionnaireData, QuestionnaireSubscaleRangesModel subscaleRange)
        //{
        //    return await CommonDataBase.SqlConnection.FindWithQueryAsync<QuestionnaireSubscaleRangeDetailsModel>(
        //        "SELECT * FROM QuestionnaireSubscaleRangeDetailsModel " +
        //        "WHERE SubScaleRangeID = ? AND IsActive = 1 AND LanguageID = ?"
        //        , subscaleRange.SubScaleRangeID, questionnaireData.LanguageID
        //    ).ConfigureAwait(false);
        //}

        private async Task<QuestionnaireSubscaleRangesModel> GetSubscaleRangeAsync(QuestionnaireSubscaleModel subscaleInfo, decimal? questionnaireScore)
        {
            return await SqlConnection.FindWithQueryAsync<QuestionnaireSubscaleRangesModel>(
                $"SELECT * FROM QuestionnaireSubscaleRangesModel " +
                $"WHERE SubScaleID = ? AND IsActive = 1 AND {questionnaireScore ?? 0} BETWEEN MinValue AND MaxValue", subscaleInfo.SubscaleID
            ).ConfigureAwait(false);
        }

        private async Task<List<PatientQuestionnaireQuestionAnswersModel>> GetActivePatientQuestionnaireQuestionAnswersAsync(string patientTaskID)
        {
            //get all answers of this questionnaire
            return await SqlConnection.QueryAsync<PatientQuestionnaireQuestionAnswersModel>(
                "SELECT * FROM PatientQuestionnaireQuestionAnswersModel WHERE PatientTaskID = ? AND TaskType = 1"
                , patientTaskID
            ).ConfigureAwait(false);
        }

        private async Task<List<PatientQuestionnaireQuestionAnswersModel>> GetPatientQuestionnaireQuestionAnswersAsync(string patientTaskID)
        {
            //get all answers of this questionnaire
            return await SqlConnection.QueryAsync<PatientQuestionnaireQuestionAnswersModel>(
                "SELECT * FROM PatientQuestionnaireQuestionAnswersModel WHERE PatientTaskID = ? AND IsActive = 1 AND TaskType = 1"
                , patientTaskID
            ).ConfigureAwait(false);
        }

        private async Task<QuestionnaireSubscaleModel> GetQuestionnaireSubscaleAsync(long questionnaireID)
        {
            // Get subscale for given questionnair
            return await SqlConnection.FindWithQueryAsync<QuestionnaireSubscaleModel>(
                $"SELECT * FROM QuestionnaireSubscaleModel WHERE QuestionnaireID = ? AND IsActive = 1"
                , questionnaireID
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Get questionnare recommendation to display in UI
        /// </summary>
        /// <param name="questionnaireData">Reference object to hold questionnare data</param>
        /// <returns>Questionnaire recommendation with operation status</returns>
        private async Task GetQuestionnaireScoreAndRecommendationAsync(QuestionnaireDTO questionnaireData)
        {
            questionnaireData.QuestionnaireSubscaleRangeDetails = await SqlConnection.QueryAsync<QuestionnaireSubscaleRangeDetailsModel>(
                "SELECT A.* FROM QuestionnaireSubscaleRangeDetailsModel A " +
                "JOIN PatientQuestionnaireScoresModel B ON A.SubScaleRangeID = B.SubScaleRangeID AND B.PatientTaskID = ? AND A.IsActive = 1 AND A.LanguageID = ?"
                , questionnaireData.PatientTaskID, questionnaireData.LanguageID
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Get questionnare data
        /// </summary>
        /// <param name="questionnaireData">Reference object to hold questionnare data</param>
        /// <returns>Questionnaire information with operation status</returns>
        public async Task GetQuestionnaireAsync(QuestionnaireDTO questionnaireData)
        {
            questionnaireData.Questionnaire = await SqlConnection.FindWithQueryAsync<QuestionnaireModel>(
                "SELECT * FROM QuestionnaireModel WHERE QuestionnaireID = ?"
                , questionnaireData.Questionnaire.QuestionnaireID
            ).ConfigureAwait(false);

            questionnaireData.QuestionnaireDetail = await SqlConnection.FindWithQueryAsync<QuestionnaireDetailsModel>(
                "SELECT A.* FROM QuestionnaireDetailsModel A " +
                "JOIN QuestionnaireModel B ON A.QuestionnaireID = B.QuestionnaireID AND B.QuestionnaireID = ? AND A.LanguageID = ? AND A.IsActive = 1"
                , questionnaireData.Questionnaire.QuestionnaireID, questionnaireData.LanguageID
            ).ConfigureAwait(false);

            questionnaireData.Questionnaire.NoOfQuestions = await GetQuestionnaireQuestionCountAsync(questionnaireData.PatientTaskID).ConfigureAwait(false);
            questionnaireData.Questionnaire.QuestionnaireAction = await GetQuestionnaireActionAsync(questionnaireData.PatientTaskID, false, false).ConfigureAwait(false);

            // Get question data / Recommendation data according to QuestionnaireAction
            switch (questionnaireData.Questionnaire.QuestionnaireAction)
            {
                case QuestionnaireAction.StartQuestionnaire:
                    // action not required
                    break;
                case QuestionnaireAction.PreviousAndNext:
                    await GetQuestionAsync(questionnaireData, ResourceConstants.R_SAVE_AND_NEXT_ACTION_KEY).ConfigureAwait(false);
                    break;
                case QuestionnaireAction.Done:
                    await GetQuestionnaireScoreAndRecommendationAsync(questionnaireData).ConfigureAwait(false);
                    break;
                default:
                    await GetQuestionAsync(questionnaireData, ResourceConstants.R_START_ACTION_KEY).ConfigureAwait(false);
                    break;
            }
        }

        /// <summary>
        /// Get previos/next patient question for the selected Questionnaire
        /// </summary>
        /// <param name="questionnaireData">Questionnaire data reference object</param>
        /// <param name="action">Action to get data for next page</param>
        /// <returns>Operation Status</returns>
        public async Task GetQuestionAsync(QuestionnaireDTO questionnaireData, string action, FileDTO fileData = null)
        {
            switch (action)
            {
                case ResourceConstants.R_START_ACTION_KEY:
                    questionnaireData.QuestionnaireQuestionAnswer = null;
                    // Update Status of Task to InProgress
                    if (questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.StartQuestionnaire)
                    {
                        await new PatientTaskDatabase().UpdateTaskStatusAsync(
                            Convert.ToInt64(questionnaireData.PatientTaskID, CultureInfo.InvariantCulture),
                            ResourceConstants.R_INPROGRESS_STATUS_KEY
                        ).ConfigureAwait(true);
                    }
                    // Get Next Question's Data
                    await GetQuestionAsync(questionnaireData, true, true, false, false, string.Empty).ConfigureAwait(false);
                    break;
                case ResourceConstants.R_PREVIOUS_ACTION_KEY:
                    // Mark current question as IsActive = false as patient can go to another flow
                    if (questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID != default)
                    {
                        questionnaireData.QuestionnaireQuestionAnswer.IsSynced = false;
                        await MarkQuestionInActiveAsync(questionnaireData);
                    }
                    // Get Previous Question's Data
                    string nextQuestionCondition = $"AND NextQuestionID = {questionnaireData.Question.QuestionID} "; //Condition to save answer
                    await GetQuestionAsync(questionnaireData, false, false, true, false, nextQuestionCondition).ConfigureAwait(false);
                    break;
                case ResourceConstants.R_NEXT_ACTION_KEY1:
                case ResourceConstants.R_SAVE_AND_NEXT_ACTION_KEY:
                    // Get Next Question
                    if (questionnaireData.Question == null)
                    {
                        await GetQuestionAsync(questionnaireData, false, false, false, false, string.Empty, fileData).ConfigureAwait(false);
                    }
                    else
                    {
                        if (questionnaireData.QuestionnaireQuestionAnswer == null
                            || (questionnaireData.Question.IsRequired && string.IsNullOrEmpty(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue)))
                        {
                            questionnaireData.ErrCode = ErrorCode.InvalidData;
                        }
                        else
                        {
                            //Condition to save answer
                            string previousQuestionCondition = (questionnaireData.Question?.QuestionID == 0)
                                ? "AND PreviousQuestionID IS NULL"
                                : $"AND PreviousQuestionID = {questionnaireData.Question.QuestionID} ";
                            // string previousQuestionCondition = $"AND PreviousQuestionID = {questionnaireData.Question.QuestionID} "; //Condition to save answer
                            // Get Next Question's Data
                            await CalculateQuestionScoreAsync(questionnaireData);
                            await GetQuestionAsync(questionnaireData, false, false, false, true, previousQuestionCondition, fileData).ConfigureAwait(false);
                        }
                    }
                    break;
                default:
                    if (questionnaireData.QuestionnaireQuestionAnswer == null
                        || (questionnaireData.Question.IsRequired && string.IsNullOrEmpty(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue)))
                    {
                        questionnaireData.ErrCode = ErrorCode.InvalidData;
                    }
                    else
                    {
                        //Calculate score and Save Answer and return recommendtion
                        await CalculateQuestionScoreAsync(questionnaireData).ConfigureAwait(false);
                        await SavePatientQuestionAnswersAsync(questionnaireData, true).ConfigureAwait(false);
                        if (questionnaireData.ErrCode == ErrorCode.OK)
                        {
                            questionnaireData.QuestionnaireQuestionAnswer = null;
                            await CalculateAndSaveQuestionnaireScoreAsync(questionnaireData).ConfigureAwait(false);
                            questionnaireData.Questionnaire.QuestionnaireAction = QuestionnaireAction.Done;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets Language specific data to download images from cdn
        /// </summary>
        /// <param name="questionnaireData">reference object to return data to be downloaded</param>
        /// <returns>data to be downloaded from web</returns>
        public async Task GetQuestionnaireDetailsToDownloadImageAsync(QuestionnaireDTO questionnaireData)
        {
            questionnaireData.QuestionnaireDetails = await SqlConnection.QueryAsync<QuestionnaireDetailsModel>(
                "SELECT * FROM QuestionnaireDetailsModel WHERE IsDataDownloaded = ?", false
            ).ConfigureAwait(false);
            questionnaireData.QuestionnaireSubscaleRangeDetails = await SqlConnection.QueryAsync<QuestionnaireSubscaleRangeDetailsModel>(
                "SELECT * FROM QuestionnaireSubscaleRangeDetailsModel WHERE IsDataDownloaded = ?", false
            ).ConfigureAwait(false);
        }

        private async Task GetQuestionFromConditionsAsync(QuestionnaireDTO questionnaireData)
        {
            List<QuestionConditionModel> conditionData = null;
            // questionnaireData.Question null check when you are on last question and you click cancel instead of ok
            if (questionnaireData.Question != null)
            {
                switch (questionnaireData.Question.QuestionTypeID)
                {
                    case QuestionType.RichTextQuestionKey:
                        conditionData = await SqlConnection.QueryAsync<QuestionConditionModel>(
                            "SELECT * FROM QuestionConditionModel WHERE QuestionID=?"
                            , questionnaireData.Question.QuestionID
                        ).ConfigureAwait(false);
                        break;
                    case QuestionType.MultiSelectQuestionKey:
                        conditionData = await GetMultiSelectConditionAsync(questionnaireData).ConfigureAwait(false);
                        break;
                    case QuestionType.SingleSelectQuestionKey:
                    case QuestionType.DropDownQuestionKey:
                        conditionData = await GetSingleSelectConditionAsync(questionnaireData).ConfigureAwait(false);
                        break;
                    case QuestionType.MultilineTextQuestionKey:
                    case QuestionType.TextQuestionKey:
                    case QuestionType.FilesAndDocumentQuestionKey:
                    case QuestionType.MeasurementQuestionKey:
                        conditionData = await GetTextQuestionConditionsAsync(questionnaireData).ConfigureAwait(false);
                        break;
                    case QuestionType.NumericQuestionKey:
                    case QuestionType.DateQuestionKey:
                    case QuestionType.DateTimeQuestionKey:
                    case QuestionType.TimeQuestionKey:
                    case QuestionType.HorizontalSliderQuestionKey:
                    case QuestionType.VerticalSliderQuestionKey:
                        conditionData = await GetBetweenConditionsAsync(questionnaireData).ConfigureAwait(false);
                        break;
                    default:
                        break;
                }
            }

            if (conditionData?.Count > 0 && conditionData.FirstOrDefault().TargetQuestionID > 0)
            {
                var questionID = conditionData.FirstOrDefault().TargetQuestionID;
                questionnaireData.Question = await GetQuestionBasedOnQuestionIDAsync(questionnaireData.Questionnaire.QuestionnaireID, questionID).ConfigureAwait(false);
            }
            else
            {
                questionnaireData.Question = null; //Last question
            }
        }

        private async Task<List<QuestionConditionModel>> GetSingleSelectConditionAsync(QuestionnaireDTO questionnaireData)
        {
            List<QuestionConditionModel> conditionData;
            if (string.IsNullOrWhiteSpace(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue))
            {
                conditionData = await GetEmptyValueCondition(questionnaireData).ConfigureAwait(false);
            }
            else
            {
                conditionData = await SqlConnection.QueryAsync<QuestionConditionModel>(
                    "SELECT * FROM QuestionConditionModel WHERE Value1=? AND QuestionID=?"
                    , questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, questionnaireData.Question.QuestionID
                ).ConfigureAwait(false);
            }
            return conditionData;
        }

        private async Task<List<QuestionConditionModel>> GetEmptyValueCondition(QuestionnaireDTO questionnaireData)
        {
            return await SqlConnection.QueryAsync<QuestionConditionModel>(
                "SELECT * FROM QuestionConditionModel WHERE Value1=? AND QuestionID=?"
                , "-10000", questionnaireData.Question.QuestionID
            ).ConfigureAwait(false);
        }

        private async Task<List<QuestionConditionModel>> GetMultiSelectConditionAsync(QuestionnaireDTO questionnaireData)
        {
            List<QuestionConditionModel> conditionData;
            if (string.IsNullOrWhiteSpace(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue))
            {
                conditionData = await GetEmptyValueCondition(questionnaireData).ConfigureAwait(false);
            }
            else
            {
                var answerValue = questionnaireData.QuestionnaireQuestionAnswer.AnswerValue.Split(Constants.SYMBOL_PIPE_SEPERATOR).FirstOrDefault();
                conditionData = await SqlConnection.QueryAsync<QuestionConditionModel>(
                    "SELECT * FROM QuestionConditionModel WHERE Value1=? AND QuestionID=?"
                    , answerValue, questionnaireData.Question.QuestionID
                ).ConfigureAwait(false);
            }
            return conditionData;
        }

        private async Task<List<QuestionConditionModel>> GetBetweenConditionsAsync(QuestionnaireDTO questionnaireData)
        {
            List<QuestionConditionModel> conditionData;
            if (string.IsNullOrWhiteSpace(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue))
            {
                conditionData = await GetEmptyValueCondition(questionnaireData).ConfigureAwait(false);
            }
            else
            {
                switch (questionnaireData.Question.QuestionTypeID)
                {
                    case QuestionType.DateQuestionKey:
                    case QuestionType.DateTimeQuestionKey:
                        var dateDisfferance = (DateTime.Parse(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, CultureInfo.InvariantCulture).Date - DateTime.UtcNow.Date).TotalDays;
                        conditionData = await SqlConnection.QueryAsync<QuestionConditionModel>(
                             "SELECT * FROM QuestionConditionModel " +
                             "WHERE Value1 <=? AND Value2 >=? AND QuestionID=?",
                             dateDisfferance, dateDisfferance, questionnaireData.Question.QuestionID).ConfigureAwait(false);
                        break;
                    case QuestionType.TimeQuestionKey:
                        var timeDifference = (DateTime.Parse(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, CultureInfo.InvariantCulture).Date - DateTime.UtcNow.Date).TotalMinutes;
                        conditionData = await SqlConnection.QueryAsync<QuestionConditionModel>(
                            "SELECT * FROM QuestionConditionModel " +
                            "WHERE Value1 <=? AND Value2 >=? AND QuestionID=?",
                            timeDifference, timeDifference, questionnaireData.Question.QuestionID
                        ).ConfigureAwait(false);
                        break;
                    default:
                        conditionData = await SqlConnection.QueryAsync<QuestionConditionModel>(
                            "SELECT * FROM QuestionConditionModel " +
                            "WHERE Value1 <=? AND Value2 >=? AND QuestionID=?"
                            , Convert.ToDecimal(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue)
                            , Convert.ToDecimal(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue)
                            , questionnaireData.Question.QuestionID
                        ).ConfigureAwait(false);
                        break;
                }

                if (conditionData?.Count == 0)
                {
                    conditionData = await SqlConnection.QueryAsync<QuestionConditionModel>(
                        "SELECT * FROM QuestionConditionModel WHERE Value1=? AND QuestionID=?"
                        , "-15000", questionnaireData.Question.QuestionID
                    ).ConfigureAwait(false);
                }
            }
            return conditionData;
        }

        private async Task<List<QuestionConditionModel>> GetTextQuestionConditionsAsync(QuestionnaireDTO questionnaireData)
        {
            List<QuestionConditionModel> conditionData;
            if (string.IsNullOrWhiteSpace(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue))
            {
                conditionData = await GetEmptyValueCondition(questionnaireData).ConfigureAwait(false);
            }
            else
            {
                conditionData = await SqlConnection.QueryAsync<QuestionConditionModel>(
                    "SELECT * FROM QuestionConditionModel WHERE Value1=? AND QuestionID=?"
                    , "-15000", questionnaireData.Question.QuestionID
                ).ConfigureAwait(false);
            }

            return conditionData;
        }

        private async Task MarkQuestionInActiveAsync(QuestionnaireDTO questionnaireData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID != default)
                {
                    transaction.Execute(
                        "UPDATE PatientQuestionnaireQuestionAnswersModel SET IsActive = 0, IsSynced = ? WHERE PatientAnswerID = ?"
                        , questionnaireData.QuestionnaireQuestionAnswer.IsSynced, questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID
                    );
                }
            }).ConfigureAwait(false);
            if (questionnaireData.Question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey)
            {
                questionnaireData?.FileDocuments?.Clear();
                /* FileDocumentModel fileDocumentModel = await CommonDataBase.SqlConnection.FindWithQueryAsync<FileDocumentModel>
                 * ($"SELECT * FROM FileDocumentModel WHERE DocumentSourceID = ? AND IsActive = 1",
                         questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID);

                 int fileDocumentModelsCount = await CommonDataBase.SqlConnection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM FileDocumentModel WHERE FileID = ? AND IsActive = 1",
                      fileDocumentModel?.FileID).ConfigureAwait(false);

                 await CommonDataBase.SqlConnection.RunInTransactionAsync(transaction =>
                 {
                     if (questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID != default)
                     {
                         transaction.Execute($"UPDATE FileDocumentModel SET IsActive = 0, IsSynced = ? WHERE DocumentSourceID = ?",
                               questionnaireData.QuestionnaireQuestionAnswer.IsSynced, questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID);

                         if (fileDocumentModelsCount == 1)
                         {
                             transaction.Execute($"UPDATE FileModel SET IsActive = 0, IsSynced = ? WHERE FileID = ?",
                                 questionnaireData.QuestionnaireQuestionAnswer.IsSynced, fileDocumentModel?.FileID);
                         }
                     }
                 }).ConfigureAwait(false);*/
            }
        }

        private async Task SavePatientQuestionAnswersAsync(QuestionnaireDTO questionnaireData, bool isNext, FileDTO fileData = null)
        {
            // Basic validation on Answer
            questionnaireData.QuestionnaireQuestionAnswer.TaskType = 1;
            questionnaireData.QuestionnaireQuestionAnswer.IsSynced = false;
            questionnaireData.QuestionnaireQuestionAnswer.IsActive = true;
            questionnaireData.QuestionnaireQuestionAnswer.PatientTaskID = questionnaireData.PatientTaskID.ToString();
            await SavePatientQuestionAnswerAsync(questionnaireData, isNext, fileData).ConfigureAwait(false);
            questionnaireData.ErrCode = ErrorCode.OK;
        }

        private async Task SavePatientQuestionAnswerAsync(QuestionnaireDTO questionnaireData, bool isNext, FileDTO fileData = null)
        {
            await SqlConnection.RunInTransactionAsync(async transaction =>
            {
                PatientQuestionnaireQuestionAnswersModel currentRecord;
                if (isNext)
                {
                    string previousQuestionCondition = questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID == null
                        ? "AND PreviousQuestionID IS NULL"
                        : $"AND PreviousQuestionID = {questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID}";

                    currentRecord = transaction.FindWithQuery<PatientQuestionnaireQuestionAnswersModel>(
                        $"SELECT * FROM PatientQuestionnaireQuestionAnswersModel WHERE QuestionID = ? AND PatientTaskID = ? AND TaskType = 1 {previousQuestionCondition}"
                        , questionnaireData.QuestionnaireQuestionAnswer.QuestionID, questionnaireData.PatientTaskID
                    );

                }
                else
                {
                    currentRecord = transaction.FindWithQuery<PatientQuestionnaireQuestionAnswersModel>(
                        "SELECT * FROM PatientQuestionnaireQuestionAnswersModel WHERE QuestionID = ? AND NextQuestionID = ? AND PatientTaskID = ? AND TaskType = 1"
                        , questionnaireData.QuestionnaireQuestionAnswer.QuestionID, questionnaireData.QuestionnaireQuestionAnswer.NextQuestionID, questionnaireData.PatientTaskID
                    );
                    questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID = currentRecord.PatientAnswerID;
                    //PAtientAnsID from here in  update case
                }
                questionnaireData.QuestionnaireQuestionAnswer.LastModifiedON = DateTimeOffset.UtcNow;
                if (currentRecord == null)
                {
                    questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID = GenerateNewGuid(transaction, false);
                    transaction.Insert(questionnaireData.QuestionnaireQuestionAnswer);
                }
                else
                {
                    questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID = currentRecord.PatientAnswerID;
                    transaction.Execute(
                        "UPDATE PatientQuestionnaireQuestionAnswersModel SET PreviousQuestionID = ?, NextQuestionID = ?, " +
                        "AnswerValue = ?, ScoreValue = ?, IsActive = ?, IsSynced = ?, LastModifiedON = ? WHERE PatientAnswerID = ?"
                        , questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID, questionnaireData.QuestionnaireQuestionAnswer.NextQuestionID
                        , questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, questionnaireData.QuestionnaireQuestionAnswer.ScoreValue
                        , questionnaireData.QuestionnaireQuestionAnswer.IsActive, questionnaireData.QuestionnaireQuestionAnswer.IsSynced
                        , questionnaireData.QuestionnaireQuestionAnswer.LastModifiedON, currentRecord.PatientAnswerID);
                }
                if (questionnaireData?.FileDocuments?.Count > 0)
                {
                    // fileData.FileDocuments.FirstOrDefault().DocumentSourceID = questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID.ToString();
                    questionnaireData.FileDocuments.FirstOrDefault().DocumentSourceID = questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID.ToString();
                    await new FilesDatabase().SaveQuestionFileAndDocumentAsync(questionnaireData.FileDocuments, questionnaireData.SelectedUserID).ConfigureAwait(false);
                    questionnaireData.FileDocuments.Clear();
                    questionnaireData.File = null;
                }
            }).ConfigureAwait(false);
        }

        private async Task CalculateQuestionScoreAsync(QuestionnaireDTO questionnaireData)
        {
            List<QuestionScoreModel> conditionData = null;
            if (string.IsNullOrWhiteSpace(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue))
            {
                //Score value for Empty condition 
                conditionData = await GetScoreValueForEmptyConditionAsync(questionnaireData).ConfigureAwait(false);
            }
            else
            {
                //Score Value for Other condifions
                switch (questionnaireData.Question.QuestionTypeID)
                {
                    case QuestionType.MultiSelectQuestionKey:
                        conditionData = await GetMultiSelectScoreConditionsAsync(questionnaireData).ConfigureAwait(false);
                        break;
                    case QuestionType.SingleSelectQuestionKey:
                    case QuestionType.DropDownQuestionKey:
                        conditionData = await GetSingleSelectScoreConditionsAsync(questionnaireData).ConfigureAwait(false);
                        break;
                    case QuestionType.MeasurementQuestionKey:
                        conditionData = await GetMeasurementQuestionScoreConditionsAsync(questionnaireData).ConfigureAwait(false);
                        break;
                    case QuestionType.MultilineTextQuestionKey:
                    case QuestionType.TextQuestionKey:
                    case QuestionType.FilesAndDocumentQuestionKey:
                        conditionData = null;
                        break;
                    case QuestionType.NumericQuestionKey:
                    case QuestionType.HorizontalSliderQuestionKey:
                    case QuestionType.VerticalSliderQuestionKey:
                    case QuestionType.DateQuestionKey:
                    case QuestionType.DateTimeQuestionKey:
                    case QuestionType.TimeQuestionKey:
                        conditionData = await GetBetweenScoreConditionsAsync(questionnaireData).ConfigureAwait(false);
                        break;
                    default:
                        break;
                }
            }

            if (conditionData == null || conditionData.Count < 1)
            {
                //Score value for Else condition
                conditionData = await GetScoreValueForElseConditionAsync(questionnaireData).ConfigureAwait(false);
            }

            if (conditionData?.Count > 0)
            {
                QuestionnaireSubscaleModel subscaleInfo = await GetQuestionnaireSubscaleAsync(questionnaireData.Questionnaire.QuestionnaireID).ConfigureAwait(false);
                questionnaireData.QuestionnaireQuestionAnswer.ScoreValue = GetQuestionScoreAsync(conditionData, subscaleInfo);
            }
            else
            {
                questionnaireData.QuestionnaireQuestionAnswer.ScoreValue = 0;
            }
        }

        private decimal? GetQuestionAnswerScoreAsync(List<PatientQuestionnaireQuestionAnswersModel> questionAnswers, QuestionnaireSubscaleModel subscale)
        {
            if (subscale?.ScoreTypeID == QuestionnaireSubscaleScoreType.AverageKey)
            {
                return questionAnswers?.Average(x => x.ScoreValue) ?? 0;
            }
            else
            {
                return questionAnswers?.Sum(x => x.ScoreValue) ?? 0;
            }
        }

        private decimal? GetQuestionScoreAsync(List<QuestionScoreModel> questionconditions, QuestionnaireSubscaleModel subscale)
        {
            if (subscale?.ScoreTypeID == QuestionnaireSubscaleScoreType.AverageKey)
            {
                return (decimal?)(questionconditions?.Average(x => x.ScoreValue) ?? 0);
            }
            else
            {
                return (decimal?)(questionconditions?.Sum(x => x.ScoreValue) ?? 0);
            }
        }

        private async Task<List<QuestionScoreModel>> GetScoreValueForEmptyConditionAsync(QuestionnaireDTO questionnaireData)
        {
            return await GetScoresAsync("0", questionnaireData.Question.QuestionID).ConfigureAwait(false);
        }

        private async Task<List<QuestionScoreModel>> GetScoreValueForElseConditionAsync(QuestionnaireDTO questionnaireData)
        {
            return await GetScoresAsync("-1", questionnaireData.Question.QuestionID).ConfigureAwait(false);
        }

        private async Task<List<QuestionScoreModel>> GetScoresAsync(string answerValues, long questionID)
        {
            return await SqlConnection.QueryAsync<QuestionScoreModel>(
                $"SELECT * FROM QuestionScoreModel WHERE QuestionID=? AND IsActive = 1 AND Value1 IN({answerValues})"
                , questionID
            ).ConfigureAwait(false);
        }

        private async Task<List<QuestionScoreModel>> GetSingleSelectScoreConditionsAsync(QuestionnaireDTO questionnaireData)
        {
            List<QuestionScoreModel> conditionData;
            conditionData = await GetScoresAsync(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, questionnaireData.Question.QuestionID).ConfigureAwait(false);
            return conditionData;
        }

        private async Task<List<QuestionScoreModel>> GetMeasurementQuestionScoreConditionsAsync(QuestionnaireDTO questionnaireData)
        {
            var reading = await new ReadingDatabase().GetPatientReadingAsync(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue);
            if (reading == null || reading.ReadingValue == 0 || (reading.ReadingValue == null && reading.ReadingValue2 == null))
            {
                //Score value for Empty condition 
                return await GetScoreValueForEmptyConditionAsync(questionnaireData).ConfigureAwait(false);
            }
            return null;
        }

        private async Task<List<QuestionScoreModel>> GetMultiSelectScoreConditionsAsync(QuestionnaireDTO questionnaireData)
        {
            var answerValues = string.Join(",", questionnaireData.QuestionnaireQuestionAnswer.AnswerValue.Split(Constants.SYMBOL_PIPE_SEPERATOR).Select(x => x));
            var conditionData = await GetScoresAsync(answerValues, questionnaireData.Question.QuestionID).ConfigureAwait(false);
            return conditionData;
        }

        private async Task<List<QuestionScoreModel>> GetBetweenScoreConditionsAsync(QuestionnaireDTO questionnaireData)
        {
            List<QuestionScoreModel> conditionData;
            switch (questionnaireData.Question.QuestionTypeID)
            {
                case QuestionType.DateQuestionKey:
                case QuestionType.DateTimeQuestionKey:
                    //var dateTime1 = Convert.ToDateTime(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue);
                    var dateDisfferance = (DateTime.Parse(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, CultureInfo.InvariantCulture).Date - DateTime.UtcNow.Date).TotalDays;
                    conditionData = await SqlConnection.QueryAsync<QuestionScoreModel>(
                        "SELECT * FROM QuestionScoreModel WHERE CAST(Value1 AS DECIMAL)<=? AND CAST(Value2 AS DECIMAL)  >=? AND QuestionID=? AND IsActive = 1"
                        , dateDisfferance, dateDisfferance, questionnaireData.Question.QuestionID
                    ).ConfigureAwait(false);
                    break;
                case QuestionType.TimeQuestionKey:
                    var timeDifference = (DateTime.Parse(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, CultureInfo.InvariantCulture)
                        - Convert.ToDateTime(DateTime.UtcNow.ToUniversalTime().ToString(Constants.DATE_TIME_STRING_FORMAT, CultureInfo.InvariantCulture))).TotalMinutes;
                    conditionData = await SqlConnection.QueryAsync<QuestionScoreModel>(
                        "SELECT * FROM QuestionScoreModel WHERE CAST(Value1 AS DECIMAL)<=? AND CAST(Value2 AS DECIMAL)  >=? AND QuestionID=? AND IsActive = 1"
                        , timeDifference, timeDifference, questionnaireData.Question.QuestionID
                    ).ConfigureAwait(false);
                    break;
                default:
                    conditionData = await SqlConnection.QueryAsync<QuestionScoreModel>(
                        "SELECT * FROM QuestionScoreModel WHERE CAST(Value1 AS DECIMAL)<=? AND CAST(Value2 AS DECIMAL)  >=? AND QuestionID=? AND IsActive = 1"
                        , Convert.ToDecimal(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue)
                        , Convert.ToDecimal(questionnaireData.QuestionnaireQuestionAnswer.AnswerValue)
                        , questionnaireData.Question.QuestionID
                    ).ConfigureAwait(false);
                    break;
            }
            return conditionData;
        }

        private void SaveQuestionnaires(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.Questionnaires))
            {
                foreach (QuestionnaireModel item in questionnaireData.Questionnaires)
                {
                    transaction.InsertOrReplace(item);
                }
            }
        }

        /// <summary>
        /// Checks if questionnaire answers is patientReadingID
        /// </summary>
        /// <param name="patientReadingID"> patientReadingID </param>
        /// <returns>true if answer value is patientReadingID else false </returns>
        public async Task<bool> CheckIfAnsValueIsReadingIDAsync(string patientReadingID)
        {
            PatientQuestionnaireQuestionAnswersModel PatientAnswer = await SqlConnection.FindWithQueryAsync<PatientQuestionnaireQuestionAnswersModel>(
                $"SELECT * FROM PatientQuestionnaireQuestionAnswersModel WHERE AnswerValue = '{patientReadingID}' "
            ).ConfigureAwait(false);
            if (PatientAnswer != null)
            {
                return true;
            }
            return false;
        }

        private void SaveQuestionnaireDetails(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireDetails))
            {
                foreach (QuestionnaireDetailsModel item in questionnaireData.QuestionnaireDetails)
                {
                    if (transaction.FindWithQuery<QuestionnaireDetailsModel>(
                        "SELECT 1 FROM QuestionnaireDetailsModel WHERE QuestionnaireID = ? AND LanguageID = ?"
                        , item.QuestionnaireID, item.LanguageID
                    ) == null)
                    {
                        transaction.Insert(item);
                    }
                    else
                    {
                        transaction.Execute(
                            $"UPDATE QuestionnaireDetailsModel SET CaptionText = ?, InstructionsText = ?, IsActive = ?, IsDataDownloaded = ? " +
                            $"WHERE QuestionnaireID = ? AND LanguageID = ?"
                            , item.CaptionText, item.InstructionsText, item.IsActive, item.IsDataDownloaded, item.QuestionnaireID, item.LanguageID
                        );
                    }
                }
            }
        }

        private void SaveQuestionnaireQuestions(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.Questions))
            {
                foreach (QuestionnaireQuestionModel item in questionnaireData.Questions)
                {
                    transaction.InsertOrReplace(item);
                }
            }
        }

        private void SaveQuestionnaireQuestionOptions(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionOptions))
            {
                foreach (QuestionnaireQuestionOptionModel item in questionnaireData.QuestionOptions)
                {
                    transaction.InsertOrReplace(item);
                }
            }
        }

        private void SaveQuestionnaireQuestionDetails(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionDetails))
            {
                foreach (QuestionnaireQuestionDetailsModel item in questionnaireData.QuestionDetails)
                {
                    if (transaction.FindWithQuery<QuestionnaireQuestionDetailsModel>(
                        "SELECT * FROM QuestionnaireQuestionDetailsModel WHERE QuestionID = ? AND LanguageID = ?"
                        , item.QuestionID, item.LanguageID
                    ) == null)
                    {
                        transaction.Insert(item);
                    }
                    else
                    {
                        transaction.Execute(
                            $"UPDATE QuestionnaireQuestionDetailsModel SET CaptionText = ?, AnswerPlaceHolder = ?, InstructionsText = ?, IsActive = ? WHERE QuestionID = ? AND LanguageID = ?"
                            , item.CaptionText, item.AnswerPlaceHolder, item.InstructionsText, item.IsActive, item.QuestionID, item.LanguageID
                        );
                    }
                }
            }
        }

        private void SaveQuestionOptionDetails(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionOptionDetails))
            {
                foreach (QuestionnaireQuestionOptionDetailsModel item in questionnaireData.QuestionOptionDetails)
                {
                    if (transaction.FindWithQuery<QuestionnaireQuestionOptionDetailsModel>(
                        "SELECT * FROM QuestionnaireQuestionOptionDetailsModel WHERE QuestionOptionID = ? AND LanguageID = ?"
                        , item.QuestionOptionID, item.LanguageID) == null)
                    {
                        transaction.Insert(item);
                    }
                    else
                    {
                        transaction.Execute(
                            $"UPDATE QuestionnaireQuestionOptionDetailsModel SET CaptionText = ?, IsActive = ? WHERE QuestionOptionID = ? AND LanguageID = ?"
                            , item.CaptionText, item.IsActive, item.QuestionOptionID, item.LanguageID);
                    }
                }
            }
        }

        private void SaveQuestionnaireSubscales(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireSubscales))
            {
                foreach (QuestionnaireSubscaleModel item in questionnaireData.QuestionnaireSubscales)
                {
                    transaction.InsertOrReplace(item);
                }
            }
        }

        private void SaveQuestionnaireSubscaleRanges(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireSubscaleRanges))
            {
                foreach (QuestionnaireSubscaleRangesModel item in questionnaireData.QuestionnaireSubscaleRanges)
                {
                    transaction.InsertOrReplace(item);
                }
            }
        }

        private void SaveQuestionnaireSubscaleRangeDetails(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireSubscaleRangeDetails))
            {
                foreach (QuestionnaireSubscaleRangeDetailsModel item in questionnaireData.QuestionnaireSubscaleRangeDetails)
                {
                    if (transaction.FindWithQuery<QuestionnaireSubscaleRangeDetailsModel>(
                        "SELECT * FROM QuestionnaireSubscaleRangeDetailsModel WHERE SubScaleRangeID = ? AND LanguageID = ?"
                        , item.SubScaleRangeID, item.LanguageID
                    ) == null)
                    {
                        transaction.Insert(item);
                    }
                    else
                    {
                        transaction.Execute(
                            $"UPDATE QuestionnaireSubscaleRangeDetailsModel SET CaptionText = ?, InstructionsText = ?, IsActive = ?, IsDataDownloaded = ? WHERE SubScaleRangeID = ? AND LanguageID = ?"
                            , item.CaptionText, item.InstructionsText, item.IsActive, item.IsDataDownloaded, item.SubScaleRangeID, item.LanguageID);
                    }
                }
            }
        }

        private void SaveQuestionnaireQuestionAnswers(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireQuestionAnswers))
            {
                foreach (PatientQuestionnaireQuestionAnswersModel item in questionnaireData.QuestionnaireQuestionAnswers)
                {
                    if (transaction.FindWithQuery<PatientQuestionnaireQuestionAnswersModel>(
                        "SELECT 1 FROM PatientQuestionnaireQuestionAnswersModel WHERE PatientAnswerID = ? "
                        , item.PatientAnswerID) == null)
                    {
                        if (transaction.FindWithQuery<PatientQuestionnaireQuestionAnswersModel>(
                            "SELECT 1 FROM PatientQuestionnaireQuestionAnswersModel WHERE PatientTaskID = ? AND QuestionID = ? AND TaskType = ?"
                            , item.PatientTaskID, item.QuestionID, item.TaskType) == null)
                        {
                            transaction.Insert(item);
                        }
                    }
                    else
                    {
                        transaction.Execute(
                            "UPDATE PatientQuestionnaireQuestionAnswersModel SET AnswerValue = ?, IsActive = ?, IsSynced = ? WHERE PatientAnswerID = ?"
                            , item.AnswerValue, item.IsActive, item.IsSynced, item.PatientAnswerID);
                    }
                }
            }
        }

        private void SaveQuestionnaireScores(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireScores))
            {
                foreach (PatientQuestionnaireScoresModel questionnaireScore in questionnaireData.QuestionnaireScores)
                {
                    if (transaction.FindWithQuery<PatientQuestionnaireScoresModel>(
                        "SELECT 1 FROM PatientQuestionnaireScoresModel WHERE PatientScoreID = ?", questionnaireScore.PatientScoreID) == null)
                    {
                        if (transaction.FindWithQuery<PatientQuestionnaireScoresModel>(
                            "SELECT 1 FROM PatientQuestionnaireScoresModel WHERE PatientTaskID = ?", questionnaireScore.PatientTaskID) == null)
                        {
                            transaction.Insert(questionnaireScore);
                        }
                    }
                    else
                    {
                        transaction.Execute("UPDATE PatientQuestionnaireScoresModel SET UserScore = ? WHERE PatientScoreID = ?"
                            , questionnaireScore.UserScore, questionnaireScore.PatientScoreID);
                    }
                }
            }
        }

        private async Task SavePatientQuestionnaireScoreAsync(QuestionnaireDTO questionnaireData, decimal? totalScore, QuestionnaireSubscaleRangesModel subscaleRange)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                PatientQuestionnaireScoresModel questionnaireScore = new PatientQuestionnaireScoresModel
                {
                    PatientScoreID = GenerateNewGuid(transaction, true),
                    SubScaleRangeID = subscaleRange.SubScaleRangeID,
                    PatientTaskID = questionnaireData.PatientTaskID,
                    UserScore = (double)totalScore,
                    IsSynced = false,
                    ErrCode = ErrorCode.OK
                };
                transaction.Insert(questionnaireScore);
            }).ConfigureAwait(false);
        }

        private void SaveQuestionScores(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionScores))
            {
                foreach (QuestionScoreModel item in questionnaireData.QuestionScores)
                {
                    if (transaction.FindWithQuery<QuestionScoreModel>(
                        "SELECT 1 FROM QuestionScoreModel WHERE Value1 = ? AND Value2 = ? AND QuestionID = ? "
                        , item.Value1, item.Value2, item.QuestionID) == null)
                    {
                        transaction.Insert(item);
                    }
                    else
                    {
                        transaction.Execute(
                            "UPDATE QuestionScoreModel SET ScoreValue = ?, IsActive = ? WHERE QuestionID = ? AND Value1 = ? AND Value2 = ? "
                            , item.ScoreValue, item.IsActive, item.QuestionID, item.Value1, item.Value2);
                    }
                }
            }
        }

        private void SaveQuestionnaireConditions(QuestionnaireDTO questionnaireData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionConditions))
            {
                foreach (QuestionConditionModel item in questionnaireData.QuestionConditions)
                {
                    string condition = $"WHERE QuestionID = {item.QuestionID} ";

                    condition += (string.IsNullOrWhiteSpace(item.Value1.ToString()))
                        ? "AND (Value1 IS NULL OR Value1 = '') "
                        : $"AND Value1 = '{item.Value1}' ";

                    condition += (string.IsNullOrWhiteSpace(item.Value2.ToString()))
                        ? "AND (Value2 IS NULL OR Value2 = '') "
                        : $"AND Value2 = '{item.Value2}' ";

                    if (transaction.FindWithQuery<QuestionConditionModel>($"SELECT 1 FROM QuestionConditionModel {condition}") == null)
                    {
                        transaction.Execute(
                            "INSERT INTO QuestionConditionModel(QuestionID, OptionID, Value1, Value2, TargetQuestionID) VALUES(?, ?, ?, ?, ?) "
                            , item.QuestionID, item.OptionID, item.Value1, item.Value2, item.TargetQuestionID);
                    }
                    else
                    {
                        transaction.Execute($"UPDATE QuestionConditionModel SET TargetQuestionID = ? {condition}", item.TargetQuestionID);
                    }
                }
            }
        }

        private Guid GenerateNewGuid(SQLiteConnection transaction, bool shouldCheckOnScoreModel)
        {
            Guid newGuid = GenericMethods.GenerateGuid();
            if (shouldCheckOnScoreModel)
            {
                while (transaction.ExecuteScalar<int>("SELECT 1 FROM PatientQuestionnaireScoresModel WHERE PatientScoreID = ?", newGuid) > 0)
                {
                    newGuid = GenericMethods.GenerateGuid();
                }
            }
            else
            {
                while (transaction.ExecuteScalar<int>("SELECT 1 FROM PatientQuestionnaireQuestionAnswersModel WHERE PatientAnswerID = ?", newGuid) > 0)
                {
                    newGuid = GenericMethods.GenerateGuid();
                }
            }
            return newGuid;
        }

        private async Task<QuestionnaireAction> GetQuestionnaireActionAsync(string patientTaskID, bool isNextAction, bool isLastQuestion)
        {
            if (isNextAction)
            {
                return QuestionnaireAction.PreviousAndNext;
            }
            else
            {
                short count = await GetQuestionnaireStatusAsync(patientTaskID).ConfigureAwait(false);
                if (count == -1)
                {
                    return QuestionnaireAction.StartQuestionnaire;
                }
                else if (count == 0 || isLastQuestion)
                {
                    return QuestionnaireAction.Done;
                }
                else
                {
                    return QuestionnaireAction.PreviousAndNext;
                }
            }
        }

        private async Task<int> GetQuestionnaireQuestionCountAsync(string patientTaskID)
        {
            return await SqlConnection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM QuestionnaireQuestionModel A JOIN TaskModel B ON A.QuestionnaireID = B.ItemID " +
                "WHERE A.IsActive = 1 AND B.PatientTaskID = ?", patientTaskID
            ).ConfigureAwait(false);
        }

        private async Task<short> GetQuestionnaireStatusAsync(string patientTaskId)
        {
            /// -1 -> Not Started
            /// 0  -> Completed
            /// >0 -> In Progress
            byte count = await SqlConnection.ExecuteScalarAsync<byte>(
                "SELECT COUNT(1) FROM PatientQuestionnaireScoresModel WHERE PatientTaskID = ?", patientTaskId
            ).ConfigureAwait(false);
            if (count > 0)
            {
                return 0;
            }
            count = await SqlConnection.ExecuteScalarAsync<byte>(
                "SELECT COUNT(1) FROM PatientQuestionnaireQuestionAnswersModel WHERE PatientTaskID = ? AND TaskType = 1 AND IsActive = 1", patientTaskId
            ).ConfigureAwait(false);
            if (count == 0)
            {
                return -1;
            }
            return count;
        }
    }
}
