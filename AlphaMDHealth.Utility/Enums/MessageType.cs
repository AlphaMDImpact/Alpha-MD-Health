namespace AlphaMDHealth.Utility;

/// <summary>
/// Types of styles used to dosplay custom message control
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Display default message style based on properties supplied to it
    /// </summary>
    Default,

    /// <summary>
    /// Message to display in popup with Fix Header
    /// </summary>
    Popup,

    /// <summary>
    /// Confirmation message style with fix header and footer
    /// </summary>
    ConfirmationPopup,

    /// <summary>
    /// Page details with with fix header and footer and scrollable content
    /// </summary>
    PageDetails,
}