using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model;

public class UserAccountModel
{
    public long AccountID { get; set; }

    [MyCustomAttributes(ResourceConstants.R_ORGANISATION_KEY)]
    public long OrganisationID { get; set; }
    public string EmailId { get; set; }
    public string PhoneNo { get; set; }
    public string AccountPassword { get; set; }
    public bool RememberMe { get; set; }
    public bool IsTempPassword { get; set; }
    public string Otp { get; set; }
    [Ignore]
    public bool IsSelfRegistration { get; set; }
}
