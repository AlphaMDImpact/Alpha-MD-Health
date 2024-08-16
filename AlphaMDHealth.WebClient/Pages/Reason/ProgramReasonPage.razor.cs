using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ProgramReasonPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO
    {
        Reason = new ReasonModel(),
        RecordCount = -1,
    };
    private List<ButtonActionModel> _actionData;
    private bool _hideConfirmationPopup = true;
    private bool _isEditable;

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
    /// selected Program Reason ID
    /// </summary>
    [Parameter]
    public long ProgramReasonID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _programData.Reason.ProgramID = ProgramID;
        _programData.Reason.ProgramReasonID = ProgramReasonID;
        await SendServiceRequestAsync(new ReasonService(AppState.webEssentials).SyncProgramReasonsFromServer(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            _isEditable = IsSynced && LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.ProgramReasonAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_programData.ErrCode.ToString()).ConfigureAwait(true);
        }
    }

    private void OnCancelClick()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task DeletePopUpCallbackAsync(object value)
    {
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                _programData.Reason.IsActive = false;
                await SaveProgramReasonAsync().ConfigureAwait(true);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _programData.Reason.ProgramReasonID = ProgramReasonID;
            _programData.Reason.ProgramID = ProgramID;
            _programData.Reason.ReasonID = (byte)(_programData?.ReasonOptionList.FirstOrDefault(x => x.IsSelected).OptionID);
            _programData.Reason.IsActive = true;
            await SaveProgramReasonAsync().ConfigureAwait(true);
        }
    }

    private async Task SaveProgramReasonAsync()
    {
        _programData.ErrCode = ErrorCode.OK;
        await SendServiceRequestAsync(new ReasonService(AppState.webEssentials).SyncProgramReasonsToServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_programData.ErrCode.ToString());
        }
        else
        {
            Error = _programData.ErrCode.ToString();
        }
    }
}