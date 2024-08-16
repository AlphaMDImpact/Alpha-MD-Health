using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;
using Wangkanai;

namespace AlphaMDHealth.WebClient;

public partial class PinCodePage : BasePage
{
    private readonly BaseDTO _pinCodeData = new BaseDTO();
    private AuthService _authService;
    private double? _pinCode;
    private double? _confirmPinCode;
    private int _maxWrongAttempts;
    private int _wrongAttempts;
    private bool _isPinCodeSetup;

    protected override async Task OnInitializedAsync()
    {
        _authService = new AuthService(AppState.webEssentials);
        //IsActive represents page is pinCode setup page
        _pinCodeData.IsActive = _isPinCodeSetup = AppState.RouterData.SelectedRoute.Page == AppPermissions.PinCodeView.ToString();
        await SendServiceRequestAsync(_authService.GetPinCodeDataAsync(_pinCodeData), _pinCodeData).ConfigureAwait(true);
        if (_pinCodeData.ErrCode == ErrorCode.OK)
        {
                _maxWrongAttempts = Convert.ToInt32(LibSettings.GetSettingValueByKey(_pinCodeData.Settings, SettingsConstants.S_MAX_LOGIN_ATTEMPTS_KEY), CultureInfo.InvariantCulture);
                _isDataFetched = true;
                return;
        }
        await NavigateToAsync(AppPermissions.LoginView.ToString()).ConfigureAwait(false);
    }

    private async Task OnSetButtonClickAsync()
    {
        Error = string.Empty;
        if (IsPinCodeValid())
        {
            SessionDTO sessionData = new SessionDTO
            {
                Session = new SessionModel { PinCode = _pinCode.ToString() },
                IsActive = _isPinCodeSetup
            };
            await SendServiceRequestAsync(_authService.VerifyPinCodeAsync(sessionData), sessionData).ConfigureAwait(true);
            switch (sessionData.ErrCode)
            {
                case ErrorCode.OK:
                    await ClearStoredDataAsync().ConfigureAwait(true);
                    await NavigateToAsync(AppPermissions.NavigationComponent.ToString(), true, null).ConfigureAwait(false);
                    break;
                case ErrorCode.HandledRedirection:
                    // No handling required
                    break;
                default:
                    await InvalidPinCodeAttemptCheckAsync(sessionData.ErrCode.ToString()).ConfigureAwait(true);
                    break;
            }
        }
    }

    private bool IsPinCodeValid()
    {
        if (IsValid())
        {
            if (_isPinCodeSetup)
            {
                if (_pinCode != _confirmPinCode)
                {
                    Error = ResourceConstants.R_PINCODE_NOT_MATCH_ERROR_KEY;
                    return false;
                }
                var isValidPinCode = _authService.ValidatePincodeStrength(_pinCode.ToString(),
                    LibSettings.GetSettingValueByKey(_pinCodeData.Settings, SettingsConstants.S_PINCODE_SEQUENCE_MATCH_REGEX_KEY),
                    LibSettings.GetSettingValueByKey(_pinCodeData.Settings, SettingsConstants.S_PINCODE_STRENGTH_MATCH_REGEX_KEY));
                if (isValidPinCode != ErrorCode.OK)
                {
                    Error = isValidPinCode.ToString();
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    private async Task InvalidPinCodeAttemptCheckAsync(string errorKey)
    {
        _wrongAttempts++;
        Error = LibResources.GetResourceValueByKey(_pinCodeData.Resources, errorKey)
            + $"{_wrongAttempts}{Constants.SYMBOL_SLASH}{_maxWrongAttempts}";
        if (_wrongAttempts >= _maxWrongAttempts)
        {
            await OnForgotPinCodeLinkClickAsync();
        }
    }

    private async Task OnForgotPinCodeLinkClickAsync()
    {
        await _authService.ClearAccountTokensAndIdAsync().ConfigureAwait(true);
        await NavigateToAsync(AppPermissions.LoginView.ToString(), true).ConfigureAwait(false);
    }

    private async Task ClearStoredDataAsync()
    {
        await _authService.DeleteSecuredValuesAsync(StorageConstants.SS_USER_NAME_KEY,StorageConstants.PR_USER_CRED_KEY).ConfigureAwait(false);
    }
}