//using System.Reflection;

//namespace AlphaMDHealth.MobileClient
//{
//    /// <summary>
//    /// Represents custom radio text list
//    /// </summary>
//    public class CustomSvgImage : Image
//    {
//        /// <summary>
//        /// invalid date event
//        /// </summary>
//        public event EventHandler OnInvalidate;

//        /// <summary>
//        /// The path to the svg file
//        /// </summary>
//        public static readonly BindableProperty SvgPathProperty = BindableProperty.Create(nameof(SvgPath), typeof(string), typeof(CustomSvgImage));

//        /// <summary>
//        /// The path to the svg file
//        /// </summary>
//        public string SvgPath
//        {
//            get => (string)GetValue(SvgPathProperty);
//            set => SetValue(SvgPathProperty, value);
//        }

//        /// <summary>
//        /// The assembly containing the svg file
//        /// </summary>
//        public static readonly BindableProperty SvgAssemblyProperty = BindableProperty.Create(nameof(SvgAssembly), typeof(Assembly), typeof(CustomSvgImage));

//        /// <summary>
//        /// The assembly containing the svg file
//        /// </summary>
//        public Assembly SvgAssembly
//        {
//            get => (Assembly)GetValue(SvgAssemblyProperty);
//            set => SetValue(SvgAssemblyProperty, value);
//        }

//        /// <summary>
//        /// tint color
//        /// </summary>
//        public static readonly BindableProperty TintColorProperty = BindableProperty.Create(nameof(TintColor), typeof(Color), typeof(CustomSvgImage), Colors.Transparent);

//        /// <summary>
//        /// tint color
//        /// </summary>
//        public Color TintColor
//        {
//            get => (Color)GetValue(TintColorProperty);
//            set => SetValue(TintColorProperty, value);
//        }

//        /// <summary>
//        /// invalid date event
//        /// </summary>
//        public void Invalidate()
//        {
//            OnInvalidate?.Invoke(this, EventArgs.Empty);
//        }

//        /// <summary>
//        /// Bindable context changes event
//        /// </summary>
//        protected override void OnBindingContextChanged()
//        {
//            base.OnBindingContextChanged();
//            Invalidate();
//        }

//        /// <summary>
//        /// On property changed event
//        /// </summary>
//        /// <param name="propertyName"> property name</param>
//        protected override void OnPropertyChanged(string propertyName = null)
//        {
//            base.OnPropertyChanged(propertyName);
//            // Changed SVG resource or assembly for SVG resource; load new one.
//            if (propertyName == SvgPathProperty.PropertyName || propertyName == SvgAssemblyProperty.PropertyName)
//            {
//                Invalidate();
//            }
//        }
//    }
//}