using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class QuestionnaireDTO : BaseDTO
    {
        public string PatientTaskID { get; set; }
        public QuestionnaireModel Questionnaire { get; set; }
        [DataMember]
        public List<QuestionnaireModel> Questionnaires { get; set; }
        public QuestionnaireDetailsModel QuestionnaireDetail { get; set; }
        [DataMember]
        public List<QuestionnaireDetailsModel> QuestionnaireDetails { get; set; }
        [DataMember]
        public List<CardModel> QuestionnaireCards { get; set; }
        [DataMember]
        public List<ContentDetailModel> PageDetails { get; set; }
        public QuestionnaireSubscaleModel QuestionnaireSubscaleData { get; set; }
        [DataMember]
        public List<QuestionnaireSubscaleModel> QuestionnaireSubscales { get; set; }
        public QuestionnaireSubscaleRangesModel QuestionnaireSubscaleRange { get; set; }
        [DataMember]
        public List<QuestionnaireSubscaleRangesModel> QuestionnaireSubscaleRanges { get; set; }
        [DataMember]
        public List<QuestionnaireSubscaleRangeDetailsModel> QuestionnaireSubscaleRangeDetails { get; set; }
        [DataMember]
        public List<QuestionnaireQuestionModel> Questions { get; set; }
        [DataMember]
        public List<QuestionnaireQuestionDetailsModel> QuestionDetails { get; set; }
        public QuestionnaireQuestionDetailsModel QuestionDetail { get; set; }
        public QuestionnaireQuestionModel Question { get; set; }
        [DataMember]
        public List<QuestionnaireQuestionOptionModel> QuestionOptions { get; set; }
        [DataMember]
        public List<QuestionnaireQuestionOptionDetailsModel> QuestionOptionDetails { get; set; }
        [DataMember]
        public List<QuestionnaireQuestionOptionModel> QuestionnaireQuestionOptionDetails { get; set; }
        [DataMember]
        public List<OptionModel> DropDownOptions { get; set; }
        [DataMember]
        public List<OptionModel> DefaultRespondants { get; set; }
        public PatientQuestionnaireQuestionAnswersModel QuestionnaireQuestionAnswer { get; set; }
        [DataMember]
        public List<PatientQuestionnaireQuestionAnswersModel> QuestionnaireQuestionAnswers { get; set; }
        public PatientQuestionnaireScoresModel QuestionnaireScore { get; set; }
        [DataMember]
        public List<PatientQuestionnaireScoresModel> QuestionnaireScores { get; set; }
        [DataMember]
        public List<SaveResultModel> QuestionnaireQuestionAnswerResult { get; set; }
        [DataMember]
        public List<SaveResultModel> QuestionnaireScoreResult { get; set; }
        [DataMember]
        public QuestionnaireQuestionsLinkModel QuestionnaireQuestionCondition { get; set; }
        [DataMember]
        public List<QuestionnaireQuestionsLinkModel> QuestionnaireLinkedQuestions { get; set; }
        [DataMember]
        public List<QuestionConditionModel> QuestionConditions { get; set; }
        [DataMember]
        public List<QuestionScoreModel> QuestionScores { get; set; }
        [DataMember]
        public List<QuestionScoreModel> QuestionnaireQuestionScores { get; set; }

        [DataMember]
        public List<OptionModel> ReadingsOptions { get; set; }

        ///[DataMember]
        public double? NumberOfQuestionAnswer { get; set; }
        public FileModel File { get; set; }
        public List<FileDocumentModel> FileDocuments { get; set; }
        public FileDocumentModel FileDocument { get; set; }
        public short ReadingCategoryID { get; set; }
    }
}
