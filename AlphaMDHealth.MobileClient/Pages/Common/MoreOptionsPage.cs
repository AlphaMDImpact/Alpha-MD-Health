using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// More options page
/// </summary>
[RouteRegistration(nameof(MoreOptionsPage))]
public class MoreOptionsPage : BasePage
{
    private readonly MenuCollectionView _menuGroupCollection;

    /// <summary>
    /// More options page
    /// </summary>
    public MoreOptionsPage() : base(PageLayoutType.MastersContentPageLayout, false)
    {
        _menuGroupCollection = new MenuCollectionView(this, null);
    }

    /// <summary>
    /// Called when page appears in UI
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _menuGroupCollection.LoadUIAsync(false);
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Called when page disappears from UI
    /// </summary>
    protected override async void OnDisappearing()
    {
        await _menuGroupCollection.UnloadUIAsync();
        base.OnDisappearing();
    }
}