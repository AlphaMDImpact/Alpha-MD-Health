namespace AlphaMDHealth.Utility;

/// <summary>
/// Enum which is used to supply http verbs
/// </summary>
public enum HttpMethods
{
    /// <summary>
    /// Represents an HTTP GET protocol method
    /// </summary>
    GET,

    /// <summary>
    /// Represents an HTTP POST protocol method that is used to post a new entity as an addition to a URI
    /// </summary>
    POST,

    /// <summary>
    /// Represents an HTTP PUT protocol method that is used to replace an entity identified by a URI
    /// </summary>
    PATCH,

    /// <summary>
    /// Represents an HTTP DELETE protocol method.
    /// </summary>
    DELETE,
}