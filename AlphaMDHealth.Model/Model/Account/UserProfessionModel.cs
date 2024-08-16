using SQLite;

namespace AlphaMDHealth.Model
{
    public class UserProfessionModel
    {
        [PrimaryKey]
        public byte ProfessionID { get; set; }
        public string Profession { get; set; }
        public bool IsActive { get; set; }
    }
}