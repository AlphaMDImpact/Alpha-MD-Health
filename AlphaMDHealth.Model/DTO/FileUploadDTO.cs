using AlphaMDHealth.Utility;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// File upload DTO
    /// </summary>
    public class FileUploadDTO : BaseDTO
    {
        /// <summary>
        /// File Type upload
        /// </summary>
        public FileTypeToUpload FileTypeUploading { get; set; }

        /// <summary>
        /// List of container paths
        /// </summary>
        [DataMember]
        public List<FileContainerModel> FileContainers { get; set; }

        /// <summary>
        /// File Name
        /// </summary>
        public string FileName { get; set; }
    }
}