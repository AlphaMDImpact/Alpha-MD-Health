using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class InstructionPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO { RecordCount = -1};
    private List<ButtonActionModel> _popupActions;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _isEditable;

    /// <summary>
    /// Instruction id parameter
    /// </summary>
    [Parameter]
    public long InstructionID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _programData.Instruction= new InstructionModel { InstructionID= InstructionID }; 
        await SendServiceRequestAsync( new ProgramService(AppState.webEssentials).SyncInstructionsFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if(_programData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.InstructionAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_programData.ErrCode.ToString());
        }
    }

    private readonly List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel
        {
            DataField=nameof(ProgramDetails.Name),
            ResourceKey= ResourceConstants.R_INSTRUCTION_NAME_KEY,
        },
        new TabDataStructureModel
        {
            DataField = nameof(ProgramDetails.Description),
            ResourceKey = ResourceConstants.R_INSTRUCTION_DESCRIPTION_KEY,
        },
    };

    private void OnCancelClickedAsync()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _popupActions = new List<ButtonActionModel> {
                new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
                new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnActionClickedAsync(object sequenceNo)
    {
        _hideDeletedConfirmationPopup = true;
        Success = Error = string.Empty;
        if (sequenceNo != null)
        {
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _programData.IsActive = _programData.Instruction.IsActive = false;
                await SaveInstructionsAsync(_programData).ConfigureAwait(false);
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
                Instruction = new InstructionModel { InstructionID = InstructionID },
                LanguageDetails = _programData.LanguageDetails,
            };
            programData.Instruction.IsActive = true;
            await SaveInstructionsAsync(programData);
        }
    }

    private async Task SaveInstructionsAsync(ProgramDTO programData)
    {
        await SendServiceRequestAsync( new ProgramService(AppState.webEssentials).SyncInstructionsToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
        if (programData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(programData.ErrCode.ToString());
        }
        else
        {
            Error = programData.ErrCode.ToString();
        }
    }
}