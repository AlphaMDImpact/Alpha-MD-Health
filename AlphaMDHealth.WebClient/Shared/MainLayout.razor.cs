using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class MainLayout : LayoutComponentBase
{
    private NavigationComponent navigator = new NavigationComponent();
    private bool _isChanged = true;
    private bool _isAfterLoginLayout;
    private bool _isDataFetched;
    private bool _menuChange;
    bool sidebarExpanded = true;
    bool rendered;
    bool multiple = true;
    private bool IsDashboardPatientMobileView = false;

    protected override async Task OnParametersSetAsync()
    {
        if (_isChanged != AppState.IsChanged)
        {
            _isChanged = AppState.IsChanged;
            StateHasChanged();
        }
        await base.OnParametersSetAsync();
    }


    protected override async void OnInitialized()
    {
        _isAfterLoginLayout = LocalStorage.IsUserAuthenticated && !AppState.IsBeforeLoginLayout;
        _isDataFetched = true;
        IsDashboardPatientMobileView = AppState.GetTempToken();
        StateHasChanged();
        base.OnInitialized();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            rendered = true;
        }
    }
    private async Task OnFeatureMenuClickAsync(MenuModel menu)
    {
        if (menu.TargetID > 0)
        {
            AppState.MasterData.Menus.ForEach(x =>
            {
                x.IsActive = x.TargetID == menu.TargetID;
            });
            _menuChange = !_menuChange;
            if (menu.PageTypeID != MenuType.Group)
            {
                if (menu.PageTypeID == MenuType.Feature)
                {
                    await navigator.NavigateToRouteAsync(AppState.RouterData.Routes.First(x => x.FeatureId == menu.TargetID).Path, false).ConfigureAwait(true);
                }
                else
                {
                    if (menu.ScrollToPage && AppState.RouterData.SelectedRoute.Page == AppPermissions.LandingView.ToString())
                    {
                        await JSRuntime.InvokeVoidAsync("scrollOnStartofElementId", string.Concat("LandingView-", menu.MenuID));
                    }
                    else
                    {
                        await navigator.NavigateToRouteAsync(AppState.RouterData.Routes.First(x => x.Page == "ContentPreviewPage").Path, false, menu.TargetID.ToString(CultureInfo.InvariantCulture), "false").ConfigureAwait(true);
                    }
                }
            }
            StateHasChanged();
        }
        else if (!string.IsNullOrWhiteSpace(menu.TargetPage))
        {
            if (menu.TargetPage == AppPermissions.Logout.ToString())
            {
                await new AuthService(AppState.webEssentials).ClearAccountTokensAndIdAsync().ConfigureAwait(true);
                await AppState.NavigateToAsync(AppPermissions.LoginView.ToString(), true).ConfigureAwait(false);
            }
            if (menu.TargetPage == AppPermissions.LoginView.ToString())
            {
                await new AuthService(AppState.webEssentials).ClearAccountTokensAndIdAsync().ConfigureAwait(true);
                await navigator.NavigateToAsync(menu.TargetPage.ToString()).ConfigureAwait(false);
            }
            else if (menu.TargetPage == AppPermissions.LanguageSelectionView.ToString())
            {
                _menuChange = !_menuChange;
                AppState.webEssentials.SetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, AppState.SelectedLanguageID);
                await AppState.NavigateToAsync(AppState.GetDefaultRoute(), true).ConfigureAwait(true);
            }
            else if (menu.TargetPage == AppPermissions.SMSAuthenticationView.ToString())
            {
                _menuChange = !_menuChange;
                AppState.webEssentials.SetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, AppState.SelectedLanguageID);
                await AppState.NavigateToAsync(string.Concat(AppState.GetDefaultRoute(), menu.TargetPage.ToString()), true).ConfigureAwait(true);
            }
            //else if (menu.TargetPage == AppPermissions.CollapsableMenusView.ToString())
            //{
            //    _showLeftMenu = !_showLeftMenu;
            //    AppState.IsLeftVisible = _showLeftMenu;
            //}
            else
            {
                await navigator.NavigateToAsync(menu.TargetPage.ToString()).ConfigureAwait(false);
            }
        }
    }
}