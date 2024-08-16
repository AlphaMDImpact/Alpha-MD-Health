using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.SignalR;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Adds a given connectionID to userID group
        /// </summary>
        /// <param name="connectionID">connection Id</param>
        /// <param name="groupNames">group names which will be used as tags</param>
        /// <returns>Task to add connection to groups</returns>
        public async Task AddToGroupAsync(string connectionID, IHubContext<NotificationHub> hubContext, params string[] groupNames)
        {
            //todo:
            //foreach (string groupName in groupNames)
            //{
            //    await hubContext.Groups.AddToGroupAsync(connectionID, groupName).ConfigureAwait(false);
            //}
        }

        /// <summary>
        /// Removes a given connectionID from userID groups in userIDs
        /// </summary>
        /// <param name="groupNames">groups from which the connectionID is to be removed</param>
        /// <returns>Task to remove connection from groups</returns>
        public async Task RemoveFromGroupsAsync(params string[] groupNames)
        {
            //todo:
            //foreach (string groupName in groupNames)
            //{
            //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName).ConfigureAwait(false);
            //}
        }

        /// <summary>
        /// Sends chat notification to all participants of a given user Id
        /// </summary>
        /// <param name="notificationMessageType">Type of notification</param>
        /// <param name="tags">Tags to which notification is to be sent</param>
        /// <param name="notificationID">Notification Id</param>
        /// <param name="connectionID">Connection Id of the user from whom the message was sent</param>
        /// <param name="senderGroupID">Sender Group Id</param>
        /// <returns>Notifies all userIds</returns>
        public async Task SendNotificationAsync(NotificationMessageType notificationMessageType, List<string> tags, Guid notificationID, string connectionID, string senderGroupID, IHubContext<NotificationHub> hubContext)
        {
            if (GenericMethods.IsListNotEmpty(tags))
            {
                //todo:
                //if (tags.Remove(senderGroupID))
                //{
                //    // Send a silent notification to selected user id to trigger update to user if
                //    // logged in from multiple systems
                //    await hubContext.Clients.GroupExcept(senderGroupID, connectionID)
                //        .SendAsync("ReceiveNotification", notificationMessageType.ToString(), notificationID, senderGroupID, true).ConfigureAwait(false);
                //}
                //await hubContext.Clients.Groups(tags).SendAsync("ReceiveNotification", notificationMessageType.ToString(), notificationID, senderGroupID, false).ConfigureAwait(false);
            }
        }
    }
}