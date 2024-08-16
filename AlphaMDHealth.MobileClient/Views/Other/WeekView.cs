//using AlphaMDHealth.Model;
//using AlphaMDHealth.Utility;
//using DevExpress.Maui.Scheduler;
//using System.Globalization;

//namespace AlphaMDHealth.MobileClient;

//public class WeekView1 : ContentView
//{
//    private readonly Grid _dayViewGrid;
//    private readonly Grid _headerViewGrid;
//    private Grid _appointmentGrid;
//    private DateTime _firstdayOfWeek;
//    private DateTime _lastdayOfWeek;
//    private CustomLabel _appointmentHeader;
//    private CustomLabel _appointmentDescription;
//    private bool _isDayView;
//    private int _scrollToGrid;
//    internal SchedulerDataStorage DataStorage;
//    private readonly ScrollView _scrollView;
//    public event EventHandler<EventArgs> OnWeekViewAppointmentCliked;
//    public string MonthText { get; set; }
//    public int Year { get; set; }

//    public WeekView1()
//    {
//        _dayViewGrid = new Grid
//        {
//            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
//        };
//        _headerViewGrid = new Grid
//        {
//            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
//            RowDefinitions ={
//                new RowDefinition { Height = new GridLength(40, GridUnitType.Absolute) },
//                new RowDefinition { Height = new GridLength(40, GridUnitType.Absolute) }
//            },
//            ColumnDefinitions =
//            {
//                 new ColumnDefinition { Width = GridLength.Auto },
//                 new ColumnDefinition { Width = GridLength.Star },
//                 new ColumnDefinition { Width = GridLength.Star },
//                 new ColumnDefinition { Width = GridLength.Star },
//                 new ColumnDefinition { Width = GridLength.Star },
//                 new ColumnDefinition { Width = GridLength.Star },
//                 new ColumnDefinition { Width = GridLength.Star },
//                 new ColumnDefinition { Width = GridLength.Star },
//            },
//        };
//        var main = new Grid
//        {
//            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
//            RowDefinitions ={
//                new RowDefinition { Height =  GridLength.Auto  },
//                new RowDefinition { Height = GridLength.Star }
//            },
//            ColumnDefinitions =
//            {
//                new ColumnDefinition { Width = GridLength.Star },
//            },
//        };
//        main.Add(_headerViewGrid, 0, 0);
//        _scrollView = new ScrollView { Content = _dayViewGrid };
//        main.Add(_scrollView, 0, 1);
//        Content = main;
//    }

//    private void GenrateTableRow(int rowCount)
//    {
//        for (int i = 0; i < rowCount; i++)
//        {
//            _dayViewGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60, GridUnitType.Absolute) });
//            _dayViewGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Absolute) });
//        }
//    }

//    private void GenrateTableColumn(int colCount)
//    {
//        _dayViewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
//        for (int i = 0; i < colCount; i++)
//        {
//            _dayViewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
//            _dayViewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Absolute) });
//        }
//    }

//    private void AddSeprator(int rowCount)
//    {
//        for (int i = 1; i < rowCount; i += 2)
//        {
//            var seprator = new BoxView
//            {
//                HeightRequest = 1,
//                VerticalOptions = LayoutOptions.Center,
//                BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_SEPERATOR_COLOR_STYLE]
//            };
//            _dayViewGrid.Add(seprator, 1, i);
//            Grid.SetColumnSpan(seprator, 13);
//        }
//    }

//    private void AddSepratorCol(int rowCount)
//    {
//        for (int i = 2; i < rowCount; i += 2)
//        {
//            var seprator = new BoxView
//            {
//                WidthRequest = 1,
//                BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_SEPERATOR_COLOR_STYLE]
//            };
//            _dayViewGrid.Add(seprator, i, 0);
//            Grid.SetRowSpan(seprator, 51);
//        }
//    }

//    private void DrawTime()
//    {
//        int top = 0;
//        for (var i = 0; i < 24; i++)
//        {
//            CustomLabelControl customLabelControl = new CustomLabelControl(LabelType.PrimarySmallLeft)
//            {
//                Text = DateTime.Now.Date.AddHours(i).ToString("HH:mm", CultureInfo.InvariantCulture),
//                VerticalOptions = LayoutOptions.Center,
//                Padding = new Thickness(15, 0)
//            };
//            _dayViewGrid.Add(customLabelControl, 0, top);
//            Grid.SetRowSpan(customLabelControl, 3);
//            top += 2;
//        }
//    }

//    public void PreWeek()
//    {
//        _firstdayOfWeek = _firstdayOfWeek.AddDays(-7);
//        _lastdayOfWeek = _lastdayOfWeek.AddDays(-7);
//        MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_firstdayOfWeek.Month);
//        Year = _firstdayOfWeek.Year;
//        CreateWeeks();
//        int intTotalChildren = _dayViewGrid.Children.Count - 1;
//        for (int intCounter = intTotalChildren; intCounter > 0; intCounter--)
//        {
//            if (_dayViewGrid.Children[intCounter] is Grid)
//            {
//                _dayViewGrid.Children.Remove(_dayViewGrid.Children[intCounter]);
//            }
//        }
//    }

//    public void NextWeek()
//    {
//        _firstdayOfWeek = _firstdayOfWeek.AddDays(7);
//        _lastdayOfWeek = _lastdayOfWeek.AddDays(7);
//        MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_firstdayOfWeek.Month);
//        Year = _firstdayOfWeek.Year;
//        CreateWeeks();
//        int intTotalChildren = _dayViewGrid.Children.Count - 1;
//        for (int intCounter = intTotalChildren; intCounter > 0; intCounter--)
//        {
//            if (_dayViewGrid.Children[intCounter] is Grid)
//            {
//                _dayViewGrid.Children.Remove(_dayViewGrid.Children[intCounter]);
//            }
//        }
//    }

//    private void CreateWeeks()
//    {
//        _headerViewGrid.Children.Clear();
//        int left = 1;
//        for (DateTime date = _firstdayOfWeek; date.Date <= _lastdayOfWeek.Date; date = date.AddDays(1))
//        {
//            CustomLabelControl dayText = new CustomLabelControl(LabelType.PrimarySmallCenter)
//            {
//                Text = date.Date.ToString("ddd", CultureInfo.InvariantCulture) + Constants.STRING_SPACE + date.Date.Day.ToString(CultureInfo.InvariantCulture),
//            };
//            _headerViewGrid.Add(dayText, left, 0);
//            left++;
//        }
//    }

//    public void DayWeekHeader(string headerText)
//    {
//        CustomLabelControl allDaysLabelControl = new CustomLabelControl(LabelType.PrimarySmallLeft)
//        {
//            Text = headerText,
//            Padding = new Thickness(15, 0)
//        };
//        var boxView = new BoxView
//        {
//            HorizontalOptions = LayoutOptions.FillAndExpand,
//            BackgroundColor = Color.FromArgb(StyleConstants.SEPERATOR_N_DISABLED_COLOR),
//        };
//        _headerViewGrid.Add(boxView, 0, 1);
//        Grid.SetColumnSpan(boxView, 8);
//        _headerViewGrid.Add(allDaysLabelControl, 0, 1);
//    }

//    public void SetView(bool isDayView, DateTime selectedDay)
//    {
//        if (isDayView)
//        {
//            _isDayView = true;
//            _firstdayOfWeek = selectedDay;
//            _lastdayOfWeek = selectedDay;
//            GenrateTableColumn(1);
//        }
//        else
//        {
//            _firstdayOfWeek = DateTime.Now.AddDays(-(DateTime.Now.DayOfWeek - DayOfWeek.Monday));
//            _lastdayOfWeek = _firstdayOfWeek.AddDays(6);
//            GenrateTableColumn(7);
//            AddSepratorCol(13);
//            CreateWeeks();
//        }
//        GenrateTableRow(25);
//        DrawTime();
//        AddSeprator(52);
//    }

//    public void FindViewPosition(AppointmentDTO weekEvents)
//    {
//        int left = 1;
//        _scrollToGrid = 0;
//        for (DateTime date = _firstdayOfWeek; date.Date <= _lastdayOfWeek.Date; date = date.AddDays(1))
//        {
//            var appoinments = weekEvents.SheduledAppoinments.Where(x => x.Key.Date == date.Date).ToList();
//            if (appoinments.Count > 0)
//            {
//                foreach (var item in appoinments)
//                {
//                    foreach (var appointmentData in item)
//                    {
//                        CreateAppointmentGrid();
//                        _scrollToGrid = appointmentData.FromDateTime.Hour < _scrollToGrid ? appointmentData.FromDateTime.Hour : _scrollToGrid;
//                        if (_scrollToGrid == 0)
//                        {
//                            _scrollToGrid = appointmentData.FromDateTime.Hour;
//                        }
//                        var start = appointmentData.FromDateTime.Hour;
//                        var diff = appointmentData.ToDateTime.Hour - appointmentData.FromDateTime.Hour;
//                        _appointmentHeader.Text = appointmentData.PageHeading;
//                        _appointmentDescription.Text = appointmentData.PageData;
//                        _appointmentGrid.Margin = new Thickness(0, appointmentData.FromDateTime.Minute, 0, 60 - appointmentData.ToDateTime.Minute);
//                        _dayViewGrid.Add(_appointmentGrid, left, 2 * (start + 1));
//                        _appointmentGrid.StyleId = appointmentData.AppointmentID.ToString(CultureInfo.InvariantCulture);
//                        var appointmentTapGesture = new TapGestureRecognizer();
//                        appointmentTapGesture.Tapped += AppointmentTapGesture_Tapped;
//                        _appointmentGrid.GestureRecognizers.Add(appointmentTapGesture);
//                        Grid.SetRowSpan(_appointmentGrid, (2 * diff) + 1);
//                        if (_isDayView)
//                        {
//                            Grid.SetColumnSpan(_appointmentGrid, 13);
//                        }
//                    }
//                }
//                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
//                {
//                    _scrollView.ScrollToAsync(0, _scrollToGrid * 60 + _scrollToGrid, false);
//                    return false;
//                });
//            }
//            left += 2;
//        }
//    }

//    private void AppointmentTapGesture_Tapped(object sender, EventArgs e)
//    {
//        OnWeekViewAppointmentCliked?.Invoke(sender, e);
//    }

//    private void CreateAppointmentGrid()
//    {
//        _appointmentHeader = new CustomLabelControl(LabelType.ListHeaderStyle);
//        _appointmentDescription = new CustomLabelControl(LabelType.PrimarySmallLeft)
//        {
//            VerticalOptions = LayoutOptions.StartAndExpand,
//        };
//        var appoinmentBoxView = new BoxView
//        {
//            WidthRequest = 5,
//            VerticalOptions = LayoutOptions.FillAndExpand,
//            BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR]
//        };
//        _appointmentGrid = new Grid
//        {
//            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
//            VerticalOptions = LayoutOptions.FillAndExpand,
//            ColumnSpacing = 10,
//            BackgroundColor = Color.FromArgb("#190078D3"),
//            RowDefinitions =
//            {
//                new RowDefinition { Height = new GridLength( (double)App.Current.Resources[StyleConstants.ST_APP_PADDING]/2, GridUnitType.Absolute) },
//                new RowDefinition { Height = GridLength.Auto },
//                new RowDefinition { Height = GridLength.Star },
//            },
//            ColumnDefinitions =
//            {
//                new ColumnDefinition { Width = GridLength.Auto },
//                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
//            }
//        };
//        _appointmentGrid.Add(_appointmentHeader, 1, 1);
//        _appointmentGrid.Add(_appointmentDescription, 1, 2);
//        _appointmentGrid.Add(appoinmentBoxView, 0, 0);
//        Grid.SetRowSpan(appoinmentBoxView, 3);
//    }
//}