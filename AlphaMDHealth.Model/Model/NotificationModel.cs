using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Notification Model
    /// </summary>
    public class NotificationModel
    {
        /// <summary>
        /// Hub ID
        /// </summary>
        public int HubID { get; set; }
        /// <summary>
        /// Device Unique Id
        /// </summary>
        public string DeviceUniqueId { get; set; }
        /// <summary>
        /// User Name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Device Handle
        /// </summary>
        public string DeviceHandle { get; set; }
        /// <summary>
        /// Device Details
        /// </summary>
        public string DeviceDetails { get; set; }
        /// <summary>
        /// Device Model
        /// </summary>
        public string DeviceModel { get; set; }
        /// <summary>
        /// Device OS
        /// </summary>
        public string DeviceOS { get; set; }
        /// <summary>
        /// Tags
        /// </summary>
        public string Tags { get; set; }
        /// <summary>
        /// Push Notification Server Type
        /// </summary>
        public PushNotificationServerType PNS { get; set; }
    }
}
