using AlphaMDHealth.Utility;
using DevExpress.Maui.Controls;
using System.Globalization;
using Device = Microsoft.Maui.Controls.Device;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Represents AppStyles
/// </summary>
public partial class AppStyles
{
    private void CreateCardsStyle()
    {
        Style cardFrameStyle = new(typeof(Frame))
        {
            Setters =
            {
                new Setter { Property = Frame.PaddingProperty, Value = new Thickness(10) },
                new Setter { Property = Frame.CornerRadiusProperty, Value = 10 },
                new Setter { Property = Frame.BackgroundColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = Frame.BorderColorProperty, Value = _separatorAndDisableColor },
                new Setter { Property = Frame.MarginProperty, Value = new Thickness(5) },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_CARD_FRAME, cardFrameStyle);
    }
}