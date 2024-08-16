using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class CountryStatesServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Retrieve countries from database
        /// </summary>
        /// <param name="countriesData">Object used to collect the data</param>
        /// <returns>Data and status is retuned using the input DTO</returns>
        public async Task GetCountriesAsync(BaseDTO countriesData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), countriesData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), DBNull.Value, DbType.DateTimeOffset, direction: ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), countriesData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            countriesData.CountryCodes = (await connection.QueryAsync<CountryModel>(SPNameConstants.USP_GET_COUNTRY_CODES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false)).ToList();
            countriesData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }
    }
}