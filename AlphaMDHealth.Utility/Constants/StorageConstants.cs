namespace AlphaMDHealth.Utility;

public class StorageConstants
{
    #region Secure Storage Keys

    /// <summary>
    /// Used to store db encryption key
    /// </summary>
    public static string SS_DB_ENCRYPTION_KEY => "DbEncryptionKey";

    /// <summary>
    /// Used to store Pincode
    /// </summary>
    public static string SS_PIN_CODE_KEY => "PinCodeKey";

    /// <summary>
    /// Used to store user's accress token
    /// </summary>
    public static string SS_ACCESS_TOKEN_KEY => "AccessTokenKey";

    /// <summary>
    /// Used to store user's refresh token
    /// </summary>
    public static string SS_REFRESH_TOKEN_KEY => "RefreshTokenKey";

    /// <summary>
    /// Preference key used to store logged in user name
    /// </summary>
    public static string SS_USER_NAME_KEY => "UserName";

    #endregion

    /// <summary>
    /// Flag representing client device is supporting 24 hour format or 12 hour format
    /// </summary>
    public static string PR_IS_24_HOUR_FORMAT => "Is24HourFormat";

    /// <summary>
    /// To Check if initial sync is completed or Not
    /// </summary>
    public static string PR_IS_INITIAL_SYNC_COMPLETE => "IsFirstSyncComplete";

    /// <summary>
    /// Key to check certificate should validated or not
    /// </summary>
    public static string PR_APPLY_CERTIFICATE_KEY => "ApplyCertificateKey";

    /// <summary>
    /// Preference Fix issue
    /// </summary>
    public static string PR_PUBLIC_KEY => "PublicKey";

    /// <summary>
    /// Preference key used to store local and utc time difference
    /// </summary>
    public static string PR_UTC_TO_LOCAL_TIME_DIFF_KEY => "UtcToLocalTimeDiff";

    /// <summary>
    /// Users Selected environment
    /// </summary>
    public static string PR_SELECTED_ENVIRONMENT_KEY => "SelectedEnvironment";

    /// <summary>
    /// Flag which decides logic based on changes done in environment
    /// </summary>
    public static string PR_IS_ENVIRONMENT_CHANGED_KEY => "IsEnvironmentChanged";

    /// <summary>
    /// Url used to store default url before clearing master
    /// </summary>
    public static string PR_SELECTED_ENVIRONMENT_DEFAULT_BASE_PATH_KEY => "SeletedEnvironmentDefaultBasePathKey";

    /// <summary>
    /// Notification token key
    /// </summary>
    public static string PR_NOTIFICATION_TOKEN_KEY => "NotificationTokenKey";

    /// <summary>
    /// Notification token changed key
    /// </summary>
    public static string PR_NOTIFICATION_TOKEN_CHANGED_KEY => "NotificationTokenChangedKey";

    /// <summary>
    /// Users selected language id
    /// </summary>
    public static string PR_SELECTED_LANGUAGE_ID_KEY => "SelectedLanguageID";

    /// <summary>
    /// Used for handling scenario when system level operation is being performed for e.g. Photo select and camera. This prevents app from going to dashboard(default page) instead of the previous page
    /// </summary>
    public static string PR_IS_WORKING_ON_BACKGROUND_MODE_KEY => "IsWorkingOnbackgroundMode";

    /// <summary>
    /// Used for handling scenario when system level operation is being performed for e.g. Photo select and camera. This prevents app from going to dashboard(default page) instead of the previous page
    /// </summary>
    public static string PR_DEVICE_COUNTRY_CODE_KEY => "DeviceCountryCode";

    /// <summary>
    /// Width of mobile screen
    /// </summary>
    public static string PR_IS_ADD_EDIT_PAGE_KEY => "IsAddEditPage";

    /// <summary>
    /// Width of mobile screen
    /// </summary>
    public static string PR_SCREEN_WIDTH_KEY => "ScreenWidth";

    /// <summary>
    /// Flag which allowed copy paste based on setting
    /// </summary>
    public static string PR_ALLOW_READ_COPY_CLIPBOARD_KEY => "AllowReadCopyClipboard";

    /// <summary>
    /// Flag which decide logic when certificatio pinning is enabled
    /// </summary>
    public static string PR_IS_CERTIFICATE_PINING_ENABLE_KEY => "CertificatePiningEnable";

    /// <summary>
    /// Used to decide back press handled or not
    /// </summary>
    public static string PR_IS_BACK_HANDLED_KEY => "IsBackHandled";

    /// <summary>
    /// SDK version used in application
    /// </summary>
    public static string PR_APPLICATION_SDK_VERSION_KEY => "ApplicationSDKVersion";

    /// <summary>
    /// Flag which decides schreen saring will allowed or not
    /// </summary>
    public static string PR_IS_SCREEN_SHARING_ENABLED_KEY => "IsScreenSharingEnabled";

    /// <summary>
    /// Height of mobile screen
    /// </summary>
    public static string PR_SCREEN_HEIGHT_KEY => "ScreenHeight";

    /// <summary>
    /// Flag which helps to take decision wether device is jail broken or not
    /// </summary>
    public static string PR_IS_JAIL_BROKEN_KEY => "IsJailBroken";

    /// <summary>
    /// Logged-in account id
    /// </summary>
    public static string PR_ACCOUNT_ID_KEY => "AccountID";

    /// <summary>
    /// Device id of user
    /// </summary>
    public static string PR_DEVICE_ID_KEY => "DeviceID";

    /// <summary>
    /// Flag which helps to wait all bearer token service call tasks when refresh token service call is in progress
    /// </summary>
    public static string PR_TOKEN_REFRESH_IN_PROGRESS_KEY => "TokenRefreshInProgress";

    /// <summary>
    /// Flag indicating the current status of app launch
    /// </summary>
    public static string PR_APP_LAUNCH_STATUS_KEY => "AppLaunchStatus";

    /// <summary>
    /// Last successful Authentication type key
    /// </summary>
    public static string PR_IS_BIOMETRIC_AUTH_PREFERRED_KEY => "IsBiometericAuthPreferred";

    /// <summary>
    /// Date time of last wrong login attempt
    /// </summary>
    public static string PR_LAST_WRONG_LOGIN_DATE_TIME_KEY => "LastWrongLoginDateTimeKey";

    /// <summary>
    /// Date time of last wrong login attempt
    /// </summary>
    public static string PR_IS_APP_MINIMISE_FOR_TIMER_KEY => "IsAppMinimiseForTimerKey";

    /// <summary>
    /// Date time of last wrong login attempt
    /// </summary>
    public static string PR_IS_TIMER_STARTED_KEY => "IsTimerStartedKey";

    /// <summary>
    /// App sleep date time
    /// </summary>
    public static string PR_APP_SLEEP_DATE_TIME_KEY => "AppSleepDateTime";

    /// <summary>
    /// App sleep date time
    /// </summary>
    public static string PR_TIMER_SLEEP_DATE_TIME_KEY => "TimerSleepDateTime";

    /// <summary>
    /// Current iOS version
    /// </summary>
    public static string PR_IS_IOS_VERSION_GREATER_THEN13_KEY => "isIosversionGreaterThen13key";

    /// <summary>
    /// Keyboard done button click
    /// </summary>
    public static string PR_IS_DONE_CLICK_KEY => "isDoneClickkey";

    /// <summary>
    /// Health Permission Call check
    /// </summary>
    public static string PR_IS_PERMISSION_CALL_COMPLETE_KEY => "IsPermissionCallComplete";

    /// <summary>
    /// Height of the header of after login pages
    /// </summary>
    public static string PR_HEADER_HEIGHT_KEY => "HeaderHeight";

    /// <summary>
    /// Preference to check if error logs are to be synced to AppCenter
    /// </summary>
    public static string PR_SYNC_ERROR_TO_APP_CENTER_KEY => "SyncErrorToAppCenter";

    /// <summary>
    /// Login Account's user ID
    /// </summary>
    public static string PR_LOGIN_USER_ID_KEY => "LoginUserIDKey";

    /// <summary>
    /// To store Last Synced Date 
    /// </summary>
    public static string PR_LAST_SYNCED_DATE_KEY => "LastSyncedDateKey";

    /// <summary>
    /// Flow direction based on selected language
    /// </summary>
    public static string PR_IS_RIGHT_ALIGNED_KEY => "IsRightAlignedKey";

    /// <summary>
    /// Permission at level ID
    /// </summary>
    public static string PR_PERMISSION_AT_LEVEL_ID_KEY => "PermissionAtLevelIDKey";

    /// <summary>
    /// Logged in Account Role Name
    /// </summary>
    public static string PR_ROLE_ID_KEY => "RoleIDKey";

    /// <summary>
    /// HealthApp Sync is Going on?
    /// </summary>
    public static string PR_IS_SYNCING_FROM_FITNESS_APP => "IsSyncingFromFitnessApp";

    /// <summary>
    ///ControlWidthKey
    /// </summary>
    public static string PR_CONTROL_WIDTH_KEY => "ControlWidthKey";

    /// <summary>
    /// Selected User ID 
    /// </summary>
    public static string PR_SELECTED_USER_ID_KEY => "SelectedUserIDKey";

    /// <summary>
    /// Selected User ID 
    /// </summary>
    public static string PR_IS_KEYBOARD_UP_KEY => "WindowSoftInputModeAdjustKey";

    /// <summary>
    /// Determines if App Intro is shown during initialization flow 
    /// </summary>
    public static string PR_IS_APP_INTRO_SHOWN_KEY => "IsAppIntroShown";

    /// <summary>
    /// Add ScrollView
    /// </summary>
    public static string PR_REMOVE_SCROLL_VIEW_KEY => "RemoveScrollViewKey";

    /// <summary>
    /// Add ScrollView
    /// </summary>
    public static string PR_IS_MAIN_LOGGEDIN_USER_KEY => "IsMainLoggedInUserKey";

    #region Preferences Keys

    /// <summary>
    /// Flag used to decide sync call is success after language change or not
    /// </summary>
    public static string PR_IS_LANGUAGE_CHANGED_KEY => "IsLanguageChangedKey";

    /// <summary>
    /// Flag wchich help to take decision when color setting is changed
    /// </summary>
    public static string PR_IS_COLOR_SETTING_CHANGED_KEY => "IsColorSettingChangedKey";

    /// <summary>
    /// Users selected organization id
    /// </summary>
    public static string PR_SELECTED_ORGANISATION_ID_KEY => "SelectedOrganisationIDKey";

    /// <summary>
    /// User cred key
    /// </summary>
    public static string PR_USER_CRED_KEY => "UserCredKey";

    /// <summary>
    /// Remember me
    /// </summary>
    public static string PR_REMEMBER_ME_KEY => "RememberMeKey";

    /// <summary>
    /// Phone number key
    /// </summary>
    public static string PR_PHONE_NUMBER_KEY => "PhoneNumberKey";

    /// <summary>
    /// Phone number key
    /// </summary>
    public static string PR_QUESTIONNAIRE_ID_KEY => "QuestionnaireIDKey";

    /// <summary>
    /// SignarR ID
    /// </summary>
    public static string PR_SIGNALR_CONNECTION_ID_KEY => "SignalRConnectionIDKey";

    /// <summary>
    /// Web local storage key
    /// </summary>
    public static string PR_LOCAL_STORAGE_KEY => "LocalStorageKey";

    /// <summary>
    /// Notification parameter key
    /// </summary>
    public static string PR_DYNAMIC_LINK_DATA_KEY => "DynamicLinkDataKey";

    /// <summary>
    /// Flah to decide whether to parameter key
    /// </summary>
    public static string PR_AWAIT_BLOB_CALL_KEY => "AwaitBlobCallKey";

    /// <summary>
    /// Notifications allowed parameter key
    /// </summary>_
    public static string PR_IS_NOTIFICATIONS_ALLOWED_KEY => "IsNotificationsAllowedKey";

    /// <summary>
    /// Health account connected parameter key
    /// </summary>
    public static string PR_IS_HEALTH_ACCOUNT_CONNECTED_KEY => "IsHealthAccountConnectedKey";

    /// <summary>
    /// Health account connected parameter key
    /// </summary>
    public static string PR_SELECTED_VIDEO_SERVICE_KEY => "SelectedVideoService";

    ///// <summary>
    ///// Medication Id when notification is clicked
    ///// </summary>
    //public static string PR_MEDICATION_ID_KEY => "MedicationID";

    ///// <summary>
    ///// To keep notification id count
    ///// </summary>
    //public static string PR_NOTIFICATION_ID_KEY => "NotificationIDKey";

    /// <summary>
    /// To keep notification tags
    /// </summary>
    public static string PR_NOTIFICATION_TAGS_KEY => "NotificationTagsKey";

    /// <summary>
    /// 
    /// </summary>
    public static string PR_IS_NAVIGATE_TO_EDIT_KEY => "IsNavigateToEditPageKey";

    /// <summary>
    /// Is Medicine Master Uploading Key
    /// </summary>
    public static string IS_MEDICINE_MASTER_UPLOADING_KEY => "IsMedicineMasterUploadingKey";

    #endregion
}