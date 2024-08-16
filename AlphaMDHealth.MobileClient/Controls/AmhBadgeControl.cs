using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class AmhBadgeControl : AmhBaseControl
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
    public AmhBadgeControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    public AmhBadgeControl(FieldTypes controlType) : base(controlType)
    {
        _label = new Label();
        Content = new Frame
        {
            Style = (Style)Application.Current.Resources[StyleConstants.BADGE_FRAME_STYLE],
            Content = _label
        };
        RenderControl();
    }

    private string GetControlValue()
    {
        return _label.Text;
    }

    private void SetControlValue()
    {
        _label.Text = _value;
        ApplyResource();
    }

    protected override void RenderControl()
    {
        string styleName = StyleName;
        if (string.IsNullOrWhiteSpace(styleName))
        {
            styleName = _fieldType switch
            {
                FieldTypes.PrimaryBadgeControl => StyleConstants.PRIMARY_BADGE_STYLE,
                FieldTypes.SecondaryBadgeControl => StyleConstants.SECONDARY_BADGE_STYLE,
                FieldTypes.DarkBadgeControl => StyleConstants.DARK_BADGE_STYLE,
                FieldTypes.LightBadgeControl => StyleConstants.LIGHT_BADGE_STYLE,
                FieldTypes.SuccessBadgeControl => StyleConstants.SUCCESS_BADGE_STYLE,
                FieldTypes.DangerBadgeControl => StyleConstants.DANGER_BADGE_STYLE,
                FieldTypes.InfoBadgeControl => StyleConstants.INFO_BADGE_STYLE,
                FieldTypes.WarningBadgeControl => StyleConstants.WARNING_BADGE_STYLE,
                _ => StyleConstants.WARNING_BADGE_STYLE,
            };
        }
        _label.Style = (Style)Application.Current.Resources[styleName];

        //Content = new Frame
        //{
        //    Style = (Style)Application.Current.Resources[StyleConstants.BADGE_COUNT_FRAME_STYLE],
        //    Content = _label
        //};
    }

    protected override void ApplyResourceValue()
    {
        ApplyResource();
    }

    protected override void EnabledDisableField(bool value)
    {
    }

    private void ApplyResource()
    {
        if (!string.IsNullOrWhiteSpace(_label.Text) && !string.IsNullOrWhiteSpace(_resource?.ResourceValue))
        {
            _label.Text = _resource.ResourceValue;
        }
    }
}