using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class LoginPage : IDisposable
{
    private readonly AuthDTO _authData = new AuthDTO { IsActive = false };
    private int _maxWrongAttempts;
    private int _wrongAttempts;
    private bool _showLockPopup;
    private string _timerString;
    private string[] _popupContent;
    private Timer _timer;
    private bool _showRegistration;
    private long _existingOrganisationID;
    private bool _showPage = true;

    protected override async Task OnInitializedAsync()
    {
        _authData.AuthenticationData = new AuthModel();
        var authService = new AuthService(AppState.webEssentials);
        await SendServiceRequestAsync(authService.GetAccountDataAsync(_authData), _authData).ConfigureAwait(true);
        _maxWrongAttempts = Convert.ToInt32(LibSettings.GetSettingValueByKey(_authData.Settings, SettingsConstants.S_MAX_LOGIN_ATTEMPTS_KEY), CultureInfo.InvariantCulture);
        _showRegistration = Convert.ToBoolean(LibSettings.GetSettingValueByKey(AppState.MasterData.Settings, SettingsConstants.S_ENABLE_SELF_REGISTRATION_KEY));
        _existingOrganisationID = (AppState.webEssentials).GetPreferenceValue(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, Constants.DEFAULT_ORGANISATION_ID);
        _authData.AddedBy = await authService.GetSecuredValueAsync(StorageConstants.PR_DYNAMIC_LINK_DATA_KEY);
        if (!string.IsNullOrWhiteSpace(_authData.AddedBy))
        {
            AppState.webEssentials.DeleteSecureStorageValue(StorageConstants.PR_DYNAMIC_LINK_DATA_KEY);
            authService.GetDynamicLinkData(_authData);
            if (_authData.ErrCode == ErrorCode.OK)
            {
                await UserLoginAsync(_authData).ConfigureAwait(true);
                _authData.ErrCode = ErrorCode.OK;
            }
        }
        _isDataFetched = true;
    }

    private async Task OnRegisterButtonClickAsync()
    {
        await NavigateToAsync(AppPermissions.RegistrationView.ToString()).ConfigureAwait(false);
    }

    private async Task OnForgotPasswordLinkClickAsync()
    {
        await NavigateToAsync(AppPermissions.ForgotPasswordView.ToString()).ConfigureAwait(false);
    }

    private void OnRememberMeClicked(string val)
    {
        _authData.AuthenticationData.RememberMe = !string.IsNullOrWhiteSpace(val) && Convert.ToInt32(val) == 1;
    }

    private async Task OnLoginButtonClickAsync()
    {
        Error = string.Empty;
        if (IsValid())
        {
            AuthDTO authData = new AuthDTO
            {
                AuthenticationData = new AuthModel
                {
                    UserName = _authData.AuthenticationData.UserName.Trim().ToLowerInvariant(),
                    AccountPassword = _authData.AuthenticationData.AccountPassword.Trim(),
                    RememberMe = _authData.AuthenticationData.RememberMe
                }
            };
            await UserLoginAsync(authData).ConfigureAwait(false);
        }
    }

    private async Task UserLoginAsync(AuthDTO authData)
    {
        await SendServiceRequestAsync(new AuthService(AppState.webEssentials).LoginAsync(authData, true, null, CancellationToken.None), authData).ConfigureAwait(true);
        _authData.ErrCode = authData.ErrCode;
        await HandleErrorcode(authData).ConfigureAwait(false);
    }

    private async Task HandleErrorcode(AuthDTO authData)
    {
        var isOrganizationSwitched = _existingOrganisationID != authData.OrganisationID && authData.AccountID > 0;
        switch (_authData.ErrCode)
        {
            case ErrorCode.InvalidEnvironment:
            case ErrorCode.InvalidData:
                InvalidLoginAttemptCheck(authData.ErrCode);
                break;
            case ErrorCode.Unauthorized:
            case ErrorCode.Forbidden:
                Error = authData.ErrCode.ToString();
                break;
            case ErrorCode.AccountLockout:
                DisableLogin(TimeSpan.FromSeconds(authData.RecordCount));
                break;
            case ErrorCode.OK:
                await ReloadMaster(authData, isOrganizationSwitched);
                await NavigateToAsync(AppPermissions.NavigationComponent.ToString(), true).ConfigureAwait(false);
                break;
            case ErrorCode.SetPinCode:
                await ReloadMaster(authData, isOrganizationSwitched);
                await NavigateToAsync(AppPermissions.PinCodeView.ToString(), true).ConfigureAwait(false);
                break;
            case ErrorCode.SMSAuthentication:
                _ = RefreshAndNavigateToAsync(authData, isOrganizationSwitched, AppPermissions.SMSAuthenticationView.ToString());
                break;
            case ErrorCode.SetNewPassword:
                await ReloadMaster(authData, isOrganizationSwitched);
                await SetLoginData(authData).ConfigureAwait(true);
                await NavigateToAsync(AppPermissions.SetNewPasswordView.ToString(), true).ConfigureAwait(false);
                break;
            default:
                //Left for further implementation
                break;
        }
    }

    private async Task ReloadMaster(AuthDTO authData, bool isOrganizationSwitched)
    {
        if (isOrganizationSwitched)
        {
            await LoadMasterPageDataAsync(authData.AccountID).ConfigureAwait(true);
        }
    }

    private List<OptionModel> GetRememberMeOption()
    {
        return new List<OptionModel> {
            new OptionModel { OptionID = 1, OptionText = LibResources.GetResourceValueByKey(_authData.Resources, ResourceConstants.R_REMEMBER_ME_KEY) }
        };
    }

    private void InvalidLoginAttemptCheck(ErrorCode serviceError)
    {
        _wrongAttempts++;
        Error = LibResources.GetResourceValueByKey(_authData.Resources, serviceError.ToString())
            + $" {_wrongAttempts}{Constants.SYMBOL_SLASH}{_maxWrongAttempts}";
        if (_wrongAttempts == _maxWrongAttempts || serviceError == ErrorCode.AccountLockout)
        {
            AppState.webEssentials.SetPreferenceValue(StorageConstants.PR_LAST_WRONG_LOGIN_DATE_TIME_KEY, GenericMethods.GetUtcDateTime.ToString(CultureInfo.InvariantCulture));
            DisableLogin(TimeSpan.FromMinutes(Convert.ToInt32(LibSettings.GetSettingValueByKey(_authData.Settings, SettingsConstants.S_LOGIN_LOCKOUT_DURATION_KEY), CultureInfo.InvariantCulture)));
            Error = string.Empty;
        }
    }

    private void DisableLogin(TimeSpan disableLoginTime)
    {
        if (disableLoginTime > TimeSpan.Zero)
        {
            _popupContent = LibResources.GetResourcePlaceHolderByKey(_authData.Resources, ErrorCode.AccountLockout.ToString()).Split("{0}");
            _showLockPopup = true;
            int count = (int)disableLoginTime.TotalSeconds;
            _timerString = count.ToString(CultureInfo.InvariantCulture);
            _timer = new Timer(new TimerCallback(_ =>
            {
                if (count <= 0)
                {
                    return;
                }
                count--;
                if (count > 0)
                {
                    _timerString = count.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    _showLockPopup = false;
                    _wrongAttempts = 0;
                    _timerString = string.Empty;
                }
                InvokeAsync(() => StateHasChanged());
            }), null, 1000, 1000);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_timer != null)
        {
            _timer.Dispose();
        }
    }
}