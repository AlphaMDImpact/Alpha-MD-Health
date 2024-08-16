using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represent Custom Bindable Picker Control 
    /// </summary>
    public class CustomBindablePickerControl : BaseContentView
    {
        private readonly CustomBindablePicker _bindableSelectPicker;
        private readonly Grid _mainLayout;
        private readonly CustomLabelControl _errorLabel;
        private readonly Border _borderFrame;
        private List<OptionModel> _options;
        private long selectedvalue;
        private string _htmlPrefixText;
        private bool _isApplyHeightToError = true;

        /// <summary>
        /// Selected Values Changed event
        /// </summary>
        public event EventHandler<EventArgs> SelectedValuesChanged;

        /// <summary>
        /// On advance entry unfocused event
        /// </summary>
        public event EventHandler<EventArgs> OnAdvancePickerUnfocused;
        /// <summary>
        /// IsApplyHeightToError
        /// </summary>
        public bool IsApplyHeightToError
        {
            get => _isApplyHeightToError;
            set
            {
                _isApplyHeightToError = value;
                _errorLabel.HeightRequest = value ? _errorLabelHeight : -1;
                _errorLabel.WidthRequest = ControlWidth;
            }
        }

        /// <summary>
        /// Control Width
        /// </summary>
        public double ControlWidth { get; set; } = -1;

        /// <summary>
        /// get adn set picker width 
        /// </summary>
        public double PickerWidthRequest
        {
            get => _bindableSelectPicker.WidthRequest;
            set
            {
                _bindableSelectPicker.WidthRequest = value;
                _borderFrame.WidthRequest = value;
            }
        }
        /// <summary>
        /// IsBackGroundTransparent
        /// </summary>
        public bool IsBackGroundTransparent
        {
            get; set;
        }
        /// <summary>
        /// get and set picker text color
        /// </summary>
        public Color PickerTextColor
        {
            get => _bindableSelectPicker.TextColor;
            set
            {
                _bindableSelectPicker.TextColor = value;
                _bindableSelectPicker.FontEnabledColor = value;
            }
        }

        /// <summary>
        /// get and set bindable picker selected index
        /// </summary>
        public int BindableSelectedIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Bindable picker source
        /// </summary>
        [DataMember]
        public IList<OptionModel> ItemSource
        {
            get => (IList<OptionModel>)_bindableSelectPicker.ItemsSource;
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
            get => selectedvalue;
            set
            {
                selectedvalue = value;
                SetSelectedValue();
            }
        }

        /// <summary>
        /// selected value text
        /// </summary>
        public string SelectedText
        {
            get;
            set;
        }

        /// <summary>
        /// Custom Bindable Picker Control class constructor
        /// </summary>
        public CustomBindablePickerControl()
        {
            _mainLayout = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                ColumnSpacing = 10,
                Margin = new Thickness(0, 0, 0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture)),
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

            _bindableSelectPicker = new CustomBindablePicker
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_BINDABLE_PICKER_STYLE],
                HeightRequest = 35,
                Margin = new Thickness(GenericMethods.GetPlatformSpecificValue(0, 5, 0), 0),
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            AutomationProperties.SetIsInAccessibleTree(_bindableSelectPicker, true);
            _bindableSelectPicker.SelectedIndexChanged += OnPickerSelectedIndexChange;
            _bindableSelectPicker.Unfocused += OnPicker_Unfocused;
            _borderFrame = new Border
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_CONTROL_STYLE],
                Padding = new Thickness(15, 0, 5, 0),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Content = _bindableSelectPicker,
            };
            _mainLayout.Add(_borderFrame, 0, 1);
            Grid.SetColumnSpan(_borderFrame, 2);
            _errorLabel = new CustomLabelControl(LabelType.ClientErrorLabel)
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                WidthRequest = ControlWidth,
                IsHtmlLabelLineCount = true,
                IsVisible = false
            };

            AutomationProperties.SetIsInAccessibleTree(_errorLabel, true);
            _mainLayout.Add(_errorLabel, 0, 2);
            Grid.SetColumnSpan(_errorLabel, 2);
            ShowHeader = true;
            IsUnderLine = true;
            Content = _mainLayout;
        }

        private void OnPicker_Unfocused(object sender, FocusEventArgs e)
        {
            CustomBindablePicker picker = (CustomBindablePicker)sender;
            OnAdvancePickerUnfocused?.Invoke(picker, e);
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
                RenderBorder(IsUnderLine);
                if (Device.RuntimePlatform == Device.Android)
                {
                    _bindableSelectPicker.BackgroundColor = IsBackGroundTransparent ? (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR] : (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                }
                else
                {
                    _borderFrame.BackgroundColor = _bindableSelectPicker.BackgroundColor = IsBackGroundTransparent ? Colors.Transparent : (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                }
            }
        }

        private void SetSelectedValue()
        {
            if (GenericMethods.IsListNotEmpty(_options))
            {
                _bindableSelectPicker.ItemsSource = _options.Select(x => x.OptionText).ToList();
                _bindableSelectPicker.SelectedIndex = (selectedvalue != -1) ? _options.FindIndex(a => a.OptionID == selectedvalue) : _options.FindIndex(a => a.IsSelected);
            }
            else
            {
                _bindableSelectPicker.ItemsSource = null;
                _bindableSelectPicker.SelectedIndex = -1;
                selectedvalue = 0;
            }
        }

        /// <summary>
        /// validate picker Control
        /// </summary>
        /// <param name="isButtonClick">validation fired after button is clicked</param>
        public override void ValidateControl(bool isButtonClick)
        {

            if (_resourceData != null && _resourceData.IsRequired)
            {
                if (SelectedValue == 0 || SelectedValue == -1 || (_bindableSelectPicker != null && _bindableSelectPicker.SelectedIndex == -1))
                {
                    string errorlabel = string.Format(CultureInfo.InvariantCulture, _htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_DROPDOWN_SELECTION_VALIDATION_KEY));
                    _errorLabel.Text = errorlabel;
                    _errorLabel.IsVisible = true;
                    _errorLabel.HeightRequest = IsApplyHeightToError ? _errorLabelHeight : _errorLabel.GetLineCount * _errorLabelHeight;
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
            if (_errorLabel.IsVisible == true)
            {
                _bindableSelectPicker.BorderColor = (Color)Application.Current.Resources[StyleConstants.ST_ERROR_COLOR];
                _borderFrame.Stroke = (Color)Application.Current.Resources[StyleConstants.ST_ERROR_COLOR];
            }
            else
            {
                _bindableSelectPicker.BorderColor = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE];
                _borderFrame.Stroke = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE];
            }
        }

        private void OnPickerSelectedIndexChange(object sender, EventArgs e)
        {
            CustomBindablePicker picker = (CustomBindablePicker)sender;
            if (picker.SelectedIndex != -1)
            {
                selectedvalue = _options[picker.SelectedIndex].OptionID;
                SelectedText = _options[picker.SelectedIndex].OptionText;
                _errorLabel.Text = string.Empty;
                _errorLabel.IsVisible = false;
                _bindableSelectPicker.BorderColor = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE];
                _borderFrame.Stroke = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE];
                picker.ClassId = this.ClassId;
                SelectedValuesChanged?.Invoke(picker, e);
            }
        }

        /// <summary>
        /// Method to Apply Resource Value
        /// </summary>
        protected override async void ApplyResourceValue()
        {
            if (_resourceData != null)
            {
                if (this.IsEnabled)
                {
                    _bindableSelectPicker.Title = _resourceData?.ResourceValue;
                }
                else
                {
                    _bindableSelectPicker.Title = _resourceData?.ResourceValue;
                }
                _htmlPrefixText = await GetSettingsValueAsync().ConfigureAwait(true);
                if (_headerLabel != null)
                {
                    FormattedString fs = new FormattedString();
                    fs.Spans.Add(new Span { Text = _resourceData?.ResourceValue });
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
        }

        private void InfoIcon_Clicked(object sender, EventArgs e)
        {
            DisplayInfoPopup(_resourceData.InfoValue);
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
            if (value)
            {
                _borderFrame.StrokeThickness = 0;
                _borderFrame.Padding = new Thickness(0);
                _bindableSelectPicker.BorderType = _LINE;
                _bindableSelectPicker.BorderColor = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE];
                _bindableSelectPicker.Margin = new Thickness(GenericMethods.GetPlatformSpecificValue(0, -1, 0), 0);
            }
            else
            {
                _bindableSelectPicker.BorderType = string.Empty;
                _bindableSelectPicker.HeightRequest = (double)Application.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT];
                _borderFrame.Padding = new Thickness(15, 0, 5, 0);
                _borderFrame.StrokeThickness = 1;
            }
        }

        /// <summary>
        /// Method to Enable value
        /// </summary>
        /// <param name="value"></param>
        protected override void EnabledValue(bool value)
        {
            _bindableSelectPicker.IsEnabled = value;
        }
    }
}