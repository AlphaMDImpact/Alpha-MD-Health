using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AlphaMDHealth.WebClient
{
    public partial class App : IDisposable
    {
        bool isloading = false;

        [Parameter]
        public string UserAgent { get; set; }

        [Parameter]
        public string IPAddress { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await RenderMasterAsync(firstRender).ConfigureAwait(false);
        }

        private async Task RenderMasterAsync(bool firstRender)
        {
            if (!isloading)
            {
                isloading = true;
                if (firstRender)
                {
                    try
                    {
                        // To do: Ask local web notification permission
                        AppState.Loader.ShowLoader(true);
                        await SetLocalOffsetAsync().ConfigureAwait(true);
                        await LoadMasterPageDataAsync(0).ConfigureAwait(true);
                        _isDataFetched = true;
                        StateHasChanged();
                        _ = RegisterSignalRAsync();
                        AppState.Loader.ShowLoader(false);
                    }
                    catch (Exception ex)
                    {
                        AppState.Loader.ShowLoader(false);
                    }
                }
                isloading = false;
            }
        }

        private async Task SetLocalOffsetAsync()
        {
            try
            {
                AppState.LocalOffset = (await JSRuntime.InvokeAsync<int>("getClientDate", null)) * -1;
                LocalStorage.LocalOffset = (await JSRuntime.InvokeAsync<int>("getClientDate", null)) * -1;
            }
            catch (Exception ex)
            {
                GenericMethods.LogData($"{ex}");
            }
        }

        private string CssClassObject(BaseDTO masterData)
        {
            List<string> cssClass = new List<string>();
            string direction = AppState?.AppDirection;
            if (masterData != null && masterData?.Settings != null)
            {
                foreach (SettingModel setting in masterData.Settings.Where(x => x.GroupName != GroupConstants.RS_COMMON_GROUP).ToList())
                {
                    cssClass.Add(string.Concat(Constants.SYMBOL_DOUBLE_HYPHEN, setting.SettingKey, Constants.SYMBOL_COLAN, setting.SettingValue));
                }
                cssClass.Add(string.Concat(Constants.DIRECTION_STYLE_VARIABLE_STRING, direction));
                return string.Join("  ", string.Join(Constants.SYMBOL_SEMI_COLAN, cssClass), Constants.SYMBOL_SEMI_COLAN);
            }
            return string.Empty;
        }

        private async Task RegisterSignalRAsync()
        {
            //todo:
            //if (LocalStorage.IsUserAuthenicated && AppState.SignalRConnection != null)
            //{
            //    await new AuthService().RegisterSignalRConnectionAsync(AppState.SignalRConnection.ConnectionId, AppState.MasterData?.Users?[0]?.UserID ?? 0).ConfigureAwait(false);
            //}
        }

        /// <summary>
        /// Default country code based on location
        /// </summary>
        /// <param name="e">country code</param>
        /// <returns></returns>
        [JSInvokable]
        public void SetDefaultCountryCode(string e)
        {
            AppState.CountryCode = e;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            //todo:
            //_ = AppState.SignalRConnection?.DisposeAsync();
        }
    }
}