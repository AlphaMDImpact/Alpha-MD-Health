using System.Globalization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using SkiaSharp.Extended.UI.Controls;
using FlowDirection = Microsoft.Maui.FlowDirection;
namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Custom Message Control
    /// </summary>
    public class CustomMessageControl : BaseContentView
    {
        private readonly Grid _mainGrid;
        private readonly Grid _messageGrid;
        private SvgImageView _icon;
        private MaskedSvgImageView _maskedIcon;
        private SKLottieView _animationView;
        private CustomLabelControl _headingLabel;
        private Label _descriptionLabel;
        private CustomWebView _webView;
        private ContentView _closeButton;
        private SvgImageView _closeIcon;
        private Grid _actionLayout;
        private DXPopup _messgePopupPage;
        private bool _showInPopup;
        private bool _showHeadingOnTop;
        private bool _showIcon = true;
        private bool _showCloseButton;
        private readonly int _controlSpacing;
        private OptionModel[] _actions;
        private Border _pancakeView;
        private readonly StackLayout _contentLayout;
        private Label _htmlDesscriptionLbl;
        private Label _htmlInfoLabel;
        private int _popUpCornerRadius = 10;
        private readonly ScrollView _scrollView;

        /// <summary>
        /// Remove apply extra margin from webview
        /// </summary>
        public bool ShouldRemoveWebViewMargin { get; set; }

        /// <summary>
        /// Callback action event 
        /// </summary>
        public EventHandler<int> OnActionClicked { get; set; }

        /// <summary>
        /// PopUpCornerRadius
        /// </summary>
        public int PopUpCornerRadius
        {
            get => _popUpCornerRadius;
            set
            {
                _popUpCornerRadius = value;
                if (_pancakeView != null)
                {
                    _pancakeView.StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(value)
                    };
                }
            }
        }

        /// <summary>
        ///ApplyButtomMarginToHtmlLabel
        /// </summary>
        public bool ApplyButtomMarginToHtmlLabel
        {
            get; set;
        }

        /// <summary>
        /// ApplyButtomMarginToDescriptionLabel
        /// </summary>
        public bool ApplyButtomMarginToDescriptionLabel
        {
            get; set;
        }

        /// <summary>
        /// Decides wiew will display in popup or not
        /// </summary>
        public bool ShowInPopup
        {
            get => _showInPopup;
            set { ShowInPopupPageAsync(value); }
        }

        /// <summary>
        /// Decides wiew will display in popup or not
        /// </summary>
        public bool LoadWebViewInPopup
        {
            get; set;
        }
        private bool _webViewHeightAutoIncrease = true;

        /// <summary>
        /// Decides wiew will display in popup or not
        /// </summary>
        public bool WebViewHeightAutoIncrease
        {
            get => _webViewHeightAutoIncrease;
            set
            {
                _webViewHeightAutoIncrease = value;
                if (_webView != null && value == false)
                {
                    _webView.IsAutoIncreaseHeight = value;
                    _webView.HeightRequest = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0) * (MobileConstants.IsAndroidPlatform && MobileConstants.IsTablet ? 0.55 : 0.8);
                }
            }
        }

        private bool _buttonsWithoutSpacing;

        /// <summary>
        /// Decides wiew will display in popup or not
        /// </summary>
        public bool ButtonsWithoutSpacing
        {
            get { return _buttonsWithoutSpacing; }
            set
            {
                _buttonsWithoutSpacing = value;
                if (_buttonsWithoutSpacing)
                {
                    _mainGrid.Padding = new Thickness(0); // needed
                    _mainGrid.Margin = new Thickness(0); // needed
                }
                else
                {
                    _mainGrid.Padding = new Thickness(_controlSpacing, _controlSpacing, _controlSpacing, 15);
                }
            }
        }

        private MessageType _messageType;

        /// <summary>
        /// Decides wiew will display in popup or not
        /// </summary>
        public MessageType MessageType
        {
            get { return _messageType; }
            set
            {
                _messageType = value;
                RenderCloseButton();
                RenderMessageHeader();
                if (_messageType == MessageType.PageDetails)
                {
                    _contentLayout.VerticalOptions = LayoutOptions.Fill;
                }
            }
        }

        /// <summary>
        /// Decides icon will be display with message or not
        /// </summary>
        public bool ShowIcon
        {
            get => _showIcon;
            set
            {
                _showIcon = value;
                _icon.IsVisible = value;
            }
        }
        /// <summary>
        /// image masking allowed
        /// </summary>
        public bool IsMaskedImage
        {
            get; set;
        }

        /// <summary>
        /// Decides icon will be display with message or not
        /// </summary>
        public bool ShowInfoValueInHtmlLabel
        {
            get; set;
        }

        /// <summary>
        /// Decides heading will be display above icon or below icon
        /// </summary>
        public bool ShowHeadingOnTop
        {
            get => _showHeadingOnTop;
            set
            {
                _showHeadingOnTop = value;
                RenderHeadingOnTop();
            }
        }

        /// <summary>
        /// Decides heading will be display above icon or below icon
        /// </summary>
        public bool ShowDiscription
        {
            get; set;
        } = true;

        /// <summary>
        /// Decides close button will display on top right cornor of header or not
        /// </summary>
        public bool ShowCloseButton
        {
            get => _showCloseButton;
            set
            {
                _showCloseButton = value;
                _closeButton.IsVisible = value;

            }
        }

        /// <summary>
        /// Display action buttons based on given input
        /// </summary>
        public OptionModel[] Actions
        {
            get { return _actions; }
            set
            {
                _actions = value;
                RenderActions(value);
            }
        }

        /// <summary>
        /// Default constructor of control
        /// </summary>
        /// <param name="showInPopup">Decides wiew will display in popup or not</param>
        /// <param name="addScrollView">Decides whether whole layout should add in scroll view or not</param>
        public CustomMessageControl(bool showInPopup, bool addScrollView = true)
        {
            double height = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0) * Constants.MASTER_PAGE_DEFAULT_SIZE_RATIO;
            _mainGrid = new Grid
            {
                Style = (Style)Application.Current.Resources[showInPopup ? StyleConstants.ST_CUSTOM_POPUP_GRID_STYLE : StyleConstants.ST_END_TO_END_GRID_STYLE],

                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(height, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(height, GridUnitType.Absolute) },
                }
            };
            _messageGrid = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                RowSpacing = 0,
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }
                }
            };
            _controlSpacing = Convert.ToInt32(Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.CurrentCulture) / 2;
            _contentLayout = new StackLayout
            {
                Children = { _messageGrid },
                VerticalOptions = LayoutOptions.Center,
            };
            if (addScrollView)
            {
                _scrollView = new ScrollView
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Default,
                    Content = _contentLayout
                };

                _mainGrid.Add(_scrollView, 0, 1);
                Grid.SetColumnSpan(_scrollView, 3);
            }
            else
            {
                _mainGrid.Add(_contentLayout, 0, 1);
                Grid.SetColumnSpan(_contentLayout, 3);
            }

            CreateAllRequiredView();
            Content = _mainGrid;
        }
        private void CreateAllRequiredView()
        {

            #region show icon
            ////var iconMargin = Convert.ToInt32(Application.Current.Resources[LibStyleConstants.ST_DEFAULT_ROW_HEIGHT], CultureInfo.CurrentCulture);
            _icon = new SvgImageView(null, AppImageSize.ImageSizeXXL, AppImageSize.ImageSizeXXL, Colors.Transparent)
            {
                Margin = new Thickness(0, 30, 0, _controlSpacing),
            };
            _maskedIcon = new MaskedSvgImageView(string.Empty);
            _messageGrid.Add(_icon, 0, 1);
            Grid.SetColumnSpan(_icon, 2);
            #endregion
            _animationView = new SKLottieView
            {
                WidthRequest = (double)AppImageSize.ImageSizeXXL,
                HeightRequest = (double)AppImageSize.ImageSizeXXL,
                RepeatMode = SKLottieRepeatMode.Reverse,
                RepeatCount = 10,
            };
            #region show close button

            _closeIcon = new SvgImageView(ImageConstants.I_CLOSE_PNG, AppImageSize.ImageSizeXS, AppImageSize.ImageSizeXS, Colors.Transparent) { Margin = new Thickness(20) };
            _closeButton = new ContentView
            {
                IsVisible = false,
                HorizontalOptions = LayoutOptions.End,
                Content = _closeIcon
            };
            _messageGrid.Add(_closeButton, 1, 0);
            TapGestureRecognizer tapGesture = new TapGestureRecognizer { StyleId = 0.ToString(CultureInfo.CurrentCulture) };
            _closeButton.GestureRecognizers.Add(tapGesture);
            tapGesture.Tapped += OnActionButtonClicked;

            #endregion

            #region header 

            _headingLabel = _headingLabel ?? new CustomLabelControl(LabelType.MessageControlMediumCenter);
            _headingLabel.LineBreakMode = LineBreakMode.WordWrap;
            _messageGrid.Add(_headingLabel, 0, 2);
            Grid.SetColumnSpan(_headingLabel, 2);

            #endregion

            #region _desscriptionLbl
            CustomLabelControl desscriptionLbl = new(LabelType.SecondrySmallCenter)
            { LineBreakMode = LineBreakMode.WordWrap };
            _htmlDesscriptionLbl ??= new Label
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_HTML_PRIMARY_LABEL_STYLE],
                TextColor = (Color)Application.Current.Resources[StyleConstants.ST_SECONDARY_TEXT_COLOR],
            };
            _htmlInfoLabel ??= new Label
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_HTML_PRIMARY_LABEL_STYLE],
            };
            _descriptionLabel = desscriptionLbl;
            _messageGrid?.Add(_descriptionLabel, 0, 4);
            Grid.SetColumnSpan(_descriptionLabel, 2);
            #endregion

            #region   _webView
            _webView = new CustomWebView
            {
                IsVisible = false,
                HeightRequest = 100,
                IsAutoIncreaseHeight = WebViewHeightAutoIncrease,
                ShouldOpenLinksInBrowser = true,
                Margin = new Thickness(0),
            };
            _messageGrid.Add(_webView, 0, 5);
            Grid.SetColumnSpan(_webView, 2);
            #endregion

            #region show popup code
            _pancakeView = _pancakeView ?? new Border
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_STYLE],
                BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR],
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(PopUpCornerRadius)
                },
                Content = null,
            };

            _messgePopupPage ??= new DXPopup
            {
                IsOpen = false,
                CloseOnScrimTap = false,
                AllowScrim = true,
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Padding = new Thickness(0),// test
                    Margin = new Thickness(0),// test
                    Children = {
                       _pancakeView
                     }
                }
            };
            #endregion

        }

        /// <summary>
        /// Pop Only message control instance from navigation
        /// </summary>
        public void PopCustomMessageControlAsync()
        {
            _messgePopupPage.IsOpen = false;
            _showInPopup = false;
        }

        private void ShowInPopupPageAsync(bool newValue)
        {
            if (_showInPopup != newValue || _messgePopupPage == null)
            {
                _showInPopup = newValue;
                if (_showInPopup)
                {
                    _pancakeView.Content ??= _mainGrid;
                    _messgePopupPage.IsOpen = true;
                    //_nameOfPage = await MauiPopup.PopupAction.DisplayPopup(_messgePopupPage).ConfigureAwait(true);
                }
                else
                {
                    if (_webView != null)
                    {
                        _webView.Source = Constants.BLANK_SOURCE;
                        _webView = null;
                    }
                    _messgePopupPage.IsOpen = false;
                    //await MauiPopup.PopupAction.ClosePopup(_nameOfPage);
                }
            }
        }

        private void OnActionButtonClicked(object sender, EventArgs e)
        {
            if (OnActionClicked != null && sender != null)
            {
                OnActionClicked.Invoke(this, sender is CustomButton ? Convert.ToInt32((sender as CustomButton).StyleId, CultureInfo.CurrentCulture) : 0);
            }
        }

        private void RenderMessageIcon()
        {
            if (_showIcon && !string.IsNullOrWhiteSpace(_resourceData?.KeyDescription))
            {
                if (_icon == null || (ImageSource)_icon.Source == null || ((ImageSource)_icon.Source).ToString() != _resourceData.KeyDescription)
                {

                    if (_resourceData.KeyDescription.Length > 100)
                    {
                        _icon.Source = ImageSource.FromStream(() => GenericMethods.GetMemoryStreamFromBase64(_resourceData.KeyDescription));
                    }
                    else if (_resourceData.KeyDescription.Length < 100 && IsFileExistInImage(_resourceData.KeyDescription))
                    {
                        if (IsMaskedImage)
                        {
                            ClearSvgICon();
                            _maskedIcon.ChangeImage(_resourceData.KeyDescription);
                            _messageGrid.Add(_maskedIcon, 0, 1);
                            Grid.SetColumnSpan(_maskedIcon, 2);
                        }
                        else
                        {
                            if (IsMaskedImage)
                            {
                                _icon.HeightRequest = -1;
                            }
                            _icon.Source = ImageSource.FromFile(_resourceData.KeyDescription);
                        }
                    }
                    else
                    {
                        ClearSvgICon();
                        _animationView.Source = (SKLottieImageSource)SKLottieImageSource.FromFile(_resourceData.KeyDescription);
                        _messageGrid.Add(_animationView, 0, 1);
                        Grid.SetColumnSpan(_animationView, 2);
                    }
                }
                _icon.IsVisible = true;
            }
            else
            {
                _icon.IsVisible = false;
            }
        }

        private bool IsFileExistInImage(string name)
        {
            var assembly = Application.Current.GetType().Assembly;
            var generatedFilename = AppStyles.NameSpaceImage + name;
            bool found = false;
            foreach (var res in assembly.GetManifestResourceNames())
            {
                if (res.Contains(generatedFilename))
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        private void RenderCloseButton()
        {
            ClearCloseButton();
            if (_showCloseButton && _messageType == MessageType.PageDetails && _showHeadingOnTop)
            {
                _closeButton.HorizontalOptions = LayoutOptions.Start;
                _closeIcon.Source = ImageSource.FromFile(ImageConstants.I_BACK_PNG);
                _closeIcon.RotationY = AppStyles.DefaultFlowDirection == FlowDirection.RightToLeft ? 180 : 0;
                if (_mainGrid.Children.Contains(_closeButton))
                {
                    _mainGrid.Children.Remove(_closeButton);
                }
                _mainGrid.Add(_closeButton, 0, 0);
            }
            _mainGrid.Add(_closeButton, _messageType == MessageType.PageDetails && _showHeadingOnTop ? 2 : 1, 0);
        }

        private void ClearSvgICon()
        {
            if (_icon != null)
            {
                if (_mainGrid.Children.Contains(_icon))
                {
                    _mainGrid.Children.Remove(_icon);
                }
                if (_messageGrid.Children.Contains(_icon))
                {
                    _messageGrid.Children.Remove(_icon);
                }
            }
        }
        private void ClearCloseButton()
        {
            if (_closeButton != null)
            {
                if (_mainGrid.Children.Contains(_closeButton))
                {
                    _mainGrid.Children.Remove(_closeButton);
                }
                if (_messageGrid.Children.Contains(_closeButton))
                {
                    _messageGrid.Children.Remove(_closeButton);
                }
            }
        }

        private void RenderMessageHeader()
        {
            if (string.IsNullOrWhiteSpace(_resourceData?.ResourceValue))
            {
                _headingLabel.IsVisible = false;
            }
            else
            {
                _headingLabel.IsVisible = true;
                if (ShowInPopup || _showHeadingOnTop || _messageType == MessageType.ConfirmationPopup)
                {
                    _headingLabel.Style = (Style)Application.Current.Resources[StyleConstants.ST_NO_MASTER_HEADER_STYLE_KEY];
                }
                _headingLabel.Margin = new Thickness(0, _showCloseButton && _showInPopup ? -30 : 30, 0, _controlSpacing);
                _headingLabel.Text = _resourceData.ResourceValue;
                _headingLabel.LineBreakMode = LineBreakMode.WordWrap;

            }
        }

        private void RenderHeadingOnTop()
        {
            ClearHeadingLabel();
            if (_messageType == MessageType.PageDetails && _showCloseButton)
            {
                _headingLabel.Margin = new Thickness(0, _controlSpacing, 0, (_showIcon && !string.IsNullOrWhiteSpace(_resourceData?.KeyDescription)) ? _controlSpacing : _controlSpacing * 4);
                _mainGrid.Padding = new Thickness(0);
                _mainGrid.Margin = new Thickness(0);
                _headingLabel.LineBreakMode = LineBreakMode.TailTruncation;
                _mainGrid.Add(_headingLabel, 1, 0);
                _closeIcon.Margin = 0;
                _closeButton.Margin = _headingLabel.Margin;
            }
            else
            {
                _messageGrid.Add(_headingLabel, 0, 0);
                _headingLabel.LineBreakMode = LineBreakMode.WordWrap;
            }
            if (!_showCloseButton)
            {
                Grid.SetColumnSpan(_headingLabel, 2);
            }
        }

        private void ClearHeadingLabel()
        {
            if (_headingLabel != null)
            {
                if (_messageGrid.Children.Contains(_headingLabel))
                {
                    _messageGrid.Children.Remove(_headingLabel);
                }
                if (_mainGrid.Children.Contains(_headingLabel))
                {
                    _mainGrid.Children.Remove(_headingLabel);
                }
            }
        }

        private void RenderDescription()
        {
            if (!string.IsNullOrWhiteSpace(_resourceData?.PlaceHolderValue) && ShowDiscription)
            {
                _descriptionLabel.IsVisible = true;
                if (ShowInPopup)
                {
                    _descriptionLabel.Text = _resourceData.PlaceHolderValue.Replace(Constants.NEW_LINE_CONSTANT, Environment.NewLine);
                }
                else
                {
                    if (_messageGrid.Children.Contains(_descriptionLabel) && _descriptionLabel is CustomLabelControl)
                    {
                        _messageGrid.Children.Remove(_descriptionLabel);
                        _descriptionLabel = _htmlDesscriptionLbl;
                        _messageGrid?.Add(_descriptionLabel, 0, 4);
                    }
                    _descriptionLabel.Text = _resourceData.PlaceHolderValue;
                }
                if (ApplyButtomMarginToDescriptionLabel)
                {
                    _descriptionLabel.Margin = new Thickness(0, 0, 0, 30);
                }
                if (_messageType == MessageType.ConfirmationPopup)
                {
                    _descriptionLabel.Margin = new Thickness(20, 0);
                }
            }
            else
            {
                _descriptionLabel.IsVisible = false;
            }
        }

        private void RenderHtmlMessage(bool showBusyIndicator)
        {
            if (string.IsNullOrWhiteSpace(_resourceData?.InfoValue))
            {
                if (_webView != null)
                {
                    _webView.IsVisible = false;
                }
            }
            else if (ShowInfoValueInHtmlLabel)
            {
                ShowInfoValueInHtml();
            }
            else
            {
                _webView.ShowBusyIndicator = showBusyIndicator;
                _webView.IsVisible = true;
                if (_messageType == MessageType.ConfirmationPopup && !ShouldRemoveWebViewMargin)
                {
                    _webView.HeightRequest = 1;
                    _webView.Margin = new Thickness(20, 0);
                }
                if (_resourceData.IsWebLink)
                {
                    ////todo: to confirm about below height in , and if possible remove it
                    WebLinkRender();
                }
                else
                {
                    if (LoadWebViewInPopup)
                    {
                        RemoveScrollView();
                    }

                    var rowSpanCount = _messageGrid.Children.ToList().Where(x => !(x as View).IsVisible).ToList().Count;
                    Grid.SetRowSpan(_webView, rowSpanCount);
                    _webView.Source = new HtmlWebViewSource { Html = _webView.AddFontStyleToHtmlString(ShouldRemoveWebViewMargin ? Constants.HTML_CENTER_TAG + _resourceData.InfoValue : _resourceData.InfoValue) };

                }
            }
        }

        private void ShowInfoValueInHtml()
        {
            if (_webView != null && _messageGrid.Children.Contains(_webView))
            {
                _messageGrid.Children.Remove(_webView);
            }
            if (ApplyButtomMarginToHtmlLabel)
            {
                _htmlInfoLabel.Margin = new Thickness(0, 0, 0, 30);
            }
            _messageGrid.Add(_htmlInfoLabel, 0, 5);
            Grid.SetColumnSpan(_htmlInfoLabel, 2);
            _htmlInfoLabel.Text = _resourceData.InfoValue;
        }

        private void WebLinkRender()
        {
            _webView.HeightRequest = 300;

            if (MobileConstants.IsAndroidPlatform && _resourceData.InfoValue.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase))
            {
                _webView.IsAutoIncreaseHeight = true;

                var rowSpanCount = _messageGrid.Children.ToList().Where(x => !(x as View).IsVisible).ToList().Count;
                Grid.SetRowSpan(_webView, rowSpanCount);
                _webView.HeightRequest = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0);
                _webView.PDFLinkName = _resourceData.InfoValue;
            }
            else
            {
                _webView.Margin = new Thickness(10, 0);
                if (_resourceData.InfoValue.Contains(Constants.CONSTANT_YOUTUBE))
                {
                    _webView.Source = new HtmlWebViewSource
                    {
                        Html = string.Format(CultureInfo.InvariantCulture, Constants.CONSTANT_YOUTUBE_VIDEO_FRAME, _resourceData.InfoValue)
                    };
                }
                else
                {
                    _webView.Source = _resourceData.InfoValue;
                    if (_resourceData.InfoValue.EndsWith(Constants.CONSTANT_PDF, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _webView.IsAutoIncreaseHeight = true;
                        // _webView.HeightRequest = App._essentials.GetPreferenceValue(LibStorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0);
                    }
                    else
                    {
                        _webView.HeightRequest = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0) * 0.8;
                        RemoveScrollView();
                    }
                }
            }
        }

        private void RemoveScrollView()
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS && _scrollView != null && _mainGrid.Children.Contains(_scrollView))
            {
                _mainGrid.Children.Remove(_scrollView);
                _mainGrid.Add(_contentLayout, 0, 1);
                Grid.SetColumnSpan(_contentLayout, 3);
            }
        }

        private void RenderActions(OptionModel[] actions)
        {
            _actionLayout?.Children?.Clear();
            if (actions?.Length > 0)
            {
                _actionLayout = _actionLayout ?? new Grid
                {
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                    VerticalOptions = LayoutOptions.EndAndExpand,
                };
                _actionLayout.ColumnDefinitions = new ColumnDefinitionCollection();
                AddActionButtons(actions);
                if (_messageType == MessageType.ConfirmationPopup || _messageType == MessageType.PageDetails)
                {
                    _actionLayout.Margin = new Thickness(0, 30, 0, 0);
                }
                _mainGrid.Add(_actionLayout, 0, 2);
                Grid.SetColumnSpan(_actionLayout, 3);
            }
        }

        private void AddActionButtons(OptionModel[] actions)
        {
            for (int index = 0; index < actions.Length; index++)
            {
                _actionLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                CustomButtonControl actionButton = new(!string.IsNullOrWhiteSpace(actions[index].GroupName) ? actions[index].GroupName.ToEnum<ButtonType>() : ButtonType.TransparentWithoutMargin)
                {
                    StyleId = (index + 1).ToString(CultureInfo.CurrentCulture),
                    Margin = new Thickness(0, (_buttonsWithoutSpacing ? 0 : _controlSpacing * 2), 0, 0),
                    Text = string.IsNullOrWhiteSpace(GetResourceValueByKey(actions[index].OptionText)) ? Constants.NEXT_TEXT : GetResourceValueByKey(actions[index].OptionText)
                    ////Note : For No internet page when resources are not loaded set button text as "Next" ;
                };
                AutomationProperties.SetIsInAccessibleTree(actionButton, true);
                actionButton.Clicked += OnActionButtonClicked;
                _actionLayout.Add(actionButton, index, 0);
            }
        }

        /// <summary>
        /// Method for  Apply resource value
        /// </summary>
        protected override void ApplyResourceValue()
        {
            RenderMessageIcon();
            RenderMessageHeader();
            RenderDescription();
            RenderHtmlMessage(true);
        }

        /// <summary>
        /// reset Layout 
        /// </summary>
        /// <param name="_isLandscape">_isLandscape</param>
        public void ResetLayoutForMessagePopUp(bool _isLandscape)
        {
            //if (AppStyles.IsTabletScaledView && MobileConstants.IsTablet && _showInPopup)
            //{
            //    if (AppStyles.IsTabletScaledView)
            //    {
            //var padding = new OnIdiom<double>
            //{
            //    Phone = Convert.ToDouble(Application.Current.Resources[LibStyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture),
            //    Tablet = _isLandscape ? App._essentials.GetPreferenceValue(LibStorageConstants.PR_SCREEN_WIDTH_KEY, 0.0) * 0.3 : App._essentials.GetPreferenceValue(LibStorageConstants.PR_SCREEN_WIDTH_KEY, 0.0) * LibGenericMethods.GetPlatformSpecificValue(0.18, 0.15, 0)
            //};
            //_messgePopupPage.Padding = new OnIdiom<Thickness>
            //{
            //    Phone = Convert.ToInt32(Application.Current.Resources[LibStyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture),
            //    Tablet = new Thickness(padding, Convert.ToInt32(Application.Current.Resources[LibStyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture))
            //};
            //}
            //else
            //{
            //    _messgePopupPage.Padding = new OnIdiom<Thickness>
            //    {
            //        Phone = Convert.ToInt32(Application.Current.Resources[LibStyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture),
            //        Tablet = new Thickness(0.3 * App._essentials.GetPreferenceValue(LibStorageConstants.PR_SCREEN_WIDTH_KEY, 0.0), Convert.ToInt32(Application.Current.Resources[LibStyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture))
            //    };
            //}
            //}
        }
        /// <summary>
        /// reset Webview Height
        /// </summary>
        public void ResetWebviewHeightOnOrientation()
        {
            RenderHtmlMessage(false);
        }
        /// <summary>
        /// Method for Render header
        /// </summary>
        protected override void RenderHeader()
        {
        }

        /// <summary>
        /// Method for RenderBorder
        /// </summary>
        /// <param name="value">Border thikness</param>
        protected override void RenderBorder(bool value)
        {
        }

        /// <summary>
        /// Method for EnabledValue
        /// </summary>
        /// <param name="value">Enable or not</param>
        protected override void EnabledValue(bool value)
        {
        }

        /// <summary>
        /// Method for Validate control
        /// </summary>
        /// <param name="isButtonClick">true/false</param>
        public override void ValidateControl(bool isButtonClick)
        {
        }
    }
}