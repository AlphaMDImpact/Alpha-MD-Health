namespace AlphaMDHealth.Model
{
    public class BillingItemsI18NModel
    {
        /// <summary>
        /// ID of billing Item
        /// </summary>
        public short BillingItemID { get; set; }

        /// <summary>
        /// Flag represent it is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// ID of the language
        /// </summary>
        public byte LanguageID { get; set; }

        /// <summary>
        /// Name 
        /// </summary>
        public string Name { get; set; }
    }
}
