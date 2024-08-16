using AlphaMDHealth.Utility;

namespace AlphaMDHealth.VideoLibrary
{
    /// <summary>
    /// Video view for Twilio Video Calling Library
    /// </summary>
    public class TwilioVideoView : VideoView
    {
        /// <summary>
        /// Speaker on off binding  property
        /// </summary>
        public static readonly BindableProperty IsSpeakerOnProperty = BindableProperty.Create(nameof(IsSpeakerOn), typeof(bool), typeof(TwilioVideoView), false);

        /// <summary>
        /// Call connect/disconnect binding property
        /// </summary>
        public static readonly BindableProperty IsCallJoinButtonClickedProperty = BindableProperty.Create(nameof(IsCallJoinButtonClicked), typeof(bool), typeof(TwilioVideoView), false);

        /// <summary>
        /// Audio mute/unmute binding property
        /// </summary>
        public static readonly BindableProperty IsUnmuteButtonClickedProperty = BindableProperty.Create(nameof(IsUnmuteButtonClicked), typeof(bool), typeof(TwilioVideoView), true);

        /// <summary>
        /// Front camera on/off(rear) binding property
        /// </summary>
        public static readonly BindableProperty IsFrontCameraOnProperty = BindableProperty.Create(nameof(IsFrontCameraOn), typeof(bool), typeof(TwilioVideoView), true);

        /// <summary>
        /// Video enable and disable binding property
        /// </summary>
        public static readonly BindableProperty IsVideoDisabledProperty = BindableProperty.Create(nameof(IsVideoEnabled), typeof(bool), typeof(TwilioVideoView), true);

        /// <summary>
        /// Token peroperty for video to connect
        /// </summary>
        public static readonly BindableProperty TokenProperty = BindableProperty.Create(nameof(Token), typeof(string), typeof(TwilioVideoView), string.Empty);

        /// <summary>
        /// Video room property binding property
        /// </summary>
        public static readonly BindableProperty RoomProperty = BindableProperty.Create(nameof(Room), typeof(string), typeof(TwilioVideoView), string.Empty);

        /// <summary>
        /// Video call status binding property
        /// </summary>
        public static readonly BindableProperty VideoCallStateProperty = BindableProperty.Create(nameof(VideoCallState), typeof(VideoState), typeof(TwilioVideoView), VideoState.None);

        /// <summary>
        /// Handle app sleep binding property
        /// </summary>
        public static readonly BindableProperty HandleAppSleepProperty = BindableProperty.Create(nameof(HandleAppSleep), typeof(bool), typeof(TwilioVideoView), true);

        /// <summary>
        /// Handle app resume binding property
        /// </summary>
        public static readonly BindableProperty HandleAppResumeProperty = BindableProperty.Create(nameof(HandleAppResume), typeof(bool), typeof(TwilioVideoView), true);

        /// <summary>
        /// video call status  property
        /// </summary>
        public VideoState VideoCallState
        {
            get
            {
                return (VideoState)GetValue(VideoCallStateProperty);
            }
            set
            {
                SetValue(VideoCallStateProperty, value);
            }
        }

        /// <summary>
        /// Video call status event callback method 
        /// </summary>
        /// <param name="sender">instance of view</param>
        public void InvokeStatusChanged(object sender)
        {
            OnVideoCallStatusChanged((sender as TwilioVideoView).VideoCallState);
        }

        /// <summary>
        /// Handle app sleep property
        /// </summary>
        public bool HandleAppSleep
        {
            get
            {
                return (bool)GetValue(HandleAppSleepProperty);
            }
            set
            {
                SetValue(HandleAppSleepProperty, value);
            }
        }

        /// <summary>
        /// Handle app resume property
        /// </summary>
        public bool HandleAppResume
        {
            get
            {
                return (bool)GetValue(HandleAppResumeProperty);
            }
            set
            {
                SetValue(HandleAppResumeProperty, value);
            }
        }

        /// <summary>
        /// Speaker on off Property
        /// </summary>
        public bool IsSpeakerOn
        {
            get
            {
                return (bool)GetValue(IsSpeakerOnProperty);
            }
            set
            {
                SetValue(IsSpeakerOnProperty, value);
            }
        }

        /// <summary>
        /// Call connect disconnect property
        /// </summary>
        public bool IsCallJoinButtonClicked
        {
            get
            {
                return (bool)GetValue(IsCallJoinButtonClickedProperty);
            }
            set
            {
                SetValue(IsCallJoinButtonClickedProperty, value);
            }
        }

        /// <summary>
        /// Video call token property
        /// </summary>
        public string Token
        {
            get
            {
                return (string)GetValue(TokenProperty);
            }
            set
            {
                SetValue(TokenProperty, value);
            }
        }

        /// <summary>
        /// Video call room join property
        /// </summary>
        public string Room
        {
            get
            {
                return (string)GetValue(RoomProperty);
            }
            set
            {
                SetValue(RoomProperty, value);
            }
        }

        /// <summary>
        /// Audio mute/unmute property
        /// </summary>
        public bool IsUnmuteButtonClicked
        {
            get
            {
                return (bool)GetValue(IsUnmuteButtonClickedProperty);
            }
            set
            {
                SetValue(IsUnmuteButtonClickedProperty, value);
            }
        }

        /// <summary>
        /// Is front camera or rear camera
        /// </summary>
        public bool IsFrontCameraOn
        {
            get
            {
                return (bool)GetValue(IsFrontCameraOnProperty);
            }
            set
            {
                SetValue(IsFrontCameraOnProperty, value);
            }
        }

        /// <summary>
        /// Video call enable/disable property
        /// </summary>
        public bool IsVideoEnabled
        {
            get
            {
                return (bool)GetValue(IsVideoDisabledProperty);
            }
            set
            {
                SetValue(IsVideoDisabledProperty, value);
            }
        }

        public TwilioVideoView()
        {
            Content = new MultipleParticipantView();
        }

        /// <summary>
        /// Turn on/off microphone
        /// </summary>
        /// <param name="turnOn">true if microphone is to be turned on</param>
        /// <returns>Returns task for turning on/off microphone</returns>
        public override Task TurnOnOffMicrophoneAsync(bool turnOn)
        {
            IsUnmuteButtonClicked = turnOn;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Turn on/off video
        /// </summary>
        /// <param name="turnOn">true if video is to be turned on</param>
        /// <returns>Returns task for turning on/off video</returns>
        public override Task TurnOnOffVideoAsync(bool turnOn)
        {
            IsVideoEnabled = turnOn;
            return Task.CompletedTask;
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
            Token = token;
            Room = sessionID;
            IsCallJoinButtonClicked = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disconnects the video call
        /// </summary>
        /// <returns>Returns task for disconnecting the call</returns>
        public override Task DisconnectCallAsync()
        {
            IsCallJoinButtonClicked = false;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Switch camera between front and rear
        /// </summary>
        /// <param name="enableFrontCamera">true if front camera is to be enabled else false for rear camera</param>
        /// <returns>Returns task for switching the camera used</returns>
        public override Task SwitchCameraAsync(bool enableFrontCamera)
        {
            IsFrontCameraOn = enableFrontCamera;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Switch orientation of video view
        /// </summary>
        /// <param name="isPortrait">true if portrait</param>
        /// <returns>Returns task for switching orientation of video view</returns>
        public override Task SwitchOrientationAsync(bool isPortrait)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// To be called when app is pushed to background
        /// </summary>
        /// <returns>Returns task for handling app sleep</returns>
        public override Task HandleAppSleepAsync()
        {
            HandleAppSleep = !HandleAppSleep;
            return base.HandleAppSleepAsync();
        }

        /// <summary>
        /// To be called when app is moved to foreground
        /// </summary>
        /// <returns>Returns task for handling app resume</returns>
        public override Task HandleAppResumeAsync()
        {
            HandleAppResume = !HandleAppResume;
            return base.HandleAppResumeAsync();
        }

        /// <summary>
        /// Invoked when remote participant is joined
        /// </summary>
        /// <param name="participantVideo">Participant video</param>
        public void InvokeRemoteParticipantJoined(ParticipantVideoView participantVideo)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                (Content as MultipleParticipantView).AddParticipant(participantVideo);
            });
        }

        /// <summary>
        /// Invoked when remote participant leaves
        /// </summary>
        /// <param name="participantVideo">Participant video</param>
        public void InvokeRemoteParticipantLeft(ParticipantVideoView participantVideo)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                (Content as MultipleParticipantView).RemoveParticipant(participantVideo);
            });
        }

        /// <summary>
        /// Invoked when connection is established
        /// </summary>
        /// <param name="participantVideo">Participant video</param>
        public void InvokeConnectionEstablished(ParticipantVideoView participantVideo)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                (Content as MultipleParticipantView).AddLocalParticipant(participantVideo);
            });
        }
    }
}
