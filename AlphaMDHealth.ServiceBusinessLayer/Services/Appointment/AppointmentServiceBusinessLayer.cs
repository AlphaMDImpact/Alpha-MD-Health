using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class AppointmentServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Appointment service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public AppointmentServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get Appointments
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="selectedUserID">Selected user's ID for whoe data needs to be retrived</param>
        /// <param name="organisationID">User's organisationID</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="recordCount">Record count to decide how much data to retrive</param>
        /// <param name="appointmentID">Appointment ID to retrive specific contact</param>
        /// <param name="lastModifiedOn">Last Modified on DateTime</param>
        /// <returns>List of Appointments</returns>
        public async Task<AppointmentDTO> GetAppointmentsAsync(byte languageID, long selectedUserID, long organisationID, long permissionAtLevelID, long recordCount, long appointmentID, DateTimeOffset lastModifiedOn)
        {
            AppointmentDTO appointmentData = new AppointmentDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    appointmentData.ErrCode = ErrorCode.InvalidData;
                    return appointmentData;
                }
                if (AccountID < 1)
                {
                    appointmentData.ErrCode = ErrorCode.Unauthorized;
                    return appointmentData;
                }
                appointmentData.AccountID = AccountID;
                appointmentData.LanguageID = languageID;
                appointmentData.OrganisationID = organisationID;
                appointmentData.CountryCodes = (await GetDataFromCacheAsync(CachedDataType.Countries, string.Empty, languageID, default, 0, 0, false).ConfigureAwait(false)).CountryCodes;
                if (await GetSettingsResourcesAsync(appointmentData, true,
                    $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_APPOINTMENT_PAGE_GROUP}",
                    $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_APPOINTMENT_PAGE_GROUP},{GroupConstants.RS_TASK_STATUS_GROUP},{GroupConstants.RS_APPOINTMENT_TYPES_GROUP},{GroupConstants.RS_YES_NO_TYPE_GROUP}").ConfigureAwait(false))
                {
                    appointmentData.SelectedUserID = selectedUserID;
                    appointmentData.RecordCount = recordCount;
                    appointmentData.LastModifiedON = lastModifiedOn;
                    appointmentData.PermissionAtLevelID = permissionAtLevelID;
                    appointmentData.Appointment = new AppointmentModel
                    {
                        AppointmentID = appointmentID
                    };
                    appointmentData.FeatureFor = FeatureFor;
                    await new AppointmentServiceDataLayer().GetAppointmentsAsync(appointmentData).ConfigureAwait(false);
                    await ReplaceAppointmentParticipantsImageCdnLinkAsync(appointmentData);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                appointmentData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return appointmentData;
        }

        /// <summary>
        /// Save Appointment
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="organisationID">User's organisationID</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="appointmentData">Appointment data to be saved</param>
        /// <returns>Result of operation</returns>
        public async Task<AppointmentDTO> SaveAppointmentAsync(byte languageID, long organisationID, long permissionAtLevelID, AppointmentDTO appointmentData)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1 || appointmentData.Appointment == null || appointmentData.Appointment.AppointmentID < 0
                    || string.IsNullOrWhiteSpace(appointmentData.Appointment.AppointmentStatusID) || appointmentData.Appointment.AppointmentTypeID < 0 || IsAppointmentDetailInvalid(appointmentData))
                {
                    appointmentData.ErrCode = ErrorCode.InvalidData;
                    return appointmentData;
                }
                if (appointmentData.IsActive)
                {
                   
                    if(appointmentData.Appointment.AppointmentTypeID <= 0)
                    {
                        appointmentData.ErrCode = ErrorCode.InvalidData;
                        return appointmentData;
                    }
                    appointmentData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(appointmentData, false, string.Empty, $"{GroupConstants.RS_APPOINTMENT_PAGE_GROUP}").ConfigureAwait(false))
                    {
                        if (appointmentData.IsExternalParticipant && appointmentData.ExternalParticipant!=null)
                        {
                            if ((appointmentData.ExternalParticipant.FirstName.Length < appointmentData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_NAME_KEY).MinLength)
                                || appointmentData.ExternalParticipant.FirstName.Length > appointmentData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_NAME_KEY).MaxLength)
                            {
                                appointmentData.ErrCode = ErrorCode.InvalidData;
                                return appointmentData;
                            }
                            else if(GenericMethods.GetMobileNumberWithoutCountryCode(appointmentData.ExternalParticipant.MobileNo).Length < appointmentData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_PHONE_NUMBER_KEY).MinLength
                                || GenericMethods.GetMobileNumberWithoutCountryCode(appointmentData.ExternalParticipant.MobileNo).Length > appointmentData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_PHONE_NUMBER_KEY).MaxLength)
                            {
                                appointmentData.ErrCode = ErrorCode.InvalidData;
                                return appointmentData;
                            }
                            else if (!Regex.IsMatch(appointmentData.ExternalParticipant.EmailID, LibSettings.GetSettingValueByKey(appointmentData.Settings, SettingsConstants.S_EMAIL_REGEX_KEY)))
                            {
                                appointmentData.ErrCode = ErrorCode.InvalidData;
                                return appointmentData;
                            }
                        }
                        if (!await ValidateDataAsync(appointmentData.Appointment, appointmentData.Resources))
                        {
                            appointmentData.ErrCode = ErrorCode.InvalidData;
                            return appointmentData;
                        }
                        else if (!await ValidateDataAsync(appointmentData.AppointmentDetails, appointmentData.Resources))
                        {
                            appointmentData.ErrCode = ErrorCode.InvalidData;
                            return appointmentData;
                        }
                    }
                    else
                    {
                        return appointmentData;
                    }
                }
                appointmentData.AccountID = AccountID;
                appointmentData.PermissionAtLevelID = permissionAtLevelID;
                appointmentData.OrganisationID = organisationID;
                appointmentData.LanguageID = languageID;
                bool isAddAppointment = appointmentData.Appointment.AppointmentID == 0;
                appointmentData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_APPOINTMENT_TYPES_GROUP}", languageID, default, appointmentData.AccountID, 0, false).ConfigureAwait(false)).Resources;
                await GetAppointmetAddressForVideoCallAsync(appointmentData);
                if (appointmentData.ErrCode == ErrorCode.OK)
                {
                    appointmentData.FeatureFor = FeatureFor;
                    await new AppointmentServiceDataLayer().SaveAppointmentAsync(appointmentData).ConfigureAwait(true);
                    if (appointmentData.ErrCode == ErrorCode.OK)
                    {
                        int videoAppointmentKeyID = appointmentData.Resources.First(x => x.ResourceKey == ResourceConstants.R_VIDEO_APPOINTMENT_TYPE_KEY).ResourceKeyID;
                        if (appointmentData.IsExternalParticipant)
                        {
                            await SendCommunication(true, organisationID, appointmentData.Appointment.AppointmentID, appointmentData.ExternalParticipant.FirstName, appointmentData.ExternalParticipant.MobileNo, appointmentData.ExternalParticipant.EmailID, appointmentData.Appointment.AppointmentTypeID, videoAppointmentKeyID, appointmentData.LocaltoUtcTimeInSeconds, appointmentData.Appointment.FromDateString).ConfigureAwait(false);
                        }
                        foreach (var participent in appointmentData.InviteParticipants)
                        {
                            await SendCommunication(false, organisationID, appointmentData.Appointment.AppointmentID, participent.FirstName, participent.MobileNo, participent.EmailID, appointmentData.Appointment.AppointmentTypeID, videoAppointmentKeyID, appointmentData.LocaltoUtcTimeInSeconds, appointmentData.Appointment.FromDateString).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                appointmentData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return appointmentData;
        }

        private async Task SendCommunication(bool isExternal, long organisationID, long appointmentID, string userName, string phoneNo, string emailID, short appoitmentType, int videoAppointmentKeyID, int localtoUtcTimeDiffInSeconds, string communicationDateTime)
        {
            TemplateDTO communicationDto = new TemplateDTO();
            communicationDto.TemplateData = new TemplateModel();
            communicationDto.AccountID = AccountID;
            communicationDto.OrganisationID = organisationID;
            communicationDto.TemplateData.DataRecordPrimaryKey = appointmentID.ToString();
            communicationDto.LocaltoUtcTimeInSeconds = localtoUtcTimeDiffInSeconds;
            communicationDto.TemplateData.CommunicationDateTime = communicationDateTime;
            if (isExternal)
            {
                communicationDto.TemplateData.IsExternal = true;
                communicationDto.TemplateData.TemplateName = appoitmentType == videoAppointmentKeyID ? TemplateName.EVirtualAppointmentForExternal : TemplateName.EPhysicalAppointmentForExternal;
                communicationDto.TemplateData.ExternalUserName = userName;
                communicationDto.TemplateData.ExternalEmailID = emailID;
                communicationDto.TemplateData.ExternalMobileNo = phoneNo;
            }
            else
            {
                communicationDto.TemplateData.IsExternal = false;
                communicationDto.PhoneNumber = phoneNo;
                communicationDto.EmailID = emailID;
                communicationDto.TemplateData.TemplateName = appoitmentType == videoAppointmentKeyID ? TemplateName.EVirtualAppointment : TemplateName.EPhysicalAppointment;
            }
            await SendCommunicationAsync(communicationDto).ConfigureAwait(false);
        }

        private async Task GetAppointmetAddressForVideoCallAsync(AppointmentDTO appointmentData)
        {
            if (appointmentData.Appointment.AppointmentTypeID == appointmentData.Resources.First(x => x.ResourceKey == ResourceConstants.R_VIDEO_APPOINTMENT_TYPE_KEY).ResourceKeyID && string.IsNullOrWhiteSpace(appointmentData.Appointment.AppointmentAddress))
            {
                VideoDTO video = new VideoDTO();
                video.OrganisationID = appointmentData.OrganisationID;
                await CreateSessionForVideoCallAsync(video);
                appointmentData.ErrCode = video.ErrCode;
                if (video.ErrCode == ErrorCode.OK)
                {
                    appointmentData.Appointment.AppointmentAddress = video.Video.VideoRoomID;
                }
            }
        }

        /// <summary>
        /// Update appointment status
        /// </summary>
        /// <param name="organizationID">Organization id</param>
        /// <param name="appointmentData">Appointment data to be saved</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <returns>Result of operation</returns>
        public async Task<AppointmentDTO> UpdateAppointmentStatusAsync(long organizationID, AppointmentDTO appointmentData, long permissionAtLevelID)
        {
            try
            {
                if (permissionAtLevelID < 1 || appointmentData.Appointment == null || appointmentData.Appointment.AppointmentID < 1 || string.IsNullOrWhiteSpace(appointmentData.Appointment.AppointmentStatusID))
                {
                    appointmentData.ErrCode = ErrorCode.InvalidData;
                    return appointmentData;
                }
                if (AccountID < 1)
                {
                    appointmentData.ErrCode = ErrorCode.Unauthorized;
                    return appointmentData;
                }
                appointmentData.AccountID = AccountID;
                appointmentData.PermissionAtLevelID = permissionAtLevelID;
                appointmentData.FeatureFor = FeatureFor;
                await new AppointmentServiceDataLayer().UpdateAppointmentStatusAsync(appointmentData).ConfigureAwait(false);
                if (GenericMethods.IsListNotEmpty(appointmentData.AppointmentParticipants))
                {
                    TemplateDTO communicationDto = new TemplateDTO();
                    foreach (ParticipantsModel participant in appointmentData.AppointmentParticipants)
                    {
                        communicationDto.TemplateData = new TemplateModel();
                        communicationDto.AccountID = AccountID;
                        communicationDto.OrganisationID = organizationID;
                        communicationDto.EmailID = participant.EmailID;
                        communicationDto.TemplateData.ExternalEmailID = participant.EmailID;
                        communicationDto.PhoneNumber = participant.MobileNo;
                        communicationDto.TemplateData.ExternalMobileNo = participant.MobileNo;
                        communicationDto.TemplateData.DataRecordPrimaryKey = participant.AppointmentID.ToString();
                        communicationDto.TemplateData.TemplateName = TemplateName.EAppointmentCancelled;
                        communicationDto.TemplateData.IsExternal = participant.IsActive;
                        communicationDto.LocaltoUtcTimeInSeconds = appointmentData.LocaltoUtcTimeInSeconds;
                        await SendCommunicationAsync(communicationDto).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                appointmentData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return appointmentData;
        }

        internal async Task ReplaceAppointmentParticipantsImageCdnLinkAsync(AppointmentDTO appointmentData)
        {
            if (GenericMethods.IsListNotEmpty(appointmentData.AppointmentParticipants))
            {
                BaseDTO cdnCacheData = new BaseDTO();
                foreach (ParticipantsModel participant in appointmentData.AppointmentParticipants)
                {
                    participant.FullName = $"{participant.FirstName} {participant.LastName}";
                    if (!string.IsNullOrWhiteSpace(participant.ImageName))
                    {
                        participant.ImageName = await ReplaceCDNLinkAsync(participant.ImageName, cdnCacheData);
                    }
                }
                appointmentData.AppointmentParticipants = appointmentData.AppointmentParticipants.OrderBy(x => x.RoleName).ThenBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            }
        }

        private bool IsAppointmentDetailInvalid(AppointmentDTO appointmentData)
        {
            if (GenericMethods.IsListNotEmpty(appointmentData.AppointmentDetails))
            {
                return appointmentData.AppointmentDetails.Any(x => string.IsNullOrWhiteSpace(x.PageHeading));
            }
            return true;
        }
    }
}
