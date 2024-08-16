using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class AmhLabelControl : AmhBaseControl
{
    private Label _label;

    private LayoutOptions? _verticalOption;
    /// <summary>
    /// Width of image
    /// </summary>
    /// 
    public LayoutOptions? VerticalOption
    {
        get { return _verticalOption; }
        set
        {
            if(_verticalOption != null)
            {
                _verticalOption = value;
                _label.VerticalOptions = _verticalOption.Value;
            }            
        }
    }

    private TextAlignment _verticalTextAlignment;
    /// <summary>
    /// Width of image
    /// </summary>
    public TextAlignment VerticalTextAlignment
    {
        get { return _verticalTextAlignment; }
        set
        {
            if(_verticalTextAlignment != TextAlignment.Start) 
            {
                _verticalTextAlignment = value;
                _label.VerticalTextAlignment = _verticalTextAlignment;
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
    /// value property
    /// </summary>
    internal static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(AmhLabelControl), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
    private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AmhLabelControl control = (AmhLabelControl)bindable;
        if (newValue != null)
        {
            control.Value = newValue as string;
        }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public AmhLabelControl() : this(FieldTypes.Default)
    {
    }

    /// <summary>
    /// parameterized constructor
    /// </summary>
    /// <param name="controlType">type of control to render</param>
    internal AmhLabelControl(FieldTypes controlType) : base(controlType)
    {
        RenderControl();
        Content = _label;
    }

    private string GetControlValue()
    {
        return _label?.Text;
    }

    private void SetControlValue()
    {
        SetText();
    }

    protected override void ApplyResourceValue()
    {
        SetText();
    }

    protected override void EnabledDisableField(bool value)
    {
        SetFieldIsEnabled(_label, value);
    }

    protected override void RenderControl()
    {
        _label ??= new Label();
        string styleName = StyleName;
        if (string.IsNullOrWhiteSpace(styleName))
        {
            styleName = _fieldType switch
            {
                #region PrimaryLabelControl                       
                FieldTypes.PrimaryLargeHVCenterLabelControl => StyleConstants.ST_PRIMARY_LARGE_HV_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryLargeHVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_LARGE_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryLargeHStartVCenterLabelControl => StyleConstants.ST_PRIMARY_LARGE_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryLargeHStartVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_LARGE_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryLargeHEndVCenterLabelControl => StyleConstants.ST_PRIMARY_LARGE_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryLargeHEndVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_LARGE_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.PrimaryMediumHVCenterLabelControl => StyleConstants.ST_PRIMARY_MEDIUM_HV_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryMediumHVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_MEDIUM_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryMediumHStartVCenterLabelControl => StyleConstants.ST_PRIMARY_MEDIUM_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryMediumHStartVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_MEDIUM_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryMediumHEndVCenterLabelControl => StyleConstants.ST_PRIMARY_MEDIUM_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryMediumHEndVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_MEDIUM_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.PrimarySmallHVCenterLabelControl => StyleConstants.ST_PRIMARY_SMALL_HV_CENTER_LABEL_STYLE,
                FieldTypes.PrimarySmallHVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_SMALL_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimarySmallHStartVCenterLabelControl => StyleConstants.ST_PRIMARY_SMALL_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimarySmallHStartVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_SMALL_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimarySmallHEndVCenterLabelControl => StyleConstants.ST_PRIMARY_SMALL_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimarySmallHEndVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_SMALL_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.PrimaryMicroHVCenterLabelControl => StyleConstants.ST_PRIMARY_MICRO_HV_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryMicroHVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_MICRO_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryMicroHStartVCenterLabelControl => StyleConstants.ST_PRIMARY_MICRO_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryMicroHStartVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_MICRO_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryMicroHEndVCenterLabelControl => StyleConstants.ST_PRIMARY_MICRO_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryMicroHEndVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_MICRO_H_END_V_CENTER_BOLD_LABEL_STYLE,
                #endregion

                #region SecondaryLabelControl                
                FieldTypes.SecondaryLargeHVCenterLabelControl => StyleConstants.ST_SECONDARY_LARGE_HV_CENTER_LABEL_STYLE,
                FieldTypes.SecondaryLargeHVCenterBoldLabelControl => StyleConstants.ST_SECONDARY_LARGE_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.SecondaryLargeHStartVCenterLabelControl => StyleConstants.ST_SECONDARY_LARGE_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.SecondaryLargeHStartVCenterBoldLabelControl => StyleConstants.ST_SECONDARY_LARGE_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.SecondaryLargeHEndVCenterLabelControl => StyleConstants.ST_SECONDARY_LARGE_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.SecondaryLargeHEndVCenterBoldLabelControl => StyleConstants.ST_SECONDARY_LARGE_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.SecondaryMediumHVCenterLabelControl => StyleConstants.ST_SECONDARY_MEDIUM_HV_CENTER_LABEL_STYLE,
                FieldTypes.SecondaryMediumHVCenterBoldLabelControl => StyleConstants.ST_SECONDARY_MEDIUM_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.SecondaryMediumHStartVCenterLabelControl => StyleConstants.ST_SECONDARY_MEDIUM_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.SecondaryMediumHStartVCenterBoldLabelControl => StyleConstants.ST_SECONDARY_MEDIUM_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.SecondaryMediumHEndVCenterLabelControl => StyleConstants.ST_SECONDARY_MEDIUM_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.SecondaryMediumHEndVCenterBoldLabelControl => StyleConstants.ST_SECONDARY_MEDIUM_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.SecondarySmallHVCenterLabelControl => StyleConstants.ST_SECONDARY_SMALL_HV_CENTER_LABEL_STYLE,
                FieldTypes.SecondarySmallHVCenterBoldLabelControl => StyleConstants.ST_SECONDARY_SMALL_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.SecondarySmallHStartVCenterLabelControl => StyleConstants.ST_SECONDARY_SMALL_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.SecondarySmallHStartVCenterBoldLabelControl => StyleConstants.ST_SECONDARY_SMALL_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.SecondarySmallHEndVCenterLabelControl => StyleConstants.ST_SECONDARY_SMALL_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.SecondarySmallHEndVCenterBoldLabelControl => StyleConstants.ST_SECONDARY_SMALL_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.SecondaryMicroHVCenterLabelControl => StyleConstants.ST_SECONDARY_MICRO_HV_CENTER_LABEL_STYLE,
                FieldTypes.SecondaryMicroHVCenterBoldLabelControl => StyleConstants.ST_SECONDARY_MICRO_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.SecondaryMicroHStartVCenterLabelControl => StyleConstants.ST_SECONDARY_MICRO_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.SecondaryMicroHStartVCenterBoldLabelControl => StyleConstants.ST_SECONDARY_MICRO_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.SecondaryMicroHEndVCenterLabelControl => StyleConstants.ST_SECONDARY_MICRO_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.SecondaryMicroHEndVCenterBoldLabelControl => StyleConstants.ST_SECONDARY_MICRO_H_END_V_CENTER_BOLD_LABEL_STYLE,


                #endregion

                #region TertiaryLabelControl                
                FieldTypes.TertiaryLargeHVCenterLabelControl => StyleConstants.ST_TERTIARY_LARGE_HV_CENTER_LABEL_STYLE,
                FieldTypes.TertiaryLargeHVCenterBoldLabelControl => StyleConstants.ST_TERTIARY_LARGE_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.TertiaryLargeHStartVCenterLabelControl => StyleConstants.ST_TERTIARY_LARGE_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.TertiaryLargeHStartVCenterBoldLabelControl => StyleConstants.ST_TERTIARY_LARGE_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.TertiaryLargeHEndVCenterLabelControl => StyleConstants.ST_TERTIARY_LARGE_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.TertiaryLargeHEndVCenterBoldLabelControl => StyleConstants.ST_TERTIARY_LARGE_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.TertiaryMediumHVCenterLabelControl => StyleConstants.ST_TERTIARY_MEDIUM_HV_CENTER_LABEL_STYLE,
                FieldTypes.TertiaryMediumHVCenterBoldLabelControl => StyleConstants.ST_TERTIARY_MEDIUM_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.TertiaryMediumHStartVCenterLabelControl => StyleConstants.ST_TERTIARY_MEDIUM_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.TertiaryMediumHStartVCenterBoldLabelControl => StyleConstants.ST_TERTIARY_MEDIUM_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.TertiaryMediumHEndVCenterLabelControl => StyleConstants.ST_TERTIARY_MEDIUM_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.TertiaryMediumHEndVCenterBoldLabelControl => StyleConstants.ST_TERTIARY_MEDIUM_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.TertiarySmallHVCenterLabelControl => StyleConstants.ST_TERTIARY_SMALL_HV_CENTER_LABEL_STYLE,
                FieldTypes.TertiarySmallHVCenterBoldLabelControl => StyleConstants.ST_TERTIARY_SMALL_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.TertiarySmallHStartVCenterLabelControl => StyleConstants.ST_TERTIARY_SMALL_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.TertiarySmallHStartVCenterBoldLabelControl => StyleConstants.ST_TERTIARY_SMALL_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.TertiarySmallHEndVCenterLabelControl => StyleConstants.ST_TERTIARY_SMALL_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.TertiarySmallHEndVCenterBoldLabelControl => StyleConstants.ST_TERTIARY_SMALL_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.TertiaryMicroHVCenterLabelControl => StyleConstants.ST_TERTIARY_MICRO_HV_CENTER_LABEL_STYLE,
                FieldTypes.TertiaryMicroHVCenterBoldLabelControl => StyleConstants.ST_TERTIARY_MICRO_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.TertiaryMicroHStartVCenterLabelControl => StyleConstants.ST_TERTIARY_MICRO_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.TertiaryMicroHStartVCenterBoldLabelControl => StyleConstants.ST_TERTIARY_MICRO_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.TertiaryMicroHEndVCenterLabelControl => StyleConstants.ST_TERTIARY_MICRO_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.TertiaryMicroHEndVCenterBoldLabelControl => StyleConstants.ST_TERTIARY_MICRO_H_END_V_CENTER_BOLD_LABEL_STYLE,

                #endregion

                #region PrimaryAppLabelControl                
                FieldTypes.PrimaryAppLargeHVCenterLabelControl => StyleConstants.ST_PRIMARY_APP_LARGE_HV_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryAppLargeHVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_APP_LARGE_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryAppLargeHStartVCenterLabelControl => StyleConstants.ST_PRIMARY_APP_LARGE_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryAppLargeHStartVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_APP_LARGE_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryAppLargeHEndVCenterLabelControl => StyleConstants.ST_PRIMARY_APP_LARGE_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryAppLargeHEndVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_APP_LARGE_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.PrimaryAppMediumHVCenterLabelControl => StyleConstants.ST_PRIMARY_APP_MEDIUM_HV_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryAppMediumHVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_APP_MEDIUM_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryAppMediumHStartVCenterLabelControl => StyleConstants.ST_PRIMARY_APP_MEDIUM_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryAppMediumHStartVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_APP_MEDIUM_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryAppMediumHEndVCenterLabelControl => StyleConstants.ST_PRIMARY_APP_MEDIUM_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryAppMediumHEndVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_APP_MEDIUM_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.PrimaryAppSmallHVCenterLabelControl => StyleConstants.ST_PRIMARY_APP_SMALL_HV_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryAppSmallHVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_APP_SMALL_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryAppSmallHStartVCenterLabelControl => StyleConstants.ST_PRIMARY_APP_SMALL_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryAppSmallHStartVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_APP_SMALL_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryAppSmallHEndVCenterLabelControl => StyleConstants.ST_PRIMARY_APP_SMALL_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryAppSmallHEndVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_APP_SMALL_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.PrimaryAppMicroHVCenterLabelControl => StyleConstants.ST_PRIMARY_APP_MICRO_HV_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryAppMicroHVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_APP_MICRO_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryAppMicroHStartVCenterLabelControl => StyleConstants.ST_PRIMARY_APP_MICRO_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryAppMicroHStartVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_APP_MICRO_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.PrimaryAppMicroHEndVCenterLabelControl => StyleConstants.ST_PRIMARY_APP_MICRO_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.PrimaryAppMicroHEndVCenterBoldLabelControl => StyleConstants.ST_PRIMARY_APP_MICRO_H_END_V_CENTER_BOLD_LABEL_STYLE,
                #endregion

                #region LinkLabel
                FieldTypes.LinkHVCenterLabelControl => StyleConstants.ST_LINK_HV_CENTER_LABEL_STYLE,
                FieldTypes.LinkHVCenterBoldLabelControl => StyleConstants.ST_LINK_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.LinkHStartVCenterLabelControl => StyleConstants.ST_LINK_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.LinkHStartVCenterBoldLabelControl => StyleConstants.ST_LINK_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.LinkHEndVCenterLabelControl => StyleConstants.ST_LINK_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.LinkHEndVCenterBoldLabelControl => StyleConstants.ST_LINK_H_END_V_CENTER_BOLD_LABEL_STYLE,
                #endregion

                #region ErrorLabel
                FieldTypes.ErrorHVCenterLabelControl => StyleConstants.ST_ERROR_HV_CENTER_LABEL_STYLE,
                FieldTypes.ErrorHVCenterBoldLabelControl => StyleConstants.ST_ERROR_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.ErrorHStartVCenterLabelControl => StyleConstants.ST_ERROR_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.ErrorHStartVCenterBoldLabelControl => StyleConstants.ST_ERROR_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.ErrorHEndVCenterLabelControl => StyleConstants.ST_ERROR_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.ErrorHEndVCenterBoldLabelControl => StyleConstants.ST_ERROR_H_END_V_CENTER_BOLD_LABEL_STYLE,
                #endregion

                #region HtmlLabelControl
                FieldTypes.HtmlPrimaryLabelControl => StyleConstants.ST_HTML_PRIMARY_LABEL_STYLE,
                FieldTypes.HtmlPrimaryCenterLabelControl => StyleConstants.ST_HTML_PRIMARY_CENTER_LABEL_STYLE,

                FieldTypes.HtmlSecondaryLabelControl => StyleConstants.ST_HTML_SECONDARY_LABEL_STYLE,
                FieldTypes.HtmlSecondaryCenterLabelControl => StyleConstants.ST_HTML_SECONDARY_CENTER_LABEL_STYLE,

                FieldTypes.HtmlTertiaryLabelControl => StyleConstants.ST_HTML_TERTIARY_LABEL_STYLE,
                FieldTypes.HtmlTertiaryCenterLabelControl => StyleConstants.ST_HTML_TERTIARY_CENTER_LABEL_STYLE,

                FieldTypes.HtmlLightLabelControl => StyleConstants.ST_HTML_LIGHT_LABEL_STYLE,
                FieldTypes.HtmlLightCenterLabelControl => StyleConstants.ST_HTML_LIGHT_CENTER_LABEL_STYLE,
                #endregion

                #region WhiteLabelControl
                FieldTypes.LightLargeHVCenterLabelControl => StyleConstants.ST_LIGHT_LARGE_HV_CENTER_LABEL_STYLE,
                FieldTypes.LightLargeHVCenterBoldLabelControl => StyleConstants.ST_LIGHT_LARGE_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.LightLargeHStartVCenterLabelControl => StyleConstants.ST_LIGHT_LARGE_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.LightLargeHStartVCenterBoldLabelControl => StyleConstants.ST_LIGHT_LARGE_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.LightLargeHEndVCenterLabelControl => StyleConstants.ST_LIGHT_LARGE_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.LightLargeHEndVCenterBoldLabelControl => StyleConstants.ST_LIGHT_LARGE_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.LightMediumHVCenterLabelControl => StyleConstants.ST_LIGHT_MEDIUM_HV_CENTER_LABEL_STYLE,
                FieldTypes.LightMediumHVCenterBoldLabelControl => StyleConstants.ST_LIGHT_MEDIUM_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.LightMediumHStartVCenterLabelControl => StyleConstants.ST_LIGHT_MEDIUM_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.LightMediumHStartVCenterBoldLabelControl => StyleConstants.ST_LIGHT_MEDIUM_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.LightMediumHEndVCenterLabelControl => StyleConstants.ST_LIGHT_MEDIUM_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.LightMediumHEndVCenterBoldLabelControl => StyleConstants.ST_LIGHT_MEDIUM_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.LightSmallHVCenterLabelControl => StyleConstants.ST_LIGHT_SMALL_HV_CENTER_LABEL_STYLE,
                FieldTypes.LightSmallHVCenterBoldLabelControl => StyleConstants.ST_LIGHT_SMALL_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.LightSmallHStartVCenterLabelControl => StyleConstants.ST_LIGHT_SMALL_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.LightSmallHStartVCenterBoldLabelControl => StyleConstants.ST_LIGHT_SMALL_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.LightSmallHEndVCenterLabelControl => StyleConstants.ST_LIGHT_SMALL_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.LightSmallHEndVCenterBoldLabelControl => StyleConstants.ST_LIGHT_SMALL_H_END_V_CENTER_BOLD_LABEL_STYLE,

                FieldTypes.LightMicroHVCenterLabelControl => StyleConstants.ST_LIGHT_MICRO_HV_CENTER_LABEL_STYLE,
                FieldTypes.LightMicroHVCenterBoldLabelControl => StyleConstants.ST_LIGHT_MICRO_HV_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.LightMicroHStartVCenterLabelControl => StyleConstants.ST_LIGHT_MICRO_H_START_V_CENTER_LABEL_STYLE,
                FieldTypes.LightMicroHStartVCenterBoldLabelControl => StyleConstants.ST_LIGHT_MICRO_H_START_V_CENTER_BOLD_LABEL_STYLE,
                FieldTypes.LightMicroHEndVCenterLabelControl => StyleConstants.ST_LIGHT_MICRO_H_END_V_CENTER_LABEL_STYLE,
                FieldTypes.LightMicroHEndVCenterBoldLabelControl => StyleConstants.ST_LIGHT_MICRO_H_END_V_CENTER_BOLD_LABEL_STYLE,

                #endregion

                _ => StyleConstants.ST_PRIMARY_SMALL_H_START_V_CENTER_LABEL_STYLE,
            }; 
        }
        _label.Style = (Style)Application.Current.Resources[styleName];
        if (_fieldType.ToString().StartsWith("Html") || _fieldType.ToString().StartsWith("Link"))
        {
            AttachEventHandler();
        }
    }

    private void SetText()
    {
        _label.Text = string.IsNullOrWhiteSpace(_value) && !string.IsNullOrWhiteSpace(_resource?.ResourceValue) 
            ? _resource.ResourceValue
            : _value;
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

    protected override void ApplyStyle()
    {
        _label.Style = (Style)Application.Current.Resources[StyleName];
    }
}