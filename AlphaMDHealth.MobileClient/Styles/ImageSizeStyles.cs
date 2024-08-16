using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    /// <summary>
    /// Retrieve image size value from Properties
    /// </summary>
    /// <param name="imageSize">image Size</param>
    /// <returns>image size value</returns>
    public static int GetImageSize(AppImageSize imageSize)
    {
        var imagePropertyName = imageSize.ToString();
        return Application.Current.Resources.ContainsKey(imagePropertyName)
            ? (int)Application.Current.Resources[imagePropertyName]
            : 100;
    }

    private void CreateImageSizeStyle()
    {
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_SIZE_XXLARGE, (int)AppImageSize.ImageSizeXXL);
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_SIZE_XXXLARGE, (int)AppImageSize.ImageSizeXXXL);
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_SIZE_XLARGE, (int)AppImageSize.ImageSizeXL);
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_SIZE_LARGE, (int)AppImageSize.ImageSizeL);
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_SIZE_MEDIUM, (int)AppImageSize.ImageSizeM);
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_SIZE_DEFAULT, (int)AppImageSize.ImageSizeD);
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_SIZE_SMALL, (int)AppImageSize.ImageSizeS);
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_SIZE_EXTRA_SMALL, (int)AppImageSize.ImageSizeXS);
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_SIZE_EXTRA_X_SMALL, (int)AppImageSize.ImageSizeXXS);
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_SIZE_EXTRA_XX_SMALL, (int)AppImageSize.ImageSizeXXXS);

        Application.Current.Resources.Add(StyleConstants.ST_APP_COMPONENT_PADDING, _appComponentPadding);
        Application.Current.Resources.Add(StyleConstants.ST_APP_PADDING, _appPadding);
        Application.Current.Resources.Add(StyleConstants.ST_CONTROL_CORNER_RADIUS, _controlCornerRadius);
        Application.Current.Resources.Add(StyleConstants.ST_APP_TOP_PADDING, _appTopPadding);
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_SIZE_LOGO_HEIGHT, _imageSizeLogoHeight);
        Application.Current.Resources.Add(StyleConstants.ST_IMAGE_SIZE_LOGO_WIDTH, _imageSizeLogoWidth);
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_CONTROL_HEIGHT, _defaultControlHeight);
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE, _defaultCardHeight);
    }
}