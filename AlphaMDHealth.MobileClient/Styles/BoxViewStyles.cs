using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateBoxViewStyle()
    {
        Style colorBoxStyle = new Style(typeof(BoxView))
        {
            Setters =
            {
                new Setter { Property = BoxView.FlowDirectionProperty , Value = DefaultFlowDirection},
                new Setter { Property = BoxView.VerticalOptionsProperty , Value = LayoutOptions.Center },
                new Setter { Property = BoxView.WidthRequestProperty, Value = (int)AppImageSize.ImageSizeD },
                new Setter { Property = BoxView.HeightRequestProperty, Value = (int)AppImageSize.ImageSizeD },
                new Setter { Property = BoxView.CornerRadiusProperty, Value = ((int)AppImageSize.ImageSizeD)/2 },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_COLOR_BOX_STYLE, colorBoxStyle);

        var width = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0);

        Style lightBackgroundSeparatorStyle = new Style(typeof(BoxView))
        {
            Setters =
            {
                new Setter { Property = BoxView.HorizontalOptionsProperty, Value = LayoutOptions.Center },
                new Setter { Property = BoxView.VerticalOptionsProperty , Value =LayoutOptions.Center },
                new Setter { Property = BoxView.BackgroundColorProperty, Value = _separatorAndDisableColor },
                new Setter { Property = BoxView.ColorProperty, Value = _separatorAndDisableColor },
                new Setter { Property = BoxView.WidthRequestProperty, Value = width*0.6 },
                new Setter { Property = BoxView.HeightRequestProperty, Value = 1 },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_BACKGROUND_SEPARATOR_STYLE, lightBackgroundSeparatorStyle);

        //Style whiteSeparatorStyle = new Style(typeof(BoxView))
        //{
        //    BasedOn = lightBackgroundSeparatorStyle,
        //    Setters =
        //    {
        //            new Setter { Property = BoxView.BackgroundColorProperty, Value = _genericBackgroundColor },
        //            new Setter { Property = BoxView.OpacityProperty, Value = 0.3 },
        //            new Setter { Property = BoxView.ColorProperty, Value = _genericBackgroundColor },
        //            new Setter { Property = BoxView.HeightRequestProperty, Value = 1 },
        //    }
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_WHITE_SEPARATOR_STYLE, whiteSeparatorStyle);
    }
}