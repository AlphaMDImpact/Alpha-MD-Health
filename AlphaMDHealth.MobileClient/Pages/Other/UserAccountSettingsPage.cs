using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(UserAccountSettingsPage))]
public class UserAccountSettingsPage : BasePage
{
    private readonly UserAccountSettingsView _userAccountSettingsView;

    public UserAccountSettingsPage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        _userAccountSettingsView = new UserAccountSettingsView(this, null);
        SetPageContent(true);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _userAccountSettingsView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _userAccountSettingsView.UnloadUIAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Method to override to handle header item clicks
    /// </summary>
    /// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
    /// <param name="menuType">Menu position whether Left, Right</param>
    /// <param name="menuAction">Action type</param>
    public async override Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        if (menuAction == MenuAction.MenuActionSaveKey)
        {
            await _userAccountSettingsView.OnSaveButtonClickedAsync().ConfigureAwait(true);
        }
    }
}