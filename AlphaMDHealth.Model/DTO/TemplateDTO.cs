using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class TemplateDTO : BaseDTO
    {
        public string Username { get; set; }
        //public bool IsEmailSent { get; set; }
        //public bool IsSmsSent { get; set; }
        //public bool IsPushNoticationSent { get; set; }
        //public bool IsWhatsAppSent { get; set; }
        [DataMember]
        public TemplateModel TemplateData { get; set; }
        public List<TemplateModel> Templates { get; set; }
        [DataMember]
        public Dictionary<string, string> UserData { get; set; }
        [DataMember]
        public List<Dictionary<string, string>> UsersData { get; set; }
        public List<FileDataModel> Attachments { get; set; }
    }
}
