using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Web;

namespace AlphaMDHealth.MobileClient;

public class ShellMasterPage : Shell
{
    /// <summary>
    /// Indicates if page has main page
    /// </summary>
    public bool HasMainPage { get; private set; } = true;

    /// <summary>
    /// Gets the current main page if any
    /// </summary>
    public Page MainPage
    {
        get
        {
            return Navigation.ModalStack?.FirstOrDefault();
        }
    }

    /// <summary>
    /// Gets the current instance of shell
    /// </summary>
    public static ShellMasterPage CurrentShell
    {
        get
        {
            return Application.Current.MainPage is ShellMasterPage
                ? Application.Current.MainPage as ShellMasterPage
                : null;
        }
    }

    /// <summary>
    /// Gets the Current Page loaded in shell
    /// </summary>
    public BasePage CurrentPage
    {
        get
        {
            if (Current != null)
            {
                Page currentPage = (Current.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage ?? Current.Navigation?.NavigationStack?.LastOrDefault();
                if (currentPage is BasePage)
                {
                    return currentPage as BasePage;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Footer tabs
    /// </summary>
    public TabBar FooterTabs { get; private set; }

    /// <summary>
    /// Instance of base page for common handling
    /// </summary>
    public BasePage PageInstance { get; private set; }

    /// <summary>
    /// Indicates if page navigation is in progress
    /// </summary>
    public bool IsNavigating { get; set; }

    private bool _isRendering;

    /// <summary>
    /// Selected footer menu
    /// </summary>
    public long FooterSelectionMenuKey { get; private set; }

    /// <summary>
    /// Last navigated path
    /// </summary>
    public string LastNavigationPath { get; private set; }


    //    /// <summary>
    //    /// Push notification target key
    //    /// </summary>
    //    public string NotificationTargetKey { get; protected set; }

    //    /// <summary>
    //    /// Push notification parameter
    //    /// </summary>
    //    public string NotificationParameter { get; protected set; }

    /// <summary>
    /// Shell master page
    /// </summary>
    /// <param name="baseContentPage">Instance of base content page</param>
    public ShellMasterPage(BasePage baseContentPage)
    {
        PageInstance = baseContentPage;
        FlyoutBehavior = FlyoutBehavior.Disabled;
        RegisterRoutes();
        FooterTabs = new TabBar { Route = Constants.BASE_FOOTER_ROUTE };
        AddDefaultTab();
        Items.Add(FooterTabs);
    }

    /// <summary>
    /// Set main page
    /// </summary>
    /// <param name="page">Page to be set as main page</param>
    /// <returns>Task for setting the main page</returns>
    /// <remarks>Note: Use this method only to set MainPage. It should not be used for any other modal pages.</remarks>
    public async Task PushMainPageAsync(Page page)
    {
        HasMainPage = true;
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await ClearMainPageAsync().ConfigureAwait(true);
            await ShellMasterPage.Current.Navigation.PushModalAsync(page, false).ConfigureAwait(true);
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Pops main page
    /// </summary>
    /// <returns>Task for removing the main page</returns>
    /// <remarks>Note: Use this method only to set MainPage. It should not be used for any other modal pages.</remarks>
    public async Task PopMainPageAsync()
    {
        HasMainPage = false;
        await ClearMainPageAsync().ConfigureAwait(true);
    }

    protected async Task ClearMainPageAsync()
    {
        await PopPopupPagesAsync().ConfigureAwait(true);
        if (Navigation.ModalStack?.Count > 0)
        {
            await ShellMasterPage.Current.Navigation.PopModalAsync(false).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Clear shell content
    /// </summary>
    public void ClearShellContent()
    {
        var existingFooterItems = FooterTabs.Items.ToList();
        AddDefaultTab();
        // Remove all previous footer menus
        RemovePreviousMenus(existingFooterItems);
    }

    /// <summary>
    /// Render page async
    /// </summary>
    /// <returns>Renders page</returns>
    public async Task RenderPageAsync()
    {
        if (!_isRendering)
        {
            _isRendering = true;
            HasMainPage = false;
            Style = (Style)Application.Current.Resources[StyleConstants.ST_SHELL_MASTER_PAGE_STYLE];
            await ClearMainPageAsync().ConfigureAwait(true);
            await Task.Delay(10).ConfigureAwait(true);

            MenuDTO menuData = new MenuDTO { RecordCount = 5 };
            await new MenuService(App._essentials).GetTabbedMenusAsync(menuData).ConfigureAwait(true);
            if (menuData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(menuData.Menus))
            {
                await SetFooterMenusAsync(menuData.Menus).ConfigureAwait(true);
                //// Check if navigation for notification is required
                //if (!string.IsNullOrWhiteSpace(NotificationTargetKey))
                //{
                //    await NavigateOnNotificationClickAsync().ConfigureAwait(false);
                //}
            }
            else
            {
                AppHelper.ShowBusyIndicator = false;
                await PushErrorPageAsync(ErrorCode.RestartApp).ConfigureAwait(false);
            }
            _isRendering = false;
        }
    }

    /// <summary>
    /// Invoked when more option menu selection is changed
    /// </summary>
    /// <param name="selectedMenu">Currently selected menu</param>
    /// <returns>true if handled else return false</returns>
    public async Task<bool> OnMoreOptionSelectionChangeAsync(MenuModel selectedMenu)
    {
        if (selectedMenu.TargetPage == AppPermissions.Logout.ToString())
        {
            await LogoutAsync(false).ConfigureAwait(false);
            return true;
        }
        return false;
    }

    /// <summary>
    /// invoke logout and target navigation
    /// </summary>
    public async Task LogoutAsync(bool isLoginPage)
    {
        try
        {
            if (CurrentShell.CurrentPage != null)
            {
                await CurrentShell.CurrentPage.PopPageAsync(false).ConfigureAwait(true);
            }
            await ClearMainPageAsync().ConfigureAwait(true);
            if (isLoginPage)
            {
                await NavigateToLoginPageAsync().ConfigureAwait(true);
            }
            else
            {
                await CurrentShell.PushMainPageAsync(new PincodePage(AppPermissions.PincodeLoginView.ToString(), false)).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
        }
    }

    /// <summary>
    /// Navigates to login page after cleaning user info
    /// </summary>
    /// <returns>Operation status</returns>
    public async Task NavigateToLoginPageAsync()
    {
        await new AuthService(App._essentials).ClearAccountTokensAndIdAsync().ConfigureAwait(true);
        await PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
    }

    private async Task SetFooterMenusAsync(List<MenuModel> footerMenus)
    {
        //Dictionary<string, string> badgeCounts = await GetBadgeCountsAsync(footerMenus).ConfigureAwait(true);//todo:

        var existingFooterItems = FooterTabs.Items?.ToList();
        if (Device.RuntimePlatform == Device.Android)
        {
            // Remove all previous footer menus
            RemovePreviousMenus(existingFooterItems);
            AddDefaultTab();
            existingFooterItems = FooterTabs.Items.ToList();
        }

        //FooterTabs.Items?.Clear();//todo: toremove

        ////todo:
        foreach (MenuModel footerMenu in footerMenus)
        {
            FooterTabs.Items.Add(new Tab
            {
                Route = footerMenu.TargetPage,
                Icon = GetFooterIcon(footerMenu, false),
                Title = footerMenu.RenderType == MenuRenderType.OnlyIcon ? string.Empty : footerMenu.PageHeading,
                StyleId = footerMenu.TargetID.ToString(CultureInfo.InvariantCulture),
                //UnselectedIcon = GetFooterIcon(footerMenu, false),
                //SelectedIcon = GetFooterIcon(footerMenu, true),
                //BadgeCount = badgeCounts[footerMenu.TargetPage],
                //BadgeColor = (Color)Application.Current.Resources[StyleConstants.ST_ACCENT_COLOR],
                Items =
                {
                    new ShellContent
                    {
                        Title = footerMenu.PageHeading,
                        Route = GetTargetPageWithParameters(footerMenu),
                        ContentTemplate = new DataTemplate(GetTypeForRoute(footerMenu.TargetPage.Split(Constants.SYMBOL_QUESTION_MARK[0])[0]))
                    }
                }
            });
        }
        //// Remove all previous footer menus
        //RemovePreviousMenus(existingFooterItems);

        // Select first item in footer as default selection
        FooterSelectionMenuKey = footerMenus[0].TargetID;
    }

    private string GetTargetPageWithParameters(MenuModel footerMenu)
    {
        return $"{footerMenu.TargetPage}"
            + $"{(footerMenu.TargetPage.Contains(Constants.SYMBOL_QUESTION_MARK) ? Constants.SYMBOL_AMPERSAND : Constants.SYMBOL_QUESTION_MARK)}"
            + $"{Param.footerSettingKey}{Constants.SYMBOL_EQUAL}{footerMenu.TargetID}"
            + (footerMenu.TargetPage.Contains(Constants.STATIC_MESSAGE_PAGE)
                ? $"{Constants.SYMBOL_AMPERSAND}{Param.id}{Constants.SYMBOL_EQUAL}{footerMenu.Content}{Constants.SYMBOL_AMPERSAND}{Param.type}{Constants.SYMBOL_EQUAL}{PageType.ContentPage}"
                : string.Empty);
    }

    private void RegisterRoutes()
    {
        var assembly = typeof(ShellMasterPage).GetTypeInfo().Assembly;
        foreach (Type type in assembly.GetTypes())
        {
            object[] attributes = type.GetCustomAttributes(typeof(RouteRegistrationAttribute), true);
            if (attributes?.Length > 0)
            {
                Routing.RegisterRoute((attributes[0] as RouteRegistrationAttribute).Route, type);
            }
        }
    }

    private Type GetTypeForRoute(string route)
    {
        Type requiredType = null;
        Assembly assembly = typeof(ShellMasterPage).GetTypeInfo().Assembly;
        foreach (Type type in assembly.GetTypes())
        {
            object[] attributes = type.GetCustomAttributes(typeof(RouteRegistrationAttribute), true);
            if (attributes?.Length > 0 &&
                attributes.FirstOrDefault(x => route.Equals((x as RouteRegistrationAttribute).Route,
                StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                requiredType = type;
                break;
            }
        }
        return requiredType ?? typeof(Page);
    }

    private void AddDefaultTab()
    {
        //todo:
        FooterTabs.Items.Add(new Tab
        {
            Route = Constants.BASE_DUMMY_FOOTER_ROUTE,
            Items = {
                new ShellContent { Route = string.Empty, Content = new DefaultPage() }
            }
        });
    }

    private void RemovePreviousMenus(List<ShellSection> existingFooterItems)
    {
        foreach (ShellSection shellSection in existingFooterItems)
        {
            FooterTabs.Items.Remove(shellSection);
        }
    }

    private ImageSource GetFooterIcon(MenuModel footerMenu, bool selectedIcon)
    {
        if (footerMenu.RenderType == MenuRenderType.OnlyTitle || string.IsNullOrWhiteSpace(footerMenu.GroupIcon))
        {
            return string.Empty;
        }

        string iconName = GenericMethods.FetchMauiIcon(selectedIcon && !string.IsNullOrWhiteSpace(footerMenu.SelectedGroupIcon)
            ? footerMenu.SelectedGroupIcon
            : footerMenu.GroupIcon);

        return footerMenu.PageTypeID == MenuType.StaticPage
            ? ImageSource.FromStream(() => GenericMethods.GetMemoryStreamFromBase64(iconName))
            : ImageSource.FromFile(iconName);
    }

    /// <summary>
    /// Called when shell page is navigating to another page
    /// </summary>
    /// <param name="args">Navigation arguments</param>
    protected override async void OnNavigating(ShellNavigatingEventArgs args)
    {
        try
        {
            if (args.Source == ShellNavigationSource.PopToRoot)
            {
                return;
            }
            if (!HasMainPage && args.Source == ShellNavigationSource.Pop && !await IsBackAllowedAsync().ConfigureAwait(true))
            {
                args.Cancel();
                AppHelper.ShowBusyIndicator = false;
                return;
            }
            if (args.Source == ShellNavigationSource.Pop && (CurrentPage?.SendBackButtonPressed() ?? false))
            {
                args.Cancel();
                return;
            }
            if (CheckIfMoreOptionPage(args))
            {
                await IpadMoreOptionNavigationAsync(args).ConfigureAwait(false);
                return;
            }
            LastNavigationPath = args.Target.Location.ToString().Replace(Constants.BASE_ROUTE_PREFIX, string.Empty);
            await PopToRoot(args);
            await ShellNavigationAsync(args).ConfigureAwait(false);
            UpdateHeaderFooterPropertiesOnPop(args);
            base.OnNavigating(args);
        }
        catch (Exception ex)
        {
        }
    }

    private async Task PopPopupPagesAsync()
    {
        try
        {
            //todo:
            await Navigation.PopAsync(false);
        }
        catch
        {
            // In case of no popup, it will throw exception here
        }
    }

    //    ///// <summary>
    //    ///// Called when shell item is reselected
    //    ///// </summary>
    //    ///// <param name="shellSection">Shell section that is reselected</param>
    //    //public void ShellItemReselected(ShellSection shellSection)
    //    //{
    //    //    if (!_isRendering)
    //    //    {
    //    //        MainThread.BeginInvokeOnMainThread(async () =>
    //    //        {
    //    //            //todo:
    //    //            //if (MobileConstants.IsTablet && !HasMainPage
    //    //            //    && shellSection.Route.Contains(nameof(MoreOptionsPage))
    //    //            //    /*&& !AppStyles.IsTabletScaledView*/)
    //    //            //{
    //    //            //await Navigation.PushPopupAsync(new MoreOptionsPopupPage()).ConfigureAwait(false);
    //    //            //return;
    //    //            //}
    //    //            if (Current != null && Current.CurrentItem.Navigation.NavigationStack.Count > 1)
    //    //            {
    //    //                await Current.CurrentItem.Navigation.PopToRootAsync(false).ConfigureAwait(false);
    //    //            }
    //    //        });
    //    //    }
    //    //}

    //    /// <summary>
    //    /// Invoked when navigation is requested on notification click
    //    /// </summary>
    //    /// <returns>Task representing navigation operation to page received in notification</returns>
    //    public Task NavigateOnNotificationClickAsync()
    //    {
    //        return NavigateOnNotificationClickAsync(NotificationTargetKey, NotificationParameter);
    //    }

    private async Task PopToRoot(ShellNavigatingEventArgs args)
    {
        if (ShouldPopToRoot(args))
        {
            await Current.CurrentItem.Navigation.PopToRootAsync(false);
        }
    }

    private async Task IpadMoreOptionNavigationAsync(ShellNavigatingEventArgs args)
    {
        //if (!args.Target.Location.ToString().Contains(Param.isiPadMoreOption.ToString()))
        //{
        args.Cancel();
        if (args.Source == ShellNavigationSource.Pop)
        {
            AppHelper.ShowBusyIndicator = false;
        }
        else
        {
            //await Navigation.PushPopupAsync(new MoreOptionsPopupPage()).ConfigureAwait(false);
        }
        //}
    }

    private async Task ShellNavigationAsync(ShellNavigatingEventArgs args)
    {
        if (args.Source == ShellNavigationSource.ShellSectionChanged
            || args.Source == ShellNavigationSource.Pop
            || args.Source == ShellNavigationSource.Unknown)
        {
            SetFooterPageHeaderAndUpdateLastPage(args.Target.Location.OriginalString);
            if ((args.Source == ShellNavigationSource.ShellSectionChanged ||
                args.Source == ShellNavigationSource.Pop) && FooterSelectionMenuKey != 0
                && args.Source == ShellNavigationSource.ShellSectionChanged && Navigation.NavigationStack.Count > 1)
            {
                // Navigate back to root if any footer menu is tapped
                await Navigation.PopToRootAsync(false).ConfigureAwait(false);
            }
        }
        else
        {
            // Clear footer selection menu key as page has been navigated to some non-footer menu page
            FooterSelectionMenuKey = 0;
        }
    }

    //    private async Task<Dictionary<string, string>> GetBadgeCountsAsync(List<MenuModel> footerMenus)
    //    {
    //        List<Task<string>> badgeCountTasks = new List<Task<string>>();
    //        foreach (MenuModel footerMenu in footerMenus)
    //        {
    //            badgeCountTasks.Add(GetBadgeCountAsync(footerMenu.TargetPage));
    //        }
    //        await Task.WhenAll(badgeCountTasks).ConfigureAwait(true);
    //        Dictionary<string, string> badgeCounts = new Dictionary<string, string>();
    //        for (int i = 0; i < footerMenus.Count; i++)
    //        {
    //            badgeCounts.Add(footerMenus[i].TargetPage, await badgeCountTasks[i]);
    //        }
    //        return badgeCounts;
    //    }

    private void SetFooterPageHeaderAndUpdateLastPage(string locationString)
    {
        if (!string.IsNullOrWhiteSpace(locationString))
        {
            locationString = locationString.Substring(locationString.LastIndexOf(Constants.SYMBOL_SLASH) + 1);
            if (locationString.Contains(Param.footerSettingKey.ToString()))
            {
                // This means that navigation has taken place due to footer tab click, so we need to get headerMenu for page
                FooterSelectionMenuKey = GetFooterNodeID(locationString);
            }
            else
            {
                FooterSelectionMenuKey = 0;
            }
        }
    }

    private long GetFooterNodeID(string locationString)
    {
        UriBuilder serviceUri = new UriBuilder(new Uri("http://" + locationString));
        NameValueCollection query = HttpUtility.ParseQueryString(serviceUri.Query);
        FooterSelectionMenuKey = Convert.ToInt64(query[Param.footerSettingKey.ToString()], CultureInfo.InvariantCulture);
        return FooterSelectionMenuKey;
    }

    private void UpdateHeaderFooterPropertiesOnPop(ShellNavigatingEventArgs args)
    {
        if (args.Source == ShellNavigationSource.Pop)
        {
            BasePage previousPage = Navigation.NavigationStack[Navigation.NavigationStack.Count - 2] as BasePage;
            bool isAddEdit = previousPage?.IsAddEditPage ?? false;
            App._essentials.SetPreferenceValue(StorageConstants.PR_IS_ADD_EDIT_PAGE_KEY, isAddEdit);
            previousPage?.HideFooter(isAddEdit);
        }
    }

    private bool IsMoreOptionPage(string targetLocation)
    {
        string currentLocation = targetLocation.Split(Constants.SYMBOL_SLASH).Last();
        return currentLocation.Contains(nameof(MoreOptionsPage))
            || (MobileConstants.IsIosPlatform
                && currentLocation.Contains(LastNavigationPath.Split(Constants.SYMBOL_SLASH)
                    .Last().Split(Constants.SYMBOL_QUESTION_MARK[0]).First()));
    }

    private static bool ShouldPopToRoot(ShellNavigatingEventArgs args)
    {
        return Current != null
            && args.Source == ShellNavigationSource.ShellSectionChanged
            && Current.CurrentItem.Navigation.NavigationStack.Count > 1;
    }

    private bool CheckIfMoreOptionPage(ShellNavigatingEventArgs args)
    {
        //return false;
        //todo:
        return MobileConstants.IsTablet
            //&& !AppStyles.IsTabletScaledView
            && (args.Target.Location.ToString().Contains(nameof(MoreOptionsPage) + Constants.SYMBOL_QUESTION_MARK))
            && IsMoreOptionPage(args.Target.Location.ToString());
    }

    //    /////////////////////////////////////////////////////////////////////////////////////

    //    /// <summary>
    //    /// Get badge count
    //    /// </summary>
    //    /// <param name="target">target for which badge is to be updated</param>
    //    /// <returns>badge count</returns>
    //    protected async Task<string> GetBadgeCountAsync(string target)
    //    {
    //        return await new BaseService(App._essentials).GetBadgeCountAsync(target).ConfigureAwait(false);
    //    }

    /// <summary>
    /// Push error page
    /// </summary>
    /// <param name="errorCode">Error code for which error page is shown</param>
    /// <returns>Task representing push error page task</returns>
    protected Task PushErrorPageAsync(ErrorCode errorCode)
    {
        return PushMainPageAsync(new StaticMessagePage(errorCode.ToString()));
    }

    //    ///// <summary>
    //    ///// Create data for parameters string
    //    ///// </summary>
    //    ///// <param name="input">values for pass as parameter</param>
    //    ///// <returns>parameters string</returns>
    //    //public static Task<string> GenerateParamAsync(params string[] input)
    //    //{
    //    //    return Task.Run(() =>
    //    //    {
    //    //        string finalString = string.Empty;
    //    //        try
    //    //        {
    //    //            foreach (string value in input)
    //    //            {
    //    //                var type = value?.GetType();
    //    //                if (type != null)
    //    //                {
    //    //                    finalString = finalString + Constants.SYMBOL_PARAM_SEPERATOR + Convert.ToString(type) + Constants.SYMBOL_PARAM_SEPERATOR + value;
    //    //                }
    //    //            }
    //    //        }
    //    //        catch
    //    //        {
    //    //            // write exception handling
    //    //        }
    //    //        return finalString;
    //    //    });
    //    //}

    /// <summary>
    /// Checks if back navigation is allowed
    /// </summary>
    /// <returns>Returns true if back navigation is allowed else returns false</returns>
    protected Task<bool> IsBackAllowedAsync()
    {
        //todo:
        //if (CurrentPage is UserConsentsPage)
        //{
        //    return Task.FromResult(false);
        //}
        return Task.FromResult(true);
    }

    //    /// <summary>
    //    /// Invoked when navigation is requested on notification click
    //    /// </summary>
    //    /// <param name="targetKey">page name</param>
    //    /// <param name="parameter">page parameter</param>
    //    /// <returns>Task representing navigation operation to page received in notification</returns>
    //    public async Task NavigateOnNotificationClickAsync(string targetKey, string parameter)
    //    {
    //        try
    //        {
    //            if (HasMainPage)
    //            {
    //                NotificationTargetKey = targetKey;
    //                NotificationParameter = parameter;
    //            }
    //            else
    //            {
    //                NotificationTargetKey = string.Empty;
    //                NotificationParameter = string.Empty;
    //                ResetNotificationValue();
    //                switch (targetKey.ToEnum<Pages>())
    //                {
    //                    case Pages.ChatsPage:
    //                    case Pages.ChatPage:
    //                        await NavigateToChatPageAsync(targetKey, parameter).ConfigureAwait(true);
    //                        break;
    //                    case Pages.PatientTasksPage:
    //                    case Pages.PatientTaskPage:
    //                        await NavigateToTaskPageAsync(targetKey, parameter).ConfigureAwait(true);
    //                        break;
    //                    case Pages.AppointmentsPage:
    //                    case Pages.AppointmentViewPage:
    //                        await NavigateToAppointmentPageAsync(targetKey, parameter).ConfigureAwait(true);
    //                        break;
    //                    case Pages.PatientFilesPage:
    //                    case Pages.PatientFilePage:
    //                        await NavigateToFilePageAsync(targetKey, parameter).ConfigureAwait(false);
    //                        break;
    //                    default:
    //                        await MainThread.InvokeOnMainThreadAsync(async () =>
    //                        {
    //                            if (string.IsNullOrWhiteSpace(parameter))
    //                            {
    //                                await BaseContentPageInstance.PushPageByTargetAsync(targetKey, true, string.Empty).ConfigureAwait(false);
    //                            }
    //                            else
    //                            {
    //                                await BaseContentPageInstance.PushPageByTargetAsync(targetKey, true
    //                                    , GenericMethods.GenerateParamsWithPlaceholder(targetKey == Pages.AppointmentsPage.ToString() ? Param.appointmentID : Param.id)
    //                                    , parameter
    //                                ).ConfigureAwait(true);
    //                            }
    //                        });
    //                        break;
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            BaseContentPageInstance.LogErrors("NavigateOnNotificationClickAsync()", ex);
    //        }
    //    }

    //    private async Task NavigateToFilePageAsync(string targetKey, string parameter)
    //    {
    //        AppHelper.ShowBusyIndicator = true;
    //        await BaseContentPageInstance.SyncDataWithServerAsync(Pages.PatientFilesView, false, default).ConfigureAwait(false);
    //        FileDTO fileData = new FileDTO
    //        {
    //            File = new FileModel
    //            {
    //                FileID = new Guid(parameter)
    //            },
    //            RecordCount = -1
    //        };
    //        await new FileService(App._essentials).GetFilesAsync(fileData).ConfigureAwait(true);
    //        NavigateToTargetPage(targetKey, parameter, fileData.ErrCode == ErrorCode.OK && fileData.File != null);
    //    }

    //    private async Task NavigateToAppointmentPageAsync(string targetKey, string parameter)
    //    {
    //        AppHelper.ShowBusyIndicator = true;
    //        await BaseContentPageInstance.SyncDataWithServerAsync(Pages.AppointmentsView, false, default).ConfigureAwait(false);
    //        AppointmentDTO appointmentData = new AppointmentDTO
    //        {
    //            Appointment = new AppointmentModel
    //            {
    //                AppointmentID = Convert.ToInt64(parameter, CultureInfo.InvariantCulture)
    //            }
    //        };
    //        await new AppointmentService(App._essentials).GetAppointmentsAsync(appointmentData).ConfigureAwait(true);
    //        NavigateToTargetPage(targetKey, parameter, appointmentData.ErrCode == ErrorCode.OK && appointmentData.Appointment != null);
    //    }

    //    private async Task NavigateToTaskPageAsync(string targetKey, string parameter)
    //    {
    //        AppHelper.ShowBusyIndicator = true;
    //        await BaseContentPageInstance.SyncDataWithServerAsync(Pages.PatientTasksView, false, default).ConfigureAwait(false);
    //        ProgramDTO taskData = new ProgramDTO
    //        {
    //            Task = new TaskModel
    //            {
    //                PatientTaskID = Convert.ToInt64(parameter, CultureInfo.InvariantCulture)
    //            }
    //        };
    //        await new PatientTaskService(App._essentials).GetPatientTaskAsync(taskData).ConfigureAwait(true);
    //        NavigateToTargetPage(targetKey, parameter, taskData.ErrCode == ErrorCode.OK && taskData.Task != null);
    //    }

    //    private async Task NavigateToChatPageAsync(string targetKey, string parameter)
    //    {
    //        AppHelper.ShowBusyIndicator = true;
    //        await BaseContentPageInstance.SyncDataWithServerAsync(Pages.ChatView, false, default).ConfigureAwait(false);
    //        ChatDTO userChat = new ChatDTO
    //        {
    //            ChatDetail = new ChatDetailModel
    //            {
    //                ChatDetailID = new Guid(parameter)
    //            }
    //        };
    //        await new ChatService(App._essentials).GetChatDetailAsync(userChat).ConfigureAwait(true);
    //        NavigateToTargetPage(targetKey, parameter, userChat.ErrCode == ErrorCode.OK && userChat.ChatDetails?.Count > 0);
    //    }

    //    private void NavigateToTargetPage(string targetKey, string parameter, bool isSuccess)
    //    {
    //        if (isSuccess)
    //        {
    //            MainThread.BeginInvokeOnMainThread(async () =>
    //            {
    //                await BaseContentPageInstance.PushPageByTargetAsync(targetKey, true
    //                    , GenericMethods.GenerateParamsWithPlaceholder(
    //                        (targetKey == Pages.AppointmentViewPage.ToString() || targetKey == Pages.AppointmentsPage.ToString())
    //                            ? Param.appointmentID
    //                            : Param.id)
    //                    , parameter).ConfigureAwait(true);
    //            });
    //        }
    //        else
    //        {
    //            AppHelper.ShowBusyIndicator = false;
    //        }
    //    }

    //    private static void ResetNotificationValue()
    //    {
    //        App.NotificationParameter = string.Empty;
    //        App.NotificationTarget = null;
    //    }

    //    ///// <summary>
    //    ///// Get more option menus
    //    ///// </summary>
    //    ///// <returns>List of more option menus</returns>
    //    //public async Task<IEnumerable<IGrouping<(byte SequenceNo, long MenuGroupID, string Content), MenuModel>>> GetMoreMenusAsync()
    //    //{
    //    //    MenuDTO moreMenus = new MenuDTO();
    //    //    await new MenuService(App._essentials).GetMoreMenusAsync(moreMenus).ConfigureAwait(false);
    //    //    if (moreMenus.ErrCode == ErrorCode.OK && moreMenus.MoreOptionMenus?.Count() > 0)
    //    //    {
    //    //        return moreMenus.MoreOptionMenus;
    //    //    }
    //    //    return null;
    //    //}

    //    ///// <summary>
    //    ///// Invoked when more option menu selection is changed
    //    ///// </summary>
    //    ///// <param name="selectedMenu">Currently selected menu</param>
    //    ///// <returns>true if handled else return false</returns>
    //    //public async Task<bool> OnMoreOptionSelectionChangeAsync(MenuModel selectedMenu)
    //    //{
    //    //    if (selectedMenu.TargetPage == AppPermissions.Logout.ToString())
    //    //    {
    //    //        if (ShellMasterPage.CurrentShell.CurrentPage != null)
    //    //        {
    //    //            await ShellMasterPage.CurrentShell.CurrentPage.PopPageAsync(false).ConfigureAwait(false);
    //    //        }
    //    //        await ClearMainPageAsync().ConfigureAwait(true);
    //    //        await ShellMasterPage.CurrentShell.PushMainPageAsync(new PincodePage(AppPermissions.PincodeLoginView.ToString(), false)).ConfigureAwait(false);
    //    //        return true;
    //    //    }
    //    //    return false;
    //    //}

    //    //todo:
    //    ///// <summary>
    //    ///// Update badge count
    //    ///// </summary>
    //    ///// <param name="targetPage">Target for which badge is to be updated</param>
    //    ///// <param name="badgeCount">Badge count to be set</param>
    //    //public void UpdateBadgeCount(string targetPage, string badgeCount)
    //    //{
    //    //    var footerTab = FooterTabs.Items.FirstOrDefault(x => x.Route == targetPage);
    //    //    if (footerTab != null)
    //    //    {
    //    //        (footerTab as CustomTab).BadgeCount = badgeCount;
    //    //    }
    //    //}
}
