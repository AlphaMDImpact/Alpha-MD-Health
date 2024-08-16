namespace AlphaMDHealth.Model;

/// <summary>
/// Data transfer object for Reasons
/// </summary>
public class PatientScanHistoryDTO : BaseDTO
{
    public List<PatientScanHistoryModel> PatientScanHistoryData { get; set; }
}
