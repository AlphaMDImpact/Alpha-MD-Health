using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Default page when no page is loaded
/// </summary>
public class DefaultPage : BasePage
{
    /// <summary>
    /// Constructor default page when no page is loaded
    /// </summary>
    public DefaultPage() : base(PageLayoutType.LoginFlowPageLayout, false)
    {
        ////Shell.SetNavBarIsVisible(this, false);
        HideFooter(true);
    }

    /// <summary>
    /// Called when page appears in view
    /// </summary>
    protected override void OnAppearing()
    {
        //todo:Content.Effects.Add(new CustomSafeAreaInsetEffect());
    }
}