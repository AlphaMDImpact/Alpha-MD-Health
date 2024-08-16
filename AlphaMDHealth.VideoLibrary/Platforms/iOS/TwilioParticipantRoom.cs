using Twilio;

namespace AlphaMDHealth.VideoLibrary.Platforms.iOS
{
    /// <summary>
    /// Twilio participant room
    /// </summary>
    public class TwilioParticipantRoom : TVIRemoteParticipantDelegate
    {
        private readonly TwilioVideoViewRenderer _formsControlRenderer;

        /// <summary>
        /// Twilio participant room
        /// </summary>
        public TwilioParticipantRoom()
        {
        }

        /// <summary>
        /// Participant room
        /// </summary>
        /// <param name="formsControlRenderer">Instance of renderer</param>
        public TwilioParticipantRoom(TwilioVideoViewRenderer formsControlRenderer)
        {
            _formsControlRenderer = formsControlRenderer;
        }

        /// <summary>
        /// Invoked when participant is subscribed to Video track
        /// </summary>
        /// <param name="videoTrack">Video track</param>
        /// <param name="publication">Remote video track publication</param>
        /// <param name="participant">Remote participant</param>
        public async override void DidSubscribeToVideoTrack(TVIRemoteVideoTrack videoTrack, TVIRemoteVideoTrackPublication publication, TVIRemoteParticipant participant)
        {
            await _formsControlRenderer.SetupRemoteVideoViewAsync(videoTrack, participant.Identity).ConfigureAwait(false);
        }

        /// <summary>
        /// Invoked when participant unsubscribed from video track
        /// </summary>
        /// <param name="videoTrack">Video track</param>
        /// <param name="publication">Remote video track publication</param>
        /// <param name="participant">Remote participant</param>
        public override void DidUnsubscribeFromVideoTrack(TVIRemoteVideoTrack videoTrack, TVIRemoteVideoTrackPublication publication, TVIRemoteParticipant participant)
        {
            _formsControlRenderer.RemoveParticipantVideo(videoTrack);
        }
    }
}