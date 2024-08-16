using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Web;

namespace AlphaMDHealth.MobileClient;

public class InitializationPage : BasePage
{
    private readonly BaseDTO _initializationPageData = new BaseDTO();
    private bool _isDynamicLinkExecuted = true;
    private readonly TaskCompletionSource<string> _dynamicLinkTaskCompletion;
    private readonly Pages _flow;
    private bool _isExecutionComplete;

    /// <summary>
    /// Default Constructor
    /// </summary>
    public InitializationPage() : this(Pages.InitializationPage)
    { }

    /// <summary>
    /// Initialization page with dynamic link completion handler
    /// </summary>
    /// <param name="dynamicLinkTaskCompletion">DynamicLink evaluation task completion</param>
    public InitializationPage(TaskCompletionSource<string> dynamicLinkTaskCompletion) : this(Pages.InitializationPage)
    {
        //_dynamicLinkTaskCompletion = dynamicLinkTaskCompletion;
    }

    /// <summary>
    /// Constructor with language flow parameter
    /// </summary>
    public InitializationPage(Pages flow)
    {
        _flow = flow;
        AddRowColumnDefinition(GridLength.Star, 1, true);
        BackgroundColor = Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR);
        PageLayout.Add(new Image
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Source = ImageSource.FromFile(Device.RuntimePlatform == Device.Android
                ? ImageConstants.I_LOGIN_LOGO_PNG 
                : ImageConstants.I_SPLASH_ICON_PNG)
        }, 0, 0);
        //todo: PageLayout.Effects.Add(new CustomSafeAreaTopInsetEffect());
    }

    protected override async void OnAppearing()
    {
        if (!_isExecutionComplete)
        {
            base.OnAppearing();
            // This has been added to solve the busy indicator not appearing in iOS during launch
            await Task.Delay(100).ConfigureAwait(false);
            AppHelper.ShowBusyIndicator = true;
            await SyncAndApplyMasterDataAsync(_initializationPageData, _flow).ConfigureAwait(false);
            if (_initializationPageData.ErrCode == ErrorCode.OK)
            {
                //One link 
                string dynamicLinkData = App._essentials.GetPreferenceValue(StorageConstants.PR_DYNAMIC_LINK_DATA_KEY, string.Empty);
                if (_dynamicLinkTaskCompletion != null)
                {
                    dynamicLinkData = await _dynamicLinkTaskCompletion.Task.ConfigureAwait(true);
                    App._essentials.SetPreferenceValue(StorageConstants.PR_DYNAMIC_LINK_DATA_KEY, dynamicLinkData);
                }
                if (!string.IsNullOrWhiteSpace(dynamicLinkData)
                   && _initializationPageData.ErrCode == ErrorCode.OK
                   && _initializationPageData.ErrorDescription?.ToEnum<Pages>() != Pages.LanguageSelectionPage)
                {
                    await HandleDynamicLinkAsync(dynamicLinkData).ConfigureAwait(true);
                }
            }
            _isExecutionComplete = true;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await NavigateToPageAsync().ConfigureAwait(false);
            });
        }
    }

    private async Task SyncAndApplyMasterDataAsync(BaseDTO initializationPageData, Pages flow)
    {
        await new DataSyncService(App._essentials).InitializeApplicationAsync(initializationPageData, flow, CallServiceAsync).ConfigureAwait(false);
        if (initializationPageData.ErrCode == ErrorCode.OK)
        {
            FetchDeviceSettings();
            await Task.WhenAll(
                new AppStyles().LoadAppStylesAsync(),
                ApplySecurityChecksAsync(initializationPageData)
            ).ConfigureAwait(false);
        }
    }

    private void FetchDeviceSettings()
    {
        //todo:
        //IDeviceSettingInfo deviceSetting = DependencyService.Get<IDeviceSettingInfo>();
        bool is24HourFormat = false;//deviceSetting.Is24HourFormat();
        App._essentials.SetPreferenceValue(StorageConstants.PR_IS_24_HOUR_FORMAT, is24HourFormat);
    }

    private async Task ApplySecurityChecksAsync(BaseDTO initializationPageData)
    {
        try
        {
            await GetSettingsAsync(GroupConstants.RS_SECURITY_GROUP, GroupConstants.RS_ENVIRONMENT_GROUP).ConfigureAwait(true);
            // todo:
            //initializationPageData.ErrCode = await new SecurityHelper().ApplySecurityChecksAsync(new SecurityConfigurationModel
            //{
            //    BlockJailbrokenDevice = false,//Convert.ToBoolean(GetSettingsValueByKey(SettingsConstants.S_IS_JAIL_BREAKING_DEVICE_ALLOWED_KEY), CultureInfo.InvariantCulture),
            //    BlockTapJacking = Convert.ToBoolean(GetSettingsValueByKey(SettingsConstants.S_IS_TAP_JACKING_ALLOWED_KEY), CultureInfo.InvariantCulture),
            //    IsCertifiatePiningAllowed = Convert.ToBoolean(GetSettingsValueByKey(SettingsConstants.S_IS_CERTIFIATE_PINING_ALLOWED_KEY), CultureInfo.InvariantCulture),
            //    IsReadCopyClipboardAllowed = Convert.ToBoolean(GetSettingsValueByKey(SettingsConstants.S_IS_READ_COPY_CLIPBOARD_ALLOWED_KEY), CultureInfo.InvariantCulture),
            //    IsScreenshotAllowed = false,//Convert.ToBoolean(GetSettingsValueByKey(SettingsConstants.S_IS_SCREEN_SHOT_ALLOWED_KEY), CultureInfo.InvariantCulture)
            //}).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogErrors(nameof(InitializationPage) + ".ApplySecurityChecksAsync()", ex);
            initializationPageData.ErrCode = ErrorCode.RestartApp;
        }
    }

    private async Task NavigateToPageAsync()
    {
        if (_initializationPageData.ErrCode == ErrorCode.OK)
        {
            if (_initializationPageData.ErrorDescription == Pages.ShellMasterPage.ToString() && ShellMasterPage.CurrentShell.HasMainPage)
            {
                _initializationPageData.ErrorDescription = Pages.PincodeLoginPage.ToString();
            }
            await NavigateToPageBasedOnResponseAsync().ConfigureAwait(false);
        }
        else
        {
            if (_initializationPageData.ErrCode != ErrorCode.HandledRedirection)
            {
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(_initializationPageData.ErrCode.ToString())).ConfigureAwait(false);
            }
        }
    }

    private async Task NavigateToPageBasedOnResponseAsync()
    {
        switch (_initializationPageData.ErrorDescription?.ToEnum<Pages>())
        {
            case Pages.LanguageSelectionPage:
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new LanguageSelectionPage()).ConfigureAwait(false);
                break;
            case Pages.AppIntroPage:
                App._essentials.SetPreferenceValue<bool>(StorageConstants.PR_IS_APP_INTRO_SHOWN_KEY, true);
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new WelcomeScreensPage()).ConfigureAwait(false);
                break;
            case Pages.LoginPage:
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
                break;
            case Pages.PincodeLoginPage:
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new PincodePage(AppPermissions.PincodeLoginView.ToString(), false)).ConfigureAwait(false);
                break;
            case Pages.ShellMasterPage:
                await ShellMasterPage.CurrentShell.RenderPageAsync().ConfigureAwait(false);
                break;
            default:
                AppHelper.ShowBusyIndicator = false;
                break;
        }
    }

    private async Task HandleDynamicLinkAsync(string dynamicLinkData)
    {
        if (_isDynamicLinkExecuted)
        {
            if (Shell.Current.CurrentItem?.Navigation?.NavigationStack?.Count > 1 && Shell.Current.CurrentItem?.Navigation?.NavigationStack?[1] is ProfilePage)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.Navigation.PopAsync(false).ConfigureAwait(true);
                });
            }
            _isDynamicLinkExecuted = false;
            AuthDTO authData = new AuthDTO
            {
                AddedBy = GenericMethods.GetPlatformSpecificValue(HttpUtility.UrlDecode(dynamicLinkData), dynamicLinkData, dynamicLinkData)
                    .Split(new[] { Constants.ONELINK_PARAMETER_SEPERATOR_KEY + Constants.SYMBOL_EQUAL }, StringSplitOptions.None).Last(),
                Settings = PageData.Settings
            };
            await new AuthService(App._essentials).GetDynamicLinkDataAsync(authData, CallServiceAsync);
            if (authData.ErrCode == ErrorCode.OK)
            {
                await SyncAndApplyMasterDataAsync(_initializationPageData, _flow).ConfigureAwait(false);
                if (_initializationPageData.ErrCode != ErrorCode.OK)
                {
                    return;
                }
                await LoginAsync(authData, false).ConfigureAwait(true);
                App._essentials.SetPreferenceValue(StorageConstants.PR_DYNAMIC_LINK_DATA_KEY, string.Empty);
                _isDynamicLinkExecuted = true;
                if (authData.ErrCode == ErrorCode.OK || authData.ErrCode == ErrorCode.HandledRedirection)
                {
                    _initializationPageData.ErrCode = authData.ErrCode;
                    AppHelper.ShowBusyIndicator = false;
                    return;
                }
            }
            _initializationPageData.ErrCode = ErrorCode.HandledRedirection;
            /* If error occurs in dynamic link then redirect user to login page */
            _isDynamicLinkExecuted = true;
            AppHelper.ShowBusyIndicator = true;
            await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
        }
    }
}