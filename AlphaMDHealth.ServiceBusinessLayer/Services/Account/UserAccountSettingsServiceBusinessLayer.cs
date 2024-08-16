using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    /// <summary>
    /// Service Business logic for UseraccountSettings
    /// </summary>
    public class UserAccountSettingsServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// User Account setting service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public UserAccountSettingsServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// To Get User account settings
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>User accounts settings</returns>
        public async Task<UserAccountSettingDTO> GetUserAccountSettingsAsync(byte languageID, long permissionAtLevelID, short recordCount)
        {
            UserAccountSettingDTO userSettingData = new UserAccountSettingDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    userSettingData.ErrCode = ErrorCode.InvalidData;
                    return userSettingData;
                }
                if (AccountID < 1)
                {
                    userSettingData.ErrCode = ErrorCode.Unauthorized;
                    return userSettingData;
                }
                userSettingData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                userSettingData.LanguageID = languageID;
                if (await GetSettingsResourcesAsync(userSettingData, false, GroupConstants.RS_COMMON_GROUP, GetReadingResourceGroups()).ConfigureAwait(false) && GenericMethods.IsListNotEmpty(userSettingData.Resources))
                {
                    userSettingData.RecordCount = recordCount;
                    userSettingData.AccountID = AccountID;
                    userSettingData.PermissionAtLevelID = permissionAtLevelID;
                    userSettingData.LastModifiedON = GenericMethods.GetDefaultDateTime;
                    userSettingData.FeatureFor = FeatureFor;
                    await new UserAccountSettingsServiceDataLayer().GetUserAccountSettingsAsync(userSettingData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                userSettingData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return userSettingData;
        }

        private string GetReadingResourceGroups()
        {
            return $"{GroupConstants.RS_READINGS_GROUP},{GroupConstants.RS_READING_CATEGORY_GROUP},{GroupConstants.RS_READING_FILTERS_GROUP}," +
                $"{GroupConstants.RS_NUMERIC_GROUP},{GroupConstants.RS_COUNTER_GROUP},{GroupConstants.RS_DAILY_COUNTER_GROUP}," +
                $"{GroupConstants.RS_HIGH_RISK_LOW_RISK_GROUP},{GroupConstants.RS_POSITIVE_NEGATIVE_GROUP},{GroupConstants.RS_PRESENT_ABSENT_GROUP}," +
                $"{GroupConstants.RS_BLOOD_TYPE_GROUP},{GroupConstants.RS_GENDER_TYPE_GROUP},{GroupConstants.RS_AGE_TYPE_GROUP}," +
                $"{GroupConstants.RS_ORGANISATION_READINGS_PAGE_GROUP},{GroupConstants.RS_USER_ACCOUNT_SETTINGS_GROUP}," +
                $"{GroupConstants.RS_SUPPORTED_DEVICE_GROUP},{GroupConstants.RS_DEVICE_GROUP},{GroupConstants.RS_COMMON_GROUP}," +
                $"{GroupConstants.RS_YES_NO_TYPE_GROUP},{GroupConstants.RS_TEXT_READING_GROUP},{GroupConstants.RS_NORMAL_ABNORMAL_GROUP},{GroupConstants.RS_REACTIVE_NONREACTIVE_VALUE_TYPE_GROUP}";
        }


        /// <summary>
        /// Save User Account Settings
        /// </summary>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="userAccountSettingData">User account setting data to save</param>
        /// <returns>Operation status</returns>
        public async Task<UserAccountSettingDTO> SaveUserAccountSettingsAsync(long permissionAtLevelID, UserAccountSettingDTO userAccountSettingData)
        {
            try
            {
                if (permissionAtLevelID < 1 || userAccountSettingData.UserAccountSettings == null || userAccountSettingData.UserAccountSettings.Count < 1)
                {
                    userAccountSettingData.ErrCode = ErrorCode.InvalidData;
                    return userAccountSettingData;
                }
                if (AccountID < 1)
                {
                    userAccountSettingData.ErrCode = ErrorCode.Unauthorized;
                    return userAccountSettingData;
                }
                userAccountSettingData.AccountID = AccountID;
                userAccountSettingData.PermissionAtLevelID = permissionAtLevelID;
                await new UserAccountSettingsServiceDataLayer().SaveUserSettingsAsync(userAccountSettingData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                userAccountSettingData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return userAccountSettingData;
        }
    }
}
