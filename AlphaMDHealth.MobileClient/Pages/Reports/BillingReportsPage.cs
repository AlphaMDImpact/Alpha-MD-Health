using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(BillingReportsPage))]
public class BillingReportsPage : BasePage
{
    private readonly BillingReportsView _billingReportsView;

    public BillingReportsPage() : base(PageLayoutType.MastersContentPageLayout, false)
    {
        if (MobileConstants.CheckInternet)
        {
            _billingReportsView = new BillingReportsView(this, null);
            AddRowColumnDefinition(GridLength.Star, 1, true);
            PageLayout.Padding = new Thickness(0);
            PageLayout.Add(_billingReportsView, 0, 0);
        }
        else
        {
            DisplayOperationStatus(ErrorCode.NoInternetConnection.ToString());
        }
    }

    protected override async void OnAppearing()
    {
        if (MobileConstants.CheckInternet)
        {
            await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
            base.OnAppearing();
            await _billingReportsView.LoadUIAsync(false).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
        else
        {
            AppHelper.ShowBusyIndicator = false;
            DisplayOperationStatus(ErrorCode.NoInternetConnection.ToString());
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _billingReportsView.UnloadUIAsync().ConfigureAwait(false);
    }
}