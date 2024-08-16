using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class MenuGroupLinkModel
    {
        public long GroupID { get; set; }

        [MyCustomAttributes(ResourceConstants.R_SELECT_LINKS_KEY)]

        public MenuType PageTypeID { get; set; }
        public long TargetID { get; set; }
        public string Heading { get; set; }
        public byte SequenceNo { get; set; }
    }
}
