using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class MobileMenuGroupsPage : BasePage
{
    private readonly MenuGroupDTO _mobileMenuGroupData = new MenuGroupDTO();
    private long _clickedMenuGroupID;

    protected override async Task OnInitializedAsync()
    {
        await GetMobileMenuGroupsDataAsync().ConfigureAwait(true);
    }

    private async Task GetMobileMenuGroupsDataAsync()
    {
        _mobileMenuGroupData.MenuGroup = new MenuGroupModel();
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncMobileMenuGroupsFromServerAsync(_mobileMenuGroupData, CancellationToken.None), _mobileMenuGroupData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{ DataField=nameof(MenuGroupModel.MenuGroupID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel{ DataField=nameof(MenuGroupModel.GroupIdentifier), DataHeader=ResourceConstants.R_IDENTIFIER_KEY },
            new TableDataStructureModel{ DataField=nameof(MenuGroupModel.PageHeading), DataHeader=ResourceConstants.R_TITLE_KEY },
            new TableDataStructureModel{ DataField=nameof(MenuGroupModel.Count), DataHeader=ResourceConstants.R_LINKS_COUNT_KEY },
        };
    }

    private void OnAddEditClick(MenuGroupModel menuGroupData)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _clickedMenuGroupID = menuGroupData == null ? 0 : menuGroupData.MenuGroupID;
    }

    private async Task ShowDetailPageEventCallbackAsync(string isDataUpdated)
    {
        ShowDetailPage = false;
        _clickedMenuGroupID = 0;
        Success = Error = string.Empty;
        if (isDataUpdated == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = isDataUpdated;
            await GetMobileMenuGroupsDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = isDataUpdated;
        }
    }
}