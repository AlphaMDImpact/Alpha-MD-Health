namespace AlphaMDHealth.Utility;

/// <summary>
/// Sync types to decide it will awaitable in which flow
/// </summary>
public enum SyncTypes
{
    /// <summary>
    /// sync in background all the times
    /// </summary>
    AllAsync = 1,

    /// <summary>
    /// First time sync in background rest time await 
    /// </summary>
    FirstAsyncRestSync = 2,

    /// <summary>
    ///  First time wait rest time sync in background
    /// </summary>
    FirstSyncRestAsync = 3,

    /// <summary>
    /// await all the times
    /// </summary>
    AllSync = 4,
}