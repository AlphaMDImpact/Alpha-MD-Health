using System.Globalization;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomInfoControl : Border
    {
        private readonly CustomLabelControl _instructionLabel;
        private readonly CustomLabelControl _readMoreLabel;
        private readonly Label _htmlLabel;
        private readonly Grid _mainLayout;
        private bool _isShowingLess = true;
        private string _readMoreString;
        private string _readLessString;
        private string _infoText;
        private int _nomberOfLine;
        private readonly ScrollView _scroll;

        /// <summary>
        /// IsHtmlLabel
        /// </summary>
        public bool IsHtmlLabel
        {
            get; set;
        }

        /// <summary>
        /// CustomInfoControl
        /// </summary>
        /// <param name="isHtmlLabel">isHtmlLabel</param>
        public CustomInfoControl(bool isHtmlLabel) : this(isHtmlLabel, ImageConstants.I_INFO_ICON_PNG) { }

        /// <summary>
        /// CustomInfoControl
        /// </summary>
        /// <param name="isHtmlLabel">isHtmlLabel</param>
        /// <param name="iconName">iconName</param>
        public CustomInfoControl(bool isHtmlLabel, string iconName)
        {
            IsHtmlLabel = isHtmlLabel;
            var padding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
            _instructionLabel = new CustomLabelControl(LabelType.PrimarySmallLeft)
            {
                LineBreakMode = LineBreakMode.WordWrap,
            };
            _htmlLabel = new Label
            {
                HorizontalTextAlignment = AppStyles.DefaultFlowDirection == FlowDirection.RightToLeft ? TextAlignment.End : TextAlignment.Start,
                TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_TEXT_COLOR],
                Padding = new Thickness(0, 0, 0, padding),
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) * 1.2,
            };
            _readMoreLabel = new CustomLabelControl(LabelType.PrimarySmallLeft)
            {
                FontAttributes = FontAttributes.Bold,
            };
            Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_STYLE];
            Stroke = (Color)Application.Current.Resources[StyleConstants.ST_ACCENT_COLOR];
            BackgroundColor = Color.FromArgb(StyleConstants.PERCENT20_ALPHA_COLOR + ((Color)Application.Current.Resources[StyleConstants.ST_ACCENT_COLOR]).ToHex().Replace(Constants.REMOVE_ALPHA_PREFIX, " ").Trim());
            Margin = new Thickness(0, 0, 0, padding);
            Padding = padding;
            VerticalOptions = LayoutOptions.End;

            _mainLayout = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                ColumnSpacing = 10,
                RowSpacing = 5,
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition{ Height = GridLength.Star },
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition{ Width = GridLength.Auto },
                    new ColumnDefinition{ Width = GridLength.Auto }
                },
                Children =
                {
                    {
                        new SvgImageView(iconName, (double)AppImageSize.ImageSizeS, (double)AppImageSize.ImageSizeS)
                        {
                            VerticalOptions = LayoutOptions.Start
                        }
                    }
                }
            };

            if (IsHtmlLabel)
            {
                _mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                _scroll = new ScrollView { Content = _instructionLabel };
                _mainLayout.Add(_scroll, 1, 0);
            }
            else
            {
                _mainLayout.Add(_instructionLabel, 1, 0);
            }
            Content = new ScrollView
            {
                Content = _mainLayout
            };
        }

        /// <summary>
        /// Sets Color for instruction Label
        /// </summary>
        /// <param name="color">Layout Option</param>
        public void SetLabelTextColor(Color color)
        {
            _instructionLabel.TextColor = color;
        }

        /// <summary>
        /// Sets Vertical Options for icon
        /// </summary>
        /// <param name="layoutOption">Layout Option</param>
        public void SetIconVerticalOption(LayoutOptions layoutOption)
        {
            //todo
            //_mainLayout.Children[0].VerticalOptions = layoutOption;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="infoText"></param>
        public void SetInfoValue(string infoText)
        {
            _instructionLabel.Text = _infoText = infoText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="infoText"></param>
        /// <param name="readMore"></param>
        /// <param name="readLess"></param>
        /// <param name="numberOfLine"></param>
        public void SetInfoValue(string infoText, string readMore, string readLess, int numberOfLine)
        {
            _nomberOfLine = numberOfLine;
            _infoText = infoText;
            if (_scroll != null)
            {
                _scroll.Content = _htmlLabel;
            }
            if (infoText.Length > numberOfLine)
            {
                _readMoreLabel.Text = _readMoreString = readMore;
                _readLessString = readLess;
                _mainLayout.Add(_readMoreLabel, 1, 1);
                _isShowingLess = true;
                _scroll.HeightRequest = -1;
                _htmlLabel.Text = _infoText;
                _instructionLabel.IsHtmlLabelLineCount = true;
                _instructionLabel.Text = _htmlLabel.Text;
                _htmlLabel.Text = _htmlLabel.Text.Substring(0, _nomberOfLine);
                if (_readMoreLabel.GestureRecognizers == null || _readMoreLabel.GestureRecognizers.Count == 0)
                {
                    TapGestureRecognizer tapGesture = new();
                    tapGesture.Tapped += TapGesture_Tapped;
                    _readMoreLabel.GestureRecognizers.Add(tapGesture);
                }
            }
            else
            {
                _htmlLabel.Text = infoText;
                _instructionLabel.LineBreakMode = LineBreakMode.WordWrap;
                _mainLayout.Children.Remove(_readMoreLabel);
            }
        }

        private void TapGesture_Tapped(object sender, EventArgs e)
        {
            if (_isShowingLess)
            {
                _htmlLabel.Text = _infoText;
                _instructionLabel.Text = _htmlLabel.Text;
                if (_instructionLabel.GetLineCount > 6)
                {
                    _scroll.HeightRequest = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0) * 0.2;
                }
                else
                {
                    _scroll.HeightRequest = -1;
                }
                _readMoreLabel.Text = _readLessString;
                _isShowingLess = false;
            }
            else
            {
                _htmlLabel.Text = _infoText;
                _htmlLabel.Text = _htmlLabel.Text.Substring(0, _nomberOfLine);
                _scroll.HeightRequest = -1;
                _readMoreLabel.Text = _readMoreString;
                _isShowingLess = true;
            }
        }
    }
}