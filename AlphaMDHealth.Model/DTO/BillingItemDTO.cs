using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Data transfer object for BillingItems
    /// </summary>
    public class BillingItemDTO : BaseDTO
    {
        /// <summary>
        /// Billing Item information
        /// </summary>
        public BillingItemModel BillingItem { get; set; }

        /// <summary>
        /// List of Billing Items
        /// </summary>
        [DataMember]
        public List<BillingItemModel> BillingItems { get; set; }

        /// <summary>
        /// List of Billing Items is used for save sync data for Mobile
        /// </summary>
        [DataMember]
        public List<BillItemModel> BillItems { get; set; }

        /// <summary>
        /// List of Billing Items is used for save sync data for Mobile
        /// </summary>
        [DataMember]
        public List<BillingItemsI18NModel> BillingItemsI18N { get; set; }

        /// <summary>
        /// PatientBillItem is used for patientbill add edit
        /// </summary>
        public PatientBillModel PatientBillItem { get; set; }

        /// <summary>
        /// List of Patient Bill items
        /// </summary>
        [DataMember]
        public List<PatientBillItemModel> PatientBillItems { get; set; }

        /// <summary>
        /// List of Patient Bill items
        /// </summary>

        [DataMember]
        public List<PatientBillModel> PatientBills { get; set; }

        /// <summary>
        /// List of Patient Programs as OptionList
        /// </summary>

        [DataMember]
        public List<OptionModel> PatientProgramOptionList { get; set; }

        /// <summary>
        ///  List of Providers as OptionList
        /// </summary>

        [DataMember]
        public List<OptionModel> PatientProvidersOptionList { get; set; }

        /// <summary>
        /// List of  Payment Modes as OptionList
        /// </summary>

        [DataMember]
        public List<OptionModel> PaymentModeOptionList { get; set; }

        /// <summary>
        /// List of  Billing Item as OptionList
        /// </summary>

        [DataMember]
        public List<OptionModel> PatientBillingItems { get; set; }

        /// <summary>
        /// ProgramBillingItems
        /// </summary>
        [DataMember]
        public List<ProgramBillingModel> ProgramBillingItems { get; set; }

        /// <summary>
        /// Is BillDetail Views
        /// </summary>
        public bool IsBillDetailViews { get; set; }

        /// <summary>
        /// Bulk PatientBills information
        /// </summary>
        [DataMember]
        public List<SaveResultModel> SavePatientBills { get; set; }

    }
}
