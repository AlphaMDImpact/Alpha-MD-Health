namespace AlphaMDHealth.Utility;

/// <summary>
/// value for Content-Type header. Json for application/json or else FormEncoded for application/x-www-form-urlencoded
/// </summary>
public enum HttpContentType
{
    /// <summary>
    /// Type Of HttpContent is "Json"
    /// </summary>
    Json,

    /// <summary>
    /// Type Of HttpContent is "FormEncoded"
    /// </summary>
    FormEncoded
}