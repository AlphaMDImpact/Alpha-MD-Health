using AlphaMDHealth.Utility;
using Android.Content;
using Android.Media;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using System.ComponentModel;
using Twilio.Video;
using Application = Android.App.Application;
using View = Android.Views.View;

namespace AlphaMDHealth.VideoLibrary.Platforms.Android
{
    public class TwilioVideoViewRenderer : ViewRenderer<TwilioVideoView, View>, Room.IListener, RemoteParticipant.IListener
    {
        private Twilio.Video.VideoView _primaryVideoView;
        private LocalVideoTrack _localVideoTrack;
        private LocalAudioTrack _localAudioTrack;
        private Room _room;
        private Camera2Capturer _cameraCapturer;
        private LocalParticipant _localParticipant;
        //private IVideoRenderer _localVideoView;
        private readonly Dictionary<string, ParticipantTwilioAndroidVideoView> _videoViews = new Dictionary<string, ParticipantTwilioAndroidVideoView>();

        private TwilioVideoView FormsControl
        {
            get { return Element; }
        }

        /// <summary>
        /// Twilio video view renderer
        /// </summary>
        /// <param name="context">Android context</param>
        public TwilioVideoViewRenderer(Context context) : base(context)
        {
        }

        /// <summary>
        /// Handle app sleep
        /// </summary>
        public void OnAppSleep()
        {
            if (_localVideoTrack != null)
            {
                if (_localParticipant == null)
                {
                    return;
                }
                _localParticipant.PublishTrack(_localVideoTrack);
            }
        }

        /// <summary>
        /// Handle app resume
        /// </summary>
        public void OnAppResume()
        {
            if (_localVideoTrack != null && _cameraCapturer != null)
            {
                _localVideoTrack = LocalVideoTrack.Create(Application.Context, true, _cameraCapturer);
                _localVideoTrack.AddSink(_primaryVideoView);
                // If connected to a Room then share the local video track.
                if (_localParticipant != null)
                {
                    _localParticipant.PublishTrack(_localVideoTrack);
                }
            }
        }

        /// <summary>
        /// Called on connection failure
        /// </summary>
        /// <param name="p0">Instance of room</param>
        /// <param name="p1">Exception data</param>
        public void OnConnectFailure(Room p0, TwilioException p1)
        {
            FormsControl.VideoCallState = VideoState.StateConnectionFailure;
            Element.InvokeStatusChanged(FormsControl);
        }

        /// <summary>
        /// Called when video call is connected
        /// </summary>
        /// <param name="p0">Instance of room</param>
        public async void OnConnected(Room p0)
        {
            FormsControl.VideoCallState = VideoState.StateConnected;
            Element.InvokeStatusChanged(FormsControl);
            _localParticipant = p0.LocalParticipant;
            if (p0.RemoteParticipants?.Count > 0)
            {
                foreach (var remoteParticipant in p0.RemoteParticipants)
                {
                    await AddRemoteParticipantAsync(remoteParticipant).ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// Called when video call is disconnected
        /// </summary>
        /// <param name="p0">Instance of room</param>
        /// <param name="p1">Exception if any</param>
        public void OnDisconnected(Room p0, TwilioException p1)
        {
            MuteUnmuteAudio(false);
            if (FormsControl != null)
            {
                FormsControl.VideoCallState = VideoState.StateDisconnected;
                Element.InvokeStatusChanged(FormsControl);
            }
            _localParticipant = null;
            _room = null;
        }

        /// <summary>
        /// Called when dominant speaker is changed
        /// </summary>
        /// <param name="room">Instance of room</param>
        /// <param name="remoteParticipant">Remote participant</param>
        public void OnDominantSpeakerChanged(Room room, RemoteParticipant remoteParticipant)
        {
            //
        }

        /// <summary>
        /// Invoked when participant is connected
        /// </summary>
        /// <param name="p0">Instance of room</param>
        /// <param name="p1">Remote participants</param>
        public async void OnParticipantConnected(Room p0, RemoteParticipant p1)
        {
            await AddRemoteParticipantAsync(p1).ConfigureAwait(false);
        }

        /// <summary>
        /// Invoked when participant is disconnected
        /// </summary>
        /// <param name="p0">Instance of room</param>
        /// <param name="p1">Remote participants</param>
        public void OnParticipantDisconnected(Room p0, RemoteParticipant p1)
        {
            RemoveParticipant(p1);
        }

        /// <summary>
        /// Invoked when reconnected
        /// </summary>
        /// <param name="p0">Instance of Room</param>
        public void OnReconnected(Room p0)
        {
            FormsControl.VideoCallState = VideoState.StateConnected;
            Element.InvokeStatusChanged(FormsControl);
        }

        /// <summary>
        /// Invoked on reconnecting
        /// </summary>
        /// <param name="p0">Instance of room</param>
        /// <param name="p1">Exception data if any</param>
        public void OnReconnecting(Room p0, TwilioException p1)
        {
            //
        }

        /// <summary>
        /// Invoked when recording is started
        /// </summary>
        /// <param name="p0">Instance of room</param>
        public void OnRecordingStarted(Room p0)
        {
            //
        }

        /// <summary>
        /// Invoked when recording is stopped
        /// </summary>
        /// <param name="p0">Instance of room</param>
        public void OnRecordingStopped(Room p0)
        {
            //
        }

        /// <summary>
        /// Invoked when audio is disabled
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote audio</param>
        public void OnAudioTrackDisabled(RemoteParticipant p0, RemoteAudioTrackPublication p1)
        {
            //
        }

        /// <summary>
        /// Invoked when audio is enabled
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote audio</param>
        public void OnAudioTrackEnabled(RemoteParticipant p0, RemoteAudioTrackPublication p1)
        {
            //
        }

        /// <summary>
        /// Invoked when audio is published
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote audio</param>
        public void OnAudioTrackPublished(RemoteParticipant p0, RemoteAudioTrackPublication p1)
        {
            //
        }

        /// <summary>
        /// Invoked when audio track is subscribed
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote audio</param>
        /// <param name="p2">Remote audio track</param>
        public void OnAudioTrackSubscribed(RemoteParticipant p0, RemoteAudioTrackPublication p1, RemoteAudioTrack p2)
        {
            //
        }

        /// <summary>
        /// Invoked when audio subscription failed
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote audio</param>
        /// <param name="p2">Exception data</param>
        public void OnAudioTrackSubscriptionFailed(RemoteParticipant p0, RemoteAudioTrackPublication p1, TwilioException p2)
        {
            //
        }

        /// <summary>
        /// Invoked when audio track unpublished
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote audio</param>
        public void OnAudioTrackUnpublished(RemoteParticipant p0, RemoteAudioTrackPublication p1)
        {
            //
        }

        /// <summary>
        /// Invoked when audio track is unpublished
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote audio</param>
        /// <param name="p2">Remote audio track</param>
        public void OnAudioTrackUnsubscribed(RemoteParticipant p0, RemoteAudioTrackPublication p1, RemoteAudioTrack p2)
        { //
        }

        /// <summary>
        /// Invoked when data track is published
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote audio</param>
        public void OnDataTrackPublished(RemoteParticipant p0, RemoteDataTrackPublication p1)
        { //
        }

        /// <summary>
        /// Invoked when data track is published
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote data</param>
        /// <param name="p2">Remote data track</param>
        public void OnDataTrackSubscribed(RemoteParticipant p0, RemoteDataTrackPublication p1, RemoteDataTrack p2)
        { //
        }

        /// <summary>
        /// Invoked when data track subscription is failed
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote data</param>
        /// <param name="p2">Exception data</param>
        public void OnDataTrackSubscriptionFailed(RemoteParticipant p0, RemoteDataTrackPublication p1, TwilioException p2)
        { //
        }

        /// <summary>
        /// Invoked when data track is unpublished
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote data</param>
        public void OnDataTrackUnpublished(RemoteParticipant p0, RemoteDataTrackPublication p1)
        { //
        }

        /// <summary>
        /// Invoked when data track is unsubscribed
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote data</param>
        /// <param name="p2">Remote data track</param>
        public void OnDataTrackUnsubscribed(RemoteParticipant p0, RemoteDataTrackPublication p1, RemoteDataTrack p2)
        { //
        }

        /// <summary>
        /// Invoked when video track is disabled
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote video</param>
        public void OnVideoTrackDisabled(RemoteParticipant p0, RemoteVideoTrackPublication p1)
        { //
        }

        /// <summary>
        /// Invoked when video track is enabled
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote video</param>
        public void OnVideoTrackEnabled(RemoteParticipant p0, RemoteVideoTrackPublication p1)
        { //
        }

        /// <summary>
        /// Invoked when video track is published
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote video</param>
        public void OnVideoTrackPublished(RemoteParticipant p0, RemoteVideoTrackPublication p1)
        { //
        }

        /// <summary>
        /// Invoked when video track is subscribed
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote video</param>
        /// <param name="p2">Remote video track</param>
        public async void OnVideoTrackSubscribed(RemoteParticipant p0, RemoteVideoTrackPublication p1, RemoteVideoTrack p2)
        {
            await AddRemoteParticipantVideoAsync(p2, p0.Identity).ConfigureAwait(false);
        }

        /// <summary>
        /// Invoked when video track subscription is failed
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote video</param>
        /// <param name="p2">Exception data</param>
        public void OnVideoTrackSubscriptionFailed(RemoteParticipant p0, RemoteVideoTrackPublication p1, TwilioException p2)
        {
            //
        }

        /// <summary>
        /// Invoked when video track is unpublished
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote video</param>
        public void OnVideoTrackUnpublished(RemoteParticipant p0, RemoteVideoTrackPublication p1)
        {
            //
        }

        /// <summary>
        /// Invoked when video track is unsubscribed
        /// </summary>
        /// <param name="p0">Remote participant</param>
        /// <param name="p1">Remote video</param>
        /// <param name="p2">Remote video track</param>
        public void OnVideoTrackUnsubscribed(RemoteParticipant p0, RemoteVideoTrackPublication p1, RemoteVideoTrack p2)
        {
            RemoveParticipantVideo(p2);
        }

        /// <summary>
        /// Invoked when network quality level is changed
        /// </summary>
        /// <param name="remoteParticipant">Remote participant</param>
        /// <param name="networkQualityLevel">Network qyality level</param>
        public void OnNetworkQualityLevelChanged(RemoteParticipant remoteParticipant, NetworkQualityLevel networkQualityLevel)
        {
            //
        }

        /// <summary>
        /// Invoked when element is changed
        /// </summary>
        /// <param name="e">Element changed event args</param>
        protected async override void OnElementChanged(ElementChangedEventArgs<TwilioVideoView> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                await AddVideoViewAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Invoked when element property is changed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Property changed event args</param>
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (sender != null)
            {
                var view = sender as TwilioVideoView;
                if (e.PropertyName == TwilioVideoView.IsCallJoinButtonClickedProperty.PropertyName)
                {
                    if (view.IsCallJoinButtonClicked)
                    {
                        ConnectToRoom(view.Room, view.Token);
                    }
                    else
                    {
                        DisconnectToRoom();
                    }
                }
                if (e.PropertyName == TwilioVideoView.IsUnmuteButtonClickedProperty.PropertyName)
                {
                    MuteUnmuteAudio(view.IsUnmuteButtonClicked);
                }
                if (e.PropertyName == TwilioVideoView.IsVideoDisabledProperty.PropertyName)
                {
                    EnableDisableVideo(view);
                }
                if (e.PropertyName == TwilioVideoView.IsSpeakerOnProperty.PropertyName)
                {
                    SpeakerOn(view.IsSpeakerOn);
                }
                if (e.PropertyName == TwilioVideoView.IsFrontCameraOnProperty.PropertyName)
                {
                    SwitchFrontRearCamera(view.IsFrontCameraOn);
                }
                if (e.PropertyName == TwilioVideoView.HandleAppSleepProperty.PropertyName)
                {
                    OnAppSleep();
                }
                if (e.PropertyName == TwilioVideoView.HandleAppResumeProperty.PropertyName)
                {
                    OnAppResume();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !Element.IsCallJoinButtonClicked)
            {
                if (_localVideoTrack != null)
                {
                    _localVideoTrack?.RemoveSink(_primaryVideoView);
                    _localVideoTrack?.Release();
                    _localVideoTrack = null;
                }
                _localParticipant = null;
            }
            base.Dispose(disposing);
        }

        private string GetCameraId(CameraPosition position = CameraPosition.Front)
        {
            var enumerator = new Tvi.Webrtc.Camera2Enumerator(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity);
            var names = enumerator.GetDeviceNames();
            var cameraId = names.FirstOrDefault(n =>
            {
                return position switch
                {
                    CameraPosition.Back => enumerator.IsBackFacing(n),
                    CameraPosition.Front or _ => enumerator.IsFrontFacing(n),
                };
            });
            if (cameraId == null)
            {
                return null;
            }
            else
            {
                return cameraId;
            }
        }
        private async Task AddVideoViewAsync()
        {
            try
            {
                ParticipantTwilioVideoView primaryVideoFormsView = new ParticipantTwilioVideoView
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand
                };
                Element.InvokeConnectionEstablished(primaryVideoFormsView);
                await Task.Delay(100).ConfigureAwait(true);
                ParticipantTwilioAndroidVideoView localVideoView = Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform.GetRenderer(primaryVideoFormsView) as ParticipantTwilioAndroidVideoView;
                _primaryVideoView = localVideoView.ControlVideoView;
                //localVideoView.ControlVideoView.SetMirror(true);



                Device.BeginInvokeOnMainThread(() =>
                {
                    _cameraCapturer = new Camera2Capturer(Application.Context, GetCameraId(), null);
                    _localVideoTrack = LocalVideoTrack.Create(Application.Context, true, _cameraCapturer);
                    localVideoView.Element.ID = _localVideoTrack.Name;
                    _localVideoTrack.AddSink(localVideoView.ControlVideoView);
                    //_localVideoView = localVideoView.ControlVideoView;
                    _localAudioTrack = LocalAudioTrack.Create(Application.Context, true);
                    localVideoView.ControlVideoView.SetZOrderMediaOverlay(true);
                    SpeakerOn(true);
                });
            }
            catch (Exception ex)
            {
                // Failed to add video view
                System.Diagnostics.Debug.WriteLine($"AddVideoView failed: {ex.Message} | {ex.StackTrace}");
            }
        }

        private void ConnectToRoom(string roomName, string accessToken)
        {
            ConnectOptions connectOptions = new ConnectOptions.Builder(accessToken)
              .RoomName(roomName)
              .AudioTracks(new List<LocalAudioTrack>() { _localAudioTrack })
              .VideoTracks(new List<LocalVideoTrack>() { _localVideoTrack })
              .Build();

            //_room = Video.Connect(Application.Context, connectOptions, this);
        }

        private void DisconnectToRoom()
        {
            if (_room != null)
            {
                _room.Disconnect();
                //_cameraCapturer.StopCapture();
                _cameraCapturer = null;
                _room = null;
            }
        }

        private void SpeakerOn(bool speakerOnOff)
        {
            AudioManager audioManager = (AudioManager)Application.Context.ApplicationContext.GetSystemService(Context.AudioService);
            if (audioManager != null)
            {
                IntentFilter iFilter = new IntentFilter(Intent.ActionHeadsetPlug);
                Intent iStatus = Application.Context.ApplicationContext.RegisterReceiver(null, iFilter);
                if (iStatus?.GetIntExtra("state", 0) == 1)
                {
                    audioManager.SpeakerphoneOn = false;
                }
                else
                {
                    audioManager.SpeakerphoneOn = speakerOnOff;
                }
            }
        }

        private void MuteUnmuteAudio(bool muteUnmute)
        {
            _localAudioTrack?.Enable(muteUnmute);
        }

        private void EnableDisableVideo(TwilioVideoView videoView)
        {
            _localVideoTrack?.Enable(videoView.IsVideoEnabled);
        }

        private void SwitchFrontRearCamera(bool isFrontCameraOn)
        {
            if (_cameraCapturer != null)
            {
                //CameraSource cameraSource = _cameraCapturer.GetCameraSource();
                //if ((cameraSource == CameraSource.FrontCamera && !isFrontCameraOn)
                //    || (cameraSource != CameraSource.FrontCamera && isFrontCameraOn))
                //{
                //    _cameraCapturer.SwitchCamera();
                //}
            }
        }

        private async Task AddRemoteParticipantAsync(RemoteParticipant remoteParticipant)
        {
            if (remoteParticipant.RemoteVideoTracks?.Count > 0)
            {
                RemoteVideoTrackPublication remoteVideoTrackPublication = remoteParticipant.RemoteVideoTracks[0];
                //Only render video tracks that are subscribed to
                if (remoteVideoTrackPublication.IsTrackSubscribed)
                {
                    await AddRemoteParticipantVideoAsync(remoteVideoTrackPublication.RemoteVideoTrack, remoteParticipant.Identity).ConfigureAwait(true);
                }
            }
            remoteParticipant.SetListener(this);
        }

        private void RemoveParticipant(RemoteParticipant remoteParticipant)
        {
            // Remove participant renderer
            if (remoteParticipant.RemoteVideoTracks.Count > 0)
            {
                RemoteVideoTrackPublication remoteVideoTrackPublication = remoteParticipant.RemoteVideoTracks[0];
                // Remove video only if subscribed to participant track.
                if (remoteVideoTrackPublication.IsTrackSubscribed)
                {
                    RemoveParticipantVideo(remoteVideoTrackPublication.RemoteVideoTrack);
                }
            }
        }

        private void RemoveParticipantVideo(VideoTrack videoTrack)
        {
            if (_videoViews.TryGetValue(videoTrack.Name, out ParticipantTwilioAndroidVideoView remoteVideoView))
            {
                //videoTrack.RemoveRenderer(remoteVideoView.ControlVideoView);
                Element.InvokeRemoteParticipantLeft(remoteVideoView.Element);
                _videoViews.Remove(videoTrack.Name);
            }
        }

        private async Task AddRemoteParticipantVideoAsync(VideoTrack remoteVideoTrack, string identity)
        {
            ParticipantTwilioVideoView primaryVideoFormsView = new ParticipantTwilioVideoView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            Element.InvokeRemoteParticipantJoined(primaryVideoFormsView);
            ParticipantTwilioAndroidVideoView primaryVideoView = Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform.GetRenderer(primaryVideoFormsView) as ParticipantTwilioAndroidVideoView;
            await Task.Delay(100).ConfigureAwait(true);
            primaryVideoView.Element.ID = remoteVideoTrack.Name;
            primaryVideoView.Element.ParticipantID = identity;
            remoteVideoTrack.AddSink(primaryVideoView.ControlVideoView);
            _videoViews.Add(remoteVideoTrack.Name, primaryVideoView);
        }

        public void OnParticipantReconnected(Room room, RemoteParticipant participant)
        {
            throw new NotImplementedException();
        }

        public void OnParticipantReconnecting(Room room, RemoteParticipant participant)
        {
            throw new NotImplementedException();
        }
    }
}