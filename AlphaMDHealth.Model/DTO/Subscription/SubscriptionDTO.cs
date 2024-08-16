namespace AlphaMDHealth.Model;

public class SubscriptionDTO : BaseDTO
{
    public UserPlanModel UserPlan { get; set; }
    public PlanModel Plan { get; set; } 
    public List<PlanModel> Plans { get; set; }
    public RazorpayPaymentModel RazorpayPayment { get; set;}
    public RazorpayOrderModel RazorpayOrder { get; set;}
}