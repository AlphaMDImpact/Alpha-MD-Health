using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class ProfessionServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get Professions from database
        /// </summary>
        /// <param name="professionData">Reference object to return list of Professions</param>
        /// <returns>List of professions</returns>
        public async Task GetProfessionsAsync(ProfessionDTO professionData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProfessionModel.ProfessionID), professionData.Profession.ProfessionID, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(professionData, parameters, AppPermissions.ProfessionsView.ToString(), $"{AppPermissions.ProfessionDelete},{AppPermissions.ProfessionAddEdit}"
           );
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), professionData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROFESSIONS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                professionData.Professions = (await result.ReadAsync<ProfessionModel>().ConfigureAwait(false))?.ToList();
                await MapReturnPermissionsAsync(professionData, result).ConfigureAwait(false);
            }
            professionData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Saves Profession in Database
        /// </summary>
        /// <param name="professionData">Object that contains data to be saved in database</param>
        /// <returns>operation status</returns>
        public async Task SaveProfessionAsync(ProfessionDTO professionData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), professionData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProfessionModel.ProfessionID), professionData.Profession.ProfessionID, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, ConvertProfessionsToTable(professionData).AsTableValuedParameter());
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(MenuModel.IsActive), professionData.Profession.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(professionData, parameters, professionData.Profession.IsActive ? AppPermissions.ProfessionAddEdit.ToString() : AppPermissions.WebMenuDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PROFESSION, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            professionData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        private DataTable ConvertProfessionsToTable(ProfessionDTO professionData)
        {
            DataTable dataTable = CreateGenericTypeTable();
            foreach (ProfessionModel record in professionData.Professions)
            {
                dataTable.Rows.Add(record.ProfessionID, Guid.Empty, record.LanguageID, record.Profession, string.Empty, string.Empty);
            }
            return dataTable;
        }
    }
}