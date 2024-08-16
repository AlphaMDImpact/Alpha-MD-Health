using AlphaMDHealth.Utility;
using DevExpress.Maui.Controls;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Represents AppStyles
/// </summary>
public partial class AppStyles
{
    private void CreateSimpleButtonStyle()
    {
        Style defaultSimpleButtonStyle = new(typeof(SimpleButton))
        {
            Setters =
            {
                new Setter { Property = SimpleButton.CornerRadiusProperty, Value =  _controlCornerRadius }
            }
        };

        Style primaryButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _primaryAppColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _genericBackgroundColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_BUTTON_STYLE, primaryButtonStyle);

        Style secondaryButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _secondaryAppColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _genericBackgroundColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_BUTTON_STYLE, secondaryButtonStyle);

        Style tertiaryButtonStyle = new Style(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _tertiaryAppColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _tertiaryTextColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _tertiaryTextColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_BUTTON_STYLE, tertiaryButtonStyle);
               
        Style deleteButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.TextColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _errorColor }
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DELETE_BUTTON_STYLE, deleteButtonStyle);

        // Transparent buttons
        Style primaryTransparentButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _primaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _primaryAppColor },

            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_TRANSPARENT_BUTTON_STYLE, primaryTransparentButtonStyle);

        Style secondaryTransparentButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                 new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _secondaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _secondaryAppColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_TRANSPARENT_BUTTON_STYLE, secondaryTransparentButtonStyle);

        Style tertiaryTransparentButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _tertiaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _tertiaryAppColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_TRANSPARENT_BUTTON_STYLE, tertiaryTransparentButtonStyle);

        Style deleteTransparentButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _errorColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _errorColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DELETE_TRANSPARENT_BUTTON_STYLE, deleteTransparentButtonStyle);

        // Border Transparent buttons
        Style pincodeButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _primaryTextColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _primaryTextColor },
                new Setter { Property = SimpleButton.BorderColorProperty, Value = _separatorAndDisableColor },
                new Setter { Property = SimpleButton.BorderThicknessProperty, Value = 1 },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PINCODE_BUTTON_STYLE, pincodeButtonStyle);

        Style PrimaryBorderTransparentButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _primaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _primaryAppColor },
                new Setter { Property = SimpleButton.BorderColorProperty, Value = _primaryAppColor },
                new Setter { Property = SimpleButton.BorderThicknessProperty, Value = 1.5 },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_BORDER_TRANSPARENT_BUTTON_STYLE, PrimaryBorderTransparentButtonStyle);
        Style secondaryBorderTransparentButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                 new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _secondaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _secondaryAppColor },
                  new Setter { Property = SimpleButton.BorderColorProperty, Value = _secondaryAppColor },
                   new Setter { Property = SimpleButton.BorderThicknessProperty, Value = 1.5 },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_BORDER_TRANSPARENT_BUTTON_STYLE, secondaryBorderTransparentButtonStyle);
        Style tertiaryBorderTransparentButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _tertiaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _tertiaryAppColor },
                 new Setter { Property = SimpleButton.BorderColorProperty, Value = _tertiaryAppColor },
                   new Setter { Property = SimpleButton.BorderThicknessProperty, Value = 1.5 },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_BORDER_TRANSPARENT_BUTTON_STYLE, tertiaryBorderTransparentButtonStyle);
        Style deleteBorderTransparentButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.TextColorProperty, Value = _errorColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _errorColor },
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                  new Setter { Property = SimpleButton.BorderColorProperty, Value = _errorColor },
                   new Setter { Property = SimpleButton.BorderThicknessProperty, Value = 1.5 },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DELETE_BORDER_TRANSPARENT_BUTTON_STYLE, deleteBorderTransparentButtonStyle);

        // Expandable Buttons
        Style primaryExButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
             {
                 new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _primaryAppColor },
                 new Setter { Property = SimpleButton.TextColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _genericBackgroundColor },
                  new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Start},
             }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_EX_BUTTON_STYLE, primaryExButtonStyle);
        Style secondaryExButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _secondaryAppColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _genericBackgroundColor },
                  new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Start},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_EX_BUTTON_STYLE, secondaryExButtonStyle);
        Style tertiaryExButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _tertiaryAppColor },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _tertiaryTextColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _tertiaryTextColor },
                  new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Start},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_EX_BUTTON_STYLE, tertiaryExButtonStyle);
        Style deleteExButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.TextColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _errorColor },
                new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.End },
                  new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Start},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DELETE_EX_BUTTON_STYLE, deleteExButtonStyle);

        // Transparent buttons
        Style primaryTransparentExButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
              {
                  new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                  new Setter { Property = SimpleButton.TextColorProperty, Value = _primaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _primaryAppColor },
                       new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Start},
              }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_TRANSPARENT_EX_BUTTON_STYLE, primaryTransparentExButtonStyle);
        Style secondaryTransparentExButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
             {
                  new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                 new Setter { Property = SimpleButton.TextColorProperty, Value = _secondaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _secondaryAppColor },
                      new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Start},
             }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_TRANSPARENT_EX_BUTTON_STYLE, secondaryTransparentExButtonStyle);
        Style tertiaryTransparentExButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
             {
                 new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                 new Setter { Property = SimpleButton.TextColorProperty, Value = _tertiaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _tertiaryAppColor },
                      new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Start},
             }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_TRANSPARENT_EX_BUTTON_STYLE, tertiaryTransparentExButtonStyle);
        Style deleteTransparentExButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.TextColorProperty, Value = _errorColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _errorColor },
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                     new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Start},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DELETE_TRANSPARENT_EX_BUTTON_STYLE, deleteTransparentExButtonStyle);

        // Border Transparent buttons
        Style primaryBorderTransparentExButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
              {
                  new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                  new Setter { Property = SimpleButton.TextColorProperty, Value = _primaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _primaryAppColor },
                   new Setter { Property = SimpleButton.BorderColorProperty, Value = _primaryAppColor },
                        new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.BorderThicknessProperty, Value = 1.5 },
              }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_BORDER_TRANSPARENT_EX_BUTTON_STYLE, primaryBorderTransparentExButtonStyle);
        Style secondaryBorderTransparentExButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
             {
                  new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                 new Setter { Property = SimpleButton.TextColorProperty, Value = _secondaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _secondaryAppColor },
                   new Setter { Property = SimpleButton.BorderColorProperty, Value = _secondaryAppColor },
                     new Setter { Property = SimpleButton.BorderThicknessProperty, Value = 1.5 },
                          new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Start},
             }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_BORDER_TRANSPARENT_EX_BUTTON_STYLE, secondaryBorderTransparentExButtonStyle);
        Style tertiaryBorderTransparentExButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
             {
                 new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                 new Setter { Property = SimpleButton.TextColorProperty, Value = _tertiaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _tertiaryAppColor },
                  new Setter { Property = SimpleButton.BorderColorProperty, Value = _tertiaryAppColor },
                   new Setter { Property = SimpleButton.BorderThicknessProperty, Value = 1.5 },
                      new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Start},
             }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_BORDER_TRANSPARENT_EX_BUTTON_STYLE, tertiaryBorderTransparentExButtonStyle);
        Style deleteBorderTransparentExButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.TextColorProperty, Value = _errorColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _errorColor },
                 new Setter { Property = SimpleButton.BorderColorProperty, Value = _errorColor },
                   new Setter { Property = SimpleButton.BorderThicknessProperty, Value = 1.5 },
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = _defaultBackgroundColor },
                     new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Start},
                  new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Start},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DELETE_BORDER_TRANSPARENT_EX_BUTTON_STYLE, deleteBorderTransparentExButtonStyle);









        Style transparentSimpleButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.BackgroundColorProperty, Value = Colors.Transparent },
                new Setter { Property = SimpleButton.TextColorProperty, Value = _primaryAppColor },
                new Setter { Property = SimpleButton.IconColorProperty, Value = _primaryAppColor },
                new Setter { Property = SimpleButton.BorderColorProperty, Value = Colors.Transparent },
                new Setter { Property = SimpleButton.DisabledTextColorProperty, Value = _tertiaryTextColor},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TRANSPARENT_SIMPLE_BUTTON_STYLE, transparentSimpleButtonStyle);

        Style actionButtonStyle = new(typeof(SimpleButton))
        {
            BasedOn = defaultSimpleButtonStyle,
            Setters =
            {
                new Setter { Property = SimpleButton.VerticalOptionsProperty, Value = LayoutOptions.Center },
                new Setter { Property = SimpleButton.HorizontalOptionsProperty, Value = LayoutOptions.Center },
                new Setter { Property = SimpleButton.MarginProperty, Value = new Thickness(5) },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_ACTION_BUTTON_STYLE, actionButtonStyle);
    }

    private void CreateButtonStyle()
    {
        CreateSimpleButtonStyle();
    }
}
