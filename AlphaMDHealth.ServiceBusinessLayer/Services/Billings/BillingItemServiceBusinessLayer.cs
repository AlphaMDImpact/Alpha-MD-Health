using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class BillingItemServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// BillingItem service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public BillingItemServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }


        /// <summary>
        /// Get BillingItems from database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="billingItemID">Billing ItemID id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of BillingItems and operation status</returns>
        public async Task<BillingItemDTO> GetBillingItemsAsync(byte languageID, long permissionAtLevelID, short billingItemID, long recordCount)
        {
            BillingItemDTO billingItemData = new BillingItemDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    billingItemData.ErrCode = ErrorCode.InvalidData;
                    return billingItemData;
                }
                billingItemData.AccountID = AccountID;
                if (billingItemData.AccountID < 1)
                {
                    billingItemData.ErrCode = ErrorCode.Unauthorized;
                    return billingItemData;
                }
                billingItemData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetConfigurationDataAsync(billingItemData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_BILLING_GROUP}").ConfigureAwait(false))
                {
                    billingItemData.PermissionAtLevelID = permissionAtLevelID;
                    billingItemData.LanguageID = languageID;
                    billingItemData.RecordCount = recordCount;
                    billingItemData.BillingItem = new BillingItemModel
                    {
                        BillingItemID = billingItemID
                    };
                    billingItemData.FeatureFor = FeatureFor;
                    await new BillingItemServiceDataLayer().GetBillingItemsAsync(billingItemData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                billingItemData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return billingItemData;
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
        /// Save Billing Item into database
        /// </summary>
        /// <param name="billingItemData">Billing Item data to be saved</param>
        /// <param name="permissionAtLevelID">permission at level id</param> 
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SaveBillingItemAsync(BillingItemDTO billingItemData, long permissionAtLevelID)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || billingItemData.BillingItem == null)
                {
                    billingItemData.ErrCode = ErrorCode.InvalidData;
                    return billingItemData;
                }
                if (billingItemData.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(billingItemData.BillingItems))
                    {
                        billingItemData.ErrCode = ErrorCode.InvalidData;
                        return billingItemData;
                    }
                    billingItemData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(billingItemData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_BILLING_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(billingItemData.BillingItems, billingItemData.Resources))
                        {
                            billingItemData.ErrCode = ErrorCode.InvalidData;
                            return billingItemData;
                        }
                    }
                    else
                    {
                        return billingItemData;
                    }
                }
                billingItemData.AccountID = AccountID;
                billingItemData.PermissionAtLevelID = permissionAtLevelID;
                billingItemData.FeatureFor = FeatureFor;
                await new BillingItemServiceDataLayer().SaveBillingItemAsync(billingItemData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                billingItemData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return billingItemData;
        }

        /// <summary>
        /// Gets Patient Bill(s) Data
        /// </summary>
        /// <param name="languageID">selected language id</param>
        /// <param name="permissionAtLevelID">level at which permission required</param>
        /// <param name="patientBillID">patient Bill ID</param>
        /// <param name="recordCount">record count </param>
        /// <param name="selectedUserID">Selected user ID</param>
        /// <param name="organisationID">Selected user ID</param>
        /// <returns>Patient Bills data</returns>
        public async Task<BillingItemDTO> GetPatientBillsAsync(byte languageID, long permissionAtLevelID, Guid patientBillID, long recordCount, long selectedUserID, long organisationID)
        {
            BillingItemDTO billData = new BillingItemDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    billData.ErrCode = ErrorCode.InvalidData;
                    return billData;
                }
                if (AccountID < 1)
                {
                    billData.ErrCode = ErrorCode.Unauthorized;
                    return billData;
                }
                billData.AccountID = AccountID;
                billData.LanguageID = languageID;
                billData.OrganisationID = organisationID;
                if (await GetSettingsResourcesAsync(billData, true, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_BILLING_GROUP}").ConfigureAwait(false))
                {
                    billData.PermissionAtLevelID = permissionAtLevelID;
                    billData.SelectedUserID = selectedUserID;
                    billData.RecordCount = recordCount;
                    billData.PatientBillItem = new PatientBillModel
                    {
                        PatientBillID = patientBillID
                    };
                    billData.FeatureFor = FeatureFor;
                    await new BillingItemServiceDataLayer().GetPatientBillsAsync(billData).ConfigureAwait(false);
                }

            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                billData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return billData;
        }

        /// <summary>
        /// Saves Patient Bill Data
        /// </summary>
        /// <param name="billingItemData">Bill data to save</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SavePatientBillAsync(BillingItemDTO billingItemData, long permissionAtLevelID)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1
                    || !GenericMethods.IsListNotEmpty(billingItemData.PatientBills)
                    || !GenericMethods.IsListNotEmpty(billingItemData.PatientBillItems)
                    || billingItemData.PatientBills.Any(x => x.PatientBillID == Guid.Empty || (!x.IsActive && !billingItemData.PatientBillItems.Any(y => y.PatientBillID == x.PatientBillID && y.IsActive)))
                    || HasDuplicatePatientBills(billingItemData.PatientBillItems)
                    || !billingItemData.PatientBillItems.Any(x => x.IsActive == true))
                {
                    billingItemData.ErrCode = ErrorCode.InvalidData;
                    return billingItemData;
                }
                if (billingItemData.IsActive)
                {
                    billingItemData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(billingItemData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_BILLING_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(billingItemData.PatientBills, billingItemData.Resources))
                        {
                            billingItemData.ErrCode = ErrorCode.InvalidData;
                            return billingItemData;
                        }
                    }
                    else
                    {
                        return billingItemData;
                    }
                }
                billingItemData.AccountID = AccountID;
                billingItemData.PermissionAtLevelID = permissionAtLevelID;
                billingItemData.PatientBillItems = billingItemData.PatientBillItems.GroupBy(elem => elem.BillingItemID)
                  .Select(group => group.ToList().Count == 1 ? group.FirstOrDefault() : group.FirstOrDefault(x => x.IsActive == true)).ToList();
                billingItemData.FeatureFor = FeatureFor;
                await new BillingItemServiceDataLayer().SavePatientBillAsync(billingItemData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                billingItemData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return billingItemData;
        }

        private bool HasDuplicatePatientBills(List<PatientBillItemModel> billingItemData)
        {
            return billingItemData.FindAll(x => x.IsActive).Count != billingItemData.FindAll(x => x.IsActive).GroupBy(x => x.BillingItemID).Select(y => y.First()).ToList().Count ? true : false;
        }
    }
}