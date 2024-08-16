using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class LoginBasePage : BasePage
{
    protected int _maxWrongAttempts;
    protected int _wrongAttemptCount;

    /// <summary>
    /// Parameterized constructor of base page which will render content based on received parameter value
    /// </summary>
    /// <param name="pageLayoutType"> Page layout type to decide layout style of phone and tablet/ipad</param>
    /// <param name="withScrollBar">Flag which decides scrollbar will be available outside layout or not</param>
    public LoginBasePage(PageLayoutType pageLayoutType, bool withScrollBar) : base(pageLayoutType, withScrollBar)
    {
    }

    /// <summary>
    /// Login service used to validate user data and create token
    /// </summary>
    /// <param name="accountData">Users login details to verify on server</param>
    /// <param name="isEnvironmentCheckRequired">flag which decides need to check environment or not</param>
    /// <returns>operation status of login</returns>
    public async Task LoginAsync(AuthDTO accountData, bool isEnvironmentCheckRequired)
    {
        if (isEnvironmentCheckRequired)
        {
            accountData.Settings = PageData.Settings;
        }
        CancellationTokenSourceInstance = new CancellationTokenSource();
        await new AuthService(App._essentials).LoginAsync(accountData, isEnvironmentCheckRequired
            , InvokeSyncActionAsync, CancellationTokenSourceInstance.Token).ConfigureAwait(true);
        if (await HandelLoginServiceResponseAsync(accountData).ConfigureAwait(false))
        {
            accountData.ErrCode = ErrorCode.HandledRedirection;
        }
    }

    private async Task<bool> HandelLoginServiceResponseAsync(AuthDTO accountData)
    {
        BasePage targetPage;
        switch (accountData.ErrCode)
        {
            case ErrorCode.SetPinCode:
                targetPage = new PincodePage(AppPermissions.PinCodeView.ToString(), false);
                break;
            case ErrorCode.SMSAuthentication:
                targetPage = new SMSValidationPage(accountData.AuthenticationData.UserName, accountData.AuthenticationData.AccountPassword, accountData.AuthenticationData.PhoneNo);
                break;
            case ErrorCode.SetNewPassword:
                await SetUserDetailsToSetPasswordAsync(accountData.AuthenticationData.EmailID
                    , accountData.AuthenticationData.PhoneNo).ConfigureAwait(true);
                await new AuthService(App._essentials).SaveSecuredValueAsync(StorageConstants.PR_USER_CRED_KEY
                    , accountData.AuthenticationData.AccountPassword).ConfigureAwait(true);
                targetPage = new ResetPasswordPage(Pages.SetNewPasswordPage);
                break;
            case ErrorCode.ResetPassword:
                await SetUserDetailsToSetPasswordAsync(accountData.AuthenticationData.EmailID
                    , accountData.AuthenticationData.PhoneNo).ConfigureAwait(true);
                targetPage = new ResetPasswordPage(Pages.ResetPasswordPage);
                break;
            case ErrorCode.LanguageNotAvailable:
                targetPage = new StaticMessagePage(ErrorCode.LanguageNotAvailable.ToString());
                break;
            case ErrorCode.UnknownCertificate:
            case ErrorCode.RecordCountMismatch:
                targetPage = new StaticMessagePage(accountData.ErrCode.ToString());
                break;
            case ErrorCode.HandledRedirection:
                return true;
            default:
                return false;
        }
        if (targetPage != null && targetPage != default)
        {
            await ShellMasterPage.CurrentShell.PushMainPageAsync(targetPage).ConfigureAwait(true);
        }
        return true;
    }

    protected int GetCodeLength(string settingKey)
    {
        string settingVal = LibSettings.GetSettingValueByKey(PageData?.Settings, settingKey);
        if (string.IsNullOrWhiteSpace(settingVal))
        {
            return 0;
        }
        return Convert.ToInt32(settingVal, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Update retry message and count
    /// </summary>
    /// <param name="error">error message</param>
    protected async Task UpdateRetryCountAsync(string error)
    {
        _wrongAttemptCount++;
        DisplayOperationStatus($"{LibResources.GetResourceValueByKey(PageData?.Resources, error.ToString())} {_wrongAttemptCount}/{_maxWrongAttempts}");
        if (_wrongAttemptCount == _maxWrongAttempts || error.ToEnum<ErrorCode>() == ErrorCode.AccountLockout)
        {
            await OnMaxWrongAttemptReachedAsync().ConfigureAwait(true);
        }
        else
        {
            AppHelper.ShowBusyIndicator = false;
        }
    }

    protected virtual async Task OnMaxWrongAttemptReachedAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Checks self registration allowed or not
    /// </summary>
    /// <returns></returns>
    public bool IsSelfRegistrationAllowed()
    {
        string enableRegistration = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_ENABLE_SELF_REGISTRATION_KEY);
        return !string.IsNullOrWhiteSpace(enableRegistration)
            && Convert.ToBoolean(enableRegistration, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Navigates to login page after cleaning user info
    /// </summary>
    /// <returns>Operation status</returns>
    public async Task NavigateToLoginPageAsync()
    {
        await ShellMasterPage.CurrentShell.NavigateToLoginPageAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Save details on local storage to use it during reset password
    /// </summary>
    /// <param name="emailID">Users email id</param>
    /// <param name="phoneNumber">Users phone number</param>
    /// <returns>Operation status</returns>
    protected async Task SetUserDetailsToSetPasswordAsync(string emailID, string phoneNumber)
    {
        await new BaseService(App._essentials).SaveSecuredValueAsync(StorageConstants.SS_USER_NAME_KEY, emailID).ConfigureAwait(false);
        await new BaseService(App._essentials).SaveSecuredValueAsync(StorageConstants.PR_PHONE_NUMBER_KEY, phoneNumber).ConfigureAwait(false);
    }
}
