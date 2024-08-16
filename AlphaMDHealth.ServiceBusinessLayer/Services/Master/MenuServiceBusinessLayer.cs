using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class MenuServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Menu service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public MenuServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get mobile menu groups from database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="menuGroupID">Id of mobile menu group</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of Mobile menu groups</returns>
        public async Task<MenuGroupDTO> GetMobileMenuGroupsAsync(byte languageID, long permissionAtLevelID, long menuGroupID, long recordCount)
        {
            MenuGroupDTO mobileMenuGroupsData = new MenuGroupDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    mobileMenuGroupsData.ErrCode = ErrorCode.InvalidData;
                    return mobileMenuGroupsData;
                }
                mobileMenuGroupsData.AccountID = AccountID;
                if (mobileMenuGroupsData.AccountID < 1)
                {
                    mobileMenuGroupsData.ErrCode = ErrorCode.Unauthorized;
                    return mobileMenuGroupsData;
                }
                mobileMenuGroupsData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                mobileMenuGroupsData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP},{GroupConstants.RS_CONTENT_TYPE_GROUP}",
                    languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(mobileMenuGroupsData.Resources))
                {
                    mobileMenuGroupsData.PermissionAtLevelID = permissionAtLevelID;
                    mobileMenuGroupsData.LanguageID = languageID;
                    mobileMenuGroupsData.RecordCount = recordCount;
                    mobileMenuGroupsData.MenuGroup = new MenuGroupModel
                    {
                        MenuGroupID = menuGroupID
                    };
                    mobileMenuGroupsData.FeatureFor = FeatureFor;
                    await new MenuServiceDataLayer().GetMobileMenuGroupsAsync(mobileMenuGroupsData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                mobileMenuGroupsData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return mobileMenuGroupsData;
        }

        /// <summary>
        /// Save web menu group to database
        /// </summary>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="menuGroup">Web menu group data to be saved</param>
        /// <returns>Result of operation</returns>
        public async Task<BaseDTO> SaveWebMenuGroupAsync(long permissionAtLevelID, long organisationID, MenuGroupDTO menuGroup)
        {
            try
            {
                if (IsValid(menuGroup, permissionAtLevelID))
                {
                    if (menuGroup.IsActive)
                    {
                        menuGroup.LanguageID = 1;
                        if (await GetSettingsResourcesAsync(menuGroup, false, string.Empty, $"{GroupConstants.RS_MENU_PAGE_GROUP}").ConfigureAwait(false))
                        {
                            if (!await ValidateDataAsync(menuGroup.MenuGroupDetails, menuGroup.Resources))
                            {
                                menuGroup.ErrCode = ErrorCode.InvalidData;
                                return menuGroup;
                            }
                        }
                        else
                        {
                            return menuGroup;
                        }
                    }
                    menuGroup.AccountID = AccountID;
                    menuGroup.PermissionAtLevelID = permissionAtLevelID;
                    menuGroup.OrganisationID = organisationID;
                    menuGroup.FeatureFor = FeatureFor;
                    if (menuGroup.MenuGroup.MenuGroupID == 0)
                    {
                        await new MenuServiceDataLayer().SaveWebMenuGroupAsync(menuGroup).ConfigureAwait(false);
                    }
                    if (menuGroup.ErrCode == ErrorCode.OK)
                    {
                        await UploadImagesAsync(menuGroup).ConfigureAwait(false);
                        if (menuGroup.ErrCode == ErrorCode.OK)
                        {
                            await new MenuServiceDataLayer().SaveWebMenuGroupAsync(menuGroup).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                menuGroup.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return menuGroup;
        }


        private bool IsValid(MenuGroupDTO menuGroup, long permissionAtLevelID)
        {
            if (permissionAtLevelID < 1 || AccountID < 1 || string.IsNullOrWhiteSpace(menuGroup.MenuGroup.GroupIdentifier) || menuGroup.MenuGroupDetails == null ||
                    (menuGroup.MenuGroup.PageType != ContentType.Content && menuGroup.IsActive && (menuGroup.MenuGroupLinks == null || menuGroup.MenuGroupLinks.Count == 0)))
            {
                menuGroup.ErrCode = ErrorCode.InvalidData;
                return false;
            }
            return true;
        }

        private async Task ReplaceMenuGroupImageCdnLinkAsync(MenuGroupDTO menuGroup)
        {
            if (GenericMethods.IsListNotEmpty(menuGroup.MenuGroupDetails))
            {
                BaseDTO cdnCacheData = new BaseDTO();
                foreach (var detail in menuGroup.MenuGroupDetails)
                {
                    if (!string.IsNullOrWhiteSpace(detail.PageData))
                    {
                        detail.PageData = await ReplaceCDNLinkAsync(detail.PageData, cdnCacheData);
                    }
                }
            }
        }

        private async Task UploadImagesAsync(MenuGroupDTO menuGroup)
        {
            FileUploadDTO files = CreateFileDataObject(FileTypeToUpload.WebMenuGroupImages, menuGroup.MenuGroup.MenuGroupID.ToString(CultureInfo.InvariantCulture));
            files.FileContainers[0].FileData = (from menuDetail in menuGroup.MenuGroupDetails
                                                select CreateFileObject($"{menuDetail.LanguageID}_{menuGroup.MenuGroup.MenuGroupID}", menuDetail.PageData, true)
                                               ).ToList();
            files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
            menuGroup.ErrCode = files.ErrCode;
            if (menuGroup.ErrCode == ErrorCode.OK)
            {
                foreach (var detail in menuGroup.MenuGroupDetails)
                {
                    detail.PageData = GetBase64FileFromFirstContainer(files, $"{detail.LanguageID}_{menuGroup.MenuGroup.MenuGroupID}");
                }
            }
        }

        /// <summary>
        /// Gets list of mobile menu nodes
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="recordCount">Record Count</param>
        /// <param name="mobileMenuNodeID">ID of Mobile menu Node</param>
        /// <returns>Mobile menu nodes</returns>
        public async Task<MobileMenuNodeDTO> GetMobileMenuNodesAsync(byte languageID, long permissionAtLevelID, long recordCount, long mobileMenuNodeID)
        {
            MobileMenuNodeDTO mobileMenuNode = new MobileMenuNodeDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    mobileMenuNode.ErrCode = ErrorCode.InvalidData;
                    return mobileMenuNode;
                }
                mobileMenuNode.AccountID = AccountID;
                if (mobileMenuNode.AccountID < 1)
                {
                    mobileMenuNode.ErrCode = ErrorCode.Unauthorized;
                    return mobileMenuNode;
                }
                mobileMenuNode.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                mobileMenuNode.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP},{GroupConstants.RS_PAGE_TYPE_GROUP},{GroupConstants.RS_RENDER_TYPE_GROUP},{GroupConstants.RS_MENU_ACTION_GROUP}", languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(mobileMenuNode.Resources))
                {
                    mobileMenuNode.LanguageID = languageID;
                    mobileMenuNode.PermissionAtLevelID = permissionAtLevelID;
                    mobileMenuNode.RecordCount = recordCount;
                    mobileMenuNode.MobileMenuNode = new MobileMenuNodeModel { MobileMenuNodeID = mobileMenuNodeID };
                    mobileMenuNode.FeatureFor = FeatureFor;
                    await new MenuServiceDataLayer().GetMobileMenuNodesAsync(mobileMenuNode).ConfigureAwait(false);
                    foreach (var item in mobileMenuNode.MobileMenuNodes)
                    {
                        item.LeftMenuAction = mobileMenuNode.Resources.Find(x => x.ResourceKey == item.LeftMenuActionID.ToString())?.ResourceValue;
                        item.RightMenuAction = mobileMenuNode.Resources.Find(x => x.ResourceKey == item.RightMenuActionID.ToString())?.ResourceValue;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                mobileMenuNode.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return mobileMenuNode;
        }

        /// <summary>
        /// Saves Mobile menu node
        /// </summary>
        /// <param name="mobileMenuNode">Data to be saved</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SaveMobileMenuNodeAsync(MobileMenuNodeDTO mobileMenuNode, long permissionAtLevelID)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1 || string.IsNullOrWhiteSpace(mobileMenuNode.MobileMenuNode.NodeName) || mobileMenuNode.MobileMenuNode.TargetID == 0 || mobileMenuNode.MobileMenuNode.NodeType == 0)
                {
                    mobileMenuNode.ErrCode = ErrorCode.InvalidData;
                    return mobileMenuNode;
                }
                if (mobileMenuNode.MobileMenuNode.IsActive)
                {
                    mobileMenuNode.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(mobileMenuNode, false, string.Empty, $"{GroupConstants.RS_MENU_PAGE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(mobileMenuNode.MobileMenuNode, mobileMenuNode.Resources))
                        {
                            mobileMenuNode.ErrCode = ErrorCode.InvalidData;
                            return mobileMenuNode;
                        }
                    }
                    else
                    {
                        return mobileMenuNode;
                    }
                }
                mobileMenuNode.AccountID = AccountID;
                mobileMenuNode.PermissionAtLevelID = permissionAtLevelID;
                mobileMenuNode.FeatureFor = FeatureFor;
                await new MenuServiceDataLayer().SaveMobileMenuNodeAsync(mobileMenuNode).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                mobileMenuNode.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return mobileMenuNode;
        }

        /// <summary>
        /// Save mobile menu group to database
        /// </summary>
        /// <param name="menuGroupData">Mobile menu group data to be saved</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <returns>Result of operation</returns>
        public async Task<BaseDTO> SaveMobileMenuGroupAsync(MenuGroupDTO menuGroupData, long permissionAtLevelID)
        {
            try
            {
                if (permissionAtLevelID < 1 || menuGroupData == null || menuGroupData.MenuGroup == null || AccountID < 1)
                {
                    menuGroupData.ErrCode = ErrorCode.InvalidData;
                    return menuGroupData;
                }
                if (menuGroupData.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(menuGroupData.MenuGroupDetails))
                    {
                        menuGroupData.ErrCode = ErrorCode.InvalidData;
                        return menuGroupData;
                    }
                    menuGroupData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(menuGroupData, false, string.Empty, $"{GroupConstants.RS_MENU_PAGE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(menuGroupData.MenuGroup, menuGroupData.Resources))
                        {
                            menuGroupData.ErrCode = ErrorCode.InvalidData;
                            return menuGroupData;
                        }
                        else if (!await ValidateDataAsync(menuGroupData.MenuGroupDetails, menuGroupData.Resources))
                        {
                            menuGroupData.ErrCode = ErrorCode.InvalidData;
                            return menuGroupData;
                        }
                    }
                    else
                    {
                        return menuGroupData;
                    }
                }
                menuGroupData.AccountID = AccountID;
                menuGroupData.PermissionAtLevelID = permissionAtLevelID;
                menuGroupData.FeatureFor = FeatureFor;
                await new MenuServiceDataLayer().SaveMobileMenuGroupAsync(menuGroupData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                menuGroupData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return menuGroupData;
        }

        /// <summary>
        /// Get web menu groups
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="menuGroupID">web menu group id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of web menu groups</returns>
        public async Task<BaseDTO> GetWebMenuGroupsAsync(byte languageID, long permissionAtLevelID, long menuGroupID, long recordCount)
        {
            MenuGroupDTO webMenuData = new MenuGroupDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    webMenuData.ErrCode = ErrorCode.InvalidData;
                    return webMenuData;
                }
                webMenuData.AccountID = AccountID;
                if (webMenuData.AccountID < 1)
                {
                    webMenuData.ErrCode = ErrorCode.Unauthorized;
                    return webMenuData;
                }
                webMenuData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                webMenuData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP},{GroupConstants.RS_CONTENT_TYPE_GROUP},{GroupConstants.RS_PAGE_TYPE_GROUP}"
                        , languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (webMenuData.Resources != null)
                {
                    webMenuData.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
                    if (webMenuData.Settings != null)
                    {
                        webMenuData.PermissionAtLevelID = permissionAtLevelID;
                        webMenuData.LanguageID = languageID;
                        webMenuData.RecordCount = recordCount;
                        webMenuData.MenuGroup = new MenuGroupModel
                        {
                            MenuGroupID = menuGroupID,
                        };
                        webMenuData.FeatureFor = FeatureFor;
                        await new MenuServiceDataLayer().GetWebMenuGroupsAsync(webMenuData).ConfigureAwait(false);
                        if (webMenuData.ErrCode == ErrorCode.OK)
                        {
                            await ReplaceMenuGroupImageCdnLinkAsync(webMenuData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                webMenuData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return webMenuData;
        }

        /// <summary>
        /// Get mobile menus
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="menuID">mobile menu id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of Mobile menus</returns>
        public async Task<BaseDTO> GetMobileMenusAsync(byte languageID, long permissionAtLevelID, long menuID, long recordCount, bool isPatientMenu)
        {
            MenuDTO menuData = new MenuDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    menuData.ErrCode = ErrorCode.InvalidData;
                    return menuData;
                }
                menuData.AccountID = AccountID;
                if (menuData.AccountID < 1)
                {
                    menuData.ErrCode = ErrorCode.Unauthorized;
                    return menuData;
                }
                menuData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                menuData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP},{GroupConstants.RS_RENDER_TYPE_GROUP}", languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(menuData.Resources))
                {
                    menuData.PermissionAtLevelID = permissionAtLevelID;
                    menuData.LanguageID = languageID;
                    menuData.RecordCount = recordCount;
                    menuData.Menu = new MenuModel
                    {
                        MenuID = menuID,
                        IsPatientMenu = isPatientMenu
                    };
                    menuData.FeatureFor = FeatureFor;
                    await new MenuServiceDataLayer().GetMobileMenusAsync(menuData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                menuData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return menuData;
        }

        /// <summary>
        /// Save mobile menus data to database
        /// </summary>
        /// <param name="mobileMenuData">Mobile menus data to be saved</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <returns>Result of operation</returns>
        public async Task<BaseDTO> SaveMobileMenusAsync(MenuDTO mobileMenuData, long permissionAtLevelID)
        {
            try
            {
                if (permissionAtLevelID < 1 || Convert.ToInt32(mobileMenuData.Menu.PageTypeID) < 1 || AccountID < 1)
                {
                    mobileMenuData.ErrCode = ErrorCode.InvalidData;
                    return mobileMenuData;
                }
                if (mobileMenuData.Menu.IsActive)
                {
                    mobileMenuData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(mobileMenuData, false, string.Empty, $"{GroupConstants.RS_MENU_PAGE_GROUP},{GroupConstants.RS_COMMON_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(mobileMenuData.Menu, mobileMenuData.Resources))
                        {
                            mobileMenuData.ErrCode = ErrorCode.InvalidData;
                            return mobileMenuData;
                        }
                    }
                    else
                    {
                        return mobileMenuData;
                    }
                }
                mobileMenuData.AccountID = AccountID;
                mobileMenuData.PermissionAtLevelID = permissionAtLevelID;
                mobileMenuData.FeatureFor = FeatureFor;
                await new MenuServiceDataLayer().SaveMobileMenuAsync(mobileMenuData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                mobileMenuData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return mobileMenuData;
        }

        /// <summary>
        /// Get web menus
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="menuID">web menu group id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of web menus</returns>
        public async Task<BaseDTO> GetWebMenusAsync(byte languageID, long permissionAtLevelID, long menuID, long recordCount)
        {
            MenuDTO menuData = new MenuDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    menuData.ErrCode = ErrorCode.InvalidData;
                    return menuData;
                }
                menuData.AccountID = AccountID;
                if (menuData.AccountID < 1)
                {
                    menuData.ErrCode = ErrorCode.Unauthorized;
                    return menuData;
                }
                menuData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP},{GroupConstants.RS_RENDER_TYPE_GROUP},{GroupConstants.RS_YES_NO_TYPE_GROUP}", languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(menuData.Resources))
                {
                    menuData.PermissionAtLevelID = permissionAtLevelID;
                    menuData.LanguageID = languageID;
                    menuData.RecordCount = recordCount;
                    menuData.Menu = new MenuModel
                    {
                        MenuID = menuID
                    };
                    menuData.FeatureFor = FeatureFor;
                    await new MenuServiceDataLayer().GetWebMenusAsync(menuData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                menuData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return menuData;
        }

        /// <summary>
        /// Save web menus data
        /// </summary>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="webMenuData">Web menus data to be saved</param>
        /// <returns>Result of operation</returns>
        public async Task<BaseDTO> SaveWebMenuAsync(long permissionAtLevelID, MenuDTO webMenuData)
        {
            try
            {
                if (permissionAtLevelID < 1 || webMenuData == null || AccountID < 1)
                {
                    webMenuData.ErrCode = ErrorCode.InvalidData;
                    return webMenuData;
                }
                if (webMenuData.Menu.IsActive)
                {
                    webMenuData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(webMenuData, false, string.Empty, $"{GroupConstants.RS_MENU_PAGE_GROUP},{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_RENDER_TYPE_GROUP},{GroupConstants.RS_YES_NO_TYPE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(webMenuData.Menus, webMenuData.Resources))
                        {
                            webMenuData.ErrCode = ErrorCode.InvalidData;
                            return webMenuData;
                        }
                    }
                    else
                    {
                        return webMenuData;
                    }
                }
                webMenuData.AccountID = AccountID;
                webMenuData.PermissionAtLevelID = permissionAtLevelID;
                webMenuData.FeatureFor = FeatureFor;
                await new MenuServiceDataLayer().SaveWebMenuAsync(webMenuData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                webMenuData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return webMenuData;
        }
    }
}