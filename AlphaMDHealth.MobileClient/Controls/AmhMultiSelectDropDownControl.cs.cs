using AlphaMDHealth.Utility;
using DevExpress.Maui.Editors;

namespace AlphaMDHealth.MobileClient;

internal class AmhMultiSelectDropDownControl : AmhBaseControl
{
    private TokenEdit _multiSelectDropDown;

    private string _value;
    /// <summary>
    /// Control value as string
    /// </summary>
    internal string Value
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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(AmhMultiSelectDropDownControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhMultiSelectDropDownControl control = (AmhMultiSelectDropDownControl)bindable;
        if (newValue != null)
        {
            control.Value = (string)newValue;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhMultiSelectDropDownControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhMultiSelectDropDownControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    private string GetControlValue()
    {
        _value = string.Empty;
        if (_multiSelectDropDown.SelectedItems?.Count > 0)
        {
            _value = string.Join(Constants.PIPE_SEPERATOR,
                _options?.Where(item => _multiSelectDropDown.SelectedItems.Contains(item.OptionText))?.Select(x => x.OptionID));
        }
        return _value;
    }

    private void SetControlValue()
    {
        var selectedIds = FetchOptionIDsFromValue(_value);
        _multiSelectDropDown.SelectedItems = _options?.Where(item => selectedIds.Contains(item.OptionID))?.Select(x => x.OptionText)?.ToList();
    }

    private void OnSelectionChanged(object sender, EventArgs e)
    {
        _multiSelectDropDown.Focus();
        OnValueChangedAction(sender, e);
        //ValidateSelection();
    }

    protected override void EnabledDisableField(bool value)
    {
        SetFieldIsEnabled(_multiSelectDropDown, value);
        if (!value)
        {
            _multiSelectDropDown.IsReadOnly = true;
        }
    }

    protected override void RenderControl()
    {
        if (_multiSelectDropDown == null)
        {
            _multiSelectDropDown = new TokenEdit();
        }
        else
        {
            _multiSelectDropDown.SelectionChanged += OnSelectionChanged;
        }
        var style = _styleName;
        if (string.IsNullOrWhiteSpace(style))
        {
            style = _fieldType switch
            {
                FieldTypes.MultiSelectEditableDropdownControl => StyleConstants.ST_MULTI_SELECT_EDITABLE_DROPDOWN_STYLE,
                //default: FieldTypes.MultiSelectDropdownControl
                _ => StyleConstants.ST_MULTI_SELECT_DROPDOWN_STYLE,
            };
        }
        _multiSelectDropDown.Style = (Style)Application.Current.Resources[style];
        _multiSelectDropDown.SelectionChanged += OnSelectionChanged;
        Content = _multiSelectDropDown;
    }

    protected override void ApplyResourceValue()
    {
        if (_multiSelectDropDown != null)
        {
            SetIcon(_multiSelectDropDown);
            ApplyResource(_multiSelectDropDown);
        }

        _multiSelectDropDown.NoResultsFoundText = _resource.GroupDesc;
    }

    protected override void ApplyOptions()
    {
        _multiSelectDropDown.ItemsSource = _options.Select(item => item.OptionText).ToList();
    }

    internal override void ValidateControl(bool isButtonClick)
    {
        if (_multiSelectDropDown != null)
        {
            SetFieldError(_multiSelectDropDown, false, string.Empty);
            if (IsControlEnabled)
            {
                var val = (string)GetControlValue();
                if (_resource.IsRequired && string.IsNullOrWhiteSpace(val))
                {
                    SetFieldError(_multiSelectDropDown, true, GetRequiredResourceValue());
                }
                else if (!string.IsNullOrWhiteSpace(val))
                {
                    int count = _multiSelectDropDown.SelectedItems.Count;
                    if (_resource.MinLength > 0 && count < _resource.MinLength)
                    {
                        SetFieldError(_multiSelectDropDown, true, $"Atleast {_resource.MinLength} options should be selected");
                    }
                    else if (_resource.MaxLength > 0 && count > _resource.MaxLength)
                    {
                        SetFieldError(_multiSelectDropDown, true, $"Cannot select more than {_resource.MaxLength} options");
                    }
                }
            }
            IsValid = !_multiSelectDropDown.HasError;
        }
        if (!_isButtonClick)
        {
            _isButtonClick = isButtonClick;
        }
    }

    //private void ValidateSelection()
    //{
    //    if (_multiSelectDropDown.SelectedIndexes != null)
    //    {
    //        if (_multiSelectDropDown.SelectedIndexes.Count > 3)
    //        {
    //            _multiSelectDropDown.IsEnabled = false;
    //            _multiSelectDropDown.IsDropDownIconVisible = false;
    //        }
    //        else
    //        {
    //            _multiSelectDropDown.IsEnabled = true;
    //            _multiSelectDropDown.IsDropDownIconVisible = true;
    //        }
    //    }
    //}
}
