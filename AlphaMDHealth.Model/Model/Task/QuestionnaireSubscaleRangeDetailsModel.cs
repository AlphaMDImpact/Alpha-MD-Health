namespace AlphaMDHealth.Model
{
    public class QuestionnaireSubscaleRangeDetailsModel
    {
        public long SubScaleRangeID { get; set; }
        public string CaptionText { get; set; }
        public string InstructionsText { get; set; }
        public byte LanguageID { get; set; }
        public bool IsActive { get; set; }
        public bool IsDataDownloaded { get; set; }
    }
}
