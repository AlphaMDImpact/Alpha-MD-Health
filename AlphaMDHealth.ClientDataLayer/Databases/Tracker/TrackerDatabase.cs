using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public class TrackerDatabase : BaseDatabase
    {
        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="patientTrackerData">PatientTracker data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SavePatientTrackerDataAsync(TrackerDTO patientTrackerData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (PatientTrackersModel patienttrackers in patientTrackerData?.PatientTrackers)
                {
                    transaction.InsertOrReplace(patienttrackers);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Save Patient Tracker Values
        /// </summary>
        /// <param name="patientTrackers">Save Patient Tracker values in db</param>
        /// <param name="isDataComingFromUI">If data is coming from Add Edit page</param>
        /// <returns>Operation Status</returns>
        public async Task SavePatientTrackerValuesAsync(TrackerDTO patientTrackers)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (PatientTrackersValuesModel trackerValues in patientTrackers.PatientTrackerValues)
                {
                    var existingTracker = transaction.FindWithQuery<PatientTrackersValuesModel>($"SELECT * FROM PatientTrackersValuesModel WHERE PatientTrackerID = ? AND IsActive = 1", trackerValues.PatientTrackerID);
                    if (existingTracker != null)
                    {
                        transaction.Execute("UPDATE PatientTrackersValuesModel SET IsActive= 0 AND LastModifiedON = ? WHERE PatientTrackerID = ? AND IsActive= 1",trackerValues.LastModifiedON, trackerValues.PatientTrackerID);
                    }
                    transaction.Execute(
                        "INSERT INTO PatientTrackersValuesModel (PatientTrackerID, CurrentValue, IsActive, IsSynced, LastModifiedON, AddedON) VALUES (?, ?, ?, ?, ?, ?)"
                        , trackerValues.PatientTrackerID, trackerValues.CurrentValue, 1, trackerValues.IsSynced, trackerValues.LastModifiedON, trackerValues.LastModifiedON);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Validates Patient Tracker current value
        /// </summary>
        /// <param name="patientTrackerData">Validates patient tracker data</param>
        /// <returns>Operation Status</returns>
        public async Task ValidatePatientTrackerValuesAsync(TrackerDTO patientTrackerData)
        {
            patientTrackerData.PatientTracker = await SqlConnection.FindWithQueryAsync<PatientTrackersModel>(
                "SELECT A.PatientTrackerID , A.ToDate , A.FromDate, A.TrackerID " +
                "FROM PatientTrackersModel A " +
                "WHERE A.IsActive = 1 AND A.PatientTrackerID = ?"
            , patientTrackerData.PatientTrackerValue.PatientTrackerID).ConfigureAwait(false);

            var dateValue = GenericMethods.ConvertIsoDateStringToDateTimeOffset(patientTrackerData?.PatientTrackerValue?.CurrentValue);
            var currentDate = Math.Abs((dateValue?.Date - DateTime.UtcNow.Date)?.Days ?? 0);
            patientTrackerData.TrackerRanges = await SqlConnection.QueryAsync<TrackerRangeModel>(
                "SELECT TM.TrackerID ,  TM.FromValue , TM.ToValue " +
                "FROM TrackerRangeModel TM " +
                "WHERE TM.IsActive = 1 AND TM.TrackerID = ?"
                , patientTrackerData.PatientTracker.TrackerID).ConfigureAwait(false);

            int max = patientTrackerData.TrackerRanges.Where(item => item.TrackerID == patientTrackerData.PatientTracker.TrackerID).Max(item => item.ToValue);
            patientTrackerData.TrackerRange = await SqlConnection.FindWithQueryAsync<TrackerRangeModel>(
                "SELECT TR.TrackerRangeID , TR.TrackerID, TR.FromValue , TR.ToValue, TR.ImageName " +
                "FROM TrackerRangeModel TR " +
                "WHERE TR.IsActive = 1 AND TR.FromValue <= ? AND TR.ToValue >= ? "
                , currentDate, currentDate).ConfigureAwait(false);
        }

        /// <summary>
        /// Get Trackers name and id for dropdwon
        /// </summary>
        /// <param name="patienttrackerData">Reference object to hold tracker list</param>
        /// <returns>Operation status</returns>
        public async Task GetTrackerOptionsAsync(TrackerDTO patienttrackerData)
        {
            patienttrackerData.TrackerTypes = await SqlConnection.QueryAsync<OptionModel>(
                  "SELECT DISTINCT A.TrackerID AS OptionID, TI18N.TrackerName AS OptionText " +
                  "FROM TrackersModel A " +
                  "LEFT JOIN TrackersI18NModel TI18N ON A.TrackerID = TI18N.TrackerID AND TI18N.LanguageID = ? " +
                  "WHERE A.IsActive = 1 AND A.TrackerID NOT IN (SELECT TrackerID FROM PatientTrackersModel WHERE PatientTrackerID <> ? AND UserID = ? AND IsActive = 1)"
                  , patienttrackerData.LanguageID, patienttrackerData.PatientTracker.PatientTrackerID, patienttrackerData.SelectedUserID
              ).ConfigureAwait(false);
            if (patienttrackerData.PatientTracker?.PatientTrackerID != Guid.Empty)
            {
                patienttrackerData.PatientTracker = await SqlConnection.FindWithQueryAsync<PatientTrackersModel>(
               $"SELECT A.PatientTrackerID, A.FromDate, A.ToDate, TI18N.TrackerName, A.TrackerID, A.IsActive, A.ProgramTrackerID " +
               "FROM PatientTrackersModel A " +
               "LEFT JOIN TrackersI18NModel TI18N ON A.TrackerID = TI18N.TrackerID AND TI18N.LanguageID = ? " +
               "WHERE A.PatientTrackerID = ? AND A.IsActive = 1", patienttrackerData.LanguageID, patienttrackerData.PatientTracker.PatientTrackerID).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get Patient Tracker Detail
        /// </summary>
        /// <param name="patientTrackerData">Reference object to hold patient tracker data</param>
        /// <returns>Operation status</returns>
        public async Task GetPatientTrackerDetail(TrackerDTO patientTrackerData)
        {
            patientTrackerData.PatientTrackerValue = await SqlConnection.FindWithQueryAsync<PatientTrackersValuesModel>(
              "SELECT PTV.PatientTrackerID , PTV.CurrentValue , PTV.IsActive " +
              "FROM PatientTrackersValuesModel PTV " +
              "WHERE PTV.IsActive = 1 AND PTV.PatientTrackerID = ?"
              , patientTrackerData.PatientTracker.PatientTrackerID).ConfigureAwait(false);

            if (patientTrackerData.PatientTrackerValue == null)
            {
                patientTrackerData.PatientTracker = await SqlConnection.FindWithQueryAsync<PatientTrackersModel>(
                    "SELECT A.PatientTrackerID , A.FromDate, A.ToDate , A.TrackerID , A.ProgramTrackerID , PTM.ValueAddedBy " +
                    "FROM PatientTrackersModel A " +
                    "LEFT JOIN ProgramTrackerModel PTM ON A.ProgramTrackerID = PTM.ProgramTrackerID " +
                    "WHERE A.IsActive = 1 AND A.PatientTrackerID = ?"
                    , patientTrackerData.PatientTracker.PatientTrackerID).ConfigureAwait(false);
            }
            else
            {
                patientTrackerData.PatientTracker = await SqlConnection.FindWithQueryAsync<PatientTrackersModel>(
                     "SELECT A.PatientTrackerID , A.ToDate , A.FromDate, A.TrackerID , PTV.CurrentValue " +
                     "FROM PatientTrackersModel A " +
                     "LEFT JOIN PatientTrackersValuesModel PTV ON A.PatientTrackerID = PTV.PatientTrackerID AND PTV.IsActive = 1 " +
                     "WHERE A.IsActive = 1 AND A.PatientTrackerID = ?"
                     , patientTrackerData.PatientTracker.PatientTrackerID).ConfigureAwait(false);

                var dateValue = GenericMethods.ConvertIsoDateStringToDateTimeOffset(patientTrackerData.PatientTrackerValue.CurrentValue);
                var currentDate = Math.Abs((dateValue?.Date - DateTime.UtcNow.Date)?.Days ?? 0);

                patientTrackerData.TrackerRange = await SqlConnection.FindWithQueryAsync<TrackerRangeModel>(
                    "SELECT TR.TrackerRangeID , TR.TrackerID, TR.FromValue , TR.ToValue, TR.ImageName " +
                    "FROM TrackerRangeModel TR " +
                    "LEFT JOIN PatientTrackersModel A ON TR.TrackerID = A.TrackerID " +
                    "WHERE TR.IsActive = 1 AND TR.FromValue <= ? AND TR.ToValue >= ? AND A.TrackerID = ? "
                    , currentDate, currentDate, patientTrackerData.PatientTracker.TrackerID).ConfigureAwait(false);

                if (patientTrackerData.TrackerRange != null)
                {
                    patientTrackerData.PatientTracker = await SqlConnection.FindWithQueryAsync<PatientTrackersModel>(
                        "SELECT DISTINCT A.PatientTrackerID, A.TrackerID , PTV.CurrentValue, A.ProgramTrackerID , A.FromDate, A.ToDate , TI18N.TrackerName , TRM.ImageName, TRI18N.CaptionText , TRI18N.InstructionsText , PTM.ValueAddedBy , PTM.ProgramTrackerID " +
                        "FROM PatientTrackersModel A " +
                        "LEFT JOIN PatientTrackersValuesModel PTV ON A.PatientTrackerID = PTV.PatientTrackerID AND PTV.IsActive = 1 " +
                        "LEFT JOIN ProgramTrackerModel PTM ON A.ProgramTrackerID = PTM.ProgramTrackerID " +
                        "INNER JOIN TrackersModel TM ON A.TrackerID = TM.TrackerID " +
                        "LEFT JOIN TrackersI18NModel TI18N ON A.TrackerID = TI18N.TrackerID AND TI18N.LanguageID = ? " +
                        "LEFT JOIN TrackerRangeModel TRM ON A.TrackerID = TRM.TrackerID AND TRM.IsActive = 1 " +
                        "INNER JOIN TrackerRangesI18N TRI18N ON TRI18N.TrackerRangeID = TRM.TrackerRangeID AND TRI18N.LanguageID = ? " +
                        "WHERE A.IsActive = 1 AND A.PatientTrackerID = ? AND TRM.FromValue <= ? AND TRM.ToValue >= ?"
                        , patientTrackerData.LanguageID, patientTrackerData.LanguageID, patientTrackerData.PatientTracker.PatientTrackerID, currentDate, currentDate).ConfigureAwait(false);
                }
                else
                {
                    patientTrackerData.PatientTracker = await SqlConnection.FindWithQueryAsync<PatientTrackersModel>(
                        "SELECT A.PatientTrackerID , A.ToDate , A.FromDate , PTV.CurrentValue , TI18N.TrackerName , A.TrackerID " +
                        "FROM PatientTrackersModel A " +
                        "LEFT JOIN PatientTrackersValuesModel PTV ON A.PatientTrackerID = PTV.PatientTrackerID AND PTV.IsActive = 1 " +
                        "LEFT JOIN TrackersI18NModel TI18N ON A.TrackerID = TI18N.TrackerID AND TI18N.LanguageID = ? " +
                        "WHERE A.IsActive = 1 AND A.PatientTrackerID = ?"
                        , patientTrackerData.LanguageID, patientTrackerData.PatientTracker.PatientTrackerID).ConfigureAwait(false);
                }
            }
            patientTrackerData.TrackerRanges = await SqlConnection.QueryAsync<TrackerRangeModel>(
                "SELECT TR.TrackerID , TR.ToValue, TR.FromValue " +
                "fROM TrackerRangeModel TR " +
                "WHERE TR.IsActive = 1 "
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Save patient tracker
        /// </summary>
        /// <param name="patientTrackerData">Reference object to hold patient tracker data</param>
        /// <returns>Operation status</returns>
        public async Task SavePatientTrackerAsync(TrackerDTO patientTrackerData)
        {
            if (await SqlConnection.FindWithQueryAsync<PatientTrackersModel>(
                "SELECT 1 FROM PatientTrackersModel " +
                "WHERE IsActive = 1 AND TrackerID = ? AND PatientTrackerID <> ? AND UserID = ? COLLATE NOCASE AND ((FromDate BETWEEN ? AND ?) OR (ToDate BETWEEN ? AND ?))"
                 , patientTrackerData.PatientTracker.TrackerID, patientTrackerData.PatientTracker.PatientTrackerID, patientTrackerData.PatientTracker.UserID
                 , patientTrackerData.PatientTracker.FromDate, patientTrackerData.PatientTracker.ToDate, patientTrackerData.PatientTracker.FromDate, patientTrackerData.PatientTracker.ToDate
            ).ConfigureAwait(false) != null)
            {
                patientTrackerData.ErrCode = ErrorCode.DuplicateData;
            }
            else
            {
                await SavePatientTrackerDataAsync(patientTrackerData).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get PatientTracker which are not synced
        /// </summary>
        /// <param name="trackerData">Get sync data for patient tracker</param>
        /// <returns>Operation Status</returns>
        public async Task GetPatientTrackersForSyncAsync(TrackerDTO trackerData)
        {
            trackerData.PatientTrackers = await SqlConnection.QueryAsync<PatientTrackersModel>(
               "SELECT * FROM PatientTrackersModel WHERE IsSynced = 0").ConfigureAwait(false);
        }

        /// <summary>
        /// Get PatientTracker Values which is not synced
        /// </summary>
        /// <param name="trackerData">Get sync data for patient tracker values</param>
        /// <returns>Operation Status</returns>
        public async Task GetPatientTrackerValuesForSyncAsync(TrackerDTO trackerData)
        {
            trackerData.PatientTrackerValues = await SqlConnection.QueryAsync<PatientTrackersValuesModel>(
               "SELECT * FROM PatientTrackersValuesModel WHERE IsSynced = 0").ConfigureAwait(false);
        }

        /// <summary>
        /// Delete Patient Tracker 
        /// </summary>
        /// <param name="patientTrackerID">Holds patienttrackerid to delete</param>
        /// <returns>Operation Status</returns>
        public async Task DeletePatientTrackerAsync(Guid patientTrackerID)
        {
            await SqlConnection.RunInTransactionAsync(async transaction =>
            {
                PatientTrackersModel patientTracker = new PatientTrackersModel();
                patientTracker = await SqlConnection.FindWithQueryAsync<PatientTrackersModel>("SELECT * FROM PatientTrackersModel WHERE PatientTrackerID = ?", patientTrackerID).ConfigureAwait(false);
                if (patientTracker.FromDate.Value.Date.ToLocalTime() > DateTime.UtcNow.Date)
                {
                    transaction.Execute("UPDATE PatientTrackersModel SET IsActive = 0, IsSynced = 0 WHERE PatientTrackerID = ?", patientTracker.PatientTrackerID);
                }
                else
                {
                    transaction.Execute("UPDATE PatientTrackersModel SET ToDate = ? , IsActive = 1, IsSynced = 0 WHERE PatientTrackerID = ?", DateTime.UtcNow.Date, patientTracker.PatientTrackerID);

                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update Patient Tracker sync status
        /// </summary>
        /// <param name="patientTracker">data to update sync status</param>
        /// <returns>Operation Status</returns>
        public async Task UpdatePatientTrackerSyncStatusAsync(PatientTrackersModel patientTracker)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                transaction.Execute("UPDATE PatientTrackersModel SET IsSynced = 1 WHERE PatientTrackerID = ?", patientTracker.PatientTrackerID);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update Patient Tracker Value sync status
        /// </summary>
        /// <param name="patientTrackerValue">data to update sync status</param>
        /// <returns>Operation Status</returns>
        public async Task UpdatePatientTrackerValueSyncStatusAsync(PatientTrackersValuesModel patientTrackerValue)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                transaction.Execute("UPDATE PatientTrackersValuesModel SET IsSynced = 1 WHERE PatientTrackerID = ?", patientTrackerValue.PatientTrackerID);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Save program trackers
        /// </summary>
        /// <param name="patientTrackerDTO">program tracker records to save</param>
        /// <returns>operation status</returns>
        public async Task SaveProgramTrackersDataAsync(TrackerDTO patientTrackerDTO)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (ProgramTrackerModel programTrackers in patientTrackerDTO?.ProgramTrackers)
                {
                    transaction.InsertOrReplace(programTrackers);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Save tracker master records in the database
        /// </summary>
        /// <param name="trackersData">Reference object to hold trackers data</param>
        /// <returns>Operation Status</returns>
        public async Task SaveTrackersAsync(TrackerDTO trackersData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveTrackers(trackersData, transaction);
                SaveTrackerRanges(trackersData, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Save tracker master records in the database
        /// </summary>
        /// <param name="trackersData">Reference object to hold trackers data</param>
        /// <returns>Operation Status</returns>
        public async Task SaveTrackersI18NAsync(TrackerDTO trackersData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveTrackersI18N(trackersData, transaction);
                SaveTrackerRangesI18N(trackersData, transaction);
            }).ConfigureAwait(false);
        }

        private void SaveTrackers(TrackerDTO patientTrackerDTO, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(patientTrackerDTO.Trackers))
            {
                foreach (TrackersModel trackers in patientTrackerDTO.Trackers)
                {
                    transaction.InsertOrReplace(trackers);
                }
            }
        }

        private void SaveTrackersI18N(TrackerDTO patientTrackerDTO, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(patientTrackerDTO.TrackersI18N))
            {
                foreach (TrackersI18NModel trackersi18n in patientTrackerDTO?.TrackersI18N)
                {
                    if (transaction.FindWithQuery<TrackersI18NModel>(
                        "SELECT 1 FROM TrackersI18NModel WHERE TrackerID=? AND LanguageID=? ",
                        trackersi18n.TrackerID, trackersi18n.LanguageID) == null)
                    {
                        transaction.Execute(
                            "INSERT INTO TrackersI18NModel(TrackerID, TrackerName, LanguageID) VALUES(?, ?, ?)",
                            trackersi18n.TrackerID, trackersi18n.TrackerName, trackersi18n.LanguageID);
                    }
                    else
                    {
                        transaction.Execute("UPDATE TrackersI18NModel SET TrackerName=? WHERE TrackerID=? AND LanguageID=? ",
                            trackersi18n.TrackerName, trackersi18n.TrackerID, trackersi18n.LanguageID);
                    }
                }
            }
        }

        private void SaveTrackerRanges(TrackerDTO patientTrackerDTO, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(patientTrackerDTO.TrackerRanges))
            {
                foreach (TrackerRangeModel trackers in patientTrackerDTO?.TrackerRanges)
                {
                    transaction.InsertOrReplace(trackers);
                }
            }
        }

        private void SaveTrackerRangesI18N(TrackerDTO patientTrackerDTO, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(patientTrackerDTO.TrackerRangesI18N))
            {
                foreach (TrackerRangesI18N trackerrangesi18n in patientTrackerDTO?.TrackerRangesI18N)
                {
                    if (transaction.FindWithQuery<TrackerRangesI18N>("SELECT 1 FROM TrackerRangesI18N WHERE TrackerRangeID=? AND LanguageID=? ", trackerrangesi18n.TrackerRangeID, trackerrangesi18n.LanguageID) == null)
                    {
                        transaction.Execute("INSERT INTO TrackerRangesI18N(TrackerRangeID, CaptionText, InstructionsText, IsActive, LanguageID) " +
                            "VALUES(?, ?, ?, ?, ?)", trackerrangesi18n.TrackerRangeID, trackerrangesi18n.CaptionText, trackerrangesi18n.InstructionsText, trackerrangesi18n.IsActive, trackerrangesi18n.LanguageID);
                    }
                    else
                    {
                        transaction.Execute("UPDATE TrackerRangesI18N SET IsActive=?, LanguageID=?, CaptionText=?, InstructionsText=? WHERE TrackerRangeID=? AND LanguageID=? ",
                          trackerrangesi18n.IsActive, trackerrangesi18n.LanguageID, trackerrangesi18n.CaptionText, trackerrangesi18n.InstructionsText, trackerrangesi18n.TrackerRangeID, trackerrangesi18n.LanguageID);
                    }
                }
            }
        }

        /// <summary>
        /// Get Patient Trackers List Data
        /// </summary>
        /// <param name="patientData">tracker data</param>
        /// <returns>patient tracker data</returns>
        public async Task GetPatientTrackersAsync(TrackerDTO patientData)
        {
            string[] trackerNames = string.IsNullOrEmpty(patientData.PatientTracker?.TrackerName)
                    ? new string[0] : patientData.PatientTracker?.TrackerName?.Split('|')
                    .Select(name => name.TrimEnd().TrimStart())
                    .ToArray();
            string condition = string.Empty;
            if (trackerNames != null && trackerNames.Length > 0)
            {
                var namesList = string.Join(",", trackerNames.Select(name => $"'{name}'"));
                condition = $" AND PTM.TrackerName IN ({namesList})";
            }
            string pColor;
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
            if (isPatientData)
            {
                pColor = "PP.ProgramGroupIdentifier";
            }
            else
            {
                pColor = "P.ProgramGroupIdentifier";
            }

            patientData.PatientTrackers = await SqlConnection.QueryAsync<PatientTrackersModel>(
                "SELECT DISTINCT PTM.UserID, PTM.PatientTrackerID, PTM.ProgramTrackerID, PTM.TrackerID, PTM.ToDate, PTM.FromDate, " +
                    "TI18N.TrackerName, PTV.CurrentValue, A.ValueAddedBy, PTM.IsActive, P.ProgramDuration, A.ProgramID, " +
                    $"{pColor} AS ProgramColor " +
                "FROM PatientTrackersModel PTM " +
                "JOIN TrackersModel TM ON PTM.TrackerID = TM.TrackerID AND TM.IsActive = 1 " +
                "LEFT JOIN ProgramTrackerModel A ON PTM.ProgramTrackerID = A.ProgramTrackerID " +
                "LEFT JOIN ProgramModel P ON A.ProgramID = P.ProgramID " +
                "LEFT JOIN PatientProgramModel PP ON P.ProgramID = PP.ProgramID AND PP.PatientID = ? " +
                "LEFT JOIN TrackersI18NModel TI18N ON TM.TrackerID = TI18N.TrackerID AND TI18N.LanguageID = ? " +
                "LEFT JOIN PatientTrackersValuesModel PTV ON PTM.PatientTrackerID = PTV.PatientTrackerID AND PTV.IsActive = 1 " +
                $"WHERE PTM.UserID  = ? AND PTM.IsActive= 1 {condition} " +
                "ORDER BY PTM.FromDate DESC",
                patientData.SelectedUserID, patientData.LanguageID, patientData.SelectedUserID

            ).ConfigureAwait(false);
            if (roleID == (int)RoleName.CareTaker && GenericMethods.IsListNotEmpty(patientData.PatientTrackers))
            {
                List<PatientProgramModel> sharedPrograms = await GetActiveSharedProgramsAsync(patientData).ConfigureAwait(false);
                patientData.PatientTrackers = sharedPrograms?.Count > 0
                    ? patientData.PatientTrackers.Where(x => sharedPrograms.Any(y => y.ProgramID == x.ProgramID))?.ToList()
                    : null;
            }
            if (isPatientData && GenericMethods.IsListNotEmpty(patientData.PatientTrackers))
            {
                patientData.PatientTrackers = patientData.PatientTrackers.Where(c =>
                    c.FromDate.Value.Date <= GenericMethods.GetUtcDateTime.Date
                ).ToList();
            }
            if (patientData.RecordCount > 0 && GenericMethods.IsListNotEmpty(patientData.PatientTrackers))
            {
                patientData.PatientTrackers = patientData.PatientTrackers.Take((int)patientData.RecordCount)?.ToList();
            }
            if (GenericMethods.IsListNotEmpty(patientData.PatientTrackers))
            {
                int days = 0;
                PatientTrackersModel myTracker = new PatientTrackersModel();
                foreach (PatientTrackersModel patientTracker in patientData.PatientTrackers)
                {
                    if (patientTracker.CurrentValue != null)
                    {
                        var trackerDate = GenericMethods.ConvertIsoDateStringToDateTimeOffset(patientTracker?.CurrentValue);
                        if (trackerDate != null)
                        {
                            TimeSpan difference = DateTime.UtcNow.Date - trackerDate.Value.Date;
                            days = Math.Abs(difference.Days);
                        }
                        myTracker = await SqlConnection.FindWithQueryAsync<PatientTrackersModel>(
                            "SELECT TI18N.TrackerName, TR.ImageName " +
                            "FROM TrackersModel TM " +
                            "JOIN TrackersI18NModel TI18N ON TM.TrackerID = TI18N.TrackerID AND TI18N.LanguageID = ? " +
                            "JOIN TrackerRangeModel TR ON TM.TrackerID= TR.TrackerID AND TR.IsActive = 1 AND TR.FromValue <= ? AND TR.ToValue >= ? " +
                            $"WHERE TR.TrackerID  = ?", patientData.LanguageID, days, days, patientTracker.TrackerID
                        ).ConfigureAwait(false);
                        if (myTracker != null)
                        {
                            patientTracker.TrackerName = myTracker.TrackerName;
                            patientTracker.ImageName = myTracker.ImageName;
                        }
                    }
                }
            }
        }
    }
}
