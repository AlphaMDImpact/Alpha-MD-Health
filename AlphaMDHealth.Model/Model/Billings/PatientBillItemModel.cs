using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class PatientBillItemModel
    {
        /// <summary>
        /// Billing Item ID
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_ADD_RECORD_KEY)]
        public short BillingItemID { get; set; }

        /// <summary>
        /// Patient Bill ID
        /// </summary>
        public Guid PatientBillID { get; set; }

        /// <summary>
        /// Flag represent it is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Amount of Billing Item
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_AMOUNT_KEY)]
        public double Amount { get; set; }

        /// <summary>
        /// IsSynced 
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// Name 
        /// </summary>
        public string Name { get; set; }

        [Ignore]
        /// <summary>
        /// Name 
        /// </summary>
        public string TempBillingID { get; set; }

        /// <summary>
        /// Adden on Date
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }
    }
}
