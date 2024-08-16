using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class PatientTrackersValuesModel
    {
        public Guid PatientTrackerID { get; set; }
        public string CurrentValue { get; set; }
        public bool IsActive { get; set; }
        public bool IsSynced { get; set; }
        [Ignore]
        public ErrorCode ErrCode { get; set; } = ErrorCode.OK;
        /// <summary>
        /// Stores Created By User ID 
        /// </summary>
        public long AddedByID { get; set; }

        /// <summary>
        /// Stores Added On Date By User
        /// </summary>
        public DateTimeOffset? AddedON { get; set; }

        /// <summary>
        /// Stores Created By User ID 
        /// </summary>
        public long LastModifiedByID { get; set; }

        /// <summary>
        /// Stores Last Modified On Date By User
        /// </summary>
        public DateTimeOffset? LastModifiedON { get; set; }

    }
}
