using AlphaMDHealth.Utility;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace AlphaMDHealth.Model;

public class StorageState
{
    /// <summary>
    /// Flag to set and get User Authentication
    /// </summary>
    [JsonIgnore]
    public bool IsUserAuthenticated
    {
        get
        {
            return SecuredStorage?.ContainsKey(StorageConstants.SS_ACCESS_TOKEN_KEY) ?? false;
        }
    }

    /// <summary>
    /// Browser model
    /// </summary>
    public string DeviceModel { get; set; }

    /// <summary>
    ///Browser manufacturer
    /// </summary>
    public string DeviceManufacturer { get; set; }

    /// <summary>
    /// Browser name
    /// </summary>
    public string DeviceName { get; set; }

    /// <summary>
    /// Device OS
    /// </summary>
    public string DeviceOS { get; set; }

    /// <summary>
    /// Device OS version
    /// </summary>
    public string DeviceOSVersionString { get; set; }

    /// <summary>
    /// Secured storage data collection
    /// </summary>
    [DataMember]
    public Dictionary<string, string> SecuredStorage { get; set; }

    /// <summary>
    /// Local storage data collection
    /// </summary>
    [DataMember]
    public Dictionary<string, object> Preferences { get; set; }

    /// <summary>
    /// Local time zone offset
    /// </summary>
    public int LocalOffset { get; set; }
}
