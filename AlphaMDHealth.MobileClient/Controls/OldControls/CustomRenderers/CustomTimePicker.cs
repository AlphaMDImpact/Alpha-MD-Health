using System.Runtime.Serialization;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom time picker
    /// </summary>
    public class CustomTimePicker : TimePicker
    {
        /// <summary>
        /// TimeChanged Event handler
        /// </summary>
        public event EventHandler TimeChanged;

        /// <summary>
        /// Border Color biandable property
        /// </summary>
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(CustomTimePicker), Colors.Transparent);

        /// <summary>
        /// The HasBorder property
        /// </summary>
        public static readonly BindableProperty HasUnderlineProperty = BindableProperty.Create(nameof(HasUnderline), typeof(bool), typeof(CustomEntry), false);

        /// <summary>
        /// Font enabled color property with default color as Color.Default
        /// </summary>
        public static readonly BindableProperty FontEnabledColorProperty = BindableProperty.Create(nameof(FontEnabledColor), typeof(Color), typeof(CustomTimePicker), Colors.Transparent);

        /// <summary>
        /// Font disabled color property with default color as Color.Default
        /// </summary>
        public static readonly BindableProperty FontDisabledColorProperty = BindableProperty.Create(nameof(FontDisabledColor), typeof(Color), typeof(CustomTimePicker), Colors.Transparent);


        /// <summary>
        /// Time picker Placeholder property with default value as empty string
        /// </summary>
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(CustomTimePicker), string.Empty);

        /// <summary>
        /// pipe seperated Validation types
        /// </summary>
        public static readonly BindableProperty ValidationsProperty = BindableProperty.
            Create(nameof(Validations), typeof(List<KeyValuePair<ValidationType, string>>), typeof(CustomTimePicker), new List<KeyValuePair<ValidationType, string>>());

        /// <summary>
        ///  Validation pattern
        /// </summary>
        public static readonly BindableProperty PatternProperty = BindableProperty.Create(nameof(Pattern), typeof(string), typeof(CustomTimePicker), string.Empty);

        /// <summary>
        ///  Validation range
        /// </summary>
        public static readonly BindableProperty RangeProperty = BindableProperty.Create(nameof(Range), typeof(string), typeof(CustomTimePicker), string.Empty);

        /// <summary>
        ///  text allignment
        /// </summary>
        public static readonly BindableProperty TextAllignmentProperty = BindableProperty.Create(nameof(TextAllignment), typeof(TextAlignment), typeof(CustomTimePicker), TextAlignment.Start);

        /// <summary>
        /// Date picker Placeholder property with default value as empty string
        /// </summary>
        public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.
            Create(nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(CustomTimePicker), TextAlignment.Start);
        /// <summary>
        /// Date picker Placeholder property with default value as empty string
        /// </summary>
        public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.
            Create(nameof(VerticalTextAlignment), typeof(TextAlignment), typeof(CustomTimePicker), TextAlignment.Start);

        /// <summary>
        ///  Gets or sets the Time picker Placeholder text
        /// </summary>
        public TextAlignment HorizontalTextAlignment
        {
            get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
            set => SetValue(HorizontalTextAlignmentProperty, value);
        }
        /// <summary>
        /// VerticalTextAlignment
        /// </summary>
        public TextAlignment VerticalTextAlignment
        {
            get => (TextAlignment)GetValue(VerticalTextAlignmentProperty);
            set => SetValue(VerticalTextAlignmentProperty, value);
        }
        /// <summary>
        /// Gets or sets if the border should be shown or not
        /// </summary>
        public bool HasUnderline
        {
            get => (bool)GetValue(HasUnderlineProperty);
            set => SetValue(HasUnderlineProperty, value);
        }
        /// <summary>
        /// Border Color property
        /// </summary>
        public Color BorderColor
        {
            get => (Color)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        /// <summary>
        /// Gets or Sets Font Color
        /// </summary>
        public Color FontEnabledColor
        {
            get => (Color)GetValue(FontEnabledColorProperty);
            set => SetValue(FontEnabledColorProperty, value);
        }

        /// <summary>
        /// Gets or Sets Font Color
        /// </summary>
        public Color FontDisabledColor
        {
            get => (Color)GetValue(FontDisabledColorProperty);
            set => SetValue(FontDisabledColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the Time picker Placeholder text
        /// </summary>
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        /// <summary>
        /// pipe seperated Validation types
        /// </summary>        
        [DataMember]
        public List<KeyValuePair<ValidationType, string>> Validations
        {
            get => (List<KeyValuePair<ValidationType, string>>)GetValue(ValidationsProperty);
            set => SetValue(ValidationsProperty, value);
        }

        /// <summary>
        /// Validation pattern
        /// </summary>
        public string Pattern
        {
            get => (string)GetValue(PatternProperty);
            set => SetValue(PatternProperty, value);
        }

        /// <summary>
        /// Validation range
        /// </summary>
        public string Range
        {
            get => (string)GetValue(RangeProperty);
            set => SetValue(RangeProperty, value);
        }

        /// <summary>
        /// Text Allignment
        /// </summary>
        public TextAlignment TextAllignment
        {
            get => (TextAlignment)GetValue(TextAllignmentProperty);
            set => SetValue(TextAllignmentProperty, value);
        }

        /// <summary>
        /// On time changed event
        /// </summary>
        /// <param name="TimePicker">timepicker object</param>
        public void InvokeTimeChanged(object TimePicker)
        {
            TimeChanged?.Invoke(TimePicker, new EventArgs());
        }
    }
}