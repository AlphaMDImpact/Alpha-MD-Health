using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading metadata implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    /// <summary>
    /// Gets Metadata
    /// </summary>
    /// <param name="readingsData">Reading DTO of reading</param>
    /// <returns>Metadata</returns>
    public async Task GetMetadataAsync(PatientReadingDTO readingsData)
    {
        await _readingDB.GetMetadataAsync(readingsData).ConfigureAwait(false);
        if (GenericMethods.IsListNotEmpty(readingsData.ChartMetaData))
        {
            string readingIDs = string.Join(",", readingsData.ChartMetaData.Select(x => x.ReadingID));
            await GetReadingRangesAsync(readingsData, readingIDs);
            foreach (var metaData in readingsData.ChartMetaData)
            {
                // Fetch language specific texts
                metaData.Reading = LibResources.GetResourceValueByKeyID(PageData?.Resources, metaData.ReadingID);
                metaData.ReadingParent = metaData.ReadingParentID > 0 ? LibResources.GetResourceValueByKeyID(PageData?.Resources, metaData.ReadingParentID) : metaData.Reading;

                // Fetch Convert target ranges to user units
                metaData.TargetMinValue = await ConvertMetaDataUnitAsync(metaData, metaData.TargetMinValue);
                metaData.TargetMaxValue = await ConvertMetaDataUnitAsync(metaData, metaData.TargetMaxValue);

                // Fetch range for current reading
                var range = readingsData?.ReadingRanges?.FirstOrDefault(x => x.ReadingID == metaData.ReadingID);
                if (range != null)
                {
                    // map range colors to display in graph
                    metaData.AbsoluteBandColor = range.AbsoluteBandColor;
                    metaData.NormalBandColor = range.NormalBandColor;
                    metaData.TargetBandColor = range.TargetBandColor;

                    // Convert ranges to user units
                    metaData.AbsoluteMinValue = await ConvertMetaDataUnitAsync(metaData, range.AbsoluteMinValue);
                    metaData.AbsoluteMaxValue = await ConvertMetaDataUnitAsync(metaData, range.AbsoluteMaxValue);
                    metaData.NormalMinValue = await ConvertMetaDataUnitAsync(metaData, range.NormalMinValue);
                    metaData.NormalMaxValue = await ConvertMetaDataUnitAsync(metaData, range.NormalMaxValue);
                }
            }
        }
    }

    /// <summary>
    /// Get reading list that can be added for program
    /// </summary>
    /// <param name="readingData">object to get readings data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation status</returns>
    public async Task SyncProgramReadingsFromServerAsync(ProgramDTO readingData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<ProgramDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_PROGRAM_READING_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    { nameof(ReadingModel.ProgramReadingID), Convert.ToString( readingData.ProgramReading.ProgramReadingID, CultureInfo.InvariantCulture)}
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            readingData.ErrCode = httpData.ErrCode;
            if (readingData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(readingData, data);
                    SetPageResources(readingData.Resources);
                    if (data[nameof(ProgramDTO.ProgramReading)].Any())
                    {
                        readingData.ProgramReading = MapReading(data[nameof(ProgramDTO.ProgramReading)]);
                    }
                    readingData.OperationTypes = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_READING_CATEGORY_GROUP, string.Empty, true, readingData.ProgramReading?.ReadingCategoryID ?? -1);
                    readingData.Items = GetPickerSource(data, nameof(ProgramDTO.Items), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), readingData.ProgramReading?.ReadingID ?? -1, true, nameof(OptionModel.ParentOptionID));

                }
            }
        }
        catch (Exception ex)
        {
            readingData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Save program reading
    /// </summary>
    /// <param name="programData">object to hold reading data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation status</returns>
    public async Task SyncProgramReadingToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<ProgramDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_READING_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                },
                ContentToSend = programData
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            programData.ErrCode = httpData.ErrCode;
            if (programData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    programData.ProgramReading.ProgramReadingID = (long)data[nameof(ProgramDTO.ProgramReading)][nameof(ReadingModel.ProgramReadingID)];
                }
            }
        }
        catch (Exception ex)
        {
            programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync reading ranges and Reading details from service
    /// </summary>
    /// <param name="metadata">reading range data reference to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reading detail and List of reading ranges data received from server</returns>
    public async Task SyncReadingMetadataFromServerAsync(ReadingMasterDataDTO metadata, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_PROGRAM_READING_METADATA_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { nameof(ReadingRangeModel.ProgramReadingID), Convert.ToString(metadata.ReadingMetadata.ProgramReadingID, CultureInfo.InvariantCulture) },
                    { nameof(BaseDTO.RecordCount), Convert.ToString(metadata.RecordCount, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            metadata.ErrCode = httpData.ErrCode;
            if (metadata.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(metadata, data);
                    SetPageResources(metadata.Resources);
                    metadata.ReadingMetadatas = MapReadings(data, nameof(ReadingMasterDataDTO.ReadingMetadatas));
                    MapReadingOptionModelData(metadata);
                }
            }
        }
        catch (Exception ex)
        {
            metadata.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Save program reading metadata
    /// </summary>
    /// <param name="readingMetadata">object to hold reading data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation status</returns>
    public async Task SyncReadingMetadataToServerAsync(ReadingMasterDataDTO readingMetadata, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<ReadingMasterDataDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_READING_METADATA_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = readingMetadata,
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            readingMetadata.ErrCode = httpData.ErrCode;
            readingMetadata.Response = httpData.Response;
        }
        catch (Exception ex)
        {
            readingMetadata.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Maps program reading data from json to model
    /// </summary>
    /// <param name="data">json data</param>
    /// <param name="collectionName">collection in json</param>
    /// <returns>list of program reading data</returns>
    public List<ReadingModel> MapReadings(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select MapReading(dataItem)).ToList()
            : null;
    }


    private void MapReadingsPerGroup(PatientReadingDTO readingsData)
    {
        readingsData.ReadingDTOs = new List<PatientReadingDTO>();

        // Arrange readings display order based on latest reading and sequence number
        var list = (from m in readingsData.ChartMetaData
                                      where m.ReadingDateTime.HasValue
                                      orderby m.ReadingDateTime descending, m.SequenceNo ascending
                                      select m).ToList();

        list.AddRange((from m in readingsData.ChartMetaData
                    where !m.ReadingDateTime.HasValue
                    orderby m.ReadingDateTime descending, m.SequenceNo ascending
                    select m).ToList());

        readingsData.ChartMetaData = list;
        foreach (ReadingMetadataUIModel reading in readingsData.ChartMetaData)
        {
            // For dashboard load only number of configured readings
            if (readingsData.RecordCount > 0 && readingsData.ReadingDTOs.Count() == readingsData.RecordCount)
            {
                break;
            }
            var isGroupReading = reading.IsGroupValue && reading.ReadingParentID > 0;
            var readingID = isGroupReading ? reading.ReadingParentID : reading.ReadingID;

            // Load row only when it is not loaded alredy in some other row
            if (!readingsData.ReadingDTOs.Any(x => x.ReadingID == readingID))
            {
                if (!isGroupReading)
                {
                    reading.ReadingParent = reading.Reading;
                }

                // Decide to load readings with its childs in a row based on isGroupReading
                var groupReading = isGroupReading
                    ? readingsData.ChartMetaData.Where(x => x.ReadingParentID == reading.ReadingParentID)?.ToList()
                    : new List<ReadingMetadataUIModel> { reading };

                var patientReading = reading;
                if (isGroupReading)
                {
                    patientReading = groupReading.OrderByDescending(x => x.ReadingDateTime)?.FirstOrDefault() ?? reading;
                }

                var readingRowData = new PatientReadingDTO
                {
                    ReadingID = readingID,
                    ReadingCategoryID = reading.ReadingCategoryID,
                    ChartMetaData = groupReading,
                    Title = reading.ReadingParent,
                    SelectedUserID = readingsData.SelectedUserID,
                    LanguageID = readingsData.LanguageID
                };
                readingsData.ReadingDTOs.Add(readingRowData);
            }
        }
    }

    //private async Task GetReadingsForTypeAsync(PatientReadingDTO readingRowData, List<PatientReadingUIModel> listData)
    //{
    //    if (MobileConstants.IsMobilePlatform)
    //    {
    //        await _readingDB.GetPatientReadingsByReadingIDAsync(readingRowData);
    //    }
    //    else
    //    {
    //        readingRowData.ListData = new List<PatientReadingUIModel>();
    //        foreach (var readingSubType in readingRowData.ChartMetaData)
    //        {
    //            var readingData = listData.Where(x => x.ReadingID == readingSubType.ReadingID)
    //                .OrderByDescending(x => x.ReadingDateTime).ThenBy(x => x.SequenceNo)?
    //                .Take(readingSubType.SummaryRecordCount)?.ToList();
    //            if (GenericMethods.IsListNotEmpty(readingData))
    //            {
    //                readingRowData.ListData.AddRange(readingData);
    //            }
    //        }
    //    }
    //}

    private void MapReadingOptionModelData(ReadingMasterDataDTO readingData)
    {
        if (!MobileConstants.IsMobilePlatform)
        {
            if (GenericMethods.IsListNotEmpty(readingData.ReadingMetadatas))
            {
                readingData.ReadingMetadata = readingData.ReadingMetadatas.FirstOrDefault();
                readingData.ReadingMetadata.Reading = LibResources.GetResourceValueByKeyID(PageData?.Resources, readingData.ReadingMetadata.ReadingID);
                readingData.ReadingMetadatas.Clear();
            }

            readingData.FrequencyType = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_READING_FREQUENCY_TYPE_GROUP, string.Empty, false, readingData.ReadingMetadata?.ReadingFrequency ?? -1);
            readingData.ValueAddedByType = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_USER_TYPE_GROUP, string.Empty, false, readingData.ReadingMetadata?.ValueAddedBy ?? -1);
            readingData.ChartType = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_READING_CHART_TYPE_GROUP, string.Empty, false, readingData.ReadingMetadata?.ChartType ?? -1);
            readingData.FilterType = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_READING_FILTERS_GROUP, string.Empty, false, -1);
            readingData.OrganisationReadingDevices = new List<OptionModel> { new OptionModel { OptionID = -1, OptionText = "", GroupName = "" } };

            var resourceValues = readingData.Resources.Where(x => (x.GroupName == GroupConstants.RS_YES_NO_TYPE_GROUP)).ToList();
            readingData.ManualReadingType = GetCommonRadioOptions(resourceValues, readingData.ReadingMetadata.AllowManualAdd);
            readingData.HealthKitDataType = GetCommonRadioOptions(resourceValues, readingData.ReadingMetadata.AllowHealthKitData);
            readingData.ShowInGraphType = GetCommonRadioOptions(resourceValues, readingData.ReadingMetadata.ShowInGraph);
            readingData.ShowInDataType = GetCommonRadioOptions(resourceValues, readingData.ReadingMetadata.ShowInData);
            readingData.DeviceDataType = GetCommonRadioOptions(resourceValues, readingData.ReadingMetadata.AllowDeviceData);
            readingData.CanBeDeletedType = GetCommonRadioOptions(resourceValues, readingData.ReadingMetadata.AllowDelete);
            readingData.ShowInDifferentLinesType = GetCommonRadioOptions(resourceValues, readingData.ReadingMetadata.ShowInDifferentLines);
        }
    }

    private List<OptionModel> GetCommonRadioOptions(List<ResourceModel> data, bool value)
    {
        return (from dataItem in data
                select new OptionModel
                {
                    OptionID = (long)dataItem.ResourceKey.ToEnum<YesNoAction>(),
                    OptionText = dataItem.ResourceValue,
                    IsSelected = CommonRadioButtonSelected(dataItem, value)
                }).ToList();
    }

    private bool CommonRadioButtonSelected(ResourceModel resource, bool value)
    {
        return value
            ? resource.ResourceKey == ResourceConstants.R_YES_ACTION_KEY
            : resource.ResourceKey == ResourceConstants.R_NO_ACTION_KEY;
    }

    private ReadingModel MapReading(JToken dataItem)
    {
        return new ReadingModel
        {
            ProgramID = GetDataItem<long>(dataItem, nameof(ReadingModel.ProgramID)),
            PatientProgramID = GetDataItem<long>(dataItem, nameof(ReadingModel.PatientProgramID)),
            ProgramReadingID = GetDataItem<long>(dataItem, nameof(ReadingModel.ProgramReadingID)),
            ReadingParentID = GetDataItem<short>(dataItem, nameof(ReadingModel.ReadingParentID)),
            ReadingCategoryID = GetDataItem<short>(dataItem, nameof(ReadingModel.ReadingCategoryID)),
            ReadingID = GetDataItem<short>(dataItem, nameof(ReadingModel.ReadingID)),
            UserID = GetDataItem<long>(dataItem, nameof(ReadingModel.UserID)),
            ReadingCode = GetDataItem<string>(dataItem, nameof(ReadingModel.ReadingCode)),
            ReadingCategory = GetDataItem<string>(dataItem, nameof(ReadingModel.ReadingCategory)),
            //Reading = GetDataItem<string>(dataItem, nameof(ReadingModel.Reading)),
            SequenceNo = GetDataItem<byte>(dataItem, nameof(ReadingModel.SequenceNo)),
            IsGroupValue = GetDataItem<bool>(dataItem, nameof(ReadingModel.IsGroupValue)),
            ReadingFormula = GetDataItem<string>(dataItem, nameof(ReadingModel.ReadingFormula)),
            UnitGoupID = GetDataItem<short>(dataItem, nameof(ReadingModel.UnitGoupID)),
            ReadingValueTypeID = GetDataItem<short>(dataItem, nameof(ReadingModel.ReadingValueTypeID)),
            ReadingFrequency = GetDataItem<short>(dataItem, nameof(ReadingModel.ReadingFrequency)),
            ValueAddedBy = GetDataItem<short>(dataItem, nameof(ReadingModel.ValueAddedBy)),
            AllowManualAdd = GetDataItem<bool>(dataItem, nameof(ReadingModel.AllowManualAdd)),
            AllowHealthKitData = GetDataItem<bool>(dataItem, nameof(ReadingModel.AllowHealthKitData)),
            AllowDeviceData = GetDataItem<bool>(dataItem, nameof(ReadingModel.AllowDeviceData)),
            AllowDelete = GetDataItem<bool>(dataItem, nameof(ReadingModel.AllowDelete)),
            ShowInGraph = GetDataItem<bool>(dataItem, nameof(ReadingModel.ShowInGraph)),
            ShowInData = GetDataItem<bool>(dataItem, nameof(ReadingModel.ShowInData)),
            DigitsAfterDecimalPoint = GetDataItem<byte>(dataItem, nameof(ReadingModel.DigitsAfterDecimalPoint)),
            ShowInDifferentLines = GetDataItem<bool>(dataItem, nameof(ReadingModel.ShowInDifferentLines)),
            SummaryRecordCount = GetDataItem<short>(dataItem, nameof(ReadingModel.SummaryRecordCount)),
            ChartType = GetDataItem<short>(dataItem, nameof(ReadingModel.ChartType)),
            ReadingFilters = GetDataItem<string>(dataItem, nameof(ReadingModel.ReadingFilters)),
            DaysOfPastRecordsToSync = GetDataItem<short>(dataItem, nameof(ReadingModel.DaysOfPastRecordsToSync)),
            IsActive = GetDataItem<bool>(dataItem, nameof(ReadingModel.IsActive)),
            IsCritical = GetDataItem<bool>(dataItem, nameof(ReadingModel.IsCritical)),
        };
    }

    private List<ReadingMetadataUIModel> MapReadingUIMetadatas(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               let readingID = GetDataItem<short>(dataItem, nameof(ReadingMetadataUIModel.ReadingID))
               let readingParentID = GetDataItem<short>(dataItem, nameof(ReadingMetadataUIModel.ReadingParentID))
               select new ReadingMetadataUIModel
               {
                   UserID = GetDataItem<long>(dataItem, nameof(ReadingMetadataUIModel.UserID)),
                   ReadingID = readingID,
                   ReadingParentID = readingParentID,
                   ReadingCategoryID = GetDataItem<short>(dataItem, nameof(ReadingMetadataUIModel.ReadingCategoryID)),
                   IsGroupValue = GetDataItem<bool>(dataItem, nameof(ReadingMetadataUIModel.IsGroupValue)),
                   ReadingFormula = GetDataItem<string>(dataItem, nameof(ReadingMetadataUIModel.ReadingFormula)),
                   UnitGoupID = GetDataItem<short>(dataItem, nameof(ReadingMetadataUIModel.UnitGoupID)),
                   ReadingValueTypeID = GetDataItem<short>(dataItem, nameof(ReadingMetadataUIModel.ReadingValueTypeID)),
                   SequenceNo = GetDataItem<byte>(dataItem, nameof(ReadingMetadataUIModel.SequenceNo)),
                   DigitsAfterDecimalPoint = GetDataItem<byte>(dataItem, nameof(ReadingMetadataUIModel.DigitsAfterDecimalPoint)),
                   ReadingFrequency = GetDataItem<short>(dataItem, nameof(ReadingMetadataUIModel.ReadingFrequency)),
                   ValueAddedBy = GetDataItem<short>(dataItem, nameof(ReadingMetadataUIModel.ValueAddedBy)),
                   AllowManualAdd = GetDataItem<bool>(dataItem, nameof(ReadingMetadataUIModel.AllowManualAdd)),
                   AllowHealthKitData = GetDataItem<bool>(dataItem, nameof(ReadingMetadataUIModel.AllowHealthKitData)),
                   AllowDeviceData = GetDataItem<bool>(dataItem, nameof(ReadingMetadataUIModel.AllowDeviceData)),
                   AllowDelete = GetDataItem<bool>(dataItem, nameof(ReadingMetadataUIModel.AllowDelete)),
                   ShowInGraph = GetDataItem<bool>(dataItem, nameof(ReadingMetadataUIModel.ShowInGraph)),
                   ShowInData = GetDataItem<bool>(dataItem, nameof(ReadingMetadataUIModel.ShowInData)),
                   ShowInDifferentLines = GetDataItem<bool>(dataItem, nameof(ReadingMetadataUIModel.ShowInDifferentLines)),
                   SummaryRecordCount = GetDataItem<short>(dataItem, nameof(ReadingMetadataUIModel.SummaryRecordCount)),
                   ChartType = GetDataItem<short>(dataItem, nameof(ReadingMetadataUIModel.ChartType)),
                   ReadingFilters = GetDataItem<string>(dataItem, nameof(ReadingMetadataUIModel.ReadingFilters)),
                   DaysOfPastRecordsToSync = GetDataItem<short>(dataItem, nameof(ReadingMetadataUIModel.DaysOfPastRecordsToSync)),
                   IsActive = GetDataItem<bool>(dataItem, nameof(ReadingMetadataUIModel.IsActive)),
                   BaseUnitIdentifier = GetDataItem<string>(dataItem, nameof(ReadingMetadataUIModel.BaseUnitIdentifier)),
                   UnitIdentifier = GetDataItem<string>(dataItem, nameof(ReadingMetadataUIModel.UnitIdentifier)),
                   TargetMaxValue = GetDataItem<double?>(dataItem, nameof(ReadingMetadataUIModel.TargetMaxValue)),
                   TargetMinValue = GetDataItem<double?>(dataItem, nameof(ReadingMetadataUIModel.TargetMinValue)),
                   TargetBandColor = GetDataItem<string>(dataItem, nameof(ReadingMetadataUIModel.TargetBandColor)),
                   //UnitID = GetDataItem<short>(dataItem, nameof(ReadingMetadataUIModel.UnitID)),
                   AbsoluteMinValue = GetDataItem<double?>(dataItem, nameof(ReadingMetadataUIModel.AbsoluteMinValue)),
                   AbsoluteMaxValue = GetDataItem<double?>(dataItem, nameof(ReadingMetadataUIModel.AbsoluteMaxValue)),
                   AbsoluteBandColor = GetDataItem<string>(dataItem, nameof(ReadingMetadataUIModel.AbsoluteBandColor)),
                   NormalMaxValue = GetDataItem<double?>(dataItem, nameof(ReadingMetadataUIModel.NormalMaxValue)),
                   NormalMinValue = GetDataItem<double?>(dataItem, nameof(ReadingMetadataUIModel.NormalMinValue)),
                   NormalBandColor = GetDataItem<string>(dataItem, nameof(ReadingMetadataUIModel.NormalBandColor)),
                   Unit = GetDataItem<string>(dataItem, nameof(ReadingMetadataUIModel.Unit)),
                   ReadingDateTime = GetDataItem<DateTimeOffset>(dataItem, nameof(ReadingMetadataUIModel.ReadingDateTime)),
                   ReadingValue = GetDataItem<double?>(dataItem, nameof(ReadingMetadataUIModel.ReadingValue)),
                   ReadingValue2 = GetDataItem<string>(dataItem, nameof(ReadingMetadataUIModel.ReadingValue2)),
                   Reading = LibResources.GetResourceValueByKeyID(PageData?.Resources, readingID),
                   ReadingParent = LibResources.GetResourceValueByKeyID(PageData?.Resources, readingParentID > 0 ? readingParentID : readingID),
               }).ToList()
            : null;
    }
}