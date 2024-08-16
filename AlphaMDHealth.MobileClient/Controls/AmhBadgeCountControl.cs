using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class AmhBadgeCountControl : AmhBaseControl
{
    private Label _label;

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
    public AmhBadgeCountControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    public AmhBadgeCountControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    private string GetControlValue()
    {
        return _label.Text;
    }

    private void SetControlValue()
    {
        _label.Text = _value;
    }

    protected override void RenderControl()
    {
        _label = new Label()
        {
            Style = (Style)Application.Current.Resources[StyleConstants.BADGE_COUNT_LABEL_STYLE],
        };
        Content = new Frame
        {
            Style = (Style)Application.Current.Resources[StyleConstants.BADGE_COUNT_FRAME_STYLE],
            Content = _label
        };
    }

    protected override void ApplyResourceValue()
    {

    }

    protected override void EnabledDisableField(bool value)
    {
    }
}