namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom radio text list
    /// </summary>
    public class CustomSearchBar : SearchBar
    {
        /// <summary>
        /// border color property
        /// </summary>
        public static readonly BindableProperty BorderFocusColorProperty = BindableProperty.Create(nameof(BorderFocusColor), typeof(Color), typeof(CustomEntry), Colors.Transparent);
        /// <summary>
        /// border color property
        /// </summary>
        public static readonly BindableProperty BorderUnFocusColorProperty = BindableProperty.Create(nameof(BorderUnFocusColor), typeof(Color), typeof(CustomEntry), Colors.Transparent);

        /// <summary>
        /// Border type Rectangle or Underline
        /// </summary>
        public static readonly BindableProperty BorderTypeProperty = BindableProperty.Create(nameof(BorderType), typeof(string), typeof(CustomEntry), string.Empty);


        /// <summary>
        /// Border type Rectangle or Underline
        /// </summary>
        public string BorderType
        {
            get => (string)GetValue(BorderTypeProperty);
            set => SetValue(BorderTypeProperty, value);
        }

        /// <summary>
        /// border color property
        /// </summary>
        public Color BorderFocusColor
        {
            get => (Color)GetValue(BorderFocusColorProperty);
            set => SetValue(BorderFocusColorProperty, value);
        }

        /// <summary>
        /// border color property
        /// </summary>
        public Color BorderUnFocusColor
        {
            get => (Color)GetValue(BorderUnFocusColorProperty);
            set => SetValue(BorderUnFocusColorProperty, value);
        }
    }
}
