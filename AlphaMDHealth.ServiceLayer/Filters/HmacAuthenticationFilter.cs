using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;

namespace AlphaMDHealth.ServiceLayer
{
    public class HmacAuthenticationFilter : IAsyncAuthorizationFilter
    {
        /// <summary>
        /// Authenticates the request
        /// </summary>
        /// <param name="context">Authentication context</param>
        /// <returns>Task that will perform authentication</returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            SessionDTO sessionData = new SessionDTO { Request = context.HttpContext.Request, Response= context.HttpContext.Request.Path };
            await new AccountServiceBusinessLayer(context.HttpContext).ValidateRequestAsync(sessionData).ConfigureAwait(true);
            if (sessionData.ErrCode != ErrorCode.OK)
            {
                string languageID = context.HttpContext.Request.Query.FirstOrDefault(x => x.Key == Constants.SE_LANGUAGE_ID_QUERY_KEY).Value;

                // Note: languageID is fetched from context.Request Query params and defaults to 1
                context.Result = new ExtendedHttpActionResult<BaseDTO>(
                    sessionData, Convert.ToByte((string.IsNullOrWhiteSpace(languageID) || languageID == "0") ? "1" : languageID, CultureInfo.InvariantCulture));
            }
        }
    }
}