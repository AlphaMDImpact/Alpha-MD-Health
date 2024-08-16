namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Custom Label
    /// </summary>
    public class CustomLabel : Label
    {
        /// <summary>
        ///  Curved Corner Radius Property
        /// </summary>
        public static readonly BindableProperty CurvedCornerRadiusProperty =
            BindableProperty.Create(nameof(CurvedCornerRadius), typeof(double), typeof(CustomLabel), 12.0);

        /// <summary>
        ///  Gets or sets Curved Corner Radius
        /// </summary>
        public double CurvedCornerRadius
        {
            get { return (double)GetValue(CurvedCornerRadiusProperty); }
            set { SetValue(CurvedCornerRadiusProperty, value); }
        }

        /// <summary>
        ///  Curved Background Color Property
        /// </summary>
        public static readonly BindableProperty CurvedBackgroundColorProperty =
            BindableProperty.Create(nameof(CurvedBackgroundColor), typeof(Color), typeof(CustomLabel), Colors.Transparent);

        /// <summary>
        ///  Gets or sets the urved Background Color
        /// </summary>
        public Color CurvedBackgroundColor
        {
            get { return (Color)GetValue(CurvedBackgroundColorProperty); }
            set { SetValue(CurvedBackgroundColorProperty, value); }
        }

        /// <summary>
        /// Is Under Line Property with default value as false
        /// </summary>
        public static readonly BindableProperty MaxWidthRequestProperty = BindableProperty.Create(nameof(MaxWidthRequest), typeof(double), typeof(CustomLabel), default(double));

        /// <summary>
        ///  Gets or sets the Max Width Request
        /// </summary>
        public double MaxWidthRequest
        {
            get => (double)GetValue(MaxWidthRequestProperty);
            set => SetValue(MaxWidthRequestProperty, value);
        }
        /// <summary>
        /// Is Under Line Property with default value as false
        /// </summary>
        public static readonly BindableProperty GetLineCountProperty = BindableProperty.Create(nameof(GetLineCount),  typeof(double), typeof(CustomLabel), default(double), defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        ///  Gets or sets the Max Width Request
        /// </summary>
        public double GetLineCount
        {
            get => (double)GetValue(GetLineCountProperty);
            set => SetValue(GetLineCountProperty, value);
        }
        /// <summary>
        /// Is Html Label Line CountProperty
        /// </summary>
        public static readonly BindableProperty IsHtmlLabelLineCountProperty = BindableProperty.Create(nameof(IsHtmlLabelLineCount), typeof(bool), typeof(CustomLabel), false);

        /// <summary>
        /// Gets or Sets IsUnderLine property
        /// </summary>
        public bool IsHtmlLabelLineCount
        {
            get
            {
                return (bool)GetValue(IsHtmlLabelLineCountProperty);
            }
            set
            {
                SetValue(IsHtmlLabelLineCountProperty, value);
            }
        }
        /// <summary>
        /// Is Under Line Property with default value as false
        /// </summary>
        public static readonly BindableProperty IsHtmlLabelProperty = BindableProperty.Create(nameof(IsHtmlLabel), typeof(bool), typeof(CustomLabel), false);

        /// <summary>
        /// Gets or Sets IsUnderLine property
        /// </summary>
        public bool IsHtmlLabel
        {
            get
            {
                return (bool)GetValue(IsHtmlLabelProperty);
            }
            set
            {
                SetValue(IsHtmlLabelProperty, value);
            }
        }

    }
}
