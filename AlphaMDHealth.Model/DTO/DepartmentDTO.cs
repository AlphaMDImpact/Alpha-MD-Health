using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class DepartmentDTO : BaseDTO
    {
        public DepartmentModel Department { get; set; }
        [DataMember]
        public List<DepartmentModel> Departments { get; set; }
    }
}
