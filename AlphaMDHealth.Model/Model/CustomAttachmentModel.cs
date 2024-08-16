using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Custom Attachment Model
    /// </summary>
    public class CustomAttachmentModel
    {
        /// <summary>
        /// ID 
        /// </summary>
        public long NumericID { get; set; }
        /// <summary>
        /// ID 
        /// </summary>
        public Guid ID { get; set; }
        /// <summary>
        /// editor value
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// FileName
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// FileType
        /// </summary>
        public AppFileExtensions FileType { get; set; }

        /// <summary>
        /// AttachmentBase64
        /// </summary>
        public string AttachmentBase64 { get; set; }
        /// <summary>
        /// IsSent
        /// </summary>
        public bool IsSent { get; set; }
        /// <summary>
        /// CanEdit
        /// </summary>
        public bool IsDisabledSaveButton { get; set; }
        /// <summary>
        /// IsActive
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// IsRelationNotExpired
        /// </summary>
        public bool IsRelationNotExpired { get; set; }

        /// <summary>
        /// IsDeleteAllowed
        /// </summary>
        public bool IsDeleteAllowed { get; set; }

        /// <summary>
        /// AddedBy
        /// </summary>
        public string AddedBy { get; set; }
        /// <summary>
        /// AddedOnDate
        /// </summary>      
        public string AddedOnDate { get; set; }
        /// <summary>
        /// text Color
        /// </summary>
        public string TextColor { get; set; }
        /// <summary>
        /// AddedOnDate
        /// </summary>      
        public string DateColor { get; set; }
        /// <summary>
        /// DefaultIcon
        /// </summary>      
        public string DefaultIcon { get; set; }

        //todo:
        ///// <summary>
        ///// AttachmentSource
        ///// </summary>      
        //public ImageSource AttachmentSource { get; set; }
        public byte[] ImageBytes { get; set; }

        /// <summary>
        /// resource key of Control
        /// </summary>
        public string ControlResourceKey
        {
            get; set;
        }
        /// <summary>
        /// Unique Key for dynamic controls
        /// </summary>
        public string UniqueKey { get; set; }
    }
}
