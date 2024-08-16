using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;
using Constants = AlphaMDHealth.Utility.Constants;

namespace AlphaMDHealth.ClientBusinessLayer;

public class PatientTrackerService : BaseService
{
    public PatientTrackerService(IEssentials serviceEssentials) : base(serviceEssentials)
    {
        
    }
    /// <summary>
    /// sync Patient Trackers data to server
    /// </summary>
    /// <param name="patientTrackerData">Sync Patient Tracker data to server</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation status</returns>
    public async Task SyncPatientTrackersToServerAsync(TrackerDTO patientTrackerData, CancellationToken cancellationToken)
    {
        try
        {
            await new TrackerDatabase().GetPatientTrackersForSyncAsync(patientTrackerData).ConfigureAwait(false);
            if (GenericMethods.IsListNotEmpty(patientTrackerData.PatientTrackers))
            {
                foreach (var tracker in patientTrackerData.PatientTrackers)
                {
                    patientTrackerData.PatientTracker = tracker;
                    await SavePatientTrackerToServerAsync(patientTrackerData, cancellationToken);
                    if (patientTrackerData.ErrCode == ErrorCode.OK || patientTrackerData.ErrCode == ErrorCode.DuplicateData)
                    {
                        tracker.PatientTrackerID = patientTrackerData.PatientTracker.PatientTrackerID;
                        await new TrackerDatabase().UpdatePatientTrackerSyncStatusAsync(tracker).ConfigureAwait(false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            patientTrackerData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// sync Patient Tracker Values data to server
    /// </summary>
    /// <param name="patientTrackerData">Sync Patient Tracker Values data to server</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation status</returns>
    public async Task SyncPatientTrackerValuesToServerAsync(TrackerDTO patientTrackerData, CancellationToken cancellationToken)
    {
        try
        {
            await new TrackerDatabase().GetPatientTrackerValuesForSyncAsync(patientTrackerData).ConfigureAwait(false);
            if (GenericMethods.IsListNotEmpty(patientTrackerData.PatientTrackerValues))
            {
                foreach (var trackervalues in patientTrackerData.PatientTrackerValues)
                {
                    patientTrackerData.PatientTrackerValue = trackervalues;
                    await SavePatientTrackerValueToServerAsync(patientTrackerData, cancellationToken);
                    if (patientTrackerData.ErrCode == ErrorCode.OK || patientTrackerData.ErrCode == ErrorCode.DuplicateData)
                    {
                        trackervalues.PatientTrackerID = patientTrackerData.PatientTrackerValue.PatientTrackerID;
                        await new TrackerDatabase().UpdatePatientTrackerValueSyncStatusAsync(trackervalues).ConfigureAwait(false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            patientTrackerData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Maps json object into Model and saves data into DB
    /// </summary>
    /// <param name="result">Object which holds Operation status</param>
    /// <param name="dataItem">json object to fetch data from it</param>
    /// <returns>Operation status with record count</returns>
    public async Task MapAndSavePatientTrackersAsync(DataSyncModel result, JToken dataItem)
    {
        try
        {
            TrackerDTO patientTracker = new TrackerDTO
            {
                PatientTrackers = MapPatientTrackers(dataItem, nameof(DataSyncDTO.PatientTrackers)),
                PatientTrackerValues = MapPatientTrackerValues(dataItem, nameof(DataSyncDTO.PatientTrackerValues)),
            };
            if (GenericMethods.IsListNotEmpty(patientTracker.PatientTrackers))
            {
                await new TrackerDatabase().SavePatientTrackerDataAsync(patientTracker).ConfigureAwait(false);
                result.RecordCount = patientTracker.PatientTrackers?.Count ?? 0;
            }
            if (GenericMethods.IsListNotEmpty(patientTracker.PatientTrackerValues))
            {
                await new TrackerDatabase().SavePatientTrackerValuesAsync(patientTracker).ConfigureAwait(false);
                result.RecordCount = patientTracker.PatientTrackerValues?.Count ?? 0;
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
    /// Save patient tracker data 
    /// </summary>
    /// <param name="patientTracker">patient tracker data to save in server</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation status</returns>
    public async Task SavePatientTrackerToServerAsync(TrackerDTO patientTracker, CancellationToken cancellationToken)
    {
        if (patientTracker.PatientTracker != null)
        {
            patientTracker.PatientTracker.LastModifiedON = GenericMethods.GetUtcDateTime;
        }
        var httpData = new HttpServiceModel<TrackerDTO>
        {
            CancellationToken = cancellationToken,
            ContentToSend = patientTracker,
            PathWithoutBasePath = UrlConstants.SAVE_PATIENT_TRACKER_ASYNC_PATH,
            QueryParameters = new NameValueCollection
            {
                { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(_essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0), CultureInfo.InvariantCulture)}
            }
        };
        await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
        patientTracker.ErrCode = httpData.ErrCode;
        if (MobileConstants.IsMobilePlatform
            && (patientTracker.ErrCode == ErrorCode.OK || patientTracker.ErrCode == ErrorCode.DuplicateData)
            && patientTracker.PatientTracker.PatientTrackerID == Guid.Empty)
        {
            MapPatientTrackerID(patientTracker, httpData);
        }
    }

    /// <summary>
    /// Save Patient Tracker values
    /// </summary>
    /// <param name="patientTrackerValues">patient tracker values to save in server</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation status</returns>
    public async Task SavePatientTrackerValueToServerAsync(TrackerDTO patientTrackerValues, CancellationToken cancellationToken)
    {
        try
        {
            if (patientTrackerValues.PatientTrackerValue == null)
            {
                patientTrackerValues.ErrCode = ErrorCode.InvalidData;
                return;
            }
            var httpData = new HttpServiceModel<TrackerDTO>
            {
                CancellationToken = cancellationToken,
                ContentToSend = patientTrackerValues,
                PathWithoutBasePath = UrlConstants.SAVE_PATIENT_TRACKER_VALUE_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(_essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0), CultureInfo.InvariantCulture)}
                }
            };
            await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
            patientTrackerValues.ErrCode = httpData.ErrCode;
            if (MobileConstants.IsMobilePlatform
                && (patientTrackerValues.ErrCode == ErrorCode.OK || patientTrackerValues.ErrCode == ErrorCode.DuplicateData)
                && patientTrackerValues.PatientTrackerValue.PatientTrackerID == Guid.Empty)
            {
                MapPatientTrackerID(patientTrackerValues, httpData);
            }
        }
        catch (Exception ex)
        {
            patientTrackerValues.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Get Patient Trackers
    /// </summary>
    /// <param name="patientTrackerData">Get patient tracker data</param>
    /// <returns>Operation Status</returns>
    public async Task GetPatientTrackersAsync(TrackerDTO patientTrackerData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                patientTrackerData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                patientTrackerData.SelectedUserID = GetUserID();
                patientTrackerData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                await Task.WhenAll(
                    GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, GroupConstants.RS_TRACKERS_GROUP, GroupConstants.RS_PROGRAMS_GROUP),
                    GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_TRACKERS_GROUP, GroupConstants.RS_QUESTIONNAIRE_QUESTION_TYPE_GROUP, GroupConstants.RS_PROGRAMS_GROUP),
                    GetFeaturesAsync(AppPermissions.PatientTrackersView.ToString(), AppPermissions.PatientTrackerAddEdit.ToString(), AppPermissions.PatientTrackerDetailView.ToString()),
                    GetPatientTrackerDataFromLocalDBAsync(patientTrackerData)
                ).ConfigureAwait(false);
                if (patientTrackerData.RecordCount == 0)
                {
                    patientTrackerData.PatientTrackers = patientTrackerData.PatientTrackers?.OrderByDescending(x => x.FromDate.Value.Date)?.ThenBy(c => c.FromDate.Value.Date)?.ToList();
                }
            }
            else
            {
                await SyncPatientTrackersFromServerAsync(patientTrackerData, CancellationToken.None).ConfigureAwait(false);
            }
            GetPatientTrackersUIData(patientTrackerData);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            patientTrackerData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    private void GetPatientTrackersUIData(TrackerDTO patientTrackerData)
    {
        if (patientTrackerData.ErrCode == ErrorCode.OK)
        {
            MapFormattedData(patientTrackerData);
            CheckIsEdit(patientTrackerData);
        }
    }

    private async Task GetPatientTrackerDataFromLocalDBAsync(TrackerDTO patientTrackerData)
    {
        if (patientTrackerData.RecordCount == -1)
        {
            await new TrackerDatabase().GetTrackerOptionsAsync(patientTrackerData);
        }
        else if (patientTrackerData.RecordCount == -3)
        {
            await new TrackerDatabase().GetPatientTrackerDetail(patientTrackerData);
        }
        else
        {
            await new TrackerDatabase().GetPatientTrackersAsync(patientTrackerData);
        }
    }

    /// <summary>
    /// Delete patient tracker 
    /// </summary>
    /// <param name="patientTrackerData">delete PatientTracker Data from db</param>
    /// <returns>operation status</returns>
    public async Task<ErrorCode> DeletePatientTrackerAsync(TrackerDTO patientTrackerData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await new TrackerDatabase().DeletePatientTrackerAsync(patientTrackerData.PatientTracker.PatientTrackerID).ConfigureAwait(false);
            }
            return ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            return ErrorCode.ErrorWhileDeletingRecords;
        }
    }

    /// <summary>
    /// Save Patient Trackers
    /// </summary>
    /// <param name="patientTrackerData"></param>
    /// <returns>Oepration Status</returns>
    public async Task SavePatientTrackersAsync(TrackerDTO patientTrackerData)
    {
        try
        {
            if (patientTrackerData.PatientTracker == null)
            {
                patientTrackerData.ErrCode = ErrorCode.InvalidData;
                return;
            }
            patientTrackerData.PatientTracker.PatientTrackerID = patientTrackerData.PatientTracker.PatientTrackerID == Guid.Empty
                ? GenericMethods.GenerateGuid()
                : patientTrackerData.PatientTracker.PatientTrackerID;

            patientTrackerData.PatientTracker.UserID = GetUserID();
            if (MobileConstants.IsMobilePlatform)
            {
                patientTrackerData.PatientTrackers = new List<PatientTrackersModel> { patientTrackerData.PatientTracker };
                await new TrackerDatabase().SavePatientTrackerAsync(patientTrackerData).ConfigureAwait(false);
            }
            else
            {
                await SavePatientTrackerToServerAsync(patientTrackerData, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            patientTrackerData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
    }

    /// <summary>
    /// Save Patient Tracker Values in db
    /// </summary>
    /// <param name="patientTrackerData"></param>
    /// <returns>Operation Status</returns>
    public async Task SavePatientTrackerValueAsync(TrackerDTO patientTrackerData)
    {
        try
        {
            patientTrackerData.PatientTrackerValues = new List<PatientTrackersValuesModel> { patientTrackerData.PatientTrackerValue };
            await new TrackerDatabase().ValidatePatientTrackerValuesAsync(patientTrackerData).ConfigureAwait(false);
            if (MobileConstants.IsMobilePlatform && patientTrackerData.TrackerRange != null)
            {
                await new TrackerDatabase().SavePatientTrackerValuesAsync(patientTrackerData).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            patientTrackerData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
    }

    private void MapPatientTrackerID(TrackerDTO trackerData, HttpServiceModel<TrackerDTO> httpData)
    {
        JToken data = JToken.Parse(httpData.Response);
        if (data?.HasValues == true)
        {
            JToken patientProgramJData = data[nameof(trackerData.PatientTracker)];
            if (patientProgramJData.HasValues)
            {
                trackerData.PatientTracker.PatientTrackerID = (Guid)patientProgramJData[nameof(PatientTrackersModel.PatientTrackerID)];
            }
            JToken patientTrackerJData = data[nameof(trackerData.PatientTrackerValue)];
            if (patientTrackerJData.HasValues)
            {
                trackerData.PatientTrackerValue.PatientTrackerID = (Guid)patientTrackerJData[nameof(PatientTrackersValuesModel.PatientTrackerID)];
            }
        }
    }

    private async Task SyncPatientTrackersFromServerAsync(TrackerDTO trackerData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_PATIENT_TRACKERS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(_essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0), CultureInfo.InvariantCulture) },
                    { nameof(PatientTrackersModel.PatientTrackerID), Convert.ToString(trackerData.PatientTracker?.PatientTrackerID, CultureInfo.InvariantCulture)},
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(trackerData.RecordCount, CultureInfo.InvariantCulture)},
                    { nameof(PatientTrackersModel.TrackerName), trackerData.PatientTracker?.TrackerName },
                    { nameof(BaseDTO.FromDate), trackerData.FromDate },
                    { nameof(BaseDTO.ToDate), trackerData.ToDate }
                }
            };
            await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
            trackerData.ErrCode = httpData.ErrCode;
            if (trackerData.ErrCode == ErrorCode.OK)
            {
                MapGetPatientTrackersServiceResponse(trackerData, httpData.Response);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            trackerData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    private void MapGetPatientTrackersServiceResponse(TrackerDTO trackerData, string jsonResponse)
    {
        JToken data = JToken.Parse(jsonResponse);
        if (data != null && data.HasValues)
        {
            MapCommonData(trackerData, data);
            SetResourcesAndSettings(trackerData);
            if (trackerData.RecordCount == -1)
            {
                trackerData.PatientTracker = MapPatientTracker(data[nameof(TrackerDTO.PatientTracker)]);
                trackerData.TrackerTypes = GetPickerSource(data, nameof(TrackerDTO.TrackerTypes), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), trackerData.PatientTracker?.TrackerID ?? -1, false, nameof(OptionModel.ParentOptionID));
            }
            else if (trackerData.RecordCount == -3)
            {
                trackerData.PatientTracker = MapPatientTracker(data[nameof(TrackerDTO.PatientTracker)]);
                trackerData.TrackerRanges = MapTrackerRanges(data, nameof(TrackerDTO.TrackerRanges));
            }
            else
            {
                trackerData.PatientTrackers = MapPatientTrackers(data, nameof(TrackerDTO.PatientTrackers));
            }
        }
    }

    internal object MapPatientTrackersHistoryData(MedicalHistoryDTO medicalHistoryData, MedicalHistoryViewModel historyView, string jsonResponse)
    {
        TrackerDTO patientTrackerData = new TrackerDTO
        {
            FromDate = medicalHistoryData.FromDate,
            ToDate = medicalHistoryData.ToDate,
            RecordCount = medicalHistoryData.RecordCount,
            ErrCode = historyView.ErrorCode,
            PatientTracker = new PatientTrackersModel()
        };
        MapGetPatientTrackersServiceResponse(patientTrackerData, jsonResponse);
        patientTrackerData.FeaturePermissions = medicalHistoryData.FeaturePermissions;
        GetPatientTrackersUIData(patientTrackerData);
        historyView.HasData = GenericMethods.IsListNotEmpty(patientTrackerData?.PatientTrackers);
        return patientTrackerData;
    }

    private List<TrackerRangeModel> MapTrackerRanges(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
             ? (from dataItem in data[collectionName]
                select MapRange(dataItem)).ToList()
             : null;
    }

    private TrackerRangeModel MapRange(JToken data)
    {
        return data.HasValues
            ? new TrackerRangeModel
            {
                TrackerRangeID = GetDataItem<short>(data, nameof(TrackerRangeModel.TrackerRangeID)),
                TrackerID = GetDataItem<short>(data, nameof(TrackerRangeModel.TrackerID)),
                FromValue = GetDataItem<int>(data, nameof(TrackerRangeModel.FromValue)),
                ToValue = GetDataItem<int>(data, nameof(TrackerRangeModel.ToValue)),
                CaptionText = GetDataItem<string>(data, nameof(TrackerRangeModel.CaptionText)),
                ImageName = GetDataItem<string>(data, nameof(TrackerRangeModel.ImageName)),
                IsActive = GetDataItem<bool>(data, nameof(TrackerRangeModel.IsActive)),
            }
            : new TrackerRangeModel();
    }

    private void MapFormattedData(TrackerDTO trackerData)
    {
        if (!MobileConstants.IsMobilePlatform)
        {
            SetResourcesAndSettings(trackerData);
        }
        LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        if (GenericMethods.IsListNotEmpty(trackerData.PatientTrackers))
        {
            var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
            trackerData.PatientTrackers.ForEach(trackers =>
            {
                trackers.FromDateDisplayFormatString = $"{GenericMethods.GetLocalDateTimeBasedOnCulture(trackers.FromDate.Value, DateTimeType.Date, dayFormat, monthFormat, yearFormat)}";
                trackers.ToDateDisplayFormatString = $"{GenericMethods.GetLocalDateTimeBasedOnCulture(trackers.ToDate.Value, DateTimeType.Date, dayFormat, monthFormat, yearFormat)}";
                trackers.FromDate = _essentials.ConvertToLocalTime(trackers.FromDate.Value).Date;
                trackers.ToDate = _essentials.ConvertToLocalTime(trackers.ToDate.Value).Date;
                if (string.IsNullOrWhiteSpace(trackers.CurrentValue) || string.IsNullOrWhiteSpace(trackers.ImageName))
                {
                    trackers.LeftDefaultIcon =   ImageConstants.I_TRACKER_ICON_SVG;
                    trackers.CurrentValueDisplayFormatString = !(MobileConstants.IsMobilePlatform)
                        && (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
                        ? LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_CONFIGURE_TRACKER_KEY)
                        : string.Empty;
                }
                else
                {
                    trackers.CurrentValueInDate = GenericMethods.ConvertIsoDateStringToDateTimeOffset(trackers.CurrentValue);
                    trackers.CurrentValueDisplayFormatString = GenericMethods.GetLocalDateTimeBasedOnCulture(trackers.CurrentValueInDate.Value.ToUniversalTime(), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                    //trackers.LeftDefaultIcon = trackers.ImageName != null
                    //    ? GetImageAsBase64Async(trackers.ImageName).Result
                    //    : MobileConstants.IsMobilePlatform
                    //    ? ImageConstants.I_TRACKER_ICON_SVG
                    //    : ImageConstants.I_TRACKER_ICON_SVG;
                    //todo:
                    //trackers.LeftSourceIcon = ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(trackers.LeftDefaultIcon));
                    if (trackers.TrackerName == "Pregnancy" && trackers.CurrentValue != null)
                    {
                        TimeSpan duration = (TimeSpan)(DateTimeOffset.Now.Date - trackers.CurrentValueInDate);
                        trackers.Week = Math.Floor(duration.TotalDays / 7).ToString();
                        trackers.DueDate = trackers.CurrentValueInDate.Value.AddDays(trackers.ProgramDuration);
                        trackers.DueDateString = GenericMethods.GetLocalDateTimeBasedOnCulture(trackers.DueDate.Value.ToUniversalTime(), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                        if (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
                        {
                            trackers.CurrentValueDisplayFormatString = string.Format(CultureInfo.InvariantCulture
                                , LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_TRACKER_INFO_KEY)
                                , trackers.CurrentValueDisplayFormatString, trackers.Week, trackers.DueDateString).ToString();
                            string[] values = trackers.CurrentValueDisplayFormatString.Split('|');
                            if (values.Length > 1)
                            {
                                string[] newValues = new string[values.Length - 1];
                                Array.Copy(values, 1, newValues, 0, values.Length - 1);

                                string output = string.Join("|", newValues);
                                trackers.CurrentValueDisplayFormatString = output;
                            }
                        }
                        else
                        {
                            trackers.CurrentValueDisplayFormatString = string.Format(CultureInfo.InvariantCulture
                                , LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_TRACKER_INFO_KEY)
                                , trackers.CurrentValueDisplayFormatString, trackers.Week, trackers.DueDateString).ToString();
                        }
                    }
                }
            });
            if (roleID != (int)RoleName.Patient && roleID != (int)RoleName.CareTaker)
            {
                //SortPatientTracker(trackerData);
                GenericMethods.SortByDate(trackerData.PatientTrackers, x => x.FromDate, x => x.ToDate);
            }

        }
        else if (trackerData.RecordCount == -1 && trackerData.PatientTracker.PatientTrackerID != Guid.Empty)
        {
            trackerData.PatientTracker.FromDate = _essentials.ConvertToLocalTime(trackerData.PatientTracker.FromDate.Value);
            trackerData.PatientTracker.ToDate = _essentials.ConvertToLocalTime(trackerData.PatientTracker.ToDate.Value);
        }
        else if (!string.IsNullOrWhiteSpace(trackerData.PatientTracker.CurrentValue))
        {
            trackerData.PatientTracker.CurrentValueInDate = string.IsNullOrWhiteSpace(trackerData.PatientTracker.CurrentValue)
                ? DateTime.Now
                : GenericMethods.ConvertIsoDateStringToDateTimeOffset(trackerData.PatientTracker.CurrentValue);
        }
    }

    public async Task<bool> ValidatePatientTrackerValue(TrackerDTO patientTrackerData)
    {
        if (!MobileConstants.IsMobilePlatform)
        {
            SetResourcesAndSettings(patientTrackerData);
        }
        else
        {
            await GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP);
            await GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_TRACKERS_GROUP, GroupConstants.RS_QUESTIONNAIRE_QUESTION_TYPE_GROUP, GroupConstants.RS_PROGRAMS_GROUP);
        }
        LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        int maxValue = patientTrackerData.TrackerRanges.Where(item => item.TrackerID == patientTrackerData.PatientTracker.TrackerID).Max(value => value.ToValue);
        var currentValueInDate = GenericMethods.ConvertIsoDateStringToDateTimeOffset(patientTrackerData?.PatientTrackerValue?.CurrentValue);
        var currentDate = Math.Abs((currentValueInDate?.Date - patientTrackerData.PatientTracker.FromDate.Value.Date)?.Days ?? 0);
        DateTime time2 = patientTrackerData.PatientTracker.FromDate.Value.DateTime.Subtract(TimeSpan.FromDays(maxValue));
        if (maxValue < currentDate)
        {
            patientTrackerData.PatientTracker.FromDateDisplayFormatString = $"{GenericMethods.GetLocalDateTimeBasedOnCulture(time2, DateTimeType.Date, dayFormat, monthFormat, yearFormat)}";
            patientTrackerData.PatientTracker.ToDateDisplayFormatString = $"{GenericMethods.GetLocalDateTimeBasedOnCulture(DateTime.Now, DateTimeType.Date, dayFormat, monthFormat, yearFormat)}";
            return false;
        }
        return true;
    }

    private void CheckIsEdit(TrackerDTO trackerData)
    {
        if (trackerData.PatientTracker.ProgramTrackerID > 0)
        {
            int value;
            if (MobileConstants.IsMobilePlatform)
            {
                value = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
            }
            else
            {
                value = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)5);
            }
            if ((value == (int)RoleName.Patient || value == (int)RoleName.CareTaker) && trackerData.PatientTracker.ValueAddedBy == (short)ReadingAddedBy.PatientKey || trackerData.PatientTracker.ValueAddedBy == (short)ReadingAddedBy.BothKey)
            {
                if (trackerData.PatientTracker.ToDate.Value.Date < GenericMethods.GetUtcDateTime.Date)
                {
                    trackerData.PatientTracker.IsEdit = false;
                }
                else
                {
                    trackerData.PatientTracker.IsEdit = true;
                }
            }
            else if (value == (int)RoleName.Doctor && trackerData.PatientTracker.ValueAddedBy == (short)ReadingAddedBy.ProviderKey || trackerData.PatientTracker.ValueAddedBy == (short)ReadingAddedBy.BothKey)
            {
                if (trackerData.PatientTracker.ToDate.Value.Date < GenericMethods.GetUtcDateTime.Date)
                {
                    trackerData.PatientTracker.IsEdit = false;
                }
                else
                {
                    trackerData.PatientTracker.IsEdit = true;
                }
            }
            else
            {
                trackerData.PatientTracker.IsEdit = false;
            }
        }
        else if (trackerData.PatientTracker != null)
        {
            if (trackerData.PatientTracker.PatientTrackerID != Guid.Empty)
            {
                if (trackerData.PatientTracker.ToDate != null)
                {
                    if (trackerData.PatientTracker.ToDate.Value.Date < GenericMethods.GetUtcDateTime.Date)
                    {
                        trackerData.PatientTracker.IsEdit = false;
                    }
                    else
                    {
                        trackerData.PatientTracker.IsEdit = true;
                    }
                }
            }
            else
            {
                trackerData.PatientTracker.IsEdit = true;
            }
        }
        else
        {
            trackerData.PatientTracker.IsEdit = true;
        }
    }

    private List<PatientTrackersValuesModel> MapPatientTrackerValues(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select MapPatientTrackerValues(dataItem)).ToList()
            : new List<PatientTrackersValuesModel>();
    }

    private PatientTrackersValuesModel MapPatientTrackerValues(JToken dataItem)
    {
        return new PatientTrackersValuesModel
        {
            PatientTrackerID = GetDataItem<Guid>(dataItem, nameof(PatientTrackersValuesModel.PatientTrackerID)),
            CurrentValue = GetDataItem<string>(dataItem, nameof(PatientTrackersValuesModel.CurrentValue)),
            IsActive = GetDataItem<bool>(dataItem, nameof(PatientTrackersValuesModel.IsActive)),
            IsSynced = true,
        };
    }

    private PatientTrackersModel MapPatientTracker(JToken dataItem)
    {
        return dataItem.HasValues
             ? new PatientTrackersModel
             {
                 PatientTrackerID = (Guid)dataItem[nameof(PatientTrackersModel.PatientTrackerID)],
                 ProgramTrackerID = (long)dataItem[nameof(PatientTrackersModel.ProgramTrackerID)],
                 TrackerID = GetDataItem<short>(dataItem, nameof(PatientTrackersModel.TrackerID)),
                 TrackerName = GetDataItem<string>(dataItem, nameof(PatientTrackersModel.TrackerName)),
                 FromDate = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientTrackersModel.FromDate)),
                 ToDate = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientTrackersModel.ToDate)),
                 IsEdit = GetDataItem<bool>(dataItem, nameof(PatientTrackersModel.IsEdit)),
                 ImageName = GetDataItem<string>(dataItem, nameof(PatientTrackersModel.ImageName)),
                 InstructionsText = GetDataItem<string>(dataItem, nameof(PatientTrackersModel.InstructionsText)),
                 CaptionText = GetDataItem<string>(dataItem, nameof(PatientTrackersModel.CaptionText)),
                 CurrentValue = GetDataItem<string>(dataItem, nameof(PatientTrackersModel.CurrentValue)),
                 IsActive = GetDataItem<bool>(dataItem, nameof(PatientTrackersModel.IsActive)),
                 ProgramID = GetDataItem<long>(dataItem, nameof(PatientTrackersModel.ProgramID)),
                 UserID = GetDataItem<long>(dataItem, nameof(PatientTrackersModel.UserID)),
                 TrackerTypeID = GetDataItem<short>(dataItem, nameof(PatientTrackersModel.TrackerTypeID)),
                 AddedON = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientTrackersModel.AddedON)),
                 ValueAddedBy = GetDataItem<short>(dataItem, nameof(PatientTrackersModel.ValueAddedBy)),
                 ProgramColor = GetDataItem<string>(dataItem, nameof(PatientTrackersModel.ProgramColor)),
                 ProgramDuration = GetDataItem<int>(dataItem, nameof(PatientTrackersModel.ProgramDuration)),
                 IsSynced = true,
             }
             : new PatientTrackersModel();
    }

    private List<PatientTrackersModel> MapPatientTrackers(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select MapPatientTracker(dataItem)).ToList()
            : null;
    }
}
