namespace AlphaMDHealth.Model
{
    public class LibraryServiceLoggingModel
    {
        public long OrganisationServiceID { get; set; }
        public DateTimeOffset AddedON { get; set; }
        public string Status { get; set; }
        public string LogData { get; set; }
    }
}