using Newtonsoft.Json;

namespace AlphaMDHealth.Model;

public class RazorpayPaymentModel : RazorpayCommonPropertyModel
{
    [JsonProperty("order_id")]
    public string OrderID { get; set; }

    [JsonProperty("id")]
    public string PaymentID { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Contact { get; set; }

    [JsonProperty("international")]
    public bool IsInternational { get; set; }
    public double? AmountRefunded { get; set; }
    public string RefundStatus { get; set; }
    public string Method { get; set; }

    [JsonProperty("captured")]
    public bool IsCaptured { get; set; }

    [JsonProperty("card_id")]
    public string CardID { get; set; }
    public string Bank { get; set; }
    public string Wallet { get; set; }
    public string VPA { get; set; }

    [JsonProperty("token_id")]
    public string TokenID { get; set; }
    public double? Fee { get; set; }
    public double? Tax { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorDescription { get; set; }
    public string ErrorSource { get; set; }
    public string ErrorReason { get; set; }
    public string ErrorStep { get; set; }

    [JsonProperty("acquirer_data")]
    public AcquirerData AcquirerData { get; set; }

    [JsonProperty("invoice_id")]
    public string InvoiceID { get; set; }

    [JsonProperty("card")]
    public PaymentCardModel CardDetail { get; set; }
}

public class AcquirerData
{
    [JsonProperty("auth_code")]
    public string AuthCode { get; set; }

    [JsonProperty("bank_transaction_id")]
    public string BankTransactionID { get; set; }

    [JsonProperty("transaction_id")]
    public string TransactionID { get; set; }

}

