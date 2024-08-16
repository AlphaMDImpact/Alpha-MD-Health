using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class MenuServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get Mobile Menu Groups from database
        /// </summary>
        /// <param name="mobileMenuGroupsData">Reference object to return list of mobile menu groups</param>
        /// <returns>List of Mobile Menu groups</returns>
        public async Task GetMobileMenuGroupsAsync(MenuGroupDTO mobileMenuGroupsData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), mobileMenuGroupsData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuGroupModel.MenuGroupID), mobileMenuGroupsData.MenuGroup.MenuGroupID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(mobileMenuGroupsData, parameter, AppPermissions.MobileMenuGroupsView.ToString(), $"{AppPermissions.MobileMenuGroupDelete},{AppPermissions.MobileMenuGroupAddEdit}"
    );
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_MOBILE_MENU_GROUPS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                await MapMobileMenuGroupsDataAsync(mobileMenuGroupsData, result).ConfigureAwait(false);
                await MapReturnPermissionsAsync(mobileMenuGroupsData, result).ConfigureAwait(false);
            }
            mobileMenuGroupsData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        private async Task MapMobileMenuGroupsDataAsync(MenuGroupDTO mobileMenuGroupsData, SqlMapper.GridReader result)
        {
            mobileMenuGroupsData.MenuGroups = (await result.ReadAsync<MenuGroupModel>().ConfigureAwait(false))?.ToList();
            if (mobileMenuGroupsData.RecordCount != 0)
            {
                if (!result.IsConsumed)
                {
                    mobileMenuGroupsData.MenuGroupLinks = (await result.ReadAsync<MenuGroupLinkModel>().ConfigureAwait(false))?.ToList();
                }
                if (!result.IsConsumed)
                {
                    mobileMenuGroupsData.MenuGroupDetails = (await result.ReadAsync<ContentDetailModel>().ConfigureAwait(false))?.ToList();
                }
                if (!result.IsConsumed)
                {
                    mobileMenuGroupsData.Languages = (await result.ReadAsync<LanguageModel>().ConfigureAwait(false))?.ToList();
                }
            }
        }

        /// <summary>
        /// Saves mobile menu group in Database
        /// </summary>
        /// <param name="menuGroupData">Object that contains mobile menu group to be saved in database</param>
        /// <returns>operation status</returns>
        public async Task SaveMobileMenuGroupAsync(MenuGroupDTO menuGroupData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), menuGroupData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuGroupModel.MenuGroupID), menuGroupData.MenuGroup.MenuGroupID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuGroupModel.GroupIdentifier), menuGroupData.MenuGroup.GroupIdentifier, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuGroupDTO.IsActive), menuGroupData.IsActive, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, ConvertGroupDetailsToTable(menuGroupData).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS_2, ConvertMenuGroupLinksToTable(menuGroupData).AsTableValuedParameter());
            MapCommonSPParameters(menuGroupData, parameter, menuGroupData.IsActive ? AppPermissions.MobileMenuGroupAddEdit.ToString() : AppPermissions.MobileMenuGroupDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_MOBILE_MENU_GROUP, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            menuGroupData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        /// <summary>
        /// Gets mobile menu nodes list for given organisation
        /// </summary>
        /// <param name="mobileMenuNode">Contains information about for which organisation mobile menus need to be retrived</param>
        /// <returns>Mobile menu nodes list with requested permisisons</returns>
        public async Task GetMobileMenuNodesAsync(MobileMenuNodeDTO mobileMenuNode)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), mobileMenuNode.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(MobileMenuNodeModel.MobileMenuNodeID)), mobileMenuNode.MobileMenuNode.MobileMenuNodeID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(mobileMenuNode, parameter, AppPermissions.MobileMenuNodesView.ToString(), $"{AppPermissions.MobileMenuNodeDelete},{AppPermissions.MobileMenuNodeAddEdit}"
            );
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_MOBILE_MENU_NODES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                mobileMenuNode.MobileMenuNodes = (await result.ReadAsync<MobileMenuNodeModel>().ConfigureAwait(false))?.ToList();
                await MapMobileMenuNodeDataAsync(mobileMenuNode, result, mobileMenuNode.RecordCount == -1).ConfigureAwait(false);
            }
            mobileMenuNode.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
        }

        /// <summary>
        /// Saves mobile node in Database
        /// </summary>
        /// <param name="mobileMenuNode">Object that contains node to be saved in database</param>
        /// <returns>operation status</returns>
        public async Task SaveMobileMenuNodeAsync(MobileMenuNodeDTO mobileMenuNode)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), mobileMenuNode.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(MobileMenuNodeModel.MobileMenuNodeID)), mobileMenuNode.MobileMenuNode.MobileMenuNodeID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(MobileMenuNodeModel.NodeName)), mobileMenuNode.MobileMenuNode.NodeName, DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(MobileMenuNodeModel.NodeType)), mobileMenuNode.MobileMenuNode.NodeType, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(MobileMenuNodeModel.TargetID)), mobileMenuNode.MobileMenuNode.TargetID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(MobileMenuNodeModel.LeftMenuActionID)), mobileMenuNode.MobileMenuNode.LeftMenuActionID.ToString(), DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(MobileMenuNodeModel.RightMenuActionID)), mobileMenuNode.MobileMenuNode.RightMenuActionID.ToString(), DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(MobileMenuNodeModel.LeftMenuNodeID)), mobileMenuNode.MobileMenuNode.LeftMenuNodeID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(MobileMenuNodeModel.RightMenuNodeID)), mobileMenuNode.MobileMenuNode.RightMenuNodeID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(MobileMenuNodeModel.ShowIconInLeftMenu)), mobileMenuNode.MobileMenuNode.ShowIconInLeftMenu, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(MobileMenuNodeModel.ShowIconInRightMenu)), mobileMenuNode.MobileMenuNode.ShowIconInRightMenu, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(MobileMenuNodeModel.IsActive)), mobileMenuNode.MobileMenuNode.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(mobileMenuNode, parameter, mobileMenuNode.MobileMenuNode.IsActive ? AppPermissions.MobileMenuNodeAddEdit.ToString() : AppPermissions.MobileMenuNodeDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_MOBILE_MENU_NODE, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            mobileMenuNode.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        }

        private DataTable ConvertGroupDetailsToTable(MenuGroupDTO menuGroupData)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if (GenericMethods.IsListNotEmpty(menuGroupData.MenuGroupDetails))
            {
                foreach (ContentDetailModel record in menuGroupData.MenuGroupDetails)
                {
                    dataTable.Rows.Add(record.PageID, Guid.Empty, record.LanguageID
                        , string.IsNullOrWhiteSpace(record.PageHeading) ? string.Empty : record.PageHeading
                        , string.IsNullOrWhiteSpace(record.PageData) ? string.Empty : record.PageData
                        , string.Empty);
                }
            }
            return dataTable;
        }

        private DataTable ConvertMenuGroupLinksToTable(MenuGroupDTO menuGroupData)
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(nameof(MenuGroupLinkModel.GroupID), typeof(long)),
                    new DataColumn(nameof(MenuGroupLinkModel.TargetID), typeof(long)),
                    new DataColumn(nameof(MenuGroupLinkModel.SequenceNo), typeof(byte)),
                    new DataColumn(nameof(MenuGroupLinkModel.PageTypeID), typeof(byte)),
                }
            };
            if (GenericMethods.IsListNotEmpty(menuGroupData.MenuGroupLinks))
            {
                foreach (MenuGroupLinkModel record in menuGroupData.MenuGroupLinks)
                {
                    dataTable.Rows.Add(record.GroupID, record.TargetID, record.SequenceNo, record.PageTypeID);
                }
            }
            return dataTable;
        }

        /// <summary>
        /// Get web menu groups
        /// </summary>
        /// <param name="menuGroupsData">Reference object to return list of web menu groups</param>
        /// <returns>List of web menu groups</returns>
        public async Task GetWebMenuGroupsAsync(MenuGroupDTO menuGroupsData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), menuGroupsData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuGroupModel.MenuGroupID), menuGroupsData.MenuGroup.MenuGroupID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(menuGroupsData, parameters, AppPermissions.WebGroupContentsView.ToString(), $"{AppPermissions.WebGroupContentDelete},{AppPermissions.WebGroupContentAddEdit}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_WEB_MENU_GROUPS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                if (menuGroupsData.RecordCount == 0)
                {
                    menuGroupsData.MenuGroups = (await result.ReadAsync<MenuGroupModel>().ConfigureAwait(false))?.ToList();
                }
                else
                {
                    await MapWebMenuGroupAsync(menuGroupsData, result).ConfigureAwait(false);
                }
                await MapReturnPermissionsAsync(menuGroupsData, result).ConfigureAwait(false);
            }
            menuGroupsData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        private async Task MapWebMenuGroupAsync(MenuGroupDTO MenuGroupsData, SqlMapper.GridReader result)
        {
            if (!result.IsConsumed)
            {
                MenuGroupsData.MenuGroupLinks = (await result.ReadAsync<MenuGroupLinkModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                MenuGroupsData.MenuGroupDetails = (await result.ReadAsync<ContentDetailModel>().ConfigureAwait(false))?.ToList();
            }
            if (MenuGroupsData.MenuGroup.MenuGroupID > 0 && !result.IsConsumed)
            {
                MenuGroupsData.MenuGroups = (await result.ReadAsync<MenuGroupModel>().ConfigureAwait(false))?.ToList();
            }
        }

        /// <summary>
        /// Saves web menu group in Database
        /// </summary>
        /// <param name="menuGroup">Object that contains web menu group to be saved in database</param>
        /// <returns>operation status</returns>
        public async Task SaveWebMenuGroupAsync(MenuGroupDTO menuGroup)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), menuGroup.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuGroupModel.MenuGroupID), menuGroup.MenuGroup.MenuGroupID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuGroupModel.GroupIdentifier), menuGroup.MenuGroup.GroupIdentifier, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuGroupModel.PageType), menuGroup.MenuGroup.PageType, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), menuGroup.IsActive, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, ConvertGroupDetailsToTable(menuGroup).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS_2, ConvertMenuGroupLinksToTable(menuGroup).AsTableValuedParameter());
            MapCommonSPParameters(menuGroup, parameter, menuGroup.IsActive ? AppPermissions.WebGroupContentAddEdit.ToString() : AppPermissions.WebGroupContentDelete.ToString());
            await connection.ExecuteAsync(SPNameConstants.USP_SAVE_WEB_MENU_GROUP, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            menuGroup.MenuGroup.MenuGroupID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(MenuGroupModel.MenuGroupID));
            menuGroup.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        /// <summary>
        /// Get mobile menus
        /// </summary>
        /// <param name="mobileMenusData">Reference object to return list of mobile menus</param>
        /// <returns>List of mobile menus</returns>
        public async Task GetMobileMenusAsync(MenuDTO mobileMenusData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), mobileMenusData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.MenuID), mobileMenusData.Menu.MenuID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.IsPatientMenu), mobileMenusData.Menu.IsPatientMenu, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(mobileMenusData, parameter,
                GetMobileMenuViewPermission(mobileMenusData),
                GetReturnPermission(mobileMenusData)
            );
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_MOBILE_MENUS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                await MapMobileMenuDataAsync(mobileMenusData, result).ConfigureAwait(false);
            }
            mobileMenusData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }
        private string GetMobileMenuViewPermission(MenuDTO mobileMenusData)
        {
            return mobileMenusData.Menu.IsPatientMenu
                ? AppPermissions.PatientMobileMenusView.ToString()
                : AppPermissions.MobileMenusView.ToString();
        }
        private string GetReturnPermission(MenuDTO mobileMenusData)
        {
            return mobileMenusData.Menu.IsPatientMenu
                ? $"{AppPermissions.PatientMobileMenuDelete},{AppPermissions.PatientMobileMenuAddEdit}"
                : $"{AppPermissions.MobileMenuDelete},{AppPermissions.MobileMenuAddEdit}";
        }

        private string GetMobileMenuDeletePermission(MenuDTO mobileMenusData)
        {
            return mobileMenusData.Menu.IsPatientMenu
                ? AppPermissions.PatientMobileMenuDelete.ToString()
                : AppPermissions.MobileMenuDelete.ToString();
        }

        private async Task MapMobileMenuDataAsync(MenuDTO mobilemenu, SqlMapper.GridReader result)
        {
            if (mobilemenu.Menu.MenuID > 0 || mobilemenu.RecordCount != -1)
            {
                mobilemenu.Menus = (await result.ReadAsync<MenuModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed && mobilemenu.RecordCount == -1)
            {
                mobilemenu.MenuNodesGroups = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
            }
            await MapReturnPermissionsAsync(mobilemenu, result).ConfigureAwait(false);
        }

        /// <summary>
        /// Save mobile menu data to database
        /// </summary>
        /// <param name="mobileMenuData">mobile menu data to be saved</param>
        /// <returns>Result of operation</returns>
        public async Task SaveMobileMenuAsync(MenuDTO mobileMenuData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), mobileMenuData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.MenuID), mobileMenuData.Menu.MenuID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.PageTypeID), mobileMenuData.Menu.PageTypeID, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.TargetID), mobileMenuData.Menu.TargetID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.RenderType), mobileMenuData.Menu.RenderType, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.SequenceNo), mobileMenuData.Menu.SequenceNo, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.IsPatientMenu), mobileMenuData.Menu.IsPatientMenu, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.IsActive), mobileMenuData.Menu.IsActive, DbType.Boolean, ParameterDirection.Input);
            string mobileMenuAddeditPermission = mobileMenuData.Menu.IsPatientMenu ? AppPermissions.PatientMobileMenuAddEdit.ToString() : AppPermissions.MobileMenuAddEdit.ToString();
            string mobileMenuDeletePermission = mobileMenuData.Menu.IsPatientMenu ? AppPermissions.PatientMobileMenuDelete.ToString() : AppPermissions.MobileMenuDelete.ToString();
            MapCommonSPParameters(mobileMenuData, parameter, mobileMenuData.Menu.IsActive ? mobileMenuAddeditPermission : mobileMenuDeletePermission);
            await connection.QueryAsync(SPNameConstants.USP_SAVE_MOBILE_MENU, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            mobileMenuData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        /// <summary>
        /// Get web menus
        /// </summary>
        /// <param name="webMenusData">Reference object to return list of web menus</param>
        /// <returns>List of web menus</returns>
        public async Task GetWebMenusAsync(MenuDTO webMenusData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), webMenusData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.MenuID), webMenusData.Menu.MenuID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(webMenusData, parameters, AppPermissions.WebMenusView.ToString(), $"{AppPermissions.WebMenuDelete},{AppPermissions.WebMenuAddEdit}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_WEB_MENUS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                await MapWebMenuDataAsync(webMenusData, result).ConfigureAwait(false);
            }
            webMenusData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        private async Task MapWebMenuDataAsync(MenuDTO webMenu, SqlMapper.GridReader result)
        {
            if (webMenu.Menu.MenuID > 0 || webMenu.RecordCount != -1)
            {
                webMenu.Menus = (await result.ReadAsync<MenuModel>().ConfigureAwait(false))?.ToList();
            }
            if (webMenu.RecordCount == -1 && !result.IsConsumed)
            {
                webMenu.MenuNodesGroups = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
            }
            await MapReturnPermissionsAsync(webMenu, result).ConfigureAwait(false);
        }

        /// <summary>
        /// Save web menu data
        /// </summary>
        /// <param name="webMenuData">web menu data to be saved</param>
        /// <returns>Result of operation</returns>
        public async Task SaveWebMenuAsync(MenuDTO webMenuData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), webMenuData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.MenuID), webMenuData.Menu.MenuID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.TargetID), webMenuData.Menu.TargetID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.PageTypeID), webMenuData.Menu.PageTypeID, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.ScrollToPage), webMenuData.Menu.ScrollToPage, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.MenuLocation), webMenuData.Menu.MenuLocation, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.SequenceNo), webMenuData.Menu.SequenceNo, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.IsActive), webMenuData.Menu.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(webMenuData, parameter, webMenuData.Menu.IsActive ? AppPermissions.WebMenuAddEdit.ToString() : AppPermissions.WebMenuDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_WEB_MENUS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            webMenuData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        private async Task MapMobileMenuNodeDataAsync(MobileMenuNodeDTO mobileMenuNode, SqlMapper.GridReader result, bool isAddEditRecord)
        {
            if (isAddEditRecord)
            {
                if (!result.IsConsumed)
                {
                    mobileMenuNode.MenuActions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                }
                if (!result.IsConsumed)
                {
                    mobileMenuNode.ExistingMobileMenuNodes = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                }
                if (!result.IsConsumed)
                {
                    mobileMenuNode.MenuFeatures = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                }
            }
            await MapReturnPermissionsAsync(mobileMenuNode, result).ConfigureAwait(false);
        }
    }
}