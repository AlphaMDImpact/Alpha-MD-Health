namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Custom Gradient
    /// </summary>
    public class CustomGradient : ContentView
    {
        /// <summary>
        /// BindableProperty for Start color for Gredient
        /// </summary>
        public static readonly BindableProperty StartColorProperty = BindableProperty.Create(nameof(StartColor), typeof(Color), typeof(CustomGradient), Colors.Transparent);

        /// <summary>
        /// BindableProperty for End color for Gredient
        /// </summary>
        public static readonly BindableProperty EndColorProperty = BindableProperty.Create(nameof(EndColor), typeof(Color), typeof(CustomGradient), Colors.Transparent);

        /// <summary>
        ///  BindableProperty for  Gredient Start Position
        /// </summary>
        public static readonly BindableProperty GradientStartPositionProperty = BindableProperty.Create(nameof(GradientStartPosition), typeof(string), typeof(CustomGradient));

        /// <summary>
        /// get and set Gradient Start Position
        /// </summary>
        public string GradientStartPosition
        {
            get => (string)GetValue(GradientStartPositionProperty);
            set => SetValue(GradientStartPositionProperty, value);
        }

        /// <summary>
        /// get and set Gradient Start Color
        /// </summary>
        public Color StartColor
        {
            get => (Color)GetValue(StartColorProperty);

            set => SetValue(StartColorProperty, value);
        }

        /// <summary>
        /// get and set Gradient End Color
        /// </summary>
        public Color EndColor
        {
            get => (Color)GetValue(EndColorProperty);
            set => SetValue(EndColorProperty, value);
        }
    }
}
