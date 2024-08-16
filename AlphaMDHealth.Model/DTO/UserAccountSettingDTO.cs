using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class UserAccountSettingDTO : BaseDTO
    {
        [DataMember]
        public List<UserAccountSettingsModel> UserAccountSettings { get; set; }
        [DataMember]
        public List<OptionModel> ReadingUnitOptions { get; set; }

        public UserAccountSettingsModel UserAccountSetting { get; set; }
    }
}
