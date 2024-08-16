using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model;

/// <summary>
/// class containing common setting methods
/// </summary>
public static class LibSettings
{
    /// <summary>
    /// Finds a particular setting based on key from settingData.
    /// </summary>
    /// <param name="settingKey">key for which setting to be fetched.</param>
    /// <returns>setting for specified setting key.</returns>
    public static SettingModel GetSettingByKey(IEnumerable<SettingModel> settings, string settingKey)
    {
        if (settings != null)
        {
            return settings.FirstOrDefault(x => x.SettingKey == settingKey);
        }
        return null;
    }

    /// <summary>
    /// Get setting value based on setting key
    /// </summary>
    /// <param name="settings">object to get setting</param>
    /// <param name="settingKey">key to find setting value</param>
    /// <returns>Setting value</returns>
    public static string GetSettingValueByKey(IEnumerable<SettingModel> settings, string settingKey)
    {
        if (settings != null)
        {
            return settings?.FirstOrDefault(x => x.SettingKey == settingKey)?.SettingValue ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Get formats in out params
    /// </summary>
    /// <param name="dayFormat">day format</param>
    /// <param name="monthFormat">month format</param>
    /// <param name="yearFormat">year format</param>
    public static bool TryGetDateFormatSettings(IEnumerable<SettingModel> settings, out string dayFormat, out string monthFormat, out string yearFormat)
    {
        dayFormat = GetSettingValueByKey(settings, SettingsConstants.S_EXTENDED_DAY_FORMAT_KEY);
        monthFormat = GetSettingValueByKey(settings, SettingsConstants.S_MONTH_FORMAT_KEY);
        yearFormat = GetSettingValueByKey(settings, SettingsConstants.S_YEAR_FORMAT_KEY);
        return true;
    }

    ///// <summary>
    ///// Finds a particular setting based on key from settingData.
    ///// </summary>
    ///// <param name="settingKey">key for which setting to be fetched.</param>
    ///// <returns>setting value for specified setting key.</returns>
    //public string GetSettingsValueByKey(string settingKey)
    //{
    //    if (LibGenericMethods.IsListNotEmpty(PageData?.Settings))
    //    {
    //        return PageData.Settings.FirstOrDefault(x => x.SettingKey == settingKey)?.SettingValue?.Trim() ?? string.Empty;
    //    }
    //    return string.Empty;
    //}
}