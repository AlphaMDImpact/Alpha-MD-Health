using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer;

public class QuestionnaireServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get Questionnaire(s)
    /// </summary>
    /// <param name="questionnaireData">Questionnaires data object</param>
    /// <returns>Questionnaire(s)</returns>
    public async Task GetQuestionnairesAsync(QuestionnaireDTO questionnaireData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionnaireData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireModel.QuestionnaireID), questionnaireData.Questionnaire.QuestionnaireID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
        MapCommonSPParameters(questionnaireData, parameters,AppPermissions.QuestionnairesView.ToString(),$"{AppPermissions.QuestionnairePublish},{AppPermissions.QuestionnaireAddEdit}");
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_QUESTIONNAIRES, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            if (questionnaireData.RecordCount == -1)
            {
                await MapQuestionnairesDataAsync(questionnaireData, result).ConfigureAwait(false);
            }
            else
            {
                questionnaireData.Questionnaires = (await result.ReadAsync<QuestionnaireModel>().ConfigureAwait(false)).ToList();
            }
            await MapReturnPermissionsAsync(questionnaireData, result).ConfigureAwait(false);
        }
        questionnaireData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Saves questionnaire in Database
    /// </summary>
    /// <param name="questionnaireData">Referenece object that contains questionnaire to be saved in database</param>
    /// <param name="isAfterImageUpload">Flag representing image data is uploaded in blob storage or not</param>
    /// <returns>operation status</returns>
    public async Task SaveQuestionnaireAsync(QuestionnaireDTO questionnaireData, bool isAfterImageUpload)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionnaireData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(QuestionnaireDTO.IsActive)), questionnaireData.IsActive, DbType.Boolean, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(QuestionnaireModel.QuestionnaireCode)), questionnaireData.Questionnaire.QuestionnaireCode, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(QuestionnaireModel.QuestionnaireTypeID)), questionnaireData.Questionnaire.QuestionnaireTypeID, DbType.Int16, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(QuestionnaireModel.DefaultRespondentID)), questionnaireData.Questionnaire.DefaultRespondentID, DbType.Int16, ParameterDirection.Input);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), ConvertGroupDetailsToTable(questionnaireData, isAfterImageUpload).AsTableValuedParameter());
        parameter.Add(ConcateAt(nameof(QuestionnaireModel.QuestionnaireID)), questionnaireData.Questionnaire.QuestionnaireID, DbType.Int64, ParameterDirection.InputOutput);
        MapCommonSPParameters(questionnaireData, parameter, questionnaireData.IsActive ? AppPermissions.QuestionnaireAddEdit.ToString() : AppPermissions.QuestionnaireDelete.ToString());
        await connection.QueryAsync(SPNameConstants.USP_SAVE_QUESTIONNAIRE, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        questionnaireData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        if (questionnaireData.ErrCode == ErrorCode.OK)
        {
            questionnaireData.Questionnaire.QuestionnaireID = parameter.Get<long>(ConcateAt(nameof(QuestionnaireModel.QuestionnaireID)));
        }
    }

    /// <summary>
    /// Saves questionnaire status in Database
    /// </summary>
    /// <param name="questionnaireData">Referenece object that contains questionnaire status to be saved in database</param>
    /// <returns>operation status</returns>
    public async Task PublishQuestionnaireAsync(BaseDTO resultDTO)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), resultDTO.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireModel.QuestionnaireID), resultDTO.RecordCount, DbType.Int64, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireModel.IsPublished), resultDTO.IsActive, DbType.Boolean, ParameterDirection.Input);
        MapCommonSPParameters(resultDTO, parameters, AppPermissions.QuestionnairePublish.ToString());
        await connection.QueryAsync(SPNameConstants.USP_PUBLISH_QUESTIONNAIRE, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        resultDTO.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }



    /// <summary>
    /// GET questionnaire question data
    /// </summary>
    /// <param name="questionnaireData">Reference object to map retrived data</param>
    /// <returns>questions data mapped in reference object</returns>
    public async Task GetQuestionnaireQuestionsAsync(QuestionnaireDTO questionnaireData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionnaireData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionnaireID), questionnaireData.Question.QuestionnaireID, DbType.Int64, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionID), questionnaireData.Question.QuestionID, DbType.Int64, direction: ParameterDirection.Input);
        MapCommonSPParameters(questionnaireData, parameters, AppPermissions.QuestionsView.ToString(), $"{AppPermissions.QuestionDelete},{AppPermissions.QuestionAddEdit}"
         );
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_QUESTIONNAIRE_QUESTIONS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            if (questionnaireData.RecordCount == -1)
            {
                await MapQuestionDataFromDBAsync(questionnaireData, result).ConfigureAwait(false);
            }
            else
            {
                questionnaireData.Questions = (await result.ReadAsync<QuestionnaireQuestionModel>().ConfigureAwait(false))?.ToList();
            }
            await MapReturnPermissionsAsync(questionnaireData, result).ConfigureAwait(false);
        }
        questionnaireData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Save Questionnaire question 
    /// </summary>
    /// <param name="questionnaireData">Data to be saved</param>
    /// <returns>Opeation status</returns>
    public async Task SaveQuestionnaireQuestionAsync(QuestionnaireDTO questionnaireData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionnaireData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionnaireID), questionnaireData.Question.QuestionnaireID, DbType.Int64, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionID), questionnaireData.Question.QuestionID, DbType.Int64, direction: ParameterDirection.InputOutput);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionTypeID), questionnaireData.Question.QuestionTypeID.ToString(), DbType.String, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.AddToMedicalHistory), questionnaireData.Question.AddToMedicalHistory, DbType.Boolean, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.ShowValueToPatient), questionnaireData.Question.ShowValueToPatient, DbType.Boolean, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.SliderSteps), questionnaireData.Question.SliderSteps, DbType.Decimal, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.MinValue), questionnaireData.Question.MinValue, DbType.Decimal, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.MaxValue), questionnaireData.Question.MaxValue, DbType.Decimal, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.CategoryID), questionnaireData.Question.CategoryID, DbType.Int16, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.IsRequired), questionnaireData.Question.IsRequired, DbType.Boolean, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.IsActive), questionnaireData.Question.IsActive, DbType.Boolean, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireDTO.QuestionDetails), ConvertToPageDetailsTable(questionnaireData.QuestionDetails).AsTableValuedParameter());
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireDTO.QuestionOptions), ConvertToQuestionOptionsTable(questionnaireData.QuestionOptions).AsTableValuedParameter());
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireDTO.QuestionnaireQuestionOptionDetails), ConvertToQuestionOptionDetailsTable(questionnaireData.QuestionnaireQuestionOptionDetails).AsTableValuedParameter());
        MapCommonSPParameters(questionnaireData, parameters, questionnaireData.Question.IsActive ? AppPermissions.QuestionAddEdit.ToString() : AppPermissions.QuestionnaireDelete.ToString());
        await connection.QueryAsync(SPNameConstants.USP_SAVE_QUESTIONNAIRE_QUESTION, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        questionnaireData.Question.QuestionID = parameters.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionID));
        questionnaireData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }

    /// <summary>
    /// Get Question Conditions
    /// </summary>
    /// <param name="questionnaireData">Questionnaires data object</param>
    /// <returns>Questionnaire Question Conditions</returns>
    public async Task GetQuestionConditionsAsync(QuestionnaireDTO questionnaireData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionnaireData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionnaireID), questionnaireData.Question.QuestionnaireID, DbType.Int64, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionID), questionnaireData.Question.QuestionID, DbType.Int64, direction: ParameterDirection.Input);
        MapCommonSPParameters(questionnaireData, parameters, AppPermissions.QuestionnaireQuestionsView.ToString(), $"{AppPermissions.QuestionnaireQuestionDelete},{AppPermissions.QuestionnaireQuestionAddEdit}"
         );
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_QUESTIONNAIRE_QUESTION_CONDITIONS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            if (questionnaireData.RecordCount == -1)
            {
                await MapQuestionnairesLinkedQuestionDataAsync(questionnaireData, result).ConfigureAwait(false);
            }
            else
            {
                questionnaireData.Questions = (await result.ReadAsync<QuestionnaireQuestionModel>().ConfigureAwait(false)).ToList();
            }
            await MapReturnPermissionsAsync(questionnaireData, result).ConfigureAwait(false);
        }
        questionnaireData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Save Question Conditions Async
    /// </summary>
    /// <param name="questionData">question and condition data</param>
    /// <returns></returns>
    public async Task SaveQuestionConditionsAsync(QuestionnaireDTO questionData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionnaireID), questionData.Question.QuestionnaireID, DbType.Int64, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionID), questionData.Question.QuestionID, DbType.Int64, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.IsStartingQuestion), questionData.Question.IsStartingQuestion, DbType.Boolean, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, ConvertToQuestionConditionsTable(questionData.QuestionConditions).AsTableValuedParameter());
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.IsActive), questionData.Question.IsActive, DbType.Boolean, direction: ParameterDirection.Input);
        MapCommonSPParameters(questionData, parameters, questionData.Question.IsActive ? AppPermissions.QuestionAddEdit.ToString() : AppPermissions.QuestionnaireDelete.ToString());
        await connection.QueryAsync(SPNameConstants.USP_SAVE_QUESTIONNAIRE_QUESTION_CONDITIONS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        questionData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }

    /// <summary>
    /// Get Sub scale data for the selected questionnaire
    /// </summary>
    /// <param name="questionnaireData">Questionnaires data object</param>
    /// <returns>operation status</returns>
    public async Task GetQuestionnaireSubscaleAsync(QuestionnaireDTO questionnaireData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionnaireData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireModel.QuestionnaireID), questionnaireData.Questionnaire.QuestionnaireID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireSubscaleModel.SubscaleID), questionnaireData.QuestionnaireSubscaleData.SubscaleID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireSubscaleRangesModel.SubScaleRangeID), questionnaireData.QuestionnaireSubscaleRange.SubScaleRangeID, DbType.Int64, ParameterDirection.Input);
        MapCommonSPParameters(questionnaireData, parameters, AppPermissions.SubscalesView.ToString(), $"{AppPermissions.SubscaleDelete},{AppPermissions.SubscaleAddEdit}"
       );
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_QUESTIONNAIRE_SUBSCALE, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            if (questionnaireData.RecordCount == -2)
            {
                questionnaireData.DropDownOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false)).ToList();
            }
            else if (questionnaireData.RecordCount == -1)
            {
                await GetQuestionnaireSubscaleDataAsync(questionnaireData, result).ConfigureAwait(false);
            }
            else
            {
                questionnaireData.QuestionnaireSubscaleRanges = (await result.ReadAsync<QuestionnaireSubscaleRangesModel>().ConfigureAwait(false)).ToList();
            }
            await MapReturnPermissionsAsync(questionnaireData, result).ConfigureAwait(false);
        }
        questionnaireData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Saves Questionnaire subscale in Database
    /// </summary>
    /// <param name="questionnaireData">Object that contains subscale data to be saved in database</param>
    /// <returns>operation status</returns>
    public async Task SaveQuestionnaireSubscaleAsync(BaseDTO questionnaireData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionnaireData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireSubscaleModel.ScoreTypeID), questionnaireData.ErrorDescription, DbType.String, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireSubscaleModel.QuestionnaireID), questionnaireData.RecordCount, DbType.Int64, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireSubscaleModel.SubscaleID), 0, DbType.Int64, direction: ParameterDirection.InputOutput);
        MapCommonSPParameters(questionnaireData, parameters, AppPermissions.SubscaleAddEdit.ToString());
        await connection.QueryAsync(SPNameConstants.USP_SAVE_QUESTIONNAIRE_SUBSCALE, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        questionnaireData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        if (questionnaireData.ErrCode == ErrorCode.OK)
        {
            questionnaireData.RecordCount = parameters.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireSubscaleModel.SubscaleID));
        }
    }

    /// <summary>
    /// Saves Questionnaire subscale ranges in Database
    /// </summary>
    /// <param name="questionnaireData">Object that contains subscale ranges to be saved in database</param>
    /// <param name="isAfterImageUpload">Flag representing image data is uploaded in blob storage or not</param>
    /// <returns>operation status</returns>
    public async Task SaveQuestionnaireSubscaleRangesAsync(QuestionnaireDTO questionnaireData, bool isAfterImageUpload)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionnaireData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(QuestionnaireSubscaleRangesModel.SubScaleID)), questionnaireData.QuestionnaireSubscaleRange.SubScaleID, DbType.Int64, direction: ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(QuestionnaireSubscaleRangesModel.SubScaleRangeID)), questionnaireData.QuestionnaireSubscaleRange.SubScaleRangeID, DbType.Int64, direction: ParameterDirection.InputOutput);
        parameter.Add(ConcateAt(nameof(QuestionnaireSubscaleRangesModel.MinValue)), questionnaireData.QuestionnaireSubscaleRange.MinValue, DbType.Decimal, direction: ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(QuestionnaireSubscaleRangesModel.MaxValue)), questionnaireData.QuestionnaireSubscaleRange.MaxValue, DbType.Decimal, direction: ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(QuestionnaireDTO.IsActive)), questionnaireData.IsActive, DbType.Boolean, direction: ParameterDirection.Input);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), ConvertGroupDetailsToTable(questionnaireData, isAfterImageUpload).AsTableValuedParameter());
        MapCommonSPParameters(questionnaireData, parameter, questionnaireData.IsActive ? AppPermissions.SubscaleAddEdit.ToString() : AppPermissions.SubscaleDelete.ToString());
        await connection.QueryAsync(SPNameConstants.USP_SAVE_QUESTIONNAIRE_SUBSCALE_RANGES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        questionnaireData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        if (questionnaireData.ErrCode == ErrorCode.OK)
        {
            questionnaireData.QuestionnaireSubscaleRange.SubScaleRangeID = parameter.Get<long>(ConcateAt(nameof(QuestionnaireSubscaleRangesModel.SubScaleRangeID)));
        }
    }

    /// <summary>
    /// Get Question score 
    /// </summary>
    /// <param name="questionnaireData">Questionnaires data object</param>
    /// <returns>Questionnaire score(s)</returns>
    public async Task GetQuestionScoreAsync(QuestionnaireDTO questionnaireData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionnaireData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionnaireID), questionnaireData.Question.QuestionnaireID, DbType.Int64, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionID), questionnaireData.Question.QuestionID, DbType.Int64, direction: ParameterDirection.Input);
        MapCommonSPParameters(questionnaireData, parameters, AppPermissions.QuestionScoresView.ToString(),
             $"{AppPermissions.QuestionScoreDelete},{AppPermissions.QuestionScoreAddEdit}"
         );
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_QUESTIONNAIRE_QUESTION_SCORES, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            if (questionnaireData.RecordCount == -1)
            {
                await MapQuestionnairesScoreDataAsync(questionnaireData, result).ConfigureAwait(false);
            }
            else
            {
                questionnaireData.QuestionnaireQuestionScores = (await result.ReadAsync<QuestionScoreModel>().ConfigureAwait(false)).ToList();
            }
            await MapReturnPermissionsAsync(questionnaireData, result).ConfigureAwait(false);
        }
        questionnaireData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Save Question Score Async
    /// </summary>
    /// <param name="questionData">question and score data</param>
    /// <returns></returns>
    public async Task SaveQuestionScoresAsync(QuestionnaireDTO questionData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionnaireID), questionData.Question.QuestionnaireID, DbType.Int64, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.QuestionID), questionData.Question.QuestionID, DbType.Int64, direction: ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, ConvertToQuestionScoresTable(questionData.QuestionScores).AsTableValuedParameter());
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireQuestionModel.IsActive), questionData.Question.IsActive, DbType.Boolean, direction: ParameterDirection.Input);
        MapCommonSPParameters(questionData, parameters, questionData.Question.IsActive ? AppPermissions.QuestionScoreAddEdit.ToString() : AppPermissions.QuestionScoreDelete.ToString());
        await connection.QueryAsync(SPNameConstants.USP_SAVE_QUESTIONNAIRE_QUESTION_SCORES, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        questionData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }

    /// <summary>
    /// Saves questionnaire result in Database
    /// </summary>
    /// <param name="questionnaireData">reference object which holds patient questionnaire results</param>
    /// <returns>operation status</returns>
    public async Task SavePatientQuestionnaireResultsAsync(QuestionnaireDTO questionnaireData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionnaireData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        MapCommonSPParameters(questionnaireData, parameter, string.Empty);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireDTO.QuestionnaireQuestionAnswers), ConvertQuestionAnswersToDataTable(questionnaireData.QuestionnaireQuestionAnswers).AsTableValuedParameter());
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(QuestionnaireDTO.QuestionnaireScores), ConvertQuestionScoresToDataTable(questionnaireData.QuestionnaireScores).AsTableValuedParameter());
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_PATIENT_QUESTIONNAIRE_RESULTS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            questionnaireData.QuestionnaireQuestionAnswerResult = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false)).ToList();
            if (!result.IsConsumed)
            {
                questionnaireData.QuestionnaireScoreResult = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false)).ToList();
            }
            questionnaireData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }
    }

    private DataTable ConvertQuestionScoresToDataTable(List<PatientQuestionnaireScoresModel> questionnaireScores)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(PatientQuestionnaireScoresModel.PatientScoreID), typeof(Guid)),
                new DataColumn(nameof(PatientQuestionnaireScoresModel.SubScaleRangeID), typeof(long)),
                new DataColumn(nameof(PatientQuestionnaireScoresModel.PatientTaskID), typeof(string)),
                new DataColumn(nameof(PatientQuestionnaireScoresModel.UserScore), typeof(double)),
                new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(byte))
            }
        };
        int count = 0;
        if (GenericMethods.IsListNotEmpty(questionnaireScores))
        {
            foreach (var item in questionnaireScores)
            {
                dataTable.Rows.Add(item.PatientScoreID, item.SubScaleRangeID, item.PatientTaskID, item.UserScore, /*item.CaptionText, item.InstructionsText,*/ ++count);
            }
        }
        return dataTable;
    }

    private DataTable ConvertToQuestionConditionsTable(List<QuestionConditionModel> questionDetails)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(QuestionConditionModel.Value1), typeof(double)),
                new DataColumn(nameof(QuestionConditionModel.Value2),  typeof(double)),
                new DataColumn(nameof(QuestionConditionModel.TargetQuestionID),  typeof(string)),
            }
        };
        if (GenericMethods.IsListNotEmpty(questionDetails))
        {
            foreach (var item in questionDetails)
            {
                dataTable.Rows.Add(item.Value1 == 0 ? 0 : item.Value1, item.Value2 == 0 ? 0 : item.Value2, item.TargetQuestionID == 0 ? null : item.TargetQuestionID);
            }
        }
        return dataTable;
    }

    private DataTable ConvertToPageDetailsTable(List<QuestionnaireQuestionDetailsModel> pageDetails)
    {
        DataTable dataTable = CreateGenericTypeTable();
        foreach (var item in pageDetails)
        {
            dataTable.Rows.Add(item.QuestionID, Guid.Empty, item.LanguageID, item.CaptionText, item.InstructionsText, item.AnswerPlaceHolder);
        }
        return dataTable;
    }

    private DataTable ConvertToQuestionOptionsTable(List<QuestionnaireQuestionOptionModel> questionOptions)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(QuestionnaireQuestionOptionModel.QuestionOptionID), typeof(long)),
                new DataColumn(nameof(QuestionnaireQuestionOptionModel.QuestionID), typeof(long)),
                new DataColumn(nameof(QuestionnaireQuestionOptionModel.SequenceNo), typeof(int)),
                new DataColumn("OptionValue", typeof(float)),
                new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(int))
            }
        };
        int count = 0;
        if (questionOptions != null)
        {
            foreach (var item in questionOptions)
            {
                dataTable.Rows.Add(item.QuestionOptionID, item.QuestionID, item.SequenceNo, 0, ++count);
            }
        }
        return dataTable;
    }

    private DataTable ConvertToQuestionOptionDetailsTable(List<QuestionnaireQuestionOptionModel> questionOptionDetails)
    {
        DataTable dataTable = CreateGenericTypeTable();
        if (questionOptionDetails != null)
        {
            foreach (var item in questionOptionDetails)
            {
                dataTable.Rows.Add(item.QuestionOptionID, Guid.Empty, item.LanguageID, item.CaptionText, string.Empty, string.Empty);
            }
        }
        return dataTable;
    }

    private async Task MapQuestionDataFromDBAsync(QuestionnaireDTO questionnaireData, SqlMapper.GridReader result)
    {
        questionnaireData.QuestionDetails = (await result.ReadAsync<QuestionnaireQuestionDetailsModel>().ConfigureAwait(false)).ToList();
        if (questionnaireData.Question.QuestionID > 0)
        {
            if (!result.IsConsumed)
            {
                questionnaireData.Question = (await result.ReadAsync<QuestionnaireQuestionModel>().ConfigureAwait(false))?.FirstOrDefault();
            }
            if (!result.IsConsumed)
            {
                questionnaireData.QuestionOptions = (await result.ReadAsync<QuestionnaireQuestionOptionModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                questionnaireData.QuestionnaireQuestionOptionDetails = (await result.ReadAsync<QuestionnaireQuestionOptionModel>().ConfigureAwait(false))?.ToList();
            }
        }
        questionnaireData.DefaultRespondants = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
        questionnaireData.ReadingsOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
    }

    private async Task MapQuestionnairesDataAsync(QuestionnaireDTO questionnaireData, SqlMapper.GridReader result)
    {
        if (questionnaireData.Questionnaire.QuestionnaireID > 0)
        {
            questionnaireData.Questionnaire = (await result.ReadAsync<QuestionnaireModel>().ConfigureAwait(false)).FirstOrDefault();
        }
        if (!result.IsConsumed)
        {
            questionnaireData.PageDetails = (await result.ReadAsync<ContentDetailModel>().ConfigureAwait(false))?.ToList();
        }
    }

    private async Task MapQuestionnairesLinkedQuestionDataAsync(QuestionnaireDTO questionnaireData, SqlMapper.GridReader result)
    {
        if (!result.IsConsumed)
        {
            questionnaireData.DropDownOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
        }
        if (questionnaireData.Question.QuestionnaireID > 0)
        {
            questionnaireData.Question = (await result.ReadAsync<QuestionnaireQuestionModel>().ConfigureAwait(false)).FirstOrDefault();
        }
        if (!result.IsConsumed)
        {
            questionnaireData.QuestionConditions = (await result.ReadAsync<QuestionConditionModel>().ConfigureAwait(false))?.ToList();
        }
    }

    private async Task MapQuestionnairesScoreDataAsync(QuestionnaireDTO questionnaireData, SqlMapper.GridReader result)
    {
        if (!result.IsConsumed)
        {
            questionnaireData.DropDownOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
        }
        if (questionnaireData.Question.QuestionnaireID > 0)
        {
            questionnaireData.Question = (await result.ReadAsync<QuestionnaireQuestionModel>().ConfigureAwait(false)).FirstOrDefault();
        }
        if (!result.IsConsumed)
        {
            questionnaireData.QuestionScores = (await result.ReadAsync<QuestionScoreModel>().ConfigureAwait(false))?.ToList();
        }
    }

    private DataTable ConvertGroupDetailsToTable(QuestionnaireDTO questionnaireData, bool isAfterImageUpload)
    {
        DataTable dataTable = CreateGenericTypeTable();
        if (GenericMethods.IsListNotEmpty(questionnaireData.PageDetails))
        {
            foreach (ContentDetailModel record in questionnaireData.PageDetails)
            {
                dataTable.Rows.Add(record.PageID, Guid.Empty, record.LanguageID, record.PageHeading, isAfterImageUpload ? record.PageData : string.Empty, string.Empty);
            }
        }
        return dataTable;
    }

    private async Task GetQuestionnaireSubscaleDataAsync(QuestionnaireDTO questionnaireData, SqlMapper.GridReader result)
    {
        questionnaireData.QuestionnaireSubscaleRange = (await result.ReadAsync<QuestionnaireSubscaleRangesModel>().ConfigureAwait(false))?.First();
        if (!result.IsConsumed)
        {
            questionnaireData.PageDetails = (await result.ReadAsync<ContentDetailModel>().ConfigureAwait(false))?.ToList();
        }
    }

    private DataTable ConvertToQuestionScoresTable(List<QuestionScoreModel> questionDetails)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(QuestionScoreModel.Value1), typeof(double)),
                new DataColumn(nameof(QuestionScoreModel.Value2), typeof(double)),
                new DataColumn(nameof(QuestionScoreModel.ScoreValue), typeof(double)),
            }
        };
        if (GenericMethods.IsListNotEmpty(questionDetails))
        {
            foreach (var item in questionDetails)
            {
                dataTable.Rows.Add(item.Value1, item.Value2, item.ScoreValue);
            }
        }
        return dataTable;
    }

    private DataTable ConvertQuestionAnswersToDataTable(List<PatientQuestionnaireQuestionAnswersModel> questionnaireAnswers)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(PatientQuestionnaireQuestionAnswersModel.PatientAnswerID), typeof(Guid)),
                new DataColumn(nameof(PatientQuestionnaireQuestionAnswersModel.QuestionID), typeof(long)),
                new DataColumn(nameof(PatientQuestionnaireQuestionAnswersModel.PreviousQuestionID), typeof(long)),
                new DataColumn(nameof(PatientQuestionnaireQuestionAnswersModel.NextQuestionID), typeof(long)),
                new DataColumn(nameof(PatientQuestionnaireQuestionAnswersModel.TaskType), typeof(int)),
                new DataColumn(nameof(PatientQuestionnaireQuestionAnswersModel.PatientTaskID), typeof(string)),
                new DataColumn(nameof(PatientQuestionnaireQuestionAnswersModel.AnswerValue), typeof(string)),
                new DataColumn(nameof(PatientQuestionnaireQuestionAnswersModel.ScoreValue), typeof(decimal)),
                new DataColumn(nameof(PatientQuestionnaireQuestionAnswersModel.IsActive), typeof(bool)),
                new DataColumn(nameof(PatientQuestionnaireQuestionAnswersModel.LastModifiedON), typeof(DateTimeOffset)),
                new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(byte))
            }
        };
        int count = 0;
        if (GenericMethods.IsListNotEmpty(questionnaireAnswers))
        {
            foreach (var item in questionnaireAnswers)
            {
                dataTable.Rows.Add(item.PatientAnswerID, item.QuestionID, item.PreviousQuestionID == 0 ? null : item.PreviousQuestionID,
                    item.NextQuestionID == 0 ? null : item.NextQuestionID, item.TaskType, item.PatientTaskID, item.AnswerValue ?? string.Empty, item.ScoreValue,
                    item.IsActive, item.LastModifiedON, ++count);
            }
        }
        return dataTable;
    }
}