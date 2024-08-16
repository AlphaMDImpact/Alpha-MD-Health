using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public partial class ReadingDatabase : BaseDatabase
    {
        /// <summary>
        /// Save patient reading target data
        /// </summary>
        /// <param name="readingsData">object to save patient target data</param>
        /// <returns>list of target data</returns>
        public async Task SavePatientReadingTargetAsync(PatientReadingDTO readingsData)
        {
            try
            {
                if (readingsData.PatientReadingTarget != null)
                {
                    ReadingTargetModel existingMetaData = await SqlConnection.FindWithQueryAsync<ReadingTargetModel>
                        ($"SELECT * FROM ReadingTargetModel WHERE UserID = ? AND ReadingID = ? AND IsActive = 1", readingsData.SelectedUserID, readingsData.PatientReadingTarget.ReadingID).ConfigureAwait(false);
                    if (existingMetaData != null)
                    {
                        //this record is mark as inactive but doesn't require to sync to server (So ISynced=1) as in server we are makring this existing record as isactive=0 by default
                        await SqlConnection.ExecuteAsync($"UPDATE ReadingTargetModel SET IsActive= 0, IsSynced= 1 WHERE UserID= ? AND ReadingID= ? ", readingsData.SelectedUserID, readingsData.PatientReadingTarget.ReadingID);
                    }
                    await SqlConnection.ExecuteAsync($"INSERT INTO ReadingTargetModel " +
                        $"(TargetMinValue, TargetMaxValue,  IsActive, IsSynced,  ReadingID, UserID) VALUES( ?, ?, ?, ?, ?, ?)",
                        readingsData.PatientReadingTarget.TargetMinValue, readingsData.PatientReadingTarget.TargetMaxValue, true, false, readingsData.PatientReadingTarget.ReadingID, readingsData.SelectedUserID);
                }
            }
            catch (System.Exception e)
            {
                GenericMethods.LogData(e.Message);
            }
        }

        private void SavePatientReadingTargets(PatientReadingDTO readingData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(readingData.PatientReadingTargets))
            {
                foreach (var target in readingData.PatientReadingTargets.OrderBy(x => x.IsActive))
                {
                    string saveQuery;
                    if (transaction.FindWithQuery<ReadingTargetModel>(
                        "SELECT 1 FROM ReadingTargetModel WHERE UserID=? AND ReadingID=? ",
                        target.UserID, target.ReadingID) == null)
                    {
                        saveQuery = "INSERT INTO ReadingTargetModel(TargetMinValue, TargetMaxValue, IsActive, IsSynced, UserID, ReadingID) VALUES(?, ?, ?, ?, ?, ?) ";
                    }
                    else
                    {
                        saveQuery = "UPDATE ReadingTargetModel SET TargetMinValue=?, TargetMaxValue=?, IsActive=?, IsSynced=? WHERE UserID=? AND ReadingID=? ";
                    }
                    transaction.Execute(saveQuery, target.TargetMinValue, target.TargetMaxValue, target.IsActive, target.IsSynced, target.UserID, target.ReadingID);
                }
            }
        }

        /// <summary>
        /// To get patient readig ranges to sync data to server
        /// </summary>
        /// <param name="patientReadingData">object to get data</param>
        /// <returns>all updates reading ranges</returns>
        public async Task GetPatientReadingTargetsForSyncAsync(PatientReadingDTO patientReadingData)
        {
            patientReadingData.PatientReadingTargets = await SqlConnection.QueryAsync<ReadingTargetModel>(
                "SELECT * FROM ReadingTargetModel WHERE IsSynced = 0"
            );
        }

        /// <summary>
        /// Update Sync Status For Target Data
        /// </summary>
        /// <param name="readingsData">object to update patient target sync status</param>
        /// <returns>operation status</returns>
        public async Task UpdateSyncStatusForTargetDataAsync(PatientReadingDTO readingsData)
        {
            foreach (ReadingTargetModel target in readingsData.PatientReadingTargets)
            {
                await SqlConnection.ExecuteAsync(
                    $"UPDATE ReadingTargetModel SET IsSynced = 1 WHERE UserID= ? AND ReadingID= ? ",
                    target.UserID, target.ReadingID
                );
            }
        }
    }
}
