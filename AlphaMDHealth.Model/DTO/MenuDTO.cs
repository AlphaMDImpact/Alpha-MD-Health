using System.Runtime.Serialization;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class MenuDTO : BaseDTO
    {
        [DataMember]
        public List<MenuModel> Menus { get; set; }
        [DataMember]
        public MenuModel Menu { get; set; }
        [DataMember]
        public List<MobileMenuNodeModel> MenuNodes { get; set; }
        [DataMember]
        public List<OptionModel> MenuNodesGroups { get; set; }

        [MyCustomAttributes(ResourceConstants.R_MENU_LOCATION_KEY)]
        [DataMember]
        public List<OptionModel> MenuLocations { get; set; }
        public IEnumerable<IGrouping<(byte SequenceNo, long MenuGroupID, string Content), MenuModel>> MoreOptionMenus { get; set; }
    }
}
