using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class CaregiverModel
    {
        [PrimaryKey]
        [MyCustomAttributes(ResourceConstants.R_ITEM_KEY)]
        public long ProgramCareGiverID { get; set; }

        [MyCustomAttributes(ResourceConstants.R_CAREGIVER_KEY)]
        public long PatientCareGiverID { get; set; }
        public long ProgramID { get; set; }

        [MyCustomAttributes(ResourceConstants.R_ORGANISATION_KEY)]
        public long OrganisationID { get; set; }
        public string Department { get; set; }
        public long CareGiverID { get; set; }
        public string ProgramColor { get; set; }
        public string ProgramName { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        [MyCustomAttributes(ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY)]
        public short AssignAfterDays { get; set; }
        [MyCustomAttributes(ResourceConstants.R_SHOW_FOR_DAYS_KEY)]
        public short AssignForDays { get; set; }

        [MyCustomAttributes(ResourceConstants.R_START_DATE_KEY)]
        public string FromDateValue { get; set; }

        [MyCustomAttributes(ResourceConstants.R_END_DATE_KEY)]
        public string ToDateValue { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public string RoleName { get; set; }
        public string ImageName { get; set; }
        public string Initials { get; set; }
        [Ignore]
        public string DateStyle { get; set; }
        /// <summary>
        /// Caregiver added on date time
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }

        /// <summary>
        /// caregiver modified on date time
        /// </summary>
        public DateTimeOffset LastModifiedON { get; set; }
        public long PatientProgramID { get; set; }
        public string Degrees { get; set; }
        public long AccountID { get; set; }
    }
}
