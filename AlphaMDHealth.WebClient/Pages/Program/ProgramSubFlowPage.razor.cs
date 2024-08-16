using System.Globalization;
using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ProgramSubFlowPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO();
    private string _operationTypeValue;
    private double? _assignAfterDays;
    private double? _assignForDays;
    private bool _hideConfirmationPopup = true;
    private string _selectedTaskType;
    private List<ButtonActionModel> _actionData;
    private ProgramService _programService;
    private bool _isEditable = false;

    /// <summary>
    /// IsSynced
    /// </summary>
    [Parameter]
    public bool IsSynced { get; set; }

    /// <summary>
    /// Program ID
    /// </summary>
    [Parameter]
    public long ProgramID { get; set; }

    /// <summary>
    /// ProgramDuration
    /// </summary>
    [Parameter]
    public int ProgramDuration { get; set; }

    /// <summary>
    /// Subflow ID of Program
    /// </summary>
    [Parameter]
    public long SubflowID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _programService = new ProgramService(AppState.webEssentials);
        _programData.SubFlow = new SubFlowModel { ProgramSubFlowID = SubflowID, ProgramID = ProgramID , IsSynced = IsSynced };
        await SendServiceRequestAsync(_programService.SyncProgramSubflowFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        _isDataFetched = true;
        if (_programData.ErrCode == ErrorCode.OK)
        {
            _programData.Items = new List<OptionModel>();
            _assignAfterDays = _programData.SubFlow.AssignAfterDays;
            _assignForDays = _programData.SubFlow.AssignForDays;
            if (_programData.SubFlow.ItemID > 0)
            {
                _selectedTaskType = _programData.SubFlow.TaskType;
                await GetItemsForTaskTypeAsync(_programData.SubFlow.TaskType).ConfigureAwait(true);
            }
            LoadUI(_programData.SubFlow.SubFlowID);
            _isEditable = IsSynced;
        }
        else
        {
            await OnClose.InvokeAsync(null).ConfigureAwait(true);
        }
    }

    private async Task SelectedItemChangedAsync(object? e)
    {
        int.TryParse(e as string, out var optionId);
        _selectedTaskType = _programData.TaskTypes.FirstOrDefault(x => x.OptionID == optionId)?.GroupName;
        await GetItemsForTaskTypeAsync(_selectedTaskType);
    }

    private async Task GetItemsForTaskTypeAsync(string taskType)
    {
        ProgramDTO programData = new ProgramDTO
        {
            RecordCount = _programData.SubFlow.ItemID,
            Program = new ProgramModel { ProgramID = ProgramID, Name = taskType },
        };
        await SendServiceRequestAsync(_programService.SyncTaskItemsFromServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
        _programData.ErrCode = programData.ErrCode;
        if (programData.ErrCode == ErrorCode.OK)
        {
            _programData.Items = programData.Items;
        }
        else
        {
            Error = programData.ErrCode.ToString();
        }
    }

    private void SelectedSubFlowChanged(object? e)
    {
        int.TryParse(e as string, out var optionId);
        //_subFlowPickerValue = optionId;
        _programData.SubFlow.SubFlowID = optionId;
        LoadUI(optionId);
    }

    private void LoadUI(long subFlowID)
    {
        if (subFlowID <= 0)
        {
            _operationTypeValue = Constants.SYMBOL_DOUBLE_HYPHEN;
            return;
        }
        var subflowData = GetSubflowData(subFlowID);
        if (subflowData == null)
        {
            return;
        }
        UpdateProgramData(subflowData);
        _operationTypeValue = GetOperationTypeValue(subflowData);
    }

    private SubFlowModel GetSubflowData(long subFlowID)
    {
        return _programData.SubFlows.FirstOrDefault(x => x.SubFlowID == subFlowID);
    }

    private void UpdateProgramData(SubFlowModel subflowData)
    {
        _programData.SubFlow.Item = subflowData.Item;
        _programData.SubFlow.TaskType = subflowData.TaskType;
    }

    private string GetOperationTypeValue(SubFlowModel subflowData)
    {
        string operationTypeKey = _programData.Resources.FirstOrDefault(x => x.ResourceValue == subflowData.OperationType)?.ResourceKey;
        switch (operationTypeKey)
        {
            case ResourceConstants.R_BETWEEN_KEY:
                return $"{subflowData.OperationType} {subflowData.Value1}-{subflowData.Value2}";
            case ResourceConstants.R_GREATER_THAN_KEY:
            case ResourceConstants.R_LESS_THAN_EQUAL_TO_KEY:
            case ResourceConstants.R_GREATER_THAN_EQUAL_TO_KEY:
            case ResourceConstants.R_EQUAL_TO_KEY:
            case ResourceConstants.R_LESS_THAN_KEY:
                return $"{subflowData.OperationType} {subflowData.Value1}";
            default:
                return subflowData.OperationType;
        }
    }

    private async void OnSaveActionClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            ProgramDTO programData = new ProgramDTO
            {
                SubFlow = new SubFlowModel
                {
                    ProgramID = ProgramID,
                    ProgramSubFlowID = SubflowID,
                }
            };
            MapSubflowData(programData.SubFlow);
            var result = _programService.ValidateAssingAfterAndShowForDate(ProgramDuration, programData.SubFlow.AssignAfterDays, programData.SubFlow.AssignForDays, _programData.Resources);
            if (!(result.isValidAssignAfterDay && result.isValidShowForAfterDay))
            {
                Error = string.Format(CultureInfo.InvariantCulture, result.errorCode.ToString()).ToString();
                return;
            }
            await SendServiceRequestAsync(_programService.SyncProgramSubflowToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
            _programData.ErrCode = programData.ErrCode;
            if (_programData.ErrCode == ErrorCode.OK)
            {
                await OnClose.InvokeAsync(programData.ErrCode.ToString());
            }
            else
            {
                Error = _programData.ErrCode.ToString();
            }
            StateHasChanged();
        }
    }

    private void MapSubflowData(SubFlowModel subFlow)
    {
        var subflowData = _programData.SubFlows.FirstOrDefault(x => x.SubFlowID == _programData.SubFlow.SubFlowID);
        subFlow.SubFlowID = _programData.SubFlow.SubFlowID;
        subFlow.Name = subflowData.Name;
        subFlow.ItemID = _programData.Items.FirstOrDefault(x => x.IsSelected == true).OptionID;
        subFlow.Item = subflowData.Item;
        subFlow.OperationType = subflowData.OperationType;
        subFlow.AssignAfterDays = Convert.ToInt16(_assignAfterDays, CultureInfo.InvariantCulture);
        subFlow.AssignForDays = Convert.ToInt16(_assignForDays, CultureInfo.InvariantCulture);
        subFlow.TaskType = _selectedTaskType;
        subFlow.IsActive = true;
    }

    private void OnCloseIconClicked(bool isPopupShowing)
    {
        if (!isPopupShowing)
        {
            OnCancelActionClicked();
        }
    }
    private void OnCancelActionClicked()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private void OnDeleteActionClicked()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE,ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO,ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task OnDeleteConfirmationActionClickedAsync(object value)
    {
        Success = Error = string.Empty;
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                _programData.SubFlow.IsActive = false;
                ProgramDTO programData = new ProgramDTO
                {
                    SubFlow = _programData.SubFlow
                };
                await SendServiceRequestAsync(_programService.SyncProgramSubflowToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
                _programData.ErrCode = programData.ErrCode;
                if (_programData.ErrCode == ErrorCode.OK)
                {
                    await OnClose.InvokeAsync(_programData.ErrCode.ToString());
                }
                else
                {
                    Error = _programData.ErrCode.ToString();
                }
                StateHasChanged();
            }
        }
    }
}

