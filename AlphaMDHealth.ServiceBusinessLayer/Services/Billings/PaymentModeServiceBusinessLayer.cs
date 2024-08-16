using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class PaymentModeServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        ///  PaymentMode Service constructor
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public PaymentModeServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get PaymentModes from database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="paymentModeID">payment modeI id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of PaymentModes and operation status</returns>
        public async Task<PaymentModeDTO> GetPaymentModesAsync(byte languageID, long permissionAtLevelID, byte paymentModeID, long recordCount)
        {
            PaymentModeDTO paymentModeData = new PaymentModeDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    paymentModeData.ErrCode = ErrorCode.InvalidData;
                    return paymentModeData;
                }
                paymentModeData.AccountID = AccountID;
                if (paymentModeData.AccountID < 1)
                {
                    paymentModeData.ErrCode = ErrorCode.Unauthorized;
                    return paymentModeData;
                }
                paymentModeData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetConfigurationDataAsync(paymentModeData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_BILLING_GROUP}").ConfigureAwait(false))
                {
                    paymentModeData.PermissionAtLevelID = permissionAtLevelID;
                    paymentModeData.LanguageID = languageID;
                    paymentModeData.RecordCount = recordCount;
                    paymentModeData.PaymentMode = new PaymentModeModel
                    {
                        PaymentModeID = paymentModeID
                    };
                    paymentModeData.FeatureFor = FeatureFor;
                    await new PaymentModeServiceDataLayer().GetPaymentModesAsync(paymentModeData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                paymentModeData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return paymentModeData;
        }

        private async Task<bool> GetConfigurationDataAsync(BaseDTO baseDTO, byte languageID, string groupNames)
        {
            baseDTO.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, groupNames
                    , languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
            if (baseDTO.Resources != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Save Payment Mode into database
        /// </summary>
        /// <param name="paymentModeData">Payment Mode data to be saved</param>
        /// <param name="permissionAtLevelID">permission at level id</param> 
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SavePaymentModeAsync(PaymentModeDTO paymentModeData, long permissionAtLevelID)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || paymentModeData.PaymentMode == null)
                {
                    paymentModeData.ErrCode = ErrorCode.InvalidData;
                    return paymentModeData;
                }
                if (paymentModeData.IsActive)
                {
                    if(!GenericMethods.IsListNotEmpty(paymentModeData.PaymentModes))
                    {
                        paymentModeData.ErrCode = ErrorCode.InvalidData;
                        return paymentModeData;
                    }
                    paymentModeData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(paymentModeData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_BILLING_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(paymentModeData.PaymentModes, paymentModeData.Resources))
                        {
                            paymentModeData.ErrCode = ErrorCode.InvalidData;
                            return paymentModeData;
                        }
                    }
                    else
                    {
                        return paymentModeData;
                    }
                }
                paymentModeData.AccountID = AccountID;
                paymentModeData.PermissionAtLevelID = permissionAtLevelID;
                paymentModeData.FeatureFor = FeatureFor;
                await new PaymentModeServiceDataLayer().SavePaymentModeAsync(paymentModeData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                paymentModeData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return paymentModeData;
        }
    }
}
