using SQLite;
using AlphaMDHealth.Utility;
namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Tracker Model
    /// </summary>
    public class TrackersModel 
    {
        [PrimaryKey]
        /// <summary>
        /// Tracker ID
        /// </summary>
        public short TrackerID { get; set; }

        /// <summary>
        /// Tracker Name
        /// </summary>
        public string TrackerName { get; set; }

        /// <summary>
        /// Tracker Identifier
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_TRACKER_IDENTIFIER_KEY)]
        public string TrackerIdentifier { get; set; }

        /// <summary>
        /// Tracker Type
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_TRACKER_TYPE_KEY)]
        public string TrackerType { get; set; }

        /// <summary>
        /// Id For Tracker Type
        /// </summary>
        public short TrackerTypeID { get; set; }

        [Ignore]
        /// <summary>
        /// Range
        /// </summary>
        public string Ranges { get; set; }

        /// <summary>
        /// Flag to store IsActive
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Tracker added on date time
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }

        /// <summary>
        /// Tracker added by id
        /// </summary>
        public string AddedByID { get; set; }

        /// <summary>
        /// Last Modifiedcon
        /// </summary>
        public DateTimeOffset LastModifiedON { get; set; }

        /// <summary>
        /// Last Modified By ID
        /// </summary>
        public long LastModifiedByID { get; set; }
    }
}
