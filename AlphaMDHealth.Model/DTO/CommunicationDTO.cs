using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Communication object
    /// </summary>
    public class CommunicationDTO : BaseDTO
    {
        /// <summary>
        /// For application
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Sms sender id
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// From email id
        /// </summary>
        public string FromId { get; set; }

        /// <summary>
        /// To email ids
        /// </summary>
        [DataMember]
        public List<string> ToIds { get; set; }

        /// <summary>
        /// Bcc email ids
        /// </summary>
        [DataMember]
        public List<string> BccIds { get; set; }

        /// <summary>
        /// Cc email ids
        /// </summary>
        [DataMember]
        public List<string> CcIds { get; set; }

        /// <summary>
        /// Email subject
        /// </summary>
        public string MessageSubject { get; set; }

        /// <summary>
        /// Email body
        /// </summary>
        public string MessageBody { get; set; }

        /// <summary>
        /// Email body
        /// </summary>
        public string TeampleteName { get; set; }

        /// <summary>
        /// Attachments to send
        /// </summary>
        [DataMember]
        public List<FileDataModel> MessageAttachments { get; set; }
    }
}
