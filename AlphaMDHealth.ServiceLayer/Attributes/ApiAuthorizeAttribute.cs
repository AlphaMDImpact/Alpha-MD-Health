using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ApiAuthorizeAttribute : TypeFilterAttribute
    {
        public ApiAuthorizeAttribute() : base(typeof(ApiAuthorizationFilter))
        {
            Arguments = new object[] { default(ErrorCode) };
        }

        /// <summary>
        /// Authorization custom attribute
        /// </summary>
        /// <param name="ignoreErrorCode">If any error code check is to be skipped</param>
        public ApiAuthorizeAttribute(ErrorCode ignoreErrorCode) : base(typeof(ApiAuthorizationFilter))
        {
            Arguments = new object[] { ignoreErrorCode };
        }
    }
}