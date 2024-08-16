namespace AlphaMDHealth.Utility;

/// <summary>
/// decides when sync will happen
/// </summary>
public enum SyncTimes
{
    /// <summary>
    /// Do nt call sync for this
    /// </summary>
    DoNotCall = 0,

    /// <summary>
    /// Sync data only first time
    /// </summary>
    FirstTimeOnly = 1,

    /// <summary>
    /// Sync Data all times 
    /// </summary>
    FirstAndRestTime = 2,

    /// <summary>
    /// Sync Data only in rest time 
    /// </summary>
    RestTimeOnly = 3,
}