using System.Globalization;
using System.Runtime.CompilerServices;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Custom Mobile Control
    /// </summary>
    public class CustomMobileControl : BaseContentView
    {

        /// <summary>
        ///  Value Property
        /// </summary>
        public static readonly BindableProperty ValueProperty =
               BindableProperty.Create(nameof(Value), typeof(string), typeof(CustomMobileControl), defaultBindingMode: BindingMode.TwoWay);

        private readonly EntryValidationBehavior _validationError = new EntryValidationBehavior();
        private readonly CustomLabelControl _errorLabel;
        private readonly CustomEntry _customEntry;
        private readonly CustomBindablePicker _countryCodePicker;
        private bool _isButtonClick;
        private BaseDTO _countryCode;
        private string _valueText;
        private readonly Grid _mainLayout;
        private readonly Border _borderFrame;

        /// <summary>
        /// To Get and set control text value
        /// </summary>
        public string Value
        {
            get
            {
                AssignValue();
                return _valueText;
            }
            set
            {
                _valueText = value;
                SetValue(value);
            }
        }

        /// <summary>
        /// Get or Set the Country Code List to be Bound to the Mobile Number Control
        /// </summary>
        public BaseDTO CountrySource
        {
            get => _countryCode;
            set
            {
                _countryCode = value;
                _countryCodePicker.ItemsSource = value?.CountryCodes.Select(c => c.CountryCode).ToList();
            }
        }

        /// <summary>
        /// seclected country index value 
        /// </summary>
        public void SetSelectedCountryId(int value)
        {
            var countryPref = App._essentials.GetPreferenceValue(StorageConstants.PR_DEVICE_COUNTRY_CODE_KEY, string.Empty);
            if (!string.IsNullOrWhiteSpace(countryPref))
            {
                var country = CountrySource.CountryCodes.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.CountryCulture) && c.CountryCulture.Equals(countryPref));
                var countryMatch = CountrySource.CountryCodes.IndexOf(country);
                value = countryMatch > -1 ? countryMatch : value;
            }
            if (CountrySource != null && CountrySource.CountryCodes.Count > 0)
            {
                _countryCodePicker.SelectedIndex = value;
                _customEntry.MaxCharacterCount = CountrySource.CountryCodes[value].MobileNumberLength;
            }
        }


        /// <summary>
        /// seclected country index value 
        /// </summary>
        public void SetSelectedCountryId(string selectedCountryCode)
        {
            int value;
            if (string.IsNullOrWhiteSpace(selectedCountryCode))
            {
                if (!string.IsNullOrWhiteSpace(App._essentials.GetPreferenceValue(StorageConstants.PR_DEVICE_COUNTRY_CODE_KEY, string.Empty)))
                {
                    var countryMatch = CountrySource.CountryCodes.IndexOf(CountrySource.CountryCodes.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.CountryCulture) && c.CountryCulture.Equals(App._essentials.GetPreferenceValue(StorageConstants.PR_DEVICE_COUNTRY_CODE_KEY, string.Empty))));
                    value = countryMatch > -1 ? countryMatch : 0;
                }
                else
                {
                    value = 0;
                }
            }
            else
            {
                value = CountrySource.CountryCodes.IndexOf(CountrySource.CountryCodes.Find(c => c.CountryCode.Equals(selectedCountryCode)));
            }
            if (CountrySource != null && CountrySource.CountryCodes.Count > 0)
            {
                _countryCodePicker.SelectedIndex = value;
                //_customEntry.MaxLength = CountrySource.CountryCodes[value].MobileNumberLength;
            }
        }

        /// <summary>
        /// Constructor for class Custom Mobile Control
        /// </summary>
        public CustomMobileControl()
        {
            _customEntry = new CustomEntry
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_ENTRY_STYLE_KEY],
                PlaceholderColor = (Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_TEXT_COLOR],
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Keyboard = Keyboard.Numeric,
                Margin = new Thickness(0, 2.1, 2.7, 2.5),
                Text = Value
            };
            AutomationProperties.SetIsInAccessibleTree(_customEntry, true);
            _customEntry.Unfocused += VhsEntry_Unfocused;
            _customEntry.Focused += CustomEntry_Focused;
            SetBinding(CustomMobileControl.ValueProperty, new Binding { Source = _customEntry, Path = _TEXT, });
            _errorLabel = new CustomLabelControl(LabelType.ClientErrorLabel)
            {
                IsVisible = false
            };
            _errorLabel.SetBinding(CustomLabel.TextProperty, new Binding { Source = _validationError, Path = _VALIDATION_ERROR });
            AutomationProperties.SetIsInAccessibleTree(_errorLabel, true);
            _countryCodePicker = new CustomBindablePicker
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_BINDABLE_PICKER_STYLE],
                VerticalOptions = LayoutOptions.Center,
                FontEnabledColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_TEXT_COLOR],
            };
            AutomationProperties.SetIsInAccessibleTree(_countryCodePicker, true);
            _countryCodePicker.SelectedIndexChanged += OnCountryCodeSelectedIndexChange;
            _mainLayout = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                Margin = new Thickness(0, 0, 0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture)),
                ColumnSpacing = 10,
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                },
            };
            _mainLayout.Add(_errorLabel, 0, 2);
            ShowHeader = true;
            _borderFrame = new Border
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_CONTROL_STYLE],
                Padding = new Thickness(15, 0, 0, 0),
                BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR],
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            AutomationProperties.SetIsInAccessibleTree(_borderFrame, true);
            AutomationProperties.SetIsInAccessibleTree(_mainLayout, true);
            Content = _mainLayout;
        }

        private void VhsEntry_Unfocused(object sender, FocusEventArgs e)
        {
            _borderFrame.Stroke = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE];
        }

        private void CustomEntry_Focused(object sender, FocusEventArgs e)
        {
            _borderFrame.Stroke = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
        }
        /// <summary>
        /// validate control
        /// </summary>
        /// <param name="isButtonClick"> after click on save button it start displaying error</param>
        public override void ValidateControl(bool isButtonClick)
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
            if (IsValid && _errorLabel != null)
            {
                _errorLabel.IsVisible = false;
                _errorLabel.HeightRequest = 0;
            }
            else
            {
                if (!IsValid && _isButtonClick && _errorLabel != null)
                {
                    _errorLabel.IsVisible = true;
                    _errorLabel.HeightRequest = -1;
                }
            }
            _borderFrame.Stroke = _errorLabel.IsVisible ? (Color)Application.Current.Resources[StyleConstants.ST_ERROR_COLOR] : (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE];
        }


        /// <summary>
        /// Inveked on property changed
        /// </summary>
        /// <param name="propertyName">name of the property changed</param>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == _VALUE)
            {
                if (string.IsNullOrWhiteSpace(_customEntry.Text))
                {
                    ValidateControl(_isButtonClick);
                    return;
                }
                else
                {
                    AssignValue();
                    ValidateControl(_isButtonClick);
                }
            }
            if (propertyName == _RENDERER)
            {
                _countryCodePicker.WidthRequest = 50;
                var phoneWithContry = CreateEntryWithDropDown(_customEntry, _countryCodePicker);
                _mainLayout.Add(phoneWithContry, 0, 1);
                Grid.SetColumnSpan(phoneWithContry, 2);
            }
        }

        /// <summary>
        /// method to Apply Resource Value
        /// </summary>
        protected override async void ApplyResourceValue()
        {
            _customEntry.ClassId = _resourceData.ResourceValue;
            if (string.IsNullOrWhiteSpace(_customEntry.AutomationId))
            {
                _customEntry.AutomationId = $"{ShellMasterPage.CurrentShell.CurrentPage.GetType().Name}{_resourceData.ResourceValue}";
            }
            if (string.IsNullOrWhiteSpace(_countryCodePicker.AutomationId))
            {
                _countryCodePicker.AutomationId = $"{ShellMasterPage.CurrentShell.CurrentPage.GetType().Name}{nameof(_countryCodePicker)}{_resourceData.ResourceValue}";
            }
            if (string.IsNullOrWhiteSpace(_borderFrame.AutomationId))
            {
                _borderFrame.AutomationId = $"{ShellMasterPage.CurrentShell.CurrentPage.GetType().Name}{Constants.AUTOMATION_ID_SEPRATOR}{_resourceData.ResourceValue}";
            }
            AutomationProperties.SetName(_customEntry, _customEntry.AutomationId);
            AutomationProperties.SetName(_borderFrame, _borderFrame.AutomationId);
            AutomationProperties.SetName(_countryCodePicker, _countryCodePicker.AutomationId);
            AutomationProperties.SetName(_mainLayout, _borderFrame.AutomationId);
            var htmlPrefixText = await GetSettingsValueAsync().ConfigureAwait(true);
            if (_resourceData.IsRequired)
            {
                _customEntry.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.IsRequired, string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY))));
            }
            _customEntry.Validations.Add(new KeyValuePair<ValidationType, string>(ValidationType.PhoneNumber, string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_MINIMUM_LENGTH_VALIDATION_KEY))));

            if (_customEntry.Validations.Count > 0)
            {
                SetBinding(IsValidProperty, new Binding { Source = _validationError, Path = Constants.IS_VALID });
                _customEntry.Behaviors.Add(_validationError);
            }
            else
            {
                IsValid = true;
            }
            if (this.IsEnabled)
            {
                //_customEntry.Placeholder = _resourceData.PlaceHolderValue;
            }
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
        /// Method to render header
        /// </summary>
        protected override void RenderHeader()
        {
            AddHeaderInView(_mainLayout);
        }

        /// <summary>
        /// Method to Enable value
        /// </summary>
        /// <param name="value"></param>
        protected override void EnabledValue(bool value)
        {
            if (_customEntry != null)
            {
                _customEntry.IsEnabled = value;
                _customEntry.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_TEXT_COLOR];
            }
        }

        /// <summary>
        /// Method to set rendere value
        /// </summary>
        /// <param name="value">border thikness</param>
        protected override void RenderBorder(bool value)
        {
            throw new NotSupportedException();
        }

        private void SetValue(string value)
        {
            if (_countryCodePicker != null && value.Contains(Constants.SYMBOL_DASH))
            {
                var valueData = value.Split(Constants.SYMBOL_DASH);
                _countryCodePicker.SelectedIndex = CountrySource.CountryCodes.IndexOf(CountrySource.CountryCodes.Find(c => c.CountryCode.Equals(valueData[0])));
                //_customEntry.MaxLength = CountrySource.CountryCodes[_countryCodePicker.SelectedIndex].MobileNumberLength;
                _customEntry.Text = valueData[1];
            }
        }

        private void AssignValue()
        {
            if (_countryCodePicker != null && _countryCodePicker.SelectedItem != null)
            {
                _valueText = _countryCodePicker.SelectedItem + Constants.SYMBOL_DASH.ToString(CultureInfo.InvariantCulture) + _customEntry.Text;

            }
            if (_countryCodePicker?.SelectedItem == null && !string.IsNullOrWhiteSpace(_countryCodePicker?.Title))
            {
                _valueText = _countryCodePicker.Title + Constants.SYMBOL_DASH + _customEntry.Text;
            }
        }

        private Border CreateEntryWithDropDown(CustomEntry entry, CustomBindablePicker picker)
        {
            Grid resultLayout = new Grid
            {
                ColumnSpacing = Constants.ZERO_PADDING,
                VerticalOptions = LayoutOptions.Center,
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                },
            };
            resultLayout.Add(picker, 0, 0);
            resultLayout.Add(entry, 1, 0);
            _borderFrame.Content = resultLayout;
            return _borderFrame;
        }

        private void OnCountryCodeSelectedIndexChange(object sender, EventArgs e)
        {
            CustomBindablePicker picker = (CustomBindablePicker)sender;
            if (picker.SelectedIndex > -1)
            {
                //_customEntry.MaxLength = CountrySource.CountryCodes[picker.SelectedIndex].MobileNumberLength;
                _customEntry.Text = string.Empty;
            }

        }
    }
}