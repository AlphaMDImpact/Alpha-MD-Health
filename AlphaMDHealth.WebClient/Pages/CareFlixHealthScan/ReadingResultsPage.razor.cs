using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ReadingResultsPage : BasePage
{

    private PatientReadingDTO _readingData;
    private bool _displayCategoryFilter = true;
  //  private bool _shouldShowTargetAddEditPopup;
    private ReadingService _pageService;
   // private bool _isNotSummaryView;
    private short _readingID;
    private short _readingCategoryID;
   // private bool _showScanVitalsPage;
   // private List<ButtonActionModel> _actionData;
  //  private bool _showConfirmationPopup = true;
    private AmhViewCellModel _sourceFields;

    [Parameter]
    public long RecordCount { get;set; }
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
            //if (Parameters?.Count > 0)
            //{
            //    _readingData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(PatientReadingDTO.RecordCount)));
            //    _displayCategoryFilter = GenericMethods.MapValueType<bool>(GetParameterValue(Constants.DISPLAY_CATEGORY_FILTER_STRING));
            //    _readingCategoryID = GenericMethods.MapValueType<short>(GetParameterValue(nameof(PatientReadingDTO.ReadingCategoryID)));
            //}
            _readingData.Settings = AppState.MasterData.Settings;
        }
        else
        {
            _readingData = PageData as PatientReadingDTO;
        }
       // _isNotSummaryView = Parameters?.Count < 1 || _readingData.IsMedicalHistory;
        _readingData.ReadingCategoryID = _readingCategoryID;
        _readingData.ReadingID = _readingID;
        if (PageData == null)
        {
            await SendServiceRequestAsync(_pageService.GetPatientReadingsAsync(_readingData), _readingData).ConfigureAwait(true);
        }
        if (_readingData.RecordCount == 0)
        {
            _readingID = (_readingID == 0 && _readingData.ListData?.Count > 0)
             ? _readingData.ReadingDTOs?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.LatestValue))?.ReadingID ?? default
             : _readingID;
        }
        //if (_isNotSummaryView)
        //{
        //    if (_readingID == 0)
        //    {
        //        _readingID = _readingData.ChartMetaData?.FirstOrDefault().ReadingID ?? 0;
        //    }
        //}
        _isDataFetched = true;
    }

    private AmhViewCellModel GetViewCellModel()
    {
        return new AmhViewCellModel
        {
            ID = nameof(PatientReadingDTO.ReadingID),
            LeftIcon = nameof(PatientReadingDTO.ReadingIcon),
            LeftHeader = nameof(PatientReadingDTO.Title),
            LeftFieldType = FieldTypes.SquareWithBorderImageControl,
            LeftDescriptionFieldType = FieldTypes.HtmlPrimaryLabelControl,
            LeftDescription = nameof(PatientReadingDTO.ValueUnit),
            RightDescription = nameof(PatientReadingDTO.MinMaxReadingRanges),
        };
    }
}
