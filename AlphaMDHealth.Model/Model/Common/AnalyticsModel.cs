namespace AlphaMDHealth.Model
{
    public class AnalyticsModel
    {
        public string OperatingSystem { get; set; }
        public string OSVersion { get; set; }
        public string DeviceManufacturer { get; set; }
        public string DeviceModel { get; set; }
        public string PageName { get; set; }
        public DateTimeOffset LogDateTime { get; set; }
    }
}
