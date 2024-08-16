using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class RazorpayServiceBussinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Razorpay service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public RazorpayServiceBussinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Save Razorpay Payment & Order Detail
        /// </summary>
        /// <param name="languageID"></param>
        /// <param name="permissionAtLevelID"></param>
        /// <param name="razorpayData">Razorpay Data</param>
        /// <returns></returns>
        public async Task<BaseDTO> SaveRazorpayPaymentDetailAsync(long languageID, long permissionAtLevelID, RazorpayDTO razorpayData)
        {
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || (razorpayData.RazorpayOrder.OrderID == null))
                {
                    razorpayData.ErrCode = ErrorCode.InvalidData;
                    return razorpayData;
                }
                if (AccountID < 1)
                {
                    razorpayData.ErrCode = ErrorCode.Unauthorized;
                    return razorpayData;
                }
                razorpayData.AccountID = AccountID;
                razorpayData.PermissionAtLevelID = permissionAtLevelID;
                razorpayData.FeatureFor = FeatureFor;
                await new RazorpayServiceDataLayer().SaveRazorpayPaymentDetailAsync(razorpayData).ConfigureAwait(false);              
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                razorpayData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return razorpayData;
        }
    }
}