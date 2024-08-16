using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class SubFlowsPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO { SubFlow = new SubFlowModel() };
    private long _subFlowID;

    /// <summary>
    /// Program ID parameter
    /// </summary>
    [Parameter]
    public long SubFlowID
    {
        get { return _subFlowID; }
        set
        {
            if (_subFlowID != value)
            {
                if (_programData.RecordCount > 0 || _subFlowID == 0)
                {
                    _programData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _subFlowID = value;
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
            _programData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(ProgramDTO.RecordCount)));
        }
        await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncSubFlowsFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(SubFlowModel.SubFlowID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(SubFlowModel.Name),DataHeader=ResourceConstants.R_SUBFLOW_NAME_KEY},
            new TableDataStructureModel{DataField=nameof(SubFlowModel.TaskType),DataHeader=ResourceConstants.R_TASK_TYPE_KEY},
            new TableDataStructureModel{DataField=nameof(SubFlowModel.Item),DataHeader=ResourceConstants.R_ITEM_KEY},
            new TableDataStructureModel{DataField=nameof(SubFlowModel.OperationType),DataHeader=ResourceConstants.R_OPERATION_TYPE_KEY}
        };
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.SubflowsView.ToString()).ConfigureAwait(true);
    }

    private async Task OnAddEditClick(SubFlowModel subFlow)
    {
        Success = Error = string.Empty;
        if (_programData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.SubflowsView.ToString(), (subFlow == null ? 0 : subFlow.SubFlowID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _subFlowID = subFlow == null ? 0 : subFlow.SubFlowID;
            ShowDetailPage = true;
        }
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        _subFlowID = 0;
        Success = Error = string.Empty;
        await OnViewAllClickedAsync();
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