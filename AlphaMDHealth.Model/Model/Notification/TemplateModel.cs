using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class TemplateModel
    {
        public long CommunicationID { get; set; }
        public string ServiceTemplateID { get; set; }
        public string BccIds { get; set; }
        public string CcIds { get; set; }
        public string EmailApiKey { get; set; }
        public string EmailApiKeyID { get; set; }
        public string EmailHeader { get; set; }
        public string EmailBody { get; set; }
        public string FromId { get; set; }
        public string OrganisationLogo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string AppName { get; set; }
        public string Otp { get; set; }
        public string SmsApiKeyId { get; set; }
        public string SmsBody { get; set; }
        public string SmsHeader { get; set; }
        public string AlertBody { get; set; }
        public string AlertHeader { get; set; }
        public string WhatsAppHeader { get; set; }
        public string WhatsAppBody { get; set; }
        public string SmsProviderDomain { get; set; }
        public string SmsSenderId { get; set; }
        public int TemplateID { get; set; }
        public TemplateName TemplateName { get; set; }
        public string TemplateType { get; set; }
        public string ToId { get; set; }
        public string ToPhoneNo { get; set; }
        public string ToUserName { get; set; }
        public string FormatedUsersDetails { get; set; }
        public string FormattedUserIDs { get; set; }
        public string CommunicationType { get; set; }
        public string CommunicationDateTime { get; set; }
        public string HeaderBackGroundColor { get; set; }
        public string HeaderFontColor { get; set; }
        public string FooterBackGroundColor { get; set; }
        public string FooterFontColor { get; set; }
        public bool IsEmailSent { get; set; }
        public bool IsSMSSent { get; set; }
        public bool IsNotificationSent { get; set; }
        public bool IsWhatsAppSent { get; set; }
        public string Attachments { get; set; }
        public bool IsExternal { get; set; }
        public string ExternalEmailID { get; set; }
        public string ExternalMobileNo { get; set; }
        public string ExternalUserName { get; set; }
        public string DoctorName { get; set; }
        public string PatientName { get; set; }
        public string RegistrationLink { get; set; }
        public string DataRecordPrimaryKey { get; set; }
        public DateTimeOffset? AddedON { get; set; }
    }
}