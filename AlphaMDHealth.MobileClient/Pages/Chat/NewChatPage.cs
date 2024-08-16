using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(NewChatPage))]
public class NewChatPage : BasePage
{
    private readonly ChatsView _providerListView;

    public NewChatPage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _providerListView = new ChatsView(this, null) { Margin = 0 };
        Content = _providerListView;
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _providerListView.Parameters = AddParameters(
            CreateParameter(nameof(BaseDTO.RecordCount), 0.ToString(CultureInfo.InvariantCulture)),
            CreateParameter(SPFieldConstants.FIELD_IS_PROVIDER_LIST, true.ToString(CultureInfo.InvariantCulture)),
            CreateParameter(nameof(BaseDTO.SelectedUserID), 0.ToString(CultureInfo.InvariantCulture)));
        await _providerListView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        await _providerListView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }
}