using System.Collections.Specialized;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class VideoService : BaseService
    {
        public VideoService(IEssentials essentials):base(essentials) { }
        /// <summary>
        /// Get video token
        /// </summary>
        /// <param name="videoData">Video object to return token</param>
        /// <returns>video token</returns>
        public async Task SyncVideoSessionFromServerAsync(VideoDTO videoData)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    PathWithoutBasePath = UrlConstants.GET_VIDEO_SESSION_ASYNC,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.ROOM_ID, videoData.Video.VideoRoomID },
                        { Constants.USER_ID,videoData.AddedBy  },
                        { Constants.USER_NAME, videoData.LastModifiedBy },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                videoData.ErrCode = httpData.ErrCode;
                if (videoData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        var videoDetails = data[nameof(VideoDTO.Video)];
                        if (videoDetails.HasValues)
                        {
                            videoData.Video = MapSession(videoDetails);
                            videoData.PhoneNumber = GetDataItem<string>(data, nameof(VideoDTO.PhoneNumber));
                        }
                    }
                    else
                    {
                        videoData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                    }
                }
            }
            catch (Exception ex)
            {
                videoData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Map session data
        /// </summary>
        /// <param name="dataItem">program json object</param>
        /// <returns>Program data</returns>
        private VideoModel MapSession(JToken dataItem)
        {
            return new VideoModel
            {
                VideoRoomID = GetDataItem<string>(dataItem, nameof(VideoModel.VideoRoomID)),
                VideoLink = GetDataItem<string>(dataItem, nameof(VideoModel.VideoLink)),
                VideoToken = GetDataItem<string>(dataItem, nameof(VideoModel.VideoToken)),
                ApplicationID = GetDataItem<string>(dataItem, nameof(VideoModel.ApplicationID)),
                SecretKey = GetDataItem<string>(dataItem, nameof(VideoModel.SecretKey)),
                ServiceType = GetDataItem<ServiceType>(dataItem, nameof(VideoModel.ServiceType)),
            };
        }
    }
}
