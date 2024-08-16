using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class ProgramCareGiverPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO { ProgramCareGiver = new CaregiverModel() };
    private List<ButtonActionModel> _actionData;
    private ProgramService _programService;
    private bool _hideConfirmationPopup = true;
    private double? _assignAfterDays;
    private double? _assignForDays;
    private bool _isEditable;

    /// <summary>
    /// Program ID
    /// </summary>
    [Parameter]
    public long ProgramID { get; set; }

    [Parameter]
    public int ProgramDuration { get; set; }

    /// <summary>
    /// selected caregiver data
    /// </summary>
    [Parameter]
    public long SelectedProgramCareGiverID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _programService = new ProgramService(AppState.webEssentials);
        _programData.ProgramCareGiver.ProgramID = ProgramID;
        _programData.ProgramCareGiver.ProgramCareGiverID = SelectedProgramCareGiverID;
        await SendServiceRequestAsync(_programService.SyncProgramCaregiversFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            _assignAfterDays = _programData.ProgramCareGiver.AssignAfterDays;
            _assignForDays = _programData.ProgramCareGiver.AssignForDays;
            _isEditable = LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.ProgramCaregiverAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(null).ConfigureAwait(true);
        }
    }

    private async void OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            ProgramDTO programData = new ProgramDTO
            {
                ProgramCareGiver = new CaregiverModel
                {
                    ProgramID = ProgramID,
                    ProgramCareGiverID = SelectedProgramCareGiverID,
                    IsActive = true,
                    CareGiverID = (long)(_programData.Items.FirstOrDefault(x => x.IsSelected)?.OptionID),
                    AssignAfterDays = Convert.ToInt16(_assignAfterDays, CultureInfo.InvariantCulture),
                    AssignForDays = Convert.ToInt16(_assignForDays, CultureInfo.InvariantCulture)
                }
            };
            var result = _programService.ValidateAssingAfterAndShowForDate(ProgramDuration, programData.ProgramCareGiver.AssignAfterDays, programData.ProgramCareGiver.AssignForDays, _programData.Resources);
            if (!(result.isValidAssignAfterDay && result.isValidShowForAfterDay))
            {
                Error = string.Format(CultureInfo.InvariantCulture, result.errorCode.ToString()).ToString();
                return;
            }
            await SendServiceRequestAsync(_programService.SyncProgramCaregiverToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
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

    private async Task OnCancelClickedAsync()
    {
       await OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task OnDeleteConfirmationPopUpClickedAsync(object value)
    {
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                _programData.ProgramCareGiver.IsActive = false;
                ProgramDTO programData = new ProgramDTO
                {
                    ProgramCareGiver = _programData.ProgramCareGiver
                };
                await SendServiceRequestAsync(_programService.SyncProgramCaregiverToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
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
    }
}