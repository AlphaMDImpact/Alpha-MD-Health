using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public class MedicationDatabase : BaseDatabase
    {
        /// <summary>
        /// Get list of patient medications
        /// </summary>
        /// <param name="medicationData">Reference object to return patient medication data</param>
        /// <param name="selectedTabKey">Selected tab name to retrieve the particular list</param>
        /// <returns>Operation status with patient medications in reference object</returns>
        public async Task GetPatientMedicationsAsync(PatientMedicationDTO medicationData, string selectedTabKey)
        {
            string conditionalColumns;
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            if (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
            {
                conditionalColumns = ", ifnull(PP.ProgramGroupIdentifier , P.ProgramGroupIdentifier) AS ProgramColor";
                if (roleID == (int)RoleName.CareTaker)
                {
                    conditionalColumns = ", A.ProgramID, ifnull(PP.ProgramGroupIdentifier , P.ProgramGroupIdentifier) AS ProgramColor";
                }
            }
            else
            {
                conditionalColumns = ", P.ProgramGroupIdentifier AS ProgramColor";
            }
            medicationData.Medications = await SqlConnection.QueryAsync<PatientMedicationModel>(
                "SELECT DISTINCT A.PatientMedicationID, P.Name AS ProgramName, A.Doses, A.FullName, A.ShortName, " +
                    "A.UnitIdentifier, A.StartDate, A.EndDate,A.IsCritical,A.LastModifiedByID, A.Reminder, A.ErrCode" +
                    $" {conditionalColumns} " +
                "FROM PatientMedicationModel A " +
                "LEFT JOIN ProgramModel P ON A.ProgramID = P.ProgramID AND P.IsActive = 1 " +
                "LEFT JOIN PatientProgramModel PP ON PP.ProgramID = P.ProgramID AND PP.PatientID = ? " +
                $"WHERE A.PatientID = ? AND (A.IsActive = 1 OR (A.ProgramID > 0 AND (P.IsActive = 1 AND PP.IsActive = 1 AND A.IsActive = 1))) "
                , medicationData.SelectedUserID, medicationData.SelectedUserID
            ).ConfigureAwait(false);

            if (roleID == (int)RoleName.CareTaker && medicationData.Medications?.Count > 0)
            {
                List<PatientProgramModel> sharedPrograms = await GetActiveSharedProgramsAsync(medicationData).ConfigureAwait(false);
                var medicationlist = await SqlConnection.QueryAsync<PatientMedicationModel>(
                     "SELECT DISTINCT A.PatientMedicationID, P.Name AS ProgramName, A.Doses, A.FullName, A.ShortName, " +
                    "A.UnitIdentifier, A.StartDate, A.EndDate,A.IsCritical,A.LastModifiedByID, A.Reminder, A.ErrCode , A.AddedByID" +
                    $" {conditionalColumns} " +
                "FROM PatientMedicationModel A " +
                "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID AND P.IsActive = 1 " +
                "LEFT JOIN PatientProgramModel PP ON PP.ProgramID = P.ProgramID " +
                $"WHERE A.PatientID = ? AND A.IsActive = 1 "
                , medicationData.SelectedUserID
                    ).ConfigureAwait(false);
                medicationData.Medications = sharedPrograms?.Count > 0
                    ? medicationlist.Where(x => sharedPrograms.Any(y => y.ProgramID == x.ProgramID) || x.AddedByID == medicationData.AddedBy)?.Distinct()?.ToList()
                    : null;
                foreach (var program in sharedPrograms)
                {
                    var programCaregiverData = await SqlConnection.QueryAsync<CaregiverModel>(
                   "SELECT A.PatientCareGiverID, B.AccountID, A.CareGiverID, A.FromDate, A.ToDate, B.RoleAtLevelID AS OrganisationID" +
                       ", A.ProgramID, P.Name AS ProgramName " +
                   "FROM UserRelationModel A " +
                   "JOIN UserModel B ON A.CareGiverID = B.UserID " +
                   "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID " +
                   "WHERE A.ProgramID = ? AND A.IsActive = 1 AND A.PatientID = ? ", program.ProgramID, medicationData.SelectedUserID
                    ).ConfigureAwait(false);
                    foreach (var programcaregiver in programCaregiverData)
                    {
                        var list = medicationlist.Where(x => Convert.ToInt64(x.AddedByID) == programcaregiver.CareGiverID).ToList();
                        medicationData.Medications.AddRange(list);
                    }
                }
            }

            if (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
            {
                medicationData.Medications = selectedTabKey == ResourceConstants.R_OPEN_TASK_KEY
                    ? medicationData.Medications?.Where(x => x.EndDate.Value.Ticks >= DateTimeOffset.Now.Date.ToUniversalTime().Ticks)?.OrderBy(x => x.EndDate).ToList()
                    : medicationData.Medications?.Where(x => x.EndDate.Value.Ticks < DateTimeOffset.Now.Date.ToUniversalTime().Ticks)?.OrderByDescending(x => x.EndDate).ToList();
            }

            if (medicationData.RecordCount > 0)
            {
                medicationData.Medications = medicationData.Medications?.Take((int)medicationData.RecordCount)?.ToList();
            }

            medicationData.UnitOptions = await SqlConnection.QueryAsync<OptionModel>(
                "SELECT A.UnitGroupID AS ParentOptionID, A.UnitID AS OptionID, A.UnitIdentifier AS GroupName, B.ShortUnitName AS OptionText, B.FullUnitName AS ParentOptionText " +
                "FROM UnitModel A " +
                "JOIN SettingModel C ON C.SettingID = A.UnitGroupID AND C.SettingKey = ? " +
                "JOIN UnitI18NModel B ON B.UnitID = A.UnitID AND B.LanguageID = ? AND A.IsActive = 1 AND B.IsActive = 1"
                , SettingsConstants.S_MEDICATION_UNIT_GROUP_KEY, medicationData.LanguageID
            );
        }

        /// <summary>
        /// Get Prescription Data 
        /// </summary>
        /// <param name="medicationData">Reference object to return patient medication data</param>
        /// <returns>Operation status</returns>
        public async Task GetPrescriptionAsync(PatientMedicationDTO medicationData)
        {
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            //Get the existing medicationdata
            medicationData.Medication = await SqlConnection.FindWithQueryAsync<PatientMedicationModel>(
                   "SELECT A.PatientMedicationID, A.ProgramMedicationID, A.ProgramID, P.Name AS ProgramName, A.Doses, A.Frequency, A.FullName, A.ShortName, A.UnitIdentifier, " +
                       "A.HowOften, A.AfterDays, A.StartDate, A.EndDate, A.Note,A.IsCritical, A.Reminder, A.PatientID, A.AdditionalNotes, A.AddedByID, A.AddedOn, A.ErrCode, " +
                       "U.FirstName AS FirstName , U.LastName AS LastName , U.Dob AS Dob , U.GenderID AS Gender " +
                   "FROM PatientMedicationModel A " +
                   "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID " +
                   "JOIN UserModel U ON U.UserID = A.PatientID " +
                   "WHERE A.PatientMedicationID = ? AND A.IsActive = 1"
                   , medicationData.Medication.PatientMedicationID
               ).ConfigureAwait(false);
            long caregiverIDs;
            //Check if data is added from program and get caregiver data
            if (medicationData.Medication.ProgramID > 0)
            {
                var programCaregiverData = await SqlConnection.FindWithQueryAsync<CaregiverModel>(
                    "SELECT A.PatientCareGiverID, B.AccountID, A.CareGiverID, A.FromDate, A.ToDate, B.RoleAtLevelID AS OrganisationID" +
                        ", A.ProgramID, P.Name AS ProgramName " +
                    "FROM UserRelationModel A " +
                    "JOIN UserModel B ON A.CareGiverID = B.UserID " +
                    "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID " +
                    "WHERE A.ProgramID = ? AND A.IsActive = 1 ", medicationData.Medication.ProgramID
                ).ConfigureAwait(false);
                caregiverIDs = programCaregiverData.CareGiverID;
            }
            else
            {
                caregiverIDs = Convert.ToInt64(medicationData.Medication.AddedByID);
            }
            medicationData.Caregiver = await SqlConnection.FindWithQueryAsync<CaregiverModel>(
            "SELECT A.PatientCareGiverID, B.AccountID, A.CareGiverID, A.FromDate, A.ToDate, B.RoleAtLevelID AS OrganisationID, " +
                    "B.FirstName || ' ' || B.LastName AS FirstName, PRO.Profession AS Department, B.UserDegrees AS Degrees, " +
                    "A.ProgramID, P.Name AS ProgramName " +
                "FROM UserRelationModel A " +
                "JOIN UserModel B ON A.CareGiverID = B.UserID " +
                "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID " +
                "LEFT JOIN UserProfessionModel PRO ON B.ProffessionID = PRO.ProfessionID " +
                $"WHERE A.CareGiverID = ? ", caregiverIDs
            ).ConfigureAwait(false);
            //Get all medications list of that user
            var medications = await SqlConnection.QueryAsync<PatientMedicationModel>(
                "SELECT A.PatientMedicationID, A.ProgramMedicationID, A.ProgramID, P.Name AS ProgramName, A.Doses, A.AdditionalNotes, A.Frequency, A.FullName, A.ShortName, A.UnitIdentifier, " +
                       "A.HowOften, A.AfterDays, A.StartDate, A.EndDate, A.Note,A.IsCritical, A.Reminder, A.PatientID, A.AddedByID, A.AddedOn, A.ErrCode, " +
                       "U.FirstName AS FirstName, U.LastName AS LastName, U.Dob AS Dob, U.GenderID AS Gender " +
                   "FROM PatientMedicationModel A " +
                   "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID " +
                   "LEFT JOIN PatientProgramModel PP ON PP.ProgramID = P.ProgramID AND PP.PatientID = ? " +
                   "JOIN UserModel U ON U.UserID = A.PatientID " +
                   $"WHERE A.PatientID = ? AND (A.IsActive = 1 OR (A.ProgramID > 0 AND (P.IsActive = 1 AND PP.IsActive = 1 AND A.IsActive = 1)))"
               , medicationData.SelectedUserID, medicationData.SelectedUserID).ConfigureAwait(false);
            var medicationList = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker ? medications?.Where(x => x.EndDate.Value.Ticks >= DateTimeOffset.Now.Date.ToUniversalTime().Ticks)?.OrderBy(x => x.EndDate).ToList() :
                 medications?.Where(x => x.EndDate.Value.Ticks < DateTimeOffset.Now.Date.ToUniversalTime().Ticks)?.OrderByDescending(x => x.EndDate).ToList();

            ///Get Medication List if LoginUserID is caretaker
            if (roleID == (int)RoleName.CareTaker && medications?.Count > 0)
            {
                List<PatientProgramModel> sharedPrograms = await GetActiveSharedProgramsAsync(medicationData).ConfigureAwait(false);
                var shareMedicationList = sharedPrograms?.Count > 0
                    ? medicationList.Where(x => sharedPrograms.Any(y => y.ProgramID == x.ProgramID) || x.AddedByID == medicationData.AddedBy)?.Distinct()?.ToList()
                    : null;
                var medicationsGroupedByProgram = shareMedicationList.GroupBy(med => med.ProgramID);
                var medicationsWithoutProgram = medicationList.Where(x => x.ProgramID < 1).ToList();

                foreach (var medication in medicationsGroupedByProgram)
                {
                    long programIDs = medication.Key;
                    var programList = await SqlConnection.QueryAsync<PatientMedicationModel>(
                      "SELECT A.PatientMedicationID, A.ProgramMedicationID, A.ProgramID, P.Name AS ProgramName, A.Doses, A.AdditionalNotes, A.Frequency, A.FullName, A.ShortName, A.UnitIdentifier, " +
                           "A.HowOften, A.AfterDays, A.StartDate, A.EndDate, A.Note, A.IsCritical, A.Reminder, A.PatientID, A.AddedByID, A.AddedOn, A.ErrCode, " +
                           "U.FirstName AS FirstName, U.LastName AS LastName, U.Dob AS Dob, U.GenderID AS Gender " +
                       "FROM PatientMedicationModel A " +
                       "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID " +
                       "JOIN UserModel U ON U.UserID = A.PatientID " +
                       $"WHERE A.ProgramID = ? AND  A.PatientID = ? AND A.IsActive = 1"
                          , programIDs, medicationData.SelectedUserID).ConfigureAwait(false);
                    programList = programList?.Where(x => x.EndDate.Value.Ticks >= DateTimeOffset.Now.Date.ToUniversalTime().Ticks)?.OrderBy(x => x.EndDate).ToList();
                    var programCaregiver = await SqlConnection.QueryAsync<CaregiverModel>(
                         "SELECT A.PatientCareGiverID, B.AccountID, A.CareGiverID, A.FromDate, A.ToDate, B.RoleAtLevelID AS OrganisationID" +
                             ", A.ProgramID, P.Name AS ProgramName " +
                         "FROM UserRelationModel A " +
                         "JOIN UserModel B ON A.CareGiverID = B.UserID " +
                         "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID " +
                         "WHERE A.ProgramID = ? AND A.IsActive = 1 AND A.PatientID = ? ", programIDs, medicationData.SelectedUserID
                     ).ConfigureAwait(false);
                    foreach (var caregiver in programCaregiver)
                    {
                        if (caregiverIDs == caregiver?.CareGiverID)
                        {
                            medicationData.Medications.AddRange(programList);
                        }
                    }
                }
                if (medicationsWithoutProgram.Count != 0)
                {
                    var medicationsWithoutProgramGroup = medicationsWithoutProgram.GroupBy(med => med.AddedByID);
                    foreach (var list in medicationsWithoutProgramGroup)
                    {
                        if (Convert.ToInt64(list.Key) == caregiverIDs)
                        {
                            medicationData.Medications.AddRange(list);
                        }
                    }
                }
            }
            else
            {
                var medicationsGroupedByProgram = medicationList.GroupBy(med => med.ProgramID); // Grouping ProgramMedications
                var medicationsWithoutProgram = new List<PatientMedicationModel>();
                foreach (var medicationGroup in medicationsGroupedByProgram)
                {
                    long programIDs = medicationGroup.Key;
                    if (programIDs == 0) // Check for medications without a program ID
                    {
                        medicationsWithoutProgram.AddRange(medicationGroup);
                    }
                    else
                    {
                        var programList = await SqlConnection.QueryAsync<PatientMedicationModel>(
                        "SELECT A.PatientMedicationID, A.ProgramMedicationID, A.ProgramID, P.Name AS ProgramName, A.Doses, A.AdditionalNotes, A.Frequency, A.FullName, A.ShortName, A.UnitIdentifier, " +
                             "A.HowOften, A.AfterDays, A.StartDate, A.EndDate, A.Note, A.IsCritical, A.Reminder, A.PatientID, A.AddedByID, A.AddedOn, A.ErrCode, " +
                             "U.FirstName AS FirstName, U.LastName AS LastName, U.Dob AS Dob, U.GenderID AS Gender " +
                         "FROM PatientMedicationModel A " +
                         "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID " +
                         "JOIN UserModel U ON U.UserID = A.PatientID " +
                         $"WHERE A.ProgramID = ? AND  A.PatientID = ? AND A.IsActive = 1"
                            , programIDs, medicationData.SelectedUserID).ConfigureAwait(false);
                        programList = programList?.Where(x => x.EndDate.Value.Ticks >= DateTimeOffset.Now.Date.ToUniversalTime().Ticks)?.OrderBy(x => x.EndDate).ToList();
                        var programCaregiver = await SqlConnection.FindWithQueryAsync<CaregiverModel>(
                         "SELECT A.PatientCareGiverID, B.AccountID, A.CareGiverID, A.FromDate, A.ToDate, B.RoleAtLevelID AS OrganisationID" +
                             ", A.ProgramID, P.Name AS ProgramName " +
                         "FROM UserRelationModel A " +
                         "JOIN UserModel B ON A.CareGiverID = B.UserID " +
                         "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID " +
                         "WHERE A.ProgramID = ? AND A.IsActive = 1 ", programIDs
                     ).ConfigureAwait(false);

                        if (caregiverIDs == programCaregiver?.CareGiverID)
                        {
                            medicationData.Medications.AddRange(programList);
                        }
                    }
                }
                if (medicationsWithoutProgram.Count != 0)
                {
                    var medicationsWithoutProgramGroup = medicationsWithoutProgram.GroupBy(med => med.AddedByID);
                    foreach (var list in medicationsWithoutProgramGroup)
                    {
                        if (Convert.ToInt64(list.Key) == caregiverIDs)
                        {
                            medicationData.Medications.AddRange(list);
                        }
                    }
                }
            }
            ///Get OrganisationData
            medicationData.Organisation = await SqlConnection.FindWithQueryAsync<OrganisationModel>(
                "SELECT * FROM OrganisationModel WHERE OrganisationID = ?", medicationData.OrganisationID
                ).ConfigureAwait(false);
            byte languageID = (byte)medicationData.LanguageID;
            ContactDTO contactData = new ContactDTO { LanguageID = languageID };
            await new ContactDatabase().GetOrganisationContactsAsync(contactData);
            if (contactData?.Contacts?.Count > 0)
            {
                var OrganisationAddress = contactData.Contacts.Find(x => x.ContactTypeID == 183);
                if (OrganisationAddress != null)
                {
                    medicationData.Medication.OrganisationAddress = OrganisationAddress.ContactValue;
                }
                var OrganisationContact = contactData.Contacts.Find(x => x.ContactTypeID == 185);
                if (OrganisationContact != null)
                {
                    medicationData.Medication.OrganisationContact = OrganisationContact.ContactValue;
                }
            }
            medicationData.UnitOptions = await SqlConnection.QueryAsync<OptionModel>(
               "SELECT A.UnitGroupID AS ParentOptionID, A.UnitID AS OptionID, A.UnitIdentifier AS GroupName, B.ShortUnitName AS OptionText, B.FullUnitName AS ParentOptionText " +
               "FROM UnitModel A " +
               "JOIN SettingModel C ON C.SettingID = A.UnitGroupID AND C.SettingKey = ? " +
               "JOIN UnitI18NModel B ON B.UnitID = A.UnitID AND B.LanguageID = ? AND A.IsActive = 1 AND B.IsActive = 1"
               , SettingsConstants.S_MEDICATION_UNIT_GROUP_KEY, medicationData.LanguageID
           );
        }

        /// <summary>
        /// Get medication details bases on PatientMedicationID
        /// </summary>
        /// <param name="medicationData">Reference object to return patient medication data</param>
        /// <returns>Operation status with medication detail in reference object</returns>
        public async Task GetPatientMedicationAsync(PatientMedicationDTO medicationData)
        {
            if (medicationData.Medication.PatientMedicationID != Guid.Empty)
            {
                medicationData.Medication = await SqlConnection.FindWithQueryAsync<PatientMedicationModel>(
                    "SELECT A.PatientMedicationID, A.ProgramMedicationID, A.ProgramID, P.Name AS ProgramName, A.Doses, A.Frequency, A.FullName, A.ShortName, A.UnitIdentifier, " +
                        "A.HowOften, A.AfterDays, A.StartDate,A.LastModifiedByID, A.EndDate, A.Note, A.IsCritical, A.AdditionalNotes, A.Reminder, A.PatientID, A.AddedByID, A.AddedOn, A.ErrCode " +
                    "FROM PatientMedicationModel A " +
                    "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID " +
                    "WHERE A.PatientMedicationID = ? AND A.IsActive = 1"
                    , medicationData.Medication.PatientMedicationID
                ).ConfigureAwait(false);
                if (medicationData.Medication != null)
                {
                    if (medicationData.Medication.ProgramID > 0)
                    {
                        medicationData.Medication.AddedByName = medicationData.Medication.ProgramName;
                    }
                    else
                    {
                        UserModel addedBy = await SqlConnection.FindWithQueryAsync<UserModel>(
                            "SELECT FirstName, LastName, RoleName FROM UserModel WHERE UserId = ? AND IsActive = 1"
                            , medicationData.Medication.AddedByID);
                        medicationData.Medication.AddedByName = $"{addedBy?.FirstName} {addedBy?.LastName}";
                    }
                    medicationData.Reminders = await SqlConnection.QueryAsync<MedicationReminderModel>(
                        "SELECT * FROM MedicationReminderModel WHERE PatientMedicationID = ? AND IsActive = 1 ORDER BY ReminderDateTime"
                        , medicationData.Medication.PatientMedicationID
                    ).ConfigureAwait(false);
                }
            }
            medicationData.UnitOptions = await SqlConnection.QueryAsync<OptionModel>(
                "SELECT A.UnitGroupID AS ParentOptionID, A.UnitID AS OptionID, A.UnitIdentifier AS GroupName, B.ShortUnitName AS OptionText, B.FullUnitName AS ParentOptionText " +
                "FROM UnitModel A " +
                "JOIN SettingModel C ON C.SettingID = A.UnitGroupID AND C.SettingKey = ? " +
                "JOIN UnitI18NModel B ON B.UnitID = A.UnitID AND B.LanguageID = ? AND A.IsActive = 1 AND B.IsActive = 1"
                , SettingsConstants.S_MEDICATION_UNIT_GROUP_KEY, medicationData.LanguageID
            );
        }

        /// <summary>
        /// Fetch medications to sync to server
        /// </summary>
        /// <param name="medicationData">Reference object to store data</param>
        /// <returns>Unsynced medication data</returns>
        public async Task GetPatientMedicationForSyncAsync(PatientMedicationDTO medicationData)
        {
            medicationData.Medications = await SqlConnection.QueryAsync<PatientMedicationModel>(
                "SELECT * FROM PatientMedicationModel WHERE IsSynced = 0"
            ).ConfigureAwait(false);
            medicationData.Reminders = await SqlConnection.QueryAsync<MedicationReminderModel>(
                "SELECT * FROM MedicationReminderModel WHERE IsSynced = 0"
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch Notification data to Register UnRegister
        /// </summary>
        /// <param name="medicationData">Reference object to hold data</param>
        /// <returns>Operation status</returns>
        public async Task GetNotificationsToRegisterUnRegisterAsync(PatientMedicationDTO medicationData)
        {
            medicationData.Notifications = await SqlConnection.QueryAsync<LocalNotificationModel>(
                "SELECT A.*,  B.ShortName AS Title, (B.Doses + ' ' + B.UnitIdentifier) AS Description " +
                "FROM LocalNotificationModel A " +
                "JOIN PatientMedicationModel B ON B.PatientMedicationID = A.RecordGuidID " +
                "WHERE A.RecordGuidID <> '' AND A.IsSynced = 0 AND A.IsActive = 1 AND A.ShowNotificationDateTime > ? AND A.ShowNotificationDateTime < ?"
                , GenericMethods.GetUtcDateTime.Ticks, GenericMethods.GetUtcDateTime.AddDays(3).Ticks
            ).ConfigureAwait(false);
            if (GenericMethods.IsListNotEmpty(medicationData.Notifications) && medicationData.Notifications.Any(x => x.IsActive))
            {
                SettingModel medicationUnitGroup = await new SettingLibDatabase().GetSettingAsync(SettingsConstants.S_MEDICATION_UNIT_GROUP_KEY).ConfigureAwait(false);
                var medicationCondition = string.Join("','", medicationData.Notifications.GroupBy(y => y.RecordGuidID).Select(x => x.First().RecordGuidID));
                medicationData.Medications = await SqlConnection.QueryAsync<PatientMedicationModel>(
                   "SELECT A.PatientMedicationID, A.PatientID, A.Doses, A.ShortName, C.FullUnitName AS AddedByID " +
                   "FROM PatientMedicationModel A " +
                   "JOIN UnitModel B ON B.UnitIdentifier = A.UnitIdentifier AND B.UnitGroupID = ? " +
                   "JOIN UnitI18NModel C ON C.UnitID = B.UnitID AND C.LanguageID = ? " +
                   $"WHERE A.PatientMedicationID IN ('{medicationCondition}')"
                   , medicationUnitGroup.SettingID, medicationData.LanguageID
                ).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Save patient medication
        /// </summary>
        /// <param name="medicationData">Reference object to hold patient medication data</param>
        /// <returns>Operation status</returns>
        public async Task SavePatientMedicationDataAsync(PatientMedicationDTO medicationData)
        {
            if (GenericMethods.IsListNotEmpty(medicationData.Medications))
            {
                PatientMedicationModel medication = medicationData.Medications.FirstOrDefault();
                if (!medication.IsSynced &&
                    await SqlConnection.FindWithQueryAsync<PatientMedicationModel>(
                    "SELECT 1 FROM PatientMedicationModel " +
                    "WHERE IsActive = 1 AND PatientMedicationID <> ? AND ShortName = ? COLLATE NOCASE AND PatientID = ? " +
                    "AND ((StartDate BETWEEN ? AND ?) OR (EndDate BETWEEN ? AND ?))"
                    , medication.PatientMedicationID, medication.ShortName, medication.PatientID, medication.StartDate, medication.EndDate, medication.StartDate, medication.EndDate
                ).ConfigureAwait(false) != null)
                {
                    medicationData.ErrCode = ErrorCode.DuplicateData;
                }
                else
                {
                    await SavePatientMedicationAsync(medicationData).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Save patient medications
        /// </summary>
        /// <param name="medicationData">Reference object to hold patient medication data</param>
        /// <returns>Operation status</returns>
        public async Task SavePatientMedicationAsync(PatientMedicationDTO medicationData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveMedications(medicationData, transaction);
                SaveReminders(medicationData, transaction);
                SaveNotifications(medicationData, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete medication Notification records
        /// </summary>
        /// <returns>Operation status</returns>
        public async Task DeleteNotificationsAsync()
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                transaction.Execute("DELETE FROM LocalNotificationModel");
                transaction.Execute("DELETE FROM sqlite_sequence WHERE name = 'LocalNotificationModel'");
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Reset medication Notification status
        /// </summary>
        /// <returns>Operation status</returns>
        public async Task ResetNotificationStatusAsync()
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                ResetNotificationStatus(transaction, null);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update Notification Register UnRegister data
        /// </summary>
        /// <param name="medicationData">Reference object containing data</param>
        /// <returns>Operation status</returns>
        public async Task UpdateNotificationsRegisterUnRegisterStatusAsync(PatientMedicationDTO medicationData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                ////UpdateMedicationsData(medicationData, transaction);
                ////UpdateRemindersData(medicationData, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update medication sync status
        /// </summary>
        /// <param name="medicationData">data to update sync status</param>
        public async Task UpdateMedicationsSyncStatusAsync(PatientMedicationDTO medicationData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                UpdateMedicationsData(medicationData, transaction);
                UpdateRemindersData(medicationData, transaction);
            }).ConfigureAwait(false);
        }

        private void SaveNotifications(PatientMedicationDTO medicationData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(medicationData.Notifications))
            {
                foreach (var notification in medicationData.Notifications)
                {
                    var existingNotification = transaction.FindWithQuery<LocalNotificationModel>(
                        "SELECT * FROM LocalNotificationModel WHERE RecordGuidID = ? AND ShowNotificationDateTime = ?"
                        , notification.RecordGuidID, notification.ShowNotificationDateTime);
                    if (existingNotification == null)
                    {
                        if (notification.IsActive || !notification.IsSynced)
                        {
                            transaction.Execute(
                                "INSERT INTO LocalNotificationModel (RecordGuidID, ShowNotificationDateTime, IsActive, IsSynced) VALUES (?, ?, ?, ?)"
                                , notification.RecordGuidID, notification.ShowNotificationDateTime, notification.IsActive, notification.IsSynced);
                        }
                    }
                    else if (!notification.IsActive && notification.IsSynced)
                    {
                        transaction.Execute("DELETE FROM LocalNotificationModel WHERE NotificationID = ?"
                            , existingNotification.NotificationID);
                    }
                    else
                    {
                        transaction.Execute("UPDATE LocalNotificationModel SET IsActive = ?, IsSynced = ? WHERE NotificationID = ?"
                            , notification.IsActive, notification.IsSynced, existingNotification.NotificationID);
                    }
                }
            }
        }

        private void SaveReminders(PatientMedicationDTO medicationData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(medicationData.Reminders))
            {
                foreach (var reminder in medicationData.Reminders)
                {
                    if (transaction.FindWithQuery<MedicationReminderModel>(
                        "SELECT 1 FROM MedicationReminderModel WHERE PatientMedicationID = ? AND ReminderDateTime = ?"
                        , reminder.PatientMedicationID, reminder.ReminderDateTime
                    ) == null)
                    {
                        transaction.Execute("INSERT INTO MedicationReminderModel (PatientMedicationID, ReminderDateTime, IsActive, IsSynced) VALUES (?, ?, ?, ?)"
                            , reminder.PatientMedicationID, reminder.ReminderDateTime, reminder.IsActive, reminder.IsSynced);
                    }
                    else
                    {
                        transaction.Execute("UPDATE MedicationReminderModel SET IsActive = ?, IsSynced = ? WHERE PatientMedicationID = ? AND ReminderDateTime = ?"
                            , reminder.IsActive, reminder.IsSynced, reminder.PatientMedicationID, reminder.ReminderDateTime);
                    }
                }
            }
        }

        private void SaveMedications(PatientMedicationDTO medicationData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(medicationData.Medications))
            {
                foreach (var medication in medicationData.Medications)
                {
                    transaction.InsertOrReplace(medication);
                    transaction.Execute("UPDATE MedicationReminderModel SET IsActive = 0, IsSynced = 0 WHERE PatientMedicationID = ?", medication.PatientMedicationID);
                    ResetNotificationStatus(transaction, medication);
                }
            }
        }

        private void ResetNotificationStatus(SQLiteConnection transaction, PatientMedicationModel medication)
        {
            var condition = medication == null ? "" : $", IsActive = 0 WHERE RecordGuidID = '{medication.PatientMedicationID}'";
            transaction.Execute($"UPDATE LocalNotificationModel SET IsSynced = 0 {condition}");
        }

        private void UpdateMedicationsData(PatientMedicationDTO medicationData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(medicationData.Medications))
            {
                foreach (PatientMedicationModel medication in medicationData.Medications)
                {
                    SaveResultModel result = medicationData.SaveMedications?.FirstOrDefault(x => x.ClientGuid == medication.PatientMedicationID);
                    medication.ErrCode = result == null ? ErrorCode.OK : result.ErrCode;
                    switch (medication.ErrCode)
                    {
                        case ErrorCode.OK:
                            // Data is successfully synced, so only update sync flag
                            transaction.Execute("UPDATE PatientMedicationModel SET IsSynced = 1, ErrCode = ? WHERE PatientMedicationID = ?", medication.ErrCode, medication.PatientMedicationID);
                            break;
                        case ErrorCode.DuplicateGuid:
                            // Update with new Guid
                            Guid patientMedicationID = GenerateNewGuid(transaction);
                            transaction.Execute("UPDATE PatientMedicationModel SET PatientMedicationID = ?, IsSynced = 0 WHERE PatientMedicationID = ?", patientMedicationID, medication.PatientMedicationID);
                            transaction.Execute("UPDATE MedicationReminderModel SET PatientMedicationID = ?, IsSynced = 0 WHERE PatientMedicationID = ?", patientMedicationID, medication.PatientMedicationID);
                            medicationData.ErrCode = medication.ErrCode;
                            break;
                        default:
                            // Mark record with the received error code
                            transaction.Execute("UPDATE PatientMedicationModel SET ErrCode = ? WHERE PatientMedicationID = ?", medication.ErrCode, medication.PatientMedicationID);
                            break;
                    }
                }
            }
        }

        private void UpdateRemindersData(PatientMedicationDTO medicationData, SQLiteConnection transaction)
        {
            if (!GenericMethods.IsListNotEmpty(medicationData.SaveMedications) && GenericMethods.IsListNotEmpty(medicationData.Reminders))
            {
                foreach (var reminder in medicationData.Reminders)
                {
                    transaction.Execute(
                        "UPDATE MedicationReminderModel SET IsSynced = 1 WHERE PatientMedicationID = ? AND ReminderDateTime = ?"
                        , reminder.PatientMedicationID, reminder.ReminderDateTime);
                }
            }
        }

        private Guid GenerateNewGuid(SQLiteConnection transaction)
        {
            Guid newGuid = GenericMethods.GenerateGuid();
            while (transaction.ExecuteScalar<int>("SELECT 1 FROM PatientMedicationModel WHERE PatientMedicationID = ?", newGuid) > 0)
            {
                newGuid = GenericMethods.GenerateGuid();
            }
            return newGuid;
        }
    }
}
