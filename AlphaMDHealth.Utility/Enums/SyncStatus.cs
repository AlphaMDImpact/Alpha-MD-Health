namespace AlphaMDHealth.Utility;

/// <summary>
/// Enum which stores Sync status
/// </summary>
public enum SyncStatus
{
    /// <summary>
    /// Default status of all syncs 
    /// </summary>
    Pending,

    /// <summary>
    /// Status which represents sync in progress status
    /// </summary>
    InProgress,

    /// <summary>
    /// Status which represent sync operarion is done
    /// </summary>
    Done,

    /// <summary>
    /// Status which represent sync operation is failed
    /// </summary>
    Failed
}