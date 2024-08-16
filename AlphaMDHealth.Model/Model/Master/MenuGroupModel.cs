using AlphaMDHealth.Utility;
using SQLite;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class MenuGroupModel
    {
        [PrimaryKey]
        public long MenuGroupID { get; set; }

        [MyCustomAttributes(ResourceConstants.R_IDENTIFIER_KEY)]
        public string GroupIdentifier { get; set; }
        public string PageHeading { get; set; }
        public ContentType PageType { get; set; }
        public string PageTypeName { get; set; }
        public byte Count { get; set; }
        public bool IsActive { get; set; }
    }
}
