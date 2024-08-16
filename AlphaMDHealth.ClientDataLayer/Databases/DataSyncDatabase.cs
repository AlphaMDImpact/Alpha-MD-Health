using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;
using System.Globalization;

namespace AlphaMDHealth.ClientDataLayer
{
    /// <summary>
    /// represents Sync database module
    /// </summary>
    public class DataSyncDatabase : BaseDatabase
    {
        /// <summary>
        /// Returns DataSync for given Table
        /// </summary>
        /// <param name="syncData">Table for which DataSync info is requested</param>
        /// <returns>DataSync info Record</returns>
        public async Task GetDataSyncForAsync(BaseDataSyncDTO syncData)
        {
            string syncForString = string.Join(",", syncData.DataSyncFor.Select(x => string.Format(CultureInfo.CurrentCulture, "'{0}'", x)));
            syncData.DataSyncForRecords = await SqlConnection.QueryAsync<DataSyncModel>
                ($"SELECT * FROM DataSyncModel WHERE SyncFor IN ({syncForString}) AND PatientID = ? ORDER BY SyncFor", syncData.PatientID).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns DataSync for given Table 
        /// </summary>
        /// <param name="dataSyncFor">Table for which DataSync info is requested</param>
        /// <returns>DataSync info Record</returns>
        public async Task<DataSyncModel> GetDataSyncForAsync(string dataSyncFor)
        {
            return await SqlConnection.Table<DataSyncModel>().FirstOrDefaultAsync(c => c.SyncFor == dataSyncFor && c.PatientID == default);
        }

        /// <summary>
        /// Returns DataSync for given Table for post service call
        /// </summary>
        /// <param name="dataSyncFor">Table for which DataSync info is requested</param>
        /// <param name="currentSyncDateTime">sync started UTC DateTime</param>
        /// <param name="waitTime">Default wait time when operation is in-progress, after this time post record of that table again.</param>
        /// <param name="patientID">PatientID for which data needs to fetch</param>
        /// <returns>DataSync info Record with operation status</returns>
        public async Task<DataSyncModel> GetDataSyncForAsync(string dataSyncFor, DateTimeOffset currentSyncDateTime, int waitTime, long patientID)
        {
            DataSyncModel existingSyncData = await SqlConnection.Table<DataSyncModel>().FirstOrDefaultAsync(c => c.SyncFor == dataSyncFor && c.PatientID == patientID).ConfigureAwait(false);
            if (existingSyncData?.SyncToStatus == SyncStatus.InProgress && (currentSyncDateTime - existingSyncData.SyncToStartedDateTime).Minutes < waitTime)
            {
                //when sync is in progress and did not reached default wait time, return with SyncInProgress status and do not call syncto server for that table
                existingSyncData.ErrCode = ErrorCode.SyncInProgress;
                return existingSyncData;
            }
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                //Insert record for this table with SyncToStartedDateTime to currentSyncDateTime and syncToStatus InProgress
                existingSyncData = existingSyncData ?? new DataSyncModel { SyncFor = dataSyncFor, PatientID = patientID };
                existingSyncData.SyncToStatus = SyncStatus.InProgress;
                existingSyncData.SyncToStartedDateTime = currentSyncDateTime;
                SaveSyncData(transaction, existingSyncData, 2);
            }).ConfigureAwait(false);
            return existingSyncData;
        }

        /// <summary>
        /// Save DataSync info for Tables (Initialize database)
        /// </summary>
        /// <param name="syncData">DTO with list of DataSync for records</param>
        /// <returns>Save operation success status</returns>
        public async Task SaveDataSyncInfoAsync(BaseDataSyncDTO syncData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (DataSyncModel dataSyncModel in syncData.DataSyncForRecords)
                {
                    dataSyncModel.PatientID = syncData.PatientID;
                    SaveSyncData(transaction, dataSyncModel, 0);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Registers DateTime at which app was last synced from server
        /// </summary>
        /// <param name="syncFor">Table for which sync from server called</param>
        /// <param name="updatedDate">DateTime at which sync from server is completed</param>
        /// <param name="patientID">PatientID for which data needs to fetch</param>
        /// <returns>result as a reference object</returns>
        public async Task UpdateDateSyncedFromServerAsync(string syncFor, DateTimeOffset updatedDate, long patientID)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveSyncData(transaction, new DataSyncModel { SyncFor = syncFor, PatientID = patientID, SyncFromServerDateTime = updatedDate }, 1);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Registers DateTime at which app was last synced to server
        /// </summary>
        /// <param name="syncData">DateTime at which sync to server is completed</param>
        /// <returns>operation status</returns>
        public async Task UpdateDateSyncedToServerAsync(DataSyncModel syncData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveSyncData(transaction, syncData, 3);
            }).ConfigureAwait(false);
        }

        private void SaveSyncData(SQLiteConnection transaction, DataSyncModel syncData, int flow)
        {
            string condition = (syncData.PatientID == -1) ? "" : $"AND PatientID = {syncData.PatientID}";
            if (transaction.FindWithQuery<DataSyncModel>($"SELECT 1 FROM DataSyncModel WHERE SyncFor = ? {condition}", syncData.SyncFor) == null)
            {
                if (syncData.PatientID != -1)
                {
                    transaction.Execute($"INSERT INTO DataSyncModel (SyncFor, PatientID, SyncFromServerDateTime, SyncToServerDateTime, SyncToStartedDateTime, SyncToStatus) VALUES(?, ?, ?, ?, ?, ?)",
                        syncData.SyncFor, syncData.PatientID, syncData.SyncFromServerDateTime, syncData.SyncToServerDateTime, syncData.SyncToStartedDateTime, syncData.SyncToStatus);
                }
            }
            else
            {
                switch (flow)
                {
                    case 1:
                        transaction.Execute($"UPDATE DataSyncModel SET SyncFromServerDateTime = ? WHERE SyncFor = ? {condition}",
                            syncData.SyncFromServerDateTime, syncData.SyncFor);
                        break;
                    case 2:
                        transaction.Execute($"UPDATE DataSyncModel SET SyncToStartedDateTime = ?, SyncToStatus = ? WHERE SyncFor = ? {condition}",
                            syncData.SyncToStartedDateTime, syncData.SyncToStatus, syncData.SyncFor);
                        break;
                    case 3:
                        if (syncData.SyncToStatus == SyncStatus.Done)
                        {
                            transaction.Execute($"UPDATE DataSyncModel SET SyncToServerDateTime = ?, SyncToStatus = ? WHERE SyncFor = ? {condition}",
                                syncData.SyncToServerDateTime, syncData.SyncToStatus, syncData.SyncFor);
                        }
                        else
                        {
                            transaction.Execute($"UPDATE DataSyncModel SET SyncToStatus = ? WHERE SyncFor = ? {condition}", syncData.SyncToStatus, syncData.SyncFor);
                        }
                        break;
                    default:
                        transaction.Execute($"UPDATE DataSyncModel SET SyncFromServerDateTime = ?, SyncToServerDateTime = ?, SyncToStartedDateTime = ?, SyncToStatus = ? WHERE SyncFor = ? {condition}",
                            syncData.SyncFromServerDateTime, syncData.SyncToServerDateTime, syncData.SyncToStartedDateTime, syncData.SyncToStatus, syncData.SyncFor);
                        break;
                }
            }
        }
    }
}