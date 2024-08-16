using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class ProfessionDTO : BaseDTO
    {
        public ProfessionModel Profession { get; set; }
        [DataMember]
        public List<ProfessionModel> Professions { get; set; }
        [DataMember]
        public List<UserProfessionModel> UserProfessions { get; set; }
    }
}
