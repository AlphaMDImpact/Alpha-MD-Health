namespace AlphaMDHealth.Model
{
    public class BillPaymentModel
    {
        /// <summary>
        /// ID of payment
        /// </summary>
        public byte PaymentModeID { get; set; }

        /// <summary>
        /// Flag represent it is active or not
        /// </summary>
        public bool IsActive { get; set; }
    }
}
