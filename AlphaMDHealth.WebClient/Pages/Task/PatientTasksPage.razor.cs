using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class PatientTasksPage : BasePage
{
    private ProgramDTO _patientTaskData = new ProgramDTO();
    private long _patientTaskID;
    public object _pageData;

    /// <summary>
    /// Patient Task ID parameter
    /// </summary>
    [Parameter]
    public long PatientTaskID
    {
        get { return _patientTaskID; }
        set
        {
            if (_patientTaskID != value)
            {
                if (_patientTaskData.RecordCount > 0 || _patientTaskID == 0)
                {
                    _patientTaskData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _patientTaskID = value;
            }
        }
    }

    /// <summary>
    /// External data to load
    /// </summary>
    [Parameter]
    public object PageData
    {
        get
        {
            return _pageData;
        }
        set
        {
            _pageData = value;
            if (_pageData != null)
            {
                _patientTaskData = _pageData as ProgramDTO;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (PageData == null)
        {
            if (Parameters?.Count > 0)
            {
                _patientTaskData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(ContentPageDTO.RecordCount)));
            }
            await GetDataAsync().ConfigureAwait(false);
        }
        else
        {
            _patientTaskData = PageData as ProgramDTO;
            _isDataFetched = true;
        }
    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new PatientTaskService(AppState.webEssentials).GetTasksAsync(_patientTaskData, string.Empty), _patientTaskData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        var tableStructure = new List<TableDataStructureModel>()
        {
             new TableDataStructureModel{DataField=nameof(TaskModel.ProgramTaskID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
             new TableDataStructureModel{BorderColorDataField=nameof(TaskModel.ProgramColor)},
             new TableDataStructureModel{DataField=nameof(TaskModel.SelectedItemName),DataHeader=ResourceConstants.R_TASK_NAME_KEY},
             new TableDataStructureModel{DataField=nameof(TaskModel.TaskType),DataHeader=ResourceConstants.R_TASK_TYPE_KEY},
             new TableDataStructureModel{DataField=nameof(TaskModel.Name),DataHeader=ResourceConstants.R_PROGRAM_TITLE_KEY},
        };
        if (_patientTaskData.RecordCount < 1)
        {
            tableStructure.Add(new TableDataStructureModel { DataField = nameof(TaskModel.FromDateValue), DataHeader = ResourceConstants.R_START_DATE_KEY, Formatter = _patientTaskData.Task.FromDateString });
            tableStructure.Add(new TableDataStructureModel { DataField = nameof(TaskModel.ToDateValue), DataHeader = ResourceConstants.R_END_DATE_KEY, Formatter = _patientTaskData.Task.FromDateString });
            tableStructure.Add(new TableDataStructureModel { DataField = nameof(TaskModel.StatusValue), DataHeader = ResourceConstants.R_STATUS_KEY, IsBadge = true, BadgeFieldType = nameof(TaskModel.StatusColor) });
        }
        return tableStructure;
    }

    private async Task OnAddEditClick(TaskModel taskModel)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        if (_patientTaskData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.PatientTasksView.ToString(), (taskModel == null ? 0 : taskModel.PatientTaskID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _patientTaskID = taskModel == null ? 0 : taskModel.PatientTaskID;
        }
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.PatientTasksView.ToString()).ConfigureAwait(true);
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        Success = Error = string.Empty;
        ShowDetailPage = false;
        if (PageData == null) await OnViewAllClickedAsync();
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

    private bool ShowQuestionnairePage()
    {
        var selectedTask = _patientTaskData.Tasks.Find(x => x.PatientTaskID == PatientTaskID);
        return LibPermissions.HasPermission(_patientTaskData.FeaturePermissions, AppPermissions.QuestionnaireTaskView.ToString())
            && (selectedTask.TaskRespondent.ToEnum<ReadingAddedBy>() == ReadingAddedBy.ProviderKey || selectedTask.TaskRespondent.ToEnum<ReadingAddedBy>() == ReadingAddedBy.BothKey)
            && (selectedTask.Status == ResourceConstants.R_NEW_STATUS_KEY || selectedTask.Status == ResourceConstants.R_INPROGRESS_STATUS_KEY)
            && selectedTask.FromDate?.Date <= DateTime.Today ;
    }
}