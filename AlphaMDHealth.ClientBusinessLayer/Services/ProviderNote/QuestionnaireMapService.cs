using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class QuestionnaireService : BaseService
{
    public QuestionnaireService(IEssentials essentials) : base(essentials)
    {
        
    }
    private List<PatientQuestionnaireQuestionAnswersModel> MapPatientQuestionnaireQuestionAnswers(JToken data, string nameOfToken)
    {
        return (data[nameOfToken].Any())
            ? (from dataItem in data[nameOfToken]
               select MapQuestionAnswer(dataItem)).ToList()
            : new List<PatientQuestionnaireQuestionAnswersModel>();
    }

    private PatientQuestionnaireQuestionAnswersModel MapQuestionAnswer(JToken dataItem)
    {
        return new PatientQuestionnaireQuestionAnswersModel
        {
            PatientAnswerID = GetDataItem<Guid>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.PatientAnswerID)),
            TaskType = GetDataItem<byte>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.TaskType)),
            QuestionID = GetDataItem<long>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.QuestionID)),
            PatientTaskID = GetDataItem<string>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.PatientTaskID)),
            AnswerValue = GetDataItem<string>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.AnswerValue)),
            PreviousQuestionID = GetDataItem<long>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.PreviousQuestionID)),
            NextQuestionID = GetDataItem<long>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.NextQuestionID)),
            ScoreValue = GetDataItem<decimal>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.ScoreValue)),
            IsActive = GetDataItem<bool>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.IsActive)),
            LastModifiedON = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientQuestionnaireQuestionAnswersModel.LastModifiedON)),
            ErrCode = ErrorCode.OK,
            IsSynced = true
        };
    }

    private List<PatientProviderNoteModel> MapProviderNotes(JToken data, string nameOfToken)
    {
        return (data[nameOfToken].Any())
            ? (from dataItem in data[nameOfToken]
               select MapPatientProviderNote(dataItem)).ToList()
            : new List<PatientProviderNoteModel>();
    }

    private PatientProviderNoteModel MapPatientProviderNote(JToken data)
    {
        return new PatientProviderNoteModel
        {
            ProviderNoteID = GetDataItem<Guid>(data, nameof(PatientProviderNoteModel.ProviderNoteID)),
            CareGiverID = GetDataItem<long>(data, nameof(PatientProviderNoteModel.CareGiverID)),
            ProgramNoteID = GetDataItem<long>(data, nameof(PatientProviderNoteModel.ProgramNoteID)),
            PatientID = GetDataItem<long>(data, nameof(PatientProviderNoteModel.PatientID)),
            NoteDateTime = GetDataItem<DateTimeOffset>(data, nameof(PatientProviderNoteModel.NoteDateTime)),
            ProgramID = GetDataItem<long>(data, nameof(PatientProviderNoteModel.ProgramID)),
            UserName = GetDataItem<string>(data, nameof(PatientProviderNoteModel.UserName)),
            IsActive = GetDataItem<bool>(data, nameof(PatientProviderNoteModel.IsActive)),
            AddedOn = GetDataItem<DateTimeOffset>(data, nameof(PatientProviderNoteModel.AddedOn)),
            ProgramName = GetDataItem<string>(data, nameof(PatientProviderNoteModel.ProgramName)),
            ProgramGroupIdentifier = GetDataItem<string>(data, nameof(PatientProviderNoteModel.ProgramGroupIdentifier)),
            IsSynced = true
        };
    }

    private List<QuestionConditionModel> MapQuestionsConditions(JToken data, string nameOfToken)
    {
        int count = 1;
        return (data[nameOfToken].Any())
            ? (from dataItem in data[nameOfToken]
               select new QuestionConditionModel
               {
                   OptionID = GetDataItem<int>(dataItem, nameof(QuestionConditionModel.OptionID)),
                   QuestionID = GetDataItem<long>(dataItem, nameof(QuestionConditionModel.QuestionID)),
                   Value1 = GetDataItem<double>(dataItem, nameof(QuestionConditionModel.Value1)),
                   Value2 = GetDataItem<double>(dataItem, nameof(QuestionConditionModel.Value2)),
                   TargetQuestionID = GetDataItem<long>(dataItem, nameof(QuestionConditionModel.TargetQuestionID)),
                   UIID = count++,
                   OptionText = GetDataItem<string>(dataItem, nameof(QuestionConditionModel.OptionText)),
               }).ToList()
            : new List<QuestionConditionModel>();
    }

    /// <summary>
    /// Map json data to QuestionnaireQuestion model
    /// </summary>
    /// <param name="data">jtoken data</param>
    /// <param name="nameOfToken">name of token</param>
    /// <returns></returns>
    public List<QuestionnaireQuestionModel> MapQuestionnaireQuestions(JToken data, string nameOfToken)
    {
        return (data[nameOfToken].Any())
            ? (from dataItem in data[nameOfToken]
               select MapQuestionnaireQuestion(dataItem)).ToList()
            : new List<QuestionnaireQuestionModel>();
    }

    private QuestionnaireQuestionModel MapQuestionnaireQuestion(JToken question)
    {
        return new QuestionnaireQuestionModel
        {
            QuestionnaireID = GetDataItem<long>(question, nameof(QuestionnaireQuestionModel.QuestionnaireID)),
            QuestionID = GetDataItem<long>(question, nameof(QuestionnaireQuestionModel.QuestionID)),
            QuestionTypeID = GetDataItem<string>(question, nameof(QuestionnaireQuestionModel.QuestionTypeID)).ToEnum<QuestionType>(),
            CaptionText = GetDataItem<string>(question, nameof(QuestionnaireQuestionModel.CaptionText)),
            InstructionsText = GetDataItem<string>(question, nameof(QuestionnaireQuestionModel.InstructionsText)),
            ProviderNoteID = GetDataItem<string>(question, nameof(QuestionnaireQuestionModel.ProviderNoteID)),
            AnswerPlaceHolder = GetDataItem<string>(question, nameof(QuestionnaireQuestionModel.AnswerPlaceHolder)),
            IsStartingQuestion = GetDataItem<bool>(question, nameof(QuestionnaireQuestionModel.IsStartingQuestion)),
            IsStartingQuestionValue = GetDataItem<string>(question, nameof(QuestionnaireQuestionModel.IsStartingQuestionValue)),
            ShowValueToPatient = GetDataItem<bool>(question, nameof(QuestionnaireQuestionModel.ShowValueToPatient)),
            AddToMedicalHistory = GetDataItem<bool>(question, nameof(QuestionnaireQuestionModel.AddToMedicalHistory)),
            SliderSteps = GetDataItem<float>(question, nameof(QuestionnaireQuestionModel.SliderSteps)),
            MinValue = GetDataItem<float>(question, nameof(QuestionnaireQuestionModel.MinValue)),
            MaxValue = GetDataItem<float>(question, nameof(QuestionnaireQuestionModel.MaxValue)),
            CategoryID = GetDataItem<short>(question, nameof(QuestionnaireQuestionModel.CategoryID)),
            IsRequired = GetDataItem<bool>(question, nameof(QuestionnaireQuestionModel.IsRequired)),
            IsActive = GetDataItem<bool>(question, nameof(QuestionnaireQuestionModel.IsActive)),
            Flows = GetDataItem<int>(question, nameof(QuestionnaireQuestionModel.Flows)),
        };
    }

    /// <summary>
    /// Map json data to questionnair Question option model
    /// </summary>
    /// <param name="data">json data</param>
    /// <param name="nameOfToken">name of token</param>
    /// <returns></returns>
    public List<QuestionnaireQuestionOptionModel> MapQuestionOptions(JToken data, string nameOfToken)
    {
        return data[nameOfToken].Any()
            ? (from dataItem in data[nameOfToken]
               select MapQuestionOption(dataItem)).ToList()
            : new List<QuestionnaireQuestionOptionModel>();
    }

    private QuestionnaireQuestionOptionModel MapQuestionOption(JToken dataItem)
    {
        return new QuestionnaireQuestionOptionModel
        {
            QuestionID = GetDataItem<long>(dataItem, nameof(QuestionnaireQuestionOptionModel.QuestionID)),
            QuestionOptionID = GetDataItem<long>(dataItem, nameof(QuestionnaireQuestionOptionModel.QuestionOptionID)),
            SequenceNo = GetDataItem<int>(dataItem, nameof(QuestionnaireQuestionOptionModel.SequenceNo)),
            CaptionText = GetDataItem<string>(dataItem, nameof(QuestionnaireQuestionOptionModel.CaptionText)),
            IsActive = GetDataItem<bool>(dataItem, nameof(QuestionnaireQuestionOptionModel.IsActive)),
        };
    }
}