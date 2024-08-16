using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class RegistrationPage : BasePage
{
    private readonly UserDTO _userData = new UserDTO { IsActive = false };
    private string _confirmPassword;
    private bool _showOtpPopup;
    private int _maxWrongAttempts;
    private int _wrongAttempts;
    private string _wrongAttemptsString;

    protected override async Task OnInitializedAsync()
    {
        _userData.User = new UserModel();
        await SendServiceRequestAsync(new AuthService(AppState.webEssentials).GetAccountDataAsync(_userData), _userData).ConfigureAwait(true);
        if (_userData.ErrCode == ErrorCode.OK)
        {
            _confirmPassword = _userData.User.AccountPassword;
            _maxWrongAttempts = Convert.ToInt32(LibSettings.GetSettingValueByKey(_userData.Settings, SettingsConstants.S_MAX_LOGIN_ATTEMPTS_KEY), CultureInfo.InvariantCulture);
        }
        _isDataFetched = true;
    }

    private async Task OnVerifyOtpClickAsync(string otp) 
    {
        await RegisterUserAsync();
    }

    private async Task RegisterUserAsync()
    {
        Error = Success = string.Empty;
        _wrongAttemptsString = string.Empty;
        if (IsValid())
        {
            if (_userData.User.AccountPassword != _confirmPassword)
            {
                Error = ResourceConstants.R_PASSWORD_MISMATCH_ERROR_KEY;
                return;
            }
            UserDTO userData = await MapAndSaveUserDataAsync(_userData.User.Otp, true);
            _userData.ErrCode = userData.ErrCode;
            if (userData.ErrCode == ErrorCode.SMSAuthentication)
            {
                _showOtpPopup = true;
            }
            else
            {
                if (_userData.ErrCode != ErrorCode.OK && _userData.ErrCode != ErrorCode.SMSAuthentication &&
                  _userData.ErrCode != ErrorCode.SetPinCode && _userData.ErrCode != ErrorCode.OrganisationSetup && _userData.ErrCode != ErrorCode.HandledRedirection)
                {
                    Error = _userData.ErrCode.ToString();
                    if (!string.IsNullOrWhiteSpace(userData.User.Otp))
                    {
                        InvalidOtpAttempt();
                    }
                }
            }
        }
    }

    private async Task OnResendOtpClickAsync(BaseDTO baseData)
    {
        UserDTO userData = await MapAndSaveUserDataAsync(string.Empty, false);
        baseData.ErrCode = userData.ErrCode;
    }

    private async Task<UserDTO> MapAndSaveUserDataAsync(string otp, bool isUser)
    {
        UserDTO userData = new UserDTO
        {
            User = new UserModel
            {
                FirstName = _userData.User.FirstName?.Trim(),
                LastName = _userData.User.LastName?.Trim(),
                EmailId = _userData.User.EmailId?.Trim(),
                PhoneNo = _userData.User.PhoneNo?.Trim(),
                AccountPassword = _userData.User.AccountPassword?.Trim(),
                Otp = otp,
                RememberMe = false,
                IsSelfRegistration = true,
                IsUser = isUser,
            }
        };
        await SendServiceRequestAsync(new UserService(AppState.webEssentials).RegisterUserAsync(userData), userData).ConfigureAwait(true);
        return userData;
    }

    private void InvalidOtpAttempt()
    {
        _wrongAttempts++;
        _wrongAttemptsString = $"{_wrongAttempts}{Constants.SYMBOL_SLASH}{_maxWrongAttempts}";
        if (_wrongAttempts >= _maxWrongAttempts)
        {
            _wrongAttempts = 0;
            _userData.User.Otp = string.Empty;
            _showOtpPopup = false;
        }
    }

    private async Task OnLoginViewClickAsync(object s)
    {
        await NavigateToAsync(AppPermissions.LoginView.ToString()).ConfigureAwait(false);
    } 
}