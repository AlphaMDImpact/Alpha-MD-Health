using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientDataLayer
{
    public class ConsentDatabase : BaseDatabase
    {
        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="consentData">consent data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveConsentsAsync(ConsentDTO consentData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(consentData.Consents))
                {
                    foreach (ConsentModel consent in consentData.Consents)
                    {
                        transaction.InsertOrReplace(consent);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="consentData">consent data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveUserConsentsAsync(ConsentDTO consentData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(consentData.UserConsents))
                {
                    foreach (UserConsentModel userConsent in consentData.UserConsents)
                    {
                        transaction.InsertOrReplace(userConsent);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="consentData">consent data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task UpdateUserConsentAsync(ConsentDTO consentData, bool isAfterSync)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (ConsentModel consent in consentData.Consents)
                {
                    if (isAfterSync)
                    {
                        transaction.Execute("UPDATE UserConsentModel SET IsSynced = ? WHERE ConsentID = ? "
                            , consent.IsSynced, consent.ConsentID);
                    }
                    else
                    {
                        ConsentModel pageData = transaction.FindWithQuery<ConsentModel>("SELECT ConsentID FROM ConsentModel WHERE PageID = ? ", consent.PageID);
                        if (transaction.Query<UserConsentModel>("SELECT * FROM UserConsentModel WHERE ConsentID = ? ", pageData.ConsentID).Count == 0)
                        {
                            transaction.Execute("INSERT INTO UserConsentModel (ConsentID, IsAccepted, AcceptedOn, IsSynced ) VALUES (?, ?, ?, ?)"
                                , pageData.ConsentID, consent.IsAccepted, consent.AcceptedOn, consent.IsSynced);
                        }
                        else
                        {
                            transaction.Execute("UPDATE UserConsentModel SET IsAccepted = ?, AcceptedOn = ?, IsSynced = ? WHERE ConsentID = ? "
                                , consent.IsAccepted, consent.AcceptedOn, consent.IsSynced, pageData.ConsentID);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Gets list of consents
        /// </summary>
        /// <param name="consentData">object for Consent list </param>
        /// <returns>list of consents with operation status in consentData reference</returns>
        public async Task GetUserConsentsAsync(ConsentDTO consentData)
        {
            consentData.Consents = await SqlConnection.QueryAsync<ConsentModel>(
                "SELECT DISTINCT A.ConsentID, A.PageID, B.PageHeading AS ConsentName, B.Description AS Description " +
                ", A.IsRequired, A.ConsentFor, C.IsAccepted, C.AcceptedOn " +
                "FROM ConsentModel A " +
                "JOIN ContentDetailModel B ON B.PageID = A.PageID AND B.LanguageID = ? AND B.IsActive = 1 " +
                "LEFT JOIN UserConsentModel C ON C.ConsentID = A.ConsentID  " +
                "WHERE A.IsActive = 1 ORDER BY A.SequenceNo"
                , consentData.LanguageID
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Get consnets which are not synced to server
        /// </summary>
        /// <param name="consentData">Consent object </param>
        /// <returns>Consent data</returns>
        public async Task GetConsentDataForServerSyncAsync(ConsentDTO consentData)
        {
            consentData.Consents = await SqlConnection.QueryAsync<ConsentModel>(
                "SELECT A.ConsentID, A.IsAccepted, A.AcceptedOn FROM UserConsentModel A WHERE IsSynced = 0"
            ).ConfigureAwait(false);
        }
    }
}