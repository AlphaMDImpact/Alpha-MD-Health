using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model to store language specific data of file categories
    /// </summary>
    public class FileCategoryDetailModel
    {
        /// <summary>
        /// ID of File category
        /// </summary>
        [PrimaryKey]
        public long FileCategoryID { get; set; }

        /// <summary>
        /// Name of the language
        /// </summary>
        public string LanguageName { get; set; }

        /// <summary>
        /// Users selected language id
        /// </summary>
        public byte LanguageID { get; set; }

        /// <summary>
        /// Title of the file category
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_FILE_CATEGORY_NAME_KEY)]
        public string Name { get; set; }

        /// <summary>
        /// Detail of File Category
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_FILE_CATEGORY_DESCRIPTION_KEY)]
        public string Description { get; set; }

        /// <summary>
        /// Flag representing data is active or deleted
        /// </summary>
        public bool IsActive { get; set; }
    }
}