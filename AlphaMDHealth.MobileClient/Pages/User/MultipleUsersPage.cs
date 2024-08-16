using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(MultipleUsersPage))]
[QueryProperty(nameof(TargetPage), "name")]
[QueryProperty(nameof(TargetPageParams), "identifier")]
public class MultipleUsersPage : BasePage
{
    private readonly MultipleUserView _multipleUsersView;
    private readonly bool _isBeforeLogin;

    private string _targetPage;

    /// <summary>
    /// User Id of the selected conversation
    /// </summary>
    public string TargetPage
    {
        get
        {
            return _targetPage;
        }
        set => _targetPage = Uri.UnescapeDataString(value);
    }

    private string _targetPageParams;

    /// <summary>
    /// User Id of the selected conversation
    /// </summary>
    public string TargetPageParams
    {
        get
        {
            return _targetPageParams;
        }
        set => _targetPageParams = Uri.UnescapeDataString(value);
    }

    public MultipleUsersPage() : this(false) { }

    public MultipleUsersPage(bool isBeforeLogin) : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        _isBeforeLogin = isBeforeLogin;
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.StartAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        _multipleUsersView = new MultipleUserView(this, null);
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        //if (Shell.Current?.Navigation?.NavigationStack?.Count > 1 && (Device.Idiom != TargetIdiom.Tablet 
        //    || !IsMoreOptionRootPage(Shell.Current?.CurrentState?.Location.ToString())))
        //{
        //    await _setHeaderCompletionSource.Task.ConfigureAwait(true);
        //    ShowHideLeftRightHeader(MenuLocation.Left, false);
        //}
        base.OnAppearing();
        _multipleUsersView.Parameters = AddParameters(
            CreateParameter(nameof(BaseDTO.IsActive), _isBeforeLogin.ToString(CultureInfo.InvariantCulture)),
            CreateParameter(nameof(TargetPage), _targetPage),
            CreateParameter(nameof(TargetPageParams), _targetPageParams)
        );
        await _multipleUsersView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        await _multipleUsersView.UnloadUIAsync();
        base.OnDisappearing();
    }

    /// <summary>
    /// Method to override when back or close is clicked
    /// </summary>
    /// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
    /// <param name="menuType">Menu position whether Left, Right</param>
    /// <param name="menuAction">Action type</param>
    /// <returns>true if navigation is handled else returns false if default handling is to be used</returns>
    public override async Task<bool> OnBackCloseClickAsync(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        if (menuAction == MenuAction.MenuActionCloseKey)
        {
            await ShellMasterPage.CurrentShell.RenderPageAsync().ConfigureAwait(false);
            return true;
        }
        return await base.OnBackCloseClickAsync(headerLocation, menuType, menuAction);
    }
}