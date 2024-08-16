using System;

namespace AlphaMDHealth.WebClient
{
    // This implementation is temporary, in the future we'll want to have
    // a more performant/properly designed routing set of abstractions.
    // To be more precise these are some things we are scoping out:
    // * We are not doing link generation.
    // * We are not supporting all the route constraint formats supported by ASP.NET server-side routing.
    // The class in here just takes care of parsing a route and extracting
    // simple parameters from it.
    // Some differences with ASP.NET Core routes are:
    // * We don't support catch all parameter segments.
    // * We don't support optional parameter segments.
    // * We don't support complex segments.
    // The things that we support are:
    // * Literal path segments. (Like /Path/To/Some/Page)
    // * Parameter path segments (Like /Customer/{Id}/Orders/{OrderId})
    internal static class TemplateParser
    {
        public static readonly char[] InvalidParameterNameCharacters =
            new[] { '*', '?', '{', '}', '=', '.' };

        internal static RouteTemplate ParseTemplate(string template)
        {
            string originalTemplate = template;
            template = template.Trim('/');
            if (template == "")
            {
                return new RouteTemplate("/", Array.Empty<TemplateSegment>());
            }
            string[] segments = template.Split('/');
            TemplateSegment[] templateSegments = new TemplateSegment[segments.Length];
            CreateTemplateSegments(template, originalTemplate, segments, templateSegments);
            ////CheckSegmentInTemplate(template, templateSegments);
            return new RouteTemplate(template, templateSegments);
        }

        private static void CheckSegmentInTemplate(string template, TemplateSegment[] templateSegments)
        {
            for (int i = 0; i < templateSegments.Length; i++)
            {
                TemplateSegment currentSegment = templateSegments[i];
                if (!currentSegment.IsParameter)
                {
                    continue;
                }
                foreach (TemplateSegment nextSegment in templateSegments)
                {
                    if (!nextSegment.IsParameter)
                    {
                        continue;
                    }
                    if (string.Equals(currentSegment.Value, nextSegment.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Invalid {nameof(template)} '{template}'. The parameter '{currentSegment}' appears multiple times.");
                    }
                }
            }
        }

        private static void CreateTemplateSegments(string template, string originalTemplate, string[] segments, TemplateSegment[] templateSegments)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                if (string.IsNullOrEmpty(segment))
                {
                    throw new InvalidOperationException($"Invalid {nameof(template)} '{template}'. Empty {nameof(segments)} are not allowed.");
                }
                if (segment[0] != '{')
                {
                    if (segment[^1] == '}')
                    {
                        throw new InvalidOperationException($"Invalid {nameof(template)} '{template}'. Missing '{{' in parameter segment '{segment}'.");
                    }
                    templateSegments[i] = new TemplateSegment(originalTemplate, segment, isParameter: false);
                }
                else
                {
                    MapClosingTemplateSegment(template, originalTemplate, templateSegments, i, segment);
                }
            }
        }

        private static void MapClosingTemplateSegment(string template, string originalTemplate, TemplateSegment[] templateSegments, int i, string segment)
        {
            const int segmentLength = 3;
            if (segment[^1] != '}')
            {
                throw new InvalidOperationException($"Invalid {nameof(template)} '{template}'. Missing '}}' in parameter {nameof(segment)} '{segment}'.");
            }
            if (segment.Length < segmentLength)
            {
                throw new InvalidOperationException($"Invalid {nameof(template)} '{template}'. Empty parameter name in {nameof(segment)} '{segment}' is not allowed.");
            }
            int invalidCharacter = segment.IndexOfAny(InvalidParameterNameCharacters, 1, segment.Length - 2);
            if (invalidCharacter != -1)
            {
                throw new InvalidOperationException($"Invalid {nameof(template)} '{template}'. The character '{segment[invalidCharacter]}' in parameter {nameof(segment)} '{segment}' is not allowed.");
            }
            templateSegments[i] = new TemplateSegment(originalTemplate, segment[1..^1], isParameter: true);
        }
    }
}
