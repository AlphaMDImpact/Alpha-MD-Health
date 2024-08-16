namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Country Model
    /// </summary>
    public class CountryModel
    {
        /// <summary>
        /// Stores User's Country Name
        /// </summary>
        public string CountryName { get; set; }
        /// <summary>
        /// Stores User's Country Code
        /// </summary>
        public string CountryCode { get; set; }
        /// <summary>
        /// Stores User's Country Code
        /// </summary>
        public string CountryCulture { get; set; }
        /// <summary>
        /// Stores language ID 
        /// </summary>
        public byte LanguageID { get; set; }

        /// <summary>
        /// Stores Mobile Number Min Length
        /// </summary>
        public byte MobileNumberLength { get; set; }

        /// <summary>
        /// Stores Mobile Number Max Length
        /// </summary>
        public byte MobileNumberLengthMax { get; set; }

        /// <summary>
        /// Stores Last Modified On Date By User
        /// </summary>
        public DateTimeOffset? LastModifiedOn { get; set; }
    }
}
