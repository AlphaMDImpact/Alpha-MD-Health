using System.Collections.Generic;

namespace AlphaMDHealth.WebClient
{
    /// <summary>
    /// Class for custom routes  
    /// </summary>
    public class RouterData
    {
        /// <summary>
        /// Route path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Page assembly - Project.BlazorSite.Components.Shared.MasterBlaster, Project.BlazorSite
        /// </summary>
        public string Page { get; set; }


        /// <summary>
        /// Children to the page
        /// </summary>
        public IEnumerable<RouterData> Children { get; private set; }

        public void SetChildren(IEnumerable<RouterData> routeData)
        {
            Children = routeData;
        }
        /// <summary>
        /// Featureid of the corresponding Route
        /// </summary>
        public int FeatureId { get; set; }

        /// <summary>
        /// Language Specific Feature Text
        /// </summary>
        public string FeatureText { get; set; }
    }

    /// <summary>
    /// Start root/page containing a list of pages
    /// </summary>
    public class RouterDataRoot
    {
        public IEnumerable<RouterData> Routes { get; private set; }

        public RouterData SelectedRoute { get; set; }

        public void SetRoutes(IEnumerable<RouterData> routeData)
        {
            Routes = routeData;
        }
    }


}
