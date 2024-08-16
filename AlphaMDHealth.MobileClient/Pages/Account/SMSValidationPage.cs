using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(SMSValidationPage))]
public class SMSValidationPage : LoginBasePage
{
    private readonly string _username;
    private readonly string _password;
    private readonly string _phoneNumber;
    private readonly PincodeView _pincodeView;
    private readonly AmhLabelControl _otpInfo;
    private readonly AmhLabelControl _resendOtpLink;
    private Timer _timer;
    private string _timerString = string.Empty;
    private string[] _timerContent;

    public SMSValidationPage(string username, string password, string phoneNo) : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        _username = username;
        _password = password;
        _phoneNumber = phoneNo;

        PageService = new AuthService(App._essentials);
        SetPageLayoutOption(LayoutOptions.CenterAndExpand, false);
        AddRowColumnDefinition(GridLength.Auto, 2, true);

        _pincodeView = new PincodeView(this);
        MasterGrid.Add(_pincodeView._buttonGrid, 0, 3);
        PageLayout.Add(_pincodeView._pincodeGrid, 0, 0);

        _otpInfo = new AmhLabelControl(FieldTypes.PrimarySmallHVCenterLabelControl)
        {
            ResourceKey = ResourceConstants.R_OTP_CODE_COUNTDOWN_MESSAGE_KEY,
            HorizontalOptions = LayoutOptions.Center
        };

        _resendOtpLink = new AmhLabelControl()
        {
            ResourceKey = ResourceConstants.R_RESEND_OTP_ACTION_KEY,
        };
    }

    /// <summary>
    /// On page appearing
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await (PageService as AuthService).GetAccountDataAsync(PageData, false, AppPermissions.SMSAuthenticationView.ToString(), AppPermissions.PinCodeView.ToString()).ConfigureAwait(true);
        ApplyPageResources();
        Heading.Value = LibPermissions.GetFeatureText(PageData.FeaturePermissions, AppPermissions.SMSAuthenticationView.ToString());
       
        _pincodeView.OnSubmitPincode += OnInputSubmited;
        _pincodeView._noOfDots = GetCodeLength(SettingsConstants.S_OTP_LENGTH_KEY);
        _maxWrongAttempts = GetCodeLength(SettingsConstants.S_MAX_LOGIN_ATTEMPTS_KEY);
        _otpInfo.PageResources = _resendOtpLink.PageResources = PageData;
        StartResendSmsTimer();
        _pincodeView.ResetPincodeGrid();
        await _pincodeView.RegisterClickEventAsync(false).ConfigureAwait(true);
        _resendOtpLink.OnValueChanged += OnResendOtpButtonClick;
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// On page disappearing
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _pincodeView.UnRegisterButtonEvents();
        _pincodeView.OnSubmitPincode -= OnInputSubmited;
        _resendOtpLink.OnValueChanged -= OnResendOtpButtonClick;
    }

    /// <summary>
    /// OnSizeAllocated
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    protected async override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (MobileConstants.IsTablet)
        {
            SetOrientation();
            await Task.Delay(10).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Start resend sms timer
    /// </summary>
    protected void StartResendSmsTimer()
    {
        DisplayTimerString(true);
        _timerContent = LibResources.GetResourceValueByKey(PageData.Resources, ResourceConstants.R_OTP_CODE_COUNTDOWN_MESSAGE_KEY).Split("{0}");
        StartResendOtpTimer();

    }

    private void StartResendOtpTimer()
    {
        int count = Convert.ToInt16(LibSettings.GetSettingValueByKey(PageData.Settings, SettingsConstants.S_OTP_RESEND_DURATION_KEY), CultureInfo.InvariantCulture) * 60;
        _timerString = count.ToString(CultureInfo.InvariantCulture);
        _timer = new Timer(new TimerCallback(_ =>
        {
            if (count <= 0)
            {
                DisplayTimerString(false);
                _timer.Dispose();
                return;
            }
            count--;
            _timerString = count > 0 ? count.ToString(CultureInfo.InvariantCulture) : null;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _otpInfo.Value = _timerContent[0] + _timerString + _timerContent[1];
            });
        }), null, 1000, 1000);
    }

    private void DisplayTimerString(bool shouldDisplay)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (shouldDisplay)
            {
                if (PageLayout.Contains(_resendOtpLink)) { PageLayout.Remove(_resendOtpLink); }
                if (!PageLayout.Contains(_otpInfo)) { PageLayout.Add(_otpInfo, 0, 1); }
            }
            else
            {
                if (PageLayout.Contains(_otpInfo)) { PageLayout.Remove(_otpInfo); }
                if (!PageLayout.Contains(_resendOtpLink)) { PageLayout.Add(_resendOtpLink, 0, 1); }
            }
        });
    }

    protected override async Task OnMaxWrongAttemptReachedAsync()
    {
        await NavigateToLoginPageAsync().ConfigureAwait(true);
    }

    private async void OnInputSubmited(object sender, int actionIndex)
    {
        DisplayOperationStatus(string.Empty);
        if (await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;
            AuthDTO userDetails = new AuthDTO
            {
                AuthenticationData = new AuthModel { UserName = _username, EmailID = _username, PhoneNo = _phoneNumber, AccountPassword = _password, RememberMe = true, Otp = _pincodeView.Code, }
            };
            await LoginAsync(userDetails, false).ConfigureAwait(true);
            switch (userDetails.ErrCode)
            {
                case ErrorCode.HandledRedirection:
                    //// Redirection already handelled
                    break;
                case ErrorCode.OK:
                    //todo:
                    //StartInitialSync(true);
                    //await WaitAndNavigateAsync().ConfigureAwait(false);
                    return;
                case ErrorCode.Unauthorized:
                case ErrorCode.AccountLockout:
                    await NavigateToLoginPageAsync().ConfigureAwait(true);
                    break;
                default:
                    _pincodeView.ResetPincodeGrid();
                    await UpdateRetryCountAsync(userDetails.ErrCode.ToString()).ConfigureAwait(true);
                    break;
            }
        }
        else
        {
            _pincodeView.ResetPincodeGrid();
            AppHelper.ShowBusyIndicator = false;
        }
    }

    protected async void OnResendOtpButtonClick(object sender, EventArgs e)
    {
        DisplayOperationStatus(string.Empty);
        if (await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;
            AuthDTO returnResult = new AuthDTO
            {
                AuthenticationData = new AuthModel { UserName = _username }
            };
            await new AuthService(App._essentials).ResendSmsAsync(returnResult).ConfigureAwait(true);
            if (returnResult.ErrCode == ErrorCode.OK)
            {
                StartResendSmsTimer();
                AppHelper.ShowBusyIndicator = false;
            }
            else
            {
                DisplayOperationStatus(LibResources.GetResourceValueByKey(PageData?.Resources, ErrorCode.InvalidData.ToString()));
            }
        }
    }
}