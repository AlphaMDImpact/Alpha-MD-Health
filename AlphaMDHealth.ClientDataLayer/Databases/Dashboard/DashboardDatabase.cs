using AlphaMDHealth.Model;

namespace AlphaMDHealth.ClientDataLayer
{
    public class DashboardDatabase : DashboardLibDatabase
    {
        /// <summary>
        /// insert or update record in the database
        /// </summary>
        /// <param name="dashboardData">Object which holds record to save</param>
        /// <returns>Operation Status</returns>
        public async Task SaveDashboardConfigurationDataAsync(DashboardLibDTO dashboardData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveDashboardConfigurations(dashboardData, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get dashboard page settings to display the different views
        /// </summary>
        /// <param name="dashboardData">list of settings as reference</param>
        /// <returns>list of dashboard page settings in settingsList reference</returns>
        public async Task GetDashboardDataAsync(DashboardDTO dashboardData)
        {
            //instead of greater then zero Not equals to zero is added because When user Added in offline mode the selected user id is in negative
            dashboardData.ConfigurationRecords = dashboardData.SelectedUserID != 0 
                ? await SqlConnection.QueryAsync<ConfigureDashboardModel>
                    ($"SELECT DISTINCT A.*, (CASE WHEN B.FeatureText IS NULL THEN A.FeatureCode ELSE B.FeatureText END) AS FeatureText " +
                    $"FROM ConfigureDashboardModel A " +
                    $"LEFT JOIN OrganizationFeaturePermissionModel B ON B.FeatureCode = A.FeatureCode AND B.IsActive = 1 " +
                    $"WHERE A.RoleID = ? AND A.IsActive = 1 AND A.SequenceNo > 0 ORDER BY A.SequenceNo ASC"
                    , dashboardData.ConfigurationRecord.RoleID).ConfigureAwait(false) 
                : await SqlConnection.QueryAsync<ConfigureDashboardModel>
                    ($"SELECT A.*, B.FeatureText, B.FeatureCode, (SELECT MobileMenuNodeID FROM MobileMenuNodeModel WHERE TargetID = A.FeatureID AND MobileMenuGroupID = 0) AS NodeID " +
                    $"FROM ConfigureDashboardModel A " +
                    $"JOIN OrganizationFeaturePermissionModel B ON B.FeatureID = A.FeatureID AND B.IsActive = 1 " +
                    $"WHERE A.RoleID = ? AND A.IsActive = 1 AND A.SequenceNo > 0 ORDER BY A.SequenceNo ASC"
                    , dashboardData.ConfigurationRecord.RoleID).ConfigureAwait(false);
            await GetFeatureParametersAsync(dashboardData).ConfigureAwait(false);
        }
    }
}