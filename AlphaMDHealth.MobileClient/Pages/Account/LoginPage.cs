using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class LoginPage : LoginBasePage
{
    private readonly AmhEntryControl _usernameEntry;
    private readonly AmhEntryControl _passwordEntry;
    private readonly AmhButtonControl _signinButton;
    private readonly AmhButtonControl _forgotPasswordButton;
    private readonly AmhButtonControl _registerUserButton;
    private readonly AmhHorizontalRuleControl _seperator;
    private string _resourceValue;

    public LoginPage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        PageService = new AuthService(App._essentials);
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.StartAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        AddRowColumnDefinition(new GridLength(1, GridUnitType.Auto), 6, true);
        AddRowColumnDefinition(new GridLength(1, GridUnitType.Star), 1, true);

        _usernameEntry = new AmhEntryControl()
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY,
            Icon = ImageConstants.I_USER_ID_PNG,
        };
        PageLayout.Add(_usernameEntry, 0, 1);

        _passwordEntry = new AmhEntryControl()
        {
            ResourceKey = ResourceConstants.R_PASSWORD_KEY,
            Icon = ImageConstants.I_PASSWORD_ICON_PNG,
        };
        PageLayout.Add(_passwordEntry, 0, 2);

        _signinButton = new AmhButtonControl()
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        PageLayout.Add(_signinButton, 0, 3);

        _forgotPasswordButton = new AmhButtonControl()
        {
            ResourceKey = ResourceConstants.R_FORGOT_PASSWORD_ACTION_KEY
        };
        PageLayout.Add(_forgotPasswordButton, 0, 4);

        _seperator = new AmhHorizontalRuleControl() { IsVisible = false };
        PageLayout.Add(_seperator, 0, 5);

        _registerUserButton = new AmhButtonControl()
        {
            ResourceKey = ResourceConstants.R_REGISTER_ACTION_KEY,
            IsVisible = false
        };
        PageLayout.Add(_registerUserButton, 0, 6);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LogAnalyticsData();
        await Task.WhenAll(
            (PageService as AuthService).CleanUserNotificationsAsync(false, true),
            (PageService as AuthService).GetAccountDataAsync(PageData, false
                , AppPermissions.LoginView.ToString(), AppPermissions.ForgotPasswordView.ToString(), AppPermissions.RegisterPasswordView.ToString())
        ).ConfigureAwait(true);
        App._essentials.SetPreferenceValue(StorageConstants.PR_IS_INITIAL_SYNC_COMPLETE, false);
        ApplyPageResources();
        _usernameEntry.PageResources = _passwordEntry.PageResources = _signinButton.PageResources = _forgotPasswordButton.PageResources = _registerUserButton.PageResources = PageData;
        Heading.Value = LibPermissions.GetFeatureText(PageData.FeaturePermissions, AppPermissions.LoginView.ToString());
        CheckIsSelfRegistrationAllowed();
        CheckAndRenderLinks();
        await EnableDisableLoginAsync().ConfigureAwait(true);
        _maxWrongAttempts = Convert.ToInt32(LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_MAX_LOGIN_ATTEMPTS_KEY), CultureInfo.InvariantCulture);
        _signinButton.OnValueChanged += OnSignInButtonClicked;
        _forgotPasswordButton.OnValueChanged += OnForgotPasswordButtonClicked;
        _registerUserButton.OnValueChanged += OnRegisterButtonClicked;
        AppHelper.ShowBusyIndicator = false;
    }

    protected override void OnDisappearing()
    {
        _signinButton.OnValueChanged -= OnSignInButtonClicked;
        _forgotPasswordButton.OnValueChanged -= OnForgotPasswordButtonClicked;
        _registerUserButton.OnValueChanged -= OnRegisterButtonClicked;
        base.OnDisappearing();
    }

    private async void OnSignInButtonClicked(object sender, EventArgs e)
    {
        DisplayOperationStatus(string.Empty);
        _signinButton.OnValueChanged -= OnSignInButtonClicked;
        if (await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true)
            && IsFormValid())
        {
            AppHelper.ShowBusyIndicator = true;
            AuthDTO accountData = new AuthDTO
            {
                AuthenticationData = new AuthModel
                {
                    UserName = _usernameEntry.Value?.Trim(),
                    AccountPassword = _passwordEntry.Value?.Trim(),
                    RememberMe = true
                },
            };
            await LoginAsync(accountData, true).ConfigureAwait(true);
            switch (accountData.ErrCode)
            {
                case ErrorCode.HandledRedirection:
                    //// Redirection already handelled
                    break;
                case ErrorCode.InvalidEnvironment:
                case ErrorCode.InvalidData:
                case ErrorCode.AccountLockout:
                    await UpdateRetryCountAsync(accountData.ErrCode.ToString()).ConfigureAwait(true);
                    break;
                default:
                    DisplayServerError(accountData.ErrCode);
                    break;
            }
        }
        _signinButton.OnValueChanged += OnSignInButtonClicked;
    }

    private async void OnForgotPasswordButtonClicked(object sender, EventArgs e)
    {
        if (await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;
            await ShellMasterPage.CurrentShell.PushMainPageAsync(new ForgotPasswordPage()).ConfigureAwait(false);
        }
    }

    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        if (await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            //todo: to uncomment
            //AppHelper.ShowBusyIndicator = true;
            //await ShellMasterPage.CurrentShell.PushMainPageAsync(new RegistrationPage()).ConfigureAwait(false);
        }
        //todo: to remove
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new ControlDemoPage()).ConfigureAwait(false);
        //await ShellMasterPage.CurrentShell.PushMainPageAsync(new RegistrationPage()).ConfigureAwait(false);
    }

    private void CheckAndRenderLinks()
    {
        if (TryToCreateLinkPath(SettingsConstants.S_ORGANISATION_TERMS_OF_USE_URL, out Uri? termsAndConditionsPath)
            && TryToCreateLinkPath(SettingsConstants.S_ORGANISATION_PRIVACY_URL, out Uri? privacyPolicyPath))
        {
            var acceptanceLayout = new Grid
            {
                VerticalOptions = LayoutOptions.EndAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
            };

            var signingAcceptanceLabel = new AmhLabelControl(FieldTypes.PrimarySmallHVCenterLabelControl)
            {
                ResourceKey = ResourceConstants.R_LOGINPAGE_TERMS_OFUSE_SENTENCE_KEY,
                PageResources = PageData
            };
            acceptanceLayout.Add(signingAcceptanceLabel, 0, 0);

            var termsOfUseLabel = new AmhLabelControl(FieldTypes.LinkHStartVCenterLabelControl)
            {
                Value = LibResources.GetResourcePlaceHolderByKey(PageData?.Resources, ResourceConstants.R_LOGINPAGE_TERMS_OFUSE_SENTENCE_KEY)
            };
            acceptanceLayout.Add(termsOfUseLabel, 1, 0);
            termsOfUseLabel.OnValueChanged += (s, e) => { Launcher.OpenAsync(termsAndConditionsPath); };

            var privacypolicylabel = new AmhLabelControl(FieldTypes.LinkHVCenterLabelControl)
            {
                Value = LibResources.GetResourcePlaceHolderByKey(PageData?.Resources, ResourceConstants.R_LOGINPAGE_PRIVACY_SENTENCE_KEY)
            };
            acceptanceLayout.Add(privacypolicylabel, 0, 1);
            privacypolicylabel.OnValueChanged += (s, e) => { Launcher.OpenAsync(privacyPolicyPath); };
            Grid.SetColumnSpan(privacypolicylabel, 2);

            PageLayout.Add(acceptanceLayout, 0, 7);
        }
    }

    private void CheckIsSelfRegistrationAllowed()
    {
        _seperator.IsVisible = _registerUserButton.IsVisible = IsSelfRegistrationAllowed() && PageLayout.Children.Contains(_registerUserButton);
    }

    private bool TryToCreateLinkPath(string settingKey, out Uri? pathValue)
    {
        pathValue = null;
        var settingValue = LibSettings.GetSettingValueByKey(PageData?.Settings, settingKey);
        return !string.IsNullOrWhiteSpace(settingValue)
            && Uri.TryCreate(settingValue, UriKind.Absolute, out pathValue)
            && pathValue != null;
    }

    private void DisplayServerError(ErrorCode error)
    {
        var errorMessage = LibResources.GetResourceValueByKey(PageData?.Resources, error.ToString());
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            errorMessage = LibResources.GetResourceValueByKey(PageData?.Resources, ErrorCode.InternalServerError.ToString());
        }
        DisplayOperationStatus(errorMessage);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async Task OnMaxWrongAttemptReachedAsync()
    {
        App._essentials.SetPreferenceValue(StorageConstants.PR_LAST_WRONG_LOGIN_DATE_TIME_KEY, GenericMethods.GetUtcDateTime.ToString(CultureInfo.InvariantCulture));
        await EnableDisableLoginAsync().ConfigureAwait(true);
        _wrongAttemptCount = 0;
        AppHelper.ShowBusyIndicator = false;
    }

    private async Task EnableDisableLoginAsync()
    {
        TimeSpan disableLoginTime = await new AuthService(App._essentials).CheckBlockedStatusAsync().ConfigureAwait(true);
        if (disableLoginTime > TimeSpan.Zero)
        {
            DisplayOperationStatus(string.Empty);
            var message = GetResourceByKey(ErrorCode.AccountLockout.ToString());
            ResourceModel resourceForPopup = new ResourceModel
            {
                KeyDescription = message.KeyDescription,
                ResourceKey = message.ResourceKey,
                PlaceHolderValue = message.PlaceHolderValue,
                ResourceValue = message.ResourceValue,
                InfoValue = message.InfoValue
            };
            DisplayPopup(resourceForPopup, resourceForPopup.PlaceHolderValue, disableLoginTime);
        }
    }

    private void DisplayPopup(in ResourceModel resource, string value, TimeSpan disableLoginTime)
    {
        int seconds = (int)disableLoginTime.TotalSeconds;
        //_messagePopup.ShowInPopup = true;
        //_messagePopup.ApplyButtomMarginToDescriptionLabel = true;
        //_messagePopup.PageResources = new BaseDTO { Resources = new List<ResourceModel> { resource } };
        //_messagePopup.PageResources.Resources[0].PlaceHolderValue = string.Format(CultureInfo.InvariantCulture, value, seconds.ToString(CultureInfo.InvariantCulture));
        //_messagePopup.IsVisible = true;
        //_messagePopup.ControlResourceKey = ErrorCode.AccountLockout.ToString();
        StartTimer(seconds);
        _resourceValue = value;
        OnTimerChanging += LoginPage_OnTimerChanging;
        OnTimerStop += LoginPage_OnTimerStop;
    }

    private void LoginPage_OnTimerStop(object sender, EventArgs e)
    {
        //_messagePopup.ShowInPopup = false;
    }

    private void LoginPage_OnTimerChanging(object sender, EventArgs e)
    {
        //_messagePopup.PageResources.Resources[0].PlaceHolderValue = string.Format(CultureInfo.InvariantCulture, _resourceValue, ((int)sender).ToString(CultureInfo.InvariantCulture));
        //_messagePopup.ControlResourceKey = ErrorCode.AccountLockout.ToString();
    }

    private void LogAnalyticsData()
    {
        AnalyticsModel analyticsData = new AnalyticsModel
        {
            OperatingSystem = DeviceInfo.Platform.ToString(),
            OSVersion = DeviceInfo.Version.ToString(),
            DeviceModel = DeviceInfo.Model,
            DeviceManufacturer = DeviceInfo.Manufacturer,
            PageName = nameof(LoginPage),
            LogDateTime = DateTimeOffset.UtcNow
        };
        //todo:
        //App.AtomAnalytics.LogDeviceInfo(analyticsData);
    }
}