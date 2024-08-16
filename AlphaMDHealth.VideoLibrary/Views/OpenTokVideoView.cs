using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.Service;

namespace VideoLibrary
{
    public class OpenTokVideoView : VideoView
    {
        private readonly Dictionary<string, View> _openTokParticipants = new Dictionary<string, View>();
        private bool _frontCamera = true;

        public OpenTokVideoView()
        {
            MultipleParticipantView participantsView = new MultipleParticipantView();
            CrossOpenTok.Current.StreamIdCollectionChanged += Current_StreamIdCollectionChanged;
            OpenTokPublisherView OpentokPublisher = new OpenTokPublisherView { HeightRequest = 100 };
            participantsView.AddLocalParticipant(OpentokPublisher);           
            Content = participantsView;
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
            CrossOpenTok.Current.ApiKey = apiKey;
            CrossOpenTok.Current.SessionId = sessionID;
            CrossOpenTok.Current.UserToken = token;
            CrossOpenTok.Current.Error += (error) =>
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {error}");
                OnVideoCallStatusChanged(VideoState.StateConnectionFailure);
            };
            OnVideoCallStatusChanged(CrossOpenTok.Current.TryStartSession() ? VideoState.StateConnected : VideoState.StateConnectionFailure);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Turn on/off microphone
        /// </summary>
        /// <param name="turnOn">true if microphone is to be turned on</param>
        /// <returns>Returns task for turning on/off microphone</returns>
        public override Task TurnOnOffMicrophoneAsync(bool turnOn)
        {
            CrossOpenTok.Current.IsAudioPublishingEnabled = turnOn;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Turn on/off video
        /// </summary>
        /// <param name="turnOn">true if video is to be turned on</param>
        /// <returns>Returns task for turning on/off video</returns>
        public override Task TurnOnOffVideoAsync(bool turnOn)
        {
            CrossOpenTok.Current.IsVideoPublishingEnabled = turnOn;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disconnects the video call
        /// </summary>
        /// <returns>Returns task for disconnecting the call</returns>
        public override Task DisconnectCallAsync()
        {
            CrossOpenTok.Current.EndSession();
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
            if (enableFrontCamera != _frontCamera)
            {
                CrossOpenTok.Current.CycleCamera();
                _frontCamera = enableFrontCamera;
            }
            return Task.CompletedTask;
        }

        private void Current_StreamIdCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            List<string> streamIds = new List<string>();
            foreach (string streamId in CrossOpenTok.Current.StreamIdCollection)
            {
                streamIds.Add(streamId);
                if (!_openTokParticipants.ContainsKey(streamId))
                {
                    OpenTokSubscriberView participantVideo = new OpenTokSubscriberView
                    {
                        StreamId = streamId
                    };
                    _openTokParticipants.Add(participantVideo.StreamId, participantVideo);
                    (Content as MultipleParticipantView).AddParticipant(participantVideo);
                }
            }
            var participantsToRemove = _openTokParticipants.Where(x => !streamIds.Contains(x.Key));
            foreach (var item in participantsToRemove)
            {
                (Content as MultipleParticipantView).RemoveParticipant(item.Value);
            }
        }
    }
}
