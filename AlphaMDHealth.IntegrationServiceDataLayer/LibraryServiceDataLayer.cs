using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.IntegrationServiceDataLayer
{
    public class LibraryServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        ///  Get client data
        /// </summary>
        /// <param name="libraryDetails">object to get data</param>
        /// <returns>Library service details and operation status code</returns>
        public async Task GetClientDataAsync(LibraryServiceDTO libraryDetails)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(LibraryServiceModel.ForApplication), libraryDetails.LibraryInfo.ForApplication, DbType.String);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(LibraryServiceModel.ClientIdentifier), libraryDetails.LibraryInfo.ClientIdentifier, DbType.String);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), libraryDetails.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            libraryDetails.LibraryDetails = (await connection.QueryAsync<LibraryServiceModel>(Constants.USP_GET_CLIENT_DATA, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false)).ToList();
            libraryDetails.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save MicroService Call Logs to Database
        /// </summary>
        /// <param name="libraryServiceLog">Service Call Logs DTO</param>
        /// <returns>Operation Status</returns>
        public async Task SaveServiceCallLogsAsync(LibraryServiceLoggingDto libraryServiceLog)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(LibraryServiceLoggingModel.OrganisationServiceID), libraryServiceLog.LibraryServiceLog.OrganisationServiceID, DbType.Int64);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(LibraryServiceLoggingModel.Status), libraryServiceLog.LibraryServiceLog.Status, DbType.String);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(LibraryServiceLoggingModel.LogData), libraryServiceLog.LibraryServiceLog.LogData, DbType.String);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), libraryServiceLog.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            await connection.ExecuteAsync(Constants.USP_SAVE_SERVICE_LOGS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            libraryServiceLog.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }
    }
}
