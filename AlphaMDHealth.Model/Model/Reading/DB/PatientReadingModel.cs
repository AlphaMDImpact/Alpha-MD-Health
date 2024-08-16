using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model used to store patient reading data
    /// </summary>
    public class PatientReadingModel
    {
        /// <summary>
        /// Uniqe id of patient reading value
        /// </summary>
        [PrimaryKey]
        public Guid PatientReadingID { get; set; }

        /// <summary>
        /// ID of the patient for which this reading value is
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// Unique id of the reading
        /// </summary>
        public short ReadingID { get; set; }

        /// <summary>
        /// Task id of the patient
        /// </summary>
        public long PatientTaskID { get; set; }

        /// <summary>
        /// Datetime when reading value has been taken
        /// </summary>
        public DateTimeOffset? ReadingDateTime { get; set; }

        /// <summary>
        /// Value of reading
        /// </summary>
        public double? ReadingValue { get; set; }

        /// <summary>
        /// Value 2 of reading
        /// </summary>
        public string? ReadingValue2 { get; set; }

        /// <summary>
        /// Message entered while reading value has taken
        /// </summary>
        public string ReadingNotes { get; set; }

        /// <summary>
        /// Type of source or device via reading value is fetched
        /// </summary>
        public string ReadingSourceType { get; set; }

        /// <summary>
        /// ID of the reading value source
        /// </summary>
        public Guid? ReadingSourceID { get; set; }

        /// <summary>
        /// Quantity of intake
        /// </summary>
        public double? SourceQuantity { get; set; }

        /// <summary>
        /// Name of the reading value source
        /// </summary>
        public string SourceName { get; set; }

        /// <summary>
        /// Id of the user, who has entered the value
        /// </summary>
        public string AddedByID { get; set; }

        /// <summary>
        /// Date time when value has been taken
        /// </summary>
        public DateTimeOffset? AddedON { get; set; }

        /// <summary>
        /// Date time when value has been taken
        /// </summary>
        public DateTimeOffset? LastModifiedON { get; set; }

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

        ///// <summary>
        ///// reading local date time
        ///// </summary>
        //[Ignore]
        //public DateTime ReadingDate
        //{
        //    get
        //    {
        //        return ReadingDateTime.LocalDateTime;
        //    }
        //}

        /// <summary>
        /// Reading Frequency used to decide date time 
        /// </summary>
        public short ReadingFrequency { get; set; }


        /// <summary>
        /// Reading UnitIdentifier
        /// </summary>
        [Ignore]
        public string UnitIdentifier { get; set; }
    }
}