using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Notification Message Model
    /// </summary>
    public class NotificationMessageModel
    {
        /// <summary>
        /// Hub ID
        /// </summary>
        public int HubID { get; set; }
        /// <summary>
        /// Notification Tags
        /// </summary>
        public string NotificationTags { get; set; }
        /// <summary>
        /// Push Notification Server Type
        /// </summary>
        public PushNotificationServerType PNS { get; set; }
        /// <summary>
        /// Notification Title
        /// </summary>
        public string NotificationTitle { get; set; }
        /// <summary>
        /// Notification Body
        /// </summary>
        public string NotificationBody { get; set; }
        /// <summary>
        /// Notification Category
        /// </summary>
        public string NotificationCategory { get; set; }
        /// <summary>
        /// Notification Category ID
        /// </summary>
        public string NotificationCategoryID { get; set; }
        /// <summary>
        /// Badge Count
        /// </summary>
        public int BadgeCount { get; set; }
    }
}
