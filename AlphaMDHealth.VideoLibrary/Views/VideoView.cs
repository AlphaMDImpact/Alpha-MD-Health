
namespace AlphaMDHealth.VideoLibrary
{
    /// <summary>
    /// Video view for all video calling libraries
    /// </summary>
    public class VideoView : ContentView
    {
        /// <summary>
        /// Turn on/off microphone
        /// </summary>
        /// <param name="turnOn">true if microphone is to be turned on</param>
        /// <returns>Returns task for turning on/off microphone</returns>
        public virtual Task TurnOnOffMicrophoneAsync(bool turnOn)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Turn on/off video
        /// </summary>
        /// <param name="turnOn">true if video is to be turned on</param>
        /// <returns>Returns task for turning on/off video</returns>
        public virtual Task TurnOnOffVideoAsync(bool turnOn)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Starts the video call
        /// </summary>
        /// <param name="apiKey">api key</param>
        /// <param name="token">token</param>
        /// <param name="sessionID">session ID or room ID</param>
        /// <param name="videoLink">video Url as required by some video packages</param>
        /// <param name="userID">Id of the user</param>
        /// <returns>Returns task for connecting the call</returns>
        public virtual Task ConnectCallAsync(string apiKey, string token, string sessionID, string videoLink, string userID)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Disconnects the video call
        /// </summary>
        /// <returns>Returns task for disconnecting the call</returns>
        public virtual Task DisconnectCallAsync()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Switch camera between front and rear
        /// </summary>
        /// <param name="enableFrontCamera">true if front camera is to be enabled else false for rear camera</param>
        /// <returns>Returns task for switching the camera used</returns>
        public virtual Task SwitchCameraAsync(bool enableFrontCamera)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Switch orientation of video view
        /// </summary>
        /// <param name="isPortrait">true if portrait</param>
        /// <returns>Returns task for switching orientation of video view</returns>
        public virtual Task SwitchOrientationAsync(bool isPortrait)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Handle changes on page load
        /// </summary>
        /// <returns>Returns a task which handles page load</returns>
        public virtual Task OnLoadAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle changes on page unload
        /// </summary>
        /// <returns>Returns a task which handles page unload</returns>
        public virtual Task OnUnloadAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// To be called when app is pushed to background
        /// </summary>
        /// <returns>Returns task for handling app sleep</returns>
        public virtual Task HandleAppSleepAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// To be called when app is moved to foreground
        /// </summary>
        /// <returns>Returns task for handling app resume</returns>
        public virtual Task HandleAppResumeAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when video call status is changed
        /// </summary>
        public event EventHandler<EventArgs> VideoCallStatusChanged;

        /// <summary>
        /// The event-invoking method that derived classes can override.
        /// </summary>
        /// <param name="sender">Updated video state</param>
        public virtual void OnVideoCallStatusChanged(object sender)
        {
            // Safely raise the event for all subscribers
            VideoCallStatusChanged?.Invoke(sender, EventArgs.Empty);
        }
    }
}