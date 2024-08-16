using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class QuestionnaireQuestionDetailsModel
    {
        public long QuestionID { get; set; }
        [MyCustomAttributes(ResourceConstants.R_QUESTION_KEY)]
        public string CaptionText { get; set; }
        public string AnswerPlaceHolder { get; set; }
        public string InstructionsText { get; set; }
        public byte LanguageID { get; set; }
        [Ignore]
        public string LanguageName { get; set; }
        public bool IsActive { get; set; }
    }
}
