using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model used to store device reading data
    /// </summary>
    public class PatientDeviceModel
    {
        /// <summary>
        /// Reading Source ID
        /// </summary>
        [PrimaryKey]
        public Guid ReadingSourceID { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// Deivce UUID
        /// </summary>
        public string DeviceUUID { get; set; }

        /// <summary>
        /// Device Identifier
        /// </summary>
        public string DeviceIdentifier { get; set; }

        /// <summary>
        /// Gets the native device object Address. Should be cast to the appropriate type on each platform.
        /// </summary>
        public string NativeDeviceAddress { get; set; }

        /// <summary>
        /// Model no
        /// </summary>
        public string ModelNumber { get; set; }

        /// <summary>
        /// Device serial no
        /// </summary>
        public string DeviceSerialNo { get; set; }

        /// <summary>
        /// Firmware Version
        /// </summary>
        public string DeviceFirmwareVersion { get; set; }

        /// <summary>
        /// Software revision
        /// </summary>
        public string SoftwareRevision { get; set; }

        /// <summary>
        /// Latest reading ID
        /// </summary>
        public long LastReadingID { get; set; }

        /// <summary>
        /// Device description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Device name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Last sync date time
        /// </summary>
        public DateTimeOffset LastSyncDateTime { get; set; }

        /// <summary>
        /// Is Active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Is Synced
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// Last sync date time
        /// </summary>
        public DateTimeOffset LastModifiedON { get; set; }

        /// <summary>
        /// Error Code
        /// </summary>
        public ErrorCode ErrCode { get; set; }

        /// <summary>
        /// Icon of device
        /// </summary>
        [Ignore]
        public string DeviceIcon { get; set; }

        //todo:
        ///// <summary>
        ///// Device Image
        ///// </summary>
        //[Ignore]
        //public ImageSource DeviceImage { get; set; }
        public byte[] ImageBytes { get; set; }

        /// <summary>
        /// List arrow icon
        /// </summary>
        [Ignore]
        public string DeviceArrowIcon { get; set; }

        /// <summary>
        /// Is Scanning
        /// </summary>
        [Ignore]
        public bool IsScaning { get; set; }

        /// <summary>
        /// Show nearby status
        /// </summary>
        [Ignore]
        public bool ShowNearbyStatus { get; set; }

        /// <summary>
        /// Nearby status
        /// </summary>
        [Ignore]
        public string NearbyStatus { get; set; }

        /// <summary>
        /// Show no record
        /// </summary>
        [Ignore]
        public bool ShowNoRecord { get; set; }

        /// <summary>
        /// Show Icon
        /// </summary>
        [Ignore]
        public bool ShowIcon { get; set; } = true;

        /// <summary>
        /// Record Count
        /// </summary>
        [Ignore]
        public int RecordCount { get; set; }
    }
}