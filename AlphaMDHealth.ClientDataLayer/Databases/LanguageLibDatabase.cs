using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientDataLayer
{
    /// <summary>
    /// represents resources database module
    /// </summary>
    public class LanguageDatabase : BaseDatabase
    {
        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="languageData">Languages data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveLanguagesAsync(LanguageDTO languageData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (languageData.LastModifiedON == null)
                {
                    //When Last sync datetime is default datetime, clear all existing records from DB and insert new one for avoid duplicate record issue as each server will contains different ServerID(PrimaryKey) and InsertOrReplace() function update records based on PrimaryKey
                    transaction.Execute("DELETE FROM LanguageModel");
                }
                if (GenericMethods.IsListNotEmpty(languageData.Languages))
                {
                    // variable used to store current UTC Datetime for use with in foreach loop
                    foreach (var language in languageData.Languages)
                    {
                        if (language.IsActive)
                        {
                            transaction.InsertOrReplace(language);
                        }
                        else
                        {
                            transaction.Execute($"DELETE FROM LanguageModel WHERE LanguageID = ?", language.LanguageID);
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets list of supported languages
        /// </summary>
        /// <param name="languageData">object for Language list </param>
        /// <returns>list of Languages with operation status in languageData reference</returns>
        public async Task GetLanguagesAsync(LanguageDTO languageData)
        {
            languageData.Languages = await SqlConnection.QueryAsync<LanguageModel>
                ("SELECT * FROM LanguageModel WHERE IsActive = 1 ORDER BY LanguageName COLLATE NOCASE ASC").ConfigureAwait(false);
        }

        /// <summary>
        /// Checks if given language is available in database
        /// </summary>
        /// <param name="selectedLanguageID">Id of the language to be verfied</param>
        /// <returns>flag which represents language is selected or not</returns>
        public async Task<bool> VerifySelectedLanguageAsync(byte selectedLanguageID)
        {
            return await SqlConnection.FindAsync<LanguageModel>(selectedLanguageID).ConfigureAwait(false) != null;
        }
    }
}