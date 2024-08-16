namespace AlphaMDHealth.Utility;

/// <summary>
/// Sync group name
/// </summary>
public enum ServiceSyncGroups
{
    /// <summary>
    /// Default value
    /// </summary>
    Default = 0,

    /// <summary>
    /// group used to sync data from server
    /// </summary>
    RSSyncFromServerGroup = 1,

    /// <summary>
    /// Group used to sync data to server
    /// </summary>
    RSSyncToServerGroup = 2,

    /// <summary>
    /// Group used to sync data from device
    /// </summary>
    RSSyncFromDeviceGroup = 3
}