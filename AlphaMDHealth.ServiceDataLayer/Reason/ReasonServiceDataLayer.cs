using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer;

public class ReasonServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get Reasons
    /// </summary>
    /// <param name="reasonData">Reference object to return list of Reason</param>
    /// <returns>Reasons Data With Operation Status</returns>
    public async Task GetReasonsAsync(ReasonDTO reasonData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), reasonData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(ReasonModel.ReasonID)), reasonData.Reason.ReasonID, DbType.Byte, ParameterDirection.Input);
        MapCommonSPParameters(reasonData, parameters, AppPermissions.ReasonsView.ToString(), $"{AppPermissions.ReasonDelete},{AppPermissions.ReasonAddEdit}"
        );
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_REASONS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            reasonData.Reasons = (await result.ReadAsync<ReasonModel>().ConfigureAwait(false))?.ToList();
            await MapReturnPermissionsAsync(reasonData, result).ConfigureAwait(false);
        }
        reasonData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Gets Program Reason related Data
    /// </summary>
    /// <param name="programData">Object to get Program Reason Data</param>
    /// <returns>returns Program reason data with Operation Data</returns>
    public async Task GetProgramReasonsAsync(ProgramDTO programData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(ReasonModel.ProgramReasonID)), programData.Reason.ProgramReasonID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), default(DateTimeOffset), DbType.DateTimeOffset, ParameterDirection.Input);
        MapCommonSPParameters(programData, parameters, AppPermissions.ProgramReasonsView.ToString(), $"{AppPermissions.ProgramReasonDelete},{AppPermissions.ProgramReasonAddEdit}");
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_REASONS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            programData.ReasonOptionList = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
            if (!result.IsConsumed && programData.Reason.ProgramReasonID > 0)
            {
                programData.Reason = await result.ReadFirstAsync<ReasonModel>().ConfigureAwait(false);
            }
            await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
        }
        programData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
    }

    /// <summary>
    /// Saves Reason in Database
    /// </summary>
    /// <param name="reasonData">Object that contains Reason to be saved in database</param>
    /// <returns>operation status</returns>
    public async Task SaveReasonAsync(ReasonDTO reasonData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), reasonData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(ReasonModel.ReasonID)), reasonData.Reason.ReasonID, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapReasonsToTable(reasonData).AsTableValuedParameter());
        parameter.Add(ConcateAt(nameof(BaseDTO.IsActive)), reasonData.IsActive, DbType.Boolean, ParameterDirection.Input);
        MapCommonSPParameters(reasonData, parameter, reasonData.IsActive ? AppPermissions.ReasonAddEdit.ToString() : AppPermissions.ReasonDelete.ToString());
        await connection.ExecuteAsync(SPNameConstants.USP_SAVE_REASON, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        reasonData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }

    /// <summary>
    /// Save Program Reason 
    /// </summary>
    /// <param name="programData">Object to be saved </param>
    /// <returns>operation status</returns>
    public async Task SaveProgramReasonAsync(ProgramDTO programData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(ReasonModel.ProgramReasonID)), programData.Reason.ProgramReasonID, DbType.Int64, ParameterDirection.InputOutput);
        parameter.Add(ConcateAt(nameof(ReasonModel.ProgramID)), programData.Reason.ProgramID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(ReasonModel.ReasonID)), programData.Reason.ReasonID, DbType.Int16, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(ReasonModel.IsActive)), programData.Reason.IsActive, DbType.Boolean, ParameterDirection.Input);
        MapCommonSPParameters(programData, parameter, programData.Reason.IsActive ? AppPermissions.ProgramReasonAddEdit.ToString() : AppPermissions.ProgramReasonDelete.ToString());
        await connection.QueryAsync(SPNameConstants.USP_SAVE_PROGRAM_REASON, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        if (programData.ErrCode == ErrorCode.OK)
        {
            programData.Reason.ProgramReasonID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_PROGRAM_REASON_ID);
        }
    }

    private DataTable MapReasonsToTable(ReasonDTO reasonData)
    {
        DataTable dataTable = CreateGenericTypeTable();
        if (GenericMethods.IsListNotEmpty(reasonData.Reasons))
        {
            foreach (ReasonModel record in reasonData.Reasons)
            {
                dataTable.Rows.Add(0, Guid.Empty, record.LanguageID, record.Reason, record.ReasonDescription, string.Empty);
            }
        }
        return dataTable;
    }
}