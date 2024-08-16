using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class MobileMenuNodesPage : BasePage
{
    private readonly MobileMenuNodeDTO _mobileMenuNodeData = new MobileMenuNodeDTO();
    private long _menuNodeID;

    /// <summary>
    /// MenuNodeID parameter
    /// </summary>
    [Parameter]
    public long MenuNodeID
    {
        get { return _menuNodeID; }
        set
        {
            if (_menuNodeID != value)
            {
                if (_mobileMenuNodeData.RecordCount > 0 || _menuNodeID == 0)
                {
                    _mobileMenuNodeData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _menuNodeID = value;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        if (Parameters?.Count > 0)
        {
            _mobileMenuNodeData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(MobileMenuNodeDTO.RecordCount)));
        }
        _mobileMenuNodeData.MobileMenuNode = new MobileMenuNodeModel { MobileMenuNodeID = 0 };
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncMobileMenuNodesFromServerAsync(_mobileMenuNodeData, CancellationToken.None), _mobileMenuNodeData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(MobileMenuNodeModel.MobileMenuNodeID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(MobileMenuNodeModel.NodeName),DataHeader=ResourceConstants.R_NODE_NAME_KEY},
            new TableDataStructureModel{DataField=nameof(MobileMenuNodeModel.MenuType),DataHeader=ResourceConstants.R_MENU_TYPE_KEY},
            new TableDataStructureModel{DataField=nameof(MobileMenuNodeModel.TargetPage),DataHeader=ResourceConstants.R_NODE_TARGET_KEY},
            new TableDataStructureModel{DataField=nameof(MobileMenuNodeModel.LeftMenuAction),DataHeader=ResourceConstants.R_NODE_LEFT_HEADER_KEY},
            new TableDataStructureModel{DataField=nameof(MobileMenuNodeModel.RightMenuAction),DataHeader=ResourceConstants.R_NODE_RIGHT_HEADER_KEY}
        };
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.MobileMenuNodesView.ToString()).ConfigureAwait(true);
    }

    private async Task OnAddEditClickedAsync(MobileMenuNodeModel menuNode)
    {
        Success = Error = string.Empty;
        if (_mobileMenuNodeData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.MobileMenuNodesView.ToString(), (menuNode == null ? 0 : menuNode.MobileMenuNodeID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _menuNodeID = menuNode == null ? 0 : menuNode.MobileMenuNodeID;
            ShowDetailPage = true;
        }
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        _menuNodeID = 0;
        Success = Error = string.Empty;
        if (errorMessage == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = ErrorCode.OK.ToString();
            await GetDataAsync().ConfigureAwait(true);
            StateHasChanged();
        }
        else
        {
            Error = errorMessage;
        }
    }
}