namespace AlphaMDHealth.Model
{
    public class ProgramBillingModel
    {
        /// <summary>
        /// Program Billing ItemID
        /// </summary>
        public long ProgramBillingItemID { get; set; }

        /// <summary>
        ///  ProgramID
        /// </summary>
        public long ProgramID { get; set; }

        /// <summary>
        /// OrganisationID
        /// </summary>
        public long OrganisationID { get; set; }

        /// <summary>
        /// BillingItemID
        /// </summary>
        public short BillingItemID { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>
        public bool IsActive { get; set; }
    }
}
