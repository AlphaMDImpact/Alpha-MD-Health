using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace AlphaMDHealth.WebClient
{
    public class LiveSwitchVideo : IVideoInterface
    {
        protected IJSRuntime JavaScript { get; set; }

        public LiveSwitchVideo(IJSRuntime javascriptRuntime)
        {
            JavaScript = javascriptRuntime;
        }

        public async Task<bool> JoinAsync(string roomName, string address, string userId, string token, string applicationId, string registerToken, string link)
        { 
            var status =  await JavaScript.CreateOrJoinFMkRoomAsync(applicationId, userId, roomName, registerToken, token,link).ConfigureAwait(true);
            return status;
        }

        public async Task<bool> LeaveAsync(string roomName, string token, string userId)
        {
            return await JavaScript.LeaveRoomLiveSwitchAsync();
        }

        public async ValueTask MuteAudioAsync()
        {
            await JavaScript.MuteLiveSwitchAudioAsync();
        }

        public async ValueTask MuteVideoAsync()
        {
            await JavaScript.MuteLiveSwitchVideoAsync();
        }

        public async ValueTask UnMuteAudioAsync()
        {
            await JavaScript.UnMuteLiveSwitchAudioAsync();
        }

        public async ValueTask UnMuteVideoAsync()
        {
            await JavaScript.UnMuteLiveSwitchVideoAsync();
        }
    }
}
