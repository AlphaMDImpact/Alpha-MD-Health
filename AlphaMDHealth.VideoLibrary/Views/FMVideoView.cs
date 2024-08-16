using System;
using System.Linq;
using System.Threading.Tasks;
//using Xamarin.Forms;

namespace AlphaMDHealth.VideoLibrary
{
    public class FMVideoView : VideoView
    {
        private readonly IFMChatApp _iFMChatApp = DependencyService.Get<IFMChatApp>();
        private readonly AbsoluteLayout _videoContainer = new AbsoluteLayout();

        public FMVideoView()
        {
            _iFMChatApp.Mode = IntegrationLibrary.Models.Modes.Sfu;
            Content = _videoContainer;
        }

        /// <summary>
        /// Video call status event callback method 
        /// </summary>
        /// <param name="videoState">Video state</param>
        public void InvokeStatusChanged(VideoState videoState)
        {
            OnVideoCallStatusChanged(videoState);
        }

        /// <summary>
        /// Starts the video call
        /// </summary>
        /// <param name="apiKey">api key</param>
        /// <param name="token">token</param>
        /// <param name="sessionID">session ID or room ID</param>
        /// <param name="videoLink">video host URL</param>
        /// <param name="userID">Id of the user</param>
        /// <returns>Returns task for connecting the call</returns>
        public override Task ConnectCallAsync(string apiKey, string token, string sessionID, string videoLink, string userID)
        {
            _iFMChatApp.ChannelId = sessionID;
            _iFMChatApp.StartLocalMedia(_videoContainer).Then<object>((o) =>
            {
                return _iFMChatApp.JoinAsync(apiKey, token, videoLink, userID).Then(m => 
                {
                    OnVideoCallStatusChanged(VideoState.StateConnected);
                }).Fail(ex => 
                {
                    System.Diagnostics.Debug.WriteLine($"----------||ConnectCallAsync()1 Message : {ex.Message}, Exception: {ex}||{DateTimeOffset.Now:yyyy-MM-ddTHH:mm:ssZ}");
                    OnVideoCallStatusChanged(VideoState.StateConnectionFailure);
                });
            }, ex =>
            {
                System.Diagnostics.Debug.WriteLine($"----------||ConnectCallAsync()2 Message : {ex.Message}, Exception: {ex}||{DateTimeOffset.Now:yyyy-MM-ddTHH:mm:ssZ}");
                OnVideoCallStatusChanged(VideoState.StateConnectionFailure);
            }).Fail(ex =>
            {
                System.Diagnostics.Debug.WriteLine($"----------||ConnectCallAsync()3 Message : {ex.Message}, Exception: {ex}||{DateTimeOffset.Now:yyyy-MM-ddTHH:mm:ssZ}");
                OnVideoCallStatusChanged(VideoState.StateConnectionFailure);
            });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disconnects the video call
        /// </summary>
        /// <returns>Returns task for disconnecting the call</returns>
        public override Task DisconnectCallAsync()
        {
            _iFMChatApp.StopLocalMedia();
            _iFMChatApp.LeaveAsync();
            OnVideoCallStatusChanged(VideoState.StateDisconnected);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Switch camera between front and rear
        /// </summary>
        /// <param name="enableFrontCamera">true if front camera is to be enabled else false for rear camera</param>
        /// <returns>Returns task for switching the camera used</returns>
        public override Task SwitchCameraAsync(bool enableFrontCamera)
        {
            var videoInputSources = _iFMChatApp.PMedia.GetVideoSourceInputs();
            _iFMChatApp.PMedia.VideoTrack.ChangeSourceInput(enableFrontCamera ? videoInputSources.Result.FirstOrDefault(x => x.Name == videoInputSources.Result[1].Name) : videoInputSources.Result.FirstOrDefault(x => x.Name == videoInputSources.Result[0].Name));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Turn on/off microphone
        /// </summary>
        /// <param name="turnOn">true if microphone is to be turned on</param>
        /// <returns>Returns task for turning on/off microphone</returns>
        public override Task TurnOnOffMicrophoneAsync(bool turnOn)
        {
            _iFMChatApp.PMedia.AudioMuted = !turnOn;
            _iFMChatApp.PMedia.AudioTrack.Muted= !turnOn;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Turn on/off video
        /// </summary>
        /// <param name="turnOn">true if video is to be turned on</param>
        /// <returns>Returns task for turning on/off video</returns>
        public override Task TurnOnOffVideoAsync(bool turnOn)
        {
            _iFMChatApp.PMedia.VideoMuted = !turnOn;
            return Task.CompletedTask;
        }
    }
}
