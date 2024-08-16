using SQLite;
using AlphaMDHealth.Utility;
namespace AlphaMDHealth.Model
{
    public class EductaionCatergoryModel
    {
        [PrimaryKey]
        public long EducationCategoryID { get; set; }

        [MyCustomAttributes(ResourceConstants.R_EDUCATION_CATEGORY_IMAGE_KEY)]
        public string ImageName { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        [Ignore]
        public string DeletedImageName { get; set; }
        public bool IsDataDownloaded { get; set; }
    }
}
