using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class MenuService : BaseService
{
    public MenuService(IEssentials essentials = null) : base(essentials)
    { }

    #region Mobile Menus

    /// <summary>
    /// Get menus data
    /// </summary>
    /// <param name="menus">Reference object to return mobile menus</param>
    /// <returns>Mobile menus in reference object</returns>
    public async Task GetTabbedMenusAsync(MenuDTO menus)
    {
        try
        {
            menus.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            await new MenuDataBase().GetTabbedMenusAsync(menus).ConfigureAwait(false);
            // Add more options only if the items in menu is greater than FOOTER_MENU_MAX_COUNT
            if (menus.IsActive)
            {
                await AddMoreMenuOptionAsync(menus).ConfigureAwait(false);
            }
            menus.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            menus.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Get more menus
    /// </summary>
    /// <param name="moreMenus">Reference object to return more mobile menus</param>
    /// <returns>More options menu in reference object</returns>
    public async Task GetMoreMenusAsync(MenuDTO moreMenus)
    {
        try
        {
            moreMenus.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            await Task.WhenAll(
                GetResourcesAsync(GroupConstants.RS_COMMON_GROUP),
                new MenuDataBase().GetMoreMenuNodesAsync(moreMenus)
            ).ConfigureAwait(false);
            List<Task> badgeTasks = new List<Task>();

            string nextIcon = GetNextIcon();
            foreach (var menu in moreMenus.Menus)
            {
                menu.GroupIcon = GenericMethods.FetchMauiIcon(menu.GroupIcon);
                badgeTasks.Add(SetBadgeCountAsync(menu));
                menu.NavIcon = nextIcon;
            }
            await Task.WhenAll(badgeTasks);

            ////todo: need to rearrange data based on new grouping implementation of List
            //moreMenus.MoreOptionMenus = moreMenus.Menus.GroupBy(x => (x.SequenceNo, x.MenuGroupID, x.GroupHeading));

            var profileMenus = moreMenus.Menus?.Where(x => x.TargetPage.Equals(Pages.ProfilePage.ToString(), StringComparison.InvariantCultureIgnoreCase));
            if (profileMenus?.Count() > 0)
            {
                UserDTO userData = await new UserService(_essentials).GetLoggedInUserProfileAsync().ConfigureAwait(false);
                if (userData.User != null)
                {
                    foreach (MenuModel profileMenu in profileMenus)
                    {
                        profileMenu.Image = string.IsNullOrWhiteSpace(userData.User.ImageName)
                            ? GetInitials($"{userData.User.FirstName} {userData.User.LastName}")
                            : userData.User.ImageName;
                    }
                }
            }
            moreMenus.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            moreMenus.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private async Task AddMoreMenuOptionAsync(MenuDTO menus)
    {
        ResourceModel moreOptionResource = await new ResourceService(_essentials).GetResourceAsync(ResourceConstants.R_MORE_OPTIONS_MENU_KEY).ConfigureAwait(false);
        menus.Menus?.Add(new MenuModel
        {
            TargetID = -1,
            MenuID = 0,
            RenderType = MenuRenderType.Both,
            PageHeading = moreOptionResource?.ResourceValue,
            TargetPage = moreOptionResource?.KeyDescription,
            GroupIcon = moreOptionResource?.InfoValue,
            PageTypeID = MenuType.Group
        });
    }

    /// <summary>
    /// Get node data based on given node id or node name
    /// </summary>
    /// <param name="menuNode">Reference object to return mobile menu node</param>
    /// <returns>Node data based on given node id or node name</returns>
    public async Task GetMenuNodeAsync(MobileMenuNodeDTO menuNode)
    {
        try
        {
            menuNode.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            if (menuNode.MobileMenuNode.MobileMenuNodeID == -1)
            {
                // Load more options data
                ResourceModel moreOptionResource = await new ResourceService(_essentials).GetResourceAsync(ResourceConstants.R_MORE_OPTIONS_MENU_KEY).ConfigureAwait(false);
                menuNode.MobileMenuNode = new MobileMenuNodeModel
                {
                    PageHeading = moreOptionResource.ResourceValue,
                    TargetPage = moreOptionResource.KeyDescription
                };
            }
            else
            {
                await GetResourcesAsync(GroupConstants.RS_MENU_ACTION_GROUP).ConfigureAwait(false);
                using (MenuDataBase menuDb = new MenuDataBase())
                {
                    await menuDb.GetMenuNodeAsync(menuNode).ConfigureAwait(false);
                }
                await SetLeftRightActionsAsync(menuNode).ConfigureAwait(false);
            }
            menuNode.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            menuNode.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private async Task SetBadgeCountAsync(MenuModel menu)
    {
        menu.BadgeCount = await GetBadgeCountAsync(menu.TargetPage).ConfigureAwait(false);
    }

    private async Task SetLeftRightActionsAsync(MobileMenuNodeDTO menuNode)
    {
        List<Task> menuIconTasks = new List<Task> { SetProfileActionAsync(menuNode) };
        SetMenuIcon(menuNode, menuNode.MobileMenuNode.LeftMenuActionID, menuNode.MobileMenuNode.LeftMenuNodeID, true, menuIconTasks);
        SetMenuIcon(menuNode, menuNode.MobileMenuNode.RightMenuActionID, menuNode.MobileMenuNode.RightMenuNodeID, false, menuIconTasks);
        await Task.WhenAll(menuIconTasks).ConfigureAwait(false);
    }

    private void SetMenuIcon(MobileMenuNodeDTO menuNode, MenuAction menuActionID, long? menuNodeID, bool isLeft, List<Task> menuIconTasks)
    {
        if (menuActionID != MenuAction.MenuActionProfileKey)
        {
            if (menuActionID == MenuAction.MenuActionDefaultKey)
            {
                if (menuNodeID > 0)
                {
                    menuIconTasks.Add(new MenuDataBase().GetLeftRightNodeDataAsync(menuNode, isLeft));
                }
            }
            else
            {
                ResourceModel actionResource = LibResources.GetResourceByKey(PageData?.Resources, menuActionID.ToString());
                if (isLeft)
                {
                    menuNode.MobileMenuNode.LeftMenuAction = actionResource.KeyDescription;
                    menuNode.MobileMenuNode.LeftMenuActionText = actionResource.ResourceValue;
                }
                else
                {
                    menuNode.MobileMenuNode.RightMenuAction = actionResource.KeyDescription;
                    menuNode.MobileMenuNode.RightMenuActionText = actionResource.ResourceValue;
                }
            }
        }
    }

    private async Task SetProfileActionAsync(MobileMenuNodeDTO menuNode)
    {
        if (menuNode.MobileMenuNode?.LeftMenuActionID == MenuAction.MenuActionProfileKey
            || menuNode.MobileMenuNode?.RightMenuActionID == MenuAction.MenuActionProfileKey)
        {
            //// Get user profile image
            UserDTO userData = await new UserService(_essentials).GetLoggedInUserProfileAsync().ConfigureAwait(false);
            if (userData.User != null && string.IsNullOrWhiteSpace(menuNode.MobileMenuNode.ImageName))
            {
                menuNode.MobileMenuNode.ImageName = GetInitials($"{userData.User.FirstName} {userData.User.LastName}");
            }
        }
    }

    #endregion

    /// <summary>
    /// Sync Mobile Menu Nodes from service
    /// </summary>
    /// <param name="mobileMenuNode">Mobile menu node reference object to return data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mobile menu nodes retrived from server in mobileMenuNode</returns>
    public async Task SyncMobileMenuNodesFromServerAsync(MobileMenuNodeDTO mobileMenuNode, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_MOBILE_MENU_NODES_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(mobileMenuNode.RecordCount, CultureInfo.InvariantCulture) },
                    { Constants.SE_MOBILE_MENU_NODE_ID_QUERY_KEY, Convert.ToString(mobileMenuNode.MobileMenuNode.MobileMenuNodeID, CultureInfo.InvariantCulture) }
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            mobileMenuNode.ErrCode = httpData.ErrCode;
            if (mobileMenuNode.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(mobileMenuNode, data);
                    MapMobileMenuNodeData(data, mobileMenuNode);
                }
            }
        }
        catch (Exception ex)
        {
            mobileMenuNode.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Saves mobile menu node to Database
    /// </summary>
    /// <param name="mobileMenuNode">Data to be saved</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation Status</returns>
    public async Task SyncMobileMenuNodeToServerAsync(MobileMenuNodeDTO mobileMenuNode, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<MobileMenuNodeDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_MOBILE_MENU_NODE_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = mobileMenuNode
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            mobileMenuNode.ErrCode = httpData.ErrCode;
            mobileMenuNode.Response = httpData.Response;
        }
        catch (Exception ex)
        {
            mobileMenuNode.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync mobile menus from service
    /// </summary>
    /// <param name="menus">mobile menus reference to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>mobile menus reveived from server in mobile menus</returns>
    public async Task SyncMobileMenusFromServerAsync(MenuDTO menus, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_MOBILE_MENUS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_MENU_ID_QUERY_KEY, Convert.ToString(menus.Menu.MenuID, CultureInfo.InvariantCulture) },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(menus.RecordCount, CultureInfo.InvariantCulture) },
                    { Constants.SE_IS_PATEINT_MENU_QUERY_KEY, Convert.ToString(menus.Menu.IsPatientMenu, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            menus.ErrCode = httpData.ErrCode;
            if (menus.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(menus, data);
                    MapMobileAndWebMenus(data, menus, false);
                }
            }
        }
        catch (Exception ex)
        {
            menus.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync web menu data to server
    /// </summary>
    /// <param name="requestData">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status call</returns>
    public async Task SyncWebMenuToServerAsync(MenuDTO requestData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<MenuDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_WEB_MENUS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = requestData,
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            requestData.ErrCode = httpData.ErrCode;
            requestData.Response = httpData.Response;
        }
        catch (Exception ex)
        {
            requestData.ErrCode = ErrorCode.InvalidData;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync web menus from service
    /// </summary>
    /// <param name="menus">web menus reference to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>web menus received from server</returns>
    public async Task SyncWebMenusFromServerAsync(MenuDTO menus, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_WEB_MENUS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(menus.RecordCount, CultureInfo.InvariantCulture) },
                    { Constants.SE_MENU_ID_QUERY_KEY, Convert.ToString(menus.Menu.MenuID, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            menus.ErrCode = httpData.ErrCode;
            if (menus.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(menus, data);
                    MapMobileAndWebMenus(data, menus, true);
                }
            }
        }
        catch (Exception ex)
        {
            menus.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Maps json object into Model and saves data into DB
    /// </summary>
    /// <param name="result">Object which holds Operation status</param>
    /// <param name="data">json object to fetch data from it</param>
    /// <returns>Operation status with record count</returns>
    internal async Task MapAndSaveMenusAsync(DataSyncModel result, JToken syncForResponse)
    {
        MenuDTO menuData = new MenuDTO
        {
            Menus = MapMenus(syncForResponse, nameof(DataSyncDTO.Menus)),
            MenuNodes = MapMenuNodes(syncForResponse, nameof(DataSyncDTO.MenuNodes))
        };
        if (GenericMethods.IsListNotEmpty(menuData.Menus) || GenericMethods.IsListNotEmpty(menuData.MenuNodes))
        {
            await new MenuDataBase().SaveMenusAsync(menuData).ConfigureAwait(false);
            result.RecordCount = menuData.Menus?.Count ?? 0 + menuData.MenuNodes?.Count ?? 0;
        }
        result.ErrCode = ErrorCode.OK;
    }

    /// <summary>
    /// Maps json object into Model and saves data into DB
    /// </summary>
    /// <param name="result">Object which holds Operation status</param>
    /// <param name="data">json object to fetch data from it</param>
    /// <returns>Operation status with record count</returns>
    internal async Task MapAndSaveMenuGroupsAsync(DataSyncModel result, JToken syncForResponse)
    {
        MenuGroupDTO menuGroupData = new MenuGroupDTO
        {
            MenuGroups = MapMobileMenuGroups(syncForResponse, nameof(DataSyncDTO.MenuGroups))
        };
        if (GenericMethods.IsListNotEmpty(menuGroupData.MenuGroups))
        {
            await new MenuDataBase().SaveMenuGroupsAsync(menuGroupData).ConfigureAwait(false);
            result.RecordCount = menuGroupData.MenuGroups.Count;
        }
        result.ErrCode = ErrorCode.OK;
    }

    internal List<MenuModel> MapMenus(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0) ?
            (from dataItem in data[collectionName]
             select new MenuModel
             {
                 MenuID = (long)dataItem[nameof(MenuModel.MenuID)],
                 PageTypeID = (MenuType)(byte)dataItem[nameof(MenuModel.PageTypeID)],
                 TargetID = (long)dataItem[nameof(MenuModel.TargetID)],
                 ScrollToPage = (bool)dataItem[nameof(MenuModel.ScrollToPage)],
                 MenuLocation = (MenuLocation)(byte)dataItem[nameof(MenuModel.MenuLocation)],
                 SequenceNo = (byte)dataItem[nameof(MenuModel.SequenceNo)],
                 IsActive = (bool)dataItem[nameof(MenuModel.IsActive)],
                 MenuType = (string)dataItem[nameof(MenuModel.MenuType)],
                 TargetPage = (string)dataItem[nameof(MenuModel.TargetPage)],
                 NodeName = (string)dataItem[nameof(MenuModel.NodeName)],
                 Content = (string)dataItem[nameof(MenuModel.Content)],
                 GroupIcon = (string)dataItem[nameof(MenuModel.GroupIcon)],
                 GroupType = (ContentType)(byte)dataItem[nameof(MenuModel.GroupType)],
                 MenuGroupID = Convert.ToInt64(dataItem[nameof(MenuModel.MenuGroupID)], CultureInfo.InvariantCulture),
                 PageHeading = (string)dataItem[nameof(MenuModel.PageHeading)],
                 RenderType = (MenuRenderType)(byte)dataItem[nameof(MenuModel.RenderType)],
                 AvailableAtOrganisationLevel = (bool)dataItem[nameof(MenuModel.AvailableAtOrganisationLevel)],
                 AvailableAtBranchLevel = (bool)dataItem[nameof(MenuModel.AvailableAtBranchLevel)],
                 AvailableAtDepartmentLevel = (bool)dataItem[nameof(MenuModel.AvailableAtDepartmentLevel)],
                 IsScrollable = GetIsScrollabel((bool)dataItem[nameof(MenuModel.ScrollToPage)]),
                 IsPatientMenu = !string.IsNullOrEmpty((string)dataItem[nameof(MenuModel.IsPatientMenu)]) && (bool)dataItem[nameof(MenuModel.IsPatientMenu)]
             }).ToList() : null;
    }

    private string GetIsScrollabel(bool Scroll)
    {
        return LibResources.GetResourceValueByKey(PageData?.Resources, Scroll ? ResourceConstants.R_YES_ACTION_KEY : ResourceConstants.R_NO_ACTION_KEY);
    }

    private List<MobileMenuNodeModel> MapMenuNodes(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0) ?
               (from dataItem in data[collectionName]
                select new MobileMenuNodeModel
                {
                    MobileMenuNodeID = (long)dataItem[nameof(MobileMenuNodeModel.MobileMenuNodeID)],
                    MobileMenuGroupID = (long?)dataItem[nameof(MobileMenuNodeModel.MobileMenuGroupID)] ?? 0,
                    SequenceNo = (byte?)dataItem[nameof(MobileMenuNodeModel.SequenceNo)] ?? 0,
                    NodeName = (string)dataItem[nameof(MobileMenuNodeModel.NodeName)],
                    NodeType = (MenuType)(byte)dataItem[nameof(MobileMenuNodeModel.NodeType)],
                    TargetID = (long)dataItem[nameof(MobileMenuNodeModel.TargetID)],
                    LeftMenuActionID = (MenuAction)(byte)dataItem[nameof(MobileMenuNodeModel.LeftMenuActionID)],
                    LeftMenuNodeID = (long?)dataItem[nameof(MobileMenuNodeModel.LeftMenuNodeID)],
                    ShowIconInLeftMenu = (bool?)dataItem[nameof(MobileMenuNodeModel.ShowIconInLeftMenu)],
                    RightMenuActionID = (MenuAction)(byte)dataItem[nameof(MobileMenuNodeModel.RightMenuActionID)],
                    RightMenuNodeID = (long?)dataItem[nameof(MobileMenuNodeModel.RightMenuNodeID)],
                    ShowIconInRightMenu = (bool?)dataItem[nameof(MobileMenuNodeModel.ShowIconInRightMenu)],
                    TargetPage = (string)dataItem[nameof(MobileMenuNodeModel.TargetPage)],
                    IsActive = (bool)dataItem[nameof(MobileMenuNodeModel.IsActive)],
                }).ToList()
                : null;
    }

    /// <summary>
    /// Sync web menu groups from service
    /// </summary>
    /// <param name="menuGroups">web menu groups object reference to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Web menu groups received from server</returns>
    public async Task SyncWebMenuGroupsFromServerAsync(MenuGroupDTO menuGroups, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_WEB_MENU_GROUPS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_MENU_GROUP_ID_QUERY_KEY, Convert.ToString(menuGroups.MenuGroup.MenuGroupID, CultureInfo.InvariantCulture) },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(menuGroups.RecordCount, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            menuGroups.ErrCode = httpData.ErrCode;
            if (menuGroups.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    menuGroups.Response = null;
                    MapCommonData(menuGroups, data);
                    MapWebMenuGroup(data, menuGroups);
                }
            }
        }
        catch (Exception ex)
        {
            menuGroups.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync web menu groups to server
    /// </summary>
    /// <param name="requestData">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status call</returns>
    public async Task SyncWebMenuGroupsToServerAsync(MenuGroupDTO requestData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<MenuGroupDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_WEB_MENU_GROUPS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = requestData,
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            requestData.ErrCode = httpData.ErrCode;
            requestData.Response = httpData.Response;
        }
        catch (Exception ex)
        {
            requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void MapMobileMenuNodeData(JToken data, MobileMenuNodeDTO mobileMenuNode)
    {
        SetPageResources(mobileMenuNode.Resources);
        mobileMenuNode.MobileMenuNodes = (data[nameof(mobileMenuNode.MobileMenuNodes)].Any()) ?
            (from dataItem in data[nameof(mobileMenuNode.MobileMenuNodes)]
             select new MobileMenuNodeModel
             {
                 MobileMenuNodeID = (long)dataItem[nameof(MobileMenuNodeModel.MobileMenuNodeID)],
                 NodeName = (string)dataItem[nameof(MobileMenuNodeModel.NodeName)],
                 NodeType = (MenuType)(int)dataItem[nameof(MobileMenuNodeModel.NodeType)],
                 TargetID = (long)dataItem[nameof(MobileMenuNodeModel.TargetID)],
                 LeftMenuActionID = (MenuAction)(int)dataItem[nameof(MobileMenuNodeModel.LeftMenuActionID)],
                 LeftMenuNodeID = (long?)dataItem[nameof(MobileMenuNodeModel.LeftMenuNodeID)],
                 ShowIconInLeftMenu = (bool?)dataItem[nameof(MobileMenuNodeModel.ShowIconInLeftMenu)],
                 RightMenuActionID = (MenuAction)(int)dataItem[nameof(MobileMenuNodeModel.RightMenuActionID)],
                 RightMenuNodeID = (long?)dataItem[nameof(MobileMenuNodeModel.RightMenuNodeID)],
                 ShowIconInRightMenu = (bool?)dataItem[nameof(MobileMenuNodeModel.ShowIconInRightMenu)],
                 IsActive = (bool)dataItem[nameof(MobileMenuNodeModel.IsActive)],
                 MenuType = (string)dataItem[nameof(MobileMenuNodeModel.MenuType)],
                 TargetPage = ((string)dataItem[nameof(MobileMenuNodeModel.TargetPage)])?.Replace(Constants.STRING_FROMAT, string.Empty),
                 LeftMenuAction = (string)dataItem[nameof(MobileMenuNodeModel.LeftMenuAction)],
                 RightMenuAction = (string)dataItem[nameof(MobileMenuNodeModel.RightMenuAction)],
             }).ToList() : null;

        if (mobileMenuNode.RecordCount == -1 && mobileMenuNode.MobileMenuNodes != null)
        {
            mobileMenuNode.MobileMenuNode = mobileMenuNode.MobileMenuNodes.FirstOrDefault();
            mobileMenuNode.MobileMenuNodes.Clear();
        }

        mobileMenuNode.MenuActions = (data[nameof(mobileMenuNode.MenuActions)].Any()) ?
            GetPickerSource(data, nameof(mobileMenuNode.MenuActions), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), 0, false, null)
            : new List<OptionModel>();
        mobileMenuNode.ExistingMobileMenuNodes = (data[nameof(mobileMenuNode.ExistingMobileMenuNodes)].Any()) ?
            GetPickerSource(data, nameof(mobileMenuNode.ExistingMobileMenuNodes), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), 0, false, null)
             : new List<OptionModel>();
        mobileMenuNode.MenuFeatures = (data[nameof(mobileMenuNode.MenuFeatures)].Any()) ?
            GetPickerSource(data, nameof(mobileMenuNode.MenuFeatures), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), mobileMenuNode.MobileMenuNode?.TargetID ?? 0, false, nameof(OptionModel.ParentOptionID))
             : new List<OptionModel>();
        mobileMenuNode.ErrCode = (ErrorCode)(int)data[nameof(BaseDTO.ErrCode)];
    }

    private void MapWebMenuGroup(JToken data, MenuGroupDTO menuGroups)
    {
        SetPageResources(menuGroups.Resources);
        menuGroups.MenuGroups = (data[nameof(MenuGroupDTO.MenuGroups)]?.Count() > 0) ?
                        (from dataItem in data[nameof(MenuGroupDTO.MenuGroups)]
                         select new MenuGroupModel
                         {
                             MenuGroupID = (long)dataItem[nameof(MenuGroupModel.MenuGroupID)],
                             GroupIdentifier = (string)dataItem[nameof(MenuGroupModel.GroupIdentifier)],
                             PageType = dataItem[nameof(MenuGroupModel.PageType)].ToString().ToEnum<ContentType>(),
                             PageHeading = (string)dataItem[nameof(MenuGroupModel.PageHeading)],
                             PageTypeName = GetPageType(dataItem[nameof(MenuGroupModel.PageType)].ToString().ToEnum<ContentType>()),
                         }).ToList() : null;

        menuGroups.MenuGroupDetails = (data[nameof(MenuGroupDTO.MenuGroupDetails)]?.Count() > 0) ?
                        (from dataItem in data[nameof(MenuGroupDTO.MenuGroupDetails)]
                         select new ContentDetailModel
                         {
                             PageID = (long)dataItem[nameof(ContentDetailModel.PageID)],
                             PageHeading = (string)dataItem[nameof(ContentDetailModel.PageHeading)],
                             PageData = (string)dataItem[nameof(ContentDetailModel.PageData)],
                             LanguageID = (byte)dataItem[nameof(ContentDetailModel.LanguageID)],
                             LanguageName = (string)dataItem[nameof(ContentDetailModel.LanguageName)],
                         }).ToList() : null;

        //menuGroups.Languages = (data[nameof(MenuGroupDTO.MenuGroupDetails)]?.Count() > 0) ?
        //                (from dataItem in data[nameof(MenuGroupDTO.MenuGroupDetails)]
        //                 select new LanguageModel
        //                 {
        //                     LanguageID = (byte)dataItem[nameof(LanguageModel.LanguageID)],
        //                     LanguageName = (string)dataItem[nameof(LanguageModel.LanguageName)],
        //                 }).ToList() : null;

        menuGroups.PageTypes = (from option in menuGroups.Resources
                                where option.GroupName == GroupConstants.RS_CONTENT_TYPE_GROUP
                                select new OptionModel
                                {
                                    OptionID = (long)GetOptionID(option.ResourceKey),
                                    OptionText = option.ResourceValue,
                                    IsSelected = menuGroups.MenuGroups == null ? option.ResourceKey == ResourceConstants.R_CONTENT_KEY : menuGroups.MenuGroups.FirstOrDefault().PageType == GetOptionID(option.ResourceKey),
                                }).ToList();

        MapWebMenuLinks(data, menuGroups);
        if (menuGroups.RecordCount == -1 && menuGroups.MenuGroups != null)
        {
            menuGroups.MenuGroup = menuGroups.MenuGroups.FirstOrDefault();
            menuGroups.MenuGroups.Clear();
        }
        menuGroups.ErrCode = (ErrorCode)(int)data[nameof(BaseDTO.ErrCode)];
    }

    private void MapWebMenuLinks(JToken data, MenuGroupDTO menuGroups)
    {
        menuGroups.MenuNodes = data[nameof(MenuGroupDTO.MenuGroupLinks)]?.Count() > 0 ?
                               (from dataItem in data[nameof(MenuGroupDTO.MenuGroupLinks)]
                                select new OptionModel
                                {
                                    OptionID = (long)dataItem[nameof(MenuGroupLinkModel.TargetID)],
                                    OptionText = GetHeading(dataItem),
                                    ParentOptionID = (byte)dataItem[nameof(MenuGroupLinkModel.PageTypeID)],
                                    SequenceNo = (long)dataItem[nameof(MenuGroupLinkModel.SequenceNo)],
                                    IsSelected = (byte)dataItem[nameof(MenuGroupLinkModel.SequenceNo)] > 0
                                }).ToList() : new List<OptionModel>();
    }

    private string GetHeading(JToken dataItem)
    {
        return $"{LibResources.GetResourceValueByKey(PageData?.Resources, dataItem[nameof(MenuGroupLinkModel.PageTypeID)].ToString().ToEnum<MenuType>() == MenuType.Feature ? ResourceConstants.R_FEATURE_TYPE_KEY : ResourceConstants.R_PAGE_KEY)}" +
            $" {Constants.DASH_INDICATOR} {(string)dataItem[nameof(MenuGroupLinkModel.Heading)]}";
    }

    private string GetPageType(ContentType contentType)
    {
        switch (contentType)
        {
            case ContentType.Content:
                return LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_CONTENT_KEY);
            case ContentType.Link:
                return LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_LINKS_KEY);
            case ContentType.Both:
                return LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_BOTH_KEY);
            default:
                return string.Empty;
        }
    }

    private ContentType GetOptionID(string optionKey)
    {
        if (optionKey == ResourceConstants.R_CONTENT_KEY)
        {
            return ContentType.Content;
        }
        else if (optionKey == ResourceConstants.R_LINKS_KEY)
        {
            return ContentType.Link;
        }
        else if (optionKey == ResourceConstants.R_BOTH_KEY)
        {
            return ContentType.Both;
        }
        else
        {
            return 0;
        }
    }

    private void MapMobileAndWebMenus(JToken data, MenuDTO menus, bool isWebMenu)
    {
        SetPageResources(menus.Resources);
        menus.Menus = MapMenus(data, nameof(DataSyncDTO.Menus));
        if (menus.RecordCount == -1 && menus.Menus != null)
        {
            menus.Menu = menus.Menus.FirstOrDefault();
            menus.Menus.Clear();
        }
        MapMenuNodesGroups(data, menus, menus.Menu ?? new MenuModel());
        List<ResourceModel> options = isWebMenu
            ? menus.Resources.Where(x => x.ResourceKey == ResourceConstants.R_HEADER_KEY || x.ResourceKey == ResourceConstants.R_FOOTER_KEY).ToList()
            : menus.Resources.Where(x => x.GroupName == GroupConstants.RS_RENDER_TYPE_GROUP).ToList();
        menus.MenuLocations = (from dataItem in options
                               select new OptionModel
                               {
                                   OptionID = dataItem.ResourceKeyID,
                                   OptionText = dataItem.ResourceValue,
                                   IsSelected = IsRadioButtonSelected(dataItem, isWebMenu ? (int)menus.Menu.MenuLocation : (int)menus.Menu.RenderType, isWebMenu),
                                   GroupName = dataItem.ResourceKey
                               }).ToList();
        menus.ErrCode = (ErrorCode)(int)data[nameof(MenuDTO.ErrCode)];
    }

    private void MapMenuNodesGroups(JToken data, MenuDTO menus, MenuModel menu)
    {
        menus.MenuNodesGroups = (data[nameof(menus.MenuNodesGroups)]?.Count() > 0) ?
         (from dataItem in data[nameof(menus.MenuNodesGroups)]
          select new OptionModel
          {
              OptionID = (long)dataItem[nameof(OptionModel.OptionID)],
              OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
              ParentOptionID = (long)dataItem[nameof(OptionModel.ParentOptionID)],
              IsSelected = (long)dataItem[nameof(OptionModel.OptionID)] == menu.TargetID && (long)dataItem[nameof(OptionModel.ParentOptionID)] == (long)menu.PageTypeID
          }).ToList() : new List<OptionModel>();
    }

    private bool IsRadioButtonSelected(ResourceModel resource, int id, bool isWebMenu)
    {
        if (isWebMenu)
        {
            return id == 2 ? resource.ResourceKey == ResourceConstants.R_FOOTER_KEY : resource.ResourceKey == ResourceConstants.R_HEADER_KEY;
        }
        else if (id == 2)
        {
            return resource.ResourceKey == ResourceConstants.R_SHOW_LABEL_KEY;
        }
        else if (id == 3)
        {
            return resource.ResourceKey == ResourceConstants.R_SHOW_BOTH_KEY;
        }
        else
        {
            return resource.ResourceKey == ResourceConstants.R_SHOW_ICON_KEY;
        }
    }

    /// <summary>
    /// Sync sample data to server
    /// </summary>
    /// <param name="requestData">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status call</returns>
    public async Task SyncMobileMenuToServerAsync(MenuDTO requestData, CancellationToken cancellationToken)
    {
        try
        {
            if (requestData.Menu != null)
            {
                var httpData = new HttpServiceModel<MenuDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_MOBILE_MENU_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    },
                    ContentToSend = requestData,
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                requestData.ErrCode = httpData.ErrCode;
                requestData.Response = httpData.Response;
            }
        }
        catch (Exception ex)
        {
            requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }


}