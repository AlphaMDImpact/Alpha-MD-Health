using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer;

public class PatientProgramServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get Programs assigned to patient
    /// </summary>
    /// <param name="patientProgramData">Object in which daata will be passed</param>
    /// <returns>Patient Program Data</returns>
    public async Task GetPatientProgramsAsync(ProgramDTO patientProgramData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), patientProgramData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProgramModel.PatientProgramID)), patientProgramData.CreatedByID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.LanguageID)), patientProgramData.LanguageID, DbType.Byte, direction: ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), patientProgramData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.RecordCount)), patientProgramData.RecordCount, DbType.Int16, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.ErrCode)), patientProgramData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
        MapCommonSPParameters(patientProgramData, parameter, AppPermissions.PatientProgramsView.ToString()
            , $"{AppPermissions.PatientProgramAddEdit}{(patientProgramData.RecordCount == -1 ? $",{AppPermissions.PatientProgramDelete}" : string.Empty)}");
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PATIENT_PROGRAMS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            if (patientProgramData.RecordCount == -1)
            {
                patientProgramData.TrackerTypes = (await result.ReadAsync<OptionModel>().ConfigureAwait(false)).ToList();
                patientProgramData.Items = (await result.ReadAsync<OptionModel>().ConfigureAwait(false)).ToList();
                if (patientProgramData.CreatedByID > 0)
                {
                    patientProgramData.PatientProgram = (await result.ReadAsync<PatientProgramModel>().ConfigureAwait(false)).FirstOrDefault();
                }
            }
            else
            {
                patientProgramData.Programs = (await result.ReadAsync<ProgramModel>().ConfigureAwait(false)).ToList();
            }
            await MapReturnPermissionsAsync(patientProgramData, result).ConfigureAwait(false);
        }
        patientProgramData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
    }

    /// <summary>
    /// Save patient Programs Data
    /// </summary>
    /// <param name="programData">Reference object which holds patient Program Data</param>
    /// <returns>Operation Status Code</returns>
    public async Task SavePatientProgramsAsync(PatientProgramDTO programData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProgramModel.PatientProgramID)), programData.PatientProgram.PatientProgramID, DbType.Int64, ParameterDirection.InputOutput);
        parameter.Add(ConcateAt(nameof(PatientProgramModel.PatientID)), programData.PatientProgram.PatientID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProgramModel.ProgramGroupIdentifier)), programData.PatientProgram.ProgramGroupIdentifier, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProgramModel.ProgramID)), programData.PatientProgram.ProgramID, DbType.Int64, direction: ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProgramModel.ProgramEntryPoint)), programData.PatientProgram.ProgramEntryPoint, DbType.Int32, direction: ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProgramModel.EntryDate)), programData.PatientProgram.EntryDate, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProgramModel.TrackerID)), programData.PatientProgram.TrackerID, DbType.Int16, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProgramModel.ProgramEntryPoint)), programData.PatientProgram.ProgramEntryPoint, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProgramModel.EntryTypeID)), programData.PatientProgram.EntryTypeID, DbType.Int32, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(PatientProgramModel.IsActive)), programData.PatientProgram.IsActive, DbType.Boolean, direction: ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.AddedON)), programData.PatientProgram.AddedON, DbType.DateTimeOffset, ParameterDirection.Input);
        MapCommonSPParameters(programData, parameter, AppPermissions.PatientProgramAddEdit.ToString());
        await connection.QueryAsync(SPNameConstants.USP_SAVE_PATIENT_PROGRAMS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        programData.PatientProgram.PatientProgramID = parameter.Get<long>(ConcateAt(nameof(PatientProgramModel.PatientProgramID)));
        programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
    }
}