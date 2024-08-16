using AlphaMDHealth.Utility;
using Microsoft.Maui.Controls.Shapes;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Class to handle circle image view
    /// </summary>
    public class CircleImageView : Border
    {
        /// <summary>
        /// Creates a circle image view 
        /// </summary>
        /// <param name="source">Image source to whose view needs to be changed</param>
        /// <param name="imageWidth">width of the image</param>
        /// <param name="imageHeight">height of image</param>
        /// <param name="isCircle">boolean value to decide whether it needs to be circle or not</param>
        /// <param name="imageAspect">aspect ration of the image</param>
        public CircleImageView(ImageSource source, AppImageSize imageWidth, AppImageSize imageHeight, bool isCircle, Aspect imageAspect)
        {
            int widthValue = (int)Application.Current.Resources[imageWidth.ToString()];
            int heightValue = (int)Application.Current.Resources[imageHeight.ToString()];
            var picImage = new Image
            {
                Margin = 0,
                Source = source,
                HeightRequest = heightValue,
                WidthRequest = widthValue,
                Aspect = imageAspect,

            };

            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_PANCACKEVIEW_STYLE];
            StrokeShape= new RoundRectangle
            {
                CornerRadius = new CornerRadius( isCircle ? Convert.ToSingle(widthValue / 2) : (float)Application.Current.Resources[StyleConstants.ST_CONTROL_CORNER_RADIUS])
            };

            HeightRequest = heightValue;
            WidthRequest = widthValue;
            Content = picImage;
        }

        /// <summary>
        /// Creates a circle image view 
        /// </summary>
        /// <param name="source">Image source to whose view needs to be changed</param>
        /// <param name="imageAspect">aspect ration of the image</param>
        public CircleImageView(ImageSource source, Aspect imageAspect)
        {
            var PicImage = new Image
            {
                Margin = 0,
                Source = source,
                Aspect = imageAspect,
            };
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_PANCACKEVIEW_STYLE];
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius((float)Application.Current.Resources[StyleConstants.ST_CONTROL_CORNER_RADIUS])
            };
            Content = PicImage;
        }

        /// <summary>
        /// Creates a circle image view 
        /// </summary>
        /// <param name="imageAspect">aspect ration of the image</param>
        /// <param name="imagePathSource">source image url</param>
        /// <param name="imageWidth">width of the image</param>
        /// <param name="imageHeight">height of image</param>
        public CircleImageView(Aspect imageAspect, string imagePathSource, AppImageSize imageWidth, AppImageSize imageHeight)
        {
            int widthValue = (int)Application.Current.Resources[imageWidth.ToString()];
            int heightValue = (int)Application.Current.Resources[imageHeight.ToString()];
            var picImage = new Image
            {
                Margin = 0,
                Source = ImageSource.FromUri(new Uri(imagePathSource)),
                HeightRequest = heightValue,
                WidthRequest = widthValue,
                Aspect = imageAspect
            };
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_PANCACKEVIEW_STYLE];
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius((float)Application.Current.Resources[StyleConstants.ST_CONTROL_CORNER_RADIUS])
            };
            HeightRequest = heightValue;
            WidthRequest = widthValue;
            Content = picImage;
        }

        /// <summary>
        /// Creates a circle image view 
        /// </summary>
        /// <param name="imagePathSource">Image source url</param>
        /// <param name="imageAspect">aspect ration of the image</param>
        public CircleImageView(string imagePathSource, Aspect imageAspect)
        {
            var PicImage = new Image
            {
                Margin = 0,
                Source = ImageSource.FromUri(new Uri(imagePathSource)),
                Aspect = imageAspect,
            };
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_PANCACKEVIEW_STYLE];
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius((float)Application.Current.Resources[StyleConstants.ST_CONTROL_CORNER_RADIUS])
            };
            Content = PicImage;
        }
    }

    /// <summary>
    /// Class to handle rounded image view
    /// </summary>
    public class RoundedImageView : CustomRoundedBorderView
    {
        /// <summary>
        /// Image to be shown
        /// </summary>
        public Image PicImage { get; set; }

        /// <summary>
        /// Creates a rounded image view 
        /// </summary>
        /// <param name="source">Image source to whose view needs to be changed</param>
        /// <param name="imageWidth">width of the image</param>
        /// <param name="imageHeight">height of image</param>
        /// <param name="isCircle">boolean value to decide whether it needs to be circle or not</param>
        /// <param name="imageAspect">aspect ration of the image</param>
        public RoundedImageView(ImageSource source, AppImageSize imageWidth, AppImageSize imageHeight, bool isCircle, Aspect imageAspect)
        {
            int widthValue = (int)Application.Current.Resources[imageWidth.ToString()];
            int heightValue = (int)Application.Current.Resources[imageHeight.ToString()];

            PicImage = new Image
            {
                Margin = 0,
                Source = source,
                HeightRequest = heightValue,
                WidthRequest = widthValue,
                Aspect = imageAspect,
            };
            IsClippedToBorder = true;
            CornerRadius = isCircle ? Convert.ToSingle(widthValue / 2) : (float)Application.Current.Resources[StyleConstants.ST_CONTROL_CORNER_RADIUS];
            HeightRequest = heightValue;
            WidthRequest = widthValue;
            Margin = new Thickness(0);
            Padding = new Thickness(0);
            Content = PicImage;
            BackgroundColor = Colors.Transparent;
        }

        /// <summary>
        /// Creates a circle image view 
        /// </summary>
        /// <param name="source">Image source to whose view needs to be changed</param>
        /// <param name="imageAspect">aspect ration of the image</param>
        public RoundedImageView(ImageSource source, Aspect imageAspect)
        {
            PicImage = new Image
            {
                Margin = 0,
                Source = source,
                Aspect = imageAspect,
            };
            IsClippedToBounds = true;
            Margin = new Thickness(0);
            Padding = new Thickness(0);
            CornerRadius = (float)Application.Current.Resources[StyleConstants.ST_CONTROL_CORNER_RADIUS];
            Content = PicImage;
        }
    }
}