namespace AlphaMDHealth.Model
{
    public class PaymentModeI18NModel
    {
        /// <summary>
        /// ID of payment
        /// </summary>
        public byte PaymentModeID { get; set; }

        /// <summary>
        /// Flag represent it is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// ID of the language
        /// </summary>
        public byte LanguageID { get; set; }

        /// <summary>
        /// payment type
        /// </summary>
        public string Name { get; set; }
    }
}
