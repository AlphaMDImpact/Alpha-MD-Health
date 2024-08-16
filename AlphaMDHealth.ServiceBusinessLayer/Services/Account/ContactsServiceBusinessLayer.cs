using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class ContactsServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Organisation service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public ContactsServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get Contacts
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="selectedUserID">Selected user's ID for whoe data needs to be retrived</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="recordCount">Record count to decide how much data to retrive</param>
        /// <param name="contactID">Contact ID to retrive specific contact</param>
        /// <param name="lastModifiedOn">Last Modified on DateTime</param>
        /// <param name="contactType">Requested contact is of organisation level or user level</param>
        /// <returns>All available contacts for requested data</returns>
        public async Task<ContactDTO> GetContactsAsync(byte languageID, long selectedUserID, long permissionAtLevelID, long recordCount, Guid contactID, DateTimeOffset lastModifiedOn, ContactType contactType)
        {
            ContactDTO contacts = new ContactDTO();
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1)
                {
                    contacts.ErrCode = ErrorCode.InvalidData;
                    return contacts;
                }
                contacts.AccountID = AccountID;
                if (contacts.AccountID < 1)
                {
                    contacts.ErrCode = ErrorCode.Unauthorized;
                    return contacts;
                }
                contacts.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                contacts.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_CONTACT_PAGE_GROUP},{GroupConstants.RS_CONTACT_GROUP}", languageID, default, contacts.AccountID, permissionAtLevelID, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(contacts.Resources))
                {
                    contacts.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, $"{GroupConstants.RS_COMMON_GROUP}", languageID, default, contacts.AccountID, permissionAtLevelID, false).ConfigureAwait(false)).Settings;
                    if (GenericMethods.IsListNotEmpty(contacts.Settings))
                    {
                        contacts.LanguageID = languageID;
                        contacts.SelectedUserID = selectedUserID;
                        contacts.RecordCount = recordCount;
                        contacts.ContactType = contactType;
                        contacts.LastModifiedON = lastModifiedOn;
                        contacts.PermissionAtLevelID = permissionAtLevelID;
                        contacts.Contact = new ContactModel { ContactID = contactID };
                        contacts.FeatureFor = FeatureFor;
                        await new ContactsServiceDataLayer().GetContactsAsync(contacts).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                contacts.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return contacts;
        }

        /// <summary>
        /// Save contacts
        /// </summary>
        /// <param name="contactData">Contacts data to be saved</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <returns>Result of operation</returns>
        public async Task<ContactDTO> SaveContactsAsync(ContactDTO contactData, long permissionAtLevelID)
        {
            try
            {
                if (permissionAtLevelID < 1 || contactData.Contact == null || AccountID < 1)
                {
                    contactData.ErrCode = ErrorCode.InvalidData;
                    return contactData;
                }
                if (contactData.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(contactData.ContactDetails))
                    {
                        contactData.ErrCode = ErrorCode.InvalidData;
                        return contactData;
                    }
                    contactData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(contactData, false, string.Empty, $"{GroupConstants.RS_CONTACT_PAGE_GROUP},{GroupConstants.RS_CONTACT_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(contactData.Contact, contactData.Resources))
                        {
                            contactData.ErrCode = ErrorCode.InvalidData;
                            return contactData;
                        }
                        else if (!await ValidateDataAsync(contactData.ContactDetails, contactData.Resources))
                        {
                            contactData.ErrCode = ErrorCode.InvalidData;
                            return contactData;
                        }
                    }
                    else
                    {
                        return contactData;
                    }
                }
                contactData.AccountID = AccountID;
                contactData.PermissionAtLevelID = permissionAtLevelID;
                contactData.FeatureFor = FeatureFor;
                await new ContactsServiceDataLayer().SaveContactsAsync(contactData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                contactData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return contactData;
        }
    }
}
