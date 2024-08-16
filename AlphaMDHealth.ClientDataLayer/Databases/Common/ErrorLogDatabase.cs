using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientDataLayer
{
    /// <summary>
    /// Represents errorlog database module
    /// </summary>
    public class ErrorLogDatabase : BaseDatabase
    {
        /// <summary>
        /// Gets the list of error log from the database
        /// </summary>
        /// <param name="errorLogData">Object reference to return error log </param>
        /// <returns>ErrorLog List</returns>
        public async Task GetErrorLogsToSyncAsync(ErrorLogDTO errorLogData)
        {
            errorLogData.ErrorLogs = await SqlConnection.QueryAsync<ErrorLogModel>("SELECT * FROM ErrorLogModel").ConfigureAwait(false);
        }

        /// <summary>
        /// Logs the error into the Local DB
        /// </summary>
        /// <param name="errorLog">The error Details</param>
        /// <returns>The task object representing the operation to log the error into the Local DB</returns>
        public async Task LogErrorsAsync(ErrorLogModel errorLog)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                errorLog.CreatedOn = GenericMethods.GetUtcDateTime;
                transaction.Execute("INSERT INTO ErrorLogModel (ErrorFunction, ErrorLineNumber, ErrorLogLevel, ErrorMessage, ErrorNumber, AccountID, CreatedOn) VALUES (?, ?, ?, ?, ?, ?, ?)",
                    errorLog.ErrorFunction, errorLog.ErrorLineNumber, errorLog.ErrorLogLevel, errorLog.ErrorMessage, errorLog.ErrorNumber,
                    errorLog.AccountID, errorLog.CreatedOn);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// To delete the records of ErrorLogModel from local DB
        /// </summary>
        /// <returns> Deletes all records from ErrorLogModel</returns>
        public async Task DeleteErrorLogAsync()
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                transaction.Execute("DELETE FROM ErrorLogModel");
            }).ConfigureAwait(false);
        }
    }
}