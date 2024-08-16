using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Scheduler;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

internal class AmhCalendarControl : AmhBaseControl, IAppointmentCustomizer
{
    private AmhLabelControl _headerLabel;
    private AmhLabelControl _monthViewLabel;
    private AmhLabelControl _weekViewLabel;
    private AmhLabelControl _dayViewLabel;
    private AmhLabelControl _todayLabel;
    private DataSource _dataSource;
    private Grid _container;
    private MonthView _monthView;
    private WeekView _weekView;
    private DayView _dayView;

    private List<OptionModel> _value;
    /// <summary>
    /// Control value as object
    /// </summary>
    internal List<OptionModel> Value
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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(List<OptionModel>), typeof(AmhCalendarControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhCalendarControl control = (AmhCalendarControl)bindable;
        if (newValue != null)
        {
            control.Value = newValue as List<OptionModel>;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhCalendarControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    public AmhCalendarControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    /// <summary>
    /// Get value of control
    /// </summary>
    /// <returns>value of control</returns>
    private List<OptionModel> GetControlValue()
    {
        return _options;
    }

    /// <summary>
    /// Set value to control
    /// </summary>
    private void SetControlValue()
    {
        _dataSource.BindingContext = _value;
    }

    /// <summary>
    /// Applies Resource Value Method implementation
    /// </summary>
    protected override void ApplyResourceValue()
    {
        _monthViewLabel.PageResources = _weekViewLabel.PageResources = _dayViewLabel.PageResources = _todayLabel.PageResources = PageResources;
    }

    protected override void EnabledDisableField(bool value)
    {
        _monthView.IsEnabled = _dayView.IsEnabled = _weekView.IsEnabled = value;
    }

    protected override void ApplyOptions()
    {
        _dataSource.AppointmentsSource = _options;
    }

    /// <summary>
    /// Render Control baseed on Specified type
    /// </summary>
    protected override void RenderControl()
    {
        _dataSource = new DataSource
        {
            AppointmentMappings = new AppointmentMappings
            {
                Id = nameof(OptionModel.OptionID),
                Start = nameof(OptionModel.From),
                End = nameof(OptionModel.To),
                Subject = nameof(OptionModel.OptionText),
            }
        };
        SchedulerDataStorage dataStorage = new SchedulerDataStorage { DataSource = _dataSource };
        CreateMonthView(dataStorage);
        CreateWeekView(dataStorage);
        CreateDayView(dataStorage);

        _container = CreateGridContainer(false);
        _container.ColumnSpacing = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
        _container.RowDefinitions = new RowDefinitionCollection {
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Star },
        };
        _container.ColumnDefinitions = new ColumnDefinitionCollection {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Auto },
        };

        _todayLabel = CreateLabel(ResourceConstants.R_TODAY_TEXT_KEY);

        _headerLabel = new AmhLabelControl(FieldTypes.PrimarySmallHStartVCenterBoldLabelControl);
        _monthViewLabel = CreateLabel(ResourceConstants.R_MONTH_TEXT_KEY);
        _weekViewLabel = CreateLabel(ResourceConstants.R_WEEK_TEXT_KEY);
        _dayViewLabel = CreateLabel(ResourceConstants.R_DAY_TEXT_KEY);
        SetSelectedTab(ResourceConstants.R_DAY_TEXT_KEY, _dayViewLabel);

        _container.Add(_todayLabel, 0, 0);
        _container.Add(_headerLabel, 1, 0);
        _container.Add(_monthViewLabel, 2, 0);
        _container.Add(_weekViewLabel, 3, 0);
        _container.Add(_dayViewLabel, 4, 0);

        Content = _container;
    }

    private void OnCurrentViewClicked()
    {
        _monthView.Start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        _weekView.Start = DateTime.Now;
        _dayView.Start = DateTime.Now;
    }

    private void OnAppointmentTapped(object sender, SchedulerGestureEventArgs e)
    {
        OnValueChangedAction(e.AppointmentInfo, null);
    }

    private void CreateMonthView(SchedulerDataStorage dataStorage)
    {
        _monthView = new MonthView
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_CALANDAR_VIEWBASE_STYLE],
            DataStorage = dataStorage,
            CellAppearance = new MonthViewCellAppearance
            {
                TodayBackgroundColor = Color.FromArgb(StyleConstants.TERTIARY_APP_COLOR),
                TodayDayNumberBackgroundColor = Color.FromArgb(StyleConstants.PRIMARY_APP_COLOR),
                TodayDayNumberTextColor = Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR),
            },
            HeaderItemAppearance = new MonthViewHeaderItemAppearance
            {
                TodayWeekDayTextColor = Color.FromArgb(StyleConstants.ACCENT_COLOR),
                BackgroundColor = Color.FromArgb(StyleConstants.TERTIARY_APP_COLOR)
            },
            AppointmentAppearance = CreateAppointmentAppearance(),
        };
        _monthView.Tap += OnAppointmentTapped;
       
    }

    private void CreateWeekView(SchedulerDataStorage dataStorage)
    {
        _weekView = new WeekView
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_CALANDAR_VIEWBASE_STYLE],
            DataStorage = dataStorage,
            CellAppearance = CreateDayViewCellAppearance(),
            HeaderItemAppearance = CreateDayViewHeaderItemAppearance(),
            AppointmentAppearance = CreateAppointmentAppearance(),
        };
        _weekView.Tap += OnAppointmentTapped;
    }

    private void CreateDayView(SchedulerDataStorage dataStorage)
    {
        _dayView = new DayView
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_CALANDAR_VIEWBASE_STYLE],
            DataStorage = dataStorage,
            CellAppearance = CreateDayViewCellAppearance(),
            HeaderItemAppearance = CreateDayViewHeaderItemAppearance(),
            AppointmentAppearance = CreateAppointmentAppearance()
        };
        _dayView.Tap += OnAppointmentTapped;
    }

    private DayViewCellAppearance CreateDayViewCellAppearance()
    {
        return new DayViewCellAppearance
        {
            TodayBackgroundColor = Color.FromArgb(StyleConstants.TERTIARY_APP_COLOR),
        };
    }

    private DayViewHeaderItemAppearance CreateDayViewHeaderItemAppearance()
    {
        return new DayViewHeaderItemAppearance
        {
            TodayWeekDayTextColor = Color.FromArgb(StyleConstants.ACCENT_COLOR),
            TodayDayNumberBackgroundColor = Color.FromArgb(StyleConstants.PRIMARY_APP_COLOR),
            TodayDayNumberTextColor = Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR),
        };
    }

    private AppointmentAppearance CreateAppointmentAppearance()
    {
        return new AppointmentAppearance
        {
            TextColor = Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR),
            Customizer = this
        };
    }

    private AmhLabelControl CreateLabel(string key)
    {
        var tabLabel = new AmhLabelControl(FieldTypes.PrimarySmallHEndVCenterLabelControl)
        {
            ResourceKey = key
        };
        var tapGestureRecognizerWeek = new TapGestureRecognizer();
        tapGestureRecognizerWeek.Tapped += (s, e) =>
        {
            SetSelectedTab(key, tabLabel);
        };
        tabLabel.GestureRecognizers.Add(tapGestureRecognizerWeek);
        return tabLabel;
    }

    private void SetSelectedTab(string key, AmhLabelControl tabLabel)
    {
        if (key == ResourceConstants.R_TODAY_TEXT_KEY)
        {
            OnCurrentViewClicked();
        }
        else
        {
            _monthViewLabel.FieldType = _weekViewLabel.FieldType = _dayViewLabel.FieldType = FieldTypes.PrimarySmallHEndVCenterLabelControl;
            RemoveView(_monthView);
            RemoveView(_weekView);
            RemoveView(_dayView);
            switch (key)
            {
                case ResourceConstants.R_MONTH_TEXT_KEY:
                    AddView(_monthView);
                    _headerLabel.Value = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month)} {DateTime.Now.Year}";
                    break;
                case ResourceConstants.R_WEEK_TEXT_KEY:
                    AddView(_weekView);
                    DateTime currentDate = DateTime.Now;
                    DateTime startOfWeek = currentDate.AddDays((int)DayOfWeek.Sunday - (int)currentDate.DayOfWeek);
                    DateTime endOfWeek = startOfWeek.AddDays(6);
                    _headerLabel.Value = $"{startOfWeek:M/d/yyyy} - {endOfWeek:M/d/yyyy}";
                    break;
                case ResourceConstants.R_DAY_TEXT_KEY:
                default:
                    AddView(_dayView);
                    DateTime currDate = DateTime.Now;
                    _headerLabel.Value = $"{currDate:M/d/yyyy}";
                    break;
            }
            tabLabel.FieldType = FieldTypes.PrimaryAppSmallHEndVCenterBoldLabelControl;
        }
    }

    private void RemoveView(View view)
    {
        if (_container.Contains(view))
        {
            _container.Remove(view);
        }
    }

    private void AddView(View view)
    {
        _container.Add(view, 0, 1);
        _container.SetColumnSpan(view, 5);
    }

    public void Customize(AppointmentViewModel appointment)
    {
        appointment.BackgroundColor = Color.FromArgb(StyleConstants.PRIMARY_APP_COLOR);
    }
}