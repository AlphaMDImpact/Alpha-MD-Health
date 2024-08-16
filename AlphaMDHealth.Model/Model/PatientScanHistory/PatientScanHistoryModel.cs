namespace AlphaMDHealth.Model;

public class PatientScanHistoryModel 
{
    /// <summary>
    /// Total Scans
    /// </summary>
    public long TotalScans { get; set; }
    /// <summary>
    /// Used Scans
    /// </summary>
    public long UsedScans { get; set; }

    /// <summary>
    /// Scan Purchase Date
    /// </summary>
    public DateTimeOffset ScanPurchaseDate { get; set; }
    public string ScanPurchaseDateTime { get; set; }

}
