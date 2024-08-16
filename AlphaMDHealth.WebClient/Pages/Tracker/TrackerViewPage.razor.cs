using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class TrackerViewPage : BasePage
{
    private readonly TrackerDTO _patientTrackerData = new TrackerDTO { RecordCount = -3, PatientTracker = new PatientTrackersModel() };
    private PatientTrackerService _trackerService;

    /// <summary>
    /// Patient Tracker ID parameter
    /// </summary>
    [Parameter]
    public Guid PatientTrackerID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _trackerService = new PatientTrackerService(AppState.webEssentials);
        _patientTrackerData.PatientTrackers = new List<PatientTrackersModel>();
        _patientTrackerData.PatientTracker.PatientTrackerID = PatientTrackerID;
        await SendServiceRequestAsync(_trackerService.GetPatientTrackersAsync(_patientTrackerData), _patientTrackerData).ConfigureAwait(true);
        if (_patientTrackerData.ErrCode == ErrorCode.OK)
        {
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_patientTrackerData.ErrCode.ToString());
        }
    }

    private void OnCancelClick()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Error = Success = string.Empty;
        if (IsValid())
        {
            _patientTrackerData.PatientTrackerValue = new PatientTrackersValuesModel();
            _patientTrackerData.PatientTrackerValue.PatientTrackerID = PatientTrackerID;
            _patientTrackerData.PatientTrackerValue.CurrentValue = GenericMethods.ConvertToIsoDatetimeOffset(_patientTrackerData.PatientTracker.CurrentValueInDate.Value);
            _patientTrackerData.PatientTrackerValue.IsActive = true;
            _patientTrackerData.PatientTrackerValue.LastModifiedON = GenericMethods.GetUtcDateTime;
            await SavePatientTrackerValueAsync().ConfigureAwait(true);
        }
    }

    private async Task SavePatientTrackerValueAsync()
    {
        if (await _trackerService.ValidatePatientTrackerValue(_patientTrackerData))
        {
            await SendServiceRequestAsync(_trackerService.SavePatientTrackerValueToServerAsync(_patientTrackerData, CancellationToken.None), _patientTrackerData).ConfigureAwait(true);
            if (_patientTrackerData.ErrCode == ErrorCode.OK)
            {
                await OnClose.InvokeAsync(_patientTrackerData.ErrCode.ToString());
            }
            else
            {
                Error = _patientTrackerData.ErrCode.ToString();
            }
        }
        else
        {                                                   
            Error = string.Format(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(_patientTrackerData.Resources, ResourceConstants.R_RANGE_VALUE_VALIDATION_KEY)
                , _patientTrackerData.PatientTracker.FromDateDisplayFormatString, _patientTrackerData.PatientTracker.ToDateDisplayFormatString).ToString();
        }
    }
}