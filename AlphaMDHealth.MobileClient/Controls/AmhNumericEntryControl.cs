using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Editors;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

internal class AmhNumericEntryControl : AmhBaseControl
{
    internal NumericEdit _numericEntry;

    private decimal? _value;
    /// <summary>
    /// Control value as string
    /// </summary>
    internal decimal? Value
    {
        get
        {
            return GetControlValue();
        }
        set
        {
            if (_value != value && value != 0)
            {
                _value = value;
                SetControlValue();
            }
        }
    }

    /// <summary>
    /// value property
    /// </summary>
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(decimal?), typeof(AmhNumericEntryControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhNumericEntryControl control = (AmhNumericEntryControl)bindable;
        if (newValue != null)
        {
            control.Value = (decimal?)newValue;
        }
    }

    /// <summary>
    /// set digits after decimal to decimal control
    /// </summary>
    public int DigitsAfterDecimal { get; set; } = Constants.NUMBER_TWO_VALUE;

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhNumericEntryControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    public AmhNumericEntryControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    /// <summary>
    /// Get value of control
    /// </summary>
    /// <returns>value of control</returns>
    private decimal? GetControlValue()
    {
        return _numericEntry.Value;
    }

    /// <summary>
    /// Set value to control
    /// </summary>
    private void SetControlValue()
    {
        switch (_fieldType)
        {
            case FieldTypes.DecimalEntryControl:
                _numericEntry.Value = _value != null ? Convert.ToDecimal(_value) : default;
                break;
            case FieldTypes.CounterEntryControl:
                _numericEntry.Value = _value != null ? Convert.ToInt32(_value) : default;
                break;
            default:
                _numericEntry.Value = _value != null && _value.ToString().Length<19 ? Convert.ToInt64(_value) : default;
                break;
        }
    }

    private void NumericEntry_ValueChanged(object sender, EventArgs e)
    {
        OnValueChangedAction(sender, e);
    }

    /// <summary>
    /// Method implementation to Enable Disable control
    /// </summary>
    /// <param name="value">flag representing control needs to enable or disable</param>
    protected override void EnabledDisableField(bool value)
    {
        SetFieldIsEnabled(_numericEntry, value);
    }

    /// <summary>
    /// Render Control based on Specified type
    /// </summary>
    protected override void RenderControl()
    {
        if (_numericEntry != null)
        {
            _numericEntry.ValueChanged -= NumericEntry_ValueChanged;
            _numericEntry = null;
        }
        _numericEntry = _fieldType switch
        {
            FieldTypes.DecimalEntryControl => new NumericEdit
            {
                MaxDecimalDigitCount = DigitsAfterDecimal,
            },
            FieldTypes.CounterEntryControl => new NumericEdit
            {
                IsUpDownIconVisible = true,
                UpDownIconAlignment = UpDownIconAlignment.Both,
                //StepValue=2
            },
            _ => new NumericEdit
            {
                MaxDecimalDigitCount = 0,
            },
        };
        _numericEntry.Style = (Style)Application.Current.Resources[StyleConstants.ST_NUMERIC_MY_ENTRY_STYLE_KEY];
        _numericEntry.ClearIconVisibility = DevExpress.Maui.Core.IconVisibility.Never;
        _numericEntry.AllowLooping = false;
        _numericEntry.ValueChanged += NumericEntry_ValueChanged;
        Content = _numericEntry;
    }

    /// <summary>
    /// Apply's Resource Value Method implementation
    /// </summary>
    protected override void ApplyResourceValue()
    {
        if (_numericEntry != null)
        {
            SetIcon(_numericEntry);
            ApplyResource(_numericEntry);
            if (IsControlEnabled && _resource != null)
            {
                _numericEntry.MaxValue = (decimal)_resource.MaxLength;
                _numericEntry.MinValue = (decimal)_resource.MinLength;
            }
        }
    }

    /// <summary>
    /// validate the control
    /// </summary>
    /// <param name="isButtonClick"></param>
    internal override void ValidateControl(bool isButtonClick)
    {
        if (_numericEntry != null)
        {
            SetFieldError(_numericEntry, false, string.Empty);
            if (IsControlEnabled && _resource != null)
            {
                if (_resource.IsRequired && !_numericEntry.Value.HasValue)
                {
                    SetFieldError(_numericEntry, true, GetRequiredResourceValue());
                }
                else if (_numericEntry.Value.HasValue)
                {
                    if (_resource.MinLength >= 0
                        && _numericEntry.Value < (decimal)_resource.MinLength)
                    {
                        SetFieldError(_numericEntry, true, string.Format(CultureInfo.CurrentCulture
                            , LibResources.GetResourceValueByKey(PageResources.Resources, ResourceConstants.R_MINIMUM_LENGTH_VALIDATION_KEY)
                            , _resource.MinLength));

                    }
                    else if (!_resource.MinLength.Equals(_resource.MaxLength)
                        && _resource.MinLength < _resource.MaxLength
                        && _numericEntry.Value > (decimal)_resource.MaxLength)
                    {
                        SetFieldError(_numericEntry, true, string.Format(CultureInfo.CurrentCulture
                            , LibResources.GetResourceValueByKey(PageResources.Resources, ResourceConstants.R_RANGE_LENGTH_VALIDATION_KEY)
                            , _resource.MinLength, _resource.MaxLength));
                    }
                }
            }
            IsValid = !_numericEntry.HasError;
        }
        if (!_isButtonClick)
        {
            _isButtonClick = isButtonClick;
        }
    }
}