using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceConfiguration;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace AlphaMDHealth.IntegrationServiceDataLayer
{
    public class BaseServiceDataLayer : IDisposable
    {
        /// <summary>
        /// Create a connection to Database
        /// </summary>
        /// <returns>returns a new Database instance.</returns>
        protected IDbConnection ConnectDatabase()
        {
            var myConfig = MyConfiguration.GetInstance;
            string sqlConnection = string.Format(myConfig.GetConfigurationValue(ConfigurationConstants.CS_DB_SETTINGS_CONNECTION_STRING)
                 , myConfig.GetConfigurationValue(ConfigurationConstants.CS_DB_SETTINGS_SERVER_NAME)
                 , myConfig.GetConfigurationValue(ConfigurationConstants.CS_DB_SETTINGS_MICRO_SERVICES_DB_NAME)
                 , myConfig.GetConfigurationValue(ConfigurationConstants.CS_DB_SETTINGS_MICRO_SERVICES_DB_USER_NAME)
                 , myConfig.GetConfigurationValue(ConfigurationConstants.CS_DB_SETTINGS_MICRO_SERVICES_DB_PASSWORD));
            return new SqlConnection(sqlConnection);
        }

        /// <summary>
        /// Save ErrorLogs into DB
        /// </summary>
        /// <param name="errorLogData">Ref object which holds data to store on server and returns status</param>
        /// <returns>Opertaion Status</returns>
        public async Task SaveErrorLogsAsync(ErrorLogDTO errorLogData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + Constants.FIELD_DETAIL_RECORDS, MapToErrorLogTable(errorLogData.ErrorLogs).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), errorLogData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), errorLogData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            await connection.ExecuteAsync(Constants.USP_ERROR_LOGGING, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            errorLogData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        private DataTable MapToErrorLogTable(List<ErrorLogModel> errorLogs)
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(Constants.FIELD_ERROR_LOG_DATE, typeof(DateTimeOffset)),
                    new DataColumn(Constants.FIELD_ERROR_THREAD_INFO, typeof(string)),
                    new DataColumn(nameof(ErrorLogModel.ErrorMessage), typeof(string)),
                    new DataColumn(Constants.FIELD_IS_DB_ERROR, typeof(bool)),
                    new DataColumn(nameof(ErrorLogModel.AccountID), typeof(long)),
                }
            };
            if (errorLogs?.Count > 0)
            {
                foreach (ErrorLogModel item in errorLogs)
                {
                    dataTable.Rows.Add(item.CreatedOn, item.ErrorFunction, item.ErrorMessage, false, item.AccountID);
                }
            }
            return dataTable;
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}