using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading target implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    /// <summary>
    /// Saves Patient Reading Target
    /// </summary>
    /// <param name="readingData">object to save patient target data</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status call</returns>
    public async Task SavePatientReadingsTargetAsync(PatientReadingDTO readingData, CancellationToken cancellationToken)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await _readingDB.SavePatientReadingTargetAsync(readingData).ConfigureAwait(false);
            }
            else
            {
                await SyncUserReadingTargetsToServerAsync(readingData, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            readingData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Get Patient reading target async
    /// </summary>
    /// <param name="readingsData">object to get patient target data</param>
    /// <returns>Patient target data based on reading</returns>
    public async Task GetPatientReadingTargetAsync(PatientReadingDTO readingsData)
    {
        await GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_READINGS_PAGE_GROUP, GroupConstants.RS_READINGS_GROUP, GroupConstants.RS_GENDER_TYPE_GROUP).ConfigureAwait(false);
        await GetMetadataAsync(readingsData).ConfigureAwait(false);
        readingsData.Resources = PageData.Resources;
    }

    /// <summary>
    /// assign absolute ranges
    /// </summary>
    /// <param name="targetData">tatget page data</param>
    /// <param name="selectedValue">selected value</param>
    /// <param name="minPlaceHolder">existing minimum value</param>
    /// <param name="maxPlaceHolder">existing maximum value</param>
    public void AssignAbsoluteRanges(PatientReadingDTO targetData, int selectedValue, string minPlaceHolder, string maxPlaceHolder)
    {
        ResourceModel absoluteMinRange = targetData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_READING_TARGET_MIN_VALUE_KEY);
        ResourceModel absoluteMaxRange = targetData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_READING_TARGET_MAX_VALUE_KEY);
        targetData.Resources.Remove(absoluteMinRange);
        targetData.Resources.Remove(absoluteMaxRange);
        var selectedTarget = targetData.PatientReadingTargets.FirstOrDefault(x => x.ReadingID == selectedValue);
        absoluteMinRange.MinLength = absoluteMaxRange.MinLength = (double)selectedTarget.AbsoluteMinValue;
        absoluteMinRange.MaxLength = absoluteMaxRange.MaxLength = (double)selectedTarget.AbsoluteMaxValue;
        var unitText = string.IsNullOrWhiteSpace(selectedTarget.Unit) ? "" : $" ({selectedTarget.Unit})";
        absoluteMinRange.PlaceHolderValue = absoluteMinRange.ResourceValue = minPlaceHolder + unitText;
        absoluteMaxRange.PlaceHolderValue = absoluteMaxRange.ResourceValue = maxPlaceHolder + unitText;
        targetData.Resources.Add(absoluteMinRange);
        targetData.Resources.Add(absoluteMaxRange);
    }

    /// <summary>
    /// Sync Reading targets Data to server
    /// </summary>
    /// <param name="readingData">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status call</returns>
    internal async Task SyncUserReadingTargetsToServerAsync(PatientReadingDTO readingData, CancellationToken cancellationToken)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await _readingDB.GetPatientReadingTargetsForSyncAsync(readingData);
            }
            if (GenericMethods.IsListNotEmpty(readingData.PatientReadingTargets))
            {
                var httpData = new HttpServiceModel<PatientReadingDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PATIENT_READING_TARGETS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    },
                    ContentToSend = readingData,
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                readingData.ErrCode = httpData.ErrCode;
                readingData.Response = httpData.Response;
                if (readingData.ErrCode == ErrorCode.OK && MobileConstants.IsMobilePlatform)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data?.HasValues == true)
                    {
                        readingData.PatientReadingTargets = MapPatientReadingTargets(data, nameof(PatientReadingDTO.PatientReadingTargets));
                        if (GenericMethods.IsListNotEmpty(readingData.PatientReadingTargets))
                        {
                            await _readingDB.UpdateSyncStatusForTargetDataAsync(readingData);
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

    private List<ReadingTargetModel> MapPatientReadingTargets(JToken data, string collectionName)
    {
        return data[collectionName]?.Count() > 0
            ? (from dataItem in data[collectionName]
               select new ReadingTargetModel
               {
                   UserID = GetDataItem<long>(dataItem, nameof(ReadingTargetModel.UserID)),
                   ReadingID = GetDataItem<short>(dataItem, nameof(ReadingTargetModel.ReadingID)),
                   TargetMinValue = GetDataItem<float>(dataItem, nameof(ReadingTargetModel.TargetMinValue)),
                   TargetMaxValue = GetDataItem<float>(dataItem, nameof(ReadingTargetModel.TargetMaxValue)),
                   IsActive = GetDataItem<bool>(dataItem, nameof(ReadingTargetModel.IsActive)),
                   IsSynced = true,
                   AbsoluteMaxValue = GetDataItem<float>(dataItem, nameof(ReadingTargetModel.AbsoluteMaxValue)),
                   AbsoluteMinValue = GetDataItem<float>(dataItem, nameof(ReadingTargetModel.AbsoluteMinValue)),
               }).ToList()
            : new List<ReadingTargetModel>();
    }


    private List<ReadingTargetModel> MapChartMetaDataToPatientReadingTargets(PatientReadingDTO readingsData)
    {
        return readingsData.ChartMetaData?.Count() > 0
              ? (from dataItem in readingsData.ChartMetaData
                 select new ReadingTargetModel
                 {
                     ReadingID = dataItem.ReadingID,
                     TargetMaxValue = dataItem.TargetMaxValue != null ? (float)dataItem.TargetMaxValue : 0,
                     TargetMinValue = dataItem.TargetMinValue != null ? (float)dataItem.TargetMinValue : 0,
                     AbsoluteMaxValue = dataItem.AbsoluteMaxValue ?? 0,
                     AbsoluteMinValue = dataItem.AbsoluteMinValue ?? 0,
                     Unit = dataItem.Unit,
                 }).ToList()
            : new List<ReadingTargetModel>();
    }

    private List<OptionModel> MapReadingTypesFromMetadata(PatientReadingDTO readingsData)
    {
        return readingsData.ChartMetaData?.Count() > 0
              ? (from dataItem in readingsData.ChartMetaData
                 select new OptionModel
                 {
                     OptionID = dataItem.ReadingID,
                     OptionText = dataItem.Reading,
                     IsSelected = dataItem.ReadingID == readingsData.ReadingID
                 }).ToList()
            : new List<OptionModel>();
    }
}