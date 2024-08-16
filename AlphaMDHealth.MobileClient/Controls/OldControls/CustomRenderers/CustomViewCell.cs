namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom video cell
    /// </summary>
    public class CustomViewCell : ViewCell
    {
        /// <summary>
        /// select background select
        /// </summary>
        public static readonly BindableProperty
        SelectedBackgroundColorProperty =
            BindableProperty.Create(nameof(SelectedBackgroundColor),
                                    typeof(Color),
                                    typeof(CustomViewCell),
                                    Colors.Transparent);

        /// <summary>
        /// select background select
        /// </summary>
        public Color SelectedBackgroundColor
        {
            get => (Color)GetValue(SelectedBackgroundColorProperty);
            set => SetValue(SelectedBackgroundColorProperty, value);
        }

        /// <summary>
        /// selected text color
        /// </summary>
        public static readonly BindableProperty
     SelectedTextColorProperty =
         BindableProperty.Create(nameof(SelectedTextColor),
                                 typeof(Color),
                                 typeof(CustomViewCell),
                                Colors.Transparent);

        /// <summary>
        /// selected text color
        /// </summary>
        public Color SelectedTextColor
        {
            get => (Color)GetValue(SelectedTextColorProperty);
            set => SetValue(SelectedTextColorProperty, value);
        }
    }
}