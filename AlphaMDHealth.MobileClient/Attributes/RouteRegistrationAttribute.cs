namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Attribute for registring routes
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RouteRegistrationAttribute : Attribute
{
    /// <summary>
    /// Page route
    /// </summary>
    public string Route { get; private set; }

    /// <summary>
    /// Register route for navigation
    /// </summary>
    /// <param name="route">Route for navigation</param>
    public RouteRegistrationAttribute(string route)
    {
        Route = route;
    }
}