using Newtonsoft.Json;

namespace AlphaMDHealth.Model;

public class PaymentCardModel
{
    [JsonProperty("id")]
    public string CardId { get; set; }
    public string Entity { get; set; }
    public string Name { get; set; }

    [JsonProperty("last4")]
    public string Last4Digit { get; set; }
    public string Network { get; set; }
    public string Type { get; set; }
    public string Issuer { get; set; }

    [JsonProperty("international")]
    public bool IsInternational { get; set; }

    [JsonProperty("emi")]
    public bool IsEMI { get; set; }
    public string SubType { get; set; }

    [JsonProperty("tokeniin")]
    public string TokenIin { get; set; }
}
