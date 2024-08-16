using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class PushNotificationServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Push notification service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public PushNotificationServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get System tags
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="clientID">Client Id</param>
        /// <param name="notification">Notification Registration data</param>
        /// <returns>syatem Tags data with operation status</returns>
        public async Task<BaseDTO> RegisterDeviceForNotificationsAsync(byte languageID, long organisationID, string clientID, NotificationDTO notification)
        {
            SystemIdentifierDTO result = new SystemIdentifierDTO();
            try
            {
                if (ValidateNotificationData(languageID, organisationID, clientID, notification))
                {
                    result.ErrCode = ErrorCode.InvalidData;
                    return result;
                }
                notification.AccountID = AccountID;
                if (notification.AccountID < 1)
                {
                    result.ErrCode = ErrorCode.Unauthorized;
                    return result;
                }
                // Get generic tags
                result.SystemIdentifiers = ((SystemIdentifierDTO)await GetDataFromCacheAsync(CachedDataType.SystemIdentifiers, clientID, languageID, default, 0, 0, false))?.SystemIdentifiers;
                if (result.SystemIdentifiers == null)
                {
                    result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                }
                else
                {
                    notification.NotificationData.UserName = Constants.ACCOUNT_TAG_PREFIX + notification.AccountID;
                    if (!string.IsNullOrWhiteSpace(notification.NotificationData.Tags) && !notification.NotificationData.Tags.EndsWith(Constants.SYMBOL_COMMA_SEPERATOR_STRING))
                    {
                        notification.NotificationData.Tags += Constants.SYMBOL_COMMA_SEPERATOR_STRING;
                    }
                    notification.NotificationData.Tags += Constants.ORGANISATION_TAG_PREFIX + organisationID + Constants.COMMA_SEPARATOR + notification.NotificationData.UserName +
                        Constants.COMMA_SEPARATOR + Constants.APPLICATION_NAME_TEXT + Constants.COMMA_SEPARATOR +
                        result.SystemIdentifiers.FirstOrDefault().IdentifierTags;

                    var settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, languageID, default, default, organisationID, false).ConfigureAwait(false)).Settings;
                    string[] notificationSetting = GetSettingValueByKey(settings, SettingsConstants.S_NOTIFICATION_MICRO_SERVICE_KEY).Split(Constants.SYMBOL_PIPE_SEPERATOR);
                    notification.ForApplication = notificationSetting[0];
                    HttpServiceModel<NotificationDTO> httpData = new HttpServiceModel<NotificationDTO>
                    {
                        CancellationToken = new CancellationToken(),
                        BaseUrl = new Uri(UrlConstants.MICRO_SERVICE_PATH),
                        PathWithoutBasePath = UrlConstants.REGISTER_DEVICE_FOR_NOTIFICATION_ASYNC,
                        AuthType = AuthorizationType.Basic,
                        ClientIdentifier = notificationSetting[1],
                        ClientSecret = notificationSetting[2],
                        ForApplication = notificationSetting[0],
                        ContentToSend = notification
                    };
                    notification.NotificationData.HubID = Convert.ToInt32(notificationSetting[3], CultureInfo.InvariantCulture);
                    await new HttpLibService(new HttpService()).PostAsync(httpData).ConfigureAwait(false);
                    result.ErrCode = httpData.ErrCode == ErrorCode.OK ? httpData.ErrCode : ErrorCode.ErrorWhileSavingRecords;
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            finally
            {
                result.SystemIdentifiers = null;
                result.Settings = null;
                result.Resources = null;
            }
            return result;
        }

        /// <summary>
        /// Send Notification
        /// </summary>
        /// <param name="notificationData">notification Data </param>
        /// <param name="languageID">User's language ID</param>
        /// <param name="organisationID">User's Organisation </param>
        /// <returns>Returns the Page resources & Settings</returns>
        public async Task<BaseDTO> SendNotificationAsync(NotificationDTO notificationData, byte languageID, long organisationID)
        {
            BaseDTO result = new BaseDTO();
            try
            {
                if (organisationID < 1 || notificationData.NotificationMessage == null || languageID < 1 
                    || string.IsNullOrWhiteSpace(notificationData.NotificationMessage.NotificationBody)
                    || string.IsNullOrWhiteSpace(notificationData.NotificationMessage.NotificationTags))
                {
                    result.ErrCode = ErrorCode.InvalidData;
                    return result;
                }
                var settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, languageID, default, default, organisationID, false).ConfigureAwait(false)).Settings;
                string[] notificationSetting = GetSettingValueByKey(settings, SettingsConstants.S_NOTIFICATION_MICRO_SERVICE_KEY).Split(Constants.SYMBOL_PIPE_SEPERATOR);
                notificationData.ForApplication = notificationSetting[0];
                HttpServiceModel<NotificationDTO> httpData = new HttpServiceModel<NotificationDTO>
                {
                    CancellationToken = new CancellationToken(),
                    BaseUrl = new Uri(UrlConstants.MICRO_SERVICE_PATH),
                    PathWithoutBasePath = UrlConstants.SEND_NOTIFICATIONS_MESSAGE_ASYNC,
                    AuthType = AuthorizationType.Basic,
                    ClientIdentifier = notificationSetting[1],
                    ClientSecret = notificationSetting[2],
                    ForApplication = notificationSetting[0],
                    ContentToSend = notificationData
                };
                notificationData.NotificationMessage.HubID = Convert.ToInt32(notificationSetting[3], CultureInfo.InvariantCulture);
                notificationData.NotificationMessage.BadgeCount = 1;
                await new HttpLibService(new HttpService()).PostAsync(httpData).ConfigureAwait(false);
                result.ErrCode = httpData.ErrCode == ErrorCode.OK ? httpData.ErrCode : ErrorCode.ErrorWhileSavingRecords;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return result;
        }

        private bool ValidateNotificationData(byte languageID, long organisationID, string clientID, NotificationDTO notification)
        {
            return organisationID < 1 || languageID < 1 || notification?.NotificationData == null || string.IsNullOrWhiteSpace(notification.NotificationData.DeviceUniqueId) || string.IsNullOrWhiteSpace(notification.NotificationData.DeviceHandle)
                                || string.IsNullOrWhiteSpace(notification.NotificationData.DeviceModel) || string.IsNullOrWhiteSpace(notification.NotificationData.DeviceOS) || string.IsNullOrWhiteSpace(clientID);
        }
    }
}
