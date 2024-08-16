using System.Runtime.Serialization;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Custom Entry
    /// </summary>
    public class CustomPasswordEntry : DevExpress.Maui.Editors.PasswordEdit
    {
        
        /// <summary>
        /// is decimal property
        /// </summary>
        private readonly BindableProperty IsDecimalProperty = BindableProperty.Create(nameof(IsDecimal), typeof(bool), typeof(CustomEntry), false);

      
        /// <summary>
        /// pipe seperated Validation types
        /// </summary>
        private readonly BindableProperty ValidationsProperty = BindableProperty.
            Create(nameof(Validations), typeof(List<KeyValuePair<ValidationType, string>>), typeof(CustomEntry), new List<KeyValuePair<ValidationType, string>>());

        
        /// <summary>
        ///  Validation range
        /// </summary>
        private readonly BindableProperty RangeProperty = BindableProperty.Create(nameof(Range), typeof(string), typeof(CustomEntry), string.Empty);
        
        /// <summary>
        ///  Validation pattern
        /// </summary>
        private readonly BindableProperty PatternProperty = BindableProperty.Create(nameof(Pattern), typeof(string), typeof(CustomEntry), string.Empty);


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
        /// Gets or sets if the border should be shown or not
        /// </summary>
        public bool IsDecimal
        {
            get => (bool)GetValue(IsDecimalProperty);
            set => SetValue(IsDecimalProperty, value);
        }
    }
}
