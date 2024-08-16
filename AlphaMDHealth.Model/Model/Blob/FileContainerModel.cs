using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// File Container details
    /// </summary>
    public class FileContainerModel
    {
        /// <summary>
        /// ID to create folder
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// list of files
        /// </summary>
        [DataMember]
        public List<FileDataModel> FileData { get; set; }
    }
}