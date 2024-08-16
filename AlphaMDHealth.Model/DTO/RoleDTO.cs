using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class RoleDTO : BaseDTO
    {
        [DataMember]
        public List<UserRolesModel> Roles { get; set; }
    }
}
