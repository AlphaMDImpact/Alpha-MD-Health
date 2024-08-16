using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class ContentPageService : BaseService
    {
        public ContentPageService(IEssentials serviceEssentials) : base(serviceEssentials)
        {

        }
        #region SyncServices

        /// <summary>
        /// Sync Content Page from service
        /// </summary>
        /// <param name="contentPage">Content Pages reference to return output</param>
        /// <param name="lastSyncedDate">Last sync datetime</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ContentPages and Operation Status</returns>
        public async Task SyncContentPageFromServerAsync(ContentPageDTO contentPage, DateTimeOffset lastSyncedDate, CancellationToken cancellationToken)
        {
            try
            {
                var isBasic = string.IsNullOrWhiteSpace(await GetSecuredValueAsync(StorageConstants.SS_ACCESS_TOKEN_KEY).ConfigureAwait(true));
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    AuthType = isBasic
                        ? AuthorizationType.Basic
                        : AuthorizationType.Bearer,
                    PathWithoutBasePath = isBasic
                        ? UrlConstants.GET_BASIC_CONTENT_PAGES_ASYNC_PATH
                        : UrlConstants.GET_CONTENT_PAGES_ASYNC_PATH,
                    CancellationToken = cancellationToken,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { nameof(BaseDTO.RecordCount), Convert.ToString(contentPage.RecordCount, CultureInfo.InvariantCulture) },
                        { nameof(ContentPageModel.PageID), Convert.ToString(contentPage.Page?.PageID, CultureInfo.InvariantCulture) },
                        { nameof(BaseDTO.SelectedUserID), MobileConstants.IsMobilePlatform
                            ? Convert.ToString(_essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0), CultureInfo.InvariantCulture)
                            : Convert.ToString(contentPage.SelectedUserID, CultureInfo.InvariantCulture)},
                        { nameof(ContentPageModel.IsEducation), Convert.ToString(contentPage.Page?.IsEducation, CultureInfo.InvariantCulture) },
                        { Constants.FOR_PROVIDER_CONSTANT, Convert.ToString(contentPage.IsActive, CultureInfo.InvariantCulture) },
                        { nameof(BaseDTO.LastModifiedON), GetSyncDateTimeString(lastSyncedDate) },
                        { nameof(BaseDTO.FromDate), contentPage.FromDate },
                        { nameof(BaseDTO.ToDate), contentPage.ToDate },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                contentPage.ErrCode = httpData.ErrCode;
                if (contentPage.ErrCode == ErrorCode.OK)
                {
                    await MapGetContentPagesServiceResponseAsync(contentPage, httpData.Response).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                contentPage.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync published Content Page from service
        /// </summary>
        /// <param name="contentPage">Content Pages reference to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ContentPages and Operation Status</returns>
        public async Task SyncPublilshedContentToServerAsync(ContentPageDTO contentPage, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ContentPageDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.PUBLISH_CONTENT_PAGE_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { nameof(ContentPageModel.PageID), Convert.ToString(contentPage.Page?.PageID, CultureInfo.InvariantCulture) },
                        { nameof(ContentPageModel.IsPublished), Convert.ToString(contentPage.Page?.IsPublished , CultureInfo.InvariantCulture)}
                    }
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                contentPage.ErrCode = httpData.ErrCode;
            }
            catch (Exception ex)
            {
                contentPage.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }


        private async Task MapGetContentPagesServiceResponseAsync(ContentPageDTO contentPage, string jsonResponse)
        {
            JToken data = JToken.Parse(jsonResponse);
            if (data != null && data.HasValues)
            {
                contentPage.Response = null;
                if (MobileConstants.IsMobilePlatform)
                {
                    contentPage.Resources = new ResourceService(_essentials).MapResourcesData(data);
                }
                else
                {
                    MapCommonData(contentPage, data);
                }
                MapContentPages(data, contentPage);
                //Save synced records in mobile db
                if (MobileConstants.IsMobilePlatform && contentPage.RecordCount > -1)
                {
                    await new ContentPageDatabase().SaveContentPagesAsync(contentPage).ConfigureAwait(false);
                    contentPage.ErrCode = ErrorCode.OK;
                    _ = DownloadImagesAsync().ConfigureAwait(false);
                }
            }
        }

        internal async Task<object> MapContentPagesHistoryData(MedicalHistoryDTO medicalHistoryData, MedicalHistoryViewModel historyView, string jsonResponse)
        {
            ContentPageDTO contentPageData = new ContentPageDTO
            {
                IsMedicalHistory = true,
                FromDate = medicalHistoryData.FromDate,
                ToDate = medicalHistoryData.ToDate,
                RecordCount = medicalHistoryData.RecordCount,
                ErrCode = historyView.ErrorCode,
                IsActive = false
            };
            await MapGetContentPagesServiceResponseAsync(contentPageData, jsonResponse);
            contentPageData.FeaturePermissions = medicalHistoryData.FeaturePermissions;
            GetContentPagesUIData(contentPageData);
            historyView.HasData = GenericMethods.IsListNotEmpty(contentPageData.PatientEducations);
            return contentPageData;
        }


        /// <summary>
        /// Sync Content Page data to server
        /// </summary>
        /// <param name="requestData">object to return operation status</param>
        /// <param name="isPublishUnpublish">Check publish or not publish</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status</returns>
        public async Task SyncContentPageToServerAsync(ContentPageDTO requestData, bool isPublishUnpublish, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ContentPageDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_CONTENT_PAGE_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { Constants.SE_IS_PUBLISH_UNPUBLISH_QUERY_KEY, Convert.ToString(isPublishUnpublish, CultureInfo.InvariantCulture) }
                    },
                    ContentToSend = requestData,
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                requestData.ErrCode = httpData.ErrCode;
                requestData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }


        /// <summary>
        /// Sync Assign education data to server
        /// </summary>
        /// <param name="educationData">object to return operation status</param>    
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status</returns>
        public async Task SyncPatientEducationToServerAsync(ContentPageDTO educationData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ContentPageDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PATIENT_EDUCATION_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    },
                    ContentToSend = educationData,
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                educationData.ErrCode = httpData.ErrCode;
                if (httpData.ErrCode == ErrorCode.OK && MobileConstants.IsMobilePlatform)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data?.HasValues == true)
                    {
                        educationData.PatientEducation.PatientEducationID = (long)data[nameof(ContentPageDTO.PatientEducation)][nameof(PatientEducationModel.PatientEducationID)];
                    }
                    await new ContentPageDatabase().SavePatientEducationAsync(educationData.PatientEducation).ConfigureAwait(false);
                }
                educationData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                educationData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync Assign education data to server
        /// </summary>
        /// <param name="educationData">object to return operation status</param>    
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status</returns>
        public async Task SyncEducationStatusToServerAsync(ContentPageDTO educationData, CancellationToken cancellationToken)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    await new ContentPageDatabase().GetEducationStatusAsync(educationData).ConfigureAwait(false);
                }
                if (GenericMethods.IsListNotEmpty(educationData.PatientEducations))
                {
                    var httpData = new HttpServiceModel<ContentPageDTO>
                    {
                        CancellationToken = cancellationToken,
                        PathWithoutBasePath = UrlConstants.SAVE_EDUCATION_STATUS_ASYNC_PATH,
                        QueryParameters = new NameValueCollection
                        {
                            { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        },
                        ContentToSend = educationData,
                    };
                    await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                    educationData.ErrCode = httpData.ErrCode;
                    educationData.Response = httpData.Response;
                    if (educationData.ErrCode == ErrorCode.OK && MobileConstants.IsMobilePlatform)
                    {
                        JToken data = JToken.Parse(httpData.Response);
                        if (data?.HasValues == true)
                        {
                            educationData.PatientEducations.ForEach(x => x.IsSynced = true);
                            if (GenericMethods.IsListNotEmpty(educationData.PatientEducations))
                            {
                                await new ContentPageDatabase().UpdateEducationStatusAsync(educationData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                educationData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        #endregion

        /// <summary>
        /// Get Content Pages 
        /// </summary>
        /// <param name="contentPage">Reference object to return ContentPages data</param>
        /// <returns>ContentPages and Operation Status</returns>
        public async Task GetContentPagesAsync(ContentPageDTO contentPage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(GetSecuredValueAsync(nameof(TempSessionModel.TokenIdentifier)).Result))
                {
                    contentPage.SelectedUserID = GetUserID();
                }
                if (MobileConstants.IsMobilePlatform)
                {
                    contentPage.CreatedByID = GetLoginUserID();
                    long educationID = contentPage.Page.PageID;
                    contentPage.Page.IsEducation = true;
                    contentPage.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                    contentPage.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                    await Task.WhenAll(
                        GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_MENU_PAGE_GROUP),
                        GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                        GetFeaturesAsync(AppPermissions.PatientEducationAddEdit.ToString(), AppPermissions.EducationPreview.ToString(), AppPermissions.PatientEducationsView.ToString()),
                        new ContentPageDatabase().GetContentPagesAsync(contentPage)
                    ).ConfigureAwait(false);
                    var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
                    if (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
                    {
                        contentPage.Pages = contentPage.Pages.Where(x => x.FromDate.Value.Date <= GenericMethods.GetUtcDateTime.Date).ToList();
                        contentPage.Pages = contentPage.Pages.GroupBy(elem => elem.PageID).Select(group => group.First()).ToList();
                    }
                    if (contentPage.RecordCount == -2)
                    {
                        if (contentPage.Page == null)
                        {
                            await GetEducationSyncServiceAsync(contentPage, educationID).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                        if (GenericMethods.IsListNotEmpty(contentPage.Pages))
                        {
                            if (contentPage.RecordCount > 0 || contentPage.SelectedUserID > 0)
                            {
                                contentPage.Pages = DistictEducationsByPageID(contentPage.Pages);
                                //Case of dashboard
                                foreach (var pages in contentPage.Pages)
                                {
                                    MapContentPageUIData(pages);
                                }
                                contentPage.Pages.ForEach(Page =>
                                {
                                    MapFormattedDate(dayFormat, monthFormat, yearFormat, Page, false);
                                    Page.FromDateString = $"{Page.FromDateString} {Constants.SYMBOL_DASH} {Page.ToDateString}";
                                    var catergorynameString = Page.ProgramName == null ? Page.CategoryName : string.Concat(Page.CategoryName, Constants.SYMBOL_BAR, Page.ProgramName);
                                    Page.CategoryName = $"{catergorynameString}";
                                });
                            }
                            else
                            {
                                contentPage.EducationGroup = new List<EducationCategoryGroupModel>();
                                //Get Education Library Page data
                                if (contentPage.Page.EducationCategoryID < 1)
                                {
                                    GetEducations(contentPage);
                                }
                                else
                                {
                                    GetEducationCategoryGroup(contentPage);
                                }
                                if (GenericMethods.IsListNotEmpty(contentPage.Pages))
                                {
                                    contentPage.Pages.ForEach(Page =>
                                    {
                                        Page.ToDateString = GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(Page.ToDate.Value), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                                    });
                                }
                            }
                            contentPage.ErrCode = ErrorCode.OK;
                        }
                    }
                    contentPage.Pages = contentPage.Pages.OrderByDescending(x => x.FromDate.Value.Date).ToList();
                }
                else
                {
                    await SyncContentPageFromServerAsync(contentPage, GenericMethods.GetDefaultDateTime, CancellationToken.None).ConfigureAwait(false);
                    GetContentPagesUIData(contentPage);
                }
            }
            catch (Exception ex)
            {
                contentPage.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void GetContentPagesUIData(ContentPageDTO contentPage)
        {
            if (contentPage.ErrCode == ErrorCode.OK
                && GenericMethods.IsListNotEmpty(contentPage.PatientEducations)
                && contentPage.RecordCount != -1)
            {
                LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                contentPage.PatientEducations.ForEach(patienteducation =>
                {
                    MapFormattedDate(dayFormat, monthFormat, yearFormat, patienteducation, false);
                });
                contentPage.PatientEducation = new PatientEducationModel
                {
                    FromDateString = GenericMethods.GetDateTimeFormat(DateTimeType.Date, dayFormat, monthFormat, yearFormat),
                    ToDateString = GenericMethods.GetDateTimeFormat(DateTimeType.Date, dayFormat, monthFormat, yearFormat),
                };
                var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
                if (roleID != (int)RoleName.Patient && roleID != (int)RoleName.CareTaker)
                {
                    GenericMethods.SortByDate(contentPage.PatientEducations, x => x.FromDate, x => x.ToDate);
                }

            }
        }

        private void MapFormattedDate(string dayFormat, string monthFormat, string yearFormat, PatientEducationModel patientEducation, bool isDetailPage)
        {
            patientEducation.ToDateString = GenericMethods.GetDateTimeBasedOnCulture(patientEducation.ToDate.Value, DateTimeType.Date, dayFormat, monthFormat, yearFormat);
            patientEducation.FromDateString = GenericMethods.GetDateTimeBasedOnCulture(patientEducation.FromDate.Value, DateTimeType.Date, dayFormat, monthFormat, yearFormat);
            patientEducation.DateStyle = GetDateTimeStyle(patientEducation.FromDate.Value, patientEducation.ToDate.Value);
            patientEducation.StatusValue = patientEducation.Status.ToString();
            patientEducation.StatusColor = GetStatusStyle(patientEducation.FromDate.Value, patientEducation.ToDate.Value, patientEducation.Status.ToString());
            patientEducation.LeftDefaultIcon =  ImageConstants.PATIENT_MOBILE_EDU_ICON;

        }


        private string GetDateTimeStyle(DateTimeOffset? fromDate, DateTimeOffset? toDate)
        {
            var currentDate = GenericMethods.GetUtcDateTime;
            string dateStyle;
            if (!fromDate.HasValue || !toDate.HasValue || (fromDate <= currentDate && toDate >= currentDate))
            {
                dateStyle = MobileConstants.IsMobilePlatform
                    ? StyleConstants.SUCCESS_COLOR
                    : FieldTypes.SuccessBadgeControl.ToString();
            }
            else if (fromDate > currentDate && toDate > currentDate)
            {
                dateStyle = MobileConstants.IsMobilePlatform
                    ? StyleConstants.SECONDARY_APP_COLOR
                    : FieldTypes.SecondaryBadgeControl.ToString();
            }
            else
            {
                dateStyle = MobileConstants.IsMobilePlatform
                    ? StyleConstants.ERROR_COLOR
                    : FieldTypes.DangerBadgeControl.ToString();
            }
            return dateStyle;
        }
        private string GetStatusStyle(DateTimeOffset? fromDate, DateTimeOffset? toDate,string status)
        {
            var currentDate = GenericMethods.GetUtcDateTime;
            string dateStyle;
            if (!fromDate.HasValue || !toDate.HasValue || (fromDate <= currentDate && toDate >= currentDate))
            {
                dateStyle = "<span style='color:" +StyleConstants.SUCCESS_COLOR+";'>" + status + "</b></span>";
            }

            else if (fromDate > currentDate && toDate > currentDate)
            {
                dateStyle = "<span style='color:" + StyleConstants.SECONDARY_APP_COLOR + ";'>" + status + "</b></span>";
            }
            else
            {
                dateStyle = "<span style='color:" + StyleConstants.ERROR_COLOR + ";'>" + status + "</b></span>";
            }
            return dateStyle;
        }

        private void MapFormattedDate(string dayFormat, string monthFormat, string yearFormat, ContentPageModel Page, bool isDetailPage)
        {
            if (!string.IsNullOrWhiteSpace(dayFormat) && !string.IsNullOrWhiteSpace(monthFormat) && !string.IsNullOrWhiteSpace(yearFormat))
            {
                Page.ToDateString = GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(Page.ToDate.Value), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                Page.FromDateString = isDetailPage
                    ? $"{GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(Page.FromDate.Value), DateTimeType.Date, dayFormat, monthFormat, yearFormat)} - {Page.ToDateString}"
                    : GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(Page.FromDate.Value), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
            }
        }

        /// <summary>
        /// Update the education Status
        /// </summary>
        /// <param name="educationData">Reference object to hold education status</param>
        /// <returns>Operation status</returns>
        public async Task SaveEducationStatusAsync(ContentPageDTO educationData)
        {
            try
            {
                await new ContentPageDatabase().UpdateEducationStatusAsync(educationData).ConfigureAwait(false);
                educationData.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                educationData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
        }

        private async Task GetEducationSyncServiceAsync(ContentPageDTO contentPage, long educationID)
        {
            contentPage.Page = new ContentPageModel
            {
                PageID = contentPage.PermissionAtLevelID > 0 ? Convert.ToInt64(contentPage.ErrorDescription, CultureInfo.InvariantCulture) : educationID,
                IsEducation = true
            };
            await SyncContentPageFromServerAsync(contentPage, GenericMethods.GetDefaultDateTime, CancellationToken.None).ConfigureAwait(false);
            if (contentPage.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(contentPage.PageDetails))
            {
                contentPage.Page.Title = contentPage.PageDetails.FirstOrDefault().PageHeading;
                contentPage.Page.PageData = contentPage.PageDetails.FirstOrDefault().PageData;
            }
        }

        /// <summary>
        /// Get Content Page Details
        /// </summary>
        /// <param name="pageData">Reference object to return ContentPage page data in resource</param>
        /// <returns>Content Page data</returns>
        public async Task GetContentDetailsAsync(BaseDTO pageData)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    await Task.WhenAll(
                        new ContentPageDatabase().GetContentDetailsAsync(pageData),
                        GetResourcesAsync(pageData.ErrorDescription)
                    ).ConfigureAwait(false);
                    if (GenericMethods.IsListNotEmpty(pageData.Resources))
                    {
                        pageData.Resources.AddRange(PageData.Resources);
                    }
                    else
                    {
                        if (pageData.LastModifiedBy == PageType.ContentPage.ToString())
                        {
                            await SyncAndMapContentPageFromServerAsync(pageData).ConfigureAwait(false);
                        }
                        await GetResourceAsync(ResourceConstants.R_NO_DATA_FOUND_KEY, pageData).ConfigureAwait(false);
                    }
                }
                else
                {
                    await SyncAndMapContentPageFromServerAsync(pageData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                await GetResourceAsync(ErrorCode.ErrorWhileRetrievingRecords.ToString(), pageData).ConfigureAwait(false);
            }
        }

        private async Task SyncAndMapContentPageFromServerAsync(BaseDTO pageData)
        {
            var contentPage = new ContentPageDTO
            {
                Page = new ContentPageModel { PageID = Convert.ToInt64(pageData.AddedBy, CultureInfo.InvariantCulture) },
                RecordCount = -2,
            };
            await SyncContentPageFromServerAsync(contentPage, GenericMethods.GetDefaultDateTime, CancellationToken.None).ConfigureAwait(false);
            if (contentPage.ErrCode == ErrorCode.OK)
            {
                pageData.Resources = new List<ResourceModel> { new ResourceModel{
                                ResourceKey = pageData.AddedBy,
                                ResourceValue = contentPage.PageDetails[0].PageHeading,
                                InfoValue =  contentPage.PageDetails[0].PageData,
                                LanguageID = pageData.LanguageID,
                                IsActive = contentPage.Page?.IsLink ?? false
                                } };
            }
        }

        #region MappingMethods

        private void MapContentPages(JToken data, ContentPageDTO contentPages)
        {
            SetResourcesAndSettings(contentPages);
            contentPages.Pages = MapPagesFromResponse(data, nameof(ContentPageDTO.Pages));
            if (GenericMethods.IsListNotEmpty(contentPages.Pages))
            {
                contentPages.Pages = DistictEducationsByPageID(contentPages.Pages);
            }
            contentPages.PageDetails = MapPageDetailsFromResponse(data, nameof(ContentPageDTO.PageDetails));
            contentPages.PatientEducations = MapPatientEducationsFromResponse(data, nameof(ContentPageDTO.PatientEducations));
            if (contentPages.RecordCount == -1 || contentPages.RecordCount == -2)
            {
                MapContentPage(data, contentPages);
            }
            else
            {
                if (GenericMethods.IsListNotEmpty(contentPages.Pages))
                {
                    foreach (ContentPageModel contentData in contentPages.Pages)
                    {
                        contentData.StatusColor = contentData.IsPublished ? FieldTypes.PrimaryBadgeControl.ToString() : FieldTypes.DangerBadgeControl.ToString();
                    }
                }
            }
            contentPages.ErrCode = (ErrorCode)(int)data[nameof(ContentPageDTO.ErrCode)];
        }

        private List<PatientEducationModel> MapPatientEducationsFromResponse(JToken data, string dataSelector)
        {
            return (!string.IsNullOrWhiteSpace(dataSelector) && data[dataSelector].Any())
                ? (from dataItem in data[dataSelector]
                   select new PatientEducationModel
                   {
                       PatientEducationID = (long)dataItem[nameof(PatientEducationModel.PatientEducationID)],
                       EducationTypeID = (short)dataItem[nameof(PatientEducationModel.EducationTypeID)],
                       StatusID = (short)dataItem[nameof(PatientEducationModel.StatusID)],
                       PageID = (long)dataItem[nameof(PatientEducationModel.PageID)],
                       UserID = (long)dataItem[nameof(PatientEducationModel.UserID)],
                       ProgramEducationID = (long)dataItem[nameof(PatientEducationModel.ProgramEducationID)],
                       ImageName = (string)dataItem[nameof(PatientEducationModel.ImageName)],
                       ProgramID = (long)dataItem[nameof(PatientEducationModel.ProgramID)],
                       IsActive = (bool)dataItem[nameof(PatientEducationModel.IsActive)],
                       PageHeading = (string)dataItem[nameof(PatientEducationModel.PageHeading)],
                       FromDate = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientEducationModel.FromDate)),
                       ToDate = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientEducationModel.ToDate)),
                       ProgramColor = (string)dataItem[nameof(PatientEducationModel.ProgramColor)],
                       ProgramName = (string)dataItem[nameof(PatientEducationModel.ProgramName)],
                       CategoryName = (string)dataItem[nameof(PatientEducationModel.CategoryName)],
                       Status = ((string)dataItem[nameof(PatientEducationModel.Status)])
             .ToEnum<PatientEducationStatus>() == PatientEducationStatus.Open
         ? PatientEducationStatus.New
         : ((string)dataItem[nameof(PatientEducationModel.Status)]).ToEnum<PatientEducationStatus>(),

                       IsSynced = true
                   }).ToList()
                : null;
        }

        private List<ContentPageModel> MapPagesFromResponse(JToken data, string dataSelector)
        {
            return (!string.IsNullOrWhiteSpace(dataSelector) && data[dataSelector].Any())
                ? (from dataItem in data[dataSelector]
                   select new ContentPageModel
                   {
                       PageID = (long)dataItem[nameof(ContentPageModel.PageID)],
                       EducationID = (long)dataItem[nameof(ContentPageModel.EducationID)],
                       IsEducation = (bool)dataItem[nameof(ContentPageModel.IsEducation)],
                       PageTags = (string)dataItem[nameof(ContentPageModel.PageTags)],
                       IsLink = (bool)dataItem[nameof(ContentPageModel.IsLink)],
                       IsPdf = (bool)dataItem[nameof(ContentPageModel.IsPdf)],
                       FromDate = GetDataItem<DateTimeOffset>(dataItem, nameof(ContentPageModel.FromDate)),
                       ToDate = GetDataItem<DateTimeOffset>(dataItem, nameof(ContentPageModel.ToDate)),
                       IsPublished = (bool)dataItem[nameof(ContentPageModel.IsPublished)],
                       ImageName = (string)dataItem[nameof(ContentPageModel.ImageName)],
                       PDFName = (string)dataItem[nameof(ContentPageModel.PDFName)],
                       EducationCategoryID = (long)dataItem[nameof(ContentPageModel.EducationCategoryID)],
                       ProgramEducationID = (long)dataItem[nameof(ContentPageModel.ProgramEducationID)],
                       Title = (string)dataItem[nameof(ContentPageModel.Title)],
                       Status = (string)dataItem[nameof(ContentPageModel.Status)],
                       PageType = (string)dataItem[nameof(ContentPageModel.PageType)],
                       ProgramColor = (string)dataItem[nameof(ContentPageModel.ProgramColor)],
                       ProgramName = (string)dataItem[nameof(ContentPageModel.ProgramName)],
                       AddedOn = (DateTimeOffset)dataItem[nameof(ContentPageModel.AddedOn)],
                       CategoryName = GetDataItem<string>(data, nameof(ContentPageModel.Name)),
                       IsDataDownloaded = true,
                       IsActive = GetDataItem<bool>(dataItem, nameof(ContentPageModel.IsActive)),
                       PageData = (string)dataItem[nameof(ContentPageModel.PageData)],
                       Description = (string)dataItem[nameof(ContentPageModel.Description)]
                   }).ToList()
                : null;
        }

        private List<ContentDetailModel> MapPageDetailsFromResponse(JToken data, string dataSelector)
        {
            return (!string.IsNullOrWhiteSpace(dataSelector) && data[dataSelector].Any())
                ? (from dataItem in data[dataSelector]
                   where (long)dataItem[nameof(ContentDetailModel.PageID)] >= 0
                   select new ContentDetailModel
                   {
                       PageID = (long)dataItem[nameof(ContentDetailModel.PageID)],
                       LanguageName = (string)dataItem[nameof(ContentDetailModel.LanguageName)],
                       LanguageID = (byte)dataItem[nameof(ContentDetailModel.LanguageID)],
                       PageHeading = (string)dataItem[nameof(ContentDetailModel.PageHeading)],
                       PageData = (string)dataItem[nameof(ContentDetailModel.PageData)],
                       IsActive = GetDataItem<bool>(dataItem, nameof(ContentDetailModel.IsActive)),
                       StatusID = (short)dataItem[nameof(ContentDetailModel.StatusID)],
                       Status = ((string)dataItem[nameof(ContentDetailModel.Status)]).ToEnum<PatientEducationStatus>(),
                       PageName = PageType.ContentPage,
                       Description = (string)dataItem[nameof(ContentDetailModel.Description)],
                       IsDataDownloaded = true
                   }).ToList()
                : null;
        }

        internal async Task MapAndSavePagesAsync(DataSyncModel result, JToken data, string pagesCollection, string detailsCollection, string educationsCollection)
        {
            try
            {
                ContentPageDTO contentPages = new ContentPageDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    Pages = MapPagesFromResponse(data, pagesCollection),
                    PageDetails = MapPageDetailsFromResponse(data, detailsCollection),
                    PatientEducations = MapPatientEducationsFromResponse(data, educationsCollection)
                };
                if (GenericMethods.IsListNotEmpty(contentPages.Pages)
                    || GenericMethods.IsListNotEmpty(contentPages.PageDetails)
                    || GenericMethods.IsListNotEmpty(contentPages.PatientEducations))
                {
                    await new ContentPageDatabase().SaveContentPagesAsync(contentPages).ConfigureAwait(false);
                }
                result.ErrCode = ErrorCode.OK;
                _ = DownloadImagesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        private void MapContentPage(JToken data, ContentPageDTO contentPages)
        {
            JToken taskJData = data[nameof(ContentPageDTO.Page)];
            if (GenericMethods.IsListNotEmpty(contentPages.Pages))
            {
                foreach (ContentPageModel item in contentPages.Pages)
                {
                    item.FromDate = _essentials.ConvertToLocalTime(item.FromDate.Value);
                    item.ToDate = _essentials.ConvertToLocalTime(item.ToDate.Value);
                }
            }
            contentPages.Page = GenericMethods.IsListNotEmpty(contentPages.Pages) ? contentPages.Pages.FirstOrDefault() : contentPages.Page;
            EmptyImage(contentPages);
            contentPages.Pages?.Clear();
            SetPageResources(contentPages.Resources);
            contentPages.PageTypes = (data[nameof(ContentPageDTO.PageTypes)]?.Count() > 0) ?
                (from dataItem in data[nameof(ContentPageDTO.PageTypes)]
                 select new OptionModel
                 {
                     OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                     OptionID = GetOptionID(dataItem),
                     IsSelected = GetIsSelected(dataItem, contentPages.Page)
                 }).ToList() : new List<OptionModel>();
            contentPages.EducationTypes = (data[nameof(ContentPageDTO.EducationTypes)]?.Count() > 0) ?
                (from dataItem in data[nameof(ContentPageDTO.EducationTypes)]
                 select new OptionModel
                 {
                     OptionID = (long)dataItem[nameof(OptionModel.OptionID)],
                     ParentOptionID = (long)dataItem[nameof(OptionModel.ParentOptionID)],
                     OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                     IsSelected = (bool)dataItem[nameof(OptionModel.IsSelected)], //educationData.Page.EducationCategoryID
                 }).ToList() : new List<OptionModel>();
            //AddPlaceHolder(contentPages.EducationTypes);
            // contentPages.EducationTypes = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_MENU_PAGE_GROUP, " ", true, contentPages.Page.EducationCategoryID );
            contentPages.Educations = (data[nameof(ContentPageDTO.Educations)]?.Count() > 0) ?
                (from dataItem in data[nameof(ContentPageDTO.Educations)]
                 select new OptionModel
                 {
                     OptionID = (long)dataItem[nameof(OptionModel.OptionID)],
                     ParentOptionID = (long)dataItem[nameof(OptionModel.ParentOptionID)],
                     OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                     IsSelected = MobileConstants.IsMobilePlatform ? (bool)dataItem[nameof(OptionModel.IsSelected)]
                      : contentPages.Page?.PageID == (long)dataItem[nameof(OptionModel.OptionID)],
                 }).ToList() : new List<OptionModel>();
            //AddPlaceHolder(contentPages.Educations);
            EducationCategory(data, contentPages);
        }

        private void EducationCategory(JToken data, ContentPageDTO contentPages)
        {
            contentPages.EducationCategory = (data[nameof(ContentPageDTO.EducationCategory)]?.Count() > 0) ?
               (from dataItem in data[nameof(ContentPageDTO.EducationCategory)]
                select new OptionModel
                {
                    OptionID = (long)dataItem[nameof(OptionModel.OptionID)],
                    OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                    IsSelected = contentPages.Page != null && contentPages.Page.PageID > 0 && (long)dataItem[nameof(OptionModel.OptionID)] == contentPages.Page.EducationCategoryID,
                }).ToList() : new List<OptionModel>();
            contentPages.EducationCategory.RemoveAll(x => string.IsNullOrWhiteSpace(x.OptionText));
            //AddPlaceHolder(contentPages.EducationCategory);

            contentPages.Languages = (contentPages.PageDetails != null) ?
                (from pageData in contentPages.PageDetails
                 select new LanguageModel
                 {
                     LanguageID = pageData.LanguageID,
                     LanguageName = pageData.LanguageName
                 }).ToList() : null;
        }

        private void EmptyImage(ContentPageDTO contentPages)
        {
            contentPages.Page.ImageBase64 = string.IsNullOrWhiteSpace(contentPages.Page.ImageName) ? string.Empty : contentPages.Page.ImageBase64;
        }

        private long GetOptionID(JToken dataItem)
        {
            if ((long)dataItem[nameof(OptionModel.OptionID)] == Constants.CONTENT_KEY_ID)
            {
                return (long)ContentType.Content;
            }
            else if ((long)dataItem[nameof(OptionModel.OptionID)] == Constants.LINKS_KEY_ID)
            {
                return (long)ContentType.Link;
            }
            else
            {
                return (long)ContentType.Pdf;
            }
        }

        private bool GetIsSelected(JToken dataItem, ContentPageModel page)
        {
            if (page.IsLink)
            {
                return (long)dataItem[nameof(OptionModel.OptionID)] == Constants.LINKS_KEY_ID;
            }
            if (page.IsPdf)
            {
                return (long)dataItem[nameof(OptionModel.OptionID)] == Constants.PDF_KEY_ID;
            }
            return ((long)dataItem[nameof(OptionModel.OptionID)]) == Constants.CONTENT_KEY_ID;
        }

        private void MapContentPageUIData(ContentPageModel page)
        {
            if (string.IsNullOrWhiteSpace(page.ImageName) || page.ImageName.Contains(Constants.HTTP_TAG_PREFIX))
            {
                page.LeftDefaultIcon = ImageConstants.I_DEFAULT_EDUCATION_SVG;
            }
            else
            {
                //todo:
                //page.LeftSourceIcon = ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(page.ImageName));
            }
            if (!string.IsNullOrWhiteSpace(page.Status))
            {
                page.StatusColor = page.Status.ToEnum<PatientEducationStatus>() == PatientEducationStatus.InProgress ? StyleConstants.ACCENT_COLOR : StyleConstants.SUCCESS_COLOR;
                page.Status = page.Status.ToEnum<PatientEducationStatus>().ToString();
            }
        }

        private async Task DownloadImagesAsync()
        {
            ContentPageDTO contentPages = new ContentPageDTO { Pages = new List<ContentPageModel>(), PageDetails = new List<ContentDetailModel>() };
            using (ContentPageDatabase contentPageDatabase = new ContentPageDatabase())
            {
                await contentPageDatabase.GetContentPagesStatusAsync(contentPages).ConfigureAwait(false);
            }
            await Task.WhenAll(
                GetPageImagesAsync(contentPages),
                GetPageDetailsImageAsync(contentPages)
            ).ConfigureAwait(false);
            await new ContentPageDatabase().UpdateContentPagesSyncImageStatusAsync(contentPages).ConfigureAwait(false);
        }
        #endregion

        private void GetEducations(ContentPageDTO contentPage)
        {
            foreach (var categoryGroup in contentPage.Pages.GroupBy(x => x.CategoryName).ToList())
            {
                if (GenericMethods.IsListNotEmpty(categoryGroup.ToList()))
                {
                    List<ContentPageModel> educations = DistictEducationsByPageID(categoryGroup.ToList());
                    EducationCategoryGroupModel educationCategoryGroupData = new EducationCategoryGroupModel
                    {
                        EducationCategoryID = contentPage.Page.EducationCategoryID,
                        Name = categoryGroup.Key + " (" + educations?.Count + ") ",
                        SubHeader = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_SHOW_MORE_KEY)
                    };
                    educationCategoryGroupData.AddRange(educations.OrderBy(x => x.CategoryName).Take(Constants.NO_OF_EDUCATIONS).ToList());
                    educationCategoryGroupData.ForEach(item =>
                    {
                        MapContentPageUIData(item);
                    });
                    contentPage.EducationGroup.Add(educationCategoryGroupData);
                }
            }
            contentPage.EducationGroup = contentPage.EducationGroup.OrderBy(x => x.Name).ToList();
        }

        private List<ContentPageModel> DistictEducationsByPageID(List<ContentPageModel> educations)
        {
            var distinctEducations = new List<ContentPageModel>();
            foreach (var page in educations.GroupBy(e => (e.PageID, e.ProgramColor, e.ProgramName)))
            {
                var education = page.FirstOrDefault(x => x.Status != PatientEducationStatus.Completed.ToString()) ?? page.FirstOrDefault();
                education.EducationIDs = string.Join(Constants.SYMBOL_COMMA_SEPERATOR_STRING, page.Select(x => x.EducationID.ToString()));
                distinctEducations.Add(education);
            }
            return distinctEducations.OrderBy(x => x.Title).ToList();
        }

        private List<PatientEducationModel> DistictEducationsByPageID(List<PatientEducationModel> educations)
        {
            var distinctEducations = new List<PatientEducationModel>();
            foreach (var page in educations.GroupBy(e => e.PageID))
            {
                var education = page.FirstOrDefault(x => x.Status != PatientEducationStatus.Completed) ?? page.FirstOrDefault();
                education.PatientEducationIDs = string.Join(Constants.SYMBOL_COMMA_SEPERATOR_STRING, page.Select(x => x.PatientEducationID.ToString()));
                distinctEducations.Add(education);
            }
            return distinctEducations.OrderBy(x => x.PageHeading).ToList();
        }

        private void GetEducationCategoryGroup(ContentPageDTO contentPage)
        {
            foreach (var categoryGroup in contentPage.Pages.GroupBy(x => (x.CategoryName, x.CategoryImage, x.CategoryDetails)).ToList())
            {
                if (GenericMethods.IsListNotEmpty(categoryGroup.ToList()))
                {
                    List<ContentPageModel> educations = DistictEducationsByPageID(categoryGroup.ToList());
                    EducationCategoryGroupModel educationCategoryGroupData = new EducationCategoryGroupModel
                    {
                        EducationCategoryID = contentPage.Page.EducationCategoryID,
                        Name = categoryGroup.Key.CategoryName + " (" + educations?.Count + ") ",
                        CategoryImage = categoryGroup.Key.CategoryImage,
                        CategoryDetails = categoryGroup.Key.CategoryDetails,
                        //todo:
                        //CategoryImageSource = ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(categoryGroup.Key.CategoryImage))
                    };
                    educationCategoryGroupData.AddRange(educations.OrderBy(x => x.CategoryName).ToList());
                    educationCategoryGroupData.ForEach(item =>
                    {
                        MapContentPageUIData(item);
                    });
                    contentPage.EducationGroup.Add(educationCategoryGroupData);
                }
            }
            contentPage.EducationGroup = contentPage.EducationGroup.OrderBy(x => x.Name).ToList();
        }

        private async Task GetPageImagesAsync(ContentPageDTO contentPages)
        {
            if (GenericMethods.IsListNotEmpty(contentPages.Pages))
            {
                foreach (var page in contentPages.Pages)
                {
                    page.ImageName = page.ImageName.Contains(Constants.HTTP_TAG_PREFIX)
                        ? await GetImageAsBase64Async(page.ImageName).ConfigureAwait(false)
                        : string.Empty;
                }
            }
        }

        private async Task GetPageDetailsImageAsync(ContentPageDTO contentPages)
        {
            if (GenericMethods.IsListNotEmpty(contentPages.PageDetails))
            {
                foreach (var page in contentPages.PageDetails)
                {
                    page.PageData = await GetImageContentAsync(page.PageData).ConfigureAwait(false);
                }
            }
        }

        private async Task GetResourceAsync(string key, BaseDTO pageData)
        {//todo:
         //await GetResourceAsync(key).ConfigureAwait(true);
            pageData.AddedBy = key;
            pageData.Resources = PageData.Resources;
        }
    }
}