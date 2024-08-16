using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class WebMenuGroups : BasePage
{
    private readonly MenuGroupDTO _webMenuData = new MenuGroupDTO();
    private long _webMenuGroupID;

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _webMenuData.MenuGroup = new MenuGroupModel();
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncWebMenuGroupsFromServerAsync(_webMenuData, CancellationToken.None), _webMenuData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(MenuGroupModel.MenuGroupID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(MenuGroupModel.GroupIdentifier),DataHeader=ResourceConstants.R_IDENTIFIER_KEY},
            new TableDataStructureModel{DataField=nameof(MenuGroupModel.PageHeading),DataHeader=ResourceConstants.R_TITLE_KEY},
            new TableDataStructureModel{DataField=nameof(MenuGroupModel.PageTypeName),DataHeader=ResourceConstants.R_PAGE_TYPE_KEY,}
        };
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.WebGroupContentsView.ToString()).ConfigureAwait(true);
    }

    private void OnAddEditClicked(MenuGroupModel webMenuGroup)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _webMenuGroupID = webMenuGroup == null ? 0 : webMenuGroup.MenuGroupID;
    }

    private async Task OnAddEditClosed(string errorMessage)
    {
        Success = Error = string.Empty;
        ShowDetailPage = false;
        _webMenuGroupID = 0;
        if (errorMessage == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = errorMessage;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = errorMessage;
        }
    }
}