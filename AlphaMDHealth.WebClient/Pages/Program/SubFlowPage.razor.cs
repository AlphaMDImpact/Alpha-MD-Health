using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class SubFlowPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO { RecordCount = -1, SubFlow = new SubFlowModel() };
    private List<ButtonActionModel> _popupActions;
    private ProgramService _programService;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _isEditable;
    private double? _fromValue;
    private double? _toValue;
    private string _selectedItemID;

    /// <summary>
    /// Sub Flow ID parameter
    /// </summary>
    [Parameter]
    public long SubFlowID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _programService = new ProgramService(AppState.webEssentials);
        _programData.SubFlow = new SubFlowModel { SubFlowID = SubFlowID };
        await SendServiceRequestAsync(_programService.SyncSubFlowsFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.SubflowAddEdit.ToString());
            _fromValue = Convert.ToDouble(_programData.SubFlow.Value1);
            _toValue = Convert.ToDouble(_programData.SubFlow.Value2);
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_programData.ErrCode.ToString());
        }
    }

    private void OnOperationTypeChanged(object? e)
    {
        Success = Error = string.Empty;
        _programData.SubFlow.OperationType = _programData.OperationTypes?.FirstOrDefault(x => x.IsSelected)?.GroupName;
    }

    private async Task OnTaskTypeChangedAsync(object? e)
    {
        _programData.SubFlow.TaskType = _programData.TaskTypes?.FirstOrDefault(x => x.IsSelected)?.GroupName;
        long itemId = _programData.Items.Any(x => x.IsSelected) ? _programData.Items.FirstOrDefault(x => x.IsSelected).OptionID : 0;
        _programData.Items = new List<OptionModel>();
        ProgramDTO programData = new ProgramDTO
        {
            Program = new ProgramModel { ProgramID = -1, Name = _programData.SubFlow.TaskType }
        };
        await SendServiceRequestAsync(_programService.SyncTaskItemsFromServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
        if (programData.ErrCode == ErrorCode.OK)
        {
            _programData.Items = itemId == 0 ? programData.Items : programData.Items.Select(item =>
            {
                item.IsSelected = (item.OptionID == itemId); 
                return item;
            }).ToList();
        }
    }

    private void OnCancelClicked()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _popupActions = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnActionClickAsync(object sequenceNo)
    {
        _hideDeletedConfirmationPopup = true;
        Success = Error = string.Empty;
        if (sequenceNo != null)
        {
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                ProgramDTO programData = new ProgramDTO
                {
                    SubFlow = _programData.SubFlow,
                    IsActive = false
                };
                await SaveSubflowDataAsync(programData).ConfigureAwait(true);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        if (IsValid())
        {
            if (_fromValue <= 0 || _fromValue == null)
            {
                _programData.SubFlow.Value1 = 0;
            }
            else
            {
                _programData.SubFlow.Value1 = (float)_fromValue;
            }
            if (_toValue <= 0 || _toValue == null)
            {
                _programData.SubFlow.Value2 = 0;
            }
            else
            {
                _programData.SubFlow.Value2 = (float)_toValue;
            }

            if (_programData.SubFlow.OperationType == ResourceConstants.R_BETWEEN_KEY && _programData.SubFlow.Value2 <= _programData.SubFlow.Value1)
            {
                Error = string.Format(CultureInfo.InvariantCulture,
                    LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_FROM_KEY),
                    LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_TO_KEY));
                return;
            }
            if (_programData.SubFlow.TaskType == ResourceConstants.R_NOTIFICATION_KEY || _programData.SubFlow.TaskType == ResourceConstants.R_SMS_KEY || _programData.SubFlow.TaskType == ResourceConstants.R_EMAIL_KEY)
            {
                _programData.SubFlow.ItemID = _programData.ProgramSubFlows.First(x => x.IsSelected).OptionID;
                _programData.SubFlow.TemplateID = (short)_programData.Items.First(x => x.IsSelected).OptionID;
            }
            else
            {
                _programData.SubFlow.ItemID = _programData.Items.First(x => x.IsSelected).OptionID;
            }
            _programData.IsActive = true;
            await SaveSubflowDataAsync(_programData).ConfigureAwait(true);
        }
    }

    private async Task SaveSubflowDataAsync(ProgramDTO programData)
    {
        programData.ErrCode = ErrorCode.OK;
        await SendServiceRequestAsync(_programService.SyncSubFlowToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
        if (programData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(programData.ErrCode.ToString());
        }
        else
        {
            Error = programData.ErrCode.ToString();
        }
    }

    private readonly List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel
        {
            DataField=nameof(ProgramDetails.Name),
            ResourceKey= ResourceConstants.R_SUBFLOW_NAME_KEY,
        },
        new TabDataStructureModel
        {
            DataField = nameof(ProgramDetails.Description),
            ResourceKey = ResourceConstants.R_SUBFLOW_DESCRIPTION_KEY,
            IsRequired = true,
        },
    };
}