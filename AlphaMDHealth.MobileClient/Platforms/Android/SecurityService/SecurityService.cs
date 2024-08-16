using AlphaMDHealth.MobileClient.Common;
using AlphaMDHealth.MobileClient.Platforms.Android.SecurityService;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Dependency(typeof(SecurityService))]
namespace AlphaMDHealth.MobileClient.Platforms.Android.SecurityService
{
    public class SecurityService : ISecurityService
    {
        /// <summary>
        /// Disables tap jacking
        /// </summary>
        /// <param name="blockTapJacking">true if tap jacking is to be blocked, else false</param>
        public void BlockTapJacking(bool blockTapJacking)
        {
            var window = Platform.CurrentActivity.Window;
            window.DecorView.RootView.FilterTouchesWhenObscured = blockTapJacking;
            window.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
        }

        /// <summary>
        /// Checks if application has permission to share the screen
        /// </summary>
        /// <param name="isEnabled"> boolean value to check whether screenshare is allowed or not</param>
        public void ControlScreenSharing(bool isEnabled)
        {
            var window = Platform.CurrentActivity.Window;
            if (isEnabled)
            {
                window.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
            }
            else
            {
                window.ClearFlags(WindowManagerFlags.Secure);
            }
        }
    }
}
