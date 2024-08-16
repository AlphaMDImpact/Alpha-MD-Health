using System.Globalization;
using System.Runtime.Serialization;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Custom Date Picker
    /// </summary>
    public class CustomDatePicker : DatePicker
    {
        /// <summary>
        /// Event handler for Date selected event
        /// </summary>
        public event EventHandler DateSelectedEvent;
        /// <summary>
        /// Event handler for element focused event
        /// </summary>
        public event EventHandler FocusedEvent;
        /// <summary>
        /// Nullable date property
        /// </summary>
        public static readonly BindableProperty NullableDateProperty =
            BindableProperty.Create(nameof(NullableDate), typeof(DateTime?), typeof(CustomDatePicker), defaultBindingMode: BindingMode.TwoWay);


        /// <summary>
        /// Culture Property
        /// </summary>
        public static readonly BindableProperty CultureProperty =
           BindableProperty.Create(nameof(Culture), typeof(CultureInfo), typeof(CustomDatePicker));
        /// <summary>
        /// The formatted text which is used to display culture specific date  
        /// </summary>
        public static readonly BindableProperty FormattedTextProperty = BindableProperty.Create(nameof(FormattedText), typeof(string), typeof(CustomDatePicker));
        /// <summary>
        ///  Validation range
        /// </summary>
        public static readonly BindableProperty RangeProperty = BindableProperty.Create(nameof(Range), typeof(string), typeof(CustomDatePicker), string.Empty);
        /// <summary>
        /// pipe seperated Validation types
        /// </summary>
        public static readonly BindableProperty ValidationsProperty = BindableProperty.Create(nameof(Validations), typeof(List<KeyValuePair<ValidationType, string>>), typeof(CustomDatePicker), new List<KeyValuePair<ValidationType, string>>());
        /// <summary>
        ///  Validation pattern
        /// </summary>
        public static readonly BindableProperty PatternProperty = BindableProperty.Create(nameof(Pattern), typeof(string), typeof(CustomDatePicker), string.Empty);
        /// <summary>
        /// Date picker Placeholder property with default value as empty string
        /// </summary>
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(CustomDatePicker));
        /// <summary>
        /// Entry Background Color Property with default Color as Color.Default
        /// </summary>
        public static readonly BindableProperty DateBackgroundColorProperty = BindableProperty.Create(nameof(DateBackgroundColor), typeof(Color), typeof(CustomDatePicker), Colors.Transparent);
        /// <summary>
        /// Font disabled color property with default color as Color.Default
        /// </summary>
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(CustomDatePicker), Colors.Transparent);

        /// <summary>
        /// Border Color biandable property
        /// </summary>
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(CustomDatePicker), Colors.Transparent);
        /// <summary>
        /// Font disabled color property with default color as Color.Default
        /// </summary>
        public static readonly BindableProperty FontDisabledColorProperty = BindableProperty.Create(nameof(FontDisabledColor), typeof(Color), typeof(CustomDatePicker), Colors.Transparent);
        /// <summary>
        /// Font enabled color property with default color as Color.Default
        /// </summary>
        public static readonly BindableProperty FontEnabledColorProperty = BindableProperty.Create(nameof(FontEnabledColor), typeof(Color), typeof(CustomDatePicker), Colors.Transparent);
        /// <summary>
        /// The HasBorder property
        /// </summary>
        public static readonly BindableProperty HasUnderlineProperty = BindableProperty.Create(nameof(HasUnderline), typeof(bool), typeof(CustomEntry), true);

        /// <summary>
        /// Date picker Placeholder property with default value as empty string
        /// </summary>
        public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.
            Create(nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(CustomDatePicker), TextAlignment.Start);
        /// <summary>
        /// Date picker Placeholder property with default value as empty string
        /// </summary>
        public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.
            Create(nameof(VerticalTextAlignment), typeof(TextAlignment), typeof(CustomDatePicker), TextAlignment.Start);

        /// <summary>
        /// Original Format Property
        /// </summary>
        public string OriginalFormat { get; set; }

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
        /// Placeholder color Property
        /// </summary>
        public Color PlaceholderColor
        {
            get => (Color)GetValue(PlaceholderColorProperty);
            set => SetValue(PlaceholderColorProperty, value);
        }
        /// <summary>
        /// Gets or Sets Font Enabled Color
        /// </summary>
        public Color FontEnabledColor
        {
            get => (Color)GetValue(FontEnabledColorProperty);
            set => SetValue(FontEnabledColorProperty, value);
        }
        /// <summary>
        /// Gets or Sets Font Disabled Color
        /// </summary>
        public Color FontDisabledColor
        {
            get => (Color)GetValue(FontDisabledColorProperty);
            set => SetValue(FontDisabledColorProperty, value);
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
        /// Gets or Sets Entry Background Color
        /// </summary>
        public Color DateBackgroundColor
        {
            get => (Color)GetValue(DateBackgroundColorProperty);
            set => SetValue(DateBackgroundColorProperty, value);
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
        /// pipe seperated Validation types
        /// </summary>
        [DataMember]
        public List<KeyValuePair<ValidationType, string>> Validations
        {
            get => (List<KeyValuePair<ValidationType, string>>)GetValue(ValidationsProperty);
            set => SetValue(ValidationsProperty, value);
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
        ///  Formatted Text
        /// </summary>
        public string FormattedText
        {
            get => (string)GetValue(FormattedTextProperty);
            set => SetValue(FormattedTextProperty, value);
        }

        /// <summary>
        /// Placeholder color Property
        /// </summary>
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        /// <summary>
        /// NullableDate Property
        /// </summary>
        public DateTime? NullableDate
        {
            get => (DateTime?)GetValue(NullableDateProperty);
            set
            {
                SetValue(NullableDateProperty, value);
                UpdateDateValue();
            }
        }

        /// <summary>
        /// CultureInfo Property
        /// </summary>
        public CultureInfo Culture
        {
            get => (CultureInfo)GetValue(CultureProperty);
            set => SetValue(CultureProperty, value);
        }

        private void UpdateDateValue()
        {
            if (NullableDate != null && OriginalFormat != null)
            {
                Format = OriginalFormat;
            }
        }

        /// <summary>
        /// Invoked when binding context chnaged
        /// </summary>
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (BindingContext != null)
            {
                OriginalFormat = Format;
                UpdateDateValue();
            }
        }

        /// <summary>
        /// This method invoked when property chnaged event occured
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            // desable date proerty change to void nullable date set when minDate is greater that 0

            if((propertyName == DateProperty.PropertyName && IsFocused)|| (propertyName == IsFocusedProperty.PropertyName && !IsFocused
                && (Date.ToString("d", CultureInfo.CurrentCulture) == DateTime.Now.ToString("d", CultureInfo.CurrentCulture))))
            {
                AssignDateValue();
            }


            if (propertyName == NullableDateProperty.PropertyName && NullableDate.HasValue
                && Date.ToString(OriginalFormat, CultureInfo.CurrentCulture) == DateTime.Now.ToString(OriginalFormat, CultureInfo.CurrentCulture) && NullableDate.HasValue)
            {
                //this code was done because when date selected is the actual date the"DateProperty" does not raise  
                UpdateDateValue();
            }
        }

        /// <summary>
        /// Method Cleans the Date
        /// </summary>
        public void CleanDate()
        {
            NullableDate = null;
            UpdateDateValue();

        }

        /// <summary>
        /// Method is used to Assign Date
        /// </summary>
        public void AssignDateValue()
        {
            ////if(NullableDate==null)
            ////{
            ////    return;
            ////}
            NullableDate = Date;
            UpdateDateValue();
        }

        /// <summary>
        /// Method is Used to Invoke events
        /// </summary>
        /// <param name="buttonType"></param>
        public void InvokeDateSelected(object buttonType)
        {
            DateSelectedEvent?.Invoke(buttonType, new EventArgs());
            FocusedEvent?.Invoke(buttonType, new EventArgs());
        }

    }
}