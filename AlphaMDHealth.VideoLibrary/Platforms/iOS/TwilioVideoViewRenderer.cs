using AVFoundation;
using Foundation;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
using System.ComponentModel;
using Twilio;
using UIKit;
using static The49.Twilio.Video.Maui.CameraCapturer;

//[assembly: ExportRenderer(typeof(TwilioVideoView), typeof(TwilioVideoViewRenderer))]
namespace AlphaMDHealth.VideoLibrary.Platforms.iOS
{
    public class TwilioVideoViewRenderer : ViewRenderer<TwilioVideoView, UIView>
    {
        private TVIVideoView _primaryVideoView;
        private TVILocalVideoTrack _localVideoTrack;
        private TVILocalAudioTrack _localAudioTrack;
        private TVICameraSource _cameraSource;
        private TwilioRoomDelegate _roomDelegate;
        private readonly Dictionary<string, ParticipantTwilioiOSVideoView> _videoViews = new Dictionary<string, ParticipantTwilioiOSVideoView>();

        /// <summary>
        /// Twilio Video Room
        /// </summary>
        public TVIRoom Room { get; set; }

        /// <summary>
        /// Element for Xamarin
        /// </summary>
        public TwilioVideoView FormsControl
        {
            get { return Element; }
        }

        /// <summary>
        /// Twilio Renderer
        /// </summary>
        public TwilioVideoViewRenderer() { }

        /// <summary>
        /// Invoked when element is changed
        /// </summary>
        /// <param name="e">Element changed event args</param>
        protected async override void OnElementChanged(ElementChangedEventArgs<TwilioVideoView> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                _roomDelegate = new TwilioRoomDelegate(this);
                await AddVideoViewAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Invoked when element property is changed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Property changed event args</param>
        protected async override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            var view = sender as TwilioVideoView;
            if (e.PropertyName == TwilioVideoView.IsCallJoinButtonClickedProperty.PropertyName)
            {
                if (view.IsCallJoinButtonClicked)
                {
                    ConnectToRoom(view.Room, view.Token);
                }
                else
                {
                    await DisconnectFromRoomAsync().ConfigureAwait(true);
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
        }

        /// <summary>
        /// Setup video video for remote participant
        /// </summary>
        /// <param name="videoTrack">Remote participant video track</param>
        /// <param name="identity">Remote participant identity</param>
        /// <returns>Task representing setup of remote video view</returns>
        internal async Task SetupRemoteVideoViewAsync(TVIRemoteVideoTrack videoTrack, string identity)
        {
            ParticipantTwilioVideoView primaryVideoFormsView = new ParticipantTwilioVideoView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            if (Element == null)
            {
                return;
            }
            Element.InvokeRemoteParticipantJoined(primaryVideoFormsView);
            await Task.Delay(100).ConfigureAwait(true);
            Device.BeginInvokeOnMainThread(() =>
            {
                ParticipantTwilioiOSVideoView primaryVideoView = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.GetRenderer(primaryVideoFormsView) as ParticipantTwilioiOSVideoView;
                primaryVideoView.Element.ID = videoTrack.Name;
                primaryVideoView.Element.ParticipantID = identity;
                videoTrack.AddRenderer(primaryVideoView.ControlVideoView);
                _videoViews.Add(videoTrack.Name, primaryVideoView);
            });
        }

        /// <summary>
        /// Remove remote participant
        /// </summary>
        /// <param name="remoteParticipant">Remote participant</param>
        internal void RemoveParticipant(TVIRemoteParticipant remoteParticipant)
        {
            // Remove participant renderer
            if (remoteParticipant.RemoteVideoTracks.Length > 0)
            {
                TVIRemoteVideoTrackPublication remoteVideoTrackPublication = remoteParticipant.RemoteVideoTracks[0];
                // Remove video only if subscribed to participant track.
                if (remoteVideoTrackPublication.TrackSubscribed)
                {
                    RemoveParticipantVideo(remoteVideoTrackPublication.VideoTrack);
                }
            }
        }

        /// <summary>
        /// Remove remote participant video
        /// </summary>
        /// <param name="videoTrack">Remote participant video track</param>
        internal void RemoveParticipantVideo(TVIVideoTrack videoTrack)
        {
            if (_videoViews.TryGetValue(videoTrack.Name, out ParticipantTwilioiOSVideoView remoteVideoView))
            {
                videoTrack.RemoveRenderer(remoteVideoView.ControlVideoView);
                Element.InvokeRemoteParticipantLeft(remoteVideoView.Element);
                _videoViews.Remove(videoTrack.Name);
            }
        }

        private async Task AddVideoViewAsync()
        {
            ParticipantTwilioVideoView primaryVideoFormsView = new ParticipantTwilioVideoView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            if (Element == null)
            {
                return;
            }
            Element.InvokeConnectionEstablished(primaryVideoFormsView);
            await Task.Delay(100).ConfigureAwait(true);
            ParticipantTwilioiOSVideoView localVideoView = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.GetRenderer(primaryVideoFormsView) as ParticipantTwilioiOSVideoView;
            localVideoView.ControlVideoView.Mirror = true;
            _primaryVideoView = localVideoView.ControlVideoView;
            Device.BeginInvokeOnMainThread(() =>
            {
                _cameraSource = new TVICameraSource(new CameraSourceDelegate());
                _localVideoTrack = TVILocalVideoTrack.TrackWithSource(_cameraSource);
                _localAudioTrack = TVILocalAudioTrack.TrackWithOptions(null, true, "Microphone");
                _localAudioTrack.Enabled = true;
                localVideoView.Element.ID = _localVideoTrack.Name;
                _localVideoTrack.AddRenderer(localVideoView.ControlVideoView);
                AVCaptureDevice frontCamera = TVICameraSource.CaptureDeviceForPosition(AVCaptureDevicePosition.Front);
                _cameraSource.StartCaptureWithDevice(frontCamera);
            });
        }

        private void ConnectToRoom(string roomName, string accessToken)
        {
            var connectOptions = TVIConnectOptions.OptionsWithToken(accessToken, (builder) =>
            {
                builder.RoomName = roomName;
                builder.AudioTracks = new TVILocalAudioTrack[] { _localAudioTrack };
                builder.VideoTracks = new TVILocalVideoTrack[] { _localVideoTrack };
            });
            Room = TwilioVideoSDK.ConnectWithOptions(connectOptions, _roomDelegate);
        }

        private void SpeakerOn(bool speakerOnOff)
        {
            NSError error = AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionCategoryOptions.DefaultToSpeaker);
            if (error == null && speakerOnOff && AVAudioSession.SharedInstance().SetMode(AVAudioSession.ModeVideoChat, out error) && AVAudioSession.SharedInstance().OverrideOutputAudioPort(AVAudioSessionPortOverride.Speaker, out error))
            {
                AVAudioSession.SharedInstance().SetActive(true);
            }
        }

        private void SwitchFrontRearCamera(bool isFrontCameraOn)
        {
            if ((_cameraSource.Device.Position == AVCaptureDevicePosition.Front && !isFrontCameraOn)
                || (_cameraSource.Device.Position != AVCaptureDevicePosition.Front && isFrontCameraOn))
            {
                AVCaptureDevice camera = TVICameraSource.CaptureDeviceForPosition(isFrontCameraOn ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back);
                _cameraSource.SelectCaptureDevice(camera);
                _primaryVideoView.Mirror = isFrontCameraOn;
            }
        }

        private void MuteUnmuteAudio(bool ismute)
        {
            if (_localAudioTrack != null)
            {
                _localAudioTrack.Enabled = ismute;
            }
        }

        private void EnableDisableVideo(TwilioVideoView videoView)
        {
            _localVideoTrack.Enabled = videoView.IsVideoEnabled;
        }

        private async Task DisconnectFromRoomAsync()
        {
            if (Room != null)
            {
                Room.Disconnect();
                _cameraSource?.StopCapture();
                _localVideoTrack?.RemoveRenderer(_primaryVideoView);
                _localVideoTrack = null;
                _localAudioTrack = null;
                _cameraSource = null;
                _roomDelegate = null;
                await Task.Delay(100).ConfigureAwait(true);
                Room = null;
            }
        }
    }
}