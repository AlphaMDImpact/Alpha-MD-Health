using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer
{
    /// <summary>
    /// User account setting service
    /// </summary>
    public class UserAccountSettingService : BaseService
    {
        public UserAccountSettingService(IEssentials essentials):base(essentials) { }
        /// <summary>
        /// Sync User Account Settings data to server
        /// </summary>
        /// <param name="requestData">object to return operation status</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status call</returns>
        public async Task SyncUserAccountSettingsToServerAsync(UserAccountSettingDTO requestData, CancellationToken cancellationToken)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    await new ReadingDatabase().GetUserAccountSettingsForSyncAsync(requestData).ConfigureAwait(false);
                }
                if (GenericMethods.IsListNotEmpty(requestData.UserAccountSettings))
                {
                    var httpData = new HttpServiceModel<UserAccountSettingDTO>
                    {
                        CancellationToken = cancellationToken,
                        PathWithoutBasePath = UrlConstants.SAVE_USER_ACCOUNT_SETTING_SERVICE_ASYNC_PATH,
                        QueryParameters = new NameValueCollection
                        {
                            { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() }
                        },
                        ContentToSend = requestData,
                    };
                    await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                    requestData.ErrCode = httpData.ErrCode;
                    if (requestData.ErrCode == ErrorCode.OK)
                    {
                        JToken data = JToken.Parse(httpData.Response);
                        if (data.HasValues)
                        {
                            if (MobileConstants.IsMobilePlatform)
                            {
                                requestData.UserAccountSettings.ForEach(x => x.IsSynced = true);
                                await new ReadingDatabase().SaveUserAccountSettingsAsync(requestData).ConfigureAwait(false);
                            }
                        }
                        requestData.RecordCount = requestData.UserAccountSettings.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveUserAccountSettingsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                var userAccountSettings = new UserAccountSettingDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    UserAccountSettings = MapUserAccountSettings(data, nameof(DataSyncDTO.UserAccountSettings)),
                };
                if (GenericMethods.IsListNotEmpty(userAccountSettings.UserAccountSettings))
                {
                    await new ReadingDatabase().SaveUserAccountSettingsAsync(userAccountSettings).ConfigureAwait(false);
                    result.RecordCount = userAccountSettings.UserAccountSettings.Count;
                    bool isNotificationEnabled = Convert.ToBoolean(userAccountSettings.UserAccountSettings.FirstOrDefault(x => x.SettingType == UserSettingType.NotificationKey)?.SettingValue, CultureInfo.InvariantCulture);
                    if (isNotificationEnabled != _essentials.GetPreferenceValue<bool>(StorageConstants.PR_IS_NOTIFICATIONS_ALLOWED_KEY, true))
                    {
                        await SetupRemoteNotificationAsync(isNotificationEnabled).ConfigureAwait(false);
                    }
                    _essentials.SetPreferenceValue(StorageConstants.PR_IS_NOTIFICATIONS_ALLOWED_KEY, isNotificationEnabled);
                }
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// Map User Account Settings data
        /// </summary>
        /// <param name="data">User Account Settings json data</param>
        /// <param name="collectionName">collection name</param>
        /// <returns>User Account Settings list</returns>
        internal List<UserAccountSettingsModel> MapUserAccountSettings(JToken data, string collectionName)
        {
            return data[collectionName].Any()
                ? (from dataItem in data[collectionName]
                   select new UserAccountSettingsModel
                   {
                       SettingType = GetDataItem<UserSettingType>(dataItem, nameof(UserAccountSettingsModel.SettingType)),
                       SettingTypeID = GetDataItem<int>(dataItem, nameof(UserAccountSettingsModel.SettingTypeID)),
                       SettingValue = GetDataItem<string>(dataItem, nameof(UserAccountSettingsModel.SettingValue)),
                       ReadingType = GetDataItem<string>(dataItem, nameof(UserAccountSettingsModel.ReadingType)),
                       ReadingTypeID = GetDataItem<int>(dataItem, nameof(UserAccountSettingsModel.ReadingTypeID)),
                       IsActive = GetDataItem<bool>(dataItem, nameof(UserAccountSettingsModel.IsActive)),
                       IsSynced = true,
                   }).ToList()
                : null;
        }

        /// <summary>
        /// Get user accont settings from database
        /// </summary>
        /// <param name="userAccountSettingData">Reference object to return user account seetings</param>
        /// <returns>Operation Status</returns>
        public async Task GetUserAccountSettingsAsync(UserAccountSettingDTO userAccountSettingData)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    userAccountSettingData.LanguageID =  (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                    await Task.WhenAll(
                        GetResourcesAsync(GroupConstants.RS_USER_ACCOUNT_SETTINGS_GROUP, GroupConstants.RS_READINGS_GROUP, GroupConstants.RS_READING_CATEGORY_GROUP),
                        GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_USER_ACCOUNT_SETTINGS_GROUP),
                        GetFeaturesAsync(Utility.AppPermissions.UserAccountSettingsView.ToString()),
                        new ReadingDatabase().GetUserAccountSettingsAsync(userAccountSettingData)
                    ).ConfigureAwait(false);
                    if (GenericMethods.IsListNotEmpty(userAccountSettingData.UserAccountSettings))
                    {
                        var notificationSetting = userAccountSettingData.UserAccountSettings.FirstOrDefault(x => x.SettingType == UserSettingType.NotificationKey);
                        var healthSetting = userAccountSettingData.UserAccountSettings.FirstOrDefault(x => x.SettingType == UserSettingType.HealthDataKey);
                        if (notificationSetting != null)
                        {
                            notificationSetting.IsActive = true;
                            notificationSetting.IsToogled = Convert.ToBoolean(notificationSetting.SettingValue, CultureInfo.InvariantCulture);
                        }
                        if (healthSetting != null)
                        {
                            if (MobileConstants.IsIosPlatform && MobileConstants.IsTablet)
                            {
                                userAccountSettingData.UserAccountSettings.Remove(healthSetting);
                            }
                            else
                            {
                                healthSetting.IsActive = true;
                                healthSetting.IsToogled = Convert.ToBoolean(healthSetting.SettingValue, CultureInfo.InvariantCulture);
                            }
                        }
                    }
                }
                else
                {
                    await SyncUserAccountSettingsFromServerAsync(userAccountSettingData, CancellationToken.None).ConfigureAwait(false);
                }
                if (GenericMethods.IsListNotEmpty(userAccountSettingData.UserAccountSettings))
                {
                    userAccountSettingData.UserAccountSettings.Where(x => x.SettingType == UserSettingType.MeasurementUnitsKey)?.ToList()?.ForEach(item =>
                    {
                        item.IsActive = true;
                        item.ReadingTypeKey = LibResources.GetResourceByKeyID(PageData?.Resources, item.ReadingTypeID)?.ResourceKey;
                        item.ReadingType = LibResources.GetResourceByKeyID(PageData?.Resources, item.ReadingTypeID)?.ResourceValue;
                        var readingUnitOption = userAccountSettingData.ReadingUnitOptions.FirstOrDefault(x =>
                            x.ParentOptionID == item.SettingTypeID
                            && x.OptionID == Convert.ToInt64(item.SettingValue, CultureInfo.InvariantCulture)
                        );
                        if (readingUnitOption != null)
                        {
                            readingUnitOption.IsSelected = true;
                        }
                        var unitOptions = userAccountSettingData.ReadingUnitOptions.Where(x => x.ParentOptionID == item.ReadingTypeID);
                        item.ReadingUnitOptions = unitOptions?.Select(x => x.OptionText).ToList();
                        item.ReadingUnitOption = unitOptions?.ToList();
                        item.ReadingUnitOption = GetPickerSource(item.ReadingUnitOption, nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), readingUnitOption?.OptionID ?? -1, false, nameof(OptionModel.ParentOptionID));
                        item.ReadingUnit = item.SelectedReadingUnit = userAccountSettingData.ReadingUnitOptions.FirstOrDefault(x => x.OptionID == Convert.ToInt64(item.SettingValue, CultureInfo.InvariantCulture))?.OptionText;
                    });
                }
            }
            catch (Exception ex)
            {
                userAccountSettingData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private async Task SyncUserAccountSettingsFromServerAsync(UserAccountSettingDTO userAccountSettingData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_USER_ACCOUNT_SETTING_SERVICE_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(userAccountSettingData.RecordCount, CultureInfo.InvariantCulture)},
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                userAccountSettingData.ErrCode = httpData.ErrCode;
                if (userAccountSettingData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(userAccountSettingData, data);
                        SetResourcesAndSettings(userAccountSettingData);
                        userAccountSettingData.UserAccountSettings = MapUserAccountSettings(data, nameof(userAccountSettingData.UserAccountSettings));
                        userAccountSettingData.ReadingUnitOptions = GetPickerSource(data, nameof(UserAccountSettingDTO.ReadingUnitOptions), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), -1, false, nameof(OptionModel.ParentOptionID));
                    }
                }
            }
            catch (Exception ex)
            {
                userAccountSettingData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// save user account settings
        /// </summary>
        /// <param name="userAccountSettingData">data to be save</param>
        /// <returns>operation result</returns>
        public async Task SaveUserAccountSettingsAsync(UserAccountSettingDTO userAccountSettingData)
        {
            try
            {
                await new ReadingDatabase().SaveUserAccountSettingsAsync(userAccountSettingData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                userAccountSettingData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Checks if health app is currently enabled
        /// </summary>
        /// <returns>Returns true if health app is connected else returns false</returns>
        public async Task<bool> IsHealthAppEnabledAsync()
        {
            try
            {
                return await new ReadingDatabase().IsHealthAppEnabledAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// Get Setting Value Based On Setting Type
        /// </summary>
        /// <param name="requestData">Reference object to return  setting value</param>
        /// <returns>operation status</returns>
        public async Task GetSettingValueBasedOnSettingTypeAsync(UserAccountSettingDTO userAccountSetting)
        {
            try
            {
                await new ReadingDatabase().GetSettingValueBasedOnSettingTypeAsync(userAccountSetting).ConfigureAwait(false);
                if (GenericMethods.IsListNotEmpty(userAccountSetting.UserAccountSettings))
                {
                    userAccountSetting.UserAccountSetting = userAccountSetting.UserAccountSettings.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                userAccountSetting.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// register/unregister for remote notification
        /// </summary>
        /// <param name="isNotificationEnabled">value to register/unregister device</param>
        public async Task SetupRemoteNotificationAsync(bool isNotificationEnabled)
        {
            if (MobileConstants.IsIosPlatform)
            {
                //todo:
                //if (isNotificationEnabled)
                //{
                //    DependencyService.Get<INotificationHelper>().RegisterForRemoteNotification();
                //    await Task.Delay(1000);
                //    await new UserService().RegisterDeviceAsync().ConfigureAwait(true);
                //}
                //else
                //{
                //    DependencyService.Get<INotificationHelper>().UnregisterRemoteNotification();
                //}
            }
        }
    }
}