using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class AmhFieldTypeMapper : AmhBaseControl
{
    private AmhBaseControl _field;

    private AppImageSize _imageWidth;
    /// <summary>
    /// Width of image
    /// </summary>
    internal AppImageSize ImageWidth
    {
        get { return _imageWidth; }
        set
        {
            if (_imageWidth != value)
            {
                _imageWidth = value;
                if (_field != null && IsImageField())
                {
                    (_field as AmhImageControl).ImageWidth = _imageWidth;
                }
            }
        }
    }

    private AppImageSize _imageHeight;
    /// <summary>
    /// Height of image
    /// </summary>
    internal AppImageSize ImageHeight
    {
        get { return _imageHeight; }
        set
        {
            if (_imageHeight != value)
            {
                _imageHeight = value;
                if (_field != null && IsImageField())
                {
                    (_field as AmhImageControl).ImageHeight = _imageHeight;
                }
            }
        }
    }

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
    /// set icon
    /// </summary>
    protected override void SetIcon()
    {
        if (_field != null)
        {
            _field.Icon = _icon;
        }
    }

    /// <summary>
    /// value property
    /// </summary>
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(AmhFieldTypeMapper), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhFieldTypeMapper control = (AmhFieldTypeMapper)bindable;
        if (newValue != null)
        {
            control.Value = newValue as string;
        }
    }

    public AmhFieldTypeMapper(FieldTypes fieldType) : base(fieldType)
    {
        RenderControl();
    }

    private string GetControlValue()
    {
        return _value;
    }

    private void SetControlValue()
    {
        if (_field != null)
        {
            switch (_fieldType)
            {
                case FieldTypes.BadgeCountControl:
                    if (_field is AmhBadgeCountControl)
                    {
                        (_field as AmhBadgeCountControl).Value = _value;
                    }
                    break;
                default:
                    if (_fieldType.ToString().EndsWith(FieldTypes.BadgeControl.ToString()) && _field is AmhBadgeControl)
                    {
                        (_field as AmhBadgeControl).Value = _value;
                    }
                    else if (IsImageField())
                    {
                        (_field as AmhImageControl).Value = _value;
                    }
                    else if (_field is AmhLabelControl)
                    {
                        (_field as AmhLabelControl).Value = _value;
                    }
                    break;
            }
        }
    }

    private bool IsImageField()
    {
        return _fieldType.ToString().EndsWith(FieldTypes.ImageControl.ToString()) && _field is AmhImageControl;
    }

    protected override void ApplyResourceValue()
    {
        if (_field != null)
        {
            _field.PageResources = PageResources;
        }
    }

    protected override void EnabledDisableField(bool value)
    {
        if (_field != null)
        {
            _field.IsControlEnabled = value;
        }
    }

    protected override void RenderControl()
    {
        switch (_fieldType)
        {
            case FieldTypes.BadgeCountControl:
                _field = new AmhBadgeCountControl(_fieldType) { PageResources = PageResources, ResourceKey = ResourceKey, Source = _source, Value = _value, Icon = _icon };
                break;
            default:
                if (_fieldType.ToString().EndsWith(FieldTypes.BadgeControl.ToString()))
                {
                    _field = new AmhBadgeControl(_fieldType) { PageResources = PageResources, ResourceKey = ResourceKey, Source = _source, Value = _value, Icon = _icon };
                }
                else if (_fieldType.ToString().EndsWith(FieldTypes.ImageControl.ToString()))
                {
                    _field = new AmhImageControl(_fieldType) { PageResources = PageResources, ResourceKey = ResourceKey, Source = _source, Value = _value, Icon = _icon };
                }
                else
                {
                    _field = new AmhLabelControl(_fieldType) { PageResources = PageResources, ResourceKey = ResourceKey, Source = _source, Value = _value, Icon = _icon };
                }
                break;
        }
        Content = _field;
    }
}
