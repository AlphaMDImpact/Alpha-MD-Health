using AlphaMDHealth.Model;
using Twilio.Jwt.AccessToken;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    internal class TwillioVideoLibrary : BaseService, IVideoLibrary
    {
        private readonly LibraryServiceModel _libraryServiceDetails;

        public TwillioVideoLibrary(LibraryServiceModel libraryServiceDetails)
        {
            _libraryServiceDetails = libraryServiceDetails;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="videoData"></param>
        /// <returns></returns>
        public Task GenerateSessionAsync(VideoDTO videoData)
        {
            // Create a video grant for the token
            var grant = new VideoGrant();
            //videoData.Video.VideoRoomID = "cool room";
            grant.Room = videoData.Video.VideoRoomID;
            var grants = new HashSet<IGrant> { grant };
            // Create an Access Token generator
            var token = new Token(_libraryServiceDetails.ServiceTarget.Trim(), _libraryServiceDetails.ServiceClientIdentifier.Trim(), _libraryServiceDetails.ServiceClientSecrete.Trim(), identity: videoData.AddedBy, grants: grants).ToJwt();
            videoData.Video.VideoToken = token;
            return Task.CompletedTask;
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