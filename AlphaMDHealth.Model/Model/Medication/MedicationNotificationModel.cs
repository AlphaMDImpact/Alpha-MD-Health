using SQLite;

namespace AlphaMDHealth.Model
{
    public class LocalNotificationModel
    {
        [PrimaryKey]
        [AutoIncrement]
        public int? NotificationID { get; set; }
        public long UserID { get; set; }
        public Guid RecordGuidID { get; set; }
        public string RecordID { get; set; }
        public DateTimeOffset ShowNotificationDateTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsSynced { get; set; }
    }
}