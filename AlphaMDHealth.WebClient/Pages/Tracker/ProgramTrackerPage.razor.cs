using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class ProgramTrackerPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO { ProgramTracker = new ProgramTrackerModel() };
    private List<ButtonActionModel> _actionData;
    private ProgramService _programService;
    private bool _hideConfirmationPopup = true;
    private double? _assignAfterDays;
    private double? _assignForDays;
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
    ///  Program Duration
    /// </summary>
    [Parameter]
    public int ProgramDuration { get; set; }

    /// <summary>
    /// selected Program Tracker ID
    /// </summary>
    [Parameter]
    public long ProgramTrackerID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _programService = new ProgramService(AppState.webEssentials);
        _programData.ProgramTracker.ProgramID = ProgramID;
        _programData.ProgramTracker.ProgramTrackerID = ProgramTrackerID;
        await SendServiceRequestAsync(_programService.SyncProgramTrackersFromServer(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            _isEditable = IsSynced && LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.ProgramTrackerAddEdit.ToString());
            if (_programData.ProgramTracker.ProgramTrackerID > 0)
            {
                _assignAfterDays = _programData.ProgramTracker.AssignAfterDays;
                _assignForDays = _programData.ProgramTracker.AssignForDays;

                bool isTrackerDeletedFromMaster = _programData.TrackerTypes.FirstOrDefault(x => x.OptionID == _programData.ProgramTracker.TrackerID)?.ParentOptionText == false.ToString();
                if (isTrackerDeletedFromMaster)
                {
                    _isEditable = false;
                }
            }
            _programData.TrackerTypes = _programData.TrackerTypes.FindAll(x => x.ParentOptionText == true.ToString() || x.OptionID == _programData.ProgramTracker?.TrackerID);
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
                _programData.ProgramTracker.IsActive = false;
                await SaveProgramTrackerAsync().ConfigureAwait(true);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _programData.ProgramTracker.AssignAfterDays = Convert.ToInt16(_assignAfterDays, CultureInfo.InvariantCulture);
            _programData.ProgramTracker.AssignForDays = Convert.ToInt16(_assignForDays, CultureInfo.InvariantCulture);
            var result = _programService.ValidateAssingAfterAndShowForDate(ProgramDuration, _programData.ProgramTracker.AssignAfterDays, _programData.ProgramTracker.AssignForDays, _programData.Resources);
            if (!(result.isValidAssignAfterDay && result.isValidShowForAfterDay))
            {
                Error = string.Format(CultureInfo.InvariantCulture, result.errorCode.ToString()).ToString();
                return;
            }
            _programData.ProgramTracker.PatientTrackerID = _programData.ProgramTracker.PatientTrackerID == Guid.Empty ? GenericMethods.GenerateGuid() : _programData.ProgramTracker.PatientTrackerID;
            _programData.ProgramTracker.ProgramTrackerID = ProgramTrackerID;
            _programData.ProgramTracker.ProgramID = ProgramID;
            _programData.ProgramTracker.TrackerID = (short)(_programData.TrackerTypes.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0);
            _programData.ProgramTracker.ValueAddedBy = (short)(_programData.ValueAddedByType.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0);
            _programData.ProgramTracker.IsActive = true;
            await SaveProgramTrackerAsync().ConfigureAwait(true);
        }
    }

    private async Task SaveProgramTrackerAsync()
    {
        _programData.ErrCode = ErrorCode.OK;
        await SendServiceRequestAsync(_programService.SyncProgramTrackerToServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
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