namespace AlphaMDHealth.Model;

public static class Validator
{
    public static bool HasRequiredValidationError(ResourceModel resource, bool hasValue)
    {
        bool hasError = resource.IsRequired && !hasValue;
        return hasError;
    }

    public static bool HasMinLengthValidationError(ResourceModel resource, double currentValue)
    {
        bool hasError = resource.MinLength != 0 && currentValue < resource.MinLength;
        return hasError;
    }

    public static bool HasRangeValidationError(ResourceModel resource, double currentValue)
    {
        bool hasError = resource.MaxLength != 0
            && !resource.MinLength.Equals(resource.MaxLength)
            && resource.MinLength < resource.MaxLength
            && currentValue > resource.MaxLength;
        return hasError;
    }
}