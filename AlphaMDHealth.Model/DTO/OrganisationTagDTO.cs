using System.Runtime.Serialization;

namespace AlphaMDHealth.Model;

/// <summary>
/// Data transfer object for OrganisationTags
/// </summary>
public class OrganisationTagDTO : BaseDTO
{
    /// <summary>
    /// OrganisationTag Information
    /// </summary>
    public OrganisationTagModel OrganisationTag { get; set; }

    /// <summary>
    /// List Of OrganisationTags
    /// </summary>
    [DataMember]
    public List<OrganisationTagModel> OrganisationTags { get; set; }
}
