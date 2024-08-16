using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class ErrorLogService : BaseService
    {
        public ErrorLogService(IEssentials essentials) : base(essentials)
        {
            
        }
        /// <summary>
        /// Sync ErrorLogs to server
        /// </summary>
        /// <param name="returnResult">object to hold Operation status</param>
        /// <param name="cancellationToken">token to cancel current service call when required</param>
        /// <returns>Operation Status</returns>
        public async Task SyncErrorLogsToServerAsync(BaseDTO returnResult, CancellationToken cancellationToken)
        {
            try
            {
                ErrorLogDTO requestData = new ErrorLogDTO();
                await new ErrorLogDatabase().GetErrorLogsToSyncAsync(requestData).ConfigureAwait(false);
                if (GenericMethods.IsListNotEmpty(requestData.ErrorLogs))
                {
                    var httpData = new HttpServiceModel<ErrorLogDTO>
                    {
                        AuthType = AuthorizationType.Basic,
                        CancellationToken = cancellationToken,
                        PathWithoutBasePath = UrlConstants.SAVE_ERROR_LOGS_ASYNC_PATH,
                        ContentToSend = requestData,
                    };
                    await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                    returnResult.ErrCode = httpData.ErrCode;
                    if (returnResult.ErrCode == ErrorCode.OK)
                    {
                        await new ErrorLogDatabase().DeleteErrorLogAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                returnResult.ErrCode = ErrorCode.InvalidData;
                LogError(ex.Message, ex);
            }
        }
    }
}
