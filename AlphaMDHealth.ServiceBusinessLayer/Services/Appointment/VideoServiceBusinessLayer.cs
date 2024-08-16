using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class VideoServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Appointment service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public VideoServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get video session
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="roomID">room id</param>
        /// <param name="userId">user id</param>
        /// <param name="userName">user name</param>
        /// <returns></returns>
        public async Task<VideoDTO> GetVideoSessionAsync(byte languageID, long organisationID, string roomID, string userId, string userName)
        {
            VideoDTO videoData = new VideoDTO { Video = new VideoModel() };
            try
            {
                if (languageID < 1 || organisationID < 1)
                {
                    videoData.ErrCode = ErrorCode.InvalidData;
                    return videoData;
                }
                videoData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                videoData.OrganisationID = organisationID;
                videoData.AddedBy = userId;
                videoData.LastModifiedBy = userName;
                videoData.Video.VideoRoomID = roomID;
                await GenerateSessionForVideoCallAsync(videoData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                videoData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return videoData;
        }
    }
}
