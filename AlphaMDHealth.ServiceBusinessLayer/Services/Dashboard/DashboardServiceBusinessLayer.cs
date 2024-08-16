using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class DashboardServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Dashboard service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public DashboardServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get Dashboard Configurations
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>    
        /// <param name="selectedUserID">Selected user's ID for whoes data needs to be retrived</param>
        /// <param name="dashboardSettingID">Id of Dashboard Setting</param>
        /// <param name="roleID">Id of Role</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of Dashboard Configurations</returns>
        public async Task<DashboardDTO> GetDashboardConfigurationsAsync(byte languageID, long permissionAtLevelID, long selectedUserID, long dashboardSettingID, byte roleID, long recordCount)
        {
            DashboardDTO dashboardData = new DashboardDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    dashboardData.ErrCode = ErrorCode.InvalidData;
                    return dashboardData;
                }
                if (AccountID < 1)
                {
                    dashboardData.ErrCode = ErrorCode.Unauthorized;
                    return dashboardData;
                }
                dashboardData.AccountID = AccountID;
                dashboardData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                dashboardData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_DASHBOARD_PAGE_GROUP}", languageID, default, dashboardData.AccountID, permissionAtLevelID, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(dashboardData.Resources))
                {
                    dashboardData.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, $"{GroupConstants.RS_COMMON_GROUP}", languageID, default, dashboardData.AccountID, permissionAtLevelID, false).ConfigureAwait(false)).Settings;
                    if (GenericMethods.IsListNotEmpty(dashboardData.Settings))
                    {
                        dashboardData.PermissionAtLevelID = permissionAtLevelID;
                        dashboardData.SelectedUserID = selectedUserID;
                        dashboardData.LanguageID = languageID;
                        dashboardData.RecordCount = recordCount;
                        dashboardData.ConfigurationRecord = new ConfigureDashboardModel { DashboardSettingID = dashboardSettingID, RoleID = roleID };
                        dashboardData.FeatureFor = FeatureFor;
                        await new DashboardServiceDataLayer().GetDashboardConfigurationsAsync(dashboardData).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                dashboardData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return dashboardData;
        }

        /// <summary>
        /// Save Dashboard Configuration
        /// </summary>
        /// <param name="dashboardData">Dashboard Configuration data to be saved</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <returns>Result of operation</returns>
        public async Task<BaseDTO> SaveDashboardConfigurationAsync(DashboardDTO dashboardData, long permissionAtLevelID)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1 || dashboardData.ConfigurationRecord == null ||
                    dashboardData.ConfigurationRecord.SequenceNo < 0 || dashboardData.ConfigurationRecord.FeatureID < 0 ||
                    dashboardData.ConfigurationRecord.RoleID < 0 || dashboardData.ConfigurationRecord.DashboardSettingID < 0
                    || dashboardData.ConfigurationRecordParameters.Count < 1)
                {
                    dashboardData.ErrCode = ErrorCode.InvalidData;
                    return dashboardData;
                }
                if (dashboardData.IsActive)
                {
                    dashboardData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(dashboardData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_DASHBOARD_PAGE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(dashboardData.ConfigurationRecord, dashboardData.Resources))
                        {
                            dashboardData.ErrCode = ErrorCode.InvalidData;
                            return dashboardData;
                        }
                    }
                    else
                    {
                        return dashboardData;
                    }
                }
                dashboardData.AccountID = AccountID;
                dashboardData.PermissionAtLevelID = permissionAtLevelID;
                dashboardData.FeatureFor = FeatureFor;
                await new DashboardServiceDataLayer().SaveDashboardConfigurationAsync(dashboardData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                dashboardData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return dashboardData;
        }
    }
}