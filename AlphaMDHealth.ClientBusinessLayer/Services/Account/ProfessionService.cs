using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class ProfessionService : BaseService
    {
        public ProfessionService(IEssentials serviceEssentials):base(serviceEssentials) { }
        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveProfessionsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                var professionData = new ProfessionDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    UserProfessions = MapUserProfessions(data, nameof(DataSyncDTO.Professions)),
                };
                if (GenericMethods.IsListNotEmpty(professionData.UserProfessions))
                {
                    await new ProfessionDatabase().SaveProfessionsAsync(professionData).ConfigureAwait(false);
                    result.RecordCount = professionData.UserProfessions?.Count ?? 0;
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
        /// Sync Profession and page recources from service 
        /// </summary>
        /// <param name="professionData">Profession DTO to return output</param>
        /// <returns>Professions received from server in professionData and operation status</returns>
        public async Task SyncProfessionsFromServerAsync(ProfessionDTO professionData)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    PathWithoutBasePath = UrlConstants.GET_PROFESSION_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        {
                            Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY,
                            Convert.ToString(_essentials.GetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, 0), CultureInfo.InvariantCulture)
                        },
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(professionData.RecordCount, CultureInfo.InvariantCulture) },
                        { Constants.SE_PROFESSION_ID_QUERY_KEY, Convert.ToString(professionData.Profession.ProfessionID, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                professionData.ErrCode = httpData.ErrCode;
                if (professionData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(professionData, data);
                        professionData.Response = null;
                        professionData.Professions = MapProfessions(data, nameof(ProfessionDTO.Professions));
                        professionData.ErrCode = (ErrorCode)(int)data[nameof(ProfessionDTO.ErrCode)];
                    }
                }
            }
            catch (Exception ex)
            {
                professionData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync Profession Data to server
        /// </summary>
        /// <param name="professionData">object to return operation status</param>
        /// <returns>Operation status call</returns>
        public async Task SyncProfessionToServerAsync(ProfessionDTO professionData)
        {
            try
            {
                var httpData = new HttpServiceModel<ProfessionDTO>
                {
                    PathWithoutBasePath = UrlConstants.SAVE_PROFESSION_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        {
                            Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY,
                            Convert.ToString(_essentials.GetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, 0), CultureInfo.InvariantCulture)
                        }
                    },
                    ContentToSend = professionData,
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                professionData.ErrCode = httpData.ErrCode;
                professionData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                professionData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private List<UserProfessionModel> MapUserProfessions(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
                ? (from dataItem in data[collectionName]
                   select new UserProfessionModel
                   {
                       ProfessionID = (byte)dataItem[nameof(UserProfessionModel.ProfessionID)],
                       Profession = (string)dataItem[nameof(UserProfessionModel.Profession)],
                       IsActive = GetDataItem<bool>(dataItem, nameof(UserProfessionModel.IsActive)),
                   }).ToList()
                : null;
        }

        private List<ProfessionModel> MapProfessions(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
                ? (from dataItem in data[collectionName]
                   select new ProfessionModel
                   {
                       ProfessionID = (byte)dataItem[nameof(ProfessionModel.ProfessionID)],
                       Profession = (string)dataItem[nameof(ProfessionModel.Profession)],
                       LanguageID = GetDataItem<byte>(dataItem, nameof(ProfessionModel.LanguageID)),
                       LanguageName = GetDataItem<string>(dataItem, nameof(ProfessionModel.LanguageName)),
                       IsDefault = GetDataItem<bool>(dataItem, nameof(ProfessionModel.IsDefault)),
                       IsActive = GetDataItem<bool>(dataItem, nameof(ProfessionModel.IsActive)),
                   }).ToList()
                : null;
        }
    }
}