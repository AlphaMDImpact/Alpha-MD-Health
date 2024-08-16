namespace AlphaMDHealth.Utility;

/// <summary>
/// Type of Search Based on Different Events
/// </summary>
public enum SearchedOnType
{
    /// <summary>
    /// Event for Search is "OnTextChange"
    /// </summary>
    OnTextChange,

    /// <summary>
    /// Event for Search is "OnSearchClick"
    /// </summary>
    OnSearchClick,

    /// <summary>
    /// Event for Search is "OnSearchClick" and "OnTextChange"
    /// </summary>
    Both
}