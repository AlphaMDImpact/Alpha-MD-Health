namespace AlphaMDHealth.Utility;

public class ResourceConstants
{
    #region RSCommonGroup

    /// <summaryR_READING_TYPE_KEY
    /// Action key to display refresh button text
    /// </summary>
    public static string R_REFRESH_ACTION_KEY => "RefreshActionKey";
    public const string R_PREVIOUS_ACTION_KEY = "PreviousActionKey";
    public const string R_START_ACTION_KEY = "StartActionKey";
    public const string R_FINISH_ACTION_KEY = "FinishActionKey";
    public static string R_EDIT_ACTION_KEY => "EditActionKey";
    public static string R_SET_BUTTON_KEY => "SetButtonKey";
    public static string R_RESET_ACTION_KEY => "ResetActionKey";
    public static string R_VIEW_BUTTON_KEY => "ViewButtonKey";
    public static string R_VERIFY_ACTION_KEY => "VerifyActionKey";
    public const string R_SAVE_AND_NEXT_ACTION_KEY = "Save&NextActionKey";
    
    /// <summary>
    /// Key for continue action
    /// </summary>
    public const string R_CONTINUE_ACTION_KEY = "ContinueActionKey";
    public static string R_SHOW_MORE_KEY => "ShowMoreKey";
    public static string R_SHOW_LESS_KEY => "ShowLessKey";
    //public static string R_CANCEL_BUTTON_TEXT_KEY => "CancelButtonTextKey"; // to do : to remove from db and excel
    public static string R_NON_EQUALITY_VALIDATION_KEY => "NonEqualityValidationKey";
    public const string R_SELECT_ICON_KEY = "SelectIconKey";
    public static string R_BROWSE_DESCRIPTION_KEY => "BrowseDescriptionKey";
    public static string R_BROWSE_ACTION_KEY => "BrowseActionKey";
    public static string R_UPLOAD_ACTION_KEY => "UploadActionKey";
    public static string R_MAXIMUM_IMAGE_UPLOAD_SIZE_KEY => "MaximumImageUploadSizeKey";
    public static string R_PUBLISH_KEY => "PublishKey";
    public static string R_UNPUBLISH_KEY => "UnpublishKey";
    public static string R_PUBLISH_CONFIRMATION_KEY => "PublishConfirmationKey";
    public static string R_UNPUBLISH_CONFIRMATION_KEY => "UnpublishConfirmationKey";
    public static string R_PUBLISHED_KEY => "PublishedKey";
    public static string R_UNPUBLISHED_KEY => "UnpublishedKey";
    public static string R_PREVIEW_KEY => "PreviewKey";

    public const string R_SEQUENCE_NO_KEY = "SequenceNoKey";

    public const string R_SEQUENCE_KEY = "SequenceKey";

    public const string R_IS_CRITICAL_KEY = "IsCriticalKey";

    public static string R_DETAILS_KEY => "DetailKey";
    public static string R_ADD_TO_MEDICAL_HISTORY => "AddMedicalHistoryKey";

    public static string R_SUMMARY_RECORD_KEY => "SummaryRecordKey";
    public static string R_DIGITS_AFTER_DECIMAL_POINT => "DigitsAfterDecimalPointKey";
    public static string R_READ_HISTORY_DATA_KEY => "ReadHistoryDataKey";
    public static string R_CHART_TYPE_KEY => "ReadingChartTypeKey";
    public static string R_DURATION_KEY => "ReadingFilterTypeKey";
    public static string R_DURATION_ERROR_KEY => "ReadingFilterTypeErrorKey";
    public static string R_SEQUENCE_NO_ERROR_KEY => "SequenceErrorMessageKey";
    public static string R_DROP_DOWN_PLACE_HOLDER_KEY => "DropDownPlaceHolderKey";
    public static string R_HEADER_KEY => "HeaderKey";
    public static string R_FOOTER_KEY => "FooterKey";
    public static string R_DELETE_CONFIRMATION_KEY => "DeleteConfirmationKey";
    public static string R_FEATURE_TYPE_KEY => "FeatureTypeKey";
    public static string R_PAGE_KEY => "PageKey";
    public static string R_CREATED_ON_TEXT_KEY => "CreatedOnTextKey";
    public static string R_MULTIPLE_USER_KEY => "MultipleUserKey";
    public static string R_REQUIRED_ALL_FIELD_VALIDATION_KEY => "RequiredAllValidationKey";
    public const string R_ORGANISATION_KEY = "OrganisationKey";
    public const string R_BRANCH_KEY = "BranchKey";
    public static string R_DEPARTMENT_KEY => "DepartmentKey";
    public const string R_ROLES_KEY = "RolesKey";
    public const string R_PROFESSION_KEY = "ProfessionKey";
    public static string R_DUPLICATE_DATA_KEY => "DuplicateData";
    public static string R_IS_REQUIRED_KEY => "IsRequiredKey";

    public static string R_ONE_NUMBER_KEY => "OneNumberKey";
    public static string R_TWO_NUMBER_KEY => "TwoNumberKey";
    public static string R_THREE_NUMBER_KEY => "ThreeNumberKey";
    public static string R_FOUR_NUMBER_KEY => "FourNumberKey";
    public static string R_FIVE_NUMBER_KEY => "FiveNumberKey";
    public static string R_SIX_NUMBER_KEY => "SixNumberKey";
    public static string R_SIDE_SECTION_HEADER_KEY => "SideSectionHeaderKey";
    public static string R_SIDE_SECTION_SUB_HEADER_KEY => "SideSectionSubHeaderKey";
    public static string R_ACCEPT_KEY => "AcceptTextKey";
    public static string R_DECLINE_KEY => "DeclineTextKey";
    public static string R_CONSENT_ERROR_KEY => "ConsentErrorMsgKey";
    public static string R_SYNC_DATA_TO_SERVER_ERROR_KEY => "SyncDataToServerErrorKey";
    public static string R_DOWNLOAD_TEXT_KEY => "DownloadTextKey";
    public static string R_ADD_BULK_TEXT_KEY => "AddBulkTextKey";
    public static string R_BULK_UPLOAD_TEXT_KEY => "BulkUploadTextKey";
    public static string R_BULK_UPLOAD_DATA_ENTRY_STATUS_KEY => "BulkUploadDataEntryStatusKey";
    public static string R_DOWNLOAD_EXCEL_SAMPLE_TEXT_KEY => "DownloadExcelSampleTextKey";
    public static string R_UPLOAD_EXCEL_HEADER_TEXT_KEY => "UploadExcelHeaderTextKey";
    public static string R_Report_From_Date_Key => "ReportFromDateKey";
    public static string R_Report_TO_Date_Key => "ReportToDateKey";
    public static string R_COMMON_BY_Key => "ByKey";
    public const string R_ADD_RECORD_KEY = "AddRecordKey";
    public const string R_MENU_HEADER_KEY = "MenuHeaderKey";
    public const string R_SETTING_HEADER_KEY = "SettingHeaderKey";
    #endregion

    #region SampleCodeGroup
    //todo: to remove this group data from db and constants
    public static string R_EMPLOY_NAME_KEY => "EmployNameKey";
    public static string R_EMPLOY_DOB_KEY => "EmployDobKey";
    public static string R_EMPLOY_AGE_KEY => "EmployAgeKey";
    public static string R_EMPLOY_EMAIL_ID_KEY => "EmployEmailIdKey";
    public static string R_EMPLOY_PHONE_NUMBER_KEY => "EmployPhoneNumberKey";
    public static string R_EMPLOY_CLASS_KEY => "EmployClassKey";
    public static string R_EMPLOY_NOTES_KEY => "EmployNotesKey";
    public static string R_EMPLOY_PHOTO_KEY => "EmployPhotoKey";

    #endregion

    #region RSMenuGroup

    public const string R_NODE_NAME_KEY = "NodeNameKey";

    public const string R_NODE_TARGET_KEY = "NodeTargetKey";

    public static string R_NODE_LEFT_HEADER_KEY => "NodeLeftHeaderKey";
    public static string R_RENDER_TYPE_KEY => "RenderTypeKey";

    public const string R_NODES_KEY = "NodesKey";

    public const string R_CATEGORY_KEY = "SelectCategoryKey";
    public static string R_MENU_TYPE_KEY => "MenuTypekey";
    public static string R_NODE_RIGHT_HEADER_KEY => "NodeRightHeaderKey";

    public const string R_MENU_ACTION_KEY = "MenuActionKey";

    public const string R_MENU_NODE_KEY = "MenuNodeKey";

    public const string R_TITLE_KEY = "TitleKey";

    public const string R_PAGE_TYPE_KEY = "PageTypeKey";

    public const string R_TAGS_KEY = "TagsKey";
    public static string R_STATUS_KEY => "StatusKey";
    public const string R_EDITOR_KEY = "EditorKey";
    public static string R_CONTENT_KEY => "ContentKey";
    public const string R_LINKS_KEY = "LinksKey";
    public const string R_PDF_KEY = "PdfKey";
    public static string R_BOTH_KEY => "BothKey";

    public const string R_SELECT_LINKS_KEY = "SelectLinksKey";

    public const string R_SCROLL_TO_PAGE_KEY = "ScrollToPageKey";

    public const string R_MENU_NODES_KEY = "MenuNodesKey";

    public const string R_DISPLAY_TYPE_KEY = "DisplayTypeKey";

    public const string R_MENU_LOCATION_KEY = "MenuLocationKey";
    public static string R_LINKS_COUNT_KEY => "LinksCountKey";

    public const string R_SECTION_TITLE_KEY = "SectionTitleKey";

    public const string R_PAGES_KEY = "PagesKey";

    public const string R_IDENTIFIER_KEY = "IdentifierKey";
    public static string R_MORE_OPTIONS_MENU_KEY => "MoreOptionMenuKey";

    public const string R_EDUCATION_CATEGORY_IMAGE_KEY = "ImageNameKey";

    public const string R_EDUCATION_CATEGORY_NAME_KEY = "CategoryNameKey";

    public const string R_EDUCATION_CATEGORY_DESCRIPTION_KEY = "DescriptionKey";

    public const string R_FILE_CATEGORY_IMAGE_KEY = "FileCategoryImageNameKey";

    public const string R_FILE_CATEGORY_NAME_KEY = "FileCategoryNameKey";

    public const string R_FILE_CATEGORY_DESCRIPTION_KEY = "FileCategoryDescriptionKey";

    #endregion

    #region RSRenderTypeGroup
    public static string R_SHOW_ICON_KEY => "ShowIconKey";
    public static string R_SHOW_LABEL_KEY => "ShowLabelKey";
    public static string R_SHOW_BOTH_KEY => "ShowBothKey";
    #endregion

    #region RSMenuActionGroup
    public static string R_MOBILE_MENU_ACTION_ADD_KEY => "MobileMenuActionAddkey";
    public static string R_MOBILE_MENU_ACTION_SAVE_KEY => "MobileMenuActionSavekey";
    public static string R_MOBILE_MENU_ACTION_BACK_KEY => "MobileMenuActionBackkey";
    public static string R_MOBILE_MENU_ACTION_CLOSE_KEY => "MobileMenuActionClosekey";
    public static string R_MOBILE_MENU_ACTION_PROFILE_KEY => "MobileMenuActionProfilekey";
    #endregion

    #region RSOrganisationProfileGroup
    public static string R_DEPARTMENT_NAME_KEY => "DepartmentNameKey";
    public const string R_DEPARTMENT_NAME_TEXT_KEY = "DepartmentNameTextKey";
    public static string R_UPLOAD_LOGO_KEY => "UploadLogoKey";
    public static string R_LOGO_KEY => "LogoKey";
    public const string R_ORGANISATION_NAME_KEY = "OrganisationNameKey";
    public const string R_DOMAIN_KEY = "DomainKey";
    public const string R_TAX_NUMBER_KEY = "TaxNumberKey";
    public static string R_DEFAULT_LANGUAGE_KEY => "DefaultLanguageKey";
    public static string R_SELECT_LANGUAGES_KEY => "SelectLanguagesKey";
    public static string R_SELECT_SINGLE_LANGUAGE_KEY => "SelectLanguageKey";

    public const string R_BRANCH_NAME_KEY = "BranchNameKey";
    public static string R_DEPARTEMENTS_TEXT_KEY => "DepartementsTextKey";
    public static string R_PROFESSION_NAME_TEXT_KEY => "ProfessionNameTextKey";
    public static string R_PROFESSION_TEXT_KEY => "ProfessionsTextKey";
    public static string R_DEPARTMENT_SELECTION_ERROR_KEY => "DepartmentSelectionErrorKey";
    public static string R_BRANCH_ERROR_KEY => "BranchErrorKey";
    public static string R_REASON_TEXT_KEY => "ReasonTextKey";
    public const string R_SELECT_REASON_KEY = "SelectReasonKey";
    public const string R_REASON_NAME_TEXT_KEY = "ReasonNameTextKey";
    public static string R_DESCRIPTION_NAME_TEXT_KEY => "DescriptionNameTextKey";
    public static string DESCRIPTION_TEXT_KEY => "DescriptionTextKey";
    public static string R_REGISTERED_ON_KEY => "RegisteredOnKey";
    public static string R_NUMBER_OF_EMPLOYEES_KEY => "NumberOfEmployeesKey";
    public static string R_NUMBER_OF_PATIENTS_KEY => "NumberOfPatientsKey";
    public static string R_SELECT_PAYMENT_PLAN_KEY => "SelectPaymentPlanKey";
    public static string R_SELECT_SUBSCRIPTION_PLAN_KEY => "SelectSubscriptionPlanKey";
    public const string R_SELECT_SERVICE_KEY = "SelectServiceKey";
    #endregion

    #region RSLoginFlowPagesGroup
    public static string R_USER_NAME_KEY => "UserNameKey";
    public static string R_PASSWORD_KEY => "PasswordKey";
    public static string R_REMEMBER_ME_KEY => "RememberMeKey";
    public static string R_FORGOT_PASSWORD_ACTION_KEY => "ForgotPasswordActionKey";
    public static string R_MOBILE_NUMBER_KEY => "MobileNumberKey";
    public static string R_ALREADY_HAVE_CODE_ACTION_KEY => "AlreadyHaveCodeActionKey";
    public static string R_ALREADY_HAVE_LOGIN_ACTION_KEY => "AlreadyHaveLoginActionKey";
    public static string R_NEW_PASSWORD_KEY => "NewPasswordKey";
    public static string R_CONFIRM_PASSWORD_KEY => "ConfirmPasswordKey";
    public const string R_FIRST_NAME_KEY = "FirstNameKey";
    public const string R_MIDDLE_NAME_KEY = "MiddleNameKey";
    public const string R_LAST_NAME_KEY = "LastNameKey";
    public static string R_EMAIL_ADDRESS_KEY => "EmailAddressKey";
    public static string R_LOGIN_ACTION_KEY => "LoginActionKey";
    public static string R_REGISTER_ACTION_KEY => "RegisterActionKey";
    public static string R_WEAK_PINCODE_KEY => "WeakPincodeKey";
    public static string R_PASSWORD_MISMATCH_ERROR_KEY => "PasswordMismatchErrorKey";
    public static string R_VERIFY_ACTION_ERROR_KEY => "VerifyActionKey";
    public static string R_CREATE_ACCOUNT_TEXT_KEY => "CreateAccountTextKey";
    public static string R_FORGOT_PASSWORD_PAGE_HEADER_KEY => "ForgotPasswordPageHeaderKey";
    public static string R_RESET_PASSWORD_PAGE_HEADER_KEY => "ResetPasswordPageHeaderKey";
    public static string R_SET_PASSWORD_PAGE_HEADER_KEY => "SetPasswordPageHeaderKey";
    public static string R_CHANGE_PASSWORD_PAGE_HEADER_KEY => "ChangePasswordPageHeaderKey";
    public static string R_CREATE_ACCOUNT_KEY => "CreateAccountKey";
    public static string R_REGISTER_MORE_KEY => "RegisterMoreDetailsKey";
    public static string R_PROFILE_INFO_KEY => "ProfileInfoTextKey";
    public static string R_PROFILE_EDIT_KEY => "ProfileEditTextKey";
    public static string R_DEGREE_KEY => "UserDegreeKey";
    public static string R_LOGINPAGE_TERMS_OFUSE_SENTENCE_KEY => "LoginPageTermsOfUseSentenceKey";
    public static string R_LOGINPAGE_PRIVACY_SENTENCE_KEY => "LoginPagePrivacySentenceKey";
    public static string R_CHANGE_PASSWORD_KEY => "ChangePasswordKey";

    #endregion

    #region RSContactPageGroup
    public const string R_CONTACT_TYPE_KEY = "ContactTypeKey";

    public const string R_CONTACT_KEY = "ContactKey";
    public static string R_CONTACT_DETAIL_KEY => "ContactDetailKey";
    #endregion

    #region RSContactGroup
    public const string R_ADDRESS_CONTACT_KEY = "AddressContactKey";

    public const string R_EMAIL_CONTACT_KEY = "EmailContactKey";

    public const string R_PHONE_CONTACT_KEY = "PhoneContactKey";
    #endregion

    #region RSContactTypeGroup
    public static string R_PRIMARY_CONTACT_TYPE_KEY => "PrimaryContactTypeKey";
    public static string R_HOME_CONTACT_TYPE_KEY => "HomeContactTypeKey";
    public static string R_WORK_CONTACT_TYPE_KEY => "WorkContactTypeKey";
    #endregion

    #region RSGenderGroup
    public static string R_NEUTRAL_KEY => "NeutralKey";
    public static string R_MALE_KEY => "MaleKey";
    public static string R_FEMALE_KEY => "FemaleKey";
    #endregion

    #region RSYesNoTypeGroup
    public static string R_YES_ACTION_KEY => "YesActionKey";
    public static string R_NO_ACTION_KEY => "NoActionKey";
    #endregion

    #region RSOrganisationReadings
    public static string R_ABSOLUTE_MIN_VALUE_KEY => "AbsoluteMinValueKey";
    public static string R_ABSOLUTE_MAX_VALUE_KEY => "AbsoluteMaxValueKey";
    public static string R_ABSOLUTE_BAND_COLOR_KEY => "AbsoluteBandColorKey";
    public static string R_IDEAL_MIN_VALUE_KEY => "IdealMinValueKey";
    public static string R_IDEAL_MAX_VALUE_KEY => "IdealMaxValueKey";
    public static string R_IDEAL_BAND_COLOR_KEY => "IdealBandColorKey";
    public static string R_TARGET_BAND_COLOR_KEY => "TargetBandColorKey";
    public const string R_GENDER_KEY = "GenderKey";
    public static string R_GENDER_TYPE_KEY => "GenderTypeKey";
    public static string R_AGE_GROUPS_KEY => "AgeGroupsKey";
    public static string R_FROM_AGE_KEY => "FromAgeKey";
    public static string R_TO_AGE_KEY => "ToAgeKey";
    public static string R_VALUE_CAN_BE_ADDED_BY_KEY => "ValuesCanBeAddedByKey";
    public static string R_ALLOW_MANUAL_READING_KEY => "AllowManualReadingKey";
    public static string R_ALLOW_HEALTH_KIT_DATA_KEY => "AllowHealthKitDataKey";
    public static string R_SHOW_IN_GRAPH_KEY => "ShowInGraphKey";
    public static string R_SHOW_IN_DATA_KEY => "ShowInDataKey";
    public static string R_ALLOW_DEVICE_DATA_KEY => "AllowDeviceDataKey";
    public static string R_CAN_BE_DELETED_KEY => "CanBeDeletedKey";
    public static string R_SHOW_IN_DIFFERENT_LINES_KEY => "ShowInDifferentLines";
    public static string R_READING_CODE_KEY => "ReadingCodeKey";
    public static string R_READING_FREQUENCY_KEY => "ReadingFrequencyKey";
    public static string R_READING_NAME_KEY => "ObservationNameKey";
    public static string R_SUPPORTED_OBSERVATION_MESSAGE_KEY => "SupportedObservationMessageKey";
    public static string R_SUPPORTED_OBSERVATION_ERROR_KEY => "SupportedObservationErrorKey";
    public static string R_SUPPORTED_DEVICES_KEY => "SupportedDevicesKey";

    /// <summary>
    /// Toggle Confirmation Popup Text Key
    /// </summary>
    public static string R_TOGGLE_CONFIRMATION_POPUP_TEXT_KEY => "ToggleConfirmationPopupTextKey";

    #endregion

    #region RSReadingCategoryGroup

    /// <summary>
    /// Resource key id for Food reading  category
    /// </summary>
    public static short R_FOOD_KEY_ID => 423;

    #endregion

    #region RSReadingFiltersGroup

    /// <summary>
    /// MonthTextKey
    /// </summary>
    public static string R_ALL_FILTER_KEY => "AllFilterKey";

    /// <summary>
    /// Resource key id for AllFilterKey 
    /// </summary>
    public const short R_ALL_FILTER_KEY_ID = 475;
    public const short R_YEAR_FILTER_KEY_ID = 476;
    public const short R_QUARTER_FILTER_KEY_ID = 477;
    public const short R_MONTH_FILTER_KEY_ID = 478;
    public const short R_WEEK_FILTER_KEY_ID = 479;
    public const short R_DAY_FILTER_KEY_ID = 480;

    public const string R_NEXT_KEY = "Next";

    #endregion

    #region RSGenderTypeGroup

    /// <summary>
    /// Resource key id for NeutralKey in RSGenderTypeGroup
    /// </summary>
    public static short R_GENDER_TYPE_NEUTRAL_KEY_ID => 195;

    #endregion

    #region RSAgeTypeGroup

    /// <summary>
    /// Resource key id for NeutralKey in RSAgeTypeGroup
    /// </summary>
    public static short R_AGE_TYPE_NEUTRAL_KEY_ID => 198;

    /// <summary>
    /// Resource key id for AgeRangeKey in RSAgeTypeGroup
    /// </summary>
    public static short R_AGE_TYPE_AGE_RANGE_KEY_ID => 199;

    /// <summary>
    /// Resource key for age range
    /// </summary>
    public static string R_AGE_RANGE_KEY => "AgeRangeKey";

    #endregion

    #region RSReadingsGroup

    /// <summary>
    /// Resource key id for BloodPressureKey in RSReadingsGroup
    /// </summary>
    public static short R_BLOOD_PRESSURE_KEY_ID => 656;

    /// <summary>
    /// Resource key id for BPSystolicKey in RSReadingsGroup
    /// </summary>
    public static short R_BP_SYSTOLIC_KEY_ID => 657;

    /// <summary>
    /// Resource key id for BPDiastolicKey in RSReadingsGroup
    /// </summary>
    public static short R_BP_DIASTOLIC_KEY_ID => 658;

    /// <summary>
    /// Reading type as blood pressure systolic
    /// </summary>
    public static string READING_BP_SYSTOLIC => "BPSystolic";

    /// <summary>
    /// Reading type as blood pressure diastolic
    /// </summary>
    public static string READING_BP_DIASTOLIC => "BPDiastolic";

    /// <summary>
    /// Resource key id for BloodGlucoseKey 
    /// </summary>
    public static short R_BLOOD_GLUCOSE_KEY_ID => 659;

    /// <summary>
    /// Resource key id for InsulinKey 
    /// </summary>
    public static short R_INSULIN_KEY_ID => 669;

    /// <summary>
    /// Food search result key
    /// </summary>
    public static string R_NUTRITION_KEY => "NutritionKey";

    /// <summary>
    /// Resource key id for NutritionKey in RSReadingsGroup
    /// </summary>
    public static short R_NUTRITION_KEY_ID => 553;

    /// <summary>
    /// Resource key id for WeightKey in RSReadingsGroup
    /// </summary>
    public static short R_WEIGHT_KEY_ID => 654;

    /// <summary>
    /// Resource key id for HeightKey in RSReadingsGroup
    /// </summary>
    public static short R_HEIGHT_KEY_ID => 655;

    /// <summary>
    /// Resource key id for BMIKey in RSReadingsGroup
    /// </summary>
    public static short R_BMI_KEY_ID => 676;

    #endregion

    #region RSProfileGroup
    
    public const string R_ADDITIONAL_PROFILE_DATA_KEY = "AdditionalProfileDataKey";
    public const string R_PROFILE_IMAGE_KEY = "ProfileImageKey";
    public const string R_DATE_OF_BIRTH_KEY = "DateOfBirthKey";
    public const string R_DATE_OF_JOINING_KEY = "DateOfJoining";
    public const string R_AGE_KEY = "AgeKey";
    public const string R_BLOOD_GROUP_KEY = "BloodGroupKey";
    public const string R_PREFERRED_LANGUAGE_KEY = "PreferredLanguageKey";
    public const string R_SOCIAL_SECURITY_NUMBER_KEY = "SocialSecurityNumberKey";
    public const string R_EXTERNAL_CODE_KEY = "ExternalCodeKey";
    public const string R_INTERNAL_CODE_KEY = "InternalCodeKey";
    public static string R_PROFILE_HEADER_KEY => "ProfileHeaderKey";
    public static string R_SEND_ACTIVATION_AGAIN_KEY => "SendActivationAgainKey";
    public static string R_HEIGHT_KEY => "HeightKey";
    public static string R_WEIGHT_KEY => "WeightKey";
    public static string R_EMPTY_PATIENT_VIEW_KEY => "EmptyPatientViewKey";
    public static string R_PROGRAM_TITLE_KEY => "ProgramTitleKey";

    public static string R_CRITICAL_STATUS_KEY => "CriticalStatusKey";
    public static string R_PROGRAM_TITLE_ERROR_KEY => "ProgramTitleErrorKey";
    public static string R_ENROLLED_ON_TEXT_KEY => "EnrolledOnTextKey";
    public static string R_PROGRAM_INSTRUCTION_MESSAGE_KEY => "ProgramInstructionMessageKey";
    public static string R_CREATE_PROGRAMS_ERROR_KEY => "CreateProgramsErrorKey";

    /// <summary>
    /// Resource key id for FullNameKey
    /// </summary>
    public static string R_FULL_NAME_KEY => "FullNameKey";
    #endregion

    #region RSBloodTypeGroup

    public static string R_A_POSITIVE_KEY => "APositiveKey";
    public static string R_A_NEGATIVE_KEY => "ANegativeKey";
    public static string R_B_POSITIVE_KEY => "BPositiveKey";
    public static string R_B_NEGATIVE_KEY => "BNegativeKey";
    public static string R_AB_POSITIVE_KEY => "ABPositiveKey";
    public static string R_AB_NEGATIVE_KEY => "ABNegativeKey";
    public static string R_O_POSITIVE_KEY => "OPositiveKey";
    public static string R_O_NEGATIVE_KEY => "ONegativeKey";

    #endregion

    #region QuestionnaireGroup
    public const string R_NEXT_ACTION_KEY1 = "NextActionKey";
    public static string R_QUSETIONS_TEXT_KEY => "QusetionsTextKey";
    public static string R_SUBSCALES_TEXT_KEY => "SubscalesTextKey";
    public static string R_QUESTIONNAIRE_TYPE_KEY => "QuestionnaireTypeKey";
    public static string R_CODE_TEXT_KEY => "CodeTextKey";
    public static string R_DEFAULT_RESPONDANT_KEY => "DefaultRespondantKey";
    public static string R_QUESTIONNAIRE_TITLE_TEXT_KEY => "QuestionnaireTitleTextKey";
    public static string R_INSTRUCTIONS_TEXT_KEY => "InstructionsTextKey";
    public static string R_QUESTION_INSTRUCTIONS_TEXT_KEY => "QuestionInstructionsTextKey";
    public static string R_READ_ONLY_QUESTION_CONTENT_KEY => "ReadOnlyQuestionContentKey";
    public static string R_ANSWER_PLACEHOLDER_TEXT_KEY => "AnswerPlaceHolderTextKey";
    public static string R_Standard_KEY => "StandardKey";
    public static string R_GROUPED_KEY => "GroupedKey";

    public const string R_MIN_TEXT_KEY = "MinKey";

    public const string R_MAX_TEXT_KEY = "MaxKey";
    public const string R_RECOMMENDATION_TEXT_KEY = "RecommendationKey";
    public const string R_DESCRIPTION_TEXT_KEY = "DescriptionKey";
    public static string R_SCORETYPE_TEXT_KEY => "ScoreTypeKey";
    public static string R_BASIC_HEADER_TEXT_KEY => "BasicInptHeaderKey";
    public static string R_SCORE_HEADER_TEXT_KEY => "ScoreHeaderKey";
    public const string R_QUESTION_TYPE_KEY = "QuestionTypeKey";
    public static string R_IS_STARTING_QUESTION_KEY => "IsStartingQuestionKey";
    public const string R_QUESTION_KEY = "QuestionKey";
    public static string R_ANSWER_KEY => "AnswerKey";
    public static string R_VALUE_KEY => "ValueKey";
    public static string R_SETUP_ANSWERS_KEY => "SetupAnswersKey";
    public static string R_ADD_ANSWER_KEY => "AddAnswerKey";

    public const string R_SLIDER_STEPS_KEY = "SliderStepsKey";
    public static string R_OVERLAP_RANGE_KEY => "OverlapRangeErrorKey";
    public static string R_PAGE_DESCRIPTION_KEY => "PageDescriptionKey";
    public static string R_QUESTIONNAIRE_DYNAMIC_START_KEY => "QuestionnaireDynamicStartKey";
    public static string R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY => "QuestionnaireDynamicResourceKey";
    public static string R_ADD_TO_MEDICAL_HISTORY_KEY => "AddToMedicalHistoryKey";
    public static string R_JUMP_TO_QUESTION_KEY => "JumpToQuestionKey";
    public static string R_VALUE_BLANK_CONDITION_KEY => "ValueBlankConditionKey";
    public static string R_ADD_MORE_CONDITION_KEY => "AddMoreConditionKey";
    public static string R_ACTION_KEY => "ActionKey";
    public static string R_ELSE_CONDITION_KEY => "ElseConditionKey";
    public static string R_CONDITION_KEY => "ConditionKey";
    public static string R_BETWEEN_VALUE_CONDITION_KEY => "BetweenValueConditionKey";
    public static string R_BETWEEN_AND_CONDITION_KEY => "BetweenAndConditionKey";
    public const string R_STARTING_QUESTION_SELECTION_KEY = "StartingQuestionSelectionKey";
    public static string R_CONDITION_VALUE_1_KEY => "ConditionValue1Key";
    public static string R_CONDITION_VALUE_2_KEY => "ConditionValue2Key";
    public static string R_NUMBER_OF_FLOWS_KEY => "NumberOfFlowsKey";
    public static string R_NUMBER_OF_SCORES_KEY => "NumberOfScoresKey";
    public static string R_VALUE2_GREATER_ERROR_KEY => "Value2GreaterErrorKey";
    public static string R_THEN_VALUE_KEY => "ThenValueKey";
    public static string R_SHOW_VALUE_TO_PATIENT => "ShowValueToPatientKey";

    public static string R_SELECT_MEASURMENT_KEY => "SelectMeasurmentKey";
    #endregion

    #region RSDashBoardPageGroup
    public const string R_FEATURE_NAME_KEY = "FeatureNameKey";
    public static string R_COLUMN_NAME_KEY => "ColumnNameKey";
    public static string R_PARAMETER_LIST_HEADER_KEY => "ParameterListHeaderKey";
    public static string R_PARAMETER_NAME_KEY => "ParameterNameKey";
    public static string R_PARAMETER_TYPE_NAME_KEY => "ParameterTypeNameKey";
    public static string R_PARAMETER_VALUE_NAME_KEY => "ParameterValueNameKey";
    #endregion

    #region RSAppointmentPageGroup
    public static string R_JOIN_TEXT_KEY => "JoinTextKey";
    public static string R_DECLINE_TEXT_KEY => "DeclineTextKey";
    public static string R_CANCEL_MEETING_TEXT_KEY => "CancelMeetingTextKey";

    public const string R_APPOINTMENT_TYPE_TEXT_KEY = "AppointmentTypeTextKey";
    public static string R_DECLINE_ERROR_APPOINTMENT_KEY => "DeclineErrorAppointmentKey";

    public const string R_STARTS_TEXT_KEY = "StartsTextKey";

    public const string R_ENDS_TEXT_KEY = "EndsTextKey";
    public static string R_PARTICIPANTS_TEXT_KEY => "ParticipantsTextKey";

    public const string R_APPOINTMENT_SUBJECT_TEXT_KEY = "AppointmentSubjectTextKey";

    public const string R_INFORMATION_TEXT_KEY = "InformationTextKey";
    public static string R_UPDATE_STATUS_TEXT_KEY => "UpdateStatusTextKey";
    public static string R_CAREGIVER_SELECTION_ERROR_MESSAGE_KEY => "CaregiverSelectionErrorMessageKey";
    public static string R_AVAILABLE_TEXT_KEY => "AvailableTextKey";
    public static string R_UNAVAILABLE_TEXT_KEY => "UnavailableTextKey";
    public static string R_CAREGIVER_AVAILABLE_ERROR_MESSAGE_KEY => "CaregiverAvailableErrorMessageKey";
    public static string R_VIDEO_APPOINTMENT_TYPE_KEY => "VideoAppointmentKey";
    public const long R_VIDEO_APPOINTMENT_TYPE_KEY_ID = 312;
    public static string R_ACCEPTED_STATUS_KEY => "AcceptedStatusKey";
    public static string R_CANCELLED_STATUS_KEY => "CancelledStatusKey";
    public static string R_REJECTED_STATUS_KEY => "RejctedStatusKey";
    public static string R_APPOINTMENT_NOTIFICATION_TITLE_KEY => "AppointmentNotificationHeader";
    public static string R_APPOINTMENT_NOTIFICATION_DESCRIPTION_KEY => "AppointmentNotificationDescription";
    public static string R_APPOINTMENT_INVITE_EXTERNAL_KEY => "AppointmentInviteExternalKey";
    public static string R_NAME_KEY => "NameKey";

    public const string R_NAME_TXT_KEY = "NameKey";
    public static string R_APPOINTMENT_EMAIL_KEY => "EmailKey";
    public static string R_PHONE_NUMBER_KEY => "PhoneNumberKey";



    //group id 38
    #endregion

    #region RSChatsGroup

    public static string R_DELETED_ITEM_KEY => "DeletedItemKey";
    public static string R_RELATION_EXPIRED_KEY => "RelationshipExpiredKey";
    public static string R_TODAY_TEXT_KEY => "TodayTextKey";
    public static string R_CHAT_ATTACHMENT_TEXT_KEY => "ChatAttachmentTextKey";
    public static string R_DISABLE_CHAT_TEXT_KEY => "DisableChatTextKey";
    #endregion

    #region RSCaregiverGroup
    public const string R_CAREGIVER_KEY = "CaregiverKey";
    public const string R_START_DATE_KEY = "StartDateKey";
    public const string R_END_DATE_KEY = "EndDateKey";
    public static string R_FROM_DATE_KEY => "FromDateKey";
    public static string R_TO_DATE_KEY => "ToDateKey";
    public static string R_NAME_TEXT_KEY => "NameTextKey";
    public static string R_POSITION_KEY => "PositionKey";
    #endregion

    #region RSTaskStatusGroup
    public const string R_ASSIGNED_LABEL_KEY = "AssignedLabelKey";
    public const string R_INPROGRESS_STATUS_KEY = "InProgressStatusKey";
    public const string R_NEW_STATUS_KEY = "NewStatusKey";
    public const string R_MISSED_STATUS_KEY = "MissedStatusKey";
    public const string R_COMPLETED_STATUS_KEY = "CompletedStatusKey";

    #endregion

    #region RSContentTypeGroup
    public static string R_ASSIGN_BUTTON_TEXT_KEY => "AssignButtonTextKey";
    public static string R_SELECT_TYPE_LABEL_KEY => "SelectTypeLabelKey";
    public const string R_SELECT_EDUCATION_LABEL_KEY = "SelectEducationLabelKey";
    public static string R_READ_BUTTON_TEXT_KEY => "ReadButtonTextKey";
    #endregion

    #region RSOperationTypeGroup

    public const string R_GREATER_THAN_KEY = "GreaterThanKey";
    public const string R_GREATER_THAN_EQUAL_TO_KEY = "GreaterThanEqualToKey";
    public const string R_LESS_THAN_KEY = "LessThanKey";
    public const string R_LESS_THAN_EQUAL_TO_KEY = "LessThanEqualToKey";
    public const string R_EQUAL_TO_KEY = "EqualToKey";
    public const string R_BETWEEN_KEY = "BetweenKey";

    #endregion

    #region RSTaskTypeGroup
    public static string R_EDUCATION_KEY => "EducationKey";
    public static string R_MEASUREMENT_KEY => "MeasurementKey";
    public static string R_MEASUREMENT_UNITS_KEY => "MeasurementUnitsKey";
    public static string R_QUESTIONNAIRE_KEY => "QuestionnaireKey";
    public static string R_NOTIFICATION_KEY => "NotificationKey";
    public static string R_EMAIL_KEY => "EmailKey";
    public static string R_SMS_KEY => "SMSKey";
    public static string R_INSTRUCTION_KEY => "InstructionKey";
    #endregion

    #region RSProgramsGroup
    public static string R_CONFIGURE_READINGS => "ConfigureReadingKey";
    public static string R_CONFIGURE_RANGES => "ConfigureRangesKey";

    public const string R_PROGRAM_NAME_KEY = "ProgramNameKey";
    public static string R_PROGRAM_TYPE_NAME_KEY => "ProgramTypeNameKey";

    public const string R_PROGRAM_DESCRIPTION_KEY = "ProgramDescriptionKey";

    public const string R_INSTRUCTION_NAME_KEY = "InstructionNameKey";
    public static string R_TASK_NAME_KEY => "TaskNameKey";
    public static string R_TASK_TYPE_NAME_KEY => "TaskTypeNameKey";

    public const string R_SUBFLOW_NAME_KEY = "SubFlowNameKey";
    public static string R_SELECT_SUBFLOW_KEY => "SelectSubFlowKey";

    public const string R_INSTRUCTION_DESCRIPTION_KEY = "InstructionDescriptionKey";

    public const string R_SUBFLOW_DESCRIPTION_KEY = "SubFlowDescriptionKey";
    public static string R_TASK_DESCRIPTION_KEY => "TaskDescriptionKey";

    public const string R_FROM_KEY = "FromKey";

    public const string R_TO_KEY = "ToKey";

    public const string R_OPERATION_TYPE_KEY = "OperationTypeKey";

    public const string R_TASK_TYPE_KEY = "TaskTypeKey";

    public const string R_ITEM_KEY = "ItemKey";
    public static string R_FEATURE_LABEL_KEY => "FeaturelabelKey";
    public static string R_OPERATION_LABEL_KEY => "OperationLabelKey";
    public static string R_REASON_REQUIRED_LABEL_KEY => "ReasonRequiredLabelKey";
    public static string R_THUMB_SIGNATURE_LABEL_KEY => "ThumbSignatureLabelKey";

    public static string R_TIME_BOUNDED_PROGRAM_KEY => "TimeBoundKey";
    public static string R_SELECT_ROLE_KEY => "SelectRoleKey";
    public static string R_ALTERNATE_FLOW_KEY => "AlternateFlowKey";
    public const string R_ASSIGN_AFTER_DAYS_KEY = "AssignAfterDaysKey";
    public const string R_SHOW_FOR_DAYS_KEY = "ShowForDaysKey";
    public static string R_PROGRAM_IDENTIFIER_KEY => "ProgramIdentifierKey";
    public static string R_NUMBER_OF_SUBFLOWS_KEY => "NumberOfSubFlowsKey";
    public static string R_NUMBER_OF_CAREGIVERS_KEY => "NumberOfCaregiversKey";
    public static string R_NUMBER_OF_MEDICATIONS_KEY => "NumberOfMedicationsKey";
    public static string R_NUMBER_OF_READINGS_KEY => "NumberOfReadingKey";
    public static string R_NUMBER_OF_TASKS_KEY => "NumberOfTasksKey";
    public static string R_NUMBER_OF_EDUCATIONS_KEY => "NumberOfEducationKey";
    public static string R_EXECUTE_ON_LOGIN_KEY => "ExecuteOnLoginKey";
    public static string R_DEFAULT_CAREGIVER_KEY => "DefaultCaregiverKey";
    public static string R_READINGS_KEY => "ReadingsKey";
    public static string R_SELECT_READING_KEY => "SelectReadingKey";
    public static string R_SELECT_PROVIDER_KEY => "SelectProviderKey";
    public static string R_PROGRAM_MANDATORY_TASK_ERROR_KEY => "ProgramMandatoryTaskErrorKey";

    public const string R_DEFAULT_PROGRAM_INDENTIFIER_COLOR_KEY = "DefaultProgramIndentifierColor";
    public static string R_PROGRAM_SELECTION_MANDATORY_KEY => "ProgramSelectionMandatoryKey";
    public const string R_FOR_PROVIDERS_KEY = "ForProvidersKey";
    public static string R_ALLOW_SELF_REGISTRATION_KEY => "AllowSelfSubscriptionKey";
    public static string R_ENTRY_POINT_KEY => "EntryPointKey";
    public static string R_SUBSCRIBE_PROGRAM_KEY => "SubscribeProgramActionKey";
    public static string R_SAVE_POPUP_WARNING_KEY => "SavePopupWarningKey";
    public static string R_PROGRAM_TYPE_KEY => "ProgramTypeKey";

    public const string R_SUPPORTED_CODE_SYSTEM_KEY = "SupportedCodeSystemKey";

    public const string R_PROGRAM_DURATION_TYPE_KEY = "ProgramDurationTypeKey";

    public const string R_PROGRAM_DURATION_KEY = "ProgramDurationKey";

    public static string R_OPEN_ENDED_KEY => "OpenEndedKey";

    public const string R_NOTE_TYPE_KEY = "NoteTypeKey";
    public const string R_NOTE_DESCRIPTION_KEY = "NoteDescriptionKey";
    public static string R_ALLOW_PROVIDER_TO_SCAN_KEY => "AllowProviderToScanKey";
    public static string R_ALLOW_PATIENT_TO_SCAN_KEY => "AllowPatientToScanKey";
    public static string R_ALLOW_PATIENTS_TO_BUY_CREDITS_DIRECTLY_KEY => "AllowPatientsToBuyCreditsDirectlyKey";
    public static string R_ALLOW_PROGRAM_TO_BUY_CREDICTS_DIRECTLY_KEY => "AllowProgramToBuyCreditsDirectlyKey";

    #endregion

    #region RSPatientDocumnetGroup

    public const string R_SELECT_File_CATEGORY_KEY = "SelectFileCategoryKey";
    public const string R_FILE_DESCRIPTION_KEY = "FileDescriptionKey";
    public const string R_DOCUMENT_TEXT_KEY = "DocumentTextKey";
    public const string R_FILES_TEXT_KEY = "FilesTextKey";
    public static string R_DOCUMENT_FILE_UPLOADEDBY_TEXT_KEY => "DocumentUploadedByTextKey";
    public static string R_DOCUMENT_FILE_YOU_TEXT_KEY => "DocumentFileYouTextKey";
    public static string R_UPLOADED_AT_TEXT_KEY => "UploadedAtTextKey";
    public static string R_UPDATED_ON_TEXT_KEY => "UpdatedOnTextKey";
    public static string R_DOCUMENT_FILE_EMPTY_VIEW_KEY => "EmptyFileDocumentViewKey";
    public static string R_DOCUMENT_CAPTION_KEY => "DocumentCaption/TitleKey";
    public static string R_DOCUMENT_DOWNLOAD_ERROR_KEY => "DocumentDownloadErrorKey";
    public static string R_FILE_ATTACHMENT_ERROR_MESSAGE_TEXT_KEY => "FileAttachmentErrorMessageTextKey"; // need to add 
    public static string R_UPLOAD_ONLY_ONE_DOCUMENT_KEY => "UploadOnlyOneDoumentKey";
    public static string R_NUMBER_OF_FILES_KEY => "NoOfFilesKey";

    #endregion

    #region RSConsentGroup
    public const string R_CONSENT_PAGE_KEY = "ConsentPageKey";

    public const string R_CONSENT_ROLE_KEY = "ConsentRoleKey";

    public const string R_CONSENT_PLATFORM_KEY = "ConsentPlatformKey";
    public static string R_INFORM_CONSENT => "InformedConsentKey";
    public static string R_ANDROID_PLATFORM_KEY => "AndroidPlatformKey";
    public static string R_IOS_PLATFORM_KEY => "IosPlatformKey";
    public static string R_WEB_PLATFORM_KEY => "WebPlatformKey";
    public static string R_INFORMED_CONSENT_INSTRUCTIONS_LABEL_KEY => "InformedConsentInstructionLabelKey";
    public static string R_ACCEPT_REQUIRED_CONSENT_ERROR_MSG_KEY => "AcceptedRequiredErrorMSGKey";
    #endregion

    #region RSTasksGroup
    public static string R_START_END_DATE_KEY => "StartEndDateKey";
    public static string R_SCORE_KEY => "ScoreKey";
    public static string R_COMPLETION_DATE_KEY => "CompletionDateKey";
    public const string R_OPEN_TASK_KEY = "OpenTaskKey";
    public static string R_HISTORY_TASK_KEY => "HistoryTaskKey";
    public static string R_DUE_DATE_KEY => "DueDateKey";
    public static string R_TASK_COMPLETED_MESSAGE_KEY => "TaskCompletedMessageKey";
    public static string R_QUESTIONNAIRE_MANDATORY_ANSWER_ERROR_KEY => "MandatoryAnswerErrorKey";
    public static string R_CANNOT_CONTINUE_TASK_MESSAGE_KEY => "CannotContinueTaskMessageKey";
    public static string R_FINISH_TASK_POP_UP_KEY => "FinishTaskPopUpKey";
    #endregion

    #region RSDevicesGroup



    public static string R_NEW_DEVICE_TEXT_KEY => "NewDeviceTextKey";
    public static string R_CONNECTED_DEVICE_TEXT_KEY => "ConnectedDeviceTextKey";
    public static string R_DEVICE_LIST_HEADER_TEXT_KEY => "DeviceListHeaderTextKey";
    public static string R_START_PAIRING_TEXT_KEY => "StartPairingTextKey";
    public static string R_ADD_DEVICES_TEXT_KEY => "AddDevicesTextKey";
    public static string R_SEARCHING_NEAR_BY_DEVICES_TEXT_KEY => "SearchingNearByDevicesTextKey";
    public static string R_REMOVE_DEVICE_BUTTON_TEXT_KEY => "RemoveDeviceButtonTextKey";
    public static string R_DEVICE_DONE_BUTTON_TEXT_KEY => "DeviceDoneButtonTextKey";
    public static string R_SUCCESS_TEXT_KEY => "SuccessTextKey";
    public static string R_PAIRING_DEVICE_TEXT_KEY => "PairingDeviceTextKey";
    public static string R_DEVICE_NEW_ACTIVITY_TEXT_KEY => "DeviceNewActivityTextKey";
    public static string R_PAIRING_FAILED_ERROR_KEY => "PairingFailedErrorKey";
    public static string R_DEVICE_INFO_KEY => "DeviceInfoKey";
    #endregion

    #region RSMedicationGroup

    public static string R_MEDICATION_HEADER_KEY => "MedicationHeaderKey";
    public const string R_MEDICINE_NAME_KEY = "MedicineNameKey";
    public const string R_DOSES_KEY = "DosesKey";
    public const string R_UNIT_KEY = "UnitKey";
    public const string R_FREQUENCY_KEY = "FrequencyKey";
    public const string R_HOW_OFTEN_KEY = "HowOftenKey";
    public const string R_ALTERNATE_FOR_TEXT_KEY = "AlternateForTextKey";
    public static string R_SET_REMIDERS_KEY => "SetRemidersKey";
    public static string R_DOSE_REMINDER_KEY => "DoseReminderKey";
    public static string R_DAILY_KEY => "DailyKey";
    public static string R_WEEKLY_KEY => "WeeklyKey";
    public static string R_MONTHLY_KEY => "MonthlyKey";
    public static string R_ALTERNATE_FOR_KEY => "AlternateForKey";
    public static string R_REMINDER_TIME_ERROR_KEY => "ReminderTimeErrorKey";
    public static string R_SEARCH_MEDICINE_TEXT_KEY => "SearchMedicineTextKey";
    public static string R_MEDICATION_NOTIFICATION_TITLE_KEY => "MedicationNotificationTitleKey";
    public static string R_SNOOZE_TEXT_KEY => "SnoozeTextKey";
    public static string R_IGNORE_TEXT_KEY => "IgnoreTextKey";
    public static string R_DONE_TEXT_KEY => "DoneTextKey";
    public const string R_MEDICATION_START_DATE_KEY = "MedicationStartDateKey";
    public const string R_MEDICATION_END_DATE_KEY = "MedicationEndDateKey";
    public static string R_VIEW_PRESCRIPTION_KEY => "ViewPrescriptionKey";
    public static string R_MEDICATION_DURATION_KEY => "MedicineDurationKey";

    public const string R_MEDICATION_NOTE_KEY = "MedicationNoteKey";

    #endregion

    #region RSWelcomeScreenGroup
    public const string R_HEADER_TEXT_KEY = "HeaderTextKey";
    public const string R_BODY_TEXT_KEY = "BodyTextTextKey";
    public const string R_UPLOAD_IMAGE_TEXT_KEY = "UploadImageTextKey";
    #endregion

    /// <summary>
    /// Offline operation instruction key
    /// </summary>
    public static string R_PROGRAM_SUBSCRIPTION_KEY => "SubscribedKey";
    public static string R_PROGRAM_SUBSCRIPTION_CONFIRM_KEY => "SubscribeConfirmKey";
    public static string R_SUBSCRIBE_ACTION_KEY => "SubscribeActionKey";

    #region ProgramTrackers

    public const string R_SELECT_TRACKER_TYPE_KEY = "SelectTrackerTypeKey";
    public static string R_PROGRAM_TRACKER_HEADER_TEXT_KEY => "TrackerKey";

    public const string R_TRACKER_TYPE_KEY = "TrackerTypeKey";

    public const string R_TRACKER_NAME_TEXT_KEY = "TrackerNameKey";

    public const string R_SELECT_TRACKER_NAME_TEXT_KEY = "SelectTrackerNameKey";
    public static string R_TRACKER_CURRENT_VALUE_KEY => "CurrentValueKey";
    public static string R_RANGES_NAME_TEXT_KEY => "RangesNameKey";
    public static string R_FROM_DAY_KEY => "FromDayKey";
    public static string R_FOR_DAYS_KEY => "ForDaysKey";
    public static string R_CONFIGURE_TRACKER_KEY => "ConfigureTrackerKey";
    public const string R_TRACKER_VALUE_CAN_BE_ADDED_BY_KEY = "TrackerValueCanBeAddedByKey";

    /// <summary>
    /// PatientTrackerInfo key
    /// </summary>
    public static string R_TRACKER_INFO_KEY => "TrackerInfoKey";

    public const string R_TRACKER_IDENTIFIER_KEY = "TrackerIdentifierKey";

    public static string R_TRACKER_DATE_KEY => "SelectTrackerDateKey";

    #endregion

    #region RSBillingGroup
    public const string R_BILLING_ITEM_NAME_KEY = "BillingItemNameKey";
    public static string R_PAYMENT_MODE_NAME_KEY => "PaymentModeNameKey";
    public const string R_SELECT_PAYMENT_MODE_KEY = "SelectPaymentModeKey";
    public static string R_BILLS_KEY => "BillsKey";
    public const string R_AMOUNT_PAID_KEY = "AmountPaidKey";
    public const string R_AMOUNT_KEY = "AmountKey";
    public static string R_TOTAL_AMOUNT_KEY => "TotalAmountKey";
    public const string R_PAYMENY_MODE_KEY = "PaymentModeKey";
    public static string R_GROSS_TOTAL_KEY => "GrossTotalKey";
    public static string R_BILL_DATE_KEY => "BillDateKey";
    public static string R_DISCOUNT_KEY => "DiscountKey";
    public static string R_PRINT_KEY => "PrintKey";
    public const string R_ENTER_DATE_KEY = "EnterDateKey";
    public static string R_BILLING_REPORTS_KEY => "BillingReportsKey";
    public const string R_SELECT_PROGRAM_KEY = "SelectProgramKey";
    public const string R_DOCTOR_NAME_KEY = "DoctorNameKey";
    public static string R_PROGRAM_NAMES_KEY => "ProgramNameKey";
    public static string R_SELECTPROGRAM_KEY => "SelectProgramKey";
    public const string R_BILL_DISCOUNT_KEY = "BillDiscountKey";
    public static string R_DELETE_ITEM_CONFIRMATION_KEY => "DeleteItemConfirmationKey";
    public static string R_AMOUNT_TO_BE_PAID_KEY => "AmountToBePaidKey";
    public static string R_MODE_OF_PAYMENT_KEY => "71";
    public const string R_SELECT_BILLING_PROGRAM_KEY = "SelectBillingProgramKey";

    #endregion

    #region RSPatientProviderNotes
    public static string R_DATE_AND_TIME_KEY => "DateAndTimeKey";
    public const string R_SELECT_PROVIDER_KEY1 = "SelectHealthCareProviderKey";
    public const string R_PROVIDER_NOTES_KEY = "ProviderNotesKey";
    public static string R_BY_KEY => "ByKey";
    public const string R_DATE_KEY = "DateKey";
    #endregion

    #region RSPatientProgramEndPointType
    public const string R_END_POINT_TYPE_KEY = "EndPointTypeKey";
    public const string R_PATIENT_PROGRAM_TRACKER_KEY = "PatientProgramTrackerKey";
    public const string R_PATIENT_PROGRAM_DATE_KEY = "PatientProgramDateKey";
    public const string R_PATIENT_PROGRAM_DAYS_KEY = "PatientProgramDaysKey";
    public static string R_TRACKER_OPTION_KEY => "TrackerKey";
    public static string R_DATE_OPTION_KEY => "DateKey";
    public static string R_DAY_OPTION_KEY => "DaysKey";
    public const long R_TRACKER_OPTION_ID = 812;
    public const long R_DATE_OPTION_ID = 813;
    public const long R_DAY_OPTION_ID = 814;

    #endregion

    #region ProfileShare
    public static string R_PROFILE_SHARE_WITH_KEY => "ProfileSharedWithKey";
    public static string R_USER_RELATION_TYPE_KEY => "UserRelationTypeKey";
    public static string R_SHARE_PROGRAMS_KEY => "ShareProgramsKey";
    public static string R_MENU_ACTION_SHARE_KEY => "MenuActionShareKey";
    public static string R_MENU_ACTION_ADD_KEY => "MenuActionAddKey";
    public static string R_MENU_ACTION_REFRESH_KEY => "MenuActionRefreshKey";
    #endregion
    public const string R_ORGANISATION_TAG_TEXT_KEY = "OrganisationTagTextKey";
    public const string R_ORGANISATION_TAG_DESCRIPTION_KEY = "OrganisationTagDescriptionKey";

    /// <summary>
    /// OkActionKey
    /// </summary>
    public static string R_OK_ACTION_KEY => "OkActionKey";

    /// <summary>
    /// Resource key used for action
    /// </summary>
    public static string R_NEXT_ACTION_KEY => "NextActionKey";

    /// <summary>
    /// Resource key used for save
    /// </summary>
    public static string R_SAVE_ACTION_KEY => "SaveActionKey";

    /// <summary>
    /// ApplicationNameKey
    /// </summary>
    public static string R_APPLICATION_NAME_KEY => "ApplicationNameKey";

    /// <summary>
    /// ChooseFromGalleryTextKey
    /// </summary>
    public static string R_CHOOSE_PHOTO_FROM_GALLERY_TEXT_KEY => "ChooseFromGalleryTextKey";

    /// <summary>
    /// TakePictureTextKey
    /// </summary>
    public static string R_TAKE_PHOTO_FROM_CAMERA_TEXT_KEY => "TakePictureTextKey";

    /// <summary>
    /// UploadDocumentTextKey
    /// </summary>
    public static string R_UPLOAD_DOCUMENT_TEXT_KEY => "UploadDocumentTextKey";

    /// <summary>
    /// DocumentNotSupportedTextKey
    /// </summary>
    public static string R_CORRUPTED_DOCUMENT_TEXT_KEY => "CorruptedDocumentTextKey";

    /// <summary>
    /// DeleteActionKey
    /// </summary>
    public static string R_DELETE_ACTION_KEY => "DeleteActionKey";

    /// <summary>
    /// Remove text key
    /// </summary>
    public static string R_REMOVE_TEXT_KEY => "RemoveTextKey";

    /// <summary>
    /// Remove text key
    /// </summary>
    public static string R_UPLOAD_NOW_KEY => "UploadedNowKey";

    /// <summary>
    /// select All
    /// </summary>
    public static string R_SELECT_ALL_TEXT_KEY => "SelectAllTextKey";
    /// <summary>
    /// CancelActionKey
    /// </summary>
    public static string R_CANCEL_ACTION_KEY => "CancelActionKey";

    /// <summary>
    /// DropDownSelectionValidationKey
    /// </summary>
    public static string R_DROPDOWN_SELECTION_VALIDATION_KEY => "DropDownSelectionValidationKey";

    /// <summary>
    /// RequiredFieldValidationKey
    /// </summary>
    public static string R_REQUIRED_FIELD_VALIDATION_KEY => "RequiredFieldValidationKey";

    /// <summary>
    /// ImageUploadHeaderTextKey
    /// </summary>
    public static string R_IMAGE_UPLOAD_HEADER_TEXT_KEY => "ImageUploadHeaderTextKey";

    /// <summary>
    /// InvalidData
    /// </summary>
    public static string R_INVALID_DATA_KEY => "InvalidData";

    /// <summary>
    /// SearchTextKey
    /// </summary>
    public static string R_SEARCH_TEXT_KEY => "SearchTextKey";

    /// <summary>
    /// MinimumLengthValidationKey
    /// </summary>
    public static string R_MINIMUM_LENGTH_VALIDATION_KEY => "MinimumLengthValidationKey";

    /// <summary>
    /// RangeValueValidationKey 
    /// </summary>
    public static string R_RANGE_VALUE_VALIDATION_KEY => "RangeValueValidationKey";

    /// <summary>
    /// DayTextKey
    /// </summary>
    public const string R_DAY_TEXT_KEY = "DayTextKey";

    /// <summary>
    /// WeekTextKey
    /// </summary>
    public const string R_WEEK_TEXT_KEY = "WeekTextKey";

    /// <summary>
    /// MonthTextKey
    /// </summary>
    public const string R_MONTH_TEXT_KEY = "MonthTextKey";

    /// <summary>
    /// YearTextKey
    /// </summary>
    public const string R_YEAR_TEXT_KEY = "YearTextKey";

    /// <summary>
    /// AllDaysTextKey
    /// </summary>
    public const string R_ALL_DAYS_TEXT_KEY = "AllDaysTextKey";

    /// <summary>
    /// SupportedUploadFileTypeKey
    /// </summary>
    public static string R_SUPPORTED_UPLOAD_FILE_TYPE_KEY => "SupportedUploadFileTypeKey";

    /// <summary>
    /// UploadedFileNotValidKey Key for Uploaded File Not Valid Message
    /// </summary>
    public static string R_UPLOADED_FILE_NOT_VALID_KEY => "UploadedFileNotValidKey";

    /// <summary>
    /// SendActionKey
    /// </summary>
    public static string R_SEND_ACTION_KEY => "SendActionKey";

    /// <summary>
    /// RangeLengthValidationKey
    /// </summary>
    public static string R_RANGE_LENGTH_VALIDATION_KEY => "RangeLengthValidationKey";

    /// <summary>
    /// NoDataFoundKey
    /// </summary>
    public static string R_NO_DATA_FOUND_KEY => "NoDataFoundKey";

    /// <summary>
    /// ImageUploadSubHeaderTextKey
    /// </summary>
    public static string R_IMAGE_UPLOAD_SUBHEADER_TEXT_KEY => "ImageUploadSubHeaderTextKey";

    /// <summary>
    /// FileUploadHeaderTextKey
    /// </summary>
    public static string R_FILE_UPLOAD_HEADER_TEXT_KEY => "FileUploadHeaderTextKey";

    /// <summary>
    /// ConfigureActionKey
    /// </summary>
    public static string R_CONFIGURE_ACTION_KEY => "ConfigureActionKey";

    /// <summary>
    /// AddActionKey
    /// </summary>
    public static string R_ADD_ACTION_KEY => "AddActionKey";

    /// <summary>
    /// AddActionKey
    /// </summary>
    public static string R_GM_KEY => "GoodMorningKey";

    /// <summary>
    /// AddActionKey
    /// </summary>
    public static string R_GA_KEY => "GoodAfternoonKey";

    /// <summary>
    /// AddActionKey
    /// </summary>
    public static string R_GE_KEY => "GoodEveningKey";

    /// <summary>
    /// 2 factor verification key
    /// </summary>
    public static string R_VERIFICATION_KEY => "VerificationKey";

    /// <summary>
    /// Otp Verification Key
    /// </summary>
    public static string R_OTP_CODE_COUNTDOWN_MESSAGE_KEY => "OtpCodeCountDownMessageKey";

    /// <summary>
    /// Forgot pincode link text key
    /// </summary>
    public static string R_FORGOT_PINCODE_LINK_KEY => "ForgotPincodeLinkKey";

    /// <summary>
    /// Confirm pincode key
    /// </summary>
    public static string R_CONFIRM_PINCODE_KEY => "ConfirmPinCodeKey";

    /// <summary>
    /// Resend otp action key
    /// </summary>
    public static string R_RESEND_OTP_ACTION_KEY => "ResendOtpActionKey";

    /// <summary>
    /// Pincode key
    /// </summary>
    public static string R_PINCODE_KEY => "PinCodeKey";

    /// <summary>
    /// Pincode not match error key
    /// </summary>
    public static string R_PINCODE_NOT_MATCH_ERROR_KEY => "PincodeNotMatchErrorKey";

    /// <summary>
    /// Verify finger print message key
    /// </summary>
    public static string R_VERIFY_FINGER_PRINT_MESSAGE_KEY => "VerifyFingerPrintMessageKey";

    /// <summary>
    /// Verify biometric message body face key
    /// </summary>
    public static string R_VERIFY_BIOMETRIC_MESSAGE_BODY_FACE_KEY => "VerifyBiometricMessageBodyFaceKey";

    /// <summary>
    /// Verify biometric message body finger key
    /// </summary>
    public static string R_VERIFY_BIOMETRIC_MESSAGE_BODY_FINGER_KEY => "VerifyBiometricMessageBodyFingerKey";

    /// <summary>
    /// Biometric authentication failed status key
    /// </summary>
    public static string R_BIOMETRIC_AUTHENTICATION_FAILED_STATUS_KEY => "BiometricAuthenticationFailedStatusKey";

    /// <summary>
    /// Biometric authetication result too many attempts status key
    /// </summary>
    public static string R_BIOMETRIC_AUTHENTICATION_RESULT_TOO_MANY_ATTEMPTS_STATUS_KEY => "BiometricAuthenticationResultTooManyAttemptsStatusKey";

    /// <summary>
    /// Biometric authetication result not available status key
    /// </summary>
    public static string R_BIOMETRIC_AUTHENTICATION_RESULT_NOT_AVAILABLE_STATUS_KEY => "BiometricAuthenticationResultNotAvailableStatusKey";
    /// <summary>
    /// ATTACH_FILE
    /// </summary>
    public static string R_ATTACH_FILE_KEY => "AttachFileKey";
    /// <summary>
    /// AddCaption/Title Key
    /// </summary>
    public static string R_CAPTION_KEY => "AddCaption/TitleKey";

    /// <summary>
    /// Pincode login key
    /// </summary>
    public static string R_PINCODE_LOGIN_KEY => "PincodeLoginKey";

    /// <summary>
    /// SMS authentication key
    /// </summary>
    public static string R_SMS_AUTHENTICATION_KEY => "SMSAuthenticationKey";

    /// <summary>
    /// Observation graph target label key
    /// </summary>
    public static string R_TARGET_GRAPH_LABEL_KEY => "GraphTargetLabelKey";
    public static string R_GRAPH_DATA_NOT_FOUND_KEY => "GraphDataNotFoundKey";

    /// <summary>
    /// Axis label biweekly key
    /// </summary>
    public static string R_AXIS_LABEL_BIWEEKLY_KEY => "AxisLabelBiweeklyKey";

    /// <summary>
    /// Axis label month key
    /// </summary>
    public static string R_AXIS_LABEL_MONTH_KEY => "AxisLabelMonthKey";

    /// <summary>
    /// Axis label year key
    /// </summary>
    public static string R_AXIS_LABEL_YEAR_KEY => "AxisLabelYearKey";

    /// <summary>
    /// Axis label week key
    /// </summary>
    public static string R_AXIS_LABEL_WEEK_KEY => "AxisLabelWeekKey";

    /// <summary>
    /// Axis label day key
    /// </summary>
    public static string R_AXIS_LABEL_DAY_KEY => "AxisLabelDayKey";

    /// <summary>
    /// Axis label hour key
    /// </summary>
    public static string R_AXIS_LABEL_HOUR_KEY => "AxisLabelHourKey";

    /// <summary>
    /// Interpretation text for absolute low key
    /// </summary>
    public static string R_INTERPRETATION_TEXT_ABSOLUTE_LOW_KEY => "InterpretationAbsoluteLowTextKey";

    /// <summary>
    /// Interpretation text for normal key
    /// </summary>
    public static string R_INTERPRETATION_TEXT_NORMAL_KEY => "InterpretationNormalTextKey";

    /// <summary>
    /// Interpretation text for absolute key
    /// </summary>
    public static string R_INTERPRETATION_TEXT_ABSOLUTE_KEY => "InterpretationAbsoluteTextKey";

    /// <summary>
    /// Date resource key
    /// </summary>
    public const string R_DATE_TIME_TEXT_KEY = "DateTimeTextKey";

    /// <summary>
    /// DayTextKey
    /// </summary>
    public const string R_MEASUREMENT_TYPE_TEXT_KEY = "MeasurementTypeTextKey";

    /// <summary>
    /// NoteTextKey
    /// </summary>
    public static string R_NOTE_TEXT_KEY => "NoteTextKey";

    /// <summary>
    /// CurrentValue Key
    /// </summary>
    public static string R_READING_CURRENT_HEADING_KEY => "CurrentValueKey";

    /// <summary>
    /// Reading CurrentValue Key ID
    /// </summary>
    public const long R_CURRENT_VALUE_KEY_ID = 492;

    /// <summary>
    /// Reading Goal value Key
    /// </summary>
    public static string R_GOAL_VALUE_KEY => "GoalValueKey";

    /// <summary>
    /// Reading Goal Value Key ID
    /// </summary>
    public const long R_GOAL_VALUE_KEY_ID = 493;

    /// <summary>
    /// Date heading for reading list
    /// </summary>
    public static string R_MEASUREMENT_DETAILS_LIST_DATE_HEADING_KEY => "DateHeadingKey";

    /// <summary>
    /// PermissionTextKey
    /// </summary>
    public static string R_PERMISSION_TEXT_KEY => "PermissionTextKey";

    /// <summary>
    /// LastSyncedDateTextKey
    /// </summary>
    public static string R_LAST_SYNCED_DATE_TEXT_KEY => "LastSyncedDateTextKey";

    /// <summary>
    /// Reading question text
    /// </summary>
    public static string R_READING_QUESTION_KEY => "ReadingQuestionTextKey";

    /// <summary>
    /// Select glucose identifier key
    /// </summary>
    public static string R_SELECT_GLUCOSE_IDENTIFIER_KEY => "SelectGlucoseTypeKey";

    /// <summary>
    /// Add reading title key
    /// </summary>
    public static string R_ADD_READING_TITLE_KEY => "AddReadingTitleKey";

    /// <summary>
    /// Add reading type title key
    /// </summary>
    public static string R_ADD_READING_TYPE_TITLE_KEY => "AddReadingTypeTitleKey";

    /// <summary>
    /// Max range validation key
    /// </summary>
    public static string R_MAX_RANGE_VALIDATION_KEY => "MaxRangeValidationKey";

    /// <summary>
    /// Health kit permission text key
    /// </summary>
    public static string R_HEALTH_KIT_PERMISSION_TEXT_KEY => "HealthKitPermissionTextKey";

    /// <summary>
    /// Health kit permission text key 
    /// </summary>
    public static string R_HEALTH_KIT_PERMISSION_IOS_12_TEXT_KEY => "HealthKitPermissionIOS12TextKey";

    #region RSUserTypeGroup

    /// <summary>
    /// Resource key ID for ProviderKey in RSUserTypeGroup
    /// </summary>
    public static short R_PROVIDER_KEY_ID => 201;

    /// <summary>
    /// Resource key ID for PatientKey in RSUserTypeGroup
    /// </summary>
    public static short R_PATIENT_KEY_ID => 202;

    /// <summary>
    /// Resource key ID for BothKey in RSUserTypeGroup
    /// </summary>
    public static short R_BOTH_KEY_ID => 203;

    /// <summary>
    /// Resource key ProviderKey in RSUserTypeGroup
    /// </summary>
    public static string R_PROVIDER_KEY => "ProviderKey";

    /// <summary>
    /// Resource key PatientKey in RSUserTypeGroup
    /// </summary>
    public static string R_PATIENT_KEY => "PatientKey";

    #endregion

    #region RSValueAddedByGroup

    /// <summary>
    /// AddedBy Key
    /// </summary>
    public static string R_ADDED_BY_KEY => "AddedByKey";
    /// <summary>
    /// AddedBy Key
    /// </summary>
    public static string R_LAST_ACTION_BY_KEY => "LastActionByKey";

    /// <summary>
    /// Self Performer Key
    /// </summary>
    public static string R_SELF_PERFORMER_KEY => "SelfPerformerKey";

    /// <summary>
    /// Device Key
    /// </summary>
    public static string R_DEVICE_TEXT_KEY => "DeviceTextKey";

    /// <summary>
    /// Google Fit Key
    /// </summary>
    public static string R_GOOGLE_FIT_TEXT_KEY => "GoogleFitTextKey";

    /// <summary>
    /// IHealth Key
    /// </summary>
    public static string R_IHEALTH_TEXT_KEY => "IhealthTextKey";

    #endregion

    #region RSReadingFrequencyTypeGroup

    ///// <summary>
    ///// Reading Frequency All Key
    ///// </summary>
    //public static string R_ALL_KEY => "AllKey";

    ///// <summary>
    ///// Reading Frequency Daily Sum Key
    ///// </summary>
    //public static string R_DAILY_SUM_KEY => "DailySumKey";

    ///// <summary>
    ///// Reading Frequency Hourly Sum Key
    ///// </summary>
    //public static string R_HOURLY_SUM_KEY => "HourlySumKey";

    ///// <summary>
    ///// Reading Frequency Daily Average Key
    ///// </summary>
    //public static string R_DAILY_AVG_KEY => "DailyAvgKey";

    ///// <summary>
    ///// Resource key ID for AllKey in RSReadingFrequencyTypeGroup
    ///// </summary>
    //public static short R_ALL_KEY_ID => 206;

    /// <summary>
    /// Resource key ID for DailySumKey in RSReadingFrequencyTypeGroup
    /// </summary>
    public static short R_DAILY_SUM_KEY_ID => 207;

    /// <summary>
    /// Resource key ID for HourlySumKey in RSReadingFrequencyTypeGroup
    /// </summary>
    public static short R_HOURLY_SUM_KEY_ID => 208;

    /// <summary>
    /// Resource key ID for DailyAvgKey in RSReadingFrequencyTypeGroup
    /// </summary>
    public static short R_DAILY_AVG_KEY_ID => 209;

    /// <summary>
    /// Resource key ID for HourlyAvgKey in RSReadingFrequencyTypeGroup
    /// </summary>
    public static short R_HOURLY_AVG_KEY_ID => 820;

    #endregion

    #region RSUserAccountSettingsGroup
    /// <summary>
    /// Text Key for push notification
    /// </summary>
    public static string PUSH_NOTIFICATION_KEY => "PushNotificationKey";

    /// <summary>
    /// HTML text for IOS
    /// </summary>
    public static string R_ACCOUNT_IOS_HTML_FORMAT_KEY => "AccountIosHtmlFormatKey";

    /// <summary>
    /// HTML text for android
    /// </summary>
    public static string R_ACCOUNT_ANDROID_HTML_FORMAT_KEY => "AccountAndroidHtmlFormatKey";

    /// <summary>
    /// Connect button text key
    /// </summary>
    public static string R_CONNECT_BUTTON_TEXT_KEY => "ConnectButtonTextKey";

    /// <summary>
    /// Skip button text key
    /// </summary>
    public static string R_SKIP_BUTTON_TEXT_KEY => "SkipButtonTextKey";

    /// <summary>
    /// health account text key
    /// </summary>
    public static string R_HEALTH_ACCOUNT_TEXT_KEY => "HealthAccountTextKey";
    #endregion

    /// <summary>
    /// Not Found Measurement Text Key
    /// </summary>
    public static string R_NOT_FOUND_MEASUREMENT_TEXT_KEY => "NotFoundMeasurementTextKey";

    /// <summary>
    /// Connect To Health Kit Message Key
    /// </summary>
    public static string R_CONNECT_TO_HEALTHKIT_MESSAGE_KEY => "ConnectToHealthKitMessageKey";

    /// <summary>
    /// Not Found Measurement 2 Text Key
    /// </summary>
    public static string R_NOT_FOUND_MEASUREMENT_2_TEXT_KEY => "NotFoundMeasurement2TextKey";

    /// <summary>
    /// Not Found Measurement 3 Text Key
    /// </summary>
    public static string R_NOT_FOUND_MEASUREMENT_3_TEXT_KEY => "NotFoundMeasurement3TextKey";

    /// <summary>
    /// Set Target Text Key
    /// </summary>
    public static string R_SET_TARGET_KEY => "SetTargetKey";

    /// <summary>
    /// Set Target Text Key
    /// </summary>
    public static string R_RETRIEVE_DATA_BUTTON_TEXT_KEY => "RetriveDataButtonTextKey";

    /// <summary>
    /// Reading Target Min Value Key
    /// </summary>
    public static string R_READING_TARGET_MIN_VALUE_KEY => "ReadingTargetMinValueKey";

    /// <summary>
    /// Reading Target Max ValueKey
    /// </summary>
    public static string R_READING_TARGET_MAX_VALUE_KEY => "ReadingTargetMaxValueKey";

    /// <summary>
    /// Reading Type Key
    /// </summary>
    public const string R_READING_TYPE_KEY = "ReadingTypeKey";

    /// <summary>
    /// Reading Type Key
    /// </summary>
    public static string R_SELECT_READING_TYPE_KEY => "SelectReadingTypeKey";

    /// <summary>
    /// Request bluetooth permission
    /// </summary>
    public static string R_REQUEST_BLUETOOTH_PERMISSION_TEXT_KEY => "RequestBluetoothPermissionTextKey";

    /// <summary>
    /// Denied bluetooth permission text key
    /// </summary>
    public static string R_DENIED_BLUETOOTH_PERMISSION_TEXT_KEY => "DeniedBluetoothPermissionTextKey";

    /// <summary>
    /// Enable location setting text key
    /// </summary>
    public static string R_LOCATION_SETTING_ENABLE_TEXT_KEY => "LocationSettingEnableTextKey";

    /// <summary>
    /// Turn on location instruction text key
    /// </summary>
    public static string R_LOCATION_TURN_ON_INSTRUCTION_TEXT_KEY => "LocationTurnOnInstructionTextKey";

    /// <summary>
    /// Location permission text key
    /// </summary>
    public static string R_LOCATION_PERMISSION_TEXT_KEY => "LocationPermissionTextKey";

    /// <summary>
    /// Device records updated message key
    /// </summary>
    public static string R_RECORDS_UPDATED_MESSAGE_KEY => "RecordUpdatedMessageKey";

    /// <summary>
    /// Device no new measurements found key
    /// </summary>
    public static string R_NO_NEW_MEASUREMENT_TEXT_KEY => "NoNewMeasurementTextKey";

    /// <summary>
    /// Device no new measurements found key
    /// </summary>
    public static string R_MANUAL_TEXT_KEY => "ManualTextKey";

    /// <summary>
    /// Device records updated message key
    /// </summary>
    public static string R_SEARCH_FOOD_TEXT_KEY => "SearchFoodTextKey";

    /// <summary>
    /// Device no new measurements found key
    /// </summary>
    public static string R_PORTION_SIZE_TEXT_KEY => "PortionSizeTextKey";

    /// <summary>
    /// Device no new measurements found key
    /// </summary>
    public static string R_ENTER_FOOD_TEXT_KEY => "EnterFoodTextKey";

    /// <summary>
    /// Mandatory error key
    /// </summary>
    public static string R_MANDATORY_ERROR_KEY => "MandatoryErrorKey";

    /// <summary>
    /// Food search result key
    /// </summary>
    public static string R_FOOD_SERACH_RESULT_TEXT_KEY => "FoodSearchResultTextKey";

    /// <summary>
    /// Offline operation instruction key
    /// </summary>
    public static string R_OFFLINE_OPERATION_KEY => "OfflineOperationKey";

    /// <summary>
    /// Addition Notes TextKey
    /// </summary>
    public const string R_ADDITIONAL_NOTE_TEXT_KEY = "AdditionalNotesKey";

    /// <summary>
    /// Tooltip for Priview Content
    /// </summary>
    public static string R_TOOLTIP_PRIVIEW_TEXT => "Priview Content";
    public static string R_TIMELINE_LEFT_TEXT_1 => "NOV 2023";
    public static string R_TIMELINE_LEFT_TEXT_2 => "DEC 2023";
    public static string R_TIMELINE_LEFT_TEXT_3 => "JAN 2024";
    public static string R_TIMELINE_LEFT_TEXT_4 => "FEB 2024";
    public static string R_TIMELINE_RIGHT_TEXT_1 => "Register Your Account";
    public static string R_TIMELINE_RIGHT_TEXT_2 => "Verify Your Identity";
    public static string R_TIMELINE_RIGHT_TEXT_3 => "Complete Self-Certification";
    public static string R_TIMELINE_RIGHT_TEXT_4 => "Complete Your Profile";
    public static string R_TIMELINE_DEFAULT_TEXT => "A";

    #region RSCareFlixServiceIntegrationGroup
    public const string R_TOTAL_DISCOUNT_KEY = "TotalDiscountKey";
    public const string R_DISCOUNT_PERCENTAGE_KEY = "DiscountKey";
    public const string R_AMOUNT_TO_PAY_KEY = "AmountToPayKey";
    public static string R_MAKE_PAYMENT_KEY => "MakePaymentKey";
    public static string R_AVAILABLE_SCANS_KEY => "AvailableScansKey";
    public static string R_ASSIGNED_CREDITS_KEY => "AssignedCreditsKey";
    public const string R_UNIT_PRICE_KEY = "UnitPriceKey";
    public const string R_MINIMUM_TO_BUY_KEY = "MinimumToBuyKey";
    public const string R_QUANTITY_TO_BUY_KEY = "QuantityToBuyKey";
    public static string R_TOTAL_AMOUNT_BEFORE_DISCOUNT_KEY => "TotalAmountKey";
    public static string R_DATE_TIME_KEY => "DateTimeKey";
    public static string R_PROGRAM_KEY => "ProgramKey";
    public static string R_ORGANISATION_TAGS_KEY => "OrganisationTagsKey";
    public const string R_MEDICAL_LICENSE_NUMBER_KEY = "MedicalLicenseNumberKey";

    public static string R_SCANS_KEY => "ScansKey";
    public static string R_USED_SCANS_KEY => "UsedScansKey";
    public static string R_REMAINING_SCANS_KEY => "RemainingScansKey";
    public static string R_SOURCE_KEY => "SourceKey";

    public static string R_ORGANISATION_CREDITS_KEY => "OrganisationCreditsKey";
    public static string R_SPENT_CREDITS_KEY => "SpentCreditKey";
    public static string R_AVAILABLE_CREDITS_KEY => "AvailableCreditKey";
    public static string R_TOTAL_PATIENTS_KEY => "TotalPatientsKey"; 
    public static string R_QUANTITY_KEY => "QuantityKey";

    public static string R_FOR_ORGANISATION_KEY => "ForOrganisationKey";
    public static string R_FOR_PATIENT_KEY => "ForPatientKey";
    public static string R_ORGANISATION_SERVICES_SUBSCRIPTION_KEY => "OrganisationServicesSubscriptionKey";
    /// <summary>
    /// Service Name Resource Key
    /// </summary>

    public const string R_SERVICE_NAME_KEY = "ServiceNameKey";

    /// <summary>
    /// No Of Scans Resource Key
    /// </summary>

    public const string R_NO_OF_SCANS_KEY = "NoOfScansKey";
    #endregion

    public const string R_PAYMENT_FAILED_KEY = "PaymentFailedKey";
    /// <summary>
    /// Add Vitals Resource Key
    /// </summary>
    public static string R_ADD_VITALS_KEY = "AddVitalsKey";

    /// <summary>
    /// Scan Vitals Resource Key
    /// </summary>
    public static string R_SCAN_VITALS_KEY = "ScanVitalsKey";


    /// <summary>
    /// Date of birth Scans Resource Key
    /// </summary>
    public const string R_DATE_OF_BIRTH_FOR_SCANS_KEY = "DateOfBirthKey";
    /// <summary>
    /// Height for Scans Resource Key
    /// </summary>
    public const string R_HEIGHT_FOR_SCANS_KEY = "HeightForScansKey";

    /// <summary>
    /// Weight for Scans Resource Key
    /// </summary>
    public const string R_WEIGHT_FOR_SCANS_KEY = "WeightForScansKey";

    /// <summary>
    /// Start Scan Resource Key
    /// </summary>
    public const string R_START_SCAN_KEY = "StartScanKey";

    #region RSScanTypeGroup
    public const string R_FACE_SCAN_TYPE_KEY = "FaceScanKey";
    public const string R_FINGER_SCAN_TYPE_KEY = "FingerScanKey";
    #endregion

    #region RSPostureGroup
    public const string R_RESTING_POSITION_KEY = "RestingKey";
    public const string R_STANDING_POSITION_KEY = "StandingKey";
    #endregion

    /// <summary>
    /// Scan Posture Resource Key
    /// </summary>
    public static string R_SCANS_POSTURE_KEY = "ScansPostureKey";

    /// <summary>
    /// Scan Type Resource Key
    /// </summary>
    public static string R_SCANS_TYPE_KEY = "ScansTypeKey";

    ///// <summary>
    ///// Add Vitals Resource Key
    ///// </summary>
    //public static string R_ADD_VITALS_KEY = "AddVitalsKey";

    ///// <summary>
    ///// Scan Vitals Resource Key
    ///// </summary>
    //public static string R_SCAN_VITALS_KEY = "ScanVitalsKey";

    /// <summary>
    /// Load More Resource Key
    /// </summary>
    public const string R_LOAD_MORE_KEY = "LoadMoreKey";

    /// <summary>
    /// I accept resource key
    /// </summary>
    public const string R_I_ACCEPT_TEXT_KEY = "IAcceptTextKey";
}