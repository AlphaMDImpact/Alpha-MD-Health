using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Appointment Model
    /// </summary>
    public class AppointmentModel : ContentDetailModel
    {
        /// <summary>
        /// Stores User's AppointmentID
        /// </summary>
        [PrimaryKey]
        public long AppointmentID { get; set; }
        /// <summary>
        /// Stores User's AccountID
        /// </summary>
        public long AccountID { get; set; }
        /// <summary>
        /// Stores User's AppointmentAddress
        /// </summary>
        public string AppointmentAddress { get; set; }
        /// <summary>
        /// Stores User's AppointmentTypeID
        /// </summary>
        public short AppointmentTypeID { get; set; }
        /// <summary>
        /// Store User's AppointmentTypeName
        /// </summary>
        public string AppointmentTypeName { get; set; }
        /// <summary>
        /// Store User's AppointmentStatusID
        /// </summary>
        public string AppointmentStatusID { get; set; }
        /// <summary>
        /// Store User's AppointmentStatusName
        /// </summary>
        public string AppointmentStatusName { get; set; }
        /// <summary>
        /// Stores AppointmentStatusColor
        /// </summary>
        [Ignore]
        public string AppointmentStatusColor { get; set; }
        /// <summary>
        /// Stores Appointment FromDateTime
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_STARTS_TEXT_KEY)]
        public DateTimeOffset? FromDateTime { get; set; }

        /// <summary>
        /// Stores Appointment ToDateTime
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_ENDS_TEXT_KEY)]
        public DateTimeOffset? ToDateTime { get; set; }
        /// <summary>
        /// Stores Appointment FromDateString
        /// </summary>
        [Ignore]
        public string FromDateString { get; set; }
        /// <summary>
        /// Stores Appointment ToDateString
        /// </summary>
        [Ignore]
        public string ToDateString { get; set; }
        /// <summary>
        /// Stores AppointmentTypeImage
        /// </summary>
        [Ignore]
        public string AppointmentTypeImage { get; set; }
        /// <summary>
        /// Flag to Store IsRepeatRequest
        /// </summary>
        public bool IsRepeatRequest { get; set; }
        /// <summary>
        ///  Flag to Store IsAppointmentViewRequest
        /// </summary>
        public bool IsAppointmentViewRequest { get; set; }
        /// <summary>
        ///  Flag to Store IsInitiator
        /// </summary>
        public bool IsInitiator { get; set; }
        /// <summary>
        /// Stores Appointment ServiceType
        /// </summary>
        public ServiceType ServiceType { get; set; }
        /// <summary>
        /// Stores Appointment VideoToken
        /// </summary>
        public string VideoToken { get; set; }
        /// <summary>
        /// Stores Appointment RoomID
        /// </summary>
        public string RoomID { get; set; }
        /// <summary>
        /// Stores Appointment ClientID
        /// </summary>
        public string ClientID { get; set; }
        /// <summary>
        /// Stores Appointment ClientSecret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Stores Appointment ClientSecret
        /// </summary>
        //todo: to delete
        public byte[] ImageBytes { get; set; }
    }
}