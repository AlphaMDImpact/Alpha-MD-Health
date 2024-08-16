namespace AlphaMDHealth.Utility;

public class DataValidators
{
    public bool ValidateNumericField(string FieldType, double? currentValue, bool IsRequired, double MinValue, double MaxValue)
    {
        if (currentValue == null && IsRequired)
        {
            return false;
        }
        else
        {
            if (currentValue >= MinValue && currentValue <= MaxValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool ValidateTextField(string FieldType, bool IsRequired, double MinLength, double MaxLength, string currentValue)
    {
        if (string.IsNullOrWhiteSpace(currentValue) && !IsRequired)
        {
            return true;
        }
        if (string.IsNullOrWhiteSpace(currentValue) && IsRequired)
        {
            return false;
        }
        else
        {
            if (currentValue.Trim().Length >= MinLength && currentValue.Trim().Length <= MaxLength)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool ValidateNumericField(string FieldType, DateTime? currentValue, bool IsRequired, double MinLength, double MaxLength)
    {
        if (currentValue == null && !IsRequired)
        {
            return true;
        }
        if (currentValue == null && IsRequired)
        {
            return false;
        }
        else
        {
            if (currentValue >= DateTime.UtcNow.AddMinutes(MinLength) && currentValue <= DateTime.UtcNow.AddMinutes(MaxLength))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool ValidateSingleSelectField(string FieldType, bool IsRequired, string currentValue)
    {
        return true;
        //todo: this methods need to be implemented
    }

    public bool ValidateDateTimeField(string FieldType, bool IsRequired, string currentValue)
    {
        return true;
        //todo: this methods need to be implemented
    }
    public bool ValidateDateField(string FieldType, bool IsRequired, string currentValue)
    {
        return true;
        //todo: this methods need to be implemented
    }
    public bool ValidateTimeField(string FieldType, bool IsRequired, string currentValue)
    {
        return true;
        //todo: this methods need to be implemented
    }

    public bool ValidateUploadField(string fieldType, bool isRequired, string textValue)
    {
        return true;
        //todo: this methods need to be implemented
    }
}