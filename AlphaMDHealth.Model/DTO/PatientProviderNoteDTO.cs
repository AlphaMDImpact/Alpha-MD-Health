using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class PatientProviderNoteDTO : BaseDTO
    {
        /// <summary>
        /// Patient provider note 
        /// </summary>
        public PatientProviderNoteModel PatientProviderNote { get; set; }

        /// <summary>
        /// List of Patient provider notes
        /// </summary>
        [DataMember]
        public List<PatientProviderNoteModel> PatientProviderNotes { get; set; }
        
        /// <summary>
        /// List of questionnaire questions
        /// </summary>
        [DataMember]
        public List<QuestionnaireQuestionModel> QuestionnaireQuestions { get; set; }

        /// <summary>
        /// List of questionnaire question details
        /// </summary>
        [DataMember]
        public List<QuestionnaireQuestionDetailsModel> QuestionnaireQuestiosDetails { get; set; }

        /// <summary>
        /// List of Questionnaire question answers
        /// </summary>
        [DataMember]
        public List<PatientQuestionnaireQuestionAnswersModel> QuestionnaireQuestionAnswers { get; set; }

        /// <summary>
        /// List of Question Conditions
        /// </summary>
        [DataMember]
        public List<QuestionConditionModel> QuestionConditions { get; set; }

        /// <summary>
        /// List of Questionnaire question answers
        /// </summary>
        [DataMember]
        public List<QuestionnaireQuestionOptionModel> QuestionnaireQuestionOptions { get; set; }

        /// <summary>
        /// List of patient programs
        /// </summary>
        [DataMember]
        public List<OptionModel> PatientPrograms { get; set; }

        /// <summary>
        /// List of providers (caregivers)
        /// </summary>
        [DataMember]
        public List<OptionModel> Providers { get; set; }

        /// <summary>
        /// List of program notes
        /// </summary>
        [DataMember]
        public List<OptionModel> ProgramNotes { get; set; }

        /// <summary>
        /// List of questionnaire questions
        /// </summary>
        [DataMember]
        public List<QuestionnaireQuestionModel> ProviderQuestions { get; set; }

        /// <summary>
        /// List of File Model
        /// </summary>
        [DataMember]
        public List<FileModel> Files { get; set; }

        /// <summary>
        /// List of File Documentss
        /// </summary>
        [DataMember]
        public List<FileDocumentModel> FileDocuments { get; set; }

        /// <summary>
        /// Answer list result
        /// </summary>
        [DataMember]
        public List<SaveResultModel> QuestionAnswerResult { get; set; }

        /// <summary>
        /// Document result
        /// </summary>
        [DataMember]
        public List<SaveResultModel> DocumentResult { get; set; }
    }
}
