using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public partial class BasePage : ContentPage, IDisposable
{
    #region Page Features

    /// <summary>
    /// Gets translations of for the given feature Permissions
    /// </summary>
    /// <param name="permissions">list of codes</param>
    public async Task GetFeaturesAsync(params string[] permissions)
    {
        await (PageService as BaseService).GetFeaturesAsync(permissions).ConfigureAwait(false);
        PageData.FeaturePermissions = PageService.PageData?.FeaturePermissions;
    }

    /// <summary>
    /// Finds a particular resource based on key from page feature
    /// </summary>
    /// <param name="featureCode">feature identifier</param>
    /// <returns>String value of a feature</returns>
    public string GetFeatureValueByCode(string featureCode)
    {
        if (GenericMethods.IsListNotEmpty(PageData?.FeaturePermissions))
        {
            return PageData.FeaturePermissions.FirstOrDefault(x => x.FeatureCode.Trim() == featureCode)?.FeatureText?.Trim() ?? string.Empty;
        }
        return string.Empty;
    }

    #endregion

    #region Common Methods

    /// <summary>
    /// Login service used to validate user data and create token
    /// </summary>
    /// <param name="accountData">Users login details to verify on server</param>
    /// <param name="isEnvironmentCheckRequired">flag which decides need to check environment or not</param>
    /// <returns>operation status of login</returns>
    public async Task LoginAsync(AuthDTO accountData, bool isEnvironmentCheckRequired)
    {
        if (isEnvironmentCheckRequired)
        {
            accountData.Settings = PageData.Settings;
        }
        CancellationTokenSourceInstance = new CancellationTokenSource();
        await new AuthService(App._essentials).LoginAsync(accountData, isEnvironmentCheckRequired
            , InvokeSyncActionAsync, CancellationTokenSourceInstance.Token).ConfigureAwait(true);
        if (await HandelLoginServiceResponseAsync(accountData).ConfigureAwait(false))
        {
            accountData.ErrCode = ErrorCode.HandledRedirection;
        }
    }

    private async Task<bool> HandelLoginServiceResponseAsync(AuthDTO accountData)
    {
        BasePage targetPage = null;
        switch (accountData.ErrCode)
        {
            case ErrorCode.SetPinCode:
                targetPage = new PincodePage(AppPermissions.PinCodeView.ToString(), false);
                break;
            case ErrorCode.SMSAuthentication:
                targetPage = new PincodePage(AppPermissions.SMSAuthenticationView.ToString(), accountData.AuthenticationData.UserName
                    , accountData.AuthenticationData.AccountPassword, accountData.AuthenticationData.PhoneNo);
                break;
            case ErrorCode.SetNewPassword:
                await SetUserDetailsToSetPasswordAsync(accountData.AuthenticationData.EmailID
                    , accountData.AuthenticationData.PhoneNo).ConfigureAwait(true);
                await new AuthService(App._essentials).SaveSecuredValueAsync(StorageConstants.PR_USER_CRED_KEY
                    , accountData.AuthenticationData.AccountPassword).ConfigureAwait(true);
                targetPage = new ResetPasswordPage(Pages.SetNewPasswordPage);
                break;
            case ErrorCode.ResetPassword:
                await SetUserDetailsToSetPasswordAsync(accountData.AuthenticationData.EmailID
                    , accountData.AuthenticationData.PhoneNo).ConfigureAwait(true);
                targetPage = new ResetPasswordPage(Pages.ResetPasswordPage);
                break;
            case ErrorCode.LanguageNotAvailable:
                targetPage = new StaticMessagePage(ErrorCode.LanguageNotAvailable.ToString());
                break;
            case ErrorCode.UnknownCertificate:
            case ErrorCode.RecordCountMismatch:
                targetPage = new StaticMessagePage(accountData.ErrCode.ToString());
                break;
            case ErrorCode.HandledRedirection:
                return true;
            default:
                return false;
        }
        if (targetPage != null)
        {
            await ShellMasterPage.CurrentShell.PushMainPageAsync(targetPage).ConfigureAwait(true);
        }
        return true;
    }

    /// <summary>
    /// Navigates to login page after cleaning user info
    /// </summary>
    /// <returns>Operation status</returns>
    public async Task NavigateToLoginPageAsync()
    {
        await new AuthService(App._essentials).ClearAccountTokensAndIdAsync().ConfigureAwait(true);
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
    }

    /// <summary>
    /// Save details on local storage to use it during reset password
    /// </summary>
    /// <param name="emailID">Users email id</param>
    /// <param name="phoneNumber">Users phone number</param>
    /// <returns>Operation status</returns>
    protected async Task SetUserDetailsToSetPasswordAsync(string emailID, string phoneNumber)
    {
        await new BaseService(App._essentials).SaveSecuredValueAsync(StorageConstants.SS_USER_NAME_KEY, emailID).ConfigureAwait(false);
        await new BaseService(App._essentials).SaveSecuredValueAsync(StorageConstants.PR_PHONE_NUMBER_KEY, phoneNumber).ConfigureAwait(false);
    }

    /// <summary>
    /// Generate Picker Options from Resources
    /// </summary>
    /// <param name="resources">Resource List to be converted</param>
    /// <param name="selectedValue">selected item value id any</param>
    /// <returns>List of Options</returns>
    public List<OptionModel> Options(List<ResourceModel> resources, long selectedValue)
    {
        List<OptionModel> options = new List<OptionModel>();
        if (GenericMethods.IsListNotEmpty(resources))
        {
            foreach (var resource in resources)
            {
                options.Add(new OptionModel
                {
                    OptionID = resource.ResourceKeyID,
                    OptionText = resource.ResourceValue,
                    IsSelected = resource.ResourceKeyID == selectedValue
                });
            }
        }
        return options;
    }

    /// <summary>
    /// Creates link separator
    /// </summary>
    /// <returns>Link separator</returns>
    public BoxView CreateLinkSeperator(bool isVisible)
    {
        return new BoxView
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_LIGHT_BACKGROUND_SEPARATOR_STYLE],
            IsVisible = isVisible,
            Margin = new Thickness((new OnIdiom<double> { Phone = 0.3, Tablet = 0.15 }) *
                App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0), 0),
        };
    }

    /// <summary>
    /// Ceatre horizontal seperator
    /// </summary>
    /// <param name="mainLayout">main page layout</param>
    /// <param name="left">Row number</param>
    /// <param name="top">Column number</param>
    /// <param name="colSpan">Col span</param>
    public void CreateSepratorView(Grid mainLayout, int left, int top, int colSpan)
    {
        var seprator = new BoxView
        {
            HeightRequest = 1,
            BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE]
        };
        mainLayout?.Add(seprator, left, top);
        if (colSpan > 0)
        {
            Grid.SetColumnSpan(seprator, colSpan);
        }
    }

    #endregion

    #region To Delete

    ///// <summary>
    ///// Create an tab structure
    ///// </summary>
    ///// <param name="tabs">List containing data to display</param>
    //[Obsolete("This method will be deleted soon, Use CustomTabsControl in place of this method")]
    //public PancakeView GenerateTabs(List<TabStructureModel> tabs, bool isTabFullWidth)
    //{
    //    _moreOptions = new List<OptionModel>();
    //    _buttonGrid = new Grid
    //    {
    //        Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
    //        VerticalOptions = LayoutOptions.Start,
    //        ColumnSpacing = 2,
    //        BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR],
    //        RowDefinitions =
    //            {
    //                new RowDefinition { Height = GridLength.Auto },
    //            },
    //    };
    //    _buttonGrid.ColumnDefinitions.Clear();
    //    for (int i = 0; i < tabs.Count; i++)
    //    {
    //        if (i < LibConstants.MAX_TABS_ALLOWED)
    //        {
    //            GetTabData(tabs[i], isTabFullWidth, i, false);
    //        }
    //        else
    //        {
    //            _moreOptions.Add(new OptionModel { GroupName = tabs[i].TabId, OptionText = tabs[i].TabName });
    //        }
    //    }
    //    if (tabs.Count > LibConstants.MAX_TABS_ALLOWED)
    //    {
    //        GetTabData(null, isTabFullWidth, LibConstants.MAX_TABS_ALLOWED, true);
    //    }
    //    return new PancakeView
    //    {
    //        Style = (Style)App.Current.Resources[StyleConstants.ST_PANCAKE_TAB_STYLE],
    //        ////Margin = new Thickness(0, 0, 0, 15),
    //        Content = _buttonGrid
    //    };
    //}

    //private void GetTabData(TabStructureModel tab, bool isTabFullWidth, int i, bool isMore)
    //{
    //    _buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = isTabFullWidth && !isMore ? GridLength.Star : GridLength.Auto });
    //    CustomButtonControl tabButton = new CustomButtonControl(ButtonType.TabButton)
    //    {
    //        BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_CONTROL_BACKGROUND_COLOR],
    //        TextColor = (Color)App.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR],
    //        CommandParameter = i == Constants.MAX_TABS_ALLOWED ? Constants.TABS_MORE_OPTION_CONSTANT : tab.TabId,
    //        Text = i == Constants.MAX_TABS_ALLOWED ? "..." : tab.TabName,
    //    };
    //    if (i == 0)
    //    {
    //        tabButton.BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
    //        tabButton.TextColor = (Color)App.Current.Resources[StyleConstants.ST_CONTROL_BACKGROUND_COLOR];
    //    }
    //    tabButton.Clicked += Tab_Clicked;
    //    _buttonGrid.Add(tabButton, i, 0);
    //}

    //private async void Tab_Clicked(object sender, EventArgs e)
    //{
    //    CustomButtonControl tab = (CustomButtonControl)sender;
    //    tab.IsEnabled = false;
    //    foreach (var b in _buttonGrid.Children)
    //    {
    //        if (tab.CommandParameter == ((CustomButtonControl)b).CommandParameter)
    //        {
    //            ((CustomButtonControl)b).BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
    //            ((CustomButtonControl)b).TextColor = (Color)App.Current.Resources[StyleConstants.ST_CONTROL_BACKGROUND_COLOR];
    //        }
    //        else
    //        {
    //            ((CustomButtonControl)b).BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_CONTROL_BACKGROUND_COLOR];
    //            ((CustomButtonControl)b).TextColor = (Color)App.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
    //        }
    //    }
    //    TabClicked.Invoke(tab.CommandParameter, new EventArgs());
    //    if ((string)tab.CommandParameter == Constants.TABS_MORE_OPTION_CONSTANT)
    //    {
    //        //todo:
    //        //TabMoreOptionPopupPage moreOptionPopup = new TabMoreOptionPopupPage(_moreOptions);
    //        //moreOptionPopup.OnTabChanged += OnTabClicked;
    //        //await Navigation.PushPopupAsync(moreOptionPopup).ConfigureAwait(true);
    //    }
    //    tab.IsEnabled = true;
    //}

    //private void OnTabClicked(object sender, EventArgs e)
    //{
    //    OptionModel tab = (OptionModel)sender;
    //    if (tab != null)
    //    {
    //        _moreOptions.FirstOrDefault(x => x.GroupName == tab.GroupName).IsSelected = true;
    //        TabClicked.Invoke(tab.GroupName, e);
    //    }
    //}

    #endregion
}