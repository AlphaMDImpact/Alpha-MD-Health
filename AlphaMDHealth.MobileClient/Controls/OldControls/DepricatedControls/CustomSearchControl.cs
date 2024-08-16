using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represemnts Custom Search Control
    /// </summary>
    public class CustomSearchControl : BaseContentView
    {
        private readonly CustomSearchBar _searchBar;
        private readonly ContentView _searchView;
        private readonly Grid _mainLayout;

        private readonly double _margin = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.CurrentCulture);

        /// <summary>
        /// call back event 
        /// </summary>
        public event EventHandler<EventArgs> OnSearchTextChanged;

        /// <summary>
        /// call back event 
        /// </summary>
        public event EventHandler<EventArgs> OnSearchFocused;

        /// <summary>
        /// search  text on text changed or on search button clicked 
        /// </summary>
        public SearchedOnType SearchedOn
        {
            get => SearchedOnType.Both;
            set
            {
                if (_searchBar != null)
                {
                    switch (value)
                    {
                        case SearchedOnType.OnTextChange:
                            _searchBar.TextChanged += SearchBar_TextChanged;
                            break;
                        case SearchedOnType.OnSearchClick:
                            _searchBar.SearchButtonPressed += SearchBar_SearchButtonPressed;
                            break;
                        default:
                            _searchBar.SearchButtonPressed += SearchBar_SearchButtonPressed;
                            _searchBar.TextChanged += SearchBar_TextChanged;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// IsBackGroundTransparent
        /// </summary>
        public bool IsBackGroundTransparent
        {
            get
            {
                return _searchView.BackgroundColor == default; //todo: Color.Transparent;
            }
            set
            {
                _searchBar.Margin = new Thickness(0, 0, 0, _margin);
                _searchView.Margin = new Thickness(0);
                if (value)
                {
                    _searchView.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR];
                    _searchBar.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR];
                    _mainLayout.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR];
                }
                else
                {
                    _searchView.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                    _searchBar.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                    _mainLayout.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                }
            }
        }

        /// <summary>
        /// SearchOutSideViewHide
        /// </summary>
        public bool SearchOutSideViewHide
        {
            get { return _searchView.BackgroundColor == default; } //todo:  Color.Transparent; }
            set
            {
                if (value)
                {
                    _searchBar.Margin = new Thickness(0);
                    _searchView.BackgroundColor = default; //todo: Color.Transparent;
                }
                else
                {
                    _searchView.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                }
            }
        }
        /// <summary>
        /// Set search text value
        /// </summary>
        public string Value
        {
            get { return _searchBar.Text; }
            set
            {
                if (_searchBar != null)
                {
                    _searchBar.Text = value;
                }
            }
        }
        /// <summary>
        /// IsAppliedMargin
        /// </summary>
        public bool IsAppliedMargin
        {
            get { return _searchView.Margin == default; }
            set
            {
                if (value)
                {
                    _searchView.Margin = new Thickness(0);
                }
            }
        }
        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var item = sender as CustomSearchBar;
            item.StyleId = Constants.TEXT_CHANGED;
            if (!string.IsNullOrWhiteSpace(_searchBar?.Text) && _searchBar?.Text.Length >= _resourceData?.MinLength)
            {
                OnSearchTextChanged?.Invoke(sender, e);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_searchBar?.Text))
                {
                    OnSearchTextChanged?.Invoke(sender, e);
                }
            }
        }

        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            var item = sender as CustomSearchBar;
            item.StyleId = Constants.SEARCH_BUTTON_CLICKED;
            OnSearchTextChanged?.Invoke(sender, e);
        }


        /// <summary>
        ///  Custom Search Control class constructor
        /// </summary>
        public CustomSearchControl()
        {
            _mainLayout = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                },
                ColumnDefinitions =
                {
                   new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                },
            };
            _searchBar = new CustomSearchBar
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_SEARCHBAR_KEY],
                Margin = new Thickness(_margin),
            };
            //todo:
            //_searchBar.Effects.Add(new RoundCornersEffect());
            _searchBar.Focused += OnFocused;
            AutomationProperties.SetIsInAccessibleTree(_searchBar, true);
            _searchView = new ContentView { Margin = new Thickness(-_margin, 0), Content = _searchBar };
            ShowHeader = false;
            _mainLayout.Add(_searchView, 0, 1);
            Grid.SetColumnSpan(_searchView, 2);
            Content = _mainLayout;
        }

        private void OnFocused(object sender, FocusEventArgs e)
        {
            OnSearchFocused?.Invoke(sender, e);
        }

        /// <summary>
        /// method to Apply Resource Value
        /// </summary>
        protected override void ApplyResourceValue()
        {
            _searchBar.Placeholder = _resourceData.ResourceValue;
            AutomationProperties.SetIsInAccessibleTree(_searchBar, true);
            if (string.IsNullOrWhiteSpace(_searchBar.AutomationId))
            {
                _searchBar.AutomationId = $"{ShellMasterPage.CurrentShell?.CurrentPage?.GetType().Name}{Constants.AUTOMATION_ID_SEPRATOR}{_resourceData.ResourceValue}";
                AutomationProperties.SetName(_searchBar, _searchBar.AutomationId);
            }
            if (_resourceData.MaxLength > 0)
            {
                _searchBar.MaxLength = (int)_resourceData.MaxLength;
            }
            ApplyResoures();
        }

        private void ApplyResoures()
        {
            if (_headerLabel != null)
            {
                FormattedString fs = new FormattedString();
                fs.Spans.Add(new Span { Text = _resourceData.PlaceHolderValue });
                if (_resourceData.IsRequired && this.IsEnabled)
                {
                    fs.Spans.Add(new Span { TextColor = (Color)Application.Current.Resources[StyleConstants.ST_ACCENT_COLOR], Text = Constants.IS_REQUIRED_FEILD_INDICATOR });
                }
                //// _headerLabel.Margin = new Thickness(_margin, 0);
                _headerLabel.FormattedText = fs;

            }
        }
        /// <summary>
        /// Method to fucus in searchbar
        /// </summary>
        public void UnFocusSearchBar()
        {
            _searchBar.Unfocus();
        }

        /// <summary>
        /// Method to render header
        /// </summary>
        protected override void RenderHeader()
        {
            AddHeaderInView(_mainLayout);
        }

        /// <summary>
        /// Method to set rendere value
        /// </summary>
        /// <param name="value">border thikness</param>
        protected override void RenderBorder(bool value)
        {
            _searchBar.BorderType = value ? _LINE : string.Empty;
            BackgroundColor = value ?
                (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR]
                : (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR];
        }

        /// <summary>
        /// Method to Enable value
        /// </summary>
        /// <param name="value"></param>
        protected override void EnabledValue(bool value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// validate Entry Control 
        /// </summary>
        /// <param name="isButtonClick">validation fired after button is clicked</param>
        public override void ValidateControl(bool isButtonClick)
        {
        }
    }
}