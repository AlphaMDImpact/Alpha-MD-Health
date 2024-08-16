using AlphaMDHealth.IntegrationServiceBusinessLayer;
using AlphaMDHealth.Model;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.IntegrationServiceLayer.Controllers
{
    /// <summary>
    /// Controller for generating a video call session
    /// </summary>
    [Route("api/VideoLibraryService")]
    [ApiController]
    public class VideoLibraryServiceController : ControllerBase
    {
        /// <summary>
        /// Generate Video Call Session
        /// </summary>
        /// <param name="forApplication">for application</param>
        /// <param name="roomID">video call RoomID</param>
        /// <param name="userId">video call Participant Id</param>
        /// <param name="userName">video call Participant Name</param>
        /// <returns>Returns Video Session Details</returns>
        [Route("GenerateSession")]
        [HttpGet]
        public async Task<VideoDTO> GenerateSessionAsync(string forApplication, string roomID, string userId, string userName)
        {
            return await new VideoLibraryService(HttpContext).GenerateSessionAsync(forApplication, roomID, userId, userName).ConfigureAwait(false);
        }

        /// <summary>
        /// Create Video Call Session
        /// </summary>
        /// <returns>Session ID</returns>
        [Route("CreateSession")]
        [HttpGet]
        public async Task<VideoDTO> CreateSessionAsync()
        {
            return await new VideoLibraryService(HttpContext).CreateSessionAsync().ConfigureAwait(false);
        }

    }
}
