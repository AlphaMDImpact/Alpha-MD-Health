using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;

namespace AlphaMDHealth.ClientBusinessLayer;

public class UserSubscriptionService : BaseService
{
    public UserSubscriptionService(IEssentials essentials) : base(essentials)
    {
    }

    /// <summary>
    /// Get subscription plans data
    /// </summary>
    /// <param name="subscriptionData">Subscription plans data</param>
    /// <returns>Subscription data with operation status</returns>
    public async Task GetSubscriptionPlansAsync(SubscriptionDTO subscriptionData)
    {
        try
        {
            //if (MobileConstants.IsMobilePlatform)
            //{
            //    //todo:
            //    //subscriptionData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            //}
            //else
            //{
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                PathWithoutBasePath = UrlConstants.GET_SUBSCRIPTION_PLANS_ASYNC,
                QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() }
                    }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            subscriptionData.ErrCode = httpData.ErrCode;
            if (subscriptionData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(subscriptionData, data);
                    SetPageResources(subscriptionData.Resources);
                    subscriptionData.Plans = MapPlans(data, nameof(SubscriptionDTO.Plans));
                    //subscriptionData.UserPlan = MapUserPlan(data[nameof(SubscriptionDTO.UserPlan)]);
                }
            }
            //}
            if (subscriptionData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(subscriptionData.Plans))
            {
                foreach (var plan in subscriptionData.Plans)
                {
                    plan.TotalCharges = plan.Charges - (plan.Charges * (plan.DiscountPercentage / 100));
                    plan.PlanDetails = plan.PlanDetails
                        .Replace($"@{nameof(PlanModel.TotalCharges)}", plan.TotalCharges.ToString())
                        .Replace($"@{nameof(PlanModel.Charges)}", plan.Charges.ToString())
                        .Replace($"@{nameof(PlanModel.DurationInDays)}", plan.DurationInDays.ToString())
                        .Replace($"@{nameof(PlanModel.NoOfScans)}", plan.NoOfScans.ToString())
                        .Replace($"@{nameof(PlanModel.DiscountPercentage)}", plan.DiscountPercentage.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            subscriptionData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Save user subscription plan data
    /// </summary>
    /// <param name="subscriptionData">user subscription plan data</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>user subscription plan data save operation status</returns>
    public async Task SaveUserSubscriptionPlanAsync(SubscriptionDTO subscriptionData, CancellationToken cancellationToken)
    {
        try
        {
            //if (MobileConstants.IsMobilePlatform)
            //{
            //await new SubscriptionDatabase().GetUserSubscriptionPlanForServerSyncAsync(subscriptionData).ConfigureAwait(false);
            //if (subscriptionData.UserPlan == null)
            //{
            //    return;
            //}
            //}
            var httpData = new HttpServiceModel<SubscriptionDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_USER_SUBSCRIPTION_PLAN_ASYNC,
                QueryParameters = new NameValueCollection { { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() } },
                ContentToSend = subscriptionData
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            subscriptionData.ErrCode = httpData.ErrCode;
            //if (MobileConstants.IsMobilePlatform && subscriptionData.ErrCode == ErrorCode.OK)
            //{
            //subscriptionData.UserPlan.ForEach(item => item.IsSynced = true);
            //await new SubscriptionDatabase().UpdateUserSubscriptionPlanAsync(subscriptionData, true).ConfigureAwait(false);
            //}
        }
        catch (Exception ex)
        {
            subscriptionData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private List<PlanModel> MapPlans(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select new PlanModel
               {
                   PlanID = GetDataItem<Int16>(dataItem, nameof(PlanModel.PlanID)),
                   PlanCode = GetDataItem<string>(dataItem, nameof(PlanModel.PlanCode)),
                   PlanName = GetDataItem<string>(dataItem, nameof(PlanModel.PlanName)),
                   PlanDetails = GetDataItem<string>(dataItem, nameof(PlanModel.PlanDetails)),
                   Charges = GetDataItem<decimal>(dataItem, nameof(PlanModel.Charges)),
                   DurationInDays = GetDataItem<int>(dataItem, nameof(PlanModel.DurationInDays)),
                   DiscountPercentage = GetDataItem<decimal>(dataItem, nameof(PlanModel.DiscountPercentage)),
                   NoOfScans = GetDataItem<byte>(dataItem, nameof(PlanModel.NoOfScans)),
                   SequenceNo = GetDataItem<byte>(dataItem, nameof(PlanModel.SequenceNo)),
                   IsPopularPlan = GetDataItem<bool>(dataItem, nameof(PlanModel.IsPopularPlan)),
                   IsActive = GetDataItem<bool>(dataItem, nameof(PlanModel.IsActive)),
                   AddedByID = GetDataItem<long>(dataItem, nameof(PlanModel.AddedByID)),
                   AddedON = GetDataItem<DateTimeOffset>(dataItem, nameof(PlanModel.AddedON)),
                   LastModifiedByID = GetDataItem<long>(dataItem, nameof(PlanModel.LastModifiedByID)),
                   LastModifiedON = GetDataItem<DateTimeOffset>(dataItem, nameof(PlanModel.LastModifiedON)),
               }).ToList()
            : null;
    }

    //private List<UserPlanModel> MapUserPlans(JToken data, string collectionName)
    //{
    //    return (data[collectionName]?.Count() > 0)
    //        ? (from dataItem in data[collectionName]
    //           select MapUserPlan(dataItem)).ToList()
    //        : null;
    //}

    //private UserPlanModel MapUserPlan(JToken dataItem)
    //{
    //    if (dataItem != null)
    //    {
    //        return new UserPlanModel
    //        {
    //            PlanID = GetDataItem<Int16>(dataItem, nameof(UserPlanModel.PlanID)),
    //            AccountID = GetDataItem<long>(dataItem, nameof(UserPlanModel.AccountID)),
    //            IsActive = GetDataItem<bool>(dataItem, nameof(UserPlanModel.IsActive)),
    //            AddedByID = GetDataItem<long>(dataItem, nameof(UserPlanModel.AddedByID)),
    //            AddedON = GetDataItem<DateTimeOffset>(dataItem, nameof(UserPlanModel.AddedON)),
    //            LastModifiedByID = GetDataItem<long>(dataItem, nameof(UserPlanModel.LastModifiedByID)),
    //            LastModifiedON = GetDataItem<DateTimeOffset>(dataItem, nameof(UserPlanModel.LastModifiedON))
    //        };
    //    }
    //    return null;
    //}
}