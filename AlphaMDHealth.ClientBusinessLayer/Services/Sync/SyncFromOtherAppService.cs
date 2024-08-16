using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public partial class DataSyncService : BaseService
    {
        private async Task SyncDataFromHealthAppAsync(BaseDTO result, DataSyncFor syncFor, double lastSyncedDay, List<ReadingMetadataUIModel> metaData, CancellationToken cancellationToken)
        {
            if (await new UserAccountSettingService(_essentials).IsHealthAppEnabledAsync().ConfigureAwait(false))
            {
                switch (syncFor)
                {
                    case DataSyncFor.Steps:
                    case DataSyncFor.HeartRate:
                    case DataSyncFor.Weight:
                    case DataSyncFor.Height:
                    case DataSyncFor.BloodPressure:

                    case DataSyncFor.SpO2:
                    case DataSyncFor.BloodGlucose:
                    case DataSyncFor.Hydration:
                    case DataSyncFor.BodyTemperature:
                    case DataSyncFor.StairClimbing:
                    case DataSyncFor.Pilates:
                    case DataSyncFor.Kickboxing:
                    case DataSyncFor.JumpRope:
                    case DataSyncFor.HighIntensityIntervalTraining:
                    case DataSyncFor.Yoga:
                    case DataSyncFor.Walking:
                    case DataSyncFor.Swimming:
                    case DataSyncFor.Squash:
                    case DataSyncFor.Soccer:
                    case DataSyncFor.Skating:
                    case DataSyncFor.Running:
                    case DataSyncFor.Cardio:
                    case DataSyncFor.MartialArts://not
                    case DataSyncFor.Gymnastics://gymnastic
                    case DataSyncFor.StrengthTraining://not
                    case DataSyncFor.Dancing://not
                    case DataSyncFor.Bicycling:
                    case DataSyncFor.Crossfit:
                    case DataSyncFor.Climbing://not
                    case DataSyncFor.Boxing:
                    case DataSyncFor.Badminton:
                    case DataSyncFor.NutritionIron:
                    case DataSyncFor.NutritionDietaryFiber://fiber
                    case DataSyncFor.NutritionCholesterol://not
                    case DataSyncFor.NutritionCalories:
                    case DataSyncFor.NutritionCalcium:
                    case DataSyncFor.NutritionPotassium://not
                    case DataSyncFor.NutritionProtein:
                    case DataSyncFor.NutritionVitaminC:
                    case DataSyncFor.NutritionVitaminA:
                    case DataSyncFor.NutritionTotalFat:
                    case DataSyncFor.NutritionTotalCarbs:
                    case DataSyncFor.NutritionSugar:
                    case DataSyncFor.NutritionSodium:
                     
                       
                        await SyncReadingFromOtherAppAsync(result, syncFor, lastSyncedDay, metaData).ConfigureAwait(false);
                        break;
                    case DataSyncFor.ReadingsFromOtherApps:
                        await SyncReadingsFromOtherAppAsync(result, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                        break;
                }
            }
            else
            {
                result.ErrCode = ErrorCode.Unauthorized;
            }
        }

        private async Task SyncReadingsFromOtherAppAsync(BaseDTO result, CancellationToken cancellationToken)
        {
            PatientReadingDTO readings = new PatientReadingDTO();
            ReadingService readingService = new ReadingService(_essentials);
            await Task.WhenAll(readingService.GetHealthAppReadingsMetaDataAsync(readings), GetSettingsAsync(GroupConstants.RS_READING_RELATION_GROUP)).ConfigureAwait(false);
            if (readings.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(readings.ChartMetaData))
            {
                readings.ChartMetaData = FilterActiveMetadata(readings);
                List<BaseDTO> syncResults = new List<BaseDTO>();
      
                foreach (var item in readings.ChartMetaData.Where(x => x.ReadingParentID > 0).GroupBy(x => x.ReadingParentID).Select(y => y.First()))
                {
                    //check for Flow
                    ReadingType type= (ReadingType)item.ReadingParentID;
                    System.Diagnostics.Debug.WriteLine($"reading type to sync ==={type}");
                    switch(type)
                    {
                        case ReadingType.BloodGlucose:
                        case ReadingType.BloodPressure:
                            syncResults.Add(await SyncFromOtherAppAsync(((ReadingType)item.ReadingParentID).ToString().ToEnum<DataSyncFor>(), item.DaysOfPastRecordsToSync, readings.ChartMetaData, new BaseDTO { Settings = PageData.Settings }, cancellationToken).ConfigureAwait(false));
                            break;
                        case ReadingType.Workout:
                            foreach (var reading in readings.ChartMetaData.Where(x => x.ReadingParentID == (short)ReadingType.Workout).GroupBy(x => x.ReadingID).Select(y => y.First()))
                            {
                                //check for Flow
                                syncResults.Add(await SyncFromOtherAppAsync(((ReadingType)reading.ReadingID).ToString().ToEnum<DataSyncFor>(), reading.DaysOfPastRecordsToSync, readings.ChartMetaData, new BaseDTO { Settings = PageData.Settings }, cancellationToken).ConfigureAwait(false));
                            }
                            break;
                        case ReadingType.Nutrition:
                            foreach (var reading in readings.ChartMetaData.Where(x => x.ReadingParentID == (short)ReadingType.Nutrition).GroupBy(x => x.ReadingID).Select(y => y.First()))
                            {
                                //check for Flow
                                syncResults.Add(await SyncFromOtherAppAsync(((ReadingType)reading.ReadingID).ToString().ToEnum<DataSyncFor>(), reading.DaysOfPastRecordsToSync, readings.ChartMetaData, new BaseDTO { Settings = PageData.Settings }, cancellationToken).ConfigureAwait(false));
                            }
                            break;
                    }
                }
                foreach (var item in readings.ChartMetaData.Where(x => x.ReadingParentID == 0).GroupBy(x => x.ReadingID).Select(y => y.First()))

                {
                    //check for Flow
                    System.Diagnostics.Debug.WriteLine($"reading type to (ReadingType)item.ReadingID ==={(ReadingType)item.ReadingID}");
                    syncResults.Add(await SyncFromOtherAppAsync(((ReadingType)item.ReadingID).ToString().ToEnum<DataSyncFor>(), item.DaysOfPastRecordsToSync, readings.ChartMetaData, new BaseDTO { Settings = PageData.Settings }, cancellationToken).ConfigureAwait(false));
                }
               
                if (syncResults.Any(x => x.ErrCode == ErrorCode.OK))
                {
                    result.ErrCode = ErrorCode.OK;
                    result.RecordCount = syncResults.Sum(x => x.RecordCount);
                    if (result.RecordCount > 0)
                    {
                        _ = SyncDataAsync(result, ServiceSyncGroups.RSSyncToServerGroup, DataSyncFor.PatientReadings, cancellationToken);
                    }
                }
                else
                {
                    result.ErrCode = syncResults.First().ErrCode;
                    if (MobileConstants.IsAndroidPlatform && syncResults.Any(x => x.ErrCode == ErrorCode.Unauthorized))
                    {
                        // Update Google Fit toggle when permission is denied
                        result.IsActive = false;
                        await readingService.SaveReadingConnectAccountStatusAsync(result).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<BaseDTO> SyncFromOtherAppAsync(DataSyncFor currentSyncFor, double defaultNumberOfDays, List<ReadingMetadataUIModel> metaData, BaseDTO currentSyncResult, CancellationToken cancellationToken)
        {
            await SyncDataFromHealthAppAsync(currentSyncResult, currentSyncFor, defaultNumberOfDays, metaData, cancellationToken).ConfigureAwait(false);
            return currentSyncResult;
        }

        private async Task SyncReadingFromOtherAppAsync(BaseDTO result, DataSyncFor syncFor, double lastSyncedDay, List<ReadingMetadataUIModel> metaData)
        {
            DataSyncModel syncData = await new DataSyncDatabase().GetDataSyncForAsync(syncFor.ToString()).ConfigureAwait(false);
            if (syncData != null)
            {
                ReadingService readingService = new ReadingService(_essentials);
                if (GenericMethods.IsListNotEmpty(result.Settings))
                {
                    readingService.PageData = result;
                }
                else
                {
                    await GetSettingsAsync(GroupConstants.RS_READING_RELATION_GROUP).ConfigureAwait(false);
                    readingService.PageData = PageData;
                }
                DateTimeOffset serverDateTimeUTC = GenericMethods.GetUtcDateTime;
                if (metaData == null)
                {
                    PatientReadingDTO vitals = new PatientReadingDTO();
                    await readingService.GetHealthAppReadingsMetaDataAsync(vitals).ConfigureAwait(false);
                    metaData = FilterActiveMetadata(vitals);
                }
                if (syncData.SyncFromServerDateTime == null)
                {
                    if (lastSyncedDay < 1)
                    {
                        lastSyncedDay = metaData.FirstOrDefault(x => ((ReadingType)x.ReadingID).ToString() == LibSettings.GetSettingValueByKey(PageData?.Settings, syncFor.ToString())).DaysOfPastRecordsToSync;
                    }
                }
                else
                {
                    lastSyncedDay = Math.Ceiling((GenericMethods.GetUtcDateTime - syncData.SyncFromServerDateTime.Value).TotalDays);
                }
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                await Task.WhenAny(
                    WaitForTaskResultAsync(result, 60, cancellationTokenSource.Token),
                    readingService.SyncReadingsFromHealthAppAsync(result, syncFor.ToString(), lastSyncedDay, serverDateTimeUTC, metaData)
                ).ConfigureAwait(false);
                cancellationTokenSource.Cancel();
                result.LastModifiedON = syncData.SyncFromServerDateTime;
                if (result.ErrCode == ErrorCode.OK && result.RecordCount > 0)
                {
                    await new DataSyncDatabase().UpdateDateSyncedFromServerAsync(syncFor.ToString(), serverDateTimeUTC, default).ConfigureAwait(false);
                }
            }
            else
            {
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        private List<ReadingMetadataUIModel> FilterActiveMetadata(PatientReadingDTO readingsData)
        {
            return readingsData.ChartMetaData?.Where(x => x.IsActive).ToList();
        }
    }
}