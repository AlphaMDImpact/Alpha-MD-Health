using AlphaMDHealth.Utility;
using Microsoft.Maui.Controls.Shapes;

namespace AlphaMDHealth.MobileClient;
public partial class AppStyles
{
    private void CreateUploadStyle()
    {
        Style defaultUploadFrameStyle = new(typeof(Border))
        {
            Setters =
                {
                    new Setter { Property = Border.StrokeThicknessProperty, Value =  2},
                    new Setter { Property = Border.BackgroundColorProperty, Value = Color.FromArgb(StyleConstants.DEFAULT_BACKGROUND_COLOR)},
                    new Setter { Property = Border.StrokeProperty, Value = _separatorAndDisableColor},
                    new Setter { Property = Border.StrokeShapeProperty, Value = new RoundRectangle(){ CornerRadius = _controlCornerRadius } },
                    new Setter { Property = Border.PaddingProperty, Value = 2 },
                }
        };

        Style disabledUploadFrameStyle = new(typeof(Border))
        {
            BasedOn = defaultUploadFrameStyle,
            Setters =
                {
                    new Setter {Property = Border.BackgroundColorProperty, Value = Color.FromArgb(StyleConstants.DEFAULT_BACKGROUND_COLOR) },
                    new Setter {Property = Border.StyleProperty, Value = Color.FromArgb(StyleConstants.DEFAULT_BACKGROUND_COLOR) },
                }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_UPLOAD_FRAME_STYLE, defaultUploadFrameStyle);
        Application.Current.Resources.Add(StyleConstants.ST_DISABLED_UPLOAD_FRAME_STYLE, disabledUploadFrameStyle);
    }
}

