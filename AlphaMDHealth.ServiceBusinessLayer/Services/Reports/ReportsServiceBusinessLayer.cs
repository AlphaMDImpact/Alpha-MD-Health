using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class ReportsServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Reports service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public ReportsServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        private async Task<bool> GetConfigurationDataAsync(BaseDTO baseDTO, byte languageID, string groupNames)
        {
            baseDTO.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, groupNames
                    , languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
            if (baseDTO.Resources != null)
            {
                var organisationSettings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, languageID, default, 0, baseDTO.OrganisationID, false).ConfigureAwait(false)).Settings;
                if (organisationSettings != null)
                {
                    baseDTO.Settings = organisationSettings;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get Bills from database
        /// </summary>
        /// <param name="permissionAtLevelID"></param>
        /// <param name="fromDate">From Date</param>
        /// <param name="toDate">To Date</param>
        /// <param name="languageID">Language Id</param>
        /// <param name="recordCount">Record Count</param>
        /// <returns>Get Bills and operation status</returns>
        public async Task<BillingItemDTO> GetBillsAsync(long permissionAtLevelID, string fromDate, string toDate, byte languageID, long recordCount)
        {
            BillingItemDTO billingData = new BillingItemDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    billingData.ErrCode = ErrorCode.InvalidData;
                    return billingData;
                }
                billingData.AccountID = AccountID;
                if (billingData.AccountID < 1)
                {
                    billingData.ErrCode = ErrorCode.Unauthorized;
                    return billingData;
                }
                billingData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetConfigurationDataAsync(billingData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_REPORT_GROUP},{GroupConstants.RS_BILLING_GROUP},{GroupConstants.RS_LOGIN_FLOW_PAGE_GROUP},{GroupConstants.RS_ORGANISATION_PROFILE_GROUP}").ConfigureAwait(false))
                {
                    billingData.FromDate = fromDate;
                    billingData.ToDate = toDate;
                    billingData.RecordCount = recordCount;
                    billingData.PermissionAtLevelID = permissionAtLevelID;
                    billingData.LanguageID = languageID;
                    billingData.FeatureFor = FeatureFor;
                    await new ReportsServiceDataLayer().GetBillsAsync(billingData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                billingData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return billingData;
        }
    }
}
