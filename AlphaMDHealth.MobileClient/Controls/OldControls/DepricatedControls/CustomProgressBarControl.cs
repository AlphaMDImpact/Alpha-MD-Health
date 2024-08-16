using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Custom Progress bar Control
    /// </summary>
    public class CustomProgressBarControl : ProgressBar
    {
        /// <summary>
        /// Custom Progress bar Control
        /// </summary>
        public CustomProgressBarControl()
        {
            ProgressColor = (Color)Application.Current.Resources[StyleConstants.ST_ACCENT_COLOR];
            HeightRequest = 3;
            Margin = 0;
            ScaleY = 3;
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.End;
        }

       
    }
}
