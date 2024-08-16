using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class PatientTaskPage : BasePage
{
    private readonly ProgramDTO _patientTaskData = new ProgramDTO { RecordCount = -1, Task = new TaskModel() };
    private List<ButtonActionModel> _actionData;
    private bool _hideConfirmationPopup = true;
    double resultValue;

    /// <summary>
    /// Patient Task ID parameter
    /// </summary>
    [Parameter]
    public long PatientTaskID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _patientTaskData.Task.PatientTaskID = PatientTaskID;
        _patientTaskData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        await SendServiceRequestAsync(new PatientTaskService(AppState.webEssentials).GetPatientTaskAsync(_patientTaskData), _patientTaskData).ConfigureAwait(true);
        if (_patientTaskData.ErrCode == ErrorCode.OK)
        {
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_patientTaskData.ErrCode.ToString());
        }
    }

    private async Task OnCloseIconClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task OnDeleteConfirmationPopUpClickedAsync(object value)
    {
        _hideConfirmationPopup = true;
        Success = Error = string.Empty;
        if (Convert.ToInt64(value) == 1)
        {
            ProgramDTO programData = new ProgramDTO
            {
                Task = _patientTaskData.Task,
            };
            programData.Task.IsActive = false;
            programData.SelectedUserID = _patientTaskData.SelectedUserID;
            await SendServiceRequestAsync(new PatientTaskService(AppState.webEssentials).SyncPatientTaskToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
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

    private string GetResultValue(string number)
    {
        return Convert.ToDouble(number) % 1 == 0
                ? string.Format("{0:F0}", Convert.ToDouble(number))  // Whole number, format without decimal places
                : string.Format("{0:F2}", Convert.ToDouble(number));
    }
}