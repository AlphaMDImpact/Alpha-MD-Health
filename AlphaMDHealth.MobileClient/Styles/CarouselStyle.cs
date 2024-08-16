using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateCarouselStyles()
    {
        Style carouselStyle = new Style(typeof(CarouselView))
        {
            Setters =
            {
                new Setter {Property = CarouselView.FlowDirectionProperty , Value = DefaultFlowDirection},
                new Setter {Property = CarouselView.LoopProperty, Value = true},
                new Setter {Property = CarouselView.HorizontalOptionsProperty, Value = LayoutOptions.Fill },
                new Setter {Property = CarouselView.VerticalOptionsProperty ,Value= LayoutOptions.Center,},
                new Setter {Property = CarouselView.MarginProperty ,Value= 0},
                new Setter {Property = CarouselView.IsScrollAnimatedProperty ,Value= true},
                new Setter {Property = CarouselView.IsSwipeEnabledProperty ,Value= true},
                new Setter {Property = CarouselView.IsBounceEnabledProperty ,Value= true},
            },
        };

        Style indicatorStyle = new Style(typeof(IndicatorView))
        {
            Setters =
            {
                new Setter {Property = IndicatorView.FlowDirectionProperty , Value = DefaultFlowDirection},
                new Setter {Property = IndicatorView.IndicatorColorProperty, Value = Color.FromArgb(StyleConstants.SECONDARY_TEXT_COLOR)},
                new Setter {Property = IndicatorView.SelectedIndicatorColorProperty, Value = Color.FromArgb(StyleConstants.SECONDARY_APP_COLOR) },
                new Setter {Property = IndicatorView.IndicatorSizeProperty, Value = 10},
                new Setter {Property = IndicatorView.HorizontalOptionsProperty, Value = LayoutOptions.Center},
                new Setter {Property = IndicatorView.MaximumVisibleProperty, Value = 5},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_INDICATOR_STYLE, indicatorStyle);
        Application.Current.Resources.Add(StyleConstants.ST_CAROUSEL_STYLE, carouselStyle);
    }
}