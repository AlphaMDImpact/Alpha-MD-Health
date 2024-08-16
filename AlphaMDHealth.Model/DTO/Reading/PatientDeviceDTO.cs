using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Patient Device DTO
    /// </summary>
    public class PatientDeviceDTO : BaseDTO
    {
        /// <summary>
        /// Device data
        /// </summary>
        public PatientDeviceModel Device { get; set; }

        /// <summary>
        /// Devices list
        /// </summary>
        [DataMember]
        public List<PatientDeviceModel> Devices { get; set; }

        /// <summary>
        /// Paired devices
        /// </summary>
        [DataMember]
        public List<PatientDeviceModel> PairdDevices { get; set; }

        /// <summary>
        /// Reading data
        /// </summary>
        public PatientReadingDTO ReadingData { get; set; }
    }
}