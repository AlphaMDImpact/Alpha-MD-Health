using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AlphaMDHealth.IntegrationServiceLayer
{
    public class ApiAuthorizationFilter : IAsyncAuthorizationFilter
    {
        /// <summary>
        /// Error code that is to be ignored
        /// </summary>
        public ErrorCode IgnoreErrorCode { get; private set; }

        /// <summary>
        /// Authentication filter to validate access token
        /// </summary>
        /// <param name="ignoreErrorCode"></param>
        public ApiAuthorizationFilter(ErrorCode ignoreErrorCode)
        {
            IgnoreErrorCode = ignoreErrorCode;
        }

        /// <summary>
        /// Checks if given authorization is correct based on the request received
        /// </summary>
        /// <param name="context">Request context</param>
        /// <returns>A System.Threading.Tasks.Task that on completion indicates the filter has executed</returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
        }
    }
}