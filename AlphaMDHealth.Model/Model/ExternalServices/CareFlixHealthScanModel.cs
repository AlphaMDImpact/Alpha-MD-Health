namespace AlphaMDHealth.Model
{
    public class HealthScanModel
    {
        public long TransactionID { get; set; }
        public string PaymentID { get; set; }
        public bool IsPatient { get; set; }
        public double? MinimumQuantityToBuy { get; set; }
        public double? Quantity { get; set; }
        public double? UnitPrice { get; set; }
        public double? DiscountPercentage { get; set; }
        public double? DiscountPrice { get; set; }
        public double? TotalPrice { get; set; }
        public DateTimeOffset? TransactionDateTime { get; set; }
        public string TrasactionDateTimeValue { get; set; }
        public string Discount { get; set; }
    }
}
