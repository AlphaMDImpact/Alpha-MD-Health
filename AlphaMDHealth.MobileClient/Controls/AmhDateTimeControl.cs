using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Editors;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

internal class AmhDateTimeControl : AmhBaseControl
{
    private string _format;
    private DateTimeOffset _minDate => _resource != null ? DateTimeOffset.Now.AddDays(_resource.MinLength) : DateTimeOffset.MinValue;
    private DateTimeOffset _maxDate => _resource != null ? DateTimeOffset.Now.AddDays(_resource.MaxLength) : DateTimeOffset.MaxValue;
    private string dayFormat, monthFormat, yearFormat;

    private DateEdit _dateEntry;
    private TimeEdit _timeEntry;

    private DateTimeOffset? _value;
    /// <summary>
    /// Control value as string
    /// </summary>
    internal DateTimeOffset? Value
    {
        get
        {
            return GetControlValue();
        }
        set
        {
            if (_value != value)
            {
                _value = value;
                SetControlValue();
            }
        }
    }

    /// <summary>
    /// value property
    /// </summary>
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(DateTimeOffset?), typeof(AmhDateTimeControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhDateTimeControl control = (AmhDateTimeControl)bindable;
        if (newValue != null)
        {
            control.Value = newValue as DateTimeOffset?;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhDateTimeControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhDateTimeControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    /// <summary>
    /// Get value of control
    /// </summary>
    /// <returns>value of control</returns>
    private DateTimeOffset? GetControlValue()
    {
        DateTimeOffset? dt;
        var currentDT = App._essentials.ConvertToLocalTime(DateTimeOffset.UtcNow);
        switch (_fieldType)
        {
            case FieldTypes.TimeControl:
                dt = _timeEntry?.Time != null ? new DateTimeOffset(_timeEntry.Time.Value, currentDT.Offset) : null;
                break;
            case FieldTypes.DateControl:
                dt = _dateEntry?.Date != null ? new DateTimeOffset(_dateEntry.Date.Value, currentDT.Offset) : null;
                break;
            default:
                dt = _dateEntry?.Date ?? _timeEntry?.Time;
                if ((_dateEntry?.Date.HasValue ?? false) || (_timeEntry?.Time.HasValue ?? false))
                {
                    dt = new DateTimeOffset(_dateEntry?.Date?.Year ?? 0, _dateEntry?.Date?.Month ?? 0, _dateEntry?.Date?.Day ?? 0
                        , _timeEntry?.Time?.Hour ?? 0, _timeEntry?.Time?.Minute ?? 0, _timeEntry?.Time?.Second ?? 0, currentDT.Offset);
                }
                break;
        }
        return dt;
    }

    /// <summary>
    /// Set value to control
    /// </summary>
    private void SetControlValue()
    {
        DateTimeOffset? dt = _value;
        switch (_fieldType)
        {
            case FieldTypes.TimeControl:
                if (_timeEntry != null)
                {
                    _timeEntry.Time = dt?.DateTime;
                }
                break;
            case FieldTypes.DateControl:
                if (_dateEntry != null)
                {
                    _dateEntry.Date = dt?.DateTime;
                }
                break;
            default:
                if (_dateEntry != null)
                {
                    _dateEntry.Date = dt?.Date;
                }
                if (_timeEntry != null)
                {
                    _timeEntry.Time = dt?.DateTime;
                }
                break;
        }
    }

    private void OnTimeChanged(object sender, EventArgs e)
    {
        OnValueChangedAction(sender, e);
    }

    private void OnDateChanged(object sender, EventArgs e)
    {
        OnValueChangedAction(sender, e);
    }

    private void CreateDateEntry()
    {
        _dateEntry = new DateEdit
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_DATE_STYLE],
            //CornerMode = CornerMode.Round,
            BoxMode = BoxMode.Outlined
        };
        _dateEntry.DateChanged += OnDateChanged;
    }

    private void CreateTimeEntry()
    {
        _timeEntry = new TimeEdit
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_TIME_STYLE],
            TimeFormatMode = App._essentials.GetPreferenceValue(StorageConstants.PR_IS_24_HOUR_FORMAT, false)
                ? TimeFormatMode.HourFormat24
                : TimeFormatMode.HourFormat12,
            //CornerMode = CornerMode.Round
        };
        _timeEntry.TimeChanged += OnTimeChanged;
    }

    /// <summary>
    /// Render Control baseed on Specified type
    /// </summary>
    protected override void RenderControl()
    {
        if (_dateEntry != null)
        {
            _dateEntry.DateChanged -= OnDateChanged;
            _dateEntry = null;
        }
        if (_timeEntry != null)
        {
            _timeEntry.TimeChanged -= OnTimeChanged;
            _timeEntry = null;
        }
        switch (_fieldType)
        {
            case FieldTypes.DateControl:
                CreateDateEntry();
                Content = _dateEntry;
                break;
            case FieldTypes.TimeControl:
                CreateTimeEntry();
                Content = _timeEntry;
                break;
            default:
                CreateDateEntry();
                CreateTimeEntry();
                var grid = new Grid()
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                    }
                };
                grid.Add(_dateEntry, 0, 0);
                grid.Add(_timeEntry, 2, 1);
                Content = grid;
                _timeEntry.VerticalOptions = LayoutOptions.StartAndExpand;
                break;
        }
    }

    #region ApplyResource

    /// <summary>
    /// Apply's Resource Value Method implementation
    /// </summary>
    protected override void ApplyResourceValue()
    {
        GetDateTimeFormat();
        switch (_fieldType)
        {
            case FieldTypes.TimeControl:
                RenderTimeResource();
                break;
            case FieldTypes.DateControl:
                RenderDateResource();
                break;
            default:
                RenderDateResource();
                //RenderTimeResource();
                if (_timeEntry != null)
                {
                    _timeEntry.LabelText = " ";
                    _timeEntry.PlaceholderText = GetPlaceholderText(_format);
                }
                break;
        }
    }

    private void RenderDateResource()
    {
        if (_dateEntry != null)
        {
            _dateEntry.DisplayFormat = _format;
            ApplyResource(_dateEntry);
            _dateEntry.MinDate = _minDate.DateTime;
            _dateEntry.MaxDate = _maxDate.DateTime;
        }
    }

    private void RenderTimeResource()
    {
        if (_timeEntry != null)
        {
            ApplyResource(_timeEntry);
        }
    }

    private void GetDateTimeFormat()
    {
        LibSettings.TryGetDateFormatSettings(_pageResources?.Settings, out dayFormat, out monthFormat, out yearFormat);
        _format = FieldType switch
        {
            FieldTypes.DateControl => GenericMethods.GetDateTimeFormat(DateTimeType.Date, dayFormat, monthFormat, yearFormat),
            FieldTypes.TimeControl => GenericMethods.GetDateTimeFormat(DateTimeType.Time, dayFormat, monthFormat, yearFormat),
            _ => GenericMethods.GetDateTimeFormat(DateTimeType.DateTime, dayFormat, monthFormat, yearFormat),
        };
    }
    #endregion

    #region EnabledDisable

    /// <summary>
    /// Method to Enable Disable field
    /// </summary>
    /// <param name="value">flag representing control needs to enable or disable</param>
    protected override void EnabledDisableField(bool value)
    {
        switch (_fieldType)
        {
            case FieldTypes.DateControl:
                SetFieldIsEnabled(_dateEntry, value);
                break;
            case FieldTypes.TimeControl:
                SetFieldIsEnabled(_timeEntry, value);
                break;
            default:
                SetFieldIsEnabled(_timeEntry, value);
                SetFieldIsEnabled(_dateEntry, value);
                break;
        }
    }

    #endregion

    #region ValidateControl

    internal override void ValidateControl(bool isButtonClick)
    {
        switch (_fieldType)
        {
            case FieldTypes.DateControl:
                ValidateDateField(isButtonClick);
                break;
            case FieldTypes.TimeControl:
                ValidateTimeField(isButtonClick);
                break;
            default:
                ValidateDateTimeField(isButtonClick);
                break;
        }
    }

    private void ValidateTimeField(bool isButtonClick)
    {
        if (_timeEntry != null)
        {
            SetFieldError(_timeEntry, false, string.Empty);
            if (IsControlEnabled && _resource != null)
            {
                if (Validator.HasRequiredValidationError(_resource, _timeEntry.Time.HasValue))
                {
                    SetFieldError(_timeEntry, true, GetRequiredResourceValue());
                }
                if (_timeEntry.Time.HasValue && (_timeEntry.Time.Value.Hour < _resource.MinLength || _timeEntry.Time.Value.Hour > _resource.MaxLength))
                {
                    SetFieldError(_timeEntry, true, string.Format(CultureInfo.CurrentCulture
                        , "Time should be between {0} to {1}"
                        , GenericMethods.GetDateTimeBasedOnFormatString(_minDate, _format)
                        , GenericMethods.GetDateTimeBasedOnFormatString(_maxDate, _format)));
                }
            }
        }
        SetValidationResult(isButtonClick, _timeEntry.ErrorText);
    }

    private void ValidateDateField(bool isButtonClick)
    {
        if (_dateEntry != null)
        {
            SetFieldError(_dateEntry, false, string.Empty);
            if (IsControlEnabled && _resource != null)
            {
                if (_resource.IsRequired && !_dateEntry.Date.HasValue)
                {
                    SetFieldError(_dateEntry, true, GetRequiredResourceValue());
                }
                else if (_dateEntry.Date.HasValue && (_dateEntry.Date.Value < _dateEntry.MinDate || _dateEntry.Date.Value > _dateEntry.MaxDate))
                {
                    SetFieldError(_dateEntry, true, string.Format(CultureInfo.CurrentCulture
                        , LibResources.GetResourceValueByKey(_pageResources.Resources, ResourceConstants.R_RANGE_VALUE_VALIDATION_KEY)
                        , GenericMethods.GetDateTimeBasedOnFormatString(_minDate.Date, _format)
                        , GenericMethods.GetDateTimeBasedOnFormatString(_maxDate.Date, _format))
                    );
                }
            }
        }
        SetValidationResult(isButtonClick, _dateEntry.ErrorText);
    }

    private void ValidateDateTimeField(bool isButtonClick)
    {
        DateTimeOffset? dt = Value;
        if (_dateEntry != null)
        {
            SetFieldError(_dateEntry, false, string.Empty);
            SetFieldError(_timeEntry, false, string.Empty);
            if (IsControlEnabled && _resource != null)
            {
                if (Validator.HasRequiredValidationError(_resource, dt.HasValue))
                {
                    SetFieldError(_dateEntry, true, GetRequiredResourceValue());
                }
                else if (dt.HasValue)
                {
                    var currentDT = App._essentials.ConvertToLocalTime(DateTimeOffset.UtcNow);
                    var minDateTime = new DateTimeOffset(_dateEntry.MinDate.Year, _dateEntry.MinDate.Month, _dateEntry.MinDate.Day, currentDT.Hour, currentDT.Minute, currentDT.Second, currentDT.Offset);
                    var maxDateTime = new DateTimeOffset(_dateEntry.MaxDate.Year, _dateEntry.MaxDate.Month, _dateEntry.MaxDate.Day, currentDT.Hour, currentDT.Minute, currentDT.Second, currentDT.Offset);
                    if (dt.Value < minDateTime || dt.Value > maxDateTime)
                    {
                        SetFieldError(_dateEntry, true, string.Format(CultureInfo.CurrentCulture
                            , LibResources.GetResourceValueByKey(_pageResources.Resources, ResourceConstants.R_RANGE_VALUE_VALIDATION_KEY)
                            , GenericMethods.GetDateTimeBasedOnCulture(minDateTime, DateTimeType.DateTime, dayFormat, monthFormat, yearFormat)
                            , GenericMethods.GetDateTimeBasedOnCulture(maxDateTime, DateTimeType.DateTime, dayFormat, monthFormat, yearFormat))
                        );
                    }
                }
            }
        }
        SetValidationResult(isButtonClick, _dateEntry.ErrorText);
    }

    //public override bool ValidateControl(bool isButtonClick)
    //{
    //    ErrorMessage = string.Empty;
    //    if (IsControlEnabled && _resource != null)
    //    {
    //        if (Validator.HasRequiredValidationError(_resource, Value != null))
    //        {
    //            ErrorMessage = GetRequiredResourceValue();
    //        }
    //        else if (Value != null)
    //        {
    //            ValidateDateTimeRange();
    //        }
    //    }
    //    SetValidationResult(isButtonClick);
    //    return IsValid;
    //}

    //private void ValidateDateTimeRange()
    //{
    //    switch (FieldType)
    //    {
    //        case FieldTypes.TimeControl:
    //            if (_time.HasValue && (_time?.Hours < _resource.MinLength || _time?.Hours > _resource.MaxLength))
    //            {
    //                ErrorMessage = string.Format("Time should be between {0} to {1}"
    //                    , GenericMethods.GetDateTimeBasedOnFormatString(_minDate, _format)
    //                    , GenericMethods.GetDateTimeBasedOnFormatString(_maxDate, _format));
    //            }
    //            break;
    //        case FieldTypes.DateControl:
    //            if (_dateTime.HasValue && (_dateTime?.Date < _minDate.Date || _dateTime?.Date > _maxDate.Date))
    //            {
    //                ErrorMessage = string.Format(CultureInfo.CurrentCulture
    //                    , GetResourceValue(ResourceConstants.R_RANGE_VALUE_VALIDATION_KEY)
    //                    , GenericMethods.GetDateTimeBasedOnFormatString(_minDate.Date, _format)
    //                    , GenericMethods.GetDateTimeBasedOnFormatString(_maxDate.Date, _format));
    //            }
    //            break;
    //        case FieldTypes.DateTimeControl:
    //            if (_dateTime.HasValue)
    //            {
    //                // var currentDateTime = DateTime.Now;
    //                // var _minDateTime = new DateTime(_minDate.Year, _minDate.Month, _minDate.Day, currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second);
    //                // var _maxDateTime = new DateTime(_maxDate.Year, _maxDate.Month, _maxDate.Day, currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second);
    //                if (_dateTime < _minDate.DateTime || _dateTime > _maxDate.DateTime)
    //                {
    //                    ErrorMessage = string.Format(CultureInfo.CurrentCulture
    //                        , LibResources.GetResourceValueByKey(PageResources, ResourceConstants.R_RANGE_VALUE_VALIDATION_KEY)
    //                        , GenericMethods.GetDateTimeBasedOnFormatString(_minDate, _format)
    //                        , GenericMethods.GetDateTimeBasedOnFormatString(_maxDate, _format));
    //                }
    //            }
    //            break;
    //    }
    //}

    #endregion
}