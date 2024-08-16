using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading main implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    private ReadingDatabase _readingDB { get; set; }
    private UnitConverterService UnitConverter { get; set; }

    /// <summary>
    /// Get logged in user account ID
    /// </summary>
    /// <returns> Return loggedIn user Account ID</returns>
    public string LoggedInAccountID => MobileConstants.IsMobilePlatform
            ? Convert.ToString(_essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0), CultureInfo.InvariantCulture)
            : _essentials.GetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, Constants.NUMBER_ZERO);

    /// <summary>
    /// Reading service
    /// </summary>
    public ReadingService(IEssentials serviceEssentials) : base(serviceEssentials)
    {
        _readingDB = new ReadingDatabase();
        UnitConverter = new UnitConverterService(_essentials);
    }

    /// <summary>
    /// Gets Reading Value
    /// </summary>
    /// <param name="readingsData">Refernce object having data</param>
    /// <returns>Reading Value</returns>
    public string GetLatestValue(PatientReadingDTO readingsData, List<ReadingMetadataUIModel> metadata, out long readingID)
    {
        var latestReadingData = readingsData.ListData?.OrderByDescending(x => x.ReadingDateTime)?.ThenBy(x => x.SequenceNo)?.FirstOrDefault();
        readingID = latestReadingData.ReadingID;

        var sys = readingsData.ListData.FirstOrDefault(x => x.ReadingID == ResourceConstants.R_BP_SYSTOLIC_KEY_ID);
        var dias = readingsData.ListData.FirstOrDefault(x => x.ReadingID == ResourceConstants.R_BP_DIASTOLIC_KEY_ID);
        return readingsData.ReadingID == ResourceConstants.R_BLOOD_PRESSURE_KEY_ID
            ? GetBloodPressureValue(
                FetchValueBasedOnValueType(metadata.FirstOrDefault(x => x.ReadingID == ResourceConstants.R_BP_SYSTOLIC_KEY_ID), sys?.ReadingValue, sys?.ReadingValue2),
                FetchValueBasedOnValueType(metadata.FirstOrDefault(x => x.ReadingID == ResourceConstants.R_BP_DIASTOLIC_KEY_ID), dias?.ReadingValue, dias?.ReadingValue2))
            : FetchValueBasedOnValueType(metadata.FirstOrDefault(x => x.ReadingID == latestReadingData.ReadingID), latestReadingData.ReadingValue, latestReadingData.ReadingValue2);
    }

    /// <summary>
    /// Gets Reading Value
    /// </summary>
    /// <param name="readingID">id of reading</param>
    /// <param name="metadata">Refernce object having metadata</param>
    /// <param name="latestReadingData">latest metadata</param>
    /// <returns>Reading Value</returns>
    public string SetLatestMetadataValue(short readingID, List<ReadingMetadataUIModel> metadata, ReadingMetadataUIModel latestReadingData)
    {
        if (readingID == ResourceConstants.R_BLOOD_PRESSURE_KEY_ID)
        {
            var sys = metadata.FirstOrDefault(x => x.ReadingID == ResourceConstants.R_BP_SYSTOLIC_KEY_ID);
            var dias = metadata.FirstOrDefault(x => x.ReadingID == ResourceConstants.R_BP_DIASTOLIC_KEY_ID);
            return GetBloodPressureValue(
                FetchValueBasedOnValueType(sys, sys?.ReadingValue, sys?.ReadingValue2),
                FetchValueBasedOnValueType(dias, dias?.ReadingValue, dias?.ReadingValue2)
            );
        }
        else
        {
            return FetchValueBasedOnValueType(latestReadingData, latestReadingData.ReadingValue, latestReadingData.ReadingValue2);
        }
    }

    /// <summary>
    /// Gets Patient Reading data
    /// </summary>
    /// <param name="readingsData">Reference object to provide input and store received readings data</param>
    /// <returns>return reading data for respective page</returns>
    public async Task GetPatientReadingsAsync(PatientReadingDTO readingsData)
    {
        try
        {
            readingsData.SelectedUserID = GetUserID();
            if (MobileConstants.IsMobilePlatform)
            {
                readingsData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                switch (readingsData.RecordCount)
                {
                    //// RecordCount = -3 : Get Data for Patient Reading Target Add Edit Page
                    case -3:
                        await GetPatientReadingTargetAsync(readingsData).ConfigureAwait(true);
                        break;
                    //// RecordCount = -2 : Get Data for Patient Reading Details View
                    case -2:
                        ClearGraphData(readingsData);
                        if (readingsData.FromDate == null && readingsData.ToDate == null)
                        {
                            await Task.WhenAll(
                                GetReadingPageDataAsync(readingsData),
                                GetMetadataAsync(readingsData),
                                 _readingDB.GetCategoryRelationsAsync(readingsData)
                            ).ConfigureAwait(true);
                            ReadingMetadataUIModel metadata = readingsData.ChartMetaData.FirstOrDefault();
                            if (metadata == null)
                            {
                                readingsData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                                return;
                            }
                            else
                            {
                                readingsData.ShowSetTargetButton = CheckFeaturePermissionByCode(AppPermissions.PatientReadingTargetAddEdit.ToString())
                                    && readingsData.ChartMetaData?.Count > 0
                                    && readingsData.ChartMetaData[0].IsActive;
                                metadata.ListItemLeftHeader = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DATE_TIME_TEXT_KEY);
                            }
                            MapCategoryOptions(readingsData);
                            readingsData.FilterOptions = GetDurationData(metadata.ReadingFilters, 0);
                            var defaultFilter = readingsData.FilterOptions.FirstOrDefault(x => x.IsDefault).ParentOptionID;
                            var utcDateTIme = GenericMethods.GetUtcDateTime;
                            if (defaultFilter == -1)
                            {
                                GenericMethods.TryCalculateStartEndDate((int)defaultFilter, _essentials.ConvertToLocalTime(utcDateTIme), out DateTimeOffset? startDate, out DateTimeOffset? endDate);
                                readingsData.FromDate = startDate?.ToString(CultureInfo.InvariantCulture);
                                readingsData.ToDate = endDate?.ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                readingsData.ToDate = utcDateTIme.ToString(CultureInfo.InvariantCulture);
                                readingsData.FromDate = GenericMethods.GetDateFromDateTime(_essentials.ConvertToLocalTime(utcDateTIme).AddDays(-defaultFilter + 1)).ToUniversalTime().ToString(CultureInfo.InvariantCulture);////.AddDays(-defaultFilter)).ToUniversalTime();
                            }
                        }
                        else
                        {
                            SetResourcesAndSettings(readingsData);
                            PageData.FeaturePermissions = readingsData.FeaturePermissions;
                        }
                        await _readingDB.GetPatientReadingsAsync(readingsData, DateTimeOffset.Parse(readingsData.FromDate, CultureInfo.InvariantCulture), DateTimeOffset.Parse(readingsData.ToDate, CultureInfo.InvariantCulture)).ConfigureAwait(false);
                        break;
                    //// RecordCount = -1 : Get Data for Patient Reading Add Edit Page
                    case -1:
                        await Task.WhenAll(
                            GetReadingPageDataAsync(readingsData),
                            _readingDB.GetCategoryRelationsAsync(readingsData)
                        ).ConfigureAwait(false);
                        MapCategoryOptions(readingsData);
                        await GetMetadataAsync(readingsData).ConfigureAwait(false);
                        await _readingDB.GetPatientReadingAsync(readingsData).ConfigureAwait(false);
                        break;
                    //// RecordCount = 0  : Get Data for Patient Readings List Page
                    //// RecordCount > 0  : Get Data for Patient Readings Dashboard List View
                    default:
                        await Task.WhenAll(
                            GetReadingPageDataAsync(readingsData),
                            _readingDB.GetCategoryRelationsAsync(readingsData)
                        ).ConfigureAwait(false);
                        MapCategoryOptions(readingsData);
                        if (GenericMethods.IsListNotEmpty(readingsData.FilterOptions))
                        {
                            await GetMetadataAsync(readingsData).ConfigureAwait(false);
                        }
                        var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
                        readingsData.IsActive = readingsData.ErrCode == ErrorCode.OK
                            && GenericMethods.IsListNotEmpty(readingsData.FilterOptions)
                            && IsAddAllowed(readingsData.ChartMetaData, (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker));
                        break;
                }
                readingsData.ErrCode = ErrorCode.OK;
            }
            else
            {
                await SyncReadingsPageDataFromServerAsync(readingsData).ConfigureAwait(false);
            }
            if (await MapPatientReadingsUIDataAsync(readingsData).ConfigureAwait(false))
            {
                // Call again to get data for selected filter
                await GetPatientReadingsAsync(readingsData).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            readingsData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Update validation resources based on questionnaire question 
    /// </summary>
    /// <param name="readingData">Reference object to provide input and store received readings data</param>
    public void MakeValidationResourceChangesForQuestionnaireQuestion(PatientReadingDTO readingData)
    {
        if (readingData.IsCommingFromQuestionnaireTaskPage && GenericMethods.IsListNotEmpty(readingData.Resources))
        {
            foreach (var reading in readingData.ListData)
            {
                if (reading.ReadingID != 0)
                {
                    readingData.Resources.FirstOrDefault(x => x.ResourceKeyID == reading.ReadingID).IsRequired = readingData.IsRequiredQuestion;
                }
            }
            readingData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_ENTER_FOOD_TEXT_KEY).IsRequired = readingData.IsRequiredQuestion;
            readingData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_PORTION_SIZE_TEXT_KEY).IsRequired = readingData.IsRequiredQuestion;
            readingData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SEARCH_FOOD_TEXT_KEY).IsRequired = readingData.IsRequiredQuestion;
        }
    }

    internal async Task<object> MapPatientReadingHistoryDataAsync(MedicalHistoryDTO medicalHistoryData, MedicalHistoryViewModel historyView, string readingResponse)
    {
        PatientReadingDTO readingData = new PatientReadingDTO
        {
            IsMedicalHistory = true,
            FromDate = medicalHistoryData.FromDate,
            ToDate = medicalHistoryData.ToDate,
            RecordCount = medicalHistoryData.RecordCount,
            ErrCode = historyView.ErrorCode,
        };
        await MapGetPatientReadingsServiceResponseAsync(readingData, readingResponse);
        readingData.FeaturePermissions = medicalHistoryData.FeaturePermissions;
        await MapPatientReadingsUIDataAsync(readingData);
        historyView.HasData = GenericMethods.IsListNotEmpty(readingData.FilterOptions)
            && GenericMethods.IsListNotEmpty(readingData.ChartMetaData)
            && GenericMethods.IsListNotEmpty(readingData.ReadingDTOs)
            && (readingData.ChartMetaData?.Any(reading => reading.ReadingDateTime.HasValue) ?? false);
        return readingData;
    }

    private async Task SyncReadingsPageDataFromServerAsync(PatientReadingDTO readingsData)
    {
        var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
        {
            PathWithoutBasePath = UrlConstants.GET_PATIENT_READINGS_ASYNC_PATH,
            QueryParameters = new NameValueCollection {
                { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(readingsData.SelectedUserID, CultureInfo.InvariantCulture) },
                { nameof(PatientReadingDTO.RecordCount), Convert.ToString(readingsData.RecordCount, CultureInfo.InvariantCulture) },
                { nameof(PatientReadingDTO.ReadingCategoryID),  Convert.ToString(readingsData.ReadingCategoryID, CultureInfo.InvariantCulture) },
                { nameof(PatientReadingDTO.PatientReadingID), readingsData.PatientReadingID.ToString() },
                { nameof(PatientReadingDTO.ReadingID), readingsData.ReadingID.ToString() },
                { nameof(BaseDTO.FromDate), readingsData.FromDate },
                { nameof(BaseDTO.ToDate), readingsData.ToDate },
                { nameof(PatientReadingDTO.IsMedicalHistory), Convert.ToString(readingsData.IsMedicalHistory, CultureInfo.InvariantCulture) },
                { nameof(PatientReadingDTO.IsCommingFromQuestionnaireTaskPage), Convert.ToString(readingsData.IsCommingFromQuestionnaireTaskPage, CultureInfo.InvariantCulture) },
            }
        };
        await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(true);
        readingsData.ErrCode = httpData.ErrCode;
        if (readingsData.ErrCode == ErrorCode.OK)
        {
            await MapGetPatientReadingsServiceResponseAsync(readingsData, httpData.Response).ConfigureAwait(false);
        }
    }

    public async Task SyncPatientScanVitalsDataFromServerAsync(PatientScanVitalDTO vitalsData)
    {
        var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
        {
            PathWithoutBasePath = UrlConstants.GET_PATIENT_SCAN_VITALS_DATA_ASYNC_PATH,
            QueryParameters = new NameValueCollection {
                { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(vitalsData.SelectedUserID, CultureInfo.InvariantCulture) },
                { nameof(PatientScanVitalDTO.RecordCount), Convert.ToString(vitalsData.RecordCount, CultureInfo.InvariantCulture) },
            }
        };
        await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(true);
        vitalsData.ErrCode = httpData.ErrCode;
        if (vitalsData.ErrCode == ErrorCode.OK)
        {
            JToken data = JToken.Parse(httpData.Response);
            if (data != null && data.HasValues)
            {
                MapCommonData(vitalsData, data);
                // MapEducationCategories(data, vitalsData);
            }
            await MapGetPatientScanVitalsServiceResponseAsync(vitalsData, httpData.Response).ConfigureAwait(false);
        }
    }
    private async Task MapGetPatientScanVitalsServiceResponseAsync(PatientScanVitalDTO vitalsData, string response)
    {
        JToken data = JToken.Parse(response);
        if (data != null && data.HasValues)
        {
            //MapReadingsResourcesAndSettings(vitalsData, data);
            //patientProviderNoteDTO.PatientProviderNote.NoteDateTime = _essentials.ConvertToLocalTime(patientProviderNoteDTO.PatientProviderNote.NoteDateTime.Value);
            //vitalsData.PatientScanVital.DateOfBirth = 
            //readingData.ChartMetaData = MapReadingUIMetadatas(data, nameof(PatientReadingDTO.ChartMetaData));
            //readingData.ListData = MapPatientReadingUIModels(data, nameof(PatientReadingDTO.ListData)) ?? new List<PatientReadingUIModel>();
            //readingData.ReadingUnits = MapReadingUnits(data, nameof(PatientReadingDTO.ReadingUnits));
            //readingData.UserAccountSettings = new UserAccountSettingService(_essentials).MapUserAccountSettings(data, nameof(PatientReadingDTO.UserAccountSettings));
            //if (readingData.ReadingCategoryID < 1)
            //{
            //    readingData.ReadingCategoryID = readingData.ChartMetaData?.FirstOrDefault()?.ReadingCategoryID ?? 0;
            //}
            //await ConvertMetaDataUnitAsync(readingData).ConfigureAwait(false);
            //switch (readingData.RecordCount)
            //{
            //    //// RecordCount = -3 : Get Data for Patient Reading Target Add Edit Page
            //    case -3:
            //        break;
            //    //// RecordCount = -2 : Get Data for Patient Reading Details View  
            //    case -2:
            //        break;
            //    //// RecordCount = -1 : Get Data for Patient Reading Add Edit Page 
            //    case -1:
            //        MapCategoriesFilterOptions(readingData, data);
            //        //MapCategories(readingData);
            //        readingData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            //        break;
            //    //// RecordCount = 0  : Get Data for Patient Readings List Page              ////RequestParameters : ReadingCategoryID, RecordCount 
            //    //// RecordCount > 0  : Get Data for Patient Readings Dashboard List View    ////RequestParameters : ReadingCategoryID, RecordCount 
            //    default:
            //        MapCategoriesFilterOptions(readingData, data);
            //        break;
            //}
        }
    }
    private async Task MapGetPatientReadingsServiceResponseAsync(PatientReadingDTO readingData, string response)
    {
        JToken data = JToken.Parse(response);
        if (data != null && data.HasValues)
        {
            MapReadingsResourcesAndSettings(readingData, data);
            readingData.User = MapUser(data[nameof(UserDTO.User)]);
            readingData.Genders = MapResourcesIntoOptions(GroupConstants.RS_GENDER_TYPE_GROUP, readingData.User?.GenderID ?? string.Empty, string.Empty, false);
            readingData.Posture = MapResourcesIntoOptions(GroupConstants.RS_POSTURE_GROUP, readingData.PostureID ?? string.Empty, string.Empty, false);
            readingData.ScanType = MapResourcesIntoOptions(GroupConstants.RS_SCAN_TYPE_GROUP, readingData.ScanTypeID ?? string.Empty, string.Empty, false);
            readingData.ChartMetaData = MapReadingUIMetadatas(data, nameof(PatientReadingDTO.ChartMetaData));
            readingData.ListData = MapPatientReadingUIModels(data, nameof(PatientReadingDTO.ListData)) ?? new List<PatientReadingUIModel>();
            readingData.ReadingUnits = MapReadingUnits(data, nameof(PatientReadingDTO.ReadingUnits));
            readingData.UserAccountSettings = new UserAccountSettingService(_essentials).MapUserAccountSettings(data, nameof(PatientReadingDTO.UserAccountSettings));
            if (readingData.ReadingCategoryID < 1)
            {
                readingData.ReadingCategoryID = readingData.ChartMetaData?.FirstOrDefault()?.ReadingCategoryID ?? 0;
            }
            await ConvertMetaDataUnitAsync(readingData).ConfigureAwait(false);
            switch (readingData.RecordCount)
            {
                //// RecordCount = -3 : Get Data for Patient Reading Target Add Edit Page
                case -3:
                    break;
                //// RecordCount = -2 : Get Data for Patient Reading Details View  
                case -2:
                    break;
                //// RecordCount = -1 : Get Data for Patient Reading Add Edit Page 
                case -1:
                    MapCategoriesFilterOptions(readingData, data);
                    //MapCategories(readingData);
                    readingData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                    break;
                //// RecordCount = 0  : Get Data for Patient Readings List Page              ////RequestParameters : ReadingCategoryID, RecordCount 
                //// RecordCount > 0  : Get Data for Patient Readings Dashboard List View    ////RequestParameters : ReadingCategoryID, RecordCount 
                default:
                    MapCategoriesFilterOptions(readingData, data);
                    break;
            }
        }
    }

    private UserModel MapUser(JToken dataItem)
    {
        return dataItem.HasValues
            ? new UserModel
            {
                UserID = GetDataItem<long>(dataItem, nameof(UserModel.UserID)),
                AccountID = GetDataItem<long>(dataItem, nameof(UserModel.AccountID)),
                FirstName = (string)dataItem[nameof(UserModel.FirstName)],
                MiddleName = (string)dataItem[nameof(UserModel.MiddleName)],
                LastName = (string)dataItem[nameof(UserModel.LastName)],
                IsActive = GetDataItem<bool>(dataItem, nameof(UserModel.IsActive)),
                ImageName = Convert.ToString(dataItem[nameof(UserModel.ImageName)], CultureInfo.InvariantCulture),
                GenderID = (string)dataItem[nameof(UserModel.GenderID)],
                UserDegrees = (string)dataItem[nameof(UserModel.UserDegrees)],
                EmailId = (string)dataItem[nameof(UserModel.EmailId)],
                PhoneNo = (string)dataItem[nameof(UserModel.PhoneNo)],
                IsTempPassword = GetDataItem<bool>(dataItem, nameof(UserModel.IsTempPassword)),
                OrganisationID = GetDataItem<long>(dataItem, nameof(UserModel.OrganisationID)),
                Dob = GetDataItem<DateTimeOffset>(dataItem, nameof(UserModel.Dob)),
                IsSynced = true
            }
            : new UserModel();
    }

    private async Task<bool> MapPatientReadingsUIDataAsync(PatientReadingDTO readingsData)
    {
        // Map readings UI Data
        if (readingsData.ErrCode == ErrorCode.OK)
        {
            switch (readingsData.RecordCount)
            {
                //// RecordCount = -3 : Get Data for Patient Reading Target Add Edit Page
                case -3:
                    if (GenericMethods.IsListNotEmpty(readingsData.ChartMetaData))
                    {
                        readingsData.ReadingOptions = MapReadingTypesFromMetadata(readingsData);
                        readingsData.PatientReadingTargets = MapChartMetaDataToPatientReadingTargets(readingsData);
                        if (GenericMethods.IsListNotEmpty(readingsData.ReadingOptions) && readingsData.ReadingOptions.FirstOrDefault(x => x.IsSelected) == null)
                        {
                            readingsData.ReadingOptions.FirstOrDefault().IsSelected = true;
                        }
                    }
                    break;
                //// RecordCount = -2 : Get Data for Patient Reading Details View
                case -2:
                    bool hasDurationData = true;
                    if (GenericMethods.IsListNotEmpty(readingsData.ChartMetaData))
                    {
                        var metadata = readingsData.ChartMetaData.FirstOrDefault();
                        if (readingsData.FilterOptions == null || readingsData.FilterOptions.Count < 1)
                        {
                            readingsData.FilterOptions = GetDurationData(metadata.ReadingFilters, 0);
                        }
                        else
                        {
                            short selectedID = (short)readingsData.FilterOptions.FirstOrDefault(x => x.IsSelected).OptionID;
                            readingsData.FilterOptions = GetDurationData(metadata.ReadingFilters, selectedID);
                        }
                        hasDurationData = AdjustStartAndEndDateForWeb(readingsData, readingsData.Action);
                    }
                    if (hasDurationData)
                    {
                        // Convert PatientReadings data to Patient units
                        if (GenericMethods.IsListNotEmpty(readingsData.ListData))
                        {
                            await ConvertReadingUnitsAsync(readingsData).ConfigureAwait(false);
                            if ((short)readingsData.FilterOptions.FirstOrDefault(x => x.IsSelected).ParentOptionID == -1)
                            {
                                AdjustStartEndDates(readingsData);
                            }
                            MapChartData(readingsData);
                        }
                        await GenerateChartDataAsync(readingsData, false).ConfigureAwait(true);
                    }
                    else
                    {
                        return true;
                    }
                    break;
                //// RecordCount = -1 : Get Data for Patient Reading Add Edit Page
                case -1:
                    await ConvertReadingUnitsAsync(readingsData).ConfigureAwait(false);
                    MapReadingAddEditUIDataFromMetadata(readingsData);
                    MakeValidationResourceChangesForQuestionnaireQuestion(readingsData);
                    PageData.Resources = readingsData.Resources;
                    break;
                //// RecordCount = 0  : Get Data for Patient Readings List Page
                //// RecordCount > 0  : Get Data for Patient Readings Dashboard List View
                default:
                    if (GenericMethods.IsListNotEmpty(readingsData.FilterOptions))
                    {
                        MapReadingsListUIData(readingsData);
                    }
                    readingsData.ErrCode = ErrorCode.OK;
                    break;
            }
        }
        return false;
    }

    private void ClearGraphData(PatientReadingDTO readingsData)
    {
        if (readingsData.GraphData == null)
        {
            readingsData.GraphData = new List<List<PatientReadingUIModel>>();
        }
        else
        {
            readingsData.GraphData.Clear();
        }
    }

    /// <summary>
    /// Save patient reading data
    /// </summary>
    /// <param name="readingsData">object to save reading data</param>
    /// <returns>Operation Status</returns>
    public async Task SavePatientReadingAsync(PatientReadingDTO readingsData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                if (readingsData.ListData.Any(x => x.IsActive))
                {
                    // Adjust datetime frequency
                    foreach (PatientReadingUIModel reading in readingsData.ListData)
                    {
                        var metadata = readingsData.ChartMetaData.FirstOrDefault(x => x.ReadingID == reading.ReadingID);
                        reading.ReadingDateTime = reading.ReadingDateTime.Value.ToUniversalTime();
                        await CallUnitConverterAsync(metadata, reading, false).ConfigureAwait(false);
                    }
                    await CalculateBmiAsync(readingsData).ConfigureAwait(false);
                }
                await _readingDB.SavePatientReadingsAsync(readingsData).ConfigureAwait(false);
                readingsData.ErrCode = ErrorCode.OK;
            }
            else
            {
                MapUnitsDataIntoUnitConverter(readingsData);
                if (readingsData.PatientReadings.Any(x => x.IsActive))
                {
                    // Adjust datetime frequency
                    foreach (var reading in readingsData.PatientReadings)
                    {
                        var metadata = readingsData.ChartMetaData.FirstOrDefault(x => x.ReadingID == reading.ReadingID);
                        await CallUnitConverterAsync(metadata, reading, false).ConfigureAwait(false);
                    }
                }
                await SyncPatientReadingsToServerAsync(readingsData, CancellationToken.None).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            readingsData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
    }

    /// <summary>
    /// get reading value options
    /// </summary>
    /// <param name="groupID"></param>
    /// <param name="selectedID"></param>
    /// <returns>List of reading value options</returns>
    public List<OptionModel> GetOptions(short groupID, short selectedID)
    {
        return MapResourcesIntoOptionsByGroupID(groupID, string.Empty, false, selectedID);
    }

    private bool IsAddedByProvider(PatientReadingDTO readingDtaa)
    {
        return readingDtaa.ListData[0].ReadingSourceType == ReadingSource.ProviderManual.ToString();
    }

    private string GetAllowedRoles(bool isPatientLogin)
    {
        return isPatientLogin
            ? $"{ResourceConstants.R_BOTH_KEY_ID},{ResourceConstants.R_PATIENT_KEY_ID}"
            : $"{ResourceConstants.R_BOTH_KEY_ID},{ResourceConstants.R_PROVIDER_KEY_ID}";
    }

    private void MapReadingAddEditUIDataFromMetadata(PatientReadingDTO readingData)
    {
        if (GenericMethods.IsListNotEmpty(readingData.ChartMetaData))
        {
            var isAdd = readingData.PatientReadingID == Guid.Empty;
            if (GenericMethods.IsListNotEmpty(readingData.ListData))
            {
                foreach (PatientReadingUIModel reading in readingData.ListData)
                {
                    if (isAdd)
                    {
                        var metadata = readingData.ChartMetaData.FirstOrDefault(x => x.ReadingID == reading.ReadingID);
                        reading.ReadingValue = reading.ReadingValue != null ? Math.Round(reading.ReadingValue.Value, metadata.DigitsAfterDecimalPoint) : null;
                        reading.PatientReadingID = Guid.Empty;
                        reading.ReadingDateTime = DateTimeOffset.UtcNow;
                        reading.ReadingNotes = string.Empty;
                    }
                    reading.ReadingDateTime = _essentials.ConvertToLocalTime(reading.ReadingDateTime.Value);
                }
                if (!isAdd && !readingData.IsCommingFromQuestionnaireTaskPage)
                {
                    readingData.ChartMetaData.RemoveAll(x => !readingData.ListData.Any(y => y.ReadingID == x.ReadingID));
                }
            }
            else
            {
                readingData.ListData = new List<PatientReadingUIModel>() { new PatientReadingUIModel{
                   ReadingDateTime =  _essentials.ConvertToLocalTime(DateTimeOffset.UtcNow)
                }};
            }

            MapReadingParentOptions(readingData, isAdd);
            UpdateReadingResources(readingData, isAdd);
            if (!isAdd && readingData.ListData?.Count > 0)
            {
                readingData.ListData[0].AddedByText = GetReadingPerformer(readingData.ListData[0]);
            }
        }
    }

    public static List<PatientReadingModel> MapVitals(JToken data, PatientReadingDTO readingData)
    {
        var readings = new List<PatientReadingModel>();
        var vitals = data["vitals"];
        if (vitals != null)
        {
            foreach (var readingType in Enum.GetValues(typeof(CareflixReadingType)).OfType<CareflixReadingType>())
            {
                var key = readingType.ToString().ToLower();
                if (vitals[key] != null)
                {
                    var readingValue = vitals.Value<double>(key);
                    readings.Add(new PatientReadingModel
                    {
                        PatientReadingID = GenericMethods.GenerateGuid(),
                        UserID = readingData.SelectedUserID,
                        ReadingID = (short)readingType,
                        ReadingValue = readingValue,
                        ReadingDateTime = (DateTimeOffset?)data["entry_time"],
                        ReadingSourceType = ReadingSource.Manual.ToString(),
                        AddedON = DateTime.Now,
                        LastModifiedON = DateTime.Now,
                        AddedByID = readingData.SelectedUserID.ToString(),
                        IsSynced = false,
                        IsActive = true
                    }); ;
                }
            }
        }

        var metadata = data["metadata"];
        if (metadata != null)
        {
            var physiologicalScores = metadata["physiological_scores"];
            if (physiologicalScores != null)
            {
                foreach (CareflixReadingType readingType in Enum.GetValues(typeof(CareflixReadingType)))
                {
                    if (Enum.TryParse(readingType.ToString(), true, out CareflixReadingType enumValue))
                    {
                        if (physiologicalScores[enumValue.ToString()] != null)
                        {
                            var readingValue = double.TryParse(physiologicalScores[enumValue.ToString()].ToString(), out double result) ? result : (double?)null;
                            readings.Add(new PatientReadingModel
                            {
                                PatientReadingID = GenericMethods.GenerateGuid(),
                                UserID = readingData.SelectedUserID,
                                ReadingID = (short)enumValue,
                                ReadingValue = readingValue,
                                ReadingDateTime = DateTime.Now,
                                ReadingSourceType = ReadingSource.Manual.ToString(),
                                IsActive = true,
                                AddedON = DateTime.Now,
                                LastModifiedON = DateTime.Now,
                                AddedByID = readingData.SelectedUserID.ToString(),
                                IsSynced = false,
                            });
                        }
                    }
                }
            }
        }

        return readings;
    }

    public void MapReadingParentOptions(PatientReadingDTO readingData, bool isAdd)
    {
        var metadatas = readingData.ChartMetaData.Where(x => x.ReadingCategoryID == readingData.ReadingCategoryID && (x.ReadingParentID > 0 || x.ReadingParentID == 0)).ToList();
        if (readingData.ReadingID < 1 || !metadatas.Any(x => x.ReadingID == readingData.ReadingID || x.ReadingParentID == readingData.ReadingID))
        {
            var data = metadatas.FirstOrDefault();
            readingData.ReadingID = data.ReadingParentID > 0 ? data.ReadingParentID : data.ReadingID;
        }
        readingData.ReadingParentOptions = (from metaData in metadatas
                                            where !isAdd || metaData.IsActive
                                            let parentID = metaData.ReadingParentID > 0 ? metaData.ReadingParentID : metaData.ReadingID
                                            orderby metaData.SequenceNo ascending
                                            select new OptionModel
                                            {
                                                ParentOptionID = metaData.ReadingCategoryID,
                                                OptionID = parentID,
                                                OptionText = LibResources.GetResourceValueByKeyID(PageData?.Resources, parentID),
                                                IsSelected = (isAdd && readingData.ReadingID > 0)
                                                    ? readingData.ReadingID == metaData.ReadingID || readingData.ReadingID == metaData.ReadingParentID
                                                    : (isAdd && readingData.SelectedListItemReadingID > 0)
                                                    ? readingData.SelectedListItemReadingID == metaData.ReadingID || readingData.SelectedListItemReadingID == metaData.ReadingParentID
                                                    : GenericMethods.IsListNotEmpty(readingData.ListData) && readingData.ListData.Any(r => r.ReadingID == metaData.ReadingID)
                                            })
                                            .GroupBy(x => x.OptionID)
                                            .Select(g => g.FirstOrDefault(x => x.IsSelected) ?? g.FirstOrDefault()).ToList();
    }

    public void MapChildReadingOptions(PatientReadingDTO readingData, List<ReadingMetadataUIModel> childReadings)
    {
        if (GenericMethods.IsListNotEmpty(childReadings))
        {
            readingData.ReadingOptions = (from child in childReadings
                                          let field = readingData.ListData?.FirstOrDefault(x => x.ReadingID == child.ReadingID)
                                          let val = field?.ReadingValue2 ?? field?.ReadingValue.ToString()
                                          select new OptionModel
                                          {
                                              OptionID = child.ReadingID,
                                              ParentOptionID = child.ReadingValueTypeID,
                                              OptionText = val,
                                              SequenceNo = child.DigitsAfterDecimalPoint
                                          }).ToList();
        }
    }

    private string GetReadingPerformer(PatientReadingUIModel vitalModel)
    {
        var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
        if (vitalModel.AddedByID == LoggedInAccountID
            && ((roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
                ? vitalModel.ReadingSourceType == ReadingSource.Manual.ToString()
                : vitalModel.ReadingSourceType == ReadingSource.ProviderManual.ToString()))
        {
            return LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_SELF_PERFORMER_KEY);
        }
        else
        {
            return vitalModel.ReadingSourceType.ToEnum<ReadingSource>() switch
            {
                ReadingSource.ProviderManual => LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_PROVIDER_KEY),
                ReadingSource.Device => LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DEVICE_TEXT_KEY),
                ReadingSource.HealthKit => LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_IHEALTH_TEXT_KEY),
                ReadingSource.GoogleFit => LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_GOOGLE_FIT_TEXT_KEY),
                _ => LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_PATIENT_KEY),
            };
        }
    }

    private async Task GetReadingPageDataAsync(PatientReadingDTO readingsData)
    {
        await Task.WhenAll(
            GetResourcesAsync(GroupConstants.RS_READINGS_GROUP, GroupConstants.RS_READING_CATEGORY_GROUP, GroupConstants.RS_READING_FILTERS_GROUP,
                GroupConstants.RS_NUMERIC_GROUP, GroupConstants.RS_COUNTER_GROUP, GroupConstants.RS_DAILY_COUNTER_GROUP,
                GroupConstants.RS_HIGH_RISK_LOW_RISK_GROUP, GroupConstants.RS_POSITIVE_NEGATIVE_GROUP, GroupConstants.RS_PRESENT_ABSENT_GROUP,
                GroupConstants.RS_BLOOD_TYPE_GROUP, GroupConstants.RS_GENDER_TYPE_GROUP, GroupConstants.RS_AGE_TYPE_GROUP,
                GroupConstants.RS_ORGANISATION_READINGS_PAGE_GROUP, GroupConstants.RS_USER_ACCOUNT_SETTINGS_GROUP,
                GroupConstants.RS_SUPPORTED_DEVICE_GROUP, GroupConstants.RS_DEVICE_GROUP, GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_TEXT_READING_GROUP, GroupConstants.RS_NORMAL_ABNORMAL_GROUP, GroupConstants.RS_REACTIVE_NONREACTIVE_VALUE_TYPE_GROUP),
            GetSettingsAsync(GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_READING_RELATION_GROUP),
            GetFeaturesAsync(AppPermissions.PatientReadingsView.ToString(), AppPermissions.PatientReadingDetailsView.ToString(),
                AppPermissions.PatientReadingTargetAddEdit.ToString(), AppPermissions.PatientReadingAddEdit.ToString(), AppPermissions.PatientReadingDelete.ToString())
        ).ConfigureAwait(false);
        readingsData.Resources = PageData.Resources;
        readingsData.Settings = PageData.Settings;
        readingsData.FeaturePermissions = PageData.FeaturePermissions;
    }

    private void MapReadingsResourcesAndSettings(PatientReadingDTO readingData, JToken data)
    {
        //var orgnaisationSettings = readingData.Settings;
        readingData.Response = null;
        MapCommonData(readingData, data);
        //if (orgnaisationSettings != null)
        //{
        //	readingData.Settings.AddRange(orgnaisationSettings);
        //}
        SetResourcesAndSettings(readingData);
    }

    private List<OptionModel> GetDurationData(string duration, short defaultDuration)
    {
        if (!string.IsNullOrWhiteSpace(duration))
        {
            var durationArray = duration.Trim().Split('|').Select(x => Convert.ToInt16(x, CultureInfo.InvariantCulture)).ToArray();
            if (defaultDuration < 1)
            {
                defaultDuration = durationArray[0];
            }
            return (from id in durationArray
                    let resource = LibResources.GetResourceByKeyID(PageData?.Resources, id)
                    select new OptionModel
                    {
                        OptionID = resource.ResourceKeyID,
                        OptionText = resource.ResourceValue,
                        ParentOptionID = Convert.ToInt64(resource.KeyDescription, CultureInfo.InvariantCulture),
                        IsSelected = id == defaultDuration,
                        IsDefault = id == defaultDuration
                    }).ToList();
        }
        return new List<OptionModel>();
    }

    private void MapPatientReadingDetailsListUIData(PatientReadingDTO readingsData, string dayFormat, string monthFormat, string yearFormat, List<ReadingMetadataUIModel> metadata)
    {
        if (GenericMethods.IsListNotEmpty(readingsData.ListData))
        {
            if (metadata[0].ReadingParentID == ResourceConstants.R_BLOOD_PRESSURE_KEY_ID)
            {
                readingsData.ListData = CalculateBloodPressureReadings(readingsData.ListData);
            }
            readingsData.ListData.ForEach(current =>
            {
                var meta = metadata.FirstOrDefault(x => x.ReadingID == current.ReadingID);
                current.ReadingMomentIcon = (meta.ReadingParentID == ResourceConstants.R_BLOOD_GLUCOSE_KEY_ID)
                    ? "Glucose_" + current.ReadingID + Constants.SVG_EXTENSION
                    : string.Empty;
                current.ReadingNotesIcon = string.IsNullOrWhiteSpace(current.ReadingNotes)
                    ? string.Empty
                    : nameof(PatientReadingUIModel.ReadingNotes) + Constants.SVG_EXTENSION;
                current.ReadingSourceIcon = string.IsNullOrWhiteSpace(current.ReadingSourceType) || current.ReadingSourceType == ReadingSource.Manual.ToString()
                    ? string.Empty
                    : nameof(PatientReadingUIModel.ReadingSourceType) + current.ReadingSourceType + Constants.SVG_EXTENSION;
                current.ReadingDateTimeText = GenericMethods.GetLocalDateTimeBasedOnCulture(current.ReadingDateTime.Value,
                    GetDateTimeTypeBasedOnReadingFrequency(meta.ReadingFrequency),
                    dayFormat, monthFormat, yearFormat);
                if (string.IsNullOrWhiteSpace(GetSecuredValueAsync(nameof(TempSessionModel.TokenIdentifier)).Result))
                {
                    current.ReadingValueText = $"{(string.IsNullOrWhiteSpace(current.ReadingValueText)
                    ? FetchValueBasedOnValueType(meta, current.ReadingValue, current.ReadingValue2)
                    : current.ReadingValueText)} {meta.Unit}";
                }
                else
                {
                    current.ReadingValueText = $"<br><b style='color:black; font-size:24px;margin-right:5px;'>{(string.IsNullOrWhiteSpace(current.ReadingValueText)
                                               ? FetchValueBasedOnValueType(meta, current.ReadingValue, current.ReadingValue2)
                                               : current.ReadingValueText)}</b>  <sub>{meta.Unit}</sub>";
                }
                current.Unit = meta.Unit;
                current.ImageSource = $"</img src =../images/{current.ReadingSourceIcon}></img></img src =../images/{current.ReadingMomentIcon}></img></img src =../images/{current.ReadingNotesIcon}></img>";
                if (HasGroup(meta))
                {
                    current.Reading = meta.Reading;
                }
                if (MobileConstants.IsMobilePlatform)
                {
                    //todo:
                    //Span valueSpan = new Span { Style = (Style)Application.Current.Resources[LibStyleConstants.ST_SPAN_PRIMARY_TEXT_COLOR_LARGE_STYLE], Text = current.ReadingValueText };
                    //Span spaceSpan = new Span { Text = LibConstants.STRING_SPACE };
                    //Span unitSpan = new Span { Style = (Style)Application.Current.Resources[LibStyleConstants.ST_SPAN_PRIMARY_TEXT_COLOR_SMALL_STYLE], Text = current.Unit };
                    //current.UnitText = new FormattedString
                    //{
                    //	Spans = { valueSpan, spaceSpan, unitSpan }
                    //};
                }
                else if (!string.IsNullOrWhiteSpace(current.Reading))
                {
                    current.ReadingValueText = $"<span class='text-right lbl-primary-text-body-large-regular'>{current.Reading} : </span>{current.ReadingValueText}";
                }
            });
        }
    }

    /// <summary>
    /// Gets date time type based on reading frequency
    /// </summary>
    /// <param name="frequency"></param>
    /// <returns></returns>
    public DateTimeType GetDateTimeTypeBasedOnReadingFrequency(short? frequency)
    {
        return (frequency == ResourceConstants.R_DAILY_SUM_KEY_ID || frequency == ResourceConstants.R_DAILY_AVG_KEY_ID)
                ? DateTimeType.Date
                : DateTimeType.DateTime;

    }

    private bool HasGroup(ReadingMetadataUIModel meta)
    {
        return meta.ReadingParentID > 0 && meta.IsGroupValue && meta.ReadingParentID != ResourceConstants.R_BLOOD_PRESSURE_KEY_ID;
    }

    private string FetchValueBasedOnValueType(ReadingMetadataUIModel metadata, double? value, string? value2)
    {
        if(metadata != null)
        {
            var groupDesc = LibResources.GetResourceGroupDescByGroupID(PageData?.Resources, metadata.ReadingValueTypeID);
            return groupDesc switch
            {
                GroupConstants.RS_SINGLE_SELECT_READING_VALUE_TYPE_GROUP or
                GroupConstants.RS_DROPDOWN_READING_VALUE_TYPE_GROUP
                    => value.HasValue ? LibResources.GetResourceValueByKeyID(PageData?.Resources, Convert.ToInt32(value.Value, CultureInfo.InvariantCulture)) : string.Empty,
                GroupConstants.RS_TEXT_READING_VALUE_TYPE_GROUP
                    => value2 ?? string.Empty,
                _ => value.HasValue ? Math.Round(value.Value, metadata.DigitsAfterDecimalPoint).ToString(CultureInfo.CurrentCulture) : string.Empty
            };
        }
        else
        {
            return string.Empty;
        }
    }

    private void AdjustStartEndDates(PatientReadingDTO readingsData)
    {
        if (GenericMethods.IsListNotEmpty(readingsData.ListData))
        {
            var minDate = readingsData.ListData.Min(x => x.ReadingDateTime).Value;
            var maxDate = readingsData.ListData.Max(x => x.ReadingDateTime).Value;

            readingsData.ChartData.StartDate = minDate.DateTime;
            readingsData.ChartData.EndDate = maxDate.DateTime;

            readingsData.ToDate = GenericMethods.GetStartEndOfDay(false, minDate).ToString(CultureInfo.InvariantCulture);
            readingsData.FromDate = GenericMethods.GetStartEndOfDay(true, maxDate).ToString(CultureInfo.InvariantCulture);
        }
    }

    private bool AdjustStartAndEndDateForWeb(PatientReadingDTO readingsData, string action)
    {
        if (readingsData.FromDate == null || readingsData.FromDate == default)
        {
            int defaultFilter = (int)readingsData.FilterOptions.FirstOrDefault(x => x.IsDefault).ParentOptionID;
            SetReadingDateTime(readingsData, defaultFilter, action);
            return false;
        }
        return true;
    }

    public void SetReadingDateTime(PatientReadingDTO readingsData, long defaultFilter, string action)
    {
        if (readingsData.ChartData.EndDate != null && readingsData.ChartData.EndDate != DateTime.Now && action == null)
        {
            (DateTimeOffset, DateTimeOffset) dateRange = GenericMethods.CalculateFromToDateRanges(DateTime.Now, defaultFilter, action);
            readingsData.ChartData.StartDate = _essentials.ConvertToLocalTime(dateRange.Item1).DateTime;
            readingsData.ChartData.EndDate = _essentials.ConvertToLocalTime(dateRange.Item2).DateTime;
            readingsData.FromDate = GenericMethods.ConvertDatetimeOffsetToIsoDateTimeString(dateRange.Item1);
            readingsData.ToDate = GenericMethods.ConvertDatetimeOffsetToIsoDateTimeString(dateRange.Item2);
        }
        else if(readingsData.ChartData.EndDate != null && readingsData.ChartData.EndDate != DateTime.Now && action != null)
        {
            (DateTimeOffset, DateTimeOffset) dateRange = GenericMethods.CalculateFromToDateRanges((DateTimeOffset)readingsData.ChartData.StartDate, defaultFilter, action);
            readingsData.ChartData.StartDate = _essentials.ConvertToLocalTime(dateRange.Item1).DateTime;
            readingsData.ChartData.EndDate = _essentials.ConvertToLocalTime(dateRange.Item2).DateTime;
            readingsData.FromDate = GenericMethods.ConvertDatetimeOffsetToIsoDateTimeString(dateRange.Item1);
            readingsData.ToDate = GenericMethods.ConvertDatetimeOffsetToIsoDateTimeString(dateRange.Item2);
        }
        else if (readingsData.ChartData.EndDate == null)
        {
            (DateTimeOffset, DateTimeOffset) dateRange = GenericMethods.CalculateFromToDateRanges(DateTime.Now, defaultFilter, action);
            readingsData.ChartData.StartDate = _essentials.ConvertToLocalTime(dateRange.Item1).DateTime;
            readingsData.ChartData.EndDate = _essentials.ConvertToLocalTime(dateRange.Item2).DateTime;
            readingsData.FromDate = GenericMethods.ConvertDatetimeOffsetToIsoDateTimeString(dateRange.Item1);
            readingsData.ToDate = GenericMethods.ConvertDatetimeOffsetToIsoDateTimeString(dateRange.Item2);
        }
    }

    public ChartUIDTO MapChartData(PatientReadingDTO readingsData)
    {
        readingsData.ChartData.Lines = new List<ChartLineModel>();
        readingsData.ChartData.Bands = new List<ChartBandModel>();
        var chartMetaData = readingsData.ChartMetaData.FirstOrDefault(x => x.ReadingID == readingsData.ListData[0].ReadingID);
        readingsData.ChartData.ShowGraph = chartMetaData?.ShowInGraph ?? false;

        if (readingsData.ChartData.ShowGraph)
        {
            List<DataPointModel> dataPoints = new List<DataPointModel>();
            if (chartMetaData.ShowInDifferentLines)
            {
                // Dictionary to track ChartLineModel objects created for each ReadingId
                Dictionary<int, ChartLineModel> chartLineModels = new Dictionary<int, ChartLineModel>();

                foreach (var item in readingsData.ListData)
                {
                    // Check if a ChartLineModel already exists for the ReadingId
                    if (!chartLineModels.ContainsKey(item.ReadingID))
                    {
                        ChartLineModel chartLine = new ChartLineModel
                        {
                            LineName = chartMetaData.Reading,
                            LineColor = chartMetaData.PlotColor,
                            ChartData = new List<DataPointModel>()
                        };

                        // Add the new ChartLineModel to the dictionary
                        chartLineModels.Add(item.ReadingID, chartLine);

                        // Add the ChartLineModel to readingsData.ChartData.Lines
                        readingsData.ChartData.Lines.Add(chartLine);
                    }

                    // Add a new DataPointModel to the existing ChartLineModel
                    chartLineModels[item.ReadingID].ChartData.Add(new DataPointModel
                    {
                        Value = (double)(item.ReadingValue != null ? item.ReadingValue : 0),
                        DateTime = item.ReadingDateTime.Value.DateTime,
                    });
                }
            }
            else
            {
                foreach (var item in readingsData.ListData)
                {
                    dataPoints.Add(new DataPointModel
                    {
                        Value = (double)(item.ReadingValue != null ? item.ReadingValue : 0),
                        DateTime = item.ReadingDateTime.Value.DateTime
                    });
                }
                readingsData.ChartData.Lines.Add(new ChartLineModel
                {
                    LineName = chartMetaData.Reading,
                    LineColor = chartMetaData.PlotColor,
                    ChartData = dataPoints
                });
            }
            readingsData.ChartData.Bands = new List<ChartBandModel>
            {
                 new ChartBandModel
                 {
                     MaxValue = Convert.ToDouble(chartMetaData.TargetMaxValue),
                     MinValue = Convert.ToDouble(chartMetaData.TargetMinValue),
                     Color = chartMetaData.TargetBandColor
                 },
                 new ChartBandModel
                 {
                     MaxValue = Convert.ToDouble(chartMetaData.NormalMaxValue),
                     MinValue = Convert.ToDouble(chartMetaData.NormalMinValue),
                     Color = chartMetaData.NormalBandColor
                 },
             };
        }
        return readingsData.ChartData;

    }

    private List<PatientReadingUIModel> CalculateBloodPressureReadings(List<PatientReadingUIModel> readings)
    {
        List<PatientReadingUIModel> formattedObservationList = new List<PatientReadingUIModel>();
        PatientReadingUIModel currentObservation;
        double currentValue;
        var systolicList = readings.Where(x => x.ReadingID == ResourceConstants.R_BP_SYSTOLIC_KEY_ID).ToList();
        var diastolicList = readings.Where(x => x.ReadingID == ResourceConstants.R_BP_DIASTOLIC_KEY_ID).ToList();
        PatientReadingUIModel currentDiastolicObservation;
        for (int i = 0; i < systolicList.Count; i++)
        {
            currentObservation = systolicList[i];
            currentValue = currentObservation.ReadingValue.Value;
            currentDiastolicObservation = GenericMethods.IsListNotEmpty(diastolicList) && diastolicList.Count >= i + 1 ? diastolicList[i] : null;
            if (currentDiastolicObservation != null && currentObservation.ReadingDateTime != currentDiastolicObservation.ReadingDateTime)
            {
                currentDiastolicObservation = diastolicList.FirstOrDefault(d => d.ReadingDateTime == systolicList[i].ReadingDateTime);
            }
            if (currentDiastolicObservation != null)
            {
                currentObservation.ReadingValueText = GetBloodPressureValue(
                    GenericMethods.ConvertToLocalNumber(currentValue, CultureInfo.CurrentCulture),
                    GenericMethods.ConvertToLocalNumber(currentDiastolicObservation.ReadingValue.Value, CultureInfo.CurrentCulture));
                formattedObservationList.Add(currentObservation);
            }
        }
        return formattedObservationList;
    }

    private string GetBloodPressureValue(string systolic, string diastolic)
    {
        return $"{systolic}{Constants.SYMBOL_SLASH}{diastolic}";
    }

    private void CreatePatientReadingUIModel(PatientReadingDTO readingData, ReadingMetadataUIModel readingType)
    {
        if (!readingData.ListData.Any(x => x.ReadingID == readingType.ReadingID))
        {
            readingData.ListData.Add(new PatientReadingUIModel
            {
                ReadingID = readingType.ReadingID,
            });
        }
    }

    private void MapReadingsListUIData(PatientReadingDTO readingsData)
    {
        if (GenericMethods.IsListNotEmpty(readingsData.ChartMetaData))
        {
            MapReadingsPerGroup(readingsData);
            GenerateLatestReadingData(readingsData);
        }
    }

    private void GenerateLatestReadingData(PatientReadingDTO readingsData)
    {
        if (GenericMethods.IsListNotEmpty(readingsData.ReadingDTOs))
        {
            LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
            foreach (var readings in readingsData.ReadingDTOs)
            {
                //var latestReading = readings.ChartMetaData?.FirstOrDefault();
                var latestReading = readings.ChartMetaData?.OrderByDescending(x => x.ReadingDateTime)?.ThenBy(x => x.SequenceNo)?.FirstOrDefault(x => x.ReadingValue != null || x.ReadingValue2 != null)
                    ?? readings.ChartMetaData?.OrderByDescending(x => x.ReadingDateTime)?.ThenBy(x => x.SequenceNo)?.FirstOrDefault();

                if (readings.ReadingCategoryID == Constants.LAB_VALUE_CATEGORY_ID)
                {
                    readings.ReadingIcon = ImageConstants.LAB_VALUES_SVG;
                }
                else
                {
                    readings.ReadingIcon = string.Concat(PageData.Resources.FirstOrDefault(x => x.ResourceKeyID == readings.ReadingID).ResourceKey, ".svg");
                }
                if ((latestReading.ReadingValue.HasValue || !string.IsNullOrWhiteSpace(latestReading.ReadingValue2)) && latestReading.ReadingDateTime!= GenericMethods.GetDefaultDateTime)
                {
                    readings.TitleWithIcon = $"<img src='../images/{readings.ReadingIcon}' alt='icon' /> <strong>{readings.Title}</strong>";
                    readings.LatestValue = SetLatestMetadataValue(readings.ReadingID, readings.ChartMetaData, latestReading);
                    readings.ReadingUnit = latestReading?.Unit;
                    if (readingsData.RecordCount > 0)
                    {
                        readings.LatestValueString = $"<br><b style='color:black; font-size:44px;margin-right:5px;'>{readings.LatestValue}</b>  <sub>{readings.ReadingUnit}</sub>";
                    }
                    else
                    {
                        readings.LatestValueString = $"<br><b style='color:black; font-size:24px;margin-right:5px;'>{readings.LatestValue}</b>  <sub>{readings.ReadingUnit}</sub>"; 
                    }               
                    readings.MinMaxReadingRanges = $"Range {latestReading?.AbsoluteMinValue} - {latestReading?.AbsoluteMaxValue}";
                    readings.ValueUnit = $"<br><b style=color:black;font-size:24px;>{readingsData.LatestValue}</b>   {readingsData.ReadingUnit}";
                    readings.LatestValueDateText = GenericMethods.GetLocalDateTimeBasedOnCulture(latestReading.ReadingDateTime.Value, DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                    if (MobileConstants.IsMobilePlatform)
                    {
                        //todo:
                        //Span valueSpan = new Span { Style = (Style)Application.Current.Resources[LibStyleConstants.ST_SPAN_PRIMARY_TEXT_COLOR_LARGE_STYLE], Text = readings.LatestValue };
                        //Span spaceSpan = new Span { Text = LibConstants.STRING_SPACE };
                        //Span unitSpan = new Span { Style = (Style)Application.Current.Resources[LibStyleConstants.ST_SPAN_PRIMARY_TEXT_COLOR_SMALL_STYLE], Text = readings.ReadingUnit };
                        //readings.ReadingUnitValue = new FormattedString
                        //{
                        //	Spans = { valueSpan, spaceSpan, unitSpan }
                        //};
                    }
                }
                else
                {
                    readings.TitleWithIcon = $"<img src=../images/{readings.ReadingIcon}></img> <strong>{readings.Title}</strong>";

                    readings.LatestValue = Constants.SYMBOL_DOUBLE_HYPHEN;
                    readings.LatestValueDateText = Constants.SYMBOL_DOUBLE_HYPHEN;
                    readings.LatestValueString = $"{readings.LatestValue} {readings.ReadingUnit}";

                    if (MobileConstants.IsMobilePlatform)
                    {
                        //todo:
                        //readings.ReadingUnitValue = new FormattedString
                        //{
                        //	Spans = { new Span { Style = (Style)Application.Current.Resources[LibStyleConstants.ST_SPAN_PRIMARY_TEXT_COLOR_LARGE_STYLE], Text = readings.LatestValue } }
                        //};
                    }
                }
            }
        }
    }
}