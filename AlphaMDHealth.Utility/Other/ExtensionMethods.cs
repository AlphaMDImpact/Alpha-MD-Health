namespace AlphaMDHealth.Utility;

/// <summary>
/// Extension Methods
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// this methodes takes string type value and convert it to specified type of enum
    /// </summary>
    /// <typeparam name="T">Enum Type of return value</typeparam>
    /// <param name="value">string value as input</param>
    /// <returns>Enum value</returns>
    public static T ToEnum<T>(this string value)
    {
        return ConvertToEnum<T>(value, true);
    }

    /// <summary>
    /// this methodes takes string type value and convert it to specified type of enum
    /// </summary>
    /// <typeparam name="T">Enum Type of return value</typeparam>
    /// <param name="value">string value as input</param>
    /// <param name="ignoreCase">ignore case if there ae any</param>
    /// <returns>Enum value</returns>
    public static T ToEnum<T>(this string value, bool ignoreCase)
    {
        return ConvertToEnum<T>(value, ignoreCase);
    }

    private static T ConvertToEnum<T>(string value, bool ignoreCase)
    {
        try
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }
        catch
        {
            return default;
        }
    }
}
