using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class DashboardService : BaseService
    {
        public DashboardService(IEssentials serbviceEssentials) : base(serbviceEssentials)
        {
        }
        /// <summary>
        /// Get dashboard page settings to display the different views
        /// </summary>
        /// <param name="dashboardData">Page data instance for hold data</param>
        /// <returns>List of dashboard page settings</returns>
        public async Task GetDashboardDataAsync(DashboardDTO dashboardData)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
                    dashboardData.ConfigurationRecord = new ConfigureDashboardModel
                    {
                        RoleID = (byte)(dashboardData.SelectedUserID != 0 || roleID == (int)RoleName.CareTaker
                            ? (int)RoleName.Patient
                            : roleID)
                    };
                    await Task.WhenAll(
                        GetResourcesAsync(GroupConstants.RS_COMMON_GROUP),
                        GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                        GetFeaturesAsync(AppPermissions.PatientReadingAddEdit.ToString()),
                        new DashboardDatabase().GetDashboardDataAsync(dashboardData)
                    ).ConfigureAwait(true);
                    SetCommonPageData(dashboardData);
                    dashboardData.IsActive = GenericMethods.IsListNotEmpty(dashboardData.ConfigurationRecords)
                        && dashboardData.ConfigurationRecords.Any(x => x.FeatureCode == AppPermissions.PatientReadingAddEdit.ToString() || x.FeatureCode == AppPermissions.PatientReadingsView.ToString())
                        && (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
                        && CheckFeaturePermissionByCode(AppPermissions.PatientReadingAddEdit.ToString())
                        && await new ReadingDatabase().IsAllowToAddReadingsAsync(GetLoginUserID()).ConfigureAwait(false);
                }
                else
                {
                    await SyncDashboardConfigurationsFromServerAsync(dashboardData, CancellationToken.None).ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                dashboardData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveDashboardConfigurationsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                DashboardDTO dashboardData = new DashboardDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    ConfigurationRecords = MapDashboardConfigurations(data, nameof(DataSyncDTO.DashboardConfigurations)),
                    ConfigurationRecordParameters = MapDashboardConfigurationParameters(data, nameof(DataSyncDTO.DashboardConfigurationParameters))
                };
                if (GenericMethods.IsListNotEmpty(dashboardData.ConfigurationRecords) || GenericMethods.IsListNotEmpty(dashboardData.ConfigurationRecordParameters))
                {
                    await new DashboardDatabase().SaveDashboardConfigurationDataAsync(dashboardData).ConfigureAwait(false);
                    result.RecordCount = (dashboardData.ConfigurationRecords?.Count ?? 0) + (dashboardData.ConfigurationRecordParameters?.Count ?? 0);
                }
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync Dashboard Configuration data to server
        /// </summary>
        /// <param name="requestData">object to return operation status</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status call</returns>
        public async Task SyncDashboardConfigurationToServerAsync(DashboardDTO requestData, CancellationToken cancellationToken)
        {
            try
            {
                if (GenericMethods.IsListNotEmpty(requestData.ConfigurationRecordParameters))
                {
                    var httpData = new HttpServiceModel<DashboardDTO>
                    {
                        CancellationToken = cancellationToken,
                        PathWithoutBasePath = UrlConstants.SAVE_DASHBOARD_CONFIGURATION_ASYNC_PATH,
                        QueryParameters = new NameValueCollection
                        {
                            { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        },
                        ContentToSend = requestData,
                    };
                    await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                    requestData.ErrCode = httpData.ErrCode;
                    requestData.Response = httpData.Response;
                }
            }
            catch (Exception ex)
            {
                requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private async Task SyncDashboardConfigurationsFromServerAsync(DashboardDTO dashboardData, CancellationToken cancellationToken)
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_DASHBOARD_CONFIGURATIONS_ASYNC_PATH,
                QueryParameters = new NameValueCollection {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(dashboardData.SelectedUserID, CultureInfo.InvariantCulture) },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(dashboardData.RecordCount, CultureInfo.InvariantCulture) },
                    { Constants.SE_DASHBOARD_SETTING_ID_QUERY_KEY, Convert.ToString(dashboardData.ConfigurationRecord.DashboardSettingID, CultureInfo.InvariantCulture) },
                    { Constants.SE_ROLE_ID_QUERY_KEY, Convert.ToString(dashboardData.ConfigurationRecord.RoleID, CultureInfo.InvariantCulture) }
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            dashboardData.ErrCode = httpData.ErrCode;
            if (dashboardData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    dashboardData.Response = null;
                    MapCommonData(dashboardData, data);
                    MapDashboardConfigurations(data, dashboardData);
                }
            }
        }

        private void MapDashboardConfigurations(JToken data, DashboardDTO dashboardData)
        {
            switch (dashboardData.RecordCount)
            {
                case -2:
                    dashboardData.ConfigurationRecords = MapDashboardConfigurations(data, nameof(DashboardDTO.ConfigurationRecords));
                    dashboardData.ConfigurationRecordParameters = MapDashboardConfigurationParameters(data, nameof(DashboardDTO.ConfigurationRecordParameters));
                    break;
                case -1:
                    SetPageResources(dashboardData.Resources);
                    MapDashboardConfiguration(data, dashboardData);
                    break;
                case 0:
                    SetPageResources(dashboardData.Resources);
                    dashboardData.ConfigurationRecords = MapDashboardConfigurations(data, nameof(DashboardDTO.ConfigurationRecords));
                    dashboardData.RolesOptions = (data[nameof(dashboardData.RolesOptions)].Any()) ?
                    GetPickerSource(data, nameof(dashboardData.RolesOptions), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), dashboardData.ConfigurationRecord.RoleID, false, null)
                    : new List<OptionModel>();
                    break;
                default:
                    // do not map
                    break;
            }
            dashboardData.ErrCode = (ErrorCode)(int)data[nameof(DashboardDTO.ErrCode)];
        }

        private void MapDashboardConfiguration(JToken data, DashboardDTO dashboardData)
        {
            var dashboardJData = data[nameof(DashboardDTO.ConfigurationRecord)];
            if (dashboardJData.HasValues)
            {
                dashboardData.ConfigurationRecord = MapDashboardConfiguration(dashboardJData);
            }
            //Drop Down Data
            dashboardData.FeaturesOptions = (data[nameof(DashboardDTO.FeaturesOptions)]?.Count() > 0) ?
            (from dataItem in data[nameof(DashboardDTO.FeaturesOptions)]
             select new OptionModel
             {
                 OptionID = (long)dataItem[nameof(OptionModel.OptionID)],
                 OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                 SequenceNo = (long)dataItem[nameof(OptionModel.SequenceNo)],
                 IsSelected = (long)dataItem[nameof(OptionModel.OptionID)] == dashboardData.ConfigurationRecord.FeatureID
             }).ToList() : new List<OptionModel>();
            //dashboardData.FeaturesOptions.InsertRange(0, new List<OptionModel> { new OptionModel
            //    {
            //        OptionID = -1,
            //        OptionText = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DROP_DOWN_PLACE_HOLDER_KEY),
            //        ParentOptionID = -1
            //    } });
            //Parameter List Data
            dashboardData.ConfigurationRecordParameters = MapDashboardConfigurationParameters(data, nameof(DashboardDTO.ConfigurationRecordParameters));
        }

        private List<ConfigureDashboardModel> MapDashboardConfigurations(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
                ? (from dataItem in data[collectionName]
                   select MapDashboardConfiguration(dataItem)).ToList()
                : null;
        }

        private ConfigureDashboardModel MapDashboardConfiguration(JToken dataItem)
        {
            return new ConfigureDashboardModel
            {
                DashboardSettingID = GetDataItem<long>(dataItem, nameof(ConfigureDashboardModel.DashboardSettingID)),
                FeatureID = GetDataItem<int>(dataItem, nameof(ConfigureDashboardModel.FeatureID)),
                FeatureCode = GetDataItem<string>(dataItem, nameof(ConfigureDashboardModel.FeatureCode)),
                FeatureText = GetDataItem<string>(dataItem, nameof(ConfigureDashboardModel.FeatureText)),
                RoleID = GetDataItem<byte>(dataItem, nameof(ConfigureDashboardModel.RoleID)),
                SequenceNo = GetDataItem<byte>(dataItem, nameof(ConfigureDashboardModel.SequenceNo)),
                WidgetSizeInWebPage = GetDataItem<string>(dataItem, nameof(ConfigureDashboardModel.WidgetSizeInWebPage)),
                IsActive = GetDataItem<bool>(dataItem, nameof(ConfigureDashboardModel.IsActive)),
            };
        }

        private List<SystemFeatureParameterModel> MapDashboardConfigurationParameters(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
                ? (from dataItem in data[collectionName]
                   select new SystemFeatureParameterModel
                   {
                       DashboardSettingID = GetDataItem<long>(dataItem, nameof(SystemFeatureParameterModel.DashboardSettingID)),
                       FeatureID = GetDataItem<int>(dataItem, nameof(SystemFeatureParameterModel.FeatureID)),
                       ParameterID = GetDataItem<int>(dataItem, nameof(SystemFeatureParameterModel.ParameterID)),
                       Sequence = GetDataItem<int>(dataItem, nameof(SystemFeatureParameterModel.Sequence)),
                       ParameterName = GetDataItem<string>(dataItem, nameof(SystemFeatureParameterModel.ParameterName)),
                       ParameterDescription = GetDataItem<string>(dataItem, nameof(SystemFeatureParameterModel.ParameterDescription)),
                       ParameterValue = GetDataItem<string>(dataItem, nameof(SystemFeatureParameterModel.ParameterValue)),
                       IsActive = GetDataItem<bool>(dataItem, nameof(SystemFeatureParameterModel.IsActive)),
                   }).ToList()
                : null;
        }
    }
}