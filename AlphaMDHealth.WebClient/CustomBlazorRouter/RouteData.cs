using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace AlphaMDHealth.WebClient
{
    /// <summary>
    /// Describes information determined during routing that specifies
    /// the page to be displayed.
    /// </summary>
    public sealed class RouteData
    {
        /// <summary>
        /// Constructs an instance of <see cref="RouteData"/>.
        /// </summary>
        /// <param name="pageType">The type of the page matching the route, which must implement <see cref="IComponent"/>.</param>
        /// <param name="routeValues">The route parameter values extracted from the matched route.</param>
        public RouteData(Type pageType, IDictionary<string, object> routeValues, RouterData router)
        {
            if (pageType == null)
            {
                throw new ArgumentNullException(nameof(pageType));
            }

            if (!typeof(IComponent).IsAssignableFrom(pageType))
            {
                throw new ArgumentException($"The value must implement {nameof(IComponent)}.", nameof(pageType));
            }

            PageType = pageType;
            RouterRouteData = router;
            RouteValues = routeValues as IReadOnlyDictionary<string, object> ?? throw new ArgumentNullException(nameof(routeValues));
        }

        /// <summary>
        /// Gets the type of the page matching the route.
        /// </summary>
        public Type PageType { get; }

        public RouterData RouterRouteData { get; set; }

        /// <summary>
        /// Gets route parameter values extracted from the matched route.
        /// </summary>
        public IReadOnlyDictionary<string, object> RouteValues { get; }
    }
}
