using AlphaMDHealth.Utility;
namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Education category Detail Model
    /// </summary>
    public class EducationCategoryDetailModel
    {
        /// <summary>
        /// PageID of Education/Static Page
        /// </summary>
        public long PageID { get; set; }
        /// <summary>
        /// LanguageName
        /// </summary>
        public string LanguageName { get; set; }
        /// <summary>
        /// Langugage ID
        /// </summary>
        public byte LanguageID { get; set; }
        /// <summary>
        /// Page Heading of Education/Static Page
        /// </summary>
        /// 
        [MyCustomAttributes(ResourceConstants.R_EDUCATION_CATEGORY_NAME_KEY)]
        public string PageHeading { get; set; }
        /// <summary>
        /// PageData of Education/Static Page
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_EDUCATION_CATEGORY_DESCRIPTION_KEY)]
        public string PageData { get; set; }
        /// <summary>
        /// Flag to store IsActive
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Stores Status ID of Education/Static Page
        /// </summary>
        public short StatusID { get; set; }

        /// <summary>
        /// Synced Image
        /// </summary>
        public bool IsDataDownloaded { get; set; }
    }
}
