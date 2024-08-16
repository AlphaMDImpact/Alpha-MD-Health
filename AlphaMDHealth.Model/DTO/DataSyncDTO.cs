using AlphaMDHealth.Model;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class DataSyncDTO : BaseDataSyncDTO
    {
        [DataMember]
        public List<LanguageModel> Languages { get; set; } //A1
        [DataMember]
        public List<OrganizationFeaturePermissionModel> Features { get; set; } //A2
        [DataMember]
        public List<FeatureRelationModel> FeatureRelations { get; set; } //A2
        [DataMember]
        public List<MenuModel> Menus { get; set; } //A3
        [DataMember]
        public List<MobileMenuNodeModel> MenuNodes { get; set; } //A3
        [DataMember]
        public List<MenuGroupModel> MenuGroups { get; set; }
        [DataMember]
        public List<UserModel> Users { get; set; } //A3
        [DataMember]
        public List<UserRelationModel> UserRelations { get; set; } //A3
        [DataMember]
        public List<ConfigureDashboardModel> DashboardConfigurations { get; set; } //A3
        [DataMember]
        public List<SystemFeatureParameterModel> DashboardConfigurationParameters { get; set; } //A3
        [DataMember]
        public List<ChatModel> Chats { get; set; } //A4
        [DataMember]
        public List<ChatDetailModel> ChatDetails { get; set; } //A4
        [DataMember]
        public List<ConsentModel> Consents { get; set; }
        [DataMember]
        public List<AppointmentModel> Appointments { get; set; }
        public AppointmentModel Appointment { get; set; }
        [DataMember]
        public List<ContentDetailModel> AppointmentDetails { get; set; }
        [DataMember]
        public List<AppIntroModel> AppIntros { get; set; }
        [DataMember]
        public List<OptionModel> AppointmentParticipantsDropdown { get; set; }
        [DataMember]
        public List<ParticipantsModel> AppointmentParticipants { get; set; }
        [DataMember]
        public List<ContentPageModel> Pages { get; set; }
        [DataMember]
        public List<ContentDetailModel> PagesDetails { get; set; }
        [DataMember]
        public List<EductaionCatergoryModel> EducationCategory { get; set; }
        [DataMember]
        public List<EducationCategoryDetailModel> EducationCategoryDetails { get; set; }
        [DataMember]
        public List<ContentPageModel> EducationPages { get; set; }
        [DataMember]
        public List<ContentDetailModel> EducationPageDetails { get; set; }
        [DataMember]
        public List<PatientEducationModel> PatientEducations { get; set; }
        [DataMember]
        public List<FileModel> Files { get; set; }
        [DataMember]
        public List<FileDocumentModel> FileDocuments { get; set; }
        [DataMember]
        public List<FileCategoryModel> FileCategories { get; set; }
        [DataMember]
        public List<FileCategoryDetailModel> FileCategoryDetails { get; set; }
        [DataMember]
        public List<OrganisationModel> Organisations { get; set; }
        [DataMember]
        public List<ProgramModel> Programs { get; set; }
        [DataMember]
        public List<PatientProgramModel> PatientPrograms { get; set; }
        [DataMember]
        public List<TaskModel> PatientTasks { get; set; }
        [DataMember]
        public List<UserConsentModel> UserConsents { get; set; }
        [DataMember]
        public List<UserProfessionModel> Professions { get; set; }

        [DataMember]
        public List<UserRolesModel> Roles { get; set; }
        //Questionnaire

        [DataMember]
        public List<QuestionnaireModel> Questionnaires { get; set; }
        [DataMember]
        public List<QuestionnaireDetailsModel> QuestionnaireDetails { get; set; }
        [DataMember]
        public List<QuestionnaireQuestionModel> QuestionnaireQuestions { get; set; }
        [DataMember]
        public List<QuestionnaireQuestionDetailsModel> QuestionnaireQuestionDetails { get; set; }
        [DataMember]
        public List<QuestionnaireQuestionOptionModel> QuestionnaireQuestionOptions { get; set; }
        [DataMember]
        public List<QuestionnaireQuestionOptionDetailsModel> QuestionnaireQuestionOptionDetails { get; set; }
        [DataMember]
        public List<QuestionnaireSubscaleModel> QuestionnaireSubscales { get; set; }
        [DataMember]
        public List<QuestionnaireSubscaleRangesModel> QuestionnaireSubscaleRanges { get; set; }
        [DataMember]
        public List<QuestionConditionModel> QuestionConditions { get; set; }
        [DataMember]
        public List<QuestionScoreModel> QuestionScores { get; set; }
        [DataMember]
        public List<QuestionnaireSubscaleRangeDetailsModel> QuestionnaireRecommendations { get; set; }
        [DataMember]
        public List<PatientQuestionnaireQuestionAnswersModel> PatientQuestionnaireQuestionAnswers { get; set; }
        [DataMember]
        public List<PatientQuestionnaireScoresModel> PatientQuestionnaireScores { get; set; }
        [DataMember]
        public List<ProgramTrackerModel> ProgramTrackers { get; set; }

        [DataMember]
        public List<PatientTrackersModel> PatientTrackers { get; set; }

        [DataMember]
        public List<PaymentModeModel> PaymentModes { get; set; }

        [DataMember]
        public List<PatientBillModel> PaitientBills { get; set; }

        [DataMember]
        public List<TrackersModel> Trackers { get; set; }

        [DataMember]
        public List<TrackersI18NModel> TrackersI18N { get;set; }
        [DataMember]
        public List<TrackerRangeModel> TrackerRanges { get; set; }
        [DataMember]
        public List<TrackerRangesI18N> TrackerRangesI18N { get; set; }

        [DataMember]
        public List<PatientTrackersValuesModel> PatientTrackerValues { get; set; }
        /// <summary>
        /// List of reading units
        /// </summary>
        [DataMember]
        public List<UnitModel> Units { get; set; }

        /// <summary>
        /// List of reading units language specific data
        /// </summary>
        [DataMember]
        public List<UnitI18NModel> UnitsI18N { get; set; }

        /// <summary>
        /// List of reading metadata
        /// </summary>
        [DataMember]
        public List<ReadingModel> Readings { get; set; }

        /// <summary>
        /// List of reading ranges
        /// </summary>
        [DataMember]
        public List<ReadingRangeModel> ReadingRanges { get; set; }

        /// <summary>
        /// List of patient reading targets
        /// </summary>
        [DataMember]
        public List<ReadingTargetModel> PatientReadingTargets { get; set; }

        /// <summary>
        /// List of reading sources/devices
        /// </summary>
        [DataMember]
        public List<ReadingDeviceModel> ReadingDevices { get; set; }

        /// <summary>
        /// List of patient reading sources/devices
        /// </summary>
        [DataMember]
        public List<PatientDeviceModel> PatientReadingDevices { get; set; }
        /// <summary>
        /// List of Readings Master Data/devices
        /// </summary>
        [DataMember]
        public List<ReadingMasterModel> ReadingMasters { get; set; }

        /// <summary>
        /// List of patient reading data
        /// </summary>
        [DataMember]
        public List<PatientReadingModel> PatientReadings { get; set; }

        [DataMember]
        public List<UserAccountSettingsModel> UserAccountSettings { get; set; }

        [DataMember]
        public List<MedicineModel> Medicines { get; set; }
        [DataMember]
        public List<PatientMedicationModel> PatientMedications { get; set; }
        [DataMember]
        public List<MedicationReminderModel> MedicationReminders { get; set; }
        [DataMember]
        public List<FileCategoryModel> FileCategory { get; set; }
        [DataMember]
        public List<InstructionModel> Instructions { get; set; }
        [DataMember]
        public List<InstructionI18NModel> InstructionI18N { get; set; }
        [DataMember]
        public List<PatientProviderNoteModel> PatientProviderNotes { get; set; }

        /// <summary>
        /// List of patient contact details
        /// </summary>
        [DataMember]
        public List<ContactDetailModel> PatientContactDetails { get; set; }

        /// <summary>
        /// List of patient contacts
        /// </summary>
        [DataMember]
        public List<ContactModel> PatientContacts { get; set; }

        /// <summary>
        /// List of program notes
        /// </summary>
        [DataMember]
        public List<ProgramNoteModel> ProgramNotes { get; set; }
        /// <summary>
        /// List of program notes
        /// </summary>
        [DataMember]
        public List<ProgramNoteI18NModel> ProgramNotesI18N { get; set; }

        /// <summary>
        /// Program caregivers
        /// </summary>
        [DataMember]
        public List<CaregiverModel> ProgramCaregivers { get; set; }

        /// <summary>
        /// List of provider notes
        /// </summary>
        [DataMember]
        public List<PatientProviderNoteModel> ProviderNotes { get; set; }

        /// <summary>
        /// List of provider notes
        /// </summary>
        [DataMember]
        public List<PaymentModeI18NModel> PaymentModeI18N { get; set; }
        /// <summary>
        /// List of provider notes
        /// </summary>
        [DataMember]
        public List<BillPaymentModel> BillPaymentModes { get; set; }
        /// <summary>
        /// List of Patient Bill items
        /// </summary>

        [DataMember]
        public List<PatientBillItemModel> PatientBillItems { get; set; }

        /// <summary>
        /// List of Patient Bill items
        /// </summary>

        [DataMember]
        public List<PatientBillModel> PatientBills { get; set; }

        /// <summary>
        /// List of Patient Bill items
        /// </summary>
        [DataMember]
        public List<BillItemModel> BillingItems { get; set; }

        /// <summary>
        /// List of Patient Bill items
        /// </summary>
        [DataMember]
        public List<BillingItemsI18NModel> BillingItemsI18N { get; set; }

        /// <summary>
        /// List of Patient Bill items
        /// </summary>
        [DataMember]
        public List<ProgramBillingModel> ProgramBillingItems { get; set; }
        /// <summary>
        /// Patients Shared Programs
        /// </summary>
        [DataMember]
        public List<PatientsSharedProgramsModel> PatientsSharedPrograms { get; set; }

        [DataMember]
        public List<ParticipantsModel> ExternalParticipants { get; set; }
        
    }
}