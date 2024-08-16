using SQLite;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class ProgramTrackerModel
    {
        /// <summary>
        ///  ID Of ProgramTracker
        /// </summary>
        [PrimaryKey]
        public long ProgramTrackerID { get; set; }

        /// <summary>
        /// Tracker ID
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_SELECT_TRACKER_TYPE_KEY)]
        public short TrackerID { get; set; }

        /// <summary>
        /// Name Of Tracker
        /// </summary>
        public string TrackerName { get; set; }

        /// <summary>
        /// ID Of Program
        /// </summary>
        public long ProgramID { get; set; }

        /// <summary>
        /// Represents Data added after days
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY)]
        public int AssignAfterDays { get; set; }

        /// <summary>
        /// Represnts Data shown for days
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_SHOW_FOR_DAYS_KEY)]
        public int AssignForDays { get; set; }

        /// <summary>
        /// Value Added By 
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_TRACKER_VALUE_CAN_BE_ADDED_BY_KEY)]
        public short ValueAddedBy { get; set; }

        /// <summary>
        /// Flag representing data is synced to server or not
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// Tracker added by id
        /// </summary>
        public string AddedByID { get; set; }

        /// <summary>
        /// Tracker added on date time
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }

        /// <summary>
        /// Flag representing file is active or deleted
        /// </summary>
        public bool IsActive { get; set; }
        public Guid PatientTrackerID { get; set; }
        public long UserID { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public string CurrentValue { get; set; }
        public string FileName { get; set; }
        public string CaptionText { get; set; }
        public string AddedOnString { get; set; }
        public string ProgramImage { get; set; }
    }
}
