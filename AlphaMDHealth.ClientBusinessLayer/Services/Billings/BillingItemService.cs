using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class BillingItemService : BaseService
    {
        public BillingItemService(IEssentials essentials):base(essentials)
        {
            
        }
        /// <summary>
        /// Sync BillingItems from service
        /// </summary>
        /// <param name="billingItemData">BillingItemData data to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>BillingItems received from server in billingItemData and operation status</returns>
        public async Task SyncBillingItemsFromServerAsync(BillingItemDTO billingItemData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_BILLING_ITEMS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(billingItemData.RecordCount, CultureInfo.InvariantCulture) },
                        { Constants.SE_BILLING_ITEM_ID_QUERY_KEY, Convert.ToString(billingItemData.BillingItem.BillingItemID, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                billingItemData.ErrCode = httpData.ErrCode;
                if (billingItemData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(billingItemData, data);
                        MapBillingItems(data, billingItemData);
                    }
                }
            }
            catch (Exception ex)
            {
                billingItemData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync BillingItem data to server
        /// </summary>
        /// <param name="billingItemData">object to return operation status</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status</returns>
        public async Task SyncBillingItemToServerAsync(BillingItemDTO billingItemData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<BillingItemDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_BILLING_ITEM_ASYNC_PATH,
                    ContentToSend = billingItemData,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    },
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                billingItemData.ErrCode = httpData.ErrCode;
                billingItemData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                billingItemData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapBillingItems(JToken data, BillingItemDTO billingItemData)
        {
            billingItemData.BillingItems = (data[nameof(BillingItemDTO.BillingItems)]?.Count() > 0) ?
                (from dataItem in data[nameof(BillingItemDTO.BillingItems)]
                 select new BillingItemModel
                 {
                     BillingItemID = (short)dataItem[nameof(BillingItemModel.BillingItemID)],
                     Name = (string)dataItem[nameof(BillingItemModel.Name)],
                     LanguageID = (byte)dataItem[nameof(BillingItemModel.LanguageID)],
                     LanguageName = (string)dataItem[nameof(BillingItemModel.LanguageName)],
                 }).ToList() : null;
        }
    }
}