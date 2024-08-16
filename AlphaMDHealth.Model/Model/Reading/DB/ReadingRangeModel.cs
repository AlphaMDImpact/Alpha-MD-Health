using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model used to store reading range data
    /// </summary>
    public class ReadingRangeModel
    {
        /// <summary>
        /// ID of the program for which this range is defined
        /// </summary>
        public long ProgramID { get; set; }

        /// <summary>
        /// Unique id of the reading
        /// </summary>
        public short ReadingID { get; set; }

        /// <summary>
        /// Reading id of the program
        /// </summary>
        public long ProgramReadingID { get; set; }

        /// <summary>
        /// ID for the range of reading
        /// </summary>
        public long ReadingRangeID { get; set; }

        /// <summary>
        /// Gender for which this reading range is
        /// </summary>
        public short ForGender { get; set; }

        /// <summary>
        /// Age for which this reading range is
        /// </summary>
        public short ForAgeGroup { get; set; }

        /// <summary>
        /// From age value
        /// </summary>
        public short? FromAge { get; set; }

        /// <summary>
        /// To age value
        /// </summary>
        public short? ToAge { get; set; }

        /// <summary>
        /// Minimun Absolute Value
        /// </summary>
        public double AbsoluteMinValue { get; set; }

        /// <summary>
        /// Maximum Absolute Value
        /// </summary>
        public double AbsoluteMaxValue { get; set; }

        /// <summary>
        /// Colour of Absolute Band to show in graph
        /// </summary>
        public string AbsoluteBandColor { get; set; }

        /// <summary>
        /// Minimun Normal Value
        /// </summary>
        public double NormalMinValue { get; set; }

        /// <summary>
        /// Maximum Normal Value
        /// </summary>
        public double NormalMaxValue { get; set; }

        /// <summary>
        /// Colour of Normal Band to show in graph
        /// </summary>
        public string NormalBandColor { get; set; }

        /// <summary>
        /// Colour of Target Band to show in graph
        /// </summary>
        public string TargetBandColor { get; set; }

        /// <summary>
        /// Flag representing record is active or deleted
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Name of reading (language specific text) to display in UI
        /// </summary>
        [Ignore]
        public string Reading { get; set; }

        /// <summary>
        /// Gender language specific text to display in UI, for which this reading range is
        /// </summary>
        [Ignore]
        public string GenderString { get; set; }

        /// <summary>
        /// Age language specific text to display in UI, for which this reading range is
        /// </summary>
        [Ignore]
        public string AgeGroupString { get; set; }

        /// <summary>
        /// Patient ID for which this reading range is
        /// </summary>
        [Ignore]
        public long UserID { get; set; }
    }
}
