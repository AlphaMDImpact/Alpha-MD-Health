using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class MasterService : BaseService
    {
        public MasterService(IEssentials serviceEssentials):base(serviceEssentials) 
        {
        
        }
        /// <summary>
        ///  Get Master Data Required for the Application for Web Application
        /// </summary>
        /// <param name="organisationDomain">Organisation domain</param>
        /// <returns>Organisation required configurations</returns>
        public async Task<MasterDTO> GetMasterDataAsync(string organisationDomain, long accountID)
        {
            MasterDTO masterPageData = new MasterDTO { ErrCode = ErrorCode.ErrorWhileRetrievingRecords };
            try
            {
                var isBasic = string.IsNullOrWhiteSpace(await GetSecuredValueAsync(StorageConstants.SS_ACCESS_TOKEN_KEY).ConfigureAwait(true));
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    AuthType = isBasic ? AuthorizationType.Basic : AuthorizationType.Bearer,
                    PathWithoutBasePath = UrlConstants.GET_MASTER_PAGE_DATA_ASYNC_PATH,
                    QueryParameters = new NameValueCollection {
                        { Constants.SE_ORAGNISATION_DOMAIN_QUERY_KEY, organisationDomain },
                        { nameof(BaseDTO.AccountID), (isBasic ? accountID : 0).ToString() },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(true);
                masterPageData.ErrCode = httpData.ErrCode;
                if (masterPageData.ErrCode == ErrorCode.OK && httpData.Response != null)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    await MapMasterDataAsync(organisationDomain, masterPageData, data).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                masterPageData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return masterPageData;
        }


        private async Task MapMasterDataAsync(string organisationDomain, MasterDTO masterPageData, JToken data)
        {
            if (data != null && data.HasValues)
            {
                MapCommonData(masterPageData, data);
                masterPageData.OrganisationDomain = (string)data[nameof(MasterDTO.OrganisationDomain)];
                masterPageData.OrganisationID = (long)data[nameof(MasterDTO.OrganisationID)];
                masterPageData.OrganisationName = (string)data[nameof(MasterDTO.OrganisationName)];
                masterPageData.LanguageID = (byte)data[nameof(BaseDTO.LanguageID)];
                masterPageData.DefaultRoute = (string)data[nameof(MasterDTO.DefaultRoute)];
                masterPageData.HasWelcomeScreens = (bool)data[nameof(MasterDTO.HasWelcomeScreens)];
                masterPageData.IsConsentAccepted = (bool)data[nameof(MasterDTO.IsConsentAccepted)];
                masterPageData.IsSubscriptionRequired = (bool)data[nameof(MasterDTO.IsSubscriptionRequired)];
                masterPageData.IsProfileCompleted = (bool)data[nameof(MasterDTO.IsProfileCompleted)];
                masterPageData.PermissionAtLevelID = (long)data[nameof(MasterDTO.PermissionAtLevelID)];
                if ((masterPageData.DefaultRoute == AppPermissions.LoginView.ToString() || organisationDomain.Contains($"/{AppPermissions.LoginView}"))
                    && !string.IsNullOrWhiteSpace(await GetSecuredValueAsync(StorageConstants.SS_ACCESS_TOKEN_KEY).ConfigureAwait(true)))
                {
                    // Force navigate to default page
                    masterPageData.ErrCode = ErrorCode.TokenExpired;
                }
                masterPageData.AddedON = data[nameof(MasterDTO.AddedON)] != null && data[nameof(MasterDTO.AddedON)]?.Type != JTokenType.Null ? (DateTimeOffset)data[nameof(MasterDTO.AddedON)] : default;
                masterPageData.Languages = new LanguageService(_essentials).MapLanguages(data);
                masterPageData.OrganisationFeatures = new FeatureService(_essentials).MapFeatures(data, nameof(MasterDTO.OrganisationFeatures));
                masterPageData.BranchDepartments = MapBranchDepartments(data, nameof(MasterDTO.BranchDepartments));
                masterPageData.Users = new UserService(_essentials).MapUsers(data, nameof(MasterDTO.Users));
                if (masterPageData.Users?.FirstOrDefault()?.UserID > 0)
                {
                    _essentials.SetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, masterPageData.Users[0].UserID);
                }
                //else if(!string.IsNullOrWhiteSpace(await GetSecuredValueAsync(StorageConstants.SS_ACCESS_TOKEN_KEY).ConfigureAwait(true)))
                //{
                //    // Force navigate to default page
                //    masterPageData.ErrCode = ErrorCode.TokenExpired;
                //}
                var menuService = new MenuService(_essentials);
                masterPageData.Menus = menuService.MapMenus(data, nameof(MasterDTO.Menus));
                masterPageData.MenuGroups = menuService.MapMenus(data, nameof(MasterDTO.MenuGroups));
                masterPageData.Settings.ForEach(environment =>
                {
                    if (environment.GroupName == GroupConstants.RS_ENVIRONMENT_GROUP)
                    {
                        _essentials.SetPreferenceValue(environment.SettingKey, environment.SettingValue);
                    }
                });
                masterPageData.Settings.RemoveAll(x => x.GroupName == GroupConstants.RS_ENVIRONMENT_GROUP);
                masterPageData.HasWelcomeScreens = masterPageData.HasWelcomeScreens && (_essentials.GetPreferenceValue(nameof(MasterDTO.HasWelcomeScreens), 0) == 1);
            }
        }

        /// <summary>
        /// Map branch department json into model
        /// </summary>
        /// <param name="data">branch and departments json data</param>
        /// <returns>List of branch and departments</returns>
        private List<OrganisationCollapsibleModel> MapBranchDepartments(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
                ? (from dataItem in data[collectionName]
                   select new OrganisationCollapsibleModel
                   {
                       BranchID = GetDataItem<long>(dataItem, nameof(BranchDepartmentModel.BranchID)),
                       BranchName = (string)dataItem[nameof(BranchDepartmentModel.BranchName)],
                       DepartmentID = GetDataItem<long>(dataItem, nameof(BranchDepartmentModel.DepartmentID)),
                       DepartmentName = (string)dataItem[nameof(BranchDepartmentModel.DepartmentName)],
                       AddedON = GetDataItem<DateTimeOffset>(dataItem, nameof(BranchDepartmentModel.AddedON)),
                   }).ToList()
                : null;
        }

    }
}