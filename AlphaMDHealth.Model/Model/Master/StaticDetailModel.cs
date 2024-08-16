namespace AlphaMDHealth.Model
{
    public class StaticDetailModel : LanguageModel
    {
        public long StaticPageID { get; set; }
        public string Title { get; set; }
        public string Data { get; set; }
        public long AccountID { get; set; }
    }
}