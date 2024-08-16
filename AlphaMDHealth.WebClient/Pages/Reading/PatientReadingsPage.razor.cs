using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Reflection.Metadata;

namespace AlphaMDHealth.WebClient;

public partial class PatientReadingsPage : BasePage
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; }
    private PatientReadingDTO _readingData;
    private readonly List<ButtonActionModel> _buttons = new List<ButtonActionModel>();
    private bool _displayCategoryFilter = true;
    private bool _shouldShowTargetAddEditPopup;
    private ReadingService _pageService;
    private bool _isNotSummaryView;
    private short _readingID;
    private short _readingCategoryID;
    private bool _showScanVitalsPage;
    private List<ButtonActionModel> _actionData;
    private bool _showConfirmationPopup = true;
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
                if (_readingData != null)
                {
                    if (_readingData.RecordCount > 0 || _readingID == 0)
                    {
                        _readingData.RecordCount = default;
                        _isNotSummaryView = Parameters?.Count < 1 || _readingData.IsMedicalHistory;
                    }
                }
                _readingID = value;
            }
        }
    }

    /// <summary>
    /// Reading Category ID
    /// </summary>
    [Parameter]
    public short ReadingCategoryID
    {
        get { return _readingCategoryID; }
        set
        {
            if (_readingCategoryID != value)
            {
                if (_readingData != null)
                {
                    if (_readingData.RecordCount > 0 || _readingCategoryID == 0)
                    {
                        _readingData.RecordCount = default;
                        _isNotSummaryView = Parameters?.Count < 1 || _readingData.IsMedicalHistory;
                    }
                }
                _readingCategoryID = value;
            }
        }
    }

    /// <summary>
    /// External data to load
    /// </summary>
    [Parameter]
    public object PageData { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (_pageService == null)
        {
            _pageService = new ReadingService(AppState.webEssentials);
        }
        if (PageData == null)
        {
            _readingData = new PatientReadingDTO();
            _readingData.RecordCount = 0;
            _readingData.ReadingCategoryID = 0;
            if (Parameters?.Count > 0)
            {
                _readingData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(PatientReadingDTO.RecordCount)));
                _displayCategoryFilter = GenericMethods.MapValueType<bool>(GetParameterValue(Constants.DISPLAY_CATEGORY_FILTER_STRING));
                _readingCategoryID = GenericMethods.MapValueType<short>(GetParameterValue(nameof(PatientReadingDTO.ReadingCategoryID)));
            }
            _readingData.Settings = AppState.MasterData.Settings;
        }
        else
        {
            _readingData = PageData as PatientReadingDTO;
        }
        _isNotSummaryView = Parameters?.Count < 1 || _readingData.IsMedicalHistory;
        _readingData.ReadingCategoryID = _readingCategoryID;
        _readingData.ReadingID = _readingID;
        if (PageData == null)
        {
            await SendServiceRequestAsync(_pageService.GetPatientReadingsAsync(_readingData), _readingData).ConfigureAwait(true);
        }
        if (IsPatientMobileView)
        {
            _actionButtons ??= new List<ButtonActionModel>();
            if (CheckAddAllowed(_readingData.ChartMetaData) && _readingData.RecordCount < 1 && _readingData.ReadingID == 0)
            {
                _actionButtons.Add(new ButtonActionModel
                {
                    ButtonResourceKey = ResourceConstants.R_ADD_ACTION_KEY,
                    ButtonAction = () => { ShowContextMenu(); },
                    Icon = ImageConstants.I_ADD_ICON_RESPONSIVE
                });
            }
        }
        if (_readingData.RecordCount == 0)
        {
            _readingID = (_readingID == 0 && _readingData.ListData?.Count > 0)
             ? _readingData.ReadingDTOs?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.LatestValue))?.ReadingID ?? default
             : _readingID;
        }
        if (_isNotSummaryView)
        {
            if (_readingID == 0)
            {
                _readingID = _readingData.ChartMetaData?.FirstOrDefault().ReadingID ?? 0;
            }
        }
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(PatientReadingDTO.ReadingID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{HasImage=true,ImageFieldType=FieldTypes.ImageControl,ImageHeight=AppImageSize.ImageSizeM,ImageWidth=AppImageSize.ImageSizeM, ImageIcon=nameof(PatientReadingDTO.ReadingIcon) , IsSortable= false,ShowRowColumnHeader=false},
            new TableDataStructureModel{DataField=nameof(PatientReadingDTO.LatestValueString),IsHtmlTag = true, ShowRowColumnHeader=false},
            new TableDataStructureModel{DataField=nameof(PatientReadingDTO.LatestValueDateText), IsSortable=false,ShowRowColumnHeader=false}
        };
    }
    private AmhViewCellModel getViewCellModel()
    {
        var cardList = new AmhViewCellModel
        {
            ID = nameof(PatientReadingDTO.ReadingID),
            LeftHeader = nameof(PatientReadingDTO.TitleWithIcon),
            LeftHeaderFieldType = FieldTypes.HtmlPrimaryLabelControl,
            LeftFieldType = FieldTypes.SquareImageControl,
            LeftDescription = nameof(PatientReadingDTO.LatestValueString),
            LeftDescriptionFieldType = FieldTypes.HtmlPrimaryLabelControl,
        };
        if (Parameters?.Count < 1)
        {
            cardList.LeftIcon = nameof(PatientReadingDTO.ReadingIcon);
            cardList.LeftHeader = nameof(PatientReadingDTO.Title);
            cardList.LeftHeaderFieldType = FieldTypes.PrimarySmallHStartVCenterBoldLabelControl;
            cardList.RightDescription = nameof(PatientReadingDTO.LatestValueDateText);
        };
        return cardList;
    }
  /*  private AmhViewCellModel getViewCellModel()
    {
        return new AmhViewCellModel
        {
            ID = nameof(PatientReadingDTO.ReadingID),
            LeftIcon = nameof(PatientReadingDTO.ReadingIcon),
            LeftHeader = nameof(PatientReadingDTO.Title),
            LeftFieldType = FieldTypes.SquareImageControl,
            LeftDescription = nameof(PatientReadingDTO.LatestValueString),
            LeftDescriptionFieldType = FieldTypes.HtmlPrimaryLabelControl,
            RightDescription = nameof(PatientReadingDTO.LatestValueDateText),
        };
    }*/
    private bool CheckAddAllowed(List<ReadingMetadataUIModel> metadata)
    {
        return LibPermissions.HasPermission(_readingData.FeaturePermissions, AppPermissions.PatientReadingAddEdit.ToString())
            && _pageService.IsAddAllowed(metadata, AppState.IsPatient);
    }


    private void SetSelectedFilterOption(short id)
    {
        if (GenericMethods.IsListNotEmpty(_readingData.FilterOptions))
        {
            _readingData.FilterOptions.ForEach((item) =>
            {
                item.IsSelected = item.OptionID == id;
            });
        }
    }

    private async Task OnReadingRowClickedAsync(object reading)
    {
        var readingModel = (PatientReadingDTO)reading;
        if (!_isNotSummaryView)
        {
            await NavigateToAsync(AppPermissions.PatientReadingsView.ToString(), readingModel.ReadingID.ToString(),
                _readingData.FilterOptions.FirstOrDefault(x => x.IsSelected)?.OptionID.ToString()
            ).ConfigureAwait(false);
            return;
        }
        if (_readingData.ReadingID != readingModel.ReadingID)
        {
            _readingData.ReadingID = _readingID = readingModel.ReadingID;
        }
        StateHasChanged();
    }

    private async Task OnReadingTypeClickAsync(object optionID)
    {
        short categoryID = Convert.ToInt16(optionID);
        if (_readingData.FilterOptions.FirstOrDefault(x => x.IsSelected)?.OptionID == categoryID || _readingData.FilterOptions.FirstOrDefault(x => x.IsSelected)?.OptionID != categoryID)
        {
            SetSelectedFilterOption(categoryID);
            _readingData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(PatientReadingDTO.RecordCount)));
            _readingData.ReadingCategoryID = categoryID;
            _readingData.Settings = AppState.MasterData.Settings;
            await SendServiceRequestAsync(_pageService.GetPatientReadingsAsync(_readingData), _readingData).ConfigureAwait(true);
            _readingData.ReadingID = _readingID = (short)(_readingData.ReadingID == 0 ? 0 : _readingData.ReadingID);
            var _detailPageID = _readingData.ReadingDTOs?.FirstOrDefault(x =>
                               x.ReadingID == _readingID ||
                               x.ChartMetaData.Any(y => y.ReadingID == _readingID)
                           )?.ReadingID ?? 0;
            if (_detailPageID < 1)
            {
                _detailPageID = _readingData.ReadingDTOs?.FirstOrDefault()?.ReadingID ?? default;
            }
            if (_detailPageID > 0)
            {
                _readingData.ReadingID = _readingID = _detailPageID;
            }
        }
        StateHasChanged();
    }

    private void OnAddEditClicked(PatientReadingUIModel vital, bool IsTargetPage)
    {
        ShowDetailPage = true;
        _shouldShowTargetAddEditPopup = IsTargetPage;
        _readingData.PatientReadingID = vital == null ? Guid.Empty : vital.PatientReadingID;
        _readingData.ReadingCategoryID = _readingData.ReadingCategoryID;
        _readingData.ReadingID = vital == null ? _readingID : vital.ReadingID;
        _readingID = _readingData.ReadingID;
        StateHasChanged();
    }
    private async void OnAddScanVitalsClicked()
    {
        ShowDetailPage = true;
        _showScanVitalsPage = true;
        StateHasChanged();
    }

    private async void OnDetailPageClosedAsync(string isDataUpdated)
    {
        ShowDetailPage = false;
        _shouldShowTargetAddEditPopup = false;
        Success = Error = string.Empty;
        if (!string.IsNullOrEmpty(isDataUpdated))
        {
            _isDataFetched = false;
            string[] values = isDataUpdated.Split('|');
            if (values.Length == 3)
            {
                Success = values[0];
                _readingID = _readingData.ReadingID = Convert.ToInt16(values[1]);
                _readingCategoryID = _readingData.ReadingCategoryID = Convert.ToInt16(values[2]);
            }
            await SendServiceRequestAsync(_pageService.GetPatientReadingsAsync(_readingData), _readingData).ConfigureAwait(true);
            _isDataFetched = true;
            StateHasChanged();
        }
    }

    private async void OnDetailPageCloseAsync(string isDataUpdated)
    {
        await NavigateToAsync(AppPermissions.PatientReadingsView.ToString(),true).ConfigureAwait(true);
    }


    private void OnAddButtonClicked()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
             new ButtonActionModel{ ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY }
        };
        _showConfirmationPopup = false;
    }
    private async void OnScanVitalPageClosedAsync(string isDataUpdated)
    {
        if (_showScanVitalsPage)
        {
            _showScanVitalsPage = false;
            //_showNextScanPage = true;
        }
    }
    private async Task PopUpCallbackAsync(object e)
    {
        _showConfirmationPopup = true;
        Success = Error = string.Empty;
        if (Convert.ToInt64(e) == 1)
        {
            OnAddEditClicked(null, false);
            //_menuData.Menu.IsActive = false;
        }
        else
        {
            OnAddScanVitalsClicked();
        }
    }
    private async void OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.PatientReadingsView.ToString()).ConfigureAwait(false);
    }
    private async Task ShowContextMenu()
    {
        await JSRuntime.InvokeVoidAsync("showContextMenu");
    }
    private async Task HideContextMenu()
    {
        await JSRuntime.InvokeVoidAsync("hideContextMenu");
    }
}