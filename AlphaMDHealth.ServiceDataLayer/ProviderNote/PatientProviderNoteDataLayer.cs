using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer;

public class PatientProviderNoteDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get Patient Provider Note(s) Data
    /// </summary>
    /// <param name="patientProviderNoteData">Reference object having input data</param>
    /// <returns>Provider Note(s) data with operation status</returns>
    public async Task GetPatientProviderNotesAsync(PatientProviderNoteDTO patientProviderNoteData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), patientProviderNoteData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProviderNoteModel.ProviderNoteID)), patientProviderNoteData.PatientProviderNote.ProviderNoteID, DbType.Guid, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), patientProviderNoteData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProviderNoteModel.ProgramID)), patientProviderNoteData.PatientProviderNote.ProgramID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProviderNoteModel.QuestionnaireID)), patientProviderNoteData.PatientProviderNote.QuestionnaireID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), patientProviderNoteData.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
        AddDateTimeParameter(nameof(BaseDTO.FromDate), patientProviderNoteData.FromDate, parameter, ParameterDirection.Input);
        AddDateTimeParameter(nameof(BaseDTO.ToDate), patientProviderNoteData.ToDate, parameter, ParameterDirection.Input);
        MapCommonSPParameters(patientProviderNoteData, parameter
            , patientProviderNoteData.RecordCount == -1
                ? AppPermissions.PatientProviderNoteAddEdit.ToString()
                : AppPermissions.PatientProviderNotesView.ToString()
            , patientProviderNoteData.RecordCount == -1
                ? $"{AppPermissions.PatientProviderNoteAddEdit},{AppPermissions.PatientProviderNoteDelete}"
                : AppPermissions.PatientProviderNoteAddEdit.ToString()
        );
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PATIENT_PROVIDER_NOTES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            await MapPatientProviderNotesViewDataAsync(patientProviderNoteData, result).ConfigureAwait(false);
            await MapReturnPermissionsAsync(patientProviderNoteData, result).ConfigureAwait(false);
        }
        patientProviderNoteData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
    }

    internal async Task MapPatientProviderNotesViewDataAsync(PatientProviderNoteDTO patientProviderNoteData, SqlMapper.GridReader result)
    {
        if (patientProviderNoteData.RecordCount == -1 && !result.IsConsumed)
        {
            if (patientProviderNoteData.PatientProviderNote?.ProviderNoteID != Guid.Empty && !result.IsConsumed)
            {
                patientProviderNoteData.PatientProviderNote = (await result.ReadFirstAsync<PatientProviderNoteModel>().ConfigureAwait(false));
                patientProviderNoteData.QuestionnaireQuestionAnswers = (await result.ReadAsync<PatientQuestionnaireQuestionAnswersModel>().ConfigureAwait(false))?.ToList();
                patientProviderNoteData.FileDocuments = (await result.ReadAsync<FileDocumentModel>().ConfigureAwait(false))?.ToList();
            }
            patientProviderNoteData.PatientPrograms = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
        }
        if (patientProviderNoteData.RecordCount == -2 || (patientProviderNoteData.PatientProviderNote != null && patientProviderNoteData.PatientProviderNote.ProviderNoteID != Guid.Empty) && !result.IsConsumed)
        {
            patientProviderNoteData.Providers = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
            patientProviderNoteData.ProgramNotes = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
        }
        if (patientProviderNoteData.RecordCount == -3 || (patientProviderNoteData.PatientProviderNote != null && patientProviderNoteData.PatientProviderNote?.ProviderNoteID != Guid.Empty) && !result.IsConsumed)
        {
            patientProviderNoteData.QuestionConditions = (await result.ReadAsync<QuestionConditionModel>().ConfigureAwait(false))?.ToList();
            patientProviderNoteData.QuestionnaireQuestions = (await result.ReadAsync<QuestionnaireQuestionModel>().ConfigureAwait(false))?.ToList();
            patientProviderNoteData.QuestionnaireQuestionOptions = (await result.ReadAsync<QuestionnaireQuestionOptionModel>().ConfigureAwait(false))?.ToList();
            patientProviderNoteData.Files = (await result.ReadAsync<FileModel>().ConfigureAwait(false))?.ToList();
        }
        if (patientProviderNoteData.RecordCount >= 0)
        {
            patientProviderNoteData.PatientProviderNotes = (await result.ReadAsync<PatientProviderNoteModel>().ConfigureAwait(false))?.ToList();
            patientProviderNoteData.QuestionnaireQuestions = (await result.ReadAsync<QuestionnaireQuestionModel>().ConfigureAwait(false))?.ToList();
            patientProviderNoteData.QuestionnaireQuestionOptions = (await result.ReadAsync<QuestionnaireQuestionOptionModel>().ConfigureAwait(false))?.ToList();
        }
    }

    /// <summary>
    ///Save Patient Provider Note Data
    /// </summary>
    /// <param name="patientProviderNoteData">Reference object which holds patinet provider note data</param>
    /// <returns>Operation Status Code</returns>
    public async Task SavePatientProviderNoteAsync(PatientProviderNoteDTO patientProviderNoteData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), patientProviderNoteData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientProviderNoteModel.ProviderNoteID), patientProviderNoteData.PatientProviderNote.ProviderNoteID, DbType.Guid, ParameterDirection.InputOutput);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientProviderNoteModel.CareGiverID), patientProviderNoteData.PatientProviderNote.CareGiverID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientProviderNoteModel.PatientID), patientProviderNoteData.PatientProviderNote.PatientID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientProviderNoteModel.ProgramNoteID), patientProviderNoteData.PatientProviderNote.ProgramNoteID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientProviderNoteModel.NoteDateTime), patientProviderNoteData.PatientProviderNote.NoteDateTime, DbType.DateTimeOffset, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientProviderNoteDTO.IsActive), patientProviderNoteData.PatientProviderNote.IsActive, DbType.Boolean, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientProviderNoteModel.AddedOn), patientProviderNoteData.PatientProviderNote.AddedOn, DbType.DateTimeOffset, ParameterDirection.Input);
        MapCommonSPParameters(patientProviderNoteData, parameters, string.Empty);
        await connection.ExecuteAsync(SPNameConstants.USP_SAVE_PATIENT_PROVIDER_NOTES, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        patientProviderNoteData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);

        if (patientProviderNoteData.ErrCode == ErrorCode.OK && patientProviderNoteData.PatientProviderNote.IsActive != false)
        {
            patientProviderNoteData.PatientProviderNote.ProviderNoteID = parameters.Get<Guid>(Constants.SYMBOL_AT_THE_RATE + nameof(PatientProviderNoteModel.ProviderNoteID));
            patientProviderNoteData.QuestionnaireQuestionAnswers.ForEach(x => x.PatientTaskID = patientProviderNoteData.PatientProviderNote.ProviderNoteID.ToString());
            QuestionnaireDTO questionAnswers = new QuestionnaireDTO
            {
                AccountID = patientProviderNoteData.AccountID,
                PermissionAtLevelID = patientProviderNoteData.PermissionAtLevelID,
                QuestionnaireQuestionAnswers = patientProviderNoteData.QuestionnaireQuestionAnswers,
                QuestionnaireQuestionScores = new List<QuestionScoreModel>(),
                FeatureFor = patientProviderNoteData.FeatureFor
            };
            await new QuestionnaireServiceDataLayer().SavePatientQuestionnaireResultsAsync(questionAnswers).ConfigureAwait(false);
            patientProviderNoteData.ErrCode = questionAnswers.ErrCode;
            patientProviderNoteData.QuestionAnswerResult = questionAnswers.QuestionnaireQuestionAnswerResult;
        }
    }

    /// <summary>
    /// Get next/previous question
    /// </summary>
    /// <param name="questionnaireData">Reference object which holds questionnaire question data</param>
    /// <returns>Next/previous question with operation Status Code</returns>
    public async Task GetNextQuestionAsync(QuestionnaireDTO questionnaireData, short readingCategoryID)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), questionnaireData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(QuestionnaireDTO.PatientTaskID)), questionnaireData.PatientTaskID, DbType.String, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(QuestionnaireQuestionModel.QuestionID)), questionnaireData.Question.QuestionID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(QuestionnaireModel.QuestionnaireAction)), questionnaireData.Questionnaire.QuestionnaireAction.ToString(), DbType.String, ParameterDirection.InputOutput);
        parameters.Add(ConcateAt(nameof(PatientQuestionnaireQuestionAnswersModel.PreviousQuestionID)), questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID ?? 0, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientQuestionnaireQuestionAnswersModel.AnswerValue)), questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, DbType.String, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientQuestionnaireQuestionAnswersModel.PatientAnswerID)), questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID, DbType.Guid, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(QuestionnaireDTO.LanguageID)), questionnaireData.LanguageID, DbType.Int16, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(ReadingModel.ReadingCategoryID)), readingCategoryID, DbType.Int16, ParameterDirection.InputOutput);
        MapCommonSPParameters(questionnaireData, parameters, AppPermissions.QuestionnaireTaskView.ToString());
        parameters.Add(ConcateAt(SPFieldConstants.FIELD_RETURN_PERMISSIONS_FOR), string.Empty, DbType.String, ParameterDirection.Input);
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_NEXT_OR_PREVIOUS_QUESTION, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            questionnaireData.Questionnaire = await result.ReadFirstAsync<QuestionnaireModel>().ConfigureAwait(false);
            if (questionnaireData.Questionnaire.QuestionnaireID > 0)
            {
                questionnaireData.Question = await result.ReadFirstAsync<QuestionnaireQuestionModel>().ConfigureAwait(false);
                questionnaireData.QuestionOptions = (await result.ReadAsync<QuestionnaireQuestionOptionModel>().ConfigureAwait(false))?.ToList();
                questionnaireData.QuestionnaireQuestionAnswers = (await result.ReadAsync<PatientQuestionnaireQuestionAnswersModel>().ConfigureAwait(false))?.ToList();
                var fileDocs = (await result.ReadAsync<FileDocumentModel>().ConfigureAwait(false))?.ToList();
                if (GenericMethods.IsListNotEmpty(fileDocs))
                {
                    questionnaireData.FileDocument = fileDocs.First();
                }
            }
            else
            {
                questionnaireData.Question = null;
                questionnaireData.QuestionOptions = null;
                questionnaireData.QuestionnaireQuestionAnswers = null;
                questionnaireData.FileDocument = null;
            }
            if (questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.Finish)
            {
                questionnaireData.QuestionnaireScore = await result.ReadFirstAsync<PatientQuestionnaireScoresModel>().ConfigureAwait(false);
                if (questionnaireData.QuestionnaireScore.SubScaleRangeID > 0)
                {
                    questionnaireData.QuestionnaireScore.PatientScoreID = GenericMethods.GenerateGuid();
                }
                else
                {
                    questionnaireData.QuestionnaireScore = null;
                }
                questionnaireData.QuestionnaireSubscaleRangeDetails = (await result.ReadAsync<QuestionnaireSubscaleRangeDetailsModel>().ConfigureAwait(false))?.ToList();
            }
            await MapReturnPermissionsAsync(questionnaireData, result).ConfigureAwait(false);
        }
        questionnaireData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
        if (questionnaireData.ErrCode == ErrorCode.OK)
        {
            questionnaireData.ReadingCategoryID = parameters.Get<short>(ConcateAt(nameof(ReadingModel.ReadingCategoryID)));
        }
    }
}
