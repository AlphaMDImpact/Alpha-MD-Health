using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class ProgramTaskPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO { Task = new TaskModel(), RecordCount = 0 };
    private ProgramService _programService;
    private TaskModel _selectedTask = new TaskModel();
    private List<ButtonActionModel> _actionData;
    private bool _hideConfirmationPopup = true;
    private double? _assignAfterDays;
    private double? _assignForDays;
    private bool _isEditable = false;
    private bool _executeOnLogin;

    /// <summary>
    /// IsSynced
    /// </summary>
    [Parameter]
    public bool IsSynced { get; set; }

    /// <summary>
    /// Selected program id
    /// </summary>
    [Parameter]
    public long ProgramID { get; set; }

    /// <summary>
    /// Selected task ID
    /// </summary>
    [Parameter]
    public long SelectedTaskID
    {
        get { return _programData.Task.ProgramTaskID; }
        set { _programData.Task.ProgramTaskID = value; }
    }

    /// <summary>
    ///  Program Duration
    /// </summary>
    [Parameter]
    public int ProgramDuration { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _programService =  new ProgramService(AppState.webEssentials);
        _programData.Task.ProgramID = ProgramID;
        _programData.Task.IsSynced = IsSynced;
        await SendServiceRequestAsync(_programService.SyncProgramTaskFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            if (_programData.Task.ProgramTaskID > 0)
            {
                _selectedTask = _programData.Task;
                _assignAfterDays = _programData.Task.AssignAfterDays;
                _assignForDays = _programData.Task.AssignForDays;
                _executeOnLogin = _programData.Task.ExecuteOnLogin;
            }
            else
            {
                _executeOnLogin = false;
            }
            _isEditable = IsSynced;
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(null);
        }
    }

    private void OnTaskChanged(object? e)
    {
        if (int.TryParse(e as string, out int optionId) && optionId > 0)
        {
            _selectedTask = _programData.Tasks?.FirstOrDefault(x => x.TaskID == optionId);
        }
        else
        {
            _selectedTask = new TaskModel();
        }
    }

    private async void OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            ProgramDTO programData = new ProgramDTO
            {
                Task = new TaskModel { ProgramID = ProgramID, ProgramTaskID = SelectedTaskID }
            };
            MapProgramTask(programData.Task);
            var result = _programService.ValidateAssingAfterAndShowForDate(ProgramDuration, programData.Task.AssignAfterDays, programData.Task.AssignForDays, _programData.Resources);
            if (!(result.isValidAssignAfterDay && result.isValidShowForAfterDay))
            {
                Error = string.Format(CultureInfo.InvariantCulture, result.errorCode.ToString()).ToString();
                return;
            }
            await SendServiceRequestAsync(_programService.SyncProgramTaskToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
            if (programData.ErrCode == ErrorCode.OK)
            {
                await OnClose.InvokeAsync(programData.ErrCode.ToString());
            }
            else
            {
                Error = programData.ErrCode.ToString();
            }
            StateHasChanged();
        }
    }

    private void OnCloseIconClicked(bool isPopupShowing)
    {
        if (!isPopupShowing)
        {
            OnCancelClick();
        }
    }

    private void OnCancelClick()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE,ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO,ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task DeletePopUpCallbackAsync(object value)
    {
        Success = Error = string.Empty;
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                ProgramDTO programData = new ProgramDTO { Task = new TaskModel { ProgramTaskID = SelectedTaskID, ProgramID = ProgramID, IsActive = false } };
                await SendServiceRequestAsync(_programService.SyncProgramTaskToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
                if (programData.ErrCode == ErrorCode.OK)
                {
                    await OnClose.InvokeAsync(programData.ErrCode.ToString());
                }
                else
                {
                    Error = programData.ErrCode.ToString();
                }
                StateHasChanged();
            }
        }
    }

    private void MapProgramTask(TaskModel task)
    {
        task.TaskID = _selectedTask.TaskID;
        task.Name = _selectedTask.Name;
        task.TaskType = _selectedTask.TaskType;
        task.SelectedItemName = _selectedTask.SelectedItemName;
        task.NoOfSubflows = _selectedTask.NoOfSubflows;
        task.AssignAfterDays = Convert.ToInt16(_assignAfterDays, CultureInfo.InvariantCulture);
        task.AssignForDays = Convert.ToInt16(_assignForDays, CultureInfo.InvariantCulture);
        task.ExecuteOnLogin = _executeOnLogin;
        task.IsActive = true;
    }

    private List<OptionModel> GetOptions()
    {
        return new List<OptionModel> {
            new OptionModel {
                OptionID = 1,
                OptionText = LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_EXECUTE_ON_LOGIN_KEY),
                IsSelected = _executeOnLogin }
        };
    }
}