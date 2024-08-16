using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration("ChangePasswordPage")]
public class ResetPasswordPage : LoginBasePage
{
    private AmhEntryControl _validationCodeEntry;
    private readonly AmhEntryControl _newPasswordEntry;
    private readonly AmhEntryControl _confirmPasswordEntry;
    private readonly AmhWebViewControls _staticContentWebview;
    private readonly AmhButtonControl _setButton;
    private AmhButtonControl _backToForgotPasswordButton;
    private AmhButtonControl _resendOtpButton;
    private AmhLabelControl _otpInfo;
    private ResourceModel _resendSMS;
    private readonly Pages _pageType;
    private readonly UserDTO _userData = new UserDTO();
    private AuthDTO _accountData;
    private bool _isPageCleared;

    /// <summary>
    /// To set Password
    /// </summary>
    public ResetPasswordPage() : this(Pages.ChangePasswordPage) { }

    /// <summary>
    /// To set Password of newly registered patient
    /// </summary>
    public ResetPasswordPage(UserDTO userData) : this(Pages.RegisterPasswordPage)
    {
        _userData = userData;
    }

    /// <summary>
    /// To set Password
    /// </summary>
    /// <param name="pageType">page type</param>
    public ResetPasswordPage(Pages pageType) : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        PageService = new AuthService(App._essentials);
        _pageType = pageType;
        // _headerLabel = new AmhLabelControl(FieldTypes.PrimarySmallHVCenterLabelControl);
        _newPasswordEntry = new AmhEntryControl()
        {
            ResourceKey = ResourceConstants.R_NEW_PASSWORD_KEY,
            Icon = ImageConstants.I_PASSWORD_ICON_PNG,
        };
        _confirmPasswordEntry = new AmhEntryControl()
        {
            ResourceKey = ResourceConstants.R_CONFIRM_PASSWORD_KEY,
            Icon = ImageConstants.I_PASSWORD_ICON_PNG,
        };
        _setButton = new AmhButtonControl(FieldTypes.PrimaryButtonControl);
        _staticContentWebview = new AmhWebViewControls //todo:HtmlLabel
        {
            FieldType = FieldTypes.HtmlWebviewControl
        };
        PageLayout.Add(_newPasswordEntry, 0, 3);
        PageLayout.Add(_confirmPasswordEntry, 0, 4);
        PageLayout.Add(_staticContentWebview, 0, 5);
        PageLayout.Add(_setButton, 0, 6);
        AddPageSpecificUI();
    }

    protected override async void OnAppearing()
    {
        if (!_isPageCleared)
        {
            await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
            base.OnAppearing();
            await (PageService as AuthService).GetAccountDataAsync(PageData, false, GetFeatureName());
            await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true);
            _setButton.Value = LibResources.GetResourceValueByKey(PageData?.Resources, _pageType == Pages.RegisterPasswordPage ? ResourceConstants.R_CREATE_ACCOUNT_KEY : ResourceConstants.R_SET_BUTTON_KEY);
            Heading.Value = LibPermissions.GetFeatureText(PageData.FeaturePermissions, GetFeatureName());
            var resource = PageData.Resources.Where(x => x.ResourceKey == ResourceConstants.R_NEW_PASSWORD_KEY).ToList();
            _staticContentWebview.Value = GetResourceByKey(ResourceConstants.R_NEW_PASSWORD_KEY).InfoValue;
            _confirmPasswordEntry.PageResources = PageData;
            _newPasswordEntry.PageResources = PageData;
            _setButton.OnValueChanged += OnButtonClicked;
            switch (_pageType)
            {
                case Pages.RegisterPasswordPage:
                    RenderDataInOtpField();
                    break;
                case Pages.ResetPasswordPage:
                    _backToForgotPasswordButton.Value = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_FORGOT_PASSWORD_ACTION_KEY);
                    _backToForgotPasswordButton.OnValueChanged += OnBackToForgotPasswordClicked;
                    RenderDataInOtpField();
                    break;
                default:
                    // for future implementation
                    break;
            }
            if (_validationCodeEntry != null && _validationCodeEntry.IsVisible)
            {
                _validationCodeEntry.PageResources = PageData;
            }
            AppHelper.ShowBusyIndicator = false;
        }
    }

    protected override void OnDisappearing()
    {
        _setButton.OnValueChanged -= OnButtonClicked;
        switch (_pageType)
        {
            case Pages.RegisterPasswordPage:
                _resendOtpButton.OnValueChanged -= OnResendOtpClicked;
                break;
            case Pages.ResetPasswordPage:
                _backToForgotPasswordButton.OnValueChanged -= OnBackToForgotPasswordClicked;
                _resendOtpButton.OnValueChanged -= OnResendOtpClicked;
                break;
            default:
                // for future implementation
                break;
        }
        base.OnDisappearing();
    }

    private async void OnButtonClicked(object sender, EventArgs e)
    {
        if (await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true) && IsFormValid())
        {
            if ((_newPasswordEntry.Value as string).Trim() != (_confirmPasswordEntry.Value as string).Trim())
            {
                DisplayOperationStatus(LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_PASSWORD_MISMATCH_ERROR_KEY));
                return;
            }
            AppHelper.ShowBusyIndicator = true;
            if (_pageType == Pages.RegisterPasswordPage)
            {
                await RegisterPatientAsync().ConfigureAwait(true);
            }
            else
            {
                string userName = await (PageService as AuthService).GetSecuredValueAsync(StorageConstants.SS_USER_NAME_KEY).ConfigureAwait(true);
                _accountData = new AuthDTO
                {
                    AuthenticationData = new AuthModel
                    {
                        UserName = userName,
                        EmailID = userName,
                        PhoneNo = await (PageService as AuthService).GetSecuredValueAsync(StorageConstants.PR_PHONE_NUMBER_KEY).ConfigureAwait(true),
                        AccountPassword = (_newPasswordEntry.Value as string).Trim(),
                        PageType = _pageType
                    },
                    IsActive = _pageType == Pages.ChangePasswordPage
                };
                switch (_pageType)
                {
                    case Pages.ChangePasswordPage:
                        _accountData.AuthenticationData.OldPassword = (_validationCodeEntry.Value as string).Trim();
                        break;
                    case Pages.ResetPasswordPage:
                        _accountData.AuthenticationData.Otp = (_validationCodeEntry?.Value as string)?.Trim();
                        break;
                    default:
                        _accountData.AuthenticationData.OldPassword = await (PageService as AuthService).GetSecuredValueAsync(StorageConstants.PR_USER_CRED_KEY).ConfigureAwait(true);
                        break;
                }
                await (PageService as AuthService).ResetPasswordAsync(_accountData).ConfigureAwait(true);
                await ErrorCodeMappingAsync(_accountData).ConfigureAwait(false);
            }
        }
    }

    private async void OnResendOtpClicked(object sender, EventArgs e)
    {
        if (await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;
            _accountData = new AuthDTO { AuthenticationData = new AuthModel() };
            if (_pageType == Pages.RegisterPasswordPage)
            {
                _accountData.AuthenticationData.UserName = _userData.User.FirstName;
                _accountData.AuthenticationData.EmailID = _userData.User.EmailId;
                _accountData.AuthenticationData.PhoneNo = _userData.User.PhoneNo;
                _accountData.AuthenticationData.IsExternal = _userData.User.IsSelfRegistration;
            }
            else
            {
                _accountData.AuthenticationData.UserName = await (PageService as AuthService).GetSecuredValueAsync(StorageConstants.SS_USER_NAME_KEY).ConfigureAwait(true);
            }
            _accountData.AuthenticationData.PageType = _pageType;
            if (!string.IsNullOrWhiteSpace(_accountData.AuthenticationData.UserName))
            {
                await (PageService as AuthService).ResendSmsAsync(_accountData).ConfigureAwait(true);
                if (_accountData.ErrCode == ErrorCode.OK)
                {
                    StartResendSmsTimer();
                    _resendOtpButton.IsVisible = false;
                    AppHelper.ShowBusyIndicator = false;
                    return;
                }
            }
            DisplayOperationStatus(LibResources.GetResourceValueByKey(PageData?.Resources, ErrorCode.InvalidData.ToString()));
            AppHelper.ShowBusyIndicator = false;
        }
    }

    private async void OnBackToForgotPasswordClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new ForgotPasswordPage()).ConfigureAwait(false);
    }

    private void StartResendSmsTimer()
    {
        int duration = Convert.ToInt16(LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_OTP_RESEND_DURATION_KEY), CultureInfo.InvariantCulture) * 60;
        _resendSMS = GetResourceByKey(ResourceConstants.R_OTP_CODE_COUNTDOWN_MESSAGE_KEY);
        StartTimer(duration);
        OnTimerChanging += ResetPasswordPage_OnTimerChanging;
        OnTimerStop += ResetPasswordPage_OnTimerStop;
    }

    private void ResetPasswordPage_OnTimerChanging(object sender, EventArgs e)
    {
        _otpInfo.IsVisible = true;
        _otpInfo.Value = _resendSMS?.ResourceValue?.Replace("{0}", ((int)sender).ToString(CultureInfo.InvariantCulture));
    }

    private void ResetPasswordPage_OnTimerStop(object sender, EventArgs e)
    {
        _resendOtpButton.IsVisible = true;
        _otpInfo.IsVisible = false;
    }

    private async Task RegisterPatientAsync()
    {
        _userData.IsActive = false;
        _userData.User.AccountPassword = (_newPasswordEntry.Value as string).Trim();
        _userData.User.Otp = (_validationCodeEntry?.Value as string)?.Trim();
        await new UserService(App._essentials).RegisterUserAsync(_userData).ConfigureAwait(true);
        if (_userData.ErrCode == ErrorCode.OK)
        {
            _accountData = new AuthDTO
            {
                AuthenticationData = new AuthModel { UserName = _userData.User.EmailId, AccountPassword = _userData.User.AccountPassword, RememberMe = true },
            };
            await LoginAsync(_accountData, true).ConfigureAwait(true);
            if (_accountData.ErrCode != ErrorCode.HandledRedirection)
            {
                DisplayServerError(_accountData.ErrCode);
            }
        }
        else
        {
            DisplayServerError(_userData.ErrCode);
        }
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

    private async Task ErrorCodeMappingAsync(AuthDTO accountData)
    {
        switch (accountData.ErrCode)
        {
            case ErrorCode.OK:
                await HandleSuccessCaseAsync(accountData).ConfigureAwait(false);
                break;
            case ErrorCode.AccountLockout:
                ClearShellContents();
                await DisplayMessagePopupAsync(accountData.ErrCode.ToString(), OnPupupActionClicked, false, true, true).ConfigureAwait(true);
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
                break;
            case ErrorCode.Unauthorized:
                DisplayOperationStatus(LibResources.GetResourceValueByKey(PageData?.Resources, ErrorCode.InvalidData.ToString()));
                AppHelper.ShowBusyIndicator = false;
                break;
            case ErrorCode.TokenExpired:
            case ErrorCode.InActiveUser:
                ClearShellContents();
                await UnauthorizedOrTokenExpiredTaskResultAsync(accountData, _pageType).ConfigureAwait(false);
                break;
            default:
                DisplayOperationStatus(LibResources.GetResourceValueByKey(PageData?.Resources, accountData.ErrCode.ToString()));
                AppHelper.ShowBusyIndicator = false;
                break;
        }
    }

    private async Task HandleSuccessCaseAsync(AuthDTO accountData)
    {
        if (_pageType == Pages.ResetPasswordPage)
        {
            accountData.AuthenticationData.Otp = string.Empty;
            accountData.AuthenticationData.RememberMe = true;
            await LoginAsync(accountData, false).ConfigureAwait(true);
            if (accountData.ErrCode != ErrorCode.SetPinCode && accountData.ErrCode != ErrorCode.PinCodeLogin && accountData.ErrCode != ErrorCode.SMSAuthentication
                && accountData.ErrCode != ErrorCode.SetNewPassword && accountData.ErrCode != ErrorCode.ResetPassword)
            {
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
            }
        }
        else
        {
            ClearShellContents();
            await ShellMasterPage.CurrentShell.LogoutAsync(true).ConfigureAwait(false);
        }
    }

    private void ClearShellContents()
    {
        if (_pageType == Pages.ChangePasswordPage)
        {
            _isPageCleared = true;
            ShellMasterPage.CurrentShell.ClearShellContent();
        }
    }

    private void AddPageSpecificUI()
    {
        switch (_pageType)
        {
            case Pages.RegisterPasswordPage:
                AddOtpField();
                break;
            case Pages.ResetPasswordPage:
                AddOtpField();
                _backToForgotPasswordButton = new AmhButtonControl(FieldTypes.TransparentButtonControl);
                PageLayout.Add(_backToForgotPasswordButton, 0, 7);
                break;
            case Pages.ChangePasswordPage:
                _validationCodeEntry = new AmhEntryControl(FieldTypes.PasswordEntryControl)
                {
                    ResourceKey = ResourceConstants.R_PASSWORD_KEY,
                    Icon = ImageConstants.I_PASSWORD_ICON_PNG,
                };
                PageLayout.Add(_validationCodeEntry, 0, 1);
                break;
            default:
                ////not required currently
                break;
        }
    }

    private void AddOtpField()
    {
        _validationCodeEntry = new AmhEntryControl(FieldTypes.PinCodeControl)
        {
            ResourceKey = ResourceConstants.R_VERIFICATION_KEY,
            Icon = ImageConstants.I_PASSWORD_ICON_PNG,
        };
        var padding = Convert.ToInt32(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture);
        _resendOtpButton = new AmhButtonControl(FieldTypes.TransparentButtonControl)
        {
            IsVisible = false,
            Margin = new Thickness(0, -padding / 2, 0, padding / 2)
        };
        _otpInfo = new AmhLabelControl(FieldTypes.PrimarySmallHVCenterLabelControl)
        {
            Margin = new Thickness(0, 0, 0, padding),
        };
        PageLayout.Add(_validationCodeEntry, 0, 1);
        PageLayout.Add(_otpInfo, 0, 2);
        PageLayout.Add(_resendOtpButton, 0, 2);
    }

    private void RenderDataInOtpField()
    {
        string isTwoFactorEnabled = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_TWO_FACTOR_ENABLED);
        if (string.IsNullOrWhiteSpace(isTwoFactorEnabled) || !Convert.ToBoolean(isTwoFactorEnabled, CultureInfo.InvariantCulture))
        {
            _resendOtpButton.IsVisible = false;
            _validationCodeEntry.IsVisible = false;
            _otpInfo.IsVisible = false;
        }
        else
        {
            _resendOtpButton.Value = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_RESEND_OTP_ACTION_KEY);
            _resendOtpButton.OnValueChanged += OnResendOtpClicked;
            StartResendSmsTimer();
        }
    }

    private string GetFeatureName()
    {
        switch (_pageType)
        {
            case Pages.ChangePasswordPage:
                return AppPermissions.ChangePasswordView.ToString();
            case Pages.RegisterPasswordPage:
                return AppPermissions.RegisterPasswordView.ToString();
            default:
                return AppPermissions.SetNewPasswordView.ToString();
        }
    }

}