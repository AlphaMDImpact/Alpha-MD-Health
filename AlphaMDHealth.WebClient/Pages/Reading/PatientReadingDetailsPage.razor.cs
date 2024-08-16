using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

/// <summary>
/// PatientReading Details Page
/// </summary>
public partial class PatientReadingDetailsPage : BasePage
{
    private PatientReadingDTO _readingDetailsData;
    private ReadingService _readingService;
    private short _readingID;
    bool _isSyncing = false;
    private IList<ButtonActionModel> _actionButtons = null;

    /// <summary>
    /// Reading ID
    /// </summary>
    [Parameter]
    public short ReadingID
    {
        get { return _readingID; }
        set
        {
            if (_readingID != value)
            {
                _readingID = value;
                SetReadingID();
            }
        }
    }

    /// <summary>
    /// On AddEdit clicked event fired
    /// </summary>
    [Parameter]
    public Action<PatientReadingUIModel, bool> OnAddEditClick { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _isDataFetched = false;
        _readingService = new ReadingService(AppState.webEssentials);
        await GetChartDataAsync().ConfigureAwait(true);
    }

    private async void SetReadingID()
    {
        await GetDataAsync().ConfigureAwait(true);
        if (!_isSyncing)
        {
            StateHasChanged();
        }
    }

    private async Task GetChartDataAsync()
    {
        if (!_isSyncing)
        {
            _isSyncing = true;
            _readingDetailsData ??= new PatientReadingDTO
            {
                RecordCount = -2,
                Settings = AppState.MasterData.Settings,
                Resources = AppState.MasterData.Resources,
            };
            _readingDetailsData.ChartData = new ChartUIDTO { SelectedDuration = _readingDetailsData?.ChartData?.SelectedDuration , StartDate = _readingDetailsData?.ChartData?.StartDate , EndDate = _readingDetailsData?.ChartData?.EndDate};
            _readingDetailsData.ReadingID = _readingID;
            if (_readingDetailsData.ReadingID > 0)
            {
                await SendServiceRequestAsync(_readingService.GetPatientReadingsAsync(_readingDetailsData), _readingDetailsData).ConfigureAwait(true);
            }
            if (IsPatientMobileView)
            {
                _actionButtons ??= new List<ButtonActionModel>();
                if (IsAddAllowed())
                {
                    _actionButtons.Add(new ButtonActionModel
                    {
                        ButtonResourceKey = ResourceConstants.R_ADD_ACTION_KEY,
                        ButtonAction = () => { OnAddEditClick.Invoke(null, false);  },
                        Icon = ImageConstants.I_ADD_ICON_RESPONSIVE
                    });
                }
                _actionButtons.Add(new ButtonActionModel
                {
                    ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                    ButtonAction = () => { OnCancelClickedAsync(); },
                    Icon = ImageConstants.PATIENT_MOBILE_BACK_SVG
                });
            }
            _isDataFetched = true;
            _isSyncing = false;
        }
    }

    private AmhViewCellModel getViewCellModel()
    {
        return new AmhViewCellModel
        {
            ID = nameof(PatientReadingUIModel.PatientReadingID),
            LeftHeader = nameof(PatientReadingUIModel.ReadingValueText),
            LeftHeaderFieldType = FieldTypes.HtmlPrimaryLabelControl,
            LeftDescription = nameof(PatientReadingUIModel.ImageSource),
            LeftDescriptionFieldType = FieldTypes.HtmlPrimaryLabelControl,
            RightDescription = nameof(PatientReadingUIModel.ReadingDateTimeText),
        };
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        string headerText = string.Empty;
        if (_readingDetailsData?.ChartMetaData?.Count > 0)
        {
            headerText = _readingDetailsData.ChartMetaData[0].IsGroupValue
                ? _readingDetailsData?.ChartMetaData?[0].ReadingParent
                : _readingDetailsData?.ChartMetaData?[0].Reading;
        }
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(PatientReadingUIModel.PatientReadingID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(PatientReadingUIModel.ReadingDateTimeText), DataHeader=ResourceConstants.R_DATE_TIME_TEXT_KEY,
                    ImageFields= new List<string>{
                        nameof(PatientReadingUIModel.ReadingSourceIcon),
                        nameof(PatientReadingUIModel.ReadingMomentIcon),
                        nameof(PatientReadingUIModel.ReadingNotesIcon) } ,
                    ImageHeight = AppImageSize.ImageSizeXS ,
                    ImageWidth= AppImageSize.ImageSizeXS
            },
            new TableDataStructureModel{DataField=nameof(PatientReadingUIModel.ReadingValueText), DataHeaderValue= headerText, IsEndAligned = true , IsHtmlTag = true},
        };
    }

    private FieldTypes GetGraphFieldType()
    {
        var chartType = (FieldTypes)_readingDetailsData.ChartMetaData.FirstOrDefault().ChartType;
        return chartType switch
        {
            FieldTypes.LineGraphControl => FieldTypes.LineGraphControl,
            _ => FieldTypes.BarGraphControl,
        };
    }

    protected async void OnDurationSelectionValueChanged(object optionID)
    {
        _readingDetailsData.ChartData.SelectedDuration = _readingDetailsData.FilterOptions.FirstOrDefault(x => x.IsSelected);
        if (_readingDetailsData.ChartData.SelectedDuration != null)
        {
            await GetReadingDataAsync(null);
        }
        StateHasChanged();
    }

    private async Task GetReadingDataAsync(string action)
    {
        _readingService.SetReadingDateTime(_readingDetailsData, _readingDetailsData.ChartData.SelectedDuration.OptionID, action);
        await GetChartDataAsync().ConfigureAwait(true);
    }

    protected async Task OnPrevOrNextButtonClicked(string action)
    {
        _readingDetailsData.Action = action;
        await GetReadingDataAsync(_readingDetailsData.Action);
        var selectedDuration = _readingDetailsData.FilterOptions.FirstOrDefault(x => x.IsSelected);
    }

    private bool IsTargetAddEditAllowed()
    {
        return _readingDetailsData.ErrCode == ErrorCode.OK &&
            LibPermissions.HasPermission(_readingDetailsData.FeaturePermissions, AppPermissions.PatientReadingTargetAddEdit.ToString())
            && (_readingDetailsData.ChartMetaData?.Any(x => (x.ReadingID == _readingID || x.ReadingParentID == _readingID) && x.IsActive) ?? false);
    }

    private bool IsAddAllowed()
    {
        return _readingDetailsData.ErrCode == ErrorCode.OK &&
            LibPermissions.HasPermission(_readingDetailsData.FeaturePermissions, AppPermissions.PatientReadingAddEdit.ToString())
            && _readingService.IsAddAllowed(_readingDetailsData.ChartMetaData?.Where(x => x.ReadingID == _readingID || x.ReadingParentID == _readingID)?.ToList(), AppState.IsPatient? true : false);
    }
}