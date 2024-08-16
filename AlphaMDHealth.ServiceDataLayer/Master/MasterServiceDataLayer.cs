using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer;

public class MasterServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get Master Page for App Startup
    /// </summary>
    /// <param name="masterData">reference DTO to fetch master Page Data</param>
    /// <returns>Master page data in masterData as reference</returns>
    public async Task GetMasterDataAsync(MasterDTO masterData, long userAccountID)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        MapDeviceDataInSpInputParams(masterData.Session, parameters);
        parameters.Add(ConcateAt(nameof(SessionModel.AccessToken)), masterData.Session.AccessToken, DbType.String, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(MasterDTO.OrganisationDomain)), "www.gensxty.com", DbType.String, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(BaseDTO.OrganisationID)), masterData.OrganisationID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(BaseDTO.AccountID)), masterData.AccountID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(AuthModel.UserAccountID)), userAccountID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(BaseDTO.LanguageID)), masterData.LanguageID, DbType.Byte, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(BaseDTO.ErrCode)), masterData.ErrCode, DbType.Int16, direction: ParameterDirection.InputOutput);
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_MASTER_DATA, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            var organisationData = await result.ReadFirstOrDefaultAsync().ConfigureAwait(false);
            masterData.OrganisationDomain = organisationData.OrganisationDomain;
            masterData.OrganisationID = organisationData.OrganisationID;
            masterData.LanguageID = Convert.ToByte(organisationData.LanguageID, CultureInfo.InvariantCulture);
            masterData.DefaultRoute = organisationData.DefaultRoute;
            masterData.AddedON = organisationData.AddedOn;
            masterData.OrganisationName = organisationData.OrganisationName;
            masterData.HasWelcomeScreens = organisationData.HasWelcomeScreens;
            masterData.IsConsentAccepted = organisationData.IsConsentAccepted;
            masterData.IsSubscriptionRequired = organisationData.IsSubscriptionRequired;
            masterData.IsProfileCompleted = organisationData.IsProfileCompleted;
            masterData.PermissionAtLevelID = organisationData.PermissionAtLevelID;
            await MapMasterDataAsync(masterData, result).ConfigureAwait(false);
        }
        masterData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
    }

    private async Task MapMasterDataAsync(MasterDTO masterData, SqlMapper.GridReader result)
    {
        masterData.Languages = await MapTableDataAsync<LanguageModel>(result).ConfigureAwait(false);
        masterData.Settings = await MapTableDataAsync<SettingModel>(result).ConfigureAwait(false);
        masterData.OrganisationFeatures = await MapTableDataAsync<OrganizationFeaturePermissionModel>(result).ConfigureAwait(false);
        await MapMenuDataAsync(masterData, result).ConfigureAwait(false);
        masterData.Users = await MapTableDataAsync<UserModel>(result).ConfigureAwait(false);
    }

    private async Task MapMenuDataAsync(MasterDTO masterData, SqlMapper.GridReader result)
    {
        await MapReturnPermissionsAsync(masterData, result).ConfigureAwait(false);
        masterData.BranchDepartments = await MapTableDataAsync<OrganisationCollapsibleModel>(result).ConfigureAwait(false);
        masterData.Menus = await MapTableDataAsync<MenuModel>(result).ConfigureAwait(false);
        masterData.MenuGroups = await MapTableDataAsync<MenuModel>(result).ConfigureAwait(false);
    }
}