using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/DataSyncService")]
    public class DataSyncController : BaseController
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public DataSyncController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Get master data for anonymous users
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="selectedUserID">Currently selected user id</param>
        /// <param name="syncData">sync data instance with filter data</param>
        /// <returns>Master data with operation status</returns>
        [Route("GetMobileMasterDataAsync")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetMobileMasterDataAsync(byte languageID, long organisationID, long selectedUserID, [FromBody] DataSyncDTO syncData)
        {
            return HttpActionResult(await new DataSyncServiceBusinessLayer(HttpContext).GetMobileDataAsync(languageID, organisationID, selectedUserID, false, syncData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get user data
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="selectedUserID">Currently selected user id</param>
        /// <param name="syncData">sync data instance with filter data</param>
        /// <returns>User data with operation status</returns>
        [Route("GetMobileUserDataAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> GetMobileUserDataAsync(byte languageID, long organisationID, long selectedUserID, [FromBody] DataSyncDTO syncData)
        {
            return HttpActionResult(await new DataSyncServiceBusinessLayer(HttpContext).GetMobileDataAsync(languageID, organisationID, selectedUserID, true, syncData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save all error log in database
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="organisationID">User's Organisation</param>
        /// <param name="errorLogsData">Ref object which holds data to store on server and returns status</param>
        /// <returns>operation status</returns>
        [Route("SaveErrorLogsAsync")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SaveErrorLogsAsync(byte languageID, long organisationID, [FromBody] ErrorLogDTO errorLogsData)
        {
            return HttpActionResult(await new BaseServiceBusinessLayer(HttpContext).SaveErrorLogsAsync(languageID, organisationID, errorLogsData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Registers SignalR connectionID for client
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="selectedUserID">Currently selected user id</param>
        /// <param name="connectionID">SignalR connection id</param>
        /// <returns>Result of operation</returns>
        [Route("RegisterSignalRAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> RegisterSignalRAsync(byte languageID, long organisationID, long selectedUserID, string connectionID)
        {
            return HttpActionResult(await new DataSyncServiceBusinessLayer(HttpContext).RegisterSignalRAsync(languageID, organisationID, selectedUserID, connectionID, Request.Headers, _hubContext), languageID);
        }
    }
}