using Newtonsoft.Json;

namespace AlphaMDHealth.Model;

public class RazorpayOrderModel : RazorpayCommonPropertyModel
{
    [JsonProperty("id")]
    public string OrderID { get; set; }
    public double AmountPaid { get; set; }
    public double AmountDue { get; set; }
    public int Attempts { get; set; }
    public string OfferID { get; set; }
}

