namespace AlphaMDHealth.Model
{
    public class NotificationDTO : BaseDTO
    {
        public NotificationMessageModel NotificationMessage { get; set; }

        public NotificationModel NotificationData { get; set; }
        public string ForApplication { get; set; }
    }
}
