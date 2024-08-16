using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Charts;
using System.Runtime.Serialization;
using ValueType = DevExpress.Maui.Charts.ValueType;

namespace AlphaMDHealth.MobileClient;

internal class AmhChartControl : AmhBaseControl
{
    private Grid _container;
    private AmhLabelControl _headerLabel;
    private AmhLabelControl _labelControl;
    private AmhImageControl _prevImage;
    private AmhImageControl _nextImage;
    private DateTimeAxisX _dateTimeAxisX;
    private ChartView _chartView;
    public event EventHandler<string> ButtonClicked;
    private string _dataNotFoundText;
    DateTime _startDate;

    private ChartUIDTO _value;
    /// <summary>
    /// Control value as object
    /// </summary>
    internal ChartUIDTO Value
    {
        get
        {
            return GetControlValue();
        }
        set
        {
            if (_value != value)
            {
                _value = value;
                SetControlValue();
            }
        }
    }

    /// <summary>
    /// value property
    /// </summary>
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(object), typeof(AmhChartControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhChartControl control = (AmhChartControl)bindable;
        if (newValue != null)
        {
            control.Value = newValue as ChartUIDTO;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhChartControl() : this(FieldTypes.Default) 
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    public AmhChartControl(FieldTypes controlType) : base(controlType)
    {
        _container = CreateGridContainer(false);
        _container.RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition{ Height = GridLength.Auto },
            new RowDefinition{ Height = GridLength.Star }
        };
        _container.ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = GridLength.Auto },
        };

        if (ShowHeader)
        {
            _headerLabel = new AmhLabelControl(FieldTypes.PrimarySmallHStartVCenterBoldLabelControl);
            _container.Add(_headerLabel, 0, 0);
            Grid.SetColumnSpan(_headerLabel, 3);
        }

        _chartView = new ChartView
        {
            Hint = new Hint
            {
                Behavior = new TooltipHintBehavior()
            },
            Legend = new Legend()
            {
                VerticalPosition = LegendVerticalPosition.TopOutside,
                HorizontalPosition = LegendHorizontalPosition.Center,
                Orientation = LegendOrientation.LeftToRight
            }
        };
        _container.Add(_chartView, 0, 1);
        Grid.SetColumnSpan(_chartView, 3);

        _prevImage = CreateImage(ImageConstants.I_PREVIOUS_ARROW_SVG, PrevImage_Clicked);
        _container.Add(_prevImage, 0, 1);
        _nextImage = CreateImage(ImageConstants.I_NEXT_ARROW_PNG, NextImage_Clicked);
        _container.Add(_nextImage, 2, 1);
        Content = _container;
    }

    private ChartUIDTO GetControlValue()
    {
        return _value;
    }

    /// <summary>
    /// value as ChartUIDTO
    /// </summary>
    private void SetControlValue()
    {
        _chartView.Series.Clear();
        RenderControl();
    }

    /// <summary>
    /// render control
    /// </summary>
    protected override void RenderControl()
    {
        if (_value != null)
        {
            RenderData(_chartView);
        }
    }

    protected override void ApplyResourceValue()
    {
        _dataNotFoundText = _resource?.ResourceValue;
    }

    protected override void EnabledDisableField(bool value)
    {
    }

    private void RenderData(ChartView chartView)
    {
        _dateTimeAxisX = new DateTimeAxisX();
        _dateTimeAxisX.Style = new AxisStyle
        {
            LineThickness = 1,
            LineColor = Colors.LightGray
        };
        if (GenericMethods.IsListNotEmpty(_value?.Lines))
        {
            _startDate = _value.Lines.Min(x => x.ChartData?.Min(y => y.DateTime)).Value;
            DateTime endDate = _value.Lines.Max(x => x.ChartData?.Max(y => y.DateTime)).Value;
            TimeSpan difference = endDate - _startDate;
            SetDateTimeAxis(difference, out string headerDateFormat);

            if (ShowHeader)
            {
                _headerLabel.Value = $"{_value.StartDate?.ToString(headerDateFormat)} - {_value.EndDate?.ToString(headerDateFormat)}";
            }

            SetMinAndMaxRangeOfXAxis(chartView, _startDate, endDate);
            SetLinesOnGraph(chartView);
            SetBandsOnGraph(chartView);
        }
        else
        {
            _labelControl = new AmhLabelControl(FieldTypes.PrimarySmallHVCenterBoldLabelControl);
            _labelControl.Value = _dataNotFoundText;
            _container.Children.Clear();
            _container.Children.Add(_labelControl);
        }
    }

    private void SetDateTimeAxis(TimeSpan difference, out string headerDateFormat)
    {
        string xAxisDateFormat;
        var scaleDuration = GetScaleOptionBasedOnData(difference);
        switch (scaleDuration)
        {
            case ResourceConstants.R_ALL_FILTER_KEY_ID:
                SetXAxisInAllCondition(out headerDateFormat, out xAxisDateFormat);
                break;
            case ResourceConstants.R_YEAR_FILTER_KEY_ID:
                SetXAxisInYearlyCondition(out headerDateFormat, out xAxisDateFormat);
                break;
            case ResourceConstants.R_QUARTER_FILTER_KEY_ID:
                SetXAxisInQuarterlyCondition(out headerDateFormat, out xAxisDateFormat);
                break;
            case ResourceConstants.R_MONTH_FILTER_KEY_ID:
                SetXAxisInMonthlyCondition(out headerDateFormat, out xAxisDateFormat);
                break;
            case ResourceConstants.R_WEEK_FILTER_KEY_ID:
                SetXAxisInWeeklyCondition(out headerDateFormat, out xAxisDateFormat);
                break;
            case ResourceConstants.R_DAY_FILTER_KEY_ID:
            default:
                SetXAxisInDailyCondition(out headerDateFormat, out xAxisDateFormat);
                break;
        }
        _dateTimeAxisX.Label = new AxisLabel
        {
            TextFormat = xAxisDateFormat
        };
    }

    private long GetScaleOptionBasedOnData(TimeSpan difference)
    {
        var scaleDuration = _value.SelectedDuration.OptionID;
        if (scaleDuration == ResourceConstants.R_ALL_FILTER_KEY_ID)
        {
            if (difference.TotalDays >= Constants.PLOT_DAYS_YEARLY)
            {
                // For All Years
                scaleDuration = ResourceConstants.R_ALL_FILTER_KEY_ID;
            }
            else if (difference.TotalDays >= Constants.PLOT_DAYS_QUARTERLY)
            {
                // For Yearly
                scaleDuration = ResourceConstants.R_YEAR_FILTER_KEY_ID;
            }
            else if (difference.TotalDays >= Constants.PLOT_DAYS_MONTLY)
            {
                // For quarterly
                scaleDuration = ResourceConstants.R_QUARTER_FILTER_KEY_ID;
            }
            else if (difference.TotalDays >= Constants.PLOT_DAYS_WEEKLY)
            {
                // For Monthly
                scaleDuration = ResourceConstants.R_MONTH_FILTER_KEY_ID;
            }
            else if (difference.TotalDays >= Constants.PLOT_DAYS_DAILY)
            {
                // For Weekly
                scaleDuration = ResourceConstants.R_WEEK_FILTER_KEY_ID;
            }
            else
            {
                // For Daily
                scaleDuration = ResourceConstants.R_DAY_FILTER_KEY_ID;
            }
        }
        return scaleDuration;
    }

    private void SetBandsOnGraph(ChartView chartView)
    {
        if (GenericMethods.IsListNotEmpty(_value.Bands))
        {
            NumericAxisY axisY = new NumericAxisY();
            axisY.Style = new AxisStyle
            {
                LineThickness = 1,
                LineColor = Colors.LightGray
            };
            foreach (var chartBand in _value.Bands)
            {
                NumericStrip numericStrip = new NumericStrip
                {
                    MinLimit = chartBand.MinValue,
                    MaxLimit = chartBand.MaxValue,
                    MaxLimitEnabled = true,
                    MinLimitEnabled = true,
                    LegendText = chartBand.BandName,
                    Style = new StripStyle
                    {
                        Fill = Color.FromArgb(chartBand.Color),
                    },                  
                };
                axisY.Strips.Add(numericStrip);
            }
            chartView.AxisY = axisY;
        }
    }

    private void SetLinesOnGraph(ChartView chartView)
    {
        if (GenericMethods.IsListNotEmpty(_value.Lines))
        {
            foreach (var chartLine in _value.Lines)
            {
                SeriesDataAdapter seriesDataAdapter = new SeriesDataAdapter
                {
                    DataSource = chartLine.ChartData,
                    ArgumentDataMember = nameof(DataPointModel.DateTime)
                };
                ValueDataMember valueDataMember = new ValueDataMember
                {
                    Type = ValueType.Value,
                    Member = nameof(DataPointModel.Value)
                };
                seriesDataAdapter.ValueDataMembers.Add(valueDataMember);

                switch (_fieldType)
                {
                    case FieldTypes.LineGraphControl:
                        LineSeries lineSeries = new LineSeries
                        {
                            DisplayName = chartLine.LineName,
                            MarkersVisible = true,
                            Style = Application.Current.Resources[StyleConstants.ST_LINE_SERIES_STYLE] as LineSeriesStyle,
                            Data = seriesDataAdapter
                        };
                        lineSeries.Style = new LineSeriesStyle
                        {
                            Stroke = Color.FromArgb(chartLine.LineColor)
                        };
                        chartView.Series.Add(lineSeries);
                        break;
                    default:
                        BarSeries barSeries = new BarSeries()
                        {
                            DisplayName = chartLine.LineName,
                            Style = Application.Current.Resources[StyleConstants.ST_BAR_SERIES_STYLE] as BarSeriesStyle,
                            Data = seriesDataAdapter
                        };
                        barSeries.Style = new BarSeriesStyle
                        {
                            Fill = Color.FromArgb(chartLine.LineColor)
                        };
                        chartView.Series.Add(barSeries);
                        break;
                }
            }
        }
    }

    private void SetMinAndMaxRangeOfXAxis(ChartView chartView, DateTime startDate, DateTime endDate)
    {
        if (_value.StartDate != null || _value.EndDate != null)
        {
            _dateTimeAxisX.Range = new DateTimeRange
            {
                Min = _value.StartDate.Value,
                Max = _value.EndDate.Value
            };
            chartView.AxisX = _dateTimeAxisX;
        }
        else
        {
            _dateTimeAxisX.Range = new DateTimeRange
            {
                Min = startDate,
                Max = endDate
            };

            if (ShowHeader)
            {
                string DateTimeFormat= GetDisplayDateFormat(DateTimeType.MonthYear);
                _headerLabel.Value = $"{startDate.ToString(DateTimeFormat)} - {endDate.ToString(DateTimeFormat)}";
            }
            chartView.AxisX = _dateTimeAxisX;
        }
    }

    private void SetXAxisInDailyCondition(out string headerDateFormat, out string axisXDateFormat)
    {
        _dateTimeAxisX.MeasureUnit = DateTimeMeasureUnit.Hour;
        _dateTimeAxisX.GridAlignment = DateTimeMeasureUnit.Hour;
        _dateTimeAxisX.GridSpacing = 2;
        headerDateFormat = GetDisplayDateFormat(DateTimeType.Date);
        axisXDateFormat = GetDisplayDateFormat(DateTimeType.Hour);
    }

    private void SetXAxisInWeeklyCondition(out string headerDateFormat, out string axisXDateFormat)
    {
        _dateTimeAxisX.MeasureUnit = DateTimeMeasureUnit.Day;
        _dateTimeAxisX.GridAlignment = DateTimeMeasureUnit.Day;
        _dateTimeAxisX.GridSpacing = 1;
        headerDateFormat = GetDisplayDateFormat(DateTimeType.Date);
        axisXDateFormat = GetDisplayDateFormat(DateTimeType.Day);
    }

    private void SetXAxisInMonthlyCondition(out string headerDateFormat, out string axisXDateFormat)
    {
        _dateTimeAxisX.MeasureUnit = DateTimeMeasureUnit.Week;
        _dateTimeAxisX.GridAlignment = DateTimeMeasureUnit.Week;
        _dateTimeAxisX.GridSpacing = 1;
        headerDateFormat = GetDisplayDateFormat(DateTimeType.MonthYear);
        axisXDateFormat = GetDisplayDateFormat(DateTimeType.DayMonth);
    }

    private void SetXAxisInQuarterlyCondition(out string headerDateFormat, out string axisXDateFormat)
    {
        _dateTimeAxisX.MeasureUnit = DateTimeMeasureUnit.Month;
        _dateTimeAxisX.GridAlignment = DateTimeMeasureUnit.Month;
        _dateTimeAxisX.GridSpacing = 1;
        headerDateFormat = GetDisplayDateFormat(DateTimeType.MonthYear);
        axisXDateFormat = GetDisplayDateFormat(DateTimeType.DayMonth);
    }

    private void SetXAxisInYearlyCondition(out string headerDateFormat, out string axisXDateFormat)
    {
        _dateTimeAxisX.MeasureUnit = DateTimeMeasureUnit.Month;
        _dateTimeAxisX.GridAlignment = DateTimeMeasureUnit.Month;
        _dateTimeAxisX.GridSpacing = 2;
        headerDateFormat = GetDisplayDateFormat(DateTimeType.MonthYear);
        axisXDateFormat = GetDisplayDateFormat(DateTimeType.DayMonth);
    }

    private void SetXAxisInAllCondition(out string headerDateFormat, out string axisXDateFormat)
    {
        _dateTimeAxisX.MeasureUnit = DateTimeMeasureUnit.Year;
        _dateTimeAxisX.GridAlignment = DateTimeMeasureUnit.Year;
        _dateTimeAxisX.GridSpacing = 1;
        headerDateFormat = GetDisplayDateFormat(DateTimeType.MonthYear);
        axisXDateFormat = GetDisplayDateFormat(DateTimeType.Date);
    }

    private void PrevImage_Clicked(object sender, EventArgs e)
    {
        if (_value.SelectedDuration.OptionID != ResourceConstants.R_ALL_FILTER_KEY_ID && _startDate <= _value.StartDate.Value.Date)
        {
            ButtonClicked?.Invoke(this, ChartDataFor.Prev.ToString());
        }
        else
        {
            _nextImage.IsControlEnabled= false;
        }
    }

    private void NextImage_Clicked(object sender, EventArgs e)
    {
        if (_value.SelectedDuration.OptionID != ResourceConstants.R_ALL_FILTER_KEY_ID && _value.EndDate.Value.Date != DateTime.Now.Date)
        {
            ButtonClicked?.Invoke(this, ChartDataFor.Next.ToString());
        }
        else
        {
            _nextImage.IsControlEnabled = false;
        }
    }

    private AmhImageControl CreateImage(string v, EventHandler<EventArgs> clicked)
    {

        var image = new AmhImageControl(FieldTypes.CircleImageControl)
        {
            Icon=v,
            ImageHeight= AppImageSize.ImageSizeS,
            ImageWidth= AppImageSize.ImageSizeS,
        };
        image.OnValueChanged += clicked;
        return image;
    }

    private string GetDisplayDateFormat(DateTimeType dateTimeType)
    {
        LibSettings.TryGetDateFormatSettings(_pageResources.Settings
            , out string dayFormat, out string monthFormat, out string yearFormat);
        return GenericMethods.GetDateTimeFormat(dateTimeType, dayFormat, monthFormat, yearFormat);
    }
}

















//private string GetDateTimeLabelText()
//{
//    if (Value.StartDate.Value.Date == Value.EndDate.Value.Date)
//    {
//        return GenericMethods.GetDateTimeBasedOnFormatString(new DateTimeOffset(Value.StartDate.Value), _dateFormat);
//    }
//    else
//    {
//        return string.Format("{0} - {1}"
//            , GenericMethods.GetDateTimeBasedOnFormatString(new DateTimeOffset(Value.StartDate.Value), _dateFormat)
//            , GenericMethods.GetDateTimeBasedOnFormatString(new DateTimeOffset(Value.EndDate.Value), _dateFormat));   
//    }
//}

//private string GetDisplayDateFormat()
//{
//    LibSettings.TryGetDateFormatSettings(_pageResources.Settings
//        , out string dayFormat, out string monthFormat, out string yearFormat);
//    return GenericMethods.GetDateTimeFormat(DateTimeType.Date, dayFormat, monthFormat, yearFormat);
//}