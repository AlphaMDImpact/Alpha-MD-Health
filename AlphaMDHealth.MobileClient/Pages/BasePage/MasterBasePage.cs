using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;

namespace AlphaMDHealth.MobileClient;

public partial class BasePage
{
    private Grid _headerGrid;
    private long _nodeID;

    /// <summary>
    /// True if page is for split in tablet
    /// </summary>
    public bool IsAddEditPage { get; protected set; }

    /// <summary>
    /// Task completion source to wait till header is loaded
    /// </summary>
    protected TaskCompletionSource<bool> _setHeaderCompletionSource;

    #region Shell Page Header

    /// <summary>
    /// Hide/show left item of given header
    /// </summary>
    /// <param name="menuLocation">Header location</param>
    /// <param name="show">true if should be shown, else false</param>
    public void ShowHideLeftRightHeader(MenuLocation menuLocation, bool show)
    {
        switch (menuLocation)
        {
            case MenuLocation.Left:
                if (_headerGrid?.Children?.Count > 0)
                {
                    //todo:
                    //_headerGrid.Children[0].IsVisible = show;
                }
                break;
            case MenuLocation.Right:
                if (_headerGrid?.Children?.Count > 1)
                {
                    //todo:
                    //_headerGrid.Children[1].IsVisible = show;
                }
                break;
        }
    }
    //todo:
    ///// <summary>
    ///// Overrides the page title view
    ///// </summary>
    ///// <param name="headerView">Header Menu item</param>
    ///// <returns>Updates the header title</returns>
    //public async Task OverrideTitleViewAsync(MenuView headerView)
    //{
    //    await _setHeaderCompletionSource.Task.ConfigureAwait(true);
    //    _headerGrid.RemoveAt(2);
    //    _headerGrid.Add(headerView, 1, 0);
    //}

    /// <summary>
    /// Removes page from the navigation stack
    /// </summary>
    /// <returns>A task that removes the page from the navigation stack</returns>
    public async Task RemovePageAsync()
    {
        await Task.Delay(50).ConfigureAwait(true);
        Shell.Current.Navigation.RemovePage(this);
    }

    /// <summary>
    /// Set Header Menus Async
    /// </summary>
    /// <returns>Sets the current header items</returns>
    protected async Task SetHeaderMenusAsync()
    {
        if (ShellMasterPage.CurrentShell.FooterSelectionMenuKey != 0)
        {
            if (_setHeaderCompletionSource.Task.IsCompleted)
            {
                _setHeaderCompletionSource = new TaskCompletionSource<bool>();
            }
            MobileMenuNodeModel mobileMenuNode = new MobileMenuNodeModel { MobileMenuNodeID = ShellMasterPage.CurrentShell.FooterSelectionMenuKey };
            mobileMenuNode = await GetPageDataByNodeIDAsync(mobileMenuNode).ConfigureAwait(true);
            if (mobileMenuNode == null)
            {
                _setHeaderCompletionSource.TrySetResult(false);
            }
            else
            {
                SetHeaderItems(mobileMenuNode);
            }
        }
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IconOverride = string.Empty,
            TextOverride = null,
            IsEnabled = false
        });
    }

    /// <summary>
    /// Sets the header content of the given page
    /// </summary>
    /// <param name="nodeData">node data for header items</param>
    public void SetHeaderItems(MobileMenuNodeModel nodeData)
    {
        IsAddEditPage = nodeData.IsAddEditPage;
        if (_headerGrid == null)
        {
            GenerateContentHeader(true);
        }
        _headerGrid.Children?.Clear();
        _nodeID = nodeData.MobileMenuNodeID;
        SetLeftRightHeaderMenus(nodeData, MenuLocation.Left.ToString());
        if (!string.IsNullOrWhiteSpace(nodeData.PageHeading))
        {
            Title = nodeData.PageHeading;
        }
        _headerGrid.Add(new MenuView(MenuLocation.Header, nodeData.PageHeading, IsAddEditPage), 1, 0);
        Shell.SetTitleView(this, _headerGrid);
        if (ShouldHideBackAction(nodeData))
        {
            ShowHideLeftRightHeader(MenuLocation.Left, false);
        }
        if (!_setHeaderCompletionSource.Task.IsCompleted)
        {
            _setHeaderCompletionSource.SetResult(true);
        }
        if (nodeData.IsMaster)
        {
            Shell.SetNavBarIsVisible(this, false);
            Shell.SetTabBarIsVisible(this, false);
        }
        else
        {
            Shell.SetNavBarIsVisible(this, true);
        }
    }

    private void SetLeftRightHeaderMenus(MobileMenuNodeModel nodeData, string headerLocation)
    {
        // Left menu
        _headerGrid.Add(new MenuView(MenuLocation.Left,
            nodeData.ShowIconInLeftMenu == true ? nodeData.LeftMenuAction : string.Empty,
            nodeData.LeftMenuActionID,
            nodeData.LeftMenuNodeID ?? 0,
            profileImage: nodeData.ImageName,
            profileInitials: nodeData.ImageName,
            nodeData.LeftMenuActionText,
            nodeData.IsAddEditPage)
        { StyleId = headerLocation }, 0, 0);

        // Right menu
        _headerGrid.Add(new MenuView(MenuLocation.Right,
            nodeData.ShowIconInRightMenu == true ? nodeData.RightMenuAction : string.Empty,
            nodeData.RightMenuActionID,
            nodeData.RightMenuNodeID ?? 0,
            profileImage: nodeData.ImageName,
            profileInitials: nodeData.ImageName,
            nodeData.RightMenuActionText,
            nodeData.IsAddEditPage)
        { StyleId = headerLocation }, 2, 0);
    }

    /// <summary>
    /// Refreshes the header items
    /// </summary>
    /// <returns>Operation result</returns>
    public async Task RefreshHeaderItemsAsync()
    {
        if (_nodeID != 0)
        {
            MobileMenuNodeModel mobileMenuNode = new MobileMenuNodeModel { MobileMenuNodeID = _nodeID };
            mobileMenuNode = await GetPageDataByNodeIDAsync(mobileMenuNode).ConfigureAwait(true);
            if (mobileMenuNode != null)
            {
                SetHeaderItems(mobileMenuNode);
            }
        }
    }

    private void GenerateContentHeader(bool isLeft)
    {
        double padding = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
        _headerGrid = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(IsAddEditPage? 80 + padding : 41, GridUnitType.Absolute) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(IsAddEditPage? 80 + padding : 41, GridUnitType.Absolute) },
            }
        };
        isLeft = _headerGrid.FlowDirection == FlowDirection.RightToLeft ? !isLeft : isLeft;
        _headerGrid.Margin = isLeft ? new Thickness(0, 0, padding, 0) : new Thickness(padding, 0, 0, 0);
    }

    private bool ShouldHideBackAction(MobileMenuNodeModel nodeData)
    {
        return (Shell.Current?.Navigation?.NavigationStack?.Count <= 1
           || IsMoreOptionRootPage(Shell.Current?.CurrentState?.Location.ToString()))
            && nodeData.LeftMenuActionID == MenuAction.MenuActionBackKey;
    }
    /// <summary>
    /// Checks if root page is more options page
    /// </summary>
    /// <param name="location">location to be checked</param>
    /// <returns>returns true if more options page is the root page</returns>
    public bool IsMoreOptionRootPage(string location)
    {
        return /*!AppStyles.IsTabletScaledView &&*/ location.Contains(nameof(MoreOptionsPage))
            && location.Split(new[] { nameof(MoreOptionsPage) }, StringSplitOptions.None).Last().Split(Constants.SYMBOL_SLASH).Count() < 3;
    }

    /// <summary>
    /// Show hide footer menus
    /// </summary>
    /// <param name="hideFooterMenus"></param>
    public void HideFooter(bool hideFooterMenus)
    {
        Shell.SetTabBarIsVisible(this, !hideFooterMenus);
    }

    #endregion

    #region Page Navigations

    /// <summary>
    /// Fetch menu data from db as per menuKey, update header tab data and redirect on target page.
    /// </summary>
    /// <param name="nodeID">Node ID for which header need to apply</param>
    /// <param name="paramValues">Dynamic parameter list</param>
    public async Task PushPageByNodeIDAsync(long nodeID, params string[] paramValues)
    {
        await PushPageByNodeIDAsync(nodeID, false, string.Empty, paramValues).ConfigureAwait(true);
    }

    /// <summary>
    /// Fetch menu data from db as per menuKey, update header tab data and redirect on target page.
    /// </summary>
    /// <param name="nodeID">Node ID for which header need to apply</param>
    /// <param name="isFromMoreOptions">true if called from more options</param>
    /// <param name="paramPlaceholders">Param placeholders for holding param values</param>
    /// <param name="paramValues">Dynamic parameter list</param>
    public async Task PushPageByNodeIDAsync(long nodeID, bool isFromMoreOptions, string paramPlaceholders, params string[] paramValues)
    {
        if (!ShellMasterPage.CurrentShell.IsNavigating)
        {
            ShellMasterPage.CurrentShell.IsNavigating = true;
            AppHelper.ShowBusyIndicator = true;
            MobileMenuNodeModel mobileMenuNode = new MobileMenuNodeModel { MobileMenuNodeID = nodeID };
            mobileMenuNode = await GetPageDataByNodeIDAsync(mobileMenuNode).ConfigureAwait(true);
            if (mobileMenuNode != null)
            {
                //if (isFromMoreOptions /*&& !AppStyles.IsTabletScaledView*/ && MobileConstants.IsTablet)
                //{
                //    if (MobileConstants.IsIosPlatform)
                //    {
                //        MobileMenuNodeModel moreMenuNode = new MobileMenuNodeModel { MobileMenuNodeID = -1 };
                //        moreMenuNode = await GetPageDataByNodeIDAsync(moreMenuNode).ConfigureAwait(true);
                //        if (moreMenuNode == null)
                //        {
                //            AppHelper.ShowBusyIndicator = false;
                //        }
                //        else
                //        {
                //            await PushPageAsync(moreMenuNode, GenericMethods.GenerateParamsWithPlaceholder(Param.isiPadMoreOption), true.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
                //        }
                //    }
                //    else
                //    {
                //        mobileMenuNode.TargetPage = $"{Constants.SYMBOL_SLASH}{Constants.SYMBOL_SLASH}{nameof(MoreOptionsPage)}{Constants.SYMBOL_SLASH}{mobileMenuNode.TargetPage}";
                //    }
                //}
                await PushPageAsync(mobileMenuNode, paramPlaceholders, paramValues).ConfigureAwait(true);
            }
            else
            {
                AppHelper.ShowBusyIndicator = false;
            }
            ShellMasterPage.CurrentShell.IsNavigating = false;
        }
    }

    /// <summary>
    /// Push page by target
    /// </summary>
    /// <param name="targetName">target name</param>
    /// <param name="paramPlaceholders">Params with placeholder for values</param>
    /// <param name="paramValues">Param values</param>
    /// <returns>Sets Page based on the target</returns>
    public async Task PushPageByTargetAsync(string targetName, string paramPlaceholders, params string[] paramValues)
    {
        await PushPageByTargetAsync(targetName, false, paramPlaceholders, paramValues);
    }

    /// <summary>
    /// Push page by target
    /// </summary>
    /// <param name="targetName">target name</param>
    /// <param name="shouldPopToRoot">true if stack is to be reset to root and the new page is to be pushed</param>
    /// <param name="paramPlaceholders">Params with placeholder for values</param>
    /// <param name="paramValues">Param values</param>
    /// <returns>Sets Page based on the target</returns>
    public async Task PushPageByTargetAsync(string targetName, bool shouldPopToRoot, string paramPlaceholders, params string[] paramValues)
    {
        if (!ShellMasterPage.CurrentShell.IsNavigating)
        {
            ShellMasterPage.CurrentShell.IsNavigating = true;
            AppHelper.ShowBusyIndicator = true;
            MobileMenuNodeModel mobileMenuNode = new MobileMenuNodeModel();
            if (targetName == Constants.STATIC_MESSAGE_PAGE_IDENTIFIER)
            {
                UriBuilder serviceUri = new UriBuilder(new Uri("http://" + targetName + string.Format(paramPlaceholders, paramValues)));
                NameValueCollection query = HttpUtility.ParseQueryString(serviceUri.Query);
                mobileMenuNode.NodeName = targetName;
                mobileMenuNode.TargetID = Convert.ToInt64(query[Param.id.ToString()], CultureInfo.InvariantCulture);
            }
            else
            {
                mobileMenuNode.TargetPage = targetName;
            }
            mobileMenuNode = await GetPageDataByNodeIDAsync(mobileMenuNode).ConfigureAwait(true);
            if (mobileMenuNode != null)
            {
                if (shouldPopToRoot)
                {
                    await ShellMasterPage.Current.Navigation.PopToRootAsync(false).ConfigureAwait(true);
                }
                await PushPageAsync(mobileMenuNode, paramPlaceholders, paramValues).ConfigureAwait(true);
            }
            else
            {
                AppHelper.ShowBusyIndicator = false;
            }
            ShellMasterPage.CurrentShell.IsNavigating = false;
        }
    }

    /// <summary>
    /// Redirect on specific page and set page headers
    /// </summary>
    /// <param name="menuData">Redirection menu data</param>
    /// <param name="paramPlaceholders">Param placeholders for holding param values</param>
    /// <param name="paramValues">Dynamic parameter list</param>
    protected async Task PushPageAsync(MobileMenuNodeModel menuData, string paramPlaceholders, params string[] paramValues)
    {
        try
        {
            //When target is null return with not yet implemented error message.
            if (string.IsNullOrWhiteSpace(menuData.TargetPage))
            {
                AppHelper.ShowBusyIndicator = false;

                ////Console.WriteLine($"+++++||PushPageAsync() fata navigate App.NotificationParameter menuData.TargetPage {menuData.TargetPage}===+++++");
                await DisplayMessagePopupAsync(ErrorCode.NotImplemented.ToString(), false, false, false).ConfigureAwait(false);
                return;
            }
            //Redirect on new Content page with Updated header data
            if (ShellMasterPage.CurrentShell.HasMainPage)
            {
                AppHelper.ShowBusyIndicator = false;
                return;
            }
            if (ShellMasterPage.CurrentShell.LastNavigationPath.Contains(menuData.TargetPage))
            {
                AppHelper.ShowBusyIndicator = false;
            }

            string pageParam = string.Empty;
            if (menuData.NodeType == MenuType.StaticPage)
            {
                pageParam = string.Format(CultureInfo.InvariantCulture, GenericMethods.GenerateParamsWithPlaceholder(Param.id, Param.type), menuData.TargetID, PageType.ContentPage.ToString());
            }

            if (IsFooterMenu(menuData, pageParam))
            {
                if (menuData.NodeType == MenuType.StaticPage)
                {
                    paramPlaceholders = string.IsNullOrWhiteSpace(paramPlaceholders) ? pageParam : pageParam + paramPlaceholders.Replace(Constants.SYMBOL_QUESTION_MARK, Constants.SYMBOL_AMPERSAND);
                }
                await Shell.Current.GoToAsync(string.Format(CultureInfo.InvariantCulture, menuData.TargetPage + paramPlaceholders, paramValues), false).ConfigureAwait(true);
            }
            else
            {
                paramPlaceholders = GetPageParams(menuData, paramPlaceholders);
                await Shell.Current.GoToAsync(Constants.BASE_ROUTE_PREFIX + string.Format(CultureInfo.InvariantCulture, menuData.TargetPage + paramPlaceholders, paramValues), false).ConfigureAwait(true);
            }
            ShellMasterPage.CurrentShell.CurrentPage?.SetHeaderItems(menuData);
        }
        catch (Exception ex)
        {
            ////Console.WriteLine($"+++++||PushPageAsync() fata navigate App.NotificationParameter {ex}===+++++");
            // Navigation failure when route is not found
            await DisplayMessagePopupAsync(ErrorCode.NotImplemented.ToString(), false, false, false).ConfigureAwait(false);
            new BaseLibService(App._essentials).LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Pops the current page
    /// </summary>
    /// <param name="refreshHeader">true if header neeeds to be refreshed</param>
    /// <returns>Result of the pop operation</returns>
    public async Task PopPageAsync(bool refreshHeader)
    {
        if (Shell.Current.Navigation?.NavigationStack?.Count > 1)
        {
            await Shell.Current.Navigation.PopAsync(false).ConfigureAwait(true);
        }
        if (refreshHeader && ShellMasterPage.CurrentShell.CurrentPage != null)
        {
            await ShellMasterPage.CurrentShell.CurrentPage.RefreshHeaderItemsAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// method used to pop page from add edit pages
    /// </summary>
    public async Task PopPageAsyncForPhoneAsync()
    {
        if (MobileConstants.IsDevicePhone)
        {
            await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(false);
        }
    }

    #endregion

    #region Virtual methods

    /// <summary>
    /// Method to override to handle header item clicks
    /// </summary>
    /// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
    /// <param name="menuType">Menu position whether Left, Right</param>
    /// <param name="menuAction">Action type</param>
    public virtual async Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        await Task.Run(() => { }).ConfigureAwait(true);
    }

    /// <summary>
    /// Method to override when back or close is clicked
    /// </summary>
    /// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
    /// <param name="menuType">Menu position whether Left, Right</param>
    /// <param name="menuAction">Action type</param>
    /// <returns>true if navigation is handled else returns false if default handling is to be used</returns>
    public virtual Task<bool> OnBackCloseClickAsync(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        return Task.FromResult(false);
    }

    #endregion

    private string GetPageParams(MobileMenuNodeModel menuData, string paramPlaceholders)
    {
        string footerParam = menuData.NodeType == MenuType.StaticPage
            ? string.Format(CultureInfo.InvariantCulture, GenericMethods.GenerateParamsWithPlaceholder(Param.footerSettingKey, Param.id, Param.type), menuData.MobileMenuNodeID, menuData.TargetID, PageType.ContentPage.ToString())
            : string.Format(CultureInfo.InvariantCulture, GenericMethods.GenerateParamsWithPlaceholder(Param.footerSettingKey), menuData.MobileMenuNodeID);
        return string.IsNullOrWhiteSpace(paramPlaceholders) ? footerParam : footerParam + paramPlaceholders.Replace(Constants.SYMBOL_QUESTION_MARK, Constants.SYMBOL_AMPERSAND);
    }

    private bool IsFooterMenu(MobileMenuNodeModel menuData, string pageParam)
    {
        return ShellMasterPage.CurrentShell.FooterTabs.Items.FirstOrDefault(x =>
            x.Route == menuData.TargetPage && (string.IsNullOrWhiteSpace(pageParam) ||
                x.Route.Contains(pageParam.Replace(Constants.SYMBOL_QUESTION_MARK, string.Empty)))
        ) == null;
    }


    //private Grid _headerGrid;
    //private Grid _leftHeaderGrid;
    //private Grid _rightHeaderGrid;
    //private long _nodeID;

    ///// <summary>
    ///// True if page is for split in tablet
    ///// </summary>
    //public bool IsSplitView { get; protected set; }

    ///// <summary>
    ///// True if page is for split in tablet
    ///// </summary>
    //public bool IsAddEditPage { get; protected set; }

    ///// <summary>
    ///// Task completion source to wait till header is loaded
    ///// </summary>
    //protected TaskCompletionSource<bool> _setHeaderCompletionSource;

    //#region Shell Page Header

    ///// <summary>
    ///// Hide/show left item of given header
    ///// </summary>
    ///// <param name="headerLocation">Header location</param>
    ///// <param name="show">true if should be shown, else false</param>
    //protected void ShowHideLeftHeader(MenuLocation headerLocation, bool show)
    //{
    //    var headerGrid = headerLocation == MenuLocation.Left ? _leftHeaderGrid : _rightHeaderGrid;
    //    if (headerGrid?.Children?.Count > 0)
    //    {
    //        //todo:
    //        //headerGrid.Children[0].IsVisible = show;
    //    }
    //}

    ///// <summary>
    ///// Hide/show right item of given header
    ///// </summary>
    ///// <param name="headerLocation">Header location</param>
    ///// <param name="show">true if should be shown, else false</param>
    //public void ShowHideLeftRightHeader(MenuLocation headerLocation, bool show)
    //{
    //    var headerGrid = headerLocation == MenuLocation.Left ? _leftHeaderGrid : _rightHeaderGrid;
    //    if (headerGrid?.Children?.Count > 1)
    //    {
    //        //todo:
    //        //headerGrid.Children[1].IsVisible = show;
    //    }
    //}

    ///// <summary>
    ///// Expands or collapses left header
    ///// </summary>
    ///// <param name="locationToBeShown">Menu location to be shown</param>
    ///// <param name="expand">true if expanded view is to be displayed else false</param>
    //public void ExpandCollapseLeftHeader(MenuLocation locationToBeShown, bool expand)
    //{
    //    if (MobileConstants.IsTablet && _leftHeaderGrid != null)
    //    {
    //        int index = locationToBeShown == MenuLocation.Left ? 1 : 0;
    //        //todo:
    //        //_leftHeaderGrid.Children[index].IsVisible = expand;
    //        //if (_leftHeaderGrid.Children.Count > 2)
    //        //{
    //        //    _leftHeaderGrid.Children[2].IsVisible = expand;
    //        //}
    //        double padding = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
    //        int paddingAdjustment = Device.RuntimePlatform == Device.Android ? -1 : 3;
    //        _headerGrid.ColumnDefinitions[0].Width = expand
    //            ? new GridLength(0.3 * App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, (double)0)
    //                - GenericMethods.GetPlatformSpecificValue(padding - 8, padding - 4, 0), GridUnitType.Absolute)
    //            : (AppStyles.GetImageSize(AppImageSize.ImageSizeL) + paddingAdjustment + 3 * padding);
    //    }
    //}

    ///// <summary>
    ///// Overrides the page title view
    ///// </summary>
    ///// <param name="headerView">Header Menu item</param>
    ///// <returns>Updates the header title</returns>
    //public async Task OverrideTitleViewAsync(MenuView headerView)
    //{
    //    await _setHeaderCompletionSource.Task.ConfigureAwait(true);
    //    _leftHeaderGrid.Children.RemoveAt(2);
    //    _leftHeaderGrid.Add(headerView, 1, 0);
    //}

    ///// <summary>
    ///// Overrides the right page title view
    ///// </summary>
    ///// <param name="headerView">Header Menu item</param>
    ///// <returns>Updates the header title</returns>
    //public async Task OverrideTitleViewAsync(MenuView headerView)
    //{
    //    await _setHeaderCompletionSource.Task.ConfigureAwait(true);
    //    if (_rightHeaderGrid.Children?.Count > 2)
    //    {
    //        _rightHeaderGrid.Children.RemoveAt(2);
    //    }
    //    _rightHeaderGrid.Add(headerView, 1, 0);
    //}

    ///// <summary>
    ///// Removes page from the navigation stack
    ///// </summary>
    ///// <returns>A task that removes the page from the navigation stack</returns>
    //public async Task RemovePageAsync()
    //{
    //    await Task.Delay(50).ConfigureAwait(true);
    //    Shell.Current.Navigation.RemovePage(this);
    //}

    ///// <summary>
    ///// Set Header Menus Async
    ///// </summary>
    ///// <returns>Sets the current header items</returns>
    //protected async Task SetHeaderMenusAsync()
    //{
    //    if (ShellMasterPage.CurrentShell.FooterSelectionMenuKey != 0)
    //    {
    //        if (_setHeaderCompletionSource.Task.IsCompleted)
    //        {
    //            _setHeaderCompletionSource = new TaskCompletionSource<bool>();
    //        }
    //        MobileMenuNodeModel mobileMenuNode = new MobileMenuNodeModel { MobileMenuNodeID = ShellMasterPage.CurrentShell.FooterSelectionMenuKey };
    //        mobileMenuNode = await GetPageDataByNodeIDAsync(mobileMenuNode).ConfigureAwait(true);
    //        if (mobileMenuNode == null)
    //        {
    //            _setHeaderCompletionSource.TrySetResult(false);
    //        }
    //        else
    //        {
    //            SetHeaderItems(mobileMenuNode);
    //        }
    //    }
    //    Shell.SetBackButtonBehavior(this, new BackButtonBehavior
    //    {
    //        IconOverride = string.Empty,
    //        TextOverride = null,
    //        IsEnabled = false
    //    });
    //}

    ///// <summary>
    ///// Sets the header content of the given page
    ///// </summary>
    ///// <param name="nodeData">node data for header items</param>
    //public void SetHeaderItems(MobileMenuNodeModel nodeData)
    //{
    //    IsAddEditPage = nodeData.IsAddEditPage;
    //    if (_leftHeaderGrid == null)
    //    {
    //        GenerateHeader();
    //    }
    //    _leftHeaderGrid.Children?.Clear();
    //    _nodeID = nodeData.MobileMenuNodeID;
    //    SetLeftRightHeaderMenus(nodeData, _leftHeaderGrid, MenuLocation.Left.ToString());

    //    if (!string.IsNullOrWhiteSpace(nodeData.PageHeading))
    //    {
    //        Title = nodeData.PageHeading;
    //    }
    //    _leftHeaderGrid.Add(new MenuView(MenuLocation.Header, nodeData.PageHeading, IsAddEditPage), 1, 0);
    //    Shell.SetTitleView(this, _headerGrid);
    //    if (ShouldHideBackAction(nodeData))
    //    {
    //        ShowHideLeftHeader(MenuLocation.Left, false);
    //    }
    //    if (!_setHeaderCompletionSource.Task.IsCompleted)
    //    {
    //        _setHeaderCompletionSource.SetResult(true);
    //    }
    //    if (nodeData.IsMaster)
    //    {
    //        Shell.SetNavBarIsVisible(this, false);
    //        Shell.SetTabBarIsVisible(this, false);
    //    }
    //    else
    //    {
    //        Shell.SetNavBarIsVisible(this, true);
    //    }
    //}

    ///// <summary>
    ///// Sets the right header content of the given page
    ///// </summary>
    ///// <param name="targetPage">Target for which the header is to be loaded</param>
    ///// <returns>Updates the right header items</returns>
    //public async Task SetRightHeaderItemsAsync(string targetPage)
    //{
    //    MobileMenuNodeModel mobileMenuNode = new MobileMenuNodeModel { TargetPage = targetPage };
    //    mobileMenuNode = await GetPageDataByNodeIDAsync(mobileMenuNode).ConfigureAwait(true);
    //    if (mobileMenuNode != null)
    //    {
    //        _rightHeaderGrid.Children.Clear();
    //        SetLeftRightHeaderMenus(mobileMenuNode, _rightHeaderGrid, MenuLocation.Right.ToString());
    //        _rightHeaderGrid.Add(new MenuView(MenuLocation.Header, mobileMenuNode.PageHeading, IsAddEditPage), 1, 0);
    //        Shell.SetTitleView(this, _headerGrid);
    //        if (mobileMenuNode.LeftMenuActionID == MenuAction.MenuActionBackKey || mobileMenuNode.LeftMenuActionID == MenuAction.MenuActionCloseKey)
    //        {
    //            ShowHideLeftHeader(MenuLocation.Right, false);
    //        }
    //        if (mobileMenuNode.RightMenuActionID == MenuAction.MenuActionBackKey || mobileMenuNode.RightMenuActionID == MenuAction.MenuActionCloseKey)
    //        {
    //            ShowHideLeftRightHeader(MenuLocation.Right, false);
    //        }
    //    }
    //}

    ///// <summary>
    ///// Clear right header items
    ///// </summary>
    //public void ClearRightHeaderItems()
    //{
    //    if (_rightHeaderGrid?.Children.Count > 0)
    //    {
    //        _rightHeaderGrid.Children.Clear();
    //    }
    //}

    //private void SetLeftRightHeaderMenus(MobileMenuNodeModel nodeData, Grid headerGrid, string headerLocation)
    //{
    //    // Left menu
    //    headerGrid.Add(new MenuView(MenuLocation.Left,
    //        nodeData.ShowIconInLeftMenu == true ? nodeData.LeftMenuAction : string.Empty,
    //        nodeData.LeftMenuActionID,
    //        nodeData.LeftMenuNodeID ?? 0,
    //        profileImage: default, //todo: nodeData.ImageSource,
    //        profileInitials: nodeData.ImageName,
    //        nodeData.LeftMenuActionText,
    //        nodeData.IsAddEditPage)
    //    { StyleId = headerLocation }, 0, 0);
    //    // Right menu
    //    headerGrid.Add(new MenuView(MenuLocation.Right,
    //        nodeData.ShowIconInRightMenu == true ? nodeData.RightMenuAction : string.Empty,
    //        nodeData.RightMenuActionID,
    //        nodeData.RightMenuNodeID ?? 0,
    //        profileImage: default, //todo: nodeData.ImageSource,
    //        profileInitials: nodeData.ImageName,
    //        nodeData.RightMenuActionText,
    //        nodeData.IsAddEditPage)
    //    { StyleId = headerLocation }, 2, 0);
    //}

    //private void GenerateHeader()
    //{
    //    _leftHeaderGrid = GenerateContentHeader(true);
    //    if (IsSplitView)
    //    {
    //        _rightHeaderGrid = GenerateContentHeader(false);
    //        double padding = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
    //        _headerGrid = new Grid
    //        {
    //            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
    //            RowDefinitions =
    //            {
    //                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
    //            },
    //            ColumnDefinitions =
    //            {
    //                new ColumnDefinition { Width = new GridLength((0.3 * App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, (double)0) - GenericMethods.GetPlatformSpecificValue(padding-8, padding-4, 0)), GridUnitType.Absolute) },
    //                new ColumnDefinition { Width = GridLength.Auto },
    //                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
    //            }
    //        };
    //        _headerGrid.Add(_leftHeaderGrid, 0, 0);
    //        _headerGrid.Add(new BoxView
    //        {
    //            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE],
    //            WidthRequest = 1
    //        }, 1, 0);
    //        _headerGrid.Add(_rightHeaderGrid, 2, 0);
    //    }
    //    else
    //    {
    //        _headerGrid = _leftHeaderGrid;
    //    }
    //}

    ///// <summary>
    ///// Refreshes the header items
    ///// </summary>
    ///// <returns>Operation result</returns>
    //public async Task RefreshHeaderItemsAsync()
    //{
    //    if (_nodeID != 0)
    //    {
    //        MobileMenuNodeModel mobileMenuNode = new MobileMenuNodeModel { MobileMenuNodeID = _nodeID };
    //        mobileMenuNode = await GetPageDataByNodeIDAsync(mobileMenuNode).ConfigureAwait(true);
    //        if (mobileMenuNode != null)
    //        {
    //            SetHeaderItems(mobileMenuNode);
    //        }
    //    }
    //}

    //private Grid GenerateContentHeader(bool isLeft)
    //{
    //    double padding = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
    //    Grid headerGrid = new Grid
    //    {
    //        Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
    //        RowDefinitions =
    //            {
    //                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
    //            },
    //        ColumnDefinitions =
    //            {
    //                new ColumnDefinition { Width = new GridLength(IsAddEditPage? 80 + padding : 41, GridUnitType.Absolute) },
    //                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
    //                new ColumnDefinition { Width = new GridLength(IsAddEditPage? 80 + padding : 41, GridUnitType.Absolute) },
    //            }
    //    };
    //    isLeft = headerGrid.FlowDirection == Microsoft.Maui.FlowDirection.RightToLeft ? !isLeft : isLeft;
    //    headerGrid.Margin = isLeft ? new Thickness(0, 0, padding, 0) : new Thickness(padding, 0, 0, 0);
    //    return headerGrid;
    //}

    //private bool ShouldHideBackAction(MobileMenuNodeModel nodeData)
    //{
    //    return (Shell.Current?.Navigation?.NavigationStack?.Count <= 1
    //        || (MobileConstants.IsTablet && IsMoreOptionRootPage(Shell.Current?.CurrentState?.Location.ToString())))
    //        && nodeData.LeftMenuActionID == MenuAction.MenuActionBackKey;
    //}

    ///// <summary>
    ///// Checks if root page is more options page
    ///// </summary>
    ///// <param name="location">location to be checked</param>
    ///// <returns>returns true if more options page is the root page</returns>
    //public bool IsMoreOptionRootPage(string location)
    //{
    //    return /*!AppStyles.IsTabletScaledView &&*/ location.Contains(nameof(MoreOptionsPage))
    //        && location.Split(new[] { nameof(MoreOptionsPage) }, StringSplitOptions.None).Last().Split(Constants.SYMBOL_SLASH).Count() < 3;
    //}

    ///// <summary>
    ///// Show hide footer menus
    ///// </summary>
    ///// <param name="hideFooterMenus"></param>
    //public void HideFooter(bool hideFooterMenus)
    //{
    //    Shell.SetTabBarIsVisible(this, !hideFooterMenus);
    //}

    //#endregion

    //#region Page Navigations

    ///// <summary>
    ///// Fetch menu data from db as per menuKey, update header tab data and redirect on target page.
    ///// </summary>
    ///// <param name="nodeID">Node ID for which header need to apply</param>
    ///// <param name="paramValues">Dynamic parameter list</param>
    //public async Task PushPageByNodeIDAsync(long nodeID, params string[] paramValues)
    //{
    //    await PushPageByNodeIDAsync(nodeID, false, string.Empty, paramValues).ConfigureAwait(true);
    //}

    ///// <summary>
    ///// Fetch menu data from db as per menuKey, update header tab data and redirect on target page.
    ///// </summary>
    ///// <param name="nodeID">Node ID for which header need to apply</param>
    ///// <param name="isFromMoreOptions">true if called from more options</param>
    ///// <param name="paramPlaceholders">Param placeholders for holding param values</param>
    ///// <param name="paramValues">Dynamic parameter list</param>
    //public async Task PushPageByNodeIDAsync(long nodeID, bool isFromMoreOptions, string paramPlaceholders, params string[] paramValues)
    //{
    //    if (!ShellMasterPage.CurrentShell.IsNavigating)
    //    {
    //        ShellMasterPage.CurrentShell.IsNavigating = true;
    //        AppHelper.ShowBusyIndicator = true;
    //        MobileMenuNodeModel mobileMenuNode = new MobileMenuNodeModel { MobileMenuNodeID = nodeID };
    //        mobileMenuNode = await GetPageDataByNodeIDAsync(mobileMenuNode).ConfigureAwait(true);
    //        if (mobileMenuNode != null)
    //        {
    //            if (isFromMoreOptions /*&& !AppStyles.IsTabletScaledView*/ && MobileConstants.IsTablet)
    //            {
    //                if (MobileConstants.IsIosPlatform)
    //                {
    //                    MobileMenuNodeModel moreMenuNode = new MobileMenuNodeModel { MobileMenuNodeID = -1 };
    //                    moreMenuNode = await GetPageDataByNodeIDAsync(moreMenuNode).ConfigureAwait(true);
    //                    if (moreMenuNode == null)
    //                    {
    //                        AppHelper.ShowBusyIndicator = false;
    //                    }
    //                    else
    //                    {
    //                        await PushPageAsync(moreMenuNode, GenericMethods.GenerateParamsWithPlaceholder(Param.isiPadMoreOption), true.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
    //                    }
    //                }
    //                else
    //                {
    //                    mobileMenuNode.TargetPage = $"{Constants.SYMBOL_SLASH}{Constants.SYMBOL_SLASH}{nameof(MoreOptionsPage)}{Constants.SYMBOL_SLASH}{mobileMenuNode.TargetPage}";
    //                }
    //            }
    //            await PushPageAsync(mobileMenuNode, paramPlaceholders, paramValues).ConfigureAwait(true);
    //        }
    //        else
    //        {
    //            AppHelper.ShowBusyIndicator = false;
    //        }
    //        ShellMasterPage.CurrentShell.IsNavigating = false;
    //    }
    //}

    ///// <summary>
    ///// Push page by target
    ///// </summary>
    ///// <param name="targetName">target name</param>
    ///// <param name="paramPlaceholders">Params with placeholder for values</param>
    ///// <param name="paramValues">Param values</param>
    ///// <returns>Sets Page based on the target</returns>
    //public async Task PushPageByTargetAsync(string targetName, string paramPlaceholders, params string[] paramValues)
    //{
    //    await PushPageByTargetAsync(targetName, false, paramPlaceholders, paramValues);
    //}

    ///// <summary>
    ///// Push page by target
    ///// </summary>
    ///// <param name="targetName">target name</param>
    ///// <param name="shouldPopToRoot">true if stack is to be reset to root and the new page is to be pushed</param>
    ///// <param name="paramPlaceholders">Params with placeholder for values</param>
    ///// <param name="paramValues">Param values</param>
    ///// <returns>Sets Page based on the target</returns>
    //public async Task PushPageByTargetAsync(string targetName, bool shouldPopToRoot, string paramPlaceholders, params string[] paramValues)
    //{
    //    if (!ShellMasterPage.CurrentShell.IsNavigating)
    //    {
    //        ShellMasterPage.CurrentShell.IsNavigating = true;
    //        AppHelper.ShowBusyIndicator = true;
    //        MobileMenuNodeModel mobileMenuNode = new MobileMenuNodeModel();
    //        if (targetName == Constants.STATIC_MESSAGE_PAGE_IDENTIFIER)
    //        {
    //            UriBuilder serviceUri = new UriBuilder(new Uri("http://" + targetName + string.Format(paramPlaceholders, paramValues)));
    //            NameValueCollection query = HttpUtility.ParseQueryString(serviceUri.Query);
    //            mobileMenuNode.NodeName = targetName;
    //            mobileMenuNode.TargetID = Convert.ToInt64(query[Param.id.ToString()], CultureInfo.InvariantCulture);
    //        }
    //        else
    //        {
    //            mobileMenuNode.TargetPage = targetName;
    //        }
    //        mobileMenuNode = await GetPageDataByNodeIDAsync(mobileMenuNode).ConfigureAwait(true);
    //        if (mobileMenuNode != null)
    //        {
    //            if (shouldPopToRoot)
    //            {
    //                await ShellMasterPage.Current.Navigation.PopToRootAsync(false).ConfigureAwait(true);
    //            }
    //            await PushPageAsync(mobileMenuNode, paramPlaceholders, paramValues).ConfigureAwait(true);
    //        }
    //        else
    //        {
    //            AppHelper.ShowBusyIndicator = false;
    //        }
    //        ShellMasterPage.CurrentShell.IsNavigating = false;
    //    }
    //}

    ///// <summary>
    ///// Redirect on specific page and set page headers
    ///// </summary>
    ///// <param name="menuData">Redirection menu data</param>
    ///// <param name="paramPlaceholders">Param placeholders for holding param values</param>
    ///// <param name="paramValues">Dynamic parameter list</param>
    //protected async Task PushPageAsync(MobileMenuNodeModel menuData, string paramPlaceholders, params string[] paramValues)
    //{
    //    try
    //    {
    //        //When target is null return with not yet implemented error message.
    //        if (string.IsNullOrWhiteSpace(menuData.TargetPage))
    //        {
    //            AppHelper.ShowBusyIndicator = false;

    //            ////Console.WriteLine($"+++++||PushPageAsync() fata navigate App.NotificationParameter menuData.TargetPage {menuData.TargetPage}===+++++");
    //            await DisplayMessagePopupAsync(ErrorCode.NotImplemented.ToString(), false, false, false).ConfigureAwait(false);
    //            return;
    //        }
    //        //Redirect on new Content page with Updated header data
    //        if (ShellMasterPage.CurrentShell.HasMainPage)
    //        {
    //            AppHelper.ShowBusyIndicator = false;
    //            return;
    //        }
    //        if (ShellMasterPage.CurrentShell.LastNavigationPath.Contains(menuData.TargetPage))
    //        {
    //            AppHelper.ShowBusyIndicator = false;
    //        }

    //        string pageParam = string.Empty;
    //        if (menuData.NodeType == MenuType.StaticPage)
    //        {
    //            pageParam = string.Format(CultureInfo.InvariantCulture, GenericMethods.GenerateParamsWithPlaceholder(Param.id, Param.type), menuData.TargetID, PageType.ContentPage.ToString());
    //        }

    //        if (IsFooterMenu(menuData, pageParam))
    //        {
    //            if (menuData.NodeType == MenuType.StaticPage)
    //            {
    //                paramPlaceholders = string.IsNullOrWhiteSpace(paramPlaceholders) ? pageParam : pageParam + paramPlaceholders.Replace(Constants.SYMBOL_QUESTION_MARK, Constants.SYMBOL_AMPERSAND);
    //            }
    //            await Shell.Current.GoToAsync(string.Format(CultureInfo.InvariantCulture, menuData.TargetPage + paramPlaceholders, paramValues), false).ConfigureAwait(true);
    //        }
    //        else
    //        {
    //            paramPlaceholders = GetPageParams(menuData, paramPlaceholders);
    //            await Shell.Current.GoToAsync(Constants.BASE_ROUTE_PREFIX + string.Format(CultureInfo.InvariantCulture, menuData.TargetPage + paramPlaceholders, paramValues), false).ConfigureAwait(true);
    //        }
    //        ShellMasterPage.CurrentShell.CurrentPage?.SetHeaderItems(menuData);
    //    }
    //    catch (Exception ex)
    //    {
    //        ////Console.WriteLine($"+++++||PushPageAsync() fata navigate App.NotificationParameter {ex}===+++++");
    //        // Navigation failure when route is not found
    //        await DisplayMessagePopupAsync(ErrorCode.NotImplemented.ToString(), false, false, false).ConfigureAwait(false);
    //        new BaseLibService(App._essentials).LogError(ex.Message, ex);
    //    }
    //}

    ///// <summary>
    ///// Pops the current page
    ///// </summary>
    ///// <param name="refreshHeader">true if header neeeds to be refreshed</param>
    ///// <returns>Result of the pop operation</returns>
    //public async Task PopPageAsync(bool refreshHeader)
    //{
    //    if (Shell.Current.Navigation?.NavigationStack?.Count > 1)
    //    {
    //        await Shell.Current.Navigation.PopAsync(false).ConfigureAwait(true);
    //    }
    //    if (refreshHeader && ShellMasterPage.CurrentShell.CurrentPage != null)
    //    {
    //        await ShellMasterPage.CurrentShell.CurrentPage.RefreshHeaderItemsAsync().ConfigureAwait(false);
    //    }
    //}

    ///// <summary>
    ///// method used to pop page from add edit pages
    ///// </summary>
    //public async Task PopPageAsyncForPhoneAsync()
    //{
    //    if (MobileConstants.IsDevicePhone)
    //    {
    //        await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(false);
    //    }
    //}

    //#endregion

    //#region Virtual methods

    ///// <summary>
    ///// Method to override to handle header item clicks
    ///// </summary>
    ///// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
    ///// <param name="menuType">Menu position whether Left, Right</param>
    ///// <param name="menuAction">Action type</param>
    //public virtual async Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    //{
    //    await Task.Run(() => { }).ConfigureAwait(true);
    //}

    ///// <summary>
    ///// Method to override when back or close is clicked
    ///// </summary>
    ///// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
    ///// <param name="menuType">Menu position whether Left, Right</param>
    ///// <param name="menuAction">Action type</param>
    ///// <returns>true if navigation is handled else returns false if default handling is to be used</returns>
    //public virtual Task<bool> OnBackCloseClickAsync(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    //{
    //    return Task.FromResult(false);
    //}

    //#endregion

    //private string GetPageParams(MobileMenuNodeModel menuData, string paramPlaceholders)
    //{
    //    string footerParam = menuData.NodeType == MenuType.StaticPage
    //        ? string.Format(CultureInfo.InvariantCulture, GenericMethods.GenerateParamsWithPlaceholder(Param.footerSettingKey, Param.id, Param.type), menuData.MobileMenuNodeID, menuData.TargetID, PageType.ContentPage.ToString())
    //        : string.Format(CultureInfo.InvariantCulture, GenericMethods.GenerateParamsWithPlaceholder(Param.footerSettingKey), menuData.MobileMenuNodeID);
    //    return string.IsNullOrWhiteSpace(paramPlaceholders) ? footerParam : footerParam + paramPlaceholders.Replace(Constants.SYMBOL_QUESTION_MARK, Constants.SYMBOL_AMPERSAND);
    //}

    //private bool IsFooterMenu(MobileMenuNodeModel menuData, string pageParam)
    //{
    //    return ShellMasterPage.CurrentShell.FooterTabs.Items.FirstOrDefault(x =>
    //        x.Route == menuData.TargetPage && (string.IsNullOrWhiteSpace(pageParam) ||
    //            x.Route.Contains(pageParam.Replace(Constants.SYMBOL_QUESTION_MARK, string.Empty)))
    //    ) == null;
    //}

}