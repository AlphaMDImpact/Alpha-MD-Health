using AlphaMDHealth.Utility;
using CommunityToolkit.Maui.Behaviors;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Class to handle the SVG image view
/// </summary>
public class SvgImageView : Image
{
    /// <summary>
    /// Creates Svg image
    /// </summary>
    /// <param name="svgPath">Svg image path</param>
    /// <param name="svgWidth">Svg image width</param>
    /// <param name="svgHeight">Svg image height</param>
    public SvgImageView(string svgPath, double svgWidth, double svgHeight)
    {
        SetProperties(svgPath, svgWidth, svgHeight, Colors.Transparent);
    }

    /// <summary>
    /// Creates Svg image
    /// </summary>
    /// <param name="svgPath">Svg image path</param>
    /// <param name="svgWidth">Svg image width</param>
    /// <param name="svgHeight">Svg image height</param>
    /// <param name="svgColor">Svg tint color</param>
    public SvgImageView(string svgPath, AppImageSize svgWidth, AppImageSize svgHeight, Color svgColor)
    {
        SetProperties(svgPath, svgWidth, svgHeight, svgColor);
    }

    private void SetProperties(string svgPath, double svgWidth, double svgHeight, Color svgColor)
    {
        WidthRequest = MobileConstants.IsAndroidPlatform && svgWidth.Equals(svgHeight) ? -1 : svgWidth;
        HeightRequest = svgHeight;
        if (svgColor != Colors.Transparent)
        {
            Behaviors.Add(new IconTintColorBehavior
            {
                TintColor = svgColor
            });
        }
        if (!string.IsNullOrWhiteSpace(svgPath))
        {
            Source = svgPath.Length < 100 
                ? ImageSource.FromFile(svgPath)
                : ImageSource.FromStream(() => GenericMethods.GetMemoryStreamFromBase64(svgPath));
        }
    }

    private void SetProperties(string svgPath, AppImageSize svgWidth, AppImageSize svgHeight, Color svgColor)
    {
        if (svgColor != Colors.Transparent)
        {
            Behaviors.Add(new IconTintColorBehavior
            {
                TintColor = svgColor
            });
        }
        WidthRequest = GetSize(svgWidth, svgWidth);
        HeightRequest = GetSize(svgWidth, svgHeight); 
        if (!string.IsNullOrWhiteSpace(svgPath))
        {
            Source = svgPath.Length < 100
                ? ImageSource.FromFile(svgPath)
                : ImageSource.FromStream(() => GenericMethods.GetMemoryStreamFromBase64(svgPath));
        }
    }

    private int GetSize(AppImageSize s1, AppImageSize s2)
    {
        return s1 == AppImageSize.ImageNone ? -1 : AppStyles.GetImageSize(s2);
    }
}