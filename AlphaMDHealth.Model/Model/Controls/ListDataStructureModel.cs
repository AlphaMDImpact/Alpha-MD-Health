namespace AlphaMDHealth.Model
{
    public class ListDataStructureModel
    {
        public string Header { get; set; }
        public string Subheader { get; set; }
        public string ImageSource { get; set; }
        public bool IsSearchable { get; set; }
        public string Formatter { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsBadge { get; set; }
        public string BadgeCss { get; set; }
        public bool IsHeaderStatic { get; set; }
        public bool IsSubheaderStatic { get; set; }
    }
}
