using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/PushNotificationService")]
    [ApiAuthorize]
    public class PushNotificationController : BaseController
    {
        /// <summary>
        /// Register device for push notification
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="notificationModel">Notification data</param>
        /// <returns>returns operation status</returns>
        [Route("RegisterDeviceForNotificationsAsync")]
        [HttpPost]
        public async Task<IActionResult> RegisterDeviceForNotificationsAsync(byte languageID, long organisationID, [FromBody] NotificationDTO notificationModel)
        {
            HttpContext.Request.Headers.TryGetValue(Constants.SE_CLIENT_IDENTIFIER_HEADER_KEY, out StringValues headerValues);
            return HttpActionResult(await new PushNotificationServiceBusinessLayer(HttpContext).RegisterDeviceForNotificationsAsync(languageID, organisationID, headerValues.FirstOrDefault() ?? string.Empty, notificationModel), languageID);
        }
    }
}
