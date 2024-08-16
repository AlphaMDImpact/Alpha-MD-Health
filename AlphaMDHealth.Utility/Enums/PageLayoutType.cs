namespace AlphaMDHealth.Utility;

/// <summary>
/// Different types of main layouts
/// </summary>
public enum PageLayoutType
{
    /// <summary>
    /// This will cover end to end device height and width
    /// </summary>
    EndToEndPageLayout,

    /// <summary>
    /// layout with end to end width
    /// </summary>
    LoginFlowPageLayout,

    /// <summary>
    /// This will cover end to end for phone and will leave 10% margin from left and right for i-pad and iPhone
    /// </summary>
    MastersContentPageLayout,

    /// <summary>
    /// layout with GradientVideoCallingPageLayout
    /// </summary>
    GradientVideoCallingPageLayout
}