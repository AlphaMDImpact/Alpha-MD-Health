using AlphaMDHealth.Utility;
using DevExpress.Maui.Core.Internal;
using DevExpress.Maui.Editors;

namespace AlphaMDHealth.MobileClient;

internal class AmhSingleSelectDropDownControl : AmhBaseControl
{
    private ComboBoxEdit _dropDown;

    private long _value;
    /// <summary>
    /// Control value as string
    /// </summary>
    internal long Value
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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(long), typeof(AmhSingleSelectDropDownControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhSingleSelectDropDownControl control = (AmhSingleSelectDropDownControl)bindable;
        if (newValue != null)
        {
            control.Value = (long)newValue;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhSingleSelectDropDownControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhSingleSelectDropDownControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    private long GetControlValue()
    {
        _value = default;
        if (_dropDown.SelectedItem != null && !string.IsNullOrWhiteSpace((string)_dropDown.SelectedItem))
        {
            _value = _options?.FirstOrDefault(item => item.OptionText == (string)_dropDown.SelectedItem)?.OptionID ?? default;
        }
        return _value;
    }

    private void SetControlValue()
    {
        _dropDown.SelectedItem = _options?.FirstOrDefault(item => item.OptionID == _value)?.OptionText ?? default;
    }

    private void OnSelectionChanged(object sender, EventArgs e)
    {
        OnValueChangedAction(sender, e);
    }

    protected override void EnabledDisableField(bool value)
    {
        SetFieldIsEnabled(_dropDown, value);

        if (!value)
        {
            _dropDown.IsReadOnly = true;
        }
    }

    protected override void RenderControl()
    {
        if (_dropDown == null)
        {
            _dropDown = new ComboBoxEdit();
        }
        else
        {
            _dropDown.SelectionChanged -= OnSelectionChanged;
        }
        var style = _styleName;
        if (string.IsNullOrWhiteSpace(style))
        {
            style = _fieldType switch
            {
                FieldTypes.SingleSelectEditableDropdownControl => StyleConstants.ST_SINGLE_SELECT_EDITABLE_DROPDOWN_STYLE,
                FieldTypes.SingleSelectWithoutBorderDropDownControl => StyleConstants.ST_SINGLE_SELECT_WITHOUT_BORDER_DROPDOWN_STYLE,
                _ => StyleConstants.ST_SINGLE_SELECT_DROPDOWN_STYLE,
            };
        }
        _dropDown.Style = (Style)Application.Current.Resources[style];
        _dropDown.SelectionChanged += OnSelectionChanged;

        Content = _dropDown;
    }

    protected override void ApplyResourceValue()
    {
        if (_dropDown != null)
        {
            SetIcon(_dropDown);
            ApplyResource(_dropDown);
        }

        _dropDown.NoResultsFoundText = _resource.GroupDesc;
    }

    protected override void ApplyOptions()
    {
        _dropDown.ItemsSource = _options.Select(item => item.OptionText).ToList();
    }

    internal override void ValidateControl(bool isButtonClick)
    {
        if (_dropDown != null)
        {
            SetFieldError(_dropDown, false, string.Empty);
            if (IsControlEnabled)
            {
                GetControlValue();
                if (_resource.IsRequired && (_value == default))
                {
                    SetFieldError(_dropDown, true, GetRequiredResourceValue());
                }
            }
            IsValid = !_dropDown.HasError;
        }
        if (!_isButtonClick)
        {
            _isButtonClick = isButtonClick;
        }
    }
}