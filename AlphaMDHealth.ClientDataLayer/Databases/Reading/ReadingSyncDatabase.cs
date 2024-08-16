using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public partial class ReadingDatabase : BaseDatabase
    {
        /// <summary>
        /// Save Readings and Reading Ranges to local database
        /// </summary>
        /// <param name="readingData">Reference object of Readings And ReadingRanges</param>
        /// <returns>operation status</returns>
        public async Task SaveReadingsAsync(PatientReadingDTO readingData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveReadings(readingData, transaction);
                SaveReadingRanges(readingData, transaction);
                SavePatientReadingTargets(readingData, transaction);
                SavePatientReadings(readingData, transaction);
                SaveReadingDevices(readingData, transaction);
                SavePatientReadingDevices(readingData, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Save Reading Master to local database
        /// </summary>
        /// <param name="readingData">Reference object of ReadingMaster</param>
        /// <returns>operation status</returns>
        public async Task SaveReadingMastersAsync(PatientReadingDTO readingData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveReadingMasters(readingData, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// get all non synced user readings
        /// </summary>
        /// <param name="readingData">userreadings</param>
        /// <returns>operation status</returns>
        public async Task GetPatientReadingsAndDevicesToSyncWithServerAsync(PatientReadingDTO readingData)
        {
            await GetPatientReadingsToSyncWithServerAsync(readingData).ConfigureAwait(false);
            await GetPatientReadingDevicesToSyncWithServerAsync(readingData).ConfigureAwait(false);
        }

        private Guid GenerateNewGuid(SQLiteConnection transaction)
        {
            Guid newGuid = GenericMethods.GenerateGuid();
            while (transaction.ExecuteScalar<int>("SELECT 1 FROM PatientReadingModel WHERE PatientReadingID = ?", newGuid) > 0)
            {
                newGuid = GenericMethods.GenerateGuid();
            }
            return newGuid;
        }

    }
}