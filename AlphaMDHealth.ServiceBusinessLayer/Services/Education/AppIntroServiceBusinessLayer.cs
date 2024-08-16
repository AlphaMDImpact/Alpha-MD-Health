using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer;

public class AppIntroServiceBusinessLayer : BaseServiceBusinessLayer
{
    /// <summary>
    /// AppIntro service
    /// </summary>
    /// <param name="httpContext">Instance of HttpContext</param>
    public AppIntroServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
    {
    }

    /// <summary>
    /// Get app intros from database
    /// </summary>
    /// <param name="languageID">user's selected language</param>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="organisationID">organisation id</param>
    /// <param name="introSlideID">intro slide id</param>
    /// <param name="recordCount">number of record count to fetch</param>
    /// <returns>Operation status and list of app intros</returns>
    public async Task<BaseDTO> GetAppIntrosAsync(byte languageID, long permissionAtLevelID, long organisationID, long introSlideID, long recordCount)
    {
        AppIntroDTO appIntroData = new AppIntroDTO();
        try
        {
            if (languageID < 1 || permissionAtLevelID < 1 || organisationID < 0)
            {
                appIntroData.ErrCode = ErrorCode.InvalidData;
                return appIntroData;
            }
            if (AccountID < 1)
            {
                appIntroData.ErrCode = ErrorCode.Unauthorized;
                return appIntroData;
            }
            appIntroData.OrganisationID = organisationID;
            appIntroData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            if (await GetConfigurationDataAsync(appIntroData, languageID).ConfigureAwait(false))
            {
                appIntroData.AccountID = AccountID;
                appIntroData.RecordCount = recordCount;
                appIntroData.PermissionAtLevelID = permissionAtLevelID;
                appIntroData.LanguageID = languageID;
                appIntroData.AppIntro = new AppIntroModel
                {
                    IntroSlideID = introSlideID,
                };
                appIntroData.FeatureFor = FeatureFor;
                await new AppIntroServiceDataLayer().GetAppIntrosAsync(appIntroData).ConfigureAwait(false);
                if (appIntroData.ErrCode == ErrorCode.OK)
                {
                    await ReplaceAppIntroImageCdnLinkAsync(appIntroData);
                }
            }
        }
        catch (Exception ex)
        {
            appIntroData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
        return appIntroData;
    }

    /// <summary>
    /// Save Files to database
    /// </summary>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="appIntroData">files data to be saved</param>
    /// <returns>Operation status</returns>
    public async Task<BaseDTO> SaveAppIntroAsync(long permissionAtLevelID, AppIntroDTO appIntroData)
    {
        try
        {
            if (AccountID < 1 || permissionAtLevelID < 0 || appIntroData.AppIntro == null
                || (appIntroData.AppIntro.IsActive && string.IsNullOrWhiteSpace(appIntroData.AppIntro.ImageName)))
            {
                appIntroData.ErrCode = ErrorCode.InvalidData;
                return appIntroData;
            }
            if (appIntroData.AppIntro.IsActive)
            {
                appIntroData.LanguageID = 1;
                if (await GetSettingsResourcesAsync(appIntroData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_WELCOME_SCREEN_GROUP}").ConfigureAwait(false))
                {
                    if (!await ValidateDataAsync(appIntroData.AppIntros, appIntroData.Resources))
                    {
                        appIntroData.ErrCode = ErrorCode.InvalidData;
                        return appIntroData;
                    }
                }
                else
                {
                    return appIntroData;
                }
            }
            appIntroData.AccountID = AccountID;
            appIntroData.PermissionAtLevelID = permissionAtLevelID;
            appIntroData.FeatureFor = FeatureFor;
            if (appIntroData.AppIntro.IntroSlideID == 0)
            {
                await new AppIntroServiceDataLayer().SaveAppIntroAsync(appIntroData).ConfigureAwait(false);
            }
            if (appIntroData.ErrCode == ErrorCode.OK)
            {
                await UploadImagesAsync(appIntroData).ConfigureAwait(false);
                if (appIntroData.ErrCode == ErrorCode.OK)
                {
                    await new AppIntroServiceDataLayer().SaveAppIntroAsync(appIntroData).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            appIntroData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return appIntroData;
    }

    internal async Task ReplaceAppIntrosImageCdnLinkAsync(List<AppIntroModel> appIntros, BaseDTO cdnCacheData)
    {
        if (GenericMethods.IsListNotEmpty(appIntros))
        {
            foreach (var appIntro in appIntros)
            {
                if (!string.IsNullOrWhiteSpace(appIntro?.ImageName))
                {
                    appIntro.ImageName = await ReplaceCDNLinkAsync(appIntro.ImageName, cdnCacheData);
                }
                if (!string.IsNullOrWhiteSpace(appIntro.SubHeaderText))
                {
                    appIntro.SubHeaderText = await ReplaceCDNLinkAsync(appIntro.SubHeaderText, cdnCacheData);
                }
            }
        }
    }

    private async Task ReplaceAppIntroImageCdnLinkAsync(AppIntroDTO appIntroData)
    {
        BaseDTO cdnCacheData = new BaseDTO();
        if (appIntroData.RecordCount == -1 && !string.IsNullOrWhiteSpace(appIntroData.AppIntro?.ImageName))
        {
            appIntroData.AppIntro.ImageName = await ReplaceCDNLinkAsync(appIntroData.AppIntro.ImageName, cdnCacheData);
        }
        await ReplaceAppIntrosImageCdnLinkAsync(appIntroData.AppIntros, cdnCacheData);
    }

    private async Task UploadImagesAsync(AppIntroDTO appIntroData)
    {
        FileUploadDTO files = CreateFileDataObject(FileTypeToUpload.AppIntroImages, appIntroData.AppIntro.IntroSlideID.ToString());
        files.FileContainers[0].FileData.AddRange(from introData in appIntroData.AppIntros
                                                  select new FileDataModel
                                                  {
                                                      HasMultiple = true,
                                                      Base64File = introData.SubHeaderText,
                                                      RecordID = $"{introData.LanguageID}_{nameof(introData.SubHeaderText)}",
                                                  });
        files.FileContainers[0].FileData.Add(new FileDataModel
        {
            HasMultiple = false,
            Base64File = appIntroData.AppIntro.ImageName,
            RecordID = $"{appIntroData.LanguageID}_{nameof(appIntroData.AppIntro.IntroSlideID)}",
        });
        files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
        appIntroData.ErrCode = files.ErrCode;
        if (appIntroData.ErrCode == ErrorCode.OK)
        {
            appIntroData.AppIntro.ImageName = GetBase64FileFromFirstContainer(files, $"{appIntroData.LanguageID}_{nameof(appIntroData.AppIntro.IntroSlideID)}");
            foreach (var introData in appIntroData.AppIntros)
            {
                introData.SubHeaderText = GetBase64FileFromFirstContainer(files, $"{introData.LanguageID}_{nameof(introData.SubHeaderText)}");
            }
        }
    }


    private async Task<bool> GetConfigurationDataAsync(AppIntroDTO appintroData, byte languageID)
    {
        appintroData.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
        if (GenericMethods.IsListNotEmpty(appintroData.Settings))
        {
            appintroData.Settings.AddRange((await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, languageID, default, 0, appintroData.OrganisationID, false).ConfigureAwait(false)).Settings);
            appintroData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_WELCOME_SCREEN_GROUP}"
                , languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
            if (GenericMethods.IsListNotEmpty(appintroData.Resources))
            {
                return true;
            }
        }
        return false;
    }
}