using AlphaMDHealth.Model;
using FM.LiveSwitch;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    internal class LiveSwitchVideoLibrary : BaseService, IVideoLibrary
    {
        private readonly LibraryServiceModel _libraryServiceDetails;

        public LiveSwitchVideoLibrary(LibraryServiceModel libraryServiceDetails)
        {
            _libraryServiceDetails = libraryServiceDetails;
        }

        public async Task GenerateSessionAsync(VideoDTO videoData)
        {
            videoData.Video.SecretKey = Token.GenerateClientRegisterToken(
                 _libraryServiceDetails.ServiceClientIdentifier,
                 videoData.AddedBy,
                "",
                "",
                new[] { "Participants" },
                videoData.Video.VideoRoomID,
                _libraryServiceDetails.ServiceClientSecrete
            );
            if (!string.IsNullOrEmpty(videoData.Video.SecretKey))
            {
                videoData.Video.VideoToken = Token.GenerateClientJoinToken(
                              _libraryServiceDetails.ServiceClientIdentifier,
                              videoData.AddedBy,
                             "",
                             "",
                             videoData.Video.VideoRoomID,
                             _libraryServiceDetails.ServiceClientSecrete
                         );
            }
            videoData.Video.ApplicationID = _libraryServiceDetails.ServiceClientIdentifier;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Create SessionID 
        /// </summary>
        /// <param name="videoData">object to return session ID</param>
        /// <returns>Session ID</returns>
        public Task CreateSessionAsync(VideoDTO videoData)
        {
            videoData.Video.VideoRoomID = Guid.NewGuid().ToString();
            return Task.CompletedTask;
        }
    }
}