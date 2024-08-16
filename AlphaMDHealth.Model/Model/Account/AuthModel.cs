using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class AuthModel
    {
        public string UserName { get; set; }
        public string PhoneNo { get; set; }
        public string AccountPassword { get; set; }
        public bool RememberMe { get; set; }
        public string Otp { get; set; }
        public string OldPassword { get; set; }
        public string EmailID { get; set; }
        public long UserOrganisationID { get; set; }
        public long UserAccountID { get; set; }
        public int LockoutDuration { get; set; }
        public Pages PageType { get; set; }
		public byte RoleID { get; set; }
        public bool IsExternal { get; set; }
    }
}
