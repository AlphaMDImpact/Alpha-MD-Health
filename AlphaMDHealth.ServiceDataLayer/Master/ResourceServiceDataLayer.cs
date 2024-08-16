using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class ResourceServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Gets resource data from database
        /// </summary>
        /// <param name="resourceData"> Used to The filter criterias (group name(s), LanguageId) the return the resources </param>
        /// <returns> uses theResources variable to return resources based on the group and operation status</returns>
        public async Task GetResourcesAsync(BaseDTO resourceData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LanguageID), resourceData.LanguageID, DbType.Byte, ParameterDirection.Input);
            AddDateTimeParameter(nameof(BaseDTO.LastModifiedON), string.Empty, parameter, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), resourceData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID), resourceData.OrganisationID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_IS_WEB, resourceData.IsActive, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), resourceData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            resourceData.Resources = (await connection.QueryAsync<ResourceModel>(SPNameConstants.USP_GET_RESOURCES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false)).ToList();
            resourceData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }
    }
}