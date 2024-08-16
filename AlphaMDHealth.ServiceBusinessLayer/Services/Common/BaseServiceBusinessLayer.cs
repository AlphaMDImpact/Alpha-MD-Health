using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public partial class BaseServiceBusinessLayer
    {
        public long AccountID { get; private set; }
        public byte FeatureFor { get; private set; }
        protected HttpContext _httpContext { get; private set; }

        public static async Task<bool> ValidateDataAsync<T>(IEnumerable<T> source, List<ResourceModel> resources) where T : new()
        {
            foreach (T item in source)
            {
                if (!await ValidateDataAsync(item, resources))
                {
                    return false;
                }
            }
            return true;
        }

        public static async Task<bool> ValidateDataAsync<T>(T item, List<ResourceModel> resources) where T : new()
        {
            var type = item.GetType();
            var properties = item.GetType().GetProperties();
            ResourceModel currentResource = new ResourceModel();
            List<string> multipleAttributes;
            string resourceField = string.Empty;
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(false);
                var columnMapping = attributes.FirstOrDefault(a => a.GetType() == typeof(MyCustomAttributes));
                if (columnMapping != null)
                {
                    var mapsto = columnMapping as MyCustomAttributes;
                    if (mapsto.TheResourceKey.Contains(','))
                    {
                        multipleAttributes = [.. mapsto.TheResourceKey.Split(',')];
                        foreach (string myGroup in multipleAttributes)
                        {
                            if (resources.FirstOrDefault(x => x.ResourceKey == myGroup) != null)
                            {
                                resourceField = myGroup;
                                break;
                            }
                        }
                    }
                    else
                    {
                        resourceField = mapsto.TheResourceKey;

                    }
                    currentResource = resources.FirstOrDefault(res => res.ResourceKey == mapsto.TheResourceKey);
                    if (currentResource != null)
                    {
                        if (!await GetFieldValidatedAsync(currentResource, Convert.ToString(typeof(T).GetProperty(property.Name).GetValue(item))))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        static private async Task<bool> GetFieldValidatedAsync(ResourceModel currentResource, string textValue)
        {
            DataValidators dataValidator = new DataValidators();
            string fieldType = currentResource.FieldType;
            bool isFieldValid = false;
            if (!string.IsNullOrWhiteSpace(fieldType))
            {
                switch (fieldType)
                {
                    case nameof(FieldTypes.MobileNumberControl):  //.ToString():
                        break;
                    case nameof(FieldTypes.RichTextControl):
                    case nameof(FieldTypes.TextEntryControl):
                    case nameof(FieldTypes.AlphaEntryControl):
                    case nameof(FieldTypes.MultiLineEntryControl):
                    case nameof(FieldTypes.PasswordEntryControl):
                    case nameof(FieldTypes.ColorPickerControl):
                        isFieldValid = dataValidator.ValidateTextField(fieldType, currentResource.IsRequired, currentResource.MinLength, currentResource.MaxLength, textValue);
                        break;
                    case nameof(FieldTypes.UploadControl):
                        isFieldValid = dataValidator.ValidateUploadField(fieldType, currentResource.IsRequired, textValue);
                        break;
                    case nameof(FieldTypes.TimeControl):
                        isFieldValid = dataValidator.ValidateTimeField(fieldType, currentResource.IsRequired, textValue);
                        break;

                    case nameof(FieldTypes.DateTimeControl):
                        isFieldValid = dataValidator.ValidateDateTimeField(fieldType, currentResource.IsRequired, textValue);
                        break;
                    case nameof(FieldTypes.VerticalRadioButtonControl):
                    case nameof(FieldTypes.HorizontalRadioButtonControl):
                    case nameof(FieldTypes.SingleSelectDropdownControl):
                    case nameof(FieldTypes.SingleSelectEditableDropdownControl):
                        isFieldValid = dataValidator.ValidateSingleSelectField(fieldType, currentResource.IsRequired, textValue);
                        break;
                    case nameof(FieldTypes.VerticalCheckBoxControl):
                    case nameof(FieldTypes.HorizontalCheckBoxControl):
                    case nameof(FieldTypes.CheckBoxControl):
                    case nameof(FieldTypes.MultiSelectDropdownControl):
                        break;
                    case nameof(FieldTypes.NumericEntryControl):
                    case nameof(FieldTypes.DecimalEntryControl):
                        isFieldValid = dataValidator.ValidateNumericField(fieldType, Convert.ToDouble(textValue), currentResource.IsRequired, currentResource.MinLength, currentResource.MaxLength);
                        break;
                    case nameof(FieldTypes.HorizontalSliderControl):
                    case nameof(FieldTypes.VerticalSliderControl):
                        break;
                    case nameof(FieldTypes.EmailEntryControl):
                        break;
                    case nameof(FieldTypes.DateControl):
                        isFieldValid = dataValidator.ValidateDateField(fieldType, currentResource.IsRequired, textValue);
                        break;
                    default:
                        break;
                }
            }
            return isFieldValid;
        }
        //one private method to know the type of value
        //another private methods to validate. Like one for string fields valdiation. one for numeric etc.

        public BaseServiceBusinessLayer(HttpContext httpContext)
        {
            _httpContext = httpContext;
            AccountID = httpContext?.GetAccountID() ?? 0;
            FeatureFor = httpContext?.GetFeatureFor() ?? 0;
        }

        public FieldValidator GetField(string key, object val, FieldTypes type = default, int precision = 0)
        {
            return new FieldValidator { ResourceKey = key, Value = val, Type = type, DecimalPrecision = precision };
        }

        /// <summary>
        /// Validate fields
        /// </summary>
        /// <param name="dataObj">reference object containing fields to validate</param>
        /// <returns>flag representing fields are valid or not</returns>
        public bool AreFieldsValidAsync(BaseDTO dataObj)
        {
            if (dataObj?.Fields?.Count > 0 && GenericMethods.IsListNotEmpty(dataObj.Resources))
            {
                foreach (var field in dataObj.Fields)
                {
                    if (!field.IsValid(dataObj))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Gets resource value based on parameters
        /// Gets resource value based on parameters
        /// </summary>
        /// <param name="groupKey">resource group key</param>
        /// <param name="resourceKey">resource key</param>
        /// <param name="languageId">language id</param>
        /// <returns>resource value</returns>
        public async Task<string> GetResourceValueByKeyAsync(string groupKey, string resourceKey, byte languageId)
        {
            return (await GetDataFromCacheAsync(CachedDataType.Resources, groupKey, languageId, default, 0, 0, false).ConfigureAwait(false))?.Resources?.
                FirstOrDefault(x => x.ResourceKey?.Trim() == resourceKey?.Trim())?.ResourceValue ?? string.Empty;
        }

        /// <summary>
        /// Logs errors in database
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="organisationID">User's Organisation</param>
        /// <param name="errorLogsData">Ref object which holds data to store on server and returns status</param>
        /// <returns>operation status</returns>
        public async Task<BaseDTO> SaveErrorLogsAsync(byte languageID, long organisationID, ErrorLogDTO errorLogsData)
        {
            try
            {
                if (organisationID < 1 || languageID < 1 || errorLogsData.ErrorLogs.Count < 0)
                {
                    errorLogsData.ErrCode = ErrorCode.InvalidData;
                    return errorLogsData;
                }
                errorLogsData.AccountID = AccountID;
                await new BaseServiceDataLayer().SaveErrorLogsAsync(errorLogsData).ConfigureAwait(false);
                errorLogsData.ErrCode = ErrorCode.OK;
            }
            catch
            {
                errorLogsData.ErrCode = ErrorCode.DatabaseError;
            }
            return errorLogsData;
        }

        /// <summary>
        /// Gets a value of resource from resources list
        /// </summary>
        /// <param name="resources">list of resource values</param>
        /// <param name="resourceKey">resource key</param>
        /// <returns>value of resource</returns>
        protected string GetResourceValueByKey(List<ResourceModel> resources, string resourceKey)
        {
            return resources?.FirstOrDefault(x => x.ResourceKey?.Trim() == resourceKey?.Trim())?.ResourceValue;
        }

        /// <summary>
        /// Gets a value of setting from Settings list
        /// </summary>
        /// <param name="settings">list of setting values</param>
        /// <param name="settingKey">setting key</param>
        /// <returns>a string value of setting</returns>
        protected string GetSettingValueByKey(List<SettingModel> settings, string settingKey)
        {
            return settings?.FirstOrDefault(x => x.SettingKey == settingKey)?.SettingValue;
        }

        /// <summary>
        /// Logs error in database
        /// </summary>
        /// <param name="errorMessage">detial error message</param>
        /// <param name="errorLocation">location of error</param>
        protected void LogError(string errorMessage, Exception errorLocation)
        {
            try
            {
                string projectMode;
#if DEBUG
                projectMode = "Debug: ";
#else
                projectMode = "Release: ";
#endif
                ErrorLogDTO errorLogs = new ErrorLogDTO
                {
                    ErrorLogs = new List<ErrorLogModel>
                    {
                        new ErrorLogModel
                        {
                            CreatedOn = DateTimeOffset.UtcNow,
                            ErrorFunction = errorLocation.StackTrace,
                            ErrorMessage = projectMode + errorMessage,
                            ErrorLineNumber = 1,
                            ErrorLogLevel = 1,
                            AccountID = AccountID
                        }
                    }
                };
                new BaseServiceDataLayer().SaveErrorLogsAsync(errorLogs).ConfigureAwait(false);
            }
            catch
            {
                //To be implemented
            }
        }

        /// <summary>
        /// Map http request headers in session model
        /// </summary>
        /// <param name="headers">http request header data</param>
        /// <param name="sessionData">session model ref variable to store resultant data</param>
        protected void MapHeadersInSession(IHeaderDictionary headers, SessionModel sessionData)
        {
            if (headers != null)
            {
                headers.TryGetValue(Constants.SE_CLIENT_IDENTIFIER_HEADER_KEY, out StringValues headerValues);
                sessionData.ClientIdentifier = headerValues.FirstOrDefault() ?? string.Empty;
                headers.TryGetValue(Constants.SE_DEVICE_TYPE_HEADER_KEY, out headerValues);
                sessionData.DeviceType = headerValues.FirstOrDefault() ?? string.Empty;
                headers.TryGetValue(Constants.SE_DEVICE_PLATFORM_HEADER_KEY, out headerValues);
                sessionData.DevicePlatform = headerValues.FirstOrDefault() ?? string.Empty;
                headers.TryGetValue(Constants.SE_DEVICE_UNIQUE_ID_HEADER_KEY, out headerValues);
                sessionData.DeviceID = headerValues.FirstOrDefault() ?? string.Empty;
                headers.TryGetValue(Constants.SE_DEVICE_INFORMATION_HEADER_KEY, out headerValues);
                MapDeviceInfo(sessionData, headerValues);
                headers.TryGetValue(Constants.SE_AUTHORIZATION_HEADER_KEY, out headerValues);
                sessionData.AccessToken = headerValues.FirstOrDefault()?.Replace(Constants.SE_BEARER_TEXT_KEY, string.Empty, StringComparison.InvariantCulture) ?? string.Empty;
            }
        }

        /// <summary>
        /// Setup SignalR tags based on the user and device platform
        /// </summary>
        /// <param name="requestData">Request data containing account id</param>
        /// <param name="connectionID">SignalR connection ID</param>
        /// <param name="headers">Request headers</param>
        /// <param name="hubContext">SignalR Hub context</param>
        /// <returns>Result of operation</returns>
        protected async Task SetupSignalRTagsAsync(BaseDTO requestData, string connectionID, IHeaderDictionary headers, IHubContext<NotificationHub> hubContext)
        {
            List<string> groups = new List<string> { Constants.ORGANISATION_TAG_PREFIX + requestData.OrganisationID };
            if (requestData.AccountID > 0)
            {
                groups.Add(Constants.ACCOUNT_TAG_PREFIX + requestData.AccountID.ToString(CultureInfo.InvariantCulture));
                if (requestData.SelectedUserID > 0)
                {
                    groups.Add(Constants.USER_TAG_PREFIX + requestData.SelectedUserID.ToString(CultureInfo.InvariantCulture));
                }
            }
            SessionModel sessionData = new SessionModel();
            MapHeadersInSession(headers, sessionData);
            if (!string.IsNullOrWhiteSpace(sessionData.ClientIdentifier))
            {
                SystemIdentifierModel systemIdentifier = ((SystemIdentifierDTO)await GetDataFromCacheAsync(CachedDataType.SystemIdentifiers, sessionData.ClientIdentifier, 0, default, 0, 0, false).ConfigureAwait(false))?.SystemIdentifiers?.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(systemIdentifier?.IdentifierTags))
                {
                    groups.AddRange(systemIdentifier.IdentifierTags.Split(Constants.COMMA_SEPARATOR));
                    if (sessionData.DeviceType == AlphaMDHealth.Utility.ForPlatform.Web.ToString())
                    {
                        groups.Add(sessionData.DeviceDetail);
                    }
                }
            }
            await new NotificationHub().AddToGroupAsync(connectionID, hubContext, groups.ToArray()).ConfigureAwait(false);
        }

        private void MapDeviceInfo(SessionModel sessionData, StringValues headerValues)
        {
            string[] deviceInfoList = headerValues.FirstOrDefault()?.Split(Constants.COMMA_SEPARATOR);
            if (deviceInfoList?.Length > 3)
            {
                sessionData.DeviceDetail = deviceInfoList[0];
                sessionData.DeviceModel = deviceInfoList[1];
                sessionData.DeviceOS = deviceInfoList[2];
                sessionData.DeviceOSVersion = deviceInfoList[3];
            }
        }

        private string GenerateUniqueID(EditorType editorType, byte languageID, long recordID)
        {
            return recordID.ToString(CultureInfo.InvariantCulture) + editorType.ToString() + languageID;
        }

        protected string GenerateTemporaryPassword()
        {
            /*
            * Generate new temporary password only when
            * User is registered for first time
            * User clicks on Resend activation mail
            */
            return GenericMethods.RandomString(10);
        }

        /// <summary>
        /// Get Access token and session id
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        protected async Task GenerateSessionForVideoCallAsync(VideoDTO video)
        {
            try
            {
                video.Settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, 0, default, 0, video.OrganisationID, false).ConfigureAwait(false)).Settings;
                if (GenericMethods.IsListNotEmpty(video.Settings))
                {
                    string clientIdentifier = GetSettingValueByKey(video.Settings, SettingsConstants.S_VIDEO_MICRO_SERVICE_KEY);
                    video.ForApplication = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[0];
                    var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                    {
                        BaseUrl = new Uri(UrlConstants.MICRO_SERVICE_PATH),
                        PathWithoutBasePath = UrlConstants.GENERATE_SESSION_FOR_VIDEO_ASYNC,
                        AuthType = AuthorizationType.Basic,
                        ClientIdentifier = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[1],
                        ClientSecret = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[2],
                        ForApplication = video.ForApplication,
                        QueryParameters = new NameValueCollection
                    {
                        {
                            Constants.FOR_APPLICATION,video.ForApplication

                        },
                         { Constants.ROOM_ID, video.Video.VideoRoomID },
                        { Constants.USER_ID,video.AddedBy  },
                        { Constants.USER_NAME, video.LastModifiedBy },
                    }
                    };
                    await new HttpLibService(new HttpService()).GetAsync(httpData).ConfigureAwait(false);
                    video.ErrCode = httpData.ErrCode;
                    if (video.ErrCode == ErrorCode.OK)
                    {
                        JToken data = JToken.Parse(httpData.Response);
                        if (data != null && data.HasValues)
                        {
                            var videoDetails = data[nameof(VideoDTO.Video)];
                            if (videoDetails.HasValues)
                            {
                                video.Video = MapSession(videoDetails);
                                video.PhoneNumber = GetDataItem<string>(data, nameof(VideoDTO.PhoneNumber));
                            }
                        }

                        else
                        {
                            video.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                video.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// Get Access token and session id
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        protected async Task CreateSessionForVideoCallAsync(VideoDTO video)
        {
            try
            {
                video.Settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, 0, default, 0, video.OrganisationID, false).ConfigureAwait(false)).Settings;
                if (GenericMethods.IsListNotEmpty(video.Settings))
                {
                    string clientIdentifier = GetSettingValueByKey(video.Settings, SettingsConstants.S_VIDEO_MICRO_SERVICE_KEY);
                    video.ForApplication = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[0];
                    var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                    {
                        BaseUrl = new Uri(UrlConstants.MICRO_SERVICE_PATH),
                        PathWithoutBasePath = UrlConstants.CREATE_SESSION_FOR_VIDEO_ASYNC,
                        AuthType = AuthorizationType.Basic,
                        ClientIdentifier = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[1],
                        ClientSecret = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[2],
                        ForApplication = video.ForApplication,
                        QueryParameters = new NameValueCollection { { Constants.FOR_APPLICATION, video.ForApplication } }
                    };
                    await new HttpLibService(new HttpService()).GetAsync(httpData).ConfigureAwait(false);
                    video.ErrCode = httpData.ErrCode;
                    if (video.ErrCode == ErrorCode.OK)
                    {
                        JToken data = JToken.Parse(httpData.Response);
                        if (data != null && data.HasValues)
                        {
                            var videoDetails = data[nameof(VideoDTO.Video)];
                            if (videoDetails.HasValues)
                            {
                                video.Video = MapSession(videoDetails);

                            }
                        }

                        else
                        {
                            video.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                video.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// Map session data
        /// </summary>
        /// <param name="dataItem">program json object</param>
        /// <returns>Program data</returns>
        protected VideoModel MapSession(JToken dataItem)
        {
            return new VideoModel
            {
                VideoRoomID = GetDataItem<string>(dataItem, nameof(VideoModel.VideoRoomID)),
                VideoToken = GetDataItem<string>(dataItem, nameof(VideoModel.VideoToken)),
                VideoLink = GetDataItem<string>(dataItem, nameof(VideoModel.VideoLink)),
                ApplicationID = GetDataItem<string>(dataItem, nameof(VideoModel.ApplicationID)),
                SecretKey = GetDataItem<string>(dataItem, nameof(VideoModel.SecretKey)),
                ServiceType = GetDataItem<ServiceType>(dataItem, nameof(VideoModel.ServiceType)),
            };
        }

        /// <summary>
        /// Check token and Extract Value
        /// </summary>
        /// <typeparam name="T">DataType of Value</typeparam>
        /// <param name="dataItem">Jtoken item</param>
        /// <param name="fieldName">Key of the DataItem</param>
        /// <returns></returns>
        protected T GetDataItem<T>(JToken dataItem, string fieldName)
        {
            return string.IsNullOrWhiteSpace((string)dataItem[fieldName]) ? default : (T)Convert.ChangeType(dataItem[fieldName], typeof(T), CultureInfo.InvariantCulture);
        }
    }
}