using AlphaMDHealth.Model;
using System.Globalization;

namespace AlphaMDHealth.ClientDataLayer
{
    /// <summary>
    /// represents setting database module
    /// </summary>
    public class SettingLibDatabase : BaseDatabase
    {
        /// <summary>
        /// Fetch settings from DB based on certain conditions
        /// </summary>
        /// <param name="settingsData">object to get settings</param>
        /// <param name="groupList">List of groups for which search to be performed in settings db</param>
        /// <returns>List of settings with errorcode</returns>
        public async Task GetSettingsAsync(BaseDTO settingsData, params string[] groupList)
        {
            string commaSeperatedList = string.Join(",", groupList.Select(x => string.Format(CultureInfo.CurrentCulture, "'{0}'", x)));
            settingsData.Settings = await SqlConnection.QueryAsync<SettingModel>($"SELECT * FROM SettingModel WHERE GroupName IN ({commaSeperatedList})").ConfigureAwait(false);
        }

        /// <summary>
        /// To get settings from DB based on SettingKey
        /// </summary>
        /// <param name="settingsKey">Key to search in settings db</param>
        /// <returns>setting with for specified key with errorcode</returns>
        public async Task<SettingModel> GetSettingAsync(string settingsKey)
        {
            return await SqlConnection.FindWithQueryAsync<SettingModel>($"SELECT * FROM SettingModel WHERE SettingKey = ?", settingsKey).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves Settings to Settings Table
        /// </summary>
        /// <param name="settingsData">Settings DTO with list of settings to insert or update and  result as reference with error code</param>
        /// <returns>Operation status</returns>
        public async Task SaveSettingsAsync(BaseDTO settingsData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (settingsData.LastModifiedON == null)
                {
                    //When Last sync datetime is default datetime, clear all existing records from DB and insert new one for avoid duplicate record issue as each server will contains different ServerID(PrimaryKey) and InsertOrReplace() function update records based on PrimaryKey
                    transaction.Execute("DELETE FROM SettingModel");
                }
                // variable used to store current UTC Datetime for use with in foreach loop
                foreach (var setting in settingsData.Settings)
                {
                    if (setting.IsActive)
                    {
                        transaction.InsertOrReplace(setting);
                    }
                    else
                    {
                        transaction.Execute($"DELETE FROM SettingModel WHERE SettingID = ?", setting.SettingID);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets static setting count 
        /// </summary>
        /// <returns>no of static settings available in DB</returns>
        public async Task<int> GetStaticSettingsCountAsync()
        {
            var settingCount = await SqlConnection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM SettingModel WHERE IsDynamic = 0").ConfigureAwait(false);
            //LibGenericMethods.LogData($"**********|GetStaticSettingsCountAsync().settingCount : {settingCount}|**********");
            return settingCount;
        }
    }
}
