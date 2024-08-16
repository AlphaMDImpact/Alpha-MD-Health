using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Class to create and handle dropdown multi select control
    /// </summary>
    public class CustomDropDownMultiSelectControl : BaseContentView
    {
        private readonly CollectionView _collections;
        private readonly CustomSearchControl _customSearch;
        private readonly CustomMessageControl _messageView;
        private readonly CustomLabelControl _errorLabel;
        private readonly CustomButtonControl _addButton;
        private readonly Grid _mainLayout;
        private string _htmlPrefixText;
        private readonly double _cellHeight;
        private readonly double _margin = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
        /// <summary>
        /// Event handler on seach list
        /// </summary>
        public event EventHandler<EventArgs> OnSearchList;

        /// <summary>
        /// Event handler on add event
        /// </summary>
        public event EventHandler<EventArgs> OnAddEvent;

        /// <summary>
        /// Item source property with property changed event handler
        /// </summary>
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.
            Create(nameof(ItemsSource), typeof(IEnumerable), typeof(CustomDropDownMultiSelectControl), propertyChanged: OnItemsSourceChanged);

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            CustomDropDownMultiSelectControl control = bindable as CustomDropDownMultiSelectControl;
            control._collections.ItemsSource = Enumerable.Empty<object>();
            control._collections.ItemsSource = (IEnumerable)newValue;
            var itemCoun = control.GetEnumerableCount((IEnumerable)newValue);
            control._collections.HeightRequest = control._cellHeight * (itemCoun > 0 ? itemCoun : 3);

        }
        /// <summary>
        /// height of list
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public double GetEnumerableCount(IEnumerable items)
        {
            return (from object Item in items
                    select Item).Count();

        }
        private IList<object> _selectedFinalItem;
        private IList<object> _selectedItem;
        /// <summary>
        /// ItemSource property
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// List of items selected in control
        /// </summary>
        [DataMember]
        public IList<object> SelectedItems
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                _collections.SelectedItems = _selectedItem;
            }
        }
        /// <summary>
        /// Creates a customised dropdown
        /// </summary>
        /// <param name="customCellModel">cell model data to be added in item template</param>
        public CustomDropDownMultiSelectControl(CustomCellModel customCellModel)
        {

            _customSearch = new CustomSearchControl
            {
                ControlResourceKey = ResourceConstants.R_SEARCH_TEXT_KEY,
                IsUnderLine = true,
                SearchedOn = SearchedOnType.Both
            };

            _cellHeight = (double)customCellModel.IconSize + 2 * _margin + new OnIdiom<double> { Phone = _margin + 8, Tablet = 0 };
            _customSearch.Margin = new Thickness(0, -10, 0, -5);
            AutomationProperties.SetIsInAccessibleTree(_customSearch, true);

            _customSearch.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
            _addButton = new CustomButtonControl(ButtonType.PrimaryWithoutMargin) { Margin = new Thickness(0, 10), IsVisible = false };
            _addButton.Clicked += AddButton_Clicked;
            _errorLabel = new CustomLabelControl(LabelType.ClientErrorLabel)
            {
                IsVisible = false
            };
            AutomationProperties.SetIsInAccessibleTree(_errorLabel, true);
            _collections = new CollectionView
            {
                SelectionMode = SelectionMode.Multiple,
                IsVisible = false,
                ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
                ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
                VerticalOptions = LayoutOptions.FillAndExpand,
                ItemTemplate = new DataTemplate(() =>
                {
                    return new ResponsiveView(customCellModel);
                })
            };

            AutomationProperties.SetIsInAccessibleTree(_collections, true);
            _collections.SelectionChanged += Collections_SelectionChanged;
            _collections.SetBinding(CollectionView.BackgroundColorProperty, ".");
            _mainLayout = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                RowSpacing = 0,
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},

                },
                ColumnDefinitions =
                {
                   new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                   new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                },
            };
            _mainLayout.Add(_customSearch, 0, 1);
            Grid.SetColumnSpan(_customSearch, 2);
            _mainLayout.Add(_collections, 0, 2);
            Grid.SetColumnSpan(_collections, 2);
            _mainLayout.Add(_addButton, 0, 3);
            Grid.SetColumnSpan(_addButton, 2);
            _mainLayout.Add(_errorLabel, 0, 2);
            _messageView = new CustomMessageControl(false) { ControlResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY };
            _collections.EmptyView = _messageView;
            ShowHeader = true;
            Content = _mainLayout;
        }

        private void Collections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = sender as CollectionView;
            _selectedItem = item.SelectedItems;
        }

        private void AddButton_Clicked(object sender, EventArgs e)
        {
            //  _selectedItem = _selectedFinalItem;
            _customSearch.Value = string.Empty;
            HideView();
            OnAddEvent?.Invoke(sender, e);
        }

        private void HideView()
        {
            _collections.IsVisible = false;
            _addButton.IsVisible = false;
        }

        private void CustomSearch_OnSearchTextChanged(object sender, EventArgs e)
        {
            var item = sender as CustomSearchBar;
            if (item.StyleId.Equals(Constants.TEXT_CHANGED))
            {
                if (((TextChangedEventArgs)e)?.NewTextValue == string.Empty)
                {
                    HideView();
                    return;
                }
                else
                {
                    ItemInList(sender, e);
                }
            }
            else
            {
                ItemInList(sender, e);
            }
        }

        private async void ItemInList(object sender, EventArgs e)
        {
            OnSearchList?.Invoke(sender, e);
            _collections.IsVisible = true;
            _errorLabel.IsVisible = false;
            await System.Threading.Tasks.Task.Delay(1).ConfigureAwait(true);
            _addButton.IsVisible = true;
            if (GetEnumerableCount(_collections.ItemsSource) > 0)
            {
                _messageView.IsVisible = false;
            }
            else
            {
                _messageView.IsVisible = true;
                _addButton.IsVisible = false;
            }
        }

        /// <summary>
        /// To validate the control for errors
        /// </summary>
        /// <param name="isButtonClick">boolean value to check whether button was clicked or not</param>
        public override void ValidateControl(bool isButtonClick)
        {
            if (_resourceData.MinLength > 0)
            {
                if (SelectedItems == null || SelectedItems?.Count < 1)
                {
                    string errorlabel = string.Format(CultureInfo.InvariantCulture, _htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_DROPDOWN_SELECTION_VALIDATION_KEY));
                    _errorLabel.Text = errorlabel;
                    _errorLabel.IsVisible = true;
                    IsValid = false;
                }
                else
                {
                    _errorLabel.Text = string.Empty;
                    _errorLabel.IsVisible = false;
                    IsValid = true;
                }
            }
            else
            {
                IsValid = true;
            }
        }

        /// <summary>
        /// Apply resource values to the control
        /// </summary>
        protected override async void ApplyResourceValue()
        {
            _htmlPrefixText = await GetSettingsValueAsync().ConfigureAwait(true);
            _customSearch.PageResources = PageResources;
            _messageView.PageResources = PageResources;
            _addButton.Text = GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
            ApplyResoures();
        }

        private void ApplyResoures()
        {
            if (_headerLabel != null)
            {
                FormattedString fs = new FormattedString();
                fs.Spans.Add(new Span { Text = _resourceData.ResourceValue });
                if (_resourceData.IsRequired && this.IsEnabled)
                {
                    fs.Spans.Add(new Span { TextColor = (Color)Application.Current.Resources[StyleConstants.ST_ACCENT_COLOR], Text = Constants.IS_REQUIRED_FEILD_INDICATOR });
                }
                _headerLabel.FormattedText = fs;
            }
            if (ShowHeader && !string.IsNullOrWhiteSpace(_resourceData.InfoValue))
            {
                _infoIcon.IsVisible = true;
                _infoIcon.Clicked += InfoIcon_Clicked;
            }
        }

        private void InfoIcon_Clicked(object sender, EventArgs e)
        {
            DisplayInfoPopup(_resourceData.InfoValue);
        }

        /// <summary>
        /// Checks whether to enable the value
        /// </summary>
        /// <param name="value"> boolean value to check</param>
        protected override void EnabledValue(bool value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Checks whether to render the border
        /// </summary>
        /// <param name="value">boolean value to check</param>
        protected override void RenderBorder(bool value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Renders the header into the control 
        /// </summary>
        protected override void RenderHeader()
        {
            AddHeaderInView(_mainLayout);
        }
    }
}
