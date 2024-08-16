using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    /// <summary>
    /// File category service
    /// </summary>
    public partial class FileCategoryService : BaseService
    {
        /// <summary>
        /// Map and Save File Categories Data From Sever
        /// </summary>
        /// <param name="result">Data Sync Result</param>
        /// <param name="data">Jtoken Data From Sync call</param>
        /// <returns>Operation Status and No of records saved</returns>
        internal async Task MapAndSaveFileCategoriesAsync(DataSyncModel result, JToken data)
        {
            try
            {
                FileCategoryDTO fileCategoriesData = new FileCategoryDTO
                {
                    FileCategories = MapFileCategories(data, nameof(DataSyncDTO.FileCategories))
                };
                if (GenericMethods.IsListNotEmpty(fileCategoriesData.FileCategories))
                {
                    await new FileCategoryDatabase().SaveFileCategoriesAsync(fileCategoriesData).ConfigureAwait(false);
                    result.RecordCount = fileCategoriesData.FileCategories?.Count ?? 0;
                }
                _ = DownloadFileCategoryImagesAsync().ConfigureAwait(false);
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// Map and Save File Category Details Data From Sever
        /// </summary>
        /// <param name="result">Data Sync Result</param>
        /// <param name="data">Jtoken Data From Sync call</param>
        /// <returns>Operation Status and No of records saved</returns>
        internal async Task MapAndSaveFileCategoryDetailsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                FileCategoryDTO fileCategoriesData = new FileCategoryDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    FileCategoryDetails = MapFileCategoryDetails(data, nameof(DataSyncDTO.FileCategoryDetails))
                };
                if (GenericMethods.IsListNotEmpty(fileCategoriesData.FileCategoryDetails))
                {
                    await new FileCategoryDatabase().SaveFileCategoryDetailsAsync(fileCategoriesData).ConfigureAwait(false);
                    result.RecordCount = fileCategoriesData.FileCategoryDetails?.Count ?? 0;
                }
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        private async Task DownloadFileCategoryImagesAsync()
        {
            try
            {
                FileCategoryDTO fileCategoryData = new FileCategoryDTO { FileCategories = new List<FileCategoryModel>() };
                await new FileCategoryDatabase().GetFileCategoriesToSyncImagesAsync(fileCategoryData).ConfigureAwait(false);
                if (GenericMethods.IsListNotEmpty(fileCategoryData.FileCategories))
                {
                    await GetFileCategoryImagesAsync(fileCategoryData).ConfigureAwait(false);
                    await new FileCategoryDatabase().UpdateFileCategoriesImageSyncStatusAsync(fileCategoryData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
            }
        }

        private async Task GetFileCategoryImagesAsync(FileCategoryDTO fileCategoryData)
        {
            foreach (FileCategoryModel category in fileCategoryData?.FileCategories)
            {
                if (!string.IsNullOrWhiteSpace(category.ImageName) 
                    && category.ImageName.Contains(Constants.HTTP_TAG_PREFIX))
                {
                    category.ImageName = await GetImageAsBase64Async(category.ImageName).ConfigureAwait(false);
                }
            }
        }

        private List<FileCategoryModel> MapFileCategories(JToken data, string dataSelector)
        {
            return (data[dataSelector].Any()) ?
                 (from dataItem in data[dataSelector]
                  select new FileCategoryModel
                  {
                      FileCategoryID = GetDataItem<long>(dataItem, nameof(FileCategoryModel.FileCategoryID)),
                      ImageName = GetDataItem<string>(dataItem, nameof(FileCategoryModel.ImageName)),
                      Name = GetDataItem<string>(dataItem, nameof(FileCategoryModel.Name)),
                      IsActive = GetDataItem<bool>(dataItem, nameof(FileCategoryModel.IsActive)),
                  }).ToList() : null;
        }

        private List<FileCategoryDetailModel> MapFileCategoryDetails(JToken data, string dataSelector)
        {
            return (data[dataSelector].Any()) ?
                 (from dataItem in data[dataSelector]
                  select new FileCategoryDetailModel
                  {
                      FileCategoryID = GetDataItem<long>(dataItem, nameof(FileCategoryDetailModel.FileCategoryID)),
                      LanguageID = GetDataItem<byte>(dataItem, nameof(FileCategoryDetailModel.LanguageID)),
                      Name = GetDataItem<string>(dataItem, nameof(FileCategoryDetailModel.Name)),
                      Description = GetDataItem<string>(dataItem, nameof(FileCategoryDetailModel.Description)),
                      IsActive = GetDataItem<bool>(dataItem, nameof(FileCategoryDetailModel.IsActive)),
                  }).ToList() : null;
        }
    }
}