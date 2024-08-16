namespace AlphaMDHealth.Model;

public class PlanI18NModel
{
    public Int16 PlanID { get; set; }
    public byte LanguageID { get; set; }
    public string PlanName { get; set; }
    public string PlanDetails { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset AddedON { get; set; }
    public long AddedByID { get; set; }
    public DateTimeOffset LastModifiedON { get; set; }
    public long LastModifiedByID { get; set; }
}
