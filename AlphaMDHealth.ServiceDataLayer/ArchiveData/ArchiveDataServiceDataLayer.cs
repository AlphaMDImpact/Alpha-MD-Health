using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class ArchiveDataServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Archives Logs From main Db to Archive Db
        /// </summary>
        /// <returns>Operation Status Code</returns>
        public async Task<ErrorCode> ArchiveDataTasksAsync(JobAction jobAction)
        {
            string storeproc;
            if (jobAction == JobAction.ArchiveErrorLogs)
            {
                storeproc = SPNameConstants.USP_ARCHIVE_ERROR_LOGS;
            }
            else if (jobAction == JobAction.ArchiveAuditLogs)
            {
                storeproc = SPNameConstants.USP_ARCHIVE_AUDIT_LOGS;
            }
            else if (jobAction == JobAction.ArchiveUserAccountSessionsHistory)
            {
                storeproc = SPNameConstants.USP_ARCHIVE_USER_ACCOUNT_SESSIONS_HISTORY;
            }
            else
            {
                storeproc = SPNameConstants.USP_ARCHIVE_USER_COMMUNICATIONS_HISTORY;
            }

            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.ErrCode)), 200, DbType.Int16, direction: ParameterDirection.Output);
            await connection.QueryAsync(storeproc, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        }
    }
}