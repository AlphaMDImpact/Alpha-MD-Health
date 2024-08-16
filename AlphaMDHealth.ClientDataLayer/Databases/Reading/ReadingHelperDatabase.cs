using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public partial class ReadingDatabase : BaseDatabase
    {
        /// <summary>
        /// Gets Units data list 
        /// </summary>
        /// <param name="readingsData">Reference object to store units data</param>
        /// <returns>List of units data</returns>
        public async Task GetDataToCalculateBmiAsync(PatientReadingDTO readingsData)
        {
            await GetMetadataAsync(readingsData).ConfigureAwait(false);
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                //// Step 1 : Check metadata is available and having metadata of BMI and Height
                //// Step 2 : For Edit Reading, Fetch Existing BMI and Weight value available in DB
                //// Step 3 : Get Latest Value of Patient Height to calcuulate BMI with current weight value
                if (GenericMethods.IsListNotEmpty(readingsData.ChartMetaData)
                    && readingsData.ChartMetaData.Any(x => x.ReadingID == ResourceConstants.R_BMI_KEY_ID)
                    && readingsData.ChartMetaData.Any(x => x.ReadingID == ResourceConstants.R_HEIGHT_KEY_ID))
                {
                    List<PatientReadingUIModel> existingPatientReadings = GetExistingBMIPatientReadings(readingsData, transaction);
                    bool isHeightFound = false;
                    foreach (var reading in readingsData.ListData)
                    {
                        PatientReadingUIModel heightData = transaction.FindWithQuery<PatientReadingUIModel>
                            ($"{GetPatientReadingsQuery(string.Empty)} AND A.ReadingDateTime <= ? ORDER BY A.ReadingDateTime DESC LIMIT 1"
                            , ResourceConstants.R_HEIGHT_KEY_ID, readingsData.SelectedUserID, reading.ReadingDateTime);
                        if (heightData != null && !existingPatientReadings.Contains(heightData))
                        {
                            isHeightFound = true;
                            existingPatientReadings.Add(heightData);
                        }
                    }
                    if (isHeightFound && GenericMethods.IsListNotEmpty(existingPatientReadings))
                    {
                        readingsData.ListData = existingPatientReadings;
                        return;
                    }
                }
                readingsData.ErrCode = ErrorCode.InternalServerError;
            });
        }

        private List<PatientReadingUIModel> GetExistingBMIPatientReadings(PatientReadingDTO readingsData, SQLiteConnection transaction)
        {
            var weightReadingIDs = readingsData.ListData.Count > 1
                ? string.Join("','", readingsData.ListData.Select(x => x.PatientReadingID))
                : $"{readingsData.ListData[0].PatientReadingID}";
            var existingPatientReadings = transaction.Query<PatientReadingUIModel>(
                $"{GetPatientReadingsQuery(string.Empty)} AND A.PatientReadingID IN ('{weightReadingIDs}')"
                , ResourceConstants.R_WEIGHT_KEY_ID, readingsData.SelectedUserID);
            if (GenericMethods.IsListNotEmpty(existingPatientReadings))
            {
                var bmis = new List<PatientReadingUIModel>();
                foreach (var weight in existingPatientReadings)
                {
                    var existingBMI = transaction.FindWithQuery<PatientReadingUIModel>(
                        $"{GetPatientReadingsQuery(string.Empty)} AND A.ReadingDateTime = ? "
                        , ResourceConstants.R_BMI_KEY_ID, readingsData.SelectedUserID, weight.ReadingDateTime);
                    if (existingBMI != null)
                    {
                        bmis.Add(existingBMI);
                    }
                }
                if (GenericMethods.IsListNotEmpty(bmis))
                {
                    existingPatientReadings.AddRange(bmis);
                }
            }
            else
            {
                existingPatientReadings = new List<PatientReadingUIModel>();
            }
            return existingPatientReadings;
        }

        /// <summary>
        /// Get details of Task to fetch reading
        /// </summary>
        /// <param name="readingsData">Object to store data</param>
        /// <returns>Reading type</returns>
        public async Task<bool> GetTaskDetailsAsync(PatientReadingDTO readingsData)
        {
            ReadingModel readingData = await SqlConnection.FindWithQueryAsync<ReadingModel>(
               "SELECT RTM.ReadingID, RTM.ReadingParentID, RTM.ReadingCategoryID " +
               "FROM ReadingModel RTM " +
               "WHERE RTM.IsActive = 1 AND (RTM.ReadingID = ?)",
               readingsData.ReadingID);
            TaskModel taskData = await new PatientTaskDatabase().GetTaskByPatientTaskIDAsync(readingsData.PatientTaskID);
            if (taskData == null || readingData == null)
            {
                readingsData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                return true;
            }
            else
            {
                readingsData.ReadingID = readingData.ReadingID;
                readingsData.ReadingCategoryID = readingData.ReadingCategoryID;
                if (readingsData.RecordCount != -1)
                {
                    readingsData.PatientReadingID = (await SqlConnection.FindWithQueryAsync<PatientReadingUIModel>(
                        $"{GetPatientReadingsQuery(string.Empty)} AND A.PatientTaskID = ? ORDER BY A.ReadingDateTime DESC LIMIT 1"
                        , readingsData.ReadingID, readingsData.SelectedUserID, readingsData.PatientTaskID
                    ).ConfigureAwait(false))?.PatientReadingID ?? Guid.Empty;
                }
                return false;
            }
        }
    }
}