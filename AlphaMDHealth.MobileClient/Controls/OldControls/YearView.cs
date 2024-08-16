//namespace AlphaMDHealth.MobileClient
//{
//    /// <summary>
//    /// Year View 
//    /// </summary>
//    public class YearView : ContentView
//    {
//        private readonly Calendar _janMonth;
//        private readonly Calendar _febMonth;
//        private readonly Calendar _marchMonth;
//        private readonly Calendar _aprilMonth;
//        private readonly Calendar _mayMonth;
//        private readonly Calendar _juneMonth;
//        private readonly Calendar _julyMonth;
//        private readonly Calendar _augustMonth;
//        private readonly Calendar _septempberMonth;
//        private readonly Calendar _octoberMonth;
//        private readonly Calendar _novemberMonth;
//        private readonly Calendar _decemberMonth;
//        /// <summary>
//        /// Year Month Tab Event
//        /// </summary>
//        public event EventHandler<EventArgs> OnYearMonthTabbed;

//        /// <summary>
//        /// Year View Constructor
//        /// </summary>
//        public YearView()
//        {
//            var yearGrid = new Grid
//            {
//                Style = (Style)Application.Current.Resources[LibStyleConstants.ST_END_TO_END_GRID_STYLE],
//                Padding = 20,
//                ColumnSpacing = 50,
//                RowSpacing = LibGenericMethods.GetPlatformSpecificValue(10, 5, 0),
//                RowDefinitions =
//                {
//                    new RowDefinition { Height = GridLength.Star },
//                    new RowDefinition { Height = GridLength.Star },
//                    new RowDefinition { Height = GridLength.Star },
//                },
//                ColumnDefinitions =
//                {
//                     new ColumnDefinition { Width = GridLength.Star },
//                     new ColumnDefinition { Width = GridLength.Star },
//                     new ColumnDefinition { Width = GridLength.Star },
//                     new ColumnDefinition { Width = GridLength.Star },
//                },
//            };

//            _janMonth = CreateCalendarView(1, DateTime.UtcNow.Year);
//            _febMonth = CreateCalendarView(2, DateTime.UtcNow.Year);
//            _marchMonth = CreateCalendarView(3, DateTime.UtcNow.Year);
//            _aprilMonth = CreateCalendarView(4, DateTime.UtcNow.Year);
//            _mayMonth = CreateCalendarView(5, DateTime.UtcNow.Year);
//            _juneMonth = CreateCalendarView(6, DateTime.UtcNow.Year);
//            _julyMonth = CreateCalendarView(7, DateTime.UtcNow.Year);
//            _augustMonth = CreateCalendarView(8, DateTime.UtcNow.Year);
//            _septempberMonth = CreateCalendarView(9, DateTime.UtcNow.Year);
//            _octoberMonth = CreateCalendarView(10, DateTime.UtcNow.Year);
//            _novemberMonth = CreateCalendarView(11, DateTime.UtcNow.Year);
//            _decemberMonth = CreateCalendarView(12, DateTime.UtcNow.Year);
//            yearGrid.Add(_janMonth, 0, 0);
//            yearGrid.Add(_febMonth, 1, 0);
//            yearGrid.Add(_marchMonth, 2, 0);
//            yearGrid.Add(_aprilMonth, 3, 0);
//            yearGrid.Add(_mayMonth, 0, 1);
//            yearGrid.Add(_juneMonth, 1, 1);
//            yearGrid.Add(_julyMonth, 2, 1);
//            yearGrid.Add(_augustMonth, 3, 1);
//            yearGrid.Add(_septempberMonth, 0, 2);
//            yearGrid.Add(_octoberMonth, 1, 2);
//            yearGrid.Add(_novemberMonth, 2, 2);
//            yearGrid.Add(_decemberMonth, 3, 2);
//            Content = yearGrid;
//        }
//        /// <summary>
//        /// Changes Calender Year
//        /// </summary>
//        /// <param name="year">year</param>
//        public void ChangeCalendarYear(int year)
//        {
//            _janMonth.Year = year;
//            _febMonth.Year = year;
//            _marchMonth.Year = year;
//            _aprilMonth.Year = year;
//            _mayMonth.Year = year;
//            _juneMonth.Year = year;
//            _julyMonth.Year = year;
//            _augustMonth.Year = year;
//            _septempberMonth.Year = year;
//            _octoberMonth.Year = year;
//            _novemberMonth.Year = year;
//            _decemberMonth.Year = year;
//        }

//        private Calendar CreateCalendarView(int month, int year)
//        {
//            var calendar = new Calendar
//            {
//                Style = (Style)Application.Current.Resources[LibStyleConstants.ST_YEAR_CALENDAR_STYLE],
//                DaysLabelStyle = (Style)Application.Current.Resources[LibStyleConstants.ST_DAY_LABEL_STYLE],
//                DaysTitleLabelStyle = (Style)Application.Current.Resources[LibStyleConstants.ST_DAY_LABEL_STYLE],
//                Year = year,
//                Month = month,
//            };
//            calendar.HeaderSectionTemplate = new DataTemplate(() => { return new CustomLabelControl(LabelType.PrimarySmallLeft) { FontAttributes = FontAttributes.Bold, Text = calendar.MonthText, TextColor = DateTime.Now.Month == calendar.Month ? Color.FromArgb(LibStyleConstants.ST_ERROR_COLOR) : (Color)Application.Current.Resources[LibStyleConstants.ST_PRIMARY_TEXT_COLOR] }; });
//            var calendarTapGesture = new TapGestureRecognizer();
//            calendarTapGesture.Tapped += CalendarTapGesture_Tapped;
//            calendar.GestureRecognizers.Add(calendarTapGesture);
//            return calendar;
//        }

//        private void CalendarTapGesture_Tapped(object sender, EventArgs e)
//        {
//            OnYearMonthTabbed?.Invoke(sender, e);
//        }
//    }
//}