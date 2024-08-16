using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class EducationCategoryServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get Education Categories from database
        /// </summary>
        /// <param name="categoryDTO">Reference object to return list of Education Category</param>
        /// <returns>List of Education Category</returns>
        public async Task GetEducationCategoriesAsync(EducationCategoryDTO categoryDTO)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), categoryDTO.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(EductaionCatergoryModel.EducationCategoryID), categoryDTO.EductaionCatergory.EducationCategoryID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), categoryDTO.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(categoryDTO, parameter,
               AppPermissions.EducationCategoriesView.ToString(), $"{AppPermissions.EducationCategoryDelete},{AppPermissions.EducationCategoryAddEdit}"
            );
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_EDUCATION_CATEGORIES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                await MapEducationCategoryDataAsync(categoryDTO, result).ConfigureAwait(false);
                await MapReturnPermissionsAsync(categoryDTO, result).ConfigureAwait(false);
            }
            categoryDTO.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        private async Task MapEducationCategoryDataAsync(EducationCategoryDTO categoryData, SqlMapper.GridReader result)
        {
            categoryData.EductaionCatergories = (await result.ReadAsync<EductaionCatergoryModel>().ConfigureAwait(false))?.ToList();
            if (categoryData.RecordCount == -1 && !result.IsConsumed)
            {
                categoryData.CategoryDetails = (await result.ReadAsync<EducationCategoryDetailModel>().ConfigureAwait(false))?.ToList();
            }
        }

        /// <summary>
        /// Saves Education Category in Database
        /// </summary>
        /// <param name="categoryDTO">Object that contains Education Category to be saved in database</param>
        /// <param name="isAfterImageUpload">Flag representing image data is uploaded in blob storage or not</param>
        /// <returns>operation status</returns>
        public async Task SaveEducationCategoryAsync(EducationCategoryDTO categoryDTO, bool isAfterImageUpload)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), categoryDTO.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(EductaionCatergoryModel.EducationCategoryID), categoryDTO.EductaionCatergory.EducationCategoryID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(EductaionCatergoryModel.ImageName), isAfterImageUpload ? categoryDTO.EductaionCatergory.ImageName : string.Empty, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, ConvertGroupDetailsToTable(categoryDTO).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(EducationCategoryDTO.IsActive), categoryDTO.EductaionCatergory.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(categoryDTO, parameter, categoryDTO.IsActive ? AppPermissions.EducationCategoryAddEdit.ToString() : AppPermissions.EducationCategoryDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_EDUCATION_CATEGORY, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            categoryDTO.EductaionCatergory.EducationCategoryID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(categoryDTO.EductaionCatergory.EducationCategoryID));
            categoryDTO.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        private DataTable ConvertGroupDetailsToTable(EducationCategoryDTO CategoryData)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if (GenericMethods.IsListNotEmpty(CategoryData.CategoryDetails))
            {
                foreach (EducationCategoryDetailModel record in CategoryData.CategoryDetails)
                {
                    dataTable.Rows.Add(record.PageID, Guid.Empty, record.LanguageID, record.PageHeading, record.PageData, string.Empty);
                }
            }
            return dataTable;
        }
    }
}
