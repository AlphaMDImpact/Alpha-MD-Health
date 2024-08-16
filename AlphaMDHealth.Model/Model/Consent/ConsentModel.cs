using AlphaMDHealth.Utility;
using SQLite;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class ConsentModel
    {
        [PrimaryKey]
        public long ConsentID { get; set; }
        [MyCustomAttributes(ResourceConstants.R_CONSENT_PAGE_KEY)]

        public long PageID { get; set; }
        public bool IsRequired { get; set; }

        [MyCustomAttributes(ResourceConstants.R_SEQUENCE_NO_KEY)]
        public byte SequenceNo { get; set; }
        public bool IsActive { get; set; }
        public string ConsentName { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public bool IsAccepted { get; set; }
        public string ConsentFor { get; set; }
        public DateTimeOffset AcceptedOn { get; set; }
        public string FormattedText { get; set; }
        public bool IsSynced { get; set; }
        [DataMember]
        [MyCustomAttributes(ResourceConstants.R_CONSENT_ROLE_KEY)]
        public byte RoleID { get; set; }
        [Ignore]
        public string Accepted { get; set; }

        public string OrganisationName { get; set; }
    }
}
