using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class ForgotPasswordPage : LoginBasePage
{
    private readonly AmhEntryControl _emailEntry;
    private readonly AmhMobileNumberControl _phoneNumberEntry;
    private readonly AmhButtonControl _sendButton;
    private readonly AmhButtonControl _alreadyHaveCodeButton;
    private readonly AmhButtonControl _alreadyHaveAccountButton;

    public ForgotPasswordPage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        _emailEntry = new AmhEntryControl()
        {
            ResourceKey = ResourceConstants.R_EMAIL_ADDRESS_KEY,
            Icon = ImageConstants.I_EMAIL_ICON_PNG,
        };
        _phoneNumberEntry = new AmhMobileNumberControl()
        {
            ResourceKey = ResourceConstants.R_MOBILE_NUMBER_KEY
        };
        _sendButton = new AmhButtonControl() 
        {
            ResourceKey = ResourceConstants.R_SEND_ACTION_KEY,
        };
        _alreadyHaveAccountButton = new AmhButtonControl() 
        {
            ResourceKey = ResourceConstants.R_ALREADY_HAVE_LOGIN_ACTION_KEY
        };
        _alreadyHaveCodeButton = new AmhButtonControl() 
        {
            ResourceKey = ResourceConstants.R_ALREADY_HAVE_CODE_ACTION_KEY
        };
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.StartAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        AddRowColumnDefinition(GridLength.Auto, 7, true);
        PageLayout.Add(_emailEntry, 0, 1);
        PageLayout.Add(_phoneNumberEntry, 0, 2);
        PageLayout.Add(_sendButton, 0, 3);
        PageLayout.Add(_alreadyHaveAccountButton, 0, 6);
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        AuthService authService = new AuthService(App._essentials);
        if (!string.IsNullOrWhiteSpace(await authService.GetSecuredValueAsync(StorageConstants.SS_USER_NAME_KEY).ConfigureAwait(true))
            && !string.IsNullOrWhiteSpace(await authService.GetSecuredValueAsync(StorageConstants.PR_PHONE_NUMBER_KEY).ConfigureAwait(true)))
        {
            PageLayout.Add(_alreadyHaveCodeButton, 0, 4);
            PageLayout.Add(new AmhHorizontalRuleControl(), 0, 5);
        }
        await authService.GetAccountDataAsync(PageData, false, AppPermissions.ForgotPasswordView.ToString());
        ApplyPageResources();
        await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true);
        _phoneNumberEntry.PageResources = PageData;
        _emailEntry.PageResources = PageData;
        _sendButton.PageResources = _alreadyHaveAccountButton.PageResources = _alreadyHaveCodeButton.PageResources = PageData;
        Heading.Value = LibPermissions.GetFeatureText(PageData.FeaturePermissions, AppPermissions.ForgotPasswordView.ToString());
        _sendButton.OnValueChanged += OnSendButtonClicked;
        _alreadyHaveCodeButton.OnValueChanged += OnAlreadyHaveCodeButtonClicked;
        _alreadyHaveAccountButton.OnValueChanged += OnAlreadyHaveAccountButtonClicked;
        AppHelper.ShowBusyIndicator = false;
    }

    protected override void OnDisappearing()
    {
        _sendButton.OnValueChanged -= OnSendButtonClicked;
        _alreadyHaveCodeButton.OnValueChanged -= OnAlreadyHaveCodeButtonClicked;
        _alreadyHaveAccountButton.OnValueChanged -= OnAlreadyHaveAccountButtonClicked;
        base.OnDisappearing();
    }

    private async void OnSendButtonClicked(object sender, EventArgs e)
    {
        if (await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true) /*&& IsFormValid()*/)
        {
            AppHelper.ShowBusyIndicator = true;
            AuthDTO accountData = new AuthDTO
            {
                AuthenticationData = new AuthModel
                {
                    EmailID = (_emailEntry.Value as string).Trim(),
                    PhoneNo = (_phoneNumberEntry.Value as string).Trim()
                },
                Settings = PageData.Settings
            };
            CancellationTokenSourceInstance = new CancellationTokenSource();
            await new AuthService(App._essentials).ForgotPasswordAsync(accountData, InvokeSyncActionAsync, true).ConfigureAwait(true);
            if (accountData.ErrCode == ErrorCode.SMSAuthentication || accountData.ErrCode == ErrorCode.ResetPassword)
            {
                await SetUserDetailsToSetPasswordAsync((_emailEntry.Value as string).Trim(), (_phoneNumberEntry.Value as string).Trim()).ConfigureAwait(true);
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new ResetPasswordPage(Pages.ResetPasswordPage)).ConfigureAwait(false);
            }
            else
            {
                DisplayOperationStatus(LibResources.GetResourceValueByKey(PageData?.Resources, accountData.ErrCode.ToString()));
                AppHelper.ShowBusyIndicator = false;
            }
        }
    }
    private async void OnAlreadyHaveCodeButtonClicked(object sender, EventArgs e)
    {
        if (await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;
            await ShellMasterPage.CurrentShell.PushMainPageAsync(new ResetPasswordPage(Pages.ResetPasswordPage)).ConfigureAwait(false);
        }
    }

    private async void OnAlreadyHaveAccountButtonClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
    }
}