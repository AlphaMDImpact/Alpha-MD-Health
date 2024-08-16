using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class ChatServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Chat service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public ChatServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
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
        /// <returns>Chat list and chat details data</returns>
        public async Task<BaseDTO> GetChatsAsync(byte languageID, long permissionAtLevelID, long organisationID, long toID, Guid chatID, long recordCount)
        {
            ChatDTO chatData = new ChatDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || organisationID < 1)
                {
                    chatData.ErrCode = ErrorCode.InvalidData;
                    return chatData;
                }
                if (AccountID < 1)
                {
                    chatData.ErrCode = ErrorCode.Unauthorized;
                    return chatData;
                }
                chatData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetConfigurationDataAsync(chatData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_CHAT_PAGE_GROUP}", organisationID).ConfigureAwait(false))
                {
                    chatData.AccountID = AccountID;
                    chatData.PermissionAtLevelID = permissionAtLevelID;
                    chatData.RecordCount = recordCount;
                    chatData.LanguageID = languageID;
                    chatData.Chat = new ChatModel { ToID = toID, ChatID = chatID };
                    chatData.FeatureFor = FeatureFor;
                    await new ChatServiceDataLayer().GetChatsAsync(chatData).ConfigureAwait(false);
                    if (chatData.ErrCode == ErrorCode.OK)
                    {
                        await ReplaceChatUserImageCdnLinkAsync(chatData);
                        await ReplaceChatMessageImageCdnLinkAsync(chatData.ChatDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                chatData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
            return chatData;
        }

        private async Task ReplaceChatUserImageCdnLinkAsync(ChatDTO chatData)
        {
            if (GenericMethods.IsListNotEmpty(chatData.Chats))
            {
                BaseDTO cdnCacheData = new BaseDTO();
                foreach (var chat in chatData.Chats)
                {
                    if (!string.IsNullOrWhiteSpace(chat.ImageName))
                    {
                        chat.ImageName = await ReplaceCDNLinkAsync(chat.ImageName, cdnCacheData);
                    }
                    if (!string.IsNullOrWhiteSpace(chat.LatestMessages))
                    {
                        chat.LatestMessages = await ReplaceCDNLinkAsync(chat.LatestMessages, cdnCacheData);
                    }
                }
            }
        }

        internal async Task ReplaceChatMessageImageCdnLinkAsync(List<ChatDetailModel> chatDetails)
        {
            if (GenericMethods.IsListNotEmpty(chatDetails))
            {
                BaseDTO cdnCacheData = new BaseDTO();
                foreach (var message in chatDetails)
                {
                    if (!string.IsNullOrWhiteSpace(message.FileName))
                    {
                        message.FileName = await ReplaceCDNLinkAsync(message.FileName, cdnCacheData);
                    }
                }
            }
        }

        /// <summary>
        /// Save Chat to database and send SignalR notification
        /// </summary>
        /// <param name="chatData">Chats data to be saved</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="selectedUserID">Selected user's ID for whoe data needs to be retrived</param>
        /// <param name="connectionID">ConnectionId for notification</param>
        /// <param name="hubContext">Notification hub context</param>
        /// <returns>Result of operation</returns>
        public async Task<BaseDTO> SaveChatAsync(ChatDTO chatData, long permissionAtLevelID, long organisationID, long selectedUserID, string connectionID, IHubContext<NotificationHub> hubContext, byte languageID)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1)
                {
                    chatData.ErrCode = ErrorCode.InvalidData;
                    return chatData;
                }
                if (IsChatDetailsInvalid(chatData.ChatDetails) && IsChatsInvalid(chatData.Chats))
                {
                    chatData.ErrCode = ErrorCode.InvalidData;
                    return chatData;
                }
                chatData.AccountID = AccountID;
                chatData.PermissionAtLevelID = permissionAtLevelID;
                chatData.OrganisationID = organisationID;
                chatData.LanguageID = languageID;
                chatData.SelectedUserID = selectedUserID;
                chatData.FeatureFor = FeatureFor;
                List<ChatModel> chatsToNotify = GetChatsToNotify(chatData);
                await new ChatServiceDataLayer().SaveChatAsync(chatData).ConfigureAwait(false);
                if (chatData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(chatsToNotify))
                {
                    await UploadChatFilesAsync(chatData).ConfigureAwait(false);
                    await new ChatServiceDataLayer().SaveChatAsync(chatData).ConfigureAwait(false);
                    _ = SendNotificationAsync(chatData, connectionID, hubContext, chatsToNotify).ConfigureAwait(false);
                    await ReplaceChatMessageImageCdnLinkAsync(chatData.ChatDetails);
                }
            }
            catch (Exception ex)
            {
                chatData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
            return chatData;
        }

        private async Task UploadChatFilesAsync(ChatDTO chatData)
        {
            var details = chatData.ChatDetails.Where(x => x.IsActive).ToList();
            if (details?.Count > 0)
            {
                FileUploadDTO files = CreateFileDataObject(FileTypeToUpload.ChatImages, null);
                files.FileContainers.AddRange(from conversationGroup in details.GroupBy(x => x.ChatID)
                                              let attachments = (from chat in conversationGroup.ToList()
                                                                 where chat.IsActive && !string.IsNullOrWhiteSpace(chat.AttachmentBase64)
                                                                     && !(chatData.SaveChatDetails?.Any(x => x.ClientGuid == chat.ChatDetailID) ?? false)
                                                                     && !(chatData.SaveChats?.Any(x => x.ClientGuid == chat.ChatID) ?? false)
                                                                 select CreateFileObject(chat.FileName, chat.AttachmentBase64, false)).ToList()
                                              where attachments?.Count > 0
                                              select new FileContainerModel
                                              {
                                                  ID = Convert.ToString(conversationGroup.Key, CultureInfo.InvariantCulture),
                                                  FileData = attachments
                                              });
                if (files.FileContainers?.Count > 0)
                {
                    files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
                    chatData.ErrCode = files.ErrCode;
                    if (files.ErrCode == ErrorCode.OK)
                    {
                        chatData.ChatDetails.ForEach(detail =>
                        {
                            if (detail.IsActive)
                            {
                                detail.IsDataDownloaded = true;
                                var file = files.FileContainers.FirstOrDefault(x => x.ID == Convert.ToString(detail.ChatID, CultureInfo.InvariantCulture))?.FileData?.FirstOrDefault(x => x.RecordID == detail.FileName);
                                if (file != null)
                                {
                                    detail.FileName = file.Base64File;
                                }
                            }
                        });
                    }
                }
                else
                {
                    chatData.ChatDetails.ForEach(detail =>
                    {
                        if (detail.IsActive)
                        {
                            detail.IsDataDownloaded = true;
                        }
                    });
                }
            }
        }

        private bool IsChatsInvalid(List<ChatModel> chats)
        {
            return chats == null || chats.Count < 1 || chats.Any(x => x.FromID < 1 || x.ToID < 1 || x.ChatID == Guid.Empty);
        }

        private bool IsChatDetailsInvalid(List<ChatDetailModel> chatDetails)
        {
            return chatDetails == null || chatDetails.Count < 1 ||
                chatDetails.Any(x => x.ChatID == Guid.Empty || x.ChatDetailID == Guid.Empty || x.FromID < 1
                    || (string.IsNullOrWhiteSpace(x.ChatText) && string.IsNullOrWhiteSpace(x.FileName))
                    || (!string.IsNullOrWhiteSpace(x.FileName) && x.FileType == AppFileExtensions.none));
        }

        private List<ChatModel> GetChatsToNotify(ChatDTO chatData)
        {
            return (from chat in chatData.Chats
                    let chatDetail = chatData.ChatDetails?.OrderByDescending(c => c.AddedOn).FirstOrDefault(x => x.ChatID == chat.ChatID)
                    where chatDetail != null
                    select new ChatModel { ChatID = chat.ChatID, FromID = chatDetail.FromID, ToID = chatDetail.FromID == chat.FromID ? chat.ToID : chat.FromID, FirstName = chatDetail.ChatDetailID.ToString() }
                    ).ToList();
        }

        private async Task SendNotificationAsync(ChatDTO chatData, string connectionID, IHubContext<NotificationHub> hubContext, List<ChatModel> chatsToNotify)
        {
            // Notify users of given chatID
            await Task.WhenAll((from chat in chatsToNotify
                                where !chatData.SaveChats.Any(x => x.ClientGuid == chat.ChatID)
                                select SendUserNotificationAsync(connectionID, hubContext, chat, chatData)
             ).ToList()).ConfigureAwait(false);
        }

        private async Task SendUserNotificationAsync(string connectionID, IHubContext<NotificationHub> hubContext, ChatModel chat, ChatDTO chatData)
        {
            await new NotificationHub().SendNotificationAsync(NotificationMessageType.NotificationChat,
                                                new List<string> { Constants.USER_TAG_PREFIX + chat.FromID, Constants.USER_TAG_PREFIX + chat.ToID },
                                                chat.ChatID, connectionID, Constants.USER_TAG_PREFIX + chat.FromID, hubContext);
            TemplateDTO communicationDto = new TemplateDTO();
            communicationDto.AccountID = AccountID;
            communicationDto.OrganisationID = chatData.OrganisationID;
            communicationDto.Username = chat.FirstName;
            communicationDto.SelectedUserID = chat.ToID;
            communicationDto.LanguageID = chatData.LanguageID;
            communicationDto.NotificationTags = $"{Constants.USER_TAG_PREFIX}{chat.ToID}";
            communicationDto.TemplateData = new TemplateModel
            {
                TemplateName = TemplateName.ENewChat,
                IsExternal = false,
            };
            await SendCommunicationAsync(communicationDto).ConfigureAwait(false);
        }

        private async Task<bool> GetConfigurationDataAsync(BaseDTO baseDTO, byte languageID, string groupNames, long organisationID)
        {
            baseDTO.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
            var organisationSettings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, languageID, default, 0, organisationID, false).ConfigureAwait(false)).Settings;
            baseDTO.Settings.AddRange(organisationSettings);
            if (baseDTO.Settings != null)
            {
                baseDTO.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, groupNames, languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (baseDTO.Resources != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
