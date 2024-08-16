using AlphaMDHealth.Model;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// Creates an IHttpActionResult using BaseDTO object's ErrorCode
        /// </summary>
        /// <typeparam name="T">any object with BaseDTO as parent</typeparam>
        /// <param name="responseObject">object that is to be send as json string in response content</param>
        /// <param name="languageId">Id of the language selected by the user</param>
        /// <returns>IHttpActionResult with a custom status code and description</returns>
        protected internal IActionResult HttpActionResult<T>(T responseObject, byte languageId) where T : BaseDTO
        {
            return new ExtendedHttpActionResult<T>(responseObject, languageId);
        }
    }
}
