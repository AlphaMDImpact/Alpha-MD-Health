using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// File and attachment data transfer class
    /// </summary>
    public class FileDTO : BaseDTO
    {
        /// <summary>
        /// File information
        /// </summary>
        public FileModel File { get; set; }

        /// <summary>
        /// List of file data
        /// </summary>
        [DataMember]
        public List<FileModel> Files { get; set; }

        /// <summary>
        /// List of file attachment
        /// </summary>
        [DataMember]
        public List<FileDocumentModel> FileDocuments { get; set; }

        /// <summary>
        /// Bulk file upload information
        /// </summary>
        [DataMember]
        public List<SaveResultModel> SaveFiles { get; set; }

        /// <summary>
        /// Bulk file attachment upload information
        /// </summary>
        [DataMember]
        public List<SaveResultModel> SaveFileDocuments { get; set; }

        /// <summary>
        /// New files
        /// </summary>
        [DataMember]
        public List<FileModel> NewFiles { get; set; }

        /// <summary>
        /// File category options to disply in dropdown
        /// </summary>
        [DataMember]
        public List<OptionModel> CategoryOptions { get; set; }
    }
}
