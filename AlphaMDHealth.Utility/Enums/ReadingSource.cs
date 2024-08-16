namespace AlphaMDHealth.Utility;

/// <summary>
/// Reading Source
/// </summary>
public enum ReadingSource
{
    /// <summary>
    /// Patient Manual (Cannot change as used in Enage)
    /// </summary>
    Manual,

    /// <summary>
    /// Device
    /// </summary>
    Device,

    /// <summary>
    /// GoogleFit
    /// </summary>
    GoogleFit,

    /// <summary>
    /// HealthKit
    /// </summary>
    HealthKit,

    /// <summary>
    /// Provider
    /// </summary>
    ProviderManual,

    /// <summary>
    /// Targeting
    /// </summary>
    Targeting,

    /// <summary>
    /// CHM
    /// </summary>
    CHM,

    /// <summary>
    /// ConnectedDevice are HealthDot and Qualcomm managed devices
    /// </summary>
    ConnectedDevice
}