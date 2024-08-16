using SQLite;

namespace AlphaMDHealth.Model;

public class UserRelationModel
{
    [PrimaryKey]
    public long PatientCareGiverID { get; set; }
    public long UserID { get; set; }
    public long CareGiverID { get; set; }
    public long PatientID { get; set; }
    public long ProgramID { get; set; } 
    public DateTimeOffset FromDate { get; set; }
    public DateTimeOffset ToDate { get; set; }
    public short RelationID { get; set; }
    public DateTimeOffset? LastModifiedON { get; set; }
    public bool IsActive { get; set; } = true;
    public string FromDateString { get; set; }
    public string CareGiverName { get; set; }
    public bool ShowRemoveButton { get; set; } = true;
    public bool IsSynced { get; set; }

    public byte[] ImageBytes { get; set; } 

    [Ignore]
    public string RoleName { get; set; }
    [Ignore]
    public string ShowRemoveButtonText { get; set; }
    public string ProgramColor { get; set; }
}