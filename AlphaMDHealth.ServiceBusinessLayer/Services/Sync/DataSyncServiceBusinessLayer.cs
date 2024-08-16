using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class DataSyncServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Datasync service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public DataSyncServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get master\user data based on batch received as input
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="selectedUserID">Currently selected user id</param>
        /// <param name="isAuthorized">Flag which decides wether it is logged in user call or not</param>
        /// <param name="syncData">sync data instance with filter data</param>
        /// <returns>Master/User data with operation status</returns>
        public async Task<DataSyncDTO> GetMobileDataAsync(byte languageID, long organisationID, long selectedUserID, bool isAuthorized, DataSyncDTO syncData)
        {
            try
            {
                if (organisationID < 1 || !(GenericMethods.IsListNotEmpty(syncData.DataSyncForRecords)))
                {
                    syncData.ErrCode = ErrorCode.InvalidData;
                    return syncData;
                }
                if (isAuthorized && AccountID < 1)
                {
                    syncData.ErrCode = ErrorCode.Unauthorized;
                    return syncData;
                }
                syncData.AccountID = AccountID;
                syncData.SelectedUserID = selectedUserID;
                syncData.PermissionAtLevelID = syncData.OrganisationID = organisationID;
                syncData.LanguageID = languageID;
                syncData.FeatureFor = FeatureFor;
                await new DataSyncServiceDataLayer().GetMobileDataAsync(syncData).ConfigureAwait(false);
                if (syncData.ErrCode == ErrorCode.OK)
                {
                    await Task.WhenAll(
                        from syncFor in syncData.DataSyncForRecords select LoadDependancyAsync(syncFor, syncData)
                    ).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                syncData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return syncData;
        }

        private async Task LoadDependancyAsync(DataSyncModel syncFor, DataSyncDTO syncData)
        {
            switch (syncFor.SyncFor.ToEnum<DataSyncFor>())
            {
                case DataSyncFor.Appointments:
                    await new AppointmentServiceBusinessLayer(_httpContext).ReplaceAppointmentParticipantsImageCdnLinkAsync(new AppointmentDTO { AppointmentParticipants = syncData.AppointmentParticipants });
                    break;
                case DataSyncFor.AppIntros:
                    await new AppIntroServiceBusinessLayer(_httpContext).ReplaceAppIntrosImageCdnLinkAsync(syncData.AppIntros, new BaseDTO());
                    break;
                case DataSyncFor.Chats:
                    await new ChatServiceBusinessLayer(_httpContext).ReplaceChatMessageImageCdnLinkAsync(syncData.ChatDetails);
                    break;
                case DataSyncFor.EducationCategories:
                    await new EducationCategoryServiceBusinessLayer(_httpContext).ReplaceEductaionCatergoryImageCdnLinkAsync(syncData.EducationCategory);
                    break;
                case DataSyncFor.EducationDetails:
                    await new ContentPageServiceBusinessLayer(_httpContext).ReplaceContentDetailImageCdnLinkAsync(syncData.EducationPageDetails, new BaseDTO());
                    break;
                case DataSyncFor.Educations:
                    await new ContentPageServiceBusinessLayer(_httpContext).ReplaceContentPageImagesCdnLinksAsync(syncData.EducationPages, new BaseDTO());
                    break;
                case DataSyncFor.FileCategories:
                    await new FileCategoryServiceBusinessLayer(_httpContext).ReplaceFileCatergoryImageCdnLinkAsync(syncData.FileCategories);
                    break;
                case DataSyncFor.Files:
                    await new FilesServiceBusinessLayer(_httpContext).ReplaceFileAttachmentImageCdnLinkAsync(syncData.FileDocuments);
                    break;
                case DataSyncFor.InstructionI18N:
                    await new ProgramServiceBusinessLayer(_httpContext).ReplaceInstructionImageCdnLinkAsync(syncData.InstructionI18N);
                    break;
                case DataSyncFor.Pages:
                    await new ContentPageServiceBusinessLayer(_httpContext).ReplaceContentPagesImageCdnLinkAsync(syncData.Pages, syncData.PagesDetails);
                    break;
                case DataSyncFor.QuestionnaireI18N:
                    var questionnaireService = new QuestionnaireServiceBusinessLayer(_httpContext);
                    await Task.WhenAll(
                        questionnaireService.ReplaceQuestionnaireDetailImageCdnLinkAsync(syncData.QuestionnaireDetails),
                        questionnaireService.ReplaceQuestionnaireRecommendationsImageCdnLinkAsync(syncData.QuestionnaireRecommendations),
                        questionnaireService.ReplaceQuestionImageCdnLinkAsync(syncData.QuestionnaireQuestionDetails)
                    );
                    break;
                case DataSyncFor.Resources:
                    await ReplaceResourcesImageCdnLinkAsync(syncData).ConfigureAwait(false);
                    break;
                case DataSyncFor.Settings:
                    await ReplaceSettingImageCdnLinkAsync(syncData).ConfigureAwait(false);
                    break;
                case DataSyncFor.Users:
                    await new UserServiceBusinessLayer(_httpContext).ReplaceUserImageCdnLinkAsync(syncData.Users);
                    break;
                case DataSyncFor.Trackers:
                    var trackerService = new TrackerServiceBusinessLayer(_httpContext);
                    await Task.WhenAll(
                        trackerService.ReplaceTrackerImageCdnLinkAsync(syncData.TrackerRanges),
                        trackerService.ReplaceTrackerRangeDetailsImageCdnLinkAsync(syncData.TrackerRangesI18N)
                    );
                    break;
                default:
                    // default case
                    break;
            }
        }

        /// <summary>
        /// Registers SignalR connectionID for client
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="selectedUserID">Currently selected user id</param>
        /// <param name="connectionID">SignalR connection id</param>
        /// <param name="headers">Request headers</param>
        /// <param name="hubContext">SignalR hub context</param>
        /// <returns>Result of operation</returns>
        public async Task<BaseDTO> RegisterSignalRAsync(byte languageID, long organisationID, long selectedUserID, string connectionID, IHeaderDictionary headers, IHubContext<NotificationHub> hubContext)
        {
            BaseDTO result = new BaseDTO();
            try
            {
                if (organisationID < 1 || languageID < 1 || string.IsNullOrWhiteSpace(connectionID))
                {
                    result.ErrCode = ErrorCode.InvalidData;
                    return result;
                }
                if (AccountID < 1)
                {
                    result.ErrCode = ErrorCode.InvalidData;
                    return result;
                }
                result.AccountID = AccountID;
                result.OrganisationID = organisationID;
                result.SelectedUserID = selectedUserID;
                // Setup SignalR connection
                await SetupSignalRTagsAsync(result, connectionID, headers, hubContext).ConfigureAwait(false);
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return result;
        }
    }
}