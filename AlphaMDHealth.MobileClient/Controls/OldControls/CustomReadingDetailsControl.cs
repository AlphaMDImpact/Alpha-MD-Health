using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using System.Globalization;
using System.Runtime.Serialization;
//using DateTimeIntervalType = OxyPlot.Axes.DateTimeIntervalType;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Control for reading graph and list
/// </summary>
public class CustomReadingDetailsControl : BaseContentView
{
    private long _currentFilterValue;
    private DateTimeOffset? _startDate;
    private DateTimeOffset? _endDate = DateTimeOffset.UtcNow;
    private readonly Grid _filterLayout;
    //todo: private PancakeView _listHeaderView;
    private readonly Grid _summaryLayout;
    private readonly CollectionView _readingList;
    //todo: private PlotView _graphPlot;
    private readonly CustomMessageControl _emptyView;
    private PanGestureRecognizer _panGestureRecognizer;

    private PanGestureRecognizer _emptyViewPanGestureRecognizer;
    private double _currentTranslation;
    private bool _isSwipeInProgress;
    private readonly Grid _mainLayout;
    private readonly Grid _headerLayout;
    private readonly Grid _headerGrid;
    private readonly CustomLabelControl _currentDateRangeLabel;
    private ReadingMetadataUIModel _chartMetadata;
    private bool _isFirstSwipe = true;
    private readonly double _microSwipeMaxLength;
    private ResourceModel _noRecordsResource;
    private readonly double _appPadding = (double)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_APP_PADDING];

    /// <summary>
    /// Type of reading
    /// </summary>
    public string ReadingType { get; set; }

    /// <summary>
    /// Has Healthkit permission or not
    /// </summary>
    public bool HasHealthKitPermission { get; set; }

    /// <summary>
    /// To show Connect button or not;
    /// </summary>
    public bool ShowConnectButton { get; set; }

    /// <summary>
    /// To show Retrieve button
    /// </summary>
    public bool ShowDeviceButton { get; set; }

    /// <summary>
    /// Input chart data
    /// </summary>
    public PatientReadingDTO ChartData { get; set; }

    /// <summary>
    /// Function invoked to fetch data on duration change
    /// </summary>
    public Func<DateTimeOffset?, DateTimeOffset?, string, Task<PatientReadingDTO>> OnDurationChange { get; set; }

    /// <summary>
    /// Event invoked when list item selection is changed
    /// </summary>
    public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

    /// <summary>
    /// Event invoked when list item selection is changed
    /// </summary>
    public event EventHandler<EventArgs> RetrieveDataButtonClicked;

    /// <summary>
    /// Action buttons to be displayed
    /// </summary>
    [DataMember]
    public List<CustomButtonControl> ActionButtons { get; set; }

    /// <summary>
    /// value property
    /// </summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(ChartData), typeof(PatientReadingDTO), typeof(CustomReadingDetailsControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValueChangeProperty);

    private static void ValueChangeProperty(BindableObject bindable, object oldValue, object newValue)
    {
        CustomReadingDetailsControl control = (CustomReadingDetailsControl)bindable;
        if (newValue != null && newValue != oldValue)
        {
            control.ChartData = (PatientReadingDTO)newValue;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await control.LoadUIAsync(false);
            });
        }
    }

    /// <summary>
    /// Control for reading graph and list
    /// </summary>
    public CustomReadingDetailsControl()
    {
        _microSwipeMaxLength = DeviceDisplay.MainDisplayInfo.Width / (Device.Idiom == TargetIdiom.Tablet ? 30d : 22d);
        _mainLayout = new Grid
        {
            Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            }
        };
        _headerLayout = new Grid
        {
            Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }
            }
        };
        _headerGrid = new Grid
        {
            Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            }
        };
        _summaryLayout = _summaryLayout ?? new Grid
        {
            Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowSpacing = _appPadding / 2 - 5,
            ColumnSpacing = _appPadding * 2,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            }
        };
        _filterLayout = new Grid
        {
            Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            ColumnSpacing = 0.5,
            BackgroundColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR],
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }
            }
        };
        _readingList = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                SnapPointsType = SnapPointsType.None,
                SnapPointsAlignment = SnapPointsAlignment.Center
            },
            ItemTemplate = new PatientRedingDataTemplateSelector
            {
                TwoRowList = new DataTemplate(() =>
                {
                    return new ContentView { Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_SELECTED_CONTENT_STYLE], Content = new TwoRowReadingViewCell { Margin = GenericMethods.GetPlatformSpecificValue(new Thickness(1, 0, 2, 0), new Thickness(0), new Thickness(1, 1, 1, 0)) } };
                }),
                SingleRowList = new DataTemplate(() =>
                {
                    return new ContentView { Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_SELECTED_CONTENT_STYLE], Content = new SingleRowReadingViewCell { Margin = GenericMethods.GetPlatformSpecificValue(new Thickness(1, 0, 2, 0), new Thickness(0), new Thickness(1, 1, 1, 0)) } };
                })
            },
            Header = _headerGrid
        };
        _emptyView = new CustomMessageControl(false, false)
        {
            ControlResourceKey = ResourceConstants.R_NOT_FOUND_MEASUREMENT_TEXT_KEY
        };
        _currentDateRangeLabel = new CustomLabelControl(LabelType.PrimarySmallLeft) { Margin = new Thickness(0, _appPadding / 2, 0, 15) };
        Content = _mainLayout;
    }

    /// <summary>
    /// Loads graph UI along with chart data
    /// </summary>
    /// <returns>Renders UI</returns>
    public async Task LoadUIAsync(bool isRefreshed)
    {
        if (ChartData == null || isRefreshed)
        {
            await FetchChartDataAsync().ConfigureAwait(true);
        }
        if (GenericMethods.IsListNotEmpty(ChartData?.ChartMetaData))
        {
            _chartMetadata = ChartData.ChartMetaData[0];
        }
        // IsActive is used to identify if for this VitalType, the given user has any data
        if (ChartData != null && ChartData.ErrCode == ErrorCode.OK && ChartData.IsActive && (_chartMetadata?.ShowInGraph ?? false))
        {
            if (_mainLayout.Children.Contains(_emptyView))
            {
                _mainLayout.Children.Remove(_emptyView);
            }
            GenerateGraphLayout();
        }
        SetDetailContents();
        _readingList.SelectionChanged -= ReadingListItem_Clicked;
        _readingList.SelectionChanged += ReadingListItem_Clicked;
    }

    private async Task FetchChartDataAsync()
    {
        ChartData = await OnDurationChange.Invoke(null, null, ReadingType).ConfigureAwait(true);
        if (ChartData?.ErrCode == ErrorCode.OK)
        {
            if (_currentFilterValue != ChartData.FilterOptions.FirstOrDefault(x => x.IsDefault).ParentOptionID)
            {
                _currentFilterValue = ChartData.FilterOptions.FirstOrDefault(x => x.IsDefault).ParentOptionID;
                CalculateStartEndDays();
                UpdateStartEndDates(_startDate.Value, _endDate.Value);
            }
            if (!ChartData.ChartMetaData[0].ShowSummary)
            {
                if (ChartData.ChartMetaData[0].IsActive && ShowDeviceButton)
                {
                    CustomButtonControl getDataBtn = new CustomButtonControl(ButtonType.TransparentWithMargin)
                    {
                        Text = GetResourceValueByKey(ResourceConstants.R_RETRIEVE_DATA_BUTTON_TEXT_KEY),
                        ImageSource = ImageSource.FromResource(ImageConstants.I_BLUETOOTH_PNG),//todo: (double)AppImageSize.ImageSizeXS, (double)AppImageSize.ImageSizeXS, default),
                        ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, GenericMethods.GetPlatformSpecificValue(5, 0, 0)),
                        Margin = new Thickness(0),
                        VerticalOptions = LayoutOptions.Center,
                        FlowDirection = FlowDirection.LeftToRight
                    };
                    ActionButtons = new List<CustomButtonControl>
                    {
                        getDataBtn
                    };
                    getDataBtn.Clicked += OnRefreshButtonClick;
                }
                GenerateFilterLayout();
                UpdateSummaryLayout();
            }
        }
    }

    /// <summary>
    /// Unloads UI and unregisters attached events
    /// </summary>
    public Task UnloadUIAsync()
    {
        _readingList.SelectionChanged -= ReadingListItem_Clicked;
        if (_panGestureRecognizer != null)
        {
            _panGestureRecognizer.PanUpdated -= GraphPlot_PanUpdated;
        }
        if (_emptyViewPanGestureRecognizer != null)
        {
            _emptyViewPanGestureRecognizer.PanUpdated -= GraphPlot_PanUpdated;
        }
        return Task.CompletedTask;
    }

    private void SetEmptyViewAncConnectButtons(ResourceModel recordsResource)
    {
        _noRecordsResource = _noRecordsResource ?? new ResourceModel { ResourceKey = recordsResource.ResourceKey, ResourceValue = recordsResource.ResourceValue, PlaceHolderValue = recordsResource.PlaceHolderValue, InfoValue = recordsResource.InfoValue };
        recordsResource.ResourceValue = string.Format(CultureInfo.InvariantCulture, _noRecordsResource.ResourceValue, ChartData.ChartMetaData[0].Reading);
        if (!IsAllowedManualAdd())
        {
            recordsResource.PlaceHolderValue = string.Empty;
        }
        if (ShowConnectButton)
        {
            LoadEmptyViewWithConnectButton(recordsResource);
        }
        recordsResource.InfoValue = string.Empty;
        _emptyView.PageResources = PageResources;
    }

    private void LoadEmptyViewWithConnectButton(ResourceModel recordsResource)
    {
        if (!_chartMetadata.IsActive || HasHealthKitPermission)
        {
            if (!recordsResource.PlaceHolderValue.Contains(_noRecordsResource.InfoValue))
            {
                recordsResource.PlaceHolderValue += string.IsNullOrWhiteSpace(recordsResource.PlaceHolderValue) ? _noRecordsResource.InfoValue : string.Concat(Constants.NEW_LINE_CONSTANT, _noRecordsResource.InfoValue);
            }
            _emptyView.Actions = null;
            _emptyView.OnActionClicked = null;
        }
        else
        {
            string healthAppName = GenericMethods.GetPlatformSpecificValue(GetResourceValueByKey(ResourceConstants.R_IHEALTH_TEXT_KEY), GetResourceValueByKey(ResourceConstants.R_GOOGLE_FIT_TEXT_KEY), "");
            string temp = string.Concat(IsAllowedManualAdd() ? GetResourceByKey(ResourceConstants.R_NOT_FOUND_MEASUREMENT_2_TEXT_KEY).PlaceHolderValue : string.Empty,
               Constants.STRING_SPACE, GetResourceValueByKey(ResourceConstants.R_NOT_FOUND_MEASUREMENT_2_TEXT_KEY), Constants.STRING_SPACE,
               string.Format(CultureInfo.InvariantCulture, GetResourceValueByKey(ResourceConstants.R_CONNECT_TO_HEALTHKIT_MESSAGE_KEY), healthAppName));
            if (!recordsResource.PlaceHolderValue.Contains(temp))
            {
                recordsResource.PlaceHolderValue += temp;
            }
        }
        if (_chartMetadata.IsActive && ShowDeviceButton)
        {
            string temp = string.Concat(GetResourceByKey(ResourceConstants.R_NOT_FOUND_MEASUREMENT_3_TEXT_KEY).PlaceHolderValue, GetResourceValueByKey(ResourceConstants.R_NOT_FOUND_MEASUREMENT_3_TEXT_KEY));
            if (!recordsResource.PlaceHolderValue.Contains(temp))
            {
                recordsResource.PlaceHolderValue += temp;
            }
        }
        recordsResource.InfoValue = string.Empty;
        _emptyView.PageResources = PageResources;
        if (_chartMetadata.IsActive && !HasHealthKitPermission)
        {
            _emptyView.Actions = new[] { new OptionModel { GroupName = ButtonType.PrimaryWithMargin.ToString(), OptionText = ResourceConstants.R_CONNECT_BUTTON_TEXT_KEY } };
            _emptyView.OnActionClicked = _emptyView.OnActionClicked ?? OnMessgeViewActionClicked;
        }
        _mainLayout.Add(_emptyView, 0, 2);
        _emptyViewPanGestureRecognizer = new PanGestureRecognizer();
        _emptyViewPanGestureRecognizer.PanUpdated += GraphPlot_PanUpdated;
        Microsoft.Maui.Controls.Application.Current.On<iOS>().SetPanGestureRecognizerShouldRecognizeSimultaneously(true);
        _emptyView.GestureRecognizers.Add(_emptyViewPanGestureRecognizer);
    }

    private void OnRefreshButtonClick(object sender, EventArgs e)
    {
        RetrieveDataButtonClicked?.Invoke(sender, e);
    }

    private bool IsAllowedManualAdd()
    {
        return _chartMetadata != null && _chartMetadata.IsActive && _chartMetadata.AllowManualAdd;
    }

    /// <summary>
    /// Button click event to perform some action on it
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">event</param>
    protected virtual async void OnMessgeViewActionClicked(object sender, int e)
    {
        _emptyView.OnActionClicked -= OnMessgeViewActionClicked;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new HealthAccountConnectPage()).ConfigureAwait(true);
    }

    private void SetDetailContents()
    {
        if (_chartMetadata != null && !_chartMetadata.ShowSummary)
        {
            _headerLayout.Add(_currentDateRangeLabel, 0, 1);
            if (_headerLayout != null && !_mainLayout.Children.Contains(_headerLayout))
            {
                _mainLayout.Add(_headerLayout, 0, 1);
            }
            //todo: 
            //if (_graphPlot != null)
            //{
            //    _graphPlot.HeightRequest = _chartMetadata.Height * App._essentials.GetPreferenceValue(LibStorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0);
            //}
            _readingList.ItemsSource = ChartData.ListData;
            SetDetailsView();
            _panGestureRecognizer = new PanGestureRecognizer();
            _panGestureRecognizer.PanUpdated += GraphPlot_PanUpdated;
            Microsoft.Maui.Controls.Application.Current.On<iOS>().SetPanGestureRecognizerShouldRecognizeSimultaneously(true);
            _headerGrid.GestureRecognizers.Add(_panGestureRecognizer);
        }
    }

    private void SetDetailsView()
    {
        if (ChartData.ListData?.Count > 0 || ChartData.GraphData?.Count > 0)
        {
            _readingList.Footer = new Label();
            _emptyView.IsVisible = false;
            if (!_mainLayout.Children.Contains(_readingList))
            {
                _mainLayout.Add(_readingList, 0, 2);
                //added for Lst cutting at bottom
                _readingList.Footer = new Label();
            }
            _readingList.IsVisible = true;
            if (ChartData.ListData?.Count > 0)
            {
                SetListHeaders();
            }
        }
        else
        {
            _readingList.IsVisible = false;
            SetEmptyViewAncConnectButtons(GetResourceByKey(ResourceConstants.R_NOT_FOUND_MEASUREMENT_TEXT_KEY));
            if (_emptyView != null && !_mainLayout.Children.Contains(_emptyView))
            {
                _mainLayout.Add(_emptyView, 0, 2);
                _emptyViewPanGestureRecognizer = new PanGestureRecognizer();
                _emptyViewPanGestureRecognizer.PanUpdated += GraphPlot_PanUpdated;
                Microsoft.Maui.Controls.Application.Current.On<iOS>().SetPanGestureRecognizerShouldRecognizeSimultaneously(true);
                _emptyView.GestureRecognizers.Add(_emptyViewPanGestureRecognizer);
            }
            _emptyView.IsVisible = true;
        }
    }

    private void GenerateFilterLayout()
    {
        _filterLayout.Children?.Clear();
        _filterLayout.ColumnDefinitions?.Clear();
        int columnIndex = 0;
        double width = _mainLayout.Width / ChartData.FilterOptions.Count;
        foreach (var filter in ChartData.FilterOptions)
        {
            if (columnIndex > 0)
            {
                _filterLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Absolute) });
                columnIndex++;
            }
            _filterLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width, GridUnitType.Star) });
            var filterButton = new CustomButtonControl(ButtonType.TabButton)
            {
                Text = filter.OptionText,
                Padding = new Thickness(0),
                StyleId = filter.ParentOptionID.ToString(CultureInfo.InvariantCulture)
            };
            if (_currentFilterValue == filter.ParentOptionID)
            {
                SetBackgroundColor(filterButton);
            }
            else
            {
                filterButton.BackgroundColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
            }
            filterButton.Clicked += FilterButton_Clicked;
            _filterLayout.Add(filterButton, columnIndex, 0);
            columnIndex++;
        }
        _mainLayout.Add(new ContentView
        {
            Content = new ContentView //todo: PancakeView
            {
                Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_PANCAKE_TAB_STYLE],
                Margin = 0,
                Content = _filterLayout,
                HorizontalOptions = LayoutOptions.Center
            },
            Padding = new Thickness(0, Convert.ToDouble(Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.CurrentCulture), 0, ShellMasterPage.CurrentShell.CurrentPage.ToString().EndsWith(Constants.PATIENTS_PAGE_CONSTANT, StringComparison.InvariantCultureIgnoreCase) ? 0 : _appPadding),
            Margin = new Thickness(ShellMasterPage.CurrentShell.CurrentPage != null && ShellMasterPage.CurrentShell.CurrentPage.ToString().EndsWith(Constants.PATIENTS_PAGE_CONSTANT, StringComparison.InvariantCultureIgnoreCase) ? 0 : -_appPadding, 0),
        }, 0, ChartData.ChartMetaData[0].ShowTopDurationFilter ? 0 : 3);
    }

    private static void SetBackgroundColor(CustomButtonControl filterButton)
    {
        filterButton.BackgroundColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
        filterButton.TextColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
    }

    private async void FilterButton_Clicked(object sender, EventArgs e)
    {
        CustomButtonControl filterButton = (CustomButtonControl)sender;
        filterButton.Clicked -= FilterButton_Clicked;
        AppHelper.ShowBusyIndicator = true;
        _isFirstSwipe = true;
        foreach (var button in _filterLayout.Children)
        {
            CustomButtonControl filter = (CustomButtonControl)button;
            filter.BackgroundColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
            filter.TextColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
        }
        SetBackgroundColor(filterButton);
        _currentFilterValue = Convert.ToInt64(filterButton.StyleId, CultureInfo.InvariantCulture);
        ChartData.FilterOptions.FirstOrDefault(x => x.ParentOptionID == _currentFilterValue).IsSelected = true;
        if (CalculateStartEndDays())
        {
            UpdateStartEndDates(_startDate.Value, _endDate.Value);
            ChartData = await OnDurationChange.Invoke(_startDate, _endDate.Value.ToUniversalTime(), ReadingType).ConfigureAwait(true);
            await LoadUIAsync(false).ConfigureAwait(true);
            UpdateSummaryLayout();
        }
        AppHelper.ShowBusyIndicator = false;
        filterButton.Clicked += FilterButton_Clicked;
    }

    private bool CalculateStartEndDays()
    {
        DateTimeOffset? startDate;
        DateTimeOffset? endDate;
        if (_currentFilterValue == -1)
        {
            GenericMethods.TryCalculateStartEndDate((int)_currentFilterValue, GenericMethods.GetUtcDateTime.ToLocalTime(), out startDate, out endDate);
        }
        else
        {
            endDate = GenericMethods.GetStartEndOfDay(false, DateTimeOffset.UtcNow).ToLocalTime();
            startDate = GenericMethods.GetStartEndOfDay(true, endDate.Value.AddDays(-_currentFilterValue + 1)).ToLocalTime();
        }
        if (_startDate == startDate && _endDate == endDate)
        {
            return false;
        }
        else
        {
            _startDate = startDate;
            _endDate = endDate;
            return true;
        }
    }

    private void UpdateStartEndDates(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        LibSettings.TryGetDateFormatSettings(_pageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        _currentDateRangeLabel.Text = GenericMethods.GetRangeLabel(dayFormat, monthFormat, yearFormat, startDate, endDate);
    }

    private void UpdateSummaryLayout()
    {
        _summaryLayout.Children.Clear();
        _summaryLayout.ColumnDefinitions.Clear();
        int columnIndex = 0;
        if (ChartData.SummaryDataOptions?.Count > 0)
        {
            foreach (OptionModel summaryItem in ChartData.SummaryDataOptions)
            {
                _summaryLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                _summaryLayout.Add(new CustomLabelControl(LabelType.SecondrySmallLeft) { Text = summaryItem.GroupName }, columnIndex, 0);
                if (summaryItem.GroupName == "Current")
                {
                    _summaryLayout.Add(new Label
                    {
                        Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_DEFAULT_LEFT_LABEL_STYLE],
                        FormattedText = new FormattedString
                        {
                            Spans = { new Span { Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_SPAN_PRIMARY_TEXT_COLOR_LARGE_STYLE], Text = summaryItem.OptionText },
                            new Span { Text = Constants.STRING_SPACE },
                            new Span { Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_SPAN_PRIMARY_TEXT_COLOR_SMALL_STYLE], Text = summaryItem.ParentOptionText }
                        }
                        }
                    }, columnIndex, 1);
                }
                else
                {
                    _summaryLayout.Add(new Label
                    {
                        Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_DEFAULT_LEFT_LABEL_STYLE],
                        FormattedText = new FormattedString
                        {
                            Spans = { new Span { Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_SPAN_PRIMARY_TEXT_COLOR_LARGE_STYLE], Text = summaryItem.OptionText , FontSize = 14},
                            new Span { Text = Constants.STRING_SPACE },
                            new Span { Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[StyleConstants.ST_SPAN_PRIMARY_TEXT_COLOR_SMALL_STYLE], Text = summaryItem.ParentOptionText }
                        }
                        }
                    }, columnIndex, 1);
                }
                columnIndex++;
            }
        }
        _summaryLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        columnIndex++;
        if (ActionButtons != null)
        {
            foreach (var button in ActionButtons)
            {
                _summaryLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                _summaryLayout.Add(button, columnIndex, 0);
                Grid.SetRowSpan(button, 2);
                columnIndex++;
            }
        }
        if (!_headerLayout.Children.Contains(_summaryLayout))
        {
            _headerLayout.Add(_summaryLayout, 0, 0);
        }
    }

    private void GenerateGraphLayout()
    {
        if (_chartMetadata.ShowSummary && (!_chartMetadata.ShowInGraph))
        {
            return;
        }
        ClearExistingLayout();
        //todo: 
        //var model = new PlotModel
        //{
        //    PlotAreaBorderColor = OxyColors.Transparent,
        //    PlotAreaBorderThickness = new OxyThickness(0),
        //    Padding = new OxyThickness(0)
        //};
        //model.PlotMargins = new OxyThickness(model.PlotMargins.Left, model.PlotMargins.Top, 0, model.PlotMargins.Bottom);
        //_graphPlot = new PlotView
        //{
        //    Model = model
        //};
        //model.Axes.Clear();
        //model.ResetAllAxes();
        //GenerateXAxis(model);
        //LinearAxis linearAxis = new LinearAxis
        //{
        //    Key = LibConstants.YKey,
        //    Position = AxisPosition.Left,
        //    IsAxisVisible = _chartMetadata.ShowAxes
        //};
        //if (_chartMetadata.ChartType == (short)GraphType.Bar)
        //{
        //    linearAxis.Minimum = 0;
        //}
        //else
        //{
        //    linearAxis.MaximumPadding = linearAxis.MinimumPadding = 0.1;
        //}
        //SetAxisStyle(linearAxis);
        //model.Axes.Add(linearAxis);
        //GeneratePlots(model);
        //SetRanges(model);
        if (_chartMetadata.ShowSummary)
        {
            var contentGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition{ Height = new GridLength(1, GridUnitType.Star)}
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition{ Width = new GridLength(1, GridUnitType.Star)}
                },
            };
            //todo: contentGrid.Add(_graphPlot, 0, 0);
            contentGrid.Add(new BoxView
            {
                //todo: BackgroundColor = Color.Transparent
            }, 0, 0);
            Content = contentGrid;
        }
        else
        {
            AddGraphViewAndUpdateDateLabel();
        }
    }

    private void ClearExistingLayout()
    {
        //todo: 
        //if (_headerGrid != null && _headerGrid.Children.Count > 0 && _headerGrid.Children.Contains(_graphPlot))
        //{
        //    _headerGrid?.Children?.Remove(_graphPlot);
        //    _graphPlot = null;
        //}
    }

    private void AddGraphViewAndUpdateDateLabel()
    {
        if (_chartMetadata.ShowInGraph)
        {
            //todo: 
            //_graphPlot.Margin = new Thickness(0, _appPadding / 2, 0, 20);
            //_headerGrid.Add(_graphPlot, 0, 0);
        }
        else
        {
            if (_currentFilterValue == -1 && _chartMetadata.ShowInData && GenericMethods.IsListNotEmpty(ChartData.ListData))
            {
                DateTimeOffset? maxDate = GenericMethods.GetStartEndOfDay(false, ChartData.ListData.Max(x => x.ReadingDateTime.Value));
                DateTimeOffset? minDate = GenericMethods.GetStartEndOfDay(true, ChartData.ListData.Min(x => x.ReadingDateTime.Value));
                var duration = (long)Math.Ceiling((maxDate - minDate).Value.TotalDays);
                var approxDuration = ChartData.FilterOptions.OrderBy(x => x.ParentOptionID).FirstOrDefault(x => x.ParentOptionID >= duration)?.ParentOptionID;
                if (approxDuration != null)
                {
                    minDate = GenericMethods.GetStartEndOfDay(true, maxDate.Value.AddDays(-approxDuration.Value + 1));
                }
                UpdateStartEndDates(minDate.Value.ToUniversalTime(), maxDate.Value.ToUniversalTime());
            }
        }
    }

    //todo: 
    //private void GeneratePlots(PlotModel model)
    //{
    //    OxyColor lineColor = GetGraphLineColor();
    //    if (ChartData.GraphData?.Count > 0)
    //    {
    //        switch ((GraphType)ChartData.ChartMetaData[0].ChartType)
    //        {
    //            case GraphType.Line:
    //                foreach (var datatem in ChartData.GraphData)
    //                {
    //                    var lineSeries = new LineSeries
    //                    {
    //                        DataFieldX = nameof(PatientReadingUIModel.ReadingLocalDateTime),
    //                        DataFieldY = nameof(PatientReadingUIModel.ReadingValue),
    //                        ItemsSource = datatem,
    //                        Color = lineColor,
    //                        XAxisKey = LibConstants.XKey,
    //                        YAxisKey = LibConstants.YKey
    //                    };
    //                    model.Series.Add(lineSeries);
    //                    SetAxisMarkers(model, lineColor, datatem, lineSeries);
    //                }
    //                break;
    //            case GraphType.Bar:
    //                foreach (var datatem in ChartData.GraphData)
    //                {
    //                    model.Series.Add(new BarSeries
    //                    {
    //                        ValueField = nameof(PatientReadingUIModel.ReadingValue),
    //                        ItemsSource = datatem,
    //                        FillColor = lineColor,
    //                        XAxisKey = LibConstants.YKey,
    //                        YAxisKey = LibConstants.XKey,
    //                        StrokeColor = lineColor
    //                    });
    //                }
    //                break;
    //            //case GraphType.Circular:
    //            //case GraphType.Spline:
    //            //case GraphType.Slider:
    //            default:
    //                // Not supported
    //                break;
    //        }
    //    }
    //}

    //private OxyColor GetGraphLineColor()
    //{
    //    return string.IsNullOrWhiteSpace(ChartData.ChartMetaData[0].PlotColor)
    //                                        ? ((Color)Microsoft.Maui.Controls.Application.Current.Resources[LibStyleConstants.ST_PRIMARY_APP_COLOR]).ToOxyColor()
    //                                        : OxyColor.Parse(ChartData.ChartMetaData[0].PlotColor);
    //}

    //private void SetAxisMarkers(PlotModel model, OxyColor lineColor, List<PatientReadingUIModel> datatem, LineSeries lineSeries)
    //{
    //    if (_chartMetadata.ShowSummary)
    //    {
    //        model.Series.Add(new ScatterSeries
    //        {
    //            MarkerSize = 5,
    //            DataFieldX = nameof(PatientReadingUIModel.ReadingLocalDateTime),
    //            DataFieldY = nameof(PatientReadingUIModel.ReadingValue),
    //            ItemsSource = new List<PatientReadingUIModel> { datatem.LastOrDefault() },
    //            MarkerStrokeThickness = 1,
    //            MarkerType = MarkerType.Circle,
    //            MarkerFill = lineColor,
    //            MarkerStroke = OxyColor.Parse(((Color)Microsoft.Maui.Controls.Application.Current.Resources[LibStyleConstants.ST_CONTROL_BACKGROUND_COLOR]).ToHex())
    //        });
    //    }
    //    else
    //    {
    //        lineSeries.MarkerType = MarkerType.Circle;
    //        lineSeries.MarkerSize = 5;
    //        lineSeries.MarkerStroke = lineSeries.MarkerFill = lineColor;
    //    }
    //}

    //private void GenerateXAxis(PlotModel model)
    //{
    //    Axis xAxis = null;
    //    switch ((GraphType)_chartMetadata.ChartType)
    //    {
    //        case GraphType.Line:
    //            xAxis = new DateTimeAxis();
    //            break;
    //        case GraphType.Bar:
    //            xAxis = new CategoryAxis();
    //            break;
    //        //case GraphType.Circular:
    //        //case GraphType.Spline:
    //        //case GraphType.Slider:
    //        //break;
    //        default:
    //            // Not supported
    //            break;
    //    }
    //    if (xAxis != null)
    //    {
    //        xAxis.IsAxisVisible = _chartMetadata.ShowAxes;
    //        xAxis.Key = LibConstants.XKey;
    //        xAxis.Position = AxisPosition.Bottom;
    //        SetAxisStyle(xAxis);
    //        xAxis.MinorGridlineColor = xAxis.TextColor;
    //        xAxis.MinorGridlineThickness = 0.5;
    //        xAxis.MinorTickSize = xAxis.MajorTickSize;
    //        if (_chartMetadata.ShowSummary)
    //        {
    //            if (xAxis is DateTimeAxis)
    //            {
    //                xAxis.MaximumPadding = xAxis.MinimumPadding = 0.1;
    //            }
    //        }
    //        else
    //        {
    //            SetLinePlotAxes(xAxis);
    //        }
    //        model.Axes.Add(xAxis);
    //    }
    //}

    //private void SetAxisStyle(Axis axis)
    //{
    //    axis.AxisTitleDistance = 10;
    //    axis.TitleColor = OxyColor.Parse(((Color)Microsoft.Maui.Controls.Application.Current.Resources[LibStyleConstants.ST_TERTIARY_TEXT_COLOR]).ToHex());
    //    axis.TextColor = OxyColor.Parse(((Color)Microsoft.Maui.Controls.Application.Current.Resources[LibStyleConstants.ST_SECONDARY_TEXT_COLOR]).ToHex());
    //    axis.TitleFontSize = axis.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)) * 0.9;
    //    axis.TicklineColor = axis.TextColor;
    //    axis.AxislineColor = axis.TextColor;
    //    axis.AxislineThickness = 1;
    //    axis.AxislineStyle = LineStyle.Solid;
    //    axis.MajorGridlineColor = axis.TextColor;
    //    axis.MajorGridlineThickness = 0.5;
    //    axis.MinorTickSize = 0;
    //    axis.IsZoomEnabled = false;
    //    axis.IsPanEnabled = false;
    //}

    //private void SetLinePlotAxes(Axis axis)
    //{
    //    DateTime maxDate;
    //    DateTime minDate;
    //    long plotDays;
    //    if (_currentFilterValue == -1 && LibGenericMethods.IsListNotEmpty(ChartData.GraphData) && LibGenericMethods.IsListNotEmpty(ChartData.ListData) && (ChartData.GraphData?.Any(x => x.Count > 0) ?? false))
    //    {
    //        maxDate = axis is CategoryAxis ? ChartData.ListData.Max(x => x.ReadingLocalDateTime).Date : ChartData.ListData.Max(x => x.ReadingLocalDateTime).Date.AddHours(24).AddTicks(-1);
    //        minDate = ChartData.ListData.Min(x => x.ReadingLocalDateTime).Date;
    //        plotDays = (long)Math.Ceiling((maxDate - minDate).TotalDays);
    //        var approxDuration = ChartData.FilterOptions.OrderBy(x => x.ParentOptionID).FirstOrDefault(x => x.ParentOptionID >= plotDays)?.ParentOptionID;
    //        if (approxDuration != null)
    //        {
    //            //minDate = maxDate.Date.AddDays(-approxDuration.Value + 1);
    //            //plotDays = (long)Math.Ceiling((maxDate - minDate).TotalDays);
    //        }
    //        UpdateStartEndDates(minDate.ToUniversalTime(), maxDate.ToUniversalTime());
    //    }
    //    else
    //    {
    //        maxDate = _endDate.Value.LocalDateTime;
    //        minDate = _startDate.Value.LocalDateTime;
    //        plotDays = _currentFilterValue;
    //    }

    //    double stepSize = 2;
    //    double minorStepSize = 1;
    //    DateTimeIntervalType timeunit = DateTimeIntervalType.Hours;
    //    string format = string.Empty;
    //    if (plotDays <= 1)
    //    {
    //        if (_currentFilterValue == -1)
    //        {
    //            minDate = minDate.Date;
    //            maxDate = minDate.AddDays(1).AddTicks(-1);
    //        }
    //        timeunit = DateTimeIntervalType.Hours;
    //        format = LibGenericMethods.GetDateTimeFormat(DateTimeType.Hour, string.Empty, string.Empty, string.Empty);
    //        stepSize = 3.0 / LibConstants.PLOT_DAYS_HOURS;
    //        minorStepSize = 1.0 / LibConstants.PLOT_DAYS_HOURS;
    //    }
    //    else if (plotDays <= LibConstants.PLOT_DAYS_WEEKLY)
    //    {
    //        timeunit = DateTimeIntervalType.Days;
    //        format = LibGenericMethods.GetDateTimeFormat(DateTimeType.Day, string.Empty, string.Empty, string.Empty);
    //        stepSize = minorStepSize = 1;
    //    }
    //    else if (plotDays <= LibConstants.PLOT_DAYS_BI_WEEKLY)
    //    {
    //        timeunit = DateTimeIntervalType.Days;
    //        format = LibGenericMethods.GetDateTimeFormat(DateTimeType.Day, string.Empty, string.Empty, string.Empty);
    //        stepSize = 2;
    //        minorStepSize = 1;
    //    }
    //    else if (plotDays <= LibConstants.PLOT_DAYS_MONTLY)
    //    {
    //        timeunit = DateTimeIntervalType.Weeks;
    //        format = LibGenericMethods.GetDateTimeFormat(DateTimeType.DayMonth, ChartData.ChartMetaData[0].DayFormat, ChartData.ChartMetaData[0].MonthFormat, string.Empty);
    //        stepSize = minorStepSize = LibConstants.PLOT_DAYS_WEEKLY;
    //    }
    //    else if (plotDays <= LibConstants.PLOT_DAYS_QUARTERLY)
    //    {
    //        timeunit = DateTimeIntervalType.Weeks;
    //        format = LibGenericMethods.GetDateTimeFormat(DateTimeType.DayMonth, ChartData.ChartMetaData[0].DayFormat, ChartData.ChartMetaData[0].MonthFormat, string.Empty);
    //        stepSize = 2 * LibConstants.PLOT_DAYS_WEEKLY;
    //        minorStepSize = LibConstants.PLOT_DAYS_WEEKLY;
    //    }
    //    else if (plotDays > LibConstants.PLOT_DAYS_QUARTERLY && plotDays <= LibConstants.PLOT_DAYS_YEARLY)
    //    {
    //        timeunit = DateTimeIntervalType.Months;
    //        format = ChartData.ChartMetaData[0].MonthFormat;
    //        stepSize = minorStepSize = LibConstants.PLOT_DAYS_MONTHLY_STEP;
    //    }
    //    else
    //    {
    //        if (plotDays > LibConstants.PLOT_DAYS_YEARLY)
    //        {
    //            timeunit = DateTimeIntervalType.Years;
    //            format = ChartData.ChartMetaData[0].YearFormat;
    //            stepSize = LibConstants.PLOT_DAYS_YEARLY_STEP * Math.Ceiling(Math.Ceiling(plotDays / LibConstants.PLOT_DAYS_YEARLY_STEP) / 10);
    //            minorStepSize = LibConstants.PLOT_DAYS_YEARLY_STEP;
    //        }
    //    }
    //    axis.StringFormat = format;
    //    if (axis is DateTimeAxis dateTimeAxis)
    //    {
    //        dateTimeAxis.IntervalType = timeunit;
    //        axis.MajorStep = stepSize;
    //        axis.MinorStep = minorStepSize;
    //        double padding = stepSize / 2;
    //        var maxValue = DateTimeAxis.ToDouble(maxDate.AddDays(padding));
    //        var minValue = DateTimeAxis.ToDouble(minDate.AddDays(-padding));
    //        axis.Minimum = minValue;
    //        axis.Maximum = maxValue;
    //    }
    //    else
    //    {
    //        if (axis is CategoryAxis categoryAxis && LibGenericMethods.IsListNotEmpty(ChartData.GraphData))
    //        {
    //            categoryAxis.MajorStep = plotDays <= 1 ? 3 : 1;
    //            categoryAxis.MajorTickSize = 0;
    //            categoryAxis.ItemsSource = ChartData.GraphData[0]?.Select(x => plotDays < 2 ? x.ReadingLocalDateTime : x.ReadingLocalDateTime.Date);
    //        }
    //    }
    //}

    //private void SetRanges(PlotModel model)
    //{
    //    foreach (var metaData in ChartData.ChartMetaData)
    //    {
    //        if (!_chartMetadata.ShowSummary && !string.IsNullOrWhiteSpace(metaData.NormalBandColor))
    //        {
    //            OxyColor bandColor = OxyColor.FromAColor(150, OxyColor.Parse(metaData.NormalBandColor));
    //            model.Annotations.Add(new RectangleAnnotation
    //            {
    //                MinimumY = metaData.NormalMinValue.Value,
    //                MaximumY = metaData.NormalMaxValue.Value,
    //                Layer = AnnotationLayer.BelowSeries,
    //                Fill = bandColor,
    //                Stroke = bandColor,
    //                StrokeThickness = 1
    //            });
    //        }
    //        if (metaData.TargetMaxValue > 0 && metaData.TargetMinValue > 0 && !string.IsNullOrWhiteSpace(metaData.TargetBandColor))
    //        {
    //            OxyColor bandColor = OxyColor.FromAColor(150, OxyColor.Parse(metaData.TargetBandColor));
    //            model.Annotations.Add(new RectangleAnnotation
    //            {
    //                MaximumY = metaData.TargetMaxValue.Value,
    //                MinimumY = metaData.TargetMinValue.Value,
    //                Layer = AnnotationLayer.BelowSeries,
    //                Fill = bandColor,
    //                Stroke = bandColor,
    //                StrokeThickness = 1
    //            });
    //        }
    //    }
    //}

    private async void GraphPlot_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (!_isSwipeInProgress)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    _currentTranslation = e.TotalX;
                    break;
                case GestureStatus.Completed:
                    // If all movements(regardless of direction) was less than treshold: fire microswipe event
                    if (Math.Abs(_currentTranslation) >= _microSwipeMaxLength)
                    {
                        await HandleSwipeDataUpdateAsync();
                    }
                    break;
                case GestureStatus.Started:
                    break;
                case GestureStatus.Canceled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _isSwipeInProgress = false;
        }
        //if (!_isSwipeInProgress)
        //{
        //    if (e.StatusType == GestureStatus.Running)
        //    {
        //        _currentTranslation = e.TotalX;
        //    }

        //    if (DeviceInfo.Platform == DevicePlatform.Android && e.StatusType == GestureStatus.Running && Math.Abs(_currentTranslation) > (0.25 * _mainLayout.Width))
        //    {
        //        await HandleSwipeDataUpdateAsync();
        //    }
        //    else if (e.StatusType == GestureStatus.Completed && Math.Abs(_currentTranslation) > (0.25 * _mainLayout.Width))
        //    {
        //        await HandleSwipeDataUpdateAsync();
        //    }
        //    else
        //    {
        //        // Do nothing
        //    }
        //    _isSwipeInProgress = false;
        //}
    }

    private int GetDirection(int direction)
    {
        return AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? direction : -direction;
    }

    private async Task HandleSwipeDataUpdateAsync()
    {
        int direction = _currentTranslation > 0 ? GetDirection(-1) : GetDirection(1);
        if (_currentFilterValue != -1 && ((direction == -1 && _startDate > GenericMethods.GetDefaultDateTime) || (direction == 1 && _endDate < DateTimeOffset.UtcNow)))
        {
            AppHelper.ShowBusyIndicator = true;
            //todo:
            //if (_graphPlot != null)
            //{
            //    MainThread.BeginInvokeOnMainThread(() =>
            //    {
            //        _graphPlot.TranslationX = direction * _graphPlot.Width;
            //        _ = _graphPlot.TranslateTo(0, _graphPlot.TranslationY);
            //    });
            //}
            _currentTranslation = 0;
            if (_isFirstSwipe)
            {
                _isFirstSwipe = false;
                GenericMethods.TryCalculateStartEndDate((int)_currentFilterValue, _startDate.Value.AddTicks(-1), out _startDate, out _endDate);
            }
            else
            {
                DateTimeOffset newStart;
                newStart = direction < 0 ? _startDate.Value.AddDays(direction) : GetNewStartDate(direction);
                GenericMethods.TryCalculateStartEndDate((int)_currentFilterValue, newStart, out _startDate, out _endDate);
            }
            UpdateStartEndDates(_startDate.Value, _endDate.Value);
            ChartData = await OnDurationChange.Invoke(_startDate, _endDate, ReadingType);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // _panGestureRecognizer.PanUpdated -= GraphPlot_PanUpdated;
                // _headerGrid.GestureRecognizers.Remove(_panGestureRecognizer);
                await LoadUIAsync(false).ConfigureAwait(true);
                UpdateSummaryLayout();
            });
            AppHelper.ShowBusyIndicator = false;
        }
    }

    private DateTimeOffset GetNewStartDate(int direction)
    {
        /*
         _currentFilterValue < LibConstants.PLOT_DAYS_QUARTERLY && _currentFilterValue > LibConstants.PLOT_DAYS_MONTLY
        this condition is for QUARTER period handling, since we don't know when to start/end like we have fix Day, Week, Month and Year handling
        */
        return (_currentFilterValue < Constants.PLOT_DAYS_QUARTERLY && _currentFilterValue > Constants.PLOT_DAYS_MONTLY) ?
                                    _endDate.Value.AddDays(direction * _currentFilterValue)
                                    : _endDate.Value.AddDays(direction);
    }

    private void SetListHeaders()
    {
        //todo:
        //if (_listHeaderView == null)
        //{
        //    double rowHeight = (double)Microsoft.Maui.Controls.Application.Current.Resources[LibStyleConstants.ST_DEFAULT_ROW_HEIGHT];
        //    Grid listHeaderGrid = new Grid
        //    {
        //        Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[LibStyleConstants.ST_END_TO_END_GRID_STYLE],
        //        HeightRequest = rowHeight,
        //        ColumnDefinitions = new ColumnDefinitionCollection
        //        {
        //            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        //        },
        //        RowDefinitions = new RowDefinitionCollection
        //        {
        //            new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
        //        }
        //    };
        //    var metadata = ChartData.ChartMetaData[0];
        //    listHeaderGrid.Add(new CustomLabelControl(LabelType.PrimarySmallLeft)
        //    {
        //        Text = metadata.ListItemLeftHeader,
        //    }, 0, 0);
        //    listHeaderGrid.Add(new CustomLabelControl(LabelType.PrimarySmallRight)
        //    {
        //        Text = metadata.ReadingParentID > 0 && !metadata.IsGroupValue ? metadata.Reading : metadata.ReadingParent,
        //    }, 1, 0);
        //    _listHeaderView = new PancakeView
        //    {
        //        Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources[LibStyleConstants.ST_PANCAKE_STYLE],
        //        Padding = new Thickness(_appPadding, Convert.ToInt32(Microsoft.Maui.Controls.Application.Current.Resources[LibStyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.CurrentCulture) / 2),
        //        Content = listHeaderGrid
        //    };
        //    _listHeaderView.Margin = new Thickness(1, 0, 2, 0);
        //}
        //if (!_headerGrid.Children.Contains(_listHeaderView))
        //{
        //    _headerGrid.Add(_listHeaderView, 0, 1);
        //}
    }

    private void ReadingListItem_Clicked(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null)
        {
            SelectionChanged?.Invoke(sender, e);
        }
    }

    #region Not Supported Methods

    /// <summary>
    /// Validates a Control Abstract Method
    /// </summary>
    /// <param name="isButtonClick">true if validation is on button click</param>
    public override void ValidateControl(bool isButtonClick)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Apply's Resource Value Abstract Method
    /// </summary>
    protected override void ApplyResourceValue()
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Enabled Value
    /// </summary>
    /// <param name="value">true if to be enabled else false</param>
    protected override void EnabledValue(bool value)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Renders Border
    /// </summary>
    /// <param name="value">true if border is to be rendered else false</param>
    protected override void RenderBorder(bool value)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Renders Header
    /// </summary>
    protected override void RenderHeader()
    {
        throw new NotSupportedException();
    }

    #endregion
}