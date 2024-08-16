using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class MobileMenuNodeDTO : BaseDTO
    {
        [DataMember]
        public List<MobileMenuNodeModel> MobileMenuNodes { get; set; }
        public MobileMenuNodeModel MobileMenuNode { get; set; }
        [DataMember]
        public List<OptionModel> MenuActions { get; set; }
        [DataMember]
        public List<OptionModel> ExistingMobileMenuNodes { get; set; }
        [DataMember]
        public List<OptionModel> MenuFeatures { get; set; }
    }
}
