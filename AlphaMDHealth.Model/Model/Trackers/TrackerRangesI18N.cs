namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Tracker Range Detail Model
    /// </summary>
    public class TrackerRangesI18N
    {
        /// <summary>
        /// Tracker Range ID
        /// </summary>
        public long TrackerRangeID { get; set; }
        /// <summary>
        /// Tracker Caption Text
        /// </summary>
        public string CaptionText { get; set; }
        /// <summary>
        /// Tracker Instruction Text
        /// </summary>
        public string InstructionsText { get; set; }
        /// <summary>
        /// Tracker Range Added On
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }
        /// <summary>
        /// Tracker added by id
        /// </summary>
        public string AddedByID { get; set; }
        /// <summary>
        /// Tracker Range Last Modified
        /// </summary>
        public DateTimeOffset LastModifiedON { get; set; }
        /// <summary>
        /// TRacker Range Last Modified By ID
        /// </summary>
        public long LastModifiedByID { get; set; }
        /// <summary>
        /// Flag which indicates language is active or not
        /// </summary>
        public bool IsActive { get; set; } = true;
        /// <summary>
        /// Id used to identify uniquely
        /// </summary>
        public byte LanguageID { get; set; }

        /// <summary>
        /// Code of language
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Name of Language
        /// </summary>
        public string LanguageName { get; set; }

    }
}
