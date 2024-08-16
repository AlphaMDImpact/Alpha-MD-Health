using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Represents checkbox AppStyles
/// </summary>
public partial class AppStyles
{
    private void CreateRadioButtonStyle()
    {
        CreateMauiRadioButtonStyle();
        CreateOldRadioButtonStyle();
    }

    private void CreateMauiRadioButtonStyle()
    {
        Style primaryRadioButtonStyle = new Style(typeof(RadioButton))
        {
            Setters =
            {
                new Setter { Property = RadioButton.FlowDirectionProperty , Value = DefaultFlowDirection },
                new Setter { Property = RadioButton.TextColorProperty,  Value = _primaryTextColor },
                //new Setter { Property = RadioButton.BackgroundColorProperty, Value = _tertiaryAppColor },
                new Setter { Property = RadioButton.BorderWidthProperty, Value = 0 },

                //new Setter {Property = RadioButton.ButtonTintColorProperty, Value =_primaryAppColor },
                //new Setter {Property = RadioButton.FontSizeProperty, Value = Device.GetNamedSize(NamedSize.Micro, typeof(CustomRadioText))* GenericMethods.GetPlatformSpecificValue(0.7, 0.8, 0)},
                //new Setter {Property = RadioButton.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                //new Setter {Property = RadioButton.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                //new Setter {Property = RadioButton.MarginProperty, Value = new Thickness(0,-15,0,0)},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_RADIO_BUTTON_STYLE, primaryRadioButtonStyle);
    }

    private void CreateOldRadioButtonStyle()
    {
        //Style defaultRadioButtonStyle = new Style(typeof(CustomRadioText))
        //{
        //    Setters =
        //    {
        //        new Setter {Property = CustomRadioText.TextColorProperty,  Value = _tertiaryTextColor  },
        //        new Setter {Property = CustomRadioText.BackgroundColorProperty, Value = _tertiaryAppColor},
        //        new Setter {Property = CustomRadioText.ButtonTintColorProperty, Value =_primaryAppColor},
        //        new Setter {Property = CustomRadioText.FontSizeProperty, Value = Device.GetNamedSize(NamedSize.Micro, typeof(CustomRadioText))* GenericMethods.GetPlatformSpecificValue(0.7, 0.8, 0)},
        //        new Setter {Property = CustomRadioText.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
        //        new Setter {Property = CustomRadioText.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
        //        new Setter {Property = CustomRadioText.FlowDirectionProperty , Value = DefaultFlowDirection},
        //        new Setter {Property = CustomRadioText.MarginProperty, Value = new Thickness(0,-15,0,0)},
        //    }
        //};

        //Style secondaryRadioButtonStyle = new Style(typeof(CustomRadioText))
        //{
        //    BasedOn = defaultRadioButtonStyle,
        //    Setters =
        //    {
        //        new Setter {Property = CustomRadioText.MarginProperty, Value = new Thickness(0,0,0,0)},
        //        new Setter {Property = CustomRadioText.PaddingProperty, Value = new Thickness(0,-6,0,0)},
        //    }
        //};

        //Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_RADIO_BUTTON_KEY, defaultRadioButtonStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_RADIO_BUTTON_KEY, secondaryRadioButtonStyle);
    }
}
