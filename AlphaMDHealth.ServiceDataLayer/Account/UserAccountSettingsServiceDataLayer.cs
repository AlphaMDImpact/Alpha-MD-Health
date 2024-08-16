using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer
{
    /// <summary>
    /// Service DataLayer for UseraccountSettings
    /// </summary>
    public class UserAccountSettingsServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get User settings form database
        /// </summary>
        /// <param name="userAccountSettingData">Reference object to return list of User account settings</param>
        /// <returns>list of User account settings</returns>
        public async Task GetUserAccountSettingsAsync(UserAccountSettingDTO userAccountSettingData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), userAccountSettingData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            MapCommonSPParameters(userAccountSettingData, parameters, AppPermissions.UserAccountSettingsView.ToString(), string.Empty);
            parameters.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_USER_ACCOUNT_SETTINGS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                userAccountSettingData.UserAccountSettings = (await result.ReadAsync<UserAccountSettingsModel>().ConfigureAwait(false))?.ToList();
                userAccountSettingData.ReadingUnitOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                await MapReturnPermissionsAsync(userAccountSettingData, result).ConfigureAwait(false);
            }
            userAccountSettingData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
        }

        /// <summary>
        /// Save User Account Settings
        /// </summary>
        /// <param name="userAccountSettingData">Reference object that contains data to be saved</param>
        /// <returns>Operation status</returns>
        public async Task SaveUserSettingsAsync(UserAccountSettingDTO userAccountSettingData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.AccountID)), userAccountSettingData.AccountID, DbType.Int64, direction: ParameterDirection.Input);
            parameters.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapUserSettingsToTable(userAccountSettingData.UserAccountSettings).AsTableValuedParameter());
            MapCommonSPParameters(userAccountSettingData, parameters, AppPermissions.UserAccountSettingsView.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_USER_ACCOUNT_SETTINGS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            userAccountSettingData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        private DataTable MapUserSettingsToTable(List<UserAccountSettingsModel> userSettings)
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(nameof(UserAccountSettingsModel.SettingType), typeof(string)),
                    new DataColumn(nameof(UserAccountSettingsModel.SettingTypeID), typeof(Int16)),
                    new DataColumn(nameof(UserAccountSettingsModel.SettingValue), typeof(string)),
                }
            };
            foreach (UserAccountSettingsModel record in userSettings)
            {
                dataTable.Rows.Add(record.SettingType, record.SettingTypeID, record.SettingValue);
            }
            return dataTable;
        }
    }
}
