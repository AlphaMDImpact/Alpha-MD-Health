using System.Runtime.Serialization;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Custom MultiLine Entry
    /// </summary>
    public class CustomMultiLineEntry : Editor
    {
        /// <summary>
        ///  Validation pattern
        /// </summary>
        public static readonly BindableProperty PatternProperty = BindableProperty.Create(nameof(Pattern), typeof(string), typeof(CustomMultiLineEntry), string.Empty);

        /// <summary>
        /// Validation pattern
        /// </summary>
        public string Pattern
        {
            get => (string)GetValue(PatternProperty);
            set => SetValue(PatternProperty, value);
        }

        /// <summary>
        ///  Validation range
        /// </summary>
        public static readonly BindableProperty RangeProperty = BindableProperty.Create(nameof(Range), typeof(string), typeof(CustomMultiLineEntry), string.Empty);

        /// <summary>
        /// Validation range
        /// </summary>
        public string Range
        {
            get => (string)GetValue(RangeProperty);
            set => SetValue(RangeProperty, value);
        }

        /// <summary>
        /// pipe seperated Validation types
        /// </summary>
        private readonly BindableProperty ValidationsProperty = BindableProperty.Create(nameof(Validations), typeof(List<KeyValuePair<ValidationType, string>>), typeof(CustomMultiLineEntry), new List<KeyValuePair<ValidationType, string>>());


        /// <summary>
        /// Border type Rectangle or Underline
        /// </summary>
        public static readonly BindableProperty BorderTypeProperty = BindableProperty.Create(nameof(BorderType), typeof(string), typeof(CustomMultiLineEntry), string.Empty);
        /// <summary>
        /// Entry focus color.
        /// </summary>
        public static readonly BindableProperty EntryFocusColorProperty = BindableProperty.Create(nameof(EntryFocusColor), typeof(Color), typeof(CustomMultiLineEntry), default(Color));

        /// <summary>
        /// Entry unfocus color property
        /// </summary>
        public static readonly BindableProperty EntryUnFocusColorProperty = BindableProperty.Create(nameof(EntryUnFocusColor), typeof(Color), typeof(CustomMultiLineEntry), default(Color));
        /// <summary>
        /// The HasBorder property
        /// </summary>
        public static readonly BindableProperty HasBorderProperty = BindableProperty.Create(nameof(HasBorder), typeof(bool), typeof(CustomMultiLineEntry), true);

        /// <summary>
        /// The HasBorder property
        /// </summary>
        public static readonly BindableProperty AddConstraintProperty = BindableProperty.Create(nameof(AddConstraint), typeof(bool), typeof(CustomMultiLineEntry), false);

        /// <summary>
        /// The HasBorder property
        /// </summary>
        public static readonly BindableProperty ReadOnlyProperty = BindableProperty.Create(nameof(ReadOnly), typeof(bool), typeof(CustomMultiLineEntry), false);

        /// <summary>
        /// Gets or sets Border type
        /// </summary>
        public string BorderType
        {
            get => (string)GetValue(BorderTypeProperty);
            set => SetValue(BorderTypeProperty, value);
        }
        /// <summary>
        ///  Gets or sets Entry Focus Color
        /// </summary>
        public Color EntryFocusColor
        {
            get => (Color)GetValue(EntryFocusColorProperty);
            set => SetValue(EntryFocusColorProperty, value);
        }
        /// <summary>
        /// Entry unfocus Color property
        /// </summary>
        public Color EntryUnFocusColor
        {
            get => (Color)GetValue(EntryUnFocusColorProperty);
            set => SetValue(EntryUnFocusColorProperty, value);
        }

        /// <summary>
        /// Gets or sets if the border should be shown or not
        /// </summary>
        public bool HasBorder
        {
            get => (bool)GetValue(HasBorderProperty);
            set => SetValue(HasBorderProperty, value);
        }

        /// <summary>
        /// Gets or sets if you want to remove constraint 
        /// </summary>
        public bool AddConstraint
        {
            get => (bool)GetValue(AddConstraintProperty);
            set => SetValue(AddConstraintProperty, value);
        }

        /// <summary>
        /// Gets or sets if you want to remove constraint 
        /// </summary>
        public bool ReadOnly
        {
            get => (bool)GetValue(ReadOnlyProperty);
            set => SetValue(ReadOnlyProperty, value);
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
    }
}
