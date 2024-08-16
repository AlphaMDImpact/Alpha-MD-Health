using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class ConsentServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Consent service service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public ConsentServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get Consent(s)
        /// </summary>
        /// <param name="languageID">Id of current language</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="consentId">Consent Id</param>
        /// <param name="recordCount">Record Count</param>
        /// <returns>Consent Data and operation status</returns>
        public async Task<ConsentDTO> GetConsentsAsync(byte languageID, long permissionAtLevelID, long consentId, long recordCount)
        {
            ConsentDTO consents = new ConsentDTO();
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1)
                {
                    consents.ErrCode = ErrorCode.InvalidData;
                    return consents;
                }
                if (AccountID < 1)
                {
                    consents.ErrCode = ErrorCode.Unauthorized;
                    return consents;
                }
                consents.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                consents.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_CONSENT_GROUP},{GroupConstants.RS_PLATFORM_TYPE_GROUP},{GroupConstants.RS_TASK_STATUS_GROUP}",
                    languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(consents.Resources))
                {
                    consents.AccountID = AccountID;
                    consents.LanguageID = languageID;
                    consents.PermissionAtLevelID = permissionAtLevelID;
                    consents.RecordCount = recordCount;
                    consents.Consent = new ConsentModel { ConsentID = consentId };
                    consents.FeatureFor = FeatureFor;
                    await new ConsentServiceDataLayer().GetConsentsAsync(consents).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                consents.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return consents;
        }

        /// <summary>
        /// Saves Consent 
        /// </summary>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="consentData">reference object which holds questionnaire data</param>
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SaveConsentAsync(long permissionAtLevelID, ConsentDTO consentData)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || consentData == null || consentData.Consent.PageID < 0 || consentData.Consent.RoleID < 0)
                {
                    consentData ??= new ConsentDTO();
                    consentData.ErrCode = ErrorCode.InvalidData;
                    return consentData;
                }
                if (consentData.IsActive)
                {
                    consentData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(consentData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_CONSENT_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(consentData.Consent, consentData.Resources))
                        {
                            consentData.ErrCode = ErrorCode.InvalidData;
                            return consentData;
                        }
                    }
                    else
                    {
                        return consentData;
                    }
                }
                consentData.AccountID = AccountID;
                consentData.PermissionAtLevelID = permissionAtLevelID;
                consentData.FeatureFor = FeatureFor;
                await new ConsentServiceDataLayer().SaveConsentAsync(consentData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                consentData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return consentData;
        }

        /// <summary>
        /// Save User Consents
        /// </summary>
        /// <param name="consentData">reference object which holds questionnaire data</param>
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SaveUserConsentAsync(ConsentDTO consentData)
        {
            try
            {
                if (consentData == null || consentData.Consents == null || consentData.Consents.Count < 1)
                {
                    consentData ??= new ConsentDTO();
                    consentData.ErrCode = ErrorCode.InvalidData;
                    return consentData;
                }
                if (AccountID < 1)
                {
                    consentData.ErrCode = ErrorCode.Unauthorized;
                    return consentData;
                }
                consentData.AccountID = AccountID;
                await new ConsentServiceDataLayer().SaveUserConsentAsync(consentData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                consentData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return consentData;
        }

        /// <summary>
        /// Get User Consent(s)
        /// </summary>
        /// <param name="languageID">Id of current language</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <returns>User Consent Data and operation status</returns>
        public async Task<ConsentDTO> GetUserConsentsAsync(byte languageID, long permissionAtLevelID, long recordCount)
        {
            ConsentDTO consents = new ConsentDTO();
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1)
                {
                    consents.ErrCode = ErrorCode.InvalidData;
                    return consents;
                }
                if (AccountID < 1)
                {
                    consents.ErrCode = ErrorCode.Unauthorized;
                    return consents;
                }
                consents.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                consents.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_CONSENT_GROUP},{GroupConstants.RS_PLATFORM_TYPE_GROUP},{GroupConstants.RS_TASK_STATUS_GROUP}",
                    languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(consents.Resources))
                {
                    consents.AccountID = AccountID;
                    consents.LanguageID = languageID;
                    consents.PermissionAtLevelID = permissionAtLevelID;
                    consents.RecordCount = recordCount;
                    consents.FeatureFor = FeatureFor;
                    await new ConsentServiceDataLayer().GetUserConsentsAsync(consents).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                consents.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return consents;
        }
    }
}