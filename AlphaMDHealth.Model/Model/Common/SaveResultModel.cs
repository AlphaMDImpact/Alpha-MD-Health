using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class SaveResultModel
    {
        public Guid ClientGuid { get; set; }
        public ErrorCode ErrCode { get; set; }
        public Guid ServerGuid { get; set; }
        public long ID { get; set; }
    }
}
