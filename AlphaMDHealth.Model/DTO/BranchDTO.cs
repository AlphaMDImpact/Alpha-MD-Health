using System.Runtime.Serialization;

namespace AlphaMDHealth.Model;

public class BranchDTO : BaseDTO
{
    [DataMember]
    public List<BranchModel> Branches { get; set; }
    public BranchModel Branch { get; set; }
    [DataMember]
    public List<DepartmentModel> Departments { get; set; }
}
