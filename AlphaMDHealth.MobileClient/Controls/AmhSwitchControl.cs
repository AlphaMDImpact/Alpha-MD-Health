using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class AmhSwitchControl : AmhBaseControl
{
    private Grid _container;
    private AmhLabelControl _headerLabel;
    private AmhLabelControl _placeHolderLabel;
    private AmhLabelControl _infoLabel;
    private AmhLabelControl _errorLabel;
    private Switch _switch;

    private bool _value;
    /// <summary>
    /// Control value as bool
    /// </summary>
    internal bool Value
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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(bool), typeof(AmhSwitchControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhSwitchControl control = (AmhSwitchControl)bindable;
        if (newValue != null)
        {
            control.Value = (bool)newValue;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhSwitchControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhSwitchControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    protected override void ApplyResourceValue()
    {
        ApplyResource(_headerLabel, _placeHolderLabel, _infoLabel);
    }

    protected override void EnabledDisableField(bool value)
    {
        SetFieldIsEnabled(_container, value);
        ApplySwitchStyle();
        _switch.IsEnabled = value;
    }

    private bool GetControlValue()
    {
        return _switch.IsToggled;
    }

    private void SetControlValue()
    {
        _switch.IsToggled = _value;
        ApplySwitchStyle();
    }

    protected override void RenderControl()
    {
        if (_container == null)
        {
            CreateWrapperControls(out _container, out _headerLabel, out _placeHolderLabel, out _infoLabel, out _errorLabel);
            _container.RowDefinitions = new RowDefinitionCollection {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            };
            _container.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            };
            _switch = new Switch();
        }
        _switch.Toggled -= OnToggled;
        _container.Children?.Clear();
        ApplySwitchStyle();
        if (ShowHeader)
        {
            AddView(_container, _headerLabel, 0, 0);
            AddView(_container, _placeHolderLabel, 0, 1);
            AddView(_container, _infoLabel, 0, 2);
            AddView(_container, _switch, 1, 0);
        }
        else
        {
            AddView(_container, _switch, 0, 0);
            Grid.SetColumnSpan(_switch, 2);
        }
        Grid.SetRowSpan(_switch, 3);

        AddView(_container, _errorLabel, 0, 3);
        Grid.SetColumnSpan(_errorLabel, 2);

        _switch.Toggled += OnToggled;
        Content = _container;
    }

    internal override void ValidateControl(bool isButtonClick)
    {
        _errorLabel.Value = string.Empty;
        if (IsControlEnabled)
        {
            if (_switch.IsToggled == false)
            {
                _errorLabel.Value = GetRequiredResourceValue();
            }
        }
        IsValid = string.IsNullOrWhiteSpace(_errorLabel.Value as string);
        if (!_isButtonClick)
        {
            _isButtonClick = isButtonClick;
        }
    }

    private void OnToggled(object sender, ToggledEventArgs e)
    {
        ApplySwitchStyle();
        OnValueChangedAction(sender, e);
    }

    private void ApplySwitchStyle()
    {
        var style = !IsControlEnabled
            ? StyleConstants.DISABLE_SWITCH_STYLE
            : string.IsNullOrWhiteSpace(_styleName)
                ? ApplyOnOffSwitchStyle()
                : _styleName;

        _switch.Style = (Style)Application.Current.Resources[style];
    }

    private string ApplyOnOffSwitchStyle()
    {
        return _switch.IsToggled
            ? StyleConstants.ON_SWITCH_STYLE
            : StyleConstants.OFF_SWITCH_STYLE;
    }
}