using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Data.Helpers;
using DevExpress.Maui.CollectionView;
using System.Collections;
using SwipeItem = DevExpress.Maui.CollectionView.SwipeItem;

namespace AlphaMDHealth.MobileClient;

internal class AmhListViewControl<T> : AmhBaseControl
{
    private readonly Grid _container;
    private readonly AmhLabelControl _listHeader;
    private readonly AmhEntryControl _searchBar;
    private readonly AmhButtonControl _addButton;
    private DXCollectionView _collectionView;
    private readonly AmhMessageControl _message;

    private object _value;
    /// <summary>
    /// Control value as string
    /// </summary>
    internal object Value
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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(object), typeof(AmhListViewControl<T>), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhListViewControl<T> control = (AmhListViewControl<T>)bindable;
        if (newValue != null)
        {
            control.Value = newValue;
        }
    }

    internal ErrorCode ErrorCode { get; set; } = ErrorCode.OK;

    /// <summary>
    /// Show More label text to display on Group header Cell
    /// </summary>
    internal string GroupShowMoreText { get; set; }

    /// <summary>
    /// Show More Icon to display on Group header Cell
    /// </summary>
    internal string GroupShowMoreIcon { get; set; }

    /// <summary>
    /// Gets or sets whether the pull-to-refresh functionality is enabled in the CollectionView.
    /// Value: true to enable the pull-to-refresh functionality; otherwise, false.
    /// </summary>
    public bool IsPullToRefreshEnabled
    {
        get { return _collectionView.IsPullToRefreshEnabled; }
        set
        {
            if (_collectionView.IsPullToRefreshEnabled != value)
            {
                SetIsPullToRefreshEnabled(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the load-more functionality is enabled in the CollectionView.
    /// Value: true to enable the load-more functionality; otherwise, false.
    /// </summary>
    public bool IsLoadMoreEnabled
    {
        get { return _collectionView.IsLoadMoreEnabled; }
        set
        {
            if (_collectionView.IsLoadMoreEnabled != value)
            {
                SetIsLoadMoreEnabled(value);
            }
        }
    }

    private bool _isGroupedData;
    /// <summary>
    /// Data source to load on list
    /// </summary>
    internal bool IsGroupedData
    {
        get
        {
            return _isGroupedData;
        }
        set
        {
            if (_isGroupedData != value)
            {
                _isGroupedData = value;
                RenderControl();
            }
        }
    }

    private IEnumerable<T> _dataSource;
    /// <summary>
    /// Data source to load on list
    /// </summary>
    internal IEnumerable<T> DataSource
    {
        get
        {
            return _dataSource;
        }
        set
        {
            if (_dataSource != value)
            {
                _dataSource = value;
                ApplyDataSource();
            }
        }
    }

    private AmhViewCellModel _sourceFields;
    /// <summary>
    /// Placeholder fields of data source
    /// </summary>
    internal AmhViewCellModel SourceFields
    {
        get
        {
            return _sourceFields;
        }
        set
        {
            if (_sourceFields != value)
            {
                _sourceFields = value;
            }
        }
    }

    private bool _showSearchBar = true;
    /// <summary>
    /// Flag to decide search bar will be visible or not
    /// </summary>
    internal bool ShowSearchBar
    {
        get
        {
            return _showSearchBar;
        }
        set
        {
            if (_showSearchBar != value)
            {
                _showSearchBar = value;
                SetSearchOption();
            }
        }
    }

    private bool _showAddButton = true;
    /// <summary>
    /// Flag to decide add label will be visible or not
    /// </summary>
    internal bool ShowAddButton
    {
        get
        {
            return _showAddButton;
        }
        set
        {
            if (_showAddButton != value)
            {
                _showAddButton = value;
                SetAddButton();
            }
        }
    }

    /// <summary>
    /// Cell Right Content Description
    /// </summary>
    internal List<SwipeItem> StartSwapItems { get; set; } = null;

    /// <summary>
    /// Cell Right Content Description
    /// </summary>
    internal List<SwipeItem> EndSwapItems { get; set; } = null;

    /// <summary>
    /// Event for invoke on click of end icon or view
    /// </summary>
    internal event EventHandler<EventArgs> OnGroupShowMoreClicked = null;

    /// <summary>
    /// Event for invoke on click of end icon or view
    /// </summary>
    internal event EventHandler<EventArgs> OnRightViewClicked = null;

    /// <summary>
    /// Event for load more option
    /// </summary>
    internal event EventHandler<EventArgs> OnLoadMore = null;

    /// <summary>
    /// Event for pull to refresh option
    /// </summary>
    internal event EventHandler<EventArgs> OnPullToRefresh = null;

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhListViewControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhListViewControl(FieldTypes controlType) : base(controlType)
    {
        _container = CreateGridContainer(false);
        _listHeader = new AmhLabelControl(FieldTypes.PrimarySmallHStartVCenterLabelControl) { ResourceKey = ResourceKey };
        _searchBar = new AmhEntryControl(FieldTypes.TextEntryControl)
        {
            Icon = ImageConstants.I_SEARCH_ICON_PNG,
            ResourceKey = @ResourceConstants.R_SEARCH_TEXT_KEY,
            ShowHeader = false,
        };
        _addButton = new AmhButtonControl(FieldTypes.TransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_ADD_ACTION_KEY,
        };
        _message = new AmhMessageControl(FieldTypes.MessageControl)
        {
            ResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY,
        };
        InitializeCollectionView();
        Content = _container;
    }

    private object GetControlValue()
    {
        _value = _collectionView.SelectedItem;
        return _value;
    }

    private void SetControlValue()
    {
        //SelectedItem = "{Binding SelectedContact}
        _collectionView.SelectedItem = _value;
    }

    protected override void RenderControl()
    {
        if (_sourceFields != null && _dataSource != null)
        {
            ResetRowsAndColumns();
            RemoveCollectionView();
            _collectionView = new DXCollectionView
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_DX_COLLECTION_VIEW_STYLE],
                AllowCascadeUpdate = false,
                ReduceSizeToContent = true,
                ItemTemplate = new DataTemplate(() =>
                {
                    var swipeContainer = new SwipeContainer
                    {
                        ItemView = new AmhListViewCell(_sourceFields, false, OnRightViewClicked)
                    };
                    AddSwapItems(swipeContainer);
                    return swipeContainer;
                }),
                SelectedItemTemplate = new DataTemplate(() =>
                {
                    return new AmhListViewCell(_sourceFields, true, OnRightViewClicked);
                })
            };
            if (_isGroupedData)
            {
                _collectionView.GroupDescription = new GroupDescription
                {
                    FieldName = _sourceFields.GroupName,
                    GroupInterval = DevExpress.Maui.Core.DataGroupInterval.Value
                };
                _collectionView.GroupHeaderTemplate = new DataTemplate(() =>
                {
                    return new AmhListGroupViewCell(GroupShowMoreText, GroupShowMoreIcon, OnGroupShowMoreClicked);
                });
            }
            AddViewsInContainer();
        }
    }

    protected override void ApplyResourceValue()
    {
        _message.PageResources = _listHeader.PageResources = _searchBar.PageResources = _addButton.PageResources = PageResources;
        _listHeader.Value = _resource?.ResourceValue;
        InitializeCollectionView();
    }

    protected override void EnabledDisableField(bool value)
    {
    }

    protected override void ApplyOptions()
    {
    }

    private void OnSelectionChanged(object sender, CollectionViewSelectionChangedEventArgs e)
    {
        OnValueChangedAction(sender, e);
    }

    private void OnRightViewTapped(object sender, TappedEventArgs e)
    {
        OnRightViewClicked?.Invoke(sender, e);
    }

    private void OnSearchTextChanged(object sender, EventArgs e)
    {
        //_collectionView.FilterExpression = new FunctionOperator(FunctionOperatorType.Contains, new OperandProperty(_sourceFields.LeftHeader), new ConstantValue("M"));
    }

    private void OnLoadMoreAction(object sender, EventArgs e)
    {
        //var lastItem = _itemSource.Last(); 
        OnLoadMore?.Invoke(sender, e);
    }

    private void OnPullToRefreshAction(object sender, EventArgs e)
    {
        OnPullToRefresh.Invoke(sender, e);
    }

    private void AddSwapItems(SwipeContainer swipeContainer)
    {
        AddSwapItem(swipeContainer.StartSwipeItems, StartSwapItems);
        AddSwapItem(swipeContainer.EndSwipeItems, EndSwapItems);
    }

    private void AddSwapItem(SwipeItemCollection swipeContainer, List<SwipeItem> swipeItems)
    {
        if (swipeItems?.Count > 0)
        {
            foreach (SwipeItem swapItem in swipeItems)
            {
                swipeContainer.Add(swapItem);
            }
        }
    }

    private void ApplyDataSource()
    {
        InitializeCollectionView();
        _collectionView.ItemsSource = null;
        SetListViewContent();
        SetIsPullToRefreshEnabled(_collectionView.IsPullToRefreshEnabled);
        SetIsLoadMoreEnabled(_collectionView.IsLoadMoreEnabled);
    }

    private void SetListViewContent()
    {
        if (ErrorCode == ErrorCode.OK && _dataSource != null && _dataSource.Count() > 0)
        {
            RemoveMessageView();
            AddCollectionView();
            _collectionView.ItemsSource = _dataSource;
        }
        else
        {
            RemoveCollectionView();
            AddMessageView();
            _message.ResourceKey = ErrorCode == ErrorCode.OK ? ResourceConstants.R_NO_DATA_FOUND_KEY : ErrorCode.ToString();
        }
    }

    private void InitializeCollectionView()
    {
        if (_collectionView == null)
        {
            RenderControl();
        }
    }

    private void SetIsPullToRefreshEnabled(bool value)
    {
        _collectionView.PullToRefresh -= OnPullToRefreshAction;
        _collectionView.IsPullToRefreshEnabled = value;
        if (value)
        {
            _collectionView.PullToRefresh += OnPullToRefreshAction;
        }
    }

    private void SetIsLoadMoreEnabled(bool value)
    {
        _collectionView.LoadMore -= OnLoadMoreAction;
        _collectionView.IsLoadMoreEnabled = value;
        if (value)
        {
            _collectionView.LoadMore += OnLoadMoreAction;
        }
    }

    private void SetTitle()
    {
        _container.Remove(_listHeader);
        if (ShowHeader)
        {
            _container.Add(_listHeader, 0, 0);
        }
    }

    private void SetSearchOption()
    {
        _searchBar.OnValueChanged -= OnSearchTextChanged;
        _container.Remove(_searchBar);
        if (_showSearchBar)
        {
            if (MobileConstants.IsTablet)
            {
                _container.Add(_searchBar, 1, 0);
            }
            else
            {
                _container.Add(_searchBar, 0, 1);
            }
            //searchBar._entry.PlaceholderText = GetRequiredResourceValue();
            _searchBar.OnValueChanged += OnSearchTextChanged;
        }
    }

    private void SetAddButton()
    {
        _addButton.OnValueChanged -= OnValueChangedAction;
        _container.Remove(_addButton);
        if (_showSearchBar)
        {
            if (MobileConstants.IsTablet)
            {
                _container.Add(_addButton, 2, 0);
            }
            else
            {
                _container.Add(_addButton, 1, 1);
            }
            _addButton.OnValueChanged += OnValueChangedAction;
        }
    }

    private void AddViewsInContainer()
    {
        SetTitle();
        SetSearchOption();
        SetAddButton();
        AddCollectionView();
    }

    private void AddCollectionView()
    {
        if (!_container.Contains(_collectionView))
        {
            if (MobileConstants.IsTablet)
            {
                _container.Add(_collectionView, 0, 1);
                _container.SetRowSpan(_collectionView, 2);
            }
            else
            {
                _container.Add(_collectionView, 0, 2);
            }
            _container.SetColumnSpan(_collectionView, 3);
            _collectionView.SelectionChanged += OnSelectionChanged;
        }
    }

    private void AddMessageView()
    {
        if (!_container.Contains(_message))
        {
            if (MobileConstants.IsTablet)
            {
                _container.Add(_message, 0, 1);
                _container.SetRowSpan(_message, 2);
            }
            else
            {
                _container.Add(_message, 0, 2);
            }
            _container.SetColumnSpan(_message, 3);
        }
    }

    private void RemoveCollectionView()
    {
        if (_collectionView != null && _container.Contains(_collectionView))
        {
            _collectionView.SelectionChanged -= OnSelectionChanged;
            _container.Remove(_collectionView);
        }
    }

    private void RemoveMessageView()
    {
        if (_container.Contains(_message))
        {
            _container.Remove(_message);
        }
    }

    private void ResetRowsAndColumns()
    {
        _container.Children?.Clear();
        _container.RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition { Height = GridLength.Auto }, // Headers(heading, searchbar, add)
            new RowDefinition { Height = GridLength.Auto }, // Headers(heading, searchbar, add)
            new RowDefinition { Height = GridLength.Star }  // actual collection view
        };

        _container.ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Star },  // Heading 
            new ColumnDefinition { Width = GridLength.Auto },  //SearchBar
            new ColumnDefinition { Width = GridLength.Auto }   //Add
        };
    }

    ////private async void OnPullToRefresh(object sender, EventArgs e)
    ////{
    ////    await Task.Delay(1000);
    ////    List<AppointmentModel> shuffledData = Shuffle(DataSource);
    ////    ItemSource.Clear();
    ////    DataSource = shuffledData;
    ////    _collectionView.ItemsSource = DataSource;
    ////    _collectionView.IsRefreshing = false;
    ////}

    //private List<AppointmentModel> Shuffle(List<AppointmentModel> collection)
    //{
    //    // shuffle it
    //    List<AppointmentModel> list = collection.ToList();
    //    Random rng = new Random();
    //    int n = list.Count;
    //    while (n > 1)
    //    {
    //        n--;
    //        int k = rng.Next(n + 1);
    //        AppointmentModel value = list[k];
    //        list[k] = list[n];
    //        list[n] = value;
    //    }
    //    return new List<AppointmentModel>(list);
    //}

    //private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    //{
    //    var searchText = e.NewTextValue.ToLower();
    //    if (string.IsNullOrWhiteSpace(searchText))
    //    {
    //        _collectionView.ItemsSource = ItemSource;
    //    }
    //    else
    //    {
    //        var filteredData = DataSource.Where(appointment =>
    //            appointment.AppointmentID.ToString().Contains(searchText) ||
    //            appointment.AppointmentTypeName.ToLower().Contains(searchText) ||
    //            appointment.ContentKey.ToLower().Contains(searchText) ||
    //            appointment.Description.ToLower().Contains(searchText) ||
    //            appointment.PageHeading.ToLower().Contains(searchText) ||
    //            appointment.PageData.ToLower().Contains(searchText)
    //           ).ToList();
    //        _collectionView.ItemsSource = filteredData;
    //        _collectionView.IsLoadMoreEnabled = false;
    //    }
    //}
}