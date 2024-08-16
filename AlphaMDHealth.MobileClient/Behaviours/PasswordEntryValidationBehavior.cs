using System.Globalization;
using System.Text.RegularExpressions;
using AlphaMDHealth.Utility;
namespace AlphaMDHealth.MobileClient
{
	public class PasswordEntryValidationBehavior : Behavior<CustomPasswordEntry>
    {

        private readonly BindableProperty ValidationErrorColorPropertyKey = BindableProperty.Create(nameof(ValidationErrorColor), typeof(Color), typeof(EntryValidationBehavior), Colors.Transparent);
        private readonly BindableProperty IsValidPropertyKey = BindableProperty.Create(nameof(IsValid), typeof(bool), typeof(EntryValidationBehavior), true, BindingMode.TwoWay);
        private readonly BindableProperty ValidationErrorPropertyKey = BindableProperty.Create(nameof(ValidationError), typeof(string), typeof(EntryValidationBehavior), string.Empty);
        private string[] _rangeValidationText;

        /// <summary>
        /// Is valid property of Editor
        /// </summary>
        public bool IsValid
        {
            private set => SetValue(IsValidPropertyKey, value);
            get => (bool)GetValue(IsValidPropertyKey);
        }


        /// <summary>
        /// It is used for Decimal precision value
        /// </summary>
        public int DecimalPrecision { get; set; }

        /// <summary>
        /// It is used for Decimal precision value
        /// </summary>
        public int MaxLength { get; set; }
        /// <summary>
        /// Set Entry is required or not for validation
        /// </summary>
        public bool IsRequiredfield { get; set; }

        /// <summary>
        /// Validation error of Editor
        /// </summary>
        public string ValidationError
        {
            set => SetValue(ValidationErrorPropertyKey, value);
            get => (string)GetValue(ValidationErrorPropertyKey);
        }

        /// <summary>
        /// Validation Error Color of Editor
        /// </summary>
        public Color ValidationErrorColor
        {
            set => SetValue(ValidationErrorColorPropertyKey, value);
            get => (Color)GetValue(ValidationErrorColorPropertyKey);
        }/// <summary>
         /// Attach bindables to super classes
         /// </summary>
         /// <param name="bindable"></param>
        protected override void OnAttachedTo(CustomPasswordEntry bindable)
        {
            if (bindable != null)
            {
                bindable.TextChanged += OnEntryTextChanged;
            }
            base.OnAttachedTo(bindable);
        }

        /// <summary>
        /// Detache from superclasses
        /// </summary>
        /// <param name="bindable"></param>
        protected override void OnDetachingFrom(CustomPasswordEntry bindable)
        {
            if (bindable != null)
            {
                bindable.TextChanged -= OnEntryTextChanged;
            }
            base.OnDetachingFrom(bindable);
        }

        private void OnEntryTextChanged(object sender, EventArgs args)
        {
            CustomPasswordEntry entry = sender as CustomPasswordEntry;
            if (entry.Keyboard == Keyboard.Numeric && !string.IsNullOrWhiteSpace(entry.Text))
            {
                if (!entry.IsDecimal && !entry.Text.All(char.IsDigit))
                {
                    entry.Text = entry.Text.Remove(entry.Text.Length - 1);
                    return;
                }
                if (entry.Text.Contains(Constants.DOT_SEPARATOR) || entry.IsDecimal)
                {
                    if (entry.Text.Contains(Constants.DOT_SEPARATOR) && entry.Text.IndexOf(Constants.DOT_SEPARATOR) != entry.Text.Length - 1 && !Regex.IsMatch(entry.Text.ToString(CultureInfo.InvariantCulture), entry.Pattern))
                    {
                        entry.Text = entry.Text.Remove(entry.Text.Length - 1);
                        return;
                    }
                    else
                    {
                        // do nothing
                    }
                    SetEntryValidation(args, entry);
                }

                if (!double.TryParse(entry.Text.
                    Replace(Constants.SYMBOL_COMMA, Constants.DOT_SEPARATOR), NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE), out _))
                {
                    entry.Text = entry.Text.Remove(entry.Text.Length - 1);
                }
            }
        }

        private void SetEntryValidation(EventArgs args, CustomPasswordEntry entry)
        {
            if (entry.IsDecimal && entry.MaxCharacterCount > 0 && entry.Text.Length >= MaxLength)
            {
                if (entry.Text.Contains(Constants.DOT_SEPARATOR))
                {
                    entry.MaxCharacterCount = MaxLength + 1 + DecimalPrecision;
                }
                else
                {
                    if (entry.Text.Length > MaxLength)
                    {
                        entry.Text = entry.Text.Remove(entry.Text.Length - 1);
                        entry.MaxCharacterCount = MaxLength + 1;
                        return;
                    }
                    //  entry.Text = args.OldTextValue;
                    entry.MaxCharacterCount = MaxLength + 1;
                }
            }
        }

        /// <summary>
        /// Used to validate control
        /// </summary>
        /// <param name="sender"></param>
        public void ValidateControl(object sender)
        {
            IsValid = ValidateControl((CustomPasswordEntry)sender);
            if (IsValid)
            {
                ValidationError = string.Empty;
            }
        }

        /// <summary>
        /// Validate view with their validation logic
        /// </summary>
        /// <param name="entry">Viw on which validation will be applicable</param>
        /// <returns>Validation flag value</returns>
        private bool ValidateControl(CustomPasswordEntry entry)
        {
            bool isValid = false;
            decimal tryOutput = 0;
            if (!string.IsNullOrWhiteSpace(entry.Range))
            {
                _rangeValidationText = entry.Range?.Split(Constants.SYMBOL_COLAN);
            }
            string entryText = entry.Text;
            if (entry.Validations.Count > 0)
            {
                for (int errorIndex = 0; errorIndex < entry.Validations.Count; errorIndex++)
                {
                    ValidateControls(entry, ref isValid, ref tryOutput, entryText, errorIndex);

                    if (!isValid)
                    {
                        isValid = (!IsRequiredfield) ? DisplayErrors(entry, isValid, errorIndex) : DisplayValidationRangeErrors(entry, isValid, errorIndex);
                        return isValid;
                    }
                    else
                    {
                        ValidationError = string.Empty;
                    }
                }
            }
            else
            {
                isValid = true;
            }

            return isValid;
        }

        private bool DisplayValidationRangeErrors(CustomPasswordEntry entry, bool isValid, int errorIndex)
        {
            if (entry.Validations[errorIndex].Key == ValidationType.ValidationRange)
            {
                if ((entry.Validations.Any(x => x.Key == ValidationType.Decimal) && string.IsNullOrWhiteSpace(entry.Text)))
                {
                    IsValid = true;
                }
                else
                {
                    ValidationError = string.Format(CultureInfo.CurrentCulture, entry.Validations[errorIndex].Value, _rangeValidationText[0], _rangeValidationText[1]);
                }
            }
            else
            {
                ValidationError = entry.Validations[errorIndex].Value.Replace("{0}", entry.ClassId);
            }
            return isValid;
        }

        private bool DisplayErrors(CustomPasswordEntry entry, bool isValid, int errorIndex)
        {
            string lenthString = string.Empty;
            if (string.IsNullOrWhiteSpace(entry.Text))
            {
                IsValid = true;
                isValid = true;
            }
            if (entry.Validations[errorIndex].Key == ValidationType.ValidationRange)
            {
                if ((entry.Validations.Any(x => x.Key == ValidationType.Decimal) && string.IsNullOrWhiteSpace(entry.Text)))
                {
                    IsValid = true;
                    isValid = true;
                }
                else
                {
                    ValidationError = string.Format(entry.Validations[errorIndex].Value, _rangeValidationText?[0], _rangeValidationText?[1], CultureInfo.InvariantCulture);
                }
            }
            else if (entry.Validations[errorIndex].Key == ValidationType.PhoneNumber)
            {
                lenthString = " " + entry.MaxCharacterCount;
                ValidationError = entry.Validations[errorIndex].Value.Replace("{0}", lenthString);
            }
            else if (entry.Validations[errorIndex].Key == ValidationType.MinimumLength)
            {
                lenthString = " " + entry.StyleId;
                ValidationError = entry.Validations[errorIndex].Value.Replace("{0}", lenthString);
            }
            else
            {
                ValidationError = entry.Validations[errorIndex].Value;
            }

            return isValid;
        }

        private void ValidateControls(CustomPasswordEntry entry, ref bool isValid, ref decimal tryOutput, string entryText, int errorIndex)
        {
            switch (entry.Validations[errorIndex].Key)
            {
                case ValidationType.IsRequired:
                    isValid = ValidateIsRequired(entryText);
                    break;
                case ValidationType.MinimumLength:
                    isValid = ValidateMinimumLength(entry, entryText);
                    break;
                case ValidationType.ValidationRegxString:
                    isValid = ValidationRegxString(entry, entryText);
                    break;
                case ValidationType.Numeric:
                    isValid = ValidateIsNumeric(entryText);
                    break;
                case ValidationType.PhoneNumber:
                    isValid = ValidatePhoneNumber(entry, entryText);
                    break;
                case ValidationType.Decimal:
                    isValid = decimal.TryParse(entryText, out tryOutput);
                    break;
                case ValidationType.ValidationRange:
                    isValid = ValidateRange(entry);
                    break;
                default:
                    isValid = true;
                    break;
            }
        }

        private bool ValidateRange(CustomPasswordEntry entry)
        {
            bool isValid = false;
            if (!string.IsNullOrWhiteSpace(entry.Text))
            {
                isValid = CheckValidationRange(entry);
            }
            return isValid;
        }

        private bool ValidatePhoneNumber(CustomPasswordEntry entry, string entryText)
        {
            bool isValided = false;
            if (!string.IsNullOrWhiteSpace(entryText))
            {
                isValided = entryText.All(char.IsDigit);
                if (isValided)
                {
                    isValided = entryText.Length == entry.MaxCharacterCount;
                }
            }

            return isValided;
        }

        private bool ValidateIsNumeric(string entryText)
        {
            bool isValid = false;
            if (!string.IsNullOrWhiteSpace(entryText))
            {
                isValid = entryText.All(char.IsDigit);
            }

            return isValid;
        }

        private bool ValidationRegxString(CustomPasswordEntry entry, string entryText)
        {
            bool isValid;
            try
            {
                isValid = Regex.Match(entryText, entry.Pattern).Success;
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;
        }

        private bool CheckMinLengthValidation(CustomPasswordEntry entry, string entryText)
        {
            CultureInfo metricCulture = CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE);
            if (!double.TryParse(entryText, NumberStyles.AllowDecimalPoint, metricCulture, out _))
            {
                return entryText.Trim().Length >= Convert.ToInt16(entry.StyleId, CultureInfo.CurrentCulture);
            }
            else
            {
                double val = Convert.ToDouble(entryText, metricCulture);
                return val >= Convert.ToInt16(entry.StyleId, CultureInfo.CurrentCulture);
            }
        }

        private bool ValidateMinimumLength(CustomPasswordEntry entry, string entryText)
        {
            bool isValid = false;
            if (!string.IsNullOrWhiteSpace(entryText))
            {
                isValid = CheckMinLengthValidation(entry, entryText);
            }

            return isValid;
        }

        private bool ValidateIsRequired(string entryText)
        {
            bool isValid = !string.IsNullOrWhiteSpace(entryText);
            IsRequiredfield = !isValid;
            return isValid;
        }

        private bool CheckValidationRange(CustomPasswordEntry entry)
        {
            bool isValid = false;
            CultureInfo metricCulture = CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE);
            _rangeValidationText = entry.Range?.Split(Constants.SYMBOL_COLAN);

            if (!entry.Text.Contains(Constants.SYMBOL_COLAN))
            {
                if (entry.Text.Any(char.IsDigit))
                {
                    if (!double.TryParse(entry.Text.Replace(Constants.SYMBOL_COMMA, Constants.DOT_SEPARATOR),
                        NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE), out _))
                    {
                        isValid = false;
                    }
                    else
                    {
                        double val = Convert.ToDouble(entry.Text.Replace(Constants.SYMBOL_COMMA, Constants.DOT_SEPARATOR), metricCulture);
                        isValid = !(val < Convert.ToDouble(_rangeValidationText[0], metricCulture) || val > Convert.ToDouble(_rangeValidationText[1], metricCulture));
                    }
                }
            }
            else
            {
                isValid = false;
            }
            return isValid;
        }
    }
}

