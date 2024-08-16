using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model used to store patient reading targets
    /// </summary>
    public class ReadingTargetModel
    {
        /// <summary>
        /// Patient ID for which this reading target is
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// Unique id of the reading
        /// </summary>
        public short ReadingID { get; set; }

        /// <summary>
        /// Patient targets minimum value for selected reading
        /// </summary>
        public float TargetMinValue { get; set; }

        /// <summary>
        /// Patient targets maximum value for selected reading
        /// </summary>
        public float TargetMaxValue { get; set; }

        /// <summary>
        /// Reading unit
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Minimun Absolute Value
        /// </summary>
        public double AbsoluteMinValue { get; set; }

        /// <summary>
        /// Maximum Absolute Value
        /// </summary>
        public double AbsoluteMaxValue { get; set; }

        /// <summary>
        /// Flag representing record is active or deleted
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Flag representing record is synced with server or not
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// Error code received from server while syncing reading data to server
        /// </summary>
        public ErrorCode ErrCode { get; set; }
    }
}
