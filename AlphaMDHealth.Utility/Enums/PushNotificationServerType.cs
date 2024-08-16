namespace AlphaMDHealth.Utility;

/// <summary>
/// Push notification server type
/// </summary>
public enum PushNotificationServerType
{
    /// <summary>
    /// Notification for all
    /// </summary>
    ALL = 0,

    /// <summary>
    /// Notification for iOS device
    /// </summary>
    APPLE = 1,

    /// <summary>
    /// Notification for android device
    /// </summary>
    GOOGLE = 2,

    /// <summary>
    /// Notification for windows device
    /// </summary>
    WINDOWS = 3
}