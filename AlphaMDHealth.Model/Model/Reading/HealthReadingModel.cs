using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Health reading data
    /// </summary>
    public class HealthReadingModel
    {
        /// <summary>
        /// Activity type
        /// </summary>
        public string ActivityType { get; set; }

        /// <summary>
        /// Duration
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Reading type
        /// </summary>
        public ReadingType ReadingType { get; set; }

        /// <summary>
        /// Reading value
        /// </summary>
        public double ReadingValue { get; set; }

        /// <summary>
        /// Reading unit
        /// </summary>
        public ReadingUnit ReadingUnit { get; set; }

        /// <summary>
        /// Reading moment
        /// </summary>
        public ReadingMoment? ReadingMoment { get; set; }

        /// <summary>
        /// Sequence number
        /// </summary>
        public long SequenceNumber { get; set; }

        /// <summary>
        /// Created On
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        public ErrorCode ErrCode { get; set; }
    }
}
