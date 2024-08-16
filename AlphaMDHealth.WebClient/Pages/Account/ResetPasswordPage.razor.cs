using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.WebClient;
public partial class ResetPasswordPage : BasePage
{
    private AuthService _authService;
    private readonly AuthDTO _authData = new AuthDTO { AuthenticationData = new AuthModel() };
    private string _confirmPassword = string.Empty;
    private bool _isTwoFactorEnabled;

    protected override async Task OnInitializedAsync()
    {
        _authService = new AuthService(AppState.webEssentials);
        SetPageType();
        if (_authData.AuthenticationData.PageType != Utility.Pages.ChangePasswordPage)
        {
            _authData.AuthenticationData.EmailID = await _authService.GetSecuredValueAsync(StorageConstants.SS_USER_NAME_KEY).ConfigureAwait(true);
            _authData.AuthenticationData.PhoneNo = await _authService.GetSecuredValueAsync(StorageConstants.PR_PHONE_NUMBER_KEY).ConfigureAwait(true);
            _authData.AuthenticationData.OldPassword = await _authService.GetSecuredValueAsync(StorageConstants.PR_USER_CRED_KEY).ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(_authData.AuthenticationData.EmailID) && string.IsNullOrWhiteSpace(_authData.AuthenticationData.PhoneNo))
            {
                await NavigateToAsync(_authData.AuthenticationData.PageType == Utility.Pages.SetNewPasswordPage
                    ? AppPermissions.LoginView.ToString()
                    : AppPermissions.ForgotPasswordView.ToString()).ConfigureAwait(false);
                return;
            }
        }
        await SendServiceRequestAsync(_authService.GetAccountDataAsync(_authData, _authData.AuthenticationData.PageType == Utility.Pages.ChangePasswordPage), _authData).ConfigureAwait(true);
        if (_authData.AuthenticationData.PageType == Utility.Pages.ResetPasswordPage && _authData.ErrCode == ErrorCode.OK)
        {
            string setting = LibSettings.GetSettingValueByKey(_authData.Settings, SettingsConstants.S_TWO_FACTOR_ENABLED);
            _isTwoFactorEnabled = !string.IsNullOrWhiteSpace(setting) && Convert.ToBoolean(setting, CultureInfo.InvariantCulture);
        }
        _isDataFetched = true;
    }

    private void SetPageType()
    {
        if (AppState.RouterData.SelectedRoute.Page == AppPermissions.ChangePasswordView.ToString())
        {
            _authData.AuthenticationData.PageType = Utility.Pages.ChangePasswordPage;
        }
        else if (AppState.RouterData.SelectedRoute.Page == AppPermissions.SetNewPasswordView.ToString())
        {
            _authData.AuthenticationData.PageType = Utility.Pages.SetNewPasswordPage;
        }
        else
        {
            _authData.AuthenticationData.PageType = Utility.Pages.ResetPasswordPage;
        }
    }

    private async Task OnForgotPasswordClickAsync()
    {
        await NavigateToAsync(AppPermissions.ForgotPasswordView.ToString()).ConfigureAwait(false);
    }

    private async Task OnAlreadyHaveAccountClickAsync()
    {
        await NavigateToAsync(AppPermissions.LoginView.ToString()).ConfigureAwait(false);
    }

    private async Task OnResendLabelClickAsync(BaseDTO pageData)
    {
        Success = Error = string.Empty;
        AuthDTO authData = new AuthDTO
        {
            AuthenticationData = new AuthModel
            {
                UserName = _authData.AuthenticationData.EmailID,
                PageType = Utility.Pages.ResetPasswordPage
            }
        };
        await SendServiceRequestAsync(_authService.ResendSmsAsync(authData), authData).ConfigureAwait(true);
        pageData.ErrCode = authData.ErrCode;
    }

    private async Task OnSendClickAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            if (_confirmPassword.Trim() != _authData.AuthenticationData.AccountPassword.Trim())
            {
                Error = ResourceConstants.R_PASSWORD_MISMATCH_ERROR_KEY;
            }
            else
            {
                AuthDTO userData = new AuthDTO
                {
                    AuthenticationData = new AuthModel
                    {
                        PageType = _authData.AuthenticationData.PageType,
                        EmailID = _authData.AuthenticationData.EmailID,
                        PhoneNo = _authData.AuthenticationData.PhoneNo,
                        Otp = _authData.AuthenticationData.Otp?.Trim(),
                        AccountPassword = _authData.AuthenticationData.AccountPassword.Trim(),
                        OldPassword = _authData.AuthenticationData.PageType == Utility.Pages.ResetPasswordPage ? string.Empty : _authData.AuthenticationData.OldPassword?.Trim()
                    },
                };
                await SendServiceRequestAsync(_authService.ResetPasswordAsync(userData), userData).ConfigureAwait(true);
                switch (userData.ErrCode)
                {
                    case ErrorCode.OK:
                    case ErrorCode.AccountLockout:
                        await _authService.ClearAccountTokensAndIdAsync().ConfigureAwait(true);
                        await NavigateToAsync(AppPermissions.LoginView.ToString(), true).ConfigureAwait(false);
                        break;
                    default:
                        Error = userData.ErrCode.ToString();
                        break;
                }
            }
        }
    }
}