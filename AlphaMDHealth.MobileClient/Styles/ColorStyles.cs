using AlphaMDHealth.Utility;
using Maui.ColorPicker;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateColorStyle()
    {
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_BACKGROUND_COLOR, _defaultBackgroundColor);
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_COLOR, _primaryAppColor);
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_APP_COLOR, _secondaryAppColor);
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_APP_COLOR, _tertiaryAppColor);

        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_TEXT_COLOR, _primaryTextColor);
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_TEXT_COLOR, _secondaryTextColor);
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_TEXT_COLOR, _tertiaryTextColor);

        Application.Current.Resources.Add(StyleConstants.ST_GENERIC_BACKGROUND_COLOR, _genericBackgroundColor);
        Application.Current.Resources.Add(StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE, _separatorAndDisableColor);

        Application.Current.Resources.Add(StyleConstants.ST_ERROR_COLOR, _errorColor);
        Application.Current.Resources.Add(StyleConstants.ST_SUCCESS_COLOR, _successColor);
        Application.Current.Resources.Add(StyleConstants.ST_ACCENT_COLOR, _accentColor);

        Application.Current.Resources.Add(StyleConstants.ST_IS_PROFILE_CIRCULAR_STYLE, _isProfileCircular);
        Application.Current.Resources.Add(StyleConstants.ST_DROP_SHADOW_COLOR, _boxShadowColor);
        Application.Current.Resources.Add(StyleConstants.ST_IS_SEPARATOR_BELOW_HEADER_STYLE, _isSeperatorBelowHeader);
        //Application.Current.Resources.Add(StyleConstants.ST_BASE_PAGE_BACKGROUD_HEIGHT_PERCENT, basePageBackgroudheightPercent);
        //Application.Current.Resources.Add(StyleConstants.ST_TRIPLE_ROW_HEIGHT, _tripleRowHeight);
        //Application.Current.Resources.Add(StyleConstants.ST_DOUBLE_ROW_HEIGHT, _doubleRowHeight);
        Application.Current.Resources.Add(StyleConstants.ST_FLOW_DIRECTION, DefaultFlowDirection);

        CreateDevExpressColorPickerStyle();
    }

    private void CreateDevExpressColorPickerStyle()
    {
        Style defaultColorPickerStyle = new(typeof(ColorPicker))
        {
            Setters =
            {
                new Setter { Property = ColorPicker.FlowDirectionProperty , Value = DefaultFlowDirection },
                new Setter { Property = ColorPicker.ColorFlowDirectionProperty , Value = ColorFlowDirection.Horizontal },
                new Setter { Property = ColorPicker.ColorSpectrumStyleProperty , Value = ColorSpectrumStyle.TintToHueToShadeStyle },
                new Setter { Property = ColorPicker.PointerRingBorderUnitsProperty , Value = 0.3 },
                new Setter { Property = ColorPicker.PointerRingDiameterUnitsProperty , Value = 0.7 }
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_COLOR_PICKER_STYLE, defaultColorPickerStyle);
    }
}