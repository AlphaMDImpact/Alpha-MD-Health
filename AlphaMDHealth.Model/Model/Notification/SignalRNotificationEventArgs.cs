namespace AlphaMDHealth.Model
{
    public class SignalRNotificationEventArgs : EventArgs
    {
        public string NotificationID { get; set; }
        public string NotificationFromID { get; set; }
        public string NotificationMessageType { get; set; }
        public bool IsSilent { get; set; }
    }
}
