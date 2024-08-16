using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class ForgotPasswordPage
{
    private readonly AuthDTO _authData = new AuthDTO { AuthenticationData = new AuthModel(), IsActive = false };
    private bool _shouldShowLink;
    private long _existingOrganizationID;
    private AuthService _authService;

    protected override async Task OnInitializedAsync()
    {
        _authService = new AuthService(AppState.webEssentials);
        _shouldShowLink = !string.IsNullOrWhiteSpace(await _authService.GetSecuredValueAsync(StorageConstants.SS_USER_NAME_KEY).ConfigureAwait(true))
            && !string.IsNullOrWhiteSpace(await _authService.GetSecuredValueAsync(StorageConstants.PR_PHONE_NUMBER_KEY).ConfigureAwait(true));
        await SendServiceRequestAsync(_authService.GetAccountDataAsync(_authData), _authData).ConfigureAwait(true);
        _isDataFetched = true;
        _existingOrganizationID = AppState.webEssentials.GetPreferenceValue(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, Constants.DEFAULT_ORGANISATION_ID);
    }

    private async Task OnAlreadyHaveAccountClickAsync()
    {
        await NavigateToAsync(AppPermissions.LoginView.ToString()).ConfigureAwait(false);
    }

    private async Task OnAlreadyHaveCodeClickAsync()
    {
        await NavigateToAsync(AppPermissions.ResetPasswordView.ToString()).ConfigureAwait(false);
    }

    private async Task OnSendClickAsync()
    {
        if (IsValid())
        {
            AuthDTO accountData = new AuthDTO
            {
                AuthenticationData = new AuthModel
                {
                    EmailID = _authData.AuthenticationData.EmailID.Trim().ToLowerInvariant(),
                    PhoneNo = _authData.AuthenticationData.PhoneNo.Trim()
                },
                Settings = _authData.Settings
            };
            await SendServiceRequestAsync(_authService.ForgotPasswordAsync(accountData, null, true), accountData).ConfigureAwait(true);
            if (accountData.ErrCode == ErrorCode.SMSAuthentication || accountData.ErrCode == ErrorCode.ResetPassword)
            {
                if (_existingOrganizationID != accountData.OrganisationID)
                {
                    AppState.MasterData.OrganisationID = accountData.OrganisationID;
                    await LoadMasterPageDataAsync(accountData.AccountID);
                    await SaveSecureStorageValuesAsync(accountData);
                    await NavigateToAsync(AppPermissions.ResetPasswordView.ToString(), true).ConfigureAwait(true);
                }
                else
                {
                    await SaveSecureStorageValuesAsync(accountData);
                    await NavigateToAsync(AppPermissions.ResetPasswordView.ToString(), true).ConfigureAwait(false);
                }
            }
            else
            {
                Error = accountData.ErrCode.ToString();
            }
        }
    }

    private async Task SaveSecureStorageValuesAsync(AuthDTO accountData)
    {
        await _authService.SaveSecuredValueAsync(StorageConstants.SS_USER_NAME_KEY, accountData.AuthenticationData.EmailID).ConfigureAwait(true);
        await _authService.SaveSecuredValueAsync(StorageConstants.PR_PHONE_NUMBER_KEY, accountData.AuthenticationData.PhoneNo).ConfigureAwait(true);
    }
}