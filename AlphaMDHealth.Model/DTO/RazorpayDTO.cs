namespace AlphaMDHealth.Model;

public class RazorpayDTO : BaseDTO
{
    public RazorpayPaymentModel RazorpayPayment { get; set; }

    public RazorpayOrderModel RazorpayOrder { get; set; }

    public string Page { get; set; }
}
