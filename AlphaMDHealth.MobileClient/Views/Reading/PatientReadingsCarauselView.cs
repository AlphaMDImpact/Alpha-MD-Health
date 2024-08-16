using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Carausel view used to display readings
/// </summary>
public class CustomReadingCarauselView : ContentView
{
    private readonly int _recordCount = 3;
    private readonly CarouselView _carouselView;
    private readonly ReadingCarouselIndicatorView _indicators;
    private readonly ObservableCollection<Grid> _carouselSource = new ObservableCollection<Grid>();
    private double _width, _height;

    /// <summary>
    /// On carausel item click
    /// </summary>
    public event EventHandler OnItemClicked;

    /// <summary>
    /// Data used to display Carausel view
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(PatientReadingDTO), typeof(CustomReadingCarauselView), default(PatientReadingDTO), propertyChanged: ItemsChanged);

    /// <summary>
    /// Data used to display Carausel view
    /// </summary>
    public PatientReadingDTO ItemsSource
    {
        get { return (PatientReadingDTO)this.GetValue(ItemsSourceProperty); }
        set { this.SetValue(ItemsSourceProperty, value); }
    }

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="isMain">Featue parameters to render view</param>
    public CustomReadingCarauselView(BasePage page, bool isMain)
    {
        IsVisible = false;
        _carouselView = new CarouselView
        {
            ItemTemplate = new DataTemplate(typeof(ReadingCarouselindicatorViewCell)),
            Margin = new Thickness(0, 0, 0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.InvariantCulture)),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            HeightRequest = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture),
        };
        _carouselView.PositionChanged += OnCarouselViewPositionChanged;
        _indicators = new ReadingCarouselIndicatorView
        {
            IndicatorHeight = (double)AppImageSize.ImageSizeS,
            IndicatorWidth = (double)AppImageSize.ImageSizeS,
            UnselectedIndicator = ImageConstants.I_CAROUSEL_UNSELECTED_PNG,
            SelectedIndicator = ImageConstants.I_CAROUSEL_SELECTED_PNG,
            Padding = new Thickness(0),
            TranslationY = -5,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition{ Width = GridLength.Star }
            }
        };
        Grid carouselLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto},
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition{ Width = GridLength.Star }
            }
        };
        carouselLayout.Add(_carouselView, 0, 0);
        carouselLayout.Add(_indicators, 0, 1);
        if (AppStyles.NameSpace == "AlphaMDHealth.MobileClient")
        {
            carouselLayout.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR];
        }
        //CustomGradient carouselGradientView = page.SetGradientBackground(carouselLayout, GradientPosition.Start);
        //carouselGradientView.VerticalOptions = LayoutOptions.StartAndExpand;
        Content = carouselLayout;
    }

    private static void ItemsChanged(object bindable, object oldValue, object newValue)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var data = newValue as PatientReadingDTO;
            var carausel = bindable as CustomReadingCarauselView;
            carausel._carouselSource.Clear();
            if (data.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(data.ReadingDTOs))
            {
                int j = 0;
                double count = Math.Ceiling(data.ReadingDTOs.Count / (double)carausel._recordCount);
                for (int i = 0; i < count; i++)
                {
                    // i => used to create number for carousel 
                    // j => used to create carousel item inside carousel cell
                    // k => used to iterate over dashboardData.MeasurementDetails.MeasuremetUIData
                    j = RenderCarauselPage(data, carausel, j);
                }
                carausel._carouselView.ItemsSource = carausel._carouselSource;
                carausel._indicators.ItemsSource = carausel._carouselSource;
                carausel._indicators.IsVisible = carausel._carouselSource.Count > 1;
                carausel.IsVisible = true;
            }
            else
            {
                carausel.IsVisible = false;
            }
        });
    }

    /// <summary>
    /// Size Allocation change overriden method
    /// </summary>
    /// <param name="width">New Width</param>
    /// <param name="height">New Height</param>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if ((Math.Sign(_width) != Math.Sign(width) || Math.Sign(_height) != Math.Sign(height)) && Device.RuntimePlatform == Device.Android)
        {
            _width = width;
            _height = height;
            var items = _carouselView.ItemsSource;
            _carouselView.ItemsSource = Enumerable.Empty<ContentView>();
            _carouselView.ItemsSource = items;
        }
    }

    private static int RenderCarauselPage(PatientReadingDTO data, CustomReadingCarauselView carausel, int j)
    {
        Grid readingGrid = new Grid
        {
            FlowDirection = AppStyles.DefaultFlowDirection,
            HeightRequest = carausel._carouselView.HeightRequest,
            ColumnDefinitions = new ColumnDefinitionCollection(),
        };
        for (int index = 0; index < carausel._recordCount; index++)
        {
            readingGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        }
        int col = 0;
        int k = j;
        for (; j < k + carausel._recordCount; j++)
        {
            if (j < data.ReadingDTOs.Count)
            {
                var content = new PatientReadingViewCell(true, true, carausel._carouselView.HeightRequest)
                {
                    BindingContext = data.ReadingDTOs[j],
                    BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR]
                };
                ////ParentPage.AddSpacingForSeperatorLine(content, true);
                readingGrid.Add(content, col, 0);
                if (carausel.OnItemClicked != null)
                {
                    Button readingGridAction = new Button
                    {
                        BindingContext = data.ReadingDTOs[j],
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        //todo:
                        //BorderColor = Color.Transparent,
                        //TextColor = Color.Transparent,
                        Opacity = GenericMethods.GetPlatformSpecificValue(1, 0, 0)
                    };
                    readingGridAction.Clicked += (s, e) =>
                    {
                        carausel.OnItemClicked.Invoke(s, e);
                    };
                    readingGrid.Add(readingGridAction, col, 0);
                }
                col++;
            }
        }
        if (readingGrid.Children?.Count > 0)
        {
            carausel._carouselSource.Add(readingGrid);
        }
        return j;
    }

    private void OnCarouselViewPositionChanged(object sender, PositionChangedEventArgs e)
    {
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            _indicators.Position = e.CurrentPosition;
        });
    }
}

/////// <summary>
/////// Added below code code to test 
/////// </summary>
////public class CustomCarauselView : ContentView
////{
////    private readonly CarouselView _carouselView;
////    private readonly IndicatorView _indicators1;

////    /// <summary>
////    /// Data used to display Carausel view
////    /// </summary>
////    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(PatientReadingDTO), typeof(CustomCarauselView), default(PatientReadingDTO), propertyChanged: ItemsChanged);

////    /// <summary>
////    /// Data used to display Carausel view
////    /// </summary>
////    public PatientReadingDTO ItemsSource
////    {
////        get { return (PatientReadingDTO)this.GetValue(ItemsSourceProperty); }
////        set { this.SetValue(ItemsSourceProperty, value); }
////    }

////    /// <summary>
////    /// Parameterized constructor containing page instance and Parameters
////    /// </summary>
////    /// <param name="page">page instance on which view is rendering</param>
////    /// <param name="parameters">Featue parameters to render view</param>
////    public CustomCarauselView(BasePage page, object parameters)
////    {
////        _indicators1 = new IndicatorView
////        {
////            SelectedIndicatorColor = Color.Black,
////            IndicatorColor = Color.Gray,
////            IndicatorSize = 50,
////        };
////        var height = Convert.ToDouble(Application.Current.Resources[LibStyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture);
////        _carouselView = new CarouselView
////        {
////            Margin = new Thickness(0, 0, 0, Convert.ToDouble(Application.Current.Resources[LibStyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.InvariantCulture)),
////            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
////            HeightRequest = height,
////            ItemTemplate = new DataTemplate(() =>
////            {
////                return new PatientReadingLibViewCell(true, true, height);
////            }),
////            IndicatorView = _indicators1
////        };
////        _carouselView.PositionChanged += OnCarouselViewPositionChanged;
////        Content = _carouselView;
////    }

////    private static void ItemsChanged(object bindable, object oldValue, object newValue)
////    {
////        MainThread.BeginInvokeOnMainThread((Action)(() =>
////        {
////            var carausel = bindable as CustomCarauselView;
////            var data = newValue as PatientReadingDTO;
////            carausel._carouselView.ItemsSource = data.ReadingDTOs;
////            carausel._indicators1.ItemsSource = carausel.ItemsSource.ReadingDTOs;
////            carausel.IsVisible = (data.ErrCode == ErrorCode.OK && LibGenericMethods.IsListNotEmpty(data.ReadingDTOs));
////        }));
////    }

////    private void OnCarouselViewPositionChanged(object sender, PositionChangedEventArgs e)
////    {
////        MainThread.InvokeOnMainThreadAsync(() =>
////        {
////            _indicators1.Position = e.CurrentPosition;
////        });
////    }
////}

/// <summary>
/// View cell to detect indicator progress
/// </summary>
public class ReadingCarouselindicatorViewCell : ContentView
{
    /// <summary>
    /// View cell to detect indicator progress
    /// </summary>
    public ReadingCarouselindicatorViewCell()
    {
        this.SetBinding(ContentProperty, Constants.SYMBOL_DOT_STRING);
    }
}

////#region Old Code

////using AlphaMDHealth.Model;
////using AlphaMDHealth.Utility;
////using System;
////using System.Collections.ObjectModel;
////using System.Globalization;
////using System.Threading.Tasks;
////using Xamarin.Essentials;
////using Xamarin.Forms;

////namespace AlphaMDHealth.MobileClient
////{
////    /// <summary>
////    /// Carausel view used to display readings
////    /// </summary>
////    public class PatientReadingsCarauselView : ViewManager
////    {
////        private readonly int _recordCount = 3;
////        private readonly CarouselView _carouselView;
////        private readonly ReadingCarouselIndicatorView _indicators;
////        private ObservableCollection<StackLayout> _carouselSource = new ObservableCollection<StackLayout>();

////        /// <summary>
////        /// Data used to display Carausel view
////        /// </summary>
////        public PatientReadingDTO ReadingData { get; set; }

////        /// <summary>
////        /// Parameterized constructor containing page instance and Parameters
////        /// </summary>
////        /// <param name="page">page instance on which view is rendering</param>
////        /// <param name="parameters">Featue parameters to render view</param>
////        public PatientReadingsCarauselView(BasePage page, object parameters) : base(page, parameters)
////        {
////            IsVisible = false;
////            _carouselView = new CarouselView
////            {
////                ItemTemplate = new DataTemplate(typeof(ReadingCarouselindicatorViewCell)),
////                ItemsSource = _carouselSource,
////                Margin = new Thickness(0, 0, 0, Convert.ToDouble(Application.Current.Resources[LibStyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.InvariantCulture)),
////                HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
////                HeightRequest = Convert.ToDouble(Application.Current.Resources[LibStyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture),
////            };
////            _carouselView.PositionChanged += OnCarouselViewPositionChanged;
////            _indicators = new ReadingCarouselIndicatorView
////            {
////                IndicatorHeight = (double)AppImageSize.ImageSizeS,
////                IndicatorWidth = (double)AppImageSize.ImageSizeS,
////                UnselectedIndicator = LibImageConstants.I_CAROUSEL_UNSELECTED_SVG_IMAGE,
////                SelectedIndicator = LibImageConstants.I_CAROUSEL_SELECTED_SVG_IMAGE,
////                Padding = new Thickness(0),
////                TranslationY = -5,
////                ColumnDefinitions = new ColumnDefinitionCollection
////                {
////                    new ColumnDefinition{ Width = GridLength.Star }
////                }
////            };
////            Grid carouselLayout = new Grid
////            {
////                Style = (Style)Application.Current.Resources[LibStyleConstants.ST_END_TO_END_GRID_STYLE],
////                RowDefinitions = new RowDefinitionCollection
////                {
////                    new RowDefinition { Height = GridLength.Auto},
////                    new RowDefinition { Height = GridLength.Auto },
////                },
////                ColumnDefinitions = new ColumnDefinitionCollection
////                {
////                    new ColumnDefinition{ Width = GridLength.Star }
////                }
////            };
////            carouselLayout.Add(_carouselView, 0, 0);
////            carouselLayout.Add(_indicators, 0, 1);
////            if (AppStyles.NameSpace == "AlphaMDHealth.MobileClient")
////            {
////                carouselLayout.BackgroundColor = (Color)Application.Current.Resources[LibStyleConstants.ST_APP_BACKGROUND_COLOR];
////            }
////            CustomGradient carouselGradientView = page.SetGradientBackground(carouselLayout, GradientPosition.Start);
////            Grid.SetColumnSpan(carouselLayout, 2);
////            carouselGradientView.VerticalOptions = LayoutOptions.StartAndExpand;
////            Content = carouselGradientView;
////        }

////        /// <summary>
////        /// Load UI data of view
////        /// </summary>
////        /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
////        /// <returns>Returns true if required view is found, else return false</returns>
////        public override async Task LoadUIAsync(bool isRefreshRequest)
////        {
////            MainThread.BeginInvokeOnMainThread(() =>
////            {

////                _carouselSource = new ObservableCollection<StackLayout>();
////                if (ReadingData.ErrCode == ErrorCode.OK && LibGenericMethods.IsListNotEmpty(ReadingData.ReadingDTOs))
////                {
////                    int j = 0;
////                    double count = Math.Ceiling(ReadingData.ReadingDTOs.Count / (double)_recordCount);
////                    double layoutPadding = Convert.ToDouble(Application.Current.Resources[LibStyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.InvariantCulture);
////                    double screenWidth = App._essentials.GetPreferenceValue(LibStorageConstants.PR_SCREEN_WIDTH_KEY, (double)0) - (2 * layoutPadding);
////                    if (IsPatientPage())
////                    {
////                        screenWidth -= AppStyles.GetSvgSize(AppImageSize.ImageSizeL) + (8 * layoutPadding);
////                    }
////                    var _layoutWidth = new OnIdiom<double> { Phone = screenWidth, Tablet = screenWidth * (IsPatientPage() ? 1 : 0.7) };
////                    double cardWidth = _layoutWidth / _recordCount;
////                    for (int i = 0; i < count; i++)
////                    {
////                        // i => used to create number for carousel 
////                        // j => used to create carousel item inside carousel cell
////                        // k => used to iterate over dashboardData.MeasurementDetails.MeasuremetUIData
////                        StackLayout obsStack = new StackLayout
////                        {
////                            Style = (Style)Application.Current.Resources[LibStyleConstants.ST_DEFAULT_END_TO_END_LAYOUT_KEY],
////                            Orientation = StackOrientation.Horizontal,
////                            HeightRequest = _carouselView.HeightRequest
////                        };
////                        int k = j;
////                        for (; j < k + _recordCount; j++)
////                        {
////                            if (j < ReadingData.ReadingDTOs.Count)
////                            {
////                                RenderCarauselCard(j, cardWidth, obsStack);
////                            }
////                        }
////                        if (obsStack.Children.Count > 0)
////                        {
////                            _carouselSource.Add(obsStack);
////                        }
////                    }
////                    _carouselView.ItemsSource = _carouselSource;
////                    _indicators.ItemsSource = _carouselSource;
////                    _indicators.IsVisible = _carouselSource.Count > 1;
////                    IsVisible = true;
////                }
////                else
////                {
////                    IsVisible = false;
////                }
////                ShowTitle = false;
////            });
////            await Task.CompletedTask;
////        }

////        private void RenderCarauselCard(int j, double cardWidth, StackLayout obsStack)
////        {
////            Grid readingGrid = new Grid
////            {
////                FlowDirection = AppStyles.DefaultFlowDirection,
////                WidthRequest = cardWidth,
////                BackgroundColor = AppStyles.NameSpace == "AlphaMDHealth.MobileClient"
////                    ? (Color)Application.Current.Resources[LibStyleConstants.ST_CONTROL_BACKGROUND_COLOR]
////                    : Color.Transparent
////            };
////            var content = new PatientReadingLibViewCell(true, true, _carouselView.HeightRequest)
////            {
////                BindingContext = ReadingData.ReadingDTOs[j],
////            };
////            ParentPage.AddSpacingForSeperatorLine(content, true);
////            readingGrid.Add(content, 0, 0);
////            Button readingGridAction = new Button { BindingContext = ReadingData.ReadingDTOs[j], HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, BorderColor = Color.Transparent, TextColor = Color.Transparent, Opacity = LibGenericMethods.GetPlatformSpecificValue(1, 0, 0) };
////            if (!IsPatientOverview(ReadingData.RecordCount))
////            {
////                readingGridAction.Clicked += async (s, e) =>
////                {
////                    AppHelper.ShowBusyIndicator = true;
////                    ReadingMetadataUIModel readingType = ((s as Button).BindingContext as PatientReadingDTO).ChartMetaData[0];
////                    if (MobileConstants.IsTablet)
////                    {
////                        await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientReadingsPage), LibGenericMethods.GenerateParamsWithPlaceholder(Param.type, Param.id), readingType.ReadingTypeIdentifier, readingType.ReadingCategoryID).ConfigureAwait(true);
////                    }
////                    else
////                    {
////                        await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientReadingDetailsPage), LibGenericMethods.GenerateParamsWithPlaceholder(Param.type, Param.name), readingType.ReadingTypeIdentifier, readingType.ReadingCategoryID).ConfigureAwait(true);
////                    }
////                };
////            }
////            readingGrid.Add(readingGridAction);
////            obsStack.Add(readingGrid);
////        }

////        /// <summary>
////        /// Unregister events of Views
////        /// </summary>
////        public override async Task UnloadUIAsync()
////        {
////            _carouselView.PositionChanged -= OnCarouselViewPositionChanged;
////            await Task.CompletedTask;
////        }

////        private void OnCarouselViewPositionChanged(object sender, PositionChangedEventArgs e)
////        {
////            MainThread.InvokeOnMainThreadAsync(() =>
////            {
////                _indicators.Position = e.CurrentPosition;
////            });
////        }
////    }

////    /// <summary>
////    /// View cell to detect indicator progress
////    /// </summary>
////    public class ReadingCarouselindicatorViewCell : ContentView
////    {
////        /// <summary>
////        /// View cell to detect indicator progress
////        /// </summary>
////        public ReadingCarouselindicatorViewCell()
////        {
////            var stack = new ContentView();
////            stack.SetBinding(ContentView.ContentProperty, LibConstants.SYMBOL_DOT_STRING);
////            Content = stack;
////        }
////    }
////}

////#endregion