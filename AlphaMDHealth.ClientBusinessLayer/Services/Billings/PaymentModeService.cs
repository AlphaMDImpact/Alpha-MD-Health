using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class PaymentModeService : BaseService
    {
        public PaymentModeService(IEssentials essentials) : base(essentials)
        {
            
        }
        /// <summary>
        /// Sync PaymentModes from service
        /// </summary>
        /// <param name="paymentModeData">PaymentModeData data to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>PaymentModes received from server in paymentModeData and operation status</returns>
        public async Task SyncPaymentModesFromServerAsync(PaymentModeDTO paymentModeData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PAYMENT_MODES_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(paymentModeData.RecordCount, CultureInfo.InvariantCulture) },
                        { Constants.SE_PAYMENT_MODE_ID_QUERY_KEY, Convert.ToString(paymentModeData.PaymentMode.PaymentModeID, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                paymentModeData.ErrCode = httpData.ErrCode;
                if (paymentModeData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(paymentModeData, data);
                        MapPaymentModes(data, paymentModeData);
                    }
                }
            }
            catch (Exception ex)
            {
                paymentModeData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync PaymentMode data to server
        /// </summary>
        /// <param name="paymentModeData">object to return operation status</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status</returns>
        public async Task SyncPaymentModeToServerAsync(PaymentModeDTO paymentModeData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<PaymentModeDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PAYMENT_MODE_ASYNC_PATH,
                    ContentToSend = paymentModeData,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    },
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                paymentModeData.ErrCode = httpData.ErrCode;
                paymentModeData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                paymentModeData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapPaymentModes(JToken data, PaymentModeDTO paymentModeData)
        {
            paymentModeData.PaymentModes = (data[nameof(PaymentModeDTO.PaymentModes)]?.Count() > 0) ?
                (from dataItem in data[nameof(PaymentModeDTO.PaymentModes)]
                 select new PaymentModeModel
                 {
                     PaymentModeID = (byte)dataItem[nameof(PaymentModeModel.PaymentModeID)],
                     Name = (string)dataItem[nameof(PaymentModeModel.Name)],
                     LanguageID = (byte)dataItem[nameof(PaymentModeModel.LanguageID)],
                     LanguageName = (string)dataItem[nameof(PaymentModeModel.LanguageName)],
                 }).ToList() : null;
        }
    }
}
