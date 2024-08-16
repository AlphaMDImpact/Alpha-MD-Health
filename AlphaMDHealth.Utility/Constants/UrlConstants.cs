namespace AlphaMDHealth.Utility;

public static class UrlConstants
{
    #region Environment Specific paths

    /// <summary>
    /// For mobile local services base url
    /// In case not working in Windows check 127.0.0.1 is allowed for http(s) in \.vs\AlphaMDHealth\config\applicationhost.config.
    /// When 127.0.0.1 ip will be working in system browser, 10.0.2.2 ip will work in emulator/device browser
    /// </summary>
    public static string DEFAULT_ENVIRONMENT_KEY_VALUE => GenericMethods.GetPlatformSpecificValue(
        "https://10.0.2.2:5001/api/",
        "https://10.0.2.2:5001/api/",
        "https://localhost:5001/api/"
    );

    ///// <summary>
    ///// hosted services base url
    ///// </summary>
    //public static string DEFAULT_ENVIRONMENT_KEY_VALUE => Constants.DEFAULT_ENVIRONMENT_KEY == Constants.DEV_KEY
    //                                                         ? "https://devalphamdhealth.com/api/" //DEV
    //                                                     : Constants.DEFAULT_ENVIRONMENT_KEY == Constants.UAT_KEY
    //                                                         ? "https://simpledosesservicesuat.azurewebsites.net/api/" //UAT
    //                                                     : Constants.DEFAULT_ENVIRONMENT_KEY == Constants.DEV_INDIA_KEY
    //                                                         ? "https://services-alphamd-uat.azurewebsites.net/api/" //DEV-INDIA
    //                                                         : "https://alphamdhealthprod1service.azurewebsites.net/api/"; //PROD

    /// <summary>
    /// For local services base url
    /// In case not working in Windows check 127.0.0.1 is allowed for http(s) in \.vs\AlphaMDHealth\config\applicationhost.config
    /// When 127.0.0.1 ip will be working in system browser, 10.0.2.2 ip will work in emulator/device browser
    /// </summary>
    public static string MICRO_SERVICE_PATH => GenericMethods.GetPlatformSpecificValue(
        "https://10.0.2.2:7230/api/",
        "https://10.0.2.2:7230/api/",
        "https://localhost:7230/api/"
    );

    /// <summary>
    /// microservice base url
    /// </summary>
    //public static string MICRO_SERVICE_PATH => Constants.DEFAULT_ENVIRONMENT_KEY == Constants.DEV_KEY
    //                                                   ? "https://simpledosesmicroservicesdev.azurewebsites.net/api/" //DEV
    //                                                : Constants.DEFAULT_ENVIRONMENT_KEY == Constants.DEV_INDIA_KEY
    //                                                   ? "https://microservice-alphamd-uat.azurewebsites.net/api/" //DEV-INDIA
    //                                               : Constants.DEFAULT_ENVIRONMENT_KEY == Constants.UAT_KEY
    //                                                   ? "https://simpledosesmicroservicesuat.azurewebsites.net/api/" //UAT
    //                                                   : "https://alphamdhealthprod1microservices.azurewebsites.net/api/"; //PROD

    /// <summary>
    /// Fare-base path
    /// </summary>
    public static string FIREBASE_DOMAIN_URL => "https://firebasedynamiclinks.googleapis.com/v1/shortLinks?key=";

    /// <summary>
    /// local service Organization Domain
    /// </summary>
    public static string LOCAL_SERVICE_ORGANISATION_DOMAIN => Constants.DEFAULT_ENVIRONMENT_KEY == Constants.DEV_KEY
                                                   ? "devalphamdhealth.com" //DEV
                                                : Constants.DEFAULT_ENVIRONMENT_KEY == Constants.DEV_INDIA_KEY
                                                   ? "webapp-alphamd-uat.azurewebsites.net" //DEV-INDIA
                                                : Constants.DEFAULT_ENVIRONMENT_KEY == Constants.UAT_KEY
                                                   ? "simpledoseswebuat.azurewebsites.net" //UAT
                                                   : "alphamdhealthprod1web.azurewebsites.net"; //PROD

    /// <summary>
    /// service Organization Domain will be used in html base tag
    /// </summary>
    public static string SERVICE_ORGANISATION_DOMAIN => Constants.DEFAULT_ENVIRONMENT_KEY == Constants.DEV_KEY
                                                   ? "https://devalphamdhealth.com" //DEV
                                                : Constants.DEFAULT_ENVIRONMENT_KEY == Constants.DEV_INDIA_KEY
                                                   ? "https://webapp-alphamd-dev-centralindia.azurewebsites.net" //DEV-INDIA	
                                                : Constants.DEFAULT_ENVIRONMENT_KEY == Constants.UAT_KEY
                                                   ? "https://simpledoseswebuat.azurewebsites.net" //UAT
                                                   : "https://alphamdhealthprod1web.azurewebsites.net"; //PROD
    #endregion

    #region Generic services

    // Sync all services data in batch from mobile
    public static string GET_MOBILE_MASTER_DATA_ASYNC_PATH => "DataSyncService/GetMobileMasterDataAsync";
    public static string GET_MOBILE_USER_DATA_ASYNC_PATH => "DataSyncService/GetMobileUserDataAsync";

    // Get master page data services used for web
    public static string GET_MASTER_PAGE_DATA_ASYNC_PATH => "MasterService/GetMasterDataAsync";

    // ErrorLog services
    public static string SAVE_ERROR_LOGS_ASYNC_PATH => "DataSyncService/SaveErrorLogsAsync";

    public static string REGISTER_SIGNALR_ASYNC_PATH => "DataSyncService/RegisterSignalRAsync";

    #endregion

    #region Master Data Services

    //// Countries related data
    public static string GET_COUNTRIES_ASYNC_PATH => "MasterDataService/GetCountriesAsync";

    //// Language related services
    public static string GET_MOBILE_LANGUAGES_ASYNC_PATH => "LanguageService/GetMobileLanguagesAsync";

    //// Setting related services
    public static string GET_MOBILE_SETTINGS_ASYNC_PATH => "MasterDataService/GetMobileSettingsAsync";

    //// Resource related services
    public static string GET_RESOURCES_ASYNC_PATH => "ResourceService/GetResourcesAsync";

    public static string GET_PROFESSION_ASYNC_PATH => "ProfessionService/GetProfessionsAsync";
    public static string SAVE_PROFESSION_ASYNC_PATH => "ProfessionService/SaveProfessionAsync";

    public static string GET_EDUCATION_CATEGORY_ASYNC_PATH => "EducationCategoryService/GetEducationCategoriesAsync";
    public static string SAVE_EDUCATION_CATEGORY_ASYNC_PATH => "EducationCategoryService/SaveEducationCategoryAsync";

    #endregion

    #region Chat services

    public static string GET_CHATS_ASYNC_PATH => "ChatService/GetChatsAsync";
    public static string SAVE_CHAT_ASYNC_PATH => "ChatService/SaveChatAsync";

    #endregion

    #region Mobile Menus services

    public static string GET_MOBILE_MENUS_ASYNC_PATH => "MenuService/GetMobileMenusAsync";
    public static string SAVE_MOBILE_MENU_ASYNC_PATH => "MenuService/SaveMobileMenusAsync";

    #endregion

    #region Web menu services

    public static string GET_WEB_MENUS_ASYNC_PATH => "MenuService/GetWebMenusAsync";
    public static string SAVE_WEB_MENUS_ASYNC_PATH => "MenuService/SaveWebMenusAsync";

    #endregion

    #region Organisation services

    public static string GET_ORGANISATION_VIEW_ASYNC_PATH => "OrganisationService/GetOrganisationViewAsync";
    public static string GET_ORGANISATION_DATA_ASYNC_PATH => "OrganisationService/GetOrganisationProfileAsync";
    public static string SAVE_ORGANISATION_PROFILE_ASYNC_PATH => "OrganisationService/SaveOrganisationProfileAsync";
    public static string GET_ORGANISATION_BRANCHES_ASYNC_PATH => "OrganisationService/GetOrganisationBranchesAsync";
    public static string SAVE_ORGANISATION_BRANCH_ASYNC_PATH => "OrganisationService/SaveOrganisationBranchAsync";
    public static string UPDATE_ORGANISATION_SETTINGS_ASYNC_PATH => "OrganisationService/UpdateOrganisationSettingsAsync";
    public static string GET_ORGANISATION_SETTINGS_ASYNC_PATH => "OrganisationService/GetOrganisationSettingsAsync";

    public static string GET_ORGANISATION_TAGS_ASYNC_PATH => "OrganisationTagService/GetOrganisationTagsAsync";

    public static string SAVE_ORGANISATION_TAG_ASYNC_PATH => "OrganisationTagService/SaveOrganisationTagAsync";

    #endregion

    #region Program Reading services

    /// <summary>
    /// Name of service endpoint to get program reading
    /// </summary>
    public static string GET_PROGRAM_READING_ASYNC_PATH => "ProgramReadingService/GetProgramReadingAsync";

    /// <summary>
    /// Name of service endpoint to save program reading
    /// </summary>
    public static string SAVE_PROGRAM_READING_ASYNC_PATH => "ProgramReadingService/SaveProgramReadingAsync";

    /// <summary>
    /// Name of service endpoint to get program reading metadata
    /// </summary>
    public static string GET_PROGRAM_READING_METADATA_ASYNC_PATH => "ProgramReadingService/GetReadingMetadataAsync";

    /// <summary>
    /// Name of service endpoint to save program reading metadata
    /// </summary>
    public static string SAVE_PROGRAM_READING_METADATA_ASYNC_PATH => "ProgramReadingService/SaveReadingMetadataAsync";

    /// <summary>
    /// Name of service endpoint to get program reading ranges
    /// </summary>
    public static string GET_PROGRAM_READING_RANGES_ASYNC_PATH => "ProgramReadingService/GetProgramReadingRangesAsync";

    /// <summary>
    /// Name of service endpoint to save program reading range
    /// </summary>
    public static string SAVE_PROGRAM_READING_RANGE_ASYNC_PATH => "ProgramReadingService/SaveProgramReadingRangeAsync";

    public static string GET_READING_TARGETS_ASYNC_PATH => "ProgramReadingService/GetReadingTargetsAsync";

    #endregion

    #region Patient Reading services

    public static string GET_PATIENT_READINGS_ASYNC_PATH => "PatientReadingService/GetPatientReadingsAsync";
    public static string SAVE_PATIENT_READINGS_ASYNC_PATH => "PatientReadingService/SavePatientReadingsAsync";
    public static string SAVE_PATIENT_READING_TARGETS_ASYNC_PATH => "PatientReadingService/SavePatientReadingTargetsAsync";
    public static string GET_SEARCH_FOOD_ITEM_ASYNC => "PatientReadingService/SearchFoodItemAsync";
    public static string GET_FOOD_NUTRITION_DATA_ASYNC => "PatientReadingService/GetFoodNutritionDataAsync";
    public static string GET_PATIENT_SCAN_VITALS_DATA_ASYNC_PATH => "PatientReadingService/GetPatientScanVitalsDataAsync";


    #endregion

    #region Menu Services
    public static string GET_WEB_MENU_GROUPS_ASYNC_PATH => "MenuService/GetWebMenuGroupsAsync";
    public static string SAVE_WEB_MENU_GROUPS_ASYNC_PATH => "MenuService/SaveWebMenuGroupAsync";
    public static string GET_MOBILE_MENU_NODES_ASYNC_PATH => "MenuService/GetMobileMenuNodesAsync";
    public static string SAVE_MOBILE_MENU_NODE_ASYNC_PATH => "MenuService/SaveMobileMenuNodeAsync";
    public static string GET_MOBILE_MENU_GROUPS_ASYNC_PATH => "MenuService/GetMobileMenuGroupsAsync";
    public static string SAVE_MOBILE_MENU_GROUP_ASYNC_PATH => "MenuService/SaveMobileMenuGroupAsync";
    #endregion

    #region Login Flow Services

    public static string GET_ACCOUNT_DATA_ASYNC_PATH => "AuthService/GetAccountDataAsync";
    public static string GET_CHANGE_PASSWORD_DATA_ASYNC_PATH => "AuthService/GetChangePasswordDataAsync";
    public static string GET_PINCODE_DATA_ASYNC_PATH => "AuthService/GetPincodeDataAsync";
    public static string GET_PINCODE_LOGIN_DATA_ASYNC_PATH => "AuthService/GetPincodeLoginDataAsync";
    public static string GET_TOKEN_ASYNC_PATH => "AuthService/GetTokenAsync";
    public static string REGISTER_USER_ASYNC_PATH => "UserService/RegisterUserAsync";
    public static string LOGIN_ASYNC_PATH => "AuthService/LoginAsync"; 
    public static string LOGIN_WITH_TEMP_TOKEN_ASYNC_PATH => "AuthService/LoginWithTempTokenAsync";
    public static string FORGOT_PASSWORD_ASYNC_PATH => "AuthService/ForgotPasswordAsync";
    public static string RESET_PASSWORD_ASYNC_PATH => "AuthService/ResetPasswordAsync";
    public static string CHANGE_PASSWORD_ASYNC_PATH => "AuthService/ChangePasswordAsync";
    public static string RESEND_OTP_ASYNC_PATH => "AuthService/ResendOtpAsync";
    public static string VERIFY_PINCODE_ASYNC_PATH => "AuthService/VerifyPincodeAsync";
    public static string SET_PINCODE_ASYNC_PATH => "AuthService/SetPincodeAsync";

    #endregion

    #region ContentPage Services
    public static string GET_BASIC_CONTENT_PAGES_ASYNC_PATH => "ContentPageService/GetBasicContentPagesAsync";
    public static string GET_CONTENT_PAGES_ASYNC_PATH => "ContentPageService/GetContentPagesAsync";
    public static string SAVE_CONTENT_PAGE_ASYNC_PATH => "ContentPageService/SaveContentPagesAsync";
    public static string SAVE_PATIENT_EDUCATION_ASYNC_PATH => "PatientEducationService/SavePatientEducationAsync";
    public static string SAVE_EDUCATION_STATUS_ASYNC_PATH => "ContentPageService/SaveEducationStatusAsync";
    #endregion

    #region Department Service
    public static string GET_DEPARTMENTS_ASYNC_PATH => "DepartmentService/GetDepartmentsAsync";
    public static string SAVE_DEPARTMENT_ASYNC_PATH => "DepartmentService/SaveDepartmentAsync";
    #endregion

    #region BillingItem Service
    public static string GET_BILLING_ITEMS_ASYNC_PATH => "BillingItemService/GetBillingItemsAsync";
    public static string SAVE_BILLING_ITEM_ASYNC_PATH => "BillingItemService/SaveBillingItemAsync";

    public static string GET_PAYMENT_MODES_ASYNC_PATH => "BillingItemService/GetPaymentModesAsync";
    public static string SAVE_PAYMENT_MODE_ASYNC_PATH => "BillingItemService/SavePaymentModeAsync";

    public static string GET_PATIENT_BILLS_ASYNC_PATH => "BillingItemService/GetPatientBillsAsync";
    public static string SAVE_PATIENT_BILL_ASYNC_PATH => "BillingItemService/SavePatientBillAsync";

    /// <summary>
    ///  Name of service endpoint to get program Billing Item(s)
    /// </summary>
    public static string GET_PROGRAM_BILLING_ITEM_ASYNC_PATH => "ProgramService/GetProgramBillingItemsAsync";

    /// <summary>
    ///  Name of service endpoint to save program Billing Item
    /// </summary>
    public static string SAVE_PROGRAM_BILLING_ITEM_ASYNC_PATH => "ProgramService/SaveProgramBillingItemAsync";

    /// <summary>
    ///  Name of service endpoint to save program reason configuration
    /// </summary>
    public static string SAVE_PROGRAM_REASON_CONFIGURATIION_ASYNC_PATH => "ProgramService/SaveProgramReasonConfigurationsAsync";

    /// <summary>
    ///  Name of service endpoint to get program Reason (s)
    /// </summary>
    public static string GET_PROGRAM_REASONS_ASYNC_PATH => "ReasonService/GetProgramReasonsAsync";

    /// <summary>
    ///  Name of service endpoint to save program Reason
    /// </summary>
    public static string SAVE_PROGRAM_REASON_ASYNC_PATH => "ReasonService/SaveProgramReasonAsync";
    #endregion

    #region
    public static string GET_REASONS_ASYNC_PATH => "ReasonService/GetReasonsAsync";
    public static string SAVE_REASON_ASYNC_PATH => "ReasonService/SaveReasonAsync";
    #endregion

    #region User Service
    public static string GET_USERS_ASYNC_PATH => "UserService/GetUsersAsync";
    public static string SAVE_USER_ASYNC_PATH => "UserService/SaveUserAsync";
    public static string DELETE_USER_ASYNC_PATH => "UserService/DeleteUserAsync";
    public static string RESND_ACTIVATION_ASYNC_PATH => "UserService/ResendActivationAsync";
    #endregion

    #region Contacts Service
    public static string GET_CONTACTS_ASYNC_PATH => "ContactsService/GetContactsAsync";
    public static string SAVE_CONTACTS_ASYNC_PATH => "ContactsService/SaveContactsAsync";
    #endregion

    #region Questionnaire Service
    public static string GET_QUESTIONNAIRES_ASYNC_PATH => "QuestionnaireService/GetQuestionnairesAsync";
    public static string SAVE_QUESTIONNAIRE_ASYNC_PATH => "QuestionnaireService/SaveQuestionnaireAsync";
    public static string PUBLISH_QUESTIONNAIRE_ASYNC => "QuestionnaireService/PublishQuestionnaireAsync";
    public static string GET_QUESTIONNAIRE_SUBSCALE_ASYNC_PATH => "QuestionnaireService/GetQuestionnaireSubscaleAsync";
    public static string SAVE_QUESTIONNAIRE_SUBSCALE_ASYNC_PATH => "QuestionnaireService/SaveQuestionnaireSubscaleAsync";
    public static string SAVE_QUESTIONNAIRE_SUBSCALE_RANGES_ASYNC_PATH => "QuestionnaireService/SaveQuestionnaireSubscaleRangesAsync";
    public static string GET_QUESTIONNAIRE_QUESTIONS_ASYNC_PATH => "QuestionnaireService/GetQuestionnaireQuestionsAsync";
    public static string SAVE_QUESTIONNAIRE_QUESTIONS_ASYNC_PATH => "QuestionnaireService/SaveQuestionnaireQuestionAsync";
    public static string SAVE_QUESTIONNAIRE_CONDITIONS_ASYNC_PATH => "QuestionnaireService/SaveQuestionConditionsAsync";
    public static string SAVE_PATIENT_QUESTIONNAIRE_RESULTS_ASYNC_PATH => "QuestionnaireService/SavePatientQuestionnaireResultsAsync";
    public static string GET_QUESTION_CONDITIONS_ASYNC_PATH => "QuestionnaireService/GetQuestionConditionsAsync";
    public static string GET_QUESTION_SCORE_ASYNC_PATH => "QuestionnaireService/GetQuestionScoreAsync";
    public static string SAVE_QUESTION_SCORE_ASYNC_PATH => "QuestionnaireService/SaveQuestionScoreAsync";

    /// <summary>
    /// Save and next questionnaire api endpoint
    /// </summary>
    public static string GET_NEXT_QUESTION_ASYNC_PATH => "PatientTaskService/GetNextQuestionAsync";

    #endregion

    #region DashBoard Service

    public static string GET_DASHBOARD_CONFIGURATIONS_ASYNC_PATH => "DashboardService/GetDashboardConfigurationsAsync";
    public static string SAVE_DASHBOARD_CONFIGURATION_ASYNC_PATH => "DashboardService/SaveDashboardConfigurationAsync";

    #endregion

    #region Appointment Service
    public static string GET_APPOINTMENTS_ASYNC_PATH => "AppointmentService/GetAppointmentsAsync";
    public static string SAVE_APPOINTMENT_ASYNC_PATH => "AppointmentService/SaveAppointmentAsync";
    public static string UPDATE_APPOINTMENT_STATUS_ASYNC_PATH => "AppointmentService/UpdateAppointmentStatusAsync";
    #endregion

    #region Caregiver Service
    public static string GET_CAREGIVERS_ASYNC_PATH => "UserService/GetPatientCaregiversAsync";
    public static string SAVE_CAREGIVER_ASYNC_PATH => "UserService/SavePatientCaregiverAsync";
    #endregion

    #region Program Service

    public static string GET_SUBFLOWS_ASYNC_PATH => "ProgramService/GetSubFlowsAsync";
    public static string SAVE_SUBFLOW_ASYNC_PATH => "ProgramService/SaveSubFlowAsync";
    public static string GET_TASKS_ASYNC_PATH => "ProgramService/GetTasksAsync";
    public static string SAVE_TASK_ASYNC_PATH => "ProgramService/SaveTaskAsync";
    public static string GET_TASK_SUBFLOW_ASYNC_PATH => "ProgramService/GetTaskSubFlowAsync";
    public static string SAVE_TASK_SUBFLOW_ASYNC_PATH => "ProgramService/SaveTaskSubFlowAsync";
    public static string GET_PROGRAMS_ASYNC_PATH => "ProgramService/GetProgramsAsync";
    public static string SAVE_PROGRAM_ASYNC_PATH => "ProgramService/SaveProgramAsync";
    public static string SAVE_PUBLISH_PROGRAM_ASYNC_PATH => "ProgramService/PublishProgramAsync";
    public static string GET_PROGRAM_TASK_ASYNC_PATH => "ProgramService/GetProgramTaskAsync";
    public static string SAVE_PROGRAM_TASK_ASYNC_PATH => "ProgramService/SaveProgramTaskAsync";
    public static string GET_PROGRAM_SUBFLOW_ASYNC_PATH => "ProgramService/GetProgramSubFlowAsync";
    public static string SAVE_PROGRAM_SUBFLOW_ASYNC_PATH => "ProgramService/SaveProgramSubflowAsync";
    public static string GET_PROGRAM_CAREGIVER_ASYNC_PATH => "ProgramService/GetProgramCaregiverAsync";
    public static string SAVE_PROGRAM_CAREGIVER_ASYNC_PATH => "ProgramService/SaveProgramCaregiverAsync";
    public static string GET_ITEMS_BASED_ON_TASK_ASYNC_PATH => "ProgramService/GetItemsBasedOnTaskTypeAsync";
    public static string SAVE_PATIENT_PROGRAMS_ASYNC_PATH => "PatientProgramService/SavePatientProgramsAsync";
    public static string GET_PATIENT_PROGRAMS_ASYNC_PATH => "PatientProgramService/GetPatientProgramsAsync";
    public static string SAVE_PROGRAM_EDUCATION_ASYNC_PATH => "ProgramService/SaveProgramEducationAsync";
    public static string GET_PROGRAM_EDUCATION_ASYNC_PATH => "ProgramService/GetProgramEducationAsync";
    public static string SAVE_SUBSCRIBE_PROGRAM_ASYNC_PATH => "ProgramService/SubscribeProgramAsync";
    public static string GET_INSTRUCTIONS_ASYNC_PATH => "ProgramService/GetInstructionsAsync";
    public static string SAVE_INSTRUCTIONS_ASYNC_PATH => "ProgramService/SaveInstructionsAsync";

    #endregion

    #region Document Service
    public static string GET_FILES_ASYNC_PATH => "FilesService/GetFilesAsync";
    public static string SAVE_FILES_ASYNC_PATH => "FilesService/SaveFilesAsync";
    public static string DELETE_FILES_ASYNC_PATH => "FilesService/DeleteFilesAsync";
    public static string UPDATE_DOCUMENT_READ_STATUS_ASYNC_PATH => "FilesService/UpdateDocumentStatusAsync";
    #endregion

    #region Micro Service

    public static string UPLOAD_FILE_STORAGE_ASYNC => "FileStorageService/UploadFilesAsync";
    public static string GET_FILE_STORAGE_REPLACEMENT_CDN_LINK_ASYNC => "FileStorageService/GetReplacementCdnLinkAsync";
    public static string SEND_EMAIL_MESSAGE_ASYNC => "CommunicationService/SendEmailMessageAsync";
    public static string SEND_SMS_MESSAGE_ASYNC => "CommunicationService/SendSmsMessageAsync";
    public static string SEND_NOTIFICATIONS_MESSAGE_ASYNC => "CommunicationService/SendNotificationsMessageAsync";
    public static string SEND_WHATS_APP_MESSAGE_ASYNC => "CommunicationService/SendWhatsAppMessageAsync";
    public static string GENERATE_SESSION_FOR_VIDEO_ASYNC => "VideoLibraryService/GenerateSession";
    public static string CREATE_SESSION_FOR_VIDEO_ASYNC => "VideoLibraryService/CreateSession";
    public static string FOOD_LIBRARY_SEARCH_FOOD_ITEM_ASYNC => "FoodLibraryService/SearchFoodItemAsync";
    public static string GET_FOOD_DATA_ASYNC => "FoodLibraryService/GetFoodDataAsync";
    public static string REGISTER_DEVICE_FOR_NOTIFICATION_ASYNC => "CommunicationService/RegisterDeviceForNotificationAsync";

    #endregion

    #region Subscription Service

    /// <summary>
    /// API call from Subscription Service to get Subscription Plans
    /// </summary>
    public static string GET_SUBSCRIPTION_PLANS_ASYNC => "SubscriptionService/GetSubscriptionPlansAsync";

    /// <summary>
    /// API call from Subscription Service to post user Subscription Plan
    /// </summary>
    public static string SAVE_USER_SUBSCRIPTION_PLAN_ASYNC => "SubscriptionService/SaveUserSubscriptionPlanAsync";

    #endregion

    #region Consent Service
    public static string GET_CONSENTS_ASYNC => "ConsentService/GetConsentsAsync";
    public static string SAVE_CONSENT_ASYNC => "ConsentService/SaveConsentAsync";
    public static string GET_USER_CONSENTS_ASYNC => "ConsentService/GetUserConsentsAsync";
    public static string SAVE_USER_CONSENT_ASYNC => "ConsentService/SaveUserConsentAsync";

    #endregion

    #region Task Service

    public static string GET_PATIENT_TASKS_ASYNC_PATH => "PatientTaskService/GetPatientTasksAsync";
    public static string SAVE_PATIENT_TASK_ASYNC_PATH => "PatientTaskService/SavePatientTaskAsync";
    public static string UPDATE_PATIENT_TASK_STATUS_ASYNC_PATH => "PatientTaskService/UpdatePatientTaskStatusAsync";

    #endregion

    #region Medication service
    public static string GET_MEDICINES_ASYNC_PATH => "MedicationService/GetMedicinesAsync";
    public static string GET_MEDICATIONS_ASYNC_PATH => "MedicationService/GetMedicationsAsync";
    public static string SAVE_MEDICATION_ASYNC_PATH => "MedicationService/SaveMedicationAsync";
    #endregion

    #region AppIntro Service

    public static string GET_APP_INTROS_ASYNC_PATH => "AppIntroService/GetAppIntrosAsync";

    public static string SAVE_APP_INTROS_ASYNC_PATH => "AppIntroService/SaveAppIntroAsync";

    #endregion

    #region User Account Setting

    /// <summary>
    /// save user account setting url
    /// </summary>
    public static string SAVE_USER_ACCOUNT_SETTING_SERVICE_ASYNC_PATH => "UserAccountSettingService/SaveUserAccountSettingsAsync";

    /// <summary>
    /// Name of service endpoint to get UserAccountSettings
    /// </summary>
    public static string GET_USER_ACCOUNT_SETTING_SERVICE_ASYNC_PATH => "UserAccountSettingService/GetUserAccountSettingsAsync";

    #endregion

    /// <summary>
    /// API to call from azure scheduler
    /// </summary>
    public static string TRIGGER_JOBS_ASYNC => "ScheduleJobService/TriggerJobsAsync";

    public static string REGISTER_DEVICE_FOR_NOTIFICATION_PATH => "PushNotificationService/RegisterDeviceForNotificationsAsync";

    public static string GET_VIDEO_SESSION_ASYNC => "VideoService/GetVideoSessionAsync";

    #region
    public static string GET_FILE_CATEGORY_ASYNC_PATH => "FileCategoryService/GetFileCategoriesAsync";
    public static string SAVE_FILE_CATEGORY_ASYNC_PATH => "FileCategoryService/SaveFileCategoryAsync";
    public static string SAVE_USERS_FROM_EXCEL_ASYNC_PATH => "UserService/SaveUsersFromExcelAsync";
    #endregion

    #region Program Tracker service
    public static string GET_PROGRAM_TRACKER_ASYNC_PATH => "ProgramService/GetProgramTrackersAsync";
    public static string Save_PROGRAM_TRACKER_ASYNC_PATH => "ProgramService/SaveProgramTrackerAsync";
    public static string GET_TRACKERS_ASYNC_PATH => "TrackerService/GetTrackersAsync";
    public static string SAVE_TRACKER_ASYNC_PATH => "TrackerService/SaveTrackerAsync";

    public static string SAVE_TRACKER_RANGES_ASYNC_PATH => "TrackerService/SaveTrackerRangesAsync";
    #endregion

    #region Manage Organisation"
    /// <summary>
    /// get organisations url
    /// </summary>
    public static string GET_ORGANISATIONS_ASYNC_PATH => "OrganisationService/GetOrganisationsAsync";
    #endregion

    /// <summary>
    /// get program note(s) url
    /// </summary>
    public static string GET_PROGRAM_NOTES_ASYNC_PATH => "ProgramService/GetProgramNotesAsync";

    /// <summary>
    /// save-delete program note url
    /// </summary>
    public static string SAVE_PROGRAM_NOTE_ASYNC_PATH => "ProgramService/SaveProgramNoteAsync";

    public static string GET_PATIENT_PROVIDER_NOTE_ASYNC_PATH => "PatientProviderNoteService/GetPatientProviderNotesAsync";
    public static string SAVE_PATIENT_PROVIDER_NOTE_ASYNC_PATH => "PatientProviderNoteService/SavePatientProviderNoteAsync";


    /// <summary>
    /// get bill report url
    /// </summary>
    public static string GET_BILLS_ASYNC => "ReportsService/GetBillsAsync";

    /// <summary>
    /// get patient tracker(s) url
    /// </summary>
    public static string GET_PATIENT_TRACKERS_ASYNC_PATH => "TrackerService/GetPatientTrackersAsync";

    /// <summary>
    /// save-delete patient tracker url
    /// </summary>
    public static string SAVE_PATIENT_TRACKER_ASYNC_PATH => "TrackerService/SavePatientTrackerAsync";

    /// <summary>
    /// Save patient tracker value url
    /// </summary>
    public static string SAVE_PATIENT_TRACKER_VALUE_ASYNC_PATH => "TrackerService/SavePatientTrackerValueAsync";

    #region Medical History Service

    /// <summary>
    /// Name of service endpoint to get Medical History
    /// </summary>
    public static string GET_MEDICAL_HISTORY_ASYNC_PATH => "MedicalHistoryService/GetMedicalHistoryAsync";

    #endregion

    #region Medical Report Forwards
    public static string SAVE_MEDICAL_REPORT_FORWARDS_ASYNC_PATH => "MedicalHistoryService/SaveMedicalReportForwardsAsync";

    public static string GET_MEDICAL_REPORT_FORWARDS_ASYNC_PATH => "MedicalHistoryService/GetMedicalReportForwardsAsync";
    #endregion

    #region HealthScans

    public static string GET_HEALTH_SCANS_ASYNC => "HealthScansService/GetHealthScansAsync";
    public static string SAVE_HEALTH_SCANS_ASYNC => "HealthScansService/SaveHealthScanAsync";
    public static string SAVE_ASSIGN_SCAN_ASYNC => "HealthScansService/SaveAssignScanAsync";
    public static string GET_PATIENT_SCAN_HISTORY_ASYNC_PATH => "PatientScanHistoryService/GetPatientScanHistoryAsync";

    #endregion

    #region ProgramServices
    /// <summary>
    /// get program note(s) url
    /// </summary>
    public static string GET_PROGRAM_SERVICES_ASYNC_PATH => "ProgramService/GetProgramServicesAsync";


    /// <summary>
    /// save-delete program note url
    /// </summary>
    public static string SAVE_PROGRAM_SERVICE_ASYNC_PATH => "ProgramService/SaveProgramServiceAsync";

    /// <summary>
    /// publish - content page
    /// </summary>
    public static string PUBLISH_CONTENT_PAGE_ASYNC_PATH => "ContentPageService/PublishContentPageAsync";
    #endregion

    public static string SAVE_RAZORPAY_PAYMENT_DETAIL_ASYNC => "RazorpayService/SaveRazorpayPaymentDetailAsync";
}