using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(ProfilePage))]
public class ProfilePage : BasePage
{
    private readonly ProfileView _profileView;

    public ProfilePage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        BackgroundColor = Color.FromArgb(StyleConstants.DEFAULT_BACKGROUND_COLOR);
        if (MobileConstants.IsDevicePhone)
        {
            HideFooter(true);
        }
        _profileView = new ProfileView(this, null);
    }

    protected override async void OnAppearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false) && ShellMasterPage.CurrentShell.CurrentPage is ProfilePage)
        {
            base.OnAppearing();
            await _profileView.LoadUIAsync(false).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
    }

    protected override async void OnDisappearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            await _profileView.UnloadUIAsync().ConfigureAwait(true);
            if (MobileConstants.IsDevicePhone)
            {
                HideFooter(false);
            }
            base.OnDisappearing();
        }
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
            await _profileView.OnSaveButtonClickedAsync().ConfigureAwait(true);
        }
    }
}