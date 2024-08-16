using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class QuestionnaireQuestionModel
    {
        [MyCustomAttributes(ResourceConstants.R_STARTING_QUESTION_SELECTION_KEY)]
        [PrimaryKey]
        public long QuestionID { get; set; }
        public long QuestionnaireID { get; set; }

        [MyCustomAttributes(ResourceConstants.R_QUESTION_TYPE_KEY)]
        public QuestionType QuestionTypeID { get; set; }
        public float SliderSteps { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public bool IsRequired { get; set; }
        public bool AddToMedicalHistory { get; set; }

        /// <summary>
        /// Flag Determins to show value to patient or not 
        /// </summary>
        public bool ShowValueToPatient { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsActive { get; set; }
        public bool IsStartingQuestion { get; set; }
        public Int16? CategoryID { get; set; }
        public int Flows { get; set; }
        public string ProviderNoteID { get; set; }
        [Ignore]
        public string IsStartingQuestionValue { get; set; }
        [Ignore]
        public string CaptionText { get; set; }
        [Ignore]
        public string InstructionsText { get; set; }
        [Ignore]
        public byte LanguageID { get; set; }
        [Ignore]
        public string AnswerPlaceHolder { get; set; }
    }
}
