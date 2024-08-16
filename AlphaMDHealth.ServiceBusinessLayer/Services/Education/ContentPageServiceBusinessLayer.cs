using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer;

public class ContentPageServiceBusinessLayer : BaseServiceBusinessLayer
{
    /// <summary>
    /// Content Page service
    /// </summary>
    /// <param name="httpContext">Instance of HttpContext</param>
    public ContentPageServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
    {
    }

    /// <summary>
    /// Get Content Pages data
    /// </summary>
    /// <param name="languageID">language id</param>
    /// <param name="permissionAtLevelID">level at which permission is required</param>
    /// <param name="organisationID">id of organization</param>
    /// <param name="lastModifiedOn">last modified date time</param>
    /// <param name="pageID">Content Page ID</param>
    /// <param name="selectedUserID">User's ID</param>
    /// <param name="isEducation">Is Education is parameter to decide the page is education or Static content page</param>
    /// <param name="recordCount">number of record count to fetch</param>
    /// <param name="forProvider">Is Education is for provider or all educations of organisation</param>
    /// <param name="isBasic">Is basic flag to decide content is requested for before login page or after login page</param>
    /// <param name="fromDate">Start date from where data needs to fetch</param>
    /// <param name="toDate">End date, till data needs to fetched</param>
    /// <returns>List of Content Pages and operation status</returns>
    public async Task<ContentPageDTO> GetContentPagesAsync(byte languageID, long permissionAtLevelID, long organisationID, DateTimeOffset lastModifiedOn, long pageID, long selectedUserID, bool isEducation, long recordCount, bool forProvider, bool isBasic, string fromDate, string toDate)
    {
        ContentPageDTO contentPages = new ContentPageDTO();
        try
        {
            if (permissionAtLevelID < 1 || languageID < 1)
            {
                contentPages.ErrCode = ErrorCode.InvalidData;
                return contentPages;
            }
            if (!isBasic && AccountID < 1)
            {
                contentPages.ErrCode = ErrorCode.Unauthorized;
                return contentPages;
            }
            if (await GetContentPagesResourceAndSettingsAsync(contentPages, organisationID, languageID))
            {
                contentPages.LastModifiedON = lastModifiedOn;
                contentPages.PermissionAtLevelID = permissionAtLevelID;
                contentPages.RecordCount = recordCount;
                contentPages.SelectedUserID = selectedUserID;
                contentPages.Page = new ContentPageModel { PageID = pageID, IsEducation = isEducation, };
                contentPages.IsActive = forProvider;
                contentPages.FromDate = fromDate;
                contentPages.ToDate = toDate;
                contentPages.FeatureFor = FeatureFor;
                await new ContentPageServiceDataLayer().GetContentPagesAsync(contentPages, isBasic).ConfigureAwait(false);
                await MapContentPagesDependancyAsync(contentPages);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            contentPages.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        return contentPages;
    }

    internal async Task MapContentPagesDependancyAsync(ContentPageDTO contentPages)
    {
        if (contentPages.ErrCode == ErrorCode.OK)
        {
            await ReplaceContentPagesImageCdnLinkAsync(contentPages.Pages, contentPages.PageDetails);
            await ReplaceContentPagesImageCdnLinkAsync(contentPages.PatientEducations);
        }
    }

    internal async Task<bool> GetContentPagesResourceAndSettingsAsync(ContentPageDTO contentPageData, long organisationID, byte languageID)
    {
        contentPageData.AccountID = AccountID;
        contentPageData.OrganisationID = organisationID;
        contentPageData.LanguageID = languageID;
        if (await GetSettingsResourcesAsync(contentPageData, true, GroupConstants.RS_COMMON_GROUP,
            $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP},{GroupConstants.RS_CONTENT_TYPE_GROUP},{GroupConstants.RS_TASK_STATUS_GROUP},{GroupConstants.RS_EDUCATION_TYPE_GROUP}"
        ).ConfigureAwait(false))
        {
            contentPageData.ErrCode = ErrorCode.OK;
            return true;
        }
        return false;
    }


    /// <summary>
    /// Save Content Pages to database
    /// </summary>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="organisationID">Organisation id</param>
    /// <param name="contentPages">Content Pages data to be saved</param>
    /// <returns>Operation status</returns>
    public async Task<BaseDTO> SaveContentPagesAsync(long permissionAtLevelID, long organisationID, ContentPageDTO contentPages)
    {
        try
        {
            if (permissionAtLevelID < 1 || AccountID < 1 || contentPages.Page == null)
            {
                contentPages.ErrCode = ErrorCode.InvalidData;
                return contentPages;
            }
            if (contentPages.IsActive)
            {
                if (!GenericMethods.IsListNotEmpty(contentPages.PageDetails))
                {
                    contentPages.ErrCode = ErrorCode.InvalidData;
                    return contentPages;
                }
                contentPages.LanguageID = 1;
                if (await GetSettingsResourcesAsync(contentPages, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP},{GroupConstants.RS_CONTENT_TYPE_GROUP},{GroupConstants.RS_TASK_STATUS_GROUP},{GroupConstants.RS_EDUCATION_TYPE_GROUP}").ConfigureAwait(false))
                {
                    if (!await ValidateDataAsync(contentPages.PageDetails, contentPages.Resources))
                    {
                        contentPages.ErrCode = ErrorCode.InvalidData;
                        return contentPages;
                    }
                }
                else
                {
                    return contentPages;
                }
            }
            contentPages.PermissionAtLevelID = permissionAtLevelID;
            contentPages.FeatureFor = FeatureFor;
            contentPages.OrganisationID = organisationID;
            contentPages.AccountID = AccountID;
            await new ContentPageServiceDataLayer().SaveContentPagesAsync(contentPages, false).ConfigureAwait(false);
            if (contentPages.ErrCode == ErrorCode.OK)
            {
                await UploadImagesAsync(contentPages).ConfigureAwait(false);
                if (contentPages.ErrCode == ErrorCode.OK)
                {
                    await new ContentPageServiceDataLayer().SaveContentPagesAsync(contentPages, true).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            contentPages.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return contentPages;
    }

    /// <summary>
    /// Publish/ UnPublish ContentPage
    /// </summary>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="pageID">Page id that need to published or unpublished</param>
    /// <param name="isPublished">Publish or Unpublish</param>
    /// <returns>Operation Status</returns>
    public async Task<BaseDTO> PublishContentPageAsync(long permissionAtLevelID, long pageID, bool isPublished)
    {
        BaseDTO contentPage = new BaseDTO();
        try
        {
            if (permissionAtLevelID < 1 || AccountID < 1 || pageID < 1)
            {
                contentPage.ErrCode = ErrorCode.InvalidData;
                return contentPage;
            }
            contentPage.AccountID = AccountID;
            contentPage.PermissionAtLevelID = permissionAtLevelID;
            contentPage.RecordCount = pageID;
            contentPage.IsActive = isPublished;
            contentPage.FeatureFor = FeatureFor;
            await new ContentPageServiceDataLayer().PublishContentPageAsync(contentPage).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            contentPage.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return contentPage;
    }

    /// Save Content Page status to database
    /// </summary>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="organisationID">Organisation id</param>
    /// <param name="contentPages">Content Pages data to be saved</param>
    /// <returns>Operation status</returns>
    public async Task<BaseDTO> SaveEducationStatusAsync(long permissionAtLevelID, long organisationID, ContentPageDTO contentPages)
    {
        try
        {
            if (permissionAtLevelID < 1 || !GenericMethods.IsListNotEmpty(contentPages.PatientEducations))
            {
                contentPages.ErrCode = ErrorCode.InvalidData;
            }
            if (contentPages.ErrCode == ErrorCode.OK)
            {
                contentPages.PermissionAtLevelID = permissionAtLevelID;
                contentPages.OrganisationID = organisationID;
                contentPages.FeatureFor = FeatureFor;
                await new ContentPageServiceDataLayer().SaveEducationStatusAsync(contentPages).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            contentPages.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return contentPages;
    }

    internal async Task ReplaceContentPagesImageCdnLinkAsync(List<ContentPageModel> pages, List<ContentDetailModel> pageDetails)
    {
        BaseDTO cdnCacheData = new BaseDTO();
        await ReplaceContentPageImagesCdnLinksAsync(pages, cdnCacheData);
        var isLink = pages?.All(c => c.IsLink == true);
        if (isLink == false)
        {
            await ReplaceContentDetailImageCdnLinkAsync(pageDetails, cdnCacheData);
        }
    }
    internal async Task ReplaceContentPagesImageCdnLinkAsync(List<PatientEducationModel> pages)
    {
        BaseDTO cdnCacheData = new BaseDTO();
        await ReplaceContentPageImagesCdnLinksAsync(pages, cdnCacheData);
    }

    internal async Task ReplaceContentPageImagesCdnLinksAsync(List<ContentPageModel> pages, BaseDTO cdnCacheData)
    {
        if (GenericMethods.IsListNotEmpty(pages))
        {
            foreach (var page in pages)
            {
                if (!string.IsNullOrWhiteSpace(page.ImageName))
                {
                    page.ImageName = await ReplaceCDNLinkAsync(page.ImageName, cdnCacheData);
                }
                if (!string.IsNullOrWhiteSpace(page.PDFName))
                {
                    page.PDFName = await ReplaceCDNLinkAsync(page.PDFName, cdnCacheData);
                }
            }
        }
    }

    internal async Task ReplaceContentPageImagesCdnLinksAsync(List<PatientEducationModel> pages, BaseDTO cdnCacheData)
    {
        if (GenericMethods.IsListNotEmpty(pages))
        {
            foreach (var page in pages)
            {
                if (!string.IsNullOrWhiteSpace(page.ImageName))
                {
                    page.ImageName = await ReplaceCDNLinkAsync(page.ImageName, cdnCacheData);
                }
            }
        }
    }

    internal async Task ReplaceContentDetailImageCdnLinkAsync(List<ContentDetailModel> pageDetails, BaseDTO cdnCacheData)
    {
        if (GenericMethods.IsListNotEmpty(pageDetails))
        {
            foreach (var detail in pageDetails)
            {
                if (!string.IsNullOrWhiteSpace(detail.PageData))
                {
                    detail.PageData = await ReplaceCDNLinkAsync(detail.PageData, cdnCacheData);
                }
            }
        }
    }

    private async Task UploadImagesAsync(ContentPageDTO contentPages)
    {
        FileUploadDTO files = CreateFileDataObject(FileTypeToUpload.PageImages, contentPages.Page.PageID.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrWhiteSpace(contentPages.Page.ImageName))
        {
            files.FileContainers[0].FileData.Add(CreateFileObject($"{nameof(ContentPageModel.ImageName)}_{contentPages.Page.PageID}", contentPages.Page.ImageName, false));
        }
        if (!string.IsNullOrWhiteSpace(contentPages.Page.PDFName))
        {
            files.FileContainers[0].FileData.Add(CreateFileObject($"{nameof(ContentPageModel.PDFName)}_{contentPages.Page.PageID}", contentPages.Page.PDFName, false));
        }
        files.FileContainers[0].FileData.AddRange(from detail in contentPages.PageDetails
                                                  select new FileDataModel
                                                  {
                                                      HasMultiple = true,
                                                      Base64File = detail.PageData,
                                                      RecordID = $"{detail.LanguageID}_{detail.PageID}",
                                                  });
        files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
        contentPages.ErrCode = files.ErrCode;
        if (contentPages.ErrCode == ErrorCode.OK)
        {
            contentPages.Page.ImageName = GetBase64FileFromFirstContainer(files, $"{nameof(ContentPageModel.ImageName)}_{contentPages.Page.PageID}");
            contentPages.Page.PDFName = GetBase64FileFromFirstContainer(files, $"{nameof(ContentPageModel.PDFName)}_{contentPages.Page.PageID}");
            foreach (var detail in contentPages.PageDetails)
            {
                detail.PageData = GetBase64FileFromFirstContainer(files, $"{detail.LanguageID}_{detail.PageID}");
            }
        }
    }
}