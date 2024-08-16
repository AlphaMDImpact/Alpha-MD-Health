using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class EducationCategoryService : BaseService
    {
        public EducationCategoryService(IEssentials essentials):base(essentials) { }
        /// <summary>
        /// Sync Education categories and page recources from service 
        /// </summary>
        /// <param name="educationCategoryData">Education Category DTO to return output</param>
        /// <param name="lastSyncedDate">Last sync datetime</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Edcation categories received from server in educationCategoryData and operation status</returns>
        public async Task SyncEducationCategoriesFromServerAsync(EducationCategoryDTO educationCategoryData, DateTimeOffset lastSyncedDate, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_EDUCATION_CATEGORY_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { nameof(BaseDTO.LastModifiedON), GetSyncDateTimeString(lastSyncedDate) },
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(educationCategoryData.RecordCount, CultureInfo.InvariantCulture) },
                        { Constants.SE_EDUCATION_CATEGORY_ID_QUERY_KEY, Convert.ToString(educationCategoryData.EductaionCatergory.EducationCategoryID, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                educationCategoryData.ErrCode = httpData.ErrCode;
                if (educationCategoryData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(educationCategoryData, data);
                        MapEducationCategories(data, educationCategoryData);
                    }
                }
            }
            catch (Exception ex)
            {
                educationCategoryData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private async Task ImageMappingAsync()
        {
            EducationCategoryDTO categories = new EducationCategoryDTO { EductaionCatergories = new List<EductaionCatergoryModel>(), CategoryDetails = new List<EducationCategoryDetailModel>() };
            using (EducationCategoryDatabase categoryDatabase = new EducationCategoryDatabase())
            {
                await categoryDatabase.GetEducationCategoryStatusAsync(categories).ConfigureAwait(false);
            }
            await Task.WhenAll(GetPageImagesAsync(categories),
                GetPageDetailsImageAsync(categories)).ConfigureAwait(false);
            await new EducationCategoryDatabase().UpdateContentPagesSyncImageStatusAsync(categories).ConfigureAwait(false);
        }

        private async Task GetPageImagesAsync(EducationCategoryDTO educationCategory)
        {
            foreach (EductaionCatergoryModel page in educationCategory?.EductaionCatergories)
            {
                page.ImageName = page.ImageName.Contains(Constants.HTTP_TAG_PREFIX) ? await GetImageAsBase64Async(page.ImageName).ConfigureAwait(false) : string.Empty;
            }
        }

        private async Task GetPageDetailsImageAsync(EducationCategoryDTO educationCategory)
        {
            foreach (EducationCategoryDetailModel page in educationCategory?.CategoryDetails)
            {
                page.PageData = await GetImageContentAsync(page.PageData).ConfigureAwait(false);
            }
        }

        private async Task SaveEducationCategoryAsync(EducationCategoryDTO educationCategoryData)
        {
            using (EducationCategoryDatabase categoryDatabase = new EducationCategoryDatabase())
            {
                await categoryDatabase.SaveEducationCategoryAsync(educationCategoryData).ConfigureAwait(false);
            }
            educationCategoryData.RecordCount = educationCategoryData.EductaionCatergories.Count;
            educationCategoryData.ErrCode = ErrorCode.OK;
        }

        /// <summary>
        /// Sync Education Category Data to server
        /// </summary>
        /// <param name="educationCategoryData">object to return operation status</param>
        /// /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status call</returns>
        public async Task SyncEducationCategoryToServerAsync(EducationCategoryDTO educationCategoryData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<EducationCategoryDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_EDUCATION_CATEGORY_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    },
                    ContentToSend = educationCategoryData,
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                educationCategoryData.ErrCode = httpData.ErrCode;
                educationCategoryData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                educationCategoryData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private List<EductaionCatergoryModel> MapEducationCategoriesFromResponse(JToken data, string dataSelector)
        {
            return (data[dataSelector].Any()) ?
                 (from dataItem in data[dataSelector]
                  select new EductaionCatergoryModel
                  {
                      EducationCategoryID = (long)dataItem[nameof(EductaionCatergoryModel.EducationCategoryID)],
                      ImageName = (string)dataItem[nameof(EductaionCatergoryModel.ImageName)],
                      Name = (string)dataItem[nameof(EductaionCatergoryModel.Name)],
                      IsActive = (bool)dataItem[nameof(EductaionCatergoryModel.IsActive)],
                      IsDataDownloaded = !MobileConstants.IsMobilePlatform
                  }).ToList() : null;
        }

        private List<EducationCategoryDetailModel> MapEducationCategoryDetailsFromResponse(JToken data, string dataSelector)
        {
            return (data[dataSelector].Any()) ?
                 (from dataItem in data[dataSelector]
                  select new EducationCategoryDetailModel
                  {
                      LanguageID = (byte)dataItem[nameof(EducationCategoryDetailModel.LanguageID)],
                      PageHeading = (string)dataItem[nameof(EducationCategoryDetailModel.PageHeading)],
                      PageData = (string)dataItem[nameof(EducationCategoryDetailModel.PageData)],
                      PageID = (long)dataItem[nameof(EducationCategoryDetailModel.PageID)],
                      IsActive = (bool)dataItem[nameof(EducationCategoryDetailModel.IsActive)],
                      LanguageName = (string)dataItem[nameof(EducationCategoryDetailModel.LanguageName)],
                      IsDataDownloaded = !MobileConstants.IsMobilePlatform
                  }).ToList() : null;
        }

        /// <summary>
        /// Save EducationCategories for mobile or tablet
        /// </summary>
        /// <param name="DataSyncModel">object to return operation status</param>
        /// <returns>Operation status call</returns>
        internal async Task MapAndSaveEducationCategoryAsync(DataSyncModel result, JToken data)
        {
            try
            {
                EducationCategoryDTO category = new EducationCategoryDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    EductaionCatergories = MapEducationCategoriesFromResponse(data, nameof(DataSyncDTO.EducationCategory)),
                    CategoryDetails = MapEducationCategoryDetailsFromResponse(data, nameof(DataSyncDTO.EducationCategoryDetails)),
                };
                if (GenericMethods.IsListNotEmpty(category.EductaionCatergories))
                {
                    await SaveEducationCategoryAsync(category).ConfigureAwait(true);
                    result.RecordCount = category.EductaionCatergories.Count;
                    _ = ImageMappingAsync().ConfigureAwait(false);
                }
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        private void MapEducationCategories(JToken data, EducationCategoryDTO educationCategoryData)
        {
            if (educationCategoryData.RecordCount == -11)
            {
                educationCategoryData.EductaionCatergories = MapEducationCategoriesFromResponse(data, nameof(EducationCategoryDTO.EductaionCatergories));
                educationCategoryData.CategoryDetails = MapEducationCategoryDetailsFromResponse(data, nameof(EducationCategoryDTO.CategoryDetails));
            }
            else
            {
                educationCategoryData.EductaionCatergories = MapEducationCategoriesFromResponse(data, nameof(EducationCategoryDTO.EductaionCatergories));
                if (educationCategoryData.RecordCount == -1)
                {
                    educationCategoryData.EductaionCatergory = GenericMethods.IsListNotEmpty(educationCategoryData.EductaionCatergories) ? educationCategoryData.EductaionCatergories[0] : new EductaionCatergoryModel();
                    educationCategoryData.CategoryDetails = MapEducationCategoryDetailsFromResponse(data, nameof(EducationCategoryDTO.CategoryDetails));
                }
            }
        }
    }
}
