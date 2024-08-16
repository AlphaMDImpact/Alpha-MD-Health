namespace AlphaMDHealth.Model;

public class TempSessionModel
{
    public string TempToken { get; set; }
    public string TokenIdentifier { get; set; } 

    public DateTimeOffset? TempTokenExpiryDateTime { get; set; }

    public long AccountID { get; set; }
}