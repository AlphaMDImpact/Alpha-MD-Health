using System.Runtime.Serialization;

namespace AlphaMDHealth.Model;

public class UsersDTO : BaseDTO
{
    [DataMember]
    public List<OptionModel> Organisations { get; set; }
    [DataMember]
    public List<OptionModel> Departments { get; set; }
    [DataMember]
    public List<OptionModel> Branches { get; set; }
}