using SQLite;
using AlphaMDHealth.Utility;
namespace AlphaMDHealth.Model
{
    public class PatientProviderNoteModel
    {
        /// <summary>
        /// ProviderNoteID
        /// </summary>
        [PrimaryKey]
        public Guid ProviderNoteID { get; set; }

        /// <summary>
        /// CaregiverID
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_SELECT_PROVIDER_KEY1)]
        public long CareGiverID { get; set; }

        /// <summary>
        /// PatientID
        /// </summary>
        public long PatientID { get; set; }

        /// <summary>
        /// Program Note ID
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_PROVIDER_NOTES_KEY)]
        public long ProgramNoteID { get; set; }

        /// <summary>
        /// Note Date Time
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_DATE_KEY)]
        public DateTimeOffset? NoteDateTime { get; set; }

        /// <summary>
        /// Note Date Time String
        /// </summary>
        public string NoteDateTimeString { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// IsSynced
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// QuestionnaireID
        /// </summary>
        public long QuestionnaireID { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// ProgramName
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// ProgramID
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_SELECT_PROGRAM_KEY)]
        public long ProgramID { get; set; }

        /// <summary>
        /// Program group identifier
        /// </summary>
        public string ProgramGroupIdentifier { get; set; }

        /// <summary>
        /// Note AddedOn DateTime 
        /// </summary>
        public DateTimeOffset AddedOn { get;set; }

    }
}