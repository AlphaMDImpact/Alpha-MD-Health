using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer;
public class OrganisationTagServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get OrganisationTags from server
    /// </summary>
    /// <param name="organisationTagData">Object to hold OrganisationTag data</param>
    /// <returns>List of OrganisationTag in organisationTagData and Operation Status</returns>
    public async Task GetOrganisationTagsAsync(OrganisationTagDTO organisationTagData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), organisationTagData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(OrganisationTagModel.OrganisationTagID)), organisationTagData.OrganisationTag.OrganisationTagID, DbType.Int64, ParameterDirection.Input);
        MapCommonSPParameters(organisationTagData, parameters,
            AppPermissions.OrganisationTagsView.ToString(), $"{AppPermissions.OrganisationTagDelete},{AppPermissions.OrganisationTagAddEdit}"
        );
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_ORGANISATION_TAGS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            organisationTagData.OrganisationTags = (await result.ReadAsync<OrganisationTagModel>().ConfigureAwait(false))?.ToList();
            await MapReturnPermissionsAsync(organisationTagData, result).ConfigureAwait(false);
        }
        organisationTagData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Saves OrganisationTag in Database
    /// </summary>
    /// <param name="organisationTagData">Object that contains OrganisationTag to be saved in database</param>
    /// <returns>operation status</returns>
    public async Task SaveOrganisationTagAsync(OrganisationTagDTO organisationTagData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), organisationTagData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(OrganisationTagModel.OrganisationTagID)), organisationTagData.OrganisationTag.OrganisationTagID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapOrganisationTagsToTable(organisationTagData).AsTableValuedParameter());
        parameter.Add(ConcateAt(nameof(BaseDTO.IsActive)), organisationTagData.IsActive, DbType.Boolean, ParameterDirection.Input);
        MapCommonSPParameters(organisationTagData, parameter, organisationTagData.IsActive ? AppPermissions.OrganisationTagAddEdit.ToString() : AppPermissions.OrganisationTagDelete.ToString());
        await connection.ExecuteAsync(SPNameConstants.USP_SAVE_ORGANISATION_TAG, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        organisationTagData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }

    private DataTable MapOrganisationTagsToTable(OrganisationTagDTO organisationTagData)
    {
        DataTable dataTable = CreateGenericTypeTable();
        foreach (OrganisationTagModel record in organisationTagData.OrganisationTags)
        {
            dataTable.Rows.Add(record.OrganisationTagID, Guid.Empty, record.LanguageID, record.TagText, record.TagDescription, string.Empty);
        }
        return dataTable;
    }
}



