namespace AlphaMDHealth.Model
{
    public class EducationCategoryGroupModel : List<ContentPageModel>
    {
        public long EducationCategoryID { get; set; }
        public string Name { get; set; }
        public string CategoryImage { get; set; }
        public string CategoryDetails { get; set; }
        public string SubHeader { get; set; }

        //todo:
        //[Ignore]
        //public ImageSource CategoryImageSource { get; set; }
        public byte[] ImageBytes { get; set; }
    }
}
