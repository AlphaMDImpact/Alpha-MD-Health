using System.Reflection;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Custom Button
    /// </summary>
    public class CustomButton : Button
    {
        /// <summary>
        /// On Invali date event
        /// </summary>
        public event EventHandler OnInvalidate;
        
        // if working remove this
        /// <summary>
        /// The path to the svg file
        /// </summary>
        public static readonly BindableProperty SvgPathProperty = BindableProperty.Create(nameof(SvgPath), typeof(string), typeof(CustomButton));

        /// <summary>
        /// The assembly containing the svg file
        /// </summary>
        private readonly BindableProperty SvgAssemblyProperty = BindableProperty.Create(nameof(SvgAssembly), typeof(Assembly), typeof(CustomButton));

        // remove this if not used in atom
        /// <summary>
        /// The HasBorder property
        /// </summary>
        private readonly BindableProperty HasShadowProperty = BindableProperty.Create(nameof(HasShadow), typeof(bool), typeof(CustomButton), false);

        /// <summary>
        /// Margin for svg
        /// </summary>
        public static readonly BindableProperty SvgMarginPercentageProperty = BindableProperty.Create(nameof(SvgMarginPercentage), typeof(Thickness), typeof(CustomButton), default(Thickness));

        /// <summary>
        /// Bindable property for button content vertical alignment.
        /// </summary>
        public static readonly BindableProperty VerticalContentAlignmentProperty = BindableProperty.
            Create(nameof(VerticalContentAlignment), typeof(TextAlignment), typeof(CustomButton), TextAlignment.Center);

        /// <summary>
        /// Bindable property for button content horizontal alignment.
        /// </summary>       
        public static readonly BindableProperty HorizontalContentAlignmentProperty = BindableProperty.Create(nameof(HorizontalContentAlignment), typeof(TextAlignment), typeof(CustomButton), TextAlignment.Center);

        /// <summary>
        /// Bindable property for disabled button text color.
        /// </summary>
        public static readonly BindableProperty DisabledTextColorProperty = BindableProperty.Create(nameof(DisabledTextColor), typeof(Color), typeof(CustomButton), Colors.Transparent);



        /// <summary>
        /// The path to the svg file
        /// </summary>
        public string SvgPath
        {
            get => (string)GetValue(SvgPathProperty);
            set => SetValue(SvgPathProperty, value);
        }

        /// <summary>
        /// Margin for svg
        /// </summary>
        public Thickness SvgMarginPercentage
        {
            get => (Thickness)GetValue(SvgMarginPercentageProperty);
            set => SetValue(SvgMarginPercentageProperty, value);
        }

        /// <summary>
        /// The assembly containing the svg file
        /// </summary>
        public Assembly SvgAssembly
        {
            get => (Assembly)GetValue(SvgAssemblyProperty);
            set => SetValue(SvgAssemblyProperty, value);
        }

        /// <summary>
        /// Gets or sets the content vertical alignment.
        /// </summary>
        public TextAlignment VerticalContentAlignment
        {
            get => (TextAlignment)GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the content horizontal alignment.
        /// </summary>
        public TextAlignment HorizontalContentAlignment
        {
            get => (TextAlignment)GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the disabled button text color.
        /// </summary>
        public Color DisabledTextColor
        {
            get => (Color)GetValue(DisabledTextColorProperty);
            set => SetValue(DisabledTextColorProperty, value);
        }

        /// <summary>
        /// Gets or sets if the border should be shown or not
        /// </summary>
        public bool HasShadow
        {
            get => (bool)GetValue(HasShadowProperty);
            set => SetValue(HasShadowProperty, value);
        }

        /// <summary>
        /// clean the view.
        /// </summary>
        public void Invalidate()
        {
            OnInvalidate?.Invoke(this, EventArgs.Empty);
        }


        ///// <summary>
        ///// Invoked when binding context chnaged
        ///// </summary>
        //protected override void OnBindingContextChanged()
        //{
        //    try
        //    {
        //        base.OnBindingContextChanged();
        //        Invalidate();
        //    }
        //    catch
        //    {
        //        //To be implemented
        //    }
        //}

        ///// <summary>
        ///// This method invoked when property chnaged event occured
        ///// </summary>
        ///// <param name="propertyName">Name of the property changed</param>
        //protected override void OnPropertyChanged(string propertyName = null)
        //{
        //    try
        //    {
        //        base.OnPropertyChanged(propertyName);
        //        //Changed SVG resource or assembly for SVG resource; load new one.
        //        if (propertyName == SvgPathProperty.PropertyName || propertyName == SvgAssemblyProperty.PropertyName)
        //        {
        //            Invalidate();
        //        }
        //    }
        //    catch
        //    {
        //        //To be implemented
        //    }
        //}
    }
}
