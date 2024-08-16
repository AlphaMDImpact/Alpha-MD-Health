using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class TasksPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO();
    private long _taskID;

    /// <summary>
    /// TaskID parameter
    /// </summary>
    [Parameter]
    public long TaskID
    {
        get { return _taskID; }
        set
        {
            if (_taskID != value)
            {
                if (_programData.RecordCount > 0 || _taskID == 0)
                {
                    _programData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _taskID = value;
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
        _programData.Task = new TaskModel();
        await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncTasksFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(TaskModel.TaskID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(TaskModel.Name),DataHeader=ResourceConstants.R_TASK_NAME_KEY },
            new TableDataStructureModel{DataField=nameof(TaskModel.TaskType),DataHeader=ResourceConstants.R_TASK_TYPE_KEY},
            new TableDataStructureModel{DataField=nameof(TaskModel.SelectedItemName),DataHeader=ResourceConstants.R_ITEM_KEY},
            new TableDataStructureModel{DataField=nameof(TaskModel.NoOfSubflows),DataHeader=ResourceConstants.R_NUMBER_OF_SUBFLOWS_KEY,}
        };
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.TasksView.ToString()).ConfigureAwait(true);
    }

    private async Task OnAddEditClickedAsync(TaskModel task)
    {
        Success = Error = string.Empty;
        if (_programData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.TasksView.ToString(), (task == null ? 0 : task.TaskID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _taskID = task == null ? 0 : task.TaskID;
            ShowDetailPage = true;
        }
    }

    private async Task OnAddEditClosedAsync(string response)
    {
        Success = Error = string.Empty;
        ShowDetailPage = false;
        _taskID = 0;
        await OnViewAllClickedAsync();
        if (response == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = response;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = response;
        }
    }
}
