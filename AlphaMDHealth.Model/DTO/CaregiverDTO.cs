using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class CaregiverDTO : UsersDTO
    {
        public CaregiverModel Caregiver { get; set; }
        [DataMember]
        public List<CaregiverModel> Caregivers { get; set; }
        [DataMember]
        public List<OptionModel> CaregiverOptions { get; set; }
        [DataMember]
        public List<OptionModel> DepartmentList { get; set; }
        [DataMember]
        public List<OptionModel> CaregiverList { get; set; }
    }
}
