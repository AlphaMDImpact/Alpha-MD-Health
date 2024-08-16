using System;
using System.Collections.Generic;

namespace AlphaMDHealth.WebClient
{
    internal class RouteEntry
    {
        public RouteEntry(RouteTemplate template, Type handler, string[] unusedRouteParameterNames)
        {
            Template = template;
            UnusedRouteParameterNames = unusedRouteParameterNames;
            Handler = handler;
        }

        public RouteTemplate Template { get; }

        public string[] UnusedRouteParameterNames { get; }

        public Type Handler { get; }

        internal void Match(RouteContext context)
        {
            if (Template.Segments.Length != context.Segments.Length)
            {
                return;
            }

            // Parameters will be lazily initialized.
            Dictionary<string, object> parameters = null;
            for (int i = 0; i < Template.Segments.Length; i++)
            {
                TemplateSegment segment = Template.Segments[i];
                string pathSegment = context.Segments[i];
                if (!segment.Match(pathSegment, out object matchedParameterValue))
                {
                    return;
                }
                else
                {
                    if (segment.IsParameter)
                    {
                        parameters ??= new Dictionary<string, object>(StringComparer.Ordinal);
                        parameters[segment.Value] = matchedParameterValue;
                    }
                }
            }

            // In addition to extracting parameter values from the URL, each route entry
            // also knows which other parameters should be supplied with null values. These
            // are parameters supplied by other route entries matching the same handler.
            if (UnusedRouteParameterNames?.Length > 0)
            {
                parameters ??= new Dictionary<string, object>(StringComparer.Ordinal);
                for (int i = 0; i < UnusedRouteParameterNames.Length; i++)
                {
                    parameters[UnusedRouteParameterNames[i]] = null;
                }
            }

            context.Parameters = parameters;
            context.Handler = Handler;
        }
    }
}
