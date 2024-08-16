namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Custom Check box
    /// </summary>
    public class CustomCheckBox : CheckBox
    {
        /// <summary>
        /// Default Text Bindable  property with default Text string.Empty
        /// </summary>
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(CustomCheckBox), string.Empty);

        /// <summary>
        /// Text Color Bindable  property with default color as Black
        /// </summary>        
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(CustomCheckBox), Colors.Black);

        /// <summary>
        /// Font Size Bindable  property with default value as -1
        /// </summary>     
        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(CustomCheckBox), -1.0);

        /// <summary>
        /// Selected Background Color  Bindable  property 
        /// </summary>
        public static readonly BindableProperty SelectedBackgroundColorProperty = BindableProperty.Create(nameof(SelectedBackgroundColor), typeof(string), typeof(string));

        /// <summary>
        /// Default Text Bindable  property with default Text string.Empty
        /// </summary>
        public static readonly BindableProperty AddResizeTextProperty = BindableProperty.Create(nameof(AddResizeText), typeof(bool), typeof(CustomCheckBox), false);

        /// <summary>
        /// Gets or Sets Text color property
        /// </summary>
        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);

            set => SetValue(TextColorProperty, value);
        }

        /// <summary>
        /// Gets or Sets Font size property
        /// </summary>
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        /// <summary>
        /// Gets the Text property
        /// </summary>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// Gets selected background color
        /// </summary>
        public string SelectedBackgroundColor
        {
            get => (string)GetValue(SelectedBackgroundColorProperty);
            set => SetValue(SelectedBackgroundColorProperty, value);
        }

        /// <summary>
        /// Get check box ID
        /// </summary>
        public string CheckBoxId { get; set; }


        /// <summary>
        /// Gets or Sets Text color property
        /// </summary>
        public bool AddResizeText
        {
            get => (bool)GetValue(AddResizeTextProperty);

            set => SetValue(AddResizeTextProperty, value);
        }

    }
}
