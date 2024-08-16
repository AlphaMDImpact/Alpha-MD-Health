using Newtonsoft.Json;

namespace AlphaMDHealth.Model;

public class RazorpayCommonPropertyModel
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    [JsonProperty("created_at")]
    public string CreatedAt { get; set; }
    public string Entity { get; set; }
    public string Status { get; set; }

    public DateTimeOffset CreatedDateTime { get; set; }
}
