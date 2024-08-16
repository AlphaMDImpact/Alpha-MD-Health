using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class TaskSubFlowPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO();
    private List<ButtonActionModel> _popupActions;
    private bool _hideDeletedConfirmationPopup = true;
    private string _operationTypeValue;
    private double? _assignAfterDays;
    private double? _assignForDays;
    private bool _isEditable; 

    /// <summary>
    /// Selected Task Subflow ID
    /// </summary>
    [Parameter]
    public long TaskSubflowID { get; set; }

    /// <summary>
    /// Task ID
    /// </summary>
    [Parameter]
    public long TaskID { get; set; }

    /// <summary>
    /// Callback Event to execute code after Edit page is closed
    /// </summary>
    [Parameter]
    public EventCallback<SubFlowModel> OnClosed { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _programData.SubFlow = new SubFlowModel { TaskID = TaskID, TaskSubFlowID = TaskSubflowID };
        await SendServiceRequestAsync( new ProgramService(AppState.webEssentials).SyncTaskSubflowFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            if (_programData.SubFlow.SubFlowID > 0)
            {
                _assignAfterDays = Convert.ToDouble(_programData.SubFlow.AssignAfterDays);
                _assignForDays = Convert.ToDouble(_programData.SubFlow.AssignForDays);
            }
            _isEditable = LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.SubflowAddEdit.ToString());
            LoadUI(_programData.SubFlow.SubFlowID);
            _isDataFetched = true;
        }
        else
        {
            await OnClosed.InvokeAsync(null).ConfigureAwait(true);
        }
    }

    private void SelectedSubFlowChanged(object option)
    {
        if(option != null)
        {
            if (!string.IsNullOrWhiteSpace(option as string))
            {
                LoadUI(Convert.ToInt64(option));
            }
        }
    }

    private void LoadUI(long subFlowID)
    {
        if (subFlowID > 0)
        {
            _programData.SubFlow = _programData.SubFlows.FirstOrDefault(x => x.SubFlowID == subFlowID);
            string operationTyeKey = _programData.Resources.FirstOrDefault(x => x.ResourceValue == _programData.SubFlow.OperationType).ResourceKey;
            if (operationTyeKey == ResourceConstants.R_BETWEEN_KEY)
            {
                _operationTypeValue = string.Concat(_programData.SubFlow.OperationType, Constants.STRING_SPACE, _programData.SubFlow.Value1, Constants.DASH_KEY, _programData.SubFlow.Value2);
            }
            else if (operationTyeKey == ResourceConstants.R_GREATER_THAN_KEY
                || operationTyeKey == ResourceConstants.R_LESS_THAN_EQUAL_TO_KEY
                || operationTyeKey == ResourceConstants.R_GREATER_THAN_EQUAL_TO_KEY
                || operationTyeKey == ResourceConstants.R_EQUAL_TO_KEY
                || operationTyeKey == ResourceConstants.R_LESS_THAN_KEY)
            {
                _operationTypeValue = string.Concat(_programData.SubFlow.OperationType, Constants.STRING_SPACE, _programData.SubFlow.Value1);
            }
            else
            {
                _operationTypeValue = _programData.SubFlow.OperationType;
            }
        }
        else
        {
            _programData.SubFlow = new SubFlowModel();
            _operationTypeValue = Constants.SYMBOL_DOUBLE_HYPHEN;
        }
    }

    private void OnCancelActionClicked()
    {
        OnClosed.InvokeAsync(null);
    }

    private void OnDeleteClicked()
    {
        _popupActions = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnDeleteConfirmationActionClickedAsync(object sequenceNo)
    {
        _hideDeletedConfirmationPopup = true;
        Success = Error = string.Empty;
        if (sequenceNo != null)
        {
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                ProgramDTO programData = new ProgramDTO
                {
                    SubFlow = new SubFlowModel
                    {
                        TaskID = TaskID,
                        TaskSubFlowID = TaskSubflowID,
                        IsActive = false
                    }
                };
                await SendServiceRequestAsync( new ProgramService(AppState.webEssentials).SyncTaskSubflowToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
                _programData.ErrCode = programData.ErrCode;
                if (_programData.ErrCode == ErrorCode.OK)
                {
                    await OnClosed.InvokeAsync(programData.SubFlow);
                }
                else
                {
                    Error = _programData.ErrCode.ToString();
                }
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            ProgramDTO programData = new ProgramDTO
            {
                SubFlow = new SubFlowModel
                {
                    TaskID = TaskID,
                    TaskSubFlowID = TaskSubflowID,
                    SubFlowID = _programData.ProgramSubFlows.FirstOrDefault(x => x.IsSelected).OptionID,
                    AssignAfterDays = Convert.ToInt16(_assignAfterDays, CultureInfo.InvariantCulture),
                    AssignForDays = Convert.ToInt16(_assignForDays, CultureInfo.InvariantCulture),
                    IsActive = true
                }
            };
            await SendServiceRequestAsync( new ProgramService(AppState.webEssentials).SyncTaskSubflowToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
            _programData.ErrCode = programData.ErrCode;
            if (_programData.ErrCode == ErrorCode.OK)
            {
                programData.SubFlow.Name = _programData.SubFlow.Name;
                programData.SubFlow.Item = _programData.SubFlow.Item;
                programData.SubFlow.TaskType = _programData.SubFlow.TaskType;
                await OnClosed.InvokeAsync(programData.SubFlow);
            }
            else
            {
                Error = _programData.ErrCode.ToString();
            }
        }
    }
}