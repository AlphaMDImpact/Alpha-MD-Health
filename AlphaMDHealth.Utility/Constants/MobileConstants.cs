namespace AlphaMDHealth.Utility;

public static class MobileConstants
{
    /// <summary>
    /// Checks internet conectivity
    /// </summary>
    /// <returns>Internet connection status</returns>
    public static bool CheckInternet
    {
        get
        {
            try
            {
                if (IsDevicePhone || IsDeviceTablet)
                {
                    NetworkAccess accessType = Connectivity.Current.NetworkAccess;
                    // Connection to internet is available
                    return accessType == NetworkAccess.Internet;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Returns true if platform is Android or iOS
    /// </summary>
    public static bool IsMobilePlatform
    {
        get
        {
            try
            {
                return IsAndroidPlatform || IsIosPlatform;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Returns true if platform is iOS
    /// </summary>
    public static bool IsIosPlatform
    {
        get
        {
            try
            {
                return DeviceInfo.Platform == DevicePlatform.iOS;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Returns true if platform is Android
    /// </summary>
    public static bool IsAndroidPlatform
    {
        get
        {
            try
            {
                return DeviceInfo.Platform == DevicePlatform.Android;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Returns true if Idiom is Tablet
    /// </summary>
    public static bool IsTablet
    {
        get
        {
            try
            {
                return DeviceInfo.Idiom == DeviceIdiom.Tablet;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Returns true if Idiom is Phone
    /// </summary>
    public static bool IsDeviceTablet
    {
        get
        {
            try
            {
                return DeviceInfo.Idiom == DeviceIdiom.Tablet;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Returns true if Idiom is Phone
    /// </summary>
    public static bool IsDevicePhone
    {
        get
        {
            try
            {
                return DeviceInfo.Idiom == DeviceIdiom.Phone;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// footer max count
    /// </summary>
    public static int FOOTER_MENU_MAX_COUNT
    {
        get
        {
            try
            {
                return IsIosPlatform && IsTablet ? 8 : 5;
            }
            catch
            {
                return 5;
            }
        }
    }
}