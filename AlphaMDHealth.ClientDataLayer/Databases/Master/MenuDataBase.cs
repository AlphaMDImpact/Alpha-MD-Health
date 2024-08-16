using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientDataLayer
{
    public class MenuDataBase : BaseDatabase
    {
        /// <summary>
        /// Save Mobile menus to database
        /// </summary>
        /// <param name="menuData">mobile menus to be saved</param>
        /// <returns>Operation result</returns>
        public async Task SaveMenusAsync(MenuDTO menuData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(menuData.Menus))
                {
                    foreach (MenuModel menu in menuData.Menus)
                    {
                        transaction.InsertOrReplace(menu);
                    }
                }
                if (GenericMethods.IsListNotEmpty(menuData.MenuNodes))
                {
                    foreach (MobileMenuNodeModel node in menuData.MenuNodes)
                    {
                        if (transaction.FindWithQuery<MobileMenuNodeModel>("SELECT 1 FROM MobileMenuNodeModel WHERE MobileMenuNodeID = ? AND MobileMenuGroupID = ?", node.MobileMenuNodeID, node.MobileMenuGroupID) == null)
                        {
                            if (node.IsActive)
                            {
                                transaction.Insert(node);
                            }
                        }
                        else
                        {
                            transaction.Execute("UPDATE MobileMenuNodeModel SET NodeName = ?, NodeType = ?, TargetID = ?, LeftMenuActionID = ?, LeftMenuNodeID = ?, ShowIconInLeftMenu = ?, RightMenuActionID = ?, " +
                                "RightMenuNodeID = ?, ShowIconInRightMenu = ?, IsActive = ?, MenuType = ?, TargetPage = ?, PageHeading = ?, SequenceNo = ? " +
                                "WHERE MobileMenuNodeID = ? AND MobileMenuGroupID = ?",
                                node.NodeName, node.NodeType, node.TargetID, node.LeftMenuActionID, node.LeftMenuNodeID, node.ShowIconInLeftMenu, node.RightMenuActionID, node.RightMenuNodeID, node.ShowIconInRightMenu,
                                node.IsActive, node.MenuType, node.TargetPage, node.PageHeading, node.SequenceNo, node.MobileMenuNodeID, node.MobileMenuGroupID);
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Save mobile group menus to database
        /// </summary>
        /// <param name="menuGroupData">mobile menus to be saved</param>
        /// <returns>Operation result</returns>
        public async Task SaveMenuGroupsAsync(MenuGroupDTO menuGroupData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (MenuGroupModel menu in menuGroupData.MenuGroups)
                {
                    transaction.InsertOrReplace(menu);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get Mobile menus from database
        /// </summary>
        /// <param name="menus">Reference object to return mobile menus</param>
        /// <returns>Mobile menus in reference object</returns>
        public async Task GetTabbedMenusAsync(MenuDTO menus)
        {
            await SqlConnection.RunInTransactionAsync((transaction) =>
            {
                menus.IsActive = transaction.Table<MenuModel>().Count(x => x.IsActive && x.SequenceNo > 0) > MobileConstants.FOOTER_MENU_MAX_COUNT;
                menus.Menus = transaction.Query<MenuModel>(
                    "SELECT DISTINCT A.*, B.NodeType as PageTypeID, B.GroupIcon, B.MobileMenuNodeID AS TargetID, B.TargetID AS Content, B.TargetPage, " +
                    "CASE WHEN B.NodeType = ? THEN C.FeatureText ELSE E.PageHeading END AS PageHeading, " +
                    "CASE WHEN B.NodeType = ? THEN C.GroupIcon ELSE D.ImageName END AS GroupIcon " +
                    "FROM MenuModel A INNER JOIN MobileMenuNodeModel B ON A.TargetID = B.MobileMenuNodeID AND A.PageTypeID = ? " +
                    "LEFT JOIN OrganizationFeaturePermissionModel C ON B.TargetID = C.FeatureID AND B.NodeType = ? AND C.LanguageID = ? " +
                    "LEFT JOIN ContentPageModel D ON B.TargetID = D.PageID AND B.NodeType = ? " +
                    "LEFT JOIN ContentDetailModel E ON E.PageID = D.PageID AND B.NodeType = ? AND E.LanguageID = ? " +
                    "WHERE B.MobileMenuGroupID = 0 AND A.IsActive = 1 AND B.IsActive = 1 AND ((B.NodeType = ? AND C.IsActive = 1) OR (B.NodeType = ? AND D.IsActive = 1 AND E.IsActive = 1))" +
                    "ORDER BY A.SequenceNo LIMIT ?", MenuType.Feature, MenuType.Feature, MenuType.Node, MenuType.Feature, menus.LanguageID, MenuType.StaticPage, MenuType.StaticPage,
                    menus.LanguageID, MenuType.Feature, MenuType.StaticPage, MobileConstants.FOOTER_MENU_MAX_COUNT - (menus.IsActive ? 1 : 0));
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get node data based on given node id or node name
        /// </summary>
        /// <param name="menuNode">Reference object to return mobile menu node</param>
        /// <returns>Node data based on given node id or node name</returns>
        public async Task GetMenuNodeAsync(MobileMenuNodeDTO menuNode)
        {
            menuNode.MobileMenuNode = await SqlConnection.FindWithQueryAsync<MobileMenuNodeModel>("SELECT A.*, " +
                "CASE WHEN A.LeftMenuNodeID > 0 AND D.IsActive = 0 THEN 0 ELSE A.LeftMenuNodeID END AS LeftMenuNodeID, " +
                "CASE WHEN A.LeftMenuNodeID > 0 AND D.IsActive = 0 THEN ? ELSE A.LeftMenuActionID END AS LeftMenuActionID, " +
                "CASE WHEN A.RightMenuNodeID > 0 AND E.IsActive = 0 THEN 0 ELSE A.RightMenuNodeID END AS RightMenuNodeID, " +
                "CASE WHEN A.RightMenuNodeID > 0 AND E.IsActive = 0 THEN ? ELSE A.RightMenuActionID END AS RightMenuActionID, " +
                "CASE WHEN A.NodeName <> ? AND A.NodeType = ? THEN B.FeatureText ELSE C.PageHeading END AS PageHeading " +
                "FROM MobileMenuNodeModel A LEFT JOIN OrganizationFeaturePermissionModel B ON A.TargetID = B.FeatureID AND A.NodeType = ? " +
                "LEFT JOIN ContentDetailModel C ON ((A.TargetID = C.PageID AND A.NodeType = ?) OR (A.NodeName = ? AND C.PageID = ?)) AND C.LanguageID = ? " +
                "LEFT JOIN MobileMenuNodeModel D ON A.LeftMenuNodeID = D.MobileMenuNodeID AND D.IsActive = 1 " +
                "LEFT JOIN MobileMenuNodeModel E ON A.RightMenuNodeID = E.MobileMenuNodeID AND E.IsActive = 1 " +
                "WHERE A.MobileMenuGroupID = 0 AND (A.MobileMenuNodeID = ? OR A.TargetPage = ? OR A.NodeName = ?) AND A.IsActive = 1 AND ((A.NodeType = ? AND B.IsActive = 1) OR (A.NodeType = ? AND C.IsActive = 1))",
                MenuAction.MenuActionDefaultKey, MenuAction.MenuActionDefaultKey, Constants.STATIC_MESSAGE_PAGE_IDENTIFIER,
                MenuType.Feature, MenuType.Feature, MenuType.StaticPage, Constants.STATIC_MESSAGE_PAGE_IDENTIFIER, menuNode.MobileMenuNode.TargetID, menuNode.LanguageID,
                menuNode.MobileMenuNode.MobileMenuNodeID, menuNode.MobileMenuNode.TargetPage, menuNode.MobileMenuNode.NodeName, MenuType.Feature, MenuType.StaticPage)
                .ConfigureAwait(false);
        }

        public async Task GetLeftRightNodeDataAsync(MobileMenuNodeDTO menuNode, bool isLeft)
        {
            MobileMenuNodeModel nodeData = await SqlConnection.FindWithQueryAsync<MobileMenuNodeModel>("SELECT " +
                "CASE WHEN A.NodeType = ? THEN B.ImageName ELSE D.GroupIcon END AS GroupIcon, " +
                "CASE WHEN A.NodeType = ? THEN B.PageHeading ELSE D.FeatureText END AS PageHeading FROM MobileMenuNodeModel A " +
                "LEFT JOIN ContentPageModel B ON A.MobileMenuNodeID = B.PageID AND A.NodeType = ?" +
                "LEFT JOIN ContentPageModel C ON B.PageID = C.PageID AND A.NodeType = ?" +
                "LEFT JOIN OrganizationFeaturePermissionModel D ON A.MobileMenuNodeID = D.FeatureID AND A.NodeType = ? WHERE A.MobileMenuNodeID = ? AND A.MobileMenuGroupID = 0",
                MenuType.StaticPage, MenuType.StaticPage, MenuType.StaticPage, MenuType.Feature, isLeft ? menuNode.MobileMenuNode.LeftMenuNodeID : menuNode.MobileMenuNode.RightMenuNodeID).ConfigureAwait(false);
            if (isLeft)
            {
                menuNode.MobileMenuNode.LeftMenuAction = nodeData.GroupIcon;
                menuNode.MobileMenuNode.LeftMenuActionText = nodeData.PageHeading;
            }
            else
            {
                menuNode.MobileMenuNode.RightMenuAction = nodeData.GroupIcon;
                menuNode.MobileMenuNode.RightMenuActionText = nodeData.PageHeading;
            }
        }

        /// <summary>
        /// Get More option menus from database
        /// </summary>
        /// <param name="moreMenus">Reference object to return more menus</param>
        /// <returns>More menus in reference object</returns>
        public async Task GetMoreMenuNodesAsync(MenuDTO moreMenus)
        {
            moreMenus.Menus = await SqlConnection.QueryAsync<MenuModel>("SELECT A.*, B.MobileMenuNodeID AS TargetID, B.TargetPage, B.MobileMenuGroupID AS MenuGroupID, " +
                "CASE WHEN A.RenderType = ? THEN '' ELSE (CASE WHEN B.NodeType = ? THEN C.GroupIcon ELSE D.ImageName END) END AS GroupIcon, " +
                "E.PageHeading AS GroupHeading, " +
                "CASE WHEN A.RenderType = ? THEN '' ELSE (CASE WHEN B.NodeType = ? THEN C.FeatureText ELSE F.PageHeading END) END AS PageHeading " +
                "FROM MenuModel A INNER JOIN MobileMenuNodeModel B ON B.MobileMenuGroupID = 0 AND (A.TargetID = B.MobileMenuNodeID AND A.PageTypeID = ?) OR (A.TargetID = B.MobileMenuGroupID AND A.PageTypeID = ?) " +
                "LEFT JOIN OrganizationFeaturePermissionModel C ON B.TargetID = C.FeatureID AND B.NodeType = ? AND C.LanguageID = ? " +
                "LEFT JOIN ContentPageModel D ON B.TargetID = D.PageID AND B.NodeType = ? " +
                "LEFT JOIN ContentDetailModel F ON D.PageID = F.PageID AND B.NodeType = ? AND F.LanguageID = ? " +
                "LEFT JOIN MenuGroupModel E ON B.MobileMenuGroupID = E.MenuGroupID " +
                "WHERE A.IsActive = 1 AND B.IsActive = 1 AND ((B.NodeType = ? AND C.IsActive = 1) OR (B.NodeType = ? AND D.IsActive = 1 AND F.IsActive = 1)) " +
                "ORDER BY A.SequenceNo, B.SequenceNo", MenuRenderType.OnlyTitle, MenuType.Feature, MenuRenderType.OnlyIcon, MenuType.Feature, MenuType.Node, MenuType.Group, MenuType.Feature, moreMenus.LanguageID,
                MenuType.StaticPage, MenuType.StaticPage, moreMenus.LanguageID, MenuType.Feature, MenuType.StaticPage).ConfigureAwait(false);
            moreMenus.Menus = moreMenus.Menus.Except(moreMenus.Menus.Where(x => x.MenuGroupID == 0).Take(MobileConstants.FOOTER_MENU_MAX_COUNT - 1)).ToList();
        }
    }
}
