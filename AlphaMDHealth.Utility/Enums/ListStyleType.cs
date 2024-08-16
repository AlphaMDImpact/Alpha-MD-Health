namespace AlphaMDHealth.Utility;

/// <summary>
/// Types of list representations
/// </summary>
public enum ListStyleType
{
    /// <summary>
    /// Default flat list
    /// </summary>
    Default,

    /// <summary>
    /// Listview with seperator line in between
    /// </summary>
    SeperatorView,

    /// <summary>
    /// Listview having childrens in boxes
    /// </summary>
    BoxView,

    /// <summary>
    /// Listview having childrens in cards
    /// </summary>
    CardView,

    /// <summary>
    /// Listview in Carausel View
    /// </summary>
    CarauselView,

    /// <summary>
    /// Listview having childrens in HorzontalManaer
    /// </summary>
    HorizontalView,
}