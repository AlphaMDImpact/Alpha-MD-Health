using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public class AppointmentService : BaseService
{
    public AppointmentService(IEssentials essentials) : base(essentials)
    {

    }

    /// <summary>
    /// Sync Appointments from service
    /// </summary>
    /// <param name="appointmentData">appointmentData reference to return output</param>
    /// <param name="lastSyncedDate">Last sync date-time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Appointments received from server in appointmentData</returns>
    public async Task SyncAppointmentsFromServerAsync(AppointmentDTO appointmentData, DateTimeOffset lastSyncedDate, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_APPOINTMENTS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_APPOINTMENT_ID_QUERY_KEY, Convert.ToString(appointmentData.Appointment.AppointmentID, CultureInfo.InvariantCulture) },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(appointmentData.RecordCount, CultureInfo.InvariantCulture) },
                    { Constants.SE_LAST_MODIFIED_ON_QUERY_KEY, MobileConstants.IsMobilePlatform ? GetSyncDateTimeString(lastSyncedDate) : GetSyncDateTimeString(GenericMethods.GetDefaultDateTime) },
                    { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(appointmentData.SelectedUserID, CultureInfo.InvariantCulture) }
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            appointmentData.ErrCode = httpData.ErrCode;
            if (appointmentData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(appointmentData, data);
                    SetResourcesAndSettings(appointmentData);
                    await MapAppointmentRecordsAsync(data, appointmentData).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            appointmentData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Update appointment status
    /// </summary>
    /// <param name="appointment">object to send appontment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public async Task UpdateAppointmentAsync(AppointmentDTO appointment, CancellationToken cancellationToken)
    {
        try
        {
            appointment.LastModifiedON = GenericMethods.GetUtcDateTime;
            if (appointment.SelectedUserID < 1)
            {
                switch (appointment.Appointment.AppointmentTypeImage)
                {
                    case nameof(AppPermissions.PatientAppointmentsView):
                        appointment.SelectedUserID = GetUserID();
                        break;
                    case nameof(AppPermissions.AppointmentsView):
                        appointment.SelectedUserID = GetLoginUserID();
                        break;
                    default:
                        appointment.SelectedUserID = GetUserID();
                        break;
                }

            }
            var httpData = new HttpServiceModel<AppointmentDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.UPDATE_APPOINTMENT_STATUS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = appointment,
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            appointment.ErrCode = httpData.ErrCode;
            if (appointment.ErrCode == ErrorCode.OK && MobileConstants.IsMobilePlatform)
            {
                await new AppointmentDatabase().UpdateAppointmentStatusAsync(appointment).ConfigureAwait(false);
                await CancelNotificationForAppointment(appointment, appointment.Appointment.AppointmentID.ToString()).ConfigureAwait(false);
                if (appointment.Appointment.AppointmentStatusID == ResourceConstants.R_ACCEPTED_STATUS_KEY)
                {
                    await GenerateNotificationData(appointment).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            appointment.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Appointment data to server
    /// </summary>
    /// <param name="requestData">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status call</returns>
    public async Task SyncAppointmentToServerAsync(AppointmentDTO requestData, CancellationToken cancellationToken)
    {
        try
        {
            requestData.AddedON = GenericMethods.GetUtcDateTime;
            var httpData = new HttpServiceModel<AppointmentDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_APPOINTMENT_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = requestData,
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            requestData.ErrCode = httpData.ErrCode;
            if (requestData.ErrCode == ErrorCode.DuplicateData)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data?.HasValues == true)
                {
                    await MapParticipantsFromJsonAsync(data, requestData).ConfigureAwait(false);
                }
            }
            if (requestData.ErrCode == ErrorCode.OK && MobileConstants.IsMobilePlatform)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data?.HasValues == true)
                {
                    MapAppointmentIDFromJson(data, requestData);
                }
                await new AppointmentDatabase().SaveAppointmentsDataAsync(requestData).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }
    private void MapAppointmentIDFromJson(JToken data, AppointmentDTO appointmentData)
    {
        if (appointmentData.Appointment.AppointmentID == 0)
        {
            if (data?.HasValues == true)
            {
                JToken patientProgramJData = data[nameof(appointmentData.Appointment)];
                if (patientProgramJData.HasValues)
                {
                    appointmentData.Appointment.AppointmentID = (long)patientProgramJData[nameof(AppointmentModel.AppointmentID)];
                }
            }
            if (GenericMethods.IsListNotEmpty(appointmentData.AppointmentDetails))
            {
                appointmentData.AppointmentsDetails = new List<AppointmentDetailModel>();
                foreach (var item in appointmentData.AppointmentDetails)
                {
                    item.PageID = appointmentData.Appointment.AppointmentID;
                    appointmentData.AppointmentsDetails.Add(new AppointmentDetailModel
                    {
                        AppointmentID = item.PageID,
                        AppointmentHeader = item.PageHeading,
                        AppointmentInfo = item.PageData,
                        IsActive = item.IsActive,
                        LanguageID = item.LanguageID
                    });
                }
            }
            foreach (var item in appointmentData.AppointmentParticipants)
            {
                item.AppointmentID = appointmentData.Appointment.AppointmentID;
            }
        }
    }

    /// <summary>
    /// Get Appointments from server
    /// </summary>
    /// <param name="appointmentData">appointmentData reference to return output</param>
    /// <returns>Appointments received from server in contactData</returns>
    public async Task GetAppointmentsAsync(AppointmentDTO appointmentData)
    {
        try
        {
            appointmentData.AppointmentOptions = new List<OptionModel>();
            if (MobileConstants.IsMobilePlatform)
            {
                GetLanguageIDandAccountID(appointmentData);
                await Task.WhenAll(
                    GetFeaturesAsync(AppPermissions.AppointmentAddEdit.ToString(), AppPermissions.AppointmentView.ToString(), AppPermissions.AppointmentsView.ToString(), AppPermissions.PatientAppointmentsView.ToString(),
                    AppPermissions.AppointmentJoin.ToString(), AppPermissions.AppointmentDecline.ToString()),
                    GetResourcesAsync(
                        GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_TASK_STATUS_GROUP, GroupConstants.RS_MENU_ACTION_GROUP,
                        GroupConstants.RS_APPOINTMENT_TYPES_GROUP, GroupConstants.RS_APPOINTMENT_PAGE_GROUP, GroupConstants.RS_YES_NO_TYPE_GROUP),
                    //GetSettingsAsync(GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                    GetSettingsAsync(GroupConstants.RS_APPOINTMENT_PAGE_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                    new CountryDatabase().GetCountriesAsync(appointmentData),
                    new AppointmentDatabase().GetAppointmentsAsync(appointmentData)
                ).ConfigureAwait(false);
                //todo:
                //if (GenericMethods.IsListNotEmpty(appointmentData.Appointments))
                //{
                //    GetDateFormats(out string dayFormat, out string monthFormat);
                //    foreach (var appointment in appointmentData.Appointments)
                //    {
                //        appointment.FromDateTime = _essentials.ConvertToLocalTime(appointment.FromDateTime);
                //        appointment.FromDateString = GenericMethods.GetDateTimeBasedOnCulture(appointment.FromDateTime, DateTimeType.DateTime, dayFormat, monthFormat, string.Empty);
                //        appointment.ToDateTime = _essentials.ConvertToLocalTime(appointment.ToDateTime);
                //        appointment.AppointmentStatusID = IsAppointmentMissed(appointment.ToDateTime, appointment.AppointmentStatusID)
                //            ? ResourceConstants.R_MISSED_STATUS_KEY
                //            : appointment.AppointmentStatusID;
                //        appointment.AppointmentStatusName = LibResources.GetResourceValueByKey(PageData?.Resources, appointment.AppointmentStatusID);
                //        appointment.AppointmentStatusColor = GetAppointmentStatusColor(appointment.AppointmentStatusID);
                //        appointment.AppointmentTypeName = LibResources.GetResourceByKeyID(PageData?.Resources, appointment.AppointmentTypeID).ResourceValue;
                //        appointment.AppointmentTypeImage = appointment.AppointmentTypeName == LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_VIDEO_APPOINTMENT_TYPE_KEY)
                //            ? ImageConstants.I_APPOINTMENT_VIDEO_VISIT_SVG
                //            : ImageConstants.I_APPOINTMENT_CALENDAR_VISIT_SVG;
                //    }
                //}
                //SortPatientAppointments(appointmentData);
                appointmentData.ErrCode = ErrorCode.OK;
            }
            else
            {
                await SyncAppointmentsFromServerAsync(appointmentData, GenericMethods.GetDefaultDateTime, CancellationToken.None).ConfigureAwait(false);
            }
            if (appointmentData.ErrCode == ErrorCode.OK)
            {
                GetDateFormats(out string dayFormat, out string monthFormat);
                if (GenericMethods.IsListNotEmpty(appointmentData.Appointments))
                {
                    foreach (var appointment in appointmentData.Appointments)
                    {
                        appointment.FromDateTime = _essentials.ConvertToLocalTime(appointment.FromDateTime.Value);
                        appointment.ToDateTime = _essentials.ConvertToLocalTime(appointment.ToDateTime.Value);

                        appointment.FromDateString = GenericMethods.GetDateTimeBasedOnCulture(appointment.FromDateTime, DateTimeType.DateTime, dayFormat, monthFormat, string.Empty);
                        appointment.ToDateString = GenericMethods.GetDateTimeBasedOnCulture(appointment.ToDateTime, DateTimeType.DateTime, dayFormat, monthFormat, string.Empty);
                        appointment.AppointmentStatusName = LibResources.GetResourceValueByKey(PageData?.Resources, appointment.AppointmentStatusID);
                        appointmentData.AppointmentOptions.Add(new OptionModel
                        {
                            OptionID = appointment.AppointmentID,
                            OptionText = appointment.PageHeading,
                            ParentOptionText = appointment.AppointmentTypeName,
                            IsDefault = true,
                            GroupName = LibResources.GetResourceValueByKey(appointmentData.Resources, appointment.AppointmentStatusID),
                            From = appointment.FromDateTime.Value.DateTime,
                            To = appointment.ToDateTime.Value.DateTime,
                        });
                    }
                }
                if ((appointmentData.RecordCount == -2 || appointmentData.RecordCount == -1) && appointmentData?.Appointment != null)
                {
                    if (!appointmentData.Appointment.FromDateTime.HasValue)
                    {
                        appointmentData.Appointment.FromDateTime = DateTimeOffset.UtcNow;
                        appointmentData.Appointment.ToDateTime = DateTimeOffset.UtcNow;
                    }
                    appointmentData.Appointment.FromDateTime = _essentials.ConvertToLocalTime(appointmentData.Appointment.FromDateTime.Value);
                    appointmentData.Appointment.ToDateTime = _essentials.ConvertToLocalTime(appointmentData.Appointment.ToDateTime.Value);

                    appointmentData.Appointment.FromDateString = GenericMethods.GetDateTimeBasedOnCulture(appointmentData.Appointment.FromDateTime, DateTimeType.DateTime, dayFormat, monthFormat, string.Empty);
                    appointmentData.Appointment.ToDateString = GenericMethods.GetDateTimeBasedOnCulture(appointmentData.Appointment.ToDateTime, DateTimeType.Time, string.Empty, string.Empty, string.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            appointmentData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    ///// <summary>
    ///// Get Appointment
    ///// </summary>
    ///// <param name="appointmentData">appointmentData reference to return output</param>
    ///// <returns>Contacts received from server in contactData</returns>
    //public async Task GetAppointmentAsync(AppointmentDTO appointmentData)
    //{
    //    try
    //    {
    //        if (MobileConstants.IsMobilePlatform && appointmentData.RecordCount != -1)
    //        {
    //            GetLanguageIDandAccountID(appointmentData);
    //            await Task.WhenAll(
    //                GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_MENU_ACTION_GROUP, GroupConstants.RS_APPOINTMENT_PAGE_GROUP,
    //                    GroupConstants.RS_TASK_STATUS_GROUP, GroupConstants.RS_YES_NO_TYPE_GROUP, GroupConstants.RS_APPOINTMENT_TYPES_GROUP),
    //                GetFeaturesAsync(AppPermissions.AppointmentAddEdit.ToString(), AppPermissions.AppointmentView.ToString(), AppPermissions.AppointmentJoin.ToString(), AppPermissions.AppointmentDecline.ToString()),
    //                GetSettingsAsync(GroupConstants.RS_APPOINTMENT_PAGE_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
    //                new CountryDatabase().GetCountriesAsync(appointmentData),
    //                new AppointmentDatabase().GetAppointmentAsync(appointmentData)).ConfigureAwait(false);
    //            if (appointmentData.Appointment != null)
    //            {
    //                appointmentData.Appointment.AppointmentStatusName = LibResources.GetResourceValueByKey(PageData?.Resources, appointmentData.Appointment.AppointmentStatusID);
    //                appointmentData.Appointment.AppointmentTypeName = LibResources.GetResourceByKeyID(PageData?.Resources, appointmentData.Appointment.AppointmentTypeID)?.ResourceValue;
    //                appointmentData.Appointment.AppointmentStatusColor = GetAppointmentStatusColor(appointmentData.Appointment.AppointmentStatusID);
    //            }
    //            if (GenericMethods.IsListNotEmpty(appointmentData.AppointmentParticipants))
    //            {
    //                foreach (var participant in appointmentData.AppointmentParticipants)
    //                {
    //                    participant.AppointmentStatusID = IsAppointmentMissed(_essentials.ConvertToLocalTime(appointmentData.Appointment.ToDateTime), participant.AppointmentStatusID)
    //                                                     ? ResourceConstants.R_MISSED_STATUS_KEY : participant.AppointmentStatusID;
    //                    participant.AppointmentStatusName = LibResources.GetResourceValueByKey(PageData?.Resources, participant.AppointmentStatusID);
    //                    participant.FullName = $"{participant.FirstName} {participant.LastName}";
    //                    participant.NameInitials = GetInitials(participant.FullName);
    //                    participant.AppointmentStatusColor = GetAppointmentStatusColor(participant.AppointmentStatusID);
    //                    //Todo:
    //                    //participant.ImageSource = !string.IsNullOrWhiteSpace(participant.ImageBase64)
    //                    //    ? ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(participant.ImageBase64))
    //                    //    : null;
    //                }
    //            }
    //        }
    //        else
    //        {
    //            await SyncAppointmentsFromServerAsync(appointmentData, GenericMethods.GetDefaultDateTime, CancellationToken.None).ConfigureAwait(false);
    //            //SInce this data will be used by UI so writing it here
    //            if(appointmentData.RecordCount == -1)
    //            {
    //                GetDateFormats(out string dayFormat, out string monthFormat);
    //                appointmentData.Appointment.FromDateTime = GenericMethods.GetDateTimeAccordingEssentials(appointmentData.Appointment.FromDateTime.ToLocalTime());
    //                appointmentData.Appointment.ToDateTime = GenericMethods.GetDateTimeAccordingEssentials(appointmentData.Appointment.ToDateTime.ToLocalTime());
    //                appointmentData.Appointment.FromDateString = GenericMethods.GetDateTimeBasedOnCulture(appointmentData.Appointment.FromDateTime, DateTimeType.DateTime, dayFormat, monthFormat, string.Empty);
    //                appointmentData.Appointment.ToDateString = GenericMethods.GetDateTimeBasedOnCulture(appointmentData.Appointment.ToDateTime, DateTimeType.Time, string.Empty, string.Empty, string.Empty);

    //            }
    //        }
    //        appointmentData.ErrCode = ErrorCode.OK;
    //    }
    //    catch (Exception ex)
    //    {
    //        appointmentData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
    //        LogError(ex.Message, ex);
    //    }
    //}

    /// <summary>
    /// Maps json object into Model and saves data into DB
    /// </summary>
    /// <param name="result">Object which holds Operation status</param>
    /// <param name="data">json object to fetch data from it</param>
    /// <returns>Operation status with record count</returns>
    internal async Task MapAndSaveAppointmentsAsync(DataSyncModel result, JToken data)
    {
        try
        {
            AppointmentDTO appointmentDtata = new AppointmentDTO();

            GetDateFormats(out string dayFormat, out string monthFormat);
            MapAppointments(data, appointmentDtata);
            await MapParticipantsFromJsonAsync(data, appointmentDtata).ConfigureAwait(false);
            //todo: to verify date string
            MapExternalPaticipant(data, appointmentDtata);
            if (GenericMethods.IsListNotEmpty(appointmentDtata.AppointmentParticipants) || GenericMethods.IsListNotEmpty(appointmentDtata.Appointments) || GenericMethods.IsListNotEmpty(appointmentDtata.ExternalParticipants))
            {
                await new AppointmentDatabase().SaveAppointmentsDataAsync(appointmentDtata).ConfigureAwait(false);
                _ = ImageMappingAsync().ConfigureAwait(false);
                result.RecordCount = appointmentDtata.AppointmentParticipants?.Count ?? 0 + appointmentDtata.Appointments?.Count ?? 0;
            }
            if (MobileConstants.IsMobilePlatform)
            {
                if (GenericMethods.IsListNotEmpty(appointmentDtata.Appointments))
                {
                    await CancelNotificationForAppointment(appointmentDtata, string.Join("','", appointmentDtata.Appointments.Select(x => x.AppointmentID))).ConfigureAwait(false);
                }
                await GenerateNotificationData(null).ConfigureAwait(false);
            }
            result.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            result.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }


    /// <summary>
    /// Maps json object into Model and saves data into DB
    /// </summary>
    /// <param name="result">Object which holds Operation status</param>
    /// <param name="data">json object to fetch data from it</param>
    /// <returns>Operation status with record count</returns>
    internal async Task MapAndSaveAppointmentDetailsAsync(DataSyncModel result, JToken data)
    {
        try
        {
            AppointmentDTO appointmentData = new AppointmentDTO();
            MapAppointmetsDetailsFromJson(data, appointmentData);
            if (GenericMethods.IsListNotEmpty(appointmentData.AppointmentsDetails))
            {
                await new AppointmentDatabase().SaveAppointmentDetailsAsync(appointmentData).ConfigureAwait(false);
                result.RecordCount = appointmentData.AppointmentsDetails.Count;
            }
            result.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            result.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void GetLanguageIDandAccountID(AppointmentDTO appointmentData)
    {
        appointmentData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
        appointmentData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
        if (appointmentData.SelectedUserID < 1)
        {
            appointmentData.SelectedUserID = GetUserID();
        }
        appointmentData.ErrorDescription = GetLoginUserID().ToString();
    }

    private string GetAppointmentStatusColor(string resourceKey)
    {
        if (!string.IsNullOrWhiteSpace(resourceKey))
        {
            return resourceKey.ToLower(CultureInfo.InvariantCulture) == ResourceConstants.R_NEW_STATUS_KEY.ToLower(CultureInfo.InvariantCulture)
              || resourceKey.ToLower(CultureInfo.InvariantCulture) == ResourceConstants.R_ACCEPTED_STATUS_KEY.ToLower(CultureInfo.InvariantCulture)
             || resourceKey.ToLower(CultureInfo.InvariantCulture) == ResourceConstants.R_INPROGRESS_STATUS_KEY.ToLower(CultureInfo.InvariantCulture)
             || resourceKey.ToLower(CultureInfo.InvariantCulture) == ResourceConstants.R_COMPLETED_STATUS_KEY.ToLower(CultureInfo.InvariantCulture)
             ?
             StyleConstants.SUCCESS_COLOR : StyleConstants.ERROR_COLOR;
        }
        return string.Empty;
    }

    private async Task MapAppointmentRecordsAsync(JToken data, AppointmentDTO appointmentData)
    {
        if (appointmentData.RecordCount > -1)
        {
            MapAppointments(data, appointmentData);
        }
        else if (appointmentData.RecordCount == -1)
        {
            appointmentData.CountryCodes = new CountryService(_essentials).MapCountryCodes(data);
            await MapAppointmentAsync(data, appointmentData).ConfigureAwait(false);
        }
        else
        {
            ////Record count = -2
            MapAppointmentFromJson(data, appointmentData);
            await MapParticipantsFromJsonAsync(data, appointmentData).ConfigureAwait(false);
        }
        appointmentData.ErrCode = (ErrorCode)(byte)data[nameof(ContactDTO.ErrCode)];
    }

    private void MapAppointments(JToken data, AppointmentDTO appointmentData)
    {
        appointmentData.Appointments = data[nameof(appointmentData.Appointments)].Any()
            ? (from dataItem in data[nameof(AppointmentDTO.Appointments)]
               select MapAppointment(appointmentData, dataItem)).ToList()
            : new List<AppointmentModel>();
    }

    private string GetAppointmentTypeName(AppointmentDTO appointmentData, JToken dataItem)
    {
        return appointmentData.Resources != null && ((short)dataItem[nameof(AppointmentModel.AppointmentTypeID)]) > 0
            ? appointmentData.Resources.FirstOrDefault(x => x.ResourceKeyID == (short)dataItem[nameof(AppointmentModel.AppointmentTypeID)]).ResourceValue
            : string.Empty;
    }

    private async Task MapAppointmentAsync(JToken data, AppointmentDTO appointmentData)
    {
        MapAppointmentFromJson(data, appointmentData);
        MapAppointmetDetailsFromJson(data, appointmentData);
        MapExternalPaticipant(data, appointmentData);
        //Participants
        if (MobileConstants.IsMobilePlatform)
        {
            await GetResourcesAsync(GroupConstants.RS_TASK_STATUS_GROUP, GroupConstants.RS_APPOINTMENT_TYPES_GROUP).ConfigureAwait(false);
            await MapParticipantsFromJsonAsync(data, appointmentData).ConfigureAwait(false);
            using (LanguageDatabase languagesDB = new LanguageDatabase())
            {
                LanguageDTO languageData = new LanguageDTO();
                await languagesDB.GetLanguagesAsync(languageData).ConfigureAwait(false);
                appointmentData.LanguageTabs = languageData.Languages;
            }
        }
        else
        {
            appointmentData.AppointmentParticipantsDropdown = (from dataItem in data[nameof(appointmentData.AppointmentParticipants)]
                                                               select new OptionModel
                                                               {
                                                                   OptionID = (int)dataItem[nameof(ParticipantsModel.ParticipantID)],
                                                                   OptionText = $"{(string)dataItem[nameof(ParticipantsModel.FirstName)]} {(string)dataItem[nameof(ParticipantsModel.LastName)]} - {(string)dataItem[nameof(ParticipantsModel.RoleName)]}",
                                                                   IsSelected = (bool)dataItem[nameof(ParticipantsModel.IsSelected)],
                                                                   SequenceNo = (long)dataItem[nameof(ParticipantsModel.RoleID)],
                                                               }).ToList();
        }
        appointmentData.AppointmentTypes = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_APPOINTMENT_TYPES_GROUP, string.Empty, false, appointmentData.Appointment.AppointmentTypeID);
    }

    private void MapAppointmetsDetailsFromJson(JToken data, AppointmentDTO appointmentData)
    {
        appointmentData.AppointmentsDetails = (data[nameof(appointmentData.AppointmentDetails)].Any())
            ? (from dataItem in data[nameof(AppointmentDTO.AppointmentDetails)]
               select new AppointmentDetailModel
               {
                   LanguageID = (byte)dataItem[nameof(ContentDetailModel.LanguageID)],
                   LanguageName = (string)dataItem[nameof(ContentDetailModel.LanguageName)],
                   AppointmentHeader = (string)dataItem[nameof(ContentDetailModel.PageHeading)],
                   AppointmentInfo = (string)dataItem[nameof(ContentDetailModel.PageData)],
                   AppointmentID = (long)dataItem[nameof(ContentDetailModel.PageID)],
                   IsActive = (bool)dataItem[nameof(AppointmentModel.IsActive)],
               }).ToList()
            : new List<AppointmentDetailModel>();
    }

    private void MapAppointmetDetailsFromJson(JToken data, AppointmentDTO appointmentData)
    {
        appointmentData.AppointmentDetails = (data[nameof(appointmentData.AppointmentDetails)].Any())
            ? (from dataItem in data[nameof(AppointmentDTO.AppointmentDetails)]
               select new ContentDetailModel
               {
                   LanguageID = (byte)dataItem[nameof(ContentDetailModel.LanguageID)],
                   PageHeading = (string)dataItem[nameof(ContentDetailModel.PageHeading)],
                   PageData = (string)dataItem[nameof(ContentDetailModel.PageData)],
                   PageID = (long)dataItem[nameof(ContentDetailModel.PageID)],
                   IsActive = (bool)dataItem[nameof(AppointmentModel.IsActive)],
                   LanguageName = (string)dataItem[nameof(ContentDetailModel.LanguageName)],
                   PageName = PageType.AppointmentPage
               }).ToList()
            : new List<ContentDetailModel>();
    }
    ///{{   "ParticipantID": 0,   "AccountID": 0,   "AppointmentID": 1359,   "RoleID": 0,   "RoleName": null,   "FirstName": "nila ",   "LastName": "",   "NameInitials": null,   "Profession": "",   "ImageName": "",   "IsDataDownloaded": false,   "ImageBase64": null,   "AppointmentStatusID": "NewStatusKey",   "AppointmentStatusName": null,   "IsSelected": false,   "IsActive": true,   "MobileNo": "+91-4937593475",   "EmailID": "nila@yopmail.com",   "AppointmentStatusColor": null,   "ImageSource": null,   "FullName": null,   "ShowRemoveButton": false,   "ShowRemoveButtonText": null,   "CellFirstMiddleSatusContentHeader": null,   "CellSecondMiddleSatusContentHeader": null,   "CellFirstMiddleContentHeaderColor": null,   "CellSecondMiddleSatusContentHeaderColor": null }}
    private void MapExternalPaticipant(JToken data, AppointmentDTO appointmentData)
    {
        if (MobileConstants.IsMobilePlatform && appointmentData.RecordCount != -1)
        {
            appointmentData.ExternalParticipants = (data[nameof(appointmentData.ExternalParticipants)].Any())
              ? (from dataItem in data[nameof(AppointmentDTO.ExternalParticipants)]
                 select new ParticipantsModel
                 {
                     ParticipantID = (long)dataItem[nameof(ParticipantsModel.ParticipantID)],
                     FirstName = (string)dataItem[nameof(ParticipantsModel.FirstName)],
                     AccountID = (long)dataItem[nameof(ParticipantsModel.AccountID)],
                     AppointmentStatusID = (string)dataItem[nameof(ParticipantsModel.AppointmentStatusID)],
                     AppointmentID = (long)dataItem[nameof(ParticipantsModel.AppointmentID)],
                     IsActive = (bool)dataItem[nameof(ParticipantsModel.IsActive)],
                     MobileNo = (string)dataItem[nameof(ParticipantsModel.MobileNo)],
                     EmailID = (string)dataItem[nameof(ParticipantsModel.EmailID)],
                 }).ToList()
              : new List<ParticipantsModel>();
        }
        else
        {
            JToken appointmentJData = data[nameof(AppointmentDTO.ExternalParticipant)];
            if (appointmentJData.HasValues)
            {

                appointmentData.ExternalParticipant = new ParticipantsModel
                {
                    ParticipantID = (long)appointmentJData[nameof(ParticipantsModel.ParticipantID)],
                    FirstName = (string)appointmentJData[nameof(ParticipantsModel.FirstName)],
                    AccountID = (long)appointmentJData[nameof(ParticipantsModel.AccountID)],
                    AppointmentStatusID = (string)appointmentJData[nameof(ParticipantsModel.AppointmentStatusID)],
                    AppointmentID = (long)appointmentJData[nameof(ParticipantsModel.AppointmentID)],
                    IsActive = (bool)appointmentJData[nameof(ParticipantsModel.IsActive)],
                    MobileNo = (string)appointmentJData[nameof(ParticipantsModel.MobileNo)],
                    EmailID = (string)appointmentJData[nameof(ParticipantsModel.EmailID)],
                };

            }
        }
    }

    private async Task MapParticipantsFromJsonAsync(JToken data, AppointmentDTO appointmentData)
    {
        appointmentData.AppointmentParticipants = (from dataItem in data[nameof(appointmentData.AppointmentParticipants)]
                                                   select new ParticipantsModel
                                                   {
                                                       ParticipantID = (long)dataItem[nameof(ParticipantsModel.ParticipantID)],
                                                       FirstName = (string)dataItem[nameof(ParticipantsModel.FirstName)],
                                                       LastName = (string)dataItem[nameof(ParticipantsModel.LastName)],
                                                       Profession = (string)dataItem[nameof(ParticipantsModel.Profession)],
                                                       RoleName = (string)dataItem[nameof(ParticipantsModel.RoleName)],
                                                       NameInitials = GetInitials($"{(string)dataItem[nameof(ParticipantsModel.FirstName)]} {(string)dataItem[nameof(ParticipantsModel.LastName)]}"),
                                                       FullName = $"{(string)dataItem[nameof(ParticipantsModel.FirstName)]} {(string)dataItem[nameof(ParticipantsModel.LastName)]} {GetUserStatus(dataItem)}",
                                                       RoleID = (byte)dataItem[nameof(ParticipantsModel.RoleID)],
                                                       IsSelected = (bool)dataItem[nameof(ParticipantsModel.IsSelected)],
                                                       //ImageName = (string)dataItem[nameof(ParticipantsModel.ImageName)],
                                                       AccountID = (long)dataItem[nameof(ParticipantsModel.AccountID)],
                                                       AppointmentStatusName = (string)dataItem[nameof(ParticipantsModel.AppointmentStatusName)],
                                                       AppointmentStatusID = !MobileConstants.IsMobilePlatform && appointmentData.Appointment != null
                                                            && IsAppointmentMissed(_essentials.ConvertToLocalTime(appointmentData.Appointment.ToDateTime.Value), (string)dataItem[nameof(ParticipantsModel.AppointmentStatusID)])
                                                                ? ResourceConstants.R_MISSED_STATUS_KEY
                                                                : (string)dataItem[nameof(ParticipantsModel.AppointmentStatusID)],
                                                       AppointmentID = (long)dataItem[nameof(ParticipantsModel.AppointmentID)],
                                                       IsActive = (bool)dataItem[nameof(AppointmentModel.IsActive)],
                                                       CellFirstMiddleSatusContentHeader = LibResources.GetResourceValueByKey(PageData?.Resources, (string)dataItem[nameof(ParticipantsModel.AppointmentStatusID)]),
                                                       CellFirstMiddleContentHeaderColor = GetAppointmentStatusColor((string)dataItem[nameof(ParticipantsModel.AppointmentStatusID)]),
                                                       CellSecondMiddleSatusContentHeader = LibResources.GetResourceValueByKey(PageData?.Resources, (string)dataItem[nameof(ParticipantsModel.AppointmentStatusName)]),
                                                       CellSecondMiddleSatusContentHeaderColor = GetAppointmentStatusColor((string)dataItem[nameof(ParticipantsModel.AppointmentStatusName)])
                                                   }).ToList();

        if (MobileConstants.IsMobilePlatform && appointmentData.RecordCount == -1)
        {
            foreach (var participant in appointmentData.AppointmentParticipants)
            {
                participant.ShowRemoveButtonText = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DELETE_ACTION_KEY);
                participant.ShowRemoveButton = participant.IsSelected;
                if (!string.IsNullOrWhiteSpace(participant.ImageName))
                {
                    participant.ImageName = await GetAppointmentmentBase64ImageAsync(participant).ConfigureAwait(false);
                    //Todo:
                    // participant.ImageSource = ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(participant.ImageName));
                }
            }
        }
        else if (appointmentData.RecordCount == -2)
        {
            foreach (var participant in appointmentData.AppointmentParticipants)
            {
                participant.ImageName = await GetAppointmentmentBase64ImageAsync(participant).ConfigureAwait(false);
            }
        }
    }

    private string GetUserStatus(JToken dataItem)
    {
        string role = (string)dataItem[nameof(ParticipantsModel.RoleName)];
        if (!string.IsNullOrWhiteSpace(role))
        {
            return $" - {role}";
        }
        return string.Empty;
    }

    private void MapAppointmentFromJson(JToken data, AppointmentDTO appointmentData)
    {
        JToken appointmentJData = data[nameof(AppointmentDTO.Appointment)];
        if (appointmentJData.HasValues)
        {
            appointmentData.Appointment = MapAppointment(appointmentData, appointmentJData);
        }
    }

    private AppointmentModel MapAppointment(AppointmentDTO appointmentData, JToken dataItem)
    {
        if ((long)dataItem[nameof(AppointmentModel.AppointmentID)] != 0)
        {
            return new AppointmentModel
            {
                AppointmentID = (long)dataItem[nameof(AppointmentModel.AppointmentID)],
                AppointmentAddress = (string)dataItem[nameof(AppointmentModel.AppointmentAddress)],
                AppointmentStatusID = !MobileConstants.IsMobilePlatform
                    && IsAppointmentMissed(_essentials.ConvertToLocalTime((DateTimeOffset)dataItem[nameof(AppointmentModel.ToDateTime)]), (string)dataItem[nameof(ParticipantsModel.AppointmentStatusID)])
                        ? ResourceConstants.R_MISSED_STATUS_KEY
                        : (string)dataItem[nameof(AppointmentModel.AppointmentStatusID)],
                AppointmentTypeID = GetDataItem<short>(dataItem, nameof(AppointmentModel.AppointmentTypeID)),
                AppointmentTypeName = GetAppointmentTypeName(appointmentData, dataItem),
                FromDateTime = (DateTimeOffset)dataItem[nameof(AppointmentModel.FromDateTime)],
                ToDateTime = (DateTimeOffset)dataItem[nameof(AppointmentModel.ToDateTime)],
                PageHeading = (string)dataItem[nameof(AppointmentModel.PageHeading)],
                PageData = (string)dataItem[nameof(AppointmentModel.PageData)],
                IsInitiator = (bool)dataItem[nameof(AppointmentModel.IsInitiator)],
                AccountID = (long)dataItem[nameof(AppointmentModel.AccountID)],
                IsActive = (bool)dataItem[nameof(AppointmentModel.IsActive)],
            };
        }
        else
        {
            return new AppointmentModel();
        }
    }



    private bool IsAppointmentMissed(DateTimeOffset toDateTime, string statusID)
    {
        return !string.IsNullOrWhiteSpace(statusID) && DateTime.Now > toDateTime
            && statusID != ResourceConstants.R_COMPLETED_STATUS_KEY
            && statusID != ResourceConstants.R_INPROGRESS_STATUS_KEY
            && statusID != ResourceConstants.R_REJECTED_STATUS_KEY;
    }

    private string OptionTextBuilder(long appointmentID, string firstName, string lastName, string roleName, DateTimeOffset toDateTime, string statusID)
    {
        if (appointmentID > 0 && !string.IsNullOrEmpty(statusID))
        {
            statusID = IsAppointmentMissed(toDateTime, statusID) ? ResourceConstants.R_MISSED_STATUS_KEY : statusID;
            string statusName = LibResources.GetResourceValueByKey(PageData?.Resources, statusID);
            return "<span>" + firstName + " " + lastName + "-" + roleName + "&nbsp<b class =" + DetermineAppointmenttStatusColor(statusID) + ">&nbsp" + statusName + "</b></span>";
        }
        else
        {
            return $"{firstName} {lastName} - {roleName}";
        }
    }

    /// <summary>
    /// Determine appointment status badge color
    /// </summary>
    /// <param name="appointmentStatusID">appointment status</param>
    /// <returns>badge style</returns>
    public string DetermineAppointmenttStatusColor(string appointmentStatusID)
    {
        return (appointmentStatusID.ToLower(CultureInfo.InvariantCulture) == ResourceConstants.R_NEW_STATUS_KEY.ToLower(CultureInfo.InvariantCulture)
           || appointmentStatusID.ToLower(CultureInfo.InvariantCulture) == ResourceConstants.R_ACCEPTED_STATUS_KEY.ToLower(CultureInfo.InvariantCulture)
           || appointmentStatusID.ToLower(CultureInfo.InvariantCulture) == ResourceConstants.R_INPROGRESS_STATUS_KEY.ToLower(CultureInfo.InvariantCulture)
           || appointmentStatusID.ToLower(CultureInfo.InvariantCulture) == ResourceConstants.R_COMPLETED_STATUS_KEY.ToLower(CultureInfo.InvariantCulture))
               ? "badge-success "
               : "badge-error";
    }

    private async Task ImageMappingAsync()
    {
        AppointmentDTO appointmentData = new AppointmentDTO { AppointmentParticipants = new List<ParticipantsModel>() };
        await new AppointmentDatabase().GetAppointmentImageStatusAsync(appointmentData).ConfigureAwait(false);
        await GetAppointmentImagesAsync(appointmentData).ConfigureAwait(false);
        await new AppointmentDatabase().UpdateAppointmentSyncImageStatusAsync(appointmentData).ConfigureAwait(false);
    }

    private async Task GetAppointmentImagesAsync(AppointmentDTO appointmentData)
    {
        foreach (ParticipantsModel participant in appointmentData?.AppointmentParticipants)
        {
            participant.ImageName = await GetAppointmentmentBase64ImageAsync(participant).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Get participant profile image
    /// </summary>
    /// <param name="imageName">image name/link</param>
    /// <returns>image base64 data</returns>
    public async Task<string> GetAppointmentmentBase64ImageAsync(ParticipantsModel participant)
    {
        return participant.ImageName != null && participant.ImageName.Contains(Constants.HTTP_TAG_PREFIX)
            ? await GetImageAsBase64Async(participant.ImageName).ConfigureAwait(false)
            : GetInitials(string.Join(Constants.STRING_SPACE, participant.FirstName, participant.LastName));
    }

    private void GetDateFormats(out string dayFormat, out string monthFormat)
    {
        dayFormat = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_DATE_DAY_FORMAT_KEY);
        monthFormat = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_MONTH_FORMAT_KEY);
    }

    private void SortPatientAppointments(AppointmentDTO taskData)
    {
        List<AppointmentModel> newTasks = new List<AppointmentModel>();
        List<AppointmentModel> misssedTasks = new List<AppointmentModel>();
        foreach (var categoryGroup in taskData.Appointments.GroupBy(x => x.AppointmentStatusID).ToList())
        {
            if (GenericMethods.IsListNotEmpty(categoryGroup.ToList()) && (categoryGroup.Key == ResourceConstants.R_NEW_STATUS_KEY))
            {
                newTasks.AddRange(categoryGroup.ToList());
            }
            else
            {
                misssedTasks.AddRange(categoryGroup.ToList());
            }
        }
        var newTaskList = newTasks.OrderBy(x => x.FromDateTime).ToList();
        var missedTaskList = misssedTasks.OrderByDescending(x => x.FromDateTime).ToList();
        taskData.Appointments = new List<AppointmentModel>();
        taskData.Appointments.AddRange(newTaskList);
        taskData.Appointments.AddRange(missedTaskList);
    }

    private async Task GetResourcesAndSettings()
    {
        if (!GenericMethods.IsListNotEmpty(PageData.Resources) || !GenericMethods.IsListNotEmpty(PageData.Settings))
        {
            await Task.WhenAll(
               GetSettingsAsync(GroupConstants.RS_COMMON_GROUP),
               GetResourcesAsync(GroupConstants.RS_APPOINTMENT_PAGE_GROUP, GroupConstants.RS_APPOINTMENT_TYPES_GROUP)
            ).ConfigureAwait(false);
        }
    }

    #region Local Notification

    private async Task GenerateNotificationData(AppointmentDTO appointmentData)
    {
        try
        {
            appointmentData = appointmentData ?? new AppointmentDTO { AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0) };
            await new AppointmentDatabase().GetDataForNotification(appointmentData).ConfigureAwait(false);
            ////await GenerateTestNotification();
            if (GenericMethods.IsListNotEmpty(appointmentData.Appointments))
            {
                appointmentData.Notifications = (from appointment in appointmentData.Appointments.Where(x => x.FromDateTime.Value.AddMinutes(-15) >= GenericMethods.GetUtcDateTime)
                                                 select new LocalNotificationModel
                                                 {
                                                     RecordID = appointment.AppointmentID.ToString(),
                                                     ShowNotificationDateTime = appointment.FromDateTime.Value.AddMinutes(-15),
                                                     IsActive = appointment.IsActive,
                                                     IsSynced = false
                                                 }).ToList();
                await new AppointmentDatabase().SaveNotificationData(appointmentData).ConfigureAwait(false);
                await RegisterNotificationAsync();
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Registers patient appointment notifications
    /// </summary>
    /// <returns></returns>
    public async Task RegisterNotificationAsync()
    {
        try
        {
            AppointmentDTO appointmentData = new AppointmentDTO();
            // Get Notifications data from DB
            await new AppointmentDatabase().GetNotificationData(appointmentData).ConfigureAwait(false);
            if (GenericMethods.IsListNotEmpty(appointmentData.Notifications))
            {
                await GetResourcesAndSettings().ConfigureAwait(false);
                //todo:
                //INotificationService notificationService = DependencyService.Get<INotificationService>();

                //var notificationActions = new List<string> {
                //    LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_SNOOZE_TEXT_KEY),
                //    LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_IGNORE_TEXT_KEY),
                //    LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DONE_TEXT_KEY) };
                //string title = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_APPOINTMENT_NOTIFICATION_TITLE_KEY);
                //string description = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_APPOINTMENT_NOTIFICATION_DESCRIPTION_KEY);
                //string snoozeTimeOut = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_SNOOZE_TIMEOUT_KEY);
                //foreach (var notification in appointmentData.Notifications)
                //{
                //    notificationService.Cancel(notification.NotificationID.Value);
                //    if (notification.IsActive)
                //    {
                //        var appointment = appointmentData.Appointments.FirstOrDefault(x => x.AppointmentID.ToString() == notification.RecordID);
                //        notificationService.Show(CreateNotificationInstance(
                //            notification.NotificationID.Value, notification.ShowNotificationDateTime,
                //            title,
                //            string.Format(description, LibResources.GetResourceByKeyID(PageData?.Resources, appointment.AppointmentTypeID)?.ResourceValue,
                //                LibGenericMethods.GetLocalDateTimeBasedOnCulture(notification.ShowNotificationDateTime.AddMinutes(15), DateTimeType.Time, string.Empty, string.Empty, string.Empty)),
                //            notificationActions, LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_SNOOZE_TIMEOUT_KEY),
                //            Pages.AppointmentsPage.ToString(), Convert.ToString(notification.RecordID, CultureInfo.InvariantCulture),
                //            //// Need to handle multiple user case
                //            0.ToString()));
                //    }
                //    notification.IsSynced = true;
                //}
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
        }
    }

    private async Task CancelNotificationForAppointment(AppointmentDTO appointmentData, string listOfAppointmentIDs)
    {
        await new AppointmentDatabase().GetNotificationDataBasedOnAppointmentIDs(appointmentData, listOfAppointmentIDs).ConfigureAwait(false);
        if (GenericMethods.IsListNotEmpty(appointmentData.Notifications))
        {
            //todo:
            //    INotificationService notificationService = DependencyService.Get<INotificationService>();
            //    foreach (var notification in appointmentData.Notifications)
            //    {
            //        notificationService.Cancel(notification.NotificationID.Value);
            //    }
        }
    }

    private NotificationRequest CreateNotificationInstance(int notificationId, DateTimeOffset dateTime, string title, string description,
        List<string> buttonResources, string snoozemilliSeconds, string targetPage, params string[] parameters)
    {
        var list = new List<string>
        {
            //0, AppointmentListMenuID
            targetPage,
            notificationId.ToString(CultureInfo.InvariantCulture)
        };
        NotificationRequest notificationRequest = new NotificationRequest
        {
            NotificationId = notificationId
        };
        notificationRequest.Title = title;
        notificationRequest.Description = description;
        notificationRequest.IsSnoozeRequest = notificationId < 0;
        //2, Snooze Operation --3
        list.Add(notificationRequest.IsSnoozeRequest.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture));
        var buttons = new List<string>
        {
            buttonResources[0],
            buttonResources[1],
            buttonResources[2]
        };
        var serializer = new ObjectSerializer<List<string>>();
        //3, Notification Buttons --4
        list.Add(serializer.SerializeObject(buttons));
        list.Add(title);
        list.Add(description);
        list.Add(snoozemilliSeconds);
        //7, Appointmnet ID --2
        if (parameters.Length > 0)
        {
            list.AddRange(parameters);
        }
        notificationRequest.ReturningData = serializer.SerializeObject(list);
        notificationRequest.NotifyTime = _essentials.ConvertToLocalTime(dateTime).DateTime;
        return notificationRequest;
    }

    #endregion
}