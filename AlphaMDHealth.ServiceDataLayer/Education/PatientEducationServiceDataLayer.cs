using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer;

public class PatientEducationServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Save Patient Education to Database
    /// </summary>
    /// <param name="educationData">Assigned Education to be Saved</param>
    /// <returns>Operation Status</returns>
    public async Task SavePatientEducationAsync(ContentPageDTO educationData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), educationData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientEducationModel.PatientEducationID)), educationData.PatientEducation.PatientEducationID, DbType.Int64, ParameterDirection.InputOutput);
        parameter.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), educationData.PatientEducation.UserID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientEducationModel.PageID)), educationData.PatientEducation.PageID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientEducationModel.Status)), educationData.PatientEducation.Status.ToString(), DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), educationData.PatientEducation.IsActive, DbType.Boolean, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientEducationModel.FromDate)), educationData.PatientEducation.FromDate, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientEducationModel.ToDate)), educationData.PatientEducation.ToDate, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.AccountID)), educationData.AccountID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.AddedON)), educationData.PatientEducation.AddedOn, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), educationData.PatientEducation.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.PermissionAtLevelID)), educationData.PermissionAtLevelID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_CHECK_PERMISSION),educationData.IsActive ? AppPermissions.PatientEducationAddEdit.ToString() : AppPermissions.PatientEducationDelete, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.ErrCode)), educationData.ErrCode, DbType.Int16, ParameterDirection.Output);
        MapCommonSPParameters(educationData, parameter, AppPermissions.PatientEducationAddEdit.ToString());
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_PATIENT_EDUCATION, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        educationData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        if (educationData.ErrCode == ErrorCode.OK)
        {
            educationData.PatientEducation.PatientEducationID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(PatientEducationModel.PatientEducationID));
        }
    }
}