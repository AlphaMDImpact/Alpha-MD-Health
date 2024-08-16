using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class ChatDTO : BaseDTO
    {
        public ChatModel Chat { get; set; }
        [DataMember]
        public List<ChatModel> Chats { get; set; }
        public ChatDetailModel ChatDetail { get; set; }
        [DataMember]
        public List<ChatDetailModel> ChatDetails { get; set; }
        [DataMember]
        public List<AttachmentModel> ChatAttachments { get; set; }
        [DataMember]
        public List<CardModel> UserChatCards { get; set; }
        [DataMember]
        public List<SaveResultModel> SaveChats { get; set; }
        [DataMember]
        public List<SaveResultModel> SaveChatDetails { get; set; }
        public string BadgeCount { get; set; }
    }
}
