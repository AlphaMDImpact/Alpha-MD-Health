using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Patient reading implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    /// <summary>
    /// Maps json object into Model and saves data into DB
    /// </summary>
    /// <param name="result">Object which holds Operation status</param>
    /// <param name="data">json object to fetch data from it</param>
    /// <returns>Operation status with record count</returns>
    internal async Task MapAndSaveReadingsAsync(DataSyncModel result, JToken data)
    {
        try
        {
            PatientReadingDTO readingData = new PatientReadingDTO
            {
                Readings = MapReadings(data, nameof(DataSyncDTO.Readings)),
                ReadingRanges = MapReadingRanges(data, nameof(DataSyncDTO.ReadingRanges)),
                PatientReadingTargets = MapPatientReadingTargets(data, nameof(DataSyncDTO.PatientReadingTargets)),
                //readingData.ReadingDevices = MapOrganisationDevices(data, nameof(DataSyncDTO.ReadingDevices));
                PatientReadingDevices = MapDevices(data, nameof(DataSyncDTO.PatientReadingDevices)),
                PatientReadings = MapPatientReadings(data, nameof(DataSyncDTO.PatientReadings))
            };
            if (GenericMethods.IsListNotEmpty(readingData.Readings) || GenericMethods.IsListNotEmpty(readingData.ReadingRanges)
                || GenericMethods.IsListNotEmpty(readingData.PatientReadingTargets) //|| LibGenericMethods.IsListNotEmpty(readingData.ReadingDevices)
                || GenericMethods.IsListNotEmpty(readingData.PatientReadingDevices) || GenericMethods.IsListNotEmpty(readingData.PatientReadings))
            {
                await _readingDB.SaveReadingsAsync(readingData).ConfigureAwait(false);
                result.RecordCount = 1;
            }
            result.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    public async Task MapAndSaveReadingMasterData(DataSyncModel result, JToken data)
    {
        try
        {
            PatientReadingDTO readingData = new PatientReadingDTO
            {
                ReadingMasters = MapReadingMasters(data, nameof(DataSyncDTO.ReadingMasters)),
            };
            if (GenericMethods.IsListNotEmpty(readingData.ReadingMasters))
            {
                await _readingDB.SaveReadingMastersAsync(readingData).ConfigureAwait(false);
                result.RecordCount = 1;
            }
            result.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Maps program reading data from json to model
    /// </summary>
    /// <param name="data">json data</param>
    /// <param name="collectionName">collection in json</param>
    /// <returns>list of program reading data</returns>
    public List<ReadingMasterModel> MapReadingMasters(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select MapReadingMaster(dataItem)).ToList()
            : null;
    }

    private ReadingMasterModel MapReadingMaster(JToken dataItem)
    {
        return new ReadingMasterModel
        {
            ReadingParentID = GetDataItem<short>(dataItem, nameof(ReadingMasterModel.ReadingParentID)),
            ReadingCategoryID = GetDataItem<short>(dataItem, nameof(ReadingMasterModel.ReadingCategoryID)),
            ReadingID = GetDataItem<short>(dataItem, nameof(ReadingMasterModel.ReadingID)),
            IsGroupValue = GetDataItem<bool>(dataItem, nameof(ReadingMasterModel.IsGroupValue)),
            UnitGoupID = GetDataItem<short>(dataItem, nameof(ReadingMasterModel.UnitGoupID)),
            ReadingValueTypeID = GetDataItem<short>(dataItem, nameof(ReadingMasterModel.ReadingValueTypeID)),
            ReadingFilters = GetDataItem<string>(dataItem, nameof(ReadingMasterModel.ReadingFilters)),
            DaysOfPastRecordsToSync = GetDataItem<short>(dataItem, nameof(ReadingMasterModel.DaysOfPastRecordsToSync)),
            IsActive = GetDataItem<bool>(dataItem, nameof(ReadingMasterModel.IsActive)),
        };
    }

    /// <summary>
    /// Sync Readings Data to server
    /// </summary>
    /// <param name="readingData">object to sync readings and return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status call</returns>
    internal async Task SyncPatientReadingsToServerAsync(PatientReadingDTO readingData, CancellationToken cancellationToken)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await _readingDB.GetPatientReadingsAndDevicesToSyncWithServerAsync(readingData).ConfigureAwait(false);
            }
            if (GenericMethods.IsListNotEmpty(readingData.PatientReadings) || GenericMethods.IsListNotEmpty(readingData.PatientReadingDevices))
            {
                var httpData = new HttpServiceModel<PatientReadingDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PATIENT_READINGS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() }
                    },
                    ContentToSend = readingData,
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                readingData.ErrCode = httpData.ErrCode;
                if (readingData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data?.HasValues == true)
                    {
                        readingData.SaveReadingSources = MapSaveResponse(data, nameof(PatientReadingDTO.SaveReadingSources));
                        readingData.SaveReadings = MapSaveResponse(data, nameof(PatientReadingDTO.SaveReadings));
                        if (MobileConstants.IsMobilePlatform)
                        {
                            await _readingDB.UpdatePatientReadingsSyncStatusAsync(readingData).ConfigureAwait(false);
                            ErrorCode errCode = readingData.ErrCode;
                            await _readingDB.UpdatePatientReadingsSourcesSyncStatusAsync(readingData).ConfigureAwait(false);
                            if (errCode == ErrorCode.DuplicateGuid)
                            {
                                readingData.ErrCode = errCode;
                            }
                        }
                        else
                        {
                            CheckResultStatus(readingData);
                        }
                        if (readingData.ErrCode == ErrorCode.DuplicateGuid)
                        {
                            readingData.ErrCode = ErrorCode.OK;
                            await SyncPatientReadingsToServerAsync(readingData, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            readingData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Update reading sync status in main dto
    /// </summary>
    /// <param name="readingData">data to update sync status</param>
    public void CheckResultStatus(PatientReadingDTO readingData)
    {
        foreach (PatientReadingModel reading in readingData.PatientReadings)
        {
            SaveResultModel result = readingData.SaveReadings?.FirstOrDefault(x => x.ClientGuid == reading.PatientReadingID);
            reading.ErrCode = result == null ? ErrorCode.OK : result.ErrCode;
            switch (reading.ErrCode)
            {
                case ErrorCode.OK:
                    // do not do anything
                    break;
                case ErrorCode.DuplicateGuid:
                    // UPDATE ClientGUID with new Guid
                    reading.PatientReadingID = GenericMethods.GenerateGuid();
                    readingData.ErrCode = reading.ErrCode;
                    break;
                default:
                    readingData.ErrCode = reading.ErrCode;
                    break;
            }
        }
    }

    /// <summary>
    /// Fetch List of Readings
    /// </summary>
    /// <param name="readingsData">search item text</param>
    /// <returns>List of Readings</returns>
    public async Task GetPatientOverviewReading(PatientReadingDTO readingsData)
    {
        await _readingDB.GetPatientOverviewReadingAsync(readingsData).ConfigureAwait(false);
    }

    private List<PatientReadingUIModel> MapPatientReadingUIModels(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
             ? (from dataItem in data[collectionName]
                select new PatientReadingUIModel
                {
                    PatientReadingID = GetDataItem<Guid>(dataItem, nameof(PatientReadingUIModel.PatientReadingID)),
                    PatientTaskID = GetDataItem<long>(dataItem, nameof(PatientReadingUIModel.PatientTaskID)),
                    ReadingID = GetDataItem<short>(dataItem, nameof(PatientReadingUIModel.ReadingID)),
                    ReadingSourceID = GetDataItem<Guid>(dataItem, nameof(PatientReadingUIModel.ReadingSourceID)),
                    ReadingValue = GetDataItem<double?>(dataItem, nameof(PatientReadingUIModel.ReadingValue)),
                    ReadingValue2 = GetDataItem<string>(dataItem, nameof(PatientReadingUIModel.ReadingValue2)),
                    ReadingDateTime = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientReadingUIModel.ReadingDateTime)),
                    Reading = GetDataItem<string>(dataItem, nameof(PatientReadingUIModel.Reading)),
                    Unit = GetDataItem<string>(dataItem, nameof(PatientReadingUIModel.Unit)),
                    ReadingNotes = GetDataItem<string>(dataItem, nameof(PatientReadingUIModel.ReadingNotes)),
                    SourceName = GetDataItem<string>(dataItem, nameof(PatientReadingUIModel.SourceName)),
                    SourceQuantity = GetDataItem<double?>(dataItem, nameof(PatientReadingUIModel.SourceQuantity)),
                    AddedON = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientReadingUIModel.AddedON)),
                    AddedByID = (string)dataItem[nameof(PatientReadingUIModel.AddedByID)],
                    ReadingSourceType = GetDataItem<string>(dataItem, nameof(PatientReadingUIModel.ReadingSourceType)),
                    SequenceNo = GetDataItem<byte>(dataItem, nameof(PatientReadingUIModel.SequenceNo))
                }).ToList()
             : new List<PatientReadingUIModel>();
    }

    private List<PatientReadingModel> MapPatientReadings(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select new PatientReadingModel
               {
                   PatientReadingID = (Guid)dataItem[nameof(PatientReadingModel.PatientReadingID)],
                   UserID = (long)dataItem[nameof(PatientReadingModel.UserID)],
                   ReadingID = (short)dataItem[nameof(PatientReadingModel.ReadingID)],
                   SourceName = GetDataItem<string>(dataItem, nameof(PatientReadingModel.SourceName)),
                   SourceQuantity = GetDataItem<double?>(dataItem, nameof(PatientReadingModel.SourceQuantity)),
                   ReadingValue = GetDataItem<double?>(dataItem, nameof(PatientReadingModel.ReadingValue)),
                   ReadingValue2 = GetDataItem<string>(dataItem, nameof(PatientReadingModel.ReadingValue2)),
                   ReadingDateTime = (DateTimeOffset)dataItem[nameof(PatientReadingModel.ReadingDateTime)],
                   ReadingNotes = (string)dataItem[nameof(PatientReadingModel.ReadingNotes)],
                   ReadingSourceType = GetDataItem<string>(dataItem, nameof(PatientReadingModel.ReadingSourceType)),
                   ReadingSourceID = GetDataItem<Guid>(dataItem, nameof(PatientReadingModel.ReadingSourceID)),
                   PatientTaskID = GetDataItem<long>(dataItem, nameof(PatientReadingModel.PatientTaskID)),
                   AddedByID = (string)dataItem[nameof(PatientReadingModel.AddedByID)],
                   AddedON = (DateTimeOffset)dataItem[nameof(PatientReadingModel.AddedON)],
                   IsActive = (bool)dataItem[nameof(PatientReadingModel.IsActive)],
                   IsSynced = true,
                   ErrCode = ErrorCode.OK
               }).ToList()
            : null;
    }

    private void FoodNutritionData(PatientReadingDTO foodData, JToken data)
    {
        //// Map food Nutrition data for given food reading identifier
        foodData.ListData = MapPatientReadingUIModels(data, nameof(PatientReadingDTO.ListData));
    }
}