using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Base for Collection View
/// </summary>
public class BaseLibCollectionView : ViewManager
{
    private readonly Style _selectedStyle = (Style)Application.Current.Resources[StyleConstants.ST_SELECTED_CONTENT_STYLE];

    /// <summary>
    /// Indicates whether the collection view is Horizontal or Vertical
    /// </summary>
    protected bool isHorizontal;

    /// <summary>
    /// on click event of cell
    /// </summary>
    public event EventHandler<EventArgs> OnActionButtonClicked;

    /// <summary>
    /// Required height of each cell
    /// </summary>
    public double CellRowHeight { get; set; }

    /// <summary>
    /// Reference of search control
    /// </summary>
    public CustomSearchControl SearchField { get; set; }

    /// <summary>
    /// Reference of collection view
    /// </summary>
    public CollectionView CollectionViewField { get; set; }

    /// <summary>
    /// TabletHeader
    /// </summary>
    public CustomLabelControl TabletHeader { get; set; }

    /// <summary>
    /// TabletActionButton
    /// </summary>
    public CustomLabelControl TabletActionButton { get; set; }

    /// <summary>
    /// Reference of list view
    /// </summary>
    public CustomListView ListViewField { get; set; }

    /// <summary>
    /// Reference of separator boxview
    /// </summary>
    public BoxView Separator { get; set; }

    /// <summary>
    /// tablet header with header text and 
    /// </summary>
    public bool IsTabletListHeaderDisplay { get; set; }

    /// <summary>
    /// Empty collection view
    /// </summary>
    protected readonly CustomMessageControl _emptyListView;

    /// <summary>
    /// App padding
    /// </summary>
    protected readonly double _margin = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];

    /// <summary>
    /// Width of screen
    /// </summary>
    protected readonly double _screenWidth = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, (double)0) -
        (2 * Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.InvariantCulture));

    /// <summary>
    /// Default constructor
    /// </summary>
    protected BaseLibCollectionView() : this(null, null)
    {
    }

    /// <summary>
    /// Parameterized constructor containing page inance
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    protected BaseLibCollectionView(BasePage page, object parameters) : base(page, parameters)
    {
        _emptyListView = new CustomMessageControl(false);
    }

    /// <summary>
    /// Method to perform selection mode and register event of collection view
    /// </summary>
    /// <param name="onSelectionChanged">Event to register or unregister</param>
    /// <param name="isRegister">Flag which decides need to register or unregister</param>
    public void OnListItemSelection(EventHandler<SelectionChangedEventArgs> onSelectionChanged, bool isRegister)
    {
        if (CollectionViewField != null)
        {
            CollectionViewField.SelectionChanged -= onSelectionChanged;
            if (isRegister)
            {
                CollectionViewField.SelectionMode = SelectionMode.Single;
                CollectionViewField.SelectionChanged += onSelectionChanged;
            }
            else
            {
                CollectionViewField.SelectionMode = SelectionMode.None;
            }
        }
    }

    /// <summary>
    /// Adds search control to given search view
    /// </summary>
    /// <param name="mainLayout">Layout to which search control is to be added</param>
    ///<param name="appliedMargin">margin to search</param>
    public void AddSearchView(Grid mainLayout, bool appliedMargin)
    {
        CreateSearch(appliedMargin, false);
        mainLayout?.Add(SearchField, 0, 0);
    }

    private void CreateSearch(bool appliedMargin, bool hideBG)
    {
        SearchField = new CustomSearchControl
        {
            ControlResourceKey = ResourceConstants.R_SEARCH_TEXT_KEY,
            SearchedOn = SearchedOnType.Both,
            SearchOutSideViewHide = hideBG,
            IsAppliedMargin = appliedMargin
        };
    }

    /// <summary>
    /// Adds separator view to given layout
    /// </summary>
    /// <param name="mainLayout">Layout to which separator is to be added</param>
    /// <param name="left">Column position</param>
    /// <param name="top">Row position</param>
    public void AddSeparatorView(Grid mainLayout, int left, int top)
    {
        Separator = new BoxView
        {
            WidthRequest = 1,
            HorizontalOptions = LayoutOptions.Start,
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE]
        };
        mainLayout?.Add(Separator, left, top);
    }

    /// <summary>
    /// Add list view to given layout
    /// </summary>
    /// <param name="mainLayout">Layout to which ListView is to be added</param>
    /// <param name="customCellModel">Item model</param>
    /// <param name="left">Column position</param>
    /// <param name="top">Row position</param>
    public void AddListView(Grid mainLayout, CustomCellModel customCellModel, int left, int top)
    {
        AddListView(mainLayout, customCellModel, left, top, null, ListViewCachingStrategy.RetainElement, StyleConstants.TRANSPARENT_COLOR_STRING);
    }

    /// <summary>
    /// Add list view to given layout
    /// </summary>
    /// <param name="mainLayout">Layout to which ListView is to be added</param>
    /// <param name="customCellModel">Item model</param>
    /// <param name="left">Column position</param>
    /// <param name="top">Row position</param>
    /// <param name="style">style</param>
    /// <param name="listViewCachingStrategy">listViewCachingStrategy</param>
    public void AddListView(Grid mainLayout, CustomCellModel customCellModel, int left, int top, Style style, ListViewCachingStrategy listViewCachingStrategy, string cellColor)
    {
        ListViewField = new CustomListView(listViewCachingStrategy)
        {
            Style = style ?? (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_LIST_KEY],
            ItemTemplate = new DataTemplate(() =>
            {
                return new ViewCell
                {
                    View = new ContentView
                    {
                        Style = _selectedStyle,
                        BackgroundColor = Color.FromArgb(cellColor),
                        Content = new ResponsiveView(customCellModel)
                    },
                    IsEnabled = customCellModel.IsEnabled
                };
            })
        };
        CellRowHeight = customCellModel.ArrangeHorizontal
            ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture)
            : (double)customCellModel.IconSize + 2 * _margin + new OnIdiom<double> { Phone = 6, Tablet = 8 };
        mainLayout?.Add(ListViewField, left, top);
    }

    /// <summary>
    /// AddCollectionViewWithHeader for Tablet
    /// </summary>
    /// <param name="mainLayout"></param>
    /// <param name="customCellModel"></param>
    public void AddCollectionViewWithTabletHeader(Grid mainLayout, CustomCellModel customCellModel)
    {
        TabletHeader = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft);
        mainLayout.Add(TabletHeader, 0, 0);
        CreateSearch(true, true);
        mainLayout.Add(SearchField, 1, 0);
        TabletActionButton = new CustomLabelControl(LabelType.LinkLabelSmallLeft)
        {
            TextDecorations = TextDecorations.None,
            Padding = AppStyles.DefaultFlowDirection == Microsoft.Maui.FlowDirection.LeftToRight
                ? new Thickness(15, 0, 0, 0)
                : new Thickness(0, 0, 15, 0)
        };
        TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
        TabletActionButton.GestureRecognizers.Add(tapGestureRecognizer);
        mainLayout.Add(TabletActionButton, 2, 0);
        if (customCellModel != null)
        {
            AddCollectionView(mainLayout, customCellModel, 0, 1);
            Grid.SetColumnSpan(CollectionViewField, 3);
        }
    }

    /// <summary>
    /// hide and show add button
    /// </summary>
    /// <param name="mainLayout">mainLayout</param>
    /// <param name="hideButton">hideAddButton</param>
    public void HideAddButton(Grid mainLayout, bool hideButton)
    {
        if (hideButton)
        {
            TabletActionButton.IsVisible = false;
            SearchField.Margin = new Thickness(-mainLayout.ColumnSpacing, 0);
        }
        else
        {
            SearchField.Margin = new Thickness(0);
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        OnActionButtonClicked?.Invoke(sender, new EventArgs());
    }

    /// <summary>
    /// Add collection view to given layout
    /// </summary>
    /// <param name="mainLayout">Layout to which ListView is to be added</param>
    /// <param name="customCellModel">Item model</param>
    /// <param name="left">Column position</param>
    /// <param name="top">Row position</param>
    public virtual void AddCollectionView(Grid mainLayout, CustomCellModel customCellModel, int left, int top)
    {
        CollectionViewField = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemsLayout = ItemLayoutCreate(customCellModel.ArrangeHorizontal),
            Margin = new Thickness(0, _margin, 0, 0),
            ItemTemplate = new DataTemplate(() =>
            {
                var content = new ContentView
                {
                    Style = _selectedStyle,
                    Content = new ResponsiveView(customCellModel),
                };
                ////if (MobileConstants.IsTablet)
                ////{
                ////    content.Padding = LibGenericMethods.GetPlatformSpecificValue(new Thickness(1, 1, 1, 0), new Thickness(0), new Thickness(1, 1, 1, 0));
                ////}
                return content;
            }),
        };
        isHorizontal = customCellModel.ArrangeHorizontal;
        CellRowHeight = customCellModel.ArrangeHorizontal
            ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture)
            : (double)customCellModel.IconSize + 2 * _margin + new OnIdiom<double> { Phone = _margin + 8, Tablet = 8 };
        mainLayout?.Add(CollectionViewField, left, top);
    }

    /// <summary>
    /// Create column definitions for tablet view
    /// </summary>
    /// <returns>Column definitions</returns>
    public ColumnDefinitionCollection CreateTabletViewColumn(bool isDashboard)
    {
        if (IsTabletListHeaderDisplay)
        {
            return new ColumnDefinitionCollection
            {
                new ColumnDefinition{ Width = new GridLength(2, GridUnitType.Star)},
                new ColumnDefinition{ Width = new GridLength(1.5, GridUnitType.Star) },
                new ColumnDefinition{ Width = new GridLength(1, GridUnitType.Auto) }
            };
        }
        else
        {
            return /*AppStyles.IsTabletScaledView || */isDashboard ? CreateTabletViewFullWidthColumn() : new ColumnDefinitionCollection
            {
                new ColumnDefinition{ Width = new GridLength(0.29, GridUnitType.Star) },
                new ColumnDefinition{ Width = new GridLength(1, GridUnitType.Absolute) },
                new ColumnDefinition{ Width = new GridLength(0.70, GridUnitType.Star) }
            };
        }
    }

    /// <summary>
    /// Create full-width column definitions for tablet view
    /// </summary>
    /// <returns>Column definitions</returns>
    public ColumnDefinitionCollection CreateTabletViewFullWidthColumn()
    {
        return new ColumnDefinitionCollection
        {
                new ColumnDefinition{ Width = new GridLength(1, GridUnitType.Star) }
        };
    }

    /// <summary>
    /// Provides layout
    /// </summary>
    /// <param name="isHorizontal">flag to decide is horizantal orientation</param>
    /// <returns>returns item layout </returns>
    public ItemsLayout ItemLayoutCreate(bool isHorizontal)
    {
        if (isHorizontal)
        {
            return new GridItemsLayout(ItemsLayoutOrientation.Horizontal)
            {
                SnapPointsType = SnapPointsType.None,
                SnapPointsAlignment = SnapPointsAlignment.End,
                HorizontalItemSpacing = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING],
            };
        }
        else
        {
            return new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                SnapPointsType = SnapPointsType.None,
                SnapPointsAlignment = SnapPointsAlignment.Center,
                ItemSpacing = new OnIdiom<double> { Phone = 10, Tablet = 0 }
            };
        }
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override Task LoadUIAsync(bool isRefreshRequest)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override Task UnloadUIAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// DashBoardListView Scroll disable
    /// </summary>
    /// <param name="_list">List view</param>
    /// <param name="_mainLayout">_mainLayout Grid</param>
    /// <param name="itemCount">item count</param>
    public void DashBoardListView(CustomListView _list, Grid _mainLayout, int itemCount)
    {
        DashBoardListView(_list, _mainLayout, itemCount, true);
    }

    /// <summary>
    /// DashBoardListView Scroll disable
    /// </summary>
    /// <param name="_list">List view</param>
    /// <param name="_mainLayout">_mainLayout Grid</param>
    /// <param name="itemCount">item count</param>
    /// <param name="isDashboard">is dashbord views</param>
    public void DashBoardListView(CustomListView _list, Grid _mainLayout, int itemCount, bool isDashboard)
    {
        if (isDashboard)
        {
            _list.Footer = null;
            _list.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
            _list.HeightRequest = CellRowHeight * itemCount;
            _list.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
            _mainLayout.HeightRequest = CellRowHeight * itemCount;
        }
        else
        {
            //todo:
            //_list.BackgroundColor = Color.Transparent;
            _list.Footer = string.Empty;
        }
    }

    /// <summary>
    /// Render empty view
    /// </summary>
    /// <param name="mainLayout">Page layout</param>
    /// <param name="resourceKey">Error key</param>
    /// <param name="isDashboardView">Flag to check dashboard view</param>
    [Obsolete("Use RenderErrorView(Grid mainLayout, string resourceKey, bool isDashboardView, double height, bool isListEmptyView, bool showIcon) in place of it")]
    public void RenderErrorView(Grid mainLayout, string resourceKey, bool isDashboardView)
    {
        RenderErrorView(mainLayout, resourceKey, isDashboardView, 0, false, true);
    }

    /// <summary>
    /// Render empty view
    /// </summary>
    /// <param name="mainLayout">Page layout</param>
    /// <param name="resourceKey">Error key</param>
    [Obsolete("Use RenderErrorView(Grid mainLayout, string resourceKey, bool isDashboardView, double height, bool isListEmptyView, bool showIcon) in place of it")]
    public void RenderListErrorView(Grid mainLayout, string resourceKey)
    {
        RenderErrorView(mainLayout, resourceKey, false, 0, true, false);
    }

    /// <summary>
    /// Render empty view
    /// </summary>
    /// <param name="mainLayout">Page layout</param>
    /// <param name="resourceKey">Error key</param>
    /// <param name="isDashboardView">Flag to check dashboard view</param>
    /// <param name="height">Layout options</param>
    [Obsolete("Use RenderErrorView(Grid mainLayout, string resourceKey, bool isDashboardView, double height, bool isListEmptyView, bool showIcon) in place of it")]
    public void RenderErrorView(Grid mainLayout, string resourceKey, bool isDashboardView, double height)
    {
        RenderErrorView(mainLayout, resourceKey, isDashboardView, height, false, true);
    }

    /// <summary>
    /// Render empty view
    /// </summary>
    /// <param name="mainLayout">Page layout</param>
    /// <param name="resourceKey">Error key</param>
    /// <param name="isDashboardView">Flag to check dashboard view</param>
    /// <param name="height">Layout options</param>
    /// <param name="isListEmptyView">Flag to decide which view to render in list page</param>
    /// <param name="showIcon">Flag to decide No record message will display with icon or not</param>
    public void RenderErrorView(Grid mainLayout, string resourceKey, bool isDashboardView, double height, bool isListEmptyView, bool showIcon)
    {
        if (CollectionViewField != null)
        {
            _emptyListView.ControlResourceKey = resourceKey;
            if (isDashboardView)
            {
                mainLayout.HeightRequest = height < 1 ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT], CultureInfo.InvariantCulture) : height;
                CollectionViewField.ItemsSource = new List<string>();
                CollectionViewField.Header = null;
                CollectionViewField.Footer = new ContentView
                {
                    Padding = isListEmptyView ? new Thickness(0) : new Thickness(1, 1, 2, 2),
                    //todo:
                    //Content = new PancakeView
                    //{
                    //    Style = (Style)Application.Current.Resources[LibStyleConstants.ST_PANCAKE_STYLE],
                    //    BorderColor = isListEmptyView ? Color.Transparent : (Color)Application.Current.Resources[LibStyleConstants.ST_SEPERATOR_COLOR_STYLE],
                    //    BackgroundColor = (Color)Application.Current.Resources[isListEmptyView ? LibStyleConstants.ST_AFTER_LOGIN_BACKGROUND_COLOR : LibStyleConstants.ST_CONTROL_BACKGROUND_COLOR],
                    Content = RenderNoRecordLebel(resourceKey, height, isListEmptyView)
                    //}
                };
                CollectionViewField.HeightRequest = height;
            }
            else if (!showIcon)
            {
                CollectionViewField.EmptyView = RenderNoRecordLebel(resourceKey, height, isListEmptyView);
            }
            else
            {
                CollectionViewField.EmptyView = _emptyListView;
            }
        }
        else if (ListViewField != null)
        {
            LoadErrorMessageInListView(mainLayout, resourceKey, isDashboardView, height);
        }
        else
        {
            //for future implementation
        }
    }

    private void LoadErrorMessageInListView(Grid mainLayout, string resourceKey, bool isDashboardView, double height)
    {
        ListViewField.ItemsSource = null;
        _emptyListView.ControlResourceKey = resourceKey;
        if (isDashboardView)
        {
            mainLayout.HeightRequest = height < 1 ? GetHeightForCell() : height;

            ListViewField.Footer = new CustomLabelControl(LabelType.SecondrySmallCenter)
            {
                VerticalTextAlignment = TextAlignment.Center,
                HeightRequest = mainLayout.HeightRequest,
                Text = ParentPage.GetResourceValueByKey(resourceKey),
            };
            ListViewField.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
        }
        else
        {
            _emptyListView.VerticalOptions = LayoutOptions.Center;
            _emptyListView.HeightRequest = height < 1 ? App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, (double)0) * 0.7 : height;
            mainLayout.HeightRequest = _emptyListView.HeightRequest;
            ListViewField.HeightRequest = _emptyListView.HeightRequest;
            ListViewField.Footer = _emptyListView;
        }
    }

    private double GetHeightForCell()
    {
        return ListViewField.RowHeight == 0 ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT], CultureInfo.InvariantCulture) : ListViewField.RowHeight;
    }

    private CustomLabelControl RenderNoRecordLebel(string resourceKey, double height, bool isListEmptyView)
    {
        var lebel = new CustomLabelControl(isListEmptyView ? LabelType.SecondrySmallCenter : LabelType.TertiarySmallCenter)
        {
            Text = ParentPage.GetResourceValueByKey(resourceKey),
            HorizontalOptions = LayoutOptions.CenterAndExpand,
            VerticalOptions = LayoutOptions.CenterAndExpand,
            WidthRequest = CollectionViewField.WidthRequest > 1 ? CollectionViewField.WidthRequest : -1
        };
        if (height > 0)
        {
            lebel.HeightRequest = height - 18;
        }
        return lebel;
    }

    /// <summary>
    /// Override for reassigning ItemsLayout of collectionview
    /// </summary>
    /// <param name="width">Current Width</param>
    /// <param name="height">Current Height</param>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (MobileConstants.IsAndroidPlatform && CollectionViewField != null)
        {
            CollectionViewField.ItemsLayout = ItemLayoutCreate(isHorizontal);
        }
    }
}