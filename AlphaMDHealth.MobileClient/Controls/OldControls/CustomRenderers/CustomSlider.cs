namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom radio text list
    /// </summary>
    public class CustomSlider : Slider
    {
        /// <summary>
        /// slider color property
        /// </summary>
        public static readonly BindableProperty SliderColorProperty = BindableProperty.Create(nameof(SliderColor), typeof(Color), typeof(CustomSlider), Colors.Transparent);

        /// <summary>
        /// slider color property
        /// </summary>
        public Color SliderColor
        {
            get => (Color)base.GetValue(SliderColorProperty);
            set => base.SetValue(SliderColorProperty, value);
        }

        /// <summary>
        /// thumb base 64 property
        /// </summary>
        public static readonly BindableProperty ThumbBase64Property = BindableProperty.Create(nameof(ThumbBase64), typeof(string), typeof(CustomSlider), string.Empty);

        /// <summary>
        /// thumb base 64 property
        /// </summary>
        public string ThumbBase64
        {
            get => (string)base.GetValue(ThumbBase64Property);
            set => base.SetValue(ThumbBase64Property, value);
        }

        /// <summary>
        /// image resource property
        /// </summary>
        public static readonly BindableProperty ImageResourceProperty = BindableProperty.Create(nameof(ImageResource), typeof(string), typeof(CustomSlider), string.Empty);

        /// <summary>
        /// image resource property
        /// </summary>
        public string ImageResource
        {
            get => (string)base.GetValue(ImageResourceProperty);
            set => base.SetValue(ImageResourceProperty, value);
        }
    }
}
