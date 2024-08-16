using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class SMSValidationPage : BasePage
{
    private AuthDTO _authData = new AuthDTO { IsActive = false, AuthenticationData = new AuthModel() };
    private int _maxWrongAttempts;
    private int _wrongAttempts;

    /// <summary>
    /// Control value represents text of button control
    /// </summary>
    [Parameter]
    public BaseDTO PageData { get; set; }

    /// <summary>
    /// Control value 
    /// </summary>
    [Parameter]
    public string Value
    {
        get { return _authData.AuthenticationData.Otp; }
        set
        {
            if (_authData.AuthenticationData.Otp != value)
            {
                _authData.AuthenticationData.Otp = value;
                ValueChanged.InvokeAsync(value);
            }
        }
    }

    /// <summary>
    /// Bindable property of Value
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// On resend button click action
    /// </summary>
    [Parameter]
    public EventCallback<BaseDTO> OnResendClicked { get; set; }

    /// <summary>
    /// On verify button click action
    /// </summary>
    [Parameter]
    public EventCallback<string> OnVerifyClicked { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (PageData != null)
        {
            _authData.Resources = PageData.Resources;
            _authData.Settings = PageData.Settings;
        }
        else
        {
            _authData.AuthenticationData = new AuthModel();
            var task = LoadDataAsync();
            await SendServiceRequestAsync(task, _authData).ConfigureAwait(true);
            if (task.Result)
            {
                await NavigateToAsync(AppPermissions.LoginView.ToString()).ConfigureAwait(false);
                return;
            }
        }
        _isDataFetched = true;
    }

    private async Task OnVerifyButtonClickAsync()
    {
        Error = Success = string.Empty;
        if (IsValid())
        {
            if (PageData == null)
            {
                AuthDTO authData = new AuthDTO
                {
                    AuthenticationData = new AuthModel
                    {
                        UserName = _authData.AuthenticationData.UserName,
                        AccountPassword = _authData.AuthenticationData.AccountPassword,
                        RememberMe = _authData.AuthenticationData.RememberMe,
                        Otp = _authData.AuthenticationData.Otp
                    }
                };
                await SendServiceRequestAsync(new AuthService(AppState.webEssentials).LoginAsync(authData, false, null, CancellationToken.None), authData).ConfigureAwait(true);
                _authData.ErrCode = authData.ErrCode;
                switch (_authData.ErrCode)
                {
                    case ErrorCode.InvalidData:
                    case ErrorCode.InValidOTP:
                        await InvalidOtpAttemptCheckAsync().ConfigureAwait(false);
                        break;
                    case ErrorCode.HandledRedirection:
                        await ClearStoredDataAsync().ConfigureAwait(false);
                        break;
                    case ErrorCode.OK:
                        await ClearStoredDataAsync().ConfigureAwait(true);
                        await NavigateToAsync(AppPermissions.NavigationComponent.ToString(), true).ConfigureAwait(false);
                        break;
                    case ErrorCode.SetPinCode:
                        await NavigateToAsync(AppPermissions.PinCodeView.ToString()).ConfigureAwait(false);
                        break;
                    case ErrorCode.SetNewPassword:
                        await NavigateToAsync(AppPermissions.SetNewPasswordView.ToString()).ConfigureAwait(false);
                        break;
                    default:
                        //For futrher implementation
                        break;
                }
            }
            else
            {
                await OnVerifyClicked.InvokeAsync(Value);
            }
        }
    }

    private async Task OnResendLabelClickAsync(BaseDTO pageData)
    {
        if (PageData == null)
        {
            Success = Error = string.Empty;
            AuthDTO authData = new AuthDTO
            {
                AuthenticationData = new AuthModel
                {
                    UserName = _authData.AuthenticationData.UserName
                }
            };
            await new AuthService(AppState.webEssentials).ResendSmsAsync(authData).ConfigureAwait(true);
            await ExecuteTaskResultAsync(authData).ConfigureAwait(true);
            _authData.ErrCode = authData.ErrCode;
            pageData.ErrCode = authData.ErrCode;
        }
        else
        {
            await OnResendClicked.InvokeAsync(pageData);
        }
    }

    private async Task<bool> LoadDataAsync()
    {
        AuthService authService = new AuthService(AppState.webEssentials);
        await authService.GetAccountDataAsync(_authData).ConfigureAwait(true);
        _authData.AuthenticationData.UserName = await authService.GetSecuredValueAsync(StorageConstants.SS_USER_NAME_KEY).ConfigureAwait(true);
        _authData.AuthenticationData.AccountPassword = await authService.GetSecuredValueAsync(StorageConstants.PR_USER_CRED_KEY).ConfigureAwait(true);
        if (string.IsNullOrWhiteSpace(_authData.AuthenticationData.UserName) || string.IsNullOrWhiteSpace(_authData.AuthenticationData.AccountPassword))
        {
            return true;
        }
        _authData.AuthenticationData.RememberMe = (AppState.webEssentials).GetPreferenceValue(StorageConstants.PR_REMEMBER_ME_KEY, false);
        _maxWrongAttempts = Convert.ToInt32(LibSettings.GetSettingValueByKey(_authData.Settings, SettingsConstants.S_MAX_LOGIN_ATTEMPTS_KEY), CultureInfo.InvariantCulture);
        return false;
    }

    private async Task InvalidOtpAttemptCheckAsync()
    {
        _wrongAttempts++;
        Error = LibResources.GetResourceValueByKey(_authData.Resources, _authData.ErrCode.ToString())
            + $" {_wrongAttempts}{Constants.SYMBOL_SLASH}{_maxWrongAttempts}";
        if (_wrongAttempts >= _maxWrongAttempts)
        {
            await ClearStoredDataAsync().ConfigureAwait(true);
            await NavigateToAsync(AppPermissions.LoginView.ToString()).ConfigureAwait(false);
        }
    }

    private async Task ClearStoredDataAsync()
    {
        AppState.webEssentials.DeletePreferenceValue(StorageConstants.PR_REMEMBER_ME_KEY);
        await new BaseService(AppState.webEssentials).DeleteSecuredValuesAsync(StorageConstants.SS_USER_NAME_KEY,
            StorageConstants.PR_USER_CRED_KEY,
            StorageConstants.PR_PHONE_NUMBER_KEY
        ).ConfigureAwait(false);
    }
}