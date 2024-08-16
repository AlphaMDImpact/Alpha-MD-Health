using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class FileCategoryServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get File Categories
        /// </summary>
        /// <param name="categoryDTO">Reference object to return list of File Category</param>
        /// <returns>File Categories Data With Operation Status</returns>
        public async Task GetFileCategoriesAsync(FileCategoryDTO categoryDTO)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), categoryDTO.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(FileCategoryModel.FileCategoryID)), categoryDTO.FileCatergory.FileCategoryID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), categoryDTO.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(categoryDTO, parameter, AppPermissions.FilesCategoriesView.ToString(), $"{AppPermissions.FileCategoryDelete},{AppPermissions.FileCategoryAddEdit}"
            );
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_FILE_CATEGORIES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                await MapFileCategoryDataAsync(categoryDTO, result).ConfigureAwait(false);
                await MapReturnPermissionsAsync(categoryDTO, result).ConfigureAwait(false);
            }
            categoryDTO.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
        }

        /// <summary>
        /// Saves File Category in Database
        /// </summary>
        /// <param name="categoryDTO">Object that contains File Category to be saved in database</param>
        /// <returns>operation status</returns>
        public async Task SaveFileCategoryAsync(FileCategoryDTO categoryDTO)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), categoryDTO.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(FileCategoryModel.FileCategoryID), categoryDTO.FileCatergory.FileCategoryID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(FileCategoryModel.ImageName), categoryDTO.FileCatergory.ImageName, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, ConvertGroupDetailsToTable(categoryDTO).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(FileCategoryDTO.IsActive), categoryDTO.FileCatergory.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(categoryDTO, parameter, categoryDTO.FileCatergory.IsActive ? AppPermissions.FileCategoryAddEdit.ToString() : AppPermissions.FileCategoryDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_FILE_AND_DOCUMENT_CATEGORY, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            categoryDTO.FileCatergory.FileCategoryID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(categoryDTO.FileCatergory.FileCategoryID));
            categoryDTO.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        private async Task MapFileCategoryDataAsync(FileCategoryDTO categoryData, SqlMapper.GridReader result)
        {
           categoryData.FileCategories = (await result.ReadAsync<FileCategoryModel>().ConfigureAwait(false))?.ToList();
           if (categoryData.RecordCount == -1 && !result.IsConsumed)
                {
                    categoryData.FileCategoryDetails = (await result.ReadAsync<FileCategoryDetailModel>().ConfigureAwait(false))?.ToList();
                }
        }

        private DataTable ConvertGroupDetailsToTable(FileCategoryDTO CategoryData)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if (GenericMethods.IsListNotEmpty(CategoryData.FileCategoryDetails))
            {
                foreach (FileCategoryDetailModel record in CategoryData.FileCategoryDetails)
                {
                    dataTable.Rows.Add(0, Guid.Empty, record.LanguageID, record.Name, record.Description, string.Empty);
                }
            }
            return dataTable;
        }
    }
}
