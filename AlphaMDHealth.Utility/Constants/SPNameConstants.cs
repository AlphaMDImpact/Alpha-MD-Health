namespace AlphaMDHealth.Utility;

public static class SPNameConstants
{
    #region Care Programs

    public static string USP_GET_SUBFLOWS => "usp_GetSubflows";
    public static string USP_SAVE_SUBFLOW => "usp_SaveSubflow";
    public static string USP_GET_TASKS => "usp_GetTasks";
    public static string USP_SAVE_TASK => "usp_SaveTask";
    public static string USP_GET_TASK_SUBFLOWS => "usp_GetTaskSubflows";
    public static string USP_SAVE_TASK_SUBFLOW => "usp_SaveTaskSubflow";
    public static string USP_GET_PROGRAMS => "usp_GetPrograms";
    public static string USP_SAVE_PROGRAM => "usp_SaveProgram";
    public static string USP_PUBLISH_PROGRAM => "usp_PublishProgram";
    public static string USP_GET_PROGRAM_TASKS => "usp_GetProgramTasks";
    public static string USP_SAVE_PROGRAM_TASK => "usp_SaveProgramTask";
    public static string USP_GET_PROGRAM_SUBFLOWS => "usp_GetProgramSubflows";
    public static string USP_SAVE_PROGRAM_SUBFLOW => "usp_SaveProgramSubflow";
    public static string USP_GET_PROGRAM_CARE_GIVERS => "usp_GetProgramCaregivers";
    public static string USP_SAVE_PROGRAM_CAREGIVER => "usp_SaveProgramCareGiver";
    public static string USP_GET_PROGRAM_EDUCATIONS => "usp_GetProgramEducations";
    public static string USP_SAVE_PROGRAM_EDUCATION => "usp_SaveProgramEducation";
    public static string USP_GET_ITEMS_FOR_TASK_TYPE => "usp_GetItemsForTaskType";
    public static string USP_SAVE_PATIENT_PROGRAMS => "usp_SavePatientPrograms";
    public static string USP_SUBSCRIBE_PROGRAM => "usp_SubscribeProgram";
    public static string USP_GET_INSTRUCTIONS => "usp_GetInstructions";
    public static string USP_SAVE_INSTRUCTIONS => "usp_SaveInstructions";
    
    #endregion

    #region Subscription Plans

    /// <summary>
    /// Procedure name using which Subscription Plans will be retrieved from server
    /// </summary>
    public static string USP_GET_SUBSCRIPTION_PLANS => "usp_GetSubscriptionPlans";

    /// <summary>
    /// Procedure name using which User Subscription Plan will be saved into server
    /// </summary>
    public static string USP_SAVE_USER_SUBSCRIPTION_PLAN => "usp_SaveUserSubscriptionPlan"; 

    #endregion

    public static string USP_SAVE_DEPARTMENT => "usp_SaveDepartment";
    public static string USP_GET_DEPARTMENTS => "usp_GetDepartments";
    public static string USP_GET_BILLING_ITEMS => "usp_GetBillingItems";
    public static string USP_SAVE_BILLING_ITEM => "usp_SaveBillingItem";

    public static string USP_GET_PAYMENT_MODES => "usp_GetPaymentModes";
    public static string USP_SAVE_PAYMENT_MODE => "usp_SavePaymentMode";

    public static string USP_GET_REASONS => "usp_GetReasons";
    public static string USP_SAVE_REASON => "usp_SaveReason";
    public static string USP_GET_TRACKERS => "usp_GetTrackers";
    public static string USP_SAVE_TRACKER => "usp_SaveTracker";
    public static string USP_SAVE_TRACKER_RANGES => "usp_SaveTrackerRanges";
    public static string USP_GET_ORGANISATION_BRANCHES => "usp_GetOrganisationBranches";
    public static string USP_SAVE_ORGANISATION_BRANCH => "usp_SaveOrganisationBranch";
    public static string USP_GET_MASTER_DATA => "usp_GetMasterData";
    public static string USP_GET_MOBILE_DATA => "usp_GetMobileData";
    public static string USP_GET_RESOURCES => "usp_GetResources";
    public static string USP_GET_SETTINGS => "usp_GetSettings";
    public static string USP_GET_LANGUAGES => "usp_GetLanguages";
    public static string USP_VALIDATE_ACCESS_TOKEN => "usp_ValidateAccessToken";
    public static string USP_GET_SYSTEM_IDENTIFIERS => "usp_GetSystemIdentifiers";
    public static string USP_GET_TOKEN => "usp_GetToken";
    public static string USP_GET_COUNTRY_CODES => "usp_GetCountryCodes";
    public static string USP_GET_ORGANISATION_SETTINGS => "usp_GetOrganisationSettings";
    public static string USP_GET_LANGUAGE => "usp_GetLanguage";
    public static string USP_GET_MOBILE_SETTINGS => "usp_GetMobileSettings";
    public static string USP_ERROR_LOGGING => "usp_ErrorLogging";
    public static string USP_REGISTER_USER => "usp_RegisterUser";
    public static string USP_SAVE_MOBILE_MENU_NODE => "usp_SaveMobileMenuNode";
    public static string USP_GET_MOBILE_MENU_NODES => "usp_GetMobileMenuNodes";
    public static string USP_GET_WEB_MENU_GROUPS => "usp_GetWebMenuGroups";
    public static string USP_SAVE_WEB_MENU_GROUP => "usp_SaveWebMenuGroup";
    public static string USP_GET_MOBILE_MENU_GROUPS => "usp_GetMobileMenuGroups";
    public static string USP_GET_MOBILE_MENUS => "usp_GetMobileMenus";
    public static string USP_SAVE_MOBILE_MENU => "usp_SaveMobileMenu";
    public static string USP_LOGIN => "usp_Login";
    public static string USP_LOGIN_WITH_TEMP_TOKEN => "usp_LoginWithTempToken";
    public static string USP_FORGOT_PASSWORD => "usp_ForgotPassword";
    public static string USP_GET_ORGANISATION_PROFILE => "usp_GetOrganisationProfile";
    public static string USP_SAVE_ORGANISATION_PROFILE => "usp_SaveOrganisationProfile";
    public static string USP_SET_PASSWORD => "usp_SetPassword";
    public static string USP_UPDATE_ORGANISATION_SETTINGS => "usp_UpdateOrganisationSettings";
    public static string USP_GET_CONTENT_PAGES => "usp_GetContentPages";
    public static string USP_SAVE_CONTENT_PAGE => "usp_SaveContentPage";
    public static string USP_PUBLISH_CONTENT_PAGE => "usp_PublishContentPage";
    public static string USP_SAVE_EDUCATION_STATUS => "usp_SaveEducationStatus";
    public static string USP_GET_WEB_MENUS => "usp_GetWebMenus";
    public static string USP_CREATE_OTP => "usp_CreateOTP";
    public static string USP_SAVE_MOBILE_MENU_GROUP => "usp_SaveMobileMenuGroup";
    public static string USP_SAVE_WEB_MENUS => "usp_SaveWebMenu";
    public static string USP_PINCODE => "usp_Pincode";
    public static string USP_GET_CONTACTS => "usp_GetContacts";
    public static string USP_SAVE_CONTACTS => "usp_SaveContacts";
    public static string USP_GET_ORGANISATION_DATA => "usp_GetOrganisationData";
    public static string USP_GET_USERS => "usp_GetUsers";
    public static string USP_SAVE_USER => "usp_SaveUser";
    public static string USP_DELETE_USER => "usp_DeleteUser";
    public static string USP_UPDATE_PASSWORD => "usp_UpdatePassword";
    public static string USP_GET_PROFESSIONS => "usp_GetProfessions";
    public static string USP_SAVE_PROFESSION => "usp_SaveProfession";
    public static string USP_CREATE_PATIENT_TEMP_TOKEN => "usp_CreatePatientTempToken";

    /// <summary>
    /// Procedure name using which program readings will be retrieved from server
    /// </summary>
    public static string USP_GET_PROGRAM_READINGS => "usp_GetProgramReadings";

    /// <summary>
    /// Procedure name using which program reading will be saved in server
    /// </summary>
    public static string USP_SAVE_PROGRAM_READING => "usp_SaveProgramReading";

    /// <summary>
    /// Procedure name using which program reading metadata will be retrieved from server
    /// </summary>
    public static string USP_GET_PROGRAM_READING_METADATA => "usp_GetProgramReadingMetadata";

    /// <summary>
    /// Procedure name using which program reading metadata will be saved in server
    /// </summary>
    public static string USP_SAVE_PROGRAM_READING_METADATA => "usp_SaveProgramReadingMetadata";

    /// <summary>
    /// Procedure name using which program reading ranges will be retrieved from server
    /// </summary>
    public static string USP_GET_PROGRAM_READING_RANGES => "usp_GetProgramReadingRanges";

    /// <summary>
    /// Procedure name using which program reading ranges will be saved in server
    /// </summary>
    public static string USP_SAVE_PROGRAM_READING_RANGE => "usp_SaveProgramReadingRange";

    /// <summary>
    /// Procedure name using which readings will be retrieved from server
    /// </summary>
    public static string USP_GET_PATIENT_READINGS => "usp_GetPatientReadings";

    /// <summary>
    /// Procedure name using which patient reading targets will be saved in server
    /// </summary>
    public static string USP_SAVE_PATIENT_READING_TARGETS => "usp_SavePatientReadingTargets";

    /// <summary>
    /// Procedure name using which Questionnaires will be retrieved from server
    /// </summary>
    public static string USP_GET_QUESTIONNAIRES => "usp_GetQuestionnaires";

    /// <summary>
    /// Procedure name using which questionnaires will be saved in server
    /// </summary>
    public static string USP_SAVE_QUESTIONNAIRE => "usp_SaveQuestionnaire";

    public static string USP_SAVE_PATIENT_QUESTIONNAIRE_RESULTS => "usp_SavePatientQuestionnaireResults";

    /// <summary>
    /// Procedure name using which MedicalHistory will be retrieved from server
    /// </summary>
    public static string USP_GET_MEDICAL_HISTORY => "usp_GetMedicalHistory";

    public static string USP_SAVE_PATIENT_READINGS => "usp_SavePatientReadings";
    public static string USP_PUBLISH_QUESTIONNAIRE => "usp_PublishQuestionnaire";
    public static string USP_GET_QUESTIONNAIRE_SUBSCALE => "usp_GetQuestionnaireSubscale";
    public static string USP_SAVE_QUESTIONNAIRE_SUBSCALE => "usp_SaveQuestionnaireSubscale";
    public static string USP_SAVE_QUESTIONNAIRE_SUBSCALE_RANGES => "usp_SaveQuestionnaireSubscaleRanges";
    public static string USP_GET_QUESTIONNAIRE_QUESTIONS => "usp_GetQuestionnaireQuestions";
    public static string USP_SAVE_QUESTIONNAIRE_QUESTION => "usp_SaveQuestionnaireQuestion";
    public static string USP_GET_DASHBOARD_CONFIGURATIONS => "usp_GetDashboardConfigurations";
    public static string USP_SAVE_DASHBOARD_CONFIGURATION => "usp_SaveDashboardConfiguration";
    public static string USP_GET_TEMPLATE_WITH_DATA => "usp_GetTemplateWithData";
    public static string USP_GET_PENDING_COMMUNICATIONS => "usp_GetPendingCommunications";
    public static string USP_SAVE_PENDING_COMMUNICATION => "usp_SavePendingCommunication";
    public static string USP_GET_APPOINTMENTS => "usp_GetAppointments";
    public static string USP_SAVE_APPOINTMENT => "usp_SaveAppointment";
    public static string USP_UPDATE_APPOINTMENT_STATUS => "usp_UpdateAppointmentStatus";
    public static string USP_GET_CHATS => "usp_GetChats";
    public static string USP_SAVE_CHATS => "usp_SaveChats";
    public static string USP_GET_PATIENT_CAREGIVERS => "usp_GetPatientCaregivers";
    public static string USP_SAVE_PATIENT_CAREGIVER => "usp_SavePatientCaregiver";
    public static string USP_SAVE_PATIENT_EDUCATION => "usp_SavePatientEducation";
    public static string USP_GET_EDUCATION_CATEGORIES => "usp_GetEducationCategories";
    public static string USP_SAVE_EDUCATION_CATEGORY => "usp_SaveEducationCategory";
    public static string USP_GET_SERVICE_DETAILS => "usp_GetServiceDetails";
    public static string USP_SAVE_SERVICE_LOGS => "usp_SaveServiceLogs";
    public static string USP_GET_PATIENT_FILES_AND_DOCUMENTS => "usp_GetPatientFilesAndDocuments";
    public static string USP_SAVE_PATIENT_FILES_AND_DOCUMENTS => "usp_SavePatientFilesAndDocuments";
    public static string USP_UPDATE_DOCUMENT_STATUS => "usp_UpdateDocumentStatus";
    public static string USP_DELETE_PATIENT_FILE => "usp_DeletePatientFile";
    public static string USP_GET_CONSENTS => "usp_GetConsents";
    public static string USP_SAVE_CONSENT => "usp_SaveConsent";
    public static string USP_SAVE_USER_CONSENT => "usp_SaveUserConsent";
    public static string USP_GET_PATIRNT_TASKS => "usp_GetPatientTasks";
    public static string USP_GET_EXTERNAL_SERVICE_TRANSACTIONS => "usp_GetExternalServiceTransactions";
    public static string USP_SAVE_EXTERNAL_SERVICE_TRANSACTION => "usp_SaveExternalServiceTransaction";
    public static string USP_SAVE_PATIENT_TASK => "usp_SavePatientTask";
    public static string USP_CHECK_AND_UPDATE_MISSED_TASKS => "usp_CheckAndUpdateMissedTasks";
    public static string USP_UPDATE_PATIENT_TASK_STATUS => "usp_UpdatePatientTaskStatus";
    public static string USP_GET_PATIENT_PROGRAMS => "usp_GetPatientPrograms";

    /// <summary>
    ///  Procedure name using which  useraccountsettings will be retrived from server
    /// </summary>
    public static string USP_GET_USER_ACCOUNT_SETTINGS => "usp_GetUserAccountSettings";

    /// <summary>
    ///  Procedure name using which  useraccountsettings will be saved to server
    /// </summary>
    public static string USP_SAVE_USER_ACCOUNT_SETTINGS => "usp_SaveUserAccountSettings";
    public static string USP_SAVE_PATIENT_READING_SOURCES => "usp_SavePatientReadingSources";
    public static string USP_GET_MEDICINES => "usp_GetMedicines";
    public static string USP_GET_MEDICATIONS => "usp_GetMedications";
    public static string USP_SAVE_MEDICATIONS => "usp_SaveMedications";
    public static string USP_GET_ORGANISATION_APP_INTRO_SLIDES => "usp_GetOrganisationAppIntroSlides";
    public static string USP_SAVE_ORGANISATION_APP_INTRO_SLIDES => "usp_SaveOrganisationAppIntroSlides";
    public static string USP_GET_QUESTIONNAIRE_QUESTION_CONDITIONS => "usp_GetQuestionnaireQuestionConditions";
    public static string USP_SAVE_QUESTIONNAIRE_QUESTION_CONDITIONS => "usp_SaveQuestionnaireQuestionConditions";
    public static string USP_GET_QUESTIONNAIRE_QUESTION_SCORES => "usp_GetQuestionnaireQuestionScores";
    public static string USP_SAVE_QUESTIONNAIRE_QUESTION_SCORES => "usp_saveQuestionnaireQuestionScores";
    public static string USP_GET_FILE_CATEGORIES => "usp_GetFileAndDocumentCategories";
    public static string USP_SAVE_FILE_AND_DOCUMENT_CATEGORY => "usp_SaveFileAndDocumentCategory";
    public static string USP_GET_PROGRAM_TRACKERS => "usp_GetProgramTrackers";
    public static string USP_SAVE_PROGRAM_TRACKERS => "usp_SaveProgramTracker";
    public static string USP_GET_PROGRAM_NOTES => "usp_GetProgramNotes";
    public static string USP_GET_PROGRAM_REASON_CONFIGURATIONS => "usp_GetProgramReasonConfigurations";
    public static string USP_SAVE_PROGRAM_NOTE => "usp_SaveProgramNote";
    public static string USP_GET_PATIENT_PROVIDER_NOTES => "usp_GetPatientProviderNotes";
    public static string USP_SAVE_PATIENT_PROVIDER_NOTES => "usp_SavePatientProviderNotes";
    public static string USP_SAVE_NOTE_DOCUMENTS => "usp_SaveNoteDocuments";

    /// <summary>
    /// Procedure name using which Program Reason  will be retrieved from server
    /// </summary>
    public static string USP_GET_PROGRAM_REASONS => "usp_GetProgramReasons";

    /// <summary>
    /// Procedure name using which Program Billing Items will be retrieved from server
    /// </summary>
    public static string USP_GET_PROGRAM_BILLING_ITEMS => "usp_GetProgramBillingItems";

    /// <summary>
    /// Procedure name using which Program Reasons will be saved to server
    /// </summary>
    public static string USP_SAVE_PROGRAM_REASON => "usp_SaveProgramReason";

    /// <summary>
    ///  Procedure name using which Program billing will be saved to server
    /// </summary>
    public static string USP_SAVE_PROGRAM_BILLING_ITEM => "usp_SaveProgramBillingItem";

    /// <summary>
    ///  Procedure name using which Program Reason Configuration will be saved to server
    /// </summary>
    public static string USP_SAVE_PROGRAM_REASON_CONFIGURATION => "usp_SaveProgramReasonConfigurations";


    /// <summary>
    ///   Procedure name using which  bill will be saved to server
    /// </summary>
    public static string USP_SAVE_PATIENT_BILL => "usp_SavePatientBill";

    /// <summary>
    ///  Procedure name using which  bill will be retrived from server
    /// </summary>
    public static string USP_GET_PATIENT_BILLS => "usp_GetPatientBills";

    /// <summary>
    /// Procedure name using which Billings will be retrieved from server
    /// </summary>
    public static string USP_GET_BILLS_REPORT => "usp_GetBillsReport";

    /// <summary>
    /// Procedure name using which Trackers for Patient will be retrieved from server
    /// </summary>
    public static string USP_GET_PATIENT_TRACKERS => "usp_GetPatientTrackers";

    /// <summary>
    /// Procedure name using which Trackers for Patient will be saved on server
    /// </summary>
    public static string USP_SAVE_PATIENT_TRACKERS => "usp_SavePatientTrackers";

    /// <summary>
    /// Procedure name using which save Patient tracker value on server
    /// </summary>
    public static string USP_SAVE_PATIENT_TRACKER_VALUE => "usp_SavePatientTrackerValue";

    /// <summary>
    /// Procedure name using which error logs will be archieved
    /// </summary>
    public static string USP_ARCHIVE_ERROR_LOGS => "usp_ArchiveErrorLogs";

    /// <summary>
    /// Procedure name using which Audit logs will be archieved
    /// </summary>
    public static string USP_ARCHIVE_AUDIT_LOGS => "usp_ArchiveAuditLogs";

    /// <summary>
    /// Procedure name using which User Communications History will be archieved
    /// </summary>
    public static string USP_ARCHIVE_USER_COMMUNICATIONS_HISTORY => "usp_ArchiveUserCommunicationsHistory";

    /// <summary>
    /// Procedure name using which User Account Sessions History will be archieved
    /// </summary>
    public static string USP_ARCHIVE_USER_ACCOUNT_SESSIONS_HISTORY => "usp_ArchiveUserAccountSessionsHistory";

    /// <summary>
    /// Procedure name using which User Account Sessions History will be archieved
    /// </summary>
    public static string USP_GET_NEXT_OR_PREVIOUS_QUESTION => "usp_GetNextOrPreviousQuestion";

    /// <summary>
    /// 
    /// </summary>
    public static string USP_SAVE_MEDICAL_REPORT_FORWARDS => "usp_SaveMedicalReportForwards";

    /// <summary>
    /// Procedure name using which OrganisationTag will retrieve from server
    /// </summary>
    public static string USP_GET_ORGANISATION_TAGS => "usp_GetOrganisationTags";

    /// <summary>
    /// Procedure name using which OrganisationTag will be saved on server
    /// </summary>
    public static string USP_SAVE_ORGANISATION_TAG => "usp_SaveOrganisationTag";

    /// <summary>
    ///  Procedure name using which ProgramServices will retrieve from server
    /// </summary>
    public static string USP_GET_PROGRAM_SERVICES => "usp_GetProgramServices";

    /// <summary>
    /// Procedure name using which ProgramServices will be saved on server
    /// </summary>
    public static string USP_SAVE_PROGRAM_SERVICE => "usp_SaveProgramService";

    /// <summary>
    ///  Procedure name using which Razor pay payment detail will be saved on server
    /// </summary>
    public static string USP_SAVE_RAZORPAY_PAYMENT_DETAIL => "usp_SaveRazorpayPaymentDetail";

    /// <summary>
    /// Procedure name using which Scan Vitals Data will be retrieve from server
    /// </summary>
    public static string USP_GET_SCAN_VITALS_DATA => "usp_GetScanVitalsData";

    /// <summary>
    /// Procedure name using which Scan Vitals Data will be saved to server
    /// </summary>
    public static string USP_SAVE_SCAN_VITALS_DATA => "usp_SaveScanVitalsData";


    /// <summary>
    /// Procedure name using which PatientScanHistory will retrieve from server
    /// </summary>
    public static string USP_GET_PATIENT_SCAN_HISTORY => "usp_GetPatientScanHistory";
}