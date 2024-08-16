using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class ProgramModel
    {
        [PrimaryKey]
        public long ProgramID { get; set; }
        public string Name { get; set; }

        [MyCustomAttributes(ResourceConstants.R_DEFAULT_PROGRAM_INDENTIFIER_COLOR_KEY)]
        public string ProgramGroupIdentifier { get; set; }

        [MyCustomAttributes(ResourceConstants.R_SUPPORTED_CODE_SYSTEM_KEY)]
        public long CodeSystemID { get; set; }
        public bool IsPublished { get; set; }
        public bool AllowSelfRegistration { get; set; }
        public long PatientID { get; set; }
        public bool IsActive { get; set; }
        public bool IsSynced { get; set; }
        public DateTimeOffset AddedOn { get; set; }
        public string ProgramEntryPoint { get; set; }
        [Ignore]
        public string AddedOnString { get; set; }
        [Ignore]
        public string ProgramImage { get; set; }
        [Ignore]
        public int NoOfSubflows { get; set; }
        [Ignore]
        public int NoOfTasks { get; set; }
        [Ignore]
        public int NoOfDefaultCareGivers { get; set; }
        [Ignore]
        public int NoOfReadings { get; set; }
        [Ignore]
        public int NoOfEducations { get; set; }
        [Ignore]
        public int NoOfMedications { get; set; }
        [Ignore]
        public bool IsColorChanged { get; set; }
        public long PatientProgramID { get; set; }
        public short ProgramTypeID { get; set; }
      
        public int ProgramDuration { get; set; }
        public bool AllowProviderToScan { get; set; }
        public bool AllowPatientToScan { get; set; }
        public bool AllowPatientToBuyCredits { get; set; }
        public bool AllowProgramToBuyCredits { get; set; }
    }
}