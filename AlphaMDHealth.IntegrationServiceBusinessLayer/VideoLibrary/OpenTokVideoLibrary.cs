using AlphaMDHealth.Model;
using OpenTokSDK;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    internal class OpenTokVideoLibrary : BaseService, IVideoLibrary
    {
        private readonly LibraryServiceModel _libraryServiceDetails;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="libraryServiceDetails"></param>
        public OpenTokVideoLibrary(LibraryServiceModel libraryServiceDetails)
        {
            _libraryServiceDetails = libraryServiceDetails;
        }

        /// <summary>
        /// Generate session
        /// </summary>
        /// <param name="videoData">object to return video token</param>
        /// <returns>Returns task status</returns>
        public Task GenerateSessionAsync(VideoDTO videoData)
        {
            OpenTok openTok = new OpenTok(Convert.ToInt32(_libraryServiceDetails.ServiceClientIdentifier), _libraryServiceDetails.ServiceClientSecrete);
            videoData.Video.VideoToken = openTok.GenerateToken(videoData.Video.VideoRoomID);
            videoData.Video.ApplicationID = _libraryServiceDetails.ServiceClientIdentifier;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Create SessionID 
        /// </summary>
        /// <param name="videoData">object to return session ID</param> 
        /// <returns>Session ID</returns>
        public Task CreateSessionAsync(VideoDTO videoData)
        {
            OpenTok openTok = new OpenTok(Convert.ToInt32(_libraryServiceDetails.ServiceClientIdentifier), _libraryServiceDetails.ServiceClientSecrete);
            videoData.Video.VideoRoomID = openTok.CreateSession().Id;
            return Task.CompletedTask;
        }
    }
}