using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer;

public class SubscriptionServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get Subscription
    /// </summary>
    /// <param name="subscriptionData">Subscription data object</param>
    /// <returns>Subscription(s)</returns>
    public async Task GetSubscriptionPlansAsync(SubscriptionDTO subscriptionData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), subscriptionData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        MapCommonSPParameters(subscriptionData, parameters, AppPermissions.SubscriptionPlansView.ToString(), $"{AppPermissions.BuySubscriptionPlan}");
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_SUBSCRIPTION_PLANS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            subscriptionData.Plans = (await result.ReadAsync<PlanModel>().ConfigureAwait(false)).ToList();
            await MapReturnPermissionsAsync(subscriptionData, result).ConfigureAwait(false);
        }
        subscriptionData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
    }

    /// <summary>
    /// Saves Subscription in Database
    /// </summary>
    /// <param name="subscriptionData">Referenece object that contains Subscription data to be saved in database</param>
    /// <returns>operation status</returns>
    public async Task SaveUserSubscriptionPlanAsync(SubscriptionDTO subscriptionData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), subscriptionData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(UserPlanModel.PlanID)), subscriptionData.UserPlan.PlanID, DbType.Int16, direction: ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(SubscriptionDTO.AccountID)), subscriptionData.AccountID, DbType.Int64, ParameterDirection.Input);
        AddDateTimeParameter(nameof(BaseDTO.FromDate), subscriptionData.UserPlan.FromDate, parameters, ParameterDirection.Input);
        AddDateTimeParameter(nameof(BaseDTO.ToDate), subscriptionData.UserPlan.ToDate, parameters, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(UserPlanModel.IsActive)), subscriptionData.UserPlan.IsActive, DbType.Boolean, direction: ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(UserPlanModel.PaymentID)), subscriptionData.UserPlan.PaymentID, DbType.String, direction: ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(SubscriptionDTO.PermissionAtLevelID)), subscriptionData.PermissionAtLevelID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(SPFieldConstants.FIELD_CHECK_PERMISSION), AppPermissions.BuySubscriptionPlan.ToString(), DbType.String, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(UserPlanModel.UserPlanID)), subscriptionData.UserPlan.UserPlanID, DbType.Int64, direction: ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(SubscriptionDTO.ErrCode)), subscriptionData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
        await connection.QueryAsync(SPNameConstants.USP_SAVE_USER_SUBSCRIPTION_PLAN, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        subscriptionData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(SubscriptionDTO.ErrCode))), OperationType.Save);
        subscriptionData.UserPlan.UserPlanID = parameters.Get<long>(ConcateAt(nameof(UserPlanModel.UserPlanID)));
    }
}