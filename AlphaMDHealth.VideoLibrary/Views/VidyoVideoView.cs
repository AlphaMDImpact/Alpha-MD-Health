using IntegrationLibrary.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VideoLibrary
{
    public class VidyoVideoView : VideoView
    {
        private readonly IVidyoController _vidyoController;
        private readonly VidyoViewModel _viewModel;
        private const string VIDEO_API_VERSION = "v ";
        private bool _frontCamera = true;

        public VidyoVideoView()
        {
            Content = new MultipleParticipantView();
            _vidyoController = DependencyService.Get<IVidyoController>();
            _viewModel = VidyoViewModel.GetInstance();
        }

        void VidyoControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Constants.CONNECTOR_STATE)
            {
                SetVideoConnectorState(_vidyoController.ConnectorState);
            }
        }

        /// <summary>
        /// Sets the state of the video connector
        /// </summary>
        /// <param name="vidyoConnectorState"></param>
        public void SetVideoConnectorState(VidyoConnectorState vidyoConnectorState)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Update the toggle connect button to either start call or end call image
                _viewModel.CallAction = vidyoConnectorState == VidyoConnectorState.VidyoConnectorStateConnected ?
                VidyoCallAction.VidyoCallActionConnect : VidyoCallAction.VidyoCallActionDisconnect;
                if (vidyoConnectorState == VidyoConnectorState.VidyoConnectorStateConnected)
                {
                    //remove all controls or modify UI
                    OnVideoCallStatusChanged(VideoState.StateConnected);
                }
                else if (vidyoConnectorState == VidyoConnectorState.VidyoConnectorStateDisconnected)
                {
                    //remove anything that needs to be removed
                    _vidyoController.Cleanup();
                    OnVideoCallStatusChanged(VideoState.StateDisconnected);
                }
                else
                {
                    _vidyoController.Cleanup();
                    OnVideoCallStatusChanged(VideoState.StateConnectionFailure);
                }
            });
        }

        /// <summary>
        /// Handle changes on page load
        /// </summary>
        /// <returns>Returns a task which handles page load</returns>
        public override Task OnLoadAsync()
        {
            (_vidyoController as INotifyPropertyChanged).PropertyChanged += VidyoControllerPropertyChanged;
            _vidyoController.OnConnectionEstablished += OnConnectionEstablished;
            _vidyoController.OnRemoteParticipantJoined += OnRemoteParticipantJoined;
            _vidyoController.OnRemoteParticipantLeft += OnRemoteParticipantLeft;
            _viewModel.ClientVersion = VIDEO_API_VERSION + _vidyoController.OnAppStart();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Unregister or clean up on unload
        /// </summary>
        /// <returns>Returns a task which handles page unload</returns>
        public override Task OnUnloadAsync()
        { 
            (_vidyoController as INotifyPropertyChanged).PropertyChanged -= VidyoControllerPropertyChanged;
            _vidyoController.OnConnectionEstablished -= OnConnectionEstablished;
            _vidyoController.OnRemoteParticipantJoined -= OnRemoteParticipantJoined;
            _vidyoController.OnRemoteParticipantLeft -= OnRemoteParticipantLeft;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Turn on/off microphone
        /// </summary>
        /// <param name="turnOn">true if microphone is to be turned on</param>
        /// <returns>Returns task for turning on/off microphone</returns>
        public override Task TurnOnOffMicrophoneAsync(bool turnOn)
        {
            _vidyoController.SetMicrophonePrivacy(!turnOn);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Turn on/off video
        /// </summary>
        /// <param name="turnOn">true if video is to be turned on</param>
        /// <returns>Returns task for turning on/off video</returns>
        public override Task TurnOnOffVideoAsync(bool turnOn)
        {
            _vidyoController.SetCameraPrivacy(!turnOn);
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
            return _vidyoController.ConnectAsync(videoLink, token, userID, sessionID);
        }

        /// <summary>
        /// Disconnects the video call
        /// </summary>
        /// <returns>Returns task for disconnecting the call</returns>
        public override Task DisconnectCallAsync()
        {
            return _vidyoController.DisconnectAsync();
        }

        /// <summary>
        /// Switch camera between front and rear
        /// </summary>
        /// <param name="enableFrontCamera">true if front camera is to be enabled else false for rear camera</param>
        /// <returns>Returns task for switching the camera used</returns>
        public override Task SwitchCameraAsync(bool enableFrontCamera)
        {
            if(enableFrontCamera != _frontCamera)
            {
                _vidyoController.CycleCamera();
                _frontCamera = enableFrontCamera;
            }
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
            _vidyoController.OnAppSleep();
            return base.HandleAppSleepAsync();
        }

        /// <summary>
        /// To be called when app is moved to foreground
        /// </summary>
        /// <returns>Returns task for handling app resume</returns>
        public override Task HandleAppResumeAsync()
        {
            _vidyoController.OnAppResume();
            return base.HandleAppResumeAsync();
        }

        private void OnRemoteParticipantLeft(object sender, System.EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                (Content as MultipleParticipantView).RemoveParticipant(sender as View);
            });
        }

        private void OnRemoteParticipantJoined(object sender, System.EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                (Content as MultipleParticipantView).AddParticipant(sender as View);
            });
        }

        private void OnConnectionEstablished(object sender, System.EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                (Content as MultipleParticipantView).AddLocalParticipant(sender as View);
            });
        }
    }
}
