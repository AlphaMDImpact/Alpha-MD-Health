using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class EducationCategoryServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Education Category service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public EducationCategoryServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Gets list of Education Categories
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="lastModifiedOn">get records modified after last modified date</param>
        /// <param name="recordCount">Record Count</param>
        /// <param name="educationCategoryID">ID of Education Category</param>
        /// <returns>Education Categories</returns>
        public async Task<EducationCategoryDTO> GetEducationCategoriesAsync(byte languageID, long permissionAtLevelID, DateTimeOffset lastModifiedOn, long recordCount, long educationCategoryID)
        {
            EducationCategoryDTO categoryData = new EducationCategoryDTO();
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
                if (await GetConfigurationDataAsync(categoryData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP}").ConfigureAwait(false))
                {
                    categoryData.LanguageID = languageID;
                    categoryData.PermissionAtLevelID = permissionAtLevelID;
                    categoryData.RecordCount = recordCount;
                    categoryData.LastModifiedON = lastModifiedOn;
                    categoryData.EductaionCatergory = new EductaionCatergoryModel { EducationCategoryID = educationCategoryID };
                    categoryData.FeatureFor = FeatureFor;
                    await new EducationCategoryServiceDataLayer().GetEducationCategoriesAsync(categoryData).ConfigureAwait(false);
                    if (categoryData.ErrCode == ErrorCode.OK)
                    {
                        await ReplaceEductaionCatergoryImageCdnLinkAsync(categoryData.EductaionCatergories);
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
        /// Saves Education Category
        /// </summary>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="organisationID">Organisation id</param>
        /// <param name="categoryDTO">Data to be saved</param>
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SaveEducationCategoryAsync(long permissionAtLevelID, long organisationID, EducationCategoryDTO categoryDTO)
        {
            try
            {
                if (permissionAtLevelID < 1 || organisationID < 1  || categoryDTO.EductaionCatergory==null || AccountID< 1)
                {
                    categoryDTO.ErrCode = ErrorCode.InvalidData;
                    return categoryDTO;
                }
                if (categoryDTO.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(categoryDTO.CategoryDetails) || string.IsNullOrWhiteSpace(categoryDTO.EductaionCatergory.ImageName))
                    {
                        categoryDTO.ErrCode = ErrorCode.InvalidData;
                        return categoryDTO;
                    }
                    categoryDTO.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(categoryDTO, false, string.Empty, $"{GroupConstants.RS_MENU_PAGE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(categoryDTO.CategoryDetails, categoryDTO.Resources))
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
                if (categoryDTO.EductaionCatergory.EducationCategoryID == 0)
                {
                    await new EducationCategoryServiceDataLayer().SaveEducationCategoryAsync(categoryDTO, false).ConfigureAwait(false);
                }
                if (categoryDTO.ErrCode == ErrorCode.OK)
                {
                    await UploadImagesAsync(categoryDTO).ConfigureAwait(false);
                    if (categoryDTO.ErrCode == ErrorCode.OK)
                    {
                        await new EducationCategoryServiceDataLayer().SaveEducationCategoryAsync(categoryDTO, true).ConfigureAwait(false);
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

        internal async Task ReplaceEductaionCatergoryImageCdnLinkAsync(List<EductaionCatergoryModel> eductaionCatergories)
        {
            if (GenericMethods.IsListNotEmpty(eductaionCatergories))
            {
                BaseDTO cdnCacheData = new BaseDTO();
                foreach (var category in eductaionCatergories)
                {
                    if (!string.IsNullOrWhiteSpace(category.ImageName))
                    {
                        category.ImageName = await ReplaceCDNLinkAsync(category.ImageName, cdnCacheData);
                    }
                }
            }
        }

        private async Task UploadImagesAsync(EducationCategoryDTO categoryData)
        {
            if (categoryData.IsActive && !string.IsNullOrWhiteSpace(categoryData.EductaionCatergory.ImageName))
            {
                FileUploadDTO files = CreateSingleFileDataObject(FileTypeToUpload.EducationCategoryImages,
                    categoryData.EductaionCatergory.EducationCategoryID.ToString(CultureInfo.InvariantCulture),
                    categoryData.EductaionCatergory.ImageName);
                files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
                categoryData.ErrCode = files.ErrCode;
                if (categoryData.ErrCode == ErrorCode.OK)
                {
                    categoryData.EductaionCatergory.ImageName = GetFirstBase64File(files);
                }
                else
                {
                    categoryData.EductaionCatergory.IsActive = false;
                }
            }
        }
    }
}