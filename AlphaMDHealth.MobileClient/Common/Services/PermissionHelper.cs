using AlphaMDHealth.Utility;
using Device = Microsoft.Maui.Controls.Device;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Permission extension methods
/// </summary>
public static class PermissionHelper
{
    private static BasePage _parentPage;

    /// <summary>
    /// Requests the permissions from the users
    /// </summary>
    /// <returns>The permissions and their status.</returns>
    /// <param name="permissions">Permissions to request.</param>
    public static async Task<Dictionary<Permission, PermissionStatus>> RequestPermissionsAsync(params Permission[] permissions)
    {
        Dictionary<Permission, PermissionStatus> permissionStatus = new Dictionary<Permission, PermissionStatus>();
        foreach (var permission in permissions)
        {
            switch (permission)
            {
                case Permission.Photos:
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        await RequestAndroidStoragePermissionAsync(permissionStatus, permission).ConfigureAwait(true);
                    }
                    else
                    {
                        permissionStatus.Add(permission, await Permissions.RequestAsync<Permissions.Photos>().ConfigureAwait(true));
                    }
                    break;
                case Permission.Storage:
                    await RequestAndroidStoragePermissionAsync(permissionStatus, permission).ConfigureAwait(true);
                    break;
                case Permission.Camera:
                    permissionStatus.Add(permission, await Permissions.RequestAsync<Permissions.Camera>().ConfigureAwait(true));
                    break;
                case Permission.Microphone:
                    permissionStatus.Add(permission, await Permissions.RequestAsync<Permissions.Microphone>().ConfigureAwait(true));
                    break;
                //todo:
                //case Permission.ActivityRecognition:
                //    permissionStatus.Add(permission, await DependencyService.Get<IActivityRecognitionPermission>().RequestAsync());
                //    break;
                default:// to do
                    break;
            }
        }
        return permissionStatus;
    }

    /// <summary>
    /// Check and grant the permission to application
    /// </summary>
    /// <param name="permission">to be check and grant</param>
    /// <param name="parentPage">page from which permission is being requested</param>
    /// <returns>True= Permission granted Else False</returns>
    public static async Task<bool> CheckPermissionStatusAsync(Permission permission, BasePage parentPage)
    {
        _parentPage = parentPage;
        Dictionary<Permission, PermissionStatus> permissionStatus = await RequestPermissionsAsync(permission).ConfigureAwait(true);
        PermissionStatus status = permissionStatus.ContainsKey(permission) ? permissionStatus[permission] : PermissionStatus.Unknown;
        if (status != PermissionStatus.Granted
            && await DisplayAlertAsync($"{permission}{ResourceConstants.R_PERMISSION_TEXT_KEY}").ConfigureAwait(true))
        {
            if (ShouldShowRationale(permission))
            {
                permissionStatus = await RequestPermissionsAsync(permission).ConfigureAwait(false);
                status = permissionStatus.ContainsKey(permission) ? permissionStatus[permission] : PermissionStatus.Unknown;
            }
            else
            {
                AppInfo.ShowSettingsUI();
            }
        }
        return status == PermissionStatus.Granted;
    }

    private static async Task RequestAndroidStoragePermissionAsync(Dictionary<Permission, PermissionStatus> permissionStatus, Permission permission)
    {
        PermissionStatus storageStatus = await Permissions.RequestAsync<Permissions.StorageRead>().ConfigureAwait(true);
        if (storageStatus == PermissionStatus.Granted)
        {
            storageStatus = await Permissions.RequestAsync<Permissions.StorageWrite>().ConfigureAwait(true);
        }
        permissionStatus.Add(permission, storageStatus);
    }

    private static bool ShouldShowRationale(Permission permission)
    {
        switch (permission)
        {
            case Permission.Photos:
                if (Device.RuntimePlatform == Device.Android)
                {
                    return ShouldShowRationale(Permission.Storage);
                }
                else
                {
                    return Permissions.ShouldShowRationale<Permissions.Photos>();
                }
            case Permission.Storage:
                return Permissions.ShouldShowRationale<Permissions.StorageRead>() && Permissions.ShouldShowRationale<Permissions.StorageWrite>();
            case Permission.Camera:
                return Permissions.ShouldShowRationale<Permissions.Camera>();
            case Permission.Microphone:
                return Permissions.ShouldShowRationale<Permissions.Microphone>();
            //todo:
            //case Permission.ActivityRecognition:
            //    return DependencyService.Get<IActivityRecognitionPermission>().ShouldShowRationale();
            default:
                return false;
        }
    }

    private static async Task<bool> DisplayAlertAsync(string messageKey)
    {
        return await _parentPage.DisplayMessagePopupAsync(messageKey, true, true, false).ConfigureAwait(false);
    }
}