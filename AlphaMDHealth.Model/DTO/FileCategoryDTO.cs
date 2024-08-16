using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Data transfer object for file categories
    /// </summary>
    public class FileCategoryDTO : BaseDTO
    {
        /// <summary>
        /// File category information
        /// </summary>
        public FileCategoryModel FileCatergory { get; set; }

        /// <summary>
        /// List of file categories
        /// </summary>
        [DataMember]
        public List<FileCategoryModel> FileCategories { get; set; }

        /// <summary>
        /// List of file category translations
        /// </summary>
        [DataMember]
        public List<FileCategoryDetailModel> FileCategoryDetails { get; set; }
    }
}