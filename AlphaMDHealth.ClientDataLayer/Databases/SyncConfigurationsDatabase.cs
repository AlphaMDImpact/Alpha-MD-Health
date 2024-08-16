using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.ClientDataLayer
{
    /// <summary>
    /// Database used for sync configuration data operations
    /// </summary>
    public class SyncConfigurationsDatabase : BaseDatabase
    {
        /// <summary>
        /// Save dashboard page settings in database
        /// </summary>
        /// <param name="settingData"> Settings data to be save in database and result as reference with error code</param>
        /// <returns>Operation Status</returns>
        public async Task SaveSyncConfigurationAsync(BaseDTO settingData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (settingData.LastModifiedON == null)
                {
                    //When Last sync datetime is default datetime, clear all existing records from DB and insert new one for avoid duplicate record issue as each server will contains different ServerID(PrimaryKey) and InsertOrReplace() function update records based on PrimaryKey
                    transaction.Execute("DELETE FROM SyncConfigurationModel");
                }
                foreach (var setting in settingData.Settings)
                {
                    if (setting.IsActive)
                    {
                        transaction.InsertOrReplace(MapSettingToSyncConfiguration(setting));
                    }
                    else
                    {
                        // DELETE Directly without filtering
                        transaction.Execute("DELETE FROM SyncConfigurationModel WHERE SettingID = ?", setting.SettingID);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// To get list of Sync Configurations from database based on the list of groups
        /// </summary>
        /// <returns>list of Sync Configurations for specified groups</returns>
        /// <param name="syncConfigurationData">Instance to store Sync Configurations</param>
        /// <param name="pageName">Page name for which Sync Configurations need to get</param>
        /// <param name="isFirstTime">IsFirstTime login flag</param>
        /// <returns>Operation Status</returns>
        public async Task GetSyncConfigurationsAsync(SyncConfigurationDTO syncConfigurationData, string pageName, bool isFirstTime)
        {
            syncConfigurationData.Configurations = isFirstTime ?
                    await SqlConnection.QueryAsync<SyncConfigurationModel>
                    ($"SELECT * FROM SyncConfigurationModel WHERE PageName = ? AND SyncTimes IN (?, ?)", pageName, SyncTimes.FirstTimeOnly, SyncTimes.FirstAndRestTime).ConfigureAwait(false)
                    : await SqlConnection.QueryAsync<SyncConfigurationModel>
                    ($"SELECT * FROM SyncConfigurationModel WHERE PageName = ? AND SyncTimes IN (?, ?)", pageName, SyncTimes.FirstAndRestTime, SyncTimes.RestTimeOnly).ConfigureAwait(false);
        }

        private SyncConfigurationModel MapSettingToSyncConfiguration(SettingModel setting)
        {
            string[] settingkey = setting.SettingKey.Split(Constants.DOT_SEPARATOR);
            string[] settingValue = setting.SettingValue.Split(Constants.DOT_SEPARATOR);
            return new SyncConfigurationModel
            {
                SettingID = setting.SettingID,
                GroupName = setting.GroupName.ToEnum<ServiceSyncGroups>(),
                PageName = settingkey[0],
                TableName = settingkey[1],
                SyncTimes = settingValue[0].ToEnum<SyncTimes>(),
                SyncTypes = settingValue[1].ToEnum<SyncTypes>(),
                Sequence = Convert.ToInt32(settingValue[2], CultureInfo.InvariantCulture),
                TablesInBatch = settingValue.Length > 3 ? settingValue[3] : settingkey[1]
            };
        }
    }
}