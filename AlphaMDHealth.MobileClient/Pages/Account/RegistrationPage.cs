using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class RegistrationPage : LoginBasePage
{
    private readonly AmhLabelControl _headerLabel;
    private readonly AmhEntryControl _firstNameEntry;
    private readonly AmhEntryControl _lastNameEntry;
    private readonly AmhMobileNumberControl _mobileEntry;
    private readonly AmhEntryControl _emailEntry;
    private readonly AmhButtonControl _signinButton;
    private readonly AmhButtonControl _alreadyAccButton;

    public RegistrationPage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        IsVisible = false;
        _headerLabel = new AmhLabelControl(FieldTypes.PrimaryAppLargeHStartVCenterBoldLabelControl);
        _firstNameEntry = new AmhEntryControl
        {
            ResourceKey = ResourceConstants.R_FIRST_NAME_KEY,
            FieldType = FieldTypes.AlphaEntryControl
        };
        _lastNameEntry = new AmhEntryControl
        {
            ResourceKey = ResourceConstants.R_LAST_NAME_KEY,
            FieldType = FieldTypes.AlphaEntryControl
        };
        _mobileEntry = new AmhMobileNumberControl
        {
            ResourceKey = ResourceConstants.R_MOBILE_NUMBER_KEY,
        };
        _emailEntry = new AmhEntryControl
        {
            ResourceKey = ResourceConstants.R_EMAIL_ADDRESS_KEY,
            FieldType = FieldTypes.EmailEntryControl,
            Icon = ImageConstants.I_EMAIL_ICON_PNG,
        };
        _signinButton = new AmhButtonControl(FieldTypes.TransparentButtonControl);
        _alreadyAccButton = new AmhButtonControl(FieldTypes.TransparentButtonControl);
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.StartAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        AddRowColumnDefinition(GridLength.Auto, 7, true);
        PageLayout.Add(_headerLabel, 0, 0);
        PageLayout.Add(_firstNameEntry, 0, 1);
        PageLayout.Add(_lastNameEntry, 0, 2);
        PageLayout.Add(_mobileEntry, 0, 3);
        PageLayout.Add(_emailEntry, 0, 4);
        PageLayout.Add(_signinButton, 0, 5);
        PageLayout.Add(_alreadyAccButton, 0, 6);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await new AuthService(App._essentials).GetAccountDataAsync(PageData).ConfigureAwait(true);
        if (PageData.ErrCode == ErrorCode.OK && IsSelfRegistrationAllowed()
            && await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            _firstNameEntry.PageResources = PageData;
            _lastNameEntry.PageResources = PageData;
            _firstNameEntry.RegexExpression = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_ALPHA_REGEX_KEY);
            _lastNameEntry.RegexExpression = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_ALPHA_REGEX_KEY);
            _emailEntry.RegexExpression = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_EMAIL_REGEX_KEY);
            _emailEntry.PageResources = PageData;
            _mobileEntry.PageResources = PageData;
            _headerLabel.Value = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_CREATE_ACCOUNT_KEY);
            _signinButton.Value = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_NEXT_ACTION_KEY);
            _alreadyAccButton.Value = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_ALREADY_HAVE_LOGIN_ACTION_KEY);
            _signinButton.OnValueChanged += OnSignInButtonClicked;
            _alreadyAccButton.OnValueChanged += OnAlreadyAccountClicked;
            IsVisible = true;
            AppHelper.ShowBusyIndicator = false;
        }
        else
        {
            await BackToLoginAsync().ConfigureAwait(false);
        }
    }

    private async void OnAlreadyAccountClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await BackToLoginAsync().ConfigureAwait(false);
    }

    private async void OnSignInButtonClicked(object sender, EventArgs e)
    {
        _signinButton.OnValueChanged -= OnSignInButtonClicked;
        if (await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true) && IsFormValid())
        {
            AppHelper.ShowBusyIndicator = true;
            UserDTO userData = new UserDTO
            {
                User = new UserModel
                {
                    FirstName = _firstNameEntry.Value.Trim(),
                    LastName = _lastNameEntry.Value.Trim(),
                    EmailId = _emailEntry.Value.Trim(),
                    PhoneNo = _mobileEntry.Value.Trim(),
                    IsUser = false,
                    IsSelfRegistration = true
                }
            };
            await new UserService(App._essentials).RegisterUserAsync(userData).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
            if (userData.ErrCode == ErrorCode.SMSAuthentication || userData.ErrCode == ErrorCode.SetNewPassword)
            {
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new ResetPasswordPage(userData)).ConfigureAwait(false);
            }
            else
            {
                DisplayServerError(userData.ErrCode);
            }
        }
        _signinButton.OnValueChanged += OnSignInButtonClicked;
    }

    protected override void OnDisappearing()
    {
        _signinButton.OnValueChanged -= OnSignInButtonClicked;
        _alreadyAccButton.OnValueChanged -= OnAlreadyAccountClicked;
        base.OnDisappearing();
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

    private async Task BackToLoginAsync()
    {
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
    }
}