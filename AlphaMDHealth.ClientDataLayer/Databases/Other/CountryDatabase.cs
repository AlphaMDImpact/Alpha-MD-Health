using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientDataLayer
{
    public class CountryDatabase : BaseDatabase
    {
        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="countryData">Country data for save into DB</param>
        /// <param name="lastSyncedDate">Last sync datetime for decide DB cleanup is required or not</param>
        /// <returns>Operation Status</returns>
        public async Task SaveCountriesAsync(BaseDTO countryData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
              {
                  countryData.LastModifiedON = GenericMethods.GetUtcDateTime;
                  foreach (CountryModel country in countryData.CountryCodes)
                  {
                      if (transaction.FindWithQuery<CountryModel>("SELECT 1 FROM CountryModel WHERE LanguageID= ? AND CountryCode=? ", country.LanguageID, country.CountryCode) == null)
                      {
                          transaction.Execute("INSERT INTO CountryModel (CountryCode, MobileNumberLength, MobileNumberLengthMax, LanguageID, CountryName, CountryCulture) VALUES (?, ?, ?, ?, ?, ?)",
                              country.CountryCode, country.MobileNumberLength, country.MobileNumberLengthMax, country.LanguageID, country.CountryName, country.CountryCulture);
                      }
                      else
                      {
                          transaction.Execute("UPDATE CountryModel SET CountryCode= ?, MobileNumberLength= ?, MobileNumberLengthMax= ?, CountryName= ?, CountryCulture= ? WHERE LanguageID= ? AND CountryCode=? ",
                              country.CountryCode, country.MobileNumberLength, country.MobileNumberLengthMax, country.CountryName, country.CountryCulture, country.LanguageID, country.CountryCode);
                      }
                  }
              }).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets list of countries based on languageId
        /// </summary>
        /// <param name="countryData">object for Country list </param>
        /// <returns>list of countries with operation status in countryData reference</returns>
        public async Task GetCountriesAsync(BaseDTO countryData)
        {
            countryData.CountryCodes = await SqlConnection.QueryAsync<CountryModel>("SELECT * FROM CountryModel").ConfigureAwait(false);
        }
    }
}