using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Device service
/// </summary>
public class DeviceService : BaseService, IDisposable
{
    /// <summary>
    /// Event to detect when a device is found in scan
    /// </summary>
    public EventHandler OnDeviceFound { get; set; }

    //Todo:
    //private DeviceManager _deviceManager;
    private readonly ReadingDatabase _devicesDatabase;

    /// <summary>
    /// Device service
    /// </summary>
    public DeviceService(IEssentials serviceEssentials) : base(serviceEssentials)
    {
        _devicesDatabase = new ReadingDatabase();
    }

    /// <summary>
    /// Get all supported devices and paired devices
    /// </summary>
    /// <param name="deviceData">Object to store device data</param>
    /// <returns>Return supported devices, paired devices and operation status</returns>
    public async Task GetDevicesToPairAsync(PatientDeviceDTO deviceData)
    {
        try
        {
            BaseService baseService = new BaseService(_essentials);
            await Task.WhenAll(
                 GetResourcesAsync(GroupConstants.RS_MENU_PAGE_GROUP, GroupConstants.RS_DEVICE_GROUP, GroupConstants.RS_SUPPORTED_DEVICE_GROUP),
                 baseService.GetFeaturesAsync(AppPermissions.SupportedDevicesView.ToString()),
                 GetSettingsAsync(GroupConstants.RS_DEVICE_GROUP),
                 _devicesDatabase.GetDevicesToPairAsync(deviceData)
            ).ConfigureAwait(false);
            PageData.FeaturePermissions = baseService.PageData.FeaturePermissions;
            if (deviceData.Devices?.Count > 0)
            {
                deviceData.Devices.ForEach(device =>
                {
                    //Todo:
                    //device.DeviceImage = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(GetDeviceIcon(device.DeviceIdentifier))));
                    device.DeviceArrowIcon = ImageConstants.I_MORE_NAV_PNG;
                });
            }
            deviceData.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            deviceData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Get Device Image Base64 string
    /// </summary>
    /// <param name="deviceName">Name of device</param>
    /// <returns>Return Device Image Base64 string based on DeviceName</returns>
    public string GetDeviceIcon(string deviceName)
    {
        //Todo:
        //switch (deviceName.ToEnum<AlphaMDHealth.Model.Device>())
        //{
        //    case AlphaMDHealth.Model.Device.MIBCS:
        //        return Constants.MIBCS_IMAGE;
        //    case AlphaMDHealth.Model.Device.Medisana:
        //        return Constants.MEDISANA_IMAGE;
        //    case AlphaMDHealth.Model.Device.Contour:
        //    case AlphaMDHealth.Model.Device.Contour_Next_One:
        //        return Constants.CONTOUR_IMAGE;
        //    case AlphaMDHealth.Model.Device.BeurerGL50Evo:
        //        return Constants.BEURER_GL50EVO_IMAGE;
        //    case AlphaMDHealth.Model.Device.BeurerBM57:
        //        return Constants.BEURER_BM57_IMAGE;
        //    case AlphaMDHealth.Model.Device.A6_BT:
        //        return Constants.A6_BT_IMAGE;
        //    case AlphaMDHealth.Model.Device.Contec_CMS50D_BT:
        //        return Constants.CONTEC_CMS50D_BT_IMAGE;
        //    default:
        //        return string.Empty;
        //}
        return string.Empty;
    }

    /// <summary>
    /// Get all paired devices
    /// </summary>
    /// <param name="deviceData">Object to store devices data</param>
    /// <returns>Return paired devices and operation status</returns> 
    public async Task GetPairedDevicesAsync(PatientDeviceDTO deviceData)
    {
        try
        {
            await Task.WhenAll(
                GetResourcesAsync(GroupConstants.RS_DEVICE_GROUP, GroupConstants.RS_SUPPORTED_DEVICE_GROUP),
                _devicesDatabase.GetPairedDevicesAsync(deviceData)
            ).ConfigureAwait(false);
            if (deviceData.Devices?.Count > 0)
            {
                deviceData.Devices.ForEach(device =>
                {
                    device.Name = LibResources.GetResourceValueByKey(PageData?.Resources, device.DeviceIdentifier);
                    device.Description = LibResources.GetResourceByKey(PageData?.Resources, device.DeviceIdentifier).PlaceHolderValue;
                    //Todo:
                    //device.DeviceImage = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(GetDeviceIcon(device.DeviceIdentifier))));
                    device.DeviceArrowIcon = ImageConstants.I_MORE_NAV_PNG;
                });
            }
            else
            {
                deviceData.Devices = new List<PatientDeviceModel>();
            }
            deviceData.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            deviceData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Gets Device details from DB to Display in UI
    /// </summary>
    /// <param name="deviceData">Reference object containg required info</param>
    /// <returns>Device info with operation status</returns>
    public async Task GetPairedDeviceAsync(PatientDeviceDTO deviceData)
    {
        try
        {
            await _devicesDatabase.GetPairedDeviceAsync(deviceData).ConfigureAwait(false);
            deviceData.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            deviceData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Clears device manager instance to clean up on-going operations
    /// </summary>
    public async Task ClearDeviceManagerAsync()
    {
        //Todo:
        //_deviceManager?.Dispose();
        //_deviceManager = null;
        await Task.Delay(50).ConfigureAwait(false);
    }

    /// <summary>
    /// Check if device is paired for given observation type
    /// </summary>
    /// <param name="readingType">Type to get paired device
    /// </param>
    /// <returns>Flag which returns Paired device is available or not
    /// 0 -> Device is not paired yet
    /// 1 -> Device pairing is done
    /// </returns>
    public async Task<int> IsDevicePairedForReadingsAsync(string readingType)
    {
        try
        {
            // ErrorDescription is used to identify devices of the given reading type identifier
            var deviceData = new PatientDeviceDTO { ErrorDescription = readingType };
            await _devicesDatabase.GetDeviceToFetchDataAsync(deviceData).ConfigureAwait(false);
            return deviceData.PairdDevices?.Count > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
        }
        return 0;
    }

    /// <summary>
    /// Sync observation based on observation type
    /// </summary>
    /// <param name="returnResult">Variable which holds Operation status</param>
    /// <param name="readingType">Type of observation to be synced</param>
    /// <returns>Operation result</returns>
    public async Task SyncReadingsFromDevicesAsync(BaseDTO returnResult, string readingType)
    {
        try
        {
            // Get list of supported and connected device for given observation type to sync data
            // ErrorDescription is used to identify devices of the given reading type identifier
            var deviceData = new PatientDeviceDTO { ErrorDescription = readingType };
            await _devicesDatabase.GetDeviceToFetchDataAsync(deviceData).ConfigureAwait(false);
            if (deviceData?.PairdDevices?.Count > 0)
            {
                await GetObservationsFromPairedDeviceAsync(deviceData, false).ConfigureAwait(false);
                returnResult.RecordCount = deviceData.RecordCount;
                returnResult.ErrCode = deviceData.ErrCode;
            }
            else
            {
                returnResult.ErrCode = ErrorCode.NotFound;
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            returnResult.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Sync readings from each devices available in reference device instance
    /// </summary>
    /// <param name="devicesData">Reference object containing list of devices to fetch data</param>
    /// <returns>Operation status</returns>
    public async Task SyncReadingsFromDevicesAsync(PatientDeviceDTO devicesData)
    {
        try
        {
            if (devicesData?.PairdDevices?.Count > 0)
            {
                await GetObservationsFromPairedDeviceAsync(devicesData, false).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            devicesData = devicesData ?? new PatientDeviceDTO();
            devicesData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Scan device and connect if there is only one device selected
    /// </summary>
    /// <param name="devicesData">Object Holds Devices data and pairing Status</param>
    /// <returns>Operation Status based on connection</returns>
    public async Task PairDeviceAndFetchDataAsync(PatientDeviceDTO devicesData)
    {
        //Todo:
        //try
        //{
        //    DeviceManager bleDeviceManager = new DeviceManager(devicesData.Device.DeviceIdentifier.ToEnum<AlphaMDHealth.Model.Device>());
        //    var timeoutTime = Convert.ToInt32(await GetSettingsValueByKeyAsync(LibSettingsConstants.S_BLE_DEVICE_SCAN_TIMEOUT_KEY).ConfigureAwait(false), CultureInfo.InvariantCulture);
        //    AlphaMDHealth.Model.DeviceModel selectedDevice = null;
        //    if (string.IsNullOrWhiteSpace(devicesData.Device.DeviceUUID))
        //    {
        //        AlphaMDHealth.Model.DeviceDTO scanedDevices = await bleDeviceManager.SearchDevicesAsync(new List<AlphaMDHealth.Model.Device> { devicesData.Device.DeviceIdentifier.ToEnum<AlphaMDHealth.Model.Device>() }, timeoutTime, null).ConfigureAwait(false);
        //        if (scanedDevices.ErrCode == ErrorCode.OK)
        //        {
        //            var devices = (from device in scanedDevices.Devices
        //                           where devicesData.PairdDevices == null || !devicesData.PairdDevices.Any(x => x.DeviceUUID == device.DeviceID.ToString())
        //                           select device).ToList();
        //            if (devices?.Count > 0)
        //            {
        //                if (devices.Count == 1)
        //                {
        //                    selectedDevice = devices.First();
        //                    devicesData.Device.DeviceUUID = selectedDevice.DeviceID.ToString();
        //                    devicesData.Device.NativeDeviceAddress = selectedDevice.BleDevice.NativeDevice.ToString();
        //                }
        //                else
        //                {
        //                    foreach (var device in devices)
        //                    {
        //                        devicesData.Devices.Add(new PatientDeviceModel
        //                        {
        //                            Name = devicesData.Device.DeviceIdentifier,
        //                            DeviceUUID = device.DeviceID.ToString(),
        //                            Description = MobileConstants.IsIosPlatform ? GetiOSDeviceNativeAddress(device.BleDevice.NativeDevice.ToString()) : device.BleDevice.NativeDevice.ToString(),
        //                            //Todo:
        //                            //DeviceImage = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(GetDeviceIcon(devicesData.Device.DeviceIdentifier)))),
        //                            DeviceIdentifier = devicesData.Device.DeviceIdentifier,
        //                            DeviceArrowIcon = LibImageConstants.I_MORE_NAV_SVG_IMAGE
        //                        });
        //                    }
        //                    devicesData.ErrCode = ErrorCode.MultipleDevices;
        //                    return;
        //                }
        //            }
        //            else
        //            {
        //                devicesData.ErrCode = ErrorCode.DeviceNotFound;
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            devicesData.ErrCode = scanedDevices.ErrCode;
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        selectedDevice = new AlphaMDHealth.Model.DeviceModel
        //        {
        //            DeviceID = new Guid(devicesData.Device.DeviceUUID)
        //        };
        //    }

        //    ErrorCode deviceInfoStatus = await bleDeviceManager.GetDeviceInformationAsync(selectedDevice).ConfigureAwait(false);
        //    if (deviceInfoStatus == ErrorCode.OK)
        //    {
        //        devicesData.Device.DeviceFirmwareVersion = selectedDevice.FirmwareRevision;
        //        devicesData.Device.ModelNumber = selectedDevice.ModelNumber;
        //        devicesData.Device.DeviceSerialNo = selectedDevice.SerialNumber;
        //        devicesData.Device.SoftwareRevision = selectedDevice.SoftwareRevision;
        //        devicesData.PairdDevices = new List<PatientDeviceModel> { devicesData.Device };
        //        await GetObservationsFromPairedDeviceAsync(devicesData, true).ConfigureAwait(false);
        //    }
        //    else
        //    {
        //        devicesData.ErrCode = deviceInfoStatus;
        //    }
        //}
        //catch (Exception ex)
        //{
        //    devicesData.ErrCode = ErrorCode.NotFound;
        //    LogError(ex.Message, ex);
        //}
    }

    /// <summary>
    /// Update Last sync date time for given device 
    /// </summary>
    /// <param name="devicesData">device data to update last sync date time</param>
    /// <returns>Operation status</returns>
    public async Task SaveDeviceAndReadingsAsync(PatientDeviceDTO devicesData)
    {
        try
        {
            PatientDeviceModel device = devicesData.PairdDevices[0];
            device.IsActive = true;
            device.IsScaning = false;
            device.ReadingSourceID = GenericMethods.GenerateGuid();
            await SavePatientDeviceAsync(device).ConfigureAwait(false);
            if (devicesData.RecordCount > 0)
            {
                devicesData.ReadingData.ListData.ForEach(x => x.ReadingSourceID = device.ReadingSourceID);
            }
            await new ReadingService(_essentials).SavePatientReadingAsync(devicesData.ReadingData).ConfigureAwait(false);

            //todo:
            //await SendAcknowledgementToLibraryAsync(new List<AlphaMDHealth.Model.DeviceModel>
            //{
            //    new AlphaMDHealth.Model.DeviceModel
            //    {
            //        Device = device.DeviceIdentifier.ToEnum<AlphaMDHealth.Model.Device>(),
            //        DeviceID = new Guid(device.DeviceUUID)
            //    }
            //}).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            devicesData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
    }

    /// <summary>
    /// Remove device from DB also unpaire that device from mobile app
    /// </summary>
    /// <param name="deviceData">Device data to Remove</param>
    /// <returns>Operation status</returns>
    public async Task RemoveDeviceAsync(PatientDeviceDTO deviceData)
    {
        try
        {
            await _devicesDatabase.RemoveDeviceAsync(deviceData).ConfigureAwait(false);
            deviceData.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            deviceData.ErrCode = ErrorCode.ErrorWhileDeletingRecords;
        }
    }

    /// <summary>
    /// Save patient device to database
    /// </summary>
    /// <param name="patientDevice">Patient device to be saved</param>
    /// <returns>operation status</returns>
    protected Task SavePatientDeviceAsync(PatientDeviceModel patientDevice)
    {
        return _devicesDatabase.SavePatientReadingDevicesAsync(new PatientReadingDTO { PatientReadingDevices = new List<PatientDeviceModel> { patientDevice } });
    }

    private async Task GetObservationsFromPairedDeviceAsync(PatientDeviceDTO devicesData, bool isPairing)
    {
        //todo:
        //devicesData.ReadingData = new PatientReadingDTO
        //{
        //    ListData = new List<PatientReadingUIModel>(),
        //    ChartMetaData = new List<ReadingMetadataUIModel>(),
        //    SelectedUserID = GetLoginUserID()
        //};
        //devicesData.RecordCount = 0;
        //var lastSyncDate = LibGenericMethods.GetUtcDateTime;
        //List<AlphaMDHealth.Model.DeviceModel> successfulDevices = new List<AlphaMDHealth.Model.DeviceModel>();
        //bool deviceFound = false;
        //// Get observations from each device
        //foreach (PatientDeviceModel device in devicesData.PairdDevices)
        //{
        //    var deviceType = device.DeviceIdentifier.ToEnum<AlphaMDHealth.Model.Device>();
        //    DeviceManager bleDeviceManager = new DeviceManager(deviceType);
        //    AlphaMDHealth.Model.DeviceModel bleDevice = new AlphaMDHealth.Model.DeviceModel
        //    {
        //        DeviceSequence = device.LastReadingID,
        //        Device = deviceType
        //    };
        //    if (string.IsNullOrWhiteSpace(device.DeviceUUID))
        //    {
        //        // Device not yet paired in this mobile
        //        devicesData.ErrCode = await RepairDeviceAsync(device, bleDeviceManager).ConfigureAwait(false);
        //        if (devicesData.ErrCode == ErrorCode.OK)
        //        {
        //            bleDevice.DeviceSequence = device.LastReadingID;
        //        }
        //        else
        //        {
        //            device.ErrCode = devicesData.ErrCode;
        //            continue;
        //        }
        //    }
        //    else
        //    {
        //        bleDevice.DeviceID = new Guid(device.DeviceUUID);
        //    }
        //    HealthReadingDTO deviceObservationData = await bleDeviceManager.GetDataAsync(bleDevice).ConfigureAwait(false);
        //    if (deviceObservationData.ErrCode == ErrorCode.OK)
        //    {
        //        bool isDeviceFound = true;
        //        successfulDevices.Add(bleDevice);
        //        device.LastSyncDateTime = lastSyncDate;
        //        if (deviceObservationData.HealthReadings?.Count > 0)
        //        {
        //            // Update device sequence number
        //            device.LastReadingID = bleDevice.DeviceSequence;
        //            await MapDeviceReadingDataAsync(devicesData.ReadingData, deviceObservationData, device.ReadingSourceID.ToString(), isPairing).ConfigureAwait(false);
        //            isDeviceFound = HandleMapResponse(devicesData, device);
        //        }
        //        if (isDeviceFound)
        //        {
        //            deviceFound = true;
        //        }
        //    }
        //    else
        //    {
        //        device.ErrCode = deviceObservationData.ErrCode;
        //    }
        //    if (isPairing)
        //    {
        //        devicesData.ErrCode = deviceObservationData.ErrCode;
        //    }
        //}
        //await UpdateDeviceAndReadingsAsync(devicesData, isPairing, successfulDevices, deviceFound).ConfigureAwait(false);
    }

    private bool HandleMapResponse(PatientDeviceDTO devicesData, PatientDeviceModel device)
    {
        if (devicesData.ReadingData.ErrCode == ErrorCode.OK)
        {
            device.RecordCount = devicesData.ReadingData.ListData.GroupBy(x => x.ReadingDateTime).Count();
            devicesData.RecordCount += device.RecordCount;
            device.ErrCode = ErrorCode.OK;
            return true;
        }
        else
        {
            device.ErrCode = devicesData.ErrCode;
            return false;
        }
    }

    //Todo:
    //private async Task UpdateDeviceAndReadingsAsync(PatientDeviceDTO devicesData, bool isPairing, List<AlphaMDHealth.Model.DeviceModel> successfulDevices, bool deviceFound)
    //{
    //    if (!isPairing)
    //    {
    //        devicesData.ErrCode = deviceFound ? ErrorCode.OK : devicesData.PairdDevices.First(x => x.ErrCode != ErrorCode.OK && x.ErrCode != 0).ErrCode;
    //        if (devicesData.ReadingData.ListData?.Count > 0)
    //        {
    //            await Task.WhenAll(
    //                    new ReadingService().SavePatientReadingAsync(devicesData.ReadingData),
    //                    _devicesDatabase.UpdateDeviceDataAsync(devicesData)
    //                ).ConfigureAwait(false);
    //            await SendAcknowledgementToLibraryAsync(successfulDevices).ConfigureAwait(false);
    //        }
    //    }
    //}

    //Todo:
    //private async Task<ErrorCode> RepairDeviceAsync(PatientDeviceModel device, DeviceManager bleDeviceManager)
    //{
    //    // device obtained from server
    //    // Scan for device of same type

    //    AlphaMDHealth.Model.DeviceDTO scannedDevices = await bleDeviceManager
    //        .SearchDevicesAsync(new List<AlphaMDHealth.Model.Device> { device.DeviceIdentifier.ToEnum<AlphaMDHealth.Model.Device>() },
    //        Convert.ToInt32(await GetSettingsValueByKeyAsync(LibSettingsConstants.S_BLE_DEVICE_SCAN_TIMEOUT_KEY).ConfigureAwait(false), CultureInfo.InvariantCulture), null).ConfigureAwait(false);

    //    if (scannedDevices.ErrCode == ErrorCode.OK && scannedDevices.Devices?.Count > 0)
    //    {
    //        var selectedDevice = scannedDevices.Devices.First();
    //        device.DeviceUUID = selectedDevice.DeviceID.ToString();
    //        device.NativeDeviceAddress = selectedDevice.BleDevice.NativeDevice.ToString();
    //        // If found get its information
    //        scannedDevices.ErrCode = await bleDeviceManager.GetDeviceInformationAsync(selectedDevice).ConfigureAwait(false);
    //        if (scannedDevices.ErrCode == ErrorCode.OK)
    //        {
    //            device.DeviceFirmwareVersion = selectedDevice.FirmwareRevision;
    //            device.SoftwareRevision = selectedDevice.SoftwareRevision;
    //            if (device.DeviceSerialNo != selectedDevice.SerialNumber)
    //            {
    //                // Deactivate old device
    //                await _devicesDatabase.RemoveDeviceAsync(new PatientDeviceDTO { Device = device }).ConfigureAwait(false);

    //                // Update device data to store as a new device
    //                device.LastReadingID = 0;
    //                device.ReadingSourceID = LibGenericMethods.GenerateGuid();
    //                device.DeviceSerialNo = selectedDevice.SerialNumber;
    //                device.ModelNumber = selectedDevice.ModelNumber;
    //            }
    //            // -> Update information in db
    //            await SavePatientDeviceAsync(device).ConfigureAwait(false);
    //        }
    //        return scannedDevices.ErrCode;
    //    }
    //    else
    //    {
    //        // If not found, return not found
    //        return ErrorCode.DeviceNotFound;
    //    }
    //}

    //private async Task SendAcknowledgementToLibraryAsync(List<AlphaMDHealth.Model.DeviceModel> successfulDevices)
    //{
    //    foreach (var device in successfulDevices)
    //    {
    //        // Send acknowledgement back to MiScale 2/Contec CMS50D-BT as data has been saved by the app successfully and can be deleted from MiScale 2/Contec CMS50D-BT
    //        if (device.Device == AlphaMDHealth.Model.Device.MIBCS || device.Device == AlphaMDHealth.Model.Device.Contec_CMS50D_BT)
    //        {
    //            DeviceManager bleDeviceManager = new DeviceManager(device.Device);
    //            await bleDeviceManager.ClearDataAsync(device).ConfigureAwait(false);
    //        }
    //    }
    //}

    //Todo:
    private async Task MapDeviceReadingDataAsync(PatientReadingDTO fetchedObservations, HealthReadingDTO deviceObservationData, string readingSourceID, bool isPairing)
    {
        PatientReadingDTO metaData = new PatientReadingDTO();
        ReadingService readingService = new ReadingService(_essentials);
        metaData.AddedBy = Constants.NUMBER_TWO;
        await readingService.GetHealthAppReadingsMetaDataAsync(metaData).ConfigureAwait(false);
        if (metaData.ErrCode == ErrorCode.OK && metaData.ChartMetaData?.Count > 0)
        {
            await GetSettingsAsync(GroupConstants.RS_READING_RELATION_GROUP).ConfigureAwait(false);
            foreach (var deviceObservation in deviceObservationData.HealthReadings)
            {
                MapDeviceReading(fetchedObservations, readingSourceID, isPairing, metaData, deviceObservation);
            }
        }
        else
        {
            fetchedObservations.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    private void MapDeviceReading(PatientReadingDTO fetchedObservations, string readingSourceID, bool isPairing, PatientReadingDTO metaData, HealthReadingModel deviceObservation)
    {
        string readingTypeIdentifier = LibSettings.GetSettingValueByKey(PageData?.Settings, GetReadingTypeFromIdentifier(deviceObservation.ReadingType).ToString());
        string readingIdentifier = GetReadingIdentifier(deviceObservation);
        //ReadingMetadataUIModel currentMetadata = metaData.ChartMetaData.FirstOrDefault(x => x.ReadingTypeIdentifier == readingTypeIdentifier && x.ReadingIdentifier == readingIdentifier);
        //if (currentMetadata != null)
        //{
        //    if (!isPairing || deviceObservation.CreatedOn >= LibGenericMethods.GetUtcDateTime.AddDays(-currentMetadata.DaysOfPastRecordsToSync))
        //    {
        //        fetchedObservations.ListData.Add(new PatientReadingUIModel
        //        {
        //            PatientReadingID = LibGenericMethods.GenerateGuid().ToString(),
        //            AddedByID = Convert.ToString(fetchedObservations.SelectedUserID, CultureInfo.InvariantCulture),
        //            ReadingID = currentMetadata.ReadingID,
        //            ReadingTypeID = currentMetadata.ReadingTypeID,
        //            ReadingTypeIdentifier = readingTypeIdentifier,
        //            ReadingDateTime = deviceObservation.CreatedOn,
        //            ReadingIdentifier = readingTypeIdentifier,
        //            ReadingValue = deviceObservation.ReadingValue,
        //            UnitIdentifier = deviceObservation.ReadingUnit.ToString(),
        //            ReadingSourceType = ReadingSource.Device.ToString(),
        //            ReadingSourceID = readingSourceID,
        //            ReadingNotes = string.Empty,
        //            IsActive = true
        //        });
        //    }
        //    if (!fetchedObservations.ChartMetaData.Any(x => x.ReadingTypeIdentifier == readingTypeIdentifier && x.ReadingIdentifier == readingIdentifier))
        //    {
        //        fetchedObservations.ChartMetaData.Add(currentMetadata);
        //    }
        //}
    }

    private string GetiOSDeviceNativeAddress(string xmlString)
    {
        string[] per = xmlString.Split(Constants.SYMBOL_COMMA);
        var value = per.Where(x => x.Contains("identifier"))?.First();
        value = string.IsNullOrEmpty(value) ? string.Empty : value.Split(Constants.SYMBOL_EQUAL[0])[1].Trim();
        return value;
    }

    private ReadingType GetReadingTypeFromIdentifier(ReadingType readingType)
    {
        switch (readingType)
        {
            case ReadingType.BPSystolic:
            case ReadingType.BPDiastolic:
                return ReadingType.BloodPressure;
            default:
                return readingType;
        }
    }

    private string GetReadingIdentifier(HealthReadingModel deviceObservation)
    {
        return LibSettings.GetSettingValueByKey(PageData?.Settings, deviceObservation.ReadingType == ReadingType.BloodGlucose ? deviceObservation.ReadingMoment.ToString() : deviceObservation.ReadingType.ToString());
    }

    /// <summary>
    /// Dispose method
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose method
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        _ = ClearDeviceManagerAsync();
    }
}