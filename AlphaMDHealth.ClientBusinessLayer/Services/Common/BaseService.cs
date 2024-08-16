using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Reflection;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class BaseService : BaseLibService
{
    /// <summary>
    /// Instance of http service implementation
    /// </summary>
    public IHttpService HttpService { get; }

    /// <summary>
    /// Base for all service classes
    /// </summary>
    public BaseService(IEssentials serviceEssentials) : base(serviceEssentials)
    {
        HttpService = new HttpService(serviceEssentials);
        GenericMethods._essentials = serviceEssentials;
    }

    /// <summary>
    /// Sets resources to _pageResources in case of web
    /// </summary>
    /// <param name="resources">list of resources</param>
    protected void SetPageResources(List<ResourceModel> resources)
    {
        PageData.Resources = resources;
    }

    /// <summary>
    /// Sets settings to _pageSettings in case of web
    /// </summary>
    /// <param name="settings">list of settings</param>
    protected void SetPageSettings(List<SettingModel> settings)
    {
        PageData.Settings = settings;
    }

    /// <summary>
    /// Sets permissions to_pagepermissions in case of web
    /// </summary>
    /// <param name="featurePermissions">list of settings</param>
    protected void SetFeaturePermissions(List<OrganizationFeaturePermissionModel> featurePermissions)
    {
        PageData.FeaturePermissions = featurePermissions;
    }

    /// <summary>
    /// Sets resources and settings in page data from given object
    /// </summary>
    /// <param name="pageData">data to set</param>
    protected void SetResourcesAndSettings(BaseDTO pageData)
    {
        SetPageResources(pageData.Resources);
        SetPageSettings(pageData.Settings);
    }

    /// <summary>
    /// Sets resources and settings in page data from given object
    /// </summary>
    /// <param name="pageData">data to set</param>
    protected void SetCommonData(BaseDTO pageData) 
    {
        SetResourcesAndSettings(pageData);
        SetFeaturePermissions(pageData.FeaturePermissions);
    }

    /// <summary>
    /// Sets common data ito given dto
    /// </summary>
    /// <param name="pageData">data to set</param>
    protected void SetCommonPageData(BaseDTO pageData)
    {
        pageData.FeaturePermissions = PageData.FeaturePermissions;
        pageData.Settings = PageData.Settings;
        pageData.Resources = PageData.Resources;
    }

    /// <summary>
    /// Gets resources for the given group
    /// </summary>
    /// <param name="groups">list of groups</param>
    public async Task GetResourcesAsync(params string[] groups)
    {
        await GetResourcesAsync(true, groups).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets resources for the given group
    /// </summary>
    /// <param name="getCommonResources">Bool to decided whether common resources are to be fetched or not</param>
    /// <param name="groups">list of groups</param>
    public async Task GetResourcesAsync(bool getCommonResources, params string[] groups)
    {
        await new ResourceService(_essentials).GetResourcesAsync(PageData, getCommonResources, groups).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves resources for given resource key
    /// </summary>
    /// <param name="resourceKey">Key for which resources to be retrieved</param>
    /// <returns>Resource model for requested key</returns>
    public async Task GetResourceAsync(string resourceKey)
    {
        await new ResourceService(_essentials).GetResourceAsync(PageData, resourceKey).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets settings for the given group
    /// </summary>
    /// <param name="groups">list of groups</param>
    public async Task GetSettingsAsync(params string[] groups)
    {
        await new SettingService(_essentials).GetSettingsAsync(PageData, groups).ConfigureAwait(false);
    }

    /// <summary>
    /// Finds a particular setting based on key from settingData.
    /// </summary>
    /// <param name="settingKey">key for which setting to be fetched.</param>
    /// <returns>setting value for specified setting key.</returns>
    public async Task<string> GetSettingsValueByKeyAsync(string settingKey)
    {
        return await new SettingService(_essentials).GetSettingsValueByKeyAsync(settingKey).ConfigureAwait(false);
    }

    /// <summary>
    /// Finds a particular setting based on key from settingData.
    /// </summary>
    /// <param name="settingKey">key for which setting to be fetched.</param>
    /// <returns>setting value for specified setting key.</returns>
    public string GetSettingsValueByKey(string settingKey)
    {
        return LibSettings.GetSettingValueByKey(PageData?.Settings, settingKey);
    }

    ///// <summary>
    ///// Get Error description based on resources key
    ///// </summary>
    ///// <param name="errorCodeResourceKey">Gets resource value based on this key</param>
    ///// <returns>Language specific Error description</returns>
    //protected string GetErrorDescription(string errorCodeResourceKey)
    //{
    //    string errorDescription = LibResources.GetResourceValueByKey(PageData?.Resources, errorCodeResourceKey);
    //    if (string.IsNullOrWhiteSpace(errorDescription))
    //    {
    //        errorDescription = LibConstants.ERROR_WHILE_RETRIEVING_RECORDS;
    //    }
    //    return errorDescription;
    //}

    /// <summary>
    /// Retrieves features from database for given group list
    /// </summary>
    /// <param name="featureData">Object to hold feature data with operation status</param>
    /// <param name="groupList">List of groups for which feature has to be retrieved</param>
    /// <returns>List of all features by given grouplist</returns>
    public async Task GetFeaturesAsync(params string[] groupList)
    {
        await new FeatureDatabase().GetFeaturesAsync(PageData, groupList.ToList()).ConfigureAwait(false);
    }

    /// <summary>
    /// Check feature permission
    /// </summary>
    /// <param name="featureCode">feature permission identifier</param>
    /// <returns>bool value</returns>
    public bool CheckFeaturePermissionByCode(string featureCode)
    {
        if (GenericMethods.IsListNotEmpty(PageData?.FeaturePermissions))
        {
            return PageData.FeaturePermissions.Any(x => x.FeatureCode.Trim() == featureCode);
        }
        return false;
    }

    /// <summary>
    /// Return the string datetime format required for service input
    /// </summary>
    /// <param name="inputDateTime">The datetime to be formated</param>
    /// <returns>the formated datetime value</returns>
    protected string GetSyncDateTimeString(DateTimeOffset inputDateTime)
    {
        return inputDateTime.ToString("O", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Fetch user id
    /// </summary>
    /// <returns>id of user for which operation is being performed</returns>
    public long GetUserID()
    {
        var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
        var userID = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker ? 0
            : _essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        return userID == 0 ? GetLoginUserID() : userID;
    }

    /// <summary>
    /// Fetch user id
    /// </summary>
    /// <param name="isPatientPage">Flag representing is patient page</param>
    /// <returns>id of user for which operation is being performed</returns>
    public long GetUserID(bool isPatientPage)
    {
        return isPatientPage
            ? _essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)
            : GetLoginUserID();
    }

    public long GetLoginUserID()
    {
        return _essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0);
    }

    /// <summary>
    /// Fetch permission at level id value from preferences
    /// </summary>
    /// <returns>Permission at level id value</returns>
    public string GetPermissionAtLevelID()
    {
        return Convert.ToString(_essentials.GetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, 0), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Provides next arrow
    /// </summary>
    /// <returns></returns>
    public string GetNextIcon()
    {
        return _essentials.GetPreferenceValue<bool>(StorageConstants.PR_IS_RIGHT_ALIGNED_KEY, false)
            ? ImageConstants.I_ARROW_MENU_ICON_RTL_PNG
            : ImageConstants.I_ARROW_MENU_ICON_LTR_PNG;
    }

    /// <summary>
    /// Get name initials
    /// </summary>
    /// <param name="names">Actual name</param>
    /// <returns>Name initials</returns>
    protected string GetInitials(string names)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(names))
            {
                return string.Empty;
            }
            // Extract the first character out of each block of non-whitespace
            string[] nameArray = names.Trim().Split(' ');
            return (string.Format(CultureInfo.CurrentCulture, "{0}{1}", nameArray[0].Substring(0, 1), (nameArray.Length > 1)
                ? nameArray[nameArray.Length - 1].Substring(0, 1) : string.Empty))?.ToUpper(CultureInfo.CurrentCulture);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            return string.Empty;
        }
    }

    /// <summary>
    /// Get user credentials from dynamic link data
    /// </summary>
    /// <param name="authData">object to transfer data</param>
    /// <returns>User credentials and operation status</returns>
    public void GetDynamicLinkData(AuthDTO authData)
    {
        try
        {
            string[] parameters = GenericMethods.GetOneLinkParameters(authData.AddedBy);
            authData.AuthenticationData = new AuthModel();
            authData.AuthenticationData.EmailID = parameters[0].Split(new[] { nameof(UserModel.EmailId) + Constants.SYMBOL_EQUAL }, StringSplitOptions.None).Last();
            authData.AuthenticationData.PhoneNo = parameters[1].Split(new[] { nameof(UserModel.PhoneNo) + Constants.SYMBOL_EQUAL }, StringSplitOptions.None).Last();
            authData.AuthenticationData.AccountPassword = parameters[2].Split(new[] { nameof(UserModel.AccountPassword) + Constants.SYMBOL_EQUAL }, StringSplitOptions.None).Last();
            authData.AuthenticationData.UserOrganisationID = Convert.ToInt64(parameters[3].Split(new[] { nameof(UserModel.OrganisationID) + Constants.SYMBOL_EQUAL }, StringSplitOptions.None).Last(), CultureInfo.InvariantCulture);
            authData.AuthenticationData.UserName = authData.AuthenticationData.EmailID;
            authData.OrganisationID = authData.AuthenticationData.UserOrganisationID;
            authData.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            authData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Clears account data
    /// </summary>
    /// <returns>operation status</returns>
    public async Task ClearAccountTokensAndIdAsync()
    {
        await new AuthService(_essentials).CleanUserNotificationsAsync(false).ConfigureAwait(false);
        _essentials.DeletePreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY);
        _essentials.DeletePreferenceValue(StorageConstants.PR_ROLE_ID_KEY);
        _essentials.DeletePreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY);
        _essentials.DeletePreferenceValue(StorageConstants.PR_SELECTED_USER_ID_KEY);
        _essentials.DeletePreferenceValue(StorageConstants.PR_IS_HEALTH_ACCOUNT_CONNECTED_KEY);
        _essentials.SetPreferenceValue(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, Constants.DEFAULT_ORGANISATION_ID);
        await ClearAccountTokensAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Update max length of resource key based on the given setting key
    /// </summary>
    /// <param name="baseData">Data from which resource is to be updated</param>
    /// <param name="lengthKey">Setting key from which length is to be fetched</param>
    /// <param name="resourceKey">Resource key in which length is to be updated</param>
    public void UpdateMaxMinLength(BaseDTO baseData, string lengthKey, string resourceKey)
    {
        var lenSetting = baseData.Settings.Find(x => x.SettingKey == lengthKey);
        if (lenSetting != null)
        {
            byte pinCodeLength = Convert.ToByte(lenSetting.SettingValue, CultureInfo.InvariantCulture);
            ResourceModel resource = baseData.Resources.FirstOrDefault(x => x.ResourceKey == resourceKey);
            if (resource != null)
            {
                resource.MaxLength = resource.MinLength = pinCodeLength;
            }
        }
    }

    protected SessionModel MapSessionData(BaseDTO responseData)
    {
        SessionModel sessionData = new SessionModel();
        JToken data = JToken.Parse(responseData.Response);
        if (data != null && data.HasValues)
        {
            data = data[nameof(SessionDTO.Session)];
            if (data != null)
            {
                sessionData.AccessToken = Convert.ToString(data[nameof(SessionModel.AccessToken)], CultureInfo.InvariantCulture);
                sessionData.RefreshToken = Convert.ToString(data[nameof(SessionModel.RefreshToken)], CultureInfo.InvariantCulture);
                sessionData.AccountID = Convert.ToInt64(data[nameof(SessionModel.AccountID)], CultureInfo.InvariantCulture);
            }
        }
        return sessionData;
    }

    protected async Task SaveTokensAsync(SessionModel tokenData)
    {
        await SaveSecuredValueAsync(StorageConstants.SS_ACCESS_TOKEN_KEY, tokenData.AccessToken).ConfigureAwait(false);
        await SaveSecuredValueAsync(StorageConstants.SS_REFRESH_TOKEN_KEY, tokenData.RefreshToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get hardcoded resources
    /// </summary>
    /// <param name="resourceData">Reference to hold values</param>
    /// <param name="key">Key to render on page</param>
    public void GetDefaultText(BaseDTO resourceData, string key, byte languageID)
    {
        resourceData.Resources = new List<ResourceModel>();
        switch (key.ToEnum<ErrorCode>())
        {
            case ErrorCode.NoInternetConnection:
                AddResource(resourceData, key, languageID, ImageConstants.I_NO_INTERNET_PNG, Constants.NO_INTERNET_CONNECTION_RESOURCE_VALUE_TEXT, Constants.NO_INTERNET_CONNECTION_PLACEHOLDER_VALUE_TEXT);
                break;
            case ErrorCode.UpdateApp:
                AddResource(resourceData, key, languageID, ImageConstants.I_UPDATE_APP_SVG, Constants.UPGRADE_REQUIRED_RESOURCE_VALUE_TEXT, Constants.UPGRADE_REQUIRED_PLACEHOLDER_VALUE_TEXT);
                break;
            case ErrorCode.RecordCountMismatch:
                AddResource(resourceData, key, languageID, ImageConstants.I_CONFIGURATION_MISMATCH_SVG, Constants.RECORD_COUNT_MISMATCH_RESOURCE_VALUE_TEXT, Constants.RECORD_COUNT_MISMATCH_PLACEHOLDER_VALUE_TEXT);
                break;
            case ErrorCode.Unauthorized:
                AddResource(resourceData, key, languageID, ImageConstants.I_UNAUTHORIZED_SVG, Constants.UNAUTHORIZED_RESOURCE_VALUE_TEXT, Constants.UNAUTHORIZED_PLACEHOLDER_VALUE_TEXT);
                break;
            case ErrorCode.UnknownCertificate:
                AddResource(resourceData, key, languageID, ImageConstants.I_UNAUTHORIZED_SVG, Constants.UNKNOWN_CERTIFICATE_RESOURCE_VALUE_TEXT, Constants.UNKNOWN_CERTIFICATE_PLACEHOLDER_VALUE_TEXT);
                break;
            case ErrorCode.JailBrokenDevice:
                AddResource(resourceData, key, languageID, ImageConstants.I_RESTART_APP_SVG, Constants.JAIL_BROKEN_DEVICE_RESOURCE_VALUE_TEXT, Constants.JAIL_BROKEN_DEVICE_PLACEHOLDER_VALUE_TEXT);
                break;
            case ErrorCode.ServiceUnavailable:
                AddResource(resourceData, key, languageID, ImageConstants.I_RESTART_APP_SVG, Constants.SERVICE_UNAVAILABLE_RESOURCE_VALUE_TEXT, Constants.SERVICE_UNAVAILABLE_PLACEHOLDER_VALUE_TEXT);
                break;
            case ErrorCode.LanguageNotAvailable:
                AddResource(resourceData, key, languageID, ImageConstants.I_RESTART_APP_SVG, Constants.LANGUAGE_NOT_AVAILABLE_RESOURCE_VALUE_TEXT, Constants.LANGUAGE_NOT_AVAILABLE_PLACEHOLDER_VALUE_TEXT);
                break;
            default:
                AddResource(resourceData, key, languageID, ImageConstants.I_RESTART_APP_SVG, Constants.RESTART_APP_RESOURCE_VALUE_TEXT, Constants.RESTART_APP_PLACEHOLDER_VALUE_TEXT);
                break;
        }
        AddResource(resourceData, ResourceConstants.R_NEXT_ACTION_KEY, languageID, default, Constants.TRY_AGAIN_ACTION_TEXT, default);
        AddResource(resourceData, ResourceConstants.R_APPLICATION_NAME_KEY, languageID, default, Constants.APPLICATION_NAME_TEXT, default);
        AddResource(resourceData, ResourceConstants.R_OK_ACTION_KEY, languageID, default, Constants.OK_TEXT, default);
    }

    /// <summary>
    /// Get Organisation Details
    /// </summary>
    /// <param name="OrganisationAddress">Hold Organisation Address</param>
    /// <param name="OrganisationContact">hold Organisation Contact</param>
    /// <returns>OrganisationDetails</returns>
    public string GetOrganisationDetails(string OrganisationAddress, string OrganisationContact)
    {
        string OrganisationDetail = OrganisationAddress + Environment.NewLine + OrganisationContact;
        return OrganisationDetail;
    }

    private void AddResource(BaseDTO resourceData, string key, byte languageID, string icon, string val, string placeholder)
    {
        resourceData.Resources.Add(new ResourceModel { LanguageID = languageID, ResourceKey = key, KeyDescription = icon, ResourceValue = val, PlaceHolderValue = placeholder });
    }

    /// <summary>
    /// Add placeholder to the list
    /// </summary>
    /// <param name="options"></param>
    public void AddPlaceHolder(List<OptionModel> options, params long[] selectedIDs)
    {
        options.InsertRange(0, new List<OptionModel> { new OptionModel{
            OptionID = -1,
            OptionText = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DROP_DOWN_PLACE_HOLDER_KEY),
            ParentOptionID = -1,
            IsSelected = selectedIDs.Length > 0 && selectedIDs.Contains(-1)
        }});
    }

    /// <summary>
    /// Get badge count
    /// </summary>
    /// <param name="target">target for which badge is to be updated</param>
    /// <returns>badge count</returns>
    public async Task<string> GetBadgeCountAsync(string target)
    {
        if (target == Pages.ChatsPage.ToString())
        {
            return await new ChatService(_essentials).GetUnreadCountAsync().ConfigureAwait(false);
        }
        if (target == Pages.PatientTasksPage.ToString())
        {
            return await new PatientTaskService(_essentials).GetUnreadCountAsync().ConfigureAwait(false);
        }
        return string.Empty;
    }

    /// <summary>
    /// Gets OptionList for display in picker
    /// </summary>
    /// <typeparam name="T">Type of input list</typeparam>
    /// <param name="data"></param>
    /// <param name="optionIDKey">Parameter which will be used to map OptionID</param>
    /// <param name="optionTextKey">Parameter which will be used to map OptionID</param>
    /// <param name="selectedID">Selected ID from the list</param>
    /// <param name="shouldAddPlaceHolder">Should show placeholder or not</param>
    /// <param name="parentIDKey">Parameter which will be used to map ParentOptionID</param>
    /// <returns>Returns OptionList for display in picker</returns>
    protected List<OptionModel> GetPickerSource<T>(List<T> data, string optionIDKey, string optionTextKey, long selectedID, bool shouldAddPlaceHolder, string parentIDKey)
    {
        PropertyInfo idPropertyInfo = typeof(T).GetProperty(optionIDKey);
        PropertyInfo namePropertyInfo = typeof(T).GetProperty(optionTextKey);
        PropertyInfo parentIDPropertyInfo = null;
        if (parentIDKey != null)
        {
            parentIDPropertyInfo = typeof(T).GetProperty(parentIDKey);
        }
        List<OptionModel> options = (from dataItem in data
                                     select new OptionModel
                                     {
                                         OptionID = Convert.ToInt64(idPropertyInfo.GetValue(dataItem), CultureInfo.InvariantCulture),
                                         OptionText = Convert.ToString(namePropertyInfo.GetValue(dataItem)),
                                         ParentOptionID = parentIDPropertyInfo == null ? default : Convert.ToInt64(parentIDPropertyInfo.GetValue(dataItem), CultureInfo.InvariantCulture),
                                         IsSelected = Convert.ToInt64(idPropertyInfo.GetValue(dataItem), CultureInfo.InvariantCulture) == selectedID,
                                     }).ToList();
        if (shouldAddPlaceHolder)
        {
            options.InsertRange(0, new List<OptionModel> { new OptionModel
            {
                OptionID = -1,
                OptionText = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DROP_DOWN_PLACE_HOLDER_KEY),
                ParentOptionID = -1
            } });
        }
        return options;
    }

    /// <summary>
    /// Gets OptionList for display in picker
    /// </summary>
    /// <param name="data">JToken Data</param>
    /// <param name="tokenValue">Item that contains list of options to match</param>
    /// <param name="optionIDKey">Parameter which whill be used to map OptionID</param>
    /// <param name="optionTextKey">Parameter which whill be used to map OptionID</param>
    /// <param name="selectedID">Selected ID from the list</param>
    /// <param name="shouldAddPlaceHolder">Should show placeholder or not</param>
    /// <param name="parentIDKey">Parameter which whill be used to map ParentOptionID</param>
    /// <param name="parentOptionKey ">Parameter which whill be used to map ParentOptionKey</param>
    /// <returns>Returns OptionList for display in picker</returns>
    protected List<OptionModel> GetPickerSource(JToken data, string tokenValue, string optionIDKey, string optionTextKey, long selectedID, bool shouldAddPlaceHolder
        , string parentIDKey, string parentOptionKey = null)
    {
        List<OptionModel> options = data[tokenValue].Any()
            ? (from dataItem in data[tokenValue]
               select new OptionModel
               {
                   OptionID = (long)dataItem[optionIDKey],
                   OptionText = (string)dataItem[optionTextKey],
                   ParentOptionID = IsValueAvailable(parentIDKey, dataItem),
                   ParentOptionText = string.IsNullOrWhiteSpace(parentOptionKey) ? string.Empty : GetDataItem<string>(dataItem, parentOptionKey),
                   IsSelected = (long)dataItem[optionIDKey] == selectedID || GetDataItem<bool>(dataItem, nameof(OptionModel.IsSelected)),
                   GroupName = (string)dataItem[nameof(OptionModel.GroupName)] ?? string.Empty,
                   IsDefault = GetDataItem<bool>(dataItem, nameof(OptionModel.IsDefault)),
                   IsDisabled = GetDataItem<bool>(dataItem, nameof(OptionModel.IsDisabled))
               }).ToList()
            : new List<OptionModel>();
        if (shouldAddPlaceHolder)
        {
            AddPlaceHolder(options);
        }
        return options;
    }

    private long IsValueAvailable(string parentIDKey, JToken dataItem)
    {
        return parentIDKey == null ? default : (long)dataItem[parentIDKey];
    }

    /// <summary>
    /// Map options data
    /// </summary>
    /// <param name="groupName">name of resource group</param>
    /// <param name="selectedKey">selected item key</param>
    /// <param name="description">descriptio to match</param>
    /// <param name="shouldAddPlaceHolder">Should show placeholder or not</param>
    /// <returns>List of option model</returns>
    protected List<OptionModel> MapResourcesIntoOptions(string groupName, string selectedKey, string description, bool shouldAddPlaceHolder)
    {
        var options = (from dataItem in PageData.Resources
                       where dataItem.GroupName == groupName && (string.IsNullOrWhiteSpace(description) || dataItem.KeyDescription == description)
                       select new OptionModel
                       {
                           OptionID = dataItem.ResourceID,
                           OptionText = dataItem.ResourceValue,
                           GroupName = dataItem.ResourceKey,
                           IsSelected = !string.IsNullOrWhiteSpace(selectedKey) && dataItem.ResourceKey == selectedKey,
                       }).ToList();
        if (options == null || options.Count < 1)
        {
            options = new List<OptionModel>();
        }
        if (shouldAddPlaceHolder)
        {
            AddPlaceHolder(options);
        }
        return options;
    }

    /// <summary>
    /// Map options data
    /// </summary>
    /// <param name="groupName">name of resource group</param>
    /// <param name="selectedIDs">selected item key</param>
    /// <param name="description">descriptio to match</param>
    /// <param name="shouldAddPlaceHolder">Should show placeholder or not</param>
    /// <returns>List of option model</returns>
    protected List<OptionModel> MapResourcesIntoOptionsByKeyID(string groupName, string description, bool shouldAddPlaceHolder, params long[] selectedIDs)
    {
        List<OptionModel> options = new List<OptionModel>();
        if (shouldAddPlaceHolder)
        {
            AddPlaceHolder(options, selectedIDs);
        }
        var otherOptions = (from dataItem in PageData.Resources
                            where dataItem.GroupName == groupName && (string.IsNullOrWhiteSpace(description) || dataItem.KeyDescription == description)
                            select new OptionModel
                            {
                                OptionID = dataItem.ResourceKeyID,
                                OptionText = dataItem.ResourceValue,
                                GroupName = dataItem.ResourceKey,
                                IsSelected = selectedIDs?.Length > 0 && selectedIDs.Contains(dataItem.ResourceKeyID)
                            }).ToList();
        if (GenericMethods.IsListNotEmpty(otherOptions))
        {
            options.AddRange(otherOptions);
        }
        return options;
    }

    /// <summary>
    /// Map options data
    /// </summary>
    /// <param name="groupID">ID of resource group</param>
    /// <param name="selectedIDs">selected item key</param>
    /// <param name="description">descriptio to match</param>
    /// <param name="shouldAddPlaceHolder">Should show placeholder or not</param>
    /// <returns>List of options for given group ID</returns>
    public List<OptionModel> MapResourcesIntoOptionsByGroupID(short groupID, string description, bool shouldAddPlaceHolder, params long[] selectedIDs)
    {
        List<OptionModel> options = new List<OptionModel>();
        if (shouldAddPlaceHolder)
        {
            AddPlaceHolder(options, selectedIDs);
        }
        if (PageData.Resources != null)
        {

            var otherOptions = (from dataItem in PageData.Resources
                                where dataItem.GroupID == groupID && (string.IsNullOrWhiteSpace(description) || dataItem.KeyDescription == description)
                                select new OptionModel
                                {
                                    OptionID = dataItem.ResourceKeyID,
                                    OptionText = dataItem.ResourceValue,
                                    GroupName = dataItem.ResourceKey,
                                    IsSelected = selectedIDs.Length > 0 && selectedIDs.Contains(dataItem.ResourceKeyID)
                                }).ToList();


            if (GenericMethods.IsListNotEmpty(otherOptions))
            {
                options.AddRange(otherOptions);
            }
        }
        return options ?? new List<OptionModel>();
    }

    //Todo:
    ///// <summary>
    ///// get icon and image source based on file type 
    ///// </summary>
    ///// <param name="fileType">file type</param>
    ///// <param name="source">base 64 string</param>
    ///// <param name="imageSource">out put paramate to return Imagesource of base 64</param>
    ///// <returns>return Imagesource of base 64 and Default Icon both</returns>
    //internal string GetFileIcon(FileExtension fileType, string source, out ImageSource imageSource)
    //{
    //	string defaultIcon = string.Empty;
    //	imageSource = null;
    //	switch (fileType)
    //	{
    //		case FileExtension.pdf:
    //			return ImageConstants.I_PDF_UPLOAD_MOBILE_SVG;
    //		case FileExtension.doc:
    //		case FileExtension.docx:
    //			return ImageConstants.I_DOC_UPLOAD_MOBILE_SVG;
    //		case FileExtension.xls:
    //		case FileExtension.xlsx:
    //			return ImageConstants.I_XSL_UPLOAD_MOBILE_SVG;
    //		case FileExtension.jpeg:
    //		case FileExtension.jpg:
    //		case FileExtension.png:
    //			imageSource = ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(source));
    //			return defaultIcon;
    //		default:
    //			return ImageConstants.I_DEFAULT_UPLOAD_ICON;
    //	}
    //}
}