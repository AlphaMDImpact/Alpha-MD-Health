using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class PatientTrackerPage : BasePage
{
    private readonly TrackerDTO _trackerData = new TrackerDTO { RecordCount = -1, PatientTracker = new PatientTrackersModel() };
    private List<ButtonActionModel> _actionData;
    private bool _hideConfirmationPopup = true;
    private bool _isEditable;
    private bool _showViewPage;

    /// <summary>
    /// Patient Tracker ID parameter
    /// </summary>
    [Parameter]
    public Guid PatientTrackerID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _trackerData.PatientTrackers = new List<PatientTrackersModel>();
        _trackerData.PatientTracker.PatientTrackerID = PatientTrackerID;
        await SendServiceRequestAsync(new PatientTrackerService(AppState.webEssentials).GetPatientTrackersAsync(_trackerData), _trackerData).ConfigureAwait(true);
        if (_trackerData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_trackerData.FeaturePermissions, AppPermissions.PatientTrackerAddEdit.ToString());
            if (_trackerData.PatientTracker.ProgramTrackerID > 0 || !_trackerData.PatientTracker.IsEdit)
            {
                _isEditable = false;
            }
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_trackerData.ErrCode.ToString());
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnDeleteButtonClicked()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
         };
        _hideConfirmationPopup = false;
    }

    private async Task DeletePatientTrackerPopUpCallbackAsync(object value)
    {
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                await SavePatientTrackerAsync(false);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        if (IsValid())
        {
            if (_trackerData.PatientTracker.FromDate.Value > _trackerData.PatientTracker.ToDate.Value)
            {
                Error = string.Format(CultureInfo.InvariantCulture,
                    LibResources.GetResourceValueByKey(_trackerData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    LibResources.GetResourceValueByKey(_trackerData.Resources, ResourceConstants.R_START_DATE_KEY),
                    LibResources.GetResourceValueByKey(_trackerData.Resources, ResourceConstants.R_END_DATE_KEY));
                return;
            }
            await SavePatientTrackerAsync(true);
        }
    }

    private async Task SavePatientTrackerAsync(bool isActive)
    {
        TrackerDTO patientTrackerData = new TrackerDTO
        {
            PatientTracker = new PatientTrackersModel
            {
                PatientTrackerID = PatientTrackerID,
                TrackerID = (short)(_trackerData.TrackerTypes.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0),
                UserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0),
                FromDate = _trackerData.PatientTracker.FromDate.Value.ToUniversalTime(),
                ToDate = _trackerData.PatientTracker.ToDate.Value.ToUniversalTime(),
                IsActive = isActive,
            }
        };
        await SendServiceRequestAsync(new PatientTrackerService(AppState.webEssentials).SavePatientTrackersAsync(patientTrackerData), patientTrackerData).ConfigureAwait(true);
        if (patientTrackerData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(patientTrackerData.ErrCode.ToString());
        }
        else
        {
            Error = patientTrackerData.ErrCode.ToString();
        }
    }

    private void OnTrackerViewClicked()
    {
        Success = Error = string.Empty;
        _showViewPage = true;
    }

    private void OnTrackerViewClosed(string message)
    {
        _showViewPage = false;
        Success = Error = string.Empty;
        if (message == ErrorCode.OK.ToString())
        {
            Success = message;
        }
        else
        {
            Error = message;
        }
    }
}