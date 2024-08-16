using SQLite;

namespace AlphaMDHealth.Model;

public class UserPlanModel
{
    [PrimaryKey]
    public long UserPlanID { get; set; }
    public short PlanID { get; set; }
    public long AccountID { get; set; }
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
    public bool IsActive { get; set; }
    public string PaymentID { get; set; }
    public DateTimeOffset AddedON { get; set; }
    public long AddedByID { get; set; }
    public DateTimeOffset LastModifiedON { get; set; }
    public long LastModifiedByID { get; set; }
}
