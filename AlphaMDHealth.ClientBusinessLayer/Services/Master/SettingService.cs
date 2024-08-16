using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class SettingService : BaseService
    {
        public SettingService(IEssentials essentials):base(essentials) { }
        private const int DEFAULT_INACTIVE_DURATION = 10;

        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveSettingsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                var setingData = new BaseDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    Settings = MapSettingsData(data)
                };
                if (GenericMethods.IsListNotEmpty(setingData.Settings))
                {
                    await Task.WhenAll(
                        SaveSettingsAsync(setingData, GroupConstants.RS_SYNC_FROM_SERVER_GROUP, GroupConstants.RS_SYNC_TO_SERVER_GROUP, GroupConstants.RS_SYNC_FROM_DEVICE_GROUP),
                        SaveSettingsAsync(setingData, GroupConstants.RS_ALL_GROUP)
                    ).ConfigureAwait(false);
                    result.RecordCount = setingData.Settings.Count;
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
        /// Checks if update is required for app
        /// </summary>
        /// <returns>true if app update is required, else returns false</returns>
        public async Task<bool> CheckAppUpdateAsync(string version, BaseDTO pageData)
        {
            try
            {
                await GetSettingsAsync(GroupConstants.RS_COMMON_GROUP).ConfigureAwait(false);
                string appVersion = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_CURRENT_APP_VERSION_KEY);
                string forceUpdate = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_APP_FORCE_UPDATE_KEY);
                pageData.ErrCode = !string.IsNullOrWhiteSpace(appVersion) && !string.IsNullOrWhiteSpace(forceUpdate)
                    && version != appVersion && Convert.ToBoolean(forceUpdate, CultureInfo.InvariantCulture) ? ErrorCode.UpdateApp : ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                pageData.ErrCode = ErrorCode.RestartApp;
            }
            return pageData.ErrCode == ErrorCode.UpdateApp;
        }

        /// <summary>
        /// Check if the SleepTime is greater than TimeOutInterval.
        /// </summary>
        /// <param name="sleepDateTime">DateTime when the app enters in background</param>
        /// <returns>True, if Timeout is reached. False, if Timeout is not reached. </returns>
        public async Task<bool> IsAppLockReachedAsync(DateTimeOffset sleepDateTime)
        {
            try
            {
                string inactiveDurationString = await new SettingService(_essentials).GetSettingsValueByKeyAsync(SettingsConstants.S_INACTIVE_DURATION_KEY).ConfigureAwait(false);
                return (GenericMethods.GetUtcDateTime - sleepDateTime).TotalMinutes >
                    (string.IsNullOrWhiteSpace(inactiveDurationString) ? DEFAULT_INACTIVE_DURATION : Convert.ToInt32(inactiveDurationString, CultureInfo.InvariantCulture));
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Fetch settings from DB based on certain conditions
        /// </summary>
        /// <param name="settingsData">object to get settings</param>
        /// <param name="groupList">List of groups for which search to be performed in settings db</param>
        /// <returns>List of settings with errorcode</returns>
        public async Task GetSettingsAsync(BaseDTO settingsData, params string[] groupList)
        {
            await new SettingLibDatabase().GetSettingsAsync(settingsData, groupList).ConfigureAwait(false);
        }

        /// <summary>
        /// To get settings value from database based on settingKey
        /// </summary>
        /// <param name="settingkey">key to search in settings db</param>
        /// <returns>Setting value of specified Key</returns>
        public async Task<string> GetSettingsValueByKeyAsync(string settingkey)
        {
            try
            {
                return (await new SettingLibDatabase().GetSettingAsync(settingkey).ConfigureAwait(false))?.SettingValue ?? string.Empty;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                return string.Empty;
            }
        }

        private async Task SaveSettingsAsync(BaseDTO settingData, params string[] groupName)
        {
            BaseDTO filteredSettings = new BaseDTO
            {
                LastModifiedON = settingData.LastModifiedON,
                Settings = groupName.Contains(GroupConstants.RS_ALL_GROUP)
                            ? settingData.Settings.Where(c =>
                                c.GroupName != GroupConstants.RS_SYNC_FROM_SERVER_GROUP &&
                                c.GroupName != GroupConstants.RS_SYNC_TO_SERVER_GROUP &&
                                c.GroupName != GroupConstants.RS_SYNC_FROM_DEVICE_GROUP).ToList()
                            : settingData.Settings.Where(c => groupName.Contains(c.GroupName)).ToList()
            };
            FilterDeletedRecords(settingData, filteredSettings);
            if (GenericMethods.IsListNotEmpty(filteredSettings.Settings))
            {
                switch (groupName.First().ToEnum<ServiceSyncGroups>())
                {
                    case ServiceSyncGroups.RSSyncFromServerGroup:
                    case ServiceSyncGroups.RSSyncToServerGroup:
                    case ServiceSyncGroups.RSSyncFromDeviceGroup:
                        await new SyncConfigurationsDatabase().SaveSyncConfigurationAsync(filteredSettings).ConfigureAwait(false);
                        break;
                    default:
                        await new SettingLibDatabase().SaveSettingsAsync(filteredSettings).ConfigureAwait(false);
                        break;
                }
            }
        }

        private void FilterDeletedRecords(BaseDTO settings, BaseDTO filteredSettings)
        {
            // Check and remove deleted settings from table directly using id as deleted data does not provide group name
            var deletedSettings = settings.Settings.Where(x => !x.IsActive).ToList();
            if (GenericMethods.IsListNotEmpty(deletedSettings))
            {
                if (GenericMethods.IsListNotEmpty(filteredSettings.Settings))
                {
                    filteredSettings.Settings.AddRange(deletedSettings);
                }
                else
                {
                    filteredSettings.Settings = deletedSettings;
                }
            }
        }

        /// <summary>
        /// Map setting json into model
        /// </summary>
        /// <param name="data">setting json data</param>
        /// <returns>List of setting</returns>
        internal List<SettingModel>? MapSettingsData(JToken data)
        {
            return (data[nameof(BaseDTO.Settings)]?.Count() > 0)
                ? (from dataItem in data[nameof(BaseDTO.Settings)]
                   select new SettingModel
                   {
                       SettingID = GetDataItem<int>(dataItem, nameof(SettingModel.SettingID)),
                       GroupName = GetDataItem<string>(dataItem, nameof(SettingModel.GroupName)),
                       OrganisationID = GetDataItem<long>(dataItem, nameof(SettingModel.OrganisationID)),
                       SettingDescription = GetDataItem<string>(dataItem, nameof(SettingModel.SettingDescription)),
                       SettingKey = GetDataItem<string>(dataItem, nameof(SettingModel.SettingKey)),
                       SettingValue = GetDataItem<string>(dataItem, nameof(SettingModel.SettingValue)),
                       SettingType = GetDataItem<string>(dataItem, nameof(SettingModel.SettingType)),
                       IsDynamic = GetDataItem<bool>(dataItem, nameof(SettingModel.IsDynamic)),
                       IsActive = GetDataItem<bool>(dataItem, nameof(SettingModel.IsActive))
                   }).ToList()
                : null;
        }

    }
}