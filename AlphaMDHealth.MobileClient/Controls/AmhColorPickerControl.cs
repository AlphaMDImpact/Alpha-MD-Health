using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Maui.ColorPicker;
using System.Text.RegularExpressions;

namespace AlphaMDHealth.MobileClient;

internal class AmhColorPickerControl : AmhBaseControl
{
    ColorPicker _colorPicker;
    AmhEntryControl _colorPickerEntry;
    StackLayout _colorPickerContainer;

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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(AmhColorPickerControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhColorPickerControl control = (AmhColorPickerControl)bindable;
        if (newValue != null)
        {
            control.Value = newValue as string;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhColorPickerControl() : this(FieldTypes.Default) 
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhColorPickerControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    private string GetControlValue()
    {
        return _colorPickerEntry.Value;
    }

    private void SetControlValue()
    {
        _colorPickerEntry.Value = _value;
    }

    private void ColorPicker_PickedColorChanged(object sender, Color colorPicked)
    {
        _colorPickerEntry.Value = colorPicked.ToHex();
        _colorPickerEntry._entry.EndIconColor = colorPicked;
    }

    private void OnColorPickerEntry_EndIconClicked(object sender, EventArgs e)
    {
        if (!_colorPickerContainer.Contains(_colorPicker))
        {
            _colorPickerContainer.Children.Add(_colorPicker);
        }
        else
        {
            _colorPickerContainer.Children.Remove(_colorPicker);
        }
    }

    private void OnTextChanged(object sender, EventArgs e)
    {
        OnValueChangedAction(sender, e);
        if (_colorPickerEntry.Value == null || string.IsNullOrWhiteSpace(_colorPickerEntry.Value as string))
        {
            _colorPickerEntry._entry.EndIconColor = Color.FromArgb($"#{Value}");//default Color
        }
    }

    protected override void EnabledDisableField(bool value)
    {
        SetFieldIsEnabled(_colorPickerEntry, value);
    }

    protected override void RenderControl()
    {
        if (_colorPickerContainer == null)
        {
            _colorPickerContainer = new StackLayout()
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_END_TO_END_LAYOUT_KEY],
            };

            _colorPickerEntry = new AmhEntryControl(FieldTypes.TextEntryControl)
            {
                Icon = $"|{ImageConstants.I_COLOR_PICKER_ICON_PNG}"
            };

            _colorPicker = new ColorPicker
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_COLOR_PICKER_STYLE],
                HeightRequest = 200,//_colorPickerEntry.HeightRequest,
                WidthRequest = _colorPickerEntry.WidthRequest,
            };
        }
        else
        {
            _colorPickerEntry.OnValueChanged -= OnTextChanged;
            _colorPickerEntry._entry.EndIconClicked -= OnColorPickerEntry_EndIconClicked;
            _colorPicker.PickedColorChanged -= ColorPicker_PickedColorChanged;
            _colorPickerContainer.Children.Clear();
        }

        _colorPickerEntry.ResourceKey = ResourceKey;
        _colorPickerEntry.Value = StyleConstants.DEFAULT_COLOR;
        //_colorPickerEntry._entry.EndIcon = ImageSource.FromFile(ImageConstants.I_COLOR_PICKER_ICON_PNG);

        _colorPickerEntry.OnValueChanged += OnTextChanged;
        _colorPickerEntry._entry.EndIconClicked += OnColorPickerEntry_EndIconClicked;
        _colorPicker.PickedColorChanged += ColorPicker_PickedColorChanged;

        _colorPickerContainer.Children.Add(_colorPickerEntry);
        Content = _colorPickerContainer;
    }

    protected override void ApplyResourceValue()
    {
        if (_colorPickerEntry != null)
        {
            _colorPickerEntry.ResourceKey = ResourceKey;
            _colorPickerEntry.PageResources = PageResources;
            //ApplyResource(_colorPickerEntry._entry);
        }
    }

    internal override void ValidateControl(bool isButtonClick)
    {
        if (_colorPickerEntry != null)
        {
            SetFieldError(_colorPickerEntry._entry, false, string.Empty);
            if (IsControlEnabled)
            {
                if (_resource != null && Validator.HasRequiredValidationError(_resource, !string.IsNullOrWhiteSpace(_colorPickerEntry.Value)))
                {
                    _colorPickerEntry._entry.EndIconColor = Color.FromArgb($"#{Value}");
                    SetFieldError(_colorPickerEntry._entry, true, GetRequiredResourceValue());
                }
                else if (!string.IsNullOrWhiteSpace(Value) && !Regex.IsMatch(Value, Constants.COLOR_REGEX_PATTERN))
                {
                    _colorPickerEntry._entry.EndIconColor = Color.FromArgb($"#{Value}");
                    SetFieldError(_colorPickerEntry._entry, true, LibResources.GetResourceValueByKey(PageResources?.Resources, ResourceConstants.R_INVALID_DATA_KEY));
                }
            }
        }
        SetValidationResult(isButtonClick, _colorPickerEntry?._entry?.ErrorText);
    }
}