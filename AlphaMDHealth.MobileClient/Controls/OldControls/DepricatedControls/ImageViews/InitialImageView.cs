using AlphaMDHealth.Utility;
using Device = Microsoft.Maui.Controls.Device;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Class to handle Initial of image view
    /// </summary>
    public class InitialImageView : Frame
    {
        /// <summary>
        /// Creates a string text in an image view
        /// </summary>
        /// <param name="titleText"> string value to be shown</param>
        /// <param name="imageWidth">width of the image</param>
        /// <param name="imageHeight">height of image</param>
        /// <param name="isCircle">boolean value to decide whether it needs to be circle or not</param>
        public InitialImageView(string titleText, AppImageSize imageWidth, AppImageSize imageHeight, bool isCircle)
        {
            int imageWidthValue = (int)Application.Current.Resources[imageWidth.ToString()];

            CustomLabelControl intialLabel = new(LabelType.SecondryAppExtarSmallCenter)
            {
                Text = titleText,
                VerticalOptions = LayoutOptions.Center,
                FontSize = imageWidthValue / (double)(int.TryParse(titleText, out _) ? 2 : 3)
            };

            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_FRAME_STYLE];
            CornerRadius = isCircle ? Convert.ToSingle(imageWidthValue / 2) : 4;
            HeightRequest = (int)Application.Current.Resources[imageHeight.ToString()];
            WidthRequest = imageWidthValue;
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
            BorderColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
            Content = intialLabel;
        }

        /// <summary>
        /// Creates a string text in an image view
        /// </summary>
        /// <param name="titleText"> string value to be shown</param>
        public InitialImageView(string titleText)
        {
            CustomLabelControl intialLabel = new CustomLabelControl(LabelType.SecondryAppSmallCenter)
            {
                Text = titleText,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(CustomLabel)),
                VerticalOptions = LayoutOptions.Center,
            };

            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_FRAME_STYLE];
            CornerRadius = 4;
            IsClippedToBounds = true;
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
            BorderColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
            Content = intialLabel;
        }
    }
}