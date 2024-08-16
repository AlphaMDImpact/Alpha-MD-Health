using AlphaMDHealth.Utility;
using Foundation;
using Twilio;

namespace AlphaMDHealth.VideoLibrary.Platforms.iOS
{
    /// <summary>
    /// Twilio room delegate
    /// </summary>
    public class TwilioRoomDelegate : TVIRoomDelegate
    {
        private readonly TwilioVideoViewRenderer _formsControlRenderer;
        private readonly TwilioParticipantRoom _twilioParticipantRoom;

        /// <summary>
        /// Twilio room delegate
        /// </summary>
        public TwilioRoomDelegate()
        {
        }

        /// <summary>
        /// Twilio room delegate
        /// </summary>
        /// <param name="formsControl">Instance of forms control</param>
        public TwilioRoomDelegate(TwilioVideoViewRenderer formsControl)
        {
            _formsControlRenderer = formsControl;
            _twilioParticipantRoom = new TwilioParticipantRoom(_formsControlRenderer);
        }

        /// <summary>
        /// Invoked when connected to room
        /// </summary>
        /// <param name="room">Instance of room</param>
        public override void DidConnectToRoom(TVIRoom room)
        {
            _formsControlRenderer.FormsControl.VideoCallState = VideoState.StateConnected;
            _formsControlRenderer.FormsControl.InvokeStatusChanged(_formsControlRenderer.FormsControl);

            if (room.RemoteParticipants.Length > 0)
            {
                foreach (var participant in room.RemoteParticipants)
                {
                    participant.Delegate = _twilioParticipantRoom;
                }
            }
        }

        /// <summary>
        /// Invoked when disconnected from room
        /// </summary>
        /// <param name="room">Instance of room</param>
        public override void DidReconnectToRoom(TVIRoom room)
        {
            _formsControlRenderer.FormsControl.VideoCallState = VideoState.StateConnected;
            _formsControlRenderer.FormsControl.InvokeStatusChanged(_formsControlRenderer.FormsControl);
        }

        /// <summary>
        /// Invoked when disconnected from room but with error
        /// </summary>
        /// <param name="room">Instance of room</param>
        /// <param name="error">Error data</param>
        public override void DidDisconnectWithError(TVIRoom room, NSError error)
        {
            _formsControlRenderer.FormsControl.VideoCallState = VideoState.StateDisconnectedUnexpected;
            _formsControlRenderer.FormsControl.InvokeStatusChanged(_formsControlRenderer.FormsControl);
        }

        /// <summary>
        /// Invoked when connected to room but with error
        /// </summary>
        /// <param name="room">Instance of room</param>
        /// <param name="error">Error data</param>
        public override void DidFailToConnectWithError(TVIRoom room, NSError error)
        {
            _formsControlRenderer.FormsControl.VideoCallState = VideoState.StateConnectionFailure;
            _formsControlRenderer.FormsControl.InvokeStatusChanged(_formsControlRenderer.FormsControl);
        }

        /// <summary>
        /// Invoked when remote participant is connected
        /// </summary>
        /// <param name="room">Instance of room</param>
        /// <param name="participant">Remote participant</param>
        public override void ParticipantDidConnect(TVIRoom room, TVIRemoteParticipant participant)
        {
            participant.Delegate = _twilioParticipantRoom;
        }

        /// <summary>
        /// Invoked when recording is started
        /// </summary>
        /// <param name="room">Instance of room</param>
        public override void RoomDidStartRecording(TVIRoom room)
        {
            //
        }

        /// <summary>
        /// Invoked when recording is stopped
        /// </summary>
        /// <param name="room">Instance of room</param>
        public override void RoomDidStopRecording(TVIRoom room)
        {
            //
        }

        /// <summary>
        /// Invoked when dominant speaker is changed
        /// </summary>
        /// <param name="room">Instance of room</param>
        /// <param name="participant">Dominant Speaker</param>
        public override void DominantSpeakerDidChange(TVIRoom room, TVIRemoteParticipant participant)
        {
            //
        }

        /// <summary>
        /// Invoked when reconnecting but with error
        /// </summary>
        /// <param name="room">Instance of room</param>
        /// <param name="error">Error data</param>
        public override void IsReconnectingWithError(TVIRoom room, NSError error)
        {
            //
        }

        /// <summary>
        /// Invoked when room participant is disconnected
        /// </summary>
        /// <param name="room">Instance of room</param>
        /// <param name="participant">Remote participant</param>
        public override void ParticipantDidDisconnect(TVIRoom room, TVIRemoteParticipant participant)
        {
            _formsControlRenderer.RemoveParticipant(participant);
        }
    }
}