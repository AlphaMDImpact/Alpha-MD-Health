using SQLite;

namespace AlphaMDHealth.Model
{
    public class UserRolesModel
    {
        [PrimaryKey]
        public byte RoleID { get; set; }
        public bool IsActive { get; set; }
        public byte RoleLevel { get; set; }
        public string RoleName { get; set; }
    }
}