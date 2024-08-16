using System.Globalization;
using System.Runtime.CompilerServices;
using AlphaMDHealth.Utility;
using CommunityToolkit.Maui.Behaviors;
namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom entry control
    /// </summary>
    public class CustomEntryControl : BaseContentView
    {
        /// <summary>
        /// value property
        /// </summary>
        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create(nameof(Value), typeof(string), typeof(CustomEntryControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: CustomEntryControl.ValueChangeProperty);

        private static void ValueChangeProperty(BindableObject bindable, object oldValue, object newValue)
        {
            CustomEntryControl control = (CustomEntryControl)bindable;
            if (newValue != null && (string)newValue != string.Empty)
            {
                control.Value = (string)newValue;
            }
        }

        private readonly CustomEntry _customEntry;
        private readonly CustomPasswordEntry _customPasswordEntry;
        private readonly EntryValidationBehavior _validationError = new();
        private readonly PasswordEntryValidationBehavior _passwordvalidationError = new();
        private bool _isButtonClick;
        private FieldTypes _entryType;
        private double _minLength, _maxLength = 0;
        private double _minMaxCounter = 0;

        /// <summary>
        /// On advance entry unfocused event
        /// </summary>
        public event EventHandler<EventArgs> OnAdvanceEntryUnfocused;

        /// <summary>
        /// On advance entry text change event
        /// </summary>
        public event EventHandler<EventArgs> OnCustomEntryTextChanged;

        /// <summary>
        /// to get and set control text value
        /// </summary>
        public string Value
        {
            get => _customEntry?.Text;
            set
            {
                if (_entryType == FieldTypes.CounterEntryControl)
                {

                    if (value != _minMaxCounter.ToString())
                    {
                        _customEntry.Text = value;
                    }
                    else
                    {

                        _customEntry.Text = _minMaxCounter.ToString();
                    }
                }
                else
                {
                    _customEntry.Text = value;
                }
            }
        }

        /// <summary>
        /// Is Master error color change
        /// </summary>
        public bool IsMaster
        {
            get {
                if (IsPasswordValue)
                {
                    return _customPasswordEntry.StyleId == "True";
                }
                else
                {
                    return _customEntry.StyleId == "True";
                }
            }
            set
            {
                if (value)
                {
                    if (IsPasswordValue)
                    {
                        _customPasswordEntry.ErrorColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_TEXT_COLOR];
                    }
                    else
                    {
                        _customEntry.ErrorColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_TEXT_COLOR];
                    }
                }
            }
        }

        /// <summary>
        /// Control type which decide the type of keybord
        /// </summary>
        public FieldTypes ControlType
        {
            get => _entryType;
            set
            {
                _entryType = value;
                switch (value)
                {
                    case FieldTypes.NumericEntryControl:
                    case FieldTypes.PinCodeControl:
                    case FieldTypes.CounterEntryControl:
                        _customEntry.Keyboard = Keyboard.Numeric;
                        break;
                    case FieldTypes.DecimalEntryControl:
                        _customEntry.Keyboard = Keyboard.Numeric;
                        _customEntry.IsDecimal = true;
                        break;
                    case FieldTypes.EmailEntryControl:
                        _customEntry.Keyboard = Keyboard.Email;
                        break;
                    default:
                        _customEntry.Keyboard = Keyboard.Text;
                        break;
                }
            }
        }

        /// <summary>
        /// It is used for Decimal precision value
        /// </summary>
        public int DecimalPrecision { get; set; } = 2;

        /// <summary>
        /// Left icon should be set in svg path property
        /// </summary>
        public string ControlIcon
        {
            set; get;
        }

        /// <summary>
        /// Decide whether entry text is passoword type or not
        /// </summary>
        public bool IsPasswordValue
        {
            get;set;
        }

        /// <summary>
        /// regex value to fired regex validaton
        /// </summary>
        public string RegexPattern
        {
            get;
            set;
        }

        /// <summary>
        /// Decide whether entry text is passoword type or not
        /// </summary>
        public bool IsBoldText
        {
            get => _customEntry.TextFontAttributes == FontAttributes.Bold;
            set => _customEntry.TextFontAttributes = FontAttributes.Bold;
        }

        /// <summary>
        /// IsBackGroundTransparent
        /// </summary>
        public bool IsBackGroundTransparent
        {
            get; set;
        }

        /// <summary>
        /// transparent border set it befor control type
        /// </summary>
        public bool ControlWithoutBorder { get; set; }

        /// <summary>
        /// custom entry control initializer
        /// </summary>
        public CustomEntryControl()
        {
            _customEntry = new CustomEntry
            {
                Pattern = RegexPattern,
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_ENTRY_STYLE_KEY],
                HorizontalOptions = LayoutOptions.Fill,
                Text = Value,
                IsReadOnly = false,
            };
            SetBinding(CustomEntryControl.ValueProperty, new Binding { Source = _customEntry, Path = _TEXT, });
            _customEntry.Unfocused += CustomEntry_Unfocused;
            _customEntry.TextChanged += CustomEntry_TextChanged;
            _customPasswordEntry = new CustomPasswordEntry
            {
                Pattern = RegexPattern,
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_ENTRY_STYLE_KEY],
                HorizontalOptions = LayoutOptions.Fill,
                Text = Value,
                IsReadOnly = false,
                EndIcon = ImageSource.FromFile(ImageConstants.I_SHOW_PNG)
            };
            _customPasswordEntry.EndIconClicked += CustomEntry_EndIconClicked;
            ShowHeader = true;
            IsUnderLine = true;
        }

        private void CustomEntry_TextChanged(object sender, EventArgs e)
        {
            OnCustomEntryTextChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// validate Entry Control 
        /// </summary>
        /// <param name="isButtonClick">validation fired after button is clicked</param>
        public override void ValidateControl(bool isButtonClick)
        {
            if(IsPasswordValue)
            {
                _passwordvalidationError.ValidateControl(_customPasswordEntry);
                if (_customPasswordEntry.Validations.Count == 0)
                {
                    IsValid = true;
                }
                if (!_isButtonClick)
                {
                    _isButtonClick = isButtonClick;
                }
                if (IsValid)
                {
                    _customPasswordEntry.HasError = false;
                }
                else
                {
                    if (!IsValid && _isButtonClick)
                    {
                        _customPasswordEntry.HasError = true;
                    }
                }
            }
            else
            {
                _validationError.ValidateControl(_customEntry);
                if (_customEntry.Validations.Count == 0)
                {
                    IsValid = true;
                }
                if (!_isButtonClick)
                {
                    _isButtonClick = isButtonClick;
                }
                if (IsValid)
                {
                    _customEntry.HasError = false;
                }
                else
                {
                    if (!IsValid && _isButtonClick)
                    {
                        _customEntry.HasError = true;
                    }
                }
            }
         
           
        }

        /// <summary>
        /// apply resource value method
        /// </summary>
        protected override async void ApplyResourceValue()
        {
            if(IsPasswordValue)
            {
                _customPasswordEntry.ClassId = _resourceData.ResourceValue;
                string htmlPrefixText = await RenderControlAsync();
                if (_resourceData.IsRequired)
                {
                    _customPasswordEntry.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.IsRequired, string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY))));
                }
                System.Diagnostics.Debug.WriteLine($"===={_resourceData.MinLength}==={_resourceData.MaxLength}========{_resourceData.ResourceValue}");
                _minLength = _resourceData.MinLength;
                _maxLength = _resourceData.MaxLength;

                if (IsNumericControls())
                {
                    if (!_minLength.Equals(_maxLength) && _minLength < _maxLength)
                    {
                        if (this.IsEnabled)
                        {
                            _customPasswordEntry.MaxCharacterCount = _customPasswordEntry.IsDecimal ? _resourceData.MaxLength.ToString().Length + 1 + DecimalPrecision : _resourceData.MaxLength.ToString().Length;
                        }

                        _passwordvalidationError.DecimalPrecision = DecimalPrecision;
                        _passwordvalidationError.MaxLength = _maxLength.ToString().Length;
                        _customPasswordEntry.Range = _minLength.ToString(CultureInfo.CurrentCulture) + Constants.SYMBOL_COLAN + _maxLength.ToString(CultureInfo.CurrentCulture);
                        _customPasswordEntry.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.ValidationRange,
                            string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_RANGE_LENGTH_VALIDATION_KEY))));
                    }
                }
                else
                {
                    if (_minLength > 0)
                    {
                        _customPasswordEntry.StyleId = Convert.ToString(_minLength, CultureInfo.CurrentCulture);
                        _customPasswordEntry.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.MinimumLength,
                            string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_MINIMUM_LENGTH_VALIDATION_KEY))));
                    }
                    if (_maxLength > 0 && this.IsEnabled)
                    {
                        _customPasswordEntry.MaxCharacterCount = (int)_maxLength;
                    }
                }
                ApplyRegularExpressionPattern(htmlPrefixText);
                if (_customPasswordEntry.Validations.Count > 0)
                {
                    SetBinding(IsValidProperty, new Binding { Source = _passwordvalidationError, Path = Constants.IS_VALID });
                    _customPasswordEntry.Behaviors.Add(_passwordvalidationError);
                }
                ApplyResoures();
            }
            else
            {
                _customEntry.ClassId = _resourceData.ResourceValue;
                string htmlPrefixText = await RenderControlAsync();
                if (_resourceData.IsRequired)
                {
                    _customEntry.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.IsRequired, string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY))));
                }
                System.Diagnostics.Debug.WriteLine($"===={_resourceData.MinLength}==={_resourceData.MaxLength}========{_resourceData.ResourceValue}");
                _minLength = _resourceData.MinLength;
                _maxLength = _resourceData.MaxLength;

                if (IsNumericControls())
                {
                    if (!_minLength.Equals(_maxLength) && _minLength < _maxLength)
                    {
                        if (this.IsEnabled)
                        {
                            _customEntry.MaxCharacterCount = _customEntry.IsDecimal ? _resourceData.MaxLength.ToString().Length + 1 + DecimalPrecision : _resourceData.MaxLength.ToString().Length;
                        }

                        _validationError.DecimalPrecision = DecimalPrecision;
                        _validationError.MaxLength = _maxLength.ToString().Length;
                        _customEntry.Range = _minLength.ToString(CultureInfo.CurrentCulture) + Constants.SYMBOL_COLAN + _maxLength.ToString(CultureInfo.CurrentCulture);
                        _customEntry.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.ValidationRange,
                            string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_RANGE_LENGTH_VALIDATION_KEY))));
                    }
                }
                else
                {
                    if (_minLength > 0)
                    {
                        _customEntry.StyleId = Convert.ToString(_minLength, CultureInfo.CurrentCulture);
                        _customEntry.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.MinimumLength,
                            string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_MINIMUM_LENGTH_VALIDATION_KEY))));
                    }
                    if (_maxLength > 0 && this.IsEnabled)
                    {
                        _customEntry.MaxCharacterCount = (int)_maxLength;
                    }
                }
                ApplyRegularExpressionPattern(htmlPrefixText);
                if (_customEntry.Validations.Count > 0)
                {
                    SetBinding(IsValidProperty, new Binding { Source = _validationError, Path = Constants.IS_VALID });
                    _customEntry.Behaviors.Add(_validationError);
                }
                ApplyResoures();
            }
           

        }

       
        private bool IsNumericControls()
        {
            return (_entryType == FieldTypes.NumericEntryControl) || _entryType == FieldTypes.DecimalEntryControl || _entryType == FieldTypes.CounterEntryControl;
        }

        private void ApplyRegularExpressionPattern(string htmlPrefixText)
        {
            if(IsPasswordValue)
            {
                if (!string.IsNullOrWhiteSpace(RegexPattern))
                {
                    _customPasswordEntry.Pattern = RegexPattern;
                    _customPasswordEntry.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.ValidationRegxString, string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_INVALID_DATA_KEY))));
                }
                if (_entryType == FieldTypes.DecimalEntryControl)
                {
                    if (this.IsEnabled)
                    {
                        _customPasswordEntry.MaxCharacterCount = _resourceData.MaxLength.ToString().Length + 1 + DecimalPrecision;
                    }
                    _passwordvalidationError.DecimalPrecision = DecimalPrecision;
                    _passwordvalidationError.MaxLength = _maxLength.ToString().Length;
                    _customPasswordEntry.Pattern = "^\\d+\\.\\d{1," + (DecimalPrecision) + "}$";
                    _customPasswordEntry.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.Decimal, GetResourceValueByKey(ResourceConstants.R_INVALID_DATA_KEY)));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(RegexPattern))
                {
                    _customEntry.Pattern = RegexPattern;
                    _customEntry.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.ValidationRegxString, string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_INVALID_DATA_KEY))));
                }
                if (_entryType == FieldTypes.DecimalEntryControl)
                {
                    if (this.IsEnabled)
                    {
                        _customEntry.MaxCharacterCount = _resourceData.MaxLength.ToString().Length + 1 + DecimalPrecision;
                    }
                    _validationError.DecimalPrecision = DecimalPrecision;
                    _validationError.MaxLength = _maxLength.ToString().Length;
                    _customEntry.Pattern = "^\\d+\\.\\d{1," + (DecimalPrecision) + "}$";
                    _customEntry.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.Decimal, GetResourceValueByKey(ResourceConstants.R_INVALID_DATA_KEY)));
                }
            }
           
        }

        private async Task<string> RenderControlAsync()
        {
            var htmlPrefixText = await GetSettingsValueAsync().ConfigureAwait(true);
            return htmlPrefixText;
        }

        /// <summary>
        /// on property change event
        /// </summary>
        /// <param name="propertyName">property name value</param>
        /// <returns></returns>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == _VALUE)
            {
                if(IsPasswordValue)
                {
                    _customPasswordEntry.Text = Value;
                }
                else
                {

                    _customEntry.Text = Value;
                }
                //_customEntry.Text = Value;
                ValidateControl(_isButtonClick);
            }
            if (propertyName == _RENDERER)
            {
                GenerateUI(_customEntry);
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    if (IsPasswordValue)
                    {
                        _customPasswordEntry.BackgroundColor = IsBackGroundTransparent ? (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR] : (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                    }
                    else
                    {
                        _customEntry.BackgroundColor = IsBackGroundTransparent ? (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR] : (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                    }
                }
            }
        }

  
        /// <summary>
        /// Render header method
        /// </summary>
        protected override void RenderHeader()
        {
            //AddHeaderInView(_mainLayout);
        }

        /// <summary>
        /// Render border
        /// </summary>
        /// <param name="value">value data</param>
        /// <returns></returns>
        protected override void RenderBorder(bool value)
        {
            if (_customEntry != null || _customPasswordEntry != null)
            {
                if (value)
                {
                    if (IsPasswordValue)
                    {

                        _customPasswordEntry.Style = (Style)Application.Current.Resources[StyleConstants.ST_UNDER_LINE_ENTRY_STYLE];
                    }
                    else
                    {
                        _customEntry.Style = (Style)Application.Current.Resources[StyleConstants.ST_UNDER_LINE_ENTRY_STYLE];

                    }
                }
                else
                {
                    if (IsPasswordValue)
                    {
                        _customPasswordEntry.Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_ENTRY_STYLE_KEY];
                    }
                    else
                    {
                        _customEntry.Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_ENTRY_STYLE_KEY];
                    }
                }
            }
        }

        /// <summary>
        /// enable value method
        /// </summary>
        /// <param name="value">value data</param>
        /// <returns></returns>
        protected override void EnabledValue(bool value)
        {

            if (_customEntry != null|| _customPasswordEntry!=null)
            {
                if (IsPasswordValue)
                {
                    _customPasswordEntry.IsEnabled = value;
                    _customPasswordEntry.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_TEXT_COLOR];
                }
                else
                {
                    _customEntry.IsEnabled = value;
                    _customEntry.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_TEXT_COLOR];
                }
            }
        }

        private void ApplyResoures()
        {
            if(IsPasswordValue)
            {
                _customPasswordEntry.PlaceholderText = this.IsEnabled && !string.IsNullOrWhiteSpace(_resourceData.PlaceHolderValue) ? _resourceData.PlaceHolderValue : string.Empty;
                if (ShowHeader)
                {
                    _customPasswordEntry.LabelText = _resourceData.ResourceValue+Constants.CHAR_SPACE+ Constants.IS_REQUIRED_FEILD_INDICATOR;
                }
                if (ShowHeader && !string.IsNullOrWhiteSpace(_resourceData.InfoValue))
                {
                    _customPasswordEntry.HelpText = _resourceData.InfoValue;
                }
            }
            else
            {
                _customEntry.PlaceholderText = _customEntry.LabelText = this.IsEnabled && !string.IsNullOrWhiteSpace(_resourceData.PlaceHolderValue) ? _resourceData.PlaceHolderValue : string.Empty;
                if (ShowHeader)
                {
                    _customEntry.LabelText = _resourceData.ResourceValue + Constants.CHAR_SPACE + Constants.IS_REQUIRED_FEILD_INDICATOR; ;
                }
                if (ShowHeader && !string.IsNullOrWhiteSpace(_resourceData.InfoValue))
                {
                    _customEntry.HelpText = _resourceData.InfoValue;
                }
            }
          
        }

        private void InfoIcon_Clicked(object sender, EventArgs e)
        {
            //DisplayInfoPopup(_resourceData.InfoValue);
        }

        private void CustomEntry_Unfocused(object sender, FocusEventArgs e)
        {
            OnAdvanceEntryUnfocused?.Invoke(sender, e);
        }

        private async void SetCounterValue(string value)
        {

            if (!string.IsNullOrWhiteSpace(value) && double.TryParse(value, out double counterVal))
            {
                await Task.Delay(100).ConfigureAwait(true);
                _minMaxCounter = counterVal;
                if (counterVal > _resourceData.MaxLength)
                {
                    _customEntry.Text = _maxLength.ToString();
                    _minMaxCounter = _maxLength;
                }
                else if (counterVal < _resourceData.MinLength)
                {
                    _customEntry.Text = _minLength.ToString();
                    _minMaxCounter = _minLength;
                }
                else if (counterVal == 0)
                {
                    _customEntry.Text = _minLength.ToString();
                    _minMaxCounter = _minLength;
                }
                else
                {
                    _customEntry.Text = value;
                    _minMaxCounter = counterVal;
                }
            }
        }

        private void GenerateUI(CustomEntry entry)
        {
            RenderBorder(IsUnderLine);
            if (ControlType == FieldTypes.CounterEntryControl )
            {
                entry.TextHorizontalAlignment = TextAlignment.Center;
              
                entry.EndIcon = ImageSource.FromFile(ImageConstants.I_COUNTER_INCREAMENT_PNG);
                entry.EndIconClicked += IncreamentCounter_Clicked;
               
                entry.StartIcon = ImageSource.FromFile(ImageConstants.I_COUNTER_DECREAMENT_PNG);
                entry.StartIconClicked += DecreamentCounter_Clicked;
                SetCounterValue(string.IsNullOrWhiteSpace(Value) ? _minLength.ToString() : Value);
            }
            if (ControlIcon != default)
            {
                entry.StartIcon = ImageSource.FromFile(_prefix + ControlIcon.Replace(_prefix, string.Empty));
            }

            entry.Margin = new Thickness(0);
            if (IsPasswordValue)
            {
                _customPasswordEntry.StartIcon = ImageSource.FromFile(_prefix + ControlIcon.Replace(_prefix, string.Empty));
                Content = _customPasswordEntry;
            }
            else
            {
                Content = _customEntry;
            }
           
        }

       
        private void CustomEntry_EndIconClicked(object sender, EventArgs e)
        {
            SvgImageView item = sender as SvgImageView;
            if (IsPasswordValue)
            {
                item.Behaviors.Add(new IconTintColorBehavior
                {
                    TintColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR]
                });
            }
            else
            {
                //item.Source = ImageSource.FromFile(LibImageConstants.I_SHOW_PNG);
                item.Behaviors.Add(new IconTintColorBehavior
                {
                    TintColor = (Color)Application.Current.Resources[StyleConstants.ST_SECONDARY_TEXT_COLOR]
                });
                //_customEntry.IsPassword = true;
            }
        }

        private void IncreamentCounter_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_customEntry.Text) && double.TryParse(_customEntry.Text, out double counterVal))
            {
                if (counterVal != _minMaxCounter)
                {
                    _minMaxCounter = counterVal;
                }
                else
                {
                    if (_minMaxCounter <= _minLength)
                    {
                        _minMaxCounter = _minLength;
                    }
                    if (_minMaxCounter >= _maxLength)
                    {
                        _minMaxCounter = _maxLength;
                    }
                    else
                    {
                        _minMaxCounter++;
                    }
                }
            }
            _customEntry.Text = _minMaxCounter.ToString();
        }

        private void DecreamentCounter_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_customEntry.Text) && double.TryParse(_customEntry.Text, out double counterVal))
            {
                if (counterVal != _minMaxCounter)
                {
                    _minMaxCounter = counterVal;
                }
                else
                {
                    if (_minMaxCounter >= _maxLength)
                    {
                        _minMaxCounter = _maxLength;
                    }
                    if (_minMaxCounter <= _minLength)
                    {
                        _minMaxCounter = _minLength;
                    }
                    else
                    {
                        _minMaxCounter--;
                    }
                }
            }
            _customEntry.Text = _minMaxCounter.ToString();
        }

    }
}