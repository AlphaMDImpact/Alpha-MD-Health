using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class ChatModel : BaseListItemModel
    {
        [PrimaryKey]
        public Guid ChatID { get; set; }
        public long FromID { get; set; }
        public long ToID { get; set; }
        public bool IsRelationExists { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImageName { get; set; }
        public string UserProfession { get; set; }
        public string UnreadMessages { get; set; }
        public string LatestMessages { get; set; }
        public DateTimeOffset AddedOn { get; set; }
        public bool IsSynced { get; set; }
        public bool IsActive { get; set; }
        public bool IsDataDownloaded { get; set; }
        public ErrorCode ErrCode { get; set; }
        [Ignore]
        public string AddedOnDate { get; set; }
        public byte[] ImageBytes { get; set; }
        //todo:
        //[Ignore]
        //public ImageSource Image { get; set; }
    }
}

