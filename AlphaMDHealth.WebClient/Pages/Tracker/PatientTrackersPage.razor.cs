using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class PatientTrackersPage : BasePage
{
    private TrackerDTO _patientTrackerData;
    private Guid _patientTrackerID;
    private bool _isDashboardView;
    public object _pageData;

    [Parameter]
    public Guid PatientTrackerID
    {
        get { return _patientTrackerID; }
        set
        {
            if (_patientTrackerID != value)
            {
                if (_isDashboardView || _patientTrackerID == Guid.Empty)
                {
                    if (_patientTrackerData != null)
                    {
                        _patientTrackerData.RecordCount = default;
                        _patientTrackerData.PatientTracker.TrackerName = default;
                    }
                    ShowDetailPage = true;
                }
                _patientTrackerID = value;
            }
        }
    }

    /// <summary>
    /// External data to load
    /// </summary>
    [Parameter]
    public object PageData
    {
        get
        {
            return _pageData;
        }
        set
        {
            _pageData = value;
            if (_pageData != null)
            {
                _patientTrackerData = _pageData as TrackerDTO;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (PageData == null)
        {
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            _isDashboardView = false;
            _patientTrackerData = PageData as TrackerDTO;
            _isDataFetched = true;
        }
    }

    private async Task GetDataAsync()
    {
        _patientTrackerData = new TrackerDTO() { PatientTracker = new PatientTrackersModel() };
        if (Parameters?.Count > 0)
        {
            _patientTrackerData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(TrackerDTO.RecordCount)));
            _patientTrackerData.PatientTracker.TrackerName = GenericMethods.MapValueType<string>(GetParameterValue(nameof(PatientTrackersModel.TrackerName)));
        }
        _isDashboardView = !string.IsNullOrWhiteSpace(_patientTrackerData.PatientTracker?.TrackerName) || _patientTrackerData.RecordCount > 0;
        _patientTrackerData.PatientTrackers = new List<PatientTrackersModel>();
        await SendServiceRequestAsync(new PatientTrackerService(AppState.webEssentials).GetPatientTrackersAsync(_patientTrackerData), _patientTrackerData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        var tblStructure = new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(PatientTrackersModel.PatientTrackerID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{BorderColorDataField=nameof(PatientTrackersModel.ProgramColor)},
            new TableDataStructureModel{HasImage=true,ImageFieldType=FieldTypes.CircleWithBorderImageControl,ImageHeight=AppImageSize.ImageSizeM,ImageWidth=AppImageSize.ImageSizeM, ImageIcon=nameof(PatientTrackersModel.LeftDefaultIcon) , ImageSrc= nameof(PatientTrackersModel.ImageName), IsSortable= false},
            new TableDataStructureModel{DataField=nameof(PatientTrackersModel.TrackerName),DataHeader=ResourceConstants.R_TRACKER_NAME_TEXT_KEY},
            new TableDataStructureModel{DataField=nameof(PatientTrackersModel.CurrentValueDisplayFormatString),DataHeader=ResourceConstants.R_TRACKER_CURRENT_VALUE_KEY,IsHtmlTag=true},
        };
        if (!_isDashboardView)
        {
            tblStructure.Add(new TableDataStructureModel { DataField = nameof(PatientTrackersModel.FromDateDisplayFormatString), DataHeader = ResourceConstants.R_START_DATE_KEY, Formatter = _patientTrackerData.PatientTracker?.FromDateDisplayFormatString });
            tblStructure.Add(new TableDataStructureModel { DataField = nameof(PatientTrackersModel.ToDateDisplayFormatString), DataHeader = ResourceConstants.R_END_DATE_KEY, Formatter = _patientTrackerData.PatientTracker?.ToDateDisplayFormatString });
        }
        return tblStructure;
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.PatientTrackersView.ToString()).ConfigureAwait(false);
    }

    private async Task OnAddEditClick(PatientTrackersModel tracker)
    {
        Success = Error = string.Empty;
        if (_isDashboardView)
        {
            await NavigateToAsync(AppPermissions.PatientTrackersView.ToString(), (tracker == null ? Guid.Empty : tracker.PatientTrackerID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _patientTrackerID = tracker == null ? Guid.Empty : tracker.PatientTrackerID;
            ShowDetailPage = true;
        }
    }

    private async Task CloseEventCallbackAsync(string errorCode)
    {
        ShowDetailPage = false;
        Success = Error = string.Empty;
        if (PageData == null)
        {
            await OnViewAllClickedAsync();
        }
        if (errorCode == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = errorCode;
            _patientTrackerID = Guid.Empty;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = errorCode;
        }
    }
}