using System.Globalization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using banditoth.MAUI.JailbreakDetector.Interfaces;
namespace AlphaMDHealth.MobileClient.Common;

/// <summary>
/// Represents Security helper
/// </summary>
public class SecurityHelper
{
    /// <summary>
    /// Apply security for application
    /// </summary>
    /// <param name="config">Security configuration</param>
    /// <returns>Tapjacking error code</returns>
    public Task<ErrorCode> ApplySecurityChecksAsync(SecurityConfigurationModel config)
    {

        ISecurityService securityService = DependencyService.Get<ISecurityService>();
        IJailbreakDetector jailbreakDetector = DependencyService.Get<IJailbreakDetector>();

        // Check for jailbroken device
        ErrorCode errorCode = CheckJailbrokenDevice(jailbreakDetector, config.BlockJailbrokenDevice);

        // Tab jacking allowed check
        ConfigureTapJacking(securityService, config.BlockTapJacking);

        // Screen shot allowed check
        ConfigureScreenshot(securityService, config.IsScreenshotAllowed);

        // Allow copy paste         
        ConfigureCopyPaste(config.IsReadCopyClipboardAllowed);

        // Certificate pining
        ConfigureCertificatePinning(config.IsCertifiatePiningAllowed);

        return Task.FromResult(errorCode);
    }
    private void ConfigureCertificatePinning(bool isCertifiatePiningAllowed)
    {
        Preferences.Set(StorageConstants.PR_IS_CERTIFICATE_PINING_ENABLE_KEY, isCertifiatePiningAllowed);
    }
    private void ConfigureCopyPaste(bool isReadCopyClipboardAllowed)
    {
        Preferences.Set(StorageConstants.PR_ALLOW_READ_COPY_CLIPBOARD_KEY, isReadCopyClipboardAllowed);
    }
    private void ConfigureScreenshot(ISecurityService securityService, bool isScreenshotAllowed)
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                securityService.ControlScreenSharing(Convert.ToBoolean(isScreenshotAllowed, CultureInfo.InvariantCulture));
            });
        }
        else
        {
            Preferences.Set(StorageConstants.PR_IS_SCREEN_SHARING_ENABLED_KEY, isScreenshotAllowed);
        }
    }
    private ErrorCode CheckJailbrokenDevice(IJailbreakDetector jailbreakDetector, bool blockJailbrokenDevice)
    {
        if (blockJailbrokenDevice)
        {
            var isRootedOrJail = jailbreakDetector.IsRootedOrJailbrokenAsync().Result;
            if (isRootedOrJail)
            {
                App._essentials.SetPreferenceValue<bool>(StorageConstants.PR_IS_JAIL_BROKEN_KEY, true);
            }
            if (App._essentials.GetPreferenceValue(StorageConstants.PR_IS_JAIL_BROKEN_KEY, false))
            {
                return ErrorCode.JailBrokenDevice;
            }
        }
        return ErrorCode.OK;
    }

    private void ConfigureTapJacking(ISecurityService securityService, bool blockTapJacking)
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            securityService.BlockTapJacking(blockTapJacking);
        }
    }

}


