//using System.Globalization;

//namespace AlphaMDHealth.MobileClient
//{
//    /// <summary>
//    /// Week View 
//    /// </summary>
//    public class WeekView : ContentView
//    {
//        private readonly Grid _dayViewGrid;
//        private readonly Grid _headerViewGrid;
//        private DateTime _firstdayOfWeek;
//        private DateTime _lastdayOfWeek;
//        private BoxView _appointmentBand;
//        private bool _isDayView;
//        private int _scrollToGrid;
//        private readonly ScrollView _scrollView;

//        /// <summary>
//        /// Week View Appointment Click Event
//        /// </summary>
//        public event EventHandler<EventArgs> OnWeekViewAppointmentCliked;

//        /// <summary>
//        /// Week View Appointment Click Event
//        /// </summary>
//        public event EventHandler<EventArgs> OnLeftWeekSwipe;

//        /// <summary>
//        /// Week View Appointment Click Event
//        /// </summary>
//        public event EventHandler<EventArgs> OnRightWeekSwipe;

//        /// <summary>
//        /// Week View Appointment Click Event
//        /// </summary>
//        public event EventHandler<EventArgs> OnLeftDaySwipe;

//        /// <summary>
//        /// Week View Appointment Click Event
//        /// </summary>
//        public event EventHandler<EventArgs> OnRightDaySwipe;

//        /// <summary>
//        /// Month Text 
//        /// </summary>
//        public string MonthText { get; set; }

//        /// <summary>
//        /// Year
//        /// </summary>
//        public int Year { get; set; }

//        /// <summary>
//        /// Week View Constructor
//        /// </summary>
//        public WeekView()
//        {
//            _dayViewGrid = new Grid
//            {
//                Style = (Style)Application.Current.Resources[LibStyleConstants.ST_END_TO_END_GRID_STYLE],
//            };
//            _headerViewGrid = new Grid
//            {
//                Style = (Style)Application.Current.Resources[LibStyleConstants.ST_END_TO_END_GRID_STYLE],
//                RowDefinitions ={
//                    new RowDefinition { Height = new GridLength(40, GridUnitType.Absolute) },
//                    new RowDefinition { Height = new GridLength(40, GridUnitType.Absolute) }
//                },
//                ColumnDefinitions =
//                {
//                     new ColumnDefinition { Width = GridLength.Auto },
//                     new ColumnDefinition { Width = GridLength.Star },
//                     new ColumnDefinition { Width = GridLength.Star },
//                     new ColumnDefinition { Width = GridLength.Star },
//                     new ColumnDefinition { Width = GridLength.Star },
//                     new ColumnDefinition { Width = GridLength.Star },
//                     new ColumnDefinition { Width = GridLength.Star },
//                     new ColumnDefinition { Width = GridLength.Star },
//                },
//            };
//            var main = new Grid
//            {
//                Style = (Style)Application.Current.Resources[LibStyleConstants.ST_END_TO_END_GRID_STYLE],
//                RowDefinitions ={
//                    new RowDefinition { Height =  GridLength.Auto  },
//                    new RowDefinition { Height = GridLength.Star }
//                },
//                ColumnDefinitions =
//                {
//                    new ColumnDefinition { Width = GridLength.Star },
//                },
//            };
//            main.Add(_headerViewGrid, 0, 0);
//            _scrollView = new ScrollView { Content = _dayViewGrid };
//            Vapolia.Lib.Ui.Gesture.SetSwipeLeftCommand(_scrollView,
//            new Command(() => { OnLeftSwipe(); })); // What's going on when left swiped.
//            Vapolia.Lib.Ui.Gesture.SetSwipeRightCommand(_scrollView,
//            new Command(() => { OnRightSwiped(); }));
//            main.Add(_scrollView, 0, 1);
//            Content = main;
//        }

//        private void OnRightSwiped()
//        {
//            if (_isDayView)
//            {
//                if (AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight)
//                {
//                    OnRightDaySwipe?.Invoke(string.Empty, new EventArgs());
//                }
//                else
//                {
//                    OnLeftDaySwipe?.Invoke(string.Empty, new EventArgs());
//                }
//            }
//            else
//            {
//                if (AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight)
//                {
//                    OnRightWeekSwipe?.Invoke(string.Empty, new EventArgs());
//                }
//                else
//                {
//                    OnLeftWeekSwipe?.Invoke(string.Empty, new EventArgs());
//                }
//            }
//        }

//        private void OnLeftSwipe()
//        {
//            if (_isDayView)
//            {
//                if (AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight)
//                {
//                    OnLeftDaySwipe?.Invoke(string.Empty, new EventArgs());
//                }
//                else
//                {
//                    OnRightDaySwipe?.Invoke(string.Empty, new EventArgs());
//                }
//            }
//            else
//            {
//                if (AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight)
//                {
//                    OnLeftWeekSwipe?.Invoke(string.Empty, new EventArgs());
//                }
//                else
//                {
//                    OnRightWeekSwipe?.Invoke(string.Empty, new EventArgs());
//                }
//            }
//        }

//        private void GenrateTableRow(int rowCount)
//        {
//            for (int i = 0; i < rowCount; i++)
//            {
//                _dayViewGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(120, GridUnitType.Absolute) });
//                _dayViewGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Absolute) });
//            }
//        }

//        private void GenrateTableColumn(int colCount)
//        {
//            _dayViewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
//            for (int i = 0; i < colCount; i++)
//            {
//                _dayViewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
//                _dayViewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Absolute) });
//            }
//        }

//        private void AddSeprator(int rowCount)
//        {
//            for (int i = 1; i < rowCount; i += 2)
//            {
//                var seprator = new BoxView
//                {
//                    HeightRequest = 1,
//                    VerticalOptions = LayoutOptions.Center,
//                    BackgroundColor = (Color)Application.Current.Resources[LibStyleConstants.ST_SEPERATOR_COLOR_STYLE]
//                };
//                AutomationProperties.SetIsInAccessibleTree(seprator, true);
//                _dayViewGrid.Add(seprator, 1, i);
//                Grid.SetColumnSpan(seprator, 13);
//            }
//        }

//        private void AddSepratorCol(int rowCount)
//        {
//            for (int i = 2; i < rowCount; i += 2)
//            {
//                var seprator = new BoxView
//                {
//                    WidthRequest = 1,
//                    BackgroundColor = (Color)Application.Current.Resources[LibStyleConstants.ST_SEPERATOR_COLOR_STYLE]
//                };
//                AutomationProperties.SetIsInAccessibleTree(seprator, true);
//                _dayViewGrid.Add(seprator, i, 0);
//                Grid.SetRowSpan(seprator, 51);
//            }
//        }

//        private void DrawTime()
//        {
//            int top = 0;
//            for (var i = 0; i < 24; i++)
//            {
//                CustomLabelControl customLabelControl = new CustomLabelControl(LabelType.PrimarySmallLeft)
//                {
//                    Text = DateTime.Now.Date.AddHours(i).ToString(LibConstants.DEFAULT_TIME_FORMAT, CultureInfo.InvariantCulture),
//                    VerticalOptions = LayoutOptions.Center,
//                    Padding = new Thickness(15, 0)
//                };
//                AutomationProperties.SetIsInAccessibleTree(customLabelControl, true);
//                _dayViewGrid.Add(customLabelControl, 0, top);
//                Grid.SetRowSpan(customLabelControl, 3);
//                top += 2;
//            }
//        }

//        /// <summary>
//        /// Previous Week Calculation
//        /// </summary>
//        public void PreWeek()
//        {
//            _firstdayOfWeek = _firstdayOfWeek.AddDays(-7);
//            _lastdayOfWeek = _lastdayOfWeek.AddDays(-7);
//            MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_firstdayOfWeek.Month);
//            Year = _firstdayOfWeek.Year;
//            CreateWeeks();
//            int intTotalChildren = _dayViewGrid.Children.Count - 1;
//            for (int intCounter = intTotalChildren; intCounter > 0; intCounter--)
//            {
//                if (_dayViewGrid.Children[intCounter] is Grid)
//                {
//                    _dayViewGrid.Children.Remove(_dayViewGrid.Children[intCounter]);
//                }
//            }
//        }

//        /// <summary>
//        /// Next Week Calculation
//        /// </summary>
//        public void NextWeek()
//        {
//            _firstdayOfWeek = _firstdayOfWeek.AddDays(7);
//            _lastdayOfWeek = _lastdayOfWeek.AddDays(7);
//            MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_firstdayOfWeek.Month);
//            Year = _firstdayOfWeek.Year;
//            CreateWeeks();
//            int intTotalChildren = _dayViewGrid.Children.Count - 1;
//            for (int intCounter = intTotalChildren; intCounter > 0; intCounter--)
//            {
//                if (_dayViewGrid.Children[intCounter] is Grid)
//                {
//                    _dayViewGrid.Children.Remove(_dayViewGrid.Children[intCounter]);
//                }
//            }
//        }

//        /// <summary>
//        /// Previous Week Calculation
//        /// </summary>
//        public void PreDay(DateTime preDayDateTime)
//        {
//            _firstdayOfWeek = preDayDateTime;
//            _lastdayOfWeek = preDayDateTime;
//            MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_firstdayOfWeek.Month);
//            Year = _firstdayOfWeek.Year;
//            int intTotalChildren = _dayViewGrid.Children.Count - 1;
//            for (int intCounter = intTotalChildren; intCounter > 0; intCounter--)
//            {
//                if (_dayViewGrid.Children[intCounter] is Grid)
//                {
//                    _dayViewGrid.Children.Remove(_dayViewGrid.Children[intCounter]);
//                }
//            }
//        }

//        /// <summary>
//        /// Next Week Calculation
//        /// </summary>
//        public void NextDay(DateTime nextDayDateTime)
//        {
//            _firstdayOfWeek = nextDayDateTime;
//            _lastdayOfWeek = nextDayDateTime;
//            MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_firstdayOfWeek.Month);
//            Year = _firstdayOfWeek.Year;
//            int intTotalChildren = _dayViewGrid.Children.Count - 1;
//            for (int intCounter = intTotalChildren; intCounter > 0; intCounter--)
//            {
//                if (_dayViewGrid.Children[intCounter] is Grid)
//                {
//                    _dayViewGrid.Children.Remove(_dayViewGrid.Children[intCounter]);
//                }
//            }
//        }

//        private void CreateWeeks()
//        {
//            _headerViewGrid.Children.Clear();
//            int left = 1;
//            for (DateTime date = _firstdayOfWeek; date.Date <= _lastdayOfWeek.Date; date = date.AddDays(1))
//            {
//                CustomLabelControl dayText = new CustomLabelControl(LabelType.PrimarySmallCenter)
//                {
//                    Text = date.Date.ToString(LibConstants.DEFAULT_DAY_FORMAT, CultureInfo.InvariantCulture) + LibConstants.STRING_SPACE + date.Date.Day.ToString(CultureInfo.InvariantCulture),
//                };
//                AutomationProperties.SetIsInAccessibleTree(dayText, true);
//                _headerViewGrid.Add(dayText, left, 0);
//                left++;
//            }
//        }

//        /// <summary>
//        /// Day Week Header
//        /// </summary>
//        /// <param name="headerText"></param>
//        public void DayWeekHeader(string headerText)
//        {
//            CustomLabelControl allDaysLabelControl = new CustomLabelControl(LabelType.PrimarySmallLeft)
//            {
//                Text = headerText,
//                Padding = new Thickness(15, 0)
//            };
//            AutomationProperties.SetIsInAccessibleTree(allDaysLabelControl, true);
//            var boxView = new BoxView
//            {
//                HorizontalOptions = LayoutOptions.FillAndExpand,
//                BackgroundColor = (Color)Application.Current.Resources[LibStyleConstants.ST_SEPERATOR_COLOR_STYLE],
//            };
//            AutomationProperties.SetIsInAccessibleTree(boxView, true);
//            _headerViewGrid.Add(boxView, 0, 1);
//            Grid.SetColumnSpan(boxView, 8);
//            _headerViewGrid.Add(allDaysLabelControl, 0, 1);
//        }

//        /// <summary>
//        /// Sets View
//        /// </summary>
//        /// <param name="isDayView">Flag for Day View</param>
//        /// <param name="selectedDay">Selected Day</param>
//        public void SetView(bool isDayView, DateTime selectedDay)
//        {
//            if (isDayView)
//            {
//                _isDayView = true;
//                _firstdayOfWeek = selectedDay;
//                _lastdayOfWeek = selectedDay;
//                GenrateTableColumn(1);
//            }
//            else
//            {
//                _firstdayOfWeek = DateTime.Now.AddDays(-(DateTime.Now.DayOfWeek - DayOfWeek.Monday));
//                _lastdayOfWeek = _firstdayOfWeek.AddDays(6);
//                GenrateTableColumn(7);
//                AddSepratorCol(13);
//                CreateWeeks();
//            }
//            GenrateTableRow(25);
//            DrawTime();
//            AddSeprator(52);
//        }

//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4017:Method signatures should not contain nested generic types", Justification = "As disscussed in team meeting on 26 Feb 2021, cannot remove list of list from parameter as it is only one way to pass the data")]
//        private void FindOverLaped(IEnumerable<IGrouping<DateTimeOffset, AppointmentModel>> weekEvents, CalenderModel calenderModel)
//        {
//            var appointments = weekEvents.Where(x => x.Key.Date >= _firstdayOfWeek && x.Key.Date <= _lastdayOfWeek.Date).Select(x => x.ToList());
//            int overLapped = 1;
//            if (appointments.ToList().Count > 0)
//            {
//                DateTime start = new DateTime();
//                DateTime end = new DateTime();
//                var firstApp = appointments.FirstOrDefault().Select(x => x.FromDateTime).Select(x => x.TimeOfDay);
//                var lastApp = appointments.LastOrDefault().Select(x => x.ToDateTime).Select(x => x.TimeOfDay);
//                calenderModel.StartHr = firstApp.First().Hours;
//                calenderModel.EndHr = lastApp.First().Hours;
//                foreach (var item in appointments)
//                {
//                    if (start != DateTime.MinValue && end != DateTime.MinValue && start.TimeOfDay < item[0].ToDateTime.TimeOfDay && end.TimeOfDay > item[0].FromDateTime.TimeOfDay)
//                    {
//                        overLapped++;
//                        var diff = lastApp.First() - firstApp.First();
//                        calenderModel.OverlappingSpan = Math.Ceiling(diff.TotalHours);
//                    }
//                    start = item[0].FromDateTime.DateTime;
//                    end = item[0].ToDateTime.DateTime;
//                }
//            }
//            calenderModel.OverLappingCount = overLapped;
//        }

//        /// <summary>
//        /// Finds a View Position
//        /// </summary>
//        /// <param name="weekEvents"></param>
//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4017:Method signatures should not contain nested generic types", Justification = "As disscussed in team meeting on 26 Feb 2021, cannot remove list of list from parameter as it is only one way to pass the data")]
//        public void FindViewPosition(IEnumerable<IGrouping<DateTimeOffset, AppointmentModel>> weekEvents)
//        {
//            int left = 1;
//            _scrollToGrid = 0;
//            int overLapped = 0;
//            DateTime startDate = new DateTime();
//            int previousStartIndex = 0;
//            DateTime endDate = new DateTime();
//            bool isStartPoint = false;
//            CalenderModel overLappedGrid = new CalenderModel();
//            FindOverLaped(weekEvents, overLappedGrid);
//            for (DateTime date = _firstdayOfWeek; date.Date <= _lastdayOfWeek.Date; date = date.AddDays(1))
//            {
//                var appoinments = weekEvents.Where(x => x.Key.Date == date.Date).ToList();
//                if (appoinments.Count > 0)
//                {
//                    isStartPoint = false;
//                    Grid mainLayout = CreateOverlapningGrid(overLappedGrid.OverLappingCount);
//                    foreach (var item in appoinments)
//                    {
//                        CreateAppointmentGrid(left, ref overLapped, ref startDate, ref previousStartIndex, ref endDate, ref isStartPoint, overLappedGrid, mainLayout, item);
//                    }
//                    Device.StartTimer(TimeSpan.FromSeconds(1), () =>
//                    {
//                        _scrollView.ScrollToAsync(0, _scrollToGrid * 120 + _scrollToGrid, false);
//                        return false;
//                    });
//                }
//                left += 2;
//            }
//        }

//        private static int GetOverlappingSpan(CalenderModel overLappedGrid)
//        {
//            return (overLappedGrid.OverlappingSpan > 2 ? 2 * (int)overLappedGrid.OverlappingSpan : (int)overLappedGrid.OverlappingSpan);
//        }

//        private static int GetHrDifference(int hrDiffrence)
//        {
//            return hrDiffrence > 2 ? 2 * hrDiffrence : hrDiffrence;
//        }

//        private void CreateOverLappingHadling(int left, ref int overLapped, DateTime startDate, ref int previousStartIndex, DateTime endDate, ref bool isStartPoint, Grid mainLayout, AppointmentModel appointmentData, out double diff, out Grid _appointmentGrid)
//        {
//            var start = appointmentData.FromDateTime.Hour;
//            diff = (appointmentData.ToDateTime - appointmentData.FromDateTime).TotalHours;
//            _appointmentGrid = CreateAppointmentGrid(string.IsNullOrWhiteSpace(appointmentData.PageData), appointmentData);
//            _appointmentGrid.Margin = new Thickness(0, 2 * appointmentData.FromDateTime.Minute, 0, 120 - (2 * appointmentData.ToDateTime.Minute));
//            _appointmentBand.HeightRequest = 2 * appointmentData.ToDateTime.Subtract(appointmentData.FromDateTime).TotalMinutes;
//            _appointmentGrid.HeightRequest = _appointmentBand.HeightRequest;
//            if (startDate != DateTime.MinValue && endDate != DateTime.MinValue && startDate.Date == appointmentData.ToDateTime.Date && startDate.TimeOfDay < appointmentData.ToDateTime.TimeOfDay && endDate.TimeOfDay > appointmentData.FromDateTime.TimeOfDay)
//            {
//                overLapped++;
//                mainLayout.Add(_appointmentGrid, overLapped, 0);
//                var newStart = start - previousStartIndex;
//                var ddd = 2 * appointmentData.FromDateTime.Minute + (newStart * 120);
//                _appointmentGrid.Margin = new Thickness(0, ddd, 0, 120 - (2 * appointmentData.ToDateTime.Minute));
//                Grid.SetRowSpan(_appointmentGrid, Convert.ToInt32(diff, CultureInfo.InvariantCulture) + 1);
//                Grid.SetColumnSpan(mainLayout, _isDayView ? 13 : 1);
//            }
//            else
//            {

//                mainLayout.Add(_appointmentGrid, overLapped, 0);
//                var newStart = start - previousStartIndex;
//                if (newStart > 0 && isStartPoint)
//                {
//                    var ddd = 2 * appointmentData.FromDateTime.Minute + (newStart * 120);
//                    _appointmentGrid.Margin = new Thickness(0, ddd, 0, 120 - (2 * appointmentData.ToDateTime.Minute));
//                }
//                else
//                {
//                    isStartPoint = true;
//                    previousStartIndex = start;
//                    _dayViewGrid.Add(mainLayout, left, 2 * (start + 1));
//                }
//                Grid.SetColumnSpan(mainLayout, _isDayView ? 13 : 1);
//                Grid.SetRowSpan(_appointmentGrid, Convert.ToInt32(diff, CultureInfo.InvariantCulture) + 1);
//            }
//        }

//        private void AppointmentTapGesture_Tapped(object sender, EventArgs e)
//        {

//            OnWeekViewAppointmentCliked?.Invoke(sender, e);
//        }

//        private Grid CreateOverlapningGrid(int count)
//        {
//            ////Random randonGen = new Random();
//            ////Color randomColor = Color.FromRgb(randonGen.Next(255), randonGen.Next(255), randonGen.Next(255));
//            var grid = new Grid
//            {
//                Style = (Style)Application.Current.Resources[LibStyleConstants.ST_END_TO_END_GRID_STYLE],
//                VerticalOptions = LayoutOptions.FillAndExpand,
//                ////  BackgroundColor= randomColor,
//                RowDefinitions =
//                {
//                    new RowDefinition { Height = GridLength.Auto },
//                },
//            };
//            for (int i = 0; i < count; i++)
//            {
//                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

//            }
//            return grid;
//        }

//        private Grid CreateAppointmentGrid(bool isHideDescription, AppointmentModel appointmentData)
//        {
//            var _appointmentHeader = new CustomLabelControl(LabelType.ListHeaderStyle) { FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(CustomLabel)), VerticalOptions = isHideDescription ? LayoutOptions.CenterAndExpand : LayoutOptions.Start };
//            AutomationProperties.SetIsInAccessibleTree(_appointmentHeader, true);
//            _appointmentHeader.Text = appointmentData.PageHeading;
//            _appointmentHeader.MaxWidthRequest = appointmentData.AppointmentID;
//            _appointmentHeader.StyleId = appointmentData.AppointmentID.ToString(CultureInfo.InvariantCulture);

//            _appointmentBand = new BoxView
//            {
//                WidthRequest = 5,
//                VerticalOptions = LayoutOptions.Fill,
//                BackgroundColor = (Color)Application.Current.Resources[LibStyleConstants.ST_PRIMARY_APP_COLOR]
//            };
//            AutomationProperties.SetIsInAccessibleTree(_appointmentBand, true);
//            var appointmentGrid = new Grid
//            {
//                Style = (Style)Application.Current.Resources[LibStyleConstants.ST_END_TO_END_GRID_STYLE],
//                VerticalOptions = LayoutOptions.Start,
//                ColumnSpacing = 10,
//                BackgroundColor = Color.FromArgb(LibStyleConstants.CALENDAR_CONTROL_COLOR),
//                RowDefinitions =
//                {
//                    new RowDefinition { Height = GridLength.Auto },
//                    new RowDefinition { Height = isHideDescription ?0: GridLength.Auto },
//                },
//                ColumnDefinitions =
//                {
//                    new ColumnDefinition { Width = GridLength.Auto },
//                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
//                }
//            };

//            appointmentGrid.Add(_appointmentBand, 0, 0);
//            Grid.SetRowSpan(_appointmentBand, isHideDescription ? 1 : 2);
//            AutomationProperties.SetIsInAccessibleTree(appointmentGrid, true);
//            appointmentGrid.Add(_appointmentHeader, 1, 0);
//            if (!isHideDescription)
//            {
//                var appointmentDescription = new CustomLabelControl(LabelType.PrimarySmallLeft)
//                {
//                    VerticalOptions = LayoutOptions.StartAndExpand,
//                    FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(CustomLabel))
//                };
//                appointmentDescription.Text = appointmentData.PageData;
//                AutomationProperties.SetIsInAccessibleTree(appointmentDescription, true);
//                appointmentGrid.Add(appointmentDescription, 1, 1);
//            }
//            return appointmentGrid;
//        }

//        private void CreateAppointmentGrid(int left, ref int overLapped, ref DateTime startDate, ref int previousStartIndex, ref DateTime endDate, ref bool isStartPoint, CalenderModel overLappedGrid, Grid mainLayout, IGrouping<DateTimeOffset, AppointmentModel> item)
//        {
//            foreach (var appointmentData in item)
//            {
//                try
//                {
//                    _scrollToGrid = appointmentData.FromDateTime.Hour < _scrollToGrid ? appointmentData.FromDateTime.Hour : _scrollToGrid;
//                    if (_scrollToGrid == 0)
//                    {
//                        _scrollToGrid = appointmentData.FromDateTime.Hour;
//                    }
//                    CreateOverLappingHadling(left, ref overLapped, startDate, ref previousStartIndex, endDate, ref isStartPoint, mainLayout, appointmentData, out double diff, out Grid _appointmentGrid);
//                    var hrDiffrence = overLappedGrid.EndHr - overLappedGrid.StartHr;
//                    var overlapcount = overLappedGrid.OverlappingSpan.Equals(0)
//                        ? GetHrDifference(hrDiffrence)
//                        : GetOverlappingSpan(overLappedGrid);
//                    mainLayout.Margin = new Thickness(0, 0, 0, -2 * appointmentData.ToDateTime.Minute);
//                    Grid.SetRowSpan(mainLayout, (overlapcount) + 1);
//                    _appointmentGrid.StyleId = appointmentData.AppointmentID.ToString(CultureInfo.InvariantCulture);
//                    var appointmentTapGesture = new TapGestureRecognizer
//                    {
//                        CommandParameter = appointmentData.AppointmentID.ToString(CultureInfo.InvariantCulture)
//                    };
//                    appointmentTapGesture.Tapped += AppointmentTapGesture_Tapped;
//                    _appointmentGrid.GestureRecognizers.Add(appointmentTapGesture);
//                    if (diff < 0)
//                    {
//                        return;
//                    }
//                    startDate = appointmentData.FromDateTime.DateTime;
//                    endDate = appointmentData.ToDateTime.DateTime;
//                }
//                catch 
//                { 
                    
//                }
//            }
//        }
//    }

//    /// <summary>
//    /// Calender model
//    /// </summary>
//    public class CalenderModel
//    {
//        /// <summary>
//        /// OverLappingCount
//        /// </summary>
//        public int OverLappingCount { get; set; }
//        /// <summary>
//        /// Overlapping Span
//        /// </summary>
//        public double OverlappingSpan { get; set; }
//        /// <summary>
//        /// appointment Start Hr
//        /// </summary>
//        public int StartHr { get; set; }
//        /// <summary>
//        /// appointment End Hr
//        /// </summary>
//        public int EndHr { get; set; }
//    }
//}