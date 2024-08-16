namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom tab
    /// </summary>
    public class CustomTab : Tab
    {
        /// <summary>
        /// Bindable property for badge count
        /// </summary>
        public static readonly BindableProperty BadgeCountProperty = BindableProperty.Create(nameof(BadgeCount), typeof(string), typeof(CustomTab));

        /// <summary>
        /// Badge count
        /// </summary>
        public string BadgeCount
        {
            get => (string)GetValue(BadgeCountProperty);
            set => SetValue(BadgeCountProperty, value);
        }

        /// <summary>
        /// Bindable property for disabled button text color
        /// </summary>
        public static readonly BindableProperty BadgeColorProperty = BindableProperty.Create(nameof(BadgeColor), typeof(Color), typeof(CustomTab), Colors.Transparent);

        /// <summary>
        /// Gets or sets the badge color.
        /// </summary>
        public Color BadgeColor
        {
            get => (Color)GetValue(BadgeColorProperty);
            set => SetValue(BadgeColorProperty, value);
        }

        /// <summary>
        /// Bindable property for selected icon
        /// </summary>
        public static readonly BindableProperty SelectedIconProperty = BindableProperty.Create(nameof(SelectedIcon), typeof(ImageSource), typeof(CustomTab), default);

        /// <summary>
        /// Gets or sets the selected icon
        /// </summary>
        public ImageSource SelectedIcon
        {
            get => (ImageSource)GetValue(SelectedIconProperty);
            set => SetValue(SelectedIconProperty, value);
        }

        /// <summary>
        /// Bindable property for unselected icon
        /// </summary>
        public static readonly BindableProperty UnselectedIconProperty = BindableProperty.Create(nameof(UnselectedIcon), typeof(ImageSource), typeof(CustomTab), default);

        /// <summary>
        /// Gets or sets the unselected icon
        /// </summary>
        public ImageSource UnselectedIcon
        {
            get => (ImageSource)GetValue(UnselectedIconProperty);
            set => SetValue(UnselectedIconProperty, value);
        }
    }
}
