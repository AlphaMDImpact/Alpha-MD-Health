using AlphaMDHealth.Utility;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class MenuGroupDTO : BaseDTO
    {
        public MenuGroupModel MenuGroup { get; set; }
        [DataMember]
        public List<MenuGroupModel> MenuGroups { get; set; }
        [DataMember]
        public List<ContentDetailModel> MenuGroupDetails { get; set; }
        [DataMember]
        public List<MenuGroupLinkModel> MenuGroupLinks { get; set; }

        [MyCustomAttributes(ResourceConstants.R_MENU_NODE_KEY)]
        [DataMember]
        public List<OptionModel> MenuNodes { get; set; }
        [DataMember]
        public List<OptionModel> PageTypes { get; set; }
        [DataMember]
        public List<LanguageModel> Languages { get; set; }
    }
}
