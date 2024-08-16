using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Chat Detail Model
    /// </summary>
    public class ChatDetailModel
    {
        /// <summary>
        /// Stores ChatDetailID
        /// </summary>
        [PrimaryKey]
        public Guid ChatDetailID { get; set; }

        /// <summary>
        /// ChatID
        /// </summary>
        public Guid ChatID { get; set; }

        /// <summary>
        /// ChatText
        /// </summary>
        public string ChatText { get; set; }

        /// <summary>
        /// AttachmentBase64
        /// </summary>
        public string AttachmentBase64 { get; set; }

        /// <summary>
        /// CompressedAttachment
        /// </summary>
        public string CompressedAttachment { get; set; }

        /// <summary>
        /// FileName
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// FileType
        /// </summary>
        public AppFileExtensions FileType { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// AddedOn
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }

        /// <summary>
        /// AddedById
        /// </summary>
        public long AddedById { get; set; }

        /// <summary>
        /// AddedBy
        /// </summary>
        public string AddedBy { get; set; }

        /// <summary>
        /// FromID
        /// </summary>
        public long FromID { get; set; }

        /// <summary>
        /// ToID
        /// </summary>
        public long ToID { get; set; }

        /// <summary>
        /// IsRead
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// IsSynced
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// IsDataDownloaded
        /// </summary>
        public bool IsDataDownloaded { get; set; }

        /// <summary>
        /// ErrCode
        /// </summary>
        public ErrorCode ErrCode { get; set; }

        /// <summary>
        /// IsSent
        /// </summary>
        [Ignore]
        public bool IsSent { get; set; }

        /// <summary>
        /// AddedOnDate
        /// </summary>
        [Ignore]
        public string AddedOnDate { get; set; }
    }
}
