using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;
using System.Globalization;

namespace AlphaMDHealth.ClientDataLayer
{
    public partial class ReadingDatabase : BaseDatabase
    {
        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="userAccountSettings">User Account settings data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveUserAccountSettingsAsync(UserAccountSettingDTO userAccountSettings)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (UserAccountSettingsModel userAccountSetting in userAccountSettings.UserAccountSettings)
                {
                    if (transaction.FindWithQuery<UserAccountSettingsModel>("SELECT 1 FROM UserAccountSettingsModel WHERE SettingType = ? AND SettingTypeID = ?", userAccountSetting.SettingType, userAccountSetting.SettingTypeID) == null)
                    {
                        transaction.Execute("INSERT INTO UserAccountSettingsModel (SettingType, SettingTypeID, SettingValue, IsActive, IsSynced) VALUES (?, ?, ?, ?, ?) ",
                            userAccountSetting.SettingType, userAccountSetting.SettingTypeID, userAccountSetting.SettingValue, userAccountSetting.IsActive, userAccountSetting.IsSynced);
                    }
                    else
                    {
                        transaction.Execute("UPDATE UserAccountSettingsModel SET SettingValue = ?, IsActive = ?, IsSynced = ? WHERE SettingType = ? AND SettingTypeID = ? ",
                             userAccountSetting.SettingValue, userAccountSetting.IsActive, userAccountSetting.IsSynced, userAccountSetting.SettingType, userAccountSetting.SettingTypeID);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get user account settings
        /// </summary>
        /// <param name="userAccountSettings">Reference object to return user account settings.</param>
        /// <returns>List of user account settings</returns>
        public async Task GetUserAccountSettingsAsync(UserAccountSettingDTO userAccountSettings)
        {
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
            var condition = isPatientData
                ? $"ReadingModel"
                : $"ReadingMasterModel";
            userAccountSettings.ReadingUnitOptions = await SqlConnection.QueryAsync<OptionModel>(
                "SELECT DISTINCT C.ReadingID AS ParentOptionID, A.UnitID AS OptionID, B.FullUnitName AS OptionText " +
                $"FROM {condition} C " +
                "JOIN UnitModel A ON C.UnitGoupID = A.UnitGroupID AND C.IsActive = 1 " +
                "JOIN UnitI18NModel B ON A.UnitID = B.UnitID AND B.LanguageID = ?"
                , userAccountSettings.LanguageID
            ).ConfigureAwait(false);
            userAccountSettings.UserAccountSettings = await SqlConnection.QueryAsync<UserAccountSettingsModel>(
                $"SELECT '{UserSettingType.NotificationKey}' AS SettingType, NULL AS SettingTypeID, 0 AS ReadingTypeID, NULL AS ReadingType, " +
                $"CASE WHEN EXISTS (SELECT SettingValue FROM UserAccountSettingsModel WHERE SettingType = {(int)UserSettingType.NotificationKey}) THEN (SELECT SettingValue FROM UserAccountSettingsModel WHERE SettingType = {(int)UserSettingType.NotificationKey} LIMIT 1) ELSE 'true' END " +
                "AS SettingValue, " +
                $"CASE WHEN EXISTS (SELECT IsSynced FROM UserAccountSettingsModel WHERE SettingType = {(int)UserSettingType.NotificationKey}) THEN (SELECT IsSynced FROM UserAccountSettingsModel WHERE SettingType = {(int)UserSettingType.NotificationKey} LIMIT 1) ELSE 'false' END " +
                "AS IsSynced " +
                "UNION ALL " +
                $"SELECT '{UserSettingType.HealthDataKey}' AS SettingType, NULL AS SettingTypeID, 0 AS ReadingTypeID, NULL AS ReadingType, " +
                $"CASE WHEN EXISTS (SELECT SettingValue FROM UserAccountSettingsModel WHERE SettingType = {(int)UserSettingType.HealthDataKey}) THEN (SELECT SettingValue FROM UserAccountSettingsModel WHERE SettingType = {(int)UserSettingType.HealthDataKey} LIMIT 1) ELSE 'false' END " +
                "AS SettingValue, " +
                $"CASE WHEN EXISTS (SELECT IsSynced FROM UserAccountSettingsModel WHERE SettingType = {(int)UserSettingType.HealthDataKey}) THEN (SELECT IsSynced FROM UserAccountSettingsModel WHERE SettingType = {(int)UserSettingType.HealthDataKey} LIMIT 1) ELSE 'false' END " +
                "AS IsSynced " +
                "UNION " +
                $"SELECT DISTINCT '{UserSettingType.MeasurementUnitsKey}' AS SettingType, B.ReadingID AS SettingTypeID, B.ReadingID AS ReadingTypeID, B.ReadingID AS ReadingType, " +
                    "CASE WHEN EXISTS (SELECT SettingValue FROM UserAccountSettingsModel WHERE SettingTypeID = B.ReadingID) THEN E.SettingValue ELSE CAST (D.UnitID AS TEXT) END " +
                    "AS SettingValue,  E.IsSynced " +
                $"FROM {condition} B " +
                "JOIN UnitModel D ON B.UnitGoupID = D.UnitGroupID AND B.IsActive = 1 AND D.UnitGroupID IN (SELECT UnitGroupID FROM UnitModel GROUP BY UnitGroupID HAVING COUNT(UnitGroupID) > 1) " +
                "AND D.IsBaseUnit = 1 AND D.IsActive = 1 " +
                "LEFT JOIN UserAccountSettingsModel E ON E.SettingTypeID = B.ReadingID "
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Get User account settings to sync
        /// </summary>
        /// <param name="requestData">Reference object to return  User account settings</param>
        /// <returns>operation status</returns>
        public async Task GetUserAccountSettingsForSyncAsync(UserAccountSettingDTO requestData)
        {
            requestData.UserAccountSettings = await SqlConnection.QueryAsync<UserAccountSettingsModel>("SELECT * FROM UserAccountSettingsModel WHERE IsSynced = 0").ConfigureAwait(false);
        }

        /// <summary>
        /// Get Setting Value Based On Setting Type
        /// </summary>
        /// <param name="requestData">Reference object to return  setting value</param>
        /// <returns>operation status</returns>
        public async Task GetSettingValueBasedOnSettingTypeAsync(UserAccountSettingDTO requestData)
        {
            requestData.UserAccountSettings = await SqlConnection.QueryAsync<UserAccountSettingsModel>($"SELECT SettingType, SettingTypeID, SettingValue FROM UserAccountSettingsModel WHERE SettingType = {(int)requestData.UserAccountSetting.SettingType} AND SettingTypeID = ?", requestData.UserAccountSetting.SettingTypeID).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks if health app is currently enabled
        /// </summary>
        /// <returns>Returns true if health app is connected else returns false</returns>
        public async Task<bool> IsHealthAppEnabledAsync()
        {
            UserAccountSettingsModel setting = await SqlConnection.FindWithQueryAsync<UserAccountSettingsModel>(
                $"SELECT SettingType, SettingTypeID, SettingValue FROM UserAccountSettingsModel WHERE SettingType = {(int)UserSettingType.HealthDataKey}");
            return Convert.ToBoolean(setting?.SettingValue ?? false.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Save reading connect account status
        /// </summary>
        /// <param name="result">object to return status</param>
        /// <returns>Operation Status</returns>
        public async Task SaveReadingConnectAccountStatusAsync(BaseDTO result)
        {
            UserAccountSettingDTO userAccountSettingData = new UserAccountSettingDTO
            {
                UserAccountSettings = new List<UserAccountSettingsModel>
                {
                    new UserAccountSettingsModel { SettingType = UserSettingType.HealthDataKey, SettingValue = result.IsActive.ToString(CultureInfo.InvariantCulture) ,IsActive = result.IsActive, IsToogled=result.IsActive}
                }
            };
            await new ReadingDatabase().SaveUserAccountSettingsAsync(userAccountSettingData);
        }

        private void GetMeasurementUnitFilterOptions(PatientReadingDTO readingsData, SQLiteConnection transaction)
        {
            readingsData.UserAccountSettings = transaction.Query<UserAccountSettingsModel>(
                "SELECT A.SettingTypeID, A.SettingValue FROM UserAccountSettingsModel A WHERE A.SettingType = ? AND A.IsActive = 1"
                , UserSettingType.MeasurementUnitsKey);
        }

    }
}