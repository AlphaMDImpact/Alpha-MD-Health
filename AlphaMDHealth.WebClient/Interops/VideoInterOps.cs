using System.Threading.Tasks;
using Microsoft.JSInterop;
using AlphaMDHealth.Model;

namespace AlphaMDHealth.WebClient
{
    public static class VideoInterOps
    {
        public static ValueTask<VideoDeviceModel[]> GetVideoDevicesAsync(this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeAsync<VideoDeviceModel[]>("videoInterop.getVideoDevices") ?? new ValueTask<VideoDeviceModel[]>();

        public static ValueTask StartVideoAsync(this IJSRuntime jSRuntime, string deviceId, string selector) =>
            jSRuntime?.InvokeVoidAsync("videoInterop.startVideo", deviceId, selector) ?? new ValueTask();

        public static ValueTask<bool> JoinTwilioRoomAsync(this IJSRuntime jsRuntime, string roomName, string token) =>
            jsRuntime?.InvokeAsync<bool>("videoInterop.createOrJoinRoom", roomName, token) ?? new ValueTask<bool>(false);

        public static ValueTask JoinOrLeaveVidyoIOAsync(this IJSRuntime jsRuntime, string roomName, string token, string userID, string link) =>
        jsRuntime?.InvokeVoidAsync("videoIOJoinInterop.connectToConference", token, userID, roomName, link) ?? new ValueTask();


        public static ValueTask<bool> JoinOpentokRoomAsync(this IJSRuntime jsRuntime, string apiKey, string roomName, string token) =>
          jsRuntime?.InvokeAsync<bool>("videoInterop.initializeSession", apiKey, roomName, token) ?? new ValueTask<bool>();

        public static ValueTask<bool> CreateOrJoinFMkRoomAsync(this IJSRuntime jsRuntime, string applicationID, string userID, string roomName, string registerToken, string token, string link)=>
            jsRuntime?.InvokeAsync<bool>("videoInterops.RegisterFrozenMountain", applicationID, userID, roomName, registerToken, token, link) ?? new ValueTask<bool>();

        public static ValueTask PageLoadForVidyoIOAsync(this IJSRuntime jsRuntime,string roomName,string token,string userId, string link) =>
         jsRuntime?.InvokeVoidAsync( "videoIOInterop.OnPageLoad", roomName, token, userId, link) ?? new ValueTask();

        public static ValueTask LeaveRoomAsync(this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeVoidAsync("videoInterop.leaveRoom") ?? new ValueTask();

        public static ValueTask MuteTwilioAudioAsync(
            this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeVoidAsync(
                "videoInterop.muteAudio") ?? new ValueTask();

        public static ValueTask UnMuteTwilioAudioAsync(
            this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeVoidAsync(
                "videoInterop.unMuteAudio") ?? new ValueTask();
        public static ValueTask MuteTwilioVideoAsync(
           this IJSRuntime jsRuntime) =>
           jsRuntime?.InvokeVoidAsync(
               "videoInterop.muteVideo") ?? new ValueTask();

        public static ValueTask UnMuteTwilioVideoAsync(
            this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeVoidAsync(
                "videoInterop.unMuteVideo") ?? new ValueTask();


        public static ValueTask<bool> LeaveRoomLiveSwitchAsync(this IJSRuntime jsRuntime)
        {
            return jsRuntime?.InvokeAsync<bool>("videoInterops.LeaveAsync") ?? new ValueTask<bool>();
        }

        public static ValueTask MuteLiveSwitchAudioAsync(
            this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeVoidAsync(
                "videoInterops.MuteAudioAsync") ?? new ValueTask();

        public static ValueTask UnMuteLiveSwitchAudioAsync(
            this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeVoidAsync(
                "videoInterops.UnMuteAudioAsync") ?? new ValueTask();
        public static ValueTask MuteLiveSwitchVideoAsync(
           this IJSRuntime jsRuntime) =>
           jsRuntime?.InvokeVoidAsync(
               "videoInterops.MuteVideoAsync") ?? new ValueTask();

        public static ValueTask UnMuteLiveSwitchVideoAsync(
            this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeVoidAsync(
                "videoInterops.UnMuteVideoAsync") ?? new ValueTask();

        public static ValueTask<bool> LeaveRoomOpenTokAsync(this IJSRuntime jsRuntime)
        {
            return jsRuntime?.InvokeAsync<bool>("videoInterop.leaveOTAsync") ?? new ValueTask<bool>();
        }

        public static ValueTask MuteOpenTokAudioAsync(
           this IJSRuntime jsRuntime) =>
           jsRuntime?.InvokeVoidAsync(
               "videoInterop.muteAudioOTAsync") ?? new ValueTask();

        public static ValueTask UnMuteOpenTokAudioAsync(
            this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeVoidAsync(
                "videoInterop.unmuteAudioOTAsync") ?? new ValueTask();
        public static ValueTask MuteOpenTokVideoAsync(
           this IJSRuntime jsRuntime) =>
           jsRuntime?.InvokeVoidAsync(
               "videoInterop.muteVideoOTAsync") ?? new ValueTask();

        public static ValueTask UnMuteOpenTokVideoAsync(
            this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeVoidAsync(
                "videoInterop.unmuteVideoOTAsync") ?? new ValueTask();




        public static ValueTask MuteVidyoAudioAsync(
           this IJSRuntime jsRuntime) =>
           jsRuntime?.InvokeVoidAsync(
               "videoIOJoinInterop.muteVidyoAudioAsync") ?? new ValueTask();

        public static ValueTask UnMuteVidyoAudioAsync(
            this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeVoidAsync(
                "videoIOJoinInterop.unmuteVidyoAudioAsync") ?? new ValueTask();
        public static ValueTask MuteVidyoVideoAsync(
           this IJSRuntime jsRuntime) =>
           jsRuntime?.InvokeVoidAsync(
               "videoIOJoinInterop.muteVidyoVideoAsnyc") ?? new ValueTask();

        public static ValueTask UnMuteVidyoVideoAsync(
            this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeVoidAsync(
                "videoIOJoinInterop.unmuteVidyoVideoAsync") ?? new ValueTask();

        public static ValueTask LeaveVidyoRoomAsync(
            this IJSRuntime jsRuntime) =>
            jsRuntime?.InvokeVoidAsync(
                "videoIOJoinInterop.LeaveVidyoAsync") ?? new ValueTask();
    }
}
