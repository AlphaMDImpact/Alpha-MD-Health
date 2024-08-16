using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceConfiguration;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace AlphaMDHealth.ServiceBusinessLayer;

public class UserServiceBusinessLayer : BaseServiceBusinessLayer
{
    /// <summary>
    /// Sample code service
    /// </summary>
    /// <param name="httpContext">Instance of HttpContext</param>
    public UserServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
    {
    }

    /// <summary>
    /// Save admin user data in database
    /// </summary>
    /// <param name="languageID">user's selected language</param>
    /// <param name="organisationID">user's organisation id</param>
    /// <param name="userData">object to save user data</param>
    /// <param name="headers">object to map device details</param>
    /// <returns>Operation status and token data</returns>
    public async Task<BaseDTO> RegisterPatientAsync(byte languageID, long organisationID, UserDTO userData, IHeaderDictionary headers)
    {
        try
        {
            if (await IsUserDataNotValidAsync(languageID, organisationID, userData).ConfigureAwait(false))
            {
                userData.ErrCode = ErrorCode.InvalidData;
                return userData;
            }
            userData.Session = new SessionModel();
            MapHeadersInSession(headers, userData.Session);
            await new UserServiceDataLayer().RegisterUserAsync(userData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            userData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return userData;
    }

    /// <summary>
    /// Get Users Temp Token
    /// </summary>
    /// <param name="languageID">user's selected language</param>
    /// <param name="organisationID">permission at level id</param>
    /// <param name="ID">user account id</param>
    /// <param name="headers">object to map device details</param>
    /// <returns>Temp Token based on ID</returns>
    public async Task<AuthDTO> GetPatientTempTokenByIDAsync(byte languageID, long organisationID, long ID, IHeaderDictionary headers)
    {
        AuthDTO userData = new AuthDTO();
        try
        {
            if (languageID < 1 || organisationID < 1 || ID < 1)
            {
                userData.ErrCode = ErrorCode.InvalidData;
                return userData;
            }
            userData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            userData.Session = new SessionModel();
            MapHeadersInSession(headers, userData.Session);
            userData.AccountID = ID;
            userData.LanguageID = languageID;
            userData.OrganisationID = organisationID;
            userData.FeatureFor = FeatureFor;
            await new UserServiceDataLayer().GetPatientTempTokenByIDAsync(userData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            userData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        return userData;
    }

    /// <summary>
    /// Save admin user data in database
    /// </summary>
    /// <param name="languageID">user's selected language</param>
    /// <param name="organisationID">user's organisation id</param>
    /// <param name="userData">object to save user data</param>
    /// <param name="headers">object to map device details</param>
    /// <returns>Operation status and token data</returns>
    public async Task<BaseDTO> RegisterUserAsync(byte languageID, long organisationID, UserDTO userData, IHeaderDictionary headers)
    {
        try
        {
            if (await IsUserDataNotValidAsync(languageID, organisationID, userData).ConfigureAwait(false))
            {
                userData.ErrCode = ErrorCode.InvalidData;
                return userData;
            }
            userData.Session = new SessionModel();
            MapHeadersInSession(headers, userData.Session);
            using (var usersDB = new UserServiceDataLayer())
            {
                await usersDB.RegisterUserAsync(userData).ConfigureAwait(false);
            }
            if (userData.ErrCode == ErrorCode.SMSAuthentication)
            {
                TemplateDTO communicationDto = new TemplateDTO
                {
                    OrganisationID = organisationID,
                    EmailID = userData.User.EmailId,
                    PhoneNumber = userData.User.PhoneNo,
                    LanguageID = languageID,
                    TemplateData = new TemplateModel
                    {
                        TemplateName = TemplateName.EVerifyRegistration,
                        IsExternal = userData.User.IsSelfRegistration,
                        ExternalUserName = userData.User.FirstName,
                        ExternalMobileNo = userData.User.PhoneNo,
                        ExternalEmailID = userData.User.EmailId
                    }
                };
                await SendCommunicationAsync(communicationDto).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            userData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return userData;
    }

    private async Task<bool> IsUserDataNotValidAsync(byte languageID, long organisationID, UserDTO userData)
    {
        if (IsRegistrationDataNotValid(languageID, organisationID, userData))
        {
            return true;
        }
        userData.OrganisationID = organisationID;
        userData.LanguageID = languageID;
        userData.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, userData.LanguageID, default, 0, userData.OrganisationID, false).ConfigureAwait(false)).Settings;

        if (!GenericMethods.IsListNotEmpty(userData.Settings))
        {
            return true;
        }


        if (userData.User.IsSelfRegistration && await IsSelfRegistrationNotAllowedAsync(userData).ConfigureAwait(false))
        {
            return true;
        }


        if ((!string.IsNullOrWhiteSpace(userData.User.AccountPassword) && !Regex.Match(userData.User.AccountPassword, GetSettingValueByKey(userData.Settings, SettingsConstants.S_PASSWORD_REGEX_KEY)).Success))
        {
            return true;
        }


        if (!Regex.Match(userData.User.EmailId, GetSettingValueByKey(userData.Settings, SettingsConstants.S_EMAIL_REGEX_KEY)).Success)
        {
            return true;
        }

        return false;

        ///// Checks : 
        ///// 1. Self registration allowed 
        ///// 2. Password pattern is Valid
        //return !GenericMethods.IsListNotEmpty(userData.Settings)
        //    || (userData.User.IsSelfRegistration && await IsSelfRegistrationNotAllowedAsync(userData).ConfigureAwait(false))
        //    || (!string.IsNullOrWhiteSpace(userData.User.AccountPassword) && !Regex.Match(userData.User.AccountPassword, GetSettingValueByKey(userData.Settings, SettingsConstants.S_PASSWORD_REGEX_KEY)).Success)
        //    || !Regex.Match(userData.User.EmailId, GetSettingValueByKey(userData.Settings, SettingsConstants.S_EMAIL_REGEX_KEY)).Success;
    }

    private bool IsRegistrationDataNotValid(byte languageID, long organisationID, UserDTO userData)
    {
        return languageID < 1 || organisationID < 1 || userData == null || userData.User == null
            || string.IsNullOrWhiteSpace(userData.User.FirstName) || string.IsNullOrWhiteSpace(userData.User.LastName)
            || string.IsNullOrWhiteSpace(userData.User.PhoneNo) || string.IsNullOrWhiteSpace(userData.User.EmailId)
            || (userData.User.IsUser && string.IsNullOrWhiteSpace(userData.User.AccountPassword));
    }

    private async Task<bool> IsSelfRegistrationNotAllowedAsync(UserDTO userData)
    {
        var organisationSettings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, userData.LanguageID, default, 0, userData.OrganisationID, false).ConfigureAwait(false)).Settings;
        userData.Settings.AddRange(organisationSettings);
        string enableRegistration = GetSettingValueByKey(userData.Settings, SettingsConstants.S_ENABLE_SELF_REGISTRATION_KEY);
        if (!string.IsNullOrWhiteSpace(enableRegistration))
        {
            bool aa = Convert.ToBoolean(enableRegistration, CultureInfo.InvariantCulture);
            return !aa;
        }
        return true;
    }

    /// <summary>
    /// Get users from database
    /// </summary>
    /// <param name="languageID">user's selected language</param>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="userID">user id</param>
    /// <param name="selectedOrganisationID">selected organization id</param>
    /// <param name="recordCount">number of record count to fetch</param>
    /// <returns>Operation status and list of users</returns>
    public async Task<BaseDTO> GetUsersAsync(byte languageID, long permissionAtLevelID, long userID, long selectedOrganisationID, long recordCount, AppPermissions viewFor)
    {
        UserDTO userData = new UserDTO();
        try
        {
            if (languageID < 1 || permissionAtLevelID < 1)
            {
                userData.ErrCode = ErrorCode.InvalidData;
                return userData;
            }
            if (AccountID < 1)
            {
                userData.ErrCode = ErrorCode.Unauthorized;
                return userData;
            }
            userData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            if (await GetConfigurationDataAsync(userData, languageID).ConfigureAwait(false))
            {
                userData.AccountID = AccountID;
                userData.RecordCount = recordCount;
                userData.LanguageID = languageID;
                userData.PermissionAtLevelID = permissionAtLevelID;
                userData.ViewFor = viewFor;
                userData.User = new UserModel
                {
                    UserID = userID,
                    SelectedOrganisationID = selectedOrganisationID,
                };
                userData.FeatureFor = FeatureFor;
                await new UserServiceDataLayer().GetUsersAsync(userData).ConfigureAwait(false);
                if (userData.ErrCode == ErrorCode.OK)
                {
                    await ReplaceUserImageCdnLinkAsync(userData.Users);

                    if (userData.RecordCount == -1)
                    {
                        //replace cdnlink of User/patient sample upload excel sheet
                        userData.SampleFilePath = string.Format(Constants.BULK_UPLOAD_SAMPLE_FILE, userData.ViewFor.ToString());
                        userData.SampleFilePath = await ReplaceCDNLinkAsync(userData.SampleFilePath, new BaseDTO());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            userData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        return userData;
    }

    /// <summary>
    /// Save users to database
    /// </summary>
    /// <param name="languageID">language id</param>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="userData">user data to be saved</param>
    /// <returns>Operation status</returns>
    public async Task<BaseDTO> SaveUserAsync(byte languageID, long permissionAtLevelID, UserDTO userData)
    {
        try
        {
            if (AccountID < 1 || languageID < 1 || permissionAtLevelID < 0)
            {
                userData.ErrCode = ErrorCode.InvalidData;
                return userData;
            }
            userData.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
            userData.LanguageID = languageID;
            if (await GetSettingsResourcesAsync(userData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_USER_PROFILE_PAGE_GROUP}," +
                    $"{GroupConstants.RS_LOGIN_FLOW_PAGE_GROUP},{GroupConstants.RS_CAREGIVER_PAGE_GROUP}," +
                    $"{GroupConstants.RS_GENDER_TYPE_GROUP},{GroupConstants.RS_BLOOD_TYPE_GROUP}," +
                    $"{GroupConstants.RS_USER_DEGREES_GROUPS}").ConfigureAwait(false))
            {
                if (!await ValidateDataAsync(userData.User, userData.Resources))
                {
                    userData.ErrCode = ErrorCode.InvalidData;
                    return userData;
                }
            }
            else
            {
                return userData;
            }
            bool isValid = true;
            if (!userData.IsCompleteProfileFlow)
            {
                ValidateUserData(userData, out isValid);
            }
            if (isValid)
            {
                if (userData.User.UserID <= 0 && userData.ViewFor != AppPermissions.LinkedUsersView)
                {
                    userData.User.AccountPassword = GenerateTemporaryPassword();
                    userData.User.IsTempPassword = true;
                }
                userData.AccountID = AccountID;
                userData.PermissionAtLevelID = permissionAtLevelID;
                userData.FeatureFor = FeatureFor;
                await new UserServiceDataLayer().SaveUserAsync(userData).ConfigureAwait(false);
                if (userData.ErrCode == ErrorCode.OK)
                {
                    await UploadUserImagesAsync(userData).ConfigureAwait(false);
                    if (userData.ErrCode == ErrorCode.OK)
                    {
                        await new UserServiceDataLayer().SaveUserAsync(userData).ConfigureAwait(false);
                    }
                    await SendOneLinkAsync(userData).ConfigureAwait(false);
                }
            }
            else
            {
                userData.ErrCode = ErrorCode.InvalidData;
                return userData;
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            userData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return userData;
    }

    private async Task UploadUserImagesAsync(UserDTO userData)
    {
        if (!string.IsNullOrWhiteSpace(userData.User.ImageName))
        {
            FileUploadDTO files = CreateSingleFileDataObject(FileTypeToUpload.ProfileImages,
                                        userData.User.UserID.ToString(CultureInfo.InvariantCulture),
                                        userData.User.ImageName);
            files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
            userData.ErrCode = files.ErrCode;
            if (files.ErrCode == ErrorCode.OK)
            {
                userData.User.ImageName = GetFirstBase64File(files);
                await new UserServiceDataLayer().SaveUserAsync(userData).ConfigureAwait(false);
            }
        }
    }

    internal async Task ReplaceUserImageCdnLinkAsync(List<UserModel> users)
    {
        if (GenericMethods.IsListNotEmpty(users))
        {
            BaseDTO cdnCacheData = new BaseDTO();
            foreach (var user in users)
            {
                if (!string.IsNullOrWhiteSpace(user.ImageName))
                {
                    user.ImageName = await ReplaceCDNLinkAsync(user.ImageName, cdnCacheData);
                }
            }
        }
    }

    /// <summary>
    /// Delete users from database
    /// </summary>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="userData">user data to be delete</param>
    /// <returns>Operation status</returns>
    public async Task<BaseDTO> DeleteUserAsync(long permissionAtLevelID, UserDTO userData)
    {
        try
        {
            if (permissionAtLevelID < 1 || userData.User == null || userData.User.UserID < 1)
            {
                userData.ErrCode = ErrorCode.InvalidData;
                return userData;
            }
            userData.AccountID = AccountID;
            userData.PermissionAtLevelID = permissionAtLevelID;
            userData.FeatureFor = FeatureFor;
            await new UserServiceDataLayer().DeleteUserAsync(userData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            userData.ErrCode = ErrorCode.ErrorWhileDeletingRecords;
        }
        return userData;
    }

    /// <summary>
    /// Resend activation link to selected user
    /// </summary>
    /// <param name="languageID">user's language id</param>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="userData">user data</param>
    /// <returns>Operation status</returns>
    public async Task<BaseDTO> ResendActivationAsync(byte languageID, long permissionAtLevelID, UserDTO userData)
    {
        try
        {
            if (languageID < 1 || permissionAtLevelID < 1 || userData.User == null || userData.User.UserID < 1)
            {
                userData.ErrCode = ErrorCode.InvalidData;
                return userData;
            }
            if (AccountID < 1)
            {
                userData.ErrCode = ErrorCode.Unauthorized;
                return userData;
            }
            userData.LanguageID = languageID;
            userData.PermissionAtLevelID = permissionAtLevelID;
            userData.User.AccountPassword = GenerateTemporaryPassword();
            userData.User.IsTempPassword = true;
            userData.AccountID = AccountID;
            userData.FeatureFor = FeatureFor;
            await new UserServiceDataLayer().ResendActivationAsync(userData).ConfigureAwait(false);
            if (userData.ErrCode == ErrorCode.OK && userData.User != null)
            {
                await GenerateOneLinkDataAsync(userData).ConfigureAwait(false);
                TemplateDTO communicationDto = new TemplateDTO();
                communicationDto.PhoneNumber = userData.User.PhoneNo;
                communicationDto.EmailID = userData.User.EmailId;
                communicationDto.LanguageID = languageID;
                communicationDto.AccountID = AccountID;
                communicationDto.OrganisationID = userData.OrganisationID;
                communicationDto.TemplateData = new TemplateModel
                {
                    RegistrationLink = userData.ErrorDescription,
                    TemplateName = TemplateName.EActivateAccount,
                    IsExternal = false,
                };
                await SendCommunicationAsync(communicationDto).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            userData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return userData;
    }

    private async Task SendOneLinkAsync(UserDTO userData)
    {
        if (userData.ErrCode == ErrorCode.OK
            && userData.User.IsTempPassword
            && !string.IsNullOrWhiteSpace(userData.User.AccountPassword))
        {
            await GenerateOneLinkDataAsync(userData).ConfigureAwait(false);
            TemplateDTO communicationDto = new TemplateDTO();
            communicationDto.PhoneNumber = userData.User.PhoneNo;
            communicationDto.LastModifiedBy = userData.User.EmailId;
            communicationDto.OrganisationID = userData.User.OrganisationID;
            communicationDto.AccountID = userData.AccountID;
            communicationDto.TemplateData = new TemplateModel
            {
                RegistrationLink = userData.ErrorDescription,
                TemplateName = userData.ViewFor == AppPermissions.LinkedUsersView ? TemplateName.EActivateAccountSharedUser : TemplateName.EActivateAccount,
                IsExternal = false,
            };
            await SendCommunicationAsync(communicationDto).ConfigureAwait(false);
        }
    }

    private async Task<bool> GetConfigurationDataAsync(UserDTO userData, byte languageID)
    {
        userData.CountryCodes = (await GetDataFromCacheAsync(CachedDataType.Countries, string.Empty, languageID, default, 0, 0, false).ConfigureAwait(false)).CountryCodes;
        if (userData.CountryCodes != null)
        {
            userData.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
            if (userData.Settings != null)
            {
                userData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources,
                    $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_USER_PROFILE_PAGE_GROUP}," +
                    $"{GroupConstants.RS_LOGIN_FLOW_PAGE_GROUP},{GroupConstants.RS_CAREGIVER_PAGE_GROUP}," +
                    $"{GroupConstants.RS_GENDER_TYPE_GROUP},{GroupConstants.RS_BLOOD_TYPE_GROUP}," +
                    $"{GroupConstants.RS_USER_DEGREES_GROUPS}"
                    , languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (userData.Resources != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private async Task GenerateOneLinkDataAsync(UserDTO userData)
    {
        userData.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_ONE_LINK_DETAILS_GROUP, userData.LanguageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
        string baseUrl = GetSettingValueByKey(userData.Settings, SettingsConstants.S_MASTER_ORGANISATION_DOMAIN_PATH_KEY);
        var myConfig = MyConfiguration.GetInstance;
        DynamicLinkModel dynamicLink = new DynamicLinkModel
        {
            FirebaseAppId = myConfig.GetConfigurationValue(ConfigurationConstants.CS_SETTINGS_FIREBASE_APP_ID),
            FirebaseDomain = UrlConstants.FIREBASE_DOMAIN_URL,
            DomainPrefixLink = myConfig.GetConfigurationValue(ConfigurationConstants.CS_SETTINGS_DOMAIN_URI_PREFIX),
            DynamicLinkParameters = string.Format(CultureInfo.InvariantCulture,
            GetSettingValueByKey(userData.Settings, SettingsConstants.S_ONE_LINK_PARAMETERS_KEY), baseUrl, userData.User.OrganisationDomain,
                                WebUtility.UrlEncode(GenerateOneLinkParameters(userData.User)))
        };
        if (baseUrl.Contains(userData.User.OrganisationDomain, StringComparison.InvariantCultureIgnoreCase))
        {
            dynamicLink.DynamicLinkParameters = dynamicLink.DynamicLinkParameters
                                                .Replace(string.Concat(Constants.ORGANISATION_KEY, userData.User.OrganisationDomain), string.Empty, StringComparison.InvariantCultureIgnoreCase);
        }
        BaseDTO result = await GetDynamicLinkAsync(dynamicLink, userData.Settings).ConfigureAwait(false);
        userData.ErrCode = result.ErrCode;
        userData.ErrorDescription = result.AddedBy;
    }

    private string GenerateOneLinkParameters(UserModel user)
    {
        return string.Join(Constants.ONELINK_SEPERATOR_KEY, nameof(UserModel.EmailId)
            + Constants.SYMBOL_EQUAL + Constants.DEFAULT_ENVIRONMENT_KEY + Constants.ENVIRONMENT_SEPERATOR + user.EmailId,
            nameof(UserModel.PhoneNo) + Constants.SYMBOL_EQUAL + user.PhoneNo,
            nameof(UserModel.AccountPassword) + Constants.SYMBOL_EQUAL + user.AccountPassword,
            nameof(UserModel.OrganisationID) + Constants.SYMBOL_EQUAL + user.OrganisationID);
    }

    private async Task<BaseDTO> GetDynamicLinkAsync(DynamicLinkModel dynamicLinkData, List<SettingModel> settings)
    {
        BaseDTO returnResult = new BaseDTO { ErrCode = ErrorCode.InternalServerError };
        HttpClient httpClient = new HttpClient(new HttpClientHandler());
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_CONTENT_TYPE_KEY, Constants.SE_ACCEPT_HEADER_JSON_KEY);
        StringContent requestBody = new StringContent(GetJsonString(dynamicLinkData, settings), System.Text.Encoding.UTF8, Constants.SE_ACCEPT_HEADER_JSON_KEY);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(dynamicLinkData.FirebaseDomain + dynamicLinkData.FirebaseAppId)) { Content = requestBody };
        HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            string data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (data != null)
            {
                returnResult.AddedBy = Convert.ToString(JToken.Parse(data)[Constants.SHORT_LINK_KEY], CultureInfo.InvariantCulture);
                returnResult.ErrCode = ErrorCode.OK;
            }
        }
        return returnResult;
    }

    private string GetJsonString(DynamicLinkModel dynamicLinkData, List<SettingModel> settings)
    {
        StringWriter sw = new StringWriter();
        JsonTextWriter writer = new JsonTextWriter(sw);
        string bundleIdentifier = GetSettingValueByKey(settings, SettingsConstants.S_MOBILE_APP_BUNDLE_IDENTIFIER_KEY);

        writer.WriteStartObject();
        writer.WritePropertyName(Constants.DYNAMIC_LINK_INFO_KEY);
        writer.WriteStartObject();
        writer.WritePropertyName(Constants.DYNAMIC_LINK_DOMAIN_PATH_PREFIX_KEY);
        writer.WriteValue(dynamicLinkData.DomainPrefixLink);
        writer.WritePropertyName(Constants.LINK_KEY);
        writer.WriteValue(dynamicLinkData.DynamicLinkParameters);
        writer.WritePropertyName(Constants.ANDROID_INFO_KEY);
        writer.WriteStartObject();
        writer.WritePropertyName(Constants.ANDROID_PACKAGE_NAME_KEY);
        writer.WriteValue(bundleIdentifier);
        writer.WriteEndObject();
        writer.WritePropertyName(Constants.IOS_INFO_KEY);
        writer.WriteStartObject();
        writer.WritePropertyName(Constants.IOS_BUNDLE_ID_KEY);
        writer.WriteValue(bundleIdentifier);
        writer.WritePropertyName(Constants.IOS_APP_STORE_ID);
        writer.WriteValue(GetSettingValueByKey(settings, SettingsConstants.S_IOS_APP_STORE_ID_KEY));
        writer.WriteEndObject();
        writer.WriteEndObject();
        writer.WriteEndObject();

        return Convert.ToString(sw, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Get Patient Caregivers
    /// </summary>
    /// <param name="languageID">user's selected language</param>
    /// <param name="organisationID">user's organisation id</param>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="selectedUserID">patient id</param>
    /// <param name="patientCareGiverID">care giver id</param>
    /// <param name="recordCount">number of record count to fetch</param>
    /// <returns>Operation status and caregiver data</returns>
    public async Task<BaseDTO> GetPatientCaregiversAsync(byte languageID, long organisationID, long permissionAtLevelID, long selectedUserID, long patientCareGiverID, long recordCount)
    {
        CaregiverDTO caregiverData = new CaregiverDTO();
        try
        {
            if (IsDataNotValid(languageID, organisationID, permissionAtLevelID, selectedUserID))
            {
                caregiverData.ErrCode = ErrorCode.InvalidData;
                return caregiverData;
            }
            if (AccountID < 1)
            {
                caregiverData.ErrCode = ErrorCode.Unauthorized;
                return caregiverData;
            }
            caregiverData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            caregiverData.Settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, languageID, default, 0, organisationID, false).ConfigureAwait(false)).Settings;
            if (GenericMethods.IsListNotEmpty(caregiverData.Settings))
            {
                caregiverData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_CAREGIVER_PAGE_GROUP}", languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(caregiverData.Resources))
                {
                    caregiverData.AccountID = AccountID;
                    caregiverData.RecordCount = recordCount;
                    caregiverData.LanguageID = languageID;
                    caregiverData.OrganisationID = organisationID;
                    caregiverData.PermissionAtLevelID = permissionAtLevelID;
                    caregiverData.SelectedUserID = selectedUserID;
                    caregiverData.Caregiver = new CaregiverModel { PatientCareGiverID = patientCareGiverID };
                    caregiverData.FeatureFor = FeatureFor;
                    await new UserServiceDataLayer().GetPatientCaregiversAsync(caregiverData).ConfigureAwait(false);
                    if (caregiverData.RecordCount == -1 || caregiverData.RecordCount == -11)
                    {
                        caregiverData.CaregiverOptions?.ForEach(item =>
                        {
                            item.OptionText = $"{item.OptionText} {item.GroupName}";
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            caregiverData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        return caregiverData;
    }

    private bool IsDataNotValid(byte languageID, long organisationID, long permissionAtLevelID, long selectedUserID)
    {
        return languageID < 1 || organisationID < 1 || permissionAtLevelID < 1 || selectedUserID < 1;
    }

    public async Task<BaseDTO> SavePatientCaregiverAsync(byte languageID, long permissionAtLevelID, CaregiverDTO caregiver)
    {
        try
        {
            if (languageID < 1 || permissionAtLevelID < 1 || caregiver.SelectedUserID < 1 || caregiver.Caregiver == null)
            {
                caregiver.ErrCode = ErrorCode.InvalidData;
                return caregiver;
            }
            if (caregiver.IsActive)
            {
                caregiver.LanguageID = 1;
                if (await GetSettingsResourcesAsync(caregiver, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_CAREGIVER_PAGE_GROUP}").ConfigureAwait(false))
                {
                    if (!await ValidateDataAsync(caregiver.Caregiver, caregiver.Resources))
                    {
                        caregiver.ErrCode = ErrorCode.InvalidData;
                        return caregiver;
                    }
                }
                else
                {
                    return caregiver;
                }
            }
            caregiver.AccountID = AccountID;
            caregiver.PermissionAtLevelID = permissionAtLevelID;
            caregiver.LanguageID = languageID;
            caregiver.FeatureFor = FeatureFor;
            await new UserServiceDataLayer().SavePatientCaregiverAsync(caregiver).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            caregiver.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return caregiver;
    }

    private DateTimeOffset? GetDateTimeValue(DateTimeOffset? dateTime)
    {
        return dateTime == null ? dateTime : dateTime.Value.UtcDateTime;
    }


    private BaseDTO ValidateUserData(UserDTO userData, out bool isValid)
    {
        isValid = true;
        List<string> Errors = new List<string>();

        if (!Regex.Match(userData.User.FirstName, GetSettingValueByKey(userData.Settings, SettingsConstants.S_ALPHA_REGEX_KEY)).Success)
        {
            isValid = false;
            Errors.Add(string.Format(GetResourceValueByKey(userData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY), nameof(userData.User.FirstName)));
        }
        if (!Regex.Match(userData.User.LastName, GetSettingValueByKey(userData.Settings, SettingsConstants.S_ALPHA_REGEX_KEY)).Success)
        {
            isValid = false;
            Errors.Add(string.Format(GetResourceValueByKey(userData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY), nameof(userData.User.LastName)));
        }

        if (!userData.User.IsLinkedUser)
        {
            if (!Regex.Match(userData.User.EmailId, GetSettingValueByKey(userData.Settings, SettingsConstants.S_EMAIL_REGEX_KEY)).Success)
            {
                isValid = false;
                Errors.Add(string.Format(GetResourceValueByKey(userData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY), nameof(userData.User.EmailId)));
            }

            if (string.IsNullOrWhiteSpace(userData.User.PhoneNo))
            {
                isValid = false;
                Errors.Add(string.Format(GetResourceValueByKey(userData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY), nameof(userData.User.PhoneNo)));
            }
        }

        //userData.RecordCount =-13 is the case of careTaker
        if (userData.RecordCount == -13)
        {
            //BloodGroupID is used for relationID for caretaker
            if (userData.User.RelationID < 1)
            {
                isValid = false;
                Errors.Add(GetResourceValueByKey(userData.Resources, ErrorCode.InvalidData.ToString()));
            }
        }
        else if (userData.User.RoleID != (int)RoleName.CareTaker)
        {
            if (string.IsNullOrWhiteSpace(userData.User.GenderID))
            {
                isValid = false;
                Errors.Add(string.Format(GetResourceValueByKey(userData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY), nameof(userData.User.GenderID)));
            }
            if (userData.User.RoleID != (byte)RoleName.Patient)
            {
                if (userData.User.Doj == default || userData.User.Doj == null)
                {
                    isValid = false;
                    Errors.Add(string.Format(GetResourceValueByKey(userData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY), nameof(userData.User.Doj)));
                }
                else if (userData.User.Doj > DateTime.Today.Date)
                {
                    isValid = false;
                    Errors.Add(GetResourceValueByKey(userData.Resources, ErrorCode.InvalidData.ToString()));
                }
                if (userData.User.OrganisationID < 1)
                {
                    isValid = false;
                    Errors.Add(string.Format(GetResourceValueByKey(userData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY), nameof(userData.User.OrganisationID)));
                }
                if (userData.User.RoleID < 1)
                {
                    isValid = false;
                    Errors.Add(string.Format(GetResourceValueByKey(userData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY), nameof(userData.User.RoleID)));
                }
            }
            else
            {
                if (userData.User.Dob == default || userData.User.Dob == null)
                {
                    isValid = false;
                    Errors.Add(string.Format(GetResourceValueByKey(userData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY), nameof(userData.User.Dob)));
                }
                else if (userData.User.Dob > DateTime.Today.Date)
                {
                    isValid = false;
                    Errors.Add(GetResourceValueByKey(userData.Resources, ErrorCode.InvalidData.ToString()));
                }
                if (!userData.User.IsLinkedUser && userData.User.PrefferedLanguageID < 1)
                {
                    isValid = false;
                    Errors.Add(string.Format(GetResourceValueByKey(userData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY), nameof(userData.User.PrefferedLanguageID)));
                }
            }
            if (userData.User.RoleID != (byte)RoleName.Patient)
            {
                var allowedType = GetSettingValueByKey(userData.Settings, SettingsConstants.S_USER_TYPE_ALLOWED_IN_BULCK_UPLOAD);
                if (!allowedType.Contains(userData.User.RoleID.ToString()))
                {
                    isValid = false;
                    Errors.Add(GetResourceValueByKey(userData.Resources, ErrorCode.InvalidUserType.ToString()));
                }
            }
        }
        if (!isValid)
        {
            userData.ErrorDescription = string.Join(Constants.PIPE_SEPERATOR, Errors);
        }
        return userData;
    }


    private async Task ReadExcel(UserDTO userData, byte languageID, long permissionAtLevelID)
    {
        try
        {
            if (userData.User.IsMobile)
            {
                string FilePath = Path.Combine(Directory.GetCurrentDirectory(), Constants.UPDATED_EXCEL_STATUS);
                var bytes = Convert.FromBase64String(userData.User.ExcelPath);
                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                    using (SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, true))
                    {
                        await MappingExcelData(userData, languageID, permissionAtLevelID, doc);
                        doc.Dispose();
                    }
                    if (File.Exists(FilePath))
                    {
                        // If file found, delete it    
                        File.Delete(FilePath);
                    }
                    File.WriteAllBytes(FilePath, stream.ToArray());
                    stream.Close();
                    byte[] bytesData;
                    Stream updatedStream = File.Open(FilePath, FileMode.Open);
                    using (var memoryStream = new MemoryStream())
                    {
                        updatedStream.CopyTo(memoryStream);
                        bytesData = memoryStream.ToArray();
                    }
                    // AddedBy is used to send base 64 data to UI 
                    userData.AddedBy = Convert.ToBase64String(bytesData);
                }
            }
            else
            {
                var bytes = Convert.FromBase64String(userData.AttachmentBase64);
                MemoryStream contents = new MemoryStream(bytes);
                // Whatever else needs to be done here.
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(contents, true))
                {
                    await MappingExcelData(userData, languageID, permissionAtLevelID, doc);
                    doc.Dispose();
                    byte[] bytesData = contents.ToArray();
                    string baseString = Convert.ToBase64String(bytesData);
                    userData.AddedBy = baseString;
                }
            }
            //CreatedByID is used for Succuss record and
            if (userData.RecordCount == 0)
            {
                return;
            }
            else if (userData.CreatedByID == userData.RecordCount)
            {
                userData.ErrCode = ErrorCode.OK;
            }
            else
            {
                userData.ErrCode = ErrorCode.BulkUploadDataEntryStatus;
            }
        }
        catch (Exception ex)
        {
            LogError($"{userData.User.ExcelPath}:{ex.Message}", ex);
            userData.ErrCode = ErrorCode.WrongExcelDataFormat;
        }
    }

    private string GetCellValue(SpreadsheetDocument document, Cell cell)
    {
        SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
        string value = string.Empty;
        if (cell.CellValue != null)
        {
            value = cell.CellValue.InnerXml;
        }
        else
        {
            value = cell.InnerText;
        }

        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        {
            return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
        }
        else
        {
            return value;
        }
    }

    private int CellReferenceToIndex(Cell cell)
    {
        int index = 0;
        string reference = cell.CellReference.ToString().ToUpper();
        foreach (char ch in reference)
        {
            if (Char.IsLetter(ch))
            {
                int value = (int)ch - (int)'A';
                index = (index == 0) ? value : ((index + 1) * 26) + value;
            }
            else
                return index;
        }
        return index;
    }

    private Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
    {
        Worksheet worksheet = worksheetPart.Worksheet;
        SheetData sheetData = worksheet.GetFirstChild<SheetData>();
        string cellReference = columnName + rowIndex;

        // If the worksheet does not contain a row with the specified row index, insert one.
        Row row;
        if (sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() != 0)
        {
            row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
        }
        else
        {
            row = new Row() { RowIndex = rowIndex };
            sheetData.Append(row);
        }

        // If there is not a cell with the specified column name, insert one.  
        if (row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
        {
            return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
        }
        else
        {
            // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
            Cell refCell = null;
            foreach (Cell cell in row.Elements<Cell>())
            {
                if (string.Compare(cell.CellReference.Value, cellReference, true) > 0)
                {
                    refCell = cell;
                    break;
                }
            }

            Cell newCell = new Cell() { CellReference = cellReference };
            row.InsertBefore(newCell, refCell);

            worksheet.Save();
            return newCell;
        }
    }

    private async Task MappingExcelData(UserDTO userData, byte languageID, long permissionAtLevelID, SpreadsheetDocument doc)
    {
        WorkbookPart workbookPart = doc.WorkbookPart;
        Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();
        var isUser = userData.ViewFor == AppPermissions.UserView;
        foreach (Sheet thesheet in thesheetcollection.OfType<Sheet>())
        {
            Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;
            WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(thesheet.Id);
            SheetData theSheetdata = theWorksheet.GetFirstChild<SheetData>();
            IEnumerable<Row> rows = theSheetdata.Descendants<Row>();
            int columnCount = 0;
            foreach (Cell cell in rows.ElementAt(0))
            {
                columnCount++;
                if (cell.CellReference.ToString().Contains(isUser ? "H" : "L"))
                {
                    break;
                }
            }
            foreach (Row row in rows)
            {
                uint rowCount = Convert.ToUInt32(row.RowIndex, CultureInfo.InvariantCulture);
                List<string> rowList = new List<string>();
                for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                {
                    if (i > 11)
                    {
                        break;
                    }
                    Cell cell = row.Descendants<Cell>().ElementAt(i);
                    int actualCellIndex = CellReferenceToIndex(cell);
                    if (actualCellIndex - rowList.Count >= 1)
                    {
                        while ((actualCellIndex > rowList.Count))
                        {
                            rowList.Insert(rowList.Count, string.Empty);
                        }
                        rowList.Insert(actualCellIndex, GetCellValue(doc, cell));
                    }
                    else
                    {
                        rowList.Insert(actualCellIndex, GetCellValue(doc, cell));
                    }

                }
                if (rowCount != 1)
                {
                    if (columnCount == (isUser ? 8 : 12))
                    {
                        var emptyRow = rowList.FirstOrDefault(x => string.IsNullOrWhiteSpace(rowList[0]) && string.IsNullOrWhiteSpace(rowList[1])
                          && string.IsNullOrWhiteSpace(rowList[2]) && string.IsNullOrWhiteSpace(rowList[3]) && string.IsNullOrWhiteSpace(rowList[4])
                          && string.IsNullOrWhiteSpace(rowList[5]) && string.IsNullOrWhiteSpace(rowList[6]) && string.IsNullOrWhiteSpace(rowList[7])
                          && string.IsNullOrWhiteSpace(rowList[8]) && string.IsNullOrWhiteSpace(rowList[9]) && string.IsNullOrWhiteSpace(rowList[10])
                          );
                        if (emptyRow == null)
                        {
                            if (rowList?.Count > 0)
                            {
                                userData.RecordCount++;
                                userData.User = new UserModel
                                {
                                    UserID = 0,
                                    FirstName = Convert.ToString(rowList[0]),
                                    MiddleName = Convert.ToString(rowList[1]),
                                    LastName = Convert.ToString(rowList[2]),
                                    GenderID = Convert.ToString(rowList[3]),
                                    OrganisationID = userData.User.OrganisationID,
                                    RoleAtLevelID = userData.User.OrganisationID,
                                    ProffessionID = default,
                                    ImageName = "",
                                    IsActive = true,
                                    OrganisationDomain = userData.User.OrganisationDomain,
                                    UserAge = Convert.ToByte(rowList[5]),
                                    EmailId = isUser ? Convert.ToString(rowList[9]) : Convert.ToString(rowList[11]),
                                    PhoneNo = isUser ? Convert.ToString(rowList[8]) : Convert.ToString(rowList[10]),
                                    Doj = isUser ? GetDateTimeValue(GenericMethods.ConvertStringToDateFormat(rowList[6])) : null,
                                    RoleID = isUser ? (string.IsNullOrWhiteSpace(rowList[7]) ? default : Convert.ToByte(rowList[7], CultureInfo.InvariantCulture)) : Convert.ToByte(RoleName.Patient),
                                    SocialSecurityNo = isUser ? default : Convert.ToString(rowList[7]),
                                    GeneralMedicalIdenfier = isUser ? default : Convert.ToString(rowList[9]),
                                    HospitalIdenfier = isUser ? default : Convert.ToString(rowList[8]),
                                    Dob = isUser ? GetDateTimeValue(GenericMethods.ConvertStringToDateFormat(rowList[4])) : GetDateTimeValue(GenericMethods.ConvertStringToDateFormat(rowList[4])),
                                    BloodGroupID = isUser ? default : string.IsNullOrWhiteSpace(rowList[5]) ? default : Convert.ToInt16(rowList[5]),
                                    PrefferedLanguageID = isUser ? default : string.IsNullOrWhiteSpace(rowList[6]) ? default : Convert.ToByte(rowList[6])
                                };
                                if (isUser)
                                {
                                    userData.User.EmailId = Convert.ToString(rowList[9]);
                                    userData.User.PhoneNo = Convert.ToString(rowList[8]);
                                    userData.User.Doj = GetDateTimeValue(GenericMethods.ConvertStringToDateFormat(rowList[6]));
                                    userData.User.RoleID = string.IsNullOrWhiteSpace(rowList[7]) ? default : Convert.ToByte(rowList[7], CultureInfo.InvariantCulture);
                                    userData.User.SocialSecurityNo = string.Empty;
                                    userData.User.GeneralMedicalIdenfier = string.Empty;
                                    userData.User.HospitalIdenfier = string.Empty;
                                    userData.User.UserAge = Convert.ToByte(rowList[5]);
                                    userData.User.Dob = GetDateTimeValue(GenericMethods.ConvertStringToDateFormat(rowList[4]));
                                    userData.User.BloodGroupID = default;
                                    userData.User.PrefferedLanguageID = default;
                                }
                                else
                                {
                                    userData.User.EmailId = Convert.ToString(rowList[11]);
                                    userData.User.PhoneNo = Convert.ToString(rowList[10]);
                                    userData.User.Doj = null;
                                    userData.User.RoleID = Convert.ToByte(RoleName.Patient);
                                    userData.User.SocialSecurityNo = Convert.ToString(rowList[7]);
                                    userData.User.GeneralMedicalIdenfier = Convert.ToString(rowList[9]);
                                    userData.User.HospitalIdenfier = Convert.ToString(rowList[8]);
                                    userData.User.Dob = GetDateTimeValue(GenericMethods.ConvertStringToDateFormat(rowList[4]));
                                    userData.User.BloodGroupID = string.IsNullOrWhiteSpace(rowList[5]) ? default : Convert.ToInt16(rowList[5]);
                                    userData.User.PrefferedLanguageID = string.IsNullOrWhiteSpace(rowList[6]) ? default : Convert.ToByte(rowList[6]);
                                }
                                string status = string.Empty;
                                userData.ErrorDescription = string.Empty;
                                await SaveUserAsync(languageID, permissionAtLevelID, userData).ConfigureAwait(false);
                                if (userData.ErrCode == ErrorCode.OK || string.IsNullOrWhiteSpace(userData.ErrorDescription))
                                {
                                    if (userData.ErrCode == ErrorCode.OK)
                                    {
                                        userData.CreatedByID++;
                                        status = GetResourceValueByKey(userData.Resources, ResourceConstants.R_SUCCESS_TEXT_KEY);
                                    }
                                    else
                                    {
                                        status = userData.ErrCode.ToString();
                                    }
                                }
                                else
                                {
                                    status = userData.ErrorDescription;
                                }
                                Cell workingCell = InsertCellInWorksheet(isUser ? "I" : "M", rowCount, worksheetPart);
                                workingCell.CellValue = new CellValue(status);
                                workingCell.DataType = new EnumValue<CellValues>(CellValues.String);
                                worksheetPart.Worksheet.Save();
                            }
                        }
                    }
                    else
                    {
                        userData.ErrCode = ErrorCode.WrongExcelDataFormat;
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Save users to database
    /// </summary>
    /// <param name="languageID">language id</param>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="userData">user data to be saved</param>
    /// <returns>Operation status</returns>
    public async Task<BaseDTO> SaveUsersFromExcelAsync(byte languageID, long permissionAtLevelID, UserDTO userData)
    {
        try
        {
            if (languageID < 1 || permissionAtLevelID < 0)
            {
                userData.ErrCode = ErrorCode.InvalidData;
                return userData;
            }
            if (AccountID < 1)
            {
                userData.ErrCode = ErrorCode.Unauthorized;
                return userData;
            }
            await ReadExcel(userData, languageID, permissionAtLevelID);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            userData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return userData;
    }
}