using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading common mapping implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    public List<PatientDeviceModel> MapDevices(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select new PatientDeviceModel
               {
                   UserID = GetDataItem<long>(dataItem, nameof(PatientDeviceModel.UserID)),
                   ReadingSourceID = (Guid)dataItem[nameof(PatientDeviceModel.ReadingSourceID)],
                   DeviceIdentifier = (string)dataItem[nameof(PatientDeviceModel.DeviceIdentifier)],
                   DeviceSerialNo = (string)dataItem[nameof(PatientDeviceModel.DeviceSerialNo)],
                   DeviceFirmwareVersion = (string)dataItem[nameof(PatientDeviceModel.DeviceFirmwareVersion)],
                   LastReadingID = (long)dataItem[nameof(PatientDeviceModel.LastReadingID)],
                   LastSyncDateTime = (DateTimeOffset)dataItem[nameof(PatientDeviceModel.LastModifiedON)],
                   IsActive = (bool)dataItem[nameof(PatientDeviceModel.IsActive)],
                   IsSynced = true,
                   ErrCode = ErrorCode.OK
               }).ToList()
            : null;
    }

    public List<ReadingDeviceModel> MapOrganisationDevices(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0) ?
                   (from dataItem in data[collectionName]
                    select new ReadingDeviceModel
                    {
                        ReadingTypeID = (long)dataItem[nameof(ReadingDeviceModel.ReadingTypeID)],
                        DeviceIdentifier = (string)dataItem[nameof(ReadingDeviceModel.DeviceIdentifier)],
                        IsActive = (bool)dataItem[nameof(ReadingDeviceModel.IsActive)],
                    }).ToList() : null;
    }
}