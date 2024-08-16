using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class FileCategoryServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// File Category service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public FileCategoryServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get File Categories
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="lastModifiedOn">get records modified after last modified date</param>
        /// <param name="recordCount">Record Count</param>
        /// <param name="fileCategoryID">ID of File Category</param>
        /// <returns>File Categories Data With Operation Status</returns>
        public async Task<FileCategoryDTO> GetFileCategoriesAsync(byte languageID, long permissionAtLevelID, DateTimeOffset lastModifiedOn, long recordCount, long fileCategoryID)
        {
            FileCategoryDTO categoryData = new FileCategoryDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    categoryData.ErrCode = ErrorCode.InvalidData;
                    return categoryData;
                }
                categoryData.AccountID = AccountID;
                if (categoryData.AccountID < 1)
                {
                    categoryData.ErrCode = ErrorCode.Unauthorized;
                    return categoryData;
                }
                categoryData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetConfigurationDataAsync(categoryData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PATIENT_DOCUMNET_GROUP}").ConfigureAwait(false))
                {
                    categoryData.LanguageID = languageID;
                    categoryData.PermissionAtLevelID = permissionAtLevelID;
                    categoryData.RecordCount = recordCount;
                    categoryData.LastModifiedON = lastModifiedOn;
                    categoryData.FileCatergory = new FileCategoryModel { FileCategoryID = fileCategoryID };
                    categoryData.FeatureFor = FeatureFor;
                    await new FileCategoryServiceDataLayer().GetFileCategoriesAsync(categoryData).ConfigureAwait(false);
                    if (categoryData.ErrCode == ErrorCode.OK)
                    {
                        await ReplaceFileCatergoryImageCdnLinkAsync(categoryData.FileCategories);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                categoryData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return categoryData;
        }


        /// <summary>
        /// Save File Category To Database
        /// </summary>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="organisationID">Organisation id</param>
        /// <param name="categoryDTO">Data to be saved</param>
        /// <returns>Operation Status</returns>
        public async Task<BaseDTO> SaveFileCategoryAsync(long permissionAtLevelID, long organisationID, FileCategoryDTO categoryDTO)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || categoryDTO.FileCatergory == null)
                {
                    categoryDTO.ErrCode = ErrorCode.InvalidData;
                    return categoryDTO;
                }
                if (categoryDTO.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(categoryDTO.FileCategoryDetails) || string.IsNullOrWhiteSpace(categoryDTO.FileCatergory.ImageName))
                    {
                        categoryDTO.ErrCode = ErrorCode.InvalidData;
                        return categoryDTO;
                    }
                    categoryDTO.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(categoryDTO, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PATIENT_DOCUMNET_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(categoryDTO.FileCategoryDetails, categoryDTO.Resources))
                        {
                            categoryDTO.ErrCode = ErrorCode.InvalidData;
                            return categoryDTO;
                        }
                    }
                    else
                    {
                        return categoryDTO;
                    }
                }
                categoryDTO.AccountID = AccountID;
                categoryDTO.PermissionAtLevelID = permissionAtLevelID;
                categoryDTO.OrganisationID = organisationID;
                categoryDTO.FeatureFor = FeatureFor;
                if (categoryDTO.FileCatergory.FileCategoryID == 0) //Add
                {
                    await new FileCategoryServiceDataLayer().SaveFileCategoryAsync(categoryDTO).ConfigureAwait(false); //File categoryID
                }
                if (categoryDTO.ErrCode == ErrorCode.OK)
                {
                    await UploadImagesAsync(categoryDTO).ConfigureAwait(false); //Gets imagename
                    if (categoryDTO.ErrCode == ErrorCode.OK)
                    {
                        await new FileCategoryServiceDataLayer().SaveFileCategoryAsync(categoryDTO).ConfigureAwait(false); //save imagename
                    }
                    else
                    {
                        //By Pass Image Storage failure for now,
                        categoryDTO.ErrCode = ErrorCode.OK;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                categoryDTO.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return categoryDTO;
        }

        private async Task<bool> GetConfigurationDataAsync(BaseDTO baseDTO, byte languageID, string groupNames)
        {
            baseDTO.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
            if (baseDTO.Settings != null)
            {
                baseDTO.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, groupNames
                    , languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (baseDTO.Resources != null)
                {
                    return true;
                }
            }
            return false;
        }

        internal async Task ReplaceFileCatergoryImageCdnLinkAsync(List<FileCategoryModel> fileCategories)
        {
            if (GenericMethods.IsListNotEmpty(fileCategories))
            {
                BaseDTO cdnCacheData = new BaseDTO();
                foreach (var category in fileCategories)
                {
                    if (!string.IsNullOrWhiteSpace(category.ImageName))
                    {
                        category.ImageName = await ReplaceCDNLinkAsync(category.ImageName, cdnCacheData);
                    }
                }
            }
        }

        private async Task UploadImagesAsync(FileCategoryDTO categoryData)
        {
            if (categoryData.IsActive && !string.IsNullOrWhiteSpace(categoryData.FileCatergory.ImageName))
            {
                FileUploadDTO files = CreateSingleFileDataObject(FileTypeToUpload.FileCategoryImages,
                    categoryData.FileCatergory.FileCategoryID.ToString(CultureInfo.InvariantCulture),
                    categoryData.FileCatergory.ImageName);
                files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
                categoryData.ErrCode = files.ErrCode;
                if (categoryData.ErrCode == ErrorCode.OK)
                {
                    categoryData.FileCatergory.ImageName = GetFirstBase64File(files);
                }
                else
                {
                    categoryData.FileCatergory.IsActive = false;
                }
            }
        }
    }
}