namespace AlphaMDHealth.Utility;

/// <summary>
/// Reading moment
/// </summary>
public enum ReadingMoment
{
    /// <summary>
    /// Observation
    /// </summary>
    Observation = 0,

    /// <summary>
    /// Glucose fasting
    /// </summary>
    GlucoseFasting = 662,

    /// <summary>
    /// Glucose after break fast
    /// </summary>
    GlucoseAfterBreakfast = 663,

    /// <summary>
    /// Glucose before lunch
    /// </summary>
    GlucoseBeforeLunch = 660,

    /// <summary>
    /// Glucose after lunch
    /// </summary>
    GlucoseAfterLunch = 661,

    /// <summary>
    /// Glucose before dinner
    /// </summary>
    GlucoseBeforeDinner = 664,

    /// <summary>
    /// Glucose after dinner
    /// </summary>
    GlucoseAfterDinner = 665,

    /// <summary>
    /// Glucose before bed
    /// </summary>
    GlucoseBeforeBed = 666,

    /// <summary>
    /// Ketone
    /// </summary>
    Ketone = 8,

    /// <summary>
    /// Control solution
    /// </summary>
    Control = 9,

    /// <summary>
    /// Sleep
    /// </summary>
    Sleep = 10,

    /// <summary>
    /// Sleep light
    /// </summary>
    SleepLight = 11,

    /// <summary>
    /// Sleed deep
    /// </summary>
    SleepDeep = 12
}