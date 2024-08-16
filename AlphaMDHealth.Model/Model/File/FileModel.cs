using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// File Information class
    /// </summary>
    public class FileModel
    {
        /// <summary>
        /// Id of the file
        /// </summary>
        [PrimaryKey]
        public Guid FileID { get; set; }

        /// <summary>
        /// User id for which file is saved
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// Category id of the file selected in dropdown
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_SELECT_File_CATEGORY_KEY)]
        public long FileCategoryID { get; set; }

        /// <summary>
        /// Name of the category
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Flag representing data is synced to server or not
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// Flag representing record is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// File added on date time
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }

        /// <summary>
        /// File sync operation status 
        /// </summary>
        public ErrorCode ErrCode { get; set; }

        /// <summary>
        /// File added by id
        /// </summary>
        public long AddedByID { get; set; }
        
        /// <summary>
        /// Number of attachment in file to display in list
        /// </summary>
        public string NumberOfFiles { get; set; }

        /// <summary>
        /// File Type
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// File Base64
        /// </summary>
        public string FileBase64 { get; set; }

        /// <summary>
        /// Image string 
        /// </summary>
        public string FileImage { get; set; }

        //todo:
        ///// <summary>
        ///// Image source to render category image in mobile
        ///// </summary>
        //[Ignore]
        //public ImageSource ImageSource { get; set; }
        public byte[] ImageBytes { get; set; }

        /// <summary>
        /// File unread status
        /// </summary>
        [Ignore]
        public bool IsUnreadHeader { get; set; }

        /// <summary>
        /// formatted date time to display in UI
        /// </summary>
        [Ignore]
        public string FormattedDate { get; set; }

        /// <summary>
        /// Formatted number of files string to display in UI
        /// </summary>
        public string FormattedNumberOfFiles { get; set; }
        
        /// <summary>
        /// File Icon of file
        /// </summary>
        public string FileIcon { get; set; }
    }
}
