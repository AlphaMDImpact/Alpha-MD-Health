using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

internal class AmhSliderControl : AmhBaseControl
{
    private Grid _container;
    private AmhLabelControl _headerLabel;
    private AmhLabelControl _placeHolderLabel;
    private AmhLabelControl _infoLabel;
    private Slider _slider;
    private AmhLabelControl _valueLabel;
    private AmhLabelControl _errorLabel;

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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(double), typeof(AmhSliderControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhSliderControl control = (AmhSliderControl)bindable;
        if (newValue != null)
        {
            control.Value = (double)newValue;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhSliderControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhSliderControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    private double GetControlValue()
    {
        return _slider.Value;
    }

    private void SetControlValue()
    {
        _slider.Value = _value;
    }

    protected override void ApplyResourceValue()
    {
        ApplyResource(_headerLabel, _placeHolderLabel, _infoLabel);
        _slider.Minimum = _resource.MinLength;
        _slider.Maximum = _resource.MaxLength;
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
            _slider = new Slider();
            _valueLabel = new AmhLabelControl(FieldTypes.PrimaryAppSmallHStartVCenterLabelControl);
        }
        _slider.Style = (Style)Application.Current.Resources[string.IsNullOrWhiteSpace(_styleName) ? StyleConstants.SLIDER_STYLE : _styleName];

        _container.Children?.Clear();
        _container.RowDefinitions = new RowDefinitionCollection();
        _container.ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Star }
        };

        int row = 0;
        if (ShowHeader)
        {
            AddView(_container, _headerLabel, 0, 0);
            AddView(_container, _placeHolderLabel, 0, 1);
            AddView(_container, _infoLabel, 0, 2);
            row = 3;
        }
        if (_fieldType == FieldTypes.VerticalSliderControl)
        {
            _slider.Rotation = -90;
            var vLayout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                //HeightRequest = _slider.WidthRequest,
                BackgroundColor = Color.FromRgba("#FFFF00")
            };
            vLayout.Add(_slider);
            AddView(_container, vLayout, 0, row); row++;
            _slider.MinimumHeightRequest = 300;
        }
        else
        {
            AddView(_container, _slider, 0, row); row++;
        }
        AddView(_container, _valueLabel, 0, row); row++;
        AddView(_container, _errorLabel, 0, row);
        
        _slider.ValueChanged += OnValueChanged;
        Content = _container;
    }

    internal override void ValidateControl(bool isButtonClick)
    {
        _errorLabel.Value = string.Empty;
        if (IsControlEnabled)
        {
            if (_slider.Value < _resource.MinLength)
            {
                _errorLabel.Value = string.Format(CultureInfo.CurrentCulture
                    , LibResources.GetResourceValueByKey(PageResources.Resources, ResourceConstants.R_MINIMUM_LENGTH_VALIDATION_KEY)
                    , _resource.MinLength);
            }
            else if (!_resource.MinLength.Equals(_resource.MaxLength) && _resource.MinLength < _resource.MaxLength && _slider.Value > _resource.MaxLength)
            {
                _errorLabel.Value = string.Format(CultureInfo.CurrentCulture
                    , LibResources.GetResourceValueByKey(PageResources.Resources, ResourceConstants.R_RANGE_LENGTH_VALIDATION_KEY)
                    , _resource.MinLength, _resource.MaxLength);
            }
        }
        IsValid = string.IsNullOrWhiteSpace(_errorLabel.Value);
        if (!_isButtonClick)
        {
            _isButtonClick = isButtonClick;
        }
    }

    private void OnValueChanged(object sender, ValueChangedEventArgs e)
    {
        double newValue = e.NewValue;
        int roundedValue = (int)Math.Round(newValue);
        _slider.Value = roundedValue;
        _valueLabel.Value = roundedValue.ToString();
        OnValueChangedAction(sender, e);
    }
}