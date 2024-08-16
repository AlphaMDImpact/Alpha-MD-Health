using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class FileCategoryService : BaseService
{
    public FileCategoryService(IEssentials essentials) : base(essentials) { }
    /// <summary>
    /// Sync File categories and page resources from service 
    /// </summary>
    /// <param name="fileCategoryData">File Category DTO to return output</param>
    /// <param name="lastSyncedDate">Last sync datetime</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Education categories received from server in FileCategoryData and operation status</returns>
    public async Task SyncFileCategoriesFromServerAsync(FileCategoryDTO fileCategoryData, DateTimeOffset lastSyncedDate, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_FILE_CATEGORY_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { nameof(BaseDTO.LastModifiedON), GetSyncDateTimeString(lastSyncedDate) },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(fileCategoryData.RecordCount, CultureInfo.InvariantCulture) },
                    { Constants.SE_FILE_CATEGORY_ID_QUERY_KEY, Convert.ToString(fileCategoryData.FileCatergory.FileCategoryID, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            fileCategoryData.ErrCode = httpData.ErrCode;
            if (fileCategoryData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(fileCategoryData, data);
                    MapFileCategories(data, fileCategoryData);
                }
            }
        }
        catch (Exception ex)
        {
            fileCategoryData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

        /// <summary>
        /// Sync File Category Data to server
        /// </summary>
        /// <param name="fileCategoryData">object to return operation status</param>
        /// /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status call</returns>
        public async Task SyncFileCategoryToServerAsync(FileCategoryDTO fileCategoryData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<FileCategoryDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_FILE_CATEGORY_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    },
                    ContentToSend = fileCategoryData,
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                fileCategoryData.ErrCode = httpData.ErrCode;
                fileCategoryData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                fileCategoryData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

    private List<FileCategoryDetailModel> MapFileCategoryDetailsFromResponse(JToken data, string dataSelector)
    {
        return (data[dataSelector].Any()) ?
             (from dataItem in data[dataSelector]
              select new FileCategoryDetailModel
              {
                  LanguageID = GetDataItem<byte>(dataItem, nameof(FileCategoryDetailModel.LanguageID)),
                  Name = (string)dataItem[nameof(FileCategoryDetailModel.Name)],
                  Description = (string)dataItem[nameof(FileCategoryDetailModel.Description)],
                  LanguageName = (string)dataItem[nameof(FileCategoryDetailModel.LanguageName)]
              }).ToList() : null;
    }

    private void MapFileCategories(JToken data, FileCategoryDTO fileCategoryData)
    {
        fileCategoryData.FileCategories = MapFileCategories(data, nameof(FileCategoryDTO.FileCategories));
        if (fileCategoryData.RecordCount == -1)
        {
            fileCategoryData.FileCatergory = GenericMethods.IsListNotEmpty(fileCategoryData.FileCategories) ? fileCategoryData.FileCategories[0] : new FileCategoryModel();
            fileCategoryData.FileCategoryDetails = MapFileCategoryDetailsFromResponse(data, nameof(FileCategoryDTO.FileCategoryDetails));
        }
    }
}