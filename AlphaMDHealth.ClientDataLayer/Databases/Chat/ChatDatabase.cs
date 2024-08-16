using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public class ChatDatabase : BaseDatabase
    {
        /// <summary>
        /// Save Chats data
        /// </summary>
        /// <param name="chatData">Chats data to save</param>
        /// <returns>Operation status</returns>
        public async Task SaveChatAsync(ChatDTO chatData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(chatData.Chats))
                {
                    foreach (ChatModel chat in chatData.Chats)
                    {
                        transaction.InsertOrReplace(chat);
                    }
                }
                if (GenericMethods.IsListNotEmpty(chatData.ChatDetails))
                {
                    foreach (ChatDetailModel chatDetail in chatData.ChatDetails)
                    {
                        chatDetail.IsDataDownloaded = false;
                        transaction.InsertOrReplace(chatDetail);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get chats from database
        /// </summary>
        /// <param name="chatsData">Reference object to return chat records</param>
        /// <returns>Chats in reference object</returns>
        public async Task GetChatsAsync(ChatDTO chatsData)
        {
            var overviewCondition = chatsData.RecordCount > 0 && chatsData.Chat.ToID > 0 && chatsData.Chat.FromID != chatsData.Chat.ToID
                ? $"(CM.FromID = {chatsData.Chat.ToID} OR CM.ToID = {chatsData.Chat.ToID}) "
                : $"(CM.FromID = {chatsData.Chat.FromID} OR CM.ToID = {chatsData.Chat.FromID}) ";

            chatsData.Chats = await SqlConnection.QueryAsync<ChatModel>(
                "SELECT DISTINCT CM.ChatID, CM.FromID, CM.ToID, UM.FirstName, UM.LastName, UM.ImageName, CDM.ChatText AS LatestMessages, CDM.AddedOn, CDM.IsActive, " +
                "(SELECT COUNT(1) FROM ChatDetailModel C WHERE C.ChatID = CM.ChatID AND C.AddedById <> ? AND C.AddedOn > CM.AddedOn) AS UnreadMessages " +
                "FROM ChatModel CM " +
                $"JOIN UserModel UM ON (UM.UserID = CM.FromID OR CM.ToID = UM.UserID) AND UM.UserID <> ? AND {overviewCondition} " +
                "LEFT JOIN ChatDetailModel CDM ON CDM.ChatDetailID = (SELECT ChatDetailID FROM ChatDetailModel WHERE ChatID = CM.ChatID ORDER BY AddedON DESC LIMIT 1) " +
                "ORDER BY CDM.AddedON DESC "
                , chatsData.Chat.FromID, chatsData.Chat.FromID
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Get ChatDetails for file sync from database
        /// </summary>
        /// <param name="chatsData">Reference object to return ContentPage</param>
        /// <returns>Chat Details data</returns>
        public async Task GetChatDetailFileStatusAsync(ChatDTO chatsData)
        {
            chatsData.ChatDetails = await SqlConnection.QueryAsync<ChatDetailModel>(
                "SELECT ChatDetailID, FileName, FileType FROM ChatDetailModel WHERE IsActive = 1 AND IsDataDownloaded = 0 "
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Get chats and chat details from database
        /// </summary>
        /// <param name="chatDTO">Reference object to return chat records</param>
        /// <returns>Chats in reference object and operation status</returns>
        public async Task GetChatDetailsAsync(ChatDTO chatDTO)
        {
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            string condition = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker
                ? $"AND URM.PatientID = {chatDTO.SelectedUserID}"
                : string.Empty;

            chatDTO.Chats = await SqlConnection.QueryAsync<ChatModel>(
                $"SELECT CM.ChatID, IFNULL(CM.FromID, {chatDTO.SelectedUserID}) AS FromID, " +
                    "IFNULL(CM.ToID, UM.UserID) AS ToID, UM.FirstName, UM.LastName, UM.ImageName, " +
                    "CASE WHEN (" +
                        "(SELECT COUNT(1) FROM UserRelationModel URM WHERE URM.IsActive = 1 " +
                        "AND CAST(strftime('%s', strftime('%Y-%m-%d', URM.FromDate/10000000 - 62135596800,'unixepoch')) AS INTEGER) <= CAST(strftime('%s', 'now') AS INTEGER) " +
                        "AND CAST(strftime('%s', strftime('%Y-%m-%d', URM.ToDate/10000000 - 62135596800,'unixepoch')) AS INTEGER) > CAST(strftime('%s', 'now') AS INTEGER) " +
                        $"AND (UM.UserID = URM.CareGiverID OR UM.UserID = URM.PatientID) {condition}) > 0)" +
                    " THEN '1' ELSE '0' END AS IsRelationExists " +
                "FROM UserModel UM " +
                $"LEFT JOIN ChatModel CM ON (UM.UserID = CM.FromID OR UM.UserID = CM.ToID) AND (CM.ToID = {chatDTO.SelectedUserID} OR CM.FromID = {chatDTO.SelectedUserID}) " +
                "WHERE UM.UserID = ?"
                , chatDTO.Chat.ToID
            ).ConfigureAwait(false);

            chatDTO.ChatDetails = await SqlConnection.QueryAsync<ChatDetailModel>(
                "SELECT ChatDetailID, ChatID, ChatText, AttachmentBase64, CompressedAttachment, FileName, FileType, IsActive, AddedOn, AddedBy, AddedById " +
                "FROM ChatDetailModel WHERE ChatID = ? ORDER BY AddedOn"
                , chatDTO.Chats.FirstOrDefault().ChatID
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Update read status for selected chat
        /// </summary>
        /// <param name="chatDTO">Reference object to store chat records</param>
        /// <returns>Operation status</returns>
        public async Task UpdateChatReadStatusAsync(ChatDTO chatDTO)
        {
            await SqlConnection.ExecuteAsync("UPDATE ChatModel SET IsSynced = 0, AddedOn = ? WHERE ChatID = ?", GenericMethods.GetUtcDateTime, chatDTO.Chat.ChatID).ConfigureAwait(false);
        }

        /// <summary>
        /// Get non-synced chats from database
        /// </summary>
        /// <param name="chatData">object to get sample data</param>
        /// <returns>sample data object</returns>
        public async Task GetChatDetailsForSyncAsync(ChatDTO chatData)
        {
            chatData.Chats = await SqlConnection.QueryAsync<ChatModel>
                ("SELECT ChatID, FromID, ToID, AddedOn FROM ChatModel WHERE IsSynced = 0 ORDER BY ChatID").ConfigureAwait(false);
            if (GenericMethods.IsListNotEmpty(chatData.Chats))
            {
                chatData.ChatDetails = await SqlConnection.QueryAsync<ChatDetailModel>
                    ("SELECT ChatDetailID, ChatID, ChatText, AttachmentBase64, FileName, FileType, FromID, IsActive, AddedOn, AddedById " +
                    "FROM ChatDetailModel WHERE IsSynced = 0 ORDER BY ChatID").ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get unread message count for chat menu
        /// </summary>
        /// <param name="chatData"> Object reference to hold badge count</param>
        /// <returns>badge count with operation status</returns>
        public async Task GetUnreadCountAsync(ChatDTO chatData)
        {
            var selectedUserID = Preferences.Get(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)0);
            chatData.Chat = await SqlConnection.FindWithQueryAsync<ChatModel>($"SELECT COUNT(CDM.ChatDetailID) AS UnreadMessages " +
                $"FROM ChatDetailModel CDM " +
                $"LEFT JOIN ChatModel CM ON CDM.ChatID = CM.ChatID " +
                $"WHERE (CM.FromID = ? OR CM.ToID = ?) AND CDM.AddedById <> ? AND CDM.AddedOn > CM.AddedOn", selectedUserID, selectedUserID, chatData.ChatDetail.AddedById).ConfigureAwait(false);
        }


        /// <summary>
        /// Get chat read status
        /// </summary>
        /// <param name="chatData"> Object reference to hold chat data</param>
        /// <returns>operation status</returns>
        public async Task<bool> IsChatUnread(ChatDTO chatData)
        {
            return await SqlConnection.FindWithQueryAsync<ChatModel>($"SELECT 1 FROM ChatDetailModel CDM " +
                $"LEFT JOIN ChatModel CM ON CDM.ChatID = CM.ChatID " +
                $"WHERE CDM.ChatDetailID = ? AND CDM.AddedById <> ? AND CDM.AddedOn > CM.AddedOn", chatData.ChatDetail.ChatDetailID, chatData.ChatDetail.AddedById).ConfigureAwait(false) == null;
        }

        /// <summary>
        /// Update ChatDetail Images name
        /// </summary>
        /// <param name="chatData">data to update sync status<</param>
        public async Task UpdateChatDetailsImageStatusAsync(ChatDTO chatData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (ChatDetailModel chatDetail in chatData.ChatDetails)
                {
                    transaction.Execute("UPDATE ChatDetailModel SET AttachmentBase64 = ?, CompressedAttachment = ?, IsDataDownloaded = 1 WHERE ChatDetailID = ?", chatDetail.AttachmentBase64, chatDetail.CompressedAttachment, chatDetail.ChatDetailID);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update chat detail sync status
        /// </summary>
        /// <param name="chatData">data to update sync status</param>
        public async Task UpdateChatDetailsSyncStatusAsync(ChatDTO chatData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                UpdateChatData(chatData, transaction);
                UpdateChatDetailsData(chatData, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get chat details from chat detail id
        /// </summary>
        /// <param name="chatData"></param>
        /// <returns></returns>
        public async Task GetChatDetailAsync(ChatDTO chatData)
        {
            chatData.ChatDetails = await SqlConnection.QueryAsync<ChatDetailModel>
                ("SELECT * FROM ChatDetailModel WHERE ChatDetailID = ? ", chatData.ChatDetail.ChatDetailID).ConfigureAwait(false);
        }

        private void UpdateChatDetailsData(ChatDTO chatData, SQLiteConnection transaction)
        {
            foreach (ChatDetailModel chatDetail in chatData.ChatDetails)
            {
                SaveResultModel result = chatData.SaveChatDetails?.FirstOrDefault(x => x.ClientGuid == chatDetail.ChatDetailID);
                chatDetail.ErrCode = result == null ? ErrorCode.OK : result.ErrCode;
                switch (chatDetail.ErrCode)
                {
                    case ErrorCode.OK:
                        // Data is successfully synced, so only update sync flag and EmpID received from server
                        transaction.Execute("UPDATE ChatDetailModel SET IsSynced = 1, IsDataDownloaded = 1, ErrCode = ? WHERE ChatDetailID = ?", chatData.ErrCode, chatDetail.ChatDetailID);
                        break;
                    case ErrorCode.DuplicateGuid:
                        // Mark chat as not synced
                        transaction.Execute("UPDATE ChatModel SET IsSynced = 0 WHERE ChatID = ?", chatDetail.ChatID);
                        // Update with new Guid
                        transaction.Execute("UPDATE ChatDetailModel SET ChatDetailID = ? WHERE ChatDetailID = ?", GenerateNewGuid(transaction, false), chatDetail.ChatDetailID);
                        chatData.ErrCode = chatDetail.ErrCode;
                        break;
                    default:
                        // Mark record with the received error code
                        transaction.Execute("UPDATE ChatDetailModel SET ErrCode = ? WHERE ChatDetailID = ?", chatDetail.ErrCode, chatDetail.ChatDetailID);
                        break;
                }
            }
        }

        private void UpdateChatData(ChatDTO chatData, SQLiteConnection transaction)
        {
            foreach (ChatModel chat in chatData.Chats)
            {
                SaveResultModel result = chatData.SaveChats?.FirstOrDefault(x => x.ClientGuid == chat.ChatID);
                chat.ErrCode = result == null ? ErrorCode.OK : result.ErrCode;
                switch (chat.ErrCode)
                {
                    case ErrorCode.OK:
                        // Data is successfully synced, so only update sync flag and EmpID received from server
                        transaction.Execute("UPDATE ChatModel SET IsSynced = 1, ErrCode = ? WHERE ChatID = ?", chatData.ErrCode, chat.ChatID);
                        break;
                    case ErrorCode.DuplicateGuid:
                        // Update with new Guid
                        Guid newGuid = GenerateNewGuid(transaction, true);
                        transaction.Execute("UPDATE ChatModel SET ChatID = ? WHERE ChatID = ?", newGuid, chat.ChatID);
                        transaction.Execute("UPDATE ChatDetailModel SET ChatID = ? WHERE ChatID = ?", newGuid, chat.ChatID);
                        chatData.ErrCode = chat.ErrCode;
                        break;
                    case ErrorCode.GuidChanged:
                        transaction.Execute("UPDATE ChatModel SET ChatID = ? WHERE ChatID = ?", result?.ServerGuid, chat.ChatID);
                        transaction.Execute("UPDATE ChatDetailModel SET ChatID = ? WHERE ChatID = ?", result?.ServerGuid, chat.ChatID);
                        chatData.ErrCode = chat.ErrCode;
                        break;
                    default:
                        // Mark record with the received error code
                        transaction.Execute("UPDATE ChatModel SET ErrCode = ? WHERE ChatID = ?", chat.ErrCode, chat.ChatID);
                        break;
                }
            }
        }

        private Guid GenerateNewGuid(SQLiteConnection transaction, bool forChatID)
        {
            Guid newGuid = GenericMethods.GenerateGuid();
            if (forChatID)
            {
                while (transaction.ExecuteScalar<int>("SELECT 1 FROM ChatModel WHERE ChatID = ?", newGuid) > 0)
                {
                    newGuid = GenericMethods.GenerateGuid();
                }
            }
            else
            {
                while (transaction.ExecuteScalar<int>("SELECT 1 FROM ChatDetailModel WHERE ChatDetailID = ?", newGuid) > 0)
                {
                    newGuid = GenericMethods.GenerateGuid();
                }
            }
            return newGuid;
        }
    }
}
