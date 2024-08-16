using AlphaMDHealth.Utility;
using System.Text.Json.Serialization;

namespace AlphaMDHealth.Model;

public class SessionModel
{
    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }

    public long AccountID { get; set; }

    public string PinCode { get; set; }

    [JsonIgnore]
    public string ConfirmPinCode { get; set; }

    [JsonIgnore]
    public string DeviceID { get; set; }

    [JsonIgnore]
    public string DeviceType { get; set; }

    [JsonIgnore]
    public string DevicePlatform { get; set; }

    [JsonIgnore]
    public string DeviceOS { get; set; }

    /// <summary>
    /// This provides the browser version in case of web and mobile OS version in case of mobile app
    /// </summary>
    [JsonIgnore]
    public string DeviceOSVersion { get; set; }

    [JsonIgnore]
    public string DeviceModel { get; set; }

    [JsonIgnore]
    public string DeviceDetail { get; set; }

    [JsonIgnore]
    public string ClientIdentifier { get; set; }

    [JsonIgnore]
    public ErrorCode? IgnoreErrorCode { get; set; }
}