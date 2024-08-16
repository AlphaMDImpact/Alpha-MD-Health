namespace AlphaMDHealth.Model
{
    public class DeviceModel
    {
        public Guid DeviceID { get; set; }
        public string Name { get; set; }
        //todo:
        //public Device Device { get; set; }
        public string ManufacturerName { get; set; }
        public string ModelNumber { get; set; }
        public string SerialNumber { get; set; }
        public string HardwareRevision { get; set; }
        public string FirmwareRevision { get; set; }
        public string SoftwareRevision { get; set; }
        public string SystemID { get; set; }
        public long DeviceSequence { get; set; }
        public long DevicePartialSequence { get; set; }
        public string UserId { get; set; }
        //todo:
        //public IDevice BleDevice { get; set; }
    }
}
