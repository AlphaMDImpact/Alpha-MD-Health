namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Custom Bindable Picker class
    /// </summary>
    public class CustomBindablePicker : Picker
    {
        /// <summary>
        /// Constructor for Custom Bindable Picker class
        /// </summary>
        public CustomBindablePicker()
        {
            SelectedIndexChanged += OnSelectedIndexChanged;
        }

        /// <summary>
        /// Font Enabled Color Property
        /// </summary>
        public static readonly BindableProperty FontEnabledColorProperty = BindableProperty.Create(nameof(FontEnabledColor), typeof(Color), typeof(CustomBindablePicker), Colors.Transparent);
        /// <summary>
        /// Font Disabled Color Property
        /// </summary>
        public static readonly BindableProperty FontDisabledColorProperty = BindableProperty.Create(nameof(FontDisabledColor), typeof(Color), typeof(CustomBindablePicker), Colors.Transparent);
        /// <summary>
        /// Placeholder Color Property
        /// </summary>
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(CustomBindablePicker), Colors.Transparent);

        /// <summary>
        /// Border type Rectangle or Underline
        /// </summary>
        private readonly BindableProperty BorderTypeProperty = BindableProperty.Create(nameof(BorderType), typeof(string), typeof(CustomBindablePicker), string.Empty);

        /// <summary>
        /// Bordertype Property
        /// </summary>
        public string BorderType
        {
            get => (string)GetValue(BorderTypeProperty);
            set => SetValue(BorderTypeProperty, value);
        }
        /// <summary>
        /// border color
        /// </summary>
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(CustomBindablePicker), Colors.Transparent);
        
        /// <summary>
        /// border color property
        /// </summary>
        public Color BorderColor
        {
            get => (Color)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        /// <summary>
        /// Placeholder Color Property
        /// </summary>
        public Color PlaceholderColor
        {
            get => (Color)GetValue(PlaceholderColorProperty);
            set => SetValue(PlaceholderColorProperty, value);
        }

        /// <summary>
        /// Enable Font Color property
        /// </summary>
        public Color FontEnabledColor
        {
            get => (Color)GetValue(FontEnabledColorProperty);
            set => SetValue(FontEnabledColorProperty, value);
        }

        /// <summary>
        ///  Disable Font Color property
        /// </summary>
        public Color FontDisabledColor
        {
            get => (Color)GetValue(FontDisabledColorProperty);
            set => SetValue(FontDisabledColorProperty, value);
        }

        //added for issue https://bugzilla.xamarin.com/show_bug.cgi?id=28618
        /// <summary>
        /// This method invoked when property chnaged event occured
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == Picker.SelectedIndexProperty.PropertyName)
            {
                InvalidateMeasure();
            }
        }

        private void OnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            SelectedItem = (SelectedIndex < 0 || SelectedIndex > Items.Count - 1) ? null : Items[SelectedIndex];
        }
    }
}
