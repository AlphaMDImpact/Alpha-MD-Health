using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    /// <summary>
    /// Represents database to store dashboard settings and do operations on it
    /// </summary>
    public class DashboardLibDatabase : BaseDatabase
    {
        /// <summary>
        /// Retrieve parameters of configured feature
        /// </summary>
        /// <param name="dashboardData">list of settings as reference</param>
        /// <returns>list of dashboard page settings in settingsList reference</returns>
        protected async Task GetFeatureParametersAsync(DashboardLibDTO dashboardData)
        {
            if (GenericMethods.IsListNotEmpty(dashboardData.ConfigurationRecords))
            {
                string features = string.Join(",", dashboardData.ConfigurationRecords.Select(x => x.FeatureID));
                dashboardData.ConfigurationRecordParameters = await SqlConnection.QueryAsync<SystemFeatureParameterModel>
                    ($"SELECT * FROM SystemFeatureParameterModel WHERE IsActive = 1 AND FeatureID IN ({features})").ConfigureAwait(false);
            }
        }

        /// <summary>
        /// insert or update record in the database
        /// </summary>
        /// <param name="dashboardData">Object which holds record to save</param>
        /// <returns>Operation Status</returns>
        public async Task SaveDashboardConfigurationsAsync(DashboardLibDTO dashboardData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (dashboardData.LastModifiedON == null)
                {
                    //When Last sync datetime is default datetime, clear all existing records from DB and insert new one for avoid duplicate record issue as each server will contains different ServerID(PrimaryKey) and InsertOrReplace() function update records based on PrimaryKey
                    transaction.Execute("DELETE FROM SystemFeatureParameterModel");
                    transaction.Execute("DELETE FROM ConfigureDashboardModel");
                }
                SaveDashboardConfigurations(dashboardData, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves dashboard configurations data without cleanup
        /// </summary>
        /// <param name="dashboardData">Data to save</param>
        /// <param name="transaction">transaction to perform operation</param>
        public void SaveDashboardConfigurations(DashboardLibDTO dashboardData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(dashboardData.ConfigurationRecords))
            {
                foreach (ConfigureDashboardModel record in dashboardData.ConfigurationRecords)
                {
                    transaction.InsertOrReplace(record);
                    transaction.Execute("UPDATE SystemFeatureParameterModel SET IsActive = 0 WHERE DashboardSettingID = ?", record.DashboardSettingID);
                }
            }
            SaveDashboardConfigurationParameters(dashboardData, transaction);
        }

        private void SaveDashboardConfigurationParameters(DashboardLibDTO dashboardData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(dashboardData.ConfigurationRecordParameters))
            {
                foreach (SystemFeatureParameterModel parameter in dashboardData.ConfigurationRecordParameters)
                {
                    if (transaction.FindWithQuery<SystemFeatureParameterModel>("SELECT 1 FROM SystemFeatureParameterModel WHERE DashboardSettingID = ? AND ParameterID = ?", parameter.DashboardSettingID, parameter.ParameterID) == null)
                    {
                        transaction.Execute("INSERT INTO SystemFeatureParameterModel (DashboardSettingID, ParameterID, FeatureID, ParameterName, ParameterValue, Sequence, IsActive) VALUES (?, ?, ?, ?, ?, ?, ?)",
                            parameter.DashboardSettingID, parameter.ParameterID, parameter.FeatureID, parameter.ParameterName, parameter.ParameterValue, parameter.Sequence, parameter.IsActive);
                    }
                    else
                    {
                        transaction.Execute("UPDATE SystemFeatureParameterModel SET FeatureID = ?, ParameterName = ?, ParameterValue = ?, Sequence = ?, IsActive = ? WHERE DashboardSettingID = ? AND ParameterID = ?",
                            parameter.FeatureID, parameter.ParameterName, parameter.ParameterValue, parameter.Sequence, parameter.IsActive, parameter.DashboardSettingID, parameter.ParameterID);
                    }
                }
            }
        }
    }
}