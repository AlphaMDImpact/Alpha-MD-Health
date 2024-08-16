using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class PatientProgramModel
    {
        [PrimaryKey]
        public long PatientProgramID { get; set; }

        [MyCustomAttributes(ResourceConstants.R_SELECT_PROGRAM_KEY)]
        public long ProgramID { get; set; }
        public long PatientID { get; set; }
        public string ProgramGroupIdentifier { get; set; }
        public bool IsActive { get; set; }
        public bool IsSynced { get; set; }
        public DateTimeOffset AddedON { get; set; }

        /// <summary>
        /// Stores last modified on date by user
        /// </summary>
        public DateTimeOffset? LastModifiedON { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// ErrCode Enum
        /// </summary>
        [Ignore]
        public ErrorCode ErrCode { get; set; } = ErrorCode.OK;

        /// <summary>
        /// Date Value 
        /// </summary>
        //[MyCustomAttributes(ResourceConstants.R_PATIENT_PROGRAM_DATE_KEY)]
        public DateTimeOffset? EntryDate { get; set; }

        /// <summary>
        /// Tracker ID
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_PATIENT_PROGRAM_TRACKER_KEY)]
        public short? TrackerID { get; set; }

        /// <summary>
        /// Day count
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_PATIENT_PROGRAM_DAYS_KEY)]
        public string ProgramEntryPoint { get; set;}

        /// <summary>
        /// ID of entry type
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_END_POINT_TYPE_KEY)]
        public int? EntryTypeID { get; set; }

        /// <summary>
        /// Entry Point
        /// </summary>
        public string EntryPoint { get; set; }
        public Guid PatientTrackerID { get; set; }
    }
}