namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom radio text list
    /// </summary>
    public class CustomRoundedBorderView : ContentView
    {
        /// <summary>
        /// corner radious bindable property
        /// </summary>
        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(CustomRoundedBorderView), 0.0);

        /// <summary>
        /// corner radious property
        /// </summary>
        public double CornerRadius
        {
            get => (double)base.GetValue(CornerRadiusProperty);
            set => base.SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// stroke bindable property
        /// </summary>
        public static readonly BindableProperty StrokeProperty = BindableProperty.Create(nameof(Stroke), typeof(Color), typeof(CustomRoundedBorderView), Colors.Transparent);

        /// <summary>
        /// stroke property
        /// </summary>
        public Color Stroke
        {
            get => (Color)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        /// <summary>
        /// stroke thickness bindable property
        /// </summary>
        public static readonly BindableProperty StrokeThicknessProperty = BindableProperty.Create(nameof(StrokeThickness), typeof(Thickness), typeof(CustomRoundedBorderView), default(Thickness));

        /// <summary>
        /// stroke thickness property
        /// </summary>
        public Thickness StrokeThickness
        {
            get => (Thickness)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        /// <summary>
        /// Is clipped to border bindable property
        /// </summary>
        public static readonly BindableProperty IsClippedToBorderProperty = BindableProperty.Create(nameof(IsClippedToBorder), typeof(bool), typeof(CustomRoundedBorderView), default(bool));


        /// <summary>
        /// Is clipped to border property
        /// </summary>
        public bool IsClippedToBorder
        {
            get => (bool)GetValue(IsClippedToBorderProperty);
            set => SetValue(IsClippedToBorderProperty, value);
        }

        // cross-platform way to take into account stroke thickness

        /// <summary>
        /// Layout from corners
        /// </summary>
        /// <param name="x">x data</param>
        /// <param name="y">y data</param>
        /// <param name="width">width fo screen</param>
        /// <param name="height">height of screen</param>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            x += StrokeThickness.Left;
            y += StrokeThickness.Top;

            width -= StrokeThickness.HorizontalThickness;
            height -= StrokeThickness.VerticalThickness;

            base.LayoutChildren(x, y, width, height);
        }
    }
}
