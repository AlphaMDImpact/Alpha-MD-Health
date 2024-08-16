using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// File category model
    /// </summary>
    public class FileCategoryModel
    {
        /// <summary>
        /// ID of file category
        /// </summary>
        [PrimaryKey]
        public long FileCategoryID { get; set; }

        /// <summary>
        /// Name of the category image
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_FILE_CATEGORY_IMAGE_KEY)]        
        
        public string ImageName { get; set; } 

        /// <summary>
        /// Name of the category to display 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Flag representing file is active or deleted
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Name of deleted image
        /// </summary>
        public string DeletedImageName { get; set; }

        /// <summary>
        /// Image downloaded status
        /// </summary>
        public bool IsDataDownloaded { get; set; }
    }
}