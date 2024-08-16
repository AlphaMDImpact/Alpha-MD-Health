using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class OrganisationServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Organisation service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public OrganisationServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get organisation profile data
        /// </summary>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organiationID">organisation id</param>
        /// <param name="languageID">language id</param>
        /// <returns>Organisation profile data with operation status</returns>
        public async Task<OrganisationDTO> GetOrganisationProfileAsync(long permissionAtLevelID, long organiationID, byte languageID)
        {
            OrganisationDTO organisationData = new OrganisationDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    organisationData.ErrCode = ErrorCode.InvalidData;
                    return organisationData;
                }
                organisationData.AccountID = AccountID;
                if (organisationData.AccountID < 1)
                {
                    organisationData.ErrCode = ErrorCode.Unauthorized;
                    return organisationData;
                }
                organisationData.OrganisationID = organiationID;
                organisationData.LanguageID = languageID;
                organisationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetConfigurationDataAsync(organisationData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_PROFILE_GROUP},{GroupConstants.RS_LOGIN_FLOW_PAGE_GROUP},{GroupConstants.RS_USER_PROFILE_PAGE_GROUP},{GroupConstants.RS_CARE_FLIX_SERVICE_GROUP}").ConfigureAwait(false))
                {
                    organisationData.PermissionAtLevelID = permissionAtLevelID;
                    organisationData.FeatureFor = FeatureFor;
                    await new OrganisationServiceDataLayer().GetOrganisationProfileAsync(organisationData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                organisationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return organisationData;
        }

        /// <summary>
        /// To save organisation profile 
        /// </summary>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organisationData">reference object which holds organisation data</param>
        /// <returns>Organisation ID with Operation status</returns>
        public async Task<OrganisationDTO> SaveOrganisationProfileAsync(long permissionAtLevelID, OrganisationDTO organisationData)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || organisationData == null)
                {
                    organisationData ??= new OrganisationDTO();
                    organisationData.ErrCode = ErrorCode.InvalidData;
                    return organisationData;
                }
                if (organisationData.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(organisationData.Languages))
                    {
                        organisationData.ErrCode = ErrorCode.InvalidData;
                        return organisationData;
                    }
                    organisationData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(organisationData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_PROFILE_GROUP},{GroupConstants.RS_LOGIN_FLOW_PAGE_GROUP},{GroupConstants.RS_CARE_FLIX_SERVICE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(organisationData.OrganisationProfile, organisationData.Resources))
                        {
                            organisationData.ErrCode = ErrorCode.InvalidData;
                            return organisationData;
                        }
                        if (organisationData.OrganisationProfile.IsManageOrganisation)
                        {
                            if(organisationData.OrganisationProfile.PlanID <= 0)
                            {
                                organisationData.ErrCode = ErrorCode.InvalidData;
                                return organisationData;
                            }
                            foreach (var organisationExternalService in organisationData.OrganisationExternalServices)
                            {
                                if (!await ValidateDataAsync(organisationExternalService, organisationData.Resources))
                                {
                                    organisationData.ErrCode = ErrorCode.InvalidData;
                                    return organisationData;
                                }
                            }
                        }
                    }
                    else
                    {
                        return organisationData;
                    }
                }
                organisationData.AccountID = AccountID;
                organisationData.PermissionAtLevelID = permissionAtLevelID;
                organisationData.OrganisationProfile.OrganisationDomain = organisationData.OrganisationProfile.OrganisationDomain.ToLower(CultureInfo.InvariantCulture).Trim();
                organisationData.FeatureFor = FeatureFor;
                await new OrganisationServiceDataLayer().SaveOrganisationProfileAsync(organisationData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                organisationData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return organisationData;
        }


        /// <summary>
        /// Organisation Settings or themes
        /// </summary>
        /// <param name="languageID">language Id</param>
        /// <param name="permissionAtLevelID"></param>
        /// <param name="groupName">Name Of Group which to be fetched</param>
        /// <returns>Organisation Settings or themes data</returns>
        public async Task<BaseDTO> GetOrganisationSettingsAsync(byte languageID, long permissionAtLevelID, string groupName)
        {
            BaseDTO settings = new BaseDTO();
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1 || string.IsNullOrWhiteSpace(groupName))
                {
                    settings.ErrCode = ErrorCode.InvalidData;
                    return settings;
                }
                settings.AccountID = AccountID;
                if (settings.AccountID < 1)
                {
                    settings.ErrCode = ErrorCode.Unauthorized;
                    return settings;
                }
                settings.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetConfigurationDataAsync(settings, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_SETTINGS_GROUP},{GroupConstants.RS_ORGANISATION_THEMES_STYLES_GROUP}").ConfigureAwait(false))
                {
                    BaseDTO organisationSettings = new BaseDTO
                    {
                        PermissionAtLevelID = permissionAtLevelID,
                        RecordCount = 2, // Fetch the data of given Setting Group
                        AccountID = settings.AccountID,
                        OrganisationID = settings.OrganisationID,
                        ErrorDescription = groupName,
                        AddedBy = groupName == GroupConstants.RS_ORGANISATION_SETTINGS_GROUP ? AppPermissions.OrganisationSettingsView.ToString() : AppPermissions.OrganisationThemeSettingsView.ToString(),
                        LastModifiedBy = groupName == GroupConstants.RS_ORGANISATION_SETTINGS_GROUP ? AppPermissions.OrganisationSettingsAddEdit.ToString() : AppPermissions.OrganisationThemeSettingsAddEdit.ToString()
                    };
                    if (GenericMethods.IsListNotEmpty(settings.Settings))
                    {
                        settings.Settings?.OrderBy(x => x.GroupName);
                        var orgSettingToRemovet = settings.Settings.Where(x => x.GroupName == GroupConstants.RS_ORGANISATION_SETTINGS_GROUP).ToList();
                        var indexOfFirst = settings.Settings.IndexOf(settings.Settings.FirstOrDefault(x => x.GroupName == GroupConstants.RS_ORGANISATION_SETTINGS_GROUP));
                        if (indexOfFirst > 0)
                        {
                            settings.Settings.RemoveRange(indexOfFirst, orgSettingToRemovet.Count);
                        }
                    }
                    organisationSettings.FeatureFor = FeatureFor;
                    await new SettingServiceDataLayer().GetSettingsAsync(organisationSettings).ConfigureAwait(false);
                    settings.ErrCode = organisationSettings.ErrCode;
                    if (organisationSettings.ErrCode == ErrorCode.OK)
                    {
                        MapSettingData(settings, organisationSettings);
                        await ReplaceSettingImageCdnLinkAsync(settings);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                settings.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return settings;
        }

        private void MapSettingData(BaseDTO settings, BaseDTO organisationSettings)
        {
            if (GenericMethods.IsListNotEmpty(settings.Settings))
            {
                settings.Settings.AddRange(organisationSettings.Settings);
            }
            else
            {
                settings.Settings = organisationSettings.Settings;
            }
            settings.FeaturePermissions = organisationSettings.FeaturePermissions;
        }

        /// <summary>
        /// Updates Organisation Settings
        /// </summary>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="groupName">Name of group that is to be saved</param>
        /// <param name="settings">List of settings that needed to be updated</param>
        /// <returns>Result of operation</returns>
        public async Task<BaseDTO> UpdateOrganisationSettingsAsync(long permissionAtLevelID, string groupName, BaseDTO settings)
        {
            try
            {
                if (permissionAtLevelID < 1 || string.IsNullOrWhiteSpace(groupName)
                    || !GenericMethods.IsListNotEmpty(settings.Settings))
                {
                    settings.ErrCode = ErrorCode.InvalidData;
                    return settings;
                }
                settings.AccountID = AccountID;
                if (settings.AccountID < 1)
                {
                    settings.ErrCode = ErrorCode.Unauthorized;
                    return settings;
                }
                settings.AddedBy = groupName == GroupConstants.RS_ORGANISATION_SETTINGS_GROUP
                    ? AppPermissions.OrganisationSettingsAddEdit.ToString()
                    : AppPermissions.OrganisationThemeSettingsAddEdit.ToString();
                settings.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                settings.PermissionAtLevelID = permissionAtLevelID;
                settings.FeatureFor = FeatureFor;
                await UploadAndDeleteImagesAsync(settings).ConfigureAwait(false);
                if (settings.ErrCode == ErrorCode.OK)
                {
                    settings.Settings.Where(setting => setting.SettingType == SettingType.Image.ToString()).ToList().ForEach(setting => setting.SettingValue = string.IsNullOrWhiteSpace(setting.SettingValue) ? string.Empty : setting.SettingValue);
                    await new OrganisationServiceDataLayer().UpdateOrganisationSettingsAsync(settings).ConfigureAwait(false);
                    if(settings.ErrCode == ErrorCode.OK) 
                    {
                        //clear settings from cache  cacheKey = cacheDataType.ToString() + organisationId.ToString()
                        RedisCacheBusinessLayer.ClearCacheKey(CachedDataType.OrganisationSettings.ToString() + settings.OrganisationID.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                settings.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return settings;
        }

        private async Task UploadAndDeleteImagesAsync(BaseDTO settings)
        {
            var orgID = settings.Settings?.FirstOrDefault()?.OrganisationID.ToString(CultureInfo.InvariantCulture);
            FileUploadDTO files = CreateFileDataObject(FileTypeToUpload.OrganisationLogos, orgID);
            files.FileContainers[0].FileData = (from setting in settings.Settings
                                                where setting.SettingType == SettingType.Image.ToString()
                                                    && setting.IsActive
                                                    && !string.IsNullOrWhiteSpace(setting.SettingValue)
                                                select CreateFileObject(setting.SettingID.ToString(CultureInfo.InvariantCulture), string.IsNullOrWhiteSpace(setting.SettingValue) ? string.Empty : setting.SettingValue, false)).ToList();
            if (files.FileContainers[0].FileData?.Count > 0)
            {
                files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
                settings.ErrCode = files.ErrCode;
                if (settings.ErrCode == ErrorCode.OK)
                {
                    foreach (var setting in settings.Settings)
                    {
                        if (setting.SettingType == SettingType.Image.ToString())
                        {
                            setting.SettingValue = setting.IsActive
                              ? GetBase64FileFromFirstContainer(files, setting.SettingID.ToString(CultureInfo.InvariantCulture))
                              : string.Empty;
                        }
                    }
                }
            }
            else
            {
                settings.ErrCode = ErrorCode.OK;
            }
        }

        /// <summary>
        /// Get Organisation Branches
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="branchID">Id of Branch</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of Organisation Branches</returns>
        public async Task<BranchDTO> GetOrganisationBranchesAsync(byte languageID, long permissionAtLevelID, long branchID, long recordCount)
        {
            BranchDTO branchData = new BranchDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    branchData.ErrCode = ErrorCode.InvalidData;
                    return branchData;
                }
                if (AccountID < 1)
                {
                    branchData.ErrCode = ErrorCode.Unauthorized;
                    return branchData;
                }
                branchData.AccountID = AccountID;
                branchData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetConfigurationDataAsync(branchData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_PROFILE_GROUP}").ConfigureAwait(false))
                {
                    branchData.PermissionAtLevelID = permissionAtLevelID;
                    branchData.LanguageID = languageID;
                    branchData.RecordCount = recordCount;
                    branchData.Branch = new BranchModel { BranchID = branchID };
                    branchData.FeatureFor = FeatureFor;
                    await new OrganisationServiceDataLayer().GetOrganisationBranchesAsync(branchData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                branchData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return branchData;
        }

        /// <summary>
        /// Save Organisation Branch data to database
        /// </summary>
        /// <param name="branchData">Branch data to be saved</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <returns>Result of operation</returns>
        public async Task<BaseDTO> SaveOrganisationBranchAsync(BranchDTO branchData, long permissionAtLevelID)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1 || branchData.Branches == null)
                {
                    branchData.ErrCode = ErrorCode.InvalidData;
                    return branchData;
                }
                if (branchData.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(branchData.Branches))
                    {
                        branchData.ErrCode = ErrorCode.InvalidData;
                        return branchData;
                    }
                    if(!GenericMethods.IsListNotEmpty(branchData.Departments))
                    {
                        branchData.ErrCode = ErrorCode.InvalidData;
                        return branchData;
                    }
                    branchData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(branchData, false, string.Empty, $"{GroupConstants.RS_ORGANISATION_PROFILE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(branchData.Branches, branchData.Resources))
                        {
                            branchData.ErrCode = ErrorCode.InvalidData;
                            return branchData;
                        }
                    }
                    else
                    {
                        return branchData;
                    }
                }
                branchData.AccountID = AccountID;
                branchData.PermissionAtLevelID = permissionAtLevelID;
                branchData.FeatureFor = FeatureFor;
                await new OrganisationServiceDataLayer().SaveOrganisationBranchAsync(branchData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                branchData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return branchData;
        }

        private async Task<bool> GetConfigurationDataAsync(BaseDTO baseDTO, byte languageID, string groupNames)
        {
            baseDTO.LanguageID = languageID;
            return await GetSettingsResourcesAsync(baseDTO, true, GroupConstants.RS_COMMON_GROUP, groupNames);
        }

        /// <summary>
        /// Gets organisation view data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organiationID">Selected user OrganisationID</param>
        /// <param name="recordCount">For organisation data decision</param>
        /// <returns>Organisation view data</returns>
        public async Task<OrganisationDTO> GetOrganisationViewAsync(byte languageID, long permissionAtLevelID, long organiationID, long recordCount)
        {
            OrganisationDTO organisationData = new OrganisationDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || organiationID < 1)
                {
                    organisationData.ErrCode = ErrorCode.InvalidData;
                    return organisationData;
                }
                organisationData.AccountID = AccountID;
                if (organisationData.AccountID < 1)
                {
                    organisationData.ErrCode = ErrorCode.Unauthorized;
                    return organisationData;
                }
                organisationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                organisationData.OrganisationID = organiationID;
                organisationData.LanguageID = languageID;
                if (await GetOrgConfigurationDataAsync(organisationData, languageID).ConfigureAwait(false))
                {
                    organisationData.RecordCount = recordCount;
                    organisationData.PermissionAtLevelID = permissionAtLevelID;
                    organisationData.FeatureFor = FeatureFor;
                    await new OrganisationServiceDataLayer().GetOrganisationViewAsync(organisationData).ConfigureAwait(false);
                    if (organisationData.ErrCode == ErrorCode.OK)
                    {
                        var cdnCacheData = new BaseDTO();
                        foreach (var setting in organisationData.Settings)
                        {
                            if (setting.SettingKey == SettingsConstants.S_LOGO_KEY && !string.IsNullOrWhiteSpace(setting.SettingValue))
                            {
                                setting.SettingValue = await ReplaceCDNLinkAsync(setting.SettingValue, cdnCacheData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                organisationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return organisationData;
        }

        private async Task<bool> GetOrgConfigurationDataAsync(BaseDTO baseDTO, byte languageID)
        {
            baseDTO.Settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_THEMES_STYLES_GROUP},{GroupConstants.RS_ORGANISATION_SETTINGS_GROUP}", languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
            if (baseDTO.Settings != null)
            {
                baseDTO.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_PROFILE_GROUP}"
                    , languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (baseDTO.Resources != null)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Get organisations data
        /// </summary>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organiationID">organisation id</param>
        /// <param name="languageID">language id</param>
        /// <param name="recordCount">No of records to return</param>
        /// <returns>Return Organisation data and operation status</returns>
        public async Task<OrganisationDTO> GetOrganisationsAsync(long permissionAtLevelID, long organiationID, byte languageID, long recordCount)
        {
            OrganisationDTO organisationData = new OrganisationDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    organisationData.ErrCode = ErrorCode.InvalidData;
                    return organisationData;
                }
                organisationData.AccountID = AccountID;
                if (organisationData.AccountID < 1)
                {
                    organisationData.ErrCode = ErrorCode.Unauthorized;
                    return organisationData;
                }
                organisationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                organisationData.OrganisationID = organiationID;
                organisationData.LanguageID = languageID;
                if (await GetConfigurationDataAsync(organisationData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_PROFILE_GROUP},{GroupConstants.RS_LOGIN_FLOW_PAGE_GROUP}").ConfigureAwait(false))
                {
                    organisationData.RecordCount = recordCount;
                    organisationData.PermissionAtLevelID = permissionAtLevelID;
                    organisationData.FeatureFor = FeatureFor;
                    await new OrganisationServiceDataLayer().GetOrganisationsAsync(organisationData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                organisationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return organisationData;
        }
    }
}