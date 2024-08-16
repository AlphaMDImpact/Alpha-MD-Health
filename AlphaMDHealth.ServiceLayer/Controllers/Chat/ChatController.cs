using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/ChatService")]
    [ApiAuthorize]
    public class ChatController : BaseController
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public ChatController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Get Chats and chat details
        /// </summary>
        /// <param name="languageID">selected language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="organisationID">Organsiation ID</param>
        /// <param name="toID">Id for which chat details to be fetched</param>
        /// <param name="chatID">Chat ID of the user to be fetched</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>Chats list and chat details data</returns>
        [Route("GetChatsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetChatsAsync(byte languageID, long permissionAtLevelID, long organisationID, long toID, Guid chatID, long recordCount)
        {
            return HttpActionResult(await new ChatServiceBusinessLayer(HttpContext).GetChatsAsync(languageID, permissionAtLevelID, organisationID, toID, chatID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save chat data
        /// </summary>
        /// <param name="languageID">selected language id</param>
        /// <param name="permissionAtLevelID">requested permission at level id</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="selectedUserID">Selected user's ID for whoe data needs to be retrived</param>
        /// <param name="connectionID">SignalR ConnectionId for notification</param>
        /// <param name="chatData">Chats data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveChatAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveChatAsync(byte languageID, long permissionAtLevelID, long organisationID, long selectedUserID, string connectionID, [FromBody] ChatDTO chatData)
        {
            return HttpActionResult(await new ChatServiceBusinessLayer(HttpContext).SaveChatAsync(chatData, permissionAtLevelID, organisationID, selectedUserID, connectionID, _hubContext, languageID).ConfigureAwait(false), languageID);
        }
    }
}