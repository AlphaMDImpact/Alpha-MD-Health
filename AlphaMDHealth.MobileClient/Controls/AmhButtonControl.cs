using AlphaMDHealth.Utility;
using DevExpress.Maui.Controls;

namespace AlphaMDHealth.MobileClient;

internal class AmhButtonControl : AmhBaseControl
{
    SimpleButton _buttonControl;

    private string _value;
    /// <summary>
    /// Control value as object
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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(AmhButtonControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhButtonControl control = (AmhButtonControl)bindable;
        if (newValue != null)
        {
            control.Value = newValue as string;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhButtonControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhButtonControl(FieldTypes controlType) : base(controlType) 
    {
        RenderControl();
        Content = _buttonControl;
    }

    /// <summary>
    /// Update control style
    /// </summary>
    protected override void ApplyStyle()
    {
        if (!string.IsNullOrWhiteSpace(_styleName))
        {
            RenderControl();
        }
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
        OnValueChangedAction(sender, e);
    }

    private string GetControlValue()
    {
        return _buttonControl?.Text;
    }

    private void SetControlValue()
    {
        _buttonControl.Text = _value;
    }

    protected override void ApplyResourceValue()
    {
        RenderControl();
        _buttonControl.Text = _resource?.ResourceValue?? _value;
    }

    protected override void EnabledDisableField(bool value)
    {
        _buttonControl.IsEnabled = value;
    }

    /// <summary>
    /// set icon
    /// </summary>
    protected override void SetIcon()
    {
        if (_buttonControl!= null && !string.IsNullOrWhiteSpace(Icon))
        {
            _buttonControl.Icon = ImageSource.FromFile(Icon);
        }
    }

    protected override void RenderControl()
    {
        if (_buttonControl == null)
        {
            _buttonControl = new SimpleButton();
        }
        else
        {
            _buttonControl.Clicked -= OnButtonClicked;
        }

        string styleName = StyleName;
        if (string.IsNullOrWhiteSpace(styleName))
        {
            styleName = FieldType switch
            {
                // Regular Buttons
                FieldTypes.PrimaryButtonControl => StyleConstants.ST_PRIMARY_BUTTON_STYLE,
                FieldTypes.SecondaryButtonControl => StyleConstants.ST_SECONDARY_BUTTON_STYLE,
                FieldTypes.TertiaryButtonControl => StyleConstants.ST_TERTIARY_BUTTON_STYLE,
                FieldTypes.DeleteButtonControl => StyleConstants.ST_DELETE_BUTTON_STYLE,
                // Transparent buttons
                FieldTypes.PrimaryTransparentButtonControl => StyleConstants.ST_PRIMARY_TRANSPARENT_BUTTON_STYLE,
                FieldTypes.SecondaryTransparentButtonControl => StyleConstants.ST_SECONDARY_TRANSPARENT_BUTTON_STYLE,
                FieldTypes.TertiaryTransparentButtonControl => StyleConstants.ST_TERTIARY_TRANSPARENT_BUTTON_STYLE,
                FieldTypes.DeleteTransparentButtonControl => StyleConstants.ST_DELETE_TRANSPARENT_BUTTON_STYLE,
                // Border Transparent buttons
                FieldTypes.PrimaryBorderTransparentButtonControl => StyleConstants.ST_PRIMARY_BORDER_TRANSPARENT_BUTTON_STYLE,
                FieldTypes.SecondaryBorderTransparentButtonControl => StyleConstants.ST_SECONDARY_BORDER_TRANSPARENT_BUTTON_STYLE,
                FieldTypes.TertiaryBorderTransparentButtonControl => StyleConstants.ST_TERTIARY_BORDER_TRANSPARENT_BUTTON_STYLE,
                FieldTypes.DeleteBorderTransparentButtonControl => StyleConstants.ST_DELETE_BORDER_TRANSPARENT_BUTTON_STYLE,
                // Expandable Buttons
                FieldTypes.PrimaryExButtonControl => StyleConstants.ST_PRIMARY_EX_BUTTON_STYLE,
                FieldTypes.SecondaryExButtonControl => StyleConstants.ST_SECONDARY_EX_BUTTON_STYLE,
                FieldTypes.TertiaryExButtonControl => StyleConstants.ST_TERTIARY_EX_BUTTON_STYLE,
                FieldTypes.DeleteExButtonControl => StyleConstants.ST_DELETE_EX_BUTTON_STYLE,
                // Transparent buttons
                FieldTypes.PrimaryTransparentExButtonControl => StyleConstants.ST_PRIMARY_TRANSPARENT_EX_BUTTON_STYLE,
                FieldTypes.SecondaryTransparentExButtonControl => StyleConstants.ST_SECONDARY_TRANSPARENT_EX_BUTTON_STYLE,
                FieldTypes.TertiaryTransparentExButtonControl => StyleConstants.ST_TERTIARY_TRANSPARENT_EX_BUTTON_STYLE,
                FieldTypes.DeleteTransparentExButtonControl => StyleConstants.ST_DELETE_TRANSPARENT_EX_BUTTON_STYLE,
                // Border Transparent buttons
                FieldTypes.PrimaryBorderTransparentExButtonControl => StyleConstants.ST_PRIMARY_BORDER_TRANSPARENT_EX_BUTTON_STYLE,
                FieldTypes.SecondaryBorderTransparentExButtonControl => StyleConstants.ST_SECONDARY_BORDER_TRANSPARENT_EX_BUTTON_STYLE,
                FieldTypes.TertiaryBorderTransparentExButtonControl => StyleConstants.ST_TERTIARY_BORDER_TRANSPARENT_EX_BUTTON_STYLE,
                FieldTypes.DeleteBorderTransparentExButtonControl => StyleConstants.ST_DELETE_BORDER_TRANSPARENT_EX_BUTTON_STYLE,

                _ => StyleConstants.ST_PRIMARY_BUTTON_STYLE,
            };
        }
        _buttonControl.Style = (Style)Application.Current.Resources[styleName];
        _buttonControl.StyleId = UniqueID;
        SetIcon();
        _buttonControl.Clicked += OnButtonClicked;
    }

}