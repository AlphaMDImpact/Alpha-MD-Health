namespace AlphaMDHealth.Model
{
    public class SystemIdentifierModel
    {
        public string ClientApplicationKey { get; set; }
        public string ClientIdentifierKey { get; set; }
        public string DeviceType { get; set; }
        public string DevicePlatform { get; set; }
        public string IdentifierTags { get; set; }
        public int SystemIdentifierID { get; set; }
        public string SystemName { get; set; }
        public string AllowEndpoints { get; set; }
    }
}