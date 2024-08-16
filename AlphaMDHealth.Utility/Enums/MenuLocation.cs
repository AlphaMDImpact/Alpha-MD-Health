namespace AlphaMDHealth.Utility;

/// <summary>
/// Menu location: header, footer, left or right
/// </summary>
public enum MenuLocation
{
    /// <summary>
    /// Menu is for default
    /// </summary>
    Default = 0,

    /// <summary>
    /// Menu is for header
    /// </summary>
    Header = 1,

    /// <summary>
    /// Menu is for footer
    /// </summary>
    Footer = 2,

    /// <summary>
    /// Menu is for left header
    /// </summary>
    Left = 3,

    /// <summary>
    /// Menu is for right header
    /// </summary>
    Right = 4,

    /// <summary>
    /// Menu is for content
    /// </summary>
    Content = 5
}