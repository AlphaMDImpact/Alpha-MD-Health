using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model used to decide service calls
    /// </summary>
    public class SyncConfigurationModel : BaseDTO
    {
        /// <summary>
        /// id of sync setting
        /// </summary>
        [PrimaryKey]
        public long SettingID { get; set; }

        /// <summary>
        /// Sync group name
        /// </summary>
        public ServiceSyncGroups GroupName { get; set; }

        /// <summary>
        /// Page name from where sync will happen
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Table name for which sync will happen
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// comma seperated Table names for which sync will happen in this call
        /// </summary>
        public string TablesInBatch { get; set; }

        /// <summary>
        /// flag to decide sync will awaitable and when
        /// </summary>
        public SyncTimes SyncTimes { get; set; }

        /// <summary>
        /// Sync types to decide it will awaitable in which flow
        /// </summary>
        public SyncTypes SyncTypes { get; set; }

        /// <summary>
        /// Sequence of service calls
        /// </summary>
        public int Sequence { get; set; }
    }
}