using SQLite;

namespace AlphaMDHealth.Model
{
    public class QuestionnaireQuestionOptionModel
    {
        [PrimaryKey]
        public long QuestionOptionID { get; set; }
        public long QuestionID { get; set; }
        public int SequenceNo { get; set; }
        public bool IsActive { get; set; }
        public string CaptionText { get; set; }
        [Ignore]
        public bool IsSelected { get; set; }
        [Ignore]
        public string LanguageName { get; set; }
        [Ignore]
        public byte LanguageID { get; set; }
    }
}
