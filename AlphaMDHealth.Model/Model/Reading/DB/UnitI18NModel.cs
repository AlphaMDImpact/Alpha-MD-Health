namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model used to store language specific unit data
    /// </summary>
    public class UnitI18NModel
    {
        /// <summary>
        /// Unique ID of unit
        /// </summary>
        public int UnitID { get; set; }

        /// <summary>
        /// ID of the language
        /// </summary>
        public byte LanguageID { get; set; }

        /// <summary>
        /// Language specific fullname of the unit
        /// </summary>
        public string FullUnitName { get; set; }

        /// <summary>
        /// Language specific short-name of the unit
        /// </summary>
        public string ShortUnitName { get; set; }

        /// <summary>
        /// Flag represent it is active or not
        /// </summary>
        public bool IsActive { get; set; }
    }
}