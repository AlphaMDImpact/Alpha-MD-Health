using AlphaMDHealth.Utility;
using SQLite;
using System.Text.Json.Serialization;

namespace AlphaMDHealth.Model
{
    public class FileDocumentModel:BaseListItemModel
    {
        /// <summary>
        /// Id of FileDocument
        /// </summary>
        [PrimaryKey]
        public Guid FileDocumentID { get; set; }

        /// <summary>
        /// Id of File 
        /// </summary>
        public Guid FileID { get; set; }

        /// <summary>
        /// Name of the Document
        /// </summary>
        public string DocumentName { get; set; }

        /// <summary>
        /// Description of the Document
        /// </summary>
        public string DocumentDescription { get; set; }

        /// <summary>
        /// Flag Representing weather data is downloaded or not 
        /// </summary>
        public bool IsDownloaded { get; set; }

        /// <summary>
        /// Flag representing data is synced to server or not
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// Flag representing record is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Represents status of the document
        /// </summary>
        public string DocumentStatus { get; set; }

        /// <summary>
        /// File added on date time
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }

        /// <summary>
        /// File sync operation status 
        /// </summary>
        public ErrorCode ErrCode { get; set; }

        /// <summary>
        /// Id Of account for wich Document is added 
        /// </summary>
        public long AddedForAccountID { get; set; }

        /// <summary>
        /// File added by id
        /// </summary>
        public long AddedByID { get; set; }

        /// <summary>
        /// File Added bY UserName
        /// </summary>
        public string AddedByUserName { get; set; }

        [Ignore]
        public string ShowRemoveButtonText { get; set; }
        [Ignore]
        public bool ShowRemoveButton { get; set; }
        [Ignore]
        public string UserFirstName { get; set; }
        [Ignore]
        public string UserLastName { get; set; }
        [Ignore]
        public bool ShowUnreadBadge { get; set; }
        [Ignore]
        public bool IsUnreadHeader { get; set; }
        [Ignore]
        public string FormattedDate { get; set; }

        [Ignore]
        public string FormattedStyle { get; set; }
        [Ignore]
        public string DocumentImage { get; set; }

        //todo:
        //[Ignore]
        //[JsonIgnore]
        //public ImageSource ImageSource { get; set; }
        public byte[] ImageBytes { get; set; }

        public Guid ClientFileDocumentID { get; set; }

        /// <summary>
        /// File category id for provider notes
        /// </summary>
        public long FileCategoryID { get; set; }

        /// <summary>
        /// Name of the FileDocumentName
        /// </summary>
        public string FileDocumentName { get; set; }

        public string DocumentSourceID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UniqueKey { get; set; }
    }
}
