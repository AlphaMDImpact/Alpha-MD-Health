using System.Globalization;
using System.Text.RegularExpressions;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// represents EditorValidationBehavior
    /// </summary>
    public class EditorValidationBehavior : Behavior<CustomMultiLineEntry>
    {
        private readonly BindableProperty IsValidPropertyKey = BindableProperty.Create(nameof(IsValid), typeof(bool), typeof(EditorValidationBehavior), true, BindingMode.TwoWay);


        private bool _isRequiredfield;

        /// <summary>
        /// Is valid property of Editor
        /// </summary>
        public bool IsValid
        {
            private set => SetValue(IsValidPropertyKey, value);
            get => (bool)GetValue(IsValidPropertyKey);
        }

        private static readonly BindablePropertyKey ValidationErrorPropertyKey = BindableProperty.
            CreateReadOnly(nameof(ValidationError), typeof(string), typeof(EditorValidationBehavior), string.Empty);

        /// <summary>
        /// Validation Error Property of editor
        /// </summary>
        public static readonly BindableProperty ValidationErrorProperty = ValidationErrorPropertyKey.BindableProperty;

        /// <summary>
        /// Validation error of Editor
        /// </summary>
        public string ValidationError
        {
            set => SetValue(ValidationErrorPropertyKey, value);
            get => (string)GetValue(ValidationErrorProperty);
        }

        private static readonly BindablePropertyKey ValidationErrorColorPropertyKey = BindableProperty.CreateReadOnly(nameof(ValidationErrorColor), typeof(Color), typeof(EditorValidationBehavior), Colors.Transparent);

        /// <summary>
        /// Validation Error Color Property of Editor
        /// </summary>
        public static readonly BindableProperty ValidationErrorColorProperty = ValidationErrorColorPropertyKey.BindableProperty;


        /// <summary>
        /// Validation Error Color of Editor
        /// </summary>
        public Color ValidationErrorColor
        {
            set => SetValue(ValidationErrorColorPropertyKey, value);
            get => (Color)GetValue(ValidationErrorColorProperty);
        }

        /// <summary>
        /// Attach bindables to super classes
        /// </summary>
        /// <param name="bindable"></param>
        protected override void OnAttachedTo(CustomMultiLineEntry bindable)
        {
            bindable.Focused += OnEntryFocused;
            bindable.TextChanged += OnEditorTextChanged;
            base.OnAttachedTo(bindable);
        }

        /// <summary>
        /// Detache from superclasses
        /// </summary>
        /// <param name="bindable"></param>
        protected override void OnDetachingFrom(CustomMultiLineEntry bindable)
        {
            bindable.Focused -= OnEntryFocused;
            bindable.TextChanged -= OnEditorTextChanged;
            base.OnDetachingFrom(bindable);
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs args)
        {
            ValidateControl(sender);
        }

        private void OnEntryFocused(object sender, FocusEventArgs e)
        {
            ValidateControl(sender);
        }

        /// <summary>
        /// Used to validate control
        /// </summary>
        /// <param name="sender"></param>
        public void ValidateControl(object sender)
        {
            IsValid = ValidateControl((CustomMultiLineEntry)sender);
            if (IsValid)
            {
                ValidationError = string.Empty;
            }
        }

        private bool ValidateMinimumLength(CustomMultiLineEntry entry, string editorText)
        {
            bool isValid = false;
            if (!string.IsNullOrWhiteSpace(editorText))
            {
                isValid = editorText.Trim().Length >= Convert.ToInt16(entry.StyleId, CultureInfo.CurrentCulture);
            }
            return isValid;
        }
        /// <summary>
        /// Validate view with their validation logic
        /// </summary>
        /// <param name="editor">Viw on which validation will be applicable</param>
        /// <returns>Validation flag value</returns>
        private bool ValidateControl(CustomMultiLineEntry editor)
        {
            bool isValid = false;
            for (int errorIndex = 0; errorIndex < editor.Validations.Count; errorIndex++)
            {
                switch (editor.Validations[errorIndex].Key)
                {
                    case ValidationType.IsRequired:
                        isValid = !string.IsNullOrWhiteSpace(editor.Text);
                        _isRequiredfield = !isValid;
                        break;
                    case ValidationType.MinimumLength:
                        isValid = ValidateMinimumLength(editor, editor.Text);
                        break;
                    case ValidationType.ValidationRegxString:
                        try
                        {
                            isValid = !Regex.IsMatch(editor.Text, editor.Pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
                        }
                        catch (RegexMatchTimeoutException)
                        {
                            isValid = true;
                        }
                        break;

                    case ValidationType.ValidationRange:
                        string[] ageArr = editor.Range.Split(Constants.SYMBOL_DASH);
                        int val = Convert.ToInt32(editor.Text, CultureInfo.CurrentCulture);
                        isValid = (val < Convert.ToInt32(ageArr[0], CultureInfo.CurrentCulture) || val > Convert.ToInt32(ageArr[1], CultureInfo.CurrentCulture));
                        break;

                    default:
                        isValid = false;
                        break;
                }
                if (!isValid)
                {
                    return GetErrorMsg(editor, isValid, errorIndex);
                }
            }
            return isValid;
        }

        private bool GetErrorMsg(CustomMultiLineEntry editor, bool isValid, int errorIndex)
        {
            if (!_isRequiredfield)
            {
                if (editor.Validations[errorIndex].Key == ValidationType.MinimumLength && !string.IsNullOrWhiteSpace(editor.Text))
                {
                    string lenthString = " " + editor.StyleId;
                    ValidationError = editor.Validations[errorIndex].Value.Replace("{0}", lenthString);
                }
                else
                {
                   IsValid =  isValid = true;
                }
            }
            else
            {
                if (editor.Validations[errorIndex].Key == ValidationType.IsRequired)
                {
                    ValidationError = editor.Validations[errorIndex].Value.Replace("{0}", editor.ClassId);
                }
            }
            return isValid;
        }
    }
}