using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;
using System.Reflection.Metadata;

namespace AlphaMDHealth.ServiceDataLayer;

public class CareFlixHealthScanServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get HealthScans(s)
    /// </summary>
    /// <param name="healthScans">HealthScans data object</param>
    /// <returns>HealthScans(s)</returns>
    public async Task<HealthScanDTO> GetHealthScansAsync(HealthScanDTO healthScans)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), healthScans.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(HealthScanModel.TransactionID), healthScans.ExternalServiceTransaction.TransactionID, DbType.Int64, ParameterDirection.Input);
        MapCommonSPParameters(healthScans, parameters, $"{AppPermissions.HealthScansView},{AppPermissions.CreditHistoryView}", $"{AppPermissions.CreditAddEdit},{AppPermissions.CreditHistoryView}");
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_EXTERNAL_SERVICE_TRANSACTIONS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            await MapHealthScansDataAsync(healthScans, result).ConfigureAwait(false);
            await MapReturnPermissionsAsync(healthScans, result).ConfigureAwait(false);
        }
        healthScans.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        return healthScans;
    }

    private async Task MapHealthScansDataAsync(HealthScanDTO healthScans, SqlMapper.GridReader result)
    {
        if (healthScans.RecordCount == -1 && !result.IsConsumed)
        {
            healthScans.ExternalServiceTransaction = (await result.ReadAsync<HealthScanModel>().ConfigureAwait(false))?.FirstOrDefault();
        }
        else
        {
            healthScans.ExternalServiceTransactions = (await result.ReadAsync<HealthScanModel>().ConfigureAwait(false)).ToList();
            healthScans.OrganisationCredits = (await result.ReadSingleAsync<int>().ConfigureAwait(false));
            healthScans.CreditsAssigned = (await result.ReadSingleAsync<int>().ConfigureAwait(false));
            healthScans.CreditsAvailable = (await result.ReadSingleAsync<int>().ConfigureAwait(false));
            healthScans.NumberOfPatient = (await result.ReadSingleAsync<int>().ConfigureAwait(false));
        }
    }

    /// <summary>
    /// Save Health Scan
    /// </summary>
    /// <param name="healthScansData">Reference object which holds task data</param>
    /// <returns>Operation Status Code/returns>
    public async Task SaveHealthScanAsync(HealthScanDTO healthScansData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), healthScansData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(HealthScanModel.PaymentID), healthScansData.ExternalServiceTransaction.PaymentID, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(HealthScanModel.IsPatient), healthScansData.ExternalServiceTransaction.IsPatient, DbType.Boolean, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(HealthScanModel.Quantity), healthScansData.ExternalServiceTransaction.Quantity, DbType.Double, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(HealthScanModel.UnitPrice), healthScansData.ExternalServiceTransaction.UnitPrice, DbType.Double, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(HealthScanModel.DiscountPercentage), healthScansData.ExternalServiceTransaction.DiscountPercentage, DbType.Double, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(HealthScanModel.DiscountPrice), healthScansData.ExternalServiceTransaction.DiscountPrice, DbType.Double, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(HealthScanModel.TotalPrice), healthScansData.ExternalServiceTransaction.TotalPrice, DbType.Double, ParameterDirection.Input);
        MapCommonSPParameters(healthScansData, parameter, AppPermissions.HealthScansView.ToString());
        await connection.QueryAsync(SPNameConstants.USP_SAVE_EXTERNAL_SERVICE_TRANSACTION, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        healthScansData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }
}