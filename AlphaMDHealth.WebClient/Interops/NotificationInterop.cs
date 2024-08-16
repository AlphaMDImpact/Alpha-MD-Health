using System.Threading.Tasks;
using Microsoft.JSInterop;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;


namespace AlphaMDHealth.WebClient
{
    public static class NotificationInterop
    {
        public static ValueTask<bool> IsSupported(this IJSRuntime jsRuntime) =>
           jsRuntime?.InvokeAsync<bool>("notificationInterop.isSupported") ?? new ValueTask<bool>(false);

        public static ValueTask<string> RequestPermissionAsync(this IJSRuntime jsRuntime) =>
          jsRuntime?.InvokeAsync<string>("notificationInterop.requestPermission") ?? new ValueTask<string>();

        public static ValueTask<bool> Create(this IJSRuntime jsRuntime) =>
         jsRuntime?.InvokeAsync<bool>("notificationInterop.create") ?? new ValueTask<bool>(false);

        public static ValueTask<bool> Create(this IJSRuntime jsRuntime,string tittle, NotificationOptions options) =>
         jsRuntime?.InvokeAsync<bool>("notificationInterop.create", tittle, options) ?? new ValueTask<bool>(false);
    }
}
