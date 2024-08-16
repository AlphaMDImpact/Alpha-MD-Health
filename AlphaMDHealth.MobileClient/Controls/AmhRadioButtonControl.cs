using AlphaMDHealth.Utility;
using DevExpress.Maui.Core.Internal;

namespace AlphaMDHealth.MobileClient;

internal class AmhRadioButtonControl : AmhBaseControl
{
    private AmhLabelControl _headerLabel;
    private AmhLabelControl _placeHolderLabel;
    private AmhLabelControl _infoLabel;
    private AmhLabelControl _errorLabel;
    private Grid _container;

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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(AmhRadioButtonControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhRadioButtonControl control = (AmhRadioButtonControl)bindable;
        if (newValue != null)
        {
            control.Value = (string)newValue;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhRadioButtonControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhRadioButtonControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    private string GetControlValue()
    {
        var selected = _container.Children.FirstOrDefault(x => x is RadioButton && (x as RadioButton).IsChecked);
        _value = selected != null
            ? (selected as RadioButton).StyleId
            : string.Empty;
        return _value;
    }

    private void SetControlValue()
    {
        if (_container.Children.Count > 0)
        {
            long? selectedId = GetSelectedID();
            _container.Children.ForEach(x =>
            {
                if (x is RadioButton)
                {
                    (x as RadioButton).IsChecked = selectedId.HasValue && Convert.ToInt64((x as RadioButton).StyleId) == selectedId;
                }
            });
        }
    }

    private void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        OnValueChangedAction(sender, e);
        if (IsRadioButtonSelected())
        {
            _errorLabel.Value = string.Empty;
        }
    }

    protected override void EnabledDisableField(bool value)
    {
        SetFieldIsEnabled(_container, value);
    }

    protected override void RenderControl()
    {
        if (_container == null)
        {
            CreateWrapperControls(out _container, out _headerLabel, out _placeHolderLabel, out _infoLabel, out _errorLabel);
        }
        AddChildsInContainer();
        Content = _container;
    }

    protected override void ApplyResourceValue()
    {
        if (_resource != null)
        {
            _headerLabel.Value = _resource.ResourceValue;
            _placeHolderLabel.Value = _resource.PlaceHolderValue;
            _infoLabel.Value = _resource.InfoValue;
        }
    }

    protected override void ApplyOptions()
    {
        AddChildsInContainer();
    }

    internal override void ValidateControl(bool isButtonClick)
    {
        _errorLabel.Value = string.Empty;
        if (!IsRadioButtonSelected())
        {
            _errorLabel.Value = GetRequiredResourceValue();
        }
        IsValid = _errorLabel.Value == null || string.IsNullOrWhiteSpace(_errorLabel.Value as string);
        if (!_isButtonClick)
        {
            _isButtonClick = isButtonClick;
        }
    }

    private void AddChildsInContainer()
    {
        _container.Children?.Clear();
        _container.RowDefinitions = new RowDefinitionCollection();
        AddView(_container, _headerLabel, 0, 0);
        AddView(_container, _placeHolderLabel, 0, 1);
        AddView(_container, _infoLabel, 0, 2);
        var row = 3;
        if (GenericMethods.IsListNotEmpty(_options))
        {
            long? selectedIds = GetSelectedID();
            _options.ForEach(item =>
            {
                var radioButton = new RadioButton
                {
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_PRIMARY_RADIO_BUTTON_STYLE],
                    StyleId = item.OptionID.ToString(),
                    Content = item.OptionText,
                    GroupName = item.GroupName,
                    IsChecked = item.IsSelected || (selectedIds.HasValue && selectedIds == item.OptionID)
                };
                radioButton.CheckedChanged += OnCheckedChanged;
                AddView(_container, radioButton, 0, row);
                row++;
            });
        }
        AddView(_container, _errorLabel, 0, row);
    }

    private long? GetSelectedID()
    {
        return !string.IsNullOrWhiteSpace(_value)
            ? Convert.ToInt64(_value)
            : null;
    }

    private bool IsRadioButtonSelected()
    {
        GetControlValue();
        return !string.IsNullOrWhiteSpace(_value);
    }
}