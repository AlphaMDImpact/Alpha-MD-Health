using AlphaMDHealth.Utility;
using SQLite;
using System.ComponentModel;

namespace AlphaMDHealth.Model;

public class UserModel : UserAccountModel, INotifyPropertyChanged
{
    [PrimaryKey]
    public long UserID { get; set; }
    public long SelectedOrganisationID { get; set; }
    public long UserTempID { get; set; }

    [MyCustomAttributes(ResourceConstants.R_FIRST_NAME_KEY)]
    public string FirstName { get; set; }

    [MyCustomAttributes(ResourceConstants.R_MIDDLE_NAME_KEY)]
    public string MiddleName { get; set; } = string.Empty;

    [MyCustomAttributes(ResourceConstants.R_LAST_NAME_KEY)]
    public string LastName { get; set; }
    public string Gender { get; set; }

    [MyCustomAttributes(ResourceConstants.R_GENDER_KEY)]
    public string GenderID { get; set; }

    [MyCustomAttributes(ResourceConstants.R_PROFILE_IMAGE_KEY)]
    public string ImageName { get; set; } = string.Empty;
    public string BloodGroup { get; set; }

    [MyCustomAttributes(ResourceConstants.R_BLOOD_GROUP_KEY)]
    public short BloodGroupID { get; set; }

    [MyCustomAttributes(ResourceConstants.R_PREFERRED_LANGUAGE_KEY)]
    public byte PrefferedLanguageID { get; set; }

    [MyCustomAttributes(ResourceConstants.R_SOCIAL_SECURITY_NUMBER_KEY)]
    public string SocialSecurityNo { get; set; } = string.Empty;

    [MyCustomAttributes(ResourceConstants.R_EXTERNAL_CODE_KEY)]
    public string GeneralMedicalIdenfier { get; set; } = string.Empty;

    [MyCustomAttributes(ResourceConstants.R_INTERNAL_CODE_KEY)]
    public string HospitalIdenfier { get; set; } = string.Empty;
    public long DepartmentID { get; set; }
    public long BranchID { get; set; }
    [MyCustomAttributes(ResourceConstants.R_ROLES_KEY)]
    public byte RoleID { get; set; }
    public long RoleAtLevelID { get; set; }
    public short? RelationID { get; set; } = null;
    [MyCustomAttributes(ResourceConstants.R_PROFESSION_KEY)]
    public byte ProffessionID { get; set; }
    public string Proffession { get; set; }

    public string UserDegree { get; set; }
    public bool IsActive { get; set; }
    public bool IsSynced { get; set; }
    public string RoleName { get; set; }
    [MyCustomAttributes(ResourceConstants.R_DATE_OF_BIRTH_KEY)]
    public DateTimeOffset? Dob { get; set; }
    [MyCustomAttributes(ResourceConstants.R_DATE_OF_JOINING_KEY)]

    public DateTimeOffset? Doj { get; set; }
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
    public long PatientID { get; set; }
    [Ignore]
    public double Weight { get; set; }
    [Ignore]
    public double Height { get; set; }
    [MyCustomAttributes(ResourceConstants.R_AGE_KEY)]
    public byte UserAge { get; set; }
    [Ignore]
    public string OrganisationDomain { get; set; }
    [Ignore]
    public string DateOfBirth { get; set; }
    [Ignore]
    public bool IsUser { get; set; }

    public bool IsLinkedUser { get; set; }
    [Ignore]
    public string UserInitials { get; set; }

    [Ignore]
    public string FullName { get; set; } = string.Empty;

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    [Ignore]
    public FileStream ImageStream { get; set; }
    public string ExcelPath { get; set; }
    public string ExcelName { get; set; }
    public bool IsMobile { get; set; }
    public int ErrorCode { get; set; } = 200;
    /// <summary>
    /// This field will help to identify if user is viewed from Mange Organisation Screen
    /// </summary>
    public bool IsViewedFromOther { get; set; }

    public string UserDegrees { get; set; }

    /// <summary>
    /// Stores Last Modified On Date
    /// </summary>
    public DateTimeOffset? LastModifiedON { get; set; }
    /// <summary>
    /// Stores Last Modified On Date
    /// </summary>
    public DateTimeOffset? AddedON { get; set; }
    [MyCustomAttributes(ResourceConstants.R_MEDICAL_LICENSE_NUMBER_KEY)]
    public string MedicalLicenseNumber { get; set; } = string.Empty;
    public string OrganisationTags { get; set; }
}