using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AlphaMDHealth.ServiceLayer
{
    /// <summary>
    /// IHttpActionResult class to send custom HTTP responses
    /// </summary>
    /// <typeparam name="T">Return type inherited from BaseDTO</typeparam>
    public class ExtendedHttpActionResult<T> : IActionResult where T : BaseDTO
    {
        /// <summary>
        /// HTTP operation status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// HTTP operation status description
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// LanguageID in which status description will be retrieved
        /// </summary>
        public byte LanguageID { get; set; }

        /// <summary>
        /// Response object to store operation data
        /// </summary>
        public T ResponseObject { get; set; }

        /// <summary>
        /// Creates an HTTP repsonse with the StatusCode and ReasonPhrase set to the responseObject's ErrorCode
        /// </summary>
        /// <param name="responseObject">object that is to be send as json string in response conten</param>
        /// <param name="languageID">User's Language Id</param>
        public ExtendedHttpActionResult(T responseObject, byte languageID)
        {
            var actualStatus = (int)responseObject.ErrCode;
            if (actualStatus >= 600)
            {
                StatusCode = (int)HttpStatusCode.MultiStatus;
            }
            else
            {
                StatusCode = (int)responseObject.ErrCode;
            }
            // Clear logged in user to prevent sending of user id in response
            responseObject.AccountID = 0;
            LanguageID = languageID;
            ResponseObject = responseObject;
        }

        /// <summary>
        /// Creates response object to be sent to client and updates error description based on ErrCode
        /// </summary>
        /// <param name="context">The context in which the result is executed. The context information includes 
        /// information about the action that was executed and request information.</param>
        /// <returns>A task that represents the asynchronous execute operation</returns>
        public async Task ExecuteResultAsync(ActionContext context)
        {
            ResponseObject.ErrorDescription = await new BaseServiceBusinessLayer(context.HttpContext)
                .GetResourceValueByKeyAsync(GroupConstants.RS_COMMON_GROUP, ResponseObject.ErrCode.ToString(), LanguageID < 1 ? (byte)1 : LanguageID).ConfigureAwait(false);
            await new ObjectResult(ResponseObject)
            {
                StatusCode = StatusCode
            }.ExecuteResultAsync(context);
        }
    }
}