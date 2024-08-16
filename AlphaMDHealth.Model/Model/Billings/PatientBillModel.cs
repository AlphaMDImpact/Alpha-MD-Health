using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class PatientBillModel: BaseListItemModel
    {
        /// <summary>
        /// Patient Bill ID
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_SELECT_BILLING_PROGRAM_KEY)]
        public Guid PatientBillID { get; set; }

        /// <summary>
        /// Billing Item Id of Program
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_ITEM_KEY)]
        public long ProgramBillingItemID { get; set; }

        /// <summary>
        /// Billing Item Id
        /// </summary>
        public short BillingItemID { get; set; }

        /// <summary>
        /// Amount of Billing Item
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_AMOUNT_KEY)]
        public double Amount { get; set; }

        /// <summary>
        /// Id of Program
        /// </summary>
        public long ProgramID { get; set; }

        /// <summary>
        /// Flag represent Active or not 
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Billing Item Name
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// Adden on Date
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }

        /// <summary>
        /// Added By Id
        /// </summary>
        public long AddedById { get; set; }

        /// <summary>
        /// Last modified on
        /// </summary>
        public DateTimeOffset? LastModifiedON { get; set; }

        /// <summary>
        /// Last modified by id
        /// </summary>
        public long LastModifiedByID { get; set; }

        /// <summary>
        /// ErrCode Enum
        /// </summary>
        public ErrorCode ErrCode { get; set; } = ErrorCode.OK;

        /// <summary>
        /// ID of patient
        /// </summary>
        public long PatientID { get; set; }

        /// <summary>
        /// Provider ID
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_DOCTOR_NAME_KEY)]
        public long ProviderID { get; set; }

        /// <summary>
        /// Gross Total
        /// </summary>
        public double GrossTotal { get; set; }

        /// <summary>
        /// Discount
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_BILL_DISCOUNT_KEY)]
        public double Discount { get; set; }

        /// <summary>
        /// Total Paid
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_AMOUNT_PAID_KEY)]
        public double TotalPaid { get; set; }

        /// <summary>
        /// Payment Mode ID
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_SELECT_PAYMENT_MODE_KEY)]
        public short PaymentModeID { get; set; }

        /// <summary>
        /// Color of Program Color
        /// </summary>
        public string ProgramColor { get; set; }

        /// <summary>
        /// Name Of Program Name
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// Name Of Patient Name
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// Name Of Provider Name
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Name Of Organisation Name
        /// </summary>
        public string OrganisationName { get; set; }

        /// <summary>
        /// Stores OrganisationID
        /// </summary>
        public long OrganisationID { get; set; }
        /// <summary>
        /// Name Of Organisation Address
        /// </summary>
        public string OrganisationAddress { get; set; }
        /// <summary>
        /// Name Of Organisation Contact
        /// </summary>
        public string OrganisationContact { get; set; }
        /// <summary>
		/// Name Of Organisation Detail
		/// </summary>
		public string OrganisationDetail { get; set; }

        /// <summary>
        /// Name Of Payment Mode
        /// </summary>
        public string PaymentMode { get; set; }

        /// <summary>
        /// Name Of BillDateTime
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_ENTER_DATE_KEY)]
        public DateTimeOffset? BillDateTime { get; set; }

        /// <summary>
        /// String format of Bill Date Time
        /// </summary>
        public string BillDateTimeString { get; set; }

        /// <summary>
        /// Name Of Current Status
        /// </summary>
        public string CurrentStatus { get; set; }

        /// <summary>
        /// IsSynced 
        /// </summary>
        public bool IsSynced { get; set; }

        /// this property is used to map amount mode and ammount paid in mobile 
        /// </summary>
        [Ignore]
        public string ProgramDiscription { get; set; }

        /// <summary>
        /// AddedOnDate
        /// </summary>
        [Ignore]
        public string AddedOnDate { get; set; }

        /// <summary>
        /// Status 
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Description in mobile and tablet
        /// </summary>
        public string DescriptionInMobile { get; set; }
    }
}