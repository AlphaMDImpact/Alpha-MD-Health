using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model which holds Data sync details
    /// </summary>
    public class DataSyncModel
    {
        /// <summary>
        /// Table name for which sync details will be saved
        /// </summary>
        public string SyncFor { get; set; }

        /// <summary>
        /// PatientID for which data needs to fetch
        /// </summary>
        public long PatientID { get; set; }

        /// <summary>
        /// Data sync from datetime
        /// </summary>
        public DateTimeOffset? SyncFromServerDateTime { get; set; }

        /// <summary>
        /// Data sync to date time
        /// </summary>
        public DateTimeOffset SyncToServerDateTime { get; set; }

        /// <summary>
        /// Date time on which sync to server started
        /// </summary>
        public DateTimeOffset SyncToStartedDateTime { get; set; }

        /// <summary>
        /// Sync to server call status
        /// </summary>
        public SyncStatus SyncToStatus { get; set; }

        /// <summary>
        /// ErrCode Enum
        /// </summary>
        [Ignore]
        public ErrorCode ErrCode { get; set; } = ErrorCode.OK;

        /// <summary>
        /// Stores RecordCount 
        /// </summary>
        [Ignore]
        public long RecordCount { get; set; }
    }
}