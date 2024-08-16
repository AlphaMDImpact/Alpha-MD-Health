using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class ContactsService : BaseService
    {
        public ContactsService(IEssentials serviceEssentials) : base(serviceEssentials)
        {

        }
        /// <summary>
        /// Save PatientContacts for mobile or tablet
        /// </summary>
        /// <param name="result">object to return operation status</param>
        /// <returns>Operation status call</returns>
        internal async Task MapAndSavePatientContactsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                ContactDTO contactData = new ContactDTO
                {
                    Contacts = MapContacts(data, nameof(DataSyncDTO.PatientContacts))
                };
                if (GenericMethods.IsListNotEmpty(contactData.Contacts))
                {
                    await new ContactDatabase().SaveContactsAsync(contactData).ConfigureAwait(false);
                    result.RecordCount = contactData.Contacts?.Count ?? 0;
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
        /// Save Patient Contact details for mobile or tablet
        /// </summary>
        /// <param name="result">object to return operation status</param>
        /// <returns>Operation status call</returns>
        internal async Task MapAndSavePatientContactDetailsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                ContactDTO contactData = new ContactDTO
                {
                    PatientContactDetails = MapContactDetails(data, nameof(DataSyncDTO.PatientContactDetails))
                };
                if (GenericMethods.IsListNotEmpty(contactData.PatientContactDetails))
                {
                    await new ContactDatabase().SaveContactDetailsAsync(contactData).ConfigureAwait(false);
                    result.RecordCount = contactData.ContactDetails?.Count ?? 0;
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
        /// Sync Contact data to server
        /// </summary>
        /// <param name="contactData">object to return operation status</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status call</returns>
        internal async Task SyncContactsToServerAsync(ContactDTO contactData, CancellationToken cancellationToken)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    await new ContactDatabase().GetPatientContactsForSyncAsync(contactData).ConfigureAwait(false);
                }
                if (GenericMethods.IsListNotEmpty(contactData.Contacts) || GenericMethods.IsListNotEmpty(contactData.PatientContactDetails))
                {
                    var httpData = new HttpServiceModel<ContactDTO>
                    {
                        CancellationToken = cancellationToken,
                        PathWithoutBasePath = UrlConstants.SAVE_CONTACTS_ASYNC_PATH,
                        QueryParameters = new NameValueCollection
                        {
                            { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        },
                        ContentToSend = contactData,
                    };
                    await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                    contactData.ErrCode = httpData.ErrCode;
                    if (contactData.ErrCode == ErrorCode.OK)
                    {
                        JToken data = JToken.Parse(httpData.Response);
                        if (data?.HasValues == true)
                        {
                            contactData.SaveResults = MapSaveResponse(data, nameof(ContactDTO.SaveResults));
                            await SaveContactsSyncResultAsync(contactData, cancellationToken).ConfigureAwait(false);
                        }
                        contactData.Response = null;
                    }
                }
            }
            catch (Exception ex)
            {
                contactData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync Contacts from service
        /// </summary>
        /// <param name="contactData">contactData reference to return output</param>
        /// <param name="lastSyncedDate">Last sync datetime</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Contacts received from server in contactData</returns>
        private async Task SyncContactsFromServerAsync(ContactDTO contactData, DateTimeOffset lastSyncedDate, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_CONTACTS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { Constants.SE_CONTACT_ID_QUERY_KEY, Convert.ToString(contactData.Contact.ContactID, CultureInfo.InvariantCulture) },
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(contactData.RecordCount, CultureInfo.InvariantCulture) },
                        { Constants.SE_LAST_MODIFIED_ON_QUERY_KEY, MobileConstants.IsMobilePlatform ? GetSyncDateTimeString(lastSyncedDate) : GetSyncDateTimeString(GenericMethods.GetDefaultDateTime) },
                        { Constants.SE_CONTACT_TYPE_QUERY_KEY, contactData.ContactType.ToString() },
                        { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(contactData.SelectedUserID, CultureInfo.InvariantCulture) }
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                contactData.ErrCode = httpData.ErrCode;
                if (contactData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(contactData, data);
                        MapContacts(data, contactData);
                    }
                }
            }
            catch (Exception ex)
            {
                contactData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private async Task SaveContactsSyncResultAsync(ContactDTO contactData, CancellationToken cancellationToken)
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await new ContactDatabase().UpdateContactsSyncStatusAsync(contactData).ConfigureAwait(false);
            }
            else
            {
                // Map error result to main object
                contactData.ErrCode = contactData.SaveResults?.FirstOrDefault(x => x.ErrCode != ErrorCode.OK)?.ErrCode ?? ErrorCode.OK;
                if (contactData.ErrCode == ErrorCode.DuplicateGuid)
                {
                    contactData.Contacts.FirstOrDefault().ContactID = GenericMethods.GenerateGuid();
                    foreach (var detail in contactData.PatientContactDetails)
                    {
                        detail.ContactID = contactData.Contacts.FirstOrDefault().ContactID;
                    }
                }
            }
            if (contactData.ErrCode == ErrorCode.DuplicateGuid)
            {
                contactData.ErrCode = ErrorCode.OK;
                await SyncContactsToServerAsync(contactData, cancellationToken).ConfigureAwait(false);
            }
            contactData.RecordCount = contactData.Contacts?.Count ?? 0 + contactData.ContactDetails?.Count ?? 0;
        }

        /// <summary>
        /// Get Contacts from server
        /// </summary>
        /// <param name="contactData">contactData reference to return output</param>
        /// <returns>Contacts received from server in contactData</returns>
        public async Task GetContactsAsync(ContactDTO contactData)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    contactData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, (long)0);
                    contactData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                    contactData.Contact.UserID = GetUserID();
                    List<Task> tasks = new List<Task>{
                        GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                        GetResourcesAsync(GroupConstants.RS_CONTACT_TYPE_GROUP, GroupConstants.RS_CONTACT_GROUP, GroupConstants.RS_CONTACT_PAGE_GROUP, GroupConstants.RS_COMMON_GROUP),
                        GetFeaturesAsync(AppPermissions.PatientContactsView.ToString(), AppPermissions.PatientContactAddEdit.ToString(), AppPermissions.PatientContactDelete.ToString())
                    };
                    if (contactData.RecordCount == -1)
                    {
                        tasks.Add(new ContactDatabase().GetPatientContactAsync(contactData));
                        tasks.Add(new ContactDatabase().GetPatientContactDetailsAsync(contactData));
                        tasks.Add(new CountryDatabase().GetCountriesAsync(contactData));
                    }
                    else
                    {
                        tasks.Add(new ContactDatabase().GetPatientContactsAsync(contactData));
                    }
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                    MapUIData(contactData);
                }
                else
                {
                    await SyncContactsFromServerAsync(contactData, GenericMethods.GetDefaultDateTime, CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                contactData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Save Contact to database
        /// </summary>
        /// <param name="contactData">Contacts to be saved</param>
        /// <returns>Operation result</returns>
        public async Task SaveContactsAsync(ContactDTO contactData)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {  //contactData.Contact.UserID = GetUserID();
                    contactData.Contact.IsSynced = false;
                    contactData.Contact.LastModifiedByID = contactData.AccountID;
                    contactData.Contact.LastModifiedON = GenericMethods.GetUtcDateTime;
                    if (contactData.Contact.ContactID == Guid.Empty)
                    {
                        contactData.Contact.ContactID = GenericMethods.GenerateGuid();
                        contactData.Contact.AddedByID = contactData.Contact.LastModifiedByID;
                        contactData.Contact.AddedON = contactData.Contact.LastModifiedON;
                    }
                    contactData.PatientContactDetails.ForEach(detail =>
                    {
                        detail.LastModifiedByID = contactData.Contact.LastModifiedByID;
                        detail.LastModifiedON = contactData.Contact.LastModifiedON;
                        if (detail.ContactID == Guid.Empty)
                        {
                            detail.ContactID = contactData.Contact.ContactID;
                            detail.AddedByID = detail.LastModifiedByID;
                            detail.AddedON = detail.LastModifiedON;
                        }
                        detail.IsSynced = false;
                    });
                    contactData.Contacts = new List<ContactModel> { contactData.Contact };
                    await new ContactDatabase().SaveContactAsync(contactData).ConfigureAwait(false);
                }
                else
                {
                    await SyncContactsToServerAsync(contactData, CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                contactData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapUIData(ContactDTO contactData)
        {
            if (GenericMethods.IsListNotEmpty(contactData.Contacts))
            {
                foreach (var contact in contactData.Contacts)
                {
                    contact.ContactType = $"{LibResources.GetResourceValueByKeyID(PageData?.Resources, contact.ContactTypeID)} - {LibResources.GetResourceValueByKeyID(PageData?.Resources, contact.ContactTypeIsID)}";
                }
            }
            if (contactData.RecordCount == -1)
            {
                contactData.ContactTypeOptions = PageData.Resources.Any(x => x.GroupName == GroupConstants.RS_CONTACT_GROUP)
                    ? GetPickerSource<ResourceModel>(PageData.Resources.Where(x => x.GroupName == GroupConstants.RS_CONTACT_GROUP).ToList()
                        , nameof(ResourceModel.ResourceKeyID), Constants.CNT_RESOURCE_VALUE_TEXT, contactData.Contact?.ContactTypeIsID ?? 0, true, null)
                    : new List<OptionModel>();

                contactData.ContactTypeIsOptions = PageData.Resources.Any(x => x.GroupName == GroupConstants.RS_CONTACT_TYPE_GROUP)
                    ? GetPickerSource<ResourceModel>(PageData.Resources.Where(x => x.GroupName == GroupConstants.RS_CONTACT_TYPE_GROUP).ToList()
                        , nameof(ResourceModel.ResourceKeyID), Constants.CNT_RESOURCE_VALUE_TEXT, contactData.Contact?.ContactTypeID ?? 0, true, null)
                    : new List<OptionModel>();
            }
        }

        private void MapContacts(JToken data, ContactDTO contactData)
        {
            SetPageResources(contactData.Resources);
            contactData.Contacts = MapContacts(data, nameof(ContactDTO.Contacts));
            contactData.ContactDetails = MapContacts(data, nameof(ContactDTO.ContactDetails));
            if (contactData.RecordCount == -1)
            {
                var contactJData = data[nameof(ContactDTO.Contact)];
                if (contactJData.HasValues)
                {
                    contactData.Contact = new ContactModel
                    {
                        ContactID = (Guid)contactJData[nameof(ContactModel.ContactID)],
                        ContactTypeID = (int)contactJData[nameof(ContactModel.ContactTypeID)],
                        ContactTypeIsID = (int)contactJData[nameof(ContactModel.ContactTypeIsID)],
                    };
                }
                contactData.ContactTypeIsOptions = (data[nameof(contactData.ContactTypeIsOptions)].Any()) ?
                    GetPickerSource(data, nameof(contactData.ContactTypeIsOptions), Constants.CNT_OPTION_ID, Constants.CNT_OPTION_TEXT, contactData.Contact.ContactTypeIsID, false, null)
                    : new List<OptionModel>();
                contactData.ContactTypeOptions = (data[nameof(contactData.ContactTypeOptions)].Any()) ?
                    GetPickerSource(data, nameof(contactData.ContactTypeOptions), Constants.CNT_OPTION_ID, Constants.CNT_OPTION_TEXT, contactData.Contact.ContactTypeID, false, null)
                    : new List<OptionModel>();
                contactData.CountryCodes = new CountryService(_essentials).MapCountryCodes(data);
            }
            contactData.ErrCode = (ErrorCode)(byte)data[nameof(ContactDTO.ErrCode)];
        }

        private List<ContactModel> MapContacts(JToken data, string placeholder)
        {
            return data[placeholder].Any()
                ? (from dataItem in data[placeholder]
                   select new ContactModel
                   {
                       UserID = GetDataItem<long>(dataItem, nameof(ContactModel.UserID)),
                       ContactID = GetDataItem<Guid>(dataItem, nameof(ContactModel.ContactID)),
                       ContactTypeID = GetDataItem<int>(dataItem, nameof(ContactModel.ContactTypeID)),
                       ContactTypeIsID = GetDataItem<int>(dataItem, nameof(ContactModel.ContactTypeIsID)),
                       IsActive = GetDataItem<bool>(dataItem, nameof(ContactModel.IsActive)),
                       AddedByID = GetDataItem<long>(dataItem, nameof(ContactModel.AddedByID)),
                       AddedON = GetDataItem<DateTimeOffset?>(dataItem, nameof(ContactModel.AddedON)),
                       LastModifiedByID = GetDataItem<long>(dataItem, nameof(ContactModel.LastModifiedByID)),
                       LastModifiedON = GetDataItem<DateTimeOffset?>(dataItem, nameof(ContactModel.LastModifiedON)),
                       ContactType = GetDataItem<string>(dataItem, nameof(ContactModel.ContactType)),
                       ContactTypeIs = GetDataItem<string>(dataItem, nameof(ContactModel.ContactTypeIs)),
                       LanguageID = GetDataItem<byte>(dataItem, nameof(ContactModel.LanguageID)),
                       ContactValue = GetDataItem<string>(dataItem, nameof(ContactModel.ContactValue)),
                       LanguageName = GetDataItem<string>(dataItem, nameof(ContactModel.LanguageName)),
                       IsSynced = true
                   }).ToList()
                : new List<ContactModel>();
        }

        private List<ContactDetailModel> MapContactDetails(JToken data, string placeholder)
        {
            return data[placeholder].Any()
                ? (from dataItem in data[placeholder]
                   select new ContactDetailModel
                   {
                       ContactID = GetDataItem<Guid>(dataItem, nameof(ContactDetailModel.ContactID)),
                       LanguageID = GetDataItem<byte>(dataItem, nameof(ContactDetailModel.LanguageID)),
                       ContactValue = GetDataItem<string>(dataItem, nameof(ContactDetailModel.ContactValue)),
                       IsActive = GetDataItem<bool>(dataItem, nameof(ContactDetailModel.IsActive)),
                       AddedByID = GetDataItem<long>(dataItem, nameof(ContactDetailModel.AddedByID)),
                       AddedON = GetDataItem<DateTimeOffset?>(dataItem, nameof(ContactDetailModel.AddedON)),
                       LastModifiedByID = GetDataItem<long>(dataItem, nameof(ContactDetailModel.LastModifiedByID)),
                       LastModifiedON = GetDataItem<DateTimeOffset?>(dataItem, nameof(ContactDetailModel.LastModifiedON)),
                       LanguageName = GetDataItem<string>(dataItem, nameof(ContactDetailModel.LanguageName)),
                       IsSynced = true
                   }).ToList()
                : new List<ContactDetailModel>();
        }
    }
}
