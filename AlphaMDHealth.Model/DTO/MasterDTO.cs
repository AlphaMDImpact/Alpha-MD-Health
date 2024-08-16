using System.Runtime.Serialization;

namespace AlphaMDHealth.Model;

public class MasterDTO : BaseDTO
{
    public string AbsolutePath { get; set; }
    public string OrganisationDomain { get; set; }
    public string OrganisationName { get; set; }
    public string DefaultRoute { get; set; }
    public bool HasWelcomeScreens { get; set; } = false;
    public bool IsConsentAccepted { get; set; } = true;
    public bool IsSubscriptionRequired { get; set; }
    public bool IsProfileCompleted { get; set; } = true;
    public SessionModel Session { get; set; }

    [DataMember]
    public List<OrganizationFeaturePermissionModel> OrganisationFeatures { get; set; }
    [DataMember]
    public List<LanguageModel> Languages { get; set; }
    [DataMember]
    public List<OrganisationCollapsibleModel> BranchDepartments { get; set; }
    [DataMember]
    public List<UserModel> Users { get; set; }
    [DataMember]
    public List<MenuModel> Menus { get; set; }
    [DataMember]
    public List<MenuModel> MenuGroups { get; set; }
    [DataMember]
    public List<MobileMenuNodeModel> MenuNodes { get; set; }
}