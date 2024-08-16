using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public partial class ReadingDatabase : BaseDatabase
    {
        /// <summary>
        /// Fetch vital categories data
        /// </summary>
        /// <param name="readingsData">Reference object for returning categories as output</param>
        /// <returns>Categories list in readingsData as reference</returns>
        public async Task GetCategoryRelationsAsync(PatientReadingDTO readingsData)
        {
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            if (readingsData.RecordCount == -1 && readingsData.PatientTaskID > 0
            && await GetTaskDetailsAsync(readingsData).ConfigureAwait(false))
            {
                return;
            }

            string condition = $"WHERE R.UserID = {readingsData.SelectedUserID} ";
            string columns = string.Empty;
            if (roleID == (int)RoleName.CareTaker)
            {
                condition += " AND R.IsActive = 1 ";
                columns = ", R.ProgramID AS ParentOptionID ";
            }

            if (readingsData.RecordCount == -1)
            {
                if (readingsData.PatientReadingID == Guid.Empty)
                {
                    condition += $"AND {GetAddAllowedConditionString("R")} "; 
                }
                else
                {
                    condition += $"AND (R.ReadingID = {readingsData.ReadingID} OR R.ReadingParentID= {readingsData.ReadingID}) ";
                }
            }
            else if (readingsData.RecordCount == -2)
            {
                condition += $"AND (R.ReadingID = {readingsData.ReadingID} OR R.ReadingParentID= {readingsData.ReadingID}) ";
            }
            else if (readingsData.RecordCount > -1)
            {
                condition += "AND (R.IsActive = 1 OR EXISTS ( " + 
                    "SELECT 1 FROM PatientReadingModel X WHERE X.ReadingID = R.ReadingID AND X.UserID = R.UserID AND X.IsActive = 1 LIMIT 1)) ";
            }
            readingsData.FilterOptions = await SqlConnection.QueryAsync<OptionModel>(
                $"SELECT DISTINCT R.ReadingCategoryID AS OptionID {columns}" +
                "FROM ReadingModel R " +
                $"{condition} "
            ).ConfigureAwait(false);
            if (roleID == (int)RoleName.CareTaker)
            {
                List<PatientProgramModel> sharedPrograms = await GetActiveSharedProgramsAsync(readingsData).ConfigureAwait(false);
                readingsData.FilterOptions = sharedPrograms?.Count > 0
                    ? readingsData.FilterOptions.Where(x => sharedPrograms.Any(y => y.ProgramID == x.ParentOptionID)).Distinct()?.ToList()
                    : null;
            }
        }

        /// <summary>
        /// Gets metadata of all reading types if ReadingType is None else returns metadata of given ReadingType
        /// </summary>
        /// <param name="readingsData">Reference object to return metadata. If IsActive is true then only Dashboard enabled data will be fetched</param>
        /// <returns>List of metadata</returns>
        public async Task GetMetadataAsync(PatientReadingDTO readingsData)
        {
            var conditions = readingsData.ReadingCategoryID > 0
                ? $"R.ReadingCategoryID= {readingsData.ReadingCategoryID} AND "
                : string.Empty;
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            if (readingsData.RecordCount == -1)
            {
                if (readingsData.PatientReadingID == Guid.Empty)
                {
                    conditions += $"{GetAddAllowedConditionString("R")} AND "; 
                    if (readingsData.ReadingID > 0)
                    {
                        conditions += $"(R.ReadingID= {readingsData.ReadingID} OR R.ReadingParentID= {readingsData.ReadingID}) AND ";
                    }
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(readingsData.ReadingIDs))
                {
                    conditions += $"(R.ReadingID IN ({readingsData.ReadingIDs}) OR R.ReadingParentID IN ({readingsData.ReadingID})) AND ";
                }
                else if (readingsData.ReadingID > 0)
                {
                    conditions += $"(R.ReadingID= {readingsData.ReadingID} OR R.ReadingParentID= {readingsData.ReadingID}) AND ";
                }
            }
            //Get Metadata for parent readings with Targets 
            readingsData.ChartMetaData = await SqlConnection.QueryAsync<ReadingMetadataUIModel>(
                "SELECT DISTINCT R.ReadingID, R.SequenceNo, R.DigitsAfterDecimalPoint, R.ReadingFrequency, R.ValueAddedBy, R.AllowManualAdd, R.AllowHealthKitData, " +
                "R.AllowDeviceData, R.ShowInGraph, R.ShowInData, R.AllowDelete, R.ShowInDifferentLines, R.SummaryRecordCount, R.ChartType, R.ReadingFilters, " +
                "R.DaysOfPastRecordsToSync, R.ReadingParentID, R.ReadingCategoryID, R.IsGroupValue, R.ReadingFormula, R.UnitGoupID, R.ReadingValueTypeID, " +
                "R.UserID, U.UnitIdentifier AS BaseUnitIdentifier, RT.TargetMinValue, RT.TargetMaxValue, R.ProgramID, R.IsActive, " +
                "(" +
                    "SELECT PR.ReadingDateTime FROM PatientReadingModel PR " +
                    "WHERE PR.ReadingID= R.ReadingID AND PR.UserID = R.UserID AND PR.IsActive= 1 " +
                    "ORDER BY PR.ReadingDateTime DESC LIMIT 1" +
                ") AS ReadingDateTime " +
                "FROM ReadingModel R " +
                "LEFT JOIN UnitModel U ON U.UnitGroupID= R.UnitGoupID AND U.IsBaseUnit= 1 " +
                "LEFT JOIN ReadingTargetModel RT ON RT.UserID= R.UserID AND RT.ReadingID= R.ReadingID AND RT.IsActive= 1 " +
                $"WHERE {conditions} R.UserID= ? "
                , readingsData.SelectedUserID
            ).ConfigureAwait(false);
            if (roleID == (int)RoleName.CareTaker)
            {
                List<PatientProgramModel> sharedPrograms = await GetActiveSharedProgramsAsync(readingsData).ConfigureAwait(false);
                readingsData.ChartMetaData = sharedPrograms?.Count > 0
                    ? readingsData.ChartMetaData.Where(x => sharedPrograms.Any(y => y.ProgramID == x.ProgramID) && x.IsActive).Distinct()?.ToList()
                    : null;
            }
            if (GenericMethods.IsListNotEmpty(readingsData.ChartMetaData))
            {
                readingsData.ChartMetaData = (from p in readingsData.ChartMetaData
                                              group p by p.ReadingID into g
                                              select g.FirstOrDefault(x => x.IsActive) ?? g.FirstOrDefault()).ToList();
            }
        }

        /// <summary>
        /// Check is there any reading type which allow to add data mannualy
        /// </summary>
        /// <returns>Operation status</returns>
        public async Task<bool> IsAllowToAddReadingsAsync(long selectedUserID)
        {
            return await SqlConnection.FindWithQueryAsync<ReadingModel>(
                GetMetaDataCheckQuery(selectedUserID, "", $"AND {GetAddAllowedConditionString("R")} ")
            ).ConfigureAwait(false) != null;
        }

        private string GetMetaDataCheckQuery(long selectedUserID, string categoryCondition, string addAllowcondition)
        {
            return $"SELECT 1 FROM ReadingModel R WHERE R.UserID = {selectedUserID} {categoryCondition} {addAllowcondition}";
        }

        private string GetAddAllowedConditionString(string readingTbl)
        {
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
            return $"{readingTbl}.IsActive = 1 AND {readingTbl}.AllowManualAdd = 1 " +
                $"AND {readingTbl}.ValueAddedBy IN ("
                + (isPatientData
                    ? $"{ResourceConstants.R_BOTH_KEY_ID},{ResourceConstants.R_PATIENT_KEY_ID}"
                    : $"{ResourceConstants.R_BOTH_KEY_ID},{ResourceConstants.R_PROVIDER_KEY_ID}") + ")";
        }

        private void SaveReadings(PatientReadingDTO readingData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(readingData.Readings))
            {
                foreach (var reading in readingData.Readings.OrderBy(x => x.IsActive))
                {
                    string saveQuery;
                    if (transaction.FindWithQuery<ReadingModel>(
                        "SELECT 1 FROM ReadingModel WHERE UserID = ? AND ProgramReadingID= ?",
                        reading.UserID, reading.ProgramReadingID) == null)
                    {
                        saveQuery = "INSERT INTO ReadingModel(IsActive, IsGroupValue, ReadingFormula, SequenceNo, " +
                            "DigitsAfterDecimalPoint, ReadingFrequency, ValueAddedBy, AllowManualAdd, AllowHealthKitData, AllowDeviceData, " +
                            "AllowDelete, ShowInGraph, ShowInData, ShowInDifferentLines, SummaryRecordCount, ChartType, ReadingFilters, " +
                            "DaysOfPastRecordsToSync, UnitGoupID, ReadingValueTypeID, PatientProgramID, ProgramID, ReadingParentID, " +
                            "ReadingCategoryID, ReadingID, UserID, ProgramReadingID) " +
                            "VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?) ";
                    }
                    else
                    {
                        saveQuery = "UPDATE ReadingModel SET IsActive= ?, IsGroupValue= ?, ReadingFormula= ?, SequenceNo= ?, " +
                            "DigitsAfterDecimalPoint= ?, ReadingFrequency= ?, ValueAddedBy= ?, AllowManualAdd= ?, AllowHealthKitData= ?, AllowDeviceData= ?, " +
                            "AllowDelete= ?, ShowInGraph= ?, ShowInData= ?, ShowInDifferentLines= ?, SummaryRecordCount= ?, ChartType= ?, ReadingFilters= ?, " +
                            "DaysOfPastRecordsToSync= ?, UnitGoupID= ?, ReadingValueTypeID= ?, PatientProgramID= ?, ProgramID= ?, ReadingParentID= ?, " +
                            "ReadingCategoryID= ?, ReadingID= ? WHERE UserID= ? AND ProgramReadingID= ? ";
                    }
                    transaction.Execute(saveQuery, reading.IsActive, reading.IsGroupValue, reading.ReadingFormula, reading.SequenceNo,
                        reading.DigitsAfterDecimalPoint, reading.ReadingFrequency, reading.ValueAddedBy, reading.AllowManualAdd, reading.AllowHealthKitData, reading.AllowDeviceData,
                        reading.AllowDelete, reading.ShowInGraph, reading.ShowInData, reading.ShowInDifferentLines, reading.SummaryRecordCount, reading.ChartType, reading.ReadingFilters,
                        reading.DaysOfPastRecordsToSync, reading.UnitGoupID, reading.ReadingValueTypeID, reading.PatientProgramID, reading.ProgramID, reading.ReadingParentID,
                        reading.ReadingCategoryID, reading.ReadingID, reading.UserID, reading.ProgramReadingID);
                }
            }
        }

        private void SaveReadingMasters(PatientReadingDTO readingData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(readingData.ReadingMasters))
            {
                foreach (var reading in readingData.ReadingMasters)
                {
                    transaction.Execute(transaction.FindWithQuery<ReadingMasterModel>("SELECT 1 FROM ReadingMasterModel WHERE ReadingID = ?", reading.ReadingID) == null
                       ? $"INSERT INTO ReadingMasterModel (ReadingID, ReadingCategoryID, ReadingParentID, IsGroupValue, ReadingValueTypeID, UnitGoupID, ReadingFilters, DaysOfPastRecordsToSync,IsActive) VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?)"
                       : "UPDATE ReadingMasterModel SET ReadingID= ?, ReadingCategoryID= ?, ReadingParentID= ?, IsGroupValue= ?, ReadingValueTypeID= ?, UnitGoupID= ?," +
                       " ReadingFilters= ?, DaysOfPastRecordsToSync= ?, IsActive= ? WHERE ReadingID = ?"
                       , reading.ReadingID, reading.ReadingCategoryID, reading.ReadingParentID, reading.IsGroupValue, reading.ReadingValueTypeID,
                       reading.UnitGoupID, reading.ReadingFilters, reading.DaysOfPastRecordsToSync, reading.IsActive, reading.ReadingID);
                }
            }
        }
    }
}