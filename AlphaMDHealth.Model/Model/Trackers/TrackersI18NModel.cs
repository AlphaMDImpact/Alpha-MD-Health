using AlphaMDHealth.Utility;
namespace AlphaMDHealth.Model
{
    public class TrackersI18NModel
    {
        /// <summary>
        /// Tracker ID
        /// </summary>
        public short TrackerID { get; set; }

        /// <summary>
        /// Tracker Name
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_TRACKER_NAME_TEXT_KEY)]
        public string TrackerName { get; set; }

       /// <summary>
       /// Tracker Added On
       /// </summary>
        public DateTimeOffset AddedOn { get; set; }

        /// <summary>
        /// Tracker added by id
        /// </summary>
        public string AddedByID { get; set; }

        /// <summary>
        /// Tracker Last Modified
        /// </summary>
        public DateTimeOffset LastModifiedON { get; set; }

        /// <summary>
        /// TRacker Last Modified By ID
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
