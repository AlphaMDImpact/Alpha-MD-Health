using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public partial class BaseServiceBusinessLayer
    {
        /// <summary>
        /// Get pending communication data
        /// </summary>
        /// <returns>Operation status and communication template</returns>
        public async Task<TemplateDTO> SendPendingCommunicationAsync()
        {
            TemplateDTO communicationDto = new TemplateDTO();
            try
            {
                await new BaseServiceDataLayer().GetPendingCommunicationAsync(communicationDto).ConfigureAwait(false);
                if (communicationDto.ErrCode == ErrorCode.OK && communicationDto.Templates?.Count > 0)
                {
                    foreach (var template in communicationDto.Templates)
                    {
                        if (!string.IsNullOrWhiteSpace(template.Attachments))
                        {
                            await DownloadCommunicationDocumentAsync(communicationDto).ConfigureAwait(false);
                        }
                        else
                        {
                            template.Attachments = string.Empty;
                        }
                        _ = StartCommunicationAsync(new TemplateDTO { TemplateData = template }, false).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                communicationDto.ErrCode = ErrorCode.InternalServerError;
                LogError(ex.Message, ex);
            }
            return communicationDto;
        }

        /// <summary>
        /// Send communication data based on input types
        /// </summary>
        /// <param name="communication">Object containing communication data to send</param>
        /// <returns>error code using communication dto</returns>
        public async Task SendCommunicationAsync(TemplateDTO communication)
        {
            try
            {
                string communicationDateTime = communication?.TemplateData?.CommunicationDateTime;
                TemplateModel tempTemplate = communication.TemplateData;
                await new BaseServiceDataLayer().GetTemplateDataAsync(communication).ConfigureAwait(false);
                if (communication.ErrCode == ErrorCode.OK)
                {
                    communication.TemplateData.CommunicationType = communication.Response;
                    //if (communicationDto.TemplateData.AddedON != null)
                    //{
                    //    DateTimeOffset newDateTime = communicationDto.TemplateData.AddedON?.AddSeconds(communicationDto.LocaltoUtcTimeInSeconds) ?? DateTime.Now;
                    communication.TemplateData.CommunicationDateTime = communicationDateTime;
                    //}
                    communication.TemplateData.RegistrationLink = tempTemplate.RegistrationLink;
                    communication.TemplateData.IsExternal = tempTemplate.IsExternal;
                    if (communication.TemplateData.IsExternal)
                    {
                        communication.TemplateData.ToUserName = string.IsNullOrWhiteSpace(tempTemplate.ExternalUserName) ? communication.TemplateData.ToUserName : tempTemplate.ExternalUserName;
                        communication.TemplateData.ToId = tempTemplate.ExternalEmailID;
                        communication.TemplateData.ToPhoneNo = tempTemplate.ExternalMobileNo;
                    }
                    else
                    {
                        communication.TemplateData.ToUserName = string.IsNullOrWhiteSpace(communication.TemplateData.ToUserName) ? communication.Username : communication.TemplateData.ToUserName;
                    }
                    _ = StartCommunicationAsync(communication, true);
                    //await StartCommunicationAsync(communicationDto, true);
                }
            }
            catch (Exception ex)
            {
                communication.ErrCode = ErrorCode.InternalServerError;
                LogError(ex.Message, ex);
            }
        }

        private async Task StartCommunicationAsync(TemplateDTO communicationData, bool createContent)
        {
            try
            {
                //marking all status as success first 
                communicationData.TemplateData.IsEmailSent = true;
                communicationData.TemplateData.IsSMSSent = true;
                communicationData.TemplateData.IsWhatsAppSent = true;
                communicationData.TemplateData.IsNotificationSent = true;
                await SendEmailAsync(communicationData, createContent).ConfigureAwait(false);
                //await SendWhatsAppAsync(communicationData, createContent).ConfigureAwait(false);
                if (Constants.DEFAULT_ENVIRONMENT_KEY == Constants.PROD1_KEY)
                {
                    await SendSmsAsync(communicationData, createContent).ConfigureAwait(false);
                    await SendWhatsAppAsync(communicationData, createContent).ConfigureAwait(false);
                    await SendNotificationAsync(communicationData, createContent).ConfigureAwait(false);
                }
                if (communicationData.Attachments == null || communicationData.Attachments.Count < 1)
                {
                    communicationData.TemplateData.Attachments = string.Empty;
                }
                else
                {
                    await UploadCommunicationDocumentAsync(communicationData).ConfigureAwait(false);
                }
                await new BaseServiceDataLayer().SavePendingCommunicationAsync(communicationData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                communicationData.ErrCode = ErrorCode.InternalServerError;
                LogError(ex.Message, ex);
            }
        }

        private async Task UploadCommunicationDocumentAsync(TemplateDTO communicationData)
        {
            FileUploadDTO files = CreateSingleFileDataObject(FileTypeToUpload.ExternalCommunicationFiles,
                communicationData.Attachments.FirstOrDefault().RecordID,
                communicationData.Attachments.FirstOrDefault().Base64File);
            files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
            communicationData.ErrCode = files.ErrCode;
            if (files.ErrCode == ErrorCode.OK)
            {
                communicationData.TemplateData.Attachments = GetFirstBase64File(files);
            }
        }

        private async Task DownloadCommunicationDocumentAsync(TemplateDTO communicationData)
        {
            communicationData.Attachments = new List<FileDataModel>();
            communicationData.Attachments.Add(await GetImageAsBase64Async(communicationData.TemplateData.Attachments).ConfigureAwait(false));
        }

        private async Task<FileDataModel> GetImageAsBase64Async(string cdnLink)
        {
            var file = new FileDataModel { RecordID = cdnLink };
            cdnLink = await ReplaceCDNLinkAsync(cdnLink, null);
            if (!string.IsNullOrWhiteSpace(cdnLink))
            {
                using (var client = new HttpClient())
                {
                    var bytes = await client.GetByteArrayAsync(new Uri(cdnLink)).ConfigureAwait(false);
                    var prefix = GenericMethods.GetImagePrefix(Path.GetExtension(cdnLink));
                    file.Base64File = "data:" + prefix + Convert.ToBase64String(bytes);
                }
            }
            return file;
        }

        private async Task SendSmsAsync(TemplateDTO smsData, bool createContent)
        {
            try
            {
                if (GetTemplateTypeSupport(smsData, TemplateTypes.SMS, createContent))
                {
                    var settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, 0, default, 0, smsData.OrganisationID, false).ConfigureAwait(false)).Settings;
                    if (GenericMethods.IsListNotEmpty(settings))
                    {
                        string clientIdentifier = GetSettingValueByKey(settings, SettingsConstants.S_SMS_MICRO_SERVICE_KEY);
                        if (createContent)
                        {
                            await CreateMessageHeaderAndBodyAsync(smsData, TemplateTypes.SMS);
                        }
                        CommunicationDTO communication = new CommunicationDTO
                        {
                            MessageSubject = smsData.TemplateData.SmsHeader,
                            MessageBody = smsData.TemplateData.SmsBody,
                            PhoneNumber = string.IsNullOrWhiteSpace(smsData.TemplateData.ToPhoneNo) ? smsData.Username : smsData.TemplateData.ToPhoneNo,
                            FromId = smsData.TemplateData.ServiceTemplateID
                        };
                        var httpData = new HttpServiceModel<CommunicationDTO>
                        {
                            BaseUrl = new Uri(UrlConstants.MICRO_SERVICE_PATH),
                            PathWithoutBasePath = UrlConstants.SEND_SMS_MESSAGE_ASYNC,
                            AuthType = AuthorizationType.Basic,
                            ClientIdentifier = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[1],
                            ClientSecret = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[2],
                            ForApplication = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[0],
                            ContentToSend = communication
                        };
                        await new HttpLibService(new HttpService()).PostAsync(httpData).ConfigureAwait(false);
                        smsData.ErrCode = httpData.ErrCode;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                smsData.ErrCode = ErrorCode.InternalServerError;
            }
            smsData.TemplateData.IsSMSSent = smsData.ErrCode == ErrorCode.OK;
        }

        private async Task SendNotificationAsync(TemplateDTO notificationData, bool createContent)
        {
            return;
            try
            {
                if (notificationData.TemplateData != null && GetTemplateTypeSupport(notificationData, TemplateTypes.PushNotification, createContent))
                {
                    if (createContent)
                    {
                        await CreateNotificationContentAsync(notificationData);
                    }
                    NotificationDTO notificationMessage = new NotificationDTO
                    {
                        NotificationMessage = new NotificationMessageModel
                        {
                            NotificationTitle = notificationData.TemplateData.AlertHeader,
                            NotificationBody = notificationData.TemplateData.AlertBody,
                            NotificationCategory = notificationData.TemplateData.TemplateName.ToString(),
                            NotificationCategoryID = notificationData.TemplateData.CommunicationType,
                            NotificationTags = notificationData.NotificationTags,
                            BadgeCount = 1
                        }
                    };
                    var result = await new PushNotificationServiceBusinessLayer(null).SendNotificationAsync(notificationMessage, notificationData.LanguageID, notificationData.OrganisationID);
                    notificationData.ErrCode = result.ErrCode;
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                notificationData.ErrCode = ErrorCode.InternalServerError;
            }
            notificationData.TemplateData.IsNotificationSent = notificationData.ErrCode == ErrorCode.OK;
        }

        private async Task SendEmailAsync(TemplateDTO emailData, bool createContent)
        {
            try
            {
                if (GetTemplateTypeSupport(emailData, TemplateTypes.Email, createContent))
                {
                    var settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, string.Empty, 0, default, 0, emailData.OrganisationID, false).ConfigureAwait(false)).Settings;
                    if (GenericMethods.IsListNotEmpty(settings))
                    {
                        string clientIdentifier = GetSettingValueByKey(settings, SettingsConstants.S_COMMUNICATION_EMAIL_MICRO_SERVICE_KEY);
                        emailData.TemplateData.HeaderBackGroundColor = GetSettingValueByKey(settings, SettingsConstants.S_HEADER_BACKGROUND_COLOR_KEY);
                        emailData.TemplateData.OrganisationLogo = GetSettingValueByKey(settings, SettingsConstants.S_LOGO_KEY);
                        emailData.TemplateData.HeaderFontColor = GetSettingValueByKey(settings, SettingsConstants.S_HEADER_TEXT_COLOR_KEY);
                        emailData.TemplateData.FooterBackGroundColor = GetSettingValueByKey(settings, SettingsConstants.S_FOOTER_BACKGROUND_COLOR_KEY);
                        emailData.TemplateData.FooterFontColor = GetSettingValueByKey(settings, SettingsConstants.S_FOOTER_TEXT_COLOR_KEY);
                        emailData.TemplateData.AppName = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[3];
                        if (createContent)
                        {
                            await CreateMessageHeaderAndBodyAsync(emailData, TemplateTypes.Email);
                        }
                        CommunicationDTO communication = new CommunicationDTO
                        {
                            MessageSubject = emailData.TemplateData.EmailHeader,
                            MessageBody = emailData.TemplateData.EmailBody,
                            MessageAttachments = emailData.Attachments,
                            ToIds = new List<string> { emailData.TemplateData.ToId },
                            FromId = emailData.TemplateData.FromId,
                            ApplicationName = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[0],
                        };
                        if (!string.IsNullOrWhiteSpace(emailData.TemplateData.BccIds))
                        {
                            communication.BccIds = GetEmailIds(emailData.TemplateData.BccIds);
                        }
                        if (!string.IsNullOrWhiteSpace(emailData.TemplateData.CcIds))
                        {
                            communication.CcIds = GetEmailIds(emailData.TemplateData.CcIds);
                        }
                        var httpData = new HttpServiceModel<CommunicationDTO>
                        {
                            CancellationToken = new CancellationToken(),
                            BaseUrl = new Uri(UrlConstants.MICRO_SERVICE_PATH),
                            PathWithoutBasePath = UrlConstants.SEND_EMAIL_MESSAGE_ASYNC,
                            AuthType = AuthorizationType.Basic,
                            ClientIdentifier = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[1],
                            ClientSecret = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[2],
                            ForApplication = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[0],
                            ContentToSend = communication
                        };
                        await new HttpLibService(new HttpService()).PostAsync(httpData).ConfigureAwait(false);
                        emailData.ErrCode = httpData.ErrCode;
                    }
                    else
                    {
                        emailData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                emailData.ErrCode = ErrorCode.InternalServerError;
            }
            emailData.TemplateData.IsEmailSent = emailData.ErrCode == ErrorCode.OK;
        }

        private async Task SendWhatsAppAsync(TemplateDTO whatsAppData, bool createContent)
        {
            try
            {
                if (GetTemplateTypeSupport(whatsAppData, TemplateTypes.WhatsApp, createContent))
                {
                    var settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, string.Empty, 0, default, 0, whatsAppData.OrganisationID, false).ConfigureAwait(false)).Settings;
                    if (GenericMethods.IsListNotEmpty(settings))
                    {
                        string clientIdentifier = GetSettingValueByKey(settings, SettingsConstants.S_COMMUNICATION_WHATS_APP_MICRO_SERVICE_KEY);
                        whatsAppData.TemplateData.HeaderBackGroundColor = GetSettingValueByKey(settings, SettingsConstants.S_HEADER_BACKGROUND_COLOR_KEY);
                        whatsAppData.TemplateData.OrganisationLogo = GetSettingValueByKey(settings, SettingsConstants.S_LOGO_KEY);
                        whatsAppData.TemplateData.HeaderFontColor = GetSettingValueByKey(settings, SettingsConstants.S_HEADER_TEXT_COLOR_KEY);
                        whatsAppData.TemplateData.FooterBackGroundColor = GetSettingValueByKey(settings, SettingsConstants.S_FOOTER_BACKGROUND_COLOR_KEY);
                        whatsAppData.TemplateData.FooterFontColor = GetSettingValueByKey(settings, SettingsConstants.S_FOOTER_TEXT_COLOR_KEY);
                        whatsAppData.TemplateData.AppName = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[3];
                        if (createContent)
                        {
                            await CreateMessageHeaderAndBodyAsync(whatsAppData, TemplateTypes.WhatsApp);
                        }
                        CommunicationDTO communication = new CommunicationDTO
                        {
                            MessageSubject = whatsAppData.TemplateData.WhatsAppHeader,
                            MessageBody = whatsAppData.TemplateData.WhatsAppBody,
                            MessageAttachments = whatsAppData.Attachments,
                            PhoneNumber = whatsAppData.TemplateData.ToPhoneNo,
                            ToIds = new List<string> { whatsAppData.TemplateData.ToId },
                            FromId = whatsAppData.TemplateData.FromId,
                            ApplicationName = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[0],
                        };
                        var httpData = new HttpServiceModel<CommunicationDTO>
                        {
                            CancellationToken = new CancellationToken(),
                            BaseUrl = new Uri(UrlConstants.MICRO_SERVICE_PATH),
                            PathWithoutBasePath = UrlConstants.SEND_WHATS_APP_MESSAGE_ASYNC,
                            AuthType = AuthorizationType.Basic,
                            ClientIdentifier = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[1],
                            ClientSecret = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[2],
                            ForApplication = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[0],
                            ContentToSend = communication
                        };
                        await new HttpLibService(new HttpService()).PostAsync(httpData).ConfigureAwait(false);
                        whatsAppData.ErrCode = httpData.ErrCode;
                    }
                    else
                    {
                        whatsAppData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                whatsAppData.ErrCode = ErrorCode.InternalServerError;
            }
            whatsAppData.TemplateData.IsWhatsAppSent = whatsAppData.ErrCode == ErrorCode.OK;
        }

        private bool GetTemplateTypeSupport(TemplateDTO templateData, TemplateTypes lookSupportFor, bool createContent)
        {
            if (createContent)
            {
                if (lookSupportFor == TemplateTypes.Email && templateData.TemplateData.TemplateType.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                else if (lookSupportFor == TemplateTypes.SMS && templateData.TemplateData.TemplateType.IndexOf("Y", 1, 1, StringComparison.InvariantCultureIgnoreCase) == 1)
                {
                    return true;
                }
                else if (lookSupportFor == TemplateTypes.PushNotification && templateData.TemplateData.TemplateType.IndexOf("Y", 2, 1, StringComparison.InvariantCultureIgnoreCase) == 1)
                {
                    return true;
                }
                else if (lookSupportFor == TemplateTypes.WhatsApp && templateData.TemplateData.TemplateType.EndsWith("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            else
            {
                if (lookSupportFor == TemplateTypes.Email && !string.IsNullOrWhiteSpace(templateData.TemplateData.EmailBody))
                {
                    return true;
                }
                else if (lookSupportFor == TemplateTypes.SMS && !string.IsNullOrWhiteSpace(templateData.TemplateData.SmsBody))
                {
                    return true;
                }
                else if (lookSupportFor == TemplateTypes.PushNotification && !string.IsNullOrWhiteSpace(templateData.TemplateData.AlertBody))
                {
                    return true;
                }
                else if (lookSupportFor == TemplateTypes.WhatsApp && !string.IsNullOrWhiteSpace(templateData.TemplateData.WhatsAppBody))
                {
                    return true;
                }
            }
            return false;
        }

        private async Task CreateMessageHeaderAndBodyAsync(TemplateDTO communicationData, TemplateTypes lookSupportFor)
        {
            string templateHeader = string.Empty;
            string templateBody = string.Empty;
            if (lookSupportFor == TemplateTypes.Email)
            {
                templateHeader = communicationData.TemplateData.EmailHeader;
                templateBody = communicationData.TemplateData.EmailBody;
                var result = await ReplaceConstantsWithValues(communicationData, templateHeader, templateBody).ConfigureAwait(false);
                communicationData.TemplateData.EmailHeader = result.Item1;
                communicationData.TemplateData.EmailBody = result.Item2;
            }
            else if (lookSupportFor == TemplateTypes.WhatsApp)
            {
                templateBody = communicationData.TemplateData.WhatsAppBody;
                var result = await ReplaceConstantsWithValues(communicationData, templateHeader, templateBody).ConfigureAwait(false);
                communicationData.TemplateData.WhatsAppBody = result.Item2;
            }
            else if (lookSupportFor == TemplateTypes.SMS)
            {
                templateBody = communicationData.TemplateData.SmsBody;
                var result = await ReplaceConstantsWithValues(communicationData, templateHeader, templateBody).ConfigureAwait(false);
                communicationData.TemplateData.SmsBody = result.Item2;
            }
            else if (lookSupportFor == TemplateTypes.PushNotification)
            {
                templateHeader = communicationData.TemplateData.AlertHeader;
                templateBody = communicationData.TemplateData.AlertBody;
                var result = await ReplaceConstantsWithValues(communicationData, templateHeader, templateBody).ConfigureAwait(false);
                communicationData.TemplateData.AlertHeader = result.Item1;
                communicationData.TemplateData.AlertBody = result.Item2;
            }
        }

        private async Task<(string, string)> ReplaceConstantsWithValues(TemplateDTO communicationData, string templateHeader, string templateBody)
        {
            if (!string.IsNullOrWhiteSpace(templateHeader))
            {
                templateHeader = templateHeader
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.UserName), communicationData.TemplateData.ToUserName, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.Otp), communicationData.TemplateData.Otp, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.AppName), communicationData.TemplateData.AppName, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.CompanyName), communicationData.TemplateData.CompanyName, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.ExternalUserName), communicationData.TemplateData.ExternalUserName, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.DoctorName), communicationData.TemplateData.DoctorName, StringComparison.InvariantCultureIgnoreCase);
            }
            if (!string.IsNullOrWhiteSpace(templateBody) && templateBody != "NULL")
            {
                templateBody = templateBody
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.UserName), communicationData.TemplateData.ToUserName, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.Otp), communicationData.TemplateData.Otp, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.AppName), communicationData.TemplateData.AppName, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.CompanyName), communicationData.TemplateData.CompanyName, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.HeaderBackGroundColor), communicationData.TemplateData.HeaderBackGroundColor, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.HeaderFontColor), communicationData.TemplateData.HeaderFontColor, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.FooterBackGroundColor), communicationData.TemplateData.FooterBackGroundColor, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.FooterFontColor), communicationData.TemplateData.FooterFontColor, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.ExternalUserName), communicationData.TemplateData.ExternalUserName, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.DoctorName), communicationData.TemplateData.DoctorName, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.PatientName), communicationData.TemplateData.PatientName, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.CompanyAddress), communicationData.TemplateData.CompanyAddress, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateDTO.AddedON), communicationData.TemplateData.CommunicationDateTime, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateModel.OrganisationLogo), communicationData.TemplateData.OrganisationLogo, StringComparison.InvariantCultureIgnoreCase)
                    .Replace(Constants.SYMBOL_AT_THE_RATE + nameof(TemplateDTO.TemplateData.RegistrationLink), communicationData.TemplateData.RegistrationLink, StringComparison.InvariantCultureIgnoreCase);
                //replace the images URL's with actual CDN links
                templateBody = await ReplaceCDNLinkAsync(templateBody, new BaseDTO());
            }
            return (templateHeader, templateBody);
        }

        private async Task<string> FormattedDate(DateTimeOffset inputDate, long organisationID)
        {
            var settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, string.Empty, 0, default, 0, organisationID, false).ConfigureAwait(false)).Settings;
            if (GenericMethods.IsListNotEmpty(settings))
            {
                LibSettings.TryGetDateFormatSettings(settings, out string dayFormat, out string monthFormat, out string yearFormat);
                return GenericMethods.GetDateTimeBasedOnCulture(inputDate, DateTimeType.DateTime, dayFormat, monthFormat, yearFormat);
            }
            return null;
        }

        private async Task CreateNotificationContentAsync(TemplateDTO communicationData)
        {
            if (!string.IsNullOrWhiteSpace(communicationData.TemplateData.AlertBody) && communicationData.TemplateData.AlertBody != "NULL" && communicationData.UserData != null)
            {
                foreach (var item in communicationData.UserData)
                {
                    if (communicationData.TemplateData.AlertBody.Contains(item.Key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        communicationData.TemplateData.AlertBody = communicationData.TemplateData.AlertBody.Replace(item.Key, item.Value, StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }
        }

        private List<string> GetEmailIds(string emailIds)
        {
            return emailIds.Split(Constants.SYMBOL_COMMA).ToList();
        }
    }
}