using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class BillingItemModel : LanguageModel
    {
        public short BillingItemID { get; set; }

        [MyCustomAttributes(ResourceConstants.R_BILLING_ITEM_NAME_KEY)]
        public string Name { get; set; }
    }
}