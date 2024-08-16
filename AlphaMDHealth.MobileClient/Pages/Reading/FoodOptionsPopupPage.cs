using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;
using System.Runtime.Serialization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Search Food Popup Page
/// </summary>
public class FoodOptionsPopupPage : BasePopupPage
{
    private readonly CollectionView _foodList;
    private readonly CustomSearchControl _customSearch;
    private List<OptionModel> _searchedFoodList;

    /// <summary>
    /// List of serched food items
    /// </summary>
    [DataMember]
    public List<OptionModel> SearchedFoodList
    {
        get => _searchedFoodList;
        set
        {
            _foodList.ItemsSource =  _searchedFoodList = value;               
        }
    }

    /// <summary>
    /// Search text
    /// </summary>
    public string SearchedText { get; set; }

    /// <summary>
    /// on click event of Send Button
    /// </summary>
    public event EventHandler<EventArgs> OnSelectFoodItem;

    /// <summary>
    /// Default constructor 
    /// </summary>
    public FoodOptionsPopupPage() : base(new BasePage(PageLayoutType.EndToEndPageLayout, false))
    {
        //todo: CloseWhenBackgroundIsClicked = false;
        Padding = new OnIdiom<Thickness>
        {
            Phone = new Thickness(0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture) * 3, 0, 0),
            Tablet = _parentPage.GetPopUpPagePadding(PopUpPageType.Long)
        };
        _parentPage.PageLayout.Padding = new Thickness((double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], 10);
        _parentPage.AddRowColumnDefinition(GridLength.Auto, 1, true);
        _parentPage.AddRowColumnDefinition(GridLength.Star, 1, true);
        _customSearch = new CustomSearchControl
        {
            ControlResourceKey = ResourceConstants.R_SEARCH_TEXT_KEY,
            SearchedOn = SearchedOnType.Both,
            SearchOutSideViewHide = false,
            IsAppliedMargin = false,
        };
        CustomCellModel customCellModel = new CustomCellModel
        {
            CellHeader = nameof(OptionModel.OptionText),
            CellLeftIconPath = nameof(OptionModel.ParentOptionText),
            IconSize = AppImageSize.ImageSizeL
        };
        _foodList = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                SnapPointsType = SnapPointsType.None,
                SnapPointsAlignment = SnapPointsAlignment.Center,
                ItemSpacing = new OnIdiom<double> { Phone = 10, Tablet = 0 }
            },
            ItemTemplate = new DataTemplate(() =>
            {
                if (MobileConstants.IsTablet)
                {
                    return new ContentView { Style = (Style)Application.Current.Resources[StyleConstants.ST_SELECTED_CONTENT_STYLE], Content = new ResponsiveView(customCellModel), Padding = GenericMethods.GetPlatformSpecificValue(new Thickness(1, 1, 1, 0), new Thickness(0), new Thickness(1, 1, 1, 0)) };
                }
                return new ContentView { Style = (Style)Application.Current.Resources[StyleConstants.ST_SELECTED_CONTENT_STYLE], Content = new ResponsiveView(customCellModel) };
            })
        };
        _parentPage.PageLayout.Add(_customSearch, 0, 0);
        _parentPage.PageLayout.Add(_foodList, 0, 1);
    }

    /// <summary>
    /// Appearing page
    /// </summary>
    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        base.OnAppearing();
        await _parentPage.GetResourcesAsync(GroupConstants.RS_COMMON_GROUP).ConfigureAwait(true);
        _customSearch.PageResources = _parentPage.PageData;
        _customSearch.Value = string.Empty;
        SetTitle(GetHeaderText(_searchedFoodList.Count));
        if (MobileConstants.IsTablet)
        {
            DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
            OnLeftHeaderClickedEvent += OnCancelClicked;
        }
        else
        {
            ShowCloseButton(true);
            OnCloseButtonClickedEvent += OnCancelClicked;
        }
        _foodList.ItemsSource = _searchedFoodList;
        _customSearch.OnSearchTextChanged += OnSearchFood;
        _foodList.SelectionChanged += OnFoodItemSelect;
        AppHelper.ShowBusyIndicator = false;
    }

    private string GetHeaderText(int count)
    {
        return $"{SearchedText} ({count})";
    }

    /// <summary>
    /// Disappearing page
    /// </summary>
    protected override void OnDisappearing()
    {
        _foodList.SelectionChanged -= OnFoodItemSelect;
        if (MobileConstants.IsTablet)
        {
            OnLeftHeaderClickedEvent -= OnCancelClicked;
        }
        else
        {
            OnCloseButtonClickedEvent -= OnCancelClicked;
        }
        base.OnDisappearing();
    }

    private async void OnFoodItemSelect(object sender, SelectionChangedEventArgs e)
    {
        var foodItem = (sender as CollectionView).SelectedItem;
        OnSelectFoodItem.Invoke(foodItem, new EventArgs());
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnSearchFood(object sender, EventArgs e)
    {
        var serchBar = sender as CustomSearchBar;
        _foodList.Footer = null;
        if (string.IsNullOrWhiteSpace(serchBar.Text))
        {
            _foodList.ItemsSource = new List<OptionModel>();
            await Task.Delay(10);
            _foodList.ItemsSource = _searchedFoodList;
            SetTitle(GetHeaderText(_searchedFoodList.Count));
        }
        else
        {
            var searchedFood = _searchedFoodList.FindAll(y =>
            {
                return y.OptionText.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant());
            });
            _foodList.ItemsSource = searchedFood;
            SetTitle(GetHeaderText(searchedFood.Count));
            if (searchedFood.Count <= 0 && _foodList.EmptyView ==  null)
            {
                _foodList.EmptyView = new CustomLabelControl(LabelType.SecondrySmallCenter)
                {
                    Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_NO_DATA_FOUND_KEY),
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                };
            }
        }
    }
}