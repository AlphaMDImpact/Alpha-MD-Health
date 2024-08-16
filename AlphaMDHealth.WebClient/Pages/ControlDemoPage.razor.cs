using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.JSInterop;

namespace AlphaMDHealth.WebClient;

public partial class ControlDemoPage : IDisposable
{
    private readonly AuthDTO _authData = new AuthDTO { IsActive = false };
    DateTimeOffset? FromDateTime;
    DateTimeOffset? FromDate;
    DateTimeOffset? FromTime;
    List<AttachmentModel> _singleFile;
    List<AttachmentModel> _multipleFile;
    List<AttachmentModel> _multipleFile1;

    DateTime _timelineControl;
    long _carouselControl;
    List<OptionModel> _carouselItems;
    string _textEntryWithIconValue;
    string _passwordEntryyWithIconValue;
    string _emailEntryyWithIconValue;
    string _regexValue;
    string _textEntryValue;
    string _passwordEntryValue;
    string _emailEntryValue;
    string _textEditorValue;
    double? _numericEntryWithIconValue;
    double? _numericEntryValue;
    double? _numericEntryDisabledValue;
    double? _decimalEntryValue;
    double? _counterControlValue;
    double? _counterControlDisableValue;
    double? _decimalEntryDisabledValue;
    string _colorPicker;
    string _disableColorPicker;
    //object _mudNumericValue;
    List<OptionModel> ddOptionsList;
    List<OptionModel> optionsList;
    List<OptionModel> timelineList;
    string accID;
    //List<OptionModel> CountryList;
    string _mobileNumberValue;
    string _mobileNumberDisabledValue;
    string _checkBoxValue;
    string radioButtonOptions;
    string _primaryButton;
    double? _sliderValue;
    private string _singleSelectValue;
    private string _singleSelectEditableValue;
    private string _multiSelectValue;
    private string _multiSelectEditableValue;
    private string _singleSelectDisabledValue;
    private string _singleSelectEditableDisabledValue;
    private string _multiSelectDisabledValue;
    private string _multiSelectEditableDisabledValue;
    bool _switchvalue;
    bool _switchdisablevalue;
    List<OptionModel> _calendardata;
    string imageBase64;
    byte[] imageByte;
    string imageInitial;
    string imageIcon;
    string imagePath;
    string _richTextEntryHtmlValue;
    string _htmlLabelControl;
    int _progressbarvalue;
    int _progressindicatorvalue;
    long tabID = 2;
    string _chartTypeValue;
    long _durationTabID;
    List<OptionModel> _chartTypes;
    List<OptionModel> _chartDurations;
    ChartUIDTO _chartDataSource;
    private AmhChartControl _amhChartControl;
    private FieldTypes _type;
    private long typeEnum;
    DateTimeOffset _initialStartTimeOffset;
    List<PatientMedicationModel> dataset;
    List<PatientMedicationModel> emptydataset;
    DateTimeOffset? _disableddateTimePickerControl;
    DateTimeOffset? _dateTimePickerControl;
    DateTimeOffset? _datePickerControl;
    DateTimeOffset? _timePickerControl;
    private ControlDemoService demoService;
    private string badgeValue = "In-progress";
    private List<TabDataStructureModel> DataFormatter;
    private List<LanguageModel> _languageTabList;
    private List<ButtonActionModel> _messageButtonActions;

    private List<OptionModel> _listoptions;
    private List<DemoModel> _cardList;
    private AmhViewCellModel _sourceFields;
    private bool _isGroupedData;
    private bool _showSearchBar;


    protected override async Task OnInitializedAsync()
    {
        getTableList(0);
        if (AppState.webEssentials == null)
        {
            AppState.webEssentials = new WebEssentials(LocalStorage);
        }
        _authData.AuthenticationData = new AuthModel();
        var authService = new AuthService(AppState.webEssentials);
        await SendServiceRequestAsync(authService.GetAccountDataAsync(_authData), _authData).ConfigureAwait(true);
        demoService = new ControlDemoService(AppState.webEssentials);
        demoService.GetControlDemoPageResources(_authData);
        _carouselItems = demoService.GetCarauseloptionList();

        _listoptions = demoService.GetListOptions();



        _singleFile = new List<AttachmentModel>();
        _multipleFile = new List<AttachmentModel>
        {
            new AttachmentModel
            {
                FileID = GenericMethods.GenerateGuid(),FileName = "jay.doc",IsActive = true,FileExtension = "doc",FileValue="http://ieee802.org:80/secmail/docIZSEwEqHFr.doc"
            },
            new AttachmentModel
            {
                FileID = GenericMethods.GenerateGuid(),FileName = "image.jpg",IsActive = true,FileExtension = "jpg",FileValue="https://i.postimg.cc/Bnw1YSRN/demo1.jpg"
            },
            new AttachmentModel
            {
                FileID = GenericMethods.GenerateGuid(),FileName = "pdfdemo.pdf",IsActive = true,FileExtension = "pdf",FileValue="https://www.africau.edu/images/default/sample.pdf"
            },
        };
        _multipleFile1 = new List<AttachmentModel>
{
    new AttachmentModel
    {
        FileID = GenericMethods.GenerateGuid(),FileName = "jay.doc",IsActive = true,FileExtension = "doc",FileValue="http://ieee802.org:80/secmail/docIZSEwEqHFr.doc"
    },
    new AttachmentModel
    {
        FileID = GenericMethods.GenerateGuid(),FileName = "image.jpg",IsActive = true,FileExtension = "jpg",FileValue="https://i.postimg.cc/Bnw1YSRN/demo1.jpg"
    },
    new AttachmentModel
    {
        FileID = GenericMethods.GenerateGuid(),FileName = "pdfdemo.pdf",IsActive = true,FileExtension = "pdf",FileValue="https://www.africau.edu/images/default/sample.pdf"
    },
};
        _progressbarvalue = 30;
        _progressindicatorvalue = 85;
        _textEntryWithIconValue = "sstest";
        _textEntryValue = "";
        _emailEntryValue = "sstest@yopmail.com";
        _passwordEntryValue = "password@123";
        _regexValue = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";
        _textEditorValue = "hello ,Hi There!";
        _numericEntryWithIconValue = 7;
        _numericEntryValue = 5;
        _counterControlValue = 5;
        _decimalEntryValue = 5.5;
        _decimalEntryDisabledValue = 50;
        _numericEntryDisabledValue = 50;
        _counterControlDisableValue = 36;
        _numericEntryDisabledValue = 50;
        //_colorPicker = "#0000ff";
        _disableColorPicker = "#0000ff";
        _isDataFetched = true;
        _primaryButton = "Primary Btn";
        //_tertiaryButton = "Tertiary Btn";
        optionsList = demoService.GetOptionsList();
        timelineList = demoService.GetTimelineList();
        //CountryList = demoService.GetCountryCode();
        //_checkBoxValue = "2|5";
        //radioButtonOptions = "3";
        imageInitial = "MD";
        imageBase64 = "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";
        imageByte = Convert.FromBase64String(imageBase64);
        imageIcon = ImageConstants.I_USER_ID_SVG;
        imagePath = "https://fastly.picsum.photos/id/103/536/354.jpg?hmac=nVXoeV9R4FBO5o5KV0HOPnwMoIJfY5ZkZpmukDVgFsA";
        _sliderValue = 0.0;
        ddOptionsList = demoService.GetOptionsList();
        //_singleSelectValue = "1";
        //_singleSelectEditableValue = "1";
        //_multiSelectValue = "1";
        _multiSelectEditableValue = "1|2|3|5|6";
        _singleSelectDisabledValue = "3";
        _singleSelectEditableDisabledValue = "2";
        _multiSelectDisabledValue = "4|5";
        _multiSelectEditableDisabledValue = "5|6";
        _switchvalue = false;
        _switchdisablevalue = false;
        _calendardata = demoService.GetCalanderData();
        //_richTextEntryHtmlValue = "<h2>this is default html string for rich text editor.</h2>";
        _htmlLabelControl = "<p><nbsp> <i>HtmlLabel</i> <nbsp> <b>HtmlLabel</b> <nbsp> <u>HtmlLabel</u></p>";
        //_chartData = demoService.GetGraphData1();
        _chartTypes = demoService.GetOptionsListForCharts();
        _chartDurations = demoService.GetOptionsListForTypeOfCharts();
        _durationTabID = 475;
        _amhChartControl = new AmhChartControl();
        _mobileNumberDisabledValue = "+91-9023456789";
        //_datePickerControl = "22/12/2023";
        //_timePickerControl = "03:12";
        //_dateTimePickerControl = new DateTime(2023, 12, 10, 11, 44, 00);
        _disableddateTimePickerControl = new DateTime(2022, 12, 10, 11, 44, 00);
        _languageTabList = demoService.GetLanguageTabList();
        DataFormatter = new List<TabDataStructureModel>
        {
            new TabDataStructureModel
            {
                DataField = nameof(OptionModel.OptionText),
                ResourceKey = ResourceConstants.R_USER_NAME_KEY,
                IsRequired = false
            },
            new TabDataStructureModel
            {
                DataField = nameof(OptionModel.ParentOptionText),
                ResourceKey = FieldTypes.MultiLineEntryControl.ToString(),
                IsRequired = false
            },
            new TabDataStructureModel
            {
                DataField = nameof(OptionModel.GroupName),
                FieldType=FieldTypes.RichTextControl,
                ResourceKey = FieldTypes.RichTextControl.ToString(),
                IsRequired = false
            }

        };

        _messageButtonActions = new List<ButtonActionModel>
        {
            new ButtonActionModel{ ButtonID = FieldTypes.PopupMessageControl.ToString(), ButtonResourceKey =FieldTypes.PopupMessageControl.ToString()  },
            new ButtonActionModel{ ButtonID = FieldTypes.TopHeadingPopupMessageControl.ToString(), ButtonResourceKey =FieldTypes.TopHeadingPopupMessageControl.ToString()  },
            new ButtonActionModel{ ButtonID = FieldTypes.CloseButtonPopupMessageControl.ToString(), ButtonResourceKey =FieldTypes.CloseButtonPopupMessageControl.ToString()  },
            new ButtonActionModel{ ButtonID = FieldTypes.TopHeadingWithCloseButtonPopupMessageControl.ToString(), ButtonResourceKey =FieldTypes.TopHeadingWithCloseButtonPopupMessageControl.ToString() }
        };
    }

    private AmhViewCellModel MapSourceField()
    {
        return new AmhViewCellModel
        {
            ID = nameof(OptionModel.OptionID),
            LeftFieldType = FieldTypes.ImageControl,
            LeftImage = nameof(OptionModel.GroupName),
            LeftHeaderFieldType = FieldTypes.PrimaryAppLargeHVCenterBoldLabelControl,
            LeftHeader = nameof(OptionModel.ParentOptionText),
            LeftDescriptionFieldType = FieldTypes.HtmlSecondaryCenterLabelControl,
            LeftDescription = nameof(OptionModel.OptionText),
        };
    }

    protected void OnListOptionChanged(object? e)
    {
        if (e != null)
        {
            MapListItemSourceProps(Convert.ToInt64(e));
        }
    }

    private void MapListItemSourceProps(long val)
    {
        switch (val)
        {
            case 11://TwoRowFieldWithBadgeControl
                _isGroupedData = false;
                _sourceFields = new AmhViewCellModel
                {
                    ID = nameof(DemoModel.ID),
                    LeftHeader = nameof(DemoModel.Name),
                    LeftHeaderFieldType = FieldTypes.PrimaryAppLargeHVCenterBoldLabelControl,
                    LeftDescription = nameof(DemoModel.Description),
                    RightHeader = nameof(DemoModel.SubHeader),
                    RightDescription = nameof(DemoModel.Status),
                    RightDescriptionField = nameof(DemoModel.StatusType),
                    LeftFieldType = FieldTypes.CircleWithBackgroundImageControl,
                    LeftImage = nameof(DemoModel.Image),
                    LeftIcon = nameof(DemoModel.MainIcon),

                    BandColor = nameof(DemoModel.BandColor),
                };
                break;
            case 8: //TwoRowGroupedListViewControl
            case 4: //TwoRowGroupedListViewControl
                _isGroupedData = true;
                _sourceFields = new AmhViewCellModel
                {
                    ID = nameof(DemoModel.ID),
                    LeftHeader = nameof(DemoModel.Name),
                    LeftDescription = nameof(DemoModel.Description),
                    RightHeader = nameof(DemoModel.SubHeader),
                    RightDescription = nameof(DemoModel.Status),
                    LeftFieldType = FieldTypes.CircleWithBackgroundImageControl,
                    LeftImage = nameof(DemoModel.Image),
                    LeftIcon = nameof(DemoModel.MainIcon),
                    //RightImageByte = nameof(DemoModel.ImageBytes),
                    //RightHTMLLabelField = nameof(DemoModel.AppointmentStatusName),
                    //RightImageIcon = nameof(DemoModel.LanguageName),
                    GroupIDField = nameof(DemoModel.GroupID),
                    GroupName = nameof(DemoModel.GroupName),
                    ChildItems = nameof(DemoModel.Items),
                    BandColor = nameof(DemoModel.BandColor),
                };
                break;
            case 10: //ShowMoreIconOneRowListViewControl
            case 9: //ShowMoreLabelOneRowListViewControl
            case 7: //OneRowGroupedListViewControl
            case 3: //OneRowGroupedListViewControl
                _isGroupedData = true;
                _sourceFields = new AmhViewCellModel
                {
                    ID = nameof(DemoModel.ID),
                    LeftHeader = nameof(DemoModel.Name),
                    RightHeader = nameof(DemoModel.SubHeader),
                    LeftFieldType = FieldTypes.SquareWithBackgroundImageControl,
                    LeftImage = nameof(DemoModel.Image),
                    LeftIcon = nameof(DemoModel.MainIcon),
                    GroupIDField = nameof(DemoModel.GroupID),
                    GroupName = nameof(DemoModel.GroupName),
                    ChildItems = nameof(DemoModel.Items),
                    BandColor = nameof(DemoModel.BandColor),
                };
                break;
            case 6: //TwoRowListViewControl
            case 2: //TwoRowListViewControl
                _isGroupedData = false;
                _sourceFields = new AmhViewCellModel
                {
                    ID = nameof(DemoModel.ID),
                    LeftHeader = nameof(DemoModel.Name),
                    LeftDescription = nameof(DemoModel.Description),
                    RightHeader = nameof(DemoModel.SubHeader),
                    RightDescription = nameof(DemoModel.Status),
                    LeftFieldType = FieldTypes.SquareWithBackgroundImageControl,
                    LeftImage = nameof(DemoModel.Image),
                    LeftIcon = nameof(DemoModel.MainIcon),
                    //RightImageByte = nameof(DemoModel.ImageBytes),
                    //RightHTMLLabelField = nameof(DemoModel.AppointmentStatusName),
                    RightIcon = nameof(DemoModel.NavIcon),
                    BandColor = nameof(DemoModel.BandColor),
                };
                break;
            case 5: //OneRowListViewControl
            case 1: //OneRowListViewControl
            default:
                _isGroupedData = false;
                _sourceFields = new AmhViewCellModel
                {
                    ID = nameof(DemoModel.ID),
                    LeftFieldType = FieldTypes.CircleWithBackgroundImageControl,
                    LeftImage = nameof(DemoModel.Image),
                    LeftIcon = nameof(DemoModel.MainIcon),
                    LeftHeader = nameof(DemoModel.Name),
                    RightHeader = nameof(DemoModel.SubHeader),
                };
                break;
        }

        _showSearchBar = val == 5 || val == 6 || val == 7 || val == 8;
        //GroupShowMoreIcon = val == 10 ? "arrowmenuiconltr.png" : "";
        //GroupShowMoreText = val == 9 ? "Show More > " : "";
        _cardList = GenerateLargeDataset(0);

        //OnRightViewClicked += OnListRightViewClicked;
        //OnValueChanged += OnListItemClick;
        //OnLoadMore += OnListItemLoadMore;
        //OnPullToRefresh += OnListItemPullToRefresh;
        //OnGroupShowMoreClicked += OnGroupShowMoreClicked;
    }

    private Random rnd = new Random();

    private List<DemoModel> GenerateLargeDataset(int lastIndex)
    {
        //ID = nameof(DemoModel.ID),
        //    RightIcon = nameof(DemoModel.NavIcon),
        //    LeftIcon = nameof(DemoModel.Image),
        //    LeftHeader = nameof(DemoModel.Name),
        //    LeftDescription = nameof(DemoModel.Description),
        //    RightHeader = nameof(DemoModel.SubHeader),
        //    RightDescription = nameof(DemoModel.Status),

        lastIndex += 1;
        List<DemoModel> dataset = new List<DemoModel>();
        for (int i = lastIndex; i <= lastIndex + 10; i++)
        {
            var appointment = new DemoModel
            {
                ID = i,
                MainIcon = ImageConstants.I_DOC_UPLOAD_ICON,
                Image = i % 7 == 0
                    ? "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"
                    : $"T-{i}",
                Name = $"Test-{i}",
                Description = $"sstest data description {i} ddn",
                Status = $"Active{i}",
                StatusType = GetStatusType(i),
                SubHeader = "1990",
                HtmlData = $"<p><h1 style=\"background-color:red;\">HtmlLabel</h1><nbsp><i>HtmlLabel</i> <nbsp> <b>HtmlLabel</b> <nbsp> <u>HtmlLabel</u></p>",
                NavIcon = "Delete.svg",
            };
            if (i == 5)
            {
                appointment.Image = "https://ucarecdn.com/05f649bf-b70b-4cf8-90f7-2588ce404a08/-/format/auto/";
            }
            if (i % 6 == 0)
            {
                appointment.BandColor = "#DAF7A6";
            }
            if (i % 5 == 0)
            {
                appointment.BandColor = "#7FFFD4";
            }
            else if (i % 4 == 0)
            {
                appointment.BandColor = "#0000FF";
            }
            else if (i % 3 == 0)
            {
                appointment.BandColor = "#9932CC";
            }

            if (_isGroupedData)
            {
                if (i % 5 == 0)
                {
                    appointment.GroupID = 1;
                    appointment.GroupName = "Group-1";
                }
                else if (i % 4 == 0)
                {
                    appointment.GroupID = 2;
                    appointment.GroupName = "Group-2";
                }
                else if (i % 3 == 0)
                {
                    appointment.GroupID = 3;
                    appointment.GroupName = "Group-3";
                }
                else
                {
                    appointment.GroupID = 4;
                    appointment.GroupName = "Group-0";
                }
            }
            dataset.Add(appointment);
        }
        if (_isGroupedData)
        {
            List<DemoModel> groups = new List<DemoModel>();
            var groupData = dataset.GroupBy(x => x.GroupID);
            foreach (var group in groupData)
            {
                var items = group.ToList();
                groups.Add(new DemoModel { GroupID = group.Key, GroupName = items[0].Name, Items = items });
            }
            return groups;
        }
        return dataset;
    }

    private FieldTypes GetStatusType(int i)
    {
        return i % 8 == 0 ? FieldTypes.PrimaryBadgeControl
            : i % 7 == 0 ? FieldTypes.SecondaryBadgeControl
            : i % 6 == 0 ? FieldTypes.SuccessBadgeControl
            : i % 5 == 0 ? FieldTypes.DangerBadgeControl
            : i % 4 == 0 ? FieldTypes.WarningBadgeControl
            : i % 3 == 0 ? FieldTypes.InfoBadgeControl
            : i % 2 == 0 ? FieldTypes.DarkBadgeControl
            : i % 1 == 0 ? FieldTypes.LightBadgeControl
            : FieldTypes.BadgeControl;
    }

    private async Task OnCardViewClickAsync(object item)
    {

    }

    private async Task OnShowAllClickedAsync(object item)
    {

    }

    string selectedAction = string.Empty;

    private void OnMessageActionClicked(object o)
    {
        selectedAction = string.Empty;
        if (o != null)
        {
            selectedAction = o.ToString();
        }
    }

    private async Task OnButtonClickAsync()
    {
        Error = string.Empty;
        var fasdsda = _multipleFile;
        var aa = _singleSelectValue;
        var aa2 = _singleSelectEditableValue;
        var aa3 = _multiSelectValue;
        var aa4 = _multiSelectEditableValue;
        var radio = optionsList.FirstOrDefault(x => x.IsSelected);
        if (IsValid())
        {
            Success = ErrorCode.OK.ToString();
        }
    }

    private void OnAccordianOptionChnged(object o)
    {
    }

    private async Task OndropdownButtonClicked(object o)
    {
    }

    private async Task OnLoginButtonClickAsync()
    {
        await NavigateToAsync(AppPermissions.LoginView.ToString()).ConfigureAwait(false);
    }

    protected void OnChartSelectionValueChanged(object? e)
    {
        if (e != string.Empty)
        {
            _chartDataSource = new ChartUIDTO() { SelectedDuration = (new OptionModel { OptionID = Convert.ToInt64(e) }) };
            typeEnum = _chartDataSource.SelectedDuration.OptionID = Convert.ToInt64(e);
            if (typeEnum == 1)
            {
                _type = FieldTypes.LineGraphControl;
            }
            else if (typeEnum == 2)
            {
                _type = FieldTypes.BarGraphControl;
            }
        }
    }

    protected void OnDurationSelectionValueChanged(object? e)
    {
        if (_chartTypeValue != "")
        {
            FieldTypes type = _chartTypes.FirstOrDefault(x => x.OptionID == (Convert.ToInt64(_chartTypeValue)))?.GroupName.ToEnum<FieldTypes>() ?? default;
            var selectedDuration = _chartDurations.FirstOrDefault(x => x.OptionID == Convert.ToInt64(_durationTabID));
            if (selectedDuration != null && (type == FieldTypes.BarGraphControl || type == FieldTypes.LineGraphControl))
            {
                _chartDataSource = new ChartUIDTO();
                _chartDataSource.SelectedDuration = selectedDuration;
                demoService.GetGraphData1(_chartDataSource);
                if (selectedDuration.OptionID == ResourceConstants.R_ALL_FILTER_KEY_ID)
                {
                    selectedDuration.SequenceNo = -1;
                    _chartDataSource.StartDate = _chartDataSource.Lines?.Min(x => x.ChartData?.Min(y => y.DateTime)).Value;
                    _chartDataSource.EndDate = _chartDataSource.Lines?.Max(x => x.ChartData?.Max(y => y.DateTime)).Value;
                }
                else
                {
                    _chartDataSource.SelectedDuration.SequenceNo = Math.Abs(selectedDuration.SequenceNo);
                }
                _amhChartControl.Value = _chartDataSource;
            }
        }
    }

    protected void OnPrevOrNextButtonClicked(string action)
    {
        if (_chartTypeValue != "")
        {
            FieldTypes type = _chartTypes.FirstOrDefault(x => x.OptionID == (Convert.ToInt64(_chartTypeValue)))?.GroupName.ToEnum<FieldTypes>() ?? default;
            var selectedDuration = _chartDurations.FirstOrDefault(x => x.OptionID == Convert.ToInt64(_durationTabID));
            if (selectedDuration.OptionID == ResourceConstants.R_DAY_FILTER_KEY_ID)
            {
                selectedDuration.SequenceNo = 1;
            }
            if (selectedDuration.SequenceNo != -1 && selectedDuration.OptionID != ResourceConstants.R_ALL_FILTER_KEY_ID)
            {
                _chartDataSource.SelectedDuration = selectedDuration;
                _chartDataSource.SelectedDuration.SequenceNo = Math.Abs(selectedDuration.SequenceNo);
                if (action == ResourceConstants.R_NEXT_ACTION_KEY && selectedDuration != null && (type == FieldTypes.BarGraphControl || type == FieldTypes.LineGraphControl))
                {
                    _chartDataSource.SelectedDuration.SequenceNo = -(selectedDuration.SequenceNo);
                }
                else
                {
                    _chartDataSource.SelectedDuration.SequenceNo = Math.Abs(selectedDuration.SequenceNo);
                }
                demoService.GetGraphData1(_chartDataSource);
                _amhChartControl.Value = _chartDataSource;
            }
        }
    }

    protected async Task OnAppointmentSelect(object? e)
    {
        if (e != null && e is OptionModel option)
        {
            Error = $"Selected Appointment {option.OptionID}, {option.OptionText} from {option.From} to {option.To}";
        }
    }

    private string pdfBase64;
    private bool pdfDisplayed = false;
    protected async Task CreatePDF()
    {
        string FileName = "Invoice001";
        List<string> OrgDetail = new List<string>
        {
            "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",  // Organization Logo as Base64 string
        };
        Dictionary<string, string> Header = new Dictionary<string, string>
        {
            { "Program", "My Program" },
            { "Doctor Name", "Raj Shaan" },
            { "Patient Name", "Chetan Tej" },
            { "Enter Date", "April 12, 2023" },
        };
        List<TableModel> tableData = new List<TableModel>
        {
            new TableModel { Item = "Item 1", Amount = "50.00" },
            new TableModel { Item = "Item 2", Amount = "30.00" },
            new TableModel { Item = "Item 3", Amount = "20.00" },
            new TableModel { Item = "Total Amount", Amount = "100.00" },
            new TableModel { Item = "Discount", Amount = "10.00" },
            new TableModel { Item = "Amount Paid", Amount = "90.00" },
            new TableModel { Item = "Payment Mode", Amount = "Credit Card" },
        };
        //  ExportService service = new ExportService(hostingEnvironment);
        // MemoryStream documentStream = ExportService.CreatePDF(FileName, OrgDetail, Header, tableData);
        // pdfBase64 = "data:application/pdf;base64," + Convert.ToBase64String(documentStream.ToArray());
        pdfDisplayed = true;
    }

    public void Dispose()
    {
    }

    //private async Task OnViewChartsDemoClicked()
    //{
    //    await NavigateToAsync("/ChartView");
    //}

    //private async Task NavigateToAsync(string page)
    //{
    //    string baseUrl = "http://localhost:35040/organisation/simpledoseswebdev.azurewebsites.net/";

    //    if (page == "/ChartView")
    //    {
    //        string fullUrl = $"{baseUrl}{page}";
    //        NavigationManager.NavigateTo(fullUrl);
    //    }
    //}


    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.PatientMedicationID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.MedicationDosesString),DataHeaderValue="Editable",DataHeader=ResourceConstants.R_DOSES_KEY,IsEditable = true,IsSortable=true},
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.ProgramName),DataHeader=ResourceConstants.R_PROGRAM_TITLE_KEY,IsSortable=true},
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.ProgramColor),DataHeader="With MultipleImages",ImageFieldType=FieldTypes.CircleImageControl,ImageHeight=AppImageSize.ImageNoSize,ImageWidth=AppImageSize.ImageNoSize,ImageFields=new List<string>{
                            nameof(PatientMedicationModel.MedicationImage),
                            nameof(PatientMedicationModel.MedicationReminderImage),
                            nameof(PatientMedicationModel.FullName)  } ,IsSortable=true},
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.MedicationStatusString),DataHeaderValue="HTML TAG",DataHeader=ResourceConstants.R_STATUS_KEY,IsHtmlTag=true},
            new TableDataStructureModel{HasImage=true,ImageFieldType=FieldTypes.CircleImageControl,ImageHeight=AppImageSize.ImageSizeM,ImageWidth=AppImageSize.ImageSizeM,ImageSrc="Chats.svg"},
             new TableDataStructureModel{DataField=nameof(PatientMedicationModel.IsCritical),DataHeaderValue="CheckBox",DataHeader=ResourceConstants.R_IS_CRITICAL_KEY,IsCheckBox=true},
             new TableDataStructureModel{DataField=nameof(PatientMedicationModel.IsCritical),DataHeaderValue="Badge",DataHeader=ResourceConstants.R_IS_CRITICAL_KEY,IsBadge=true,BadgeFieldType=FieldTypes.SuccessBadgeControl.ToString()},
            new TableDataStructureModel{DataHeaderValue="Link",LinkText = "https://www.google.com/", IsLink = true, IsSearchable = false }
                                 //new TableDataStructureModel{DataField=nameof(PatientMedicationModel.FormattedDate),DataHeader=ResourceConstants.R_END_DATE_KEY, },
        };
    }

    public List<PatientMedicationModel> getTableList(int lastIndex)
    {
        lastIndex += 1;
        dataset = new List<PatientMedicationModel>();
        for (int i = lastIndex; i <= lastIndex + 15; i++)
        {
            PatientMedicationModel model = new PatientMedicationModel
            {
                PatientMedicationID = Guid.NewGuid(),
                MedicationDosesString = "Dose" + i.ToString(),
                ProgramName = "Program Name Test" + i.ToString(),
                ProgramColor = "Test Image " + i.ToString(),
                MedicationImage = "ReadingSourceTypeProviderManual.svg",
                MedicationReminderImage = "ReadingSourceTypeHealthKit.svg",
                FullName = "ReadingSourceTypeGoogleFit.svg",
                MedicationStatusString = GetStatus(false),
                IsCritical = true,
            };
            dataset.Add(model);
        }
        return dataset;
    }


    private string GetStatus(bool isActiveStatus)
    {
        //-- LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_OPEN_TASK_KEY)) ? Constants.BADGE_SUCCESS_CSS : Constants.BADGE_ERROR_CSS)
        string currentStatus = "OPEN";
        string statusStyle;
        if (isActiveStatus)
        {
            statusStyle = Constants.BADGE_ERROR_CSS;
        }
        else
        {
            statusStyle = Constants.BADGE_SUCCESS_CSS;
        }
        return $"<label class ={statusStyle}>&nbsp{currentStatus}</label>";
    }

    private void OnAddEditClick(object? e)
    {
        var medicationData = e as PatientMedicationModel;
        if (medicationData != null)
        {
            Console.WriteLine(medicationData);
        }
        //dialog Options is just an example to show click functionality
        JSRuntime.InvokeVoidAsync("alert", medicationData);
    }
}