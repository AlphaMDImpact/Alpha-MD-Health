using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Class to handle SVG image button view
    /// </summary>
    public class SvgImageButtonView : CustomButton
    {
        /// <summary>
        /// Creates a SVG image button view
        /// </summary>
        /// <param name="svgPath">path of the svg image</param>
        /// <param name="svgWidth">width of svg button</param>
        /// <param name="svgHeight">height of svg button</param>
        public SvgImageButtonView(string svgPath, AppImageSize svgWidth, AppImageSize svgHeight)
        {
            if (!string.IsNullOrWhiteSpace(svgPath))
            {
                HorizontalOptions = LayoutOptions.Start;
                VerticalOptions = LayoutOptions.Center;
                BackgroundColor = Colors.Transparent;
                WidthRequest = DeviceInfo.Platform == DevicePlatform.iOS && svgWidth == svgHeight ? -1 : AppStyles.GetImageSize(svgWidth);
                HeightRequest = svgHeight == AppImageSize.ImageNone ? -1 : AppStyles.GetImageSize(svgHeight);
                ImageSource = ImageSource.FromFile(svgPath);
            }
        }
    }
}