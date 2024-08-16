using SQLite;

namespace AlphaMDHealth.Model
{
    public class BillItemModel
    {
        /// <summary>
        /// Billing Item ID
        /// </summary>
        [PrimaryKey]
        public short BillingItemID { get; set; }

        /// <summary>
        /// Flag represent it is active or not
        /// </summary>
        public bool IsActive { get; set; }
    }
}