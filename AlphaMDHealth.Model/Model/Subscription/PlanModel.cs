using SQLite;

namespace AlphaMDHealth.Model;

public class PlanModel
{
    [PrimaryKey]
    public Int16 PlanID { get; set; }
    public string PlanCode { get; set; }
    public decimal TotalCharges { get; set; }
    public decimal Charges { get; set; }
    public int DurationInDays { get; set; }
    public decimal DiscountPercentage { get; set; }
    public byte NoOfScans { get; set; }
    public byte SequenceNo { get; set; }
    public bool IsPopularPlan { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset AddedON { get; set; }
    public long AddedByID { get; set; }
    public DateTimeOffset LastModifiedON { get; set; }
    public long LastModifiedByID { get; set; }
    [Ignore]
    public string PlanName { get; set; }
    [Ignore]
    public string PlanDetails { get; set; }
}
