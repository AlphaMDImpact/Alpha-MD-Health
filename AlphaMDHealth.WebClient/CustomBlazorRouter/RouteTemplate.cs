namespace AlphaMDHealth.WebClient
{
    internal class RouteTemplate
    {
        public static readonly char[] Separators = new[] { '/' };

        public RouteTemplate(string templateText, TemplateSegment[] segments)
        {
            TemplateText = templateText;
            Segments = segments;
        }

        public string TemplateText { get; }

        public TemplateSegment[] Segments { get; }
    }
}
