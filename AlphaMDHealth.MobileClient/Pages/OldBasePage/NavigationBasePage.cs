using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public partial class BasePage : ContentPage, IDisposable
{
    /// <summary>
    /// Gets the page data based on the node id/target
    /// </summary>
    /// <param name="mobileMenuNode">Page data for input and output</param>
    /// <returns>Page data for the given node</returns>
    public async Task<MobileMenuNodeModel> GetPageDataByNodeIDAsync(MobileMenuNodeModel mobileMenuNode)
    {
        //return ShellMasterPage.CurrentShell.BaseContentPageInstance.GetPageDataByNodeIDAsync(mobileMenuNode);

        MobileMenuNodeDTO menuNode = new MobileMenuNodeDTO { MobileMenuNode = mobileMenuNode };
        await new MenuService(App._essentials).GetMenuNodeAsync(menuNode).ConfigureAwait(true);
        if (menuNode.ErrCode == ErrorCode.OK)
        {
            return menuNode.MobileMenuNode;
        }
        return null;
    }

    /// <summary>
    /// Use this method to push profile page
    /// </summary>
    /// <param name="nodeID">Node ID for which header need to apply</param>
    /// <returns>Task for page navigation</returns>
    public async Task PushProfilePageAsync(long nodeID)
    {
        //await ShellMasterPage.CurrentShell.BaseContentPageInstance.PushPageByNodeIDAsync(nodeID, false, string.Empty).ConfigureAwait(true);
        await ShellMasterPage.CurrentShell.BaseContentPageInstance.PushPageByTargetAsync(Pages.ProfilePage.ToString(), string.Empty).ConfigureAwait(true);
    }

    /// <summary>
    /// Navigate on next page based on target
    /// </summary>
    /// <param name="isForLockout">is from auto lockout</param>
    /// <param name="isBeforeLogin">is from before login page</param>
    /// <param name="flowFrom">flow from where it is being called</param>
    /// <returns>Task representing navigation to next page</returns>
    public async Task NavigateOnNextPageAsync(bool isForLockout, bool isBeforeLogin, LoginFlow flowFrom)
    {
        if (await NavigateOnConsentsFlowAsync(isBeforeLogin, flowFrom).ConfigureAwait(true))
        {
            if (await NavigateOnMultipleUsersFlowAsync(isBeforeLogin, flowFrom).ConfigureAwait(true))
            {
                if (isForLockout)
                {
                    AppHelper.ShowBusyIndicator = false;
                    await ShellMasterPage.CurrentShell.PopMainPageAsync().ConfigureAwait(false);
                    return;
                }
                if (await NavigateOnProgramSelectionFlowAsync(isBeforeLogin, flowFrom).ConfigureAwait(true))
                {
                    if (await NavigateOnProfileCompletionFlowAsync(isBeforeLogin, flowFrom).ConfigureAwait(true))
                    {
                        if (await NavigateOnHealthAppsFlowAsync(isBeforeLogin, flowFrom).ConfigureAwait(true))
                        {
                            //// Add your cases here
                            await ShellMasterPage.CurrentShell.RenderPageAsync().ConfigureAwait(true);
                            await NavigateOnTaskCompletionFlowAsync();
                        }
                    }
                }
            }
        }
    }

    private async Task NavigateOnNextSyncFlowAsync(bool isBeforeLogin, LoginFlow flowFrom, Pages syncFrom)
    {
        var flow = syncFrom.ToString();
        var loginFlow = flow.ToEnum<LoginFlow>();
        if ((int)flowFrom <= (int)loginFlow)
        {
            await SyncDataWithServerAsync(syncFrom, isBeforeLogin, default).ConfigureAwait(true);
        }
    }

    private async Task<bool> NavigateOnConsentsFlowAsync(bool isBeforeLogin, LoginFlow flowFrom)
    {
        // Check if consent is enabled 
        // and any mandatory Consent is not yet accepted by the user 
        // then navigate to consent page else continue
        switch (await new ConsentService(App._essentials).IsConsentRequiredAsync().ConfigureAwait(true))
        {
            case 1:
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new UserConsentsPage(true)).ConfigureAwait(false);
                return false;
            case 2:
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(ErrorCode.RestartApp.ToString())).ConfigureAwait(false);
                return false;
            default:
                await NavigateOnNextSyncFlowAsync(isBeforeLogin, flowFrom, Pages.UserConsentsPage).ConfigureAwait(true);

                break;
        }
        return true;
    }

    private async Task<bool> NavigateOnMultipleUsersFlowAsync(bool isBeforeLogin, LoginFlow flowFrom)
    {
        switch (await new UserService(App._essentials).CheckIfUserSelectionRequiredAsync().ConfigureAwait(true))
        {
            case 1:
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new MultipleUsersPage(true)).ConfigureAwait(false);
                return false;
            case 2:
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(ErrorCode.InActiveUser.ToString())).ConfigureAwait(false);
                return false;
            default:
                await NavigateOnNextSyncFlowAsync(isBeforeLogin, flowFrom, Pages.MultipleUsersPage).ConfigureAwait(true);
                break;
        }
        return true;
    }

    private async Task<bool> NavigateOnProgramSelectionFlowAsync(bool isBeforeLogin, LoginFlow flowFrom)
    {
        switch (await new PatientProgramService(App._essentials).IsProgramSelectionRequiredAsync().ConfigureAwait(true))
        {
            case 1:
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new PatientProgramPage(true)).ConfigureAwait(false);
                return false;
            case 2:
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(ErrorCode.RestartApp.ToString())).ConfigureAwait(false);
                return false;
            default:
                await NavigateOnNextSyncFlowAsync(isBeforeLogin, flowFrom, Pages.PatientProgramPage).ConfigureAwait(true);
                break;
        }
        return true;
    }

    private async Task<bool> NavigateOnProfileCompletionFlowAsync(bool isBeforeLogin, LoginFlow flowFrom)
    {
        switch (await new UserService(App._essentials).IsProfileCompletionRequiredAsync().ConfigureAwait(true))
        {
            case 1:
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new RegisterMoreInfoPage()).ConfigureAwait(false);
                return false;
            case 2:
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(ErrorCode.RestartApp.ToString())).ConfigureAwait(false);
                return false;
            default:
                // Continue 
                break;
        }
        if (App.NotificationTarget != null && !string.IsNullOrWhiteSpace(App.NotificationParameter))
        {
            await ShellMasterPage.CurrentShell.RenderPageAsync().ConfigureAwait(false);
            await ShellMasterPage.CurrentShell.NavigateOnNotificationClickAsync(App.NotificationTarget.ToString(), App.NotificationParameter).ConfigureAwait(true);
            return false;
        }
        await NavigateOnNextSyncFlowAsync(isBeforeLogin, flowFrom, Pages.ProfilePage).ConfigureAwait(true);
        return true;
    }

    private async Task<bool> NavigateOnHealthAppsFlowAsync(bool isBeforeLogin, LoginFlow flowFrom)
    {
        if (flowFrom != LoginFlow.HealthAccountConnectPage)
        {
            switch (await new ReadingService(App._essentials).IsReadingHealthAppEnabledAsync().ConfigureAwait(true))
            {
                case 1:
                    var isHelathAllowed = App._essentials.GetPreferenceValue<long>(StorageConstants.PR_IS_MAIN_LOGGEDIN_USER_KEY, 0);
                    if (isHelathAllowed == 0)
                    {
                        await ShellMasterPage.CurrentShell.PushMainPageAsync(new HealthAccountConnectPage(true)).ConfigureAwait(true);
                        return false;
                    }
                    if (App._essentials.GetPreferenceValue<long>(StorageConstants.PR_IS_MAIN_LOGGEDIN_USER_KEY, 0) == App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0))
                    {
                        await ShellMasterPage.CurrentShell.PushMainPageAsync(new HealthAccountConnectPage(true)).ConfigureAwait(true);
                        return false;
                    }
                    break;
                case 2:
                    await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(ErrorCode.RestartApp.ToString())).ConfigureAwait(true);
                    return false;
                default:
                    // Continue 
                    break;
            }
        }
        await NavigateOnNextSyncFlowAsync(isBeforeLogin, flowFrom, Pages.HealthAccountConnectPage).ConfigureAwait(true);
        return true;
    }

    private async Task NavigateOnTaskCompletionFlowAsync()
    {
        switch (await new PatientTaskService(App._essentials).IsTaskCompletionRequiredAsync().ConfigureAwait(true))
        {
            case 2:
                await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(ErrorCode.RestartApp.ToString())).ConfigureAwait(true);
                break;
            case 1:
                // Navigation on task list to complete execute on login tasks
                await PushPageByTargetAsync(Pages.PatientTasksPage.ToString()
                    , GenericMethods.GenerateParamsWithPlaceholder(Param.identifier)
                    , true.ToString(CultureInfo.InvariantCulture)
                ).ConfigureAwait(true);
                break;
            default:
                // Continue 
                break;
        }
    }
}