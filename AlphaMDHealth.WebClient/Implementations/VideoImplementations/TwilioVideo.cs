using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace AlphaMDHealth.WebClient
{
    public class TwilioVideo : IVideoInterface
    {
        protected IJSRuntime JavaScript { get; set; }

        public TwilioVideo(IJSRuntime javascriptRuntime)
        {
            JavaScript = javascriptRuntime;
        }

        public async Task<bool> JoinAsync(string roomName, string address, string userId, string token, string applicationId, string registerToken, string link)
        {
           
            var Devices = await JavaScript.GetVideoDevicesAsync().ConfigureAwait(true);
            if (Devices?.Length > 0)
            {
                var defaultDeviceId = Devices[0].DeviceId;
                if (!string.IsNullOrWhiteSpace(defaultDeviceId))
                {
                    await JavaScript.StartVideoAsync(defaultDeviceId, "#camera");
                }
                await JavaScript.StartVideoAsync(roomName, "#camera").ConfigureAwait(true);
                var joined = await JavaScript.JoinTwilioRoomAsync(roomName, token);
                return joined;
            }
            else
            {
                return false;
            }

        }

        public async Task<bool> LeaveAsync(string roomName, string token, string userId)
        {
            await JavaScript.LeaveRoomAsync();
            if (!string.IsNullOrWhiteSpace(roomName))
            {
                await JavaScript.StartVideoAsync(roomName, "#camera");
            }
            return true;
        }

        public async ValueTask MuteAudioAsync()
        {
            await JavaScript.MuteTwilioAudioAsync();
        }

        public async ValueTask MuteVideoAsync()
        {
            await JavaScript.MuteTwilioVideoAsync();
        }

        public async ValueTask UnMuteAudioAsync()
        {
            await JavaScript.UnMuteTwilioAudioAsync();
        }

        public async ValueTask UnMuteVideoAsync()
        {
            await JavaScript.UnMuteTwilioVideoAsync();
        }
    }
}
