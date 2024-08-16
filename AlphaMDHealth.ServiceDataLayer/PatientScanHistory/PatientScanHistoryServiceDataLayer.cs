using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer;
public class PatientScanHistoryServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get OrganisationTags from server
    /// </summary>
    /// <param name="organisationTagData">Object to hold OrganisationTag data</param>
    /// <returns>List of OrganisationTag in organisationTagData and Operation Status</returns>
    public async Task GetPatientScanHistoryAsync(PatientScanHistoryDTO patientScanHistoryData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), patientScanHistoryData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientScanHistoryDTO.SelectedUserID)), patientScanHistoryData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        MapCommonSPParameters(patientScanHistoryData, parameters, $"{AppPermissions.PatientScanHistoryView}", $"{AppPermissions.PatientScanHistoryView}"
        );
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PATIENT_SCAN_HISTORY, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            patientScanHistoryData.PatientScanHistoryData = (await result.ReadAsync<PatientScanHistoryModel>().ConfigureAwait(false)).ToList();
            await MapReturnPermissionsAsync(patientScanHistoryData, result).ConfigureAwait(false);
        }
        patientScanHistoryData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }
}



