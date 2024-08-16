using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class TrackersAddEdit : BasePage
{
    private readonly TrackerDTO _trackerData = new() { RecordCount = -1 };
    private List<ButtonActionModel> _popupActions;
    private List<OptionModel> _tabsData;
    private OptionModel _selectedTab;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _showRangeAddEdit;
    private short _trackerRangeID;
    private long _trackerID;
    private bool _isEditable = false;

    /// <summary>
    /// Tracker ID
    /// </summary>
    [Parameter]
    public short TrackerID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _trackerData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _trackerData.LanguageID = AppState.SelectedLanguageID;
        _trackerData.Tracker = new TrackersModel
        {
            TrackerID = TrackerID
        };
        _trackerData.TrackerRange = new TrackerRangeModel
        {
            TrackerID = TrackerID,
            TrackerRangeID = 0
        };
        _trackerData.ErrCode = ErrorCode.OK;
        await SendServiceRequestAsync(new TrackerService(AppState.webEssentials).SyncTrackersFromServerAsync(_trackerData, CancellationToken.None), _trackerData).ConfigureAwait(true);
        if (_trackerData.ErrCode == ErrorCode.OK)
        {
            _tabsData = new List<OptionModel>();
            AddTabAsPerPermission();
            _selectedTab = _tabsData.FirstOrDefault();
            _trackerData.TrackerRanges ??= new List<TrackerRangeModel>();
            _isEditable = LibPermissions.HasPermission(_trackerData.FeaturePermissions, AppPermissions.TrackersAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(null).ConfigureAwait(true);
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(null);
    }

    private void OnRemoveClick()
    {
        _popupActions = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
         };
        _hideDeletedConfirmationPopup = false;
    }

    protected void OnTabChanged(object? value)
    {
        _selectedTab = _tabsData.FirstOrDefault(x => x.OptionID == Convert.ToInt64(value ?? -1));
    }

    private async Task OnDeleteButtonClickAsync(object sequenceNo)
    {
        if (sequenceNo != null)
        {
            _hideDeletedConfirmationPopup = true;
            Success = Error = string.Empty;
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _trackerData.Tracker.IsActive = false;
                await SaveTrackersAsync();
            }
        }
    }

    private void OnTrackerRangeAddEditClick(object trackerRangeModel)
    {
        var trackerRange = (TrackerRangeModel)trackerRangeModel;
        _showRangeAddEdit = true;
        _trackerID = trackerRangeModel == null ? 0 : trackerRange.TrackerID;
        _trackerRangeID = trackerRangeModel == null ? (short)0 : trackerRange.TrackerRangeID;
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Error = string.Empty;
        if (IsValid())
        {
            _trackerData.Tracker.TrackerID = TrackerID;
            _trackerData.Tracker.TrackerTypeID = (short)_trackerData.TrackerTypes.FirstOrDefault(x => x.IsSelected).OptionID;
            _trackerData.Tracker.TrackerType = _trackerData.TrackerTypes.FirstOrDefault(x => x.IsSelected).OptionText;
            var languageID = (byte)AppState.webEssentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            _trackerData.Tracker.TrackerName = _trackerData.TrackersI18N.FirstOrDefault(x => x.TrackerID == TrackerID && x.LanguageID == languageID)?.TrackerName;
            _trackerData.Tracker.IsActive = true;
            _trackerData.Tracker.Ranges = Convert.ToString(_trackerData.TrackerRanges.Count);
            await SaveTrackersAsync().ConfigureAwait(true);
        }
    }

    private async Task SaveTrackersAsync()
    {
        _showRangeAddEdit = false;
        Success = Error = string.Empty;
        _trackerData.ErrCode = ErrorCode.OK;
        await SendServiceRequestAsync(new TrackerService(AppState.webEssentials).SyncTrackerToServerAsync(_trackerData, CancellationToken.None), _trackerData).ConfigureAwait(true);
        if (_trackerData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_trackerData.ErrCode.ToString());
        }
        else
        {
            Error = _trackerData.ErrCode.ToString();
        }
    }

    private async Task OnRangesAddEditClosed(string isDataUpdated)
    {
        _showRangeAddEdit = false;
        _trackerRangeID = 0;
        Success = Error = string.Empty;
        if (isDataUpdated == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = isDataUpdated;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = isDataUpdated;
        }
    }

    private void AddTabAsPerPermission()
    {
        if (_tabsData != null)
        {
            GenerateTabBasedOnPermission(AppPermissions.TrackerRangesView, 1);
        }
    }

    private void GenerateTabBasedOnPermission(AppPermissions permission, long sequence)
    {
        if (LibPermissions.HasPermission(_trackerData.FeaturePermissions, permission.ToString()))
        {
            _tabsData.Add(new OptionModel { GroupName = permission.ToString(), SequenceNo = sequence, OptionText = LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, permission.ToString()), });
        }
    }

    private static List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{ DataField=nameof(TrackerRangeModel.TrackerID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel{ DataField=nameof(TrackerRangeModel.TrackerRangeID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel{ DataField=nameof(TrackerRangeModel.FromValue), DataHeader=ResourceConstants.R_FROM_DAY_KEY },
            new TableDataStructureModel{ DataField=nameof(TrackerRangeModel.ToValue), DataHeader=ResourceConstants.R_FOR_DAYS_KEY },
            new TableDataStructureModel{ DataField=nameof(TrackerRangeModel.CaptionText), DataHeader=ResourceConstants.R_CAPTION_KEY},
        };
    }

    private readonly List<TabDataStructureModel> DataFormatter = new()
    {
        new TabDataStructureModel {
            DataField= nameof(TrackersI18NModel.TrackerName),
            ResourceKey= ResourceConstants.R_TRACKER_NAME_TEXT_KEY,
            IsRequired = true
        },
    };
}