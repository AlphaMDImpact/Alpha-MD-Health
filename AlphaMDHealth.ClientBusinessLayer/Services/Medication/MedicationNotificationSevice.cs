using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer
{
    /// <summary>
    /// Medication service 
    /// </summary>
    public partial class MedicationSevice : BaseService
    {
        public MedicationSevice(IEssentials essentials) : base(essentials)
        {
            
        }
        /// <summary>
        /// Registers patient medications notifications
        /// </summary>
        /// <returns></returns>
        public async Task RegisterNotificationAsync()
        {
            try
            {
                PatientMedicationDTO medicationData = new PatientMedicationDTO
                {
                    LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0)
                };
                // Get Notifications data from DB
                await new MedicationDatabase().GetNotificationsToRegisterUnRegisterAsync(medicationData).ConfigureAwait(false);
                if (GenericMethods.IsListNotEmpty(medicationData.Notifications))
                {
                    await GetResourcesAndSettingsAsync().ConfigureAwait(false);
                    //todo:
                    //INotificationService notificationService = DependencyService.Get<INotificationService>();
                    //////notificationService.CancelAll();

                    //var notificationActions = new List<string> { LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_SNOOZE_TEXT_KEY), LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_IGNORE_TEXT_KEY), LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DONE_TEXT_KEY) };
                    //string title = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_MEDICATION_NOTIFICATION_TITLE_KEY);
                    //string snoozeTimeOut = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_SNOOZE_TIMEOUT_KEY);
                    //foreach (var notification in medicationData.Notifications)
                    //{
                    //    notificationService.Cancel(notification.NotificationID.Value);
                    //    if (notification.IsActive)
                    //    {
                    //        var medication = medicationData.Medications.FirstOrDefault(x => x.PatientMedicationID == notification.RecordGuidID);
                    //        notificationService.Show(CreateNotificationInstance(
                    //            notification.NotificationID.Value, notification.ShowNotificationDateTime,
                    //            title.Replace(Constants.FORMATTED_STRING_PARAMETER_CONSTANT_ZERO, medication.ShortName),
                    //            $"{medication.Doses} {medication.AddedByID}, {LibGenericMethods.GetLocalDateTimeBasedOnCulture(notification.ShowNotificationDateTime, DateTimeType.Time, string.Empty, string.Empty, string.Empty)}",
                    //            notificationActions, LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_SNOOZE_TIMEOUT_KEY),
                    //            Pages.PatientMedicationsPage.ToString(), Convert.ToString(notification.RecordGuidID, CultureInfo.InvariantCulture),
                    //            Convert.ToString(medication.PatientID, CultureInfo.InvariantCulture)));
                    //    }
                    //    notification.IsSynced = true;
                    //}
                }
                await new MedicationDatabase().UpdateNotificationsRegisterUnRegisterStatusAsync(medicationData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
            }
        }

        private void GenerateNotificationData(PatientMedicationDTO medicationData)
        {
            try
            {
                if (GenericMethods.IsListNotEmpty(medicationData.Reminders) && GenericMethods.IsListNotEmpty(medicationData.Medications))
                {
                    medicationData.Notifications = new List<LocalNotificationModel>();
                    foreach (var medication in medicationData.Medications)
                    {
                        if (medication.IsActive)
                        {
                            int daystoincrement = GetDaysToIncrement(medication.HowOften, medication.AfterDays);
                            if (daystoincrement != 0)
                            {
                                int noofdays = (medication.EndDate - medication.StartDate).Value.Days + 1;
                                int noOfNotifications = daystoincrement > noofdays ? 1 : noofdays / daystoincrement;
                                int initialinc = 0;
                                while (noOfNotifications > 0)
                                {
                                    foreach (var item in medicationData.Reminders.Where(x => x.PatientMedicationID == medication.PatientMedicationID && x.IsActive))
                                    {
                                        medicationData.Notifications.Add(new LocalNotificationModel
                                        {
                                            RecordGuidID = medication.PatientMedicationID,
                                            ShowNotificationDateTime = GetNotificationDateTime(_essentials.ConvertToLocalTime(medication.StartDate.Value), initialinc, _essentials.ConvertToLocalTime(item.ReminderDateTime)),
                                            IsActive = item.IsActive,
                                            IsSynced = false
                                        });
                                    }
                                    initialinc += daystoincrement;
                                    noOfNotifications -= 1;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
            }
        }

        private DateTime GetNotificationDateTime(DateTimeOffset startDate, int i, DateTimeOffset remindertime)
        {
            var a = new DateTime(startDate.AddDays(i).Year, startDate.AddDays(i).Month, startDate.AddDays(i).Day, remindertime.Hour, remindertime.Minute, remindertime.Second).ToUniversalTime();
            return a;
        }

        private int GetDaysToIncrement(int id, byte daysAfter)
        {
            switch (LibResources.GetResourceByKeyID(PageData?.Resources, id).ResourceKey)
            {
                case "DailyKey":
                    return 1;
                case "WeeklyKey":
                    return 7;
                case "MonthlyKey":
                    return 30;
                case "AlternateForKey":
                    return daysAfter;
                default:
                    return 1;
            }
        }

        private NotificationRequest CreateNotificationInstance(int notificationId, DateTimeOffset dateTime, string title, string description, List<string> buttonResources, string snoozemilliSeconds, string targetPage, params string[] parameters)
        {
            var list = new List<string>
            {
                //0, AppointmentListMenuID
                targetPage,
                notificationId.ToString(CultureInfo.InvariantCulture)
            };
            NotificationRequest notificationRequest = new NotificationRequest
            {
                NotificationId = notificationId
            };
            notificationRequest.Title = title;
            notificationRequest.Description = description;
            notificationRequest.IsSnoozeRequest = notificationId < 0;
            //2, Snooze Operation --3
            list.Add(notificationRequest.IsSnoozeRequest.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture));
            var buttons = new List<string>
            {
                buttonResources[0],
                buttonResources[1],
                buttonResources[2]
            };
            var serializer = new ObjectSerializer<List<string>>();
            //3, Notification Buttons --4
            list.Add(serializer.SerializeObject(buttons));
            list.Add(title);
            list.Add(description);
            list.Add(snoozemilliSeconds);
            //7, Appointmnet ID --2
            if (parameters.Length > 0)
            {
                list.AddRange(parameters);
            }
            notificationRequest.ReturningData = serializer.SerializeObject(list);
            notificationRequest.NotifyTime = _essentials.ConvertToLocalTime(dateTime).DateTime;
            return notificationRequest;
        }
    }
}