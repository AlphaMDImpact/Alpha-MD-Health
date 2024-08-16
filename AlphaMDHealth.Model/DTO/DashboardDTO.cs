using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class DashboardDTO : DashboardLibDTO
    {
        public ConfigureDashboardModel ConfigurationRecord { get; set; }
        [DataMember]
        public List<UserRolesModel> Roles { get; set; }
        [DataMember]
        public List<OptionModel> RolesOptions { get; set; }
        [DataMember]
        public List<OptionModel> FeaturesOptions { get; set; }
    }
}