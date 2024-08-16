using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class TrackersPage : BasePage
{
    private readonly TrackerDTO _trackerData = new() { Tracker = new TrackersModel() };
    private bool _isDashboardView;
    private short _trackerID;

    /// <summary>
    /// TrackerID parameter
    /// </summary>
    [Parameter]
    public short TrackerID
    {
        get { return _trackerID; }
        set
        {
            if (_trackerID != value)
            {
                if (_trackerData.RecordCount > 0 || _trackerID == 0)
                {
                    _trackerData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _trackerID = value;
            }
        }
    }
    
    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

        private async Task GetDataAsync()
        {
            _trackerData.LanguageID = AppState.SelectedLanguageID;
            _trackerData.Tracker = new TrackersModel
            {
                TrackerID = 0
            };
            _trackerData.TrackerRange = new TrackerRangeModel
            {
                TrackerID = 0,
                TrackerRangeID = 0
            };
            if (Parameters?.Count > 0)
            {
                _trackerData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            }
            await SendServiceRequestAsync(new TrackerService(AppState.webEssentials).SyncTrackersFromServerAsync(_trackerData, CancellationToken.None), _trackerData).ConfigureAwait(true);
            _isDashboardView = _trackerData.RecordCount > 0;
            _isDataFetched = true;
        }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        var list = new List<TableDataStructureModel>
        {
            new TableDataStructureModel{ DataField=nameof(TrackersModel.TrackerID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel{ DataField=nameof(TrackersModel.TrackerIdentifier), DataHeader=ResourceConstants.R_TRACKER_IDENTIFIER_KEY},
            new TableDataStructureModel{ DataField=nameof(TrackersModel.TrackerName), DataHeader=ResourceConstants.R_TRACKER_NAME_TEXT_KEY },
        };
        if (!_isDashboardView)
        {
            list.Add(new TableDataStructureModel { DataField = nameof(TrackersModel.TrackerType), DataHeader = ResourceConstants.R_TRACKER_TYPE_KEY });
            list.Add(new TableDataStructureModel { DataField = nameof(TrackersModel.Ranges), DataHeader = ResourceConstants.R_RANGES_NAME_TEXT_KEY });
        }
        return list;
    }

    private async Task OnAddEditClick(TrackersModel trackerData)
    {
        Success = Error = string.Empty;
        if (_trackerData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.TrackersView.ToString(), (trackerData == null ? 0 : trackerData.TrackerID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _trackerID = trackerData == null ? (short)0 : trackerData.TrackerID;
            ShowDetailPage = true;
        }
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.TrackersView.ToString()).ConfigureAwait(true);
    }

    private async Task OnAddEditClosedAsync(string message)
    {
        ShowDetailPage = false;
        _trackerID = 0;
        Success = Error = string.Empty;
        if (message == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = message;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = message;
        }
    }
}