using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(DashboardPage))]
public class DashboardPage : BasePage
{
    private readonly DashboardView _dashboardView;

    public DashboardPage() : base(PageLayoutType.MastersContentPageLayout, true)
    {
        _dashboardView = new DashboardView(this, null);
    }

    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        await _dashboardView.LoadUIAsync(false).ConfigureAwait(true);
    }

    protected override async void OnDisappearing()
    {
        await _dashboardView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }

    /// <summary>
    /// Refresh current page view after completion of sync
    /// </summary>
    /// <param name="syncFrom">From which page/view sync is called</param>
    protected override async Task RefreshUIAsync(Pages syncFrom)
    {
        await _dashboardView.LoadUIAsync(true).ConfigureAwait(true);
    }
}