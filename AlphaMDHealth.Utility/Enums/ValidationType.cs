namespace AlphaMDHealth.Utility;

/// <summary>
/// Tyep of Validation for Controls
/// </summary>
public enum ValidationType
{
    /// <summary>
    /// ValidationType is "None"
    /// </summary>
    None,

    /// <summary>
    /// ValidationType is "IsRequired"
    /// </summary>
    IsRequired,

    /// <summary>
    /// ValidationType is "ValidationRegxString"
    /// </summary>
    ValidationRegxString,

    /// <summary>
    /// ValidationType is "ValidationRange"
    /// </summary>
    ValidationRange,

    /// <summary>
    /// ValidationType is "Numeric"
    /// </summary>
    Numeric,

    /// <summary>
    /// ValidationType is "PhoneNumber"
    /// </summary>
    PhoneNumber,

    /// <summary>
    /// ValidationType is "MinimumLength"
    /// </summary>
    MinimumLength,

    /// <summary>
    /// ValidationType is "Decimal"
    /// </summary>
    Decimal
}