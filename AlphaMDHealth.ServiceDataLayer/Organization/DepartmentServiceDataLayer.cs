using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class DepartmentServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get Departments from server
        /// </summary>
        /// <param name="departmentData">Object to hold department data</param>
        /// <returns>List of Deparments in departmentData and Operation Status</returns>
        public async Task GetDepartmentsAsync(DepartmentDTO departmentData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), departmentData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(DepartmentModel.DepartmentID), departmentData.Department.DepartmentID, DbType.Byte, ParameterDirection.Input);
            MapCommonSPParameters(departmentData, parameter,
                AppPermissions.DepartmentsView.ToString(), $"{AppPermissions.DepartmentDelete},{AppPermissions.DepartmentAddEdit}"
            );
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_DEPARTMENTS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                departmentData.Departments = (await result.ReadAsync<DepartmentModel>().ConfigureAwait(false))?.ToList();
                await MapReturnPermissionsAsync(departmentData, result).ConfigureAwait(false);
            }
            departmentData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save department Data to database
        /// </summary>
        /// <param name="departmentData">Department data to be saved</param>
        /// <returns>Operation status</returns>
        public async Task SaveDepartmentAsync(DepartmentDTO departmentData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), departmentData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(DepartmentModel.DepartmentID), departmentData.Department.DepartmentID, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapDepartmentsToTable(departmentData.Departments).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), departmentData.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(departmentData, parameter, departmentData.IsActive ? AppPermissions.DepartmentAddEdit.ToString() : AppPermissions.DepartmentDelete.ToString());
            await connection.ExecuteAsync(SPNameConstants.USP_SAVE_DEPARTMENT, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            departmentData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }
    }
}