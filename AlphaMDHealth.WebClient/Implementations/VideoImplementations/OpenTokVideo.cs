using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace AlphaMDHealth.WebClient
{
    public class OpenTokVideo : IVideoInterface
    {
        protected IJSRuntime JavaScript { get; set; }

        public OpenTokVideo(IJSRuntime javascriptRuntime)
        {
            JavaScript = javascriptRuntime;
        }

        public async Task<bool> JoinAsync(string roomName, string address, string userId, string token, string applicationId, string registerToken, string link)
        {
            return await JavaScript.JoinOpentokRoomAsync(applicationId, roomName, token);
        }

        public async Task<bool> LeaveAsync(string roomName, string token, string userId)
        {
            var asd = await JavaScript.LeaveRoomOpenTokAsync();
            return asd;
        }

        public async ValueTask MuteAudioAsync()
        {
            await JavaScript.MuteOpenTokAudioAsync();
        }

        public async ValueTask MuteVideoAsync()
        {
            await JavaScript.MuteOpenTokVideoAsync();
        }

        public async ValueTask UnMuteAudioAsync()
        {
            await JavaScript.UnMuteOpenTokAudioAsync();
        }

        public async ValueTask UnMuteVideoAsync()
        {
            await JavaScript.UnMuteOpenTokVideoAsync();
        }
    }
}
