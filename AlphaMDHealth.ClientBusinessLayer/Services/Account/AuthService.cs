using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public class AuthService : BaseService
{
    private readonly EnvironmentService environmentService;

    /// <summary>
    /// Authentication service
    /// </summary>
    public AuthService(IEssentials serviceEssentials = null) : base(serviceEssentials)
    {
        environmentService = new EnvironmentService(_essentials);
        GenericMethods._essentials = serviceEssentials;
    }

    /// <summary>
    /// Refreshes the access token and refresh token using the current refresh token
    /// </summary>
    /// <param name="responseData">Response data</param>
    /// <returns>operation status</returns>
    public async Task RefreshTokenAsync(BaseDTO responseData)
    {
        try
        {
            string refreshToken = await GetSecuredValueAsync(StorageConstants.SS_REFRESH_TOKEN_KEY).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                responseData.ErrCode = ErrorCode.TokenExpired;
            }
            else
            {
                var httpData = new HttpServiceModel<BaseDTO>
                {
                    PathWithoutBasePath = UrlConstants.GET_TOKEN_ASYNC_PATH,
                    ContentToSend = new SessionDTO { Session = new SessionModel { RefreshToken = refreshToken } },
                    AuthType = AuthorizationType.Basic
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                JToken data = JToken.Parse(httpData.Response);
                responseData.ErrCode = httpData.ErrCode;
                if (responseData.ErrCode == ErrorCode.OK && data != null && data.HasValues)
                {
                    responseData.Response = httpData.Response;
                    await SaveTokensAsync(MapSessionData(responseData)).ConfigureAwait(false);
                }
                else
                {
                    responseData.ErrCode = ErrorCode.TokenExpired;
                }
            }
        }
        catch (Exception ex)
        {
            responseData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Get login page data
    /// </summary>
    /// <param name="authData">Login data</param>
    /// <param name="isBearer">Flag to authenticate token</param>
    /// <returns>Country codes, settings and resources</returns>
    public async Task GetAccountDataAsync(BaseDTO authData, bool isBearer = false, params string[] features)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await Task.WhenAll(
                    GetSettingsAsync(GroupConstants.RS_ENVIRONMENT_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, GroupConstants.RS_COMMON_ORGANISATION_SETTINGS_GROUP, GroupConstants.RS_COMMON_GROUP),
                    GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_LOGIN_FLOW_PAGE_GROUP),
                    new CountryDatabase().GetCountriesAsync(authData),
                    GetFeaturesAsync(features)
                ).ConfigureAwait(false);
                authData.Resources = PageData.Resources;
                authData.Settings = PageData.Settings;
                authData.FeaturePermissions = PageData.FeaturePermissions;
                UpdateMaxMinLength(authData, SettingsConstants.S_OTP_LENGTH_KEY, ResourceConstants.R_VERIFICATION_KEY);
            }
            else
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    AuthType = isBearer ? AuthorizationType.Bearer : AuthorizationType.Basic,
                    PathWithoutBasePath = isBearer ? UrlConstants.GET_CHANGE_PASSWORD_DATA_ASYNC_PATH : UrlConstants.GET_ACCOUNT_DATA_ASYNC_PATH
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                authData.ErrCode = httpData.ErrCode;
                if (authData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(authData, data);
                        authData.CountryCodes = new CountryService(_essentials).MapCountryCodes(data);
                        UpdateMaxMinLength(authData, SettingsConstants.S_OTP_LENGTH_KEY, ResourceConstants.R_VERIFICATION_KEY);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            authData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Check the user's last login DateTime and block accordingly
    /// </summary>
    /// <returns>returns TimeSpan for blockedstatus</returns>
    public async Task<TimeSpan> CheckBlockedStatusAsync()
    {
        string lastWrongLogin = _essentials.GetPreferenceValue<string>(StorageConstants.PR_LAST_WRONG_LOGIN_DATE_TIME_KEY, null);
        if (lastWrongLogin != null && !string.IsNullOrWhiteSpace(lastWrongLogin))
        {
            return TimeSpan.FromMinutes(
                Convert.ToDouble(
                    await new SettingService(_essentials).GetSettingsValueByKeyAsync(SettingsConstants.S_LOGIN_LOCKOUT_DURATION_KEY).ConfigureAwait(false)
                    , new CultureInfo(Constants.ENGLISH_US_LOCALE))
                ) - GenericMethods.GetUtcDateTime.Subtract(DateTime.Parse(lastWrongLogin, CultureInfo.InvariantCulture));
        }
        return TimeSpan.Zero;
    }

    /// <summary>
    /// Sends forgot password request to server async
    /// </summary>
    /// <param name="accountData">Data required for authentication</param>
    /// <returns>Result of operation</returns>
    public async Task ForgotPasswordAsync(AuthDTO accountData, Func<BaseDTO, Task> action, bool isEnvironmentCheckRequired)
    {
        try
        {
            accountData.AuthenticationData.EmailID = CheckEnvironment(accountData, isEnvironmentCheckRequired, accountData.AuthenticationData.EmailID);
            if (accountData.ErrCode == ErrorCode.OK)
            {
                accountData.AuthenticationData.EmailID = accountData.AuthenticationData.EmailID.ToLowerInvariant();
                var httpData = new HttpServiceModel<AuthDTO>
                {
                    PathWithoutBasePath = UrlConstants.FORGOT_PASSWORD_ASYNC_PATH,
                    ContentToSend = accountData,
                    ContentType = HttpContentType.Json,
                    AuthType = AuthorizationType.Basic
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                accountData.ErrCode = httpData.ErrCode;
                accountData.Response = httpData.Response;
                await HandleUserDataAsync(accountData, action, Pages.ForgotPasswordPage).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            accountData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sends set password request to server
    /// </summary>
    /// <param name="accountData">Data required for authentication</param>
    /// <returns>Settings and resources</returns>
    public async Task ResetPasswordAsync(AuthDTO accountData)
    {
        try
        {
            if ((accountData.AuthenticationData.PageType == Pages.ChangePasswordPage
                || accountData.AuthenticationData.PageType == Pages.SetNewPasswordPage)
                && accountData.AuthenticationData.OldPassword == accountData.AuthenticationData.AccountPassword)
            {
                accountData.ErrCode = ErrorCode.SameOldPassword;
                return;
            }
            var httpData = new HttpServiceModel<AuthDTO>
            {
                PathWithoutBasePath = accountData.AuthenticationData.PageType == Pages.ChangePasswordPage
                    ? UrlConstants.CHANGE_PASSWORD_ASYNC_PATH
                    : UrlConstants.RESET_PASSWORD_ASYNC_PATH,
                ContentToSend = accountData,
                ContentType = HttpContentType.Json,
                AuthType = accountData.AuthenticationData.PageType == Pages.ChangePasswordPage
                    ? AuthorizationType.Bearer
                    : AuthorizationType.Basic
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            accountData.ErrCode = httpData.ErrCode;
            if (accountData.ErrCode == ErrorCode.OK || accountData.ErrCode == ErrorCode.AccountLockout)
            {
                await DeleteSecuredValuesAsync(
                    StorageConstants.PR_USER_CRED_KEY,
                    StorageConstants.SS_USER_NAME_KEY,
                    StorageConstants.PR_PHONE_NUMBER_KEY
                ).ConfigureAwait(true);
            }
        }
        catch (Exception ex)
        {
            accountData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    //todo:
    ///// <summary>
    ///// Establish SignalR connection
    ///// </summary>
    ///// <param name="selectedUserID">Currently logged in user id</param>
    ///// <returns>SignalR Hub Connection</returns>
    //public async Task<HubConnection> EstablishSignalRHubConnectionAsync(long selectedUserID)
    //{
    //	try
    //	{
    //		if (selectedUserID > 0)
    //		{
    //			////Connect to Hub
    //			//HubConnection hubConnection = new HubConnectionBuilder()
    //			//	.WithUrl(await accountLibraryService.GetSelectedBaseUrlAsync(UrlConstants.DEFAULT_ENVIRONMENT_KEY_VALUE).ConfigureAwait(false) + Constants.SE_SIGNALR_ENDPOINT)
    //			//	.WithAutomaticReconnect().Build();
    //			//await hubConnection.StartAsync().ConfigureAwait(false);
    //			//if (hubConnection.State == HubConnectionState.Connected)
    //			//{
    //			//	await RegisterSignalRConnectionAsync(hubConnection.ConnectionId, selectedUserID).ConfigureAwait(false);
    //			//	hubConnection.Reconnected += async (connectionID) => await RegisterSignalRConnectionAsync(connectionID, selectedUserID).ConfigureAwait(false);
    //			//	return hubConnection;
    //			//}
    //		}
    //	}
    //	catch (Exception ex)
    //	{
    //		LogError(ex.Message, ex);
    //	}
    //	return null;
    //}

    /// <summary>
    /// Registers SignalR connection
    /// </summary>
    /// <param name="connectionID">SignalR connection id</param>
    /// <param name="selectedUserID">Currently logged in user id</param>
    /// <returns>Result of operation</returns>
    public async Task RegisterSignalRConnectionAsync(string connectionID, long selectedUserID)
    {
        var httpData = new HttpServiceModel<BaseDTO>
        {
            PathWithoutBasePath = UrlConstants.REGISTER_SIGNALR_ASYNC_PATH,
            QueryParameters = new NameValueCollection
            {
                { Constants.SE_CONNECTION_ID_QUERY_KEY, connectionID },
                { Constants.SE_SELECTED_USER_ID_QUERY_KEY, selectedUserID.ToString(CultureInfo.InvariantCulture) }
            }
        };
        await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
        if (httpData.ErrCode == ErrorCode.OK)
        {
            _essentials.SetPreferenceValue(StorageConstants.PR_SIGNALR_CONNECTION_ID_KEY, connectionID);
        }
    }

    /// <summary>
    /// Verifies the login and OTP and returns status of login.
    /// </summary>
    /// <param name="accountData">Users login details to verify on server</param>
    /// <param name="isEnvironmentCheckRequired">do we need to determine env or not?</param>
    /// <param name="action">Sync call action</param>
    /// <param name="cancellationToken">Users login details to verify on server</param>
    /// <returns>operation status of login</returns>
    public async Task LoginAsync(AuthDTO accountData, bool isEnvironmentCheckRequired, Func<BaseDTO, Task> action, CancellationToken cancellationToken)
    {
        try
        {
            accountData.AuthenticationData.UserName = CheckEnvironment(accountData, isEnvironmentCheckRequired, accountData.AuthenticationData.UserName);
            if (accountData.ErrCode == ErrorCode.OK)
            {
                accountData.AuthenticationData.UserName = accountData.AuthenticationData.UserName.ToLowerInvariant();
                var httpData = new HttpServiceModel<AuthDTO>
                {
                    PathWithoutBasePath = UrlConstants.LOGIN_ASYNC_PATH,
                    ContentToSend = accountData,
                    AuthType = AuthorizationType.Basic,
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                accountData.ErrCode = httpData.ErrCode;
                accountData.Response = httpData.Response;
                await HandleUserDataAsync(accountData, action, Pages.LoginPage).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            accountData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Verifies the login via temp token
    /// </summary>
    /// <param name="accountData">Users login details to verify on server</param>
    /// <returns>operation status of login</returns>
    public async Task LoginWithTempTokenAsync(AuthDTO accountData)
    {
        try
        {
            var httpData = new HttpServiceModel<AuthDTO>
            {
                PathWithoutBasePath = UrlConstants.LOGIN_WITH_TEMP_TOKEN_ASYNC_PATH,
                ContentToSend = accountData,
                AuthType = AuthorizationType.Basic,
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            accountData.ErrCode = httpData.ErrCode;
            accountData.Response = httpData.Response;
            await HandleUserDataAsync(accountData, null, Pages.LoginPage).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            accountData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Get new sms verification code
    /// </summary>
    /// <param name="accountData">to pass data and retrieve the status of operation</param>
    /// <returns>Operation status</returns>
    public async Task ResendSmsAsync(AuthDTO accountData)
    {
        try
        {
            accountData.AuthenticationData.UserName = accountData.AuthenticationData.UserName.ToLowerInvariant();
            var httpData = new HttpServiceModel<AuthDTO>
            {
                PathWithoutBasePath = UrlConstants.RESEND_OTP_ASYNC_PATH,
                ContentToSend = accountData,
                AuthType = AuthorizationType.Basic
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            accountData.ErrCode = httpData.ErrCode;
            if (accountData.ErrCode == ErrorCode.SMSAuthentication)
            {
                accountData.ErrCode = ErrorCode.OK;
            }
        }
        catch (Exception ex)
        {
            accountData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Get pinCode data
    /// </summary>
    /// <param name="pinCodeData">object to get pinCode data</param>
    /// <returns>PinCode data and operation status code</returns>
    public async Task GetPinCodeDataAsync(BaseDTO pinCodeData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await Task.WhenAll(
                    new SettingService(_essentials).GetSettingsAsync(pinCodeData, GroupConstants.RS_ENVIRONMENT_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_COMMON_ORGANISATION_SETTINGS_GROUP),
                    new ResourceService(_essentials).GetResourcesAsync(pinCodeData, true, GroupConstants.RS_LOGIN_FLOW_PAGE_GROUP)
                ).ConfigureAwait(false);
                pinCodeData.ErrCode = ErrorCode.OK;
            }
            else
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    PathWithoutBasePath = pinCodeData.IsActive
                        ? UrlConstants.GET_PINCODE_DATA_ASYNC_PATH
                        : UrlConstants.GET_PINCODE_LOGIN_DATA_ASYNC_PATH
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                pinCodeData.ErrCode = httpData.ErrCode;
                if (pinCodeData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(pinCodeData, data);
                    }
                    UpdateMaxMinLength(pinCodeData, SettingsConstants.S_PINCODE_LENGTH_KEY, ResourceConstants.R_PINCODE_KEY);
                    if (pinCodeData.IsActive)
                    {
                        UpdateMaxMinLength(pinCodeData, SettingsConstants.S_PINCODE_LENGTH_KEY, ResourceConstants.R_CONFIRM_PINCODE_KEY);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            pinCodeData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Verify pinCode data
    /// </summary>
    /// <param name="pinCodeData">object to verify pinCode</param>
    /// <returns>operation status code</returns>
    public async Task VerifyPinCodeAsync(SessionDTO pinCodeData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                pinCodeData.ErrCode = ErrorCode.InvalidPincode;
                if (pinCodeData.IsActive)
                {
                    // Validate pinCode of confirm pinCode page
                    if (pinCodeData.Session.PinCode.Equals(pinCodeData.Session.ConfirmPinCode, StringComparison.InvariantCultureIgnoreCase))
                    {
                        await SaveSecuredValueAsync(StorageConstants.SS_PIN_CODE_KEY, pinCodeData.Session.PinCode).ConfigureAwait(true);
                        pinCodeData.ErrCode = ErrorCode.OK;
                    }
                }
                else
                {
                    // Validate pinCode of pinCode login page
                    string pinCode = await GetSecuredValueAsync(StorageConstants.SS_PIN_CODE_KEY).ConfigureAwait(true);
                    if (!string.IsNullOrWhiteSpace(pinCode) && pinCode == pinCodeData.Session.PinCode)
                    {
                        // set the value of S_LAST_WRONG_LOGIN_DATETIME_KEY to empty to disable the 'UserLocked' popup
                        _essentials.SetPreferenceValue(StorageConstants.PR_LAST_WRONG_LOGIN_DATE_TIME_KEY, string.Empty);
                        pinCodeData.ErrCode = ErrorCode.OK;
                    }
                }
            }
            else
            {
                var httpData = new HttpServiceModel<SessionDTO>
                {
                    PathWithoutBasePath = pinCodeData.IsActive ? UrlConstants.SET_PINCODE_ASYNC_PATH : UrlConstants.VERIFY_PINCODE_ASYNC_PATH,
                    ContentToSend = pinCodeData,
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                pinCodeData.ErrCode = httpData.ErrCode;
            }
        }
        catch (Exception ex)
        {
            pinCodeData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Checks User and clean or update values 
    /// </summary>
    /// <param name="accountData">Request data to verify</param>
    /// <param name="action">Sync call action</param>
    /// <returns>Operation status</returns>
    private async Task HandleUserDataAsync(AuthDTO accountData, Func<BaseDTO, Task> action, Pages page)
    {
        if (IsLoginFlowValidError(accountData, page)
            || IsForgotPasswordFlowValidError(accountData, page))
        {
            long existingOrganisationID = _essentials.GetPreferenceValue(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, accountData.OrganisationID);
            ErrorCode errorCode = accountData.ErrCode;
            accountData.OrganisationID = GetOrganisationID(accountData);
            if (page == Pages.ForgotPasswordPage ||
                (page == Pages.LoginPage && accountData.ErrCode == ErrorCode.SMSAuthentication))
            {
                GetUserAuthenticationData(accountData, page);
            }
            accountData.Session = page == Pages.LoginPage && accountData.ErrCode != ErrorCode.SMSAuthentication
                ? MapSessionData(accountData)
                : default;
            if (!(!MobileConstants.IsMobilePlatform && page == Pages.ForgotPasswordPage))
            {
                _essentials.SetPreferenceValue(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, accountData.OrganisationID);
            }
            if (MobileConstants.IsMobilePlatform)
            {
                accountData.PermissionAtLevelID = (long)JToken.Parse(accountData.Response)[nameof(BaseDTO.PermissionAtLevelID)];
                _essentials.SetPreferenceValue(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, accountData.PermissionAtLevelID);
                //Todo:
                //DependencyService.Get<INotificationHelper>().RegisterForRemoteNotification();
                await HandleOrganisationChangeAsync(accountData, action, existingOrganisationID, page).ConfigureAwait(false);
            }
            else if (page == Pages.LoginPage && errorCode != ErrorCode.SMSAuthentication)
            {
                await ClearAndSaveUserDataAsync(accountData).ConfigureAwait(false);
            }
            if (accountData.ErrCode == ErrorCode.OK)
            {
                accountData.ErrCode = errorCode;
            }
        }
        else if (accountData.ErrCode == ErrorCode.AccountLockout)
        {
            accountData.RecordCount = (long)JToken.Parse(accountData.Response)[nameof(BaseDTO.RecordCount)];
        }
        else
        {
            // Do nothing
        }
    }

    /// <summary>
    /// Select environment URL as per the username prefix and updates Username without prefix.
    /// </summary>
    /// <param name="accountData">Operation status variable instance</param>
    /// <param name="isEnvironmentCheckRequired">do we need to determine env or not?</param>
    /// <param name="username">username to determine env</param>
    public string CheckEnvironment(AuthDTO accountData, bool isEnvironmentCheckRequired, string username)
    {
        accountData.AddedBy = username;
        environmentService.CheckEnvironment(accountData, isEnvironmentCheckRequired, Constants.DEFAULT_ENVIRONMENT_KEY);
        return accountData.AddedBy;
    }

    /// <summary>
    /// Get client default country code based on IP address
    /// </summary>
    /// <param name="contryCodes">Object to get country data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Default country code</returns>
    public async Task GetDefaultCountryCodeAsync(BaseDTO contryCodes, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<string>
            {
                CancellationToken = cancellationToken,
                BaseUrl = new Uri(contryCodes.Settings.FirstOrDefault(x => x.SettingKey == SettingsConstants.S_DEFAULT_COUNTRY_CODE_PATH_KEY).SettingValue)
            };
            await new HttpService(_essentials).GetDefaultCountryCodeAsync(httpData).ConfigureAwait(false);
            contryCodes.ErrCode = httpData.ErrCode;
            if (contryCodes.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    contryCodes.CountryCodes = new List<CountryModel>
                    {
                        new CountryModel
                        {
                            CountryCulture = Convert.ToString(data["country_code"], CultureInfo.InvariantCulture).ToUpperInvariant(),
                            CountryName = Convert.ToString(data["country_name"], CultureInfo.InvariantCulture),
                        }
                    };
                }
            }
        }
        catch (Exception ex)
        {
            contryCodes.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private async Task CleanUserDataAsync()
    {
        // Initialize user data if user id is changes
        await new DataSyncService(_essentials).InitializeDataSyncForDateTimeAsync(false, -1).ConfigureAwait(false);
        await CleanUserNotificationsAsync(true).ConfigureAwait(false);
        await new CommonDatabase().CleanUserDataAsync().ConfigureAwait(false);
        await ClearAccountTokensAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Clear users local notifications
    /// </summary>
    /// <param name="isDBCleaned">flag which decides data needs to reset in db or not</param>
    /// <param name="isComingFromLoginPage">flag which decides whether call is coming for login page or not</param>
    /// <returns>Operation Status</returns>
    public async Task CleanUserNotificationsAsync(bool isDBCleaned, bool isComingFromLoginPage)
    {
        if (MobileConstants.IsMobilePlatform)
        {
            //Todo:
            //DependencyService.Get<INotificationService>().CancelAll();
            //if (isComingFromLoginPage)
            //{
            //	DependencyService.Get<INotificationHelper>().UnregisterRemoteNotification();
            //	await new MedicationDatabase().DeleteNotificationsAsync();
            //}
            //else
            //{
            //	if (!isDBCleaned)
            //	{
            //		await new MedicationDatabase().ResetNotificationStatusAsync();
            //	}
            //}
        }
    }

    /// <summary>
    /// Clear users local notifications
    /// </summary>
    /// <param name="isDBCleaned">flag which decides data needs to reset in db or not</param>
    /// <returns>Operation Status</returns>
    public async Task CleanUserNotificationsAsync(bool isDBCleaned)
    {
        await CleanUserNotificationsAsync(isDBCleaned, false).ConfigureAwait(true);
    }

    private async Task ClearAndSaveUserDataAsync(AuthDTO accountData)
    {
        long currentUserID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
        _essentials.SetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, accountData.Session.AccountID);
        if (MobileConstants.IsMobilePlatform)
        {
            if (accountData.Session.AccountID != currentUserID)
            {
                await CleanUserDataAsync().ConfigureAwait(false);
                await SaveTokensAsync(accountData.Session).ConfigureAwait(false);
            }
            else
            {
                await ClearStorageAndUpdateTokensAsync(accountData, false).ConfigureAwait(false);
            }
        }
        else
        {
            await ClearStorageAndUpdateTokensAsync(accountData, true).ConfigureAwait(false);
        }
    }

    private void GetUserAuthenticationData(AuthDTO accountData, Pages page)
    {
        var authData = JToken.Parse(accountData.Response)[nameof(AuthDTO.AuthenticationData)];
        accountData.AccountID = (long)authData[nameof(AuthModel.UserAccountID)];
        if (page == Pages.LoginPage)
        {
            _essentials.SetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)authData[nameof(AuthModel.RoleID)]);
        }
    }

    private long GetOrganisationID(BaseDTO responseData)
    {
        return (long)JToken.Parse(responseData.Response)[nameof(BaseDTO.OrganisationID)];
    }

    private async Task ClearStorageAndUpdateTokensAsync(AuthDTO accountData, bool shouldClearStorage)
    {
        if (shouldClearStorage)
        {
            await ClearAccountTokensAsync().ConfigureAwait(false);
        }
        await SaveTokensAsync(accountData.Session).ConfigureAwait(false);
    }

    /// <summary>
    /// Get user credentials from dynamic link data
    /// </summary>
    /// <param name="authData">object to transfer data</param>
    /// <returns>User credentials and operation status</returns>
    public async Task GetDynamicLinkDataAsync(AuthDTO authData, Func<BaseDTO, Task> action)
    {
        try
        {
            GetDynamicLinkData(authData);
            var existingOrganisationID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, Constants.DEFAULT_ORGANISATION_ID);
            authData.AuthenticationData.UserName = CheckEnvironment(authData, true, authData.AuthenticationData.UserName);
            if (authData.ErrCode == ErrorCode.OK)
            {
                await HandleOrganisationChangeAsync(authData, action, existingOrganisationID, Pages.DynamicLink).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            authData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }


    private async Task HandleOrganisationChangeAsync(AuthDTO accountData, Func<BaseDTO, Task> action, long existingOrganisationID, Pages page)
    {
        //Check :
        //  1. if environment is changed 
        //  2. or organisation id is changed after login response
        if (_essentials.GetPreferenceValue(StorageConstants.PR_IS_ENVIRONMENT_CHANGED_KEY, false)
            || accountData.OrganisationID != existingOrganisationID)
        {
            await new DataSyncService(_essentials).InitializeDataSyncForDateTimeAsync(true, -1).ConfigureAwait(false);
            if (accountData.ErrCode == ErrorCode.OK || accountData.ErrCode == ErrorCode.SetPinCode || accountData.ErrCode == ErrorCode.SMSAuthentication
                || IsForgotPasswordFlowValidError(accountData, page))
            {
                ErrorCode errorCode = accountData.ErrCode;
                _essentials.SetPreferenceValue(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, accountData.OrganisationID);
                _essentials.SetPreferenceValue(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, accountData.PermissionAtLevelID);
                accountData.ErrCode = await new DataSyncService(_essentials).HandelMasterDataChangeAsync(action).ConfigureAwait(false);
                if (accountData.ErrCode == ErrorCode.OK)
                {
                    await RegisterAppCenterIdsAsync();
                    _essentials.SetPreferenceValue(StorageConstants.PR_IS_ENVIRONMENT_CHANGED_KEY, false);
                    _essentials.DeletePreferenceValue(StorageConstants.PR_SELECTED_ENVIRONMENT_DEFAULT_BASE_PATH_KEY);
                    await ResetUserDataAsync().ConfigureAwait(false);
                    // Check if selected language is available in list of languages
                    await new LanguageService(_essentials).VerifySelectedLanguageAsync(accountData).ConfigureAwait(false);
                    if (accountData.ErrCode == ErrorCode.OK && page == Pages.LoginPage && errorCode != ErrorCode.SMSAuthentication)
                    {
                        _essentials.SetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, accountData.Session.AccountID);
                        await SaveTokensAsync(accountData.Session).ConfigureAwait(false);
                        accountData.ErrCode = errorCode;
                    }
                }
            }
        }
        else if (page == Pages.LoginPage && accountData.ErrCode != ErrorCode.SMSAuthentication)
        {
            await new DataSyncService(_essentials).InitializeDataSyncForDateTimeAsync(DataSyncFor.Features).ConfigureAwait(false);
            await ClearAndSaveUserDataAsync(accountData).ConfigureAwait(false);
        }
        else if (page == Pages.DynamicLink)
        {
            await ResetUserDataAsync().ConfigureAwait(false);
        }
    }

    public async Task ResetUserDataAsync()
    {
        // Reset selected account data
        _essentials.SetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
        _essentials.SetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0);
        if (MobileConstants.IsMobilePlatform)
        {
            await new DataSyncService(_essentials).InitializeDataSyncForDateTimeAsync(DataSyncFor.Features).ConfigureAwait(false);
            await CleanUserDataAsync().ConfigureAwait(false);
        }
        else
        {
            await ClearAccountTokensAsync().ConfigureAwait(false);
        }
    }

    private bool IsLoginFlowValidError(AuthDTO accountData, Pages page)
    {
        return page == Pages.LoginPage
            && (accountData.ErrCode == ErrorCode.OK
            || accountData.ErrCode == ErrorCode.SMSAuthentication
            || accountData.ErrCode == ErrorCode.SetPinCode
            || accountData.ErrCode == ErrorCode.MultipleUsers
        );
    }

    private bool IsForgotPasswordFlowValidError(AuthDTO accountData, Pages page)
    {
        return page == Pages.ForgotPasswordPage
            && (accountData.ErrCode == ErrorCode.SMSAuthentication
            || accountData.ErrCode == ErrorCode.ResetPassword);
    }

    private async Task RegisterAppCenterIdsAsync()
    {
        if (MobileConstants.IsMobilePlatform && _essentials.GetPreferenceValue(StorageConstants.PR_IS_ENVIRONMENT_CHANGED_KEY, false))
        {
            await GetSettingsAsync(GroupConstants.RS_SECURITY_GROUP, GroupConstants.RS_ENVIRONMENT_GROUP).ConfigureAwait(true);
            var selectedEnv = _essentials.GetPreferenceValue(StorageConstants.PR_SELECTED_ENVIRONMENT_KEY, Constants.DEFAULT_ENVIRONMENT_KEY);
            var settingDescription = PageData.Settings?.FirstOrDefault(x => x.GroupName == GroupConstants.RS_ENVIRONMENT_GROUP && x.SettingKey == selectedEnv)?.SettingDescription?.Split(Constants.SYMBOL_PIPE_SEPERATOR);
            if (settingDescription?.Length > 0)
            {
                //Todo:
                //DependencyService.Get<ICrashRegistration>().RegisterNewAppSecret(LibGenericMethods.GetPlatformSpecificValue(settingDescription[0], settingDescription[1], string.Empty));
                //AppCenter.Configure(String.Format(Constants.APP_CENTER_SETUP_STRING_FORMAT, settingDescription[0], settingDescription[1]));
                //if (AppCenter.Configured)
                //{
                //	AppCenter.Start(typeof(Analytics));
                //	AppCenter.Start(typeof(Crashes));
                //}
            }
        }
    }
}