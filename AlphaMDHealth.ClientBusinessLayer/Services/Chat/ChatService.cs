using System;
using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer;

public class ChatService : BaseService
{
    public ChatService(IEssentials serviceEssentials) : base(serviceEssentials)
    {
    }

    /// <summary>
    /// Get list of chats initiated with/by currently logged in user
    /// </summary>
    /// <param name="chatData">Reference object to return chats data</param>
    /// <param name="isProviderData">true if provider data is required else false</param>
    /// <returns>Chats in reference object</returns>
    public async Task GetChatsAsync(ChatDTO chatData, bool isProviderData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                chatData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                chatData.SelectedUserID = GetLoginUserID();
                chatData.Chat.FromID = GetLoginUserID();
                var tasks = new List<Task> {
                    GetResourcesAsync(GroupConstants.RS_CHAT_PAGE_GROUP),
                    GetFeaturesAsync(AppPermissions.ChatView.ToString(), AppPermissions.ChatAddEdit.ToString(), AppPermissions.ChatDelete.ToString())
                };
                if (isProviderData)
                {
                    tasks.Add(GetUsersForChatAsync(chatData));
                }
                else
                {
                    tasks.Add(GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP));
                    tasks.Add(GetChatsPerUsersAsync(chatData));
                }
                await Task.WhenAll(tasks).ConfigureAwait(false);
                chatData.ErrCode = ErrorCode.OK;
            }
            else
            {
                await SyncChatsFromServerAsync(chatData, CancellationToken.None).ConfigureAwait(false);
            }
            if (GenericMethods.IsListNotEmpty(chatData.Chats))
            {
                MapViewCellData(chatData);
                if (MobileConstants.IsMobilePlatform)
                {
                    chatData.BadgeCount = await GetUnreadCountAsync().ConfigureAwait(false);
                }
                else
                {
                    CreateUserChatCards(chatData);
                }
            }
        }
        catch (Exception ex)
        {
            chatData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Ger user which relation is not expired
    /// </summary>
    /// <param name="chatData">object to store user data</param>
    /// <returns>related users with operation status</returns>
    private async Task GetUsersForChatAsync(ChatDTO chatData)
    {
        var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        if (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
        {
            CaregiverDTO caregiverData = new CaregiverDTO
            {
                AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0),
                SelectedUserID = chatData.SelectedUserID
            };
            await new UserDatabase().GetCaregiversAsync(caregiverData, true).ConfigureAwait(false);
            if (GenericMethods.IsListNotEmpty(caregiverData.Caregivers))
            {
                chatData.Chats = (from caregiver in caregiverData.Caregivers
                                  where caregiver.FromDate.Value.Ticks <= GenericMethods.GetUtcDateTime.Ticks
                                    && caregiver.ToDate.Value.Ticks >= GenericMethods.GetUtcDateTime.Ticks
                                  select new ChatModel
                                  {
                                      ToID = caregiver.CareGiverID,
                                      FirstName = caregiver.FirstName,
                                      LastName = caregiver.LastName,
                                      ImageName = caregiver.ImageName,
                                      UserProfession = caregiver.Department,
                                  })?.GroupBy(x => x.ToID).Select(x => x.FirstOrDefault())?.ToList();
            }
        }
        else
        {
            UserDTO userData = new UserDTO
            {
                SelectedUserID = chatData.SelectedUserID,
                IsChatsView = true
            };
            await new UserDatabase().GetUsersAsync(userData).ConfigureAwait(false);
            if (GenericMethods.IsListNotEmpty(userData.Users))
            {
                chatData.Chats = (from caregiver in userData.Users
                                  where caregiver.FromDate.Value.Ticks <= GenericMethods.GetUtcDateTime.Ticks
                                    && caregiver.ToDate.Value.Ticks >= GenericMethods.GetUtcDateTime.Ticks
                                  select new ChatModel
                                  {
                                      ToID = caregiver.UserID,
                                      FirstName = caregiver.FirstName,
                                      LastName = caregiver.LastName,
                                      ImageName = caregiver.ImageName,
                                  })?.GroupBy(x => x.ToID).Select(x => x.FirstOrDefault())?.ToList();
            }
        }
    }

    private async Task GetChatsPerUsersAsync(ChatDTO chatData)
    {
        await new ChatDatabase().GetChatsAsync(chatData).ConfigureAwait(false);
        if (chatData.Chats?.Count > 0)
        {
            var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
            if (roleID == (int)RoleName.CareTaker)
            {
                CaregiverDTO caregiverData = new CaregiverDTO
                {
                    AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0),
                    SelectedUserID = chatData.SelectedUserID
                };
                await new UserDatabase().GetCaregiversAsync(caregiverData, true).ConfigureAwait(false);
                chatData.Chats = caregiverData.Caregivers?.Count > 0
                    ? chatData.Chats.Where(x => caregiverData.Caregivers.Any(y => y.CareGiverID == x.FromID || y.CareGiverID == x.ToID)).ToList()
                    : null;
            }
            if (chatData.RecordCount > 0)
            {
                chatData.Chats = chatData.Chats?.Take((int)chatData.RecordCount)?.ToList();
            }
        }
    }

    /// <summary>
    /// Get all messages for a perticlar chat
    /// </summary>
    /// <param name="chatData">Reference object to return chats data</param>
    /// <returns>Chat details in reference object</returns>
    public async Task GetChatDetailsAsync(ChatDTO chatData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await Task.WhenAll(
                    new ChatDatabase().GetChatDetailsAsync(chatData),
                    GetResourcesAsync(GroupConstants.RS_CHAT_PAGE_GROUP),
                    GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                    GetFeaturesAsync(AppPermissions.ChatAddEdit.ToString(), AppPermissions.ChatDelete.ToString())
                ).ConfigureAwait(false);
            }
            else
            {
                await SyncChatsFromServerAsync(chatData, CancellationToken.None).ConfigureAwait(false);
            }
            if (chatData.Chats?.Count() > 0 && chatData.Chat.ToID != 0)
            {
                MapChatData(chatData);
                if (MobileConstants.IsMobilePlatform)
                {
                    await new ChatDatabase().UpdateChatReadStatusAsync(chatData).ConfigureAwait(false);
                    chatData.BadgeCount = await GetUnreadCountAsync().ConfigureAwait(false);
                    chatData.ErrCode = ErrorCode.OK;
                }
            }
        }
        catch (Exception ex)
        {
            chatData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync chat detail to server
    /// </summary>
    /// <param name="requestData">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status call</returns>
    public async Task SyncChatDetailsToServerAsync(ChatDTO requestData, CancellationToken cancellationToken)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await new ChatDatabase().GetChatDetailsForSyncAsync(requestData).ConfigureAwait(false);
            }
            if (GenericMethods.IsListNotEmpty(requestData.Chats))
            {
                var httpData = new HttpServiceModel<ChatDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_CHAT_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { nameof(BaseDTO.SelectedUserID), Convert.ToString(GetLoginUserID(), CultureInfo.InvariantCulture) },
                        { Constants.SE_CONNECTION_ID_QUERY_KEY,
                            Convert.ToString(_essentials.GetPreferenceValue<string>(StorageConstants.PR_SIGNALR_CONNECTION_ID_KEY, string.Empty), CultureInfo.InvariantCulture)
                        }
                    },
                    ContentToSend = requestData,
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                requestData.ErrCode = httpData.ErrCode;
                if (requestData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data?.HasValues == true)
                    {
                        requestData.SaveChats = MapSaveResponse(data, nameof(ChatDTO.SaveChats));
                        requestData.SaveChatDetails = MapSaveResponse(data, nameof(ChatDTO.SaveChatDetails));
                        await SaveChatsSyncResultsAsync(requestData, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Save chat details to database
    /// </summary>
    /// <param name="chatData">Chat details to be saved</param>
    /// <returns>Operation result</returns>
    public async Task SaveChatDetailAsync(ChatDTO chatData)
    {
        try
        {
            chatData.ChatDetail.ChatText = chatData.ChatDetail.ChatText?.Trim();
            if (chatData.Chat.ChatID == Guid.Empty)
            {
                chatData.Chat.ChatID = GenericMethods.GenerateGuid();
                chatData.Chat.AddedOn = GenericMethods.GetUtcDateTime;
                chatData.Chat.IsSynced = false;
                chatData.Chats = new List<ChatModel> { chatData.Chat };
            }
            else
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    await new ChatDatabase().UpdateChatReadStatusAsync(chatData).ConfigureAwait(false);
                }
                else
                {
                    chatData.Chats = new List<ChatModel> { chatData.Chat };
                }
            }
            if (chatData.ChatDetail.ChatDetailID == Guid.Empty)
            {
                chatData.ChatDetail.ChatDetailID = GenericMethods.GenerateGuid();
                chatData.ChatDetail.ChatID = chatData.Chat.ChatID;
                chatData.ChatDetail.AddedById = GetLoginUserID();
            }
            chatData.ChatDetail.AddedOn = GenericMethods.GetUtcDateTime;
            chatData.ChatDetails = new List<ChatDetailModel> { chatData.ChatDetail };
            if (MobileConstants.IsMobilePlatform)
            {
                using (ChatDatabase chatDatabase = new ChatDatabase())
                {
                    await chatDatabase.SaveChatAsync(chatData).ConfigureAwait(false);
                }
            }
            else
            {
                await SyncChatDetailsToServerAsync(chatData, CancellationToken.None).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            chatData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Get unread message count for chat menu
    /// </summary>
    /// <returns>badge count with operation status</returns>
    public async Task<string> GetUnreadCountAsync()
    {
        ChatDTO chatData = new ChatDTO();
        try
        {
            chatData.ChatDetail = new ChatDetailModel
            {
                AddedById = GetLoginUserID()
            };
            await new ChatDatabase().GetUnreadCountAsync(chatData).ConfigureAwait(false);
            return chatData.Chat.UnreadMessages == "0" ? string.Empty : chatData.Chat.UnreadMessages;
        }
        catch (Exception ex)
        {
            chatData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
            return string.Empty;
        }
    }

    /// <summary>
    /// Get chat read status
    /// </summary>
    /// <param name="chatDetailId">Chat detail id to check read status</param>
    /// <returns>operation status</returns>
    public async Task<bool> IsChatUnread(string chatDetailId)
    {
        ChatDTO chatData = new ChatDTO();
        try
        {
            chatData.ChatDetail = new ChatDetailModel
            {
                ChatDetailID = new Guid(chatDetailId),
                AddedById = GetLoginUserID()
            };
            return await new ChatDatabase().IsChatUnread(chatData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            chatData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    internal async Task MapAndSaveChatsAsync(DataSyncModel result, JToken data)
    {
        ChatDTO chatData = new ChatDTO();
        MapChatDetailsData(data, chatData);
        if (GenericMethods.IsListNotEmpty(chatData.Chats) || GenericMethods.IsListNotEmpty(chatData.ChatDetails))
        {
            await SaveChatDetailsAsync(chatData).ConfigureAwait(false);
            result.RecordCount = chatData.Chats?.Count ?? 0 + chatData.ChatDetails?.Count ?? 0;
            if (_essentials.GetPreferenceValue(StorageConstants.PR_AWAIT_BLOB_CALL_KEY, false))
            {
                await ChatDetailImageMappingAsync().ConfigureAwait(false);
                _essentials.SetPreferenceValue(StorageConstants.PR_AWAIT_BLOB_CALL_KEY, false);
            }
            else
            {
                _ = ChatDetailImageMappingAsync().ConfigureAwait(false);
            }
        }
        result.ErrCode = ErrorCode.OK;
    }

    private async Task ChatDetailImageMappingAsync()
    {
        ChatDTO chatData = new ChatDTO { ChatDetails = new List<ChatDetailModel>() };
        try
        {
            using (ChatDatabase chatDatabase = new ChatDatabase())
            {
                await chatDatabase.GetChatDetailFileStatusAsync(chatData).ConfigureAwait(false);
            }
            if (GenericMethods.IsListNotEmpty(chatData.ChatDetails))
            {
                await GetChatDetailsImageAsync(chatData).ConfigureAwait(false);
                await new ChatDatabase().UpdateChatDetailsImageStatusAsync(chatData).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            chatData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }

    }

    private async Task GetChatDetailsImageAsync(ChatDTO chatData)
    {
        //todo:
        //string[] compressSize = (await new SettingsService().GetSettingsValueByKeyAsync(SettingsConstants.S_IMAGE_COMPRESSED_RESOLUTION_KEY).ConfigureAwait(false)).Split('x');
        //foreach (ChatDetailModel chatDetail in chatData?.ChatDetails.Where(x => !string.IsNullOrWhiteSpace(x.FileName)))
        //{
        //    chatDetail.AttachmentBase64 = chatDetail.FileName.Contains(Constants.HTTP_TAG_PREFIX) ? await GetImageAsBase64Async(chatDetail.FileName).ConfigureAwait(false) : string.Empty;
        //    chatDetail.CompressedAttachment = !string.IsNullOrWhiteSpace(chatDetail.AttachmentBase64) ? GetCompressAttachment(chatDetail.AttachmentBase64, chatDetail.FileType, compressSize) : string.Empty;
        //}
    }

    private void MapChatData(ChatDTO chatData)
    {
        chatData.Chat = chatData.Chats[0];
        chatData.Chat.FirstName = $"{chatData.Chat.FirstName} {chatData.Chat.LastName}";
        if (string.IsNullOrWhiteSpace(chatData.Chat.ImageName))
        {
            chatData.Chat.ImageName = GetInitials(chatData.Chat.FirstName);
            //todo:
            //chatData.Chat.Image = null;
        }
        else
        {
            if (MobileConstants.IsMobilePlatform)
            {
                //todo:
                //chatData.Chat.Image = chatData.Chat.ImageName; // ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(chatData.Chat.ImageName));
            }
        }
        if (chatData.ChatDetails != null)
        {
            MapChatDetailsData(chatData);
        }
    }

    /// <summary>
    /// get file icon based on file name 
    /// </summary>
    /// <param name="customAttachment">attacment model</param>
    /// <param name="chat">chat model</param>
    public void GetFileIcon(AttachmentModel customAttachment, ChatDetailModel chat)
    {
        if (customAttachment.FileExtension != AppFileExtensions.none.ToString())
        {
            //todo:
            //customAttachment.DefaultIcon = GetFileIcon(customAttachment.FileType, chat.CompressedAttachment, out ImageSource source);
            //customAttachment.AttachmentSource = source;
        }
    }

    /// <summary>
    /// Get chat detail
    /// </summary>
    /// <param name="chatData">object to get chat detail</param>
    /// <returns>Chat detail</returns>
    public async Task GetChatDetailAsync(ChatDTO chatData)
    {
        try
        {
            using (ChatDatabase chatDatabase = new ChatDatabase())
            {
                await chatDatabase.GetChatDetailAsync(chatData).ConfigureAwait(false);
            }
            chatData.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            chatData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void MapChatDetailsData(ChatDTO chatData)
    {
        chatData.ChatAttachments = new List<AttachmentModel>();
        LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        chatData.ChatDetails.ForEach((Action<ChatDetailModel>)(item =>
        {
            DateTimeOffset todayDate = _essentials.ConvertToLocalTime(GenericMethods.GetUtcDateTime);
            DateTimeOffset addedDate = _essentials.ConvertToLocalTime(item.AddedOn);
            var addedDateString = GenericMethods.GetDateTimeBasedOnCulture(addedDate, DateTimeType.DateTime, dayFormat, monthFormat, yearFormat);
            var addedTimeString = GenericMethods.GetDateTimeBasedOnCulture(addedDate, DateTimeType.Time, string.Empty, string.Empty, string.Empty);
            AttachmentModel attachementData = new AttachmentModel
            {
                FileID = item.ChatDetailID,
                FileValue = item.FileName,
                IsActive = item.IsActive,
                AddedOnDate = todayDate.Date == addedDate.Date ? $"{LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_TODAY_TEXT_KEY)} {addedTimeString}" : addedDateString,
                IsSent = item.AddedById == chatData.SelectedUserID,
                AddedBy = item.IsSent ? string.Empty : chatData.Chat.FirstName,
                IsDeleteAllowed = item.IsActive && chatData.Chat.IsRelationExists,
                IsRelationNotExpired = chatData.Chat.IsRelationExists
            };
            if (!item.IsActive)
            {
                attachementData.Text = MobileConstants.IsMobilePlatform ? LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DELETED_ITEM_KEY) : item.ChatText;
                attachementData.FileDescription = MobileConstants.IsMobilePlatform ? LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DELETED_ITEM_KEY) : item.ChatText;
                attachementData.FileExtension = AppFileExtensions.none.ToString();
            }
            else
            {
                attachementData.Text = item.ChatText;
                attachementData.FileDescription = item.ChatText;
                attachementData.FileExtension = item.FileType.ToString();
                if (MobileConstants.IsMobilePlatform)
                {
                    GetFileIcon(attachementData, item);
                }
            }
            attachementData.TextColor = attachementData.IsSent ? StyleConstants.GENERIC_BACKGROUND_COLOR : StyleConstants.PRIMARY_TEXT_COLOR;
            attachementData.DateColor = attachementData.IsSent ? StyleConstants.GENERIC_BACKGROUND_COLOR : StyleConstants.SECONDARY_TEXT_COLOR;
            chatData.ChatAttachments.Add(attachementData);
        }));
    }

    private void MapChatDetailsData(JToken data, ChatDTO chatData)
    {
        SetResourcesAndSettings(chatData);
        chatData.Chats = data[nameof(ChatDTO.Chats)].Any()
            ? MapChats(data)
            : new List<ChatModel>();
        chatData.ChatDetails = data[nameof(ChatDTO.ChatDetails)].Any()
            ? (from dataItem in data[nameof(ChatDTO.ChatDetails)]
               select MapChatDetails(dataItem)).ToList()
            : new List<ChatDetailModel>();
    }

    private List<ChatModel> MapChats(JToken data)
    {
        return (from dataItem in data[nameof(ChatDTO.Chats)]
                select new ChatModel
                {
                    ChatID = (Guid)dataItem[nameof(ChatModel.ChatID)],
                    FromID = (long)dataItem[nameof(ChatModel.FromID)],
                    ToID = (long)dataItem[nameof(ChatModel.ToID)],
                    IsRelationExists = (bool)dataItem[nameof(ChatModel.IsRelationExists)],
                    FirstName = (string)dataItem[nameof(ChatModel.FirstName)],
                    LastName = (string)dataItem[nameof(ChatModel.LastName)],
                    ImageName = (string)dataItem[nameof(ChatModel.ImageName)],
                    UserProfession = (string)dataItem[nameof(ChatModel.UserProfession)],
                    UnreadMessages = (string)dataItem[nameof(ChatModel.UnreadMessages)],
                    LatestMessages = (string)dataItem[nameof(ChatModel.LatestMessages)],
                    AddedOn = (DateTimeOffset)dataItem[nameof(ChatModel.AddedOn)],
                    IsSynced = true
                }).ToList();
    }

    private ChatDetailModel MapChatDetails(JToken dataItem)
    {
        return new ChatDetailModel
        {
            ChatDetailID = (Guid)dataItem[nameof(ChatDetailModel.ChatDetailID)],
            ChatID = (Guid)dataItem[nameof(ChatDetailModel.ChatID)],
            ChatText = (string)dataItem[nameof(ChatDetailModel.ChatText)],
            AttachmentBase64 = (string)dataItem[nameof(ChatDetailModel.AttachmentBase64)],
            CompressedAttachment = string.Empty,
            FileName = (string)dataItem[nameof(ChatDetailModel.FileName)],
            FileType = dataItem[nameof(ChatDetailModel.FileType)].ToString().ToEnum<AppFileExtensions>(),
            AddedOn = (DateTimeOffset)dataItem[nameof(ChatDetailModel.AddedOn)],
            AddedById = (long)dataItem[nameof(ChatDetailModel.AddedById)],
            AddedBy = (string)dataItem[nameof(ChatDetailModel.AddedBy)],
            IsRead = (bool)dataItem[nameof(ChatDetailModel.IsRead)],
            IsActive = (bool)dataItem[nameof(ChatDetailModel.IsActive)],
            IsSynced = true
        };
    }

    private string GetCompressAttachment(string attachmentData, AppFileExtensions fileExtension, string[] compressSize)
    {
        string compressAttachment = string.Empty;
        if (MobileConstants.IsMobilePlatform && (fileExtension == AppFileExtensions.jpeg || fileExtension == AppFileExtensions.jpg || fileExtension == AppFileExtensions.png))
        {
            if (attachmentData.Contains(Constants.SYMBOL_COMMA_SEPERATOR_STRING))
            {
                attachmentData = attachmentData.Split(Constants.SYMBOL_COMMA)[1];
            }
            else if (attachmentData.Contains(Constants.SYMBOL_SEMI_COLAN_STRING))
            {
                attachmentData = attachmentData.Split(Constants.SYMBOL_SEMI_COLAN)[1];
            }
            else
            {
                if (attachmentData.Contains(Constants.SYMBOL_COLAN_STRING))
                {
                    attachmentData = attachmentData.Split(Constants.SYMBOL_COLAN)[1];
                }
            }
            if (!string.IsNullOrWhiteSpace(attachmentData))
            {
                //todo:
                //compressAttachment = Convert.ToBase64String(DependencyService.Get<ICompression>().ResetImage(Convert.FromBase64String(attachmentData), Convert.ToSingle(compressSize[0], CultureInfo.InvariantCulture), Convert.ToSingle(compressSize[1], CultureInfo.InvariantCulture)));
            }
        }
        return compressAttachment;
    }

    private void CreateUserChatCards(ChatDTO chatData)
    {
        SetPageSettings(chatData.Settings);
        chatData.UserChatCards = (from userChat in chatData.Chats
                                  select new CardModel
                                  {
                                      CardId = chatData.RecordCount == -1 ? Convert.ToString(userChat.ToID, CultureInfo.InvariantCulture) : Convert.ToString(userChat.ChatID, CultureInfo.InvariantCulture),
                                      ImageBase64 = GetProfileIcon(userChat),
                                      ImageInitial = userChat.ImageName.Length > 3 ? string.Empty : userChat.ImageName,
                                      Header = userChat.FirstName,
                                      SubHeader = userChat.LatestMessages?.Length > Convert.ToInt32(LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_LIST_SUB_HEADER_LENGTH_KEY), CultureInfo.InvariantCulture) ? userChat.LatestMessages?.Substring(0, Convert.ToInt32(LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_LIST_SUB_HEADER_LENGTH_KEY), CultureInfo.InvariantCulture)) : userChat.LatestMessages,
                                      BadgeCss = GetBadgeCss(userChat),
                                      BadgeText = GetBadgeText(chatData, userChat),
                                      SelectedBackground = (Convert.ToString(userChat.ToID, CultureInfo.InvariantCulture) == Convert.ToString(chatData.Chat.ToID, CultureInfo.InvariantCulture) ||
                                      Convert.ToString(userChat.FromID, CultureInfo.InvariantCulture) == Convert.ToString(chatData.Chat.ToID, CultureInfo.InvariantCulture))
                                        ? StyleConstants.PRIMARY_APP_COLOR : StyleConstants.GENERIC_BACKGROUND_COLOR
                                  }).ToList();
        if (chatData.RecordCount == -2)
        {
            chatData.UserChatCards[0].BadgeText = Convert.ToInt32(chatData.Chats[0].UnreadMessages, CultureInfo.InvariantCulture) > 0 ? chatData.Chats[0].UnreadMessages : string.Empty;
            chatData.UserChatCards[0].SelectedBackground = StyleConstants.GENERIC_BACKGROUND_COLOR;
        }
    }

    private string GetBadgeCss(ChatModel userChat)
    {
        return (userChat.UnreadMessages == string.Empty) ? string.Empty : Constants.BADGE_NUMBER_CSS;
    }

    private string GetBadgeText(ChatDTO chatData, ChatModel userChat)
    {
        return Convert.ToString(userChat.ChatID, CultureInfo.InvariantCulture) == Convert.ToString(chatData.Chat.ChatID, CultureInfo.InvariantCulture)
                                                    ? string.Empty : userChat.UnreadMessages;
    }

    private string GetProfileIcon(ChatModel userChat)
    {
        return userChat.ImageName.Length > 3 ? userChat.ImageName : string.Empty;
    }

    private async Task SaveChatsSyncResultsAsync(ChatDTO requestData, CancellationToken cancellationToken)
    {
        if (MobileConstants.IsMobilePlatform)
        {
            await new ChatDatabase().UpdateChatDetailsSyncStatusAsync(requestData).ConfigureAwait(false);
        }
        else
        {
            // Map error result to main object as web will call save for single record
            requestData.ErrCode = FetchErrorCode(requestData.SaveChats, requestData.ErrCode);
            if (requestData.ErrCode == ErrorCode.DuplicateGuid)
            {
                requestData.Chats[0].ChatID = GenericMethods.GenerateGuid();
                requestData.ChatDetails[0].ChatID = requestData.Chats[0].ChatID;
            }
            else if (requestData.ErrCode == ErrorCode.GuidChanged)
            {
                requestData.Chats[0].ChatID = requestData.SaveChats[0].ServerGuid;
                requestData.ChatDetails[0].ChatID = requestData.Chats[0].ChatID;
            }
            else
            {
                requestData.ErrCode = FetchErrorCode(requestData.SaveChatDetails, requestData.ErrCode);
                if (requestData.ErrCode == ErrorCode.DuplicateGuid)
                {
                    requestData.ChatDetails[0].ChatDetailID = GenericMethods.GenerateGuid();
                }
            }
        }
        if (requestData.ErrCode == ErrorCode.GuidChanged || requestData.ErrCode == ErrorCode.DuplicateGuid)
        {
            requestData.ErrCode = ErrorCode.OK;
            await SyncChatDetailsToServerAsync(requestData, cancellationToken).ConfigureAwait(false);
        }
        requestData.RecordCount = requestData.ChatDetails?.Count ?? 0;
    }

    private ErrorCode FetchErrorCode(List<SaveResultModel> resultList, ErrorCode errorCode)
    {
        return GenericMethods.IsListNotEmpty(resultList) ? resultList[0].ErrCode : errorCode;
    }

    private async Task SaveChatDetailsAsync(ChatDTO chatData)
    {
        using (ChatDatabase chatDatabase = new ChatDatabase())
        {
            await chatDatabase.SaveChatAsync(chatData).ConfigureAwait(false);
        }
        chatData.RecordCount = chatData.Chats.Count;
        chatData.ErrCode = ErrorCode.OK;
    }

    private void MapViewCellData(ChatDTO chatData)
    {
        LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        chatData.Chats.ForEach(chat =>
        {
            DateTimeOffset todayDate = _essentials.ConvertToLocalTime(GenericMethods.GetUtcDateTime);
            DateTimeOffset addedDate = _essentials.ConvertToLocalTime(chat.AddedOn);
            var addedDateString = GenericMethods.GetDateTimeBasedOnCulture(addedDate, DateTimeType.DateTime, dayFormat, monthFormat, yearFormat);
            chat.FirstName = $"{chat.FirstName} {chat.LastName}";
            chat.AddedOnDate = todayDate.Date == addedDate.Date
                ? $"{LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_TODAY_TEXT_KEY)} {GenericMethods.GetDateTimeBasedOnCulture(addedDate, DateTimeType.Time, string.Empty, string.Empty, string.Empty)}"
                : addedDateString;
            chat.UnreadMessages = (Convert.ToInt32(GetUnreadCount(chat), CultureInfo.InvariantCulture) > 0 && !(chatData.Chat.ToID == chat.FromID || chatData.Chat.ToID == chat.ToID)) ? chat.UnreadMessages : GetUnreadCount(chatData, chat);
            if (MobileConstants.IsMobilePlatform)
            {
                if (string.IsNullOrWhiteSpace(chat.LatestMessages))
                {
                    chat.LatestMessages = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_CHAT_ATTACHMENT_TEXT_KEY);
                }
                if (!chat.IsActive)
                {
                    chat.LatestMessages = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DELETED_ITEM_KEY);
                }
            }
            if (string.IsNullOrWhiteSpace(chat.ImageName))
            {
                chat.ImageName = chat.LeftDefaultIcon = GetInitials(chat.FirstName);
            }
            else
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    //todo:
                    // chat.Image = chat.LeftSourceIcon = chat.ImageName; //ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(chat.ImageName));
                }
            }
        });
    }

    private string GetUnreadCount(ChatModel chat)
    {
        return string.IsNullOrWhiteSpace(chat.UnreadMessages) ? "0" : chat.UnreadMessages;
    }

    private string GetUnreadCount(ChatDTO chatData, ChatModel chat)
    {
        return chatData.RecordCount == -2 ? chat.UnreadMessages : string.Empty;
    }

    /// <summary>
    /// Sync Chat details from Service
    /// </summary>
    /// <param name="chatData">Reference object to return chats data</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>List of Chats received from server in chatDTO</returns>
    private async Task SyncChatsFromServerAsync(ChatDTO chatData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_CHATS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    {
                        Constants.SE_CONNECTION_ID_QUERY_KEY,
                        Convert.ToString(_essentials.GetPreferenceValue(StorageConstants.PR_SIGNALR_CONNECTION_ID_KEY, string.Empty), CultureInfo.InvariantCulture)
                    },
                    { nameof(BaseDTO.RecordCount), Convert.ToString(chatData.RecordCount, CultureInfo.InvariantCulture) },
                    { nameof(ChatModel.ToID), Convert.ToString(chatData.Chat.ToID, CultureInfo.InvariantCulture) },
                    { nameof(ChatModel.ChatID), Convert.ToString(chatData.Chat.ChatID, CultureInfo.InvariantCulture) }
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            chatData.ErrCode = httpData.ErrCode;
            if (chatData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(chatData, data);
                    MapChatDetailsData(data, chatData);
                }
            }
        }
        catch (Exception ex)
        {
            chatData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }
}