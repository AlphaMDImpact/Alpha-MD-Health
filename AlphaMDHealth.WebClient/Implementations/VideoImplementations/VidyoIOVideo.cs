using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace AlphaMDHealth.WebClient
{
    public class VidyoIOVideo : IVideoInterface
    {
        protected IJSRuntime JavaScript { get; set; }

        public VidyoIOVideo(IJSRuntime javascriptRuntime)
        {
            JavaScript = javascriptRuntime;
        }

        public async Task<bool> JoinAsync(string roomName, string address, string userId, string token, string applicationId, string registerToken, string link)
        {
            await JavaScript.PageLoadForVidyoIOAsync(address, token, userId, link).ConfigureAwait(true);
            await Task.Delay(5000);
            await JavaScript.JoinOrLeaveVidyoIOAsync(roomName, token, userId, link).ConfigureAwait(true);
            return true;
        }

        public async Task<bool> LeaveAsync(string roomName, string token, string userId)
        {
            await JavaScript.LeaveVidyoRoomAsync();
            return true;
        }

        public async ValueTask MuteAudioAsync()
        {
            await JavaScript.MuteVidyoAudioAsync();
        }

        public async ValueTask MuteVideoAsync()
        {
            await JavaScript.MuteVidyoVideoAsync();
        }

        public async ValueTask UnMuteAudioAsync()
        {
            await JavaScript.UnMuteVidyoAudioAsync();
        }

        public async ValueTask UnMuteVideoAsync()
        {
            await JavaScript.UnMuteVidyoVideoAsync();
        }
    }
}
