namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom radio text
    /// </summary>
    public class CustomRadioText : RadioButton
    {
        /// <summary>
        /// Identifies the TextColor bindable property with default color as Black.
        /// </summary>
        public static readonly BindableProperty ButtonTintColorProperty = BindableProperty.Create(nameof(ButtonTintColor), typeof(Color), typeof(CustomRadioText), Colors.Gray);

        /// <summary>
        /// Identifies the SelectedBackgroundColorProperty bindable property.
        /// </summary>
        public static readonly BindableProperty SelectedBackgroundColorProperty = BindableProperty.Create(nameof(SelectedBackgroundColor), typeof(Color), typeof(CustomRadioText), Colors.Transparent);

        /// <summary>
        /// Radius property with default value as 0.234
        /// </summary>
        public static readonly BindableProperty DefaultRadiusProperty = BindableProperty.Create(nameof(DefaultRadius), typeof(double), typeof(CustomRadioText), 0.6);

        /// <summary>
        /// Size property with default value as 0.1463
        /// </summary>
        public static readonly BindableProperty DefaultSizeProperty = BindableProperty.Create(nameof(DefaultSize), typeof(double), typeof(CustomRadioText), 0.3);

        /// <summary>
        /// Inner size property with default value as 0.1170
        /// </summary>
        public static readonly BindableProperty InnerDefaultSizeProperty = BindableProperty.Create(nameof(InnerDefaultSize), typeof(double), typeof(CustomRadioText), 0.1170);

        /// <summary>
        /// Inner border property with default value as 0.0351
        /// </summary>
        public static readonly BindableProperty InnerDefaultBorderProperty = BindableProperty.Create(nameof(InnerDefaultBorder), typeof(double), typeof(CustomRadioText), 0.0351);


        /// <summary>
        /// Gets or Sets Text color
        /// </summary>
        public Color ButtonTintColor
        {
            get => (Color)GetValue(ButtonTintColorProperty);

            set => SetValue(ButtonTintColorProperty, value);
        }


        /// <summary>
        /// Gets or Sets color of selected radio
        /// </summary>
        public Color SelectedBackgroundColor
        {
            get => (Color)GetValue(SelectedBackgroundColorProperty);

            set => SetValue(SelectedBackgroundColorProperty, value);
        }

        /// <summary>
        /// Gets or Sets Radius
        /// </summary>
        public double DefaultRadius
        {
            get => (double)GetValue(DefaultRadiusProperty);

            set => SetValue(DefaultRadiusProperty, value);
        }

        /// <summary>
        /// Gets or Sets Size
        /// </summary>
        public double DefaultSize
        {
            get => (double)GetValue(DefaultSizeProperty);

            set => SetValue(DefaultSizeProperty, value);
        }

        /// <summary>
        ///IsHorizontal
        /// </summary>
        public bool IsHorizontal { get; set; }

        /// <summary>
        /// Gets or Sets Inner Size
        /// </summary>
        public double InnerDefaultSize
        {
            get => (double)GetValue(InnerDefaultSizeProperty);

            set => SetValue(InnerDefaultSizeProperty, value);
        }

        /// <summary>
        /// Gets or Sets Inner Bodrder
        /// </summary>
        public double InnerDefaultBorder
        {
            get => (double)GetValue(InnerDefaultBorderProperty);

            set => SetValue(InnerDefaultBorderProperty, value);
        }

        /// <summary>
        /// Radio id property
        /// </summary>
        public int RadioId { get; set; }

        /// <summary>
        /// To set radio text vertical alignment property
        /// </summary>
        public long VerticalAlignment { get; set; }
    }
}
