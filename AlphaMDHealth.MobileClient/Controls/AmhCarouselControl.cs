using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class AmhCarouselControl : AmhBaseControl
{
    private readonly CarouselView _carouselView;
    private readonly IndicatorView _indicatorView;
    private readonly AmhLabelControl _headerLabel;
    private readonly Grid _container;

    private object _value;
    /// <summary>
    /// Control value as object
    /// </summary>
    internal object Value
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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(object), typeof(AmhCarouselControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhCarouselControl control = (AmhCarouselControl)bindable;
        if (newValue != null)
        {
            control.Value = newValue;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhCarouselControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    public AmhCarouselControl(FieldTypes controlType) : base(controlType)
    {
        _container = CreateGridContainer(false);
        _container.RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition { Height = GridLength.Star },
            new RowDefinition { Height = GridLength.Auto },
        };
        _container.ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Star }
        };
        _indicatorView = new IndicatorView()
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_INDICATOR_STYLE],
        };
        _carouselView = new CarouselView()
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_CAROUSEL_STYLE],
            ItemTemplate = new DataTemplate(() =>
            {
                return new CarouselViewCell(FieldType);
            })
        };
        _headerLabel = new AmhLabelControl(FieldTypes.PrimarySmallHVCenterBoldLabelControl)
        {
            ZIndex = 4,
        };
        _container.RowSpacing = 10;
        _container.Add(_carouselView, 0, 0);
        _container.Add(_indicatorView, 0, 1);
        Content = _container;
    }

    private object GetControlValue()
    {
        return _carouselView.CurrentItem;
    }

    private void SetControlValue()
    {
        _carouselView.CurrentItem = _value;
    }

    protected override void EnabledDisableField(bool value)
    {
        SetFieldIsEnabled(_carouselView, value);
    }

    protected override void RenderControl()
    {
    }

    private void OnItemChanged(object sender, CurrentItemChangedEventArgs e)
    {
        OnValueChangedAction(sender, e);
    }

    protected override void ApplyResourceValue()
    {
        if (ShowHeader)
        {
            _headerLabel.Value = _resource?.ResourceValue;
            _container.Add(_headerLabel, 0, 0);
        }
        else
        {
            if (_container.Contains(_headerLabel))
            {
                _container.Remove(_headerLabel);
            }
        }
    }

    protected override void ApplyOptions()
    {
        _carouselView.CurrentItemChanged -= OnItemChanged;
        _carouselView.ItemsSource = Options;
        _indicatorView.ItemsSource = Options;
        _carouselView.IndicatorView = _indicatorView;
        _carouselView.CurrentItemChanged += OnItemChanged;
    }
}