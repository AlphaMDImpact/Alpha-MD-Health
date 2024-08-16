using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class TaskPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO { RecordCount = -1 };
    private SubFlowModel _taskSubFlow;
    private ProgramService _programService;
    private List<ButtonActionModel> _messageButtonActions;
    private OptionModel _selectedType;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _isDataUpdated;
    private bool _isEditable;
    private bool _isFirstCall = false;
    private string _selectedItemID;

    /// <summary>
    /// Task id parameter
    /// </summary>
    [Parameter]
    public long TaskID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _programService = new ProgramService(AppState.webEssentials);
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _programData.Task = new TaskModel { TaskID = TaskID };
        await SendServiceRequestAsync(_programService.SyncTasksFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.TaskAddEdit.ToString());
            _programData.SubFlows ??= new List<SubFlowModel>();
            _selectedItemID = (_programData.Task?.ItemID == 0) ? string.Empty  : _programData.Task?.ItemID.ToString() ?? string.Empty;
            if(string.IsNullOrEmpty(_selectedItemID))
            {
                _isFirstCall = true;
            }
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_programData.ErrCode.ToString());
        }
    }

    private void OnSubFlowAddEditClick(SubFlowModel subFlow)
    {
        Success = Error = string.Empty;
        _taskSubFlow = subFlow ?? new SubFlowModel();
        _taskSubFlow.TaskID = TaskID;
    }

    private async Task OnTaskTypeChangedAsync()
    {
        var selected = _programData.TaskTypes?.FirstOrDefault(x => x.IsSelected);
        if (selected != _selectedType)
        {
            _selectedType = selected;
            if (_selectedType?.OptionID > 0)
            {
                Success = Error = string.Empty;
                long itemId = _programData.Items.Any(x => x.IsSelected) ? _programData.Items.FirstOrDefault(x => x.IsSelected).OptionID : 0;
                ProgramDTO programData = new ProgramDTO { Program = new ProgramModel { ProgramID = -1, Name = _selectedType.GroupName } };
                await SendServiceRequestAsync(_programService.SyncTaskItemsFromServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
                _programData.ErrCode = programData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    _programData.Items = itemId == 0 ? programData.Items : programData.Items.Select(item =>
                    {
                        item.IsSelected = (item.OptionID == itemId);
                        return item;
                    }).ToList();
                    if (_isFirstCall)
                    {
                        _selectedItemID = string.Empty;                       
                    }
                    _isFirstCall = true;
                }
                else
                {
                    Error = programData.ErrCode.ToString();
                }
            }
        }
    }

    private void OnTaskSubFlowClosedEventCallback(SubFlowModel subFlow)
    {
        if (subFlow != null)
        {
            if (_programData.SubFlows.Any(x => x.TaskSubFlowID == subFlow.TaskSubFlowID))
            {
                _programData.SubFlows.RemoveAll(x => x.TaskSubFlowID == subFlow.TaskSubFlowID);
            }
            if (subFlow.IsActive)
            {
                _programData.SubFlows.Add(subFlow);
            }
            _isDataUpdated = true;
        }
        _taskSubFlow = null;
    }

    private void OnCancelClicked()
    {
        OnClose.InvokeAsync(_isDataUpdated ? ErrorCode.OK.ToString() : string.Empty);
    }

    private void OnDeleteClicked()
    {
        Success = Error = string.Empty;
        _messageButtonActions = new List<ButtonActionModel>
        {
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey =ResourceConstants.R_OK_ACTION_KEY  },
            new ButtonActionModel{ ButtonID = Constants.NUMBER_TWO, ButtonResourceKey =ResourceConstants.R_CANCEL_ACTION_KEY  },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnDeleteActionClickAsync(object sequenceNo)
    {
        _hideDeletedConfirmationPopup = true;
        if (Convert.ToInt64(sequenceNo) == 1)
        {
            ProgramDTO programData = new ProgramDTO
            {
                Task = _programData.Task,
            };
            programData.Task.IsActive = false;
            await SyncWebMenuGroupToServerAsync(programData).ConfigureAwait(false);
        }
    }

    private async Task OnSaveActionClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _programData.Task.ItemID = Convert.ToInt64(_selectedItemID);
            _programData.Task.TaskType = _programData.TaskTypes.FirstOrDefault(x => x.IsSelected)?.GroupName ?? string.Empty;
            _programData.IsActive = true;
            ProgramDTO programData = new ProgramDTO
            {
                Task = _programData.Task,
                LanguageDetails = _programData.LanguageDetails,
            };
            programData.Task.IsActive = true;
            await SyncWebMenuGroupToServerAsync(programData);
        }
    }

    private async Task SyncWebMenuGroupToServerAsync(ProgramDTO programData)
    {
        await SendServiceRequestAsync(_programService.SyncTaskToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
        _programData.ErrCode = programData.ErrCode;
        if (_programData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_programData.ErrCode.ToString());
        }
        else
        {
            Error = _programData.ErrCode.ToString();
        }
    }


    private List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel { DataField = nameof(ProgramDetails.Name), ResourceKey = ResourceConstants.R_TASK_NAME_KEY},
        new TabDataStructureModel { DataField = nameof(ProgramDetails.Description), ResourceKey = ResourceConstants.R_TASK_DESCRIPTION_KEY, IsRequired = false }
    };


    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(SubFlowModel.TaskSubFlowID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(SubFlowModel.SubFlowID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(SubFlowModel.Name),DataHeader=ResourceConstants.R_SUBFLOW_NAME_KEY},
            new TableDataStructureModel{DataField=nameof(SubFlowModel.TaskType),DataHeader=ResourceConstants.R_TASK_TYPE_KEY},
            new TableDataStructureModel{DataField=nameof(SubFlowModel.Item),DataHeader=ResourceConstants.R_ITEM_KEY},
        };
    }
}