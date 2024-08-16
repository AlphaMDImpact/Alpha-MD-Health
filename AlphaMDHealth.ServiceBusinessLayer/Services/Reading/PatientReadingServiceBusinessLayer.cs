using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceConfiguration;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class PatientReadingServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Reading service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public PatientReadingServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get patient reading ranges and Reading Detail
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="organisationID">Id of organization</param>
        /// <param name="selectedUserID">User id for which data needs to fetch</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <param name="readingCategoryID">Category for which data needs to fetch</param>
        /// <param name="patientReadingID">Id of patient reading for which data needs to fetch</param>
        /// <param name="readingID">Reading id for which data needs to fetch</param>
        /// <param name="fromDate">Start date from where data needs to fetch</param>
        /// <param name="toDate">End date, till data needs to fetched</param>
        /// <param name="isMedicalHistory">flag representing data is requested for medical history or not</param>
        /// <param name="isCommingFromQuestionnaireTaskPage">flag for questionnaire</param>
        /// <returns>Patient reading detail and reading ranges data</returns>
        public async Task<PatientReadingDTO> GetPatientReadingsAsync(byte languageID, long permissionAtLevelID, long organisationID, long selectedUserID, long recordCount, short readingCategoryID, Guid patientReadingID, short readingID, string fromDate, string toDate, bool isMedicalHistory, bool isCommingFromQuestionnaireTaskPage)
        {
            PatientReadingDTO readingData = new PatientReadingDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || selectedUserID < 1)
                {
                    readingData.ErrCode = ErrorCode.InvalidData;
                    return readingData;
                }
                if (AccountID < 1)
                {
                    readingData.ErrCode = ErrorCode.Unauthorized;
                    return readingData;
                }
                if (await GetReadingsResourceAndSettingsAsync(readingData, organisationID, languageID).ConfigureAwait(false))
                {
                    readingData.PermissionAtLevelID = permissionAtLevelID;
                    readingData.RecordCount = recordCount;
                    readingData.SelectedUserID = selectedUserID;
                    readingData.ReadingCategoryID = readingCategoryID;
                    readingData.PatientReadingID = patientReadingID;
                    readingData.ReadingID = readingID;
                    readingData.FromDate = fromDate;
                    readingData.ToDate = toDate;
                    readingData.IsMedicalHistory = isMedicalHistory;
                    readingData.FeatureFor = FeatureFor;
                    readingData.IsCommingFromQuestionnaireTaskPage = isCommingFromQuestionnaireTaskPage;
                    await new PatientReadingServiceDataLayer().GetPatientReadingsAsync(readingData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                readingData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return readingData;
        }

        /// <summary>
        /// Get patient scan vital data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="selectedUserID">User id for which data needs to fetch</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>Patient reading scan vital data</returns>
        public async Task<PatientScanVitalDTO> GetPatientScanVitalsDataAsync(byte languageID, long permissionAtLevelID, long organisationID, long selectedUserID, long recordCount)
        {
            PatientScanVitalDTO vitalsData = new PatientScanVitalDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || selectedUserID < 1)
                {
                    vitalsData.ErrCode = ErrorCode.InvalidData;
                    return vitalsData;
                }
                if (AccountID < 1)
                {
                    vitalsData.ErrCode = ErrorCode.Unauthorized;
                    return vitalsData;
                }
                if (await GetReadingVitalsResourcesAsync(vitalsData, organisationID, languageID).ConfigureAwait(false))
                {
                    vitalsData.FeatureFor = FeatureFor;
                    vitalsData.PermissionAtLevelID = permissionAtLevelID;
                    vitalsData.RecordCount = recordCount;
                    vitalsData.SelectedUserID = selectedUserID;
                    await new PatientReadingServiceDataLayer().GetPatientScanVitalsDataAsync(vitalsData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                vitalsData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return vitalsData;
        }
        internal async Task<bool> GetReadingVitalsResourcesAsync(PatientScanVitalDTO vitalsData, long organisationID, byte languageID)
        {
            vitalsData.AccountID = AccountID;
            vitalsData.OrganisationID = organisationID;
            vitalsData.LanguageID = languageID;
            if (await GetSettingsResourcesAsync(vitalsData, true, GroupConstants.RS_COMMON_GROUP, GetReadingResourceGroups()).ConfigureAwait(false))
            {
                vitalsData.ErrCode = ErrorCode.OK;
                return true;
            }
            return false;
        }

        internal async Task<bool> GetReadingsResourceAndSettingsAsync(PatientReadingDTO readingData, long organisationID, byte languageID)
        {
            readingData.AccountID = AccountID;
            readingData.OrganisationID = organisationID;
            readingData.LanguageID = languageID;
            if (await GetSettingsResourcesAsync(readingData, true, GroupConstants.RS_COMMON_GROUP, GetReadingResourceGroups()).ConfigureAwait(false))
            {
                readingData.ErrCode = ErrorCode.OK;
                return true;
            }
            return false;
        }

        private string GetReadingResourceGroups()
        {
            var myConfig = MyConfiguration.GetInstance;
            return $"{GroupConstants.RS_READINGS_GROUP},{GroupConstants.RS_READING_CATEGORY_GROUP},{GroupConstants.RS_GENDER_TYPE_GROUP},{GroupConstants.RS_READING_FILTERS_GROUP}," +
                $"{GroupConstants.RS_COUNTER_GROUP}," +
                $"{GroupConstants.RS_GENDER_TYPE_GROUP},{GroupConstants.RS_AGE_TYPE_GROUP}," +
                $"{GroupConstants.RS_ORGANISATION_READINGS_PAGE_GROUP},{GroupConstants.RS_USER_ACCOUNT_SETTINGS_GROUP}," +
                $"{GroupConstants.RS_SUPPORTED_DEVICE_GROUP},{GroupConstants.RS_DEVICE_GROUP},{GroupConstants.RS_COMMON_GROUP}," +
                $"{GroupConstants.RS_YES_NO_TYPE_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP},{GroupConstants.RS_CARE_FLIX_SERVICE_GROUP},{GroupConstants.RS_POSTURE_GROUP},{GroupConstants.RS_SCAN_TYPE_GROUP}," + 
                $"{string.Format(myConfig.GetConfigurationValue(ConfigurationConstants.CS_READINGS_READING_TYPE_GROUPS))}";
        }

        /// <summary>
        /// Save Patient readings data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="readingsData">Patient Readings data to be saved</param>
        /// <returns>>Result of operation</returns>
        public async Task<BaseDTO> SavePatientReadingsAsync(byte languageID, long permissionAtLevelID, PatientReadingDTO readingsData)
        {
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1 || readingsData == null || readingsData.PatientReadings == null || IsPatientReadingsNotValid(readingsData))
                {
                    readingsData = readingsData ?? new PatientReadingDTO();
                    readingsData.ErrCode = ErrorCode.InvalidData;
                    return readingsData;
                }
                if (AccountID < 1)
                {
                    readingsData.ErrCode = ErrorCode.Unauthorized;
                    return readingsData;
                }
                if (readingsData.IsActive)
                {
                    readingsData.LanguageID = languageID;
                    if (await GetSettingsResourcesAsync(readingsData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GetReadingResourceGroups()}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(readingsData.PatientReadings, readingsData.Resources))
                        {
                            readingsData.ErrCode = ErrorCode.InvalidData;
                            return readingsData;
                        }
                    }
                    else
                    {
                        return readingsData;
                    }
                }
                readingsData.AccountID = AccountID;
                readingsData.PermissionAtLevelID = permissionAtLevelID;
                readingsData.FeatureFor = FeatureFor;
                await new PatientReadingServiceDataLayer().SavePatientReadingsAsync(readingsData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                readingsData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return readingsData;
        }

        /// <summary>
        /// Save user reading targets data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="readingsData">Readings targets data to be saved</param>
        /// <returns>Result of operation</returns>
        public async Task<BaseDTO> SavePatientReadingTargetsAsync(byte languageID, long permissionAtLevelID, PatientReadingDTO readingsData)
        {
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1 || readingsData == null
                    || readingsData.PatientReadingTargets == null || !GenericMethods.IsListNotEmpty(readingsData.PatientReadingTargets))
                {
                    readingsData.ErrCode = ErrorCode.InvalidData;
                    return readingsData;
                }
                if (AccountID < 1)
                {
                    readingsData.ErrCode = ErrorCode.Unauthorized;
                    return readingsData;
                }
                readingsData.AccountID = AccountID;
                readingsData.PermissionAtLevelID = permissionAtLevelID;
                readingsData.FeatureFor = FeatureFor;
                await new PatientReadingServiceDataLayer().SavePatientReadingTargetsAsync(readingsData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                readingsData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return readingsData;
        }

        /// <summary>
        /// Search food item
        /// </summary>
        /// <param name="languageID">Language id</param>
        /// <param name="search">Search food item</param>
        /// <param name="organisationID">Organisation id</param>
        /// <returns>List of food items</returns>
        public async Task<PatientReadingDTO> SearchFoodItemAsync(byte languageID, string search, long organisationID)
        {
            PatientReadingDTO foodData = new PatientReadingDTO()
            {
                OrganisationID = organisationID
            };
            try
            {
                if (languageID < 1 || string.IsNullOrWhiteSpace(search))
                {
                    foodData.ErrCode = ErrorCode.InvalidData;
                    return foodData;
                }
                if (AccountID < 1)
                {
                    foodData.ErrCode = ErrorCode.Unauthorized;
                    return foodData;
                }
                foodData.Settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP,
                    languageID, default, 0, foodData.OrganisationID, false).ConfigureAwait(false)).Settings;
                if (GenericMethods.IsListNotEmpty(foodData.Settings))
                {
                    LogError($"SETTING COUNT : {foodData.Settings.Count} {string.Join(',', foodData.Settings.Select(x => x.SettingKey).ToArray())}", new Exception());
                    string clientIdentifier = GetSettingValueByKey(foodData.Settings, SettingsConstants.S_FOOD_LIBRARY_MICRO_SERVICE_KEY);
                    HttpServiceModel<FoodItemDTO> httpData = new HttpServiceModel<FoodItemDTO>
                    {
                        CancellationToken = new CancellationToken(),
                        BaseUrl = new Uri(UrlConstants.MICRO_SERVICE_PATH),
                        PathWithoutBasePath = UrlConstants.FOOD_LIBRARY_SEARCH_FOOD_ITEM_ASYNC,
                        AuthType = AuthorizationType.Basic,
                        ClientIdentifier = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[1],
                        ClientSecret = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[2],
                        ForApplication = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[0],
                        QueryParameters = new System.Collections.Specialized.NameValueCollection
                        {
                            { nameof(search), search },
                            { Constants.SE_IS_BARCODE_QUERY_KEY, false.ToString(CultureInfo.InvariantCulture) },
                            { Constants.SE_IMAGE_REQUIRED_QUERY_KEY, false.ToString(CultureInfo.InvariantCulture) },
                            { Constants.FOR_APPLICATION, clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[0] }
                        }
                    };
                    await new HttpLibService(new HttpService()).GetAsync(httpData).ConfigureAwait(false);
                    foodData.ErrCode = httpData.ErrCode;
                    if (httpData.ErrCode == ErrorCode.OK)
                    {
                        FoodItemDTO foodItems = JsonConvert.DeserializeObject<FoodItemDTO>(httpData.Response);
                        if (GenericMethods.IsListNotEmpty(foodItems.FoodItems))
                        {
                            foodData.SummaryDataOptions = (from FoodItemModel food in foodItems.FoodItems
                                                           select new OptionModel
                                                           {
                                                               GroupName = food.FoodIdentifier,
                                                               OptionText = food.FoodTitle,
                                                               ParentOptionText = food.Image
                                                           }).ToList();
                            if (GenericMethods.IsListNotEmpty(foodData.SummaryDataOptions) && foodData.SummaryDataOptions.Count == 1)
                            {
                                await GetFoodDataAsync(foodData.SummaryDataOptions[0].GroupName, foodData).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                foodData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return foodData;
        }

        /// <summary>
        /// Get food nutritions data
        /// </summary>
        /// <param name="languageID">Language id</param>
        /// <param name="foodIdentifier">Food identifier</param>
        /// <param name="organisationID">Organisation id</param>
        /// <returns>Food nutrition data</returns>
        public async Task<PatientReadingDTO> GetFoodNutritionDataAsync(byte languageID, string foodIdentifier, long organisationID)
        {
            PatientReadingDTO foodData = new PatientReadingDTO()
            {
                OrganisationID = organisationID
            };
            try
            {
                if (languageID < 1 || string.IsNullOrWhiteSpace(foodIdentifier))
                {
                    foodData.ErrCode = ErrorCode.InvalidData;
                    return foodData;
                }
                if (AccountID < 1)
                {
                    foodData.ErrCode = ErrorCode.Unauthorized;
                    return foodData;
                }
                foodData.Settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP,
                    languageID, default, 0, foodData.OrganisationID, false).ConfigureAwait(false)).Settings;
                await GetFoodDataAsync(foodIdentifier, foodData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                foodData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return foodData;
        }

        private async Task GetFoodDataAsync(string foodIdentifier, PatientReadingDTO foodData)
        {
            if (GenericMethods.IsListNotEmpty(foodData.Settings))
            {
                string clientIdentifier = GetSettingValueByKey(foodData.Settings, SettingsConstants.S_FOOD_LIBRARY_MICRO_SERVICE_KEY);
                HttpServiceModel<FoodItemDTO> httpData = new HttpServiceModel<FoodItemDTO>
                {
                    CancellationToken = new CancellationToken(),
                    BaseUrl = new Uri(UrlConstants.MICRO_SERVICE_PATH),
                    PathWithoutBasePath = UrlConstants.GET_FOOD_DATA_ASYNC,
                    AuthType = AuthorizationType.Basic,
                    ClientIdentifier = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[1],
                    ClientSecret = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[2],
                    ForApplication = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[0],
                    QueryParameters = new System.Collections.Specialized.NameValueCollection
                    {
                        { nameof(foodIdentifier), foodIdentifier },
                        { Constants.FOR_APPLICATION, clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[0] }
                    }
                };
                await new HttpLibService(new HttpService()).GetAsync(httpData).ConfigureAwait(false);
                foodData.ErrCode = httpData.ErrCode;
                if (httpData.ErrCode == ErrorCode.OK)
                {
                    HealthReadingDTO healthReadings = JsonConvert.DeserializeObject<HealthReadingDTO>(httpData.Response);
                    if (GenericMethods.IsListNotEmpty(healthReadings.HealthReadings))
                    {
                        foodData.ListData = (from HealthReadingModel nutritients in healthReadings.HealthReadings
                                             select new PatientReadingUIModel
                                             {
                                                 ReadingID = (short)nutritients.ReadingType,
                                                 Unit = Convert.ToString(nutritients.ReadingUnit, CultureInfo.InvariantCulture),
                                                 ReadingValue = nutritients.ReadingValue,
                                             }).ToList();
                    }
                }
            }
        }

        private bool IsPatientReadingsNotValid(PatientReadingDTO readingsData)
        {
            return readingsData.PatientReadings.Any(x => x.PatientReadingID == Guid.Empty
                || (x.IsActive && (x.UserID < 1 || x.ReadingID < 1 || string.IsNullOrWhiteSpace(x.ReadingSourceType))));
        }
    }
}