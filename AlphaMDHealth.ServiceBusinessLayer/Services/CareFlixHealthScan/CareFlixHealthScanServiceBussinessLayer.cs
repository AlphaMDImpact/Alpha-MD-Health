using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class CareFlixHealthScanServiceBussinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// HealthScans service service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public CareFlixHealthScanServiceBussinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get HealthScans(s)
        /// </summary>
        /// <param name="languageID">Id of current language</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="recordCount">Record Count</param>
        /// <returns>HealthScans Data and operation status</returns>
        public async Task<HealthScanDTO> GetHealthScansAsync(byte languageID, long permissionAtLevelID, long recordCount , long selectedOrganisationID, long transacationID)
        {
            HealthScanDTO healthScans = new HealthScanDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    healthScans.ErrCode = ErrorCode.InvalidData;
                    return healthScans;
                }
                if (AccountID < 1)
                {
                    healthScans.ErrCode = ErrorCode.Unauthorized;
                    return healthScans;
                }
                healthScans.AccountID = AccountID;
                healthScans.LanguageID = languageID;
                if (await GetSettingsResourcesAsync(healthScans, true, string.Empty,$"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_CARE_FLIX_SERVICE_GROUP}").ConfigureAwait(false))
                {                  
                    healthScans.RecordCount = recordCount;
                    healthScans.PermissionAtLevelID = permissionAtLevelID;
                    healthScans.ExternalServiceTransaction = new HealthScanModel { TransactionID = transacationID };
                    healthScans.FeatureFor = FeatureFor;
                    await new CareFlixHealthScanServiceDataLayer().GetHealthScansAsync(healthScans).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                healthScans.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return healthScans;
        }

        /// <summary>
        /// Saves Health Scan
        /// </summary>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="HealthScans">reference object which holds HealthScans data</param>
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SaveHealthScanAsync(long languageID, long permissionAtLevelID, HealthScanDTO healthScansData, bool IsAssignScanPage)
        {
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || healthScansData?.ExternalServiceTransaction == null)
                {
                    healthScansData.ErrCode = ErrorCode.InvalidData;
                    return healthScansData;
                }
                if (AccountID < 1)
                {
                    healthScansData.ErrCode = ErrorCode.Unauthorized;
                    return healthScansData;
                }           
                healthScansData.AccountID = AccountID;
                healthScansData.PermissionAtLevelID = permissionAtLevelID;
                healthScansData.FeatureFor = FeatureFor;
                await new CareFlixHealthScanServiceDataLayer().SaveHealthScanAsync(healthScansData).ConfigureAwait(false);              
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                healthScansData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return healthScansData;
        }
    }
}