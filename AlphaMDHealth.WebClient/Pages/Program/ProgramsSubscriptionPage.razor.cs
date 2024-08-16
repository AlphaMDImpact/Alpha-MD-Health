using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class ProgramsSubscriptionPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO() { RecordCount = -2, Program = new ProgramModel() };
    private List<ButtonActionModel> _confirmationActions;
    private string _programDescription;
    private bool _hideConfirmationPopup = true;

    protected override async Task OnInitializedAsync()
    {
        await RenderPageDataAsync().ConfigureAwait(true);
    }

    private async Task RenderPageDataAsync()
    {
        await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncProgramsFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            SetSelectedProgramDetails(_programData.Program.ProgramID);
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_programData.ErrCode.ToString()).ConfigureAwait(true);
        }
    }

    private void SetSelectedProgramDetails(long optionId)
    {
        _programDescription = _programData.Items.FirstOrDefault(x => x.OptionID == optionId)?.ParentOptionText;
    }

    private void OnProgramSelectionChange(object optionModel)
    {
        if (!string.IsNullOrWhiteSpace(optionModel.ToString()))
        {
            long optionId = Convert.ToInt64(optionModel);
            _programData.Program.ProgramID = optionId;
            SetSelectedProgramDetails(optionId);
        }
    }

    private void OnCanceledClickAsync()
    {
        OnClose.InvokeAsync(ResourceConstants.R_CANCEL_ACTION_KEY);
    }

    private void OnSubscribeClick()
    {
        Success = Error = string.Empty;
        if (IsValid()) 
        {
            _confirmationActions = new List<ButtonActionModel>
            {
                new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey =ResourceConstants.R_OK_ACTION_KEY  },
                new ButtonActionModel{ ButtonID = Constants.NUMBER_TWO, ButtonResourceKey =ResourceConstants.R_CANCEL_ACTION_KEY  },
            };
            _hideConfirmationPopup = false;
        }
    }

    private async Task OnSubscribeActionClickAsync(object sequenceNo)
    {
        _hideConfirmationPopup = true;
        if (Convert.ToString(sequenceNo) == Constants.NUMBER_ONE)
        {
            await OnSubscribedButtonClickedAsync().ConfigureAwait(false);
        }
    }

    private async Task OnSubscribedButtonClickedAsync()
    {
        await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncSubscribeProgramToServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        _programData.ErrCode = _programData.ErrCode;
        if (_programData.ErrCode == ErrorCode.OK)
        {
            _isDataFetched = true;
            await OnClose.InvokeAsync(ResourceConstants.R_PROGRAM_SUBSCRIPTION_KEY);
        }
        else
        {
            Error = _programData.ErrCode.ToString();
        }
    }
}