using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public partial class BaseServiceBusinessLayer
    {
        /// <summary>
        /// Replace dumumy cdn link setting with actual cdn link
        /// </summary>
        /// <param name="settingData">data to replace</param>
        /// <returns></returns>
        protected async Task ReplaceSettingImageCdnLinkAsync(BaseDTO settingData)
        {
            if (GenericMethods.IsListNotEmpty(settingData.Settings))
            {
                BaseDTO cdnCacheData = new BaseDTO();
                foreach (var setting in settingData.Settings)
                {
                    if (setting.IsActive && setting.SettingType == SettingType.Image.ToString() && !string.IsNullOrWhiteSpace(setting.SettingValue))
                    {
                        setting.SettingValue = await ReplaceCDNLinkAsync(setting.SettingValue, cdnCacheData);
                    }
                }
            }
        }

        protected async Task ReplaceResourcesImageCdnLinkAsync(BaseDTO baseData)
        {
            if (GenericMethods.IsListNotEmpty(baseData.Resources))
            {
                BaseDTO cdnCacheData = new BaseDTO();
                foreach (var resource in baseData.Resources)
                {
                    if (resource.IsActive)
                    {
                        if (!string.IsNullOrWhiteSpace(resource.ResourceValue) && resource.ResourceValue.Contains(Constants.DUMMY_BLOB_URL_LINK))
                        {
                            resource.ResourceValue = await ReplaceCDNLinkAsync(resource.ResourceValue, cdnCacheData);
                        }
                        if (!string.IsNullOrWhiteSpace(resource.PlaceHolderValue) && resource.ResourceValue.Contains(Constants.DUMMY_BLOB_URL_LINK))
                        {
                            resource.PlaceHolderValue = await ReplaceCDNLinkAsync(resource.PlaceHolderValue, cdnCacheData);
                        }
                        if (!string.IsNullOrWhiteSpace(resource.InfoValue) && resource.ResourceValue.Contains(Constants.DUMMY_BLOB_URL_LINK))
                        {
                            resource.InfoValue = await ReplaceCDNLinkAsync(resource.InfoValue, cdnCacheData);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fetch Resources and Setting for given group
        /// </summary>
        /// <param name="responseobj">Reference Object to store resources and settings</param>
        /// <param name="fetchOrgSetting">Do Need to fetch organization setting for which data needs to fetch</param>
        /// <param name="settingGroups">Comma separated setting groups for which settings need to fetched</param>
        /// <param name="resourceGroups">Comma separated resource groups for which resources need to fetched</param>
        /// <returns>Flag which return operation status</returns>
        protected async Task<bool> GetSettingsResourcesAsync(BaseDTO responseobj, bool fetchOrgSetting, string settingGroups, string resourceGroups)
        {
            responseobj.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            if (!string.IsNullOrWhiteSpace(settingGroups))
            {
                responseobj.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, settingGroups, responseobj.LanguageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
                if (!GenericMethods.IsListNotEmpty(responseobj.Settings))
                {
                    return false;
                }
            }
            if (fetchOrgSetting)
            {
                var orgSettings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, responseobj.LanguageID, default, 0, responseobj.OrganisationID, false).ConfigureAwait(false)).Settings;
                if (!GenericMethods.IsListNotEmpty(orgSettings))
                {
                    return false;
                }
                if (GenericMethods.IsListNotEmpty(responseobj.Settings))
                {
                    responseobj.Settings.AddRange(orgSettings);
                }
                else
                {
                    responseobj.Settings = orgSettings;
                }
            }
            if (!string.IsNullOrWhiteSpace(resourceGroups))
            {
                responseobj.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, resourceGroups, responseobj.LanguageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (!GenericMethods.IsListNotEmpty(responseobj.Resources))
                {
                    return false;
                }
            }
            responseobj.ErrCode = ErrorCode.OK;
            return true;
        }

        /// <summary>
        /// Get resources and map other user data
        /// </summary>
        /// <param name="languageID">ID of selected language</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="recordCount">count of records</param>
        /// <param name="data">reference data object</param>
        /// <param name="groups">resource groups</param>
        /// <returns>resource data with other user data mapped</returns>
        protected async Task<bool> GetResourcesAndMapDataAsync(byte languageID, long permissionAtLevelID, long recordCount, BaseDTO data, string groups)
        {
            data.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            data.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, groups, languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
            if (GenericMethods.IsListNotEmpty(data.Resources))
            {
                data.AccountID = AccountID;
                data.FeatureFor = FeatureFor;
                data.LanguageID = languageID;
                data.PermissionAtLevelID = permissionAtLevelID;
                data.RecordCount = recordCount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns list of data from Memory Cache based on a key
        /// </summary>
        /// <param name="cacheDataType">The type of data to be pulled from Cache</param>
        /// <param name="cacheKey">Unique Cache data key</param>
        /// <param name="languageID">Language based data to be pulled</param>
        /// <param name="modifiedOn">last modified values</param>
        /// <param name="userId">UserId</param>
        /// <param name="organisationId">organisation id</param>
        /// <param name="isMobile">fetch data for mobile or web</param>
        /// <returns>list of data from Memory Cache based on a key</returns>
        protected async Task<BaseDTO> GetDataFromCacheAsync(CachedDataType cacheDataType, string cacheKey, byte languageID, DateTimeOffset modifiedOn, long userId, long organisationId, bool isMobile)
        {
            string[] cacheKeys = string.IsNullOrWhiteSpace(cacheKey) ? null : cacheKey.Split(Constants.COMMA_SEPARATOR);
            switch (cacheDataType)
            {
                case CachedDataType.Settings:
                    return await ReturnSettingsAsync(cacheDataType, cacheKeys, modifiedOn, userId, organisationId, isMobile).ConfigureAwait(false);
                case CachedDataType.OrganisationSettings:
                    return await ReturnOrganizationSettingsAsync(cacheDataType, cacheKeys, modifiedOn, userId, organisationId, isMobile).ConfigureAwait(false);
                case CachedDataType.Resources:
                    return await ReturnResourcesAsync(cacheDataType, cacheKeys, languageID, modifiedOn, userId, organisationId, isMobile).ConfigureAwait(false);
                case CachedDataType.Countries:
                    return await ReturnCountriesAsync(cacheDataType, cacheKeys, modifiedOn, userId).ConfigureAwait(false);
                case CachedDataType.Languages:
                    return await GetLanguagesFromCacheAsync(cacheDataType, userId).ConfigureAwait(false);
                case CachedDataType.SystemIdentifiers:
                    return await ReturnSystemIdentifiersAsync(cacheDataType, cacheKeys, userId).ConfigureAwait(false);
                case CachedDataType.BlobStorageCdnLink:
                    return await GetBlobStorageCdnLinkFromCache(cacheDataType).ConfigureAwait(false);
                default:
                    break;
            }
            return new BaseDTO { ErrCode = ErrorCode.NotImplemented };
        }

        private async Task<BaseDTO> ReturnSettingsAsync(CachedDataType cacheDataType, string[] cacheKeys, DateTimeOffset modifiedOn, long accountID, long organisationId, bool isMobile)
        {
            BaseDTO settingData = new BaseDTO
            {
                Settings = (await GetSettingsFromCacheAsync(cacheDataType, accountID, organisationId).ConfigureAwait(false))?
                    .Where(c => (cacheKeys == null || cacheKeys.Contains(c.GroupName.Trim()))
                        && (!isMobile || c.LastModifiedOn >= modifiedOn)
                    ).ToList()
            };
            return settingData;
        }

        private async Task<List<SettingModel>> GetSettingsFromCacheAsync(CachedDataType cacheDataType, long accountID, long organisationId)
        {
            BaseDTO settingData = new BaseDTO
            {
                Settings = RedisCacheBusinessLayer.GetCacheData<List<SettingModel>>(cacheDataType.ToString())
            };
            if (!GenericMethods.IsListNotEmpty(settingData.Settings))
            {
                settingData.RecordCount = 1;
                settingData.AccountID = accountID;
                settingData.OrganisationID = organisationId;
                settingData.ErrorDescription = string.Empty;
                settingData.AddedBy = string.Empty;
                settingData.LastModifiedBy = string.Empty;
                settingData.FeatureFor = FeatureFor;
                //get data from DB
                await new SettingServiceDataLayer().GetSettingsAsync(settingData).ConfigureAwait(false);
                if (settingData.ErrCode == ErrorCode.OK)
                {
                    await ReplaceSettingImageCdnLinkAsync(settingData);
                    //on success save it to cache
                    RedisCacheBusinessLayer.SetCacheData<List<SettingModel>>(cacheDataType.ToString(), settingData.Settings);
                }
            }
            return settingData.Settings;
        }

        private async Task<BaseDTO> ReturnOrganizationSettingsAsync(CachedDataType cacheDataType, string[] cacheKeys, DateTimeOffset modifiedOn, long accountID, long organisationId, bool isMobile)
        {
            BaseDTO settingData = new BaseDTO
            {
                Settings = (await GetOrganisationSettingsFromCacheAsync(cacheDataType, accountID, organisationId).ConfigureAwait(false))?
                    .Where(c => (cacheKeys == null || cacheKeys.Length < 1 || cacheKeys.Contains(c.GroupName.Trim()))
                        && (!isMobile || c.LastModifiedOn >= modifiedOn)
                    ).ToList()
            };
            return settingData;
        }

        private async Task<List<SettingModel>> GetOrganisationSettingsFromCacheAsync(CachedDataType cacheDataType, long accountID, long organisationId)
        {
            string cacheKey = cacheDataType.ToString() + organisationId.ToString();
            BaseDTO settingData = new BaseDTO
            {
                Settings = RedisCacheBusinessLayer.GetCacheData<List<SettingModel>>(cacheKey)
            };
            if (!GenericMethods.IsListNotEmpty(settingData.Settings))
            {
                settingData.RecordCount = 2;
                settingData.AccountID = accountID;
                settingData.OrganisationID = organisationId;
                settingData.ErrorDescription = string.Empty;
                settingData.AddedBy = string.Empty;
                settingData.LastModifiedBy = string.Empty;
                settingData.FeatureFor = FeatureFor;
                //get data from DB
                await new SettingServiceDataLayer().GetSettingsAsync(settingData).ConfigureAwait(false);
                if (settingData.ErrCode == ErrorCode.OK)
                {
                    await ReplaceSettingImageCdnLinkAsync(settingData);
                    //on success save it to cache
                    RedisCacheBusinessLayer.SetCacheData<List<SettingModel>>(cacheKey, settingData.Settings);
                }
            }
            return settingData.Settings;
        }

        private async Task<BaseDTO> ReturnResourcesAsync(CachedDataType cacheDataType, string[] cacheKeys, byte languageID, DateTimeOffset modifiedOn, long accountID, long organisationId, bool isMobile)
        {
            BaseDTO resourceData = new BaseDTO
            {
                Resources = (await GetResourcesFromCacheAsync(cacheDataType, languageID, accountID, organisationId, isMobile).ConfigureAwait(false))?
                    .Where(c => (cacheKeys == null || string.IsNullOrWhiteSpace(c.GroupName) || cacheKeys.Contains(c.GroupName.Trim()))
                        && c.LastModifiedOn >= modifiedOn
                        && CheckResourceFor(c.ResourceKeyFor, isMobile)
                    ).ToList()
            };
            return resourceData;
        }

        private async Task<List<ResourceModel>> GetResourcesFromCacheAsync(CachedDataType cacheDataType, byte languageID, long accountID, long organisationId, bool isMobile)
        {
            string cacheKey = cacheDataType.ToString() + languageID;
            BaseDTO resourceData = new BaseDTO
            {
                Resources = RedisCacheBusinessLayer.GetCacheData<List<ResourceModel>>(cacheKey)
            };
            if (!GenericMethods.IsListNotEmpty(resourceData.Resources))
            {
                resourceData.AccountID = accountID;
                resourceData.LanguageID = languageID;
                resourceData.OrganisationID = organisationId;
                resourceData.IsActive = !isMobile;
                await new ResourceServiceDataLayer().GetResourcesAsync(resourceData).ConfigureAwait(false);
                if (resourceData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(resourceData.Resources))
                {
                    await ReplaceResourcesImageCdnLinkAsync(resourceData);
                    //on success save it to cache
                    RedisCacheBusinessLayer.SetCacheData<List<ResourceModel>>(cacheKey, resourceData.Resources);
                }
            }
            return resourceData.Resources;
        }

        private async Task<BaseDTO> ReturnCountriesAsync(CachedDataType cacheDataType, string[] cacheKeys, DateTimeOffset modifiedOn, long accountID)
        {
            BaseDTO countryData = new BaseDTO
            {
                CountryCodes = (await GetCountriesFromCacheAsync(cacheDataType, accountID).ConfigureAwait(false))?
                    .Where(c => (cacheKeys == null || cacheKeys.Contains(c.CountryCode.Trim()))
                        && c.LastModifiedOn >= modifiedOn
                    ).ToList()
            };
            return countryData;
        }

        private async Task<List<CountryModel>> GetCountriesFromCacheAsync(CachedDataType cacheDataType, long userId)
        {
            BaseDTO countryCodeData = new BaseDTO
            {
                CountryCodes = RedisCacheBusinessLayer.GetCacheData<List<CountryModel>>(cacheDataType.ToString())
            };
            if (!GenericMethods.IsListNotEmpty(countryCodeData.CountryCodes))
            {
                countryCodeData.AccountID = userId;
                await new CountryStatesServiceDataLayer().GetCountriesAsync(countryCodeData).ConfigureAwait(false);
                if (countryCodeData.ErrCode == ErrorCode.OK)
                {
                    RedisCacheBusinessLayer.SetCacheData<List<CountryModel>>(cacheDataType.ToString(), countryCodeData.CountryCodes);
                }
            }
            return countryCodeData.CountryCodes;
        }

        private async Task<LanguageDTO> GetLanguagesFromCacheAsync(CachedDataType cacheDataType, long userId)
        {
            LanguageDTO laguageData = new LanguageDTO
            {
                Languages = RedisCacheBusinessLayer.GetCacheData<List<LanguageModel>>(cacheDataType.ToString())
            };
            if (!GenericMethods.IsListNotEmpty(laguageData.Languages))
            {
                laguageData.AccountID = userId;
                laguageData.FeatureFor = FeatureFor;
                //get data from DB
                await new LanguageServiceDataLayer().GetLanguagesAsync(laguageData, string.Empty, string.Empty).ConfigureAwait(false);
                if (laguageData.ErrCode == ErrorCode.OK)
                {
                    //on success save it to cache
                    RedisCacheBusinessLayer.SetCacheData<List<LanguageModel>>(cacheDataType.ToString(), laguageData.Languages);
                }
            }
            return laguageData;
        }

        private async Task<SystemIdentifierDTO> ReturnSystemIdentifiersAsync(CachedDataType cacheDataType, string[] cacheKeys, long accountID)
        {
            SystemIdentifierDTO systemIdentifierData = new SystemIdentifierDTO
            {
                SystemIdentifiers = (await GetSystemIdentifiersFromCacheAsync(cacheDataType, accountID).ConfigureAwait(false))?
                    .Where(c => cacheKeys == null || cacheKeys.Contains(c.ClientIdentifierKey.Trim())).ToList()
            };
            return systemIdentifierData;
        }

        private async Task<List<SystemIdentifierModel>> GetSystemIdentifiersFromCacheAsync(CachedDataType cacheDataType, long userId)
        {
            SystemIdentifierDTO systemIdentifierData = new SystemIdentifierDTO
            {
                SystemIdentifiers = RedisCacheBusinessLayer.GetCacheData<List<SystemIdentifierModel>>(cacheDataType.ToString())
            };
            if (!GenericMethods.IsListNotEmpty(systemIdentifierData.SystemIdentifiers))
            {
                systemIdentifierData.AccountID = userId;
                //get data from DB
                await new SystemIdentifierServiceDataLayer().GetSystemIdentifiersAsync(systemIdentifierData).ConfigureAwait(false);
                if (systemIdentifierData.ErrCode == ErrorCode.OK)
                {
                    //on success save it to cache
                    RedisCacheBusinessLayer.SetCacheData<List<SystemIdentifierModel>>(cacheDataType.ToString(), systemIdentifierData.SystemIdentifiers);
                }
            }
            return systemIdentifierData.SystemIdentifiers;
        }

        private async Task<BaseDTO> GetBlobStorageCdnLinkFromCache(CachedDataType cacheDataType)
        {
            var blobCdnLinks = new BaseDTO();
            List<string> cdnLinks = RedisCacheBusinessLayer.GetCacheData<List<string>>(cacheDataType.ToString());
            if (!GenericMethods.IsListNotEmpty(cdnLinks) || cdnLinks.Count < 2)
            {
                //get data from microservice
                FileUploadDTO files = CreateFileDataObject(FileTypeToUpload.Default);
                files = await GetDocumentsCdnLinksAsync(files).ConfigureAwait(false);
                if (files.ErrCode == ErrorCode.OK && !string.IsNullOrWhiteSpace(files.AddedBy) && !string.IsNullOrWhiteSpace(files.LastModifiedBy))
                {
                    cdnLinks = new List<string> { files.AddedBy, files.LastModifiedBy };
                    //on success save it to cache
                    RedisCacheBusinessLayer.SetCacheData<List<string>>(cacheDataType.ToString(), cdnLinks);
                }
            }
            if (cdnLinks?.Count > 1)
            {
                blobCdnLinks.AddedBy = cdnLinks[0];
                blobCdnLinks.LastModifiedBy = cdnLinks[1];
            }
            return blobCdnLinks;
        }

        private async Task<BaseDTO> GetBlobStorageCdnLinkFromCache(string FileName)
        {
            var blobCdnLinks = new BaseDTO();
            //List<string> cdnLinks = null;// RedisCacheBusinessLayer.GetCacheData<List<string>>(cacheDataType.ToString());
            //if (!GenericMethods.IsListNotEmpty(cdnLinks) || cdnLinks.Count < 2)
            //{
            //get data from microservice
            FileUploadDTO files = CreateFileDataObjectWithFileName(FileTypeToUpload.Default, FileName);
            files = await GetDocumentsCdnLinksAsync(files).ConfigureAwait(false);
            if (files.ErrCode == ErrorCode.OK && !string.IsNullOrWhiteSpace(files.AddedBy) && !string.IsNullOrWhiteSpace(files.LastModifiedBy))
            {
                //cdnLinks = [files.AddedBy, files.LastModifiedBy, files.EmailID];
                //if (cdnLinks?.Count > 1)
                //{
                blobCdnLinks.AddedBy = files.AddedBy;
                blobCdnLinks.LastModifiedBy = files.LastModifiedBy;
                blobCdnLinks.EmailID = files.EmailID;
                //}
                //on success save it to cache
                //RedisCacheBusinessLayer.SetCacheData<List<string>>(cacheDataType.ToString(), cdnLinks);
            }
            //}
            //if (cdnLinks?.Count > 1)
            //{
            //    blobCdnLinks.AddedBy = cdnLinks[0];
            //    blobCdnLinks.LastModifiedBy = cdnLinks[1];
            //    blobCdnLinks.EmailID = cdnLinks[2];
            //}
            return blobCdnLinks;
        }

        private bool CheckResourceFor(ForPlatform forPlatform, bool isMobile)
        {
            return forPlatform == ForPlatform.Both || (isMobile && forPlatform == ForPlatform.Mobile) || (!isMobile && forPlatform == ForPlatform.Web);
        }
    }
}