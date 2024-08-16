using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class DashboardServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get Dashboard Configuration
        /// </summary>
        /// <param name="dashboardData">Reference object to return list of Dashboard Configurations</param>
        /// <returns>List of Dashboard Configurations</returns>
        public async Task GetDashboardConfigurationsAsync(DashboardDTO dashboardData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), dashboardData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ConfigureDashboardModel.DashboardSettingID), dashboardData.ConfigurationRecord.DashboardSettingID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserRolesModel.RoleID), dashboardData.ConfigurationRecord.RoleID, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.SelectedUserID), dashboardData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(dashboardData, parameter, GetCheckPermission(dashboardData), ReturnPermission(dashboardData));
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_DASHBOARD_CONFIGURATIONS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                await MapDashboardConfigurationRecordsAsync(dashboardData, result).ConfigureAwait(false);
            }
            dashboardData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        private string GetCheckPermission(DashboardDTO dashboardData)
        {
            if (dashboardData.RecordCount == -1 || dashboardData.RecordCount == 0)
            {
                return AppPermissions.DashboardConfigurationView.ToString();
            }
            else
            {
                return dashboardData.SelectedUserID == 0 ? AppPermissions.DashboardView.ToString() : AppPermissions.PatientView.ToString();
            }
        }

        private string ReturnPermission(DashboardDTO dashboardData)
        {
            if (dashboardData.RecordCount == -1)
            {
                return $"{AppPermissions.DashboardConfigurationDelete},{AppPermissions.DashboardConfigurationAddEdit}";
            }
            else if (dashboardData.RecordCount == -2)
            {
                return AppPermissions.DashboardConfigurationView.ToString();
            }
            else
            {
                return dashboardData.RecordCount == 0 ? AppPermissions.DashboardConfigurationAddEdit.ToString() : string.Empty;
            }
        }

        /// <summary>
        /// Save Dashboard Configuration
        /// </summary>
        /// <param name="dashboardData">Dashboard Configuration data to be saved</param>
        /// <returns>Result of operation</returns>
        public async Task SaveDashboardConfigurationAsync(DashboardDTO dashboardData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), dashboardData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ConfigureDashboardModel.DashboardSettingID), dashboardData.ConfigurationRecord.DashboardSettingID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), dashboardData.IsActive, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserRolesModel.RoleID), dashboardData.ConfigurationRecord.RoleID, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SystemFeatureParameterModel.FeatureID), dashboardData.ConfigurationRecord.FeatureID, DbType.Int32, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ConfigureDashboardModel.SequenceNo), dashboardData.ConfigurationRecord.SequenceNo, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapDashboardConfigurationsToTable(dashboardData.ConfigurationRecordParameters).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LanguageID), dashboardData.LanguageID, DbType.Byte, ParameterDirection.Input);
            MapCommonSPParameters(dashboardData, parameter, dashboardData.IsActive ? AppPermissions.DashboardConfigurationAddEdit.ToString() : AppPermissions.DashboardConfigurationDelete.ToString());
            await connection.ExecuteAsync(SPNameConstants.USP_SAVE_DASHBOARD_CONFIGURATION, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            dashboardData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        private DataTable MapDashboardConfigurationsToTable(List<SystemFeatureParameterModel> ConfigurationRecordParameters)
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(nameof(SystemFeatureParameterModel.FeatureID), typeof(int)),
                    new DataColumn(nameof(SystemFeatureParameterModel.ParameterID), typeof(int)),
                    new DataColumn(nameof(SystemFeatureParameterModel.ParameterValue), typeof(string)),
                }
            };
            if (GenericMethods.IsListNotEmpty(ConfigurationRecordParameters))
            {
                foreach (SystemFeatureParameterModel record in ConfigurationRecordParameters)
                {
                    dataTable.Rows.Add(record.FeatureID, record.ParameterID, record.ParameterValue);
                }
            }
            return dataTable;
        }


        private async Task MapDashboardConfigurationRecordsAsync(DashboardDTO dashboardData, SqlMapper.GridReader result)
        {
            switch (dashboardData.RecordCount)
            {
                case -2:
                    dashboardData.ConfigurationRecords = await MapTableDataAsync<ConfigureDashboardModel>(result).ConfigureAwait(false);
                    dashboardData.ConfigurationRecordParameters = await MapTableDataAsync<SystemFeatureParameterModel>(result).ConfigureAwait(false);
                    break;
                case -1:
                    await MapDashboardConfigurationAsync(dashboardData, result).ConfigureAwait(false);
                    break;
                case 0:
                    await MapDashboardConfigurationsAsync(dashboardData, result).ConfigureAwait(false);
                    break;
                default:
                    // do not map
                    break;
            }
            dashboardData.FeaturePermissions = await MapTableDataAsync<OrganizationFeaturePermissionModel>(result).ConfigureAwait(false);
        }

        private async Task MapDashboardConfigurationAsync(DashboardDTO dashboardData, SqlMapper.GridReader result)
        {
            if (dashboardData.ConfigurationRecord.DashboardSettingID > 0)
            {
                dashboardData.ConfigurationRecord = (await result.ReadAsync<ConfigureDashboardModel>().ConfigureAwait(false)).FirstOrDefault();
            }
            dashboardData.FeaturesOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
            dashboardData.ConfigurationRecordParameters = (await result.ReadAsync<SystemFeatureParameterModel>().ConfigureAwait(false))?.ToList();
        }

        private async Task MapDashboardConfigurationsAsync(DashboardDTO dashboardData, SqlMapper.GridReader result)
        {
            dashboardData.RolesOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
            dashboardData.ConfigurationRecords = (await result.ReadAsync<ConfigureDashboardModel>().ConfigureAwait(false))?.ToList();
        }
    }
}
