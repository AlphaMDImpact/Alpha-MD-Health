using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class AmhProgressBarControl : AmhBaseControl
{
    internal ProgressBar _progressBar;

    private double _value;
    /// <summary>
    /// Control value as string
    /// </summary>
    internal double Value
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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(double), typeof(AmhProgressBarControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhProgressBarControl control = (AmhProgressBarControl)bindable;
        if (newValue != null)
        {
            control.Value = (double)newValue;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhProgressBarControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    public AmhProgressBarControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    private double GetControlValue()
    {
        return _progressBar.Progress;
    }

    private void SetControlValue()
    {
        _progressBar.Progress = _value;
    }

    protected override void RenderControl()
    {
        if (_progressBar == null)
        {
            _progressBar = new ProgressBar
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_PROGRESS_BAR_STYLE],
            };
        }
        ApplyStyle();
        _progressBar.Progress = _value;
        Content = _progressBar;
    }

    protected override void ApplyResourceValue()
    {
    }

    protected override void EnabledDisableField(bool value)
    {
    }

    /// <summary>
    /// Update control style
    /// </summary>
    protected override void ApplyStyle()
    {
        if (!string.IsNullOrWhiteSpace(_styleName))
        {
            _progressBar.ProgressColor = Color.FromArgb(_styleName);
        }
    }
}