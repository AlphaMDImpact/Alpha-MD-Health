using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class LanguageService : BaseService
    {
        public LanguageService(IEssentials essentials) : base(essentials) { }
        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveLanguagesAsync(DataSyncModel result, JToken data)
        {
            try
            {
                LanguageDTO languageData = new LanguageDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    Languages = MapLanguages(data),
                };
                if (languageData.LastModifiedON == null || GenericMethods.IsListNotEmpty(languageData.Languages))
                {
                    await new LanguageDatabase().SaveLanguagesAsync(languageData).ConfigureAwait(false);
                    result.RecordCount = languageData.Languages?.Count ?? 0;
                }
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Verify selected language is present or not
        /// </summary>
        /// <param name="accountData">result object to store operation status</param>
        /// <returns>Operation status</returns>
        public async Task VerifySelectedLanguageAsync(BaseDTO accountData)
        {
            // if selected language is not available in new environment, then clear selected language
            byte lang = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            if (lang != 0 && !await new LanguageDatabase().VerifySelectedLanguageAsync(lang).ConfigureAwait(false))
            {
                _essentials.SetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                accountData.ErrCode = ErrorCode.LanguageNotAvailable;
            }
        }

        /// <summary>
        /// Gets list of supported languages
        /// </summary>
        /// <param name="languageData">object for language data</param>
        /// <returns>List of languages in languageData reference</returns>
        public async Task GetSupportedLanguagesAsync(LanguageDTO languageData)
        {
            try
            {
                await Task.WhenAll(
                    GetResourcesAsync(GroupConstants.RS_COMMON_GROUP),
                    new LanguageDatabase().GetLanguagesAsync(languageData)
                ).ConfigureAwait(false);
                languageData.ErrCode = GenericMethods.IsListNotEmpty(languageData.Languages) ? ErrorCode.OK : ErrorCode.RestartApp;
                //if (languageData.Languages?.Count > 0)
                //{
                //var textColor = StyleConstants.PRIMARY_TEXT_COLOR;
                //languageData.Languages.ForEach(language => language.TextColor = textColor);
                //}
            }
            catch (Exception ex)
            {
                languageData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Map languages json into model
        /// </summary>
        /// <param name="data">languages json data</param>
        /// <returns>List of languages</returns>
        internal List<LanguageModel> MapLanguages(JToken data)
        {
            return (data[nameof(LanguageDTO.Languages)]?.Count() > 0)
                ? (from dataItem in data[nameof(LanguageDTO.Languages)]
                   select new LanguageModel
                   {
                       LanguageID = GetDataItem<byte>(dataItem, nameof(LanguageModel.LanguageID)),
                       LanguageCode = GetDataItem<string>(dataItem, nameof(LanguageModel.LanguageCode)),
                       LanguageName = GetDataItem<string>(dataItem, nameof(LanguageModel.LanguageName)),
                       IsDefault = GetDataItem<bool>(dataItem, nameof(LanguageModel.IsDefault)),
                       IsRightToLeft = GetDataItem<bool>(dataItem, nameof(LanguageModel.IsRightToLeft)),
                       IsActive = GetDataItem<bool>(dataItem, nameof(LanguageModel.IsActive))
                   }).ToList()
                : null;
        }

    }
}