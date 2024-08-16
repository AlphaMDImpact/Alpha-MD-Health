namespace AlphaMDHealth.Model
{
    //todo: 
    public class ReadingDeviceModel
    {
        public long ReadingID { get; set; }
        public long ReadingTypeID { get; set; }
        public string DeviceIdentifier { get; set; }
        public bool IsActive { get; set; }
    }
}
