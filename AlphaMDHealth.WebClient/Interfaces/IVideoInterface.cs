using System.Threading.Tasks;

namespace AlphaMDHealth.WebClient
{
    public interface IVideoInterface
    {
        /// <summary>
        /// Join Room
        /// </summary>
        /// <returns></returns>
        Task<bool> JoinAsync(string roomName,string address, string userId, string token, string applicationId, string registerToken, string link);

        /// <summary>
        /// Mute Audio
        /// </summary>
        /// <returns></returns>
        ValueTask MuteAudioAsync();

        /// <summary>
        /// UnMute Audio
        /// </summary>
        /// <returns></returns>
        ValueTask UnMuteAudioAsync();

        /// <summary>
        /// Mute Video
        /// </summary>
        /// <returns></returns>
        ValueTask MuteVideoAsync();

        /// <summary>
        /// UnMute Video
        /// </summary>
        /// <returns></returns>
        ValueTask UnMuteVideoAsync();

        /// <summary>
        /// Leave Room
        /// </summary>
        /// <returns></returns>
        Task<bool> LeaveAsync(string roomName, string token, string userId);

    }
}
