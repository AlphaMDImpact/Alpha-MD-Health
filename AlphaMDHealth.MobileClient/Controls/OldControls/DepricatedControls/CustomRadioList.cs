using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// custom radio list
    /// </summary>
    public class CustomRadioList : BaseContentView
    {
        private readonly CustomRadioTextList _singleSelectRadioList;
        private readonly Grid _mainLayout;
        private readonly CustomLabelControl _errorLabel;
        private List<OptionModel> _options;
        private long _selectedvalue;
        private string _htmlPrefixText;
        //private bool _isApplyHeightToError = true;

        /// <summary>
        /// Selected Values Changed event
        /// </summary>
        public event EventHandler<EventArgs> SelectedValuesChanged;

        ///// <summary>
        ///// IsApplyHeightToError
        ///// </summary>
        //public bool IsApplyHeightToError
        //{
        //    get => _isApplyHeightToError;
        //    set
        //    {
        //        _isApplyHeightToError = value;
        //        _errorLabel.HeightRequest = value ? _errorLabelHeight : -1;
        //        _errorLabel.WidthRequest = ControlWidth;
        //    }
        //}

        ///// <summary>
        ///// Control Width
        ///// </summary>
        //public double ControlWidth { get; set; } = -1;

        ///// <summary>
        ///// get adn set picker width 
        ///// </summary>
        //public double PickerWidthRequest
        //{
        //    get => _singleSelectRadioList.WidthRequest;
        //    set
        //    {
        //        _singleSelectRadioList.WidthRequest = value;
        //    }
        //}

        /// <summary>
        /// IsBackGroundTransparent
        /// </summary>
        public bool IsBackGroundTransparent { get; set; }

        /// <summary>
        /// get and set bindable picker selected index
        /// </summary>
        public int BindableSelectedIndex { get; set; }

        /// <summary>
        /// Bindable picker source
        /// </summary>
        [DataMember]
        public IList<OptionModel> ItemSource
        {
            get => (IList<OptionModel>)_singleSelectRadioList.ItemsSource;
            set
            {
                _options = (List<OptionModel>)value;
                SetSelectedValue();
            }
        }

        /// <summary>
        /// picker seleted value
        /// </summary>
        public long SelectedValue
        {
            get => _selectedvalue;
            set
            {
                _selectedvalue = value;
                SetSelectedValue();
            }
        }

        /// <summary>
        /// selected value text
        /// </summary>
        public string SelectedText { get; set; }

        /// <summary>
        /// Custom Bindable Picker Control class constructor
        /// </summary>
        public CustomRadioList()
        {
            _mainLayout = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                ColumnSpacing = 10,
                // Margin = new Thickness(0, 0, 0, Convert.ToDouble(Application.Current.Resources[LibStyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture)),
                Padding = new Thickness(0, 5),
                RowDefinitions =
                    {
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)}
                    },
                ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto)},
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    },
            };

            _singleSelectRadioList = new CustomRadioTextList
            {
                IsHorizontal = true,
                //RadioWidth = MobileConstants.IsIosPlatform ? (double)AppImageSize.ImageSizeXXL : 190,
                RadioButtonStyle = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_RADIO_BUTTON_KEY],
                Margin = new Thickness(0, 20, 0, 0),
                //HeightRequest = 35,
                HorizontalOptions = LayoutOptions.StartAndExpand,
            };
            if (Device.RuntimePlatform == Device.Android)
            {
                _singleSelectRadioList.RadioWidth = 190;
            }
            AutomationProperties.SetIsInAccessibleTree(_singleSelectRadioList, true);
            _singleSelectRadioList.OnSelectionChanged += OnSingleSelectRadioSelectionChanged;

            _mainLayout.Add(_singleSelectRadioList, 0, 1);
            _errorLabel = new CustomLabelControl(LabelType.ClientErrorLabel)
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                //WidthRequest = ControlWidth,
                IsHtmlLabelLineCount = true,
                IsVisible = false
            };
            ShowHeader = true;
            IsUnderLine = true;
            Content = _mainLayout;
        }

        /// <summary>
        /// on property change event
        /// </summary>
        /// <param name="propertyName">property name value</param>
        /// <returns></returns>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == _RENDERER)
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    _singleSelectRadioList.BackgroundColor = IsBackGroundTransparent
                        ? (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR]
                        : (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                }
            }
        }

        /// <summary>
        /// validate picker Control
        /// </summary>
        /// <param name="isButtonClick">validation fired after button is clicked</param>
        public override void ValidateControl(bool isButtonClick)
        {
            if (_resourceData.IsRequired && (SelectedValue == 0 || SelectedValue == -1))
            {
                string errorText = string.Format(CultureInfo.InvariantCulture, _htmlPrefixText,
                    GetResourceValueByKey(ResourceConstants.R_DROPDOWN_SELECTION_VALIDATION_KEY));
                SetError(errorText, true, -1);
                //_errorLabel.HeightRequest = IsApplyHeightToError
                //    ? _errorLabelHeight
                //    : _errorLabel.GetLineCount * _errorLabelHeight;
                IsValid = false;
            }
            else
            {
                SetError(string.Empty, false, 0);
                IsValid = true;
            }
        }

        /// <summary>
        /// Method to Apply Resource Value
        /// </summary>
        protected override async void ApplyResourceValue()
        {
            if (this.IsEnabled)
            {
                _singleSelectRadioList.Title = _resourceData.ResourceValue;
            }
            _htmlPrefixText = await GetSettingsValueAsync().ConfigureAwait(true);
            if (_headerLabel != null)
            {
                FormattedString fs = new FormattedString();
                fs.Spans.Add(new Span { Text = _resourceData.ResourceValue });
                if (_resourceData.IsRequired && this.IsEnabled)
                {
                    fs.Spans.Add(new Span
                    {
                        TextColor = (Color)Application.Current.Resources[StyleConstants.ST_ACCENT_COLOR],
                        Text = Constants.IS_REQUIRED_FEILD_INDICATOR
                    });
                }
                _headerLabel.FormattedText = fs;
            }
            if (ShowHeader && !string.IsNullOrWhiteSpace(_resourceData.InfoValue))
            {
                _infoIcon.IsVisible = true;
                _infoIcon.Clicked += InfoIcon_Clicked;
            }
        }

        private void OnSingleSelectRadioSelectionChanged(object sender, EventArgs e)
        {
            CustomRadioTextList radioData = (CustomRadioTextList)sender;
            if (radioData.SelectedIndex != -1)
            {
                _selectedvalue = _options[radioData.SelectedIndex].OptionID;
                SelectedText = _options[radioData.SelectedIndex].OptionText;
                SetError(string.Empty, false, 0);
                radioData.ClassId = this.ClassId;
                SelectedValuesChanged?.Invoke(radioData, e);
            }
        }

        private void InfoIcon_Clicked(object sender, EventArgs e)
        {
            DisplayInfoPopup(_resourceData.InfoValue);
        }

        private void SetError(string text, bool isVisible, double height)
        {
            _errorLabel.Text = text;
            _errorLabel.IsVisible = isVisible;
            _errorLabel.HeightRequest = height;
            if (isVisible)
            {
                AutomationProperties.SetIsInAccessibleTree(_errorLabel, true);
                _mainLayout.Add(_errorLabel, 0, 2);
                Grid.SetColumnSpan(_errorLabel, 2);
            }
            else
            {
                _mainLayout.Children.Remove(_errorLabel);
            }
        }

        private void SetSelectedValue()
        {
            if (GenericMethods.IsListNotEmpty(_options))
            {
                _singleSelectRadioList.ItemsSource = _options.Select(x => x.OptionText).ToList();
                _singleSelectRadioList.SelectedIndex = (_selectedvalue != -1)
                    ? _options.FindIndex(a => a.OptionID == _selectedvalue)
                    : _options.FindIndex(a => a.IsSelected);
            }
            else
            {
                _singleSelectRadioList.ItemsSource = null;
            }
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
        }

        /// <summary>
        /// Method to Enable value
        /// </summary>
        /// <param name="value"></param>
        protected override void EnabledValue(bool value)
        {
            _singleSelectRadioList.IsEnabled = value;
        }
    }
}
