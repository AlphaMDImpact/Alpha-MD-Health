using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class AmhImageControl : AmhBaseControl
{
    private Border _imageBorder;
    internal Image _image;
    private Label _initial;

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
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(AmhImageControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhImageControl control = (AmhImageControl)bindable;
        if (newValue != null)
        {
            control.Value = newValue as string;
        }
    }

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
                SetImageSize();
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
                SetImageSize();
            }
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhImageControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    public AmhImageControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
    }

    /// <summary>
    /// Get value of control
    /// </summary>
    /// <returns>value of control</returns>
    private string GetControlValue()
    {
        return _value;
    }

    /// <summary>
    /// Set value to control
    /// </summary>
    private void SetControlValue()
    {
        RenderSource();
    }

    /// <summary>
    /// Render Control baseed on Specified type
    /// </summary>
    protected override void RenderControl()
    {
        if (!IsInitialized())
        {
            _image = new Image()
            {
                Aspect = Aspect.AspectFit,
            };
            _initial = new Label
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_INITIAL_IMAGE_STYLE]
            };
            _imageBorder = new Border();
        }
        SetImageSize();
        RenderSource();
        string styleName = StyleName;
        if (string.IsNullOrWhiteSpace(styleName))
        {
            switch (FieldType)
            {
                case FieldTypes.CircleImageControl:
                    styleName = StyleConstants.ST_CIRCLE_STYLE;
                    break;
                case FieldTypes.CircleWithBorderImageControl:
                    styleName = StyleConstants.ST_CIRCLE_WITH_BORDER_STYLE;
                    break;
                case FieldTypes.SquareImageControl:
                    styleName = StyleConstants.ST_RECTANGLE_STYLE;
                    break;
                case FieldTypes.SquareWithBorderImageControl:
                    styleName = StyleConstants.ST_RECTANGLE_WITH_BORDER_STYLE;
                    break;
                case FieldTypes.CircleWithBackgroundImageControl:
                    styleName = StyleConstants.ST_CIRCLE_IMAGE_WITH_BACKGROUND_STYLE;
                    break;
                case FieldTypes.SquareWithBackgroundImageControl:
                    styleName = StyleConstants.ST_RECTANGLE_IMAGE_WITH_BACKGROUND_STYLE;
                    break;
                case FieldTypes.ImageControl:
                default:
                    styleName = StyleConstants.ST_DEFAULT_IMAGE_STYLE;
                    break;
            }
        }
        if (!string.IsNullOrWhiteSpace(styleName))
        {
            _imageBorder.Style = (Style)Application.Current.Resources[styleName];
        }
        Content = _imageBorder;
        AttachEventHandler();
    }

    /// <summary>
    /// Applies Resource Value Abstract Method
    /// </summary>
    protected override void ApplyResourceValue() { }

    /// <summary>
    /// Enabled Value Abstract Method
    /// </summary>
    /// <param name="value">flag representing control needs to enable or disable</param>
    protected override void EnabledDisableField(bool value) { }

    /// <summary>
    /// set icon
    /// </summary>
    protected override void SetIcon()
    {
        RenderSource();
    }

    private void SetImageSize()
    {
        if (_imageHeight != AppImageSize.ImageNone)
        {
            _imageBorder.HeightRequest = AppStyles.GetImageSize(_imageHeight);
        }
        if (_imageWidth != AppImageSize.ImageNone)
        {
            _imageBorder.WidthRequest = AppStyles.GetImageSize(_imageWidth);
        }
    }

    private bool IsInitialized()
    {
        return _imageBorder != null && _image != null && _initial != null;
    }

    private void RenderSource()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (IsInitialized() && (_source != null || !string.IsNullOrWhiteSpace(_value) || _icon != null))
            {
                if (_source != null && _source.Length > 0)
                {
                    // byte array image in Source to render
                    _image.Source = ImageSource.FromStream(() => GenericMethods.GetMemoryStreamFromByteArray(_source));
                    _imageBorder.Content = _image;
                }
                else if (!string.IsNullOrWhiteSpace(_value) && _value.Length > 100)
                {
                    // base64 image string in Value to render
                    _image.Source = ImageSource.FromStream(() => GenericMethods.GetMemoryStreamFromBase64(_value));
                    _imageBorder.Content = _image;
                }
                else if (!string.IsNullOrWhiteSpace(_value))//(HasStringValue(out string iconVal) && iconVal.Length < 100)
                {
                    // name initial string in Value to render
                    _initial.Text = _value;
                    _imageBorder.Content = _initial;
                }
                else if (!string.IsNullOrWhiteSpace(_icon))
                {
                    // svg image name as ControlIcon to render
                    _image.Source = ImageSource.FromFile(_icon);
                    _imageBorder.Content = _image;
                }
            }
        });
    }

    private void AttachEventHandler()
    {
        TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
        GestureRecognizers.Add(tapGestureRecognizer);
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        OnValueChangedAction(sender, e);
    }
}