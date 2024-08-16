using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient
{
    public partial class NavigationComponent
    {
        [Parameter]
        public bool IsFirstRender { get; set; } = true;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && IsFirstRender)
            {
                await NavigateToAsync(AppState.MasterData.DefaultRoute, AppState.ShouldShowBeforeLoginView(AppState.MasterData.DefaultRoute)).ConfigureAwait(true);
                base.OnAfterRender(firstRender);
            }
        }

        /// <summary>
        /// Navigate to a input route
        /// </summary>
        /// <param name="route">Page Route for Navigation</param>
        /// <param name="queryParams">Page Input Query Parameters</param>
        public new Task NavigateToAsync(string route, params string[] queryParams)
        {
            return NavigateToAsync(route, false, queryParams);
        }
    }
}