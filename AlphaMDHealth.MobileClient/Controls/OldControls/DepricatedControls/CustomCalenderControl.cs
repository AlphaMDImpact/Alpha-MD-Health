//using AlphaMDHealth.Model;
//using AlphaMDHealth.Utility;
//using System.Globalization;

//namespace AlphaMDHealth.MobileClient
//{
//    /// <summary>
//    /// Represents Custom Calendar Control
//    /// </summary>
//    public class CustomCalendarControl : BaseContentView
//    {
//        private const string _DAY_TEXT = "Day";
//        private const string _WEEK_TEXT = "Week";
//        private const string _MONTH_TEXT = "Month";
//        //todo: public readonly Xamarin.Plugin.Calendar.Controls.Calendar _calendar;
//        private readonly CustomLabelControl _monthYearLabel;
//        private readonly CustomButtonControl _dayViewButton;
//        private readonly CustomButtonControl _weekViewButton;
//        private readonly CustomButtonControl _monthViewButton;
//        private readonly SvgImageButtonView _previousButton;
//        private readonly SvgImageButtonView _nextButton;
//        private readonly ContentView _buttonPancake; //todo:PancakeView
//        private WeekView1 _weekView;
//        private WeekView1 _dayView;
//        private string _selectedView;
//        private readonly Grid _mainLayout;

//        //todo:
//        ///// <summary>
//        ///// month Events
//        ///// </summary>
//        //public EventCollection Events { get; private set; }

//        /// <summary>
//        /// On Appointment Cliked Event
//        /// </summary>
//        public event EventHandler<EventArgs> OnAppointmentCliked;

//        /// <summary>
//        /// Appointments Property
//        /// </summary>
//        public IEnumerable<IGrouping<DateTimeOffset, AppointmentModel>> Appointments
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// CustomCalendarControl Class constructor
//        /// </summary>
//        public CustomCalendarControl()
//        {
//            _monthYearLabel = new CustomLabelControl(LabelType.PrimaryMediumBoldCenter) { HorizontalOptions = LayoutOptions.EndAndExpand, Padding = AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? new Thickness(0, 0, 15, 0) : new Thickness(15, 0, 0, 0) };
//            AutomationProperties.SetIsInAccessibleTree(_monthYearLabel, true);
//            //todo:
//            //_calendar = new Xamarin.Plugin.Calendar.Controls.Calendar
//            //{
//            //    Style = (Style)Application.Current.Resources[LibStyleConstants.ST_MONTH_CALENDAR_STYLE],
//            //    SwipeUpToHideEnabled = false,
//            //};
//            //_calendar.SwipeLeftCommand = new Command(() =>
//            //{
//            //    OnLeftSwipe();
//            //});
//            //_calendar.SwipeRightCommand = new Command(() =>
//            //{
//            //    OnRightSwiped();
//            //});
//            //_calendar.DayTappedCommand = new Command(() =>
//            //{
//            //    DayViewLoad(_calendar.SelectedDate);
//            //});
//            AssignMonthYearLabelText();
//            Grid buttonGrid = new Grid
//            {
//                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
//                ColumnSpacing = 1,
//                BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR],
//                RowDefinitions =
//                {
//                    new RowDefinition { Height = GridLength.Auto },
//                },
//                ColumnDefinitions =
//                {
//                     new ColumnDefinition { Width = GridLength.Auto },
//                     new ColumnDefinition { Width = GridLength.Auto },
//                     new ColumnDefinition { Width = GridLength.Auto },
//                     new ColumnDefinition { Width = GridLength.Auto },
//                },
//            };
//            _mainLayout = new Grid
//            {
//                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
//                RowDefinitions =
//                {
//                    new RowDefinition { Height = GridLength.Auto },
//                    new RowDefinition { Height = GridLength.Star },
//                },
//                ColumnDefinitions =
//                {
//                     new ColumnDefinition { Width = GridLength.Star },
//                     new ColumnDefinition { Width = GridLength.Auto },
//                },
//            };
//            _dayViewButton = new CustomButtonControl(ButtonType.TabButton);
//            AutomationProperties.SetIsInAccessibleTree(_dayViewButton, true);
//            _weekViewButton = new CustomButtonControl(ButtonType.TabButton);
//            AutomationProperties.SetIsInAccessibleTree(_weekViewButton, true);
//            _monthViewButton = new CustomButtonControl(ButtonType.TabButton);
//            AutomationProperties.SetIsInAccessibleTree(_monthViewButton, true);
//            buttonGrid.Add(_dayViewButton, 0, 0);
//            buttonGrid.Add(_weekViewButton, 1, 0);
//            buttonGrid.Add(_monthViewButton, 2, 0);
//            _buttonPancake = new ContentView //PancakeView
//            {
//                Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_TAB_STYLE],
//                Margin = AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? new Thickness(15, 15, 0, 5) : new Thickness(0, 15, 15, 5),
//                Content = buttonGrid,
//            };
//            _nextButton = new SvgImageButtonView(ImageConstants.I_NEXT_ARROW_PNG, AppImageSize.ImageSizeL, AppImageSize.ImageSizeL) { HorizontalOptions = LayoutOptions.End, Margin = new Thickness(0, 0, 5, 0), Opacity = 0.5 };
//            _previousButton = new SvgImageButtonView(ImageConstants.I_PREVIOUS_ARROW_PNG, AppImageSize.ImageSizeL, AppImageSize.ImageSizeL) { Margin = new Thickness(5, 0, 0, 0), Opacity = 0.5 };
//            _nextButton.Clicked += NextButton_Clicked;
//            _previousButton.Clicked += PreviousButton_Clicked;
//            _mainLayout.Add(_buttonPancake, 0, 0);
//            Content = _mainLayout;
//        }


//        private void OnRightSwiped()
//        {
//            if (AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight)
//            {
//                PreviousMonth();
//            }
//            else
//            {
//                NextMonth();
//            }
//        }

//        private void OnLeftSwipe()
//        {
//            if (AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight)
//            {
//                NextMonth();
//            }
//            else
//            {
//                PreviousMonth();
//            }
//        }

//        /// <summary>
//        /// unregister all event
//        /// </summary>
//        public void UnLoadUIData()
//        {
//            _dayViewButton.Clicked -= DayView_Clicked;
//            _weekViewButton.Clicked -= WeekView_Clicked;
//            _monthViewButton.Clicked -= MonthView_Clicked;
//        }

//        private void NextMonthCal()
//        {
//            //todo:
//            //var nextMonth = _calendar.Month;
//            //if (nextMonth >= 12)
//            //{
//            //    _calendar.Month = 1;
//            //    _calendar.Year += 1;
//            //}
//            //else
//            //{
//            //    _calendar.Month = nextMonth + 1;
//            //}
//            AssignMonthYearLabelText();
//            CreateEvents();
//        }

//        private void PreviousMonthCal()
//        {
//            //todo:
//            //var previousMonth = _calendar.Month;
//            //if (previousMonth <= 1)
//            //{
//            //    _calendar.Month = 12;
//            //    _calendar.Year -= 1;
//            //}
//            //else
//            //{
//            //    _calendar.Month = previousMonth - 1;
//            //}
//            AssignMonthYearLabelText();
//            CreateEvents();
//        }

//        private void MonthView(int month)
//        {
//            _mainLayout.Children.Clear();
//            ResetBackGround(string.Empty);
//            ResetBackGround(_MONTH_TEXT);
//            //todo:_calendar.Month = month;
//            CreateEvents();
//            //todo:_calendar.EventIndicatorType = Xamarin.Plugin.Calendar.EventIndicatorType.TopDot;
//            _mainLayout.Add(_buttonPancake, 0, 0);
//            _mainLayout.Add(_monthYearLabel, 1, 0);
//            //todo:_mainLayout.Add(_calendar, 0, 1);
//            //todo:Grid.SetColumnSpan(_calendar, 2);
//            AssignMonthYearLabelText();
//            _mainLayout.Add(_previousButton, 0, 0);
//            _mainLayout.Add(_nextButton, 1, 0);
//            Grid.SetRowSpan(_previousButton, 2);
//            Grid.SetRowSpan(_nextButton, 2);
//        }

//        private void CreateEvents()
//        {
//            //todo:
//            //var appointments = Appointments.Where(x => x.Key.Month == _calendar.Month && x.Key.Year == _calendar.Year).GroupBy(a => a.Key.Date);
//            //_calendar.Events = new EventCollection();
//            //foreach (var item in appointments)
//            //{
//            //    _calendar.Events.Add(item.Key.Date, new List<object> { new object() });
//            //}
//        }


//        private void AssignMonthYearLabelText()
//        {
//            //todo:_monthYearLabel.Text = new DateTime(_calendar.Year, _calendar.Month, 1).ToString("MMMM", CultureInfo.InvariantCulture) + LibConstants.STRING_SPACE + _calendar.Year.ToString(CultureInfo.InvariantCulture);
//        }

//        private void MonthView_Clicked(object sender, EventArgs e)
//        {
//            AppHelper.ShowBusyIndicator = true;
//            MonthView(DateTime.Now.Month);
//            AppHelper.ShowBusyIndicator = false;
//        }

//        private void WeekView_Clicked(object sender, EventArgs e)
//        {
//            AppHelper.ShowBusyIndicator = true;
//            _mainLayout.Children.Clear();
//            ResetBackGround(string.Empty);
//            ResetBackGround(_WEEK_TEXT);
//            _mainLayout.Add(_buttonPancake, 0, 0);
//            _mainLayout.Add(_monthYearLabel, 1, 0);
//            _weekView = new WeekView1();
//            _weekView.OnWeekViewAppointmentCliked += WeekView_OnWeekViewAppointmentCliked;
//            //todo:
//            //_weekView.OnRightWeekSwipe += OnRightSwiped;
//            //_weekView.OnLeftWeekSwipe += OnLeftSwiped;
//            //_calendar.SelectedDate = DateTime.Now;
//            _weekView.SetView(false, DateTime.Now);
//            _weekView.DayWeekHeader(GetResourceValueByKey(ResourceConstants.R_ALL_DAYS_TEXT_KEY));
//            _weekView.FindViewPosition((AppointmentDTO)Appointments);
//            AssignMonthYearLabelText();
//            _mainLayout.Add(_weekView, 0, 1);
//            Grid.SetColumnSpan(_weekView, 2);
//            AppHelper.ShowBusyIndicator = false;
//            _mainLayout.Add(_previousButton, 0, 0);
//            _mainLayout.Add(_nextButton, 1, 0);
//            Grid.SetRowSpan(_previousButton, 2);
//            Grid.SetRowSpan(_nextButton, 2);
//        }

//        private void WeekView_OnWeekViewAppointmentCliked(object sender, EventArgs e)
//        {
//            OnAppointmentCliked?.Invoke(sender, e);
//        }

//        private void DayView_OnWeekViewAppointmentCliked(object sender, EventArgs e)
//        {
//            OnAppointmentCliked?.Invoke(sender, e);
//        }

//        private void DayView_Clicked(object sender, EventArgs e)
//        {
//            AppHelper.ShowBusyIndicator = true;
//            DayViewLoad(DateTime.Today);
//            AppHelper.ShowBusyIndicator = false;
//        }

//        /// <summary>
//        /// Method to load Day View
//        /// </summary>
//        /// <param name="selectedDate"></param>
//        public void DayViewLoad(DateTime selectedDate)
//        {
//            //todo:_calendar.SelectedDate = selectedDate;
//            _mainLayout.Children.Clear();
//            ResetBackGround(string.Empty);
//            ResetBackGround(_DAY_TEXT);
//            _dayView = new WeekView1();
//            _dayView.OnWeekViewAppointmentCliked += DayView_OnWeekViewAppointmentCliked;
//            //todo:_dayView.OnLeftDaySwipe += OnDayLeftSwiped;
//            //todo:_dayView.OnRightDaySwipe += OnDayRightSwiped;
//            _monthYearLabel.Text = selectedDate.ToString("ddd, dd MMMM yyyy", CultureInfo.CurrentCulture);
//            _dayView.SetView(true, selectedDate);
//            _dayView.DayWeekHeader(GetResourceValueByKey(ResourceConstants.R_DAY_TEXT_KEY));
//            _dayView.FindViewPosition((AppointmentDTO)Appointments);
//            _mainLayout.Add(_dayView, 0, 1);
//            Grid.SetColumnSpan(_dayView, 2);
//            _mainLayout.Add(_buttonPancake, 0, 0);
//            _mainLayout.Add(_previousButton, 0, 0);
//            _mainLayout.Add(_nextButton, 1, 0);
//            _mainLayout.Add(_monthYearLabel, 1, 0);
//            Grid.SetRowSpan(_previousButton, 2);
//            Grid.SetRowSpan(_nextButton, 2);
//        }

//        private void OnRightSwiped(object sender, EventArgs e)
//        {
//            AppHelper.ShowBusyIndicator = true;
//            PreviousWeek();
//            AppHelper.ShowBusyIndicator = false;
//        }

//        private void OnLeftSwiped(object sender, EventArgs e)
//        {
//            AppHelper.ShowBusyIndicator = true;
//            NextWeek();
//            AppHelper.ShowBusyIndicator = false;
//        }

//        private void OnDayRightSwiped(object sender, EventArgs e)
//        {
//            AppHelper.ShowBusyIndicator = true;
//            DayRightSwip();
//            AppHelper.ShowBusyIndicator = false;
//        }

//        private void OnDayLeftSwiped(object sender, EventArgs e)
//        {
//            AppHelper.ShowBusyIndicator = true;
//            DayLeftSwip();
//            AppHelper.ShowBusyIndicator = false;
//        }

//        private void PreviousMonth()
//        {
//            PreviousMonthCal();
//            //todo:_calendar.SelectedDate = new DateTime(_calendar.Year, _calendar.Month, 1);
//        }

//        private void NextMonth()
//        {
//            NextMonthCal();
//            //todo:_calendar.SelectedDate = new DateTime(_calendar.Year, _calendar.Month, 1);
//        }

//        private void NextWeek()
//        {
//            _weekView.NextWeek();
//            _weekView.DayWeekHeader(GetResourceValueByKey(ResourceConstants.R_ALL_DAYS_TEXT_KEY));
//            _monthYearLabel.Text = _weekView.MonthText + Constants.STRING_SPACE + _weekView.Year.ToString(CultureInfo.InvariantCulture);
//            _weekView.FindViewPosition((AppointmentDTO)Appointments);
//        }

//        private void PreviousWeek()
//        {
//            _weekView.PreWeek();
//            _weekView.DayWeekHeader(GetResourceValueByKey(ResourceConstants.R_ALL_DAYS_TEXT_KEY));
//            _monthYearLabel.Text = _weekView.MonthText + Constants.STRING_SPACE + _weekView.Year.ToString(CultureInfo.InvariantCulture);
//            _weekView.FindViewPosition((AppointmentDTO)Appointments);
//        }

//        private void DayLeftSwip()
//        {
//            //todo:
//            //_calendar.SelectedDate = _calendar.SelectedDate.AddDays(1);
//            //_dayView.PreDay(_calendar.SelectedDate);
//            //_monthYearLabel.Text = _calendar.SelectedDate.ToString(LibConstants.CALENDER_CONTROL_FORMAT, CultureInfo.CurrentCulture);
//            //_dayView.FindViewPosition(Appointments);
//        }

//        private void DayRightSwip()
//        {
//            //todo:
//            //_calendar.SelectedDate = _calendar.SelectedDate.AddDays(-1);
//            //_dayView.NextDay(_calendar.SelectedDate);
//            //_monthYearLabel.Text = _monthYearLabel.Text = _calendar.SelectedDate.ToString(LibConstants.CALENDER_CONTROL_FORMAT, CultureInfo.CurrentCulture);
//            //_dayView.FindViewPosition(Appointments);
//        }

//        private void PreviousButton_Clicked(object sender, EventArgs e)
//        {
//            switch (_selectedView)
//            {
//                case _DAY_TEXT:
//                    DayRightSwip();
//                    break;
//                case _WEEK_TEXT:
//                    PreviousWeek();
//                    break;
//                case _MONTH_TEXT:
//                    PreviousMonth();
//                    break;
//                default:
//                    //// for future
//                    break;
//            }
//        }

//        private void NextButton_Clicked(object sender, EventArgs e)
//        {
//            switch (_selectedView)
//            {
//                case _DAY_TEXT:
//                    DayLeftSwip();
//                    break;
//                case _WEEK_TEXT:
//                    NextWeek();
//                    break;
//                case _MONTH_TEXT:
//                    NextMonth();
//                    break;
//                default:
//                    // to be implimented
//                    break;
//            }
//        }

//        private void ResetBackGround(string styleID)
//        {
//            _selectedView = styleID;
//            switch (styleID)
//            {
//                case _DAY_TEXT:
//                    _dayViewButton.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
//                    _dayViewButton.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_CONTROL_BACKGROUND_COLOR];
//                    break;
//                case _WEEK_TEXT:
//                    _weekViewButton.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
//                    _weekViewButton.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_CONTROL_BACKGROUND_COLOR];
//                    break;
//                case _MONTH_TEXT:
//                    _monthViewButton.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
//                    _monthViewButton.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_CONTROL_BACKGROUND_COLOR];
//                    break;
//                default:
//                    ResetButton();
//                    break;
//            }
//        }

//        private void ResetButton()
//        {
//            _dayViewButton.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_CONTROL_BACKGROUND_COLOR];
//            _dayViewButton.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
//            _weekViewButton.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_CONTROL_BACKGROUND_COLOR];
//            _weekViewButton.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
//            _monthViewButton.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_CONTROL_BACKGROUND_COLOR];
//            _monthViewButton.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
//        }

//        /// <summary>
//        /// validate picker Control
//        /// </summary>
//        /// <param name="isButtonClick">validation fired after button is clicked</param>
//        public override void ValidateControl(bool isButtonClick)
//        {
//            throw new NotSupportedException();
//        }

//        /// <summary>
//        /// Method to Apply Resource Value
//        /// </summary>
//        protected override void ApplyResourceValue()
//        {
//            _dayViewButton.Text = GetResourceValueByKey(ResourceConstants.R_DAY_TEXT_KEY);
//            _weekViewButton.Text = GetResourceValueByKey(ResourceConstants.R_WEEK_TEXT_KEY);
//            _monthViewButton.Text = GetResourceValueByKey(ResourceConstants.R_MONTH_TEXT_KEY);
//            _dayViewButton.Clicked += DayView_Clicked;
//            _weekViewButton.Clicked += WeekView_Clicked;
//            _monthViewButton.Clicked += MonthView_Clicked;
//        }

//        /// <summary>
//        /// Method to render header
//        /// </summary>
//        protected override void RenderHeader()
//        {
//            throw new NotSupportedException();
//        }

//        /// <summary>
//        /// Method to set rendere value
//        /// </summary>
//        /// <param name="value">border thikness</param>
//        protected override void RenderBorder(bool value)
//        {
//            throw new NotSupportedException();
//        }

//        /// <summary>
//        /// Method to Enable value
//        /// </summary>
//        /// <param name="value"></param>
//        protected override void EnabledValue(bool value)
//        {
//            throw new NotSupportedException();
//        }
//    }
//}