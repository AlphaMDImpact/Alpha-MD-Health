using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class MenuService : BaseService
{
    #region Mobile Menu Group Services

    /// <summary>
    /// Sync mobile menu groups from service
    /// </summary>
    /// <param name="menuGroups">mobile menu groups reference to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of mobile menu groups data received from server</returns>
    public async Task SyncMobileMenuGroupsFromServerAsync(MenuGroupDTO menuGroups, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_MOBILE_MENU_GROUPS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_MENU_GROUP_ID_QUERY_KEY, Convert.ToString(menuGroups.MenuGroup.MenuGroupID, CultureInfo.InvariantCulture) },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(menuGroups.RecordCount, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
            menuGroups.ErrCode = httpData.ErrCode;
            if (menuGroups.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(menuGroups, data);
                    MapMobileMenuGroups(data, menuGroups);
                }
            }
        }
        catch (Exception ex)
        {
            menuGroups.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Mobile menu group to server
    /// </summary>
    /// <param name="menuGroupData">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status call</returns>
    public async Task SyncMobileMenuGroupToServerAsync(MenuGroupDTO menuGroupData, CancellationToken cancellationToken)
    {
        try
        {
            if (menuGroupData.MenuGroup != null)
            {
                var httpData = new HttpServiceModel<MenuGroupDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_MOBILE_MENU_GROUP_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    },
                    ContentToSend = menuGroupData,
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                menuGroupData.ErrCode = httpData.ErrCode;
            }
        }
        catch (Exception ex)
        {
            menuGroupData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void MapMobileMenuGroups(JToken data, MenuGroupDTO menuGroups)
    {
        menuGroups.MenuGroups = MapMobileMenuGroups(data, nameof(MenuGroupDTO.MenuGroups));
        menuGroups.MenuGroupDetails = (data[nameof(MenuGroupDTO.MenuGroupDetails)]?.Count() > 0) ?
           (from dataItem in data[nameof(MenuGroupDTO.MenuGroupDetails)]
            select new ContentDetailModel
            {
                PageID = (long)dataItem[nameof(ContentDetailModel.PageID)],
                LanguageID = (byte)dataItem[nameof(ContentDetailModel.LanguageID)],
                PageHeading = (string)dataItem[nameof(ContentDetailModel.PageHeading)]
            }).ToList() : null;
        menuGroups.MenuNodes = data[nameof(MenuGroupDTO.MenuGroupLinks)]?.Count() > 0 ?
            (from dataItem in data[nameof(MenuGroupDTO.MenuGroupLinks)]
             select new OptionModel
             {
                 OptionID = (long)dataItem[nameof(MenuGroupLinkModel.GroupID)],
                 OptionText = (string)dataItem[nameof(MenuGroupLinkModel.Heading)],
                 SequenceNo = (long)dataItem[nameof(MenuGroupLinkModel.SequenceNo)],
                 IsSelected = (byte)dataItem[nameof(MenuGroupLinkModel.SequenceNo)] > 0
             }).ToList() : new List<OptionModel>();
        menuGroups.Languages = (data[nameof(MenuGroupDTO.Languages)]?.Count() > 0) ?
                        (from dataItem in data[nameof(MenuGroupDTO.Languages)]
                         select new LanguageModel
                         {
                             LanguageID = (byte)dataItem[nameof(LanguageModel.LanguageID)],
                             LanguageName = (string)dataItem[nameof(LanguageModel.LanguageName)],
                             IsDefault = (bool)dataItem[nameof(LanguageModel.IsDefault)]
                         }).ToList() : null;
        if (menuGroups.RecordCount == -1 && menuGroups.MenuGroups != null)
        {
            menuGroups.MenuGroup = menuGroups.MenuGroups.FirstOrDefault();
            menuGroups.MenuGroups.Clear();
        }
        menuGroups.ErrCode = (ErrorCode)(int)data[nameof(MenuGroupDTO.ErrCode)];
    }

    private List<MenuGroupModel> MapMobileMenuGroups(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0) ?
            (from dataItem in data[collectionName]
             select new MenuGroupModel
             {
                 MenuGroupID = (long)dataItem[nameof(MenuGroupModel.MenuGroupID)],
                 PageHeading = (string)dataItem[nameof(MenuGroupModel.PageHeading)],
                 GroupIdentifier = (string)dataItem[nameof(MenuGroupModel.GroupIdentifier)],
                 Count = (byte?)dataItem[nameof(MenuGroupModel.Count)] ?? 0,
                 IsActive = (bool?)dataItem[nameof(MenuGroupModel.IsActive)] ?? true
             }).ToList() : null;
    }

    #endregion
}
