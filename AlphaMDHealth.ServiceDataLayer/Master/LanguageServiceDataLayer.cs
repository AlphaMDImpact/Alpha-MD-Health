using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class LanguageServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Returns list of languages for specified Organisation
        /// </summary>
        /// <param name = "languagesData"> languages baes on orgnisation id as reference</param>
        /// <param name = "checkPermission"> Permissions to check for</param>
        /// <param name = "returnPermission"> Permission to return</param>
        /// <returns>List of languages</returns>
        public async Task GetLanguagesAsync(LanguageDTO languagesData, string checkPermission, string returnPermission)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), languagesData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), languagesData.AccountID, DbType.Int64);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID), languagesData.OrganisationID, DbType.Int64);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), GenericMethods.ApplyUtcDateTimeFormatToUtcValue(languagesData.LastModifiedON ?? GenericMethods.GetDefaultDateTime), DbType.DateTimeOffset, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_CHECK_PERMISSION, checkPermission, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_RETURN_PERMISSIONS_FOR, checkPermission, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), languagesData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_LANGUAGES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                languagesData.Languages = (await result.ReadAsync<LanguageModel>().ConfigureAwait(false))?.ToList();
                if (!string.IsNullOrWhiteSpace(returnPermission))
                {
                    await MapReturnPermissionsAsync(languagesData, result).ConfigureAwait(false);
                }
            }
            languagesData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }
    }
}