using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class SettingServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get Settings for an Organisation
        /// </summary>
        /// <param name="settingsData">Settings data object</param>
        /// <returns>Updated BaseDTO with list of Organisation Settings</returns>
        public async Task GetSettingsAsync(BaseDTO settingsData)
        {
			using var connection = ConnectDatabase();
			connection.Open();
			DynamicParameters parameters = new DynamicParameters();
			parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), settingsData.FeatureFor, DbType.Byte, ParameterDirection.Input);
			parameters.Add(ConcateAt(SPFieldConstants.FIELD_GROUP_NAMES), settingsData.ErrorDescription, DbType.String, direction: ParameterDirection.Input);
			parameters.Add(ConcateAt(SPFieldConstants.FIELD_SETTING_TYPE), settingsData.RecordCount, DbType.String, direction: ParameterDirection.Input);
			parameters.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), GenericMethods.ApplyUtcDateTimeFormatToUtcValue(GenericMethods.GetDefaultDateTime), DbType.DateTimeOffset, ParameterDirection.Input);
			parameters.Add(ConcateAt(nameof(BaseDTO.OrganisationID)), settingsData.OrganisationID, DbType.Int64, ParameterDirection.Input);
			parameters.Add(ConcateAt(SPFieldConstants.FIELD_RETURN_PERMISSIONS_FOR), settingsData.LastModifiedBy, DbType.String, ParameterDirection.Input);
			MapCommonSPParameters(settingsData, parameters, settingsData.AddedBy);
			SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_SETTINGS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
			if (result.HasRows())
			{
				settingsData.Settings = (await result.ReadAsync<SettingModel>().ConfigureAwait(false))?.ToList();
				await MapReturnPermissionsAsync(settingsData, result).ConfigureAwait(false);
				settingsData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
			}
		}
    }
}