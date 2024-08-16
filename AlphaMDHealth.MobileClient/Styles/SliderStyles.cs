using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateSliderStyle()
    {
        Style sliderStyle = new Style(typeof(Slider))
        {
            Setters =
                {
                    new Setter { Property = Slider.ThumbColorProperty, Value =  _primaryAppColor },

                    new Setter { Property = Slider.MaximumTrackColorProperty, Value = _secondaryTextColor },

                    new Setter { Property = Slider.MinimumTrackColorProperty, Value = _primaryAppColor }
                }
        };

        Application.Current.Resources.Add(StyleConstants.SLIDER_STYLE, sliderStyle);
    }

}
