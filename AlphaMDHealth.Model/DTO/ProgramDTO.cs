using AlphaMDHealth.Utility;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class ProgramDTO : BaseDTO
    {
        /// <summary>
        /// Program Reason
        /// </summary>
        public ReasonModel Reason;

        /// <summary>
        /// List of program reason
        /// </summary>
        public List<ReasonModel> Reasons;

        /// <summary>
        /// List of  reason Option List
        /// </summary>
        public List<OptionModel> ReasonOptionList;

        /// <summary>
        /// SubFlow information
        /// </summary>
        public SubFlowModel SubFlow { get; set; }

        /// <summary>
        /// List of subflow Data
        /// </summary>
        [DataMember]
        public List<SubFlowModel> SubFlows { get; set; }

        /// <summary>
        /// Task Information
        /// </summary>
        public TaskModel Task { get; set; }

        /// <summary>
        /// List of Task Data
        /// </summary>
        [DataMember]
        public List<TaskModel> Tasks { get; set; }

        /// <summary>
        /// Program Data
        /// </summary>
        public ProgramModel Program { get; set; }

        /// <summary>
        /// List Of Program Data
        /// </summary>
        [DataMember]
        public List<ProgramModel> Programs { get; set; }

        /// <summary>
        /// Program Caregiver Data
        /// </summary>
        public CaregiverModel ProgramCareGiver { get; set; }

        /// <summary>
        /// List of Program Caregiver Data
        /// </summary>
        [DataMember]
        public List<CaregiverModel> ProgramCareGivers { get; set; }

        /// <summary>
        /// Program Education data
        /// </summary>
        public PatientEducationModel ProgramEducation { get; set; }

        /// <summary>
        /// List Of Program Education data
        /// </summary>
        [DataMember]
        public List<PatientEducationModel> ProgramEducations { get; set; }

        [DataMember]
        public List<PatientBillModel> ProgramBillItems { get; set; }

        public PatientBillModel ProgramBillItem { get; set; }

        /// <summary>
        /// ProgramReading Data
        /// </summary>
        public ReadingModel ProgramReading { get; set; }

        /// <summary>
        /// List of ProgramReading Data
        /// </summary>
        [DataMember]
        public List<ReadingModel> ProgramReadings { get; set; }

        /// <summary>
        /// List LanguageDetail
        /// </summary>
        [DataMember]
        public List<ProgramDetails> LanguageDetails { get; set; }

        /// <summary>
        /// List of OperationType
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_OPERATION_TYPE_KEY)]
        [DataMember]
        public List<OptionModel> OperationTypes { get; set; }

        /// <summary>
        /// List TaskType
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_TASK_TYPE_KEY)]
        [DataMember]
        public List<OptionModel> TaskTypes { get; set; }

        /// <summary>
        /// List of Item Data
        /// </summary>
        [DataMember]
        public List<OptionModel> Items { get; set; }

        /// <summary>
        /// List of ProgramSubFlow
        /// </summary>
        [DataMember]
        public List<OptionModel> ProgramSubFlows { get; set; }

        /// <summary>
        /// Display the count of Records
        /// </summary>
        public string BadgeCount { get; set; }

        /// <summary>
        /// Instruction Data
        /// </summary>
        public InstructionModel Instruction { get; set; }

        /// <summary>
        /// Language specific Data of Instruction
        /// </summary>
        public InstructionI18NModel InstructionI18N { get; set; }

        /// <summary>
        /// List of Instruction data
        /// </summary>
        [DataMember]
        public List<InstructionModel> Instructions { get; set; }

        /// <summary>
        /// List of Language specific  Instruction data
        /// </summary>
        [DataMember]
        public List<InstructionI18NModel> InstructionsI18N { get; set; }

        /// <summary>
        /// Program Medication Data
        /// </summary>
        public PatientMedicationModel ProgramMedication { get; set; }

        /// <summary>
        /// List of Program Medication Data
        /// </summary>
        [DataMember]
        public List<PatientMedicationModel> ProgramMedications { get; set; }

        /// <summary>
        /// List of UnitOption
        /// </summary>
        [DataMember]
        public List<OptionModel> UnitOptions { get; set; }

        /// <summary>
        /// List Of Frequency Option
        /// </summary>
        public List<OptionModel> FrequencyOptions { get; set; }

        /// <summary>
        /// List of MedicationDTO
        /// </summary>
        [DataMember]
        public List<PatientMedicationDTO> MedicationDTOs { get; set; }

        /// <summary>
        /// List Of Supported Codes
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_SUPPORTED_CODE_SYSTEM_KEY)]
        public List<OptionModel> SupportedCodes { get; set; }

        /// <summary>
        /// List Of Program Type
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_PROGRAM_DURATION_TYPE_KEY)]
        public List<OptionModel> ProgramTypes { get; set; }

        /// <summary>
        /// List Of ProgramTrackers
        /// </summary>
        public List<ProgramTrackerModel> ProgramTrackers { get; set; }

        /// <summary>
        /// ProgramTracker Information
        /// </summary>
        public ProgramTrackerModel ProgramTracker { get; set; }

        /// <summary>
        /// ProgramTrackers List
        /// </summary>
        public List<OptionModel> TrackerTypes { get; set; }

        public List<PatientBillModel> ProgramBillingItems { get; set; }
        /// <summary>
        /// Program Note
        /// </summary>
        public ProgramNoteModel ProgramNote { get; set; }

        /// <summary>
        /// List Of Program Note
        /// </summary>
        [DataMember]
        public List<ProgramNoteModel> ProgramNotes { get; set; }

        /// <summary>
        /// List Of Language specific Program Note
        /// </summary>
        [DataMember]
        public List<ProgramNoteI18NModel> ProgramNotesI18N { get; set; }

        /// <summary>
        /// List of PatientProgram Model
        /// </summary>
        public PatientProgramModel PatientProgram { get; set; }

        /// <summary>
        /// List Of Value added by type
        /// </summary>
        [DataMember]
        public List<OptionModel> ValueAddedByType { get; set; }

        public List<LanguageModel> Languages { get; set; }

        public List<OptionModel> BillingItemOptionList { get; set; }

        /// <summary>
        /// List of Questions and Answers
        /// </summary>
        [DataMember]
        public List<QuestionnaireQuestionModel> QuestionnaireQuestions { get; set; }

        /// <summary>
        /// List of Question Options
        /// </summary>
        [DataMember]
        public List<QuestionnaireQuestionOptionModel> QuestionnaireQuestionOptions { get; set; }

        /// <summary>
        /// List of questionnaire question details
        /// </summary>
        [DataMember]
        public List<QuestionnaireQuestionDetailsModel> QuestionnaireQuestiosDetails { get; set; }

        /// <summary>
        /// Patients Shared Programs
        /// </summary>
        [DataMember]
        public List<PatientsSharedProgramsModel> PatientsSharedPrograms { get; set; }

        /// <summary>
        /// List Of ProgramConfigurationModel
        /// </summary>
        [DataMember]
        public List<ProgramConfigurationModel> ProgramConfigurations { get; set; }

        public ProgramConfigurationModel ProgramConfiguration { get; set; }

        /// <summary>
        /// :ist Of Program Services
        /// </summary>
        [DataMember]
        public List<ProgramServiceModel> ProgramServices { get; set; }

        public ProgramServiceModel ProgramService { get; set;}

    }
}