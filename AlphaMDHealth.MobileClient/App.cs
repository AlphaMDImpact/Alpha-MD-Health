using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Application = Microsoft.Maui.Controls.Application;

namespace AlphaMDHealth.MobileClient;

public class App : Application
{
    private readonly TaskCompletionSource<string> _dynamicLinkTaskCompletion;
    public static Pages? NotificationTarget = null;
    public static string NotificationParameter = string.Empty;

    /// <summary>
    /// Service essentials
    /// </summary>
    public static readonly IEssentials _essentials = new MobileEssentials();  

    //todo:
    ///// <summary>
    ///// Analytics reference
    ///// </summary>
    //public static IAtomAnalytics AtomAnalytics
    //{
    //    get { return DependencyService.Get<IAtomAnalytics>(); }
    //}

    /// <summary>
    /// Application start-up class
    /// </summary>
    ///// <param name="dynamicLinkTaskCompletion">Dynamic link task for onelink</param>
    public App() //(TaskCompletionSource<string> dynamicLinkTaskCompletion)
    {
        //InitializeComponent();

        // When application is launching first time, clean storage before any storage value is used
        // Required specially in iOS as secure storage values are persisted even after uninstall
        ////App._essentials.SetPreferenceValue(LibStorageConstants.PR_IS_HEIGHT_RESET_KEY, true);
        if (App._essentials.GetPreferenceValue(StorageConstants.PR_APP_LAUNCH_STATUS_KEY, 0) == 0)
        {
            // Clear secure storage
            App._essentials.RemoveAllSecureStorage();
            App._essentials.SetPreferenceValue(StorageConstants.PR_APP_LAUNCH_STATUS_KEY, 1);
        }

        InitializeApplication();
        //_dynamicLinkTaskCompletion = dynamicLinkTaskCompletion;
        MainPage = new ShellMasterPage(new BasePage());
    }

    /// <summary>
    /// Handle when app launches 
    /// </summary>
    protected override void OnStart()
    {
        InitializeAppCenter();
        base.OnStart();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await ShellMasterPage.CurrentShell.PushMainPageAsync(new InitializationPage(_dynamicLinkTaskCompletion)).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Handle when app sleeps
    /// </summary>
    protected override void OnSleep()
    {
        App._essentials.SetPreferenceValue(StorageConstants.PR_APP_SLEEP_DATE_TIME_KEY, GenericMethods.GetUtcDateTime.ToUnixTimeSeconds());
        App._essentials.SetPreferenceValue(StorageConstants.PR_TIMER_SLEEP_DATE_TIME_KEY, GenericMethods.GetUtcDateTime.ToUnixTimeSeconds());
        base.OnSleep();
    }

    /// <summary>
    /// Handle when app resumes
    /// </summary>
    protected override void OnResume()
    {
        base.OnResume();
        var lastSleepDateTime = DateTimeOffset.FromUnixTimeSeconds(App._essentials.GetPreferenceValue(StorageConstants.PR_APP_SLEEP_DATE_TIME_KEY, (long)0));
        CheckPageNavigationAsync(lastSleepDateTime);

        App._essentials.SetPreferenceValue(StorageConstants.PR_APP_SLEEP_DATE_TIME_KEY, (long)0);
        if (App._essentials.GetPreferenceValue(StorageConstants.PR_IS_TIMER_STARTED_KEY, false))
        {
            App._essentials.SetPreferenceValue(StorageConstants.PR_IS_APP_MINIMISE_FOR_TIMER_KEY, true);
        }
    }

    /// <summary>
    /// Initializes application basic services and handlings
    /// </summary>
    public void InitializeApplication()
    {
        // Register some services which are going to be used in application
        RegisterServices();
        // initialize http client for securing operations and allow max operations
        InitializeHttpClientValidation();
        // initialize device size in preferences
        InitializeDeviceSize();
    }

    /// <summary>
    /// Initialize Http validation
    /// </summary>
    private void InitializeHttpClientValidation()
    {
        // Add below custom validation for http client velidation
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback = OnValidateCertificate;
        ServicePointManager.DefaultConnectionLimit = 100;
    }

    /// <summary>
    /// Invoked when validation is raised for http service call
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="certificate">http SSL certificate</param>
    /// <param name="chain">Represents a chain-building engine for System.Security.Cryptography.X509Certificates.X509Certificate2 certificates</param>
    /// <param name="sslPolicyErrors">Secure Socket Layer (SSL) policy errors</param>
    /// <returns>true if validation is successful else returns false</returns>
    private bool OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return new DataSyncService(App._essentials).ValidateCertificate(certificate?.GetPublicKeyString());
    }

    private void InitializeDeviceSize()
    {
        //in case of tablet we support only landscape mode
        if (MobileConstants.IsTablet && DeviceDisplay.MainDisplayInfo.Height > DeviceDisplay.MainDisplayInfo.Width)
        {
            App._essentials.SetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density);
            App._essentials.SetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density);
        }
        else
        {
            //in case of mobile
            App._essentials.SetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density);
            App._essentials.SetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density);
        }
    }

    private void RegisterServices()
    {
        Current.On<iOS>().SetEnableAccessibilityScalingForNamedFontSizes(false);
        Current.On<iOS>().SetHandleControlUpdatesOnMainThread(true);
        VersionTracking.Track();
        //SvgImageSource.RegisterAssembly();
    }

    /// <summary>
    /// Clean up preference values when app is killed or terminated
    /// </summary>
    public void ResetFlags()
    {
        App._essentials.DeletePreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY);
        App._essentials.DeletePreferenceValue(StorageConstants.PR_APP_SLEEP_DATE_TIME_KEY);

        // Add code to clean up preferences
       App._essentials.DeletePreferenceValue(StorageConstants.PR_AWAIT_BLOB_CALL_KEY);
       App._essentials.DeletePreferenceValue(StorageConstants.PR_IS_SYNCING_FROM_FITNESS_APP);
       App._essentials.DeletePreferenceValue(StorageConstants.PR_IS_APP_INTRO_SHOWN_KEY);
       App._essentials.DeletePreferenceValue(StorageConstants.IS_MEDICINE_MASTER_UPLOADING_KEY);
    }

    private void CheckPageNavigationAsync(DateTimeOffset lastSleepDateTime)
    {
        Task.Run(async () =>
        {
            // if app was working on background mode for attach file/image then do not reload app or take any other action here
            if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
            {
                // Ignore applockout case if onelink data is found
                if (string.IsNullOrWhiteSpace(App._essentials.GetPreferenceValue(StorageConstants.PR_DYNAMIC_LINK_DATA_KEY, string.Empty))
                    && await new SettingService(App._essentials).IsAppLockReachedAsync(lastSleepDateTime).ConfigureAwait(true))
                {
                    if (ShellMasterPage.CurrentShell.HasMainPage)
                    {
                        // No need to navigate when already on login Page
                        if (!(ShellMasterPage.CurrentShell.MainPage is LoginPage)
                            && !(ShellMasterPage.CurrentShell.MainPage is InitializationPage)
                            && !(ShellMasterPage.CurrentShell.MainPage is LanguageSelectionPage))
                        {
                            // Navigate to initialization page to handle before login page navigation based on current data available on app
                            await ShellMasterPage.CurrentShell.PushMainPageAsync(new InitializationPage()).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        //Navigate to Pincode login page as lock time is reached
                        await ShellMasterPage.CurrentShell.PushMainPageAsync(new PincodePage(AppPermissions.PincodeLoginView.ToString(), true)).ConfigureAwait(false);
                    }
                }
                else
                {
                    // Resume current page
                }
            }
        });
    }

    private void InitializeAppCenter()
    {
        AppCenter.Configure(App._essentials.GetPreferenceValue(StorageConstants.PR_SELECTED_ENVIRONMENT_KEY, Constants.DEFAULT_ENVIRONMENT_KEY) == Constants.PROD1_KEY
            ? Constants.APP_CENTER_APP_ID_PROD 
            : Constants.APP_CENTER_APP_ID_DEV
        );
        if (AppCenter.Configured)
        {
            AppCenter.Start(typeof(Analytics));
            AppCenter.Start(typeof(Crashes));
        }
    }

    #region SignalR Code

    /// <summary>
    /// SignalR Hub connection
    /// </summary>
    public HubConnection HubConnection
    {
        get;
        private set;
    }

    /// <summary>
    /// Event invoked when a SignalR notification is received
    /// </summary>
    public event EventHandler<SignalRNotificationEventArgs> OnReceiveNotification;

    /// <summary>
    /// Sets up SignalR connection
    /// </summary>
    /// <param name="forceRefresh">if connection is to be reset</param>
    /// <returns>SignalR connection</returns>
    public async Task SetupSignalRAsync(bool forceRefresh)
    {
        // Reset hub connnection on user change
        if (forceRefresh)
        {
            HubConnection = null;
        }
        if (HubConnection == null && Connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            await AttachSignalRHubAsync().ConfigureAwait(false);
        }
        Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
    }

    private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet)
        {
            if (HubConnection == null)
            {
                await AttachSignalRHubAsync().ConfigureAwait(false);
            }
        }
        else
        {
            if (HubConnection != null)
            {
                App._essentials.SetPreferenceValue(StorageConstants.PR_SIGNALR_CONNECTION_ID_KEY, string.Empty);
                HubConnection = null;
            }
        }
    }

    private async Task AttachSignalRHubAsync()
    {
        //todo:
        //HubConnection = await new AuthService(App._essentials).EstablishSignalRHubConnectionAsync(App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0)).ConfigureAwait(false);
        //if (HubConnection != null)
        //{
        //    //"ReceiveNotification" is name of the method which is sending notifications from server. 
        //    //in current implementation we are using this single method with parameters to dientify different types of signals
        //    //in case we have multiple methods on implemented on server we ave to register them here like the one below
        //    HubConnection.On("ReceiveNotification", async (string notificationMessageType, string notificationID, string notificationFromID, bool isSilent) =>
        //    {
        //        await SyncBasedOnNotificationAsync(notificationMessageType).ConfigureAwait(false);
        //        OnReceiveNotification?.Invoke(this, new SignalRNotificationEventArgs { NotificationID = notificationID, NotificationMessageType = notificationMessageType, NotificationFromID = notificationFromID, IsSilent = isSilent });
        //    });
        //}
    }

    //private async Task SyncBasedOnNotificationAsync(string notificationMessageType)
    //{
    //    if (notificationMessageType.ToEnum<NotificationMessageType>() == NotificationMessageType.NotificationChat)
    //    {
    //       App._essentials.SetPreferenceValue(StorageConstants.PR_AWAIT_BLOB_CALL_KEY, true);
    //        await new BasePage().SyncDataWithServerAsync(Pages.ChatsPage, ServiceSyncGroups.RSSyncFromServerGroup, DataSyncFor.Chats, DataSyncFor.Chats.ToString(), default).ConfigureAwait(false);
    //        await new BasePage().SyncDataWithServerAsync(Pages.ChatsPage, ServiceSyncGroups.RSSyncFromServerGroup, DataSyncFor.Users, DataSyncFor.Users.ToString(), default).ConfigureAwait(false);
    //    }
    //}

    #endregion

}