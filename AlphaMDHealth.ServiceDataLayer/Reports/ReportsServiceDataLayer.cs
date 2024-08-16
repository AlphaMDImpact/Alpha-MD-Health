using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class ReportsServiceDataLayer : BaseServiceDataLayer
    {

        /// <summary>
        /// Get Bills
        /// </summary>
        /// <param name="billingData">Bills Data</param>
        /// <returns>Get Bills and operation status</returns>
        public async Task GetBillsAsync(BillingItemDTO billingData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), billingData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.FromDate), billingData.FromDate, DbType.DateTimeOffset, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ToDate), billingData.ToDate, DbType.DateTimeOffset, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.RecordCount), billingData.RecordCount, DbType.Int16, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), billingData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.PermissionAtLevelID), billingData.PermissionAtLevelID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), null, DbType.DateTimeOffset, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LanguageID), billingData.LanguageID, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_CHECK_PERMISSION, AppPermissions.BillingReportsView.ToString(), DbType.String, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_RETURN_PERMISSIONS_FOR, AppPermissions.BillingReportsView.ToString(), DbType.String, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), billingData.ErrCode, DbType.Int16, direction: ParameterDirection.InputOutput);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_BILLS_REPORT, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                if (!result.IsConsumed)
                {
                    billingData.PatientBills = (await result.ReadAsync<PatientBillModel>().ConfigureAwait(false))?.ToList();
                }
                await MapReturnPermissionsAsync(billingData, result).ConfigureAwait(false);
            }
            billingData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }
    }
}