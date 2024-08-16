using AlphaMDHealth.Model;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public interface IVideoLibrary
    {
        /// <summary>
        /// Generate Video Call Session
        /// <paramref name="videoData">video call details</paramref>
        /// </summary>
        /// <returns>Food items matching the given search string</returns>
        Task GenerateSessionAsync(VideoDTO videoData);

        /// <summary>
        /// Create Video call Session
        /// <paramref name="videoData">video call details</paramref>
        /// </summary>
        /// <returns>Session ID</returns>
        Task CreateSessionAsync(VideoDTO videoData);
    }
}