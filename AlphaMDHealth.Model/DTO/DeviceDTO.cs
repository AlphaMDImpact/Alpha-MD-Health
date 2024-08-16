using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class DeviceDTO : BaseDTO
    {
        /// <summary>
        /// List of devices
        /// </summary>
        [DataMember]
        public List<DeviceModel> Devices { get; set; }

        public DeviceModel Device { get; set; }
    }
}