using System.Globalization;
using System.Runtime.CompilerServices;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Custom Date Time Control
    /// </summary>
    public class CustomDateTimeControl : BaseContentView
    {
        private readonly Grid _mainLayout;
        private CustomDatePicker _datePicker;
        private CustomTimePicker _timePicker;
        private readonly CustomLabelControl _errorLabel;
        private Border _borderFrame;
        private FieldTypes _controlType;
        private TimeSpan _previousTime;
        private TimeSpan _maxTime;
        private string _htmlPrefixText;

        private bool _isApplyHeightToError = true;

        /// <summary>
        /// OnDateSelectedValueChanged Event
        /// </summary>
        public event EventHandler<EventArgs> OnDateSelectedValueChanged;

        /// <summary>
        /// Get/ set Control type to Date and time
        /// </summary>
        public FieldTypes ControlType
        {
            get => _controlType;
            set
            {
                _controlType = value;
                RenderControl(_controlType);
            }
        }

        /// <summary>
        /// TimeHorizontalOption
        /// </summary>
        public TextAlignment TimeHorizontalOption
        {
            get => _timePicker.HorizontalTextAlignment;
            set
            {
                _timePicker.HorizontalTextAlignment = value;
            }
        }
        /// <summary>
        /// transparent border set it befor control type
        /// </summary>
        public bool ControlWithoutBorder { get; set; }

        /// <summary>
        /// IsBackGroundTransparent
        /// </summary>
        public bool IsBackGroundTransparent
        {
            get; set;
        }

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
            }
        }

        /// <summary>
        /// Get/ set Control width to Date and time
        /// </summary>
        public bool ApplyDateWidthPicker
        {
            get
            {
                return _datePicker == null || !_datePicker.WidthRequest.Equals(-1);
            }
            set
            {
                if (value && _datePicker != null)
                {
                    _datePicker.WidthRequest = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, (double)0) * 0.5;
                }
            }
        }

        /// <summary>
        /// Get/ set Control width to Date and time
        /// </summary>
        public bool ApplyTimeWidthPicker
        {
            get
            {
                return !_timePicker.WidthRequest.Equals(-1);
            }
            set
            {
                if (value && _timePicker != null)
                {
                    _timePicker.WidthRequest = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, (double)0) * 0.45;
                }
            }
        }

        private void RenderControl(FieldTypes value)
        {
            switch (value)
            {
                case FieldTypes.DateControl:
                    _datePicker = CreateDatePicker();
                    var dateView = AddPancackeView(_datePicker);
                    _mainLayout.Add(dateView, 0, 1);
                    Grid.SetColumnSpan(dateView, 2);
                    break;
                case FieldTypes.TimeControl:
                    _timePicker = CreateTimePicker();
                    _timePicker.TimeChanged += TimePicker_TimeChanged;
                    var timeView = AddPancackeView(_timePicker);
                    _mainLayout.Add(timeView, 0, 1);
                    Grid.SetColumnSpan(timeView, 2);
                    break;
                default:
                    _mainLayout.ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    };
                    _mainLayout.Add(AddPancackeView(CreateTimePicker()), 1, 1);
                    _datePicker = CreateDatePicker();
                    _mainLayout.Add(AddPancackeView(_datePicker), 0, 1);
                    ////   Grid.SetColumnSpan(_errorLabel, 2);
                    _timePicker.TimeChanged += TimePicker_TimeChanged;
                    break;
            }
        }

        /// <summary>
        /// set and get Date 
        /// </summary>
        public DateTime? GetSetDate
        {
            get
            {
                if (_controlType == FieldTypes.DateControl && _datePicker != null)
                {
                    if (_datePicker.NullableDate == null)
                    {
                        return _datePicker.NullableDate;
                    }
                    else
                    {
                        return _datePicker.Date;
                    }
                }
                else
                {
                    if (_datePicker == null)
                    {
                        return new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, DateTime.Now.Date.Day, _timePicker.Time.Hours, _timePicker.Time.Minutes, 0);
                    }
                    else
                    {
                        if (_datePicker.NullableDate == null)
                        {
                            return null;
                        }
                        else
                        {
                            return new DateTime(_datePicker.Date.Year, _datePicker.Date.Month, _datePicker.Date.Day, _timePicker.Time.Hours, _timePicker.Time.Minutes, 0);
                        }
                    }
                }
            }
            set
            {
                if (value == null)
                {
                    _datePicker.NullableDate = null;
                }
                if (value != null && _datePicker != null)
                {
                    _datePicker.Date = value.Value;
                }
                if (_datePicker != null && value != null)
                {
                    _datePicker.NullableDate = _datePicker.Date;
                }

            }
        }
        /// <summary>
        /// set date format
        /// </summary>
        public string DateFormat
        {
            get { return _datePicker.Format; }
            set
            {
                if (_datePicker != null && !string.IsNullOrWhiteSpace(value))
                {
                    _datePicker.Format = value;
                }
            }
        }
        /// <summary>
        /// set time format
        /// </summary>
        public string TimeFormat
        {
            get { return _timePicker.Format; }
            set
            {
                if (_timePicker != null && !string.IsNullOrWhiteSpace(value))
                {
                    _timePicker.Format = value;
                }
            }
        }
        /// <summary>
        /// set and get Time 
        /// </summary>
        public TimeSpan SetTime
        {
            get => _previousTime = _timePicker.Time;
            set => _timePicker.Time = value;
        }

        /// <summary>
        /// set and get max time
        /// </summary>
        public TimeSpan MaxTime
        {
            get => _maxTime;
            set => _maxTime = value.Add(TimeSpan.FromMinutes(5));
        }

        private void TimePicker_TimeChanged(object sender, EventArgs e)
        {
            CustomTimePicker timerpIcker = (CustomTimePicker)sender;
            if (_maxTime != default)
            {
                if (_datePicker.Date + timerpIcker.Time > DateTime.Today.Date + _maxTime)
                {
                    _timePicker.Time = _previousTime;
                }
                else
                {
                    _previousTime = _timePicker.Time;
                }
            }
        }

        /// <summary>
        /// CustomDateTimeControl class constructor
        /// </summary>
        public CustomDateTimeControl()
        {
            _errorLabel = new CustomLabelControl(LabelType.ClientErrorLabel)
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                IsHtmlLabelLineCount = true,
                IsVisible = false
            };
            _mainLayout = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                ColumnSpacing = 10,
                Margin = new Thickness(0, 0, 0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture)),
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)}
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width =  GridLength.Auto},
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };
            AutomationProperties.SetIsInAccessibleTree(_mainLayout, true);
            IsUnderLine = true;
            _mainLayout.Add(_errorLabel, 0, 2);
            AutomationProperties.SetIsInAccessibleTree(_errorLabel, true);
            ShowHeader = true;
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
                RenderBorder(IsUnderLine);
            }
        }
        private Border AddPancackeView(View view)
        {
            _borderFrame = new Border
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_CONTROL_STYLE],
                Padding = new Thickness(GenericMethods.GetPlatformSpecificValue(15, App._essentials.GetPreferenceValue(StorageConstants.PR_IS_RIGHT_ALIGNED_KEY, false) ? 15 : 0, 0), 0, GenericMethods.GetPlatformSpecificValue(15, 0, 0), 0),
                HeightRequest = 50,
                Content = view,
            };
            if (ControlWithoutBorder)
            {
                _borderFrame.StrokeThickness = 0;
            }
            if (view is CustomDatePicker && IsUnderLine)
            {
                _borderFrame.HeightRequest = 35;
            }
            if (view is CustomTimePicker && IsUnderLine)
            {
                _borderFrame.HeightRequest = 35;
            }
            if (IsUnderLine)
            {
                _borderFrame.Padding = new Thickness(0);
                _borderFrame.StrokeThickness = 0;
            }

            return _borderFrame;
        }



        private CustomDatePicker CreateDatePicker()
        {
            _datePicker = new CustomDatePicker
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_DATE_PICKER_STYLE],
                NullableDate = null,
            };
            AutomationProperties.SetIsInAccessibleTree(_datePicker, true);
            _datePicker.DateSelectedEvent += DatePicker_DateSelectedEvent;
            return _datePicker;
        }

        private void DatePicker_DateSelectedEvent(object sender, EventArgs e)
        {
            CustomDatePicker item = sender as CustomDatePicker;
            _datePicker.NullableDate = item.NullableDate;
            ValidateControl(true);
            OnDateSelectedValueChanged?.Invoke(sender, e);
        }

        private CustomTimePicker CreateTimePicker()
        {
            _timePicker = new CustomTimePicker
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_TIME_PICKER_STYLE],
                Time = DateTime.Now.TimeOfDay,
                Format = GenericMethods.GetTimeFormat(),
                // CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator == ""
                //    ? LibConstants.DEFAULT_TIME_FORMAT
                //    : LibConstants.TIME_FORMAT,
                HorizontalTextAlignment = TextAlignment.End
            };
            AutomationProperties.SetIsInAccessibleTree(_timePicker, true);
            var value = string.IsNullOrWhiteSpace(_resourceData?.ResourceValue) ? nameof(_timePicker) : _resourceData?.ResourceValue;
            return _timePicker;
        }

        /// <summary>
        /// Method to Apply Resource Value
        /// </summary>
        protected override async void ApplyResourceValue()
        {
            var htmlPrefixText = await GetSettingsValueAsync().ConfigureAwait(true);

            _htmlPrefixText = string.Format(CultureInfo.InvariantCulture, htmlPrefixText, GetResourceValueByKey(ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY).Replace("{0}", _resourceData.ResourceValue));
            AutomationProperties.SetName(_mainLayout, _borderFrame.AutomationId);

            if (_controlType != FieldTypes.TimeControl)
            {
                _datePicker.Placeholder = this.IsEnabled && !string.IsNullOrWhiteSpace(_resourceData.PlaceHolderValue) ? _resourceData.PlaceHolderValue : string.Empty;

                /*   if (string.IsNullOrWhiteSpace(_datePicker.AutomationId))
                   {
                       _datePicker.AutomationId = $"{ShellMasterPage.CurrentShell.CurrentPage.GetType().Name}{_resourceData.ResourceValue}";
                   }
                   AutomationProperties.SetName(_datePicker, _datePicker.AutomationId);*/
                //ignore the case of min date
                _datePicker.MinimumDate = DateTime.Now.AddDays(_resourceData.MinLength);
                _datePicker.MaximumDate = DateTime.Now.AddDays(_resourceData.MaxLength);
            }
            FormattedString fs = new FormattedString();
            fs.Spans.Add(new Span { Text = _resourceData.ResourceValue });
            if (_resourceData.IsRequired && this.IsEnabled)
            {
                fs.Spans.Add(new Span { TextColor = (Color)Application.Current.Resources[StyleConstants.ST_ACCENT_COLOR], Text = Constants.IS_REQUIRED_FEILD_INDICATOR });
            }
            _headerLabel.FormattedText = fs;

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
        /// validate date time Control 
        /// </summary>    
        /// <param name="isButtonClick">validation fired after button is clicked</param>  
        public override void ValidateControl(bool isButtonClick)
        {
            if (_resourceData.IsRequired)
            {
                if (_controlType != FieldTypes.TimeControl && _datePicker?.NullableDate == null)
                {
                    _errorLabel.Text = _htmlPrefixText;
                    _errorLabel.IsVisible = true;
                    _errorLabel.HeightRequest = IsApplyHeightToError ? _errorLabelHeight : _errorLabel.GetLineCount * _errorLabelHeight;
                    IsValid = false;
                }
                else
                {
                    _errorLabel.Text = string.Empty;
                    _errorLabel.IsVisible = false;
                    _errorLabel.HeightRequest = 0;
                    IsValid = true;
                }
            }
            else
            {
                IsValid = true;
            }
            ShowErrorLabel();
            if (_datePicker != null)
            {
                _datePicker.BorderColor = _errorLabel.IsVisible ? (Color)Application.Current.Resources[StyleConstants.ST_ERROR_COLOR] : (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE];
                if (_borderFrame != null)
                {
                    _borderFrame.Stroke = _datePicker.BorderColor;
                }
            }
        }

        private void ShowErrorLabel()
        {
            if (_resourceData.MaxLength > 0 && _controlType == FieldTypes.DateTimeControl 
                && _datePicker.Date + _timePicker.Time > DateTime.Now.AddDays(_resourceData.MaxLength).AddMinutes(5))
            {
                _errorLabel.Text = string.Format(CultureInfo.CurrentCulture, GetResourceValueByKey(ResourceConstants.R_INVALID_DATA_KEY), _resourceData.ResourceValue);
                _errorLabel.IsVisible = true;
                _errorLabel.HeightRequest = -1;
                IsValid = false;
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
        /// Method to Enable value
        /// </summary>
        /// <param name="value"></param>
        protected override void EnabledValue(bool value)
        {
            switch (_controlType)
            {
                case FieldTypes.DateControl:
                    _datePicker.IsEnabled = value;
                    break;
                case FieldTypes.TimeControl:
                    _timePicker.IsEnabled = value;
                    break;
                default:
                    _datePicker.IsEnabled = value;
                    _timePicker.IsEnabled = value;
                    break;
            }
        }

        /// <summary>
        /// Method to set rendere value
        /// </summary>
        /// <param name="value">border thikness</param>
        protected override void RenderBorder(bool value)
        {
            if (_timePicker != null)
            {
                CreateBorder(value);
            }
            if (_datePicker != null)
            {
                CreateBorder(value);
            }
            if (_datePicker != null && _timePicker != null)
            {
                CreateBorder(value);
            }
        }

        private void CreateBorder(bool value)
        {
            if (_borderFrame != null)
            {
                foreach (var child in _mainLayout?.Children)
                {
                    if (child is Border)
                    {
                        (child as Border).BackgroundColor = IsBackGroundTransparent ? Colors.Transparent : (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                    }
                }
            }
            CreatedBoderOnControlType(value);
        }

        private void CreatedBoderOnControlType(bool value)
        {
            switch (_controlType)
            {
                case FieldTypes.DateControl:
                    DateBoder(value);
                    break;
                case FieldTypes.TimeControl:
                    TimeBoder(value);
                    break;
                default:
                    DefaultCondition(value);
                    break;
            }
        }

        private void TimeBoder(bool value)
        {
            _timePicker.HasUnderline = value;
            _timePicker.BackgroundColor = MobileConstants.IsAndroidPlatform ? GetBackgouondColorForAndroid() : GetBackGroundColorForIos();
            _timePicker.BorderColor = value ? (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE] : Colors.Transparent;
        }

        private void DateBoder(bool value)
        {
            _datePicker.HasUnderline = value;
            _datePicker.BackgroundColor = MobileConstants.IsAndroidPlatform ? GetBackgouondColorForAndroid() : GetBackGroundColorForIos();
            _datePicker.BorderColor = value ? (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE] : Colors.Transparent;
        }

        private Color GetBackGroundColorForIos()
        {
            return IsBackGroundTransparent ? Colors.Transparent : (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
        }

        private Color GetBackgouondColorForAndroid()
        {
            return IsBackGroundTransparent ? (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR] : (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
        }

        private void DefaultCondition(bool value)
        {
            _datePicker.HasUnderline = value;
            _timePicker.HasUnderline = value;
            if (MobileConstants.IsAndroidPlatform)
            {
                _datePicker.BackgroundColor = IsBackGroundTransparent ? (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR] : (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                _timePicker.BackgroundColor = IsBackGroundTransparent ? (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR] : (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
            }
            else
            {
                _datePicker.BackgroundColor = IsBackGroundTransparent ? Colors.Transparent : (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                _timePicker.BackgroundColor = IsBackGroundTransparent ? Colors.Transparent : (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
            }
            _datePicker.BorderColor = value ? (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE] : Colors.Transparent;
            _timePicker.BorderColor = value ? (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE] : Colors.Transparent;
        }
    }
}
