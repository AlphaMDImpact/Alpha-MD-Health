using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class MobileMenusPage : BasePage
{
    private readonly MenuDTO _menuData = new MenuDTO() { Menu = new MenuModel()};
    private long _menuID;

    protected override async Task OnInitializedAsync()
    {
        await RetrieveDataAndDisplayAsync().ConfigureAwait(true);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_menuData.Menu.PaientMenuRoute != RouterDataRoute.Page)
        {
            await RetrieveDataAndDisplayAsync().ConfigureAwait(true);
        }
        await base.OnParametersSetAsync();
    }

    private async Task RetrieveDataAndDisplayAsync()
    {
        _menuData.Menu.IsPatientMenu = IsPatientMenu();
        _menuData.Menu.PaientMenuRoute = RouterDataRoute.Page;
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncMobileMenusFromServerAsync(_menuData, CancellationToken.None), _menuData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(MenuModel.MenuID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(MenuModel.NodeName),DataHeader=ResourceConstants.R_NODES_KEY},
            new TableDataStructureModel{DataField=nameof(MenuModel.MenuType),DataHeader=ResourceConstants.R_MENU_TYPE_KEY},
            new TableDataStructureModel{DataField=nameof(MenuModel.TargetPage),DataHeader=ResourceConstants.R_RENDER_TYPE_KEY},
            new TableDataStructureModel{DataField=nameof(MenuModel.SequenceNo),DataHeader=ResourceConstants.R_SEQUENCE_NO_KEY}
        };
    }

    private void OnAddEditClick(MenuModel menu)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _menuID = menu == null ? 0 : menu.MenuID;
    }

    private async Task OnAddEditClosedAsync(string isDataUpdated)
    {
        ShowDetailPage = false;
        _menuID = 0;
        Success = Error = string.Empty;
        if (isDataUpdated == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = isDataUpdated;
            await RetrieveDataAndDisplayAsync();
            StateHasChanged();
        }
        else
        {
            Error = isDataUpdated;
        }
    }

    private bool IsPatientMenu()
    {
        return RouterDataRoute.Page == AppPermissions.PatientMobileMenusView.ToString();
    }
}