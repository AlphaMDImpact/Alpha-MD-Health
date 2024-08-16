using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateSwitchStyle()
    {

        //_switch.HorizontalOptions = LayoutOptions.Center;

        Style defaultSwitchStyle = new Style(typeof(Switch))
        {
            Setters =
            {
                new Setter { Property = Switch.FlowDirectionProperty, Value = _appFlowDirection },
                new Setter { Property = Switch.HorizontalOptionsProperty, Value = LayoutOptions.Center },
                new Setter { Property = Switch.VerticalOptionsProperty, Value = LayoutOptions.Start },
                new Setter { Property = Switch.OnColorProperty, Value = _tertiaryAppColor },
            }
        };

        Style onSwitchStyle = new Style(typeof(Switch))
        {
            BasedOn = defaultSwitchStyle,
            Setters =
            {
                new Setter { Property = Switch.ThumbColorProperty, Value = _successColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ON_SWITCH_STYLE, onSwitchStyle);

        Style offSwitchStyle = new Style(typeof(Switch))
        {
            BasedOn = defaultSwitchStyle,
            Setters =
            {
                new Setter { Property = Switch.ThumbColorProperty, Value = _errorColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.OFF_SWITCH_STYLE, offSwitchStyle);

        Style disableSwitchStyle = new Style(typeof(Switch))
        {
            BasedOn = defaultSwitchStyle,
            Setters =
            {
                new Setter { Property = Switch.ThumbColorProperty, Value =  _secondaryTextColor },
                new Setter { Property = Switch.OnColorProperty, Value = _tertiaryTextColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.DISABLE_SWITCH_STYLE, disableSwitchStyle);
    }

}
