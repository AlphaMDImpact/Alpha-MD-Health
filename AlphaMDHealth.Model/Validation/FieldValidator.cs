using AlphaMDHealth.Utility;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Field Validation 
    /// </summary>
    public class FieldValidator
    {
        /// <summary>
        /// ResourceKey to get Label Value
        /// </summary>
        public string ResourceKey { get; set; }

        /// <summary>
        /// ResourceKey to get Label Value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// It is used for Control Type
        /// </summary>
        public FieldTypes Type { get; set; }

        /// <summary>
        /// It is used for Decimal precision value
        /// </summary>
        public int DecimalPrecision { get; set; } = (Constants.DIGITS_AFTER_DECIMAL - 1);

        /// <summary>
        /// Regex pattern
        /// </summary>
        public string RegexPattern { get; set; }

        /// <summary>
        /// Error code in specified field
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Error message in specified field
        /// </summary>
        public string ErrorMessage { get; set; }

        private double? _minLength;
        private double? _maxLength;

        /// <summary> 
        /// use for validation
        /// </summary>
        /// <returns>validation status true/false</returns>
        public bool IsValid(BaseDTO dataObj)
        {
            if (IsRequiredCheck(dataObj))
            {
                if (!ValidateMinMaxValue(dataObj))
                {
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(RegexPattern))
                {
                    return ValidateCustomRegex(dataObj);
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool IsRequiredCheck(BaseDTO dataObj)
        {
            if (LibResources.IsRequired(dataObj.Resources, ResourceKey) 
                && (string.IsNullOrWhiteSpace(Convert.ToString(Value, CultureInfo.InvariantCulture))))
            {
                Error = ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY;
                ErrorMessage = ErrorMessage = string.Format(CultureInfo.InvariantCulture
                    , LibResources.GetResourceValueByKey(dataObj.Resources, Error)
                    , LibResources.GetResourceValueByKey(dataObj.Resources, ResourceKey));
                return false;
            }
            return true;
        }

        private bool ValidateMinMaxValue(BaseDTO dataObj)
        {
            if (!string.IsNullOrWhiteSpace(Convert.ToString(Value, CultureInfo.InvariantCulture)))
            {
                GetMinMaxLength(dataObj);
                switch (Type)
                {
                    case FieldTypes.NumericEntryControl:
                        return ValidateNumericRange(dataObj);
                    case FieldTypes.DecimalEntryControl:
                        return ValidateDecimalRange(dataObj);
                    case FieldTypes.PinCodeControl:
                        return ValidatePinCodeRange(dataObj);
                    case FieldTypes.TextEntryControl:
                        return ValidateTextRange(dataObj);
                    default:
                        return true;
                }
            }
            return true;
        }

        private bool ValidateNumericRange(BaseDTO dataObj)
        {
            if (Convert.ToInt32(Value, CultureInfo.InvariantCulture) < _minLength
                || Convert.ToInt32(Value, CultureInfo.InvariantCulture) > _maxLength)
            {
                Error = ResourceConstants.R_RANGE_VALUE_VALIDATION_KEY;
                ErrorMessage = string.Format(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(dataObj.Resources, Error), _minLength, _maxLength);
                return false;
            }
            return true;
        }

        private bool ValidateDecimalRange(BaseDTO dataObj)
        {
            if (Convert.ToDouble(Value, CultureInfo.InvariantCulture) < _minLength
                || Convert.ToDouble(Value, CultureInfo.InvariantCulture) > _maxLength)
            {
                Error = ResourceConstants.R_RANGE_VALUE_VALIDATION_KEY;
                ErrorMessage = string.Format(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(dataObj.Resources, Error), _minLength, _maxLength);
                return false;
            }
            return true;
        }

        private bool ValidatePinCodeRange(BaseDTO dataObj)
        {
            if ((Convert.ToString(Value, CultureInfo.InvariantCulture)).Length < _maxLength)
            {
                Error = ResourceConstants.R_MINIMUM_LENGTH_VALIDATION_KEY;
                ErrorMessage = string.Format(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(dataObj.Resources, Error), _minLength);
                return false;
            }
            return true;
        }

        private bool ValidateTextRange(BaseDTO dataObj)
        {
            if ((Convert.ToString(Value, CultureInfo.InvariantCulture)).Length < _minLength
                || (_maxLength > 0 && (Convert.ToString(Value, CultureInfo.InvariantCulture)).Length > _maxLength))
            {
                Error = ResourceConstants.R_RANGE_LENGTH_VALIDATION_KEY;
                ErrorMessage = string.Format(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(dataObj.Resources, Error), _minLength, _maxLength);
                return false;
            }
            return true;
        }

        private bool ValidateCustomRegex(BaseDTO dataObj)
        {
            Regex regex = new Regex(RegexPattern);
            if (!regex.IsMatch(Convert.ToString(Value, CultureInfo.InvariantCulture)))
            {
                Error = ErrorCode.InvalidData.ToString();
                ErrorMessage = LibResources.GetResourceValueByKey(dataObj.Resources, Error);
                return false;
            }
            return true;
        }

        private void GetMinMaxLength(BaseDTO dataObj)
        {
            _minLength = LibResources.GetMinLengthValueByKey(dataObj.Resources, ResourceKey);
            _maxLength = LibResources.GetMaxLengthValueByKey(dataObj.Resources, ResourceKey);
            //if (Type == FieldTypes.NumericControl)
            //{
            //    _maxLength = GetMaxLength(Convert.ToString(_minLength, CultureInfo.InvariantCulture).Length
            //        , Convert.ToString(_maxLength, CultureInfo.InvariantCulture).Length);
            //}
            //else if (Type == FieldTypes.DecimalControl)
            //{
            //    _maxLength = Convert.ToString(_maxLength, CultureInfo.InvariantCulture).Length + DecimalPrecision + 2;
            //}
        }

        private double GetMaxLength(double minLength, double maxLength)
        {
            if (minLength > maxLength)
            {
                return minLength;
            }
            else
            {
                return maxLength;
            }
        }
    }
}