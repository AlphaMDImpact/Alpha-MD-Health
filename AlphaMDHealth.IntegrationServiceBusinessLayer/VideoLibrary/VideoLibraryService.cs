using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class VideoLibraryService : LibraryService
    {
        private readonly IVideoLibrary _videoLibrary;

        public VideoLibraryService(HttpContext httpContext) : base(httpContext)
        {
            switch (_libraryServiceData.LibraryInfo.ServiceType)
            {
                case ServiceType.Twillio:
                    _videoLibrary = new TwillioVideoLibrary(_libraryServiceData.LibraryInfo);
                    break;
                case ServiceType.LiveSwitch:
                    _videoLibrary = new LiveSwitchVideoLibrary(_libraryServiceData.LibraryInfo);
                    break;
                case ServiceType.Vidyo_Io:
                    _videoLibrary = new VidyoIoVideoLibrary(_libraryServiceData.LibraryInfo);
                    break;
                case ServiceType.OpenTok:
                    _videoLibrary = new OpenTokVideoLibrary(_libraryServiceData.LibraryInfo);
                    break;
                case ServiceType.Daily_Co:
                    _videoLibrary = new DailyCoVideoLibrary(_libraryServiceData.LibraryDetails);
                    break;
                default:
                    _libraryServiceData.LibraryInfo.ServiceType = ServiceType.OpenTok;
                    _videoLibrary = new OpenTokVideoLibrary(_libraryServiceData.LibraryInfo);
                    break;
            }
        }

        /// <summary>
        /// Generate Video Session for input application 
        /// </summary>
        /// <param name="forApplication">for application</param>
        /// <param name="roomID">room to be conncted</param>
        /// <param name="userID">user to be connected</param>
        /// <param name="userName">usernmae to be displayed</param>
        /// <returns>Returns Video Sesssion Details</returns>
        public async Task<VideoDTO> GenerateSessionAsync(string forApplication, string roomID, string userID, string userName)
        {
            VideoDTO videoSession = new VideoDTO();
            try
            {
                if (_libraryServiceData?.LibraryInfo != null && forApplication == _libraryServiceData.LibraryInfo.ForApplication)
                {
                    videoSession.AddedBy = userID;
                    videoSession.LastModifiedBy = userName;
                    videoSession.Video = new VideoModel { VideoRoomID = roomID, VideoLink = _libraryServiceData.LibraryInfo.ServiceTarget };
                    videoSession.Video.ServiceType = _libraryServiceData.LibraryInfo.ServiceType;
                    await _videoLibrary.GenerateSessionAsync(videoSession).ConfigureAwait(false);
                    if (_libraryServiceData.LibraryInfo.LogCalls)
                    {
                        _ = SaveServiceCallLogsAsync(_libraryServiceData.LibraryInfo.OrganisationServiceID, videoSession.ErrCode.ToString(), videoSession.ErrorDescription).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                videoSession.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                videoSession.ErrorDescription = ex.Message;
                LogError(ex.Message, ex.StackTrace);
            }
            return videoSession;
        }

        /// <summary>
        /// Create Video Session for input application 
        /// </summary>
        /// <returns>Returns Video Sesssion Details</returns>
        public async Task<VideoDTO> CreateSessionAsync()
        {
            VideoDTO videoSession = new VideoDTO();
            try
            {
                videoSession.Video = new VideoModel();
                await _videoLibrary.CreateSessionAsync(videoSession);
                videoSession.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                videoSession.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                videoSession.ErrorDescription = ex.Message;
                LogError(ex.Message, ex.StackTrace);
            }
            return videoSession;
        }
    }
}