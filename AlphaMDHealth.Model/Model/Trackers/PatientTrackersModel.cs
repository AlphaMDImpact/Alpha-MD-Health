using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class PatientTrackersModel : BaseListItemModel
    {
        /// <summary>
        /// Holds PatientTrackerID
        /// </summary>
        [PrimaryKey]
        [MyCustomAttributes(ResourceConstants.R_SELECT_TRACKER_NAME_TEXT_KEY)]
        public Guid PatientTrackerID { get; set; }

        /// <summary>
        /// Holds UserID
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// Holds Tracker Type ID
        /// </summary>
        public short TrackerTypeID { get; set; }

        /// <summary>
        /// ID Of Program
        /// </summary>
        public long ProgramID { get; set; }

        /// <summary>
        /// Patient ID
        /// </summary>
        public long PatientID { get; set; }

        /// <summary>
        /// TrackerID
        /// </summary>
        public short TrackerID { get; set; }

        /// <summary>
        /// Holds Program TrackerID
        /// </summary>
        public long ProgramTrackerID { get; set; }

        /// <summary>
        /// From date value
        /// </summary>

        [MyCustomAttributes(ResourceConstants.R_START_DATE_KEY)]
        public DateTimeOffset? FromDate { get; set; }

        /// <summary>
        /// To Date value
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_END_DATE_KEY)]
        public DateTimeOffset? ToDate { get; set; }

        /// <summary>
        /// Store date in string format
        /// </summary>

        [Ignore]
        public string FromDateDisplayFormatString { get; set; }

        /// <summary>
        ///  Store date in string format
        /// </summary>
        public string ToDateDisplayFormatString { get; set; }

        /// <summary>
        /// Holds Current Value
        /// </summary>

        public string CurrentValue { get; set; }

        /// <summary>
        /// Holds Current Value in datetime
        /// </summary>
        public DateTimeOffset? CurrentValueInDate { get; set; }

        /// <summary>
        /// string format for FromDate Value
        /// </summary>
        public string FromDateValue { get; set; }

        /// <summary>
        /// string format for currentvalue
        /// </summary>
        public string CurrentValueDisplayFormatString { get; set; }

        /// <summary>
        /// Image Name
        /// </summary>
        public string ImageName { get; set; }

        /// <summary>
        /// Name of tracker
        /// </summary>
       // [MyCustomAttributes(ResourceConstants.R_SELECT_TRACKER_NAME_TEXT_KEY)]
        public string TrackerName { get; set; }

        /// <summary>
        /// Caption Text
        /// </summary>
        public string CaptionText { get; set; }

        /// <summary>
        /// Description of Tracker
        /// </summary>
        public string InstructionsText { get; set; }

        /// <summary>
        /// Flag representing whether value is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Flag representing whether value is synced or not
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// ProgramImage
        /// </summary>
        [Ignore]
        public string ProgramImage { get; set; }

        /// <summary>
        /// Date time when value has been taken
        /// </summary>
        public DateTimeOffset AddedON { get; set; }

        /// <summary>
        /// Date time when value has been taken
        /// </summary>
        public DateTimeOffset LastModifiedON { get; set; }

        /// <summary>
        /// Tracker modified by id
        /// </summary>
        public long LastModifiedByID { get; set; }

        /// <summary>
        /// Tracker added by id
        /// </summary>
        public string AddedByID { get; set; }

        /// <summary>
        /// errorcode
        /// </summary>
        [Ignore]
        public ErrorCode ErrCode { get; set; } = ErrorCode.OK;

        //todo:
        /// <summary>
        /// Image source to render category image in mobile
        /// </summary>
        //[Ignore]
        //public ImageSource ImageSource { get; set; }
        public byte[] ImageBytes { get; set; }

        /// <summary>
        /// Tracker Range From Value
        /// </summary>
        public int FromValue { get; set; }

        /// <summary>
        /// Tracker Range To value
        /// </summary>
        public int ToValue { get; set; }

        /// <summary>
        /// Color of Program
        /// </summary>
        public string ProgramColor { get; set; }

        /// <summary>
        /// Value Added By
        /// </summary>
        public short ValueAddedBy { get; set; }

        /// <summary>
        /// Value to be edited
        /// </summary>
        public bool IsEdit { get; set; }

        /// <summary>
        /// Week
        /// </summary>
        public string Week { get; set; }

        /// <summary>
        /// No Of Weeks
        /// </summary>
        public DateTimeOffset? NoOfWeeks { get; set; }

        /// <summary>
        /// Due Date of a tracker
        /// </summary>
        public DateTimeOffset? DueDate { get; set; }

        /// <summary>
        /// DueDate string value
        /// </summary>
        public string DueDateString { get; set; }

        /// <summary>
        /// Program duration
        /// </summary>
        public int ProgramDuration { get; set; }
    }
}
