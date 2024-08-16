using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading health app implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    /// <summary>
    /// Sync and save readings
    /// </summary>
    /// <param name="result">Reference object to return errorcode and record count</param>
    /// <param name="readingTypeIdentifier">type of reading</param>
    /// <param name="lastSyncedDay">Sync from day</param>
    /// <param name="serverDateTimeUTC">last sync date time</param>
    /// <param name="metaData">metadata used to sync data</param>
    /// <returns>Operation status</returns>
    public async Task SyncReadingsFromHealthAppAsync(BaseDTO result, string readingTypeIdentifier, double lastSyncedDay, DateTimeOffset serverDateTimeUTC, List<ReadingMetadataUIModel> metaData)
    {
        try
        {
            DateTimeOffset startDate = DateTimeOffset.UtcNow.AddDays(-lastSyncedDay);
            ReadingType readingType = readingTypeIdentifier.ToEnum<ReadingType>();
            if (GenericMethods.IsListNotEmpty(metaData) && metaData.Any(x => ((ReadingType)x.ReadingID) == readingType || ((ReadingType)x.ReadingParentID) == readingType))
            {
                ReadingMetadataUIModel metadata = metaData.FirstOrDefault(x => ((ReadingType)x.ReadingID) == readingType || ((ReadingType)x.ReadingParentID) == readingType);
                if (metadata.AllowHealthKitData)
                {
                    //todo:
                    //GetReadingAggregation(metadata.ReadingFrequency, out AggregateType aggregateType, out AggregateTimeFrame aggregateTime);
                    //HealthReadingDTO healthReadings = await new HealthLibrary.HealthLibrary().ReadDataAsync(readingType, aggregateType != AggregateType.None ? new DateTimeOffset(startDate.Date).ToUniversalTime() : startDate, serverDateTimeUTC,
                    //aggregateType, aggregateTime).ConfigureAwait(false);
                    //System.Diagnostics.Debug.WriteLine($"healt sync from device {aggregateType}===readingType{readingType} time={startDate}== serverDateTimeUTC==={serverDateTimeUTC}==healthReadings.HealthReadings={healthReadings.HealthReadings?.Count}");

                    //result.ErrCode = healthReadings.ErrCode;
                    //if (healthReadings.ErrCode == ErrorCode.OK && LibGenericMethods.IsListNotEmpty(healthReadings.HealthReadings))
                    //{
                    //    PatientReadingDTO readings = new PatientReadingDTO
                    //    {
                    //        SelectedUserID = GetLoginUserID(),
                    //        ChartMetaData = metaData,
                    //        ListData = new List<PatientReadingUIModel>()
                    //    };
                    //    ReadingSource readingSource = GetReadingSource();
                    //    if (readingType == ReadingType.BloodPressure)
                    //    {
                    //        //parent  id mapp

                    //        short systolicReadingID = metaData.Find(x => x.ReadingID == ResourceConstants.R_BP_SYSTOLIC_KEY_ID).ReadingID;
                    //        short diastolicReadingID = metaData.Find(x => x.ReadingID == ResourceConstants.R_BP_DIASTOLIC_KEY_ID).ReadingID;
                    //        List<List<HealthReadingModel>> bloodPressureGroup = healthReadings.HealthReadings.GroupBy(x => x.CreatedOn).Select(y => y.ToList()).ToList();
                    //        foreach (List<HealthReadingModel> item in bloodPressureGroup)
                    //        {
                    //            readings.ListData.Add(MapBloodPressure(item.Find(x => x.ReadingType == ReadingType.BPSystolic), readingSource, metadata, readings.SelectedUserID, systolicReadingID));
                    //            readings.ListData.Add(MapBloodPressure(item.Find(x => x.ReadingType == ReadingType.BPDiastolic), readingSource, metadata, readings.SelectedUserID, diastolicReadingID));
                    //        }
                    //    }
                    //    else
                    //    {
                    //        healthReadings.HealthReadings.ForEach(item =>
                    //        {
                    //            readings.ListData.Add(new PatientReadingUIModel
                    //            {
                    //                //reading id =systolic and dashtolic and reading parent id = bloodpresure
                    //                // reading id =weight id and Parent id = 0
                    //                PatientReadingID = LibGenericMethods.GenerateGuid(),
                    //                AddedByID = Convert.ToString(readings.SelectedUserID, CultureInfo.InvariantCulture),
                    //                ReadingID = readingType == ReadingType.BloodGlucose && DeviceInfo.Platform == DevicePlatform.iOS ? (short)item.ReadingMoment : metadata.ReadingID,

                    //                //  ReadingTypeID = metadata.ReadingTypeID,//reading id

                    //                // ReadingTypeIdentifier = projectReadingType,// reading id
                    //                ReadingDateTime = item.CreatedOn,
                    //                //  ReadingIdentifier = GetReadingIdentifier(item),// check reading id
                    //                ReadingValue = HealthLibrary.GenericMethods.IsReadingWorkout(readingType) ? item.Duration : item.ReadingValue,
                    //                UnitIdentifier = HealthLibrary.GenericMethods.IsReadingWorkout(readingType) ? ReadingUnit.Minutes.ToString() : item.ReadingUnit.ToString(),
                    //                ReadingSourceType = readingSource.ToString(),
                    //                ReadingNotes = string.Empty,
                    //                IsActive = true
                    //            });
                    //        });
                    //    }
                    //    await SavePatientReadingAsync(readings).ConfigureAwait(false);
                    //    result.RecordCount = healthReadings.HealthReadings.Count;
                    //}
                    //return;
                }
                //if (readingType == ReadingType.Bicycling || readingType == ReadingType.Gymnastics || readingType == ReadingType.Cardio || readingType == ReadingType.Running || readingType == ReadingType.Yoga || readingType == ReadingType.Skating || readingType == ReadingType.Soccer || readingType == ReadingType.Swimming)

                //{

                //}

            }
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        catch (Exception ex)
        {
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }



    /// <summary>
    /// Save Reading conect account status
    /// </summary>
    /// <param name="result">object to save data</param>
    /// <returns>Operation status</returns>
    public async Task SaveReadingConnectAccountStatusAsync(BaseDTO result)
    {
        try
        {
            await _readingDB.SaveReadingConnectAccountStatusAsync(result).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Get Metadata of health app supported readings
    /// </summary>
    /// <param name="readings">Reference object to return data</param>
    /// <returns>List of reading metadata</returns>
    public async Task GetHealthAppReadingsMetaDataAsync(PatientReadingDTO readings)
    {
        try
        {
            readings.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            readings.SelectedUserID = GetLoginUserID();
            // Added by is used to get only health app (AddedBy = 1)/ device (AddedBy = 2) supported metadata when its value is not null or empty
            if (string.IsNullOrWhiteSpace(readings.AddedBy))
            {
                readings.AddedBy = Constants.NUMBER_ONE;
            }
            await _readingDB.GetMetadataAsync(readings).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            readings.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Gets User's Health Permission requests
    /// </summary>
    /// <returns>list of Health permissions</returns>
    public async Task<List<ReadingType>> GetHealthPermissionsToBeRequestedAsync()
    {
        try
        {
            PatientReadingDTO readingsData = new PatientReadingDTO();
            await new ReadingService(_essentials).GetHealthAppReadingsMetaDataAsync(readingsData).ConfigureAwait(false);
            if (readingsData.ErrCode == ErrorCode.OK)
            {
                if (IsActiveMetadataPresent(readingsData))
                {
                    await GetSettingsAsync(GroupConstants.RS_READING_RELATION_GROUP).ConfigureAwait(false);
                    // ReadingTypeIdentifier = reading id

                    List<ReadingType> readingTypes = readingsData.ChartMetaData.Where(x => x.ReadingParentID > 0).GroupBy(x => x.ReadingParentID).Select(y => (ReadingType)y.First().ReadingParentID).ToList();
                    readingTypes.AddRange(readingsData.ChartMetaData.Where(x => x.ReadingParentID == 0).Select(x => (ReadingType)x.ReadingID).ToList());
                    NormalizeReadingTypesForHealthApp(readingTypes);
                    List<UserHealthPermissionRequestModel> requestedData = await _readingDB.GetUserHealthPermissionsAsync(GetLoginUserID());
                    if (GenericMethods.IsListNotEmpty(requestedData))
                    {
                        foreach (UserHealthPermissionRequestModel request in requestedData)
                        {
                            readingTypes.RemoveAll(x => x == (ReadingType)request.ReadingID && request.IsRequested);
                        }
                    }
                    return readingTypes;
                }
                return new List<ReadingType>();
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
        }
        return null;
    }

    /// <summary>
    /// Checks if health app is currently enabled
    /// </summary>
    /// <returns>Returns true if health app is connected else returns false</returns>
    public async Task<int> IsReadingHealthAppEnabledAsync()
    {
        try
        {
            if ((MobileConstants.IsIosPlatform && MobileConstants.IsTablet)
                || _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0) == (int)RoleName.CareTaker)
            {
                return 0;
            }
            //// If There is Any SupportedReadingType having 3rd party support
            List<ReadingType> readingTypes = await GetHealthPermissionsToBeRequestedAsync().ConfigureAwait(false);
            if (readingTypes == null)
            {
                return 2;
            }
            else
            {
                if (readingTypes.Count > 0)
                {
                    // Ask Permission
                    return 1;
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            // Failed 
            return 2;
        }
        // Not enabled or already requested
        return 0;
    }

    /// <summary>
    /// Update health app permission request status
    /// </summary>
    /// <param name="observations">list of obsevations need to set is requested true</param>
    /// <returns>Status</returns>
    public async Task<ErrorCode> UpdatePermissionRequestStatusAsync(List<ReadingType> observations)
    {
        try
        {
            long userID = GetLoginUserID();
            List<UserHealthPermissionRequestModel> userHealthPermissionRequests = new List<UserHealthPermissionRequestModel>();
            foreach (short item in observations)
            {
                userHealthPermissionRequests.Add(new UserHealthPermissionRequestModel
                {
                    UserID = userID.ToString(CultureInfo.InvariantCulture),
                    ReadingID = item,
                    IsRequested = true
                });
                _essentials.SetPreferenceValue(((ReadingType)item).ToString() + StorageConstants.PR_IS_PERMISSION_CALL_COMPLETE_KEY, true);
            }
            await _readingDB.UpdatePermissionRequestStatusAsync(userHealthPermissionRequests).ConfigureAwait(false);
            return ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            return ErrorCode.ErrorWhileSavingRecords;
        }
    }

    /// <summary>
    /// Gets Reading Type based on the given identifier
    /// </summary>
    /// <param name="settings">Settings with mapping values</param>
    /// <param name="identifier">Identifier for Reading Type</param>
    /// <returns>ReadingType based on given identifier</returns>
    public ReadingType GetReadingTypeFromIdentifier(List<SettingModel> settings, string identifier)
    {
        return settings.FirstOrDefault(x => x.GroupName == GroupConstants.RS_READING_RELATION_GROUP && x.SettingValue == identifier)?.SettingKey.ToEnum<ReadingType>() ?? ReadingType.None;
    }

    /// <summary>
    /// Update Reading Type list to contain only permissions that can be requested from health app
    /// </summary>
    /// <param name="observations">All types for which readings are to be fetched from health app</param>
    public void NormalizeReadingTypesForHealthApp(List<ReadingType> observations)
    {
        System.Diagnostics.Debug.WriteLine($"{observations}");
        //Todo:
        //if (HealthLibrary.GenericMethods.IsContainsReadingWorkout(observations))
        //{
        //    observations.RemoveAll(x => (short)x > 692 && (short)x < 715);
        //    observations.Add(ReadingType.Workout);
        //}
        //if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android && HealthLibrary.GenericMethods.IsContainsReadingNutrition(observations))
        //{
        //    observations.RemoveAll(x => (short)x == 553 || (short)x > 677 && (short)x < 692);
        //    observations.Add(ReadingType.Nutrition);
        //}
    }

    private string GetReadingIdentifier(HealthReadingModel readingModel)
    {
        if (readingModel.ReadingMoment != null)
        {
            return LibSettings.GetSettingValueByKey(PageData?.Settings, readingModel.ReadingMoment.ToString());
        }
        return LibSettings.GetSettingValueByKey(PageData?.Settings, readingModel.ReadingType.ToString());
    }

    private PatientReadingUIModel MapBloodPressure(HealthReadingModel item, ReadingSource readingSource, ReadingMetadataUIModel metadata, long userID, short readingID)
    {
        return new PatientReadingUIModel
        {
            PatientReadingID = GenericMethods.GenerateGuid(),
            AddedByID = Convert.ToString(userID, CultureInfo.InvariantCulture),
            ReadingID = readingID,
            ReadingDateTime = item.CreatedOn,
            //ReadingType = ReadingType.BloodPressure.ToString(),
            //ReadingIdentifier = LibSettings.GetSettingValueByKey(PageData?.Settings, item.ReadingType.ToString()),
            ReadingValue = item.ReadingValue,
            Unit = item.ReadingUnit.ToString(),
            ReadingSourceType = readingSource.ToString(),
            ReadingNotes = string.Empty,
            IsActive = true
        };
    }

    //Todo:
    //private void GetReadingAggregation(short readingFrequency, out AggregateType aggregateType, out AggregateTimeFrame aggregateTime)
    //{
    //    if (readingFrequency == LibResourceConstants.R_DAILY_AVG_KEY_ID)
    //    {
    //        aggregateType = AggregateType.Average;
    //        aggregateTime = AggregateTimeFrame.Day;
    //    }
    //    else if (readingFrequency == LibResourceConstants.R_DAILY_SUM_KEY_ID)
    //    {
    //        aggregateType = AggregateType.Sum;
    //        aggregateTime = AggregateTimeFrame.Day;
    //    }
    //    else if (readingFrequency == LibResourceConstants.R_HOURLY_SUM_KEY_ID)
    //    {
    //        aggregateType = AggregateType.Sum;
    //        aggregateTime = AggregateTimeFrame.Hour;
    //    }
    //    else if (readingFrequency == LibResourceConstants.R_HOURLY_AVG_KEY_ID)
    //    {
    //        aggregateType = AggregateType.Average;
    //        aggregateTime = AggregateTimeFrame.Hour;
    //    }
    //    else
    //    {
    //        aggregateType = AggregateType.None;
    //        aggregateTime = AggregateTimeFrame.All;
    //    }
    //}

    private ReadingSource GetReadingSource()
    {
        return MobileConstants.IsAndroidPlatform ? ReadingSource.GoogleFit : ReadingSource.HealthKit;
    }

    private bool IsActiveMetadataPresent(PatientReadingDTO readingsData)
    {
        if (GenericMethods.IsListNotEmpty(readingsData.ChartMetaData))
        {
            readingsData.ChartMetaData = readingsData.ChartMetaData.Where(x => x.IsActive).ToList();
            return GenericMethods.IsListNotEmpty(readingsData.ChartMetaData);
        }
        return false;
    }
}