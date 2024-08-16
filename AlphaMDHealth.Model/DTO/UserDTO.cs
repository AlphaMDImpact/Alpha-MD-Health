using AlphaMDHealth.Utility;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model;

public class UserDTO : UsersDTO
{
    public AppPermissions ViewFor;
    public string SampleFilePath;
    public bool IsCompleteProfileFlow { get; set; }

    public SessionModel Session { get; set; }
    public UserModel User { get; set; }

    [DataMember]
    public List<UserModel> Users { get; set; }

    public UserRelationModel UserRelation { get; set; }

    [DataMember]
    public List<UserRelationModel> UserRelations { get; set; }

    [DataMember]
    public List<OptionModel> Languages { get; set; }

    [DataMember]
    public List<OptionModel> Genders { get; set; }

    [DataMember]
    public List<OptionModel> BloodGroups { get; set; }

    [DataMember]
    public List<OptionModel> Roles { get; set; }

    [DataMember]
    public List<OptionModel> Professions { get; set; }

    [DataMember]
    public List<OptionModel> UserDegrees { get; set; }

    [DataMember]
    public List<OrganizationFeaturePermissionModel> UserFeatures { get; set; }

    [DataMember]
    public List<PatientProgramModel> Programs { get; set; }

    [DataMember]
    public List<OptionModel> UserRelationTypes { get; set; }

    [DataMember]
    public List<PatientsSharedProgramsModel> PatientsSharedPrograms { get; set; }

    [DataMember]
    public List<OptionModel> OrganisationTags { get; set; }

    /// <summary>
    /// AttachmentBase64
    /// </summary>
    public string AttachmentBase64 { get; set; }

    /// <summary>
    /// Flag return true if it is ChatsView
    /// </summary>
    public bool IsChatsView { get; set; }

    //[Obsolete]
    ///// <summary>
    ///// Flag return true if it is Profile page
    ///// </summary>
    //public bool IsProfile { get; set; }
}