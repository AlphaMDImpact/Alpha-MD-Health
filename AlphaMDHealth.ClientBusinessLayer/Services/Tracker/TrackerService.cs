using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class TrackerService : BaseService
    {
        public TrackerService(IEssentials serviceEssentials) : base(serviceEssentials)
        {

        }
        /// <summary>
        /// sync Trackers From Server
        /// </summary>
        /// <param name="trackerData">trackerData reference to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>trackerData received from server</returns>
        public async Task SyncTrackersFromServerAsync(TrackerDTO trackerData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_TRACKERS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY,GetPermissionAtLevelID()},
                        {  Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(trackerData.RecordCount, CultureInfo.InvariantCulture)},
                        {  nameof(TrackersModel.TrackerID), Convert.ToString(trackerData.Tracker.TrackerID, CultureInfo.InvariantCulture) },
                        {  nameof(TrackerRangeModel.TrackerRangeID), Convert.ToString(trackerData.TrackerRange.TrackerRangeID, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                trackerData.ErrCode = httpData.ErrCode;
                if (trackerData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(trackerData, data);
                        SetPageResources(trackerData.Resources);
                        if (trackerData.RecordCount >= 0)
                        {
                            trackerData.Trackers = MapTrackers(data, nameof(trackerData.Trackers));
                        }
                        else if (trackerData.RecordCount == -1)
                        {
                            trackerData.Tracker = MapTracker(data[nameof(TrackerDTO.Tracker)]);
                            trackerData.TrackersI18N = MapTrackersI18N(data, nameof(TrackerDTO.TrackersI18N));
                            trackerData.Languages = new List<LanguageModel>();
                            if (GenericMethods.IsListNotEmpty(trackerData.TrackersI18N))
                            {
                                trackerData.Languages.AddRange(from languageData in trackerData.TrackersI18N
                                                               select new LanguageModel
                                                               {
                                                                   LanguageID = Convert.ToByte(languageData.LanguageID),
                                                                   LanguageName = languageData.LanguageName,
                                                                   IsDefault = languageData.IsActive
                                                               });
                            }
                            trackerData.TrackerRanges = MapTrackerRanges(data, nameof(trackerData.TrackerRanges));
                            List<ResourceModel> dataSource = trackerData.Resources.Where(x => x.GroupName == GroupConstants.RS_QUESTIONNAIRE_QUESTION_TYPE_GROUP).OrderBy(x => x.ResourceValue).ToList();
                            trackerData.TrackerTypes = GetPickerSource(dataSource, nameof(ResourceModel.ResourceKeyID), Constants.CNT_RESOURCE_VALUE_TEXT, trackerData.Tracker.TrackerTypeID, false, null);
                        }
                        else if (trackerData.RecordCount == -2)
                        {
                            trackerData.TrackerRange = MapRange(data[nameof(TrackerDTO.TrackerRange)]);
                            trackerData.TrackerRangesI18N = MapTrackerRangesI18N(data, nameof(TrackerDTO.TrackerRangesI18N));
                        }
                        trackerData.ErrCode = (ErrorCode)(int)data[nameof(TrackerDTO.ErrCode)];
                    }
                }
            }
            catch (Exception ex)
            {
                trackerData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// sync Tracker To Server
        /// </summary>
        /// <param name="trackerData">trackerData reference to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status call</returns>
        public async Task SyncTrackerToServerAsync(TrackerDTO trackerData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<TrackerDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_TRACKER_ASYNC_PATH,
                    ContentToSend = trackerData,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() }
                    },
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                trackerData.ErrCode = httpData.ErrCode;
                if (trackerData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data?.HasValues == true)
                    {
                        trackerData.Tracker.TrackerID = (short)data[nameof(TrackerDTO.Tracker)][nameof(TrackersModel.TrackerID)];
                    }
                }
            }
            catch (Exception ex)
            {
                trackerData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// sync Tracker Range To Server
        /// </summary>
        /// <param name="trackerData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Operation status call</returns>
        public async Task SyncTrackerRangesToServerAsync(TrackerDTO trackerData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<TrackerDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_TRACKER_RANGES_ASYNC_PATH,
                    ContentToSend = trackerData,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                trackerData.ErrCode = httpData.ErrCode;
                if (trackerData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data?.HasValues == true)
                    {
                        trackerData.TrackerRange.TrackerRangeID = (short)data[nameof(TrackerDTO.TrackerRange)][nameof(TrackerRangeModel.TrackerRangeID)];
                    }
                }
            }
            catch (Exception ex)
            {
                trackerData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="dataItem">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        public async Task MapAndSaveTrackersAsync(DataSyncModel result, JToken dataItem)
        {
            try
            {
                TrackerDTO patientTracker = new TrackerDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    Trackers = MapTrackers(dataItem, nameof(DataSyncDTO.Trackers)),
                    TrackerRanges = MapTrackerRanges(dataItem, nameof(DataSyncDTO.TrackerRanges)),
                };
                if (GenericMethods.IsListNotEmpty(patientTracker.Trackers)
                    || GenericMethods.IsListNotEmpty(patientTracker.TrackerRanges))
                {
                    await new TrackerDatabase().SaveTrackersAsync(patientTracker).ConfigureAwait(false);
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

        public async Task MapAndSaveTrackersI18NAsync(DataSyncModel result, JToken dataItem)
        {
            try
            {
                TrackerDTO patientTracker = new TrackerDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    TrackersI18N = MapTrackersI18N(dataItem, nameof(DataSyncDTO.TrackersI18N)),
                    TrackerRangesI18N = MapTrackerRangesI18N(dataItem, nameof(DataSyncDTO.TrackerRangesI18N)),
                };
                if (GenericMethods.IsListNotEmpty(patientTracker.TrackersI18N)
                    || GenericMethods.IsListNotEmpty(patientTracker.TrackerRangesI18N))
                {
                    await new TrackerDatabase().SaveTrackersI18NAsync(patientTracker).ConfigureAwait(false);
                    _ = LoadDependancyAsync(patientTracker).ConfigureAwait(false);
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

        private List<TrackersI18NModel> MapTrackersI18N(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
                ? (from dataItem in data[collectionName]
                   select MapTrackerI18N(dataItem)).ToList()
                : null;
        }

        private TrackersI18NModel MapTrackerI18N(JToken dataItem)
        {
            return dataItem.HasValues
                ? new TrackersI18NModel
                {
                    TrackerID = GetDataItem<short>(dataItem, nameof(TrackersI18NModel.TrackerID)),
                    TrackerName = GetDataItem<string>(dataItem, nameof(TrackersI18NModel.TrackerName)),
                    LanguageID = GetDataItem<byte>(dataItem, nameof(TrackersI18NModel.LanguageID)),
                    LanguageName = GetDataItem<string>(dataItem, nameof(TrackersI18NModel.LanguageName)),
                    IsActive = GetDataItem<bool>(dataItem, nameof(TrackersI18NModel.IsActive)),
                }
                : new TrackersI18NModel();
        }


        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="dataItem">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        public async Task MapAndSaveProgramTrackersAsync(DataSyncModel result, JToken dataItem)
        {
            try
            {
                TrackerDTO programTracker = new TrackerDTO
                {
                    ProgramTrackers = MapProgramTrackers(dataItem, nameof(DataSyncDTO.ProgramTrackers)),
                };
                if (GenericMethods.IsListNotEmpty(programTracker.ProgramTrackers))
                {
                    await new TrackerDatabase().SaveProgramTrackersDataAsync(programTracker).ConfigureAwait(false);
                    result.RecordCount = programTracker.ProgramTrackers?.Count ?? 0;
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        private async Task LoadDependancyAsync(TrackerDTO patientTracker)
        {
            if (GenericMethods.IsListNotEmpty(patientTracker.TrackerRanges))
            {
                foreach (TrackerRangeModel user in patientTracker.TrackerRanges)
                {
                    user.ImageName = await GetImageAsBase64Async(user.ImageName).ConfigureAwait(false);
                }
            }
        }

        private List<TrackersModel> MapTrackers(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
                ? (from dataItem in data[collectionName]
                   select MapTracker(dataItem)).ToList()
                : null;
        }

        private TrackersModel MapTracker(JToken dataItem)
        {
            return dataItem.HasValues ? new TrackersModel
            {
                TrackerID = GetDataItem<short>(dataItem, nameof(TrackersModel.TrackerID)),
                TrackerName = GetDataItem<string>(dataItem, nameof(TrackersModel.TrackerName)),
                TrackerIdentifier = GetDataItem<string>(dataItem, nameof(TrackersModel.TrackerIdentifier)),
                TrackerType = GetDataItem<string>(dataItem, nameof(TrackersModel.TrackerType)),
                Ranges = GetDataItem<string>(dataItem, nameof(TrackersModel.Ranges)),
                TrackerTypeID = GetDataItem<short>(dataItem, nameof(TrackersModel.TrackerTypeID)),
                IsActive = GetDataItem<bool>(dataItem, nameof(TrackersModel.IsActive)),
            } : new TrackersModel();
        }

        private List<TrackerRangesI18N> MapTrackerRangesI18N(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
               ? (from dataItem in data[collectionName]
                  select MapTrackerRangeI18N(dataItem)).ToList()
               : null;
        }

        private List<ProgramTrackerModel> MapProgramTrackers(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
               ? (from dataItem in data[collectionName]
                  select MapProgramTracker(dataItem)).ToList()
               : null;
        }

        private ProgramTrackerModel MapProgramTracker(JToken data)
        {
            return new ProgramTrackerModel
            {
                ProgramTrackerID = GetDataItem<long>(data, nameof(ProgramTrackerModel.ProgramTrackerID)),
                TrackerID = GetDataItem<short>(data, nameof(ProgramTrackerModel.TrackerID)),
                ProgramID = GetDataItem<long>(data, nameof(ProgramTrackerModel.ProgramID)),
                AssignAfterDays = GetDataItem<int>(data, nameof(ProgramTrackerModel.AssignAfterDays)),
                AssignForDays = GetDataItem<int>(data, nameof(ProgramTrackerModel.AssignForDays)),
                IsActive = GetDataItem<bool>(data, nameof(ProgramTrackerModel.IsActive)),
                ValueAddedBy = GetDataItem<short>(data, nameof(ProgramTrackerModel.ValueAddedBy))
            };
        }

        private TrackerRangesI18N MapTrackerRangeI18N(JToken data)
        {
            return new TrackerRangesI18N
            {
                CaptionText = GetDataItem<string>(data, nameof(TrackerRangesI18N.CaptionText)),
                IsActive = GetDataItem<bool>(data, nameof(TrackerRangesI18N.IsActive)),
                TrackerRangeID = GetDataItem<long>(data, nameof(TrackerRangesI18N.TrackerRangeID)),
                InstructionsText = GetDataItem<string>(data, nameof(TrackerRangesI18N.InstructionsText)),
                LanguageID = GetDataItem<byte>(data, nameof(TrackerRangesI18N.LanguageID)),
                LanguageName = GetDataItem<string>(data, nameof(TrackerRangesI18N.LanguageName))
            };
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
    }
}