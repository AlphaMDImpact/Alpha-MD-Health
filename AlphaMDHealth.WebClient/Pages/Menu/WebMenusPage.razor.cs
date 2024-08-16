using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class WebMenusPage : BasePage
{
    private readonly MenuDTO _menuData = new MenuDTO();
    private long _menuID;

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true); ;
    }

    private async Task GetDataAsync()
    {
        _menuData.Menu = new MenuModel();
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncWebMenusFromServerAsync(_menuData, CancellationToken.None), _menuData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(MenuModel.MenuID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(MenuModel.NodeName),DataHeader=ResourceConstants.R_TITLE_KEY},
            new TableDataStructureModel{DataField=nameof(MenuModel.TargetPage),DataHeader=ResourceConstants.R_MENU_LOCATION_KEY},
            new TableDataStructureModel{DataField=nameof(MenuModel.MenuType),DataHeader=ResourceConstants.R_MENU_TYPE_KEY},
            new TableDataStructureModel{DataField=nameof(MenuModel.IsScrollable),DataHeader=ResourceConstants.R_SCROLL_TO_PAGE_KEY},
            new TableDataStructureModel{DataField=nameof(MenuModel.SequenceNo),DataHeader=ResourceConstants.R_SEQUENCE_NO_KEY}
        };
    }

    private void OnAddEditClick(MenuModel menu)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _menuID = menu == null ? 0 : menu.MenuID;
    }

    private async Task OnAddEditClosed(string isDataUpdated)
    {
        Success = Error = string.Empty;
        ShowDetailPage = false;
        _menuID = 0;
        if (isDataUpdated == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = isDataUpdated;
            await GetDataAsync();
        }
        else
        {
            Error = isDataUpdated;
        }
    }
}