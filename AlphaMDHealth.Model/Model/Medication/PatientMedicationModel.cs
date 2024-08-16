using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class PatientMedicationModel : MedicineModel
	{
		[PrimaryKey]
		public Guid PatientMedicationID { get; set; }
		public long ProgramMedicationID { get; set; }
		public long ProgramID { get; set; }
        public bool IsReadOnly { get; set; }
        [MyCustomAttributes(ResourceConstants.R_DOSES_KEY)]
		public decimal Doses { get; set; }

        //[MyCustomAttributes(ResourceConstants.R_FREQUENCY_KEY)]
        public string Frequency { get; set; }

		[MyCustomAttributes(ResourceConstants.R_HOW_OFTEN_KEY)]
		public int HowOften { get; set; }
		public string HowOftenString { get; set; }

		[MyCustomAttributes(ResourceConstants.R_ALTERNATE_FOR_TEXT_KEY)]
		public byte AfterDays { get; set; }

        [MyCustomAttributes(ResourceConstants.R_SHOW_FOR_DAYS_KEY)]
        public short AssignForDays { get; set; }

        [MyCustomAttributes(ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY)]
        public short AssignAfterDays { get; set; }

        //[MyCustomAttributes(ResourceConstants.R_MEDICATION_START_DATE_KEY)]
        public DateTimeOffset? StartDate { get; set; }

        //[MyCustomAttributes(ResourceConstants.R_MEDICATION_END_DATE_KEY)]
        public DateTimeOffset? EndDate { get; set; }

        [MyCustomAttributes(ResourceConstants.R_ADDITIONAL_NOTE_TEXT_KEY)]
        public string Note { get; set; }
		public bool Reminder { get; set; }
		public long PatientID { get; set; }
		public bool IsSynced { get; set; }
		public string AddedByID { get; set; }
		public DateTimeOffset AddedOn { get; set; }
		public long LastModifiedByID { get; set; }


        /// <summary>
        /// Left default icon
        /// </summary>
		[Ignore]
        public string LeftDefaultIcon { get; set; }
        public string LeftSourceIcon { get; set; }

        [Ignore]
        public string ShortUnitName { get; set; }

        [Ignore]
		public string StartDateString { get; set; }

		[Ignore]
		public string FromDateString { get; set; }

		[Ignore]
		public string AuthorisedDateString { get; set; }

		[Ignore]
		public string EndDateString { get; set; }
		public ErrorCode ErrCode { get; set; } = ErrorCode.OK;
		[Ignore]
		public string FormattedDate { get; set; }
        [Ignore]
        public string EndDateShortFormat{ get; set; }

        [Ignore]
		public string MedicationImage { get; set; }
		[Ignore]
		public string MedicationReminderImage { get; set; }
		[Ignore]
		public string MedicationDosesString { get; set; }
		[Ignore]
		public string MedicationStatusString { get; set; }
		[Ignore]
		public string MedicationStatusColorString { get; set; }

		[Ignore]
		public string IsCriticalStatusString { get; set; }
		[Ignore]
		public string IsCriticalStatusColorString { get; set; }

		[Ignore]
		public string AddedByName { get; set; }
		public string ProgramColor { get; set; }
		public string ProgramName { get; set; }

        //[MyCustomAttributes(ResourceConstants.R_MEDICATION_NOTE_KEY)]
        public string AdditionalNotes { get; set; }
		public string Notes { get; set; }

        //[MyCustomAttributes(ResourceConstants.R_IS_CRITICAL_KEY)]
        public bool IsCritical { get; set; }
		/// <summary>
		/// Name Of Organisation Name
		/// </summary>
		public string OrganisationName { get; set; }

		/// <summary>
		/// Stores OrganisationID
		/// </summary>
		public long OrganisationID { get; set; }
		/// <summary>
		/// Name Of Organisation Address
		/// </summary>
		public string OrganisationAddress { get; set; }
		/// <summary>
		/// Name Of Organisation Detail
		/// </summary>
		public string OrganisationDetail { get; set; }
		/// <summary>
		/// Name Of Organisation Contact
		/// </summary>
		public string OrganisationContact { get; set; }

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int Age { get; set; }
		public string Gender { get; set; }
		public string PatientName { get; set; }
		public DateTimeOffset? Dob { get; set; }
		public string CaregiverName { get; set; }

		public string DoctorNameDisplayString { get; set; }

		[Ignore]
		public List<string> FrequencyOptionsString { get; set; }
		[Ignore]
		public int steps { get; set; }
		[Ignore]
		public List<OptionModel> FrequencyOptions { get; set; }

		[Ignore]
		public List<OptionModel> FrequencyTypeOptions { get; set; }

		[Ignore]
		public List<OptionModel> AdditionalNotesOptions { get; set; }

		[Ignore]
		public List<string> AdditionalNotesOptionString { get; set; }

		[Ignore]
		public List<OptionSelectModel> AdditionalNotesOptionSelect { get; set; }
	}
}