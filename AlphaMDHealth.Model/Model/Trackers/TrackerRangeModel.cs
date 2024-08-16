using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Tracker Range Model
    /// </summary>
    public class TrackerRangeModel
    {
        [PrimaryKey]
        /// <summary>
        /// Tracker Range ID
        /// </summary>
        public short TrackerRangeID { get; set; }

        /// <summary>
        /// Tracker ID
        /// </summary>
        public short TrackerID { get; set; }

        /// <summary>
        /// Tracker Range From Value
        /// </summary>
        public int FromValue { get; set; }

        /// <summary>
        /// Tracker Range To value
        /// </summary>
        public int ToValue { get; set; }

        /// <summary>
        /// Tracker Caption Text
        /// </summary>
        public string CaptionText { get; set; }

        /// <summary>
        /// Name Of the Image
        /// </summary>
        public string ImageName { get; set; }

        /// <summary>
        /// Flag to store IsActive
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Tracker Added On
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }

        /// <summary>
        /// Tracker added by id
        /// </summary>
        public string AddedByID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset LastModifiedON { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long LastModifiedByID { get; set; }
    }
}
