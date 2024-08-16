namespace AlphaMDHealth.Model
{
    public class QuestionnaireQuestionOptionDetailsModel
    {
        public long QuestionOptionID { get; set; }
        public string CaptionText { get; set; }
        public byte LanguageID { get; set; }
        public bool IsActive { get; set; }
    }
}
