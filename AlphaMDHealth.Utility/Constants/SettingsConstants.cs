namespace AlphaMDHealth.Utility;

public class SettingsConstants
{
    #region RSStoreLinksGroup

    /// <summary>
    /// Setting key of android playstore url
    /// </summary>
    public static string S_ANDROID_PLAYSTORE_LINK_KEY => "AndroidPlaystoreLinkKey";

    /// <summary>
    /// Setting key of ios Appstore url
    /// </summary>
    public static string S_IOS_APPSTORE_LINK_KEY => "IosAppstoreLinkKey";

    /// <summary>
    /// Setting key of Windows market place url
    /// </summary>
    public static string S_WINDOWS_MARKET_LINK_KEY => "WindowsMarketLinkKey";

    /// <summary>
    /// Setting key for latest version of app
    /// </summary>
    public static string S_CURRENT_APP_VERSION_KEY => "CurrentAppVersionKey";

    /// <summary>
    /// Setting key to check if force update is enabled for application
    /// </summary>
    public static string S_APP_FORCE_UPDATE_KEY => "AppForceUpdateKey";

    #endregion

    /// <summary>
    /// Pincode sequence match regex key
    /// </summary>
    public static string S_PINCODE_SEQUENCE_MATCH_REGEX_KEY => "PincodeSequenceMatchRegexKey";

    /// <summary>
    /// Pincode strength match regex key
    /// </summary>
    public static string S_PINCODE_STRENGTH_MATCH_REGEX_KEY => "PincodeStrengthMatchRegexKey";

    /// <summary>
    /// Max login attempts key
    /// </summary>
    public static string S_MAX_LOGIN_ATTEMPTS_KEY => "MaxLoginAttemptsKey";

    /// <summary>
    /// Otp length key
    /// </summary>
    public static string S_OTP_LENGTH_KEY => "OTPLengthKey";

    /// <summary>
    /// Pincode length key
    /// </summary>
    public static string S_PINCODE_LENGTH_KEY => "PincodeLengthKey";

    /// <summary>
    /// Resend otp duration key
    /// </summary>
    public static string S_OTP_RESEND_DURATION_KEY => "OTPResendDurationKey";

    /// <summary>
    /// App inactive auto lock duration
    /// </summary>
    public static string S_INACTIVE_DURATION_KEY => "InactiveDurationKey";

    /// <summary>
    /// Html Prefix For Validation Message Text
    /// </summary>
    public static string S_HTML_PREFIX_FOR_VALIDATION_MESSAGE_TEXT_KEY => "HtmlPrefixForValidationMessageTextKey";

    /// <summary>
    /// Date day format key
    /// </summary>
    public static string S_DATE_DAY_FORMAT_KEY => "DateDayFormatKey";

    /// <summary>
    /// Date month format key
    /// </summary>
    public static string S_DATE_MONTH_FORMAT_KEY => "DateMonthFormatKey";

    /// <summary>
    /// date Year format key
    /// </summary>
    public static string S_DATE_YEAR_FORMAT_KEY => "DateYearFormatKey";

    /// <summary>
    /// Key to check self registration allowed on not
    /// </summary>
    public static string S_ENABLE_SELF_REGISTRATION_KEY => "EnableSelfRegistrationKey";

    /// <summary>
    /// Year format setting key
    /// </summary>
    public static string S_EXTENDED_DAY_FORMAT_KEY => "ExtendedDayFormatKey";

    /// <summary>
    /// Month format key
    /// </summary>
    public static string S_MONTH_FORMAT_KEY => "MonthFormatKey";

    /// <summary>
    /// Year format setting key
    /// </summary>
    public static string S_YEAR_FORMAT_KEY => "YearFormatKey";

    /// <summary>
    /// Bluetooth scan timeout
    /// </summary>
    public static string S_BLE_DEVICE_SCAN_TIMEOUT_KEY => "BleDeviceScanTimeoutKey";

    /// <summary>
    /// medicine reminder time key
    /// </summary>
    public static string MEDICINE_REMINDER_TIME_KEY => "MedicineReminderTimeKey";

    #region RSSecurityGroup

    public static string S_IS_CERTIFIATE_PINING_ALLOWED_KEY => "IsCertifiatePiningAllowedKey";
    public static string S_IS_JAIL_BREAKING_DEVICE_ALLOWED_KEY => "IsJailBrokenDeviceAllowedKey";
    public static string S_IS_SCREEN_SHOT_ALLOWED_KEY => "IsScreenshotAllowedKey";
    public static string S_IS_TAP_JACKING_ALLOWED_KEY => "IsTapJackingAllowedKey";

    #endregion

    #region RSOrganisationSettingsGroup

    public static string S_ORGANISATION_TERMS_OF_USE_URL => "OrgansiationTermsOfUseUrl";
    public static string S_ORGANISATION_PRIVACY_URL => "OrgansiationPrivacyUrl";
    public static string S_ANALYTICS_ON_OFF_KEY => "AnalyticsOnOffKey";

    #endregion

    public static string S_LOGIN_LOCKOUT_DURATION_KEY => "LoginLockoutDurationKey";
    public static string S_INITIAL_SYNC_DAYS_FOR_READINGS_KEY => "InitialSyncDaysForReadingsKey";
    public static string S_IS_OFFLINE_SUPPORT_AVAILABLE_KEY => "IsOfflineSupportAvailableKey";
    public static string S_NUMERIC_REGEX_KEY => "NumericRegexKey";
    public static string S_DECIMAL_REGEX_KEY => "DecimalRegexKey";
    public static string S_DYNAMIC_LINK_USERNAME_KEY => "Username";
    public static string S_DYNAMIC_LINK_PASSWORD_KEY => "Password";
    public static string S_TWO_FACTOR_ENABLED => "TwoFactorEnabled";
    public static string S_PRIMARY_APP_COLOR_KEY => "PrimaryAppColorKey";

    //////#region CommonGroup
    public static string S_SYNC_TO_SERVER_WAIT_TIME_MINUTES_KEY => "SyncToServerWaitTimeMinutesKey";

    public static string S_EMAIL_REGEX_KEY => "EmailRegexKey";
    public static string S_ALPHA_REGEX_KEY => "AlphaRegexKey";
    public static string S_ALPHA_NUMERIC_REGEX_KEY => "AlphaNumericRegexKey";
    public static string S_DOMAIN_REGEX_KEY => "DomainRegexKey";
    public static string S_IS_READ_COPY_CLIPBOARD_ALLOWED_KEY => "IsReadCopyClipboardAllowedKey";
    public static string S_UPLOAD_IMAGE_TYPE_KEY => "UploadImageTypeKey";
    public static string S_UPLOAD_ICON_TYPE_KEY => "UploadIconTypeKey";
    public static string S_UPLOAD_FILE_TYPE_KEY => "UploadFileTypeKey";
    public static string S_UPLOAD_SUPPORTED_FILE_TYPE_KEY => "UploadSupportedFileTypeKey";
    public static string S_MAX_FILE_UPLOAD_KEYS => "MaxFileUploadSizeKey";
    public static string S_IMAGE_COMPRESSED_RESOLUTION_KEY => "ImageCompressedResolutionKey";
    public static string S_SMALL_IMAGE_RESOLUTION_KEY => "SmallImageResolutionKey";
    public static string S_LARGE_IMAGE_RESOLUTION_KEY => "LargeImageResolutionKey";
    public static string S_TEXT_BODY_KEY => "TextBodyKey";
    public static string S_PASSWORD_REGEX_KEY => "PasswordRegexKey";
    public static string S_GENERIC_TEXT_REGEX_KEY => "GenericTextRegexKey";
    public static string S_PATH_REGEX_KEY => "URLRegexKey";
    public static string S_LIST_SUB_HEADER_LENGTH_KEY => "ListSubHeaderLengthKey";
    public static string S_LOGO_KEY => "LogoKey";
    public static string S_HEADER_BACKGROUND_COLOR_KEY => "HeaderBackgroundColorKey";
    public static string S_HEADER_TEXT_COLOR_KEY => "HeaderTextColorKey";
    public static string S_HEADER_SELECTION_COLOR_KEY => "HeaderSelectionColorKey";
    public static string S_FOOTER_BACKGROUND_COLOR_KEY => "FooterBackgroundColorKey";
    public static string S_FOOTER_TEXT_COLOR_KEY => "FooterTextColorKey";
    public static string S_FOOTER_SELECTION_COLOR_KEY => "FooterSelectionColorKey";

    ////One link settings
    public static string S_MASTER_ORGANISATION_DOMAIN_PATH_KEY => "MasterOrganisationDomainUrlKey";
    public static string S_FIREBASE_APP_ID_KEY => "FirebaseAppIdKey";
    public static string S_DOMAIN_PATH_PREFIX_KEY => "DomainUriPrefixKey";
    public static string S_ONE_LINK_PARAMETERS_KEY => "OneLinkParametersKey";
    public static string S_MOBILE_APP_BUNDLE_IDENTIFIER_KEY => "MobileAppBundleIdentifierKey";
    public static string S_IOS_APP_STORE_ID_KEY => "IosAppStoreIdKey";

    //Appointment Settings
    public static string S_APPOINTMENT_DEFAULT_TIMEOUT_KEY => "AppointmentDefaultTimeOutKey";
    //Azure cdn settings
    public static string S_AZURE_CDN_LINK_KEY => "AzureCDNUrlKey";
    public static string S_AZURE_CDN_TOKEN_KEY => "AzureCDNTokenKey";

    //Micro Service

    public static string S_HTML_WRAPPER_KEY => "HTMLWrapperKey";


    //Task Group
    public static string S_INSTRUCTION_TEXT_LENGTH_KEY => "InstructionTextLengthKey";

    public static string S_DEFAULT_COUNTRY_CODE_PATH_KEY => "DefaultCountryCodeUrlKey";

    public static string S_MEDICATION_UNIT_GROUP_KEY => "MedicationUnitGroup";

    //Medication
    public static string S_SNOOZE_TIMEOUT_KEY => "SnoozeTimeoutKey";

    //Push Notification
    public static string S_NOTIFICATION_SECRET_KEY => "NotificationSecretKey";
    public static string S_NOTIFICATION_CLIENT_ID_KEY => "NotificationClientIDKey";
    public static string S_NOTIFICATION_BASE_URL_KEY => "NotificationBaseURLKey";
    public static string S_NOTIFICATION_REGISTRATION_URL_KEY => "NotificationRegistrationURLKey";
    public static string S_NOTIFICATION_SEND_URL_KEY => "NotificationSendURLKey";

    public static string S_USER_TYPE_ALLOWED_IN_BULCK_UPLOAD => "UserTypeAllowedInBulkUpload";

    //Provider Note Max Question Allow
    public static string S_MAX_QUESTIONS_ALLOWED_KEY => "MaxQuestionsAllowedKey";


    public static string S_BLOB_MICRO_SERVICE_KEY => "BlobMicroServiceKey";
    public static string FIREBASE_DOMAIN_LINK_KEY => "FirebaseDomainUrlKey";
    public static string S_COMMUNICATION_EMAIL_MICRO_SERVICE_KEY => "CommunicationEmailMicroServiceKey";
    public static string S_COMMUNICATION_WHATS_APP_MICRO_SERVICE_KEY => "CommunicationWhatsAppMicroServiceKey";
    public static string S_SMS_MICRO_SERVICE_KEY => "SMSMicroServiceKey";
    public static string S_NOTIFICATION_MICRO_SERVICE_KEY => "NotificationMicroServiceKey";
    public static string S_VIDEO_MICRO_SERVICE_KEY => "VideoMicroServiceKey";
    public static string S_FOOD_LIBRARY_MICRO_SERVICE_KEY => "FoodLibraryMicroServiceKey";

    public static string S_RAZORPAY_API_KEY => "RazorpayApiKey";

    public static string S_RAZORPAY_API_SECRET_KEY => "RazorpayApiSecretKey";
}