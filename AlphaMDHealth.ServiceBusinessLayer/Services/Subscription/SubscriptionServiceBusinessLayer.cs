using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer;

public class SubscriptionServiceBusinessLayer : BaseServiceBusinessLayer
{
    /// <summary>
    /// Subscription service
    /// </summary>
    /// <param name="httpContext">Instance of HttpContext</param>
    public SubscriptionServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
    {
    }

    /// <summary>
    /// Get Subscription
    /// </summary>
    /// <param name="languageID">Id of current language</param>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <returns>Subscription plans Data and operation status</returns>
    public async Task<SubscriptionDTO> GetSubscriptionPlansAsync(byte languageID, long permissionAtLevelID)
    {
        SubscriptionDTO subscriptionData = new SubscriptionDTO();
        try
        {
            if (languageID < 1 || permissionAtLevelID < 1)
            {
                subscriptionData.ErrCode = ErrorCode.InvalidData;
                return subscriptionData;
            }
            if (AccountID < 1)
            {
                subscriptionData.ErrCode = ErrorCode.Unauthorized;
                return subscriptionData;
            }
            subscriptionData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            subscriptionData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources
                , $"{GroupConstants.RS_ORGANISATION_PROFILE_GROUP},{GroupConstants.RS_COMMON_GROUP}",
                languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
            if (GenericMethods.IsListNotEmpty(subscriptionData.Resources))
            {
                subscriptionData.AccountID = AccountID;
                subscriptionData.LanguageID = languageID;
                subscriptionData.PermissionAtLevelID = permissionAtLevelID;
                subscriptionData.FeatureFor = FeatureFor;
                await new SubscriptionServiceDataLayer().GetSubscriptionPlansAsync(subscriptionData).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            subscriptionData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        return subscriptionData;
    }

    /// <summary>
    /// Save User Subscription Plan
    /// </summary>
    /// <param name="languageID">Language ID</param>
    /// <param name="permissionAtLevelID">level at which permission is required</param>
    /// <param name="subscriptionData">reference object which holds subscription data</param>
    public async Task<BaseDTO> SaveUserSubscriptionPlanAsync(byte languageID, long permissionAtLevelID, SubscriptionDTO subscriptionData)
    {
        try
        {
            if (languageID < 1 || permissionAtLevelID < 1 || AccountID < 1
                || subscriptionData == null || subscriptionData.UserPlan == null || subscriptionData.UserPlan.PlanID < 1
                || (subscriptionData.UserPlan.IsActive && (!subscriptionData.UserPlan.FromDate.HasValue || !subscriptionData.UserPlan.ToDate.HasValue)))
            {
                subscriptionData ??= new SubscriptionDTO();
                subscriptionData.ErrCode = ErrorCode.InvalidData;
                return subscriptionData;
            }
            subscriptionData.AccountID = AccountID;
            subscriptionData.PermissionAtLevelID = permissionAtLevelID;
            subscriptionData.FeatureFor = FeatureFor;
            await new SubscriptionServiceDataLayer().SaveUserSubscriptionPlanAsync(subscriptionData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            subscriptionData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return subscriptionData;
    }
}