namespace AlphaMDHealth.Utility;

/// <summary>
/// Represents Service essentials module
/// </summary>
public interface IEssentials
{
    /// <summary>
    /// Device or Browser model
    /// </summary>
    string DeviceModel { get; }

    /// <summary>
    /// Device or Browser manufacturer
    /// </summary>
    string DeviceManufacturer { get; }

    /// <summary>
    /// Device or Browser name
    /// </summary>
    string DeviceName { get; }

    /// <summary>
    /// Device Type
    /// </summary>
    string DeviceType { get; }

    /// <summary>
    /// Device OS
    /// </summary>
    string DeviceOS { get; }

    /// <summary>
    /// Device OS version/Browser version
    /// </summary>
    string DeviceOSVersionString { get; }

    /// <summary>
    /// Remove all storage
    /// </summary>
    void RemoveAllSecureStorage();

    /// <summary>
    /// Deletes the value of the given key from secure storage
    /// </summary>
    /// <param name="key">Secure Storage key</param>
    void DeleteSecureStorageValue(string key);

    /// <summary>
    /// Gets the value of the given key from secure storage
    /// </summary>
    /// <param name="key">Secure Storage key</param>
    /// <returns>Value for the given key</returns>
    Task<string> GetSecureStorageValueAsync(string key);

    /// <summary>
    /// Sets the value to the given key in secure storage
    /// </summary>
    /// <param name="key">Secure Storage key</param>
    /// <param name="value">Value to be set</param>
    /// <returns>Task of operation</returns>
    Task SetSecureStorageValueAsync(string key, string value);

    /// <summary>
    /// Gets ID of the device
    /// </summary>
    /// <returns>Device ID</returns>
    Task<string> GetDeviceIDAsync();

    /// <summary>
    /// Checks if the given key is found in preferences
    /// </summary>
    /// <param name="key">Key which is to be found</param>
    /// <returns>true if key is found in preference</returns>
    bool ContainsPreferenceKey(string key);

    /// <summary>
    /// Gets the value of the given key from preferences
    /// </summary>
    /// <typeparam name="T">Supported types are: bool, int, double, float, long, string, DateTime</typeparam>
    /// <param name="key">Preference key</param>
    /// <param name="defaultValue">Default value to return if the key does not exist</param>
    /// <returns>Value for the given key, or the default if it does not exist</returns>
    T GetPreferenceValue<T>(string key, T defaultValue);

    /// <summary>
    /// Sets the value to the given key in preference
    /// </summary>
    /// <typeparam name="T">Supported types are: bool, int, double, float, long, string, DateTime</typeparam>
    /// <param name="key">Preference key</param>
    /// <param name="value">Value to be set</param>
    bool SetPreferenceValue<T>(string key, T value);

    /// <summary>
    /// Deletes value of the given key from preferences
    /// </summary>
    /// <param name="key">Preference key</param>
    void DeletePreferenceValue(string key);

    /// <summary>
    /// Converts datetimeoffset to local time
    /// </summary>
    /// <param name="dateTime">Datetime value</param>
    /// <returns>Converted local datetimeoffset</returns>
    DateTimeOffset ConvertToLocalTime(DateTimeOffset dateTime);
}