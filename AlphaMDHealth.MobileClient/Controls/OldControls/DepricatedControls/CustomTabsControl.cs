using System.Globalization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Tabs control
    /// </summary>
    public class CustomTabsControl : Border
    {
        private readonly Grid _buttonGrid;
        private List<OptionModel> _moreOptions;
        private readonly double _screenHeight = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, (double)0);
        private readonly double _padding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
        private bool _isTabClicked;
        //todo
        //private TabMoreOptionPopupPage _moreOptionPopup;

        /// <summary>
        /// Tab click event
        /// </summary>
        public event EventHandler<EventArgs> TabClicked;

        /// <summary>
        /// Defult constructor
        /// </summary>
        public CustomTabsControl()
        {
            _buttonGrid = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                VerticalOptions = LayoutOptions.Start,
                ColumnSpacing = 2,
                BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR],
                RowDefinitions = { new RowDefinition { Height = GridLength.Auto } },
            };
            Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_TAB_STYLE];
            Content = _buttonGrid;
        }

        /// <summary>
        /// Loads data in tab view
        /// </summary>
        /// <param name="itemSource">data source used to display tabs</param>
        /// <param name="isTabFullWidth">Flag which decides need to float in full width or not</param>
        public void LoadUIData(List<OptionModel> itemSource, bool isTabFullWidth)
        {
            _moreOptions = new List<OptionModel>();
            _buttonGrid.ColumnDefinitions.Clear();
            _buttonGrid.Children.Clear();
            for (int i = 0; i < itemSource.Count; i++)
            {
                if (itemSource[i] != null)
                {
                    if (i < Constants.MAX_TABS_ALLOWED)
                    {
                        GetTabData(itemSource[i], isTabFullWidth, i, false);
                    }
                    else
                    {
                        _moreOptions.Add(itemSource[i]);
                    }
                }
            }
            if (itemSource.Count > Constants.MAX_TABS_ALLOWED)
            {
                GetTabData(null, isTabFullWidth, Constants.MAX_TABS_ALLOWED, true);
            }
        }

        /// <summary>
        /// Refresh tabs
        /// </summary>
        public bool InvokeTabRefresh(string selectedTab)
        {
            if (_buttonGrid.Children.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(selectedTab))
                {
                    Tab_Clicked(_buttonGrid.Children.First(), new EventArgs());
                    return true;
                }
                else
                {
                    //todo
                    //View tab = _buttonGrid.Children.FirstOrDefault(x => x is CustomButtonControl && (x as CustomButtonControl).CommandParameter.ToString() == selectedTab);
                    //if (tab == null)
                    //{
                    //    var moreOption = _moreOptions.FirstOrDefault(x => x.GroupName == selectedTab);
                    //    if (moreOption != null)
                    //    {
                    //        //tab = _buttonGrid.Children.FirstOrDefault(x => x is CustomButtonControl && (x as CustomButtonControl).CommandParameter.ToString() == LibConstants.TABS_MORE_OPTION_CONSTANT);
                    //        if (tab != null)
                    //        {
                    //            Tab_Clicked(tab, new EventArgs());
                    //        }
                    //        if (_moreOptionPopup != null)
                    //        {
                    //            _moreOptionPopup.OnTabSelectionChanged(moreOption, new EventArgs());
                    //        }
                    //        return true;
                    //    }
                    //}
                    //else
                    //{
                    //    Tab_Clicked(tab, new EventArgs());
                    //    return true;
                    //}
                }
            }
            return false;
        }

        private void GetTabData(OptionModel tab, bool isTabFullWidth, int i, bool isMore)
        {
            _buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = isTabFullWidth && !isMore ? GridLength.Star : GridLength.Auto });
            CustomButtonControl tabButton;
            if (i != Constants.MAX_TABS_ALLOWED)
            {
                tabButton = new CustomButtonControl(ButtonType.TabButton)
                {
                    HeightRequest = 35,
                    HorizontalContentAlignment = TextAlignment.Center,
                    Padding = new Thickness(_padding, 0),
                    BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR],
                    TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR],
                    CommandParameter = tab.GroupName,
                    Text = tab.OptionText.Trim(),
                };
                if (i == 0)
                {
                    tabButton.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
                    tabButton.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                }
            }
            else
            {
                tabButton = new CustomButtonControl(ButtonType.TabButton)
                {
                    HeightRequest = 35,
                    BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR],
                    CommandParameter = Constants.TABS_MORE_OPTION_CONSTANT,
                    //todo: ImageSource = SvgImageSource.FromSvgResource(LibImageConstants.I_MORE_SVG_IMAGE, AppStyles.GetSvgSize(AppImageSize.ImageSizeS), AppStyles.GetSvgSize(AppImageSize.ImageSizeS), (Color)Application.Current.Resources[LibStyleConstants.ST_PRIMARY_APP_COLOR])
                };
            }
            tabButton.Clicked += Tab_Clicked;
            _buttonGrid.Add(tabButton, i, 0);
        }

        private async void Tab_Clicked(object sender, EventArgs e)
        {
            if (!_isTabClicked)
            {
                _isTabClicked = true;
                CustomButtonControl selectedTab = (CustomButtonControl)sender;
                selectedTab.IsEnabled = false;
                foreach (var tab in _buttonGrid.Children)
                {
                    if (selectedTab.CommandParameter == ((CustomButtonControl)tab).CommandParameter)
                    {
                        ((CustomButtonControl)tab).BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
                        ((CustomButtonControl)tab).TextColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                        if ((string)((CustomButtonControl)tab).CommandParameter == Constants.TABS_MORE_OPTION_CONSTANT)
                        {
                            //todo
                            //((CustomButtonControl)tab).ImageSource = SvgImageSource.FromSvgResource(LibImageConstants.I_MORE_SVG_IMAGE, AppStyles.GetSvgSize(AppImageSize.ImageSizeS), AppStyles.GetSvgSize(AppImageSize.ImageSizeS), (Color)Application.Current.Resources[LibStyleConstants.ST_CONTROL_BACKGROUND_COLOR]);
                            //_moreOptionPopup = new TabMoreOptionPopupPage(_moreOptions);
                            //_moreOptionPopup.Padding = new Thickness(0, this.Y + (tab.HeightRequest * 2) + (_padding * (MobileConstants.IsIosPlatform ? 2 : 3)), _padding * 3, 0.13 * _screenHeight);
                            //_moreOptionPopup.OnTabChanged += OnTabClicked;
                            //TabClicked?.Invoke(sender, e);
                            //await Navigation.PushPopupAsync(_moreOptionPopup).ConfigureAwait(true);
                        }
                        else
                        {
                            TabClicked?.Invoke(selectedTab.CommandParameter, new EventArgs());
                            _moreOptions.ForEach(x => x.IsSelected = false);
                        }
                    }
                    else
                    {
                        //todo
                        //((CustomButtonControl)tab).BackgroundColor = (Color)Application.Current.Resources[LibStyleConstants.ST_CONTROL_BACKGROUND_COLOR];
                        //((CustomButtonControl)tab).TextColor = (Color)Application.Current.Resources[LibStyleConstants.ST_PRIMARY_APP_COLOR];
                        //if ((string)((CustomButtonControl)tab).CommandParameter == LibConstants.TABS_MORE_OPTION_CONSTANT)
                        //{
                        //    ((CustomButtonControl)tab).ImageSource = SvgImageSource.FromSvgResource(LibImageConstants.I_MORE_SVG_IMAGE, AppStyles.GetSvgSize(AppImageSize.ImageSizeS), AppStyles.GetSvgSize(AppImageSize.ImageSizeS), (Color)Application.Current.Resources[LibStyleConstants.ST_PRIMARY_APP_COLOR]);
                        //}
                    }
                }
                selectedTab.IsEnabled = true;
                _isTabClicked = false;
            }
        }

        private void OnTabClicked(object sender, EventArgs e)
        {
            OptionModel selectedTabData = (OptionModel)sender;
            if (selectedTabData != null)
            {
                _moreOptions.ForEach(x => x.IsSelected = false);
                foreach (var tab in _buttonGrid.Children)
                {
                    if ((string)((CustomButtonControl)tab).CommandParameter != Constants.TABS_MORE_OPTION_CONSTANT)
                    {
                        ((CustomButtonControl)tab).BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                        ((CustomButtonControl)tab).TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
                    }
                }
                _moreOptions.FirstOrDefault(x => x.GroupName == selectedTabData.GroupName).IsSelected = true;
                TabClicked?.Invoke(selectedTabData.GroupName, e);
            }
        }
    }
}