using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class PushNotificationService : BaseService
    {
        public PushNotificationService(IEssentials essentials) : base(essentials)
        {
            
        }
        /// <summary>
        /// Registers device with Notification Server for push notification ( Portal)
        /// </summary>
        /// <param name="deviceHandle">push notification token</param>
        /// <param name="deviceType">Device type eg. Android, Apple, Windows</param>
        /// <param name="notificationType">Notification type such as "GCM, FCM, APN".</param>
        /// <param name="cancellationToken">Task Cancellation.".</param>
        public async Task<BaseDTO> RegisterDeviceAsync(NotificationDTO notificationData, CancellationToken cancellationToken)
        {
            try
            {
                string oldDeviceHandle = _essentials.GetPreferenceValue<string>(StorageConstants.PR_NOTIFICATION_TOKEN_KEY, string.Empty);
                string selectedEnvironment = _essentials.GetPreferenceValue<string>(StorageConstants.PR_SELECTED_ENVIRONMENT_KEY, string.Empty);
                // call register

                var httpData = new HttpServiceModel<NotificationDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.REGISTER_DEVICE_FOR_NOTIFICATION_PATH,
                    ContentToSend = notificationData,
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                notificationData.ErrCode = httpData.ErrCode;
                if (notificationData.ErrCode == ErrorCode.OK)
                {
                    _essentials.SetPreferenceValue(StorageConstants.PR_NOTIFICATION_TAGS_KEY, notificationData.NotificationData.Tags);
                    _essentials.SetPreferenceValue(StorageConstants.PR_NOTIFICATION_TOKEN_CHANGED_KEY, false);
                }
            }
            catch (Exception ex)
            {
                notificationData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
            return notificationData;
        }

        public async Task<bool> ShouldShowNotificationAsync(string target, string identifier)
        {
            if (_essentials.GetPreferenceValue(StorageConstants.PR_IS_NOTIFICATIONS_ALLOWED_KEY, true))
            {
                if (target == TemplateName.ENewChat.ToString())
                {
                    return await new ChatService(_essentials).IsChatUnread(identifier);
                }
                return true;
            }
            return false;
        }
    }
}
