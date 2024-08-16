using AlphaMDHealth.Utility;
using DevExpress.Maui.Controls;

namespace AlphaMDHealth.MobileClient;

internal class AmhTabControl : AmhBaseControl
{
    private TabView _tabView;

    private long _value;
    /// <summary>
    /// Control value as bool
    /// </summary>
    internal long Value
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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(long), typeof(AmhTabControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhTabControl control = (AmhTabControl)bindable;
        if (newValue != null)
        {
            control.Value = (long)newValue;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhTabControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    public AmhTabControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
        Content = _tabView;
    }

    private long GetControlValue()
    {
        if (GenericMethods.IsListNotEmpty(_options))
        {
            _value = _options[_tabView.SelectedItemIndex]?.OptionID ?? default;
        }
        return _value;
    }

    private void SetControlValue()
    {
        if (_options.Count > 0)
        {
            foreach (var item in _options)
            {
                item.IsSelected = item.OptionID == Convert.ToInt64(_value);
                if (item.IsSelected)
                {
                    _tabView.SelectedItemIndex = _options.IndexOf(item);
                }
            }
        }
    }

    private void TabViewItemHeaderTapped(object sender, ItemHeaderTappedEventArgs e)
    {
        foreach (var item in _options)
        {
            item.IsSelected = item.OptionID == _options[e.Index].OptionID;
        }
        _value = _options[e.Index].OptionID;
        OnValueChangedAction(sender, e);
    }

    protected override void EnabledDisableField(bool value)
    {
        SetFieldIsEnabled(_tabView, value);
    }

    protected override void RenderControl()
    {
        if (_tabView == null)
        {
            _tabView = new TabView()
            {
                Style = (Style)Application.Current.Resources[StyleConstants.TABVIEW_STYLE],
                ItemHeaderTemplate = new DataTemplate(() =>
                {
                    return new AmhTabViewCell();
                }),
                ItemTemplate = new AmhTabContentViewCell(),
            };
        }
        else
        {
            _tabView.ItemHeaderTapped -= TabViewItemHeaderTapped;
        }
        _tabView.ItemHeaderTapped += TabViewItemHeaderTapped;
    }

    protected override void ApplyOptions()
    {
        _tabView.ItemsSource = null;
        _tabView.ItemsSource = _options;
    }

    protected override void ApplyResourceValue()
    {
    }
}