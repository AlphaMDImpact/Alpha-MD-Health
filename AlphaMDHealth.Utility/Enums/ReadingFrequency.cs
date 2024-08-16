namespace AlphaMDHealth.Utility;

/// <summary>
/// Frequency of reading
/// </summary>
public enum ReadingFrequency
{
    /// <summary>
    /// All data is to be displayed
    /// </summary>
    AllKey = 1,

    /// <summary>
    /// Daily sum is to be displayed
    /// </summary>
    DailySumKey = 2,

    /// <summary>
    /// Hourly data is to be displayed
    /// </summary>
    HourlySumKey = 3,

    /// <summary>
    /// Daily average is to be displayed
    /// </summary>
    DailyAvgKey = 4,
}