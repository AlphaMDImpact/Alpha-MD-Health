using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer;

public class ConsentServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get Consent(s)
    /// </summary>
    /// <param name="consents">Consent data object</param>
    /// <returns>Consent(s)</returns>
    public async Task GetConsentsAsync(ConsentDTO consents)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), consents.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ConsentModel.ConsentID), consents.Consent.ConsentID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
        MapCommonSPParameters(consents, parameter, AppPermissions.ConsentsView.ToString(), $"{AppPermissions.ConsentAddEdit},{AppPermissions.ConsentDelete}");
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_CONSENTS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            await MapConsentDataAsync(consents, result).ConfigureAwait(false);
            await MapReturnPermissionsAsync(consents, result).ConfigureAwait(false);
        }
        consents.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Saves consent in Database
    /// </summary>
    /// <param name="consentData">Referenece object that contains consent data to be saved in database</param>
    /// <returns>operation status</returns>
    public async Task SaveConsentAsync(ConsentDTO consentData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), consentData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ConsentModel.ConsentID), consentData.Consent.ConsentID, DbType.Int64, direction: ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ConsentModel.PageID), consentData.Consent.PageID, DbType.Int64, direction: ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ConsentModel.IsRequired), consentData.Consent.IsRequired, DbType.Boolean, direction: ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ConsentModel.SequenceNo), consentData.Consent.SequenceNo, DbType.Byte, direction: ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ConsentModel.ConsentFor), consentData.Consent.ConsentFor, DbType.String, direction: ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ConsentModel.IsActive), consentData.IsActive, DbType.Boolean, direction: ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ConsentModel.RoleID), consentData.Consent.RoleID, DbType.Byte, direction: ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), consentData.AccountID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.PermissionAtLevelID), consentData.PermissionAtLevelID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_CHECK_PERMISSION, consentData.IsActive ? AppPermissions.ConsentAddEdit.ToString() : AppPermissions.ConsentDelete.ToString(), DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), consentData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
        await connection.QueryAsync(SPNameConstants.USP_SAVE_CONSENT, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        consentData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }

    /// <summary>
    /// Get User Consent(s)
    /// </summary>
    /// <param name="consentData">User Consent data object</param>
    /// <returns>User Consent(s)</returns>
    public async Task GetUserConsentsAsync(ConsentDTO consentData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), consentData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ConsentModel.ConsentID), 0, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
        MapCommonSPParameters(consentData, parameter, AppPermissions.UserConsentsView.ToString(), AppPermissions.UserConsentsView.ToString());
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_CONSENTS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            if (consentData.RecordCount == -12)
            {
                consentData.UserConsents = (await result.ReadAsync<UserConsentModel>().ConfigureAwait(false)).ToList();
            }
            else
            {
                consentData.Consents = (await result.ReadAsync<ConsentModel>().ConfigureAwait(false)).ToList();
            }
            if (!result.IsConsumed)
            {
                await MapReturnPermissionsAsync(consentData, result).ConfigureAwait(false);
            }
        }
        consentData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Saves user consent in Database
    /// </summary>
    /// <param name="consentData">Referenece object that contains consent data to be saved in database</param>
    /// <returns>operation status</returns>
    public async Task SaveUserConsentAsync(ConsentDTO consentData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, ConvertDataToUserConsentTable(consentData).AsTableValuedParameter());
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), consentData.AccountID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), consentData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
        await connection.QueryAsync(SPNameConstants.USP_SAVE_USER_CONSENT, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        consentData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }

    private DataTable ConvertDataToUserConsentTable(ConsentDTO consent)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(ConsentModel.ConsentID), typeof(long)),
                new DataColumn(nameof(ConsentModel.IsAccepted),typeof(bool)),
                new DataColumn(nameof(ConsentModel.AcceptedOn), typeof(DateTimeOffset)),
            }
        };
        foreach (var item in consent.Consents)
        {
            dataTable.Rows.Add(item.ConsentID, item.IsAccepted, item.AcceptedOn);
        }
        return dataTable;
    }

    private async Task MapConsentDataAsync(ConsentDTO consents, SqlMapper.GridReader result)
    {
        consents.Consents = (await result.ReadAsync<ConsentModel>().ConfigureAwait(false)).ToList();
        if (consents.RecordCount != 0)
        {
            if (!result.IsConsumed)
            {
                consents.Pages = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                consents.Roles = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                await MapReturnPermissionsAsync(consents, result).ConfigureAwait(false);
            }
        }
    }
}