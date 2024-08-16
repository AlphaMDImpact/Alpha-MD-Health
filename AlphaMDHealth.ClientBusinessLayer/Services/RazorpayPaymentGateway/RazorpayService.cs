using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Collections.Specialized;

namespace AlphaMDHealth.ClientBusinessLayer;

public class RazorpayService : BaseService
{
    /// <summary>
    /// Razorpay Service
    /// </summary>
    /// <param name="essentials"></param>
    public RazorpayService(IEssentials essentials) : base(essentials)
    {

    }


    /// <summary>
    /// Save Razorpay Payment & Order Detail
    /// </summary>
    /// <param name="razorpayData">Razor pay Data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SyncRazorpayPaymentDetailToServerAsync(RazorpayDTO razorpayData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<RazorpayDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_RAZORPAY_PAYMENT_DETAIL_ASYNC,               
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = razorpayData,
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            razorpayData.ErrCode = httpData.ErrCode;          
        }
        catch (Exception ex)
        {
            razorpayData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }
}