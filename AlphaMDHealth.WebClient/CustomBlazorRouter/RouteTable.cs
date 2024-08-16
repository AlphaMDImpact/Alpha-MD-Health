using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AlphaMDHealth.WebClient
{
    internal class RouteTable
    {
        public RouteTable(RouteEntry[] routes)
        {
            Routes = routes;
        }

        public RouteEntry[] Routes { get; set; }

        /// <summary>
        /// New method to generate RouteTable from RouterDataRoot
        /// </summary>
        /// <param name="routesData"></param>
        /// <returns></returns>
        public static RouteTable CreateNew(RouterDataRoot routesData)
        {
            List<RouteEntry> routes = new List<RouteEntry>();

            foreach (RouterData item in routesData.Routes)
            {
                if (item.Children != null)
                {
                    foreach (RouterData child in item.Children)
                    {
                        AddRoute(routes, child.Page, $"{item.Path}{child.Path}");
                    }
                }
                else
                {
                    AddRoute(routes, item.Page, item.Path);
                }
            }
            return new RouteTable(routes.OrderBy(id => id, RoutePrecedence).ToArray());
        }

        private static void AddRoute(ICollection<RouteEntry> routes, string page, string path)
        {
            if (!string.IsNullOrWhiteSpace(page))
            {
                routes.Add(new RouteEntry(TemplateParser.ParseTemplate(path), Type.GetType(page), null));
            }
        }

        public static RouteTable Create(IEnumerable<Type> types)
        {
            List<RouteEntry> routes = new List<RouteEntry>();
            foreach (Type type in types)
            {
                // We're deliberately using inherit = false here.
                //
                // RouteAttribute is defined as non-inherited, because inheriting a route attribute always causes an
                // ambiguity. You end up with two components (base class and derived class) with the same route.
                IEnumerable<RouteAttribute> routeAttributes = type.GetCustomAttributes<RouteAttribute>(inherit: false);

                foreach (RouteAttribute routeAttribute in routeAttributes)
                {
                    RouteTemplate template = TemplateParser.ParseTemplate(routeAttribute.Template);
                    RouteEntry entry = new RouteEntry(template, type, null);
                    routes.Add(entry);
                }
            }

            return new RouteTable(routes.OrderBy(id => id, RoutePrecedence).ToArray());
        }

        public static IComparer<RouteEntry> RoutePrecedence { get; } = Comparer<RouteEntry>.Create(RouteComparison);

        /// <summary>
        /// Route precedence algorithm.
        /// We collect all the routes and sort them from most specific to
        /// less specific. The specificity of a route is given by the specificity
        /// of its segments and the position of those segments in the route.
        /// * A literal segment is more specific than a parameter segment.
        /// * A parameter segment with more constraints is more specific than one with fewer constraints
        /// * Segment earlier in the route are evaluated before segments later in the route.
        /// For example:
        /// /Literal is more specific than /Parameter
        /// /Route/With/{parameter} is more specific than /{multiple}/With/{parameters}
        /// /Product/{id:int} is more specific than /Product/{id}
        ///
        /// Routes can be ambiguous if:
        /// They are composed of literals and those literals have the same values (case insensitive)
        /// They are composed of a mix of literals and parameters, in the same relative order and the
        /// literals have the same values.
        /// For example:
        /// * /literal and /Literal
        /// /{parameter}/literal and /{something}/literal
        /// /{parameter:constraint}/literal and /{something:constraint}/literal
        ///
        /// To calculate the precedence we sort the list of routes as follows:
        /// * Shorter routes go first.
        /// * A literal wins over a parameter in precedence.
        /// * For literals with different values (case insensitive) we choose the lexical order
        /// * For parameters with different numbers of constraints, the one with more wins
        /// If we get to the end of the comparison routing we've detected an ambiguous pair of routes.
        /// </summary>
        internal static int RouteComparison(RouteEntry x, RouteEntry y)
        {
            RouteTemplate xTemplate = x.Template;
            RouteTemplate yTemplate = y.Template;
            if (xTemplate.Segments.Length != y.Template.Segments.Length)
            {
                return xTemplate.Segments.Length < y.Template.Segments.Length ? -1 : 1;
            }
            else
            {
                for (int i = 0; i < xTemplate.Segments.Length; i++)
                {
                    TemplateSegment xSegment = xTemplate.Segments[i];
                    TemplateSegment ySegment = yTemplate.Segments[i];
                    int comaprision = GetComaprision(xSegment, ySegment);
                    if (comaprision != 0)
                    {
                        return comaprision;
                    }
                }

                throw new InvalidOperationException($@"The following routes are ambiguous:
                            '{x.Template.TemplateText}' in '{x.Handler.FullName}'
                            '{y.Template.TemplateText}' in '{y.Handler.FullName}'
                            ");
            }
        }


        private static int GetComaprision(TemplateSegment xSegment, TemplateSegment ySegment)
        {
            if (!xSegment.IsParameter && ySegment.IsParameter)
            {
                return -1;
            }
            if (xSegment.IsParameter && !ySegment.IsParameter)
            {
                return 1;
            }

            if (xSegment.IsParameter)
            {
                if (xSegment.Constraints.Length > ySegment.Constraints.Length)
                {
                    return -1;
                }
                if (xSegment.Constraints.Length < ySegment.Constraints.Length)
                {
                    return 1;
                }
            }
            else
            {
                int comparison = string.Compare(xSegment.Value, ySegment.Value, StringComparison.OrdinalIgnoreCase);
                if (comparison != 0)
                {
                    return comparison;
                }
            }
            return 0;
        }
        internal void Route(RouteContext routeContext)
        {
            foreach (RouteEntry route in Routes)
            {
                route.Match(routeContext);
                if (routeContext.Handler != null)
                {
                    return;
                }
            }
        }
    }
}
