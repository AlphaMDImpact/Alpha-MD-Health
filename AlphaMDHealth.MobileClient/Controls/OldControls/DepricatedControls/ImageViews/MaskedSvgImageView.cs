using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    public class MaskedSvgImageView : Frame
    {
        private readonly Image _picImage;
        public MaskedSvgImageView(string svgPath)
        {
            _picImage = new Image
            {
                Margin = 0,
                HeightRequest= App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0) - 250,
                WidthRequest=-1,
                Aspect = Aspect.AspectFill,
            };
            ChangeImage(svgPath);
            Padding = new Thickness(0);
            BorderColor = Colors.Transparent;
            CornerRadius = 0;
            IsClippedToBounds = true;
            BackgroundColor = Colors.White;
            Content = _picImage;
        }

        public void ChangeImage(string svgPath)
        {
            if (!string.IsNullOrWhiteSpace(svgPath))
            {
                _picImage.Source = ImageSource.FromFile(svgPath);
            }
        }
    }
}
