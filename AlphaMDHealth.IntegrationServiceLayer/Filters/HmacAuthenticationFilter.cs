using AlphaMDHealth.IntegrationServiceBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AlphaMDHealth.IntegrationServiceLayer
{
    public class HmacAuthenticationFilter : IAsyncAuthorizationFilter
    {
        /// <summary>
        /// Authenticates client request
        /// </summary>
        /// <param name="context">Authentication context</param>
        /// <returns>Task that will perform authentication</returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            LibraryServiceDTO sessionData = new LibraryServiceDTO { Request = context.HttpContext.Request };
            await new ValidationService().ValidateRequestAsync(sessionData).ConfigureAwait(true);
            if (sessionData.ErrCode == ErrorCode.OK)
            {
                context.HttpContext.User = new ServicePrincipal
                {
                    LibraryDetails = sessionData.LibraryDetails
                };
            }
            else
            {
                context.Result = new ObjectResult(sessionData) { StatusCode = (int)sessionData.ErrCode };
            }
        }
    }
}