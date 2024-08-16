using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class SystemIdentifierServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Gets all SystemIdentifiers from database
        /// </summary>
        /// <param name="systemIdentifiersData">
        /// System Identifiers data used to send response as reference
        /// </param>
        /// <returns>Returns SystemIdentifiers data in systemIdentifiersData as reference</returns>
        public async Task GetSystemIdentifiersAsync(SystemIdentifierDTO systemIdentifiersData)
        {
            try
            {
                using var connection = ConnectDatabase();
                connection.Open();
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), systemIdentifiersData.AccountID, DbType.Int64, ParameterDirection.Input);
                parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), systemIdentifiersData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
                systemIdentifiersData.SystemIdentifiers = (await connection.QueryAsync<SystemIdentifierModel>(SPNameConstants.USP_GET_SYSTEM_IDENTIFIERS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false)).ToList();
                systemIdentifiersData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
            } 
            catch (Exception myex)
            {
                //to delete
            }
        }
    }
}
