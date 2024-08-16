using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class TrackerRangeAddEdit : BasePage
{
    private readonly TrackerDTO _trackerData = new() { TrackerRange = new TrackerRangeModel(), RecordCount = -2 };
    private List<ButtonActionModel> _popupActions;
    private bool _hideConfirmationPopup = true;
    private double? _fromDay;
    private double? _forDays;
    private bool _isEditable;

    /// <summary>
    /// Tracker ID
    /// </summary>
    [Parameter]
    public short TrackerID { get; set; }

    /// <summary>
    /// Tracker Range ID
    /// </summary>
    [Parameter]
    public short TrackerRangeID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync();
    }

    private async Task GetDataAsync()
    {
        _trackerData.TrackerRange = new TrackerRangeModel
        {
            TrackerID = TrackerID,
            TrackerRangeID = TrackerRangeID
        };
        _trackerData.Tracker = new TrackersModel
        {
            TrackerID = TrackerID,
        };
        _trackerData.ErrCode = ErrorCode.OK;
        await SendServiceRequestAsync(new TrackerService(AppState.webEssentials).SyncTrackersFromServerAsync(_trackerData, CancellationToken.None), _trackerData).ConfigureAwait(true);
        if (_trackerData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_trackerData.FeaturePermissions, AppPermissions.TrackerRangesAddEdit.ToString());
            if (_trackerData.TrackerRange.TrackerID > 0 || _trackerData.TrackerRange.TrackerRangeID != 0)
            {
                _fromDay = _trackerData.TrackerRange.FromValue;
                _forDays = _trackerData.TrackerRange.ToValue;
            }
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(null).ConfigureAwait(true);
        }
    }

    private void OnCancelClick()
    {
        OnClose.InvokeAsync(null);
    }

    private void OnRemoveClick()
    {
        _popupActions = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
         };
        _hideConfirmationPopup = false;
    }

    private async Task DeletePopUpCallbackAsync(object sequenceNo)
    {
        if (sequenceNo != null)
        {
            _hideConfirmationPopup = true;
            Success = Error = string.Empty;
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _trackerData.TrackerRange.IsActive = false;
                await SaveTrackerRangeAsync().ConfigureAwait(true);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _trackerData.TrackerRange.TrackerRangeID = TrackerRangeID;
            _trackerData.TrackerRange.CaptionText = _trackerData.TrackerRangesI18N.FirstOrDefault(x => x.TrackerRangeID == _trackerData.TrackerRange.TrackerRangeID).CaptionText;
            _trackerData.TrackerRange.TrackerID = TrackerID;
            _trackerData.TrackerRange.FromValue = (int)_fromDay;
            _trackerData.TrackerRange.ToValue = (int)_forDays;
            _trackerData.TrackerRange.IsActive = true;
            await SaveTrackerRangeAsync().ConfigureAwait(true);
        }
    }

    private readonly List<TabDataStructureModel> DataFormatter = new()
    {
        new TabDataStructureModel
        {
            DataField= nameof(TrackerRangesI18N.CaptionText),
            ResourceKey= ResourceConstants.R_CAPTION_KEY,
            //IsRequired = true
        },
        new TabDataStructureModel
        {
            DataField = nameof(TrackerRangesI18N.InstructionsText),
            ResourceKey = ResourceConstants.R_DESCRIPTION_TEXT_KEY,
            //MaxFileDimensionSize = LibSettings.GetSettingValueByKey(_trackerData.Settings,SettingsConstants.S_SMALL_IMAGE_RESOLUTION_KEY),
            //MaxFileUploadSize = LibSettings.GetSettingValueByKey(_trackerData.Settings, SettingsConstants.S_MAX_FILE_UPLOAD_KEYS),
            //IsRequired = true
        },
    };

    private async Task SaveTrackerRangeAsync()
    {
        Success = Error = string.Empty;
        _trackerData.ErrCode = ErrorCode.OK;
        await SendServiceRequestAsync(new TrackerService(AppState.webEssentials).SyncTrackerRangesToServerAsync(_trackerData, CancellationToken.None), _trackerData).ConfigureAwait(true);
        if (_trackerData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_trackerData.ErrCode.ToString());
        }
        else
        {
            Error = _trackerData.ErrCode.ToString();
        }
    }
}