using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Plugin.Fingerprint;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PincodePage))]
public class PincodePage : LoginBasePage
{
    private readonly AmhButtonControl _forgotPincodeLink;
    private readonly PincodeView _pincodeView;
    private readonly AppPermissions _pageName;
    private readonly bool _isForLockout;
    private bool _isPageCleared;
    private string _pincodeSet;
    private TaskCompletionSource<BaseDTO> _syncTaskCompletion;

    public PincodePage(string pageName, bool isForLockout) : base(PageLayoutType.LoginFlowPageLayout, false)
    {
        _isForLockout = isForLockout;
        _pageName = pageName.ToEnum<AppPermissions>();

        PageService = new AuthService(App._essentials);
        SetPageLayoutOption(LayoutOptions.CenterAndExpand, false);
        AddRowColumnDefinition(GridLength.Auto, 2, true);

        _pincodeView = new PincodeView(this);
        MasterGrid.Add(_pincodeView._buttonGrid, 0, 3);
        PageLayout.Add(_pincodeView._pincodeGrid, 0, 0);

        _forgotPincodeLink = new AmhButtonControl()
        {
            ResourceKey = ResourceConstants.R_FORGOT_PINCODE_LINK_KEY,
        };
    }

    /// <summary>
    /// On page appearing
    /// </summary>
    protected override async void OnAppearing()
    {
        if (!_isPageCleared)
        {
            await (PageService as AuthService).GetAccountDataAsync(PageData, false, _pageName.ToString()).ConfigureAwait(true);
            ApplyPageResources();
            Heading.Value = LibPermissions.GetFeatureText(PageData.FeaturePermissions, _pageName.ToString());

            _pincodeView.OnSubmitPincode += OnInputSubmited;
            _pincodeView._noOfDots = GetCodeLength(SettingsConstants.S_PINCODE_LENGTH_KEY);
            _maxWrongAttempts = GetCodeLength(SettingsConstants.S_MAX_LOGIN_ATTEMPTS_KEY);

            switch (_pageName)
            {
                case AppPermissions.PinCodeView:
                    StartInitialSync(true);
                    break;
                case AppPermissions.PincodeLoginView:
                    _forgotPincodeLink.PageResources = PageData;
                    PageLayout.Add(_forgotPincodeLink, 0, 1);
                    _forgotPincodeLink.OnValueChanged += OnForgotPincodeLinkClicked;
                    StartInitialSync(!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_INITIAL_SYNC_COMPLETE, false));

                    if (await CrossFingerprint.Current.IsAvailableAsync() &&
                       App._essentials.GetPreferenceValue(StorageConstants.PR_IS_BIOMETRIC_AUTH_PREFERRED_KEY, false))
                    {
                        _pincodeView.ResetPincodeGrid();
                        await _pincodeView.AuthenticateWithBiometricsAsync("").ConfigureAwait(true);
                    }
                    break;
            }

            if (_pincodeView._noOfDots < 1)
            {
                // if pincode length is <1 then skip set pincode flow
                await WaitAndNavigateAsync().ConfigureAwait(false);
            }
            else
            {
                _pincodeView.ResetPincodeGrid();
                await _pincodeView.RegisterClickEventAsync(_pageName == AppPermissions.PincodeLoginView).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
            }
        }
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
            //_pincodeView._pincodeGrid.Padding = new Thickness(0);
            await Task.Delay(10).ConfigureAwait(true);
            //_pincodeView._pincodeGrid.Padding = GetLayoutPadding(PageLayoutType.LoginFlowPageLayout);
        }
    }

    private async void OnInputSubmited(object sender, int actionIndex)
    {
        switch (actionIndex)
        {
            case 0:
                DisplayOperationStatus(string.Empty);
                //// Entered code of required length 
                if (_pageName == AppPermissions.PinCodeView)
                {
                    ////Set pincode
                    await SetPincodeAsync().ConfigureAwait(true);
                }
                else if (_pageName == AppPermissions.PincodeLoginView)
                {
                    ////Pincode login
                    await ValidatePincodeAsync(_pincodeView.Code, string.Empty, false).ConfigureAwait(true);
                }
                break;
            case 1:
                DisplayOperationStatus(string.Empty);
                //// Fingerprint or face Detection Success
                App._essentials.SetPreferenceValue(StorageConstants.PR_IS_BIOMETRIC_AUTH_PREFERRED_KEY, true);
                await WaitAndNavigateAsync().ConfigureAwait(false);
                break;
            default:
                //// Code/Fingerprint/Face Detection failed 
                AppHelper.ShowBusyIndicator = false;
                break;
        }
    }

    private async Task SetPincodeAsync()
    {
        if (string.IsNullOrWhiteSpace(_pincodeSet))
        {
            // Set Pincode 
            var isValidPincode = PageService.ValidatePincodeStrength(
                _pincodeView.Code,
                LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_PINCODE_SEQUENCE_MATCH_REGEX_KEY),
                LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_PINCODE_STRENGTH_MATCH_REGEX_KEY)
                    .Replace(Constants.SYMBOL_UNDERSCORE_STRING, Convert.ToString(_pincodeView._noOfDots - 1, CultureInfo.InvariantCulture)));
            if (isValidPincode == ErrorCode.OK)
            {
                _pincodeSet = _pincodeView.Code;
                // Render Confirm pincode UI
                RenderConfirmPincodePage();
            }
            else
            {
                _pincodeSet = string.Empty;
                _pincodeView.ResetPincodeGrid();
                DisplayOperationStatus(LibResources.GetResourceValueByKey(PageData?.Resources, isValidPincode.ToString()));
            }
            AppHelper.ShowBusyIndicator = false;
        }
        else
        {
            //Confirm Pincode
            DisplayOperationStatus(string.Empty);
            await ValidatePincodeAsync(_pincodeView.Code, _pincodeSet, true).ConfigureAwait(true);
        }
    }

    private async Task ValidatePincodeAsync(string pincode, string confirmPincode, bool isActive)
    {
        if (isActive)
        {
            // Validate pincode of confirm pincode page
            if (pincode.Equals(confirmPincode, StringComparison.InvariantCultureIgnoreCase))
            {
                await PageService.SaveSecuredValueAsync(StorageConstants.SS_PIN_CODE_KEY, pincode).ConfigureAwait(false);
                await WaitAndNavigateAsync().ConfigureAwait(true);
                OnSuccessfulValidation();
                return;
            }
        }
        else
        {
            // Validate pincode of pincode login page
            string storedPincode = await PageService.GetSecuredValueAsync(StorageConstants.SS_PIN_CODE_KEY).ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(storedPincode) && pincode == storedPincode)
            {
                // set the value of S_LAST_WRONG_LOGIN_DATETIME_KEY to empty to disable the 'UserLocked' popup
                App._essentials.SetPreferenceValue(StorageConstants.PR_LAST_WRONG_LOGIN_DATE_TIME_KEY, string.Empty);
                App._essentials.SetPreferenceValue(StorageConstants.PR_IS_BIOMETRIC_AUTH_PREFERRED_KEY, false);
                await WaitAndNavigateAsync().ConfigureAwait(true);
                OnSuccessfulValidation();
                return;
            }
        }
        _pincodeView.ResetPincodeGrid();
        if (isActive)
        {
            _pincodeSet = string.Empty;
            Heading.Value = LibPermissions.GetFeatureText(PageData.FeaturePermissions, AppPermissions.PinCodeView.ToString());
        }
        await UpdateRetryCountAsync(isActive ? ResourceConstants.R_PINCODE_NOT_MATCH_ERROR_KEY : ErrorCode.InvalidPincode.ToString()).ConfigureAwait(true);
    }

    private void RenderConfirmPincodePage()
    {
        _pincodeView.ResetPincodeGrid();
        DisplayOperationStatus(string.Empty);
        PageLayout.TranslateTo(-PageLayout.Width, PageLayout.Y, 450, Easing.CubicOut);
        PageLayout.FadeTo(0, 200);
        Heading.Value = LibResources.GetResourceValueByKey(PageData.Resources, ResourceConstants.R_CONFIRM_PINCODE_KEY);
        PageLayout.TranslationX = PageLayout.Width;
        //Slide Left (In view)
        PageLayout.FadeTo(1, 100);
        PageLayout.TranslateTo(0, 0, 225, Easing.CubicIn);
    }

    protected async void OnForgotPincodeLinkClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        _isPageCleared = true;
        ShellMasterPage.CurrentShell.ClearShellContent();
        await NavigateToLoginPageAsync().ConfigureAwait(true);
    }

    protected override async Task OnMaxWrongAttemptReachedAsync()
    {
        await NavigateToLoginPageAsync().ConfigureAwait(true);
    }

    protected async void OnSuccessfulValidation()
    {
        await ((App)Application.Current).SetupSignalRAsync(!_isForLockout).ConfigureAwait(true);
    }

    protected void StartInitialSync(bool isAwaitable)
    {
        // Start Syncing service in parallel thread while loading page
        if (isAwaitable)
        {
            // create instance of task result to await before navigating to next page
            _syncTaskCompletion = new TaskCompletionSource<BaseDTO>();
            _ = Task.Run(async () =>
            {
                // Start Syncing service in parallel thread
                _syncTaskCompletion.SetResult(await SyncDataWithServerAsync(Pages.PincodePage, isAwaitable, default).ConfigureAwait(false));
            });
        }
        else
        {
            // Start Syncing service in parallel thread
            _ = SyncDataWithServerAsync(Pages.PincodeLoginPage, isAwaitable, default);
        }
    }

    protected async Task WaitAndNavigateAsync()
    {
        BaseDTO syncResult = new BaseDTO();
        if (_syncTaskCompletion != null)
        {
            syncResult = await _syncTaskCompletion.Task.ConfigureAwait(true);
            switch (syncResult.ErrCode)
            {
                case ErrorCode.OK:
                    App._essentials.SetPreferenceValue(StorageConstants.PR_IS_INITIAL_SYNC_COMPLETE, true);
                    break;
                case ErrorCode.Unauthorized:
                case ErrorCode.TokenExpired:
                    if (!(ShellMasterPage.CurrentShell.MainPage is LoginPage))
                    {
                        await DisplayMessagePopupAsync(syncResult.ErrCode.ToString(), OnPupupActionClicked).ConfigureAwait(true);
                        await NavigateToLoginPageAsync().ConfigureAwait(true);
                        syncResult.ErrCode = ErrorCode.HandledRedirection;
                    }
                    break;
                case ErrorCode.HandledRedirection:
                    break;
                default:
                    await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(syncResult.ErrCode.ToString())).ConfigureAwait(true);
                    syncResult.ErrCode = ErrorCode.HandledRedirection;
                    break;

            }
        }

        if (syncResult.ErrCode != ErrorCode.HandledRedirection)
        {
            await NavigateOnNextPageAsync(_isForLockout, _pageName != AppPermissions.PincodeLoginView, LoginFlow.Pincode).ConfigureAwait(false);
        }
    }
}