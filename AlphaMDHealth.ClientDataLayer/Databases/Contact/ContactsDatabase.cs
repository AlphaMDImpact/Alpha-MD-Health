using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    /// <summary>
    /// Contact database
    /// </summary>
    public class ContactDatabase : BaseDatabase
    {
        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="contactData">contact data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveContactsAsync(ContactDTO contactData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(contactData.Contacts))
                {
                    foreach (ContactModel contact in contactData.Contacts)
                    {
                        var existingData = transaction.FindWithQuery<ContactModel>("SELECT ContactID FROM ContactModel WHERE ContactID=? AND UserID=? ", contact.ContactID, contact.UserID);
                        if (existingData == null)
                        {
                            transaction.Execute("INSERT INTO ContactModel (ContactID, UserID, ContactTypeID, ContactTypeIsID, IsSynced, IsActive, AddedON, AddedByID, LastModifiedON, LastModifiedByID) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                                contact.ContactID, contact.UserID, contact.ContactTypeID, contact.ContactTypeIsID, contact.IsSynced, contact.IsActive, contact.AddedON, contact.AddedByID, contact.LastModifiedON, contact.LastModifiedByID);
                        }
                        else
                        {
                            transaction.Execute("UPDATE ContactModel SET ContactTypeID=?, ContactTypeIsID=?, IsSynced=?, IsActive=?, AddedON=?, AddedByID=?, LastModifiedON=?, LastModifiedByID=? WHERE ContactID=? AND UserID=? ",
                                contact.ContactTypeID, contact.ContactTypeIsID, contact.IsSynced, contact.IsActive, contact.AddedON, contact.AddedByID, contact.LastModifiedON, contact.LastModifiedByID, contact.ContactID, contact.UserID);
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="contactData">contact detail data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveContactDetailsAsync(ContactDTO contactData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(contactData.PatientContactDetails))
                {
                    foreach (ContactDetailModel contact in contactData.PatientContactDetails)
                    {
                        var existingData = transaction.FindWithQuery<ContactDetailModel>("SELECT ContactDetailID FROM ContactDetailModel WHERE LanguageID=? AND ContactID=? ", contact.LanguageID, contact.ContactID);
                        if (existingData == null)
                        {
                            transaction.Execute("INSERT INTO ContactDetailModel (ContactID, LanguageID, ContactValue, IsSynced, IsActive, AddedON, AddedByID, LastModifiedON, LastModifiedByID) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                                contact.ContactID, contact.LanguageID, contact.ContactValue, contact.IsSynced, contact.IsActive, contact.AddedON, contact.AddedByID, contact.LastModifiedON, contact.LastModifiedByID);
                        }
                        else
                        {
                            transaction.Execute("UPDATE ContactDetailModel SET ContactValue=?, IsSynced=?, IsActive=?, AddedON=?, AddedByID=?, LastModifiedON=?, LastModifiedByID=? WHERE LanguageID=? AND ContactID=? ",
                                contact.ContactValue, contact.IsSynced, contact.IsActive, contact.AddedON, contact.AddedByID, contact.LastModifiedON, contact.LastModifiedByID, contact.LanguageID, contact.ContactID);
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert or update contact with detail in the database
        /// </summary>
        /// <param name="contactData">contact data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveContactAsync(ContactDTO contactData)
        {
            if (GenericMethods.IsListNotEmpty(contactData.Contacts))
            {
                ContactModel contact = contactData.Contacts.FirstOrDefault();
                if (!contact.IsSynced && contact.IsActive
                    && await SqlConnection.FindWithQueryAsync<ContactModel>(
                        "SELECT 1 FROM ContactModel " +
                        "WHERE IsActive = 1 AND ContactTypeIsID = ? AND ContactTypeID = ? AND UserID = ? AND ContactID <> ? "
                        , contact.ContactTypeIsID, contact.ContactTypeID, contact.UserID, contact.ContactID
                    ).ConfigureAwait(false) != null)
                {
                    contactData.ErrCode = ErrorCode.DuplicateData;
                    return;
                }
            }
            await SaveContactsAsync(contactData).ConfigureAwait(false);
            await SaveContactDetailsAsync(contactData).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets list of patient contacts
        /// </summary>
        /// <param name="consentData">object for patient contacts list </param>
        /// <returns>list of patient contacts with operation status in contactData reference</returns>
        public async Task GetPatientContactsAsync(ContactDTO contactData)
        {
            string limit = contactData.RecordCount > 0 ? $" LIMIT {contactData.RecordCount}" : "";
            contactData.Contacts = await SqlConnection.QueryAsync<ContactModel>(
                "SELECT C.ContactID, C.ContactTypeID, C.ContactTypeIsID, D.ContactValue " +
                "FROM ContactModel C " +
                "LEFT JOIN ContactDetailModel D ON D.ContactID = C.ContactID AND D.IsActive = 1 AND D.LanguageID = ? " +
                "WHERE C.UserID = ? AND C.IsActive = 1 " +
                $"ORDER BY C.ContactTypeIsID {limit}"
                , contactData.LanguageID, contactData.Contact.UserID
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets list of organisation contacts
        /// </summary>
        /// <param name="consentData">object for contacts list </param>
        /// <returns>list of contacts with operation status in contactData reference</returns>
        public async Task GetOrganisationContactsAsync(ContactDTO contactData)
        {
            contactData.Contacts = await SqlConnection.QueryAsync<ContactModel>(
                "SELECT C.ContactID, C.ContactTypeID, C.ContactTypeIsID, D.ContactValue " +
                "FROM ContactModel C " +
                "LEFT JOIN ContactDetailModel D ON D.ContactID = C.ContactID AND D.IsActive = 1 AND D.LanguageID = ? " +
                "WHERE C.ContactTypeID IN (183, 185) AND C.IsActive = 1 " +
                $"ORDER BY C.ContactTypeIsID"
                , contactData.LanguageID
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets list of patient contacts
        /// </summary>
        /// <param name="contactData">object for patient contacts list </param>
        /// <returns>list of patient contacts with operation status in contactData reference</returns>
        public async Task GetPatientContactAsync(ContactDTO contactData)
        {
            if (contactData.Contact.ContactID != Guid.Empty)
            {
                var contact = await SqlConnection.FindWithQueryAsync<ContactModel>(
                    "SELECT * FROM ContactModel WHERE ContactID = ?", contactData.Contact.ContactID
                ).ConfigureAwait(false);
                if (contact != null)
                {
                    contactData.Contact = contact;
                }
            }
        }

        /// <summary>
        /// Gets list of patient contact details
        /// </summary>
        /// <param name="contactData">object for patient contacts list </param>
        /// <returns>list of patient contacts with operation status in contactData reference</returns>
        public async Task GetPatientContactDetailsAsync(ContactDTO contactData)
        {
            contactData.PatientContactDetails = await SqlConnection.QueryAsync<ContactDetailModel>(
                "SELECT D.*, L.LanguageID, L.LanguageName " +
                "FROM LanguageModel L " +
                "LEFT JOIN ContactDetailModel D ON D.LanguageID = L.LanguageID AND D.IsActive = 1 AND D.ContactID = ? " +
                "WHERE L.IsActive = 1 " +
                "ORDER BY L.LanguageName COLLATE NOCASE ASC"
                , contactData.Contact.ContactID
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch medications to sync to server
        /// </summary>
        /// <param name="contactData">Reference object to store data</param>
        /// <returns>Unsynced medication data</returns>
        public async Task GetPatientContactsForSyncAsync(ContactDTO contactData)
        {
            contactData.Contacts = await SqlConnection.QueryAsync<ContactModel>(
                "SELECT * FROM ContactModel WHERE IsSynced = 0"
            ).ConfigureAwait(false);
            contactData.PatientContactDetails = await SqlConnection.QueryAsync<ContactDetailModel>(
                "SELECT * FROM ContactDetailModel WHERE IsSynced = 0"
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Update medication sync status
        /// </summary>
        /// <param name="contactData">data to update sync status</param>
        public async Task UpdateContactsSyncStatusAsync(ContactDTO contactData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                UpdateContactSyncStatus(contactData, transaction);
                UpdateContactDetailsSyncStatus(contactData, transaction);
            }).ConfigureAwait(false);
        }

        private void UpdateContactSyncStatus(ContactDTO contactData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(contactData.Contacts))
            {
                foreach (ContactModel contact in contactData.Contacts)
                {
                    SaveResultModel result = contactData.SaveResults?.FirstOrDefault(x => x.ClientGuid == contact.ContactID);
                    contact.ErrCode = result == null ? ErrorCode.OK : result.ErrCode;
                    switch (contact.ErrCode)
                    {
                        case ErrorCode.OK:
                            // Data is successfully synced, so only update sync flag
                            transaction.Execute("UPDATE ContactModel SET IsSynced = 1, ErrCode = ? WHERE ContactID = ?", contact.ErrCode, contact.ContactID);
                            break;
                        case ErrorCode.DuplicateGuid:
                            // Update with new Guid
                            Guid newContactID = GenerateNewGuid(transaction);
                            transaction.Execute("UPDATE ContactModel SET ContactID = ?, IsSynced = 0 WHERE ContactID = ?", newContactID, contact.ContactID);
                            transaction.Execute("UPDATE ContactDetailModel SET ContactID = ?, IsSynced = 0 WHERE ContactID = ?", newContactID, contact.ContactID);
                            contactData.ErrCode = contact.ErrCode;
                            if (GenericMethods.IsListNotEmpty(contactData.PatientContactDetails))
                            {
                                contactData.PatientContactDetails.ForEach(x =>
                                {
                                    if (x.ContactID == contact.ContactID)
                                    {
                                        x.ContactID = newContactID;
                                    }
                                });
                            }
                            break;
                        default:
                            // Mark record with the received error code
                            transaction.Execute("UPDATE ContactModel SET ErrCode = ? WHERE ContactID = ?", contact.ErrCode, contact.ContactID);
                            break;
                    }
                }
            }
        }

        private void UpdateContactDetailsSyncStatus(ContactDTO contactData, SQLiteConnection transaction)
        {
            if (!GenericMethods.IsListNotEmpty(contactData.SaveResults)
                && GenericMethods.IsListNotEmpty(contactData.PatientContactDetails))
            {
                foreach (ContactDetailModel detail in contactData.PatientContactDetails)
                {
                    transaction.Execute(
                        "UPDATE ContactDetailModel SET IsSynced = 1 WHERE ContactDetailID = ? "
                        , detail.ContactDetailID);
                }
            }
        }


        private Guid GenerateNewGuid(SQLiteConnection transaction)
        {
            Guid newGuid = GenericMethods.GenerateGuid();
            while (transaction.ExecuteScalar<int>("SELECT 1 FROM ContactModel WHERE ContactID = ?", newGuid) > 0)
            {
                newGuid = GenericMethods.GenerateGuid();
            }
            return newGuid;
        }
    }
}