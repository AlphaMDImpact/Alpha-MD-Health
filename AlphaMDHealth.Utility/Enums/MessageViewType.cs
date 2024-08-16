namespace AlphaMDHealth.Utility;

/// <summary>
/// Types of styles used to dosplay custom message control
/// </summary>
public enum MessageViewType
{
    /// <summary>
    /// Display default message style based on properties supplied to it
    /// </summary>
    Default,

    /// <summary>
    /// Show directly label in View
    /// </summary>
    LabelView,

    /// <summary>
    /// Show message view 
    /// </summary>
    StaticMessageView,

    /// <summary>
    /// Show message label with pinacle view
    /// </summary>
    PinacleView,
}