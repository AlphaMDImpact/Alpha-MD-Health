using SQLite;

namespace AlphaMDHealth.Model
{
    public class UserConsentModel
    {
        [PrimaryKey]
        public long ConsentID { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsSynced { get; set; }
        public DateTimeOffset AcceptedOn { get; set; }
    }
}
