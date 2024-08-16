using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.WebUtilities;

namespace AlphaMDHealth.WebClient;

public partial class DynamicLinkPage : BasePage
{
    protected override async Task OnInitializedAsync()
    {
        var authService = new AuthService(AppState.webEssentials);
        await authService.ResetUserDataAsync().ConfigureAwait(true);

        Uri uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);

        var parameters = QueryHelpers.ParseQuery(uri.Query);
        if (parameters.TryGetValue(nameof(TempSessionModel.TempToken), out var tempToken)
            && parameters.TryGetValue(nameof(TempSessionModel.TokenIdentifier), out var clientItentifier))
        {
            await authService.SaveSecuredValueAsync(nameof(TempSessionModel.TempToken), tempToken.ToString());
            await authService.SaveSecuredValueAsync(nameof(TempSessionModel.TokenIdentifier), clientItentifier.ToString());

            parameters.TryGetValue("LastPageBeforeExpiry", out var lastPageBeforeExpiry);
            await authService.SaveSecuredValueAsync("LastPageBeforeExpiry", lastPageBeforeExpiry.ToString() ?? string.Empty);

            AuthDTO authData = new AuthDTO
            {
                TempSession = new TempSessionModel
                {
                    TempToken = tempToken,
                    TokenIdentifier = clientItentifier,
                }
            };
            await SendServiceRequestAsync(authService.LoginWithTempTokenAsync(authData), authData).ConfigureAwait(true);
            if (authData.ErrCode == ErrorCode.OK)
            {
                AppState.webEssentials.SetPreferenceValue(nameof(MasterDTO.HasWelcomeScreens), 1);
                await NavigateToAsync(AppPermissions.NavigationComponent.ToString(), true).ConfigureAwait(false);
            }
            return;
        }
        else
        {
            if (parameters.TryGetValue(Constants.ONELINK_PARAMETER_SEPERATOR_KEY, out var oneLink))
            {
                await authService.SaveSecuredValueAsync(StorageConstants.PR_DYNAMIC_LINK_DATA_KEY, oneLink.ToString());
            }
        }
        await NavigateToAsync(AppPermissions.LoginView.ToString(), true).ConfigureAwait(true);
        _isDataFetched = true;
        StateHasChanged();
    }
}