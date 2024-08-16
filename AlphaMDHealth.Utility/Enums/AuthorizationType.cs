namespace AlphaMDHealth.Utility;

/// <summary>
/// Authorization used to validate client request
/// </summary>
public enum AuthorizationType
{
    /// <summary>
    /// Bearer used for user dependent services
    /// </summary>
    Bearer,

    /// <summary>
    /// Basic used for user non dependent services
    /// </summary>
    Basic,

    /// <summary>
    /// Indicates Authorization header will not be attached with request
    /// </summary>
    NoAuth
}