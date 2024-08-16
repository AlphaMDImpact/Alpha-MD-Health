using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class PaymentModeModel : LanguageModel
    {
        public byte PaymentModeID { get; set; }

        [MyCustomAttributes(ResourceConstants.R_PAYMENY_MODE_KEY)]
        public string Name { get; set; }
    }
}
