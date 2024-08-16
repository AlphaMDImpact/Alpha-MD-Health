using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class PatientEducationModel : BaseListItemModel
    {
        /// <summary>
        /// ID of PatientEducation
        /// </summary>
        [PrimaryKey]
        public long PatientEducationID { get; set; }

        /// <summary>
        /// ID of ProgramEducation
        /// </summary>
        public long ProgramEducationID { get; set; }

        /// <summary>
        /// ID of Program
        /// </summary>
        public long ProgramID { get; set; }

        /// <summary>
        /// ID of User
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// ID of Page
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_SELECT_EDUCATION_LABEL_KEY)]
        public long PageID { get; set; }

        /// <summary>
        /// ID of EducationType
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_CATEGORY_KEY)]
        public short EducationTypeID { get; set; }

        /// <summary>
        /// ID of Status
        /// </summary>
        public short StatusID { get; set; }

        /// <summary>
        /// Heading of Page
        /// </summary>
        public string PageHeading { get; set; }

        /// <summary>
        /// Type of Page
        /// </summary>
        public string PageType { get; set; }

        /// <summary>
        /// Status of PatientEducation
        /// </summary>
        public PatientEducationStatus Status { get; set; }

        /// <summary>
        /// Base64 Address of the image
        /// </summary>
        public string ImageBase64 { get; set; }

        /// <summary>
        /// Name Of the Image
        /// </summary>
        public string ImageName { get; set; }

        /// <summary>
        /// PatientEducation added on date time
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }

        /// <summary>
        /// PatientEducation modified on date time
        /// </summary>
        public DateTimeOffset LastModifiedON { get; set; }

        /// <summary>
        /// Name of the Category
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// No of days after which education must be assigned
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY)]
        public short AssignAfterDays { get; set; }

        /// <summary>
        /// No of days for which education must be assigned
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_SHOW_FOR_DAYS_KEY)]
        public short AssignForDays { get; set; }

        /// <summary>
        /// Flag representing record is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Date from which the education should be assigned 
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_START_DATE_KEY)]
        public DateTimeOffset? FromDate { get; set; }

        /// <summary>
        /// Date until which the education should be assigned 
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_END_DATE_KEY)]
        public DateTimeOffset? ToDate { get; set; }

        /// <summary>
        /// Represents Weather education belongs to provider 
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_FOR_PROVIDERS_KEY)]
        public bool ForProviders { get; set; }

        /// <summary>
        /// Flag represents is data synced from server 
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// String Format of Date from which Education should be assigned
        /// </summary>
        [Ignore]
        public string FromDateString { get; set; }

        /// <summary>
        /// String Format of Date from which Education should be assigned
        /// </summary>
        [Ignore]
        public string ToDateString { get; set; }

        /// <summary>
        /// Styling For Date 
        /// </summary>
        public string DateStyle { get; set; }

        /// <summary>
        /// ID of PatientEducation
        /// </summary>
        [Ignore]
        public string PatientEducationIDs { get; set; }

        /// <summary>
        /// Color of the Program
        /// </summary>
        public string ProgramColor { get; set; }

        /// <summary>
        /// Name of the program under which education is added
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// Value of Status
        /// </summary>
        public string StatusValue { get; set; }
        public string StatusColor { get; set; }

        /// <summary>
        /// Category wise educations
        /// </summary>
        public IEnumerable<PatientEducationModel> CategoryEducations { get; set; }

    }
}
