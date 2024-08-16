using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;
public class PatientScanHistoryService : BaseService
{
    public PatientScanHistoryService(IEssentials serviceEssentials) : base(serviceEssentials)
    {

    }
    /// <summary>
    /// Sync organisationTags and page recourses from service 
    /// </summary>
    /// <param name="organisationTagData">organisationTag DTO to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>organisationTags received from server in organisationTag data and operation status</returns>
    public async Task SyncPatientScanHistoryFromServerAsync(PatientScanHistoryDTO patientScanHistoryData, CancellationToken cancellationToken)
    {
        try
        {
            patientScanHistoryData.SelectedUserID = GetUserID();
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_PATIENT_SCAN_HISTORY_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(patientScanHistoryData.RecordCount, CultureInfo.InvariantCulture) },
                    { nameof(PatientMedicationDTO.SelectedUserID), Convert.ToString(patientScanHistoryData.SelectedUserID, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            patientScanHistoryData.ErrCode = httpData.ErrCode;
            if (patientScanHistoryData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(patientScanHistoryData, data);
                    patientScanHistoryData.Response = null;
                    patientScanHistoryData = MapPatientScanHistory(data, patientScanHistoryData);
                    if(patientScanHistoryData.PatientScanHistoryData.Count() > 0)
                    {
                        foreach (var scanHistory in patientScanHistoryData.PatientScanHistoryData)
                        {
                            scanHistory.ScanPurchaseDateTime = _essentials.ConvertToLocalTime(scanHistory.ScanPurchaseDate).DateTime.ToString("dd MMM yyyy hh:mm tt");
                        }
                    }
                    patientScanHistoryData.ErrCode = (ErrorCode)(int)data[nameof(OrganisationTagDTO.ErrCode)];
                }
            }
        }
        catch (Exception ex)
        {
            patientScanHistoryData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    public PatientScanHistoryDTO MapPatientScanHistory(JToken dataItem, PatientScanHistoryDTO patientScanHistoryData)
    {
        if (dataItem[nameof(PatientScanHistoryDTO.PatientScanHistoryData)]?.Count() > 0)
        {
            patientScanHistoryData.PatientScanHistoryData = (from data in dataItem[nameof(PatientScanHistoryDTO.PatientScanHistoryData)]
                                                              select new PatientScanHistoryModel
                                                              {
                                                                  TotalScans = GetDataItem<long>(data, nameof(PatientScanHistoryModel.TotalScans)),
                                                                  UsedScans = GetDataItem<long>(data, nameof(PatientScanHistoryModel.UsedScans)),
                                                                  ScanPurchaseDate = (DateTime)data[nameof(PatientScanHistoryModel.ScanPurchaseDate)],
                                                              }).ToList(); 
        }
        return patientScanHistoryData;
    } 
}

