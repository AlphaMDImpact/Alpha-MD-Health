using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class LibraryServiceModel
    {
        public long OrganisationServiceID { get; set; }
        public string ForApplication { get; set; }
        public string ClientIdentifier { get; set; }
        public string ClientSecrete { get; set; }
        public string ServiceClientIdentifier { get; set; }
        public string ServiceClientSecrete { get; set; }
        public string ServiceTarget { get; set; }
        public string IdentifierFor { get; set; }
        public ServiceType ServiceType { get; set; }
        public string ServiceCategory { get; set; }
        public bool LogCalls { get; set; }
        public string ContainerName { get; set; }
    }
}