namespace AlphaMDHealth.Model
{
    public class QuestionnaireDetailsModel
    {
        public long QuestionnaireID { get; set; }
        public string CaptionText { get; set; }
        public string InstructionsText { get; set; }
        public byte LanguageID { get; set; }
        public bool IsActive { get; set; }
        public bool IsDataDownloaded { get; set; }
    }
}
