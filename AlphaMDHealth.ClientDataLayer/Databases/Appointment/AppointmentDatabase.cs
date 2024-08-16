using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;
using System.Globalization;

namespace AlphaMDHealth.ClientDataLayer
{
    public class AppointmentDatabase : BaseDatabase
    {
        /// <summary>
        /// Get Appointments from database
        /// </summary>
        /// <param name="appointmentData">Reference object to return Appointment records</param>
        /// <returns>Appointments in reference object</returns>
        public async Task GetAppointmentsAsync(AppointmentDTO appointmentData)
        {
            string condition = appointmentData.SelectedUserID > 0
                ? $"AND C.ParticipantID = {appointmentData.SelectedUserID}"
                : $"OR C.ParticipantID = {Convert.ToInt64(appointmentData.ErrorDescription, CultureInfo.InvariantCulture)}";

            appointmentData.Appointments = appointmentData.RecordCount > 0
                ? await SqlConnection.QueryAsync<AppointmentModel>
                    ($"SELECT DISTINCT A.* ,B.AppointmentHeader AS PageHeading, A.AppointmentTypeID, B.AppointmentInfo AS PageData FROM AppointmentModel A " +
                    $"JOIN ParticipantsModel C ON C.AppointmentID = A.AppointmentID {condition} AND C.IsActive = 1 " +
                    $"JOIN AppointmentDetailModel B ON A.AppointmentID = B.AppointmentID AND B.LanguageID = ? " +
                    $"WHERE A.IsActive = 1 AND A.ToDateTime > ? AND A.AppointmentStatusID IN ('{ResourceConstants.R_NEW_STATUS_KEY}','{ResourceConstants.R_INPROGRESS_STATUS_KEY}','{ResourceConstants.R_COMPLETED_STATUS_KEY}')" +
                    $"ORDER BY A.FromDateTime ASC LIMIT ?", appointmentData.LanguageID, DateTime.Now.ToUniversalTime(), appointmentData.RecordCount).ConfigureAwait(false)
                : await SqlConnection.QueryAsync<AppointmentModel>
                    ($"SELECT DISTINCT A.* ,B.AppointmentHeader AS PageHeading, A.AppointmentTypeID, B.AppointmentInfo AS PageData FROM AppointmentModel A " +
                    $"JOIN ParticipantsModel C ON C.AppointmentID = A.AppointmentID {condition} AND C.IsActive = 1 " +
                    $"JOIN AppointmentDetailModel B ON A.AppointmentID = B.AppointmentID AND B.LanguageID = ? " +
                    $"WHERE A.IsActive = 1 " +
                    $"ORDER BY A.FromDateTime ASC", appointmentData.LanguageID).ConfigureAwait(false);
            appointmentData.ErrorDescription = string.Empty;
            //string condition = appointmentData.SelectedUserID > 0
            //    ? $"C.ParticipantID = {appointmentData.SelectedUserID}"
            //    : $"C.AccountID = {appointmentData.AccountID} AND C.ParticipantID = {Convert.ToInt64(appointmentData.ErrorDescription, CultureInfo.InvariantCulture)}";
            //appointmentData.Appointments = appointmentData.RecordCount > 0
            //    ? await CommonDataBase.SqlConnection.QueryAsync<AppointmentModel>
            //        ($"SELECT DISTINCT A.* ,B.AppointmentHeader AS PageHeading, B.AppointmentInfo AS PageData FROM AppointmentModel A " +
            //        $"JOIN ParticipantsModel C ON C.AppointmentID = A.AppointmentID OR {condition} AND C.IsActive = 1 " +
            //        $"JOIN AppointmentDetailModel B ON A.AppointmentID = B.AppointmentID AND B.LanguageID = ? " +
            //        $"WHERE A.IsActive = 1 AND A.ToDateTime > ? AND A.AppointmentStatusID IN ('{ResourceConstants.R_NEW_STATUS_KEY}','{ResourceConstants.R_INPROGRESS_STATUS_KEY}','{ResourceConstants.R_COMPLETED_STATUS_KEY}')" +
            //        $"ORDER BY A.FromDateTime ASC LIMIT ?", appointmentData.LanguageID, DateTime.Now.ToUniversalTime(), appointmentData.RecordCount).ConfigureAwait(false)
            //    : await CommonDataBase.SqlConnection.QueryAsync<AppointmentModel>
            //        ($"SELECT DISTINCT A.* ,B.AppointmentHeader AS PageHeading, B.AppointmentInfo AS PageData FROM AppointmentModel A " +
            //        $"JOIN ParticipantsModel C ON C.AppointmentID = A.AppointmentID OR {condition} AND C.IsActive = 1 " +
            //        $"JOIN AppointmentDetailModel B ON A.AppointmentID = B.AppointmentID AND B.LanguageID = ? " +
            //        $"WHERE A.IsActive = 1 " +
            //        $"ORDER BY A.FromDateTime ASC", appointmentData.LanguageID).ConfigureAwait(false);
            //appointmentData.ErrorDescription = string.Empty;
        }

        /// <summary>
        /// Get Appointment from database based on AppointmentID
        /// </summary>
        /// <param name="appointmentData">Reference object to return Appointment records</param>
        /// <returns>Appointment in reference object</returns>
        public async Task GetAppointmentAsync(AppointmentDTO appointmentData)
        {
            appointmentData.Appointment = await SqlConnection.FindWithQueryAsync<AppointmentModel>
                ("SELECT A.* , B.AppointmentHeader AS PageHeading, B.AppointmentInfo AS PageData FROM AppointmentModel A " +
                "JOIN AppointmentDetailModel B ON A.AppointmentID = B.AppointmentID AND B.LanguageID = ? AND A.AppointmentID = ? WHERE A.IsActive = 1",
                appointmentData.LanguageID, appointmentData.Appointment.AppointmentID).ConfigureAwait(false);

            // 3300,3043,
            //appointmentData.AppointmentParticipants = await CommonDataBase.SqlConnection.QueryAsync<ParticipantsModel>
            //     ($"SELECT A.ParticipantID, A.AppointmentStatusID, B.FirstName, B.LastName, B.ImageName, C.Profession, B.AccountID, B.ImageBase64 " +
            //     $"FROM ParticipantsModel A " +
            //     $"JOIN UserModel B ON B.UserID = A.ParticipantID " +
            //     $"LEFT JOIN UserProfessionModel C ON B.ProffessionID = C.ProfessionID " +
            //     "WHERE A.AppointmentID = ? AND A.IsActive = 1 ",
            //     $"WHERE A.AppointmentID = ? AND A.IsActive = 1 " +
            //     $"UNION SELECT 0 As ParticipantID, AppointmentStatusID, FirstName, '' As LastName, '' As ImageName, '' As Profession, " +
            //     $" 0 As AccountID, '' As ImageBase64 from ParticipantsModel WHERE AppointmentID = {appointmentData.Appointment.AppointmentID} AND IsActive = {1} AND ParticipantID = 0 " +
            //     $"ORDER BY B.FirstName COLLATE NOCASE ASC, B.LastName COLLATE NOCASE ASC",
            //     appointmentData.Appointment.AppointmentID).ConfigureAwait(false);
            appointmentData.AppointmentParticipants = await SqlConnection.QueryAsync<ParticipantsModel>
                ($"SELECT A.ParticipantID, A.AppointmentStatusID, B.FirstName, B.LastName, B.ImageName, C.Profession, B.AccountID, B.ImageBase64 " +
                $"FROM ParticipantsModel A JOIN UserModel B ON A.ParticipantID = B.UserID " +
                $"LEFT JOIN UserProfessionModel C ON B.ProffessionID = C.ProfessionID " +
                $"WHERE A.AppointmentID = ? AND A.IsActive = 1 " +
                $"UNION SELECT 0 As ParticipantID, AppointmentStatusID, FirstName, '' As LastName, '' As ImageName, '' As Profession, " +
                $" 0 As AccountID, '' As ImageBase64 from ParticipantsModel WHERE AppointmentID = {appointmentData.Appointment.AppointmentID} AND IsActive = {1} AND ParticipantID = 0 " +
                $"ORDER BY B.FirstName COLLATE NOCASE ASC, B.LastName COLLATE NOCASE ASC",
                appointmentData.Appointment.AppointmentID).ConfigureAwait(false);

            appointmentData.ExternalParticipant = await SqlConnection.FindWithQueryAsync<ParticipantsModel>
               ("SELECT ParticipantID,AppointmentID,IsActive, AppointmentStatusID, FirstName, MobileNo, EmailID " +
               "FROM ParticipantsModel WHERE AppointmentID = ? AND IsActive = 1 AND ParticipantID = 0 ",
               appointmentData.Appointment.AppointmentID, 0).ConfigureAwait(false);
        }


        /// <summary>
        /// Get AppointmentImage Image Status from database
        /// </summary>
        /// <param name="appointmentData">Reference object to return Appointment records</param>
        /// <returns>Appointments in reference object</returns>
        public async Task GetAppointmentImageStatusAsync(AppointmentDTO appointmentData)
        {
            appointmentData.AppointmentParticipants = await SqlConnection.QueryAsync<ParticipantsModel>
                ("SELECT ParticipantID, ImageName FROM ParticipantsModel WHERE  IsDataDownloaded = 0 ").ConfigureAwait(false);
        }

        /// <summary>
        /// Save Appointment , Apppintment details and partcipants to database
        /// </summary>
        /// <param name="appointmentDTO">Appointments data to be saved</param>
        /// <returns>Operation result</returns>
        public async Task SaveAppointmentsDataAsync(AppointmentDTO appointmentDTO)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (appointmentDTO.Appointment != null && appointmentDTO.Appointment.AppointmentTypeID > 0)
                {
                    transaction.InsertOrReplace(appointmentDTO.Appointment);
                }
                if (GenericMethods.IsListNotEmpty(appointmentDTO.Appointments))
                {
                    foreach (AppointmentModel appointment in appointmentDTO.Appointments)
                    {
                        transaction.InsertOrReplace(appointment);
                    }
                }
                SaveAppointmentDetails(appointmentDTO, transaction);
                SaveParticipantsData(appointmentDTO, transaction);
                SaveExternalParticipants(appointmentDTO, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Save Appointment , Apppintment details and partcipants to database
        /// </summary>
        /// <param name="appointmentDTO">Appointments data to be saved</param>
        /// <returns>Operation result</returns>
        public async Task SaveAppointmentDetailsAsync(AppointmentDTO appointmentDTO)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveAppointmentDetails(appointmentDTO, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Save Appointment Status
        /// </summary>
        /// <param name="appointment">Object containing appointment status</param>
        /// <returns>Operation result</returns>
        public async Task UpdateAppointmentStatusAsync(AppointmentDTO appointment)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (appointment.Appointment.AppointmentStatusID == ResourceConstants.R_CANCELLED_STATUS_KEY)
                {
                    transaction.Execute("UPDATE AppointmentModel SET AppointmentStatusID = ? ,IsActive = 0 WHERE AppointmentID = ?"
                        , appointment.Appointment.AppointmentStatusID, appointment.Appointment.AppointmentID);
                    transaction.Execute("UPDATE ParticipantsModel SET AppointmentStatusID = ? ,IsActive = 0 WHERE AppointmentID = ?"
                        , appointment.Appointment.AppointmentStatusID, appointment.Appointment.AppointmentID);
                    transaction.Execute("UPDATE AppointmentDetailModel SET IsActive = 0 WHERE AppointmentID = ?"
                        , appointment.Appointment.AppointmentID);
                }
                else
                {
                    transaction.Execute("UPDATE ParticipantsModel SET AppointmentStatusID = ? WHERE ParticipantID = ? AND AppointmentID = ?"
                        , appointment.Appointment.AppointmentStatusID, appointment.SelectedUserID, appointment.Appointment.AppointmentID);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update Appointment page Image and status
        /// </summary>
        /// <param name="appointmentData">Reference object to return Appointment records</param>
        /// <returns>operation status</returns>
        public async Task UpdateAppointmentSyncImageStatusAsync(AppointmentDTO appointmentData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(appointmentData.AppointmentParticipants))
                {
                    foreach (var participant in appointmentData.AppointmentParticipants)
                    {
                        transaction.Execute("UPDATE ParticipantsModel SET IsDataDownloaded = 1, ImageName = ?  WHERE ParticipantID = ?", participant.ImageName, participant.ParticipantID);
                    }
                }
            }).ConfigureAwait(false);
        }

        private void SaveAppointmentDetails(AppointmentDTO appointmentDTO, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(appointmentDTO.AppointmentsDetails))
            {
                foreach (AppointmentDetailModel appointmentDetail in appointmentDTO.AppointmentsDetails)
                {
                    if (transaction.FindWithQuery<AppointmentDetailModel>("SELECT 1 FROM AppointmentDetailModel WHERE LanguageID = ? AND AppointmentID = ?", appointmentDetail.LanguageID, appointmentDetail.AppointmentID) == null)
                    {
                        transaction.Execute("INSERT INTO AppointmentDetailModel (AppointmentID, LanguageID, AppointmentHeader, AppointmentInfo, IsActive) VALUES (?, ?, ?, ?, ?)",
                            appointmentDetail.AppointmentID, appointmentDetail.LanguageID, appointmentDetail.AppointmentHeader, appointmentDetail.AppointmentInfo, appointmentDetail.IsActive);
                    }
                    else
                    {
                        transaction.Execute("UPDATE AppointmentDetailModel SET AppointmentHeader = ?, AppointmentInfo = ?, IsActive = ? WHERE AppointmentID = ? AND LanguageID = ?",
                             appointmentDetail.AppointmentHeader, appointmentDetail.AppointmentInfo, appointmentDetail.IsActive, appointmentDetail.AppointmentID, appointmentDetail.LanguageID);
                    }
                }
            }
        }

        private void SaveParticipantsData(AppointmentDTO appointmentDTO, SQLite.SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(appointmentDTO.AppointmentParticipants))
            {
                foreach (ParticipantsModel selectedParticipants in appointmentDTO.AppointmentParticipants)
                {
                    if (transaction.FindWithQuery<ParticipantsModel>("SELECT 1 FROM ParticipantsModel WHERE ParticipantID = ? AND AppointmentID = ?", selectedParticipants.ParticipantID, selectedParticipants.AppointmentID) == null)
                    {
                        transaction.Execute("INSERT INTO ParticipantsModel (ParticipantID, AccountID, AppointmentID, AppointmentStatusID, IsActive) VALUES (?, ?, ?, ?, ?)",
                            selectedParticipants.ParticipantID, selectedParticipants.AccountID, selectedParticipants.AppointmentID, selectedParticipants.AppointmentStatusID, selectedParticipants.IsActive);
                    }
                    else
                    {
                        transaction.Execute("UPDATE ParticipantsModel SET AppointmentStatusID = ?, IsActive = ? WHERE ParticipantID = ? AND AppointmentID = ?", selectedParticipants.AppointmentStatusID, selectedParticipants.IsActive, selectedParticipants.ParticipantID, selectedParticipants.AppointmentID);
                    }
                }
            }
        }

        private void SaveExternalParticipants(AppointmentDTO appointmentDTO, SQLite.SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(appointmentDTO.ExternalParticipants))
            {
                foreach (ParticipantsModel selectedParticipants in appointmentDTO.ExternalParticipants)
                {
                    if (transaction.FindWithQuery<ParticipantsModel>("SELECT 1 FROM ParticipantsModel WHERE ParticipantID = ? AND AppointmentID = ?", selectedParticipants.ParticipantID, selectedParticipants.AppointmentID) == null)
                    {
                        transaction.Execute("INSERT INTO ParticipantsModel (ParticipantID, AccountID,FirstName, MobileNo, EmailID, AppointmentID, AppointmentStatusID, IsActive) " +
                            "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                            selectedParticipants.ParticipantID, selectedParticipants.AccountID, selectedParticipants.FirstName, selectedParticipants.MobileNo,
                            selectedParticipants.EmailID, selectedParticipants.AppointmentID, selectedParticipants.AppointmentStatusID, selectedParticipants.IsActive);
                    }
                    else
                    {
                        transaction.Execute("UPDATE ParticipantsModel SET AppointmentStatusID = ?, FirstName = ? , IsActive = ? WHERE ParticipantID = ? AND AppointmentID = ?",
                            selectedParticipants.AppointmentStatusID, selectedParticipants.FirstName, selectedParticipants.IsActive, selectedParticipants.ParticipantID, selectedParticipants.AppointmentID);
                    }
                }
            }
        }
        /// <summary>
        /// Get appointment data for generating notification
        /// </summary>
        /// <param name="appointmentData"></param>
        /// <returns>Appoints whose notification has to be set in DTO object</returns>
        public async Task GetDataForNotification(AppointmentDTO appointmentData)
        {
            string commaSeperatedString = string.Empty;
            if(appointmentData.Appointment?.AppointmentID > 0)
            {
                commaSeperatedString = appointmentData.Appointment.AppointmentID.ToString();
                appointmentData.AccountID = appointmentData.Appointment.AccountID;
            }
            else
            {
                appointmentData.AppointmentParticipants = await SqlConnection.QueryAsync<ParticipantsModel>
                ($"SELECT * FROM ParticipantsModel WHERE AccountID = ? AND AppointmentStatusID = ?", appointmentData.AccountID, ResourceConstants.R_ACCEPTED_STATUS_KEY);
                if (GenericMethods.IsListNotEmpty(appointmentData.AppointmentParticipants))
                {
                    commaSeperatedString = string.Join(",", appointmentData.AppointmentParticipants.Select(x => x.AppointmentID));
                }
            }
            if (!string.IsNullOrWhiteSpace(commaSeperatedString))
            {
                appointmentData.Appointments = await SqlConnection.QueryAsync<AppointmentModel>($"SELECT * FROM AppointmentModel WHERE AppointmentID IN ({commaSeperatedString}) AND FromDateTime > {GenericMethods.GetUtcDateTime.AddMinutes(15).Ticks}");
            }
        }

        /// <summary>
        /// Save Notification Data
        /// </summary>
        /// <param name="appointmentData"></param>
        /// <returns>Operation status</returns>
        public async Task SaveNotificationData(AppointmentDTO appointmentData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (var notification in appointmentData.Notifications)
                {
                    var existingNotification = transaction.FindWithQuery<LocalNotificationModel>
                        ("SELECT * FROM LocalNotificationModel WHERE RecordID = ?", notification.RecordID);
                    if (existingNotification == null)
                    {
                        if (notification.IsActive || !notification.IsSynced)
                        {
                            transaction.Execute("INSERT INTO LocalNotificationModel (RecordID, ShowNotificationDateTime, IsActive, IsSynced) VALUES (?, ?, ?, ?)",
                               notification.RecordID, notification.ShowNotificationDateTime, notification.IsActive, notification.IsSynced);

                            //LibGenericMethods.LogData($"--!!--SaveNotifications() SAVE AppointmentId:{notification.RecordID}, ReminderDateTime:{notification.ShowNotificationDateTime}, IsActive:{notification.IsActive}, IsSynced:{notification.IsSynced}--!!--");
                        }
                    }
                    else if (!notification.IsActive && notification.IsSynced)
                    {
                        transaction.Execute($"DELETE FROM LocalNotificationModel WHERE NotificationID = ?", existingNotification.NotificationID);
                        //LibGenericMethods.LogData($"--!!--SaveNotifications() DELETE NotificationID:{notification.NotificationID} AppointmentId:{notification.RecordID}, ReminderDateTime:{notification.ShowNotificationDateTime}, IsActive:{notification.IsActive}, IsSynced:{notification.IsSynced}--!!--");
                    }
                    else
                    {
                        transaction.Execute($"UPDATE LocalNotificationModel SET IsActive = ?, IsSynced = ?, ShowNotificationDateTime = ? WHERE NotificationID = ?",
                           notification.IsActive, notification.IsSynced, notification.ShowNotificationDateTime, existingNotification.NotificationID);
                        //LibGenericMethods.LogData($"--!!--SaveNotifications() UPDATE NotificationID:{notification.NotificationID} AppointmentId:{notification.RecordID}, ReminderDateTime:{notification.ShowNotificationDateTime}, IsActive:{notification.IsActive}, IsSynced:{notification.IsSynced}--!!--");
                    }
                }
            });
        }

        /// <summary>
        /// Get Notification data whose notification has to be set
        /// </summary>
        /// <param name="appointmentData"></param>
        /// <returns>Notificatio data</returns>
        public async Task GetNotificationData(AppointmentDTO appointmentData)
        {
            string condition = appointmentData.Appointment?.AppointmentID > 0 ? $"AND RecordID = '{appointmentData.Appointment.AppointmentID}'" : $"AND IsActive = 1 AND  ShowNotificationDateTime < {GenericMethods.GetUtcDateTime.AddDays(3).Ticks}";
            appointmentData.Notifications = await SqlConnection.QueryAsync<LocalNotificationModel>($"SELECT * FROM LocalNotificationModel WHERE RecordID <> '' {condition}");
            if (GenericMethods.IsListNotEmpty(appointmentData.Notifications))
            {
                string commaSeperatedValue = string.Join(",", appointmentData.Notifications.Select(x => x.RecordID));
                appointmentData.Appointments = await SqlConnection.QueryAsync<AppointmentModel>
                ($"SELECT * FROM AppointmentModel WHERE AppointmentID IN ({commaSeperatedValue})");
            }
        }

        /// <summary>
        /// Get Notification based on appointment IDs
        /// </summary>
        /// <param name="appointmentData"></param>
        /// <param name="appointmentIds"></param>
        /// <returns>IDs of notification in DTO object</returns>
        public async Task GetNotificationDataBasedOnAppointmentIDs(AppointmentDTO appointmentData, string appointmentIds)
        {
            appointmentData.Notifications = await SqlConnection.QueryAsync<LocalNotificationModel>($"SELECT * FROM LocalNotificationModel WHERE RecordID IN  ('{appointmentIds}')");
        }
    }
}