using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class OrganisationServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get organisation profile data from database
        /// </summary>
        /// <param name="organisationData">Reference object to return organisation profile data</param>
        /// <returns>Organisation data with operation status</returns>
        public async Task GetOrganisationProfileAsync(OrganisationDTO organisationData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), organisationData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID), organisationData.OrganisationID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), organisationData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.PermissionAtLevelID), organisationData.PermissionAtLevelID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LanguageID), organisationData.LanguageID, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_CHECK_PERMISSION, organisationData.OrganisationID > 0 ? AppPermissions.OrganisationView.ToString() : AppPermissions.OrganisationSetup.ToString(), DbType.String, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_RETURN_PERMISSIONS_FOR, organisationData.OrganisationID >= 0 ? $"{AppPermissions.OrganisationSetup},{AppPermissions.OrganisationAddEdit}" : string.Empty, DbType.String, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), organisationData.ErrCode, DbType.Int16, direction: ParameterDirection.InputOutput);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_ORGANISATION_PROFILE, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                organisationData.DropDownOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                organisationData.PaymentPlans = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                organisationData.ExternalServices = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                if (organisationData.OrganisationID > 0)
                {
                    await MapOrganisationDataAsync(result, organisationData).ConfigureAwait(false);
                }
                await MapReturnPermissionsAsync(organisationData, result).ConfigureAwait(false);
            }
            organisationData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// To save organisation profile in database
        /// </summary>
        /// <param name="organisationData">reference object which holds organisation data</param>
        /// <returns>Organisation ID with Operation status</returns>
        public async Task SaveOrganisationProfileAsync(OrganisationDTO organisationData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), organisationData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(OrganisationModel.PlanID), organisationData.OrganisationProfile.PlanID, DbType.Int16, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(OrganisationModel.OrganisationDomain), organisationData.OrganisationProfile.OrganisationDomain, DbType.String, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(OrganisationModel.TaxNumber), organisationData.OrganisationProfile.TaxNumber, DbType.String, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapToLanguagesDataTable(organisationData.Languages).AsTableValuedParameter());
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS_2, MapToOrganisationNameDataTable(organisationData.PageDetails).AsTableValuedParameter());
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS_3, MapToOrganisationExternalServiceTable(organisationData.OrganisationExternalServices).AsTableValuedParameter());
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID), organisationData.OrganisationID, DbType.Int64, direction: ParameterDirection.InputOutput);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), organisationData.IsActive, DbType.Boolean, direction: ParameterDirection.Input);
            MapCommonSPParameters(organisationData, parameters, organisationData.OrganisationID > 0 ? AppPermissions.OrganisationAddEdit.ToString() : AppPermissions.OrganisationSetup.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_ORGANISATION_PROFILE, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            organisationData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (organisationData.ErrCode == ErrorCode.OK)
            {
                organisationData.OrganisationID = parameters.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID));
            }
        }

        private DataTable MapToLanguagesDataTable(List<OptionModel> languages)
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(nameof(LanguageModel.LanguageID), typeof(byte)),
                    new DataColumn(nameof(LanguageModel.IsDefault), typeof(Boolean)),
                }
            };
            if(GenericMethods.IsListNotEmpty(languages))
            {
                foreach (OptionModel item in languages)
                {
                    dataTable.Rows.Add(item.OptionID, item.IsDefault);
                }
            }
            return dataTable;
        }

        private DataTable MapToOrganisationNameDataTable(List<ContentDetailModel> organisationNames)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if (GenericMethods.IsListNotEmpty(organisationNames))
            {
                foreach (ContentDetailModel item in organisationNames)
                {
                    dataTable.Rows.Add(0, Guid.Empty, item.LanguageID, item.PageHeading, string.Empty, string.Empty);
                }
            }
            return dataTable;
        }

        private DataTable MapToOrganisationExternalServiceTable(List<OrganisationExternalServiceModel> organisationExternalServices)
        {
            DataTable dataTable = new DataTable
            {
                Columns =
                {
                    new DataColumn(nameof(OrganisationExternalServiceModel.OrganisationServiceID), typeof(long)),
                    new DataColumn(nameof(OrganisationExternalServiceModel.OrganisationID), typeof(long)),
                    new DataColumn(nameof(OrganisationExternalServiceModel.ExternalServiceID), typeof(int)),
                    new DataColumn(nameof(OrganisationExternalServiceModel.UnitPrice), typeof(decimal)),
                    new DataColumn(nameof(OrganisationExternalServiceModel.DiscountPercentage), typeof(decimal)),
                    new DataColumn(nameof(OrganisationExternalServiceModel.MinimumQuantityToBuy), typeof(long)),
                    new DataColumn(nameof(OrganisationExternalServiceModel.ForPatient), typeof(bool)),
                    new DataColumn(nameof(OrganisationExternalServiceModel.IsActive), typeof(bool)),
                }
            };
            if (GenericMethods.IsListNotEmpty(organisationExternalServices))
            {
                foreach (OrganisationExternalServiceModel item in organisationExternalServices)
                {
                    dataTable.Rows.Add(item.OrganisationServiceID, item.OrganisationID, item.ExternalServiceID, item.UnitPrice, item.DiscountPercentage, item.MinimumQuantityToBuy, item.ForPatient, item.IsActive);
                }
            }
            return dataTable;
        }

        private async Task MapOrganisationDataAsync(SqlMapper.GridReader result, OrganisationDTO organisationData)
        {
            if (!result.IsConsumed)
            {
                organisationData.OrganisationProfile = (await result.ReadAsync<OrganisationModel>().ConfigureAwait(false))?.FirstOrDefault();
                organisationData.OrganisationExternalServices = (await result.ReadAsync<OrganisationExternalServiceModel>().ConfigureAwait(false)).ToList();
            }
            if (!result.IsConsumed)
            {
                organisationData.PageDetails = (await result.ReadAsync<ContentDetailModel>().ConfigureAwait(false))?.ToList();
            }
        }

        /// <summary>
        /// Get Organisation Branches
        /// </summary>
        /// <param name="branchData">Reference object to return list of Branches</param>
        /// <returns>List of Organisation Branches in branchData</returns>
        public async Task GetOrganisationBranchesAsync(BranchDTO branchData)
        {
            string permissionToCheck;
            if (branchData.RecordCount == -1)
            {
                permissionToCheck = AppPermissions.BranchView.ToString();
            }
            else
            {
                permissionToCheck = AppPermissions.BranchesView.ToString();
            }
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), branchData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BranchModel.BranchID), branchData.Branch.BranchID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.RecordCount), branchData.RecordCount, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LanguageID), branchData.LanguageID, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), branchData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.PermissionAtLevelID), branchData.PermissionAtLevelID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_CHECK_PERMISSION, permissionToCheck, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_RETURN_PERMISSIONS_FOR, $"{AppPermissions.BranchDelete},{AppPermissions.BranchAddEdit}" , DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), branchData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_ORGANISATION_BRANCHES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                branchData.Branches = (await result.ReadAsync<BranchModel>().ConfigureAwait(false))?.ToList();
                if (branchData.RecordCount == -1)
                {
                    branchData.Departments = (await result.ReadAsync<DepartmentModel>().ConfigureAwait(false))?.ToList();
                }
                await MapReturnPermissionsAsync(branchData, result).ConfigureAwait(false);
            }
            branchData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Organisation Branch data to database
        /// </summary>
        /// <param name="branchData">Organisation Branch data to be saved</param>
        /// <returns>Result of operation</returns>
        public async Task SaveOrganisationBranchAsync(BranchDTO branchData)
        {
            string permissionToCheck;
            if (branchData.RecordCount == -1)
            {
                permissionToCheck = AppPermissions.BranchView.ToString();
            }
            else
            {
                permissionToCheck = AppPermissions.BranchDelete.ToString();
            }
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), branchData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BranchModel.BranchID), branchData.Branch.BranchID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(branchData.IsActive), branchData.IsActive, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapBranchesToTable(branchData.Branches).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS_2, MapDepartmentsToTable(branchData.Departments).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LanguageID), branchData.LanguageID, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), branchData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.PermissionAtLevelID), branchData.PermissionAtLevelID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_CHECK_PERMISSION, permissionToCheck, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), branchData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            await connection.ExecuteAsync(SPNameConstants.USP_SAVE_ORGANISATION_BRANCH, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            branchData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        private DataTable MapBranchesToTable(List<BranchModel> branches)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if (GenericMethods.IsListNotEmpty(branches))
            {
                foreach (BranchModel record in branches)
                {
                    dataTable.Rows.Add(record.BranchID, Guid.Empty, record.LanguageID, record.BranchName, string.Empty, string.Empty);
                }
            }
            return dataTable;
        }

        /// <summary>
        /// Save organisation Settings
        /// </summary>
        /// <param name="settings">Object containing Setting Data</param>
        /// <returns>return operation status</returns>
        public async Task UpdateOrganisationSettingsAsync(BaseDTO settings)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), settings.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapSettingsToTable(settings).AsTableValuedParameter());
            MapCommonSPParameters(settings, parameter, settings.AddedBy);
            await connection.QueryAsync<SaveResultModel>(SPNameConstants.USP_UPDATE_ORGANISATION_SETTINGS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            settings.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        private DataTable MapSettingsToTable(BaseDTO settings)
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(nameof(SettingModel.SettingID), typeof(int)),
                    new DataColumn(nameof(SettingModel.SettingValue), typeof(string)),
                }
            };
            foreach (SettingModel record in settings.Settings)
            {
                dataTable.Rows.Add(record.SettingID, record.SettingValue);
            }
            return dataTable;
        }

        /// <summary>
        /// Gets organisation View from database
        /// </summary>
        /// <param name="organisationData">Reference object to return organisation view data</param>
        /// <returns>Organisation view data with operation status</returns>
        public async Task GetOrganisationViewAsync(OrganisationDTO organisationData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), organisationData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID), organisationData.OrganisationID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(organisationData, parameters, AppPermissions.OrganisationView.ToString(), string.Empty);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_ORGANISATION_DATA, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                organisationData.OrganisationProfile = (await result.ReadAsync<OrganisationModel>().ConfigureAwait(false)).FirstOrDefault();
                await MapReturnPermissionsAsync(organisationData, result).ConfigureAwait(false);
            }
            organisationData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Get organisations from database
        /// </summary>
        /// <param name="organisationData">Reference object to return manage organisation data</param>
        /// <returns>Returns Organisations data and operation status</returns>
        public async Task GetOrganisationsAsync(OrganisationDTO organisationData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), organisationData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID), organisationData.OrganisationID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.RecordCount), organisationData.RecordCount, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), organisationData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.PermissionAtLevelID), organisationData.PermissionAtLevelID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LanguageID), organisationData.LanguageID, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_CHECK_PERMISSION, AppPermissions.ManageOrganisationsView.ToString(), DbType.String, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_RETURN_PERMISSIONS_FOR, $"{AppPermissions.ManageOrganisationView},{AppPermissions.OrganisationAddEdit}", DbType.String, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), organisationData.ErrCode, DbType.Int16, direction: ParameterDirection.InputOutput);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_ORGANISATION_DATA, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                if (organisationData.OrganisationID == 0)
                {
                    await MapManageOrganisationDataAsync(result, organisationData).ConfigureAwait(false);
                }
                await MapReturnPermissionsAsync(organisationData, result).ConfigureAwait(false);
            }
            organisationData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }
        private async Task MapManageOrganisationDataAsync(SqlMapper.GridReader result, OrganisationDTO organisationData)
        {
            if (!result.IsConsumed)
            {
                organisationData.Organisations = (await result.ReadAsync<OrganisationModel>().ConfigureAwait(false))?.ToList();
            }
        }
    }
}