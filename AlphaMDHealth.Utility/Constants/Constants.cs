namespace AlphaMDHealth.Utility;

public class Constants
{
    #region ENVIRONMENT CONSTANTS

    /// <summary>
    /// Dev Environment key
    /// </summary>
    public static string DEV_KEY => "dev";

    /// <summary>
    /// dev-india Environment key
    /// </summary>
    public static string DEV_INDIA_KEY => "dev-india";

    /// <summary>
    /// Uat environment key
    /// </summary>
    public static string UAT_KEY => "uat";

    /// <summary>
    /// Note: DO NOT change the S_PROD_KEY if you want to change default environment, use DEFAULT_ENVIRONMENT_KEY instead
    /// </summary>
    public static string PROD1_KEY => "prod1";

    /// <summary>
    /// Use DEFAULT_ENVIRONMENT_KEY for changing the default environment
    /// </summary>
    public static string DEFAULT_ENVIRONMENT_KEY => DEV_KEY;

    #endregion

    #region APP-CENTER APP ID CONSTANTS
    public static string APP_CENTER_SETUP_STRING_FORMAT = $"ios={0};android={1};";
    public static string APP_CENTER_APP_ID_DEV => $"ios={APP_CENTER_IOS_APP_SECRET_DEV};android={APP_CENTER_ANDROID_APP_SECRET_DEV};";

    public static string APP_CENTER_APP_ID_PROD => $"ios={APP_CENTER_IOS_APP_SECRET_PROD};android={APP_CENTER_ANDROID_APP_SECRET_PROD};";

    public static string APP_CENTER_ANDROID_APP_SECRET_DEV => "85587dc2-975d-4352-949b-24d37eb1a4b8";
    public static string APP_CENTER_ANDROID_APP_SECRET_PROD => "00f8dab2-a2ff-4555-a1ef-a0714dedda3a";

    public static string APP_CENTER_IOS_APP_SECRET_DEV => "fdceb90e-7dac-4c3e-adf1-5ba5b9046450";
    public static string APP_CENTER_IOS_APP_SECRET_PROD => "84621349-f9be-4af6-b302-bd11f224e569";
    #endregion

    #region CONFIGURATION COUNTS
    public static string APP_VERSON_NO => "1";
    public static int CONFIG_SETTINGS_COUNT => 79;
    public static int CONFIG_RESOURCES_COUNT => 542;

    #endregion

    /// <summary>
    /// file name of medicine data script file
    /// </summary>
    public static string MEDICINE_DATA_FILE_NAME => "{0}.txt";

    /// <summary>
    /// Date time till when default medicine data is present in script, so that will fetch data from server after this date only
    /// </summary>
    public static DateTimeOffset DEFAULT_MEDICINE_DATETIME => new DateTimeOffset(2023, 08, 17, 0, 0, 0, TimeSpan.Zero);

    #region Integration Constants

    public static string DUMMY_BLOB_URL_LINK => "https://myfolder/myfile/";

    public static string EXCEL_EXTESTION => ".xlsx";

    /// <summary>
    /// Bulk upload sample excel path
    /// </summary>
    public static string BULK_UPLOAD_SAMPLE_FILE => DUMMY_BLOB_URL_LINK + "StaticFiles/{0}" + EXCEL_EXTESTION;

    public static string BACK_SLASH => "/";
    public static string MEDIA_TYPE => "application/json";
    public static string HTTP_TEXT_KEY => "http";
    public static string HTTPS_TEXT_KEY => "https";
    public static string AUTH_POST_TEXT_KEY => "POST";
    public const char SYMBOL_QUESTIONMARK = '?';
    public static string SE_CLIENT_IDENTIFIER_HEADER_KEY => "Client-Identifier";
    public static string SE_FOR_APPLICATION_HEADER_KEY => "For-Application";
    public static string SE_CLIENT_SECRETE_HEADER_KEY => "Client-Secrete";
    public static string SE_HMAC_SIGNATURE_HEADER_KEY => "X-Signature";
    public static string FIELD_ERROR_LOG_DATE => "ErrorLogDate";
    public static string FIELD_ERROR_THREAD_INFO => "ErrorThreadInfo";
    public static string FIELD_IS_DB_ERROR => "IsDBError";
    public static string FIELD_DETAIL_RECORDS => "DetailRecords";
    public static string USP_SAVE_SERVICE_LOGS => "usp_SaveServiceLogs";
    public static string USP_ERROR_LOGGING => "usp_ErrorLogging";
    public static string USP_GET_CLIENT_DATA => "usp_GetClientData";

    #endregion

    #region Integration Vidyo.io

    public static string INFOVIDYOCONNECTOR_WARNING => "info@VidyoClient info@VidyoConnector warning";
    public static string CLIENTVERSION => "Failed";
    public static string CONNECTOR_STATE => "ConnectorState";

    #endregion

    #region SYMBOLES

    public static char COMMA_SEPARATOR => ',';
    public static string JSON_NEW_LINE => "\\n";
    public static char SYMBOL_PLUS => '+';
    public static char SYMBOL_DOT => '.';
    public static string HASH_INDICATOR => "#";
    public static string OBSERVATION_PAGE_TWO_DIGIT_STRING_FORMAT => "00";
    public static char CHAR_SPACE => ' ';
    public static char SYMBOL_PARAM_SEPERATOR => '>';
    public static char SYMBOL_LESS_THAN => '<';
    #endregion

    #region DEFAULT TEXT CONSTANTS

    ////No network
    public static string NO_INTERNET_CONNECTION_RESOURCE_VALUE_TEXT => "No Internet!";
    public static string NO_INTERNET_CONNECTION_PLACEHOLDER_VALUE_TEXT => "<p>No Internet Connection Found.<br>Check Your Connection</p>";

    // Restart App
    public static string RESTART_APP_RESOURCE_VALUE_TEXT => "There's a Problem!!";
    public static string RESTART_APP_PLACEHOLDER_VALUE_TEXT => "<p>App did not Load Properly.<br>Restart App.</p>";


    // Record Count Mismatch
    public static string RECORD_COUNT_MISMATCH_RESOURCE_VALUE_TEXT => "Configuration Mismatch";
    public static string RECORD_COUNT_MISMATCH_PLACEHOLDER_VALUE_TEXT => "<p>There is inconsistency in the configuration data due to which the app cannot be started.<br>Please contact your system administrator.</p>";


    //Update app
    public static string UPGRADE_REQUIRED_RESOURCE_VALUE_TEXT => "Update Available";
    public static string UPGRADE_REQUIRED_PLACEHOLDER_VALUE_TEXT => "<p>The Version of your App is Outdated.<br>Please Update to the Latest Version.</p>";

    //Unauthorized
    public static string UNAUTHORIZED_RESOURCE_VALUE_TEXT => "Unauthorized";
    public static string UNAUTHORIZED_PLACEHOLDER_VALUE_TEXT => "<p>You don't have the permission to access the page</p>";

    //Unknown Certificate
    public static string UNKNOWN_CERTIFICATE_RESOURCE_VALUE_TEXT => "Unauthorized";
    public static string UNKNOWN_CERTIFICATE_PLACEHOLDER_VALUE_TEXT => "<p>Please contact your system administrator.</p>";

    // Service Unavailable 
    public static string SERVICE_UNAVAILABLE_RESOURCE_VALUE_TEXT => "Service Unavailable";
    public static string SERVICE_UNAVAILABLE_PLACEHOLDER_VALUE_TEXT => "<p>App is unable to connect to the server.<br>Please try again later</p>";

    // Service Unavailable 
    public static string JAIL_BROKEN_DEVICE_RESOURCE_VALUE_TEXT => "Jailbroken Device!!!";
    public static string JAIL_BROKEN_DEVICE_PLACEHOLDER_VALUE_TEXT => "<p>This application is not supported in jailbroken devices</p>";

    // Language not available 
    public static string LANGUAGE_NOT_AVAILABLE_RESOURCE_VALUE_TEXT => "Currently selected language is not available";
    public static string LANGUAGE_NOT_AVAILABLE_PLACEHOLDER_VALUE_TEXT => "<p>Selected language is not available on server.<br>Please try again later</p>";

    // Common Texts
    public static string TRY_AGAIN_ACTION_TEXT => "Try Again";
    public static string APPLICATION_NAME_TEXT => "Atom";

    public static string OK_TEXT => "Ok";
    public static string CANCEL_TEXT => "Cancel";
    public static string NEXT_ACTION_TEXT => "Next";
    public static string SELECT_LANGUAGE_TEXT => "Select Language";
    #endregion

    #region Mobile App Constants

    public static int SYNC_TO_SERVER_DEFAULT_WAIT_TIME_MINUTES => 5;

    public static string TIMEPICKER_DEFAULTTIME => "00:00 AM";
    public static string BEHAVIOR_ISVALID_PROP => "IsValid";
    public static string BEHAVIOR_VALIDATIONERROR_PROP => "ValidationError";
    public static string BEHAVIOR_VALIDATIONERROR_COLOR_PROP => "ValidationErrorColor";
    public static string IPHONE_DEVICE_NAME => "iPhone X";


    public static char MASKING_CHAR => 'X';
    public static string ONELINK_PARAMETER_SEPERATOR_KEY => "oneLinkParameter";
    public static string COLON_KEY => ": ";
    public static string NOSPACE_COLON_KEY => ":";

    public static string DASH_KEY => "-";
    #endregion

    #region HTTP SERVICE CONSTANTS

    // Web
    public static string CLIENT_ID_WEB => "PersonalHealthWeb";
    public static string CLIENT_SECRET_WEB => "PersonalHealth@Web@2022";

    // Phone
    public static string CLIENT_ID_ANDROID_PHONE => "PersonalHealthAndroidPhone";
    public static string CLIENT_SECRET_ANDROID_PHONE => "PersonalHealth@GoogleAndroidPhone@2022";
    public static string CLIENT_ID_IOS_PHONE => "PersonalHealthiOSPhone";
    public static string CLIENT_SECRET_IOS_PHONE => "PersonalHealth@AppleiOSPhone@2022";

    // Tablet
    public static string CLIENT_ID_ANDROID_TABLET => "PersonalHealthAndroidTablet";
    public static string CLIENT_SECRET_ANDROID_TABLET => "PersonalHealth@GoogleAndroidTablet@2022";
    public static string CLIENT_ID_IOS_TABLET => "PersonalHealthiOSTablet";
    public static string CLIENT_SECRET_IOS_TABLET => "PersonalHealth@AppleiOSTablet@2022";

    public static string CLIENT_PLATFORM_WEB => "Web";
    public static string CLIENT_DEVICE_TYPE_WEB => "W";


    //// Request Headers
    //public static string SE_CLIENT_IDENTIFIER_HEADER_KEY => "Client-Identifier";
    //public static string SE_FOR_APPLICATION_HEADER_KEY => "For-Application";
    //public static string SE_CLIENT_SECRETE_HEADER_KEY => "Client-Secrete";
    //public static string SE_HMAC_SIGNATURE_HEADER_KEY => "X-Signature";
    public static string SE_DEVICE_INFORMATION_HEADER_KEY => "Device-Information";
    public static string SE_DEVICE_UNIQUE_ID_HEADER_KEY => "Device-Unique-ID";
    public static string SE_DEVICE_TYPE_HEADER_KEY => "Device-Type";
    public static string SE_DEVICE_PLATFORM_HEADER_KEY => "Device-Platform";
    public static string SE_NOTIFICATION_CLIENT_IDENTIFIER_HEADER_KEY => "X-ClientId";

    //Resquest Parameters
    public static string SE_LANGUAGE_ID_QUERY_KEY => "languageID";
    public static string SE_LAST_MODIFIED_ON_QUERY_KEY => "lastModifiedOn";
    public static string SE_ORGANISATION_ID_QUERY_KEY => "organisationID";
    public static string SE_SELECTED_USER_ID_QUERY_KEY => "selectedUserID";
    public static string SE_EMPLOYEE_ID_QUERY_KEY => "empID";
    public static string SE_DEPARTMENT_ID_QUERY_KEY => "departmentID";
    public static string SE_BRANCH_ID_QUERY_KEY => "branchID";
    public static string SE_REASON_ID_QUERY_KEY => "reasonID";

    public static string SE_ORGANISATION_TAG_ID_QUERY_KEY => "organisationTagID";
    public static string SE_BILLING_ITEM_ID_QUERY_KEY => "billingItemID";
    public static string SE_PATIENT_BILL_ID => "patientBillID";
    public static string SE_PAYMENT_MODE_ID_QUERY_KEY => "paymentModeID";
    public static string SE_TRACKER_ID_QUERY_KEY => "trackerID";
    public static string SE_TRACKER_TYPE_ID_QUERY_KEY => "trackerTypeID";
    public static string SE_TRACKER_RANGE_ID_QUERY_KEY => "trackerRangeID";
    public static string SE_CONTACT_ID_QUERY_KEY => "contactID";
    public static string SE_MENU_ID_QUERY_KEY => "menuID";
    public static string SE_IS_PATEINT_MENU_QUERY_KEY => "isPatientMenu";
    public static string SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY => "permissionAtLevelID";
    public static string SE_ORAGNISATION_DOMAIN_QUERY_KEY => "organisationDomain";
    public static string SE_RECORD_COUNT_QUERY_KEY => "recordCount";
    public static string SE_MENU_GROUP_ID_QUERY_KEY => "menuGroupID";
    public static string SE_MENU_GROUP_TYPE_QUERY_KEY => "menuGroupType";
    public static string SE_SETTING_GROUP_NAME => "groupName";
    public static string SE_MOBILE_MENU_NODE_ID_QUERY_KEY => "mobileMenuNodeID";
    public static string SE_FEATURE_ID_QUERY_KEY => "featureID";
    public static string SE_CONTACT_TYPE_QUERY_KEY => "contactType";
    public static string SE_ORGANISATION_ID_FOR_SETUP_QUERY_KEY => "organisationIdForSetup";
    public static string SE_READING_RANGE_ID_KEY => "readingRangeID";
    public static string SE_GENDER_KEY => "gender";
    public static string SE_AGE_KEY => "age";
    public static string SE_PROFESSION_ID_QUERY_KEY => "professionID";
    public static string SE_EDUCATION_CATEGORY_ID_QUERY_KEY => "educationCategoryID";
    public static string SE_QUESTIONNAIRE_ID_KEY => "questionnaireID";
    public static string SE_QUESTION_ID_KEY => "questionID";
    public static string SE_DASHBOARD_SETTING_ID_QUERY_KEY => "dashboardSettingID";
    public static string SE_ROLE_ID_QUERY_KEY => "roleID";
    public static string SE_APPOINTMENT_ID_QUERY_KEY => "appointmentID";
    public static string SE_FILE_ID_QUERY_KEY => "fileID";
    public static string SE_DOCUMENT_FILE_ID_QUERY_KEY => "documentFileID";
    public static string SE_PATIENT_CAREGIVER_ID_QUERY_KEY => "patientCareGiverID";
    public static string SE_CONNECTION_ID_QUERY_KEY => "connectionID";
    public static string SE_SIGNALR_ENDPOINT => "signalr";
    public static string SE_API_PATH => "/api/";
    public static string SE_CONSENT_ID_QUERY_KEY => "consentID";
    public static string SE_SEARCH_TEXT_QUERY_KEY => "search";
    public static string SE_GET_FOOD_NUTRITIONS_QUERY_KEY => "foodIdentifier";
    public static string SE_IS_BARCODE_QUERY_KEY => "isBarcode";
    public static string SE_IMAGE_REQUIRED_QUERY_KEY => "imageRequired";
    public static string SE_FILE_CATEGORY_ID_QUERY_KEY => "fileCategoryID";
    #endregion

    #region OTHER CONSTANTS

    public static int NO_OF_EDUCATIONS => 2;
    public static long DEFAULT_ORGANISATION_ID => 1;
    public static string SPACE_PATH_ENCODE_VALUE => "%2B";
    public const string BASE64_METADATA_FORMAT = "data:{0}/{1};base64,";
    public const string PNG_IMAGE_CONST_PREFIX = "data:image/png;base64,";
    public const string BASE64_IMAGE_CONTENT = "image";
    public static string BASE64_APPLICATION_CONTENT => "application";
    public static string OFFICE_VIEW_URL => "https://view.officeapps.live.com/op/embed.aspx?src=";

    /// <summary>
    /// camera string
    /// </summary>
    public const string CAMERA_STRING = "camera";

    /// <summary>
    /// jfif type
    /// </summary>
    public const string JFIF_FILE_TYPE = "jfif";

    /// <summary>
    /// svg type
    /// </summary>
    public const string SVG_FILE_TYPE = "svg";

    /// <summary>
    /// svg+xml type
    /// </summary>
    public const string SVG_PLUS_XML_FILE_TYPE = "svg+xml";

    /// <summary>
    /// jpeg type
    /// </summary>
    public const string JPEG_FILE_TYPE = "jpeg";

    /// <summary>
    /// jpg type
    /// </summary>
    public const string JPG_FILE_TYPE = "jpg";

    /// <summary>
    /// png type
    /// </summary>
    public const string PNG_FILE_TYPE = "png";

    /// <summary>
    /// doc type
    /// </summary>
    public const string DOC_FILE_TYPE = "doc";

    /// <summary>
    /// docx type
    /// </summary>
    public const string DOCX_FILE_TYPE = "docx";

    /// <summary>
    /// pdf type
    /// </summary>
    public const string PDF_FILE_TYPE = "pdf";

    /// <summary>
    /// xls type
    /// </summary>
    public const string XLS_FILE_TYPE = "xls";

    /// <summary>
    /// xlsx type
    /// </summary>
    public const string XLSX_FILE_TYPE = "xlsx";

    /// <summary>
    /// ico type
    /// </summary>
    public const string ICO_FILE_TYPE = "ico";

    /// <summary>
    /// x-icon type
    /// </summary>
    public static string X_ICON_FILE_TYPE => "x-icon";

    public const string IMAGE_TAG_ICO = "image/x-icon";
    public const string IMAGE_TAG_JPEG = "image/jpeg";
    public const string IMAGE_TAG_JPG = "image/jpg";
    public const string IMAGE_TAG_PNG = "image/png";
    public const string IMAGE_TAG_SVG = "image/svg+xml";
    public const string IMAGE_TAG_JFIF = "image/jfif";
    public const string PDF_TAG = "application/pdf";
    public const string DOCX_TAG = "vnd.openxmlformats-officedocument.wordprocessingml.document";
    public const string DOC_TAG = "msword";
    public const string XLS_TAG = "vnd.ms-excel";
    public const string XLSX_TAG = "vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static string IMAGE_START_TAG_WITH_SRC => "<img src=";
    public static string IMAGE_WITH_BASE64_START_TAG => "<img src=\"data:image";
    //public static string BLOB_ACCOUNT_NAME => "l6axfVIF8a0jCq7rylWTvftmvV7HnbcYGtRbY8/k/sM=";
    public static string BLOB_CDN_TOKEN => "";

    public static string LTR_DIRECTION_CONSTANT => "ltr";
    public static string RTL_DIRECTION_CONSTANT => "rtl";

    public static string INPUT_CONTROL_DEFAULT_MARGIN => "my-2";
    public static string MOBILE_LITERAL => "Mobile Number";
    public static string ENTRY_GROUP_LITERAL => "EntryFormGroup";
    public static string GENDER_LITERAL => "Gender";
    public static string PROFILE_GROUP_LITERAL => "ProfileGroup";

    public static string ORGANISATION_STRING_CONST => "organisation";
    public static string ORGANISATION_SETUP_STRING_CONST => "organisationsetup";
    public static string DIRECTION_STYLE_VARIABLE_STRING => "--Direction:";

    //public static string IMAGE_PREFIX => "data:{0};base64,";
    //public static long IMAGE_SIZE => 1024;
    public static int DELAY_IN_MILLISECONDS => 8000;
    public static long MAX_BUFFER_SIZE => 102_400_000;

    #endregion

    #region Menu Constants
    public static string SE_PAGE_ID_QUERY_KEY => "pageID";
    public static string SE_IS_EDUCATION_QUERY_KEY => "isEducation";
    public static string SE_IS_PUBLISH_UNPUBLISH_QUERY_KEY => "isPublishUnpublish";

    //regex for Web
    public const string DecimalRegexReplaceString = @"/[^\d.\+\-]/g";
    public const string DecimalRegexCheck1String = @"/^[+\-]?\d*(\.\d{0,";
    public const string DecimalRegexCheck2String = @"})?$/";
    public const string NumericRegexReplaceString = @"/[^0-9]/g";
    public const string PhoneRegexReplaceString = @"[0-9]";
    public const string FormattedStringRegex = @"<.*?>";
    public const string ImageMatchingRegex = @"<img\b[^>]*>";
    public const string REGEX_REMOVE_HTML_BLANK_SPACE = @"^(&nbsp;\s*)+|(&nbsp;\s*)+$";
    public const string REGEX_REMOVE_HTML_BLANK_SPACE_BETWEEN_TAGS = @">(?:&nbsp;\s*)+|(?:&nbsp;\s*)+<";

    public static string REGEX_ALPHA => @"^[a-zA-Z ]+$";
    public static string REGEX_NUMERIC => @"^[0-9]+$";
    public static string REGEX_ALPHA_NUMERIC => @"^[a-zA-Z0-9 ]+$";
    public static string REGEX_DECIMAL => @"^\d+(\.\d{1,2})?$";////@"^[0-9]*([0-9]+\.[0-9]{1,2})*$";
    public static string HTML_SYMBOLS => @"<(.|n)*?>";
    public static string HTML_BLANK_SPACE => "&nbsp;";
    public static string RED_ASTRICK_HTML => @"<span style='color: red;'>*</span>";
    public static string ACCEPT_COLOR => "color:#41AD49;";
    public static string ERROR_COLOR => "color:#E11D48;font-size:0.8rem;";
    public static string HTML_RICH_TEXT_END_TAG => @"</p></html>";

    #endregion

    #region Mapping Constants

    public static string CNT_OPTION_ID => "OptionID";
    public static string CNT_OPTION_TEXT => "OptionText";
    public static string CNT_PARENT_OPTIONID_TEXT => "ParentOptionID";
    public static string CNT_RESOURCE_VALUE_TEXT => "ResourceValue";
    public static string CNT_NAME_TEXT => "Name";
    public static string CNT_PATIENT_TEXT => "Patient";
    public static string CNT_USER_TEXT => "User";

    #endregion

    #region Onelink Constant 

    public static string SHORT_LINK_KEY => "shortLink";
    public static string DYNAMIC_LINK_INFO_KEY => "dynamicLinkInfo";
    public static string DYNAMIC_LINK_DOMAIN_PATH_PREFIX_KEY => "domainUriPrefix";
    public static string LINK_KEY => "link";
    public static string ANDROID_INFO_KEY => "androidInfo";
    public static string ANDROID_PACKAGE_NAME_KEY => "androidPackageName";
    public static string IOS_INFO_KEY => "iosInfo";
    public static string IOS_BUNDLE_ID_KEY => "iosBundleId";
    public static string IOS_APP_STORE_ID => "iosAppStoreId";
    public static string ORGANISATION_KEY => "/organisation/";

    #endregion

    #region Push Notification
    ////Push Notification
    public static string ANDROID_PUSH_NOTIFICATION_INTENT_NAME => "VhsCHMixRegistrationIntentService";
    public static string PUSH_NOTIFICATION_MESSAGE_KEY => "message";
    public static string PUSH_NOTIFICATION_TITLE_KEY => "title";
    public static string PUSH_NOTIFICATION_BADGE_KEY => "badge";
    public static string PUSH_NOTIFICATION_CATEGORY_KEY => "category";
    public static string PUSH_NOTIFICATION_ID_KEY => "notificationID";
    public static string PUSH_NOTIFICATION_IDENTIFIER_KEY => "notificationIdentifier";
    public static string NOTIFICATION_CHANNEL_ID => "atomChannelID";
    public static string NOTIFICATION_CHANNEL_NAME => "atomChannelName";
    public static string PORTAL_PUSH_NOTIFICATION_ID_KEY => "notificationNumber";
    public static string PORTAL_PUSH_NOTIFICATION_BADGE_KEY => "msgcnt";
    public static string PUSH_NOTIFICATION_BACKEND_TYPE_KEY => "BackendType";
    public static string PUSH_NOTIFICATION_BACKEND_IID_KEY => "BackendIID";

    #endregion
    /// <summary>
    /// 
    /// </summary>
    public static double MASTER_PAGE_DEFAULT_SIZE_RATIO => 0.1;

    #region HTTP SERVICE CONSTANTS

    /// <summary>
    ///  Request Headers
    /// </summary>
    public static string SE_CONTENT_TYPE_KEY => "Content-Type";
    /// <summary>
    ///  Request Headers
    /// </summary>
    public static string SE_AUTHORIZATION_HEADER_KEY => "Authorization";

    /// <summary>
    ///  Request Headers
    /// </summary>
    public static string SE_BEARER_TEXT_KEY => "Bearer ";

    /// <summary>
    ///   Content Type - JSON
    /// </summary>
    public static string SE_ACCEPT_HEADER_JSON_KEY => "application/json";

    /// <summary>
    ///   Content Type - FORM ENCODED
    /// </summary>
    public static string SE_ACCEPT_HEADER_FORM_ENCODED_KEY => "application/x-www-form-urlencoded";

    /// <summary>
    ///  Defaults
    /// </summary>      
    public static int HTTP_GET_TIMEOUT_MINUTE_INTERVAL => 5;

    /// <summary>
    ///  http service error returned for unknown certificate
    /// </summary>      
    public static string HTTP_CERTIFICATE_ERROR => "Ssl error:1000007d:SSL routines:OPENSSL_internal:CERTIFICATE_VERIFY_FAILED";

    /// <summary>
    ///  http service error returned for unknown certificate
    /// </summary>      
    public static string HTTP_CERTIFICATE_ERROR_01 => "Error: TrustFailure";

    /// <summary>
    ///  http service error returned for unknown certificate
    /// </summary>      
    public static string HTTP_CERTIFICATE_ERROR_02 => "CertificateUnknown";

    /// <summary>
    /// Suffix used to store certificates for environments
    /// </summary>
    public static string ENVIRONMENT_PUBLIC_KEY_SUFFIX => "PublicKey";

    #endregion

    /// <summary>
    /// Face type text
    /// </summary>
    public static string FACE_TYPE => "Face";

    /// <summary>
    /// Delete button text
    /// </summary>
    public static string DELETE_BUTTON_TEXT => "delete";
    /// <summary>
    /// Delete pincode button text
    /// </summary>
    public static string BASEKEYPAD_DELETE_BUTTON_TEXT => "X";

    /// <summary>
    /// svg file extension
    /// </summary>
    public static string SVG_EXTENSION => ".svg";

    /// <summary>
    /// png file extension
    /// </summary>
    public static string PNG_EXTENSION => ".png";

    /// <summary>
    /// error code message when resource is not loaded
    /// </summary>
    public static string ERROR_WHILE_RETRIEVING_RECORDS => "Error While Retrieving Records";

    /// <summary>
    /// Value: '/'
    /// </summary>
    public static char SYMBOL_SLASH => '/';

    /// <summary>
    /// Symbol used to seperate environment prefix
    /// </summary>
    public static string ENVIRONMENT_SEPERATOR => ":";

    /// <summary>
    /// Value: "en-US"
    /// </summary>
    public static string ENGLISH_US_LOCALE => "en-US";

    /// <summary>
    ///  Value: "."
    /// </summary>
    public static string SYMBOL_DOT_STRING => ".";

    /// <summary>
    ///  Value: ','
    /// </summary>
    public static char SYMBOL_COMMA => ',';

    /// <summary>
    ///  Value: '.'
    /// </summary>
    public static char DOT_SEPARATOR => '.';

    /// <summary>
    ///  Value: " "
    /// </summary>
    public static string STRING_SPACE => " ";

    /// <summary>
    ///  Value: ":"
    /// </summary>
    public static string SYMBOL_COLAN_STRING => ":";

    /// <summary>
    ///  Value: ":"
    /// </summary>
    public static char SYMBOL_COLAN => ':';

    /// <summary>
    ///  Value: ";"
    /// </summary>
    public static string SYMBOL_SEMI_COLAN_STRING => ";";

    /// <summary>
    ///  Value: ';'
    /// </summary>
    public static char SYMBOL_SEMI_COLAN => ';';

    /// <summary>
    ///  Value: "_"
    /// </summary>
    public static string SYMBOL_UNDERSCORE_STRING => "_";

    /// <summary>
    ///  Value: ';'
    /// </summary>
    public static char SYMBOL_SEMI_COLAN_CHAR => ';';

    /// <summary>
    ///  Value: " *"
    /// </summary>
    public static string IS_REQUIRED_FEILD_INDICATOR => " *";

    /// <summary>
    ///  Value: "hh:mm tt"
    /// </summary>
    public static string TIME_FORMAT => "hh:mm tt";

    /// <summary>
    ///  Value: "HH:mm "
    /// </summary>
    public static string DEFAULT_TIME_FORMAT => "HH:mm";

    /// <summary>
    ///  Value: "H:mm "
    /// </summary>
    public static string TWENTY_FOUR_HOUR_TIME_FORMAT => "H:mm";

    #region ChartControlConstants 
    public static string WEEKLY_FORMAT => "ddd";
    public static string MONTH_DAY_FORMAT => "MMM dd";

    #endregion
    /// <summary>
    ///  Value: "ddd, dd MMMM yyyy "
    /// </summary>
    public static string CALENDER_CONTROL_FORMAT => "ddd, dd MMMM yyyy ";

    public static string EXCEL_UPLOAD_DATE_TIME_STRING_FORMAT => "dd/MM/yyyy";

    public static string ISO_TIME_STRING_FORMAT => "HH:mm";
    public static string ISO_DATE_STRING_FORMAT => "yyyy-MM-dd";
    public static string DATE_TIME_STRING_FORMAT => $"{ISO_DATE_STRING_FORMAT}T{ISO_TIME_STRING_FORMAT}:ss.sssZ";

    /// <summary>
    ///  Value: '-'
    /// </summary>
    public static char SYMBOL_DASH => '-';

    /// <summary>
    /// Value: '|'
    /// </summary>
    public static char SYMBOL_BAR => '|';

    /// <summary>
    ///  Value: 250
    /// </summary>
    public static int ATTACHMENT_MAX_HEIGHT => 250;

    /// <summary>
    ///  Value: 300
    /// </summary>
    public static int ATTACHMENT_MAX_WIDTH => 300;

    /// <summary>
    ///  Value: 120
    /// </summary>
    public static int ATTACHMENT_MIN_HEIGHT => 120;

    /// <summary>
    ///  Value: 200
    /// </summary>
    public static int ATTACHMENT_MIN_WIDTH => 200;

    /// <summary>
    /// lab value category id
    /// </summary>
    public static int LAB_VALUE_CATEGORY_ID => 422;

    /// <summary>
    ///  Value: 0
    /// </summary>
    public static double ZERO_PADDING => 0;
    /// <summary>
    ///  Value: 0
    /// </summary>
    public static int ZERO_VALUE => 0;

    /// <summary>
    ///  Value: '|'
    /// </summary>
    public static char SYMBOL_PIPE_SEPERATOR => '|';

    /// <summary>
    ///  Value: "|"
    /// </summary>
    public static string PIPE_SEPERATOR => "|";

    /// <summary>
    ///  Value: "|"
    /// </summary>
    public static string PIPE_SEPERATOR_WITH_SPACE => " | ";

    /// <summary>
    ///  Value: "-"
    /// </summary>
    public static string DASH_INDICATOR => "-";

    /// <summary>
    ///  Value: "--"
    /// </summary>
    public static string SYMBOL_DOUBLE_HYPHEN => "--";

    /// <summary>
    ///  Value: "IsValid"
    /// </summary>
    public static string IS_VALID => "IsValid";

    /// <summary>
    ///  Value: '#'
    /// </summary>
    public static char SYMBOL_HASH => '#';

    /// <summary>
    ///  Value: '#'
    /// </summary>
    public static string SYMBOL_HASH_STRING => "#";

    /// <summary>
    ///  Value: "File: "
    /// </summary>
    public static string FILE_PATH => "File: ";

    /// <summary>
    ///  Value: "File:  "
    /// </summary>
    public static string FILE_PATH_EXTRA => "File:  ";

    /// <summary>
    ///  Value: "MMM d, yyyy"
    /// </summary>
    public static string DEFAULT_DATE_FORMAT => "MMM d, yyyy";

    /// <summary>
    ///   DAY Format
    /// </summary>
    public static string DEFAULT_DAY_FORMAT => "dd";

    /// <summary>
    ///   DAY Format
    /// </summary>
    public static string DEFAULT_EXTENDED_DAY_FORMAT => "dddd";
    public static string DEFAULT_MONTH_YEAR_DATE_FORMAT => "MM/yyyy";
    public static string DEFAULT_YEAR_FORMAT => "yyyy";
    public static string DEFAULT_MONTH_FORMAT => "MMM";
    public static string DEFAULT_DAY_MONTH_FORMAT => "dd MMM";
    public static string DEFAULT_MONTH_YEAR => "MMM yy";

    /// <summary>
    ///  Value: "Clear"
    /// </summary>
    public static string CLEAR_TEXT => "Clear";

    /// <summary>
    ///  Value: "Done"
    /// </summary>
    public static string DONE_TEXT => "Done";

    /// <summary>
    ///  Value: "Next"
    /// </summary>
    public static string NEXT_TEXT => "Next";

    /// <summary>
    ///  Value: "Title"
    /// </summary>
    public static string CELL_TITLE => "Title";

    /// <summary>
    ///  Value: 10
    /// </summary>
    public static int IMAGE_COMPRESSION_DROID_THUMBNAIL => 10;

    /// <summary>
    ///  Value: 60
    /// </summary>
    public static int IMAGE_COMPRESSION_DROID => 60;

    /// <summary>
    ///  Value: "file:///android_asset/pdfjs/web/viewer.html?file=file://"
    /// </summary>
    public static string ANDROID_PDF_FILE_PATH => "file:///android_asset/pdfjs/web/viewer.html?file=file://";

    /// <summary>
    ///  Value: "file:///android_asset/SampleExcel"
    /// </summary>
    public static string ANDROID_SAMPLE_EXCEL_FILE_PATH => "file:///android_asset/Excel";

    /// <summary>
    ///  Value: "file:///android_asset/SampleExcel"
    /// </summary>
    public static string IOS_SAMPLE_EXCEL_FILE_PATH => "file:///android_asset/SampleExcel";

    /// <summary>
    ///  Value: 0.10f
    /// </summary>
    public static float IMAGE_COMPRESSION_IOS_THUMBNAIL => 0.10f;

    /// <summary>
    ///  Value: 0.6f
    /// </summary>
    public static float IMAGE_COMPRESSION_IOS => 0.6f;

    /// <summary>
    ///  Value: "FileName"
    /// </summary>
    public static string FILE_NAME_PROPERTY => "FileName";

    /// <summary>
    ///  Value: "_and_"
    /// </summary>
    public static string ONELINK_SEPERATOR_KEY => "_and_";

    /// <summary>
    ///  Value: "?"
    /// </summary>
    public static string SYMBOL_QUESTION_MARK => "?";

    /// <summary>
    ///  Value:SYMBOLAMPERSAND
    /// </summary>
    public static string SYMBOL_AMPERSAND => "&";

    /// <summary>
    ///  Value: "@"
    /// </summary>
    public static string SYMBOL_AT_THE_RATE => "@";

    /// <summary>
    ///  Value: "="
    /// </summary>
    public static string SYMBOL_EQUAL => "=";

    /// <summary>
    /// Activity to select a image id for getting action result in main activity
    /// </summary>
    public static int PICK_IMAGE_TASK_ID => 1000;

    /// <summary>
    /// Attachment preview task id for getting action result in main activity
    /// </summary>
    public static int ATTACHMENT_PREVIEW_TASK_ID => 1001;

    /// <summary>
    /// notification payload title
    /// </summary>
    public static string NOTIFICATION_PAYLOAD_TITLE => "title";

    /// <summary>
    /// notification payload alert
    /// </summary>
    public static string NOTIFICATION_PAYLOAD_ALERT => "alert";

    /// <summary>
    /// notification payload aps
    /// </summary>
    public static string NOTIFICATION_PAYLOAD_APS => "aps";

    /// <summary>
    /// Notification message key
    /// </summary>
    public static string NOTIFICATION_MESSAGE_KEY => "message";

    /// <summary>
    /// Notification id key
    /// </summary>
    public static string NOTIFICATION_ID_PREMIUM_KEY => "notificationNumber";

    /// <summary>
    /// Notification badge key 
    /// </summary>
    public static string NOTIFICATION_BADGE_PREMIUM_KEY => "msgcnt";

    /// <summary>
    /// Notification id key
    /// </summary>
    public static string NOTIFICATION_ID_KEY => "notificationID";

    /// <summary>
    /// Notification badge key
    /// </summary>
    public static string NOTIFICATION_BADGE_KEY => "badge";

    /// <summary>
    /// Notification backend type key
    /// </summary>
    public static string NOTIFICATION_BACKEND_TYPE_KEY => "BackendType";

    /// <summary>
    /// Notification Backend type key
    /// </summary>
    public static string NOTIFICATION_BACKEND_IID_KEY => "BackendIID";

    /// <summary>
    /// Notification category key
    /// </summary>
    public static string NOTIFICATION_CATEGORY_KEY => "category";

    /// <summary>
    /// Notification parameter key
    /// </summary>
    public static string NOTIFICATION_IDENTIFIER_KEY => "notificationIdentifier";

    /// <summary>
    /// Notification default channel name
    /// </summary>
    public static string NOTIFICATION_DEFAULT_CHANNEL_NAME => "defaultChannel";

    /// <summary>
    /// default country Code
    /// </summary>
    public static string DEFAULT_COUNTRY_CODE => "IN";

    /// <summary>
    /// notification alert ok
    /// </summary>
    public static string NOTIFICATION_ALERT_BOX_OK => "OK";

    /// <summary>
    /// notification alert Cancel
    /// </summary>
    public static string NOTIFICATION_ALERT_BOX_CANCEL => "Cancel";

    /// <summary>
    /// notification payload body
    /// </summary>
    public static string NOTIFICATION_PAYLOAD_BODY => "body";

    /// <summary>
    /// Notification device type Android
    /// </summary>
    public static string DEVICE_TYPE_ANDROID => "Android";

    /// <summary>
    /// Notification device type - Apple
    /// </summary>
    public static string DEVICE_TYPE_APPLE => "Apple";

    /// <summary>
    /// Message to be displayed when app is not available to display file
    /// </summary>
    public static string APP_NOT_AVAILABLE_MESSAGE => "No application available to view this file";

    /// <summary>
    /// Base route for navigation
    /// </summary>
    public static string BASE_FOOTER_ROUTE => "Footer";

    /// <summary>
    /// Route prefix for base
    /// </summary>
    public static string BASE_ROUTE_PREFIX => $"//{BASE_FOOTER_ROUTE}/";

    /// <summary>
    /// Base dummy route for navigation
    /// </summary>
    public static string BASE_DUMMY_FOOTER_ROUTE => "Dummy";

    /// <summary>
    /// Route prefix for base
    /// </summary>
    public static string KEYBOARD_DIAPPEARS => "KeyboardDisappears";

    /// <summary>
    /// Static message page
    /// </summary>
    public static string STATIC_MESSAGE_PAGE => "StaticMessagePage";

    /// <summary>
    /// comma seperator
    /// </summary>
    public static string SYMBOL_COMMA_SEPERATOR_STRING => ",";

    /// <summary>
    /// Single inverted comma seperator
    /// </summary>
    public static string SYMBOL_SINGE_INVERTED_COMMA_SEPERATOR_STRING => "'";

    /// <summary>
    /// Digits after decimal
    /// </summary>
    public static int DIGITS_AFTER_DECIMAL => 1;

    /// <summary>
    /// chat font attribute
    /// </summary>
    public static string CHAT_FONT_ATTRIBUT => "ChatFontAttribute";

    /// <summary>
    /// SearchButtonClicked
    /// </summary>
    public static string SEARCH_BUTTON_CLICKED => "SearchButtonClicked";

    /// <summary>
    /// TextChanged
    /// </summary>
    public static string TEXT_CHANGED => "TextChanged";

    /// <summary>
    /// text
    /// </summary>
    public static string TEXT_STRING => "text";

    /// <summary>
    /// password
    /// </summary>
    public static string PASSWORD_STRING => "password";

    /// <summary>
    /// Constant to identify static message page
    /// </summary>
    public static string STATIC_MESSAGE_PAGE_IDENTIFIER => "StaticMessagePageIdentifier";

    #region Directly Jump to specific Page (Number Strings)

    /// <summary>
    /// Number 0 string
    /// </summary>
    public const string NUMBER_ZERO = "0";

    /// <summary>
    /// Number 1 string
    /// </summary>
    public const string NUMBER_ONE = "1";

    /// <summary>
    /// Number 2 string
    /// </summary>
    public const string NUMBER_TWO = "2";

    /// <summary>
    /// Number 3 string
    /// </summary>
    public const string NUMBER_THREE = "3";

    /// <summary>
    /// Number 4 string
    /// </summary>
    public const string NUMBER_FOUR = "4";

    /// <summary>
    /// Number 5 string
    /// </summary>
    public const string NUMBER_FIVE = "5";

    /// <summary>
    /// Number 6 string
    /// </summary>
    public const string NUMBER_SIX = "6";

    /// <summary>
    /// Number 7 string
    /// </summary>
    public const string NUMBER_SEVEN = "7";

    /// <summary>
    /// Number 8 string
    /// </summary>
    public const string NUMBER_EIGHT = "8";

    /// <summary>
    /// Number 9 string
    /// </summary>
    public const string NUMBER_NINE = "9";

    /// <summary>
    /// Resource not added yet
    /// </summary>
    public static string MESSAGE_NEEDS_TO_BE_CONFIGURED => "This message needs to be configured.!!";

    /// <summary>
    /// Image thumbnail width and height
    /// </summary>
    public static string IMAGE_THUMBNAIL_SIZE => "200";

    /// <summary>
    /// image large width and height
    /// </summary>
    public static string IMAGE_LARGE_SIZE => "1000";

    #endregion

    /// <summary>
    /// AUTOMATION_ID_SEPRATOR
    /// </summary>
    public static string AUTOMATION_ID_SEPRATOR => ".A1";

    /// <summary>
    /// Style to apply default font on HTML text
    /// </summary>
    public static string DEFAULT_FONT_HTML => "<html><link href='https://fonts.googleapis.com/css?family=Roboto' rel='stylesheet'><style> body {  font-family:' Roboto', sans-serif;  color: #212121; }</style><body>{0}</body></html>";

    /// <summary>
    /// XKey Const
    /// </summary>
    public static string XKey => "xaxis";

    /// <summary>
    /// YKey Cosnt
    /// </summary>
    public static string YKey => "yaxis";

    /// <summary>
    /// break string
    /// </summary>
    public static string STRING_BREAK_TAG => "<br>";

    /// <summary>
    /// HTML center tag
    /// </summary>
    public static string HTML_CENTER_TAG => @"<style>body {text-align: center;}</style>";
    /// <summary>
    /// BLANK_SOURCE
    /// </summary>
    public static string BLANK_SOURCE => "about:blank";

    /// <summary>
    /// Delay to handle paramater read issue
    /// </summary>
    public static int PARAMETER_READ_DELAY => 10;

    /// <summary>
    /// dummy.pdf name
    /// </summary>
    public static string DUMMY_PDF => "dummy.pdf";

    /// <summary>
    ///pdf name
    /// </summary>
    public static string CONSTANT_PDF => ".pdf";

    /// <summary>
    ///pdf name
    /// </summary>
    public static string CONSTANT_YOUTUBE => "youtube";

    /// <summary>
    ///iframe format
    /// </summary>
    public static string CONSTANT_YOUTUBE_VIDEO_FRAME => "<iframe width=\"100%\" height=\"315\" src=\"{0}\" frameborder =\"0\" allowfullscreen></iframe>";

    /// <summary>
    /// Value: "{0}"
    /// </summary>
    public static string FORMATTED_STRING_PARAMETER_CONSTANT_ZERO => "{0}";

    /// <summary>
    /// Value: "{1}"
    /// </summary>
    public static string FORMATTED_STRING_PARAMETER_CONSTANT_ONE => "{1}";

    /// <summary>
    /// Value: "{2}"
    /// </summary>
    public static string FORMATTED_STRING_PARAMETER_CONSTANT_TWO => "{2}";

    /// <summary>
    /// Value: "{3}"
    /// </summary>
    public static string FORMATTED_STRING_PARAMETER_CONSTANT_THREE => "{3}";

    /// <summary>
    /// Value: "{3}"
    /// </summary>
    public static string FORMATTED_STRING_PARAMETER_CONSTANT_FOUR => "{4}";

    /// <summary>
    /// Value: "{3}"
    /// </summary>
    public static string FORMATTED_STRING_PARAMETER_CONSTANT_FIVE => "{5}";

    /// <summary>
    /// Value: "{3}"
    /// </summary>
    public static string FORMATTED_STRING_PARAMETER_CONSTANT_SIX => "{6}";

    /// <summary>
    /// Value: "{3}"
    /// </summary>
    public static string FORMATTED_STRING_PARAMETER_CONSTANT_SEVEN => "{7}";

    #region Special Reading Type constants

    ///// <summary>
    ///// Reading type as blood pressure
    ///// </summary>
    //public static string READING_BLOOD_PRESSURE => "BloodPressure";

    ///// <summary>
    ///// Reading type as blood pressure systolic
    ///// </summary>
    //public static string READING_BP_SYSTOLIC => "BPSystolic";

    ///// <summary>
    ///// Reading type as blood pressure diastolic
    ///// </summary>
    //public static string READING_BP_DIASTOLIC => "BPDiastolic";

    ///// <summary>
    ///// Reading type as blood glucose
    ///// </summary>
    //public static string READING_BLOOD_GLUCOSE => "BloodGlucose";

    ///// <summary>
    ///// Reading type as blood pressure
    ///// </summary>
    //public static string READING_STEPS => "Steps";

    ///// <summary>
    ///// Reading type as BMI
    ///// </summary>
    //public static string READING_BMI => "BMI";

    ///// <summary>
    ///// Reading type as Height
    ///// </summary>
    //public static string READING_HEIGHT => "Height";

    ///// <summary>
    ///// Reading type as Weight
    ///// </summary>
    //public static string READING_WEIGHT => "Weight";

    ///// <summary>
    ///// Reading Type Insulin
    ///// </summary>
    //public static string READING_INSULIN => "Insulin";

    ///// <summary>
    ///// Reading Type Nutritions
    ///// </summary>
    //public static string READING_NUTRITIONS => "Nutritions";

    ///// <summary>
    ///// Reading Type Nutrition
    ///// </summary>
    //public static string READING_NUTRITION => "Nutrition";

    ///// <summary>
    ///// Reading Type Hydration
    ///// </summary>
    //public static string READING_HYDRATION => "Hydration";

    ///// <summary>
    ///// Reading Category Food
    ///// </summary>
    //public static string READING_CATEGORY_FOOD => "Food";

    /// <summary>
    /// SVG Image Prefix
    /// </summary>
    public static string SVG_BASE64_PREFIX => "data:image/svg+xml;base64,";
    #endregion

    /// <summary>
    /// SVG Image Prefix
    /// </summary>
    public static string REMOVE_ALPHA_PREFIX => "#FF";

    #region
    /// <summary>
    /// Weekly Constant, 7 days
    /// </summary>
    public static int PLOT_DAYS_WEEKLY => 7;
    public static int PLOT_DAYS_DAILY => 1;

    /// <summary>
    /// Bi Weekly Constant, 15 days
    /// </summary>
    public static int PLOT_DAYS_BI_WEEKLY => 15;

    /// <summary>
    /// Monthly Constant, 31 days
    /// </summary>
    public static int PLOT_DAYS_MONTLY => 31;

    /// <summary>
    /// Quartely Constant, 92 days
    /// </summary>
    public static int PLOT_DAYS_QUARTERLY => 92;

    /// <summary>
    /// Yearly Constant, 366 days
    /// </summary>
    public static int PLOT_DAYS_YEARLY => 366;

    /// <summary>
    /// Yearly Constant, 24 Hrs
    /// </summary>
    public static int PLOT_DAYS_HOURS => 24;

    /// <summary>
    /// Monhly Step Constant, 30.5 days
    /// </summary>
    public static double PLOT_DAYS_MONTHLY_STEP => 29.5;

    /// <summary>
    /// Yearly Step Constant, 365.25 days
    /// </summary>
    public static double PLOT_DAYS_YEARLY_STEP => 365.25;
    #endregion

    /// <summary>
    /// NEW_LINE
    /// </summary>
    public static string NEW_LINE_CONSTANT => "<br>";

    /// <summary>
    /// Parameter of View type of list
    /// </summary>
    public static string VIEW_TYPE_STRING => "ViewType";

    /// <summary>
    /// Paameter of display filetrs
    /// </summary>
    public static string DISPLAY_CATEGORY_FILTER_STRING => "DisplayCategoryFilter";

    /// <summary>
    /// LARGE WIDTH REQUEST CONSTANT
    /// </summary>
    public static double LARGE_WIDTH_REQUEST_CONSTANT => 90;

    /// <summary>
    /// Maximum number of tabs to show
    /// </summary>
    public static int MAX_TABS_ALLOWED => 5;

    /// <summary>
    /// Tabs more option
    /// </summary>
    public static string TABS_MORE_OPTION_CONSTANT => "MoreOption";

    /// <summary>
    /// Patients Page
    /// </summary>
    public static string PATIENTS_PAGE_CONSTANT => "PatientsPage";

    /// <summary>
    /// Dashboard Page
    /// </summary>
    public static string DASHBOARD_PAGE_CONSTANT => "DashboardPage";

    /// <summary>
    /// Patient Targets
    /// </summary>
    public static string PATIENT_TARGETS => "PatientTargets";

    /// <summary>
    /// Patient Readings tile height
    /// </summary>
    public static double PATIENT_READINGS_TILE_HEIGHT => 95;

    /// <summary>
    /// Patient Readings tile height
    /// </summary>
    public static double PATIENT_DETAILS_MAX_HEIGHT => 700;

    /// <summary>
    /// Renderer
    /// </summary>
    public static string RENDERER => "Renderer";

    /// <summary>
    /// Application String
    /// </summary>
    public static string APPLICATION_STRING => "application";

    /// <summary>
    /// Html Label Font
    /// </summary>
    public static string HTML_LABEL_FONT => "Helvetica";

    /// <summary>
    /// Update Html Values
    /// </summary>
    public static string HTML_SET_VALUES => "SetValues";

    /// <summary>
    ///  Company name constant
    /// </summary>
    public static string COMPANY_NAME => "@companyname";

    /// <summary>
    /// Constant for base64 file format
    /// </summary>
    public static string BASE64_STRING_CONSTANT => "base64string";

    /// <summary>
    /// Constant for Image Height
    /// </summary>
    public static string IMAGE_HEIGHT_CONSTANT => "imageHeight";

    /// <summary>
    /// Constant for Image Width
    /// </summary>
    public static string IMAGE_WIDTH_CONSTANT => "imageWidth";

    /// <summary>
    /// Constant for default value
    /// </summary>
    public static string DEFAULT_VALUE_CONSTANT => "defaultValue";

    /// <summary>
    /// Constant for Is Cricle
    /// </summary>
    public static string IS_CIRCLE_CONSTANT => "isCircle";

    /// <summary>
    /// Constant for checking if the operation is Add Patient or not?
    /// </summary>
    public static string IS_PATIENTS_ADD_PAGE => "PatientsPage";

    /// <summary>
    /// Constant for Excel folder location
    /// </summary>
    public static string EXCEL_FOLDER => "Excel";

    /// <summary>
    /// Constant for web site Home location
    /// </summary>
    public static string HOME_VARIABLE => "HOME";

    /// <summary>
    /// Constant for web site
    /// </summary>
    public static string SITE_VARIABLE => "site";

    /// <summary>
    /// Constant forweb site www folder location
    /// </summary>
    public static string WWWROOT_VARIABLE => "wwwroot";
    /// <summary>
    /// Constant for active status
    /// </summary>
    public static string ACTIVE_VARIABLE => "Active";
    /// <summary>
    /// Constant for inactive status
    /// </summary>
    public static string INACTIVE_VARIABLE => "InActive";

    /// <summary>
    /// Number 1 byte
    /// </summary>
    public static byte NUMBER_ONE_VALUE => 1;

    /// <summary>
    /// Number 2 string
    /// </summary>
    public static byte NUMBER_TWO_VALUE => 2;

    /// <summary>
    /// Number 3 string
    /// </summary>
    public static byte NUMBER_THREE_VALUE => 3;

    public static string SMALL_TEXT_BOX_HEIGHT => "100px";
    public static string CONSTANT_ZERO => "0";
    public static string CONSTANT_NEG_ONE => "-1";
    public static string TIME_FORMATTER => "2-digit";
    public static string APPOINTMENT_VIEW_DATE_TIME_STRING_SEQUENCE => "{0} - {1}";
    public static string USER_TAG_PREFIX => "U";
    public static string ACCOUNT_TAG_PREFIX => "A";
    public static string ORGANISATION_TAG_PREFIX => "O";
    public static double CONTENT_PADDING => 5;
    public static double SLIDER_DEFAULT_VALUE => 25;
    public static double SLIDER_VERTICAL_HEIGHTREQUEST_VALUE => 120;
    public static double SLIDER_INTERVAL_VALUE => 5;
    public static double TAB_LIST_HEIGHT => 40;
    public static double TAB_LIST_WIDTH => 150;
    public static double MORE_BUTTON_WIDTH => 35;
    public static double WIDTH_REQUEST_CONSTANT => 70;
    public static string SCROLL_BAR => "scrollBar";
    public static string SCROLL_TO_BOTTOM => "scrollToBottom";
    public static string FILE_SAVE_AS => "FileSaveAs";
    public static string EXCEL_SAVE_AS => "ExcelSaveAs";
    public static string EXCEL_PREFIX => "data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,";
    public static string BTN_PRIMARY_CSS => "btn-primary padding-horizontal-md ";
    public static string BTN_PRIMARY_MAX_WIDTH_CSS => "btn-primary padding-horizontal-md max-content-width ";
    public static string BTN_SECONDARY_CSS => "btn-secondary padding-horizontal-md ";
    public static string BADGE_NUMBER_CSS => "badge-number";
    public static string BADGE_SUCCESS_CSS => "badge-success";
    public static string BADGE_ERROR_CSS => "badge-error";
    public static string DELETE_OK_CSS => "btn btn-primary float-right";
    public static string DELETE_CANCEL_CSS => "btn btn-secondary margin-horizontal-md float-right";
    public static string CALENDER_BACKGROUND_COLOR_CSS => "rgba(0,120,211,.2)";
    public static string CALENDER_TEXT_COLOR_CSS => "rgb(0,120,211)";
    public static string CALENDER_TYPE => "dayGridMonth";

    public static string SE_PROGRAM_ID_QUERY_KEY => "programID";
    public static string HTTP_TAG_PREFIX => "https:";
    public static string IMAGE_START_TAG => "<img src=";
    public static string END_TAG => @">";
    public static string EDITOR_DEFAULT_TAG => @"<p><br></p>";
    public static string HTML_TAG_CHECK => @"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>";
    public static string CATEGORY_DESCRIPTION_TEXT => "CategoryDetails";
    public static string CATEGORY_IMAGE_SOURCE => "CategoryImageSource";
    public static string HTML_TAGS => "<.*?>";
    public static string CATEGORY_PAGEDATA_TEXT => "PageData";

    public static string SEE_MORE => "SEE MORE";
    public static string STRING_FROMAT => "{0}";
    public static string FOR_PROVIDER_CONSTANT => "ForProvider";


    public static string TRUE => "true";
    public static string FALSE => "false";
    public static string SHORT => "short";
    public static string TODAYGRID => "today";
    public static string TIMEGRID => "timeGridPlugin";
    public static string TIME_GRID_DAY => "timeGridDay";
    public static string TIME_GRID_WEEK => "timeGridWeek";
    public static string DAY_GRID_MONTH => "dayGridMonth";
    public static string DAYGRID => "dayGridPlugin";
    public static string LEFT_SETTING => "prev,timeGridDay,timeGridWeek,dayGridMonth";
    public static string RIGHT_SETTING => "title,next";
    public static string INTERACTION => "interaction";

    public static string ASTERISK => "*";
    public static string PDF_FILE_EXTENSION => ".pdf";
    public static string ANDROID_PDF_FILE_NAME => "ContentData.pdf";
    public static string INSTRUCTION_KEY => "InstructionKey";
    public static int DATE_RENDER_DELAY => 10;

    /// <summary>
    /// Value "="
    /// </summary>
    public static char SYMBOL_CHAR_EQUAL => '=';

    public static string FOR_APPLICATION => "forApplication";
    public static string ROOM_ID => "roomID";
    public static string USER_ID => "userId";
    public static string USER_NAME => "userName";

    public static string PATIENT_MENU_HEADER => "Menu";

    /// <summary>
    /// Regex used for color picker
    /// </summary>
    public static string COLOR_REGEX_PATTERN => "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{8}|[A-Fa-f0-9]{3})$";

    #region Google Analytics

    public static string GA_OPERATING_SYSTEM => "operating_system";
    public static string GA_OS_VERSION => "os_version";
    public static string GA_DEVICE_MANUFACTURER => "device_manufacturer";
    public static string GA_DEVICE_MODEL => "device_model";
    public static string GA_PAGE_NAME => "page_name";
    public static string GA_DATETIME => "datetime";
    public static string GA_DEVICE_LOG_INFO => "device_log_info";
    public static string GA_NAVIGATION_PATH => "navigation_path";

    #endregion

    #region Questionnaire
    public static string IF_BLANK => "ifBlank";
    public static string ADD_BETWEEN_CONDITION => "AddBetweenCondition";
    public static string DDL_TOOLTIP => "ddl_tooltip";
    public static string ELSE => "else";
    public static string NEXT_QUESTION => "nextQuestion";
    public static string TXT_VALUE_1 => "txt_value1";
    public static string TXT_VALUE_2 => "txt_value2";
    public static string TXT_SCORE => "txt_score";
    public static string ZERO => "0";
    public static string MINUS_ONE => "-1";
    public static string TENTHOUSAND => "-10000";
    public static string FIFTEENTHOUSAND => "-15000";
    public static string FILE_LIST => "FileList";
    public static string BUTTON => "Button";

    public static string IS_COMMING_FROM_QUESTIONNAIRE_VIEW => "IsCommingFromQuestionnaireView";

    #endregion

    public static string UPDATED_EXCEL_STATUS => "UpdatedExcelStatus.xlsx";

    public static string ShowPm => "PM";
    public static string ShowAm => "AM";

    #region RichText
    public static string TOOL_BAR1_TEXT => "toolbar1";
    public static string TOOL_BAR2_TEXT => "toolbar2";
    public static string TOOL_BAR1_VALUE => "undo redo | styleselect |  forecolor backcolor | bold italic underline strikethrough superscript subscript | link image media code| table spellchecker";
    public static string TOOL_BAR2_VALUE => "h1 h2 h3 | bullist numlist | alignleft aligncenter alignright alignjustify | outdent indent |hr";
    public static string PLUGINS_TEXT => "plugins";
    public static string PLUGINS_VALUE => "advlist autolink lists link image charmap preview anchor searchreplace visualblocks code fullscreen insertdatetime media table code help wordcount";
    public static string MENU_BAR_TEXT => "menubar";
    public static string BRANDING_TEXT => "branding";
    public static string PLACEHOLDER_TEXT => "placeholder";
    #endregion

    public static int PDF_KEY_ID => 1121;
    public static int CONTENT_KEY_ID => 146;
    public static int LINKS_KEY_ID => 147;

    public static string INDIAN_CURRENCY => "INR";
}