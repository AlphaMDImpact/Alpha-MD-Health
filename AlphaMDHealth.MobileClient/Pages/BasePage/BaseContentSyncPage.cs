using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public partial class BasePage
{
    /// <summary>
    /// Property which contains running service call count
    /// </summary>
    private static int _syncCount;

    /// <summary>
    /// Property which contains running service call count
    /// </summary>
    public static int SyncCount { get => _syncCount; set => _syncCount = value; }

    /// <summary>
    /// Method which updates sync count
    /// </summary>
    /// <param name="isStart">Variable sync count should be increase or decrease</param>
    private void UpdateSyncCount(bool isStart)
    {
        int increment = isStart ? 1 : -1;
        Interlocked.Add(ref _syncCount, increment);
    }

    #region Sync Data To/From Server

    /// <summary>
    /// Refresh current page view after completion of sync
    /// </summary>
    /// <param name="syncFrom">From which page/view sync is called</param>
    protected virtual async Task RefreshUIAsync(Pages syncFrom)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Refresh current page view after completion of sync
    /// </summary>
    /// <param name="syncFrom">From which page/view sync is called</param>
    protected async Task RefreshUIAsync(string syncFrom)
    {
        await RefreshUIAsync(syncFrom.ToEnum<Pages>()).ConfigureAwait(true);
    }

    /// <summary>
    /// Sync Data from server / Sync Data to server for specified service directly without any configuration check
    /// </summary>
    /// <param name="syncFrom">Page from where sync request is raised</param>
    /// <param name="syncDataFor">Table for which data need to be synced</param>
    /// <param name="syncGroup">Group name which decides data need to Sync from or Sync To</param>
    /// <param name="tablesInBatch">Tables data which will fetched in this batch</param>
    /// <param name="patientID">PatientID for which data needs to fetch</param>
    /// <returns>Operation Status</returns>
    public async Task<BaseDTO> SyncDataWithServerAsync(Pages syncFrom, ServiceSyncGroups syncGroup, DataSyncFor syncDataFor, string tablesInBatch, long patientID)
    {
        return await new SyncManagerService(App._essentials).SyncDataAsync(
            AttachSyncServiceActionAsync, syncGroup, syncFrom.ToString(), syncDataFor.ToString(), tablesInBatch, patientID
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Sync Data To/From server for specified Settings of given page with configuration check
    /// </summary>
    /// <param name="syncFrom">Page from where sync from request is raised</param>
    /// <param name="isFirstTime">Bool value which represent call is first time call or not</param>
    /// <param name="patientID">PatientID for which data needs to fetch</param>
    /// <returns>Operation status</returns>
    public async Task<BaseDTO> SyncDataWithServerAsync(Pages syncFrom, bool isFirstTime, long patientID)
    {
        return await new SyncManagerService(App._essentials).SyncDataAsync(
            AttachSyncServiceActionAsync, syncFrom.ToString(), isFirstTime, patientID, SyncAndRefreshViewesAsync
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Reset Sync From Dates for specified Settings of given page with configuration check
    /// </summary>
    /// <param name="syncFrom">Page from where sync from request is raised</param>
    /// <param name="isFirstTime">Bool value which represent call is first time call or not</param>
    /// <param name="patientID">PatientID for which data needs to fetch</param>
    /// <returns>Operation status</returns>
    public async Task<BaseDTO> ResetSyncFromDatesAsync(Pages syncFrom, bool isFirstTime, long patientID)
    {
        return await new SyncManagerService(App._essentials).SyncDataAsync(
            AttachResetSyncFromActionAsync, syncFrom.ToString(), isFirstTime, patientID, SyncAndRefreshViewesAsync
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Invokes sync from/to server calls
    /// </summary>
    /// <param name="result">Reference object which stores input params and returns operation result</param>
    /// <returns>Operation result</returns>
    public async Task InvokeSyncActionAsync(BaseDTO result)
    {
        DataSyncFor syncFor = result.LastModifiedBy.ToEnum<DataSyncFor>();
        Pages syncFrom = result.AddedBy.ToEnum<Pages>();
        result.ErrCode = syncFor == DataSyncFor.Defaults
            ? (await SyncDataWithServerAsync(syncFrom, result.IsActive, default).ConfigureAwait(false)).ErrCode
            : (await SyncDataWithServerAsync(syncFrom, result.ErrorDescription.ToEnum<ServiceSyncGroups>(), syncFor, result.Response, default).ConfigureAwait(false)).ErrCode;
    }

    protected async Task CallServiceAsync(BaseDTO result)
    {
        if (result.RecordCount == -1)
        {
            result.ErrCode = (await ResetSyncFromDatesAsync(result.AddedBy.ToEnum<Pages>(), result.IsActive, default).ConfigureAwait(false)).ErrCode;
        }
        else
        {
            await InvokeSyncActionAsync(result).ConfigureAwait(false);
        }
    }

    private int SyncAndRefreshViewesAsync(string syncFrom)
    {
        int recordCount = 0;
        if (CancellationTokenSourceInstance != null && !CancellationTokenSourceInstance.IsCancellationRequested)
        {
            recordCount = 1;
            _ = ShellMasterPage.CurrentShell.CurrentPage?.RefreshUIAsync(syncFrom)?.ConfigureAwait(false);
        }
        return recordCount;
    }

    /// <summary>
    /// Handles token expired error
    /// </summary>
    /// <param name="result">Operation result</param>
    /// <param name="syncFrom">Page from where sync from request is raised</param>
    /// <returns>Handles token expired</returns>
    internal async Task UnauthorizedOrTokenExpiredTaskResultAsync(BaseDTO result, Pages syncFrom)
    {
        if (CancellationTokenSourceInstance != null && !CancellationTokenSourceInstance.IsCancellationRequested)
        {
            CancellationTokenSourceInstance.Cancel();
        }
        await new AuthService(App._essentials).ClearAccountTokensAndIdAsync().ConfigureAwait(true);
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (!(ShellMasterPage.CurrentShell.MainPage is LoginPage))
            {
                TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                await (ShellMasterPage.CurrentShell.MainPage as BasePage ?? ShellMasterPage.CurrentShell.CurrentPage).DisplayMessagePopupAsync(result.ErrCode.ToString(), (sender, e) =>
                {
                    //todo:
                    //CustomMessageControl messagePopup = sender as CustomMessageControl;
                    //messagePopup.ShowInPopup = false;
                    //messagePopup.IsVisible = false;
                    //taskCompletionSource.TrySetResult(true);
                }, false, false, false).ConfigureAwait(true);
                await taskCompletionSource.Task.ConfigureAwait(true);
                await Task.Delay(500).ConfigureAwait(true);
                // Allow initialization page to handle redirection so that page styles and other initialization can be done
                if (syncFrom != Pages.InitializationPage && !(ShellMasterPage.CurrentShell.MainPage is LoginPage))
                {
                    result.ErrCode = ErrorCode.HandledRedirection;
                    ForceNavigateToPage((Page)new LoginPage());
                }
            }
        }).ConfigureAwait(false);
    }

    ////public async Task NavigateOnNotificationClick(string pageType, string parameter)
    ////{
    ////    try
    ////    {
    ////        Console.WriteLine($"+++++||NavigateOnNotificationClick() Before App.NotificationTarget {App.NotificationTarget}||+++++");
    ////        Console.WriteLine($"+++++||NavigateOnNotificationClick() Before App.NotificationParameter {App.NotificationParameter}||+++++");
    ////        if (pageType == Pages.PatientMedicationsPage.ToString() && !string.IsNullOrWhiteSpace(parameter) && parameter != "<null>")
    ////        {
    ////            var parameters = parameter.Split(',');
    ////            Console.WriteLine($"+++++||NavigateOnNotificationClick() Mila App.NotificationTarget {App.NotificationTarget}||+++++");
    ////            Console.WriteLine($"+++++||NavigateOnNotificationClick() Mila App.NotificationParameter {App.NotificationParameter}||+++++");
    ////            if (App.Current.MainPage is ShellMasterPage)
    ////            {
    ////                Shell.Current?.CurrentItem.Navigation.PopToRootAsync(false);
    ////            }
    ////            if (await new UserService(App._essentials).IsMultipleUserAsync(long.Parse((string)parameters[1])).ConfigureAwait(false))
    ////            {
    ////                Console.WriteLine($"+++++||NavigateOnNotificationClick() User App.NotificationTarget {App.NotificationTarget}||+++++");
    ////                Console.WriteLine($"+++++||NavigateOnNotificationClick() User App.NotificationParameter {App.NotificationParameter}||+++++");
    ////                await base.PushPageByTargetAsync((string)Pages.MultipleUsersPage.ToString(), (string)LibGenericMethods.GenerateParamsWithPlaceholder(Param.id), parameters[(int)1]).ConfigureAwait(true);
    ////            }
    ////            else
    ////            {
    ////                Console.WriteLine($"+++++||NavigateOnNotificationClick() Karde navigate App.NotificationTarget {App.NotificationTarget}===pageType===={pageType}||+++++");
    ////                Console.WriteLine($"+++++||NavigateOnNotificationClick() Karde navigate App.NotificationParameter {App.NotificationParameter}===parameter=={parameter}=== a[0]==={a[0]}||+++++");
    ////                MainThread.BeginInvokeOnMainThread(async () =>
    ////                {
    ////                    await PushPageByTargetAsync(pageType, LibGenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.id), "-1", (string)parameters[0]).ConfigureAwait(true);
    ////                });
    ////            }
    ////        }
    ////        else if ((pageType == Pages.ChatsPage.ToString() || pageType == Pages.ChatPage.ToString()) && !string.IsNullOrWhiteSpace(parameter) && parameter != "<null>")
    ////        {
    ////            AppHelper.ShowBusyIndicator = true;
    ////            await SyncDataWithServerAsync(Pages.ChatView, false, default).ConfigureAwait(false);
    ////            ChatDTO userChat = new ChatDTO
    ////            {
    ////                ChatDetail = new ChatDetailModel
    ////                {
    ////                    ChatDetailID = new Guid(parameter)
    ////                }
    ////            };
    ////            await new ChatService(App._essentials).GetChatDetailAsync(userChat).ConfigureAwait(true);
    ////            if (userChat.ErrCode == ErrorCode.OK && userChat.ChatDetails?.Count > 0)
    ////            {
    ////                parameter = userChat.ChatDetails.FirstOrDefault().AddedById.ToString();
    ////                MainThread.BeginInvokeOnMainThread(async () =>
    ////                {
    ////                    await ShellMasterPage.Current.Navigation.PopToRootAsync(false).ConfigureAwait(true);
    ////                    await PushPageByTargetAsync(pageType, LibGenericMethods.GenerateParamsWithPlaceholder(Param.id), parameter).ConfigureAwait(true);
    ////                });
    ////            }
    ////            else
    ////            {
    ////                AppHelper.ShowBusyIndicator = false;
    ////            }
    ////        }
    ////        else
    ////        {
    ////            await SyncDataWithServerAsync(pageType.ToEnum<Pages>(), false, default).ConfigureAwait(false);
    ////            MainThread.BeginInvokeOnMainThread(async () =>
    ////            {
    ////                await ShellMasterPage.Current.Navigation.PopToRootAsync(false).ConfigureAwait(true);
    ////            });
    ////            MainThread.BeginInvokeOnMainThread(async () =>
    ////            {
    ////                if (string.IsNullOrWhiteSpace(parameter))
    ////                {
    ////                    await PushPageByTargetAsync(pageType, string.Empty).ConfigureAwait(false);
    ////                }
    ////                else
    ////                {
    ////                    await PushPageByTargetAsync(pageType, LibGenericMethods.GenerateParamsWithPlaceholder(Param.id), parameter).ConfigureAwait(true);
    ////                }
    ////            });
    ////        }
    ////        if (!ShellMasterPage.CurrentShell.IsNavigating)
    ////        {
    ////            Console.WriteLine($"+++++||NavigateOnNotificationClick() Karde navigate ShellMasterPage.CurrentShell.IsNavigating {ShellMasterPage.CurrentShell.IsNavigating}===pageType===={pageType}||+++++");
    ////            ResetNotificationValue();
    ////        }
    ////    }
    ////    catch (Exception ex)
    ////    {
    ////        LogErrors("NavigateOnNotificationClickAsync()", ex);
    ////        Console.WriteLine($"+++++||NavigateOnNotificationClick() Fata App.NotificationTarget {App.NotificationTarget} {ex}||+++++");
    ////        Console.WriteLine($"+++++||NavigateOnNotificationClick() Fata App.NotificationParameter {App.NotificationParameter} {ex}||+++++");
    ////    }
    ////}

    ////private static void ResetNotificationValue()
    ////{
    ////    App.NotificationParameter = string.Empty;
    ////    App.NotificationTarget = null;
    ////}

    private async Task AttachResetSyncFromActionAsync(BaseDTO result)
    {
        try
        {
            await new DataSyncService(App._essentials).ResetSyncFromAsync(new DataSyncDTO
            {
                SyncBatch = result.LastModifiedBy,
                DataSyncFor = result.Response.Split(Constants.COMMA_SEPARATOR),
                PatientID = result.AddedBy.ToEnum<Pages>() == Pages.LanguageSelectionPage ? -1 : result.SelectedUserID
            }).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            LogErrors(nameof(BasePage) + ".AttachResetSyncFromActionAsync()", ex);
            result.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
    }

    protected async Task AttachSyncServiceActionAsync(BaseDTO result)
    {
        var startTime = DateTimeOffset.Now;
        GenericMethods.LogData($"StartSyncDataAsync() START; syncGroup:{result.ErrorDescription}; syncFrom:{result.AddedBy}; syncFor:{result.LastModifiedBy}; tablesInBatch:{result.Response}; Patient:{result.SelectedUserID}");
        if (MobileConstants.CheckInternet)
        {
            ServiceSyncGroups syncGroup = result.ErrorDescription.ToEnum<ServiceSyncGroups>();
            Pages syncFrom = result.AddedBy.ToEnum<Pages>();
            DataSyncFor syncFor = result.LastModifiedBy.ToEnum<DataSyncFor>();
            var isSyncAwaited = result.IsActive;
            var tablesInBatch = result.Response;
            try
            {
                CancellationTokenSourceInstance = CancellationTokenSourceInstance ?? new CancellationTokenSource();
                UpdateSyncStatus(true);
                await new DataSyncService(App._essentials).SyncDataAsync(result, syncGroup, syncFor, CancellationTokenSourceInstance.Token).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                LogErrors(nameof(BasePage) + ".SyncDataAsync()", ex);
                result.ErrCode = syncGroup == ServiceSyncGroups.RSSyncFromServerGroup ? ErrorCode.ErrorWhileRetrievingRecords : ErrorCode.ErrorWhileSavingRecords;
            }
            finally
            {
                UpdateSyncStatus(false);
                await ExecuteTaskResultAsync(result, syncFrom, syncFor, syncGroup, tablesInBatch, isSyncAwaited).ConfigureAwait(false);
            }
        }
        else
        {
            result.ErrCode = ErrorCode.NoInternetConnection;
        }
        GenericMethods.LogData($"StartSyncDataAsync() END; syncGroup:{result.ErrorDescription}; syncFrom:{result.AddedBy}; syncFor:{result.LastModifiedBy}; tablesInBatch:{result.Response}; Patient:{result.SelectedUserID}; Time:{DateTimeOffset.Now - startTime}; ErrorCode:{result.ErrCode}; RecordCount:{result.RecordCount}");
    }

    /// <summary>
    /// Method which updates sync count
    /// </summary>
    /// <param name="isStart">Variable sync count should be increase or decrease</param>
    private void UpdateSyncStatus(bool isStart)
    {
        UpdateSyncCount(isStart);
        //todo:
        //if (ShellMasterPage.CurrentShell?.CurrentPage is DashboardPage)
        //{
        //    (ShellMasterPage.CurrentShell.CurrentPage as DashboardPage).ShowSyncProgress();
        //}
    }

    /// <summary>
    /// Take some action based on service sync operation status
    /// </summary>
    /// <param name="result">Service Sync operation status</param>
    /// <param name="syncFrom">Page from where sync from request is raised</param>
    /// <param name="syncFor">Type of data being synced</param>
    /// <param name="syncGroup">Type of service sync group</param>
    /// <param name="tablesInBatch">tables synced in this batch</param>
    /// <param name="isSyncAwwaited">flag wo decide call is awaited or not</param>
    private async Task ExecuteTaskResultAsync(BaseDTO result, Pages syncFrom, DataSyncFor syncFor, ServiceSyncGroups syncGroup, string tablesInBatch, bool isSyncAwwaited)
    {
        App._essentials.SetPreferenceValue(StorageConstants.PR_LAST_SYNCED_DATE_KEY, GenericMethods.GetUtcDateTime.ToString(CultureInfo.InvariantCulture));
        if (result != null)
        {
            result.AddedBy = syncFrom.ToString();
            result.LastModifiedBy = syncFor.ToString();
            switch (result.ErrCode)
            {
                case ErrorCode.OK:
                    await OkTaskResultAsync(result, syncFrom, string.IsNullOrWhiteSpace(tablesInBatch) ? result.LastModifiedBy : tablesInBatch, syncGroup, isSyncAwwaited).ConfigureAwait(false);
                    break;
                case ErrorCode.TokenExpired:
                case ErrorCode.InActiveUser:
                    await UnauthorizedOrTokenExpiredTaskResultAsync(result, syncFrom).ConfigureAwait(false);
                    if (syncFrom == Pages.InitializationPage)
                    {
                        await Task.Delay(10).ConfigureAwait(true);
                        result.ErrCode = ErrorCode.OK;
                        CancellationTokenSourceInstance = new CancellationTokenSource();
                        await AttachSyncServiceActionAsync(result).ConfigureAwait(false);
                    }
                    break;
                case ErrorCode.RecordCountMismatch:
                case ErrorCode.PlanExpired:
                case ErrorCode.RenewPlan:
                case ErrorCode.UnknownCertificate:
                    ForceNavigateToPage(new StaticMessagePage(result.ErrCode.ToString()));
                    result.ErrCode = ErrorCode.HandledRedirection;
                    break;
                default:
                    //will use for future implementation
                    break;
            }
        }
    }

    private async Task OkTaskResultAsync(BaseDTO result, Pages syncFrom, string tablesInBatch, ServiceSyncGroups syncGroup, bool isSyncAwaited)
    {
        string[] response = tablesInBatch?.Split(Constants.SYMBOL_COMMA);
        if (response.Contains(DataSyncFor.Settings.ToString())
            && await HandelSettingsChangesAsync(result, syncFrom, syncGroup).ConfigureAwait(true))
        {
            return;
        }

        //todo:
        //// Check if consent is enabled and any mandatory Consent is not yet accepted by the user then navigate to consent page else continue
        if (result.RecordCount > 0 && result.RecordCount > 0x0000
            && await IsConsentRequiredAsync(syncGroup, isSyncAwaited, response))
        {
            ForceNavigateToPage(new UserConsentsPage(true));
            result.ErrCode = ErrorCode.HandledRedirection;
            return;
        }
        //if (await IsHealthAppConnectionRequiredAsync(syncGroup, isSyncAwaited, response).ConfigureAwait(true))
        //{
        //    ForceNavigateToPage(new HealthAccountConnectPage(false));
        //    result.ErrCode = ErrorCode.HandledRedirection;
        //    return;
        //}
        await ValidateLanguageAsync(result, syncFrom);
    }

    private async Task<bool> HandelSettingsChangesAsync(BaseDTO result, Pages syncFrom, ServiceSyncGroups syncGroup)
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_APPLY_CERTIFICATE_KEY, true))
        {
            await new EnvironmentService(App._essentials).GetSelectedBaseUrlAsync(UrlConstants.DEFAULT_ENVIRONMENT_KEY_VALUE).ConfigureAwait(true);
            App._essentials.SetPreferenceValue(StorageConstants.PR_APPLY_CERTIFICATE_KEY, true);
        }
        if (result.RecordCount > 0 && result.RecordCount > 0x0000)
        {
            // If after update we get that app update is needed  // When current version is not latest one
            if (syncFrom == Pages.ShellMasterPage
                && await new SettingService(App._essentials).CheckAppUpdateAsync(Constants.APP_VERSON_NO, new BaseDTO()).ConfigureAwait(true))
            {
                ForceNavigateToPage(new StaticMessagePage(ErrorCode.UpdateApp.ToString()));
                result.ErrCode = ErrorCode.HandledRedirection;
                return true;
            }
            // Reload styles again if any style configuration changed
            if (await ShouldReloadStylesAsync(syncGroup))
            {
                ForceNavigateToPage(new StaticMessagePage(ErrorCode.RestartApp.ToString()));
                result.ErrCode = ErrorCode.HandledRedirection;
                return true;
            }
        }
        return false;
    }

    private async Task<bool> ShouldReloadStylesAsync(ServiceSyncGroups syncGroup)
    {
        return syncGroup == ServiceSyncGroups.RSSyncFromServerGroup
            && App._essentials.GetPreferenceValue(StorageConstants.PR_IS_COLOR_SETTING_CHANGED_KEY, false)
            && (await new AppStyles().LoadAppStylesAsync().ConfigureAwait(true)).ErrCode != ErrorCode.OK;
    }

    private async Task<bool> IsConsentRequiredAsync(ServiceSyncGroups syncGroup, bool isSyncAwaited, string[] response)
    {
        return (response.Contains(DataSyncFor.Consents.ToString()) || response.Contains(DataSyncFor.UserConsents.ToString()))
            && !isSyncAwaited && !(ShellMasterPage.CurrentShell.MainPage is PincodePage)
            && syncGroup == ServiceSyncGroups.RSSyncFromServerGroup
            && (await new ConsentService(App._essentials).IsConsentRequiredAsync().ConfigureAwait(true) == 1);
    }

    private async Task<bool> IsHealthAppConnectionRequiredAsync(ServiceSyncGroups syncGroup, bool isSyncAwaited, string[] response)
    {
        return (response.Contains(DataSyncFor.Readings.ToString()) || response.Contains(DataSyncFor.PatientReadings.ToString()))
            && !isSyncAwaited && !(ShellMasterPage.CurrentShell.MainPage is PincodePage)
            && syncGroup == ServiceSyncGroups.RSSyncFromServerGroup
            && await new ReadingService(App._essentials).IsReadingHealthAppEnabledAsync().ConfigureAwait(true) == 1;
    }

    private async Task ValidateLanguageAsync(BaseDTO result, Pages syncFrom)
    {
        if (result.Response != null && result.Response.Split(Constants.SYMBOL_COMMA).Contains(DataSyncFor.Languages.ToString()))
        {
            // Check if selected language is available in list of languages
            await new LanguageService(App._essentials).VerifySelectedLanguageAsync(result).ConfigureAwait(true);
            if (result.ErrCode == ErrorCode.LanguageNotAvailable && syncFrom != Pages.InitializationPage)
            {
                ForceNavigateToPage(new LanguageSelectionPage());
                result.ErrCode = ErrorCode.HandledRedirection;
            }
        }
    }

    private void ForceNavigateToPage(Page pageToNavigateTo)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (pageToNavigateTo is LoginPage)
            {
                ShellMasterPage.CurrentShell.ClearShellContent();
            }
            await ShellMasterPage.CurrentShell.PushMainPageAsync(pageToNavigateTo).ConfigureAwait(false);
        });
    }

    #endregion

    #region Sync for Language Selection

    /// <summary>
    /// Syncs language specific data and decide navigation based on service response
    /// </summary>
    /// <param name="isBeforeLogin"></param>
    /// <param name="isLanguageSelectionPage"></param>
    /// <returns></returns>
    public async Task SyncAndNavigateAfterLanguageSelectionAsync(bool isBeforeLogin, bool isLanguageSelectionPage)
    {
        var result = await SyncDataWithServerAsync(Pages.LanguageSelectionPage, isBeforeLogin, 0).ConfigureAwait(true);
        if (result.ErrCode == ErrorCode.OK)
        {
            // on success sync, navigate on initialization page to decide naviagtion based on different conditions
            await NavigateOnInitializationPageAsync(Pages.LanguageSelectionPage).ConfigureAwait(true);
        }
        else
        {
            if (result.ErrCode != ErrorCode.HandledRedirection)
            {
                if (isLanguageSelectionPage)
                {
                    // on sync fail from language selection page, navigate on error page 
                    await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(ErrorCode.RestartApp.ToString())).ConfigureAwait(true);
                }
                else
                {
                    // on sync fail from any other page, navigate on initialization page as it loades on launch 
                    await NavigateOnInitializationPageAsync(Pages.InitializationPage).ConfigureAwait(true);
                }
            }
        }
    }

    private async Task NavigateOnInitializationPageAsync(Pages page)
    {
        App._essentials.SetPreferenceValue(StorageConstants.PR_IS_LANGUAGE_CHANGED_KEY, false);
        AppHelper.ShowBusyIndicator = false;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new InitializationPage(page)).ConfigureAwait(true);
    }

    #endregion
}