using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer;
public class PatientScanHistoryServiceBuisnessLayer : BaseServiceBusinessLayer
{
    /// <summary>
    /// OrganisationTag Service
    /// </summary>
    /// <param name="httpContext">Instance of HttpContext</param>
    public PatientScanHistoryServiceBuisnessLayer(HttpContext httpContext) : base(httpContext)
    {

    }

    /// <summary>
    /// Get Reasons
    /// </summary>
    /// <param name="languageID">User's language ID</param>
    /// <param name="permissionAtLevelID">level at which permission is required</param>
    /// <param name="recordCount">Record Count</param>
    /// <param name="selectedUserID">Record Count</param>
    /// <returns>Returns list of OrganisationTag(s)</returns>
    public async Task<PatientScanHistoryDTO> GetPatientScanHistoryAsync(byte languageID, long permissionAtLevelID, long recordCount,long selectedUserID)
    {
        PatientScanHistoryDTO patientScanHistoryData = new PatientScanHistoryDTO();
        try
        {
            if (permissionAtLevelID < 1 || languageID < 1)
            {
                patientScanHistoryData.ErrCode = ErrorCode.InvalidData;
                return patientScanHistoryData;
            }
            patientScanHistoryData.AccountID = AccountID;
            if (patientScanHistoryData.AccountID < 1)
            {
                patientScanHistoryData.ErrCode = ErrorCode.Unauthorized;
                return patientScanHistoryData;
            }
            if (await GetConfigurationDataAsync(patientScanHistoryData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_CARE_FLIX_SERVICE_GROUP}").ConfigureAwait(false))
            {
                patientScanHistoryData.AccountID = AccountID;
                patientScanHistoryData.SelectedUserID = selectedUserID;
                patientScanHistoryData.PermissionAtLevelID = permissionAtLevelID;
                patientScanHistoryData.LanguageID = languageID;
                patientScanHistoryData.RecordCount = recordCount;
                patientScanHistoryData.FeatureFor = FeatureFor;
                await new PatientScanHistoryServiceDataLayer().GetPatientScanHistoryAsync(patientScanHistoryData).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            patientScanHistoryData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        return patientScanHistoryData;
    }

    private async Task<bool> GetConfigurationDataAsync(BaseDTO baseDTO, byte languageID, string groupNames)
    {
        baseDTO.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, groupNames, languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
        if (baseDTO.Resources != null)
        {
            return true;
        }
        baseDTO.ErrCode = ErrorCode.InvalidData;
        return false;
    }
}


