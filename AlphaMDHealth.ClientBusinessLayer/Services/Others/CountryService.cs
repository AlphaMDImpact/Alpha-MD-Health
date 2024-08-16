using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class CountryService : BaseService
    {
        public CountryService(IEssentials essentials):base(essentials) { }
        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveCountriesAsync(DataSyncModel result, JToken data)
        {
            try
            {
                var countriesData = new BaseDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    CountryCodes = MapCountryCodes(data),
                };
                if (GenericMethods.IsListNotEmpty(countriesData.CountryCodes))
                {
                    await new CountryDatabase().SaveCountriesAsync(countriesData).ConfigureAwait(false);
                    result.RecordCount = countriesData.CountryCodes.Count;
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
        /// Gets list of Country Code
        /// </summary>
        /// <returns>Operation Result</returns>
        public async Task GetCountryCodesAsync(BaseDTO countryCodeData)
        {
            try
            {
                using (CountryDatabase countryCodeDB = new CountryDatabase())
                {
                    await countryCodeDB.GetCountriesAsync(countryCodeData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                countryCodeData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Map country code json into model
        /// </summary>
        /// <param name="data">country code json data</param>
        /// <returns>List of country code</returns>
        internal List<CountryModel> MapCountryCodes(JToken data)
        {
            return (data[nameof(BaseDTO.CountryCodes)]?.Count() > 0)
                ? (from dataItem in data[nameof(BaseDTO.CountryCodes)]
                   select new CountryModel
                   {
                       CountryName = GetDataItem<string>(dataItem, nameof(CountryModel.CountryName)),
                       CountryCode = GetDataItem<string>(dataItem, nameof(CountryModel.CountryCode)),
                       MobileNumberLength = GetDataItem<byte>(dataItem, nameof(CountryModel.MobileNumberLength)),
					   MobileNumberLengthMax = GetDataItem<byte>(dataItem, nameof(CountryModel.MobileNumberLengthMax)),
					   CountryCulture = GetDataItem<string>(dataItem, nameof(CountryModel.CountryCulture)),
                       LanguageID = GetDataItem<byte>(dataItem, nameof(BaseDTO.LanguageID))
                   }).ToList()
                : null;
        }
    }
}