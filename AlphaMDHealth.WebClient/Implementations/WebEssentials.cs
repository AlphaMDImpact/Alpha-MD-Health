using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;
using System.Security.Cryptography;

namespace AlphaMDHealth.WebClient;

public class WebEssentials : IEssentials
{
    private readonly StorageState _storageState;

    /// <summary>
    /// Browser model
    /// </summary>
    public string DeviceModel => _storageState.DeviceModel;

    /// <summary>
    ///Browser manufacturer
    /// </summary>
    public string DeviceManufacturer => _storageState.DeviceManufacturer;

    /// <summary>
    /// Browser name
    /// </summary>
    public string DeviceName => _storageState.DeviceName;

    /// <summary>
    /// Device Type
    /// </summary>
    public string DeviceType => Constants.CLIENT_DEVICE_TYPE_WEB;

    /// <summary>
    /// Device OS
    /// </summary>
    public string DeviceOS => _storageState.DeviceOS;

    /// <summary>
    /// Device OS version
    /// </summary>
    public string DeviceOSVersionString => _storageState.DeviceOSVersionString;

    /// <summary>
    /// Web service essentials
    /// </summary>
    /// <param name="storageState">Instance of local storage service</param>
    public WebEssentials(StorageState storageState)
    {
        _storageState = storageState;
    }

    /// <summary>
    /// Remove all storage
    /// </summary>
    public void RemoveAllSecureStorage()
    {
        //todo:
    }

    /// <summary>
    /// Gets ID of the device
    /// </summary>
    /// <returns>Device ID</returns>
    public Task<string> GetDeviceIDAsync()
    {
        string deviceID = GetPreferenceValue(StorageConstants.PR_DEVICE_ID_KEY, string.Empty);
        if (string.IsNullOrWhiteSpace(deviceID))
        {
            deviceID = Guid.NewGuid().ToString() +
                  GenerateRandomNumber(100, 999).ToString(CultureInfo.CurrentCulture) +
                  DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            SetPreferenceValue(StorageConstants.PR_DEVICE_ID_KEY, deviceID);
        }
        return Task.FromResult(deviceID);
    }

    private int GenerateRandomNumber(int minValue, int maxValue)
    {
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);

            int randomInt = BitConverter.ToInt32(bytes, 0);

            // Ensure the generated number is within the specified range
            return Math.Abs(randomInt % (maxValue - minValue + 1)) + minValue;
        }
    }

    /// <summary>
    /// Checks if the given key is found in preferences
    /// </summary>
    /// <param name="key">Key which is to be found</param>
    /// <returns>true if key is found in preference</returns>
    public bool ContainsPreferenceKey(string key)
    {
        return _storageState?.Preferences?.ContainsKey(key) ?? false;
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
        if (_storageState?.Preferences?.ContainsKey(key) ?? false)
        {
            return (T)Convert.ChangeType(Convert.ToString(_storageState.Preferences[key]), typeof(T), CultureInfo.InvariantCulture);
        }
        return defaultValue;
    }

    /// <summary>
    /// Gets the value of the given key from secure storage
    /// </summary>
    /// <param name="key">Secure Storage key</param>
    /// <returns>Value for the given key</returns>
    public Task<string> GetSecureStorageValueAsync(string key)
    {
        return _storageState?.SecuredStorage?.ContainsKey(key) ?? false
            ? Task.FromResult(_storageState.SecuredStorage[key])
            : Task.FromResult(string.Empty);
    }

    /// <summary>
    /// Sets the value to the given key in preference
    /// </summary>
    /// <typeparam name="T">Supported types are: bool, int, double, float, long, string, DateTime</typeparam>
    /// <param name="key">Preference key</param>
    /// <param name="value">Value to be set</param>
    public bool SetPreferenceValue<T>(string key, T value)
    {
        if (_storageState == null)
        {
            return false;
        }
        _storageState.Preferences ??= new Dictionary<string, object>();
        if (_storageState.Preferences.ContainsKey(key))
        {
            _storageState.Preferences[key] = value;
        }
        else
        {
            _storageState.Preferences.Add(key, value);
        }
        return true;
    }

    /// <summary>
    /// Sets the value to the given key in secure storage
    /// </summary>
    /// <param name="key">Secure Storage key</param>
    /// <param name="value">Value to be set</param>
    /// <returns>Task of operation</returns>
    public Task SetSecureStorageValueAsync(string key, string value)
    {
        if (_storageState == null)
        {
            return Task.CompletedTask;
        }
        if (_storageState.SecuredStorage == null)
        {
            _storageState.SecuredStorage = new Dictionary<string, string>();
        }
        if (_storageState.SecuredStorage.ContainsKey(key))
        {
            _storageState.SecuredStorage[key] = value;
        }
        else
        {
            _storageState.SecuredStorage.Add(key, value);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes value of the given key from preferences
    /// </summary>
    /// <param name="key">Preference key</param>
    public void DeletePreferenceValue(string key)
    {
        if (_storageState?.Preferences?.ContainsKey(key) ?? false)
        {
            _storageState.Preferences.Remove(key);
        }
    }

    /// <summary>
    /// Deletes the value of the given key from secure storage
    /// </summary>
    /// <param name="key">Secure Storage key</param>
    public void DeleteSecureStorageValue(string key)
    {
        if (_storageState?.SecuredStorage?.ContainsKey(key) ?? false)
        {
            _storageState.SecuredStorage.Remove(key);
        }
    }

    /// <summary>
    /// Converts datetimeoffset to local time
    /// </summary>
    /// <param name="dateTime">Datetime value</param>
    /// <returns>Converted local datetimeoffset</returns>
    public DateTimeOffset ConvertToLocalTime(DateTimeOffset dateTime)
    {
        return dateTime.ToOffset(TimeSpan.FromMinutes(_storageState.LocalOffset));
    }
}