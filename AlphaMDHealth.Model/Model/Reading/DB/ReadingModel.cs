using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model used to store reading metadata
    /// </summary>
    public class ReadingModel
    {
        /// <summary>
        /// Program Reading Id
        /// </summary>
        
        public long ProgramReadingID { get; set; }

        /// <summary>
        /// Sequence No
        /// </summary>
        public byte SequenceNo { get; set; }

        /// <summary>
        /// PatientProgramID
        /// </summary>
        public long PatientProgramID { get; set; }

        /// <summary>
        /// Digits After Decimal Point
        /// </summary>
        public byte DigitsAfterDecimalPoint { get; set; }

        /// <summary>
        /// Reading Frequency
        /// </summary>
        public short ReadingFrequency { get; set; }

        /// <summary>
        /// Value Added By
        /// </summary>
        public short ValueAddedBy { get; set; }

        /// <summary>
        /// Allow Manual Add
        /// </summary>
        public bool AllowManualAdd { get; set; }

        /// <summary>
        /// Allow Health Kit Data
        /// </summary>
        public bool AllowHealthKitData { get; set; }

        /// <summary>
        /// Allow Device Data
        /// </summary>
        public bool AllowDeviceData { get; set; }

        /// <summary>
        /// Show In Graph
        /// </summary>
        public bool ShowInGraph { get; set; }

        /// <summary>
        /// Show In Data
        /// </summary>
        public bool ShowInData { get; set; }

        /// <summary>
        /// Allow Delete
        /// </summary>
        public bool AllowDelete { get; set; }

        /// <summary>
        /// Show In Different Lines
        /// </summary>
        public bool ShowInDifferentLines { get; set; }

        /// <summary>
        /// Summary Record Count
        /// </summary>
        public short SummaryRecordCount { get; set; }

        /// <summary>
        /// Chart Type
        /// </summary>
        public short ChartType { get; set; }

        /// <summary>
        /// Reading Filters
        /// </summary>
        public string ReadingFilters { get; set; }

        /// <summary>
        /// Days Of Past ecords ToSync
        /// </summary>
        public short DaysOfPastRecordsToSync { get; set; }

        /// <summary>
        /// Program ID for which this reading data is
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_ITEM_KEY)]
        public long ProgramID { get; set; }

        /// <summary>
        /// Patient ID for which this reading is
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// Unique id of the reading
        /// </summary>
        public short ReadingID { get; set; }

        /// <summary>
        /// Parent id of the reading for reading hierarchy 
        /// </summary>
        public short ReadingParentID { get; set; }

        /// <summary>
        /// Category id of the reading
        /// </summary>
        public short ReadingCategoryID { get; set; }

        /// <summary>
        /// Flag representing reading will be grouped or not
        /// </summary>
        public bool IsGroupValue { get; set; }

        /// <summary>
        /// Formula which will be used for reading calculation
        /// </summary>
        public string ReadingFormula { get; set; }

        /// <summary>
        /// ID of the unit group used to represent this reading
        /// </summary>
        public short UnitGoupID { get; set; }

        /// <summary>
        /// ID of control which will show value for this Reading 
        /// </summary>
        public short ReadingValueTypeID { get; set; }

        /// <summary>
        /// Flag representing record is active or deleted
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Flag representing record is critical or not
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Name of reading (language specific text) to display in UI
        /// </summary>
        public string Reading { get; set; }

        /// <summary>
        /// Name of reading parent (language specific text) to display in UI
        /// </summary>
        public string ReadingParent { get; set; }

        /// <summary>
        /// Name of reading Category (language specific text) to display in UI
        /// </summary>
        [Ignore]
        [MyCustomAttributes(ResourceConstants.R_CATEGORY_KEY)]
        public string ReadingCategory { get; set; }

        /// <summary>
        /// Code for this reading
        /// </summary>
        [Ignore]
        public string ReadingCode { get; set; }
    }
}