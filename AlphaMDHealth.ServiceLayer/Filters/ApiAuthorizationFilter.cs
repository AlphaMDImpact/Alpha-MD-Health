using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;

namespace AlphaMDHealth.ServiceLayer
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
            SessionDTO sessionData = new SessionDTO
            {
                Session = new SessionModel
                {
                    IgnoreErrorCode = IgnoreErrorCode
                },
                Request = context.HttpContext.Request,
                OrganisationID = Convert.ToInt64(context.HttpContext.Request.Query.FirstOrDefault(x => x.Key == Constants.SE_ORGANISATION_ID_QUERY_KEY).Value, CultureInfo.CurrentCulture)
            };
            await new AccountServiceBusinessLayer(context.HttpContext).ValidateAccessTokenAsync(sessionData).ConfigureAwait(false);
            if ((HasAllowAnonymous(context) && (sessionData.ErrCode == ErrorCode.Unauthorized || sessionData.ErrCode == ErrorCode.InvalidData)) || sessionData.ErrCode == ErrorCode.OK)
            {
                context.HttpContext.User = new UserPrincipal { AccountID = sessionData.AccountID };
            }
            else
            {
                // Note: languageID is fetched from context.Request Query params and defaults to 1
                string languageID = context.HttpContext.Request.Query.FirstOrDefault(x => x.Key == Constants.SE_LANGUAGE_ID_QUERY_KEY).Value;
                context.Result = new ExtendedHttpActionResult<BaseDTO>(new BaseDTO { ErrCode = sessionData.ErrCode },
                    Convert.ToByte((string.IsNullOrWhiteSpace(languageID) || languageID == "0") ? "1" : languageID, CultureInfo.CurrentCulture));
            }
        }

        private static bool HasAllowAnonymous(AuthorizationFilterContext context)
        {
            var filters = context.Filters;
            for (var i = 0; i < filters.Count; i++)
            {
                if (filters[i] is IAllowAnonymousFilter)
                {
                    return true;
                }
            }

            // When doing endpoint routing, MVC does not add AllowAnonymousFilters for AllowAnonymousAttributes that
            // were discovered on controllers and actions. To maintain compat with 2.x,
            // we'll check for the presence of IAllowAnonymous in endpoint metadata.
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return true;
            }
            return false;
        }
    }
}