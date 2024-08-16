using AlphaMDHealth.Utility;
using Microsoft.AppCenter;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Service essential implementation for mobile
/// </summary>
public class MobileEssentials : IEssentials
{
    /// <summary>
    /// Device model
    /// </summary>
    public string DeviceModel => DeviceInfo.Model;

    /// <summary>
    /// Device manufacturer
    /// </summary>
    public string DeviceManufacturer => DeviceInfo.Manufacturer;

    /// <summary>
    /// Device name
    /// </summary>
    public string DeviceName => DeviceInfo.Name;

    /// <summary>
    /// Device Type
    /// </summary>
    public string DeviceType => DeviceInfo.Idiom.ToString().Substring(0, 1);

    /// <summary>
    /// Device OS
    /// </summary>
    public string DeviceOS => DeviceInfo.Platform.ToString();

    /// <summary>
    /// Device OS version
    /// </summary>
    public string DeviceOSVersionString => DeviceInfo.VersionString;

    /// <summary>
    /// Remove all storage
    /// </summary>
    public void RemoveAllSecureStorage()
    {
        SecureStorage.Default.RemoveAll();
    }

    /// <summary>
    /// Deletes the value of the given key from secure storage
    /// </summary>
    /// <param name="key">Secure Storage key</param>
    public void DeleteSecureStorageValue(string key)
    {
        SecureStorage.Default.Remove(key);
    }

    /// <summary>
    /// Gets the value of the given key from secure storage
    /// </summary>
    /// <param name="key">Secure Storage key</param>
    /// <returns>Value for the given key</returns>
    public async Task<string> GetSecureStorageValueAsync(string key)
    {
        string value = string.Empty;
        try
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                value = await SecureStorage.Default.GetAsync(key).ConfigureAwait(false);
                value = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
            }
        }
        catch
        {
            await Task.Delay(50).ConfigureAwait(false);
            return await GetSecureStorageValueAsync(key).ConfigureAwait(false);
        }
        return value;
    }

    /// <summary>
    /// Sets the value to the given key in secure storage
    /// </summary>
    /// <param name="key">Secure Storage key</param>
    /// <param name="value">Value to be set</param>
    /// <returns>Task of operation</returns>
    public Task SetSecureStorageValueAsync(string key, string value)
    {
        return SecureStorage.Default.SetAsync(key, string.IsNullOrWhiteSpace(value) ? string.Empty : value);
    }

    /// <summary>
    /// Checks if the given key is found in preferences
    /// </summary>
    /// <param name="key">Key which is to be found</param>
    /// <returns>true if key is found in preference</returns>
    public bool ContainsPreferenceKey(string key)
    {
        return Preferences.ContainsKey(key);
    }

    /// <summary>
    /// Deletes value of the given key from preferences
    /// </summary>
    /// <param name="key">Preference key</param>
    public void DeletePreferenceValue(string key)
    {
        Preferences.Remove(key);
    }

    /// <summary>
    /// Gets Device ID and generates new if not generated previously
    /// </summary>
    /// <returns>Device ID</returns>
    public async Task<string> GetDeviceIDAsync()
    {
        string deviceID = GetPreferenceValue(StorageConstants.PR_DEVICE_ID_KEY, string.Empty);
        if (string.IsNullOrWhiteSpace(deviceID))
        {
            deviceID = (await AppCenter.GetInstallIdAsync().ConfigureAwait(false)).ToString();
            SetPreferenceValue(StorageConstants.PR_DEVICE_ID_KEY, deviceID);
        }
        return deviceID;
    }

    /// <summary>
    /// Gets the value of the given key from preferences
    /// </summary>
    /// <typeparam name="T">Supported types are: bool, int, double, float, long, string, DateTime</typeparam>
    /// <param name="key">Preference key</param>
    /// <param name="defaultValue">Default value to return if the key does not exist</param>
    /// <returns>Value for the given key, or the default if it does not exist</returns>
    public T GetPreferenceValue<T>(string key, T defaultValue)
    {
        switch (defaultValue)
        {
            case string value:
                return (T)(object)Preferences.Get(key, value);
            case int value:
                return (T)(object)Preferences.Get(key, value);
            case byte value:
                return (T)(object)Preferences.Get(key, value);
            case bool value:
                return (T)(object)Preferences.Get(key, value);
            case long value:
                return (T)(object)Preferences.Get(key, value);
            case double value:
                return (T)(object)Preferences.Get(key, value);
            case float value:
                return (T)(object)Preferences.Get(key, value);
            case DateTime value:
                return (T)(object)Preferences.Get(key, value);
            default:
                return (T)(object)Preferences.Get(key, null);
        }
    }

    /// <summary>
    /// Sets the value to the given key in preference
    /// </summary>
    /// <typeparam name="T">Supported types are: bool, int, double, float, long, string, DateTime</typeparam>
    /// <param name="key">Preference key</param>
    /// <param name="value">Value to be set</param>
    public bool SetPreferenceValue<T>(string key, T value)
    {
        switch (value)
        {
            case string valueToSet:
                Preferences.Set(key, valueToSet);
                break;
            case int valueToSet:
                Preferences.Set(key, valueToSet);
                break;
            case byte valueToSet:
                Preferences.Set(key, valueToSet);
                break;
            case bool valueToSet:
                Preferences.Set(key, valueToSet);
                break;
            case long valueToSet:
                Preferences.Set(key, valueToSet);
                break;
            case double valueToSet:
                Preferences.Set(key, valueToSet);
                break;
            case float valueToSet:
                Preferences.Set(key, valueToSet);
                break;
            case DateTime valueToSet:
                Preferences.Set(key, valueToSet);
                break;
            default:
                throw new NotSupportedException();
        }
        return true;
    }

    /// <summary>
    /// Converts datetimeoffset to local time
    /// </summary>
    /// <param name="dateTime">Datetime value</param>
    /// <returns>Converted local datetimeoffset</returns>
    public DateTimeOffset ConvertToLocalTime(DateTimeOffset dateTime)
    {
        return dateTime.ToLocalTime();
    }

}