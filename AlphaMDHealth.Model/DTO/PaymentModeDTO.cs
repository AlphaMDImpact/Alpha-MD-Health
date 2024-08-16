using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Data transfer object for PaymentModes
    /// </summary>
    public class PaymentModeDTO : BaseDTO
    {
        /// <summary>
        /// Payment Mode information
        /// </summary>
        public PaymentModeModel PaymentMode { get; set; }

        /// <summary>
        ///  List of Payment Mode
        /// </summary>

        [DataMember]
        public List<PaymentModeModel> PaymentModes { get; set; }

        /// <summary>
        /// BillPaymentMode
        /// </summary>
        [DataMember]
        public List<BillPaymentModel> BillPaymentModes { get; set; }

        /// <summary>
        /// PaymentI18Ns
        /// </summary>
        [DataMember]
        public List<PaymentModeI18NModel> PaymentModeI18N { get; set; }
    }
}
