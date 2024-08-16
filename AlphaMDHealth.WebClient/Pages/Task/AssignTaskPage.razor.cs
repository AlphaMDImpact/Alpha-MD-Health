using System.Globalization;
using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class AssignTaskPage : BasePage
{
    private readonly ProgramDTO _patientTaskData = new ProgramDTO { RecordCount = -1, Task = new TaskModel() };
    private PatientTaskService _taskService;

    protected override async Task OnInitializedAsync()
    {
        _taskService = new PatientTaskService(AppState.webEssentials);
        _patientTaskData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        await SendServiceRequestAsync(_taskService.GetPatientTaskAsync(_patientTaskData), _patientTaskData).ConfigureAwait(true);
        if (_patientTaskData.ErrCode == ErrorCode.OK)
        {
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_patientTaskData.ErrCode.ToString());
        }
    }

    private async Task OnTaskTypeChangedAsync(object optionIdObj)
    {
        Success = Error = string.Empty;
        var selectedTaskType = _patientTaskData.TaskTypes.FirstOrDefault(x => x.IsSelected);
        if (selectedTaskType == null)
        {
            _patientTaskData.Items = new List<OptionModel>();
        }
        else
        {
            ProgramDTO programData = new ProgramDTO
            {
                Program = new ProgramModel { ProgramID = -1, Name = selectedTaskType.GroupName },
                SelectedUserID = _patientTaskData.SelectedUserID,
            };
            var service = new ProgramService(AppState.webEssentials);
            service.PageData.Resources = _patientTaskData.Resources;
            await SendServiceRequestAsync(service.SyncTaskItemsFromServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
            _patientTaskData.ErrCode = programData.ErrCode;
            if (programData.ErrCode == ErrorCode.OK)
            {
                _patientTaskData.Items = programData.Items;
            }
            else
            {
                Error = programData.ErrCode.ToString();
            }
        }
    }

    private void OnCancelClick()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private async Task OnSaveActionClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            if (_patientTaskData.Task.FromDate > _patientTaskData.Task.ToDate)
            {
                Error = string.Format(CultureInfo.InvariantCulture
                    , LibResources.GetResourceValueByKey(_patientTaskData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY)
                    , LibResources.GetResourceValueByKey(_patientTaskData.Resources, ResourceConstants.R_START_DATE_KEY)
                    , LibResources.GetResourceValueByKey(_patientTaskData.Resources, ResourceConstants.R_END_DATE_KEY));
                return;
            }
            _patientTaskData.Task.ItemID = _patientTaskData.Items.Find(x => x.IsSelected).OptionID;
            _patientTaskData.Task.Status = ResourceConstants.R_NEW_STATUS_KEY;
            _patientTaskData.Task.TaskType = _patientTaskData.TaskTypes.Find(x => x.IsSelected).GroupName;
            ProgramDTO programData = new ProgramDTO
            {
                Task = _patientTaskData.Task,
            };
            programData.Task.FromDate = _patientTaskData.Task.FromDate;
            programData.Task.ToDate = _patientTaskData.Task.ToDate;
            programData.Task.IsActive = true;
            programData.SelectedUserID = _patientTaskData.SelectedUserID;
            await SendServiceRequestAsync(_taskService.SyncPatientTaskToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
            _patientTaskData.ErrCode = programData.ErrCode;
            if (_patientTaskData.ErrCode == ErrorCode.OK)
            {
                await OnClose.InvokeAsync(_patientTaskData.ErrCode.ToString());
            }
            else
            {
                Error = _patientTaskData.ErrCode.ToString();
            }
        }
    }
}