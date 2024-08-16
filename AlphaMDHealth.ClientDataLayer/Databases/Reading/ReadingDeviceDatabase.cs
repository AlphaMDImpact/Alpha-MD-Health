using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    /// <summary>
    /// Devices database
    /// </summary>
    public partial class ReadingDatabase : BaseDatabase
    {
        /// <summary>
        /// Get Paired devices data from database
        /// </summary>
        /// <param name="deviceData">Ref object to store paired devices data</param>
        /// <returns>Paired devices with operation status</returns>
        public async Task GetDevicesToPairAsync(PatientDeviceDTO deviceData)
        {
            deviceData.Devices = await SqlConnection.QueryAsync<PatientDeviceModel>
                 ("SELECT DISTINCT A.ResourceValue AS Name, A.PlaceHolderValue AS Description, A.ResourceKey AS DeviceIdentifier FROM ResourceModel A " +
                    "JOIN ReadingDeviceModel B ON B.IsActive = 1 AND B.DeviceIdentifier = A.ResourceKey " +
                    "JOIN ReadingModel C ON C.IsActive = 1 AND B.ReadingID = C.ReadingID AND C.AllowDeviceData = 1 " +
                    "WHERE A.GroupName = ? AND A.IsActive = 1 AND A.ResourceKey NOT IN (SELECT DeviceIdentifier FROM PatientDeviceModel WHERE IsActive = 1)", GroupConstants.RS_SUPPORTED_DEVICE_GROUP).ConfigureAwait(false);
        }

        /// <summary>
        /// Get Paired device data from database
        /// </summary>
        /// <param name="deviceData">Ref object to store paired devices data</param>
        /// <returns>Paired devices with operation status</returns>
        public async Task GetPairedDeviceAsync(PatientDeviceDTO deviceData)
        {
            deviceData.Device = await SqlConnection.FindWithQueryAsync<PatientDeviceModel>
                ($"SELECT A.*, IFNULL({GetOrganisationEnabledDevicesQuery(string.Empty)}, 0) AS IsActive FROM PatientDeviceModel A " +
                        "WHERE A.ReadingSourceID = ? AND A.IsActive = 1", deviceData.Device.ReadingSourceID).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves supported devices which are already paired
        /// </summary>
        /// <param name="deviceData"></param>
        /// <returns>Operation status</returns>
        public async Task GetDeviceToFetchDataAsync(PatientDeviceDTO deviceData)
        {
            deviceData.PairdDevices = await SqlConnection.QueryAsync<PatientDeviceModel>
                ($"SELECT A.* FROM PatientDeviceModel A WHERE A.IsActive = 1 AND {GetOrganisationEnabledDevicesQuery(deviceData.ErrorDescription)} = 1").ConfigureAwait(false);
        }

        private string GetOrganisationEnabledDevicesQuery(string readingTypeIdentifier)
        {
            string readingTypeQuery = string.Empty;
           /* if (!string.IsNullOrWhiteSpace(readingTypeIdentifier))
            {
                readingTypeQuery = $"JOIN ReadingTypeModel D ON D.IsActive = 1 AND C.ReadingTypeID = D.ReadingTypeID AND D.ReadingTypeIdentifier IN ('{readingTypeIdentifier}') ";
            }*/
            return "(SELECT 1 FROM ReadingDeviceModel B " +
                        "JOIN ReadingModel C ON B.IsActive = 1 AND B.DeviceIdentifier = A.DeviceIdentifier " +
                        $"AND C.IsActive = 1 AND B.ReadingID = C.ReadingID AND C.AllowDeviceData = 1 {readingTypeQuery}LIMIT 1)";
        }

        private async Task GetPatientReadingDevicesToSyncWithServerAsync(PatientReadingDTO readingData)
        {
            readingData.PatientReadingDevices = await SqlConnection.QueryAsync<PatientDeviceModel>
                ("SELECT * FROM PatientDeviceModel WHERE IsSynced = 0").ConfigureAwait(false);
        }

        /// <summary>
        /// Update reading source sync status
        /// </summary>
        /// <param name="readingData">data to update sync status</param>
        public async Task UpdatePatientReadingsSourcesSyncStatusAsync(PatientReadingDTO readingData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (PatientDeviceModel reading in readingData.PatientReadingDevices)
                {
                    SaveResultModel result = readingData.SaveReadingSources?.FirstOrDefault(x => x.ClientGuid == reading.ReadingSourceID);
                    reading.ErrCode = result == null ? ErrorCode.OK : result.ErrCode;
                    switch (reading.ErrCode)
                    {
                        case ErrorCode.OK:
                            if (result == null || result.ClientGuid == result.ServerGuid)
                            {
                                // Data is successfully synced, so only update sync flag and EmpID received from server
                                transaction.Execute("UPDATE PatientDeviceModel SET IsSynced = 1, ErrCode = ? WHERE ReadingSourceID = ?", reading.ErrCode, reading.ReadingSourceID);
                            }
                            else
                            {
                                // When Data is not present for both IDs (ClientGUID & ServerGUID), then UPDATE ClientGUID with ServerGUID in Local DB
                                // When Data is present for both IDs (ClientGUID & ServerGUID), then DELETE data of ServerGuid from local DB, and UPDATE ClientGUID with ServerGUID in Local DB
                                if (transaction.FindWithQuery<PatientDeviceModel>("SELECT 1 FROM PatientDeviceModel WHERE ReadingSourceID = ?", result.ServerGuid) != null)
                                {
                                    // DELETE data of ServerGuid from local DB
                                    transaction.Execute("DELETE FROM PatientDeviceModel WHERE ReadingSourceID = ?", result.ServerGuid);
                                }
                                transaction.Execute("UPDATE PatientReadingModel SET ReadingSourceID = ?, IsSynced = 0 WHERE ReadingSourceID = ?", result.ServerGuid, reading.ReadingSourceID);
                                //// UPDATE ClientGUID with ServerGUID in Local DB
                                transaction.Execute("UPDATE PatientDeviceModel SET ReadingSourceID = ?, IsSynced = 1, ErrCode = ? WHERE ReadingSourceID = ?", result.ServerGuid, reading.ErrCode, reading.ReadingSourceID);
                                //This is to sync updated reading against updated ReadingSourceID
                                readingData.ErrCode = ErrorCode.DuplicateGuid;
                            }
                            break;
                        case ErrorCode.DuplicateGuid:
                            // UPDATE ClientGUID with new Guid
                            Guid newGuid = GenerateNewGuid(transaction);
                            transaction.Execute("UPDATE PatientReadingModel SET ReadingSourceID = ?, IsSynced = 0 WHERE ReadingSourceID = ?", newGuid, reading.ReadingSourceID);
                            transaction.Execute("UPDATE PatientDeviceModel SET ReadingSourceID = ?, IsSynced = 0 WHERE ReadingSourceID = ?", newGuid, reading.ReadingSourceID);
                            readingData.ErrCode = reading.ErrCode;
                            break;
                        default:
                            // Mark record with the received error code
                            transaction.Execute("UPDATE PatientDeviceModel SET ErrCode = ? WHERE ReadingSourceID = ?", reading.ErrCode, reading.ReadingSourceID);
                            break;
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get Paired devices data from database
        /// </summary>
        /// <param name="deviceData">Ref object to store paired devices data</param>
        /// <returns>Paired devices with operation status</returns>
        public async Task GetPairedDevicesAsync(PatientDeviceDTO deviceData)
        {
            string query = deviceData.RecordCount == 0 ? "" : $" LIMIT {deviceData.RecordCount}";
            deviceData.Devices = await SqlConnection.QueryAsync<PatientDeviceModel>
                ($"SELECT ReadingSourceID, DeviceUUID, LastReadingID, DeviceIdentifier FROM PatientDeviceModel WHERE IsActive = 1{query}").ConfigureAwait(false);
        }

        /// <summary>
        /// Saves device data into database
        /// </summary>
        /// <param name="deviceData">Ref object to store data in DB and return operation status</param>
        /// <returns>Operation status</returns>
        public async Task UpdateDeviceDataAsync(PatientDeviceDTO deviceData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (var device in deviceData.PairdDevices)
                {
                    if (device.RecordCount > 0)
                    {
                        transaction.Execute($"UPDATE PatientDeviceModel SET LastReadingID = ?, LastSyncDateTime = ?, IsSynced = 0 WHERE DeviceIdentifier = ? AND DeviceSerialNo = ?",
                            device.LastReadingID, device.LastSyncDateTime, device.DeviceIdentifier, device.DeviceSerialNo);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove device from db
        /// </summary>
        /// <param name="deviceData">Device data to remove and store operation status</param>
        /// <returns>Operation status</returns>
        public async Task RemoveDeviceAsync(PatientDeviceDTO deviceData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                transaction.Execute($"UPDATE PatientDeviceModel SET IsActive = 0, IsSynced = 0 WHERE ReadingSourceID = ?", deviceData.Device.ReadingSourceID);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Save Readings and Reading Ranges to local database
        /// </summary>
        /// <param name="readingData">Reference object of Readings And ReadingRanges</param>
        /// <returns>operation status</returns>
        public async Task SavePatientReadingDevicesAsync(PatientReadingDTO readingData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SavePatientReadingDevices(readingData, transaction);
            }).ConfigureAwait(false);
        }

        private void SaveReadingDevices(PatientReadingDTO readingData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(readingData.ReadingDevices))
            {
                foreach (ReadingDeviceModel device in readingData.ReadingDevices.OrderBy(x => x.IsActive))
                {
                    //if (transaction.FindWithQuery<ReadingDeviceModel>($"SELECT 1 FROM ReadingDeviceModel WHERE ReadingTypeID = ? AND DeviceIdentifier = ?", deviceData.ReadingTypeID, deviceData.DeviceIdentifier) == null)
                    //{
                    //    transaction.Execute($"INSERT INTO ReadingDeviceModel (ReadingTypeID, DeviceIdentifier, IsActive) VALUES(?, ?, ?)",
                    //        deviceData.ReadingTypeID, deviceData.DeviceIdentifier, deviceData.IsActive);
                    //}
                    //else
                    //{
                    //    transaction.Execute($"UPDATE ReadingDeviceModel SET IsActive = ? WHERE ReadingTypeID = ? AND DeviceIdentifier = ?",
                    //       deviceData.IsActive, deviceData.ReadingTypeID, deviceData.DeviceIdentifier);
                    //}
                }
            }
        }

        private void SavePatientReadingDevices(PatientReadingDTO readingData, SQLiteConnection transaction)
        {
            //todo : uncomment when implemented
            if (GenericMethods.IsListNotEmpty(readingData.PatientReadingDevices))
            {
                var logedInUserID = Preferences.Get(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)default);
                foreach (PatientDeviceModel deviceData in readingData.PatientReadingDevices.OrderBy(x => x.IsActive))
                {
                    PatientDeviceModel patientDevice = transaction.FindWithQuery<PatientDeviceModel>(
                        "SELECT * FROM PatientDeviceModel WHERE DeviceIdentifier = ? AND DeviceSerialNo = ? ", deviceData.DeviceIdentifier, deviceData.DeviceSerialNo);
                    if (patientDevice == null)
                    {
                        transaction.Execute("INSERT INTO PatientDeviceModel (ReadingSourceID, UserID, DeviceUUID, DeviceIdentifier, NativeDeviceAddress, " +
                            "ModelNumber, DeviceSerialNo, DeviceFirmwareVersion, SoftwareRevision, LastReadingID, LastSyncDateTime, IsActive, IsSynced) " +
                            "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                         deviceData.ReadingSourceID, logedInUserID, deviceData.DeviceUUID, deviceData.DeviceIdentifier, deviceData.NativeDeviceAddress,
                         deviceData.ModelNumber, deviceData.DeviceSerialNo, deviceData.DeviceFirmwareVersion, deviceData.SoftwareRevision,
                         deviceData.LastReadingID, deviceData.LastSyncDateTime, deviceData.IsActive, deviceData.IsSynced);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(deviceData.DeviceUUID) || deviceData.DeviceUUID == Guid.Empty.ToString())
                        {
                            deviceData.DeviceUUID = patientDevice.DeviceUUID;
                            deviceData.LastSyncDateTime = patientDevice.LastSyncDateTime;
                        }
                        else
                        {
                            if (patientDevice.LastSyncDateTime.ToUnixTimeSeconds() > 0)
                            {
                                deviceData.LastSyncDateTime = patientDevice.LastSyncDateTime;
                            }
                        }
                        deviceData.ReadingSourceID = patientDevice.ReadingSourceID;
                        transaction.Execute($"UPDATE PatientDeviceModel SET NativeDeviceAddress = ?, DeviceUUID = ?, ModelNumber = ?, DeviceFirmwareVersion = ?, SoftwareRevision = ?, LastReadingID = ?, LastSyncDateTime = ?, IsActive = ?, IsSynced = ? WHERE DeviceIdentifier = ? AND DeviceSerialNo = ?",
                            deviceData.NativeDeviceAddress, deviceData.DeviceUUID, deviceData.ModelNumber, deviceData.DeviceFirmwareVersion, deviceData.SoftwareRevision, deviceData.LastReadingID, deviceData.LastSyncDateTime, deviceData.IsActive, deviceData.IsSynced, deviceData.DeviceIdentifier, deviceData.DeviceSerialNo);
                    }
                }
            }
        }
    }
}