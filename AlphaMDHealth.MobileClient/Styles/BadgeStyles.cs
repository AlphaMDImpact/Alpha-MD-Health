using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateBadgeCountStyle()
    {
        Style badgeCountFrameStyle = new Style(typeof(Frame))
        {
            Setters =
            {
                new Setter { Property = Frame.HeightRequestProperty, Value = mediumLabelSize*1.4 },
                new Setter { Property = Frame.WidthRequestProperty, Value = mediumLabelSize*1.4 },
                new Setter { Property = Frame.CornerRadiusProperty, Value = Convert.ToInt32((mediumLabelSize*1.4)/2.0) },
                new Setter { Property = Frame.BackgroundColorProperty, Value = _accentColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.BADGE_COUNT_FRAME_STYLE, badgeCountFrameStyle);

        Style badgeFrameStyle = new Style(typeof(Frame))
        {
            Setters =
            {
                new Setter { Property = Frame.CornerRadiusProperty, Value = Convert.ToInt32(smallLabelSize/2) },
                new Setter { Property = Frame.HorizontalOptionsProperty , Value = LayoutOptions.Center },
                new Setter { Property = Frame.VerticalOptionsProperty , Value = LayoutOptions.Center },
                new Setter { Property = Frame.PaddingProperty , Value = new Thickness(0) },
                new Setter { Property = Frame.MarginProperty , Value = new Thickness(0) },
            }
        };
        Application.Current.Resources.Add(StyleConstants.BADGE_FRAME_STYLE, badgeFrameStyle);

        Style defaultBadgeLabelStyle = new Style(typeof(Label))
        {
            Setters =
            {
                new Setter { Property = Label.FontSizeProperty , Value =  smallLabelSize },
                new Setter { Property = Label.FontAttributesProperty , Value = FontAttributes.Bold },
                //new Setter { Property = Label.HorizontalTextAlignmentProperty , Value = TextAlignment.Center },
                new Setter { Property = Label.VerticalTextAlignmentProperty , Value = TextAlignment.Center },
                new Setter { Property = Label.PaddingProperty , Value = new Thickness(_appPadding/4) },
                new Setter { Property = Label.MarginProperty , Value = new Thickness(0) },
            }
        };

        Style badgeCountLabelStyle = new Style(typeof(Label))
        {
            BasedOn = defaultBadgeLabelStyle,
            Setters =
            {
                new Setter { Property = Label.FontSizeProperty , Value =  mediumLabelSize },
                new Setter { Property = Label.FontSizeProperty , Value =  new OnIdiom<double> { Phone = mediumLabelSize*0.5, Tablet =  mediumLabelSize*0.5 } },
                new Setter { Property = Label.TextColorProperty , Value = _primaryAppColor },
                new Setter { Property = Label.PaddingProperty , Value = new Thickness(0) },
                new Setter { Property = Label.HorizontalOptionsProperty , Value = LayoutOptions.Center },
                new Setter { Property = Label.VerticalOptionsProperty , Value = LayoutOptions.Center },
                new Setter { Property = Label.MarginProperty , Value = -20 },
            }
        };
        Application.Current.Resources.Add(StyleConstants.BADGE_COUNT_LABEL_STYLE, badgeCountLabelStyle);

        Style primaryBadgeStyle = new Style(typeof(Label))
        {
            BasedOn = defaultBadgeLabelStyle,
            Setters =
            {
                new Setter { Property = Label.BackgroundColorProperty , Value = _primaryAppColor },
                new Setter { Property = Label.TextColorProperty , Value = _genericBackgroundColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.PRIMARY_BADGE_STYLE, primaryBadgeStyle);

        Style secondaryBadgeStyle = new Style(typeof(Label))
        {
            BasedOn = defaultBadgeLabelStyle,
            Setters =
            {
                new Setter { Property = Label.BackgroundColorProperty , Value = _secondaryAppColor },
                new Setter { Property = Label.TextColorProperty , Value = _genericBackgroundColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.SECONDARY_BADGE_STYLE, secondaryBadgeStyle);

        Style infoBadgeStyle = new Style(typeof(Label))
        {
            BasedOn = defaultBadgeLabelStyle,
            Setters =
            {
                //new Setter { Property = Label.BackgroundColorProperty , Value = _tertiaryAppColor },
                new Setter { Property = Label.TextColorProperty , Value = _primaryTextColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.INFO_BADGE_STYLE, infoBadgeStyle);

        Style warningBadgeStyle = new Style(typeof(Label))
        {
            BasedOn = defaultBadgeLabelStyle,
            Setters =
            {
                new Setter { Property = Label.BackgroundColorProperty , Value = _accentColor },
                new Setter { Property = Label.TextColorProperty , Value = _genericBackgroundColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.WARNING_BADGE_STYLE, warningBadgeStyle);

        Style dangerBadgeStyle = new Style(typeof(Label))
        {
            BasedOn = defaultBadgeLabelStyle,
            Setters =
            {
                new Setter { Property = Label.BackgroundColorProperty , Value = _errorColor },
                new Setter { Property = Label.TextColorProperty , Value = _genericBackgroundColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.DANGER_BADGE_STYLE, dangerBadgeStyle);

        Style successBadgeStyle = new Style(typeof(Label))
        {
            BasedOn = defaultBadgeLabelStyle,
            Setters =
            {
                new Setter { Property = Label.BackgroundColorProperty , Value = _successColor },
                new Setter { Property = Label.TextColorProperty , Value = _genericBackgroundColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.SUCCESS_BADGE_STYLE, successBadgeStyle);

        Style lightBadgeStyle = new Style(typeof(Label))
        {
            BasedOn = defaultBadgeLabelStyle,
            Setters =
            {
                new Setter { Property = Label.BackgroundColorProperty , Value = _genericBackgroundColor },
                new Setter { Property = Label.TextColorProperty , Value = _primaryTextColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.LIGHT_BADGE_STYLE, lightBadgeStyle);

        Style darkBadgeStyle = new Style(typeof(Label))
        {
            BasedOn = defaultBadgeLabelStyle,
            Setters =
            {
                new Setter { Property = Label.BackgroundColorProperty , Value = _primaryTextColor },
                new Setter { Property = Label.TextColorProperty , Value = _genericBackgroundColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.DARK_BADGE_STYLE, darkBadgeStyle);
    }
}