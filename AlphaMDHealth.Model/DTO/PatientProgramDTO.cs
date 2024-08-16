using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Data transfer object for Programs
    /// </summary>
    public class PatientProgramDTO : BaseDTO
    {

       /// <summary>
       /// Patient Program Information
       /// </summary>
        public PatientProgramModel PatientProgram { get; set; }

        /// <summary>
        /// List Of Programs
        /// </summary>
        [DataMember]
        public List<ProgramModel> Programs { get; set; }

        /// <summary>
        /// Program Information
        /// </summary>
        public ProgramModel Program { get; set; }

        /// <summary>
        /// List Of Patient Programs
        /// </summary>
        [DataMember]
        public List<PatientProgramModel> PatientPrograms { get; set; }

        /// <summary>
        /// List Of Organization Programs
        /// </summary>
        [DataMember]
        public List<OptionModel> OrganizationPrograms { get; set; }

        /// <summary>
        /// Bool Function to check If patient can select program
        /// </summary>
        public bool IsPatientAllowedForProgramSelection { get; set; }

        /// <summary>
        /// End Point Types
        /// </summary>
        public List<OptionModel> EndPointTypes { get; set; }

        /// <summary>
        /// Tracker Option List
        /// </summary>
        public List<OptionModel> TrackerTypes { get; set; }
    }
}
