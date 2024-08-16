using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AlphaMDHealth.ServiceBusinessLayer;

public class AccountServiceBusinessLayer : BaseServiceBusinessLayer
{
    /// <summary>
    /// Account service
    /// </summary>
    /// <param name="httpContext">Instance of HttpContext</param>
    public AccountServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
    {
    }

    /// <summary>
    /// Validates Refresh Token and generates new tokens if RefreshToken if valid
    /// </summary>
    /// <param name="sessionData">Session data</param>
    /// <returns>refresh token details</returns>
    public async Task<SessionDTO> GetTokenAsync(SessionDTO sessionData)
    {
        try
        {
            if (sessionData.Session == null)
            {
                sessionData.ErrCode = ErrorCode.InvalidData;
                return sessionData;
            }
            MapHeadersInSession((sessionData.Request as HttpRequest).Headers, sessionData.Session);
            if (string.IsNullOrWhiteSpace(sessionData.Session.ClientIdentifier)
                || string.IsNullOrWhiteSpace(sessionData.Session.DeviceID)
                || string.IsNullOrWhiteSpace(sessionData.Session.DeviceModel)
                || string.IsNullOrWhiteSpace(sessionData.Session.DeviceOS)
                || string.IsNullOrWhiteSpace(sessionData.Session.DeviceOSVersion)
                || string.IsNullOrWhiteSpace(sessionData.Session.RefreshToken))
            {
                sessionData.ErrCode = ErrorCode.InvalidData;
                return sessionData;
            }
            AccountServiceDataLayer account = new AccountServiceDataLayer();
            await account.GetTokenAsync(sessionData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            sessionData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        return sessionData;
    }

    /// <summary>
    /// Validate user based on the access token
    /// </summary>
    /// <param name="sessionData">Session data along with request headers</param>
    /// <returns>Operation status</returns>
    public async Task ValidateAccessTokenAsync(SessionDTO sessionData)
    {
        try
        {
            MapHeadersInSession((sessionData.Request as HttpRequest).Headers, sessionData.Session);
            if (sessionData.OrganisationID < 1 || string.IsNullOrWhiteSpace(sessionData.Session.ClientIdentifier)
                || string.IsNullOrWhiteSpace(sessionData.Session.DeviceID) || string.IsNullOrWhiteSpace(sessionData.Session.DeviceModel)
                || string.IsNullOrWhiteSpace(sessionData.Session.DeviceOS) || string.IsNullOrWhiteSpace(sessionData.Session.DeviceOSVersion)
                || string.IsNullOrWhiteSpace(sessionData.Session.AccessToken))
            {
                sessionData.ErrCode = ErrorCode.InvalidData;
                return;
            }
            using AccountServiceDataLayer accountDB = new AccountServiceDataLayer();
            await accountDB.ValidateAccessTokenAsync(sessionData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            sessionData.ErrCode = ErrorCode.Unauthorized;
        }
    }

    /// <summary>
    /// Checks if the given request has clientId, Signature and accessToken that are valid for a
    /// particular application
    /// </summary>
    /// <param name="sessionData">session data along with request data</param>
    /// <returns>Result of operation in sessionData as reference</returns>
    public async Task ValidateRequestAsync(SessionDTO sessionData)
    {
        try
        {
            sessionData.Session = new SessionModel();
            MapHeadersInSession((sessionData.Request as HttpRequest).Headers, sessionData.Session);
            (sessionData.Request as HttpRequest).Headers.TryGetValue(Constants.SE_HMAC_SIGNATURE_HEADER_KEY, out StringValues signature);
            sessionData.RequestSignature = signature.FirstOrDefault() ?? string.Empty;
            if (IsValidRequest(sessionData))
            {
                await ValidateSignatureAsync(sessionData).ConfigureAwait(false);
            }
            else
            {
                sessionData.ErrCode = ErrorCode.Unauthorized;
                return;
            }
        }
        catch (Exception ex)
        {
            sessionData.ErrCode = ErrorCode.InternalServerError;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Resources and Settings for Registration page
    /// </summary>
    /// <param name="languageID">User's Language Id</param>
    /// <param name="organisationID">User's Organisation Id</param>
    /// <returns>List of resources and settings with operation status</returns>
    public async Task<AuthDTO> GetAccountDataAsync(byte languageID, long organisationID)
    {
        AuthDTO authData = new AuthDTO();
        try
        {
            if (languageID < 1 || organisationID < 1)
            {
                authData.ErrCode = ErrorCode.InvalidData;
                return authData;
            }
            authData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            authData.OrganisationID = organisationID;
            if (await GetConfigurationDataAsync(authData, languageID).ConfigureAwait(false))
            {
                authData.ErrCode = ErrorCode.OK;
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            authData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        return authData;
    }

    /// <summary>
    /// Create user session based on user input data
    /// </summary>
    /// <param name="languageID">User's Language Id</param>
    /// <param name="organisationID">User's Organisation Id</param>
    /// <param name="authData">User input data to validate and create session</param>
    /// <param name="headers">http request received from client</param>
    /// <returns>Operation status and token in case of success</returns>
    public async Task<AuthDTO> LoginAsync(byte languageID, long organisationID, AuthDTO authData, IHeaderDictionary headers)
    {
        try
        {
            if (languageID < 1 || organisationID < 1 || authData.AuthenticationData == null
                || string.IsNullOrWhiteSpace(authData.AuthenticationData.UserName)
                || string.IsNullOrWhiteSpace(authData.AuthenticationData.AccountPassword))
            {
                authData.ErrCode = ErrorCode.InvalidData;
                return authData;
            }
            authData.Session = new SessionModel();
            MapHeadersInSession(headers, authData.Session);
            authData.LanguageID = languageID;
            authData.OrganisationID = organisationID;
            if (string.IsNullOrWhiteSpace(authData.AuthenticationData.Otp))
            {
                authData.AuthenticationData.Otp = string.Empty;
            }
            await new AccountServiceDataLayer().LoginAsync(authData).ConfigureAwait(false);
            if (authData.ErrCode == ErrorCode.SMSAuthentication)
            {
                TemplateDTO communicationDto = new TemplateDTO();
                communicationDto.PhoneNumber = authData.AuthenticationData.UserName;
                communicationDto.EmailID = authData.AuthenticationData.UserName;
                communicationDto.OrganisationID = authData.OrganisationID;
                communicationDto.LanguageID = languageID;
                communicationDto.TemplateData = new TemplateModel
                {
                    TemplateName = TemplateName.EVerifyLogin,
                    IsExternal = false,
                };
                await SendCommunicationAsync(communicationDto).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            authData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return authData;
    }

    /// <summary>
    /// Create user session based on user input data
    /// </summary>
    /// <param name="languageID">User's Language Id</param>
    /// <param name="organisationID">User's Organisation Id</param>
    /// <param name="authData">User input data to validate and create session</param>
    /// <param name="headers">http request received from client</param>
    /// <returns>Operation status and token in case of success</returns>
    public async Task<AuthDTO> LoginWithTempTokenAsync(byte languageID, long organisationID, AuthDTO authData, IHeaderDictionary headers)
    {
        try
        {
            if (languageID < 1 || organisationID < 1 || authData.TempSession == null
                || string.IsNullOrWhiteSpace(authData.TempSession.TempToken)
                || string.IsNullOrWhiteSpace(authData.TempSession.TokenIdentifier))
            {
                authData.ErrCode = ErrorCode.InvalidData;
                return authData;
            }
            authData.Session = new SessionModel();
            MapHeadersInSession(headers, authData.Session);
            authData.LanguageID = languageID;
            authData.OrganisationID = organisationID;
            await new AccountServiceDataLayer().LoginWithTempTokenAsync(authData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            authData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return authData;
    }

    /// <summary>
    /// Validates the user details sent for Forgot Password and on success sends OTP to
    /// registered Email address and Mobile number
    /// </summary>
    /// <param name="languageID">User's Language Id</param>
    /// <param name="organisationID">User's Organisation Id</param>
    /// <param name="authData">Users input data to be validated on server</param>
    /// <returns>Status confirming the authenticity of data provided by user</returns>
    public async Task<BaseDTO> ForgotPasswordAsync(byte languageID, long organisationID, AuthDTO authData)
    {
        try
        {
            if (languageID < 1 || organisationID < 0 || authData.AuthenticationData == null
                || string.IsNullOrWhiteSpace(authData.AuthenticationData.PhoneNo)
                || string.IsNullOrWhiteSpace(authData.AuthenticationData.EmailID))
            {
                authData.ErrCode = ErrorCode.InvalidData;
                return authData;
            }
            authData.LanguageID = languageID;
            authData.OrganisationID = organisationID;
            using (var accountDB = new AccountServiceDataLayer())
            {
                await accountDB.ForgotPasswordAsync(authData).ConfigureAwait(false);
            }
            if (authData.ErrCode == ErrorCode.SMSAuthentication)
            {
                TemplateDTO communicationDto = new TemplateDTO();
                communicationDto.PhoneNumber = authData.AuthenticationData.PhoneNo;
                communicationDto.EmailID = authData.AuthenticationData.EmailID;
                communicationDto.OrganisationID = authData.OrganisationID;
                communicationDto.LanguageID = languageID;
                communicationDto.TemplateData = new TemplateModel
                {
                    TemplateName = TemplateName.EVerifyResetPassword,
                    IsExternal = false,
                };
                await SendCommunicationAsync(communicationDto).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            authData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return authData;
    }

    /// <summary>
    /// Sets or Resets user password if the Data is valid
    /// </summary>
    /// <param name="languageID">User's Language Id</param>
    /// <param name="organisationID">User's Organisation Id</param>
    /// <param name="authData">User input data</param>
    /// <returns>Operation status</returns>
    public async Task<AuthDTO> ResetPasswordAsync(byte languageID, long organisationID, AuthDTO authData)
    {
        try
        {
            if (IsValidResetPasswordRequest(languageID, organisationID, authData))
            {
                authData.ErrCode = ErrorCode.InvalidData;
                return authData;
            }
            // If request is for change password then it should have account id
            if (authData.AuthenticationData.PageType == Pages.ChangePasswordPage && AccountID < 1)
            {
                authData.ErrCode = ErrorCode.Unauthorized;
                return authData;
            }
            authData.AccountID = AccountID;
            authData.OrganisationID = organisationID;
            authData.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP
                , authData.LanguageID, default, authData.AccountID, organisationID, false).ConfigureAwait(false))?.Settings;
            if (!Regex.Match(authData.AuthenticationData.AccountPassword, GetSettingValueByKey(authData.Settings, SettingsConstants.S_PASSWORD_REGEX_KEY)).Success)
            {
                authData.ErrCode = ErrorCode.InvalidData;
                return authData;
            }
            using var accountDB = new AccountServiceDataLayer();
            await accountDB.SetPasswordAsync(authData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            authData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return authData;
    }

    /// <summary>
    /// Resend otp code
    /// </summary>
    /// <param name="languageID">User's Language Id</param>
    /// <param name="organisationID">users organisation id</param>
    /// <param name="authData">data to generate OTP</param>
    /// <returns>Operation status</returns>
    public async Task<AuthDTO> ResendOtpAsync(byte languageID, long organisationID, AuthDTO authData)
    {
        try
        {
            if (languageID < 1 || organisationID < 1 || authData.AuthenticationData == null
                || string.IsNullOrWhiteSpace(authData.AuthenticationData.UserName))
            {
                authData.ErrCode = ErrorCode.InvalidData;
                return authData;
            }
            authData.OrganisationID = organisationID;
            authData.AccountID = AccountID;
            authData.LanguageID = languageID;
            using (var accountDB = new AccountServiceDataLayer())
            {
                await accountDB.ResendOtpAsync(authData).ConfigureAwait(false);
            }
            if (authData.ErrCode == ErrorCode.SMSAuthentication)
            {
                TemplateDTO communicationDto = new TemplateDTO();
                communicationDto.PhoneNumber = authData.AuthenticationData.UserName;
                communicationDto.EmailID = authData.AuthenticationData.UserName;
                communicationDto.OrganisationID = authData.OrganisationID;
                communicationDto.LanguageID = languageID;
                communicationDto.TemplateData = new TemplateModel { IsExternal = authData.AuthenticationData.IsExternal };
                if (authData.AuthenticationData.IsExternal)
                {
                    communicationDto.TemplateData.ExternalUserName = authData.AuthenticationData.UserName;
                    communicationDto.TemplateData.ExternalMobileNo = authData.AuthenticationData.EmailID;
                    communicationDto.TemplateData.ExternalEmailID = authData.AuthenticationData.EmailID;
                }
                if (authData.AuthenticationData.PageType == Pages.ResetPasswordPage)
                {
                    communicationDto.TemplateData.TemplateName = TemplateName.EVerifyResetPassword;
                }
                else if (authData.AuthenticationData.PageType == Pages.RegisterPasswordPage)
                {
                    communicationDto.TemplateData.TemplateName = TemplateName.EVerifyRegistration;
                }
                else
                {
                    communicationDto.TemplateData.TemplateName = TemplateName.EVerifyLogin;
                }
                await SendCommunicationAsync(communicationDto).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            authData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return authData;
    }

    /// <summary>
    /// Set User pincode / verify user pincode
    /// </summary>
    /// <param name="organisationID">User's Organisation Id</param>
    /// <param name="deviceID">Unique device id received in header</param>
    /// <param name="sessionData">Session data</param>
    /// <param name="isPincodeSetup">Is pincode setup flow</param>
    /// <returns>Operation status</returns>
    public async Task<BaseDTO> PincodeAsync(long organisationID, string deviceID, SessionDTO sessionData, bool isPincodeSetup)
    {
        try
        {
            if (organisationID < 1 || sessionData == null
                || string.IsNullOrWhiteSpace(sessionData.Session.PinCode)
                || string.IsNullOrWhiteSpace(deviceID))
            {
                sessionData.ErrCode = ErrorCode.InvalidData;
                return sessionData;
            }
            sessionData.AccountID = AccountID;
            if (sessionData.AccountID < 1)
            {
                sessionData.ErrCode = ErrorCode.Unauthorized;
                return sessionData;
            }
            sessionData.OrganisationID = organisationID;
            sessionData.Session.DeviceID = deviceID;
            sessionData.IsActive = isPincodeSetup;
            using var accountDB = new AccountServiceDataLayer();
            await accountDB.PincodeAsync(sessionData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            sessionData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return sessionData;
    }

    private async Task<bool> GetConfigurationDataAsync(BaseDTO authData, byte languageID)
    {
        authData.CountryCodes = (await GetDataFromCacheAsync(CachedDataType.Countries, string.Empty
            , languageID, default, 0, 0, false).ConfigureAwait(false)).CountryCodes;
        if (authData.CountryCodes != null)
        {
            authData.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings
                , $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_COMMON_ORGANISATION_SETTINGS_GROUP}"
                , languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
            if (authData.Settings != null)
            {
                authData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources
                    , $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_LOGIN_FLOW_PAGE_GROUP}"
                    , languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (authData.Resources != null)
                {
                    var organisationSettings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings
                        , GroupConstants.RS_ORGANISATION_SETTINGS_GROUP
                        , languageID, default, 0, authData.OrganisationID, false).ConfigureAwait(false)).Settings;
                    if (organisationSettings != null)
                    {
                        authData.Settings.AddRange(organisationSettings);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool IsValidRequest(SessionDTO sessionData)
    {
        return !(string.IsNullOrWhiteSpace(sessionData.Session.ClientIdentifier) || string.IsNullOrWhiteSpace(sessionData.Session.DeviceID)
            || string.IsNullOrWhiteSpace(sessionData.Session.DeviceModel) || string.IsNullOrWhiteSpace(sessionData.Session.DeviceOS)
            || string.IsNullOrWhiteSpace(sessionData.Session.DeviceOSVersion) || string.IsNullOrWhiteSpace(sessionData.RequestSignature));
    }

    private bool IsValidResetPasswordRequest(byte languageID, long organisationID, AuthDTO authData)
    {
        return languageID < 1 || organisationID < 0 || authData == null || authData.AuthenticationData == null
            || ValidateResetPasswordData(authData) || ValidateSetNewPasswordData(authData)
            || string.IsNullOrWhiteSpace(authData.AuthenticationData.AccountPassword)
            && string.IsNullOrWhiteSpace(authData.AuthenticationData.OldPassword);
    }

    private bool ValidateSetNewPasswordData(AuthDTO authData)
    {
        return authData.AuthenticationData.PageType == Pages.SetNewPasswordPage
            && string.IsNullOrWhiteSpace(authData.AuthenticationData.EmailID)
            && string.IsNullOrWhiteSpace(authData.AuthenticationData.PhoneNo);
    }

    private bool ValidateResetPasswordData(AuthDTO authData)
    {
        return authData.AuthenticationData.PageType == Pages.ResetPasswordPage
            && (string.IsNullOrWhiteSpace(authData.AuthenticationData.EmailID)
            || string.IsNullOrWhiteSpace(authData.AuthenticationData.PhoneNo));
    }

    private async Task ValidateSignatureAsync(SessionDTO sessionData)
    {
        SystemIdentifierModel systemIdentifier = ((SystemIdentifierDTO)await GetDataFromCacheAsync(CachedDataType.SystemIdentifiers
            , sessionData.Session.ClientIdentifier, 0, default, 0, 0, false).ConfigureAwait(false))?.SystemIdentifiers?.FirstOrDefault();
        if (systemIdentifier == null
            || systemIdentifier.DeviceType != sessionData.Session.DeviceType
            || systemIdentifier.DevicePlatform != sessionData.Session.DevicePlatform
            || !IsEndpointAllowed(sessionData, systemIdentifier.AllowEndpoints))
        {
            sessionData.ErrCode = ErrorCode.Unauthorized;
            return;
        }
        if (sessionData.RequestSignature == Guid.Empty.ToString())
        {
            return;
        }
            (sessionData.Request as HttpRequest).EnableBuffering();
        Stream content = (sessionData.Request as HttpRequest).Body;
        using (var reader = new StreamReader(content, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true))
        {
            await reader.ReadToEndAsync().ConfigureAwait(false);
        }
        content.Seek(0, SeekOrigin.Begin);
        byte[] hash = GenericMethods.ComputeHash(content);
        // Needs to reset content stream as the controller needs to read the content data
        // (i.e. body of POST/PUT calls)
        content.Seek(0, SeekOrigin.Begin);
        string requestContentBase64String = (hash == null) ? string.Empty : Convert.ToBase64String(hash);
        byte[] secretKeyBytes = Encoding.UTF8.GetBytes(systemIdentifier.ClientApplicationKey);
        (sessionData.Request as HttpRequest).Headers.TryGetValue(Constants.SE_AUTHORIZATION_HEADER_KEY, out StringValues authorization);
        byte[] dataBytes = Encoding.UTF8.GetBytes($"{sessionData.Session.ClientIdentifier}{(sessionData.Request as HttpRequest).Method}{(sessionData.Request as HttpRequest).GetDisplayUrl()}{authorization.FirstOrDefault() ?? string.Empty}{requestContentBase64String}");
        using HMACSHA256 hmac = new HMACSHA256(secretKeyBytes);
        var expected = Convert.ToBase64String(hmac.ComputeHash(dataBytes));
        sessionData.ErrCode = sessionData.RequestSignature.Equals(expected, StringComparison.Ordinal) ? ErrorCode.OK : ErrorCode.Unauthorized;
    }

    private bool IsEndpointAllowed(SessionDTO sessionData, string allowEndpoints)
    {
        if (!string.IsNullOrWhiteSpace(allowEndpoints))
        {
            var endpoints = allowEndpoints.Split(",");
            if (endpoints.Length > 0)
            {
                return endpoints.Contains(sessionData.Response);
            }
        }
        return true;
    }

    //private AuthDTO MapLoginFields(byte languageID, long organisationID, AuthDTO authData)
    //{
    //    authData.LanguageID = languageID;
    //    authData.OrganisationID = organisationID;
    //    authData.Fields = new List<FieldValidator>
    //    {
    //        new FieldValidator { ResourceKey= ResourceConstants.R_USER_NAME_KEY, Value= authData.AuthenticationData.UserName },
    //        new FieldValidator { ResourceKey= ResourceConstants.R_PASSWORD_KEY, Value= authData.AuthenticationData.AccountPassword, Type= FieldTypes.PasswordControl }
    //    };
    //    if (!string.IsNullOrWhiteSpace(authData.AuthenticationData.Otp))
    //    {
    //        authData.Fields.Add(new FieldValidator { ResourceKey = ResourceConstants.R_VERIFICATION_KEY, Value = authData.AuthenticationData.UserName, Type = FieldTypes.PinCodeControl });
    //    }
    //    return authData;
    //}
}