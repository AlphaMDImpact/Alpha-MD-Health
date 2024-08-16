using AlphaMDHealth.Utility;
using Microsoft.Maui.Controls.Shapes;

namespace AlphaMDHealth.MobileClient
{
    public partial class AppStyles
    {
        public void CreateImageStyles()
        {
            Style defaultImageStyle = new(typeof(Border))
            {
                Setters =
                {
                    new Setter { Property = Border.StrokeThicknessProperty, Value = 0},
                    new Setter { Property = Border.BackgroundColorProperty, Value = Colors.Transparent},
                    new Setter { Property = Image.AspectProperty, Value = Aspect.AspectFit},
                }
            };
            Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_IMAGE_STYLE, defaultImageStyle);

            #region Only With Border Style   
            Style defaultBorderStyle = new(typeof(Border))
            {
                Setters =
                {
                    new Setter { Property = Border.StrokeThicknessProperty, Value = 2},
                    new Setter { Property = Border.BackgroundColorProperty, Value = Colors.Transparent},
                    new Setter { Property = Border.StrokeProperty, Value = Colors.Black}                 
                }
            };

            Style circleWithBorderStyle = new(typeof(Border))
            {
                BasedOn = defaultBorderStyle,
                Setters =
                {
                    new Setter { Property = Border.StrokeShapeProperty, Value = new Ellipse()},
                }
            };
            Application.Current.Resources.Add(StyleConstants.ST_CIRCLE_WITH_BORDER_STYLE, circleWithBorderStyle);

            Style rectangleWithBorderStyle = new(typeof(Border))
            {
                BasedOn = defaultBorderStyle,
                Setters =
                {
                    new Setter { Property = Border.StrokeShapeProperty, Value = new RoundRectangle(){ CornerRadius = _controlCornerRadius } },
                }
            };
            Application.Current.Resources.Add(StyleConstants.ST_RECTANGLE_WITH_BORDER_STYLE, rectangleWithBorderStyle);
            #endregion

            #region Without Border and Background Style   
            Style circleStyle = new(typeof(Border))
            {
                Setters =
                {
                    new Setter { Property = Border.BackgroundColorProperty, Value = Colors.Transparent},
                    new Setter { Property = Border.StrokeShapeProperty, Value = new Ellipse()},
                }
            };
            Application.Current.Resources.Add(StyleConstants.ST_CIRCLE_STYLE, circleStyle);

            Style rectangleStyle = new(typeof(Border))
            {
                Setters =
                {
                    new Setter { Property = Border.BackgroundColorProperty, Value = Colors.Transparent},
                    new Setter { Property = Border.StrokeShapeProperty, Value = new RoundRectangle(){ CornerRadius = _controlCornerRadius }},
                }
            };
            Application.Current.Resources.Add(StyleConstants.ST_RECTANGLE_STYLE, rectangleStyle);
            #endregion

            #region Only With Background Style
            Style circleWithBackgroundStyle = new(typeof(Border))
            {
                Setters =
                {
                   new Setter { Property = Border.BackgroundColorProperty, Value = _primaryAppColor},
                   new Setter { Property = Border.StrokeShapeProperty, Value = new Ellipse()},
                }
            };
            Application.Current.Resources.Add(StyleConstants.ST_CIRCLE_IMAGE_WITH_BACKGROUND_STYLE, circleWithBackgroundStyle);

            Style rectangleWithBackgroundStyle = new(typeof(Border))
            {
                Setters =
                {
                    new Setter { Property = Border.BackgroundColorProperty, Value = _primaryAppColor},
                    new Setter { Property = Border.StrokeShapeProperty, Value = new RoundRectangle(){ CornerRadius = _controlCornerRadius }},
                }
            };
            Application.Current.Resources.Add(StyleConstants.ST_RECTANGLE_IMAGE_WITH_BACKGROUND_STYLE, rectangleWithBackgroundStyle);
            #endregion
        }
    }
}
