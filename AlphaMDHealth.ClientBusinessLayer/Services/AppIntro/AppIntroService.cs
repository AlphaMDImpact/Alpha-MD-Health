using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public class AppIntroService : BaseService
{
    public AppIntroService(IEssentials serviceEssentials) : base(serviceEssentials)
    {

    }
    /// <summary>
    /// Map and Save App Intro From Server
    /// </summary>
    /// <param name="result">Data Sync Result</param>
    /// <param name="data">Jtoken Data From Sync call</param>
    /// <returns>Operation Status and No of records saved</returns>
    internal async Task MapAndSaveAppIntrosAsync(DataSyncModel result, JToken data)
    {
        try
        {
            AppIntroDTO appIntroData = new AppIntroDTO();
            MapAppIntroRecords(data, appIntroData);
            if (GenericMethods.IsListNotEmpty(appIntroData.AppIntros))
            {
                await ImageMappingAsync(appIntroData).ConfigureAwait(false);
                await new AppIntroDatabase().SaveAppIntrosAsync(appIntroData).ConfigureAwait(false);
                result.RecordCount = appIntroData.AppIntros?.Count ?? 0;
            }
        }
        catch (Exception ex)
        {
            result.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }

    }

    /// <summary>
    /// Get list of app intros
    /// </summary>
    /// <param name="appIntroData">Reference object to return app intros data</param>
    /// <returns>app intros in reference object</returns>
    public async Task GetAppIntrosAsync(AppIntroDTO appIntroData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await Task.WhenAll(
                    GetResourcesAsync(GroupConstants.RS_COMMON_GROUP),
                    new AppIntroDatabase().GetAppIntrosAsync(appIntroData)
                ).ConfigureAwait(false);
                if (GenericMethods.IsListNotEmpty(appIntroData.AppIntros))
                {
                    appIntroData.AppIntros = appIntroData.AppIntros.OrderBy(x => x.SequenceNo).ToList();
                }
            }
            else
            {
                await SyncAppIntrosFromServerAsync(appIntroData, CancellationToken.None).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            appIntroData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Sync AppIntros from service
    /// </summary>
    /// <param name="appIntroData">appintro data reference to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppIntros data from server</returns>
    private async Task SyncAppIntrosFromServerAsync(AppIntroDTO appIntroData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_APP_INTROS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { nameof(AppIntroModel.IntroSlideID), Convert.ToString(appIntroData.AppIntro?.IntroSlideID??0, CultureInfo.InvariantCulture) },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(appIntroData.RecordCount, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            appIntroData.ErrCode = httpData.ErrCode;
            if (appIntroData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(appIntroData, data);
                    MapAppIntroRecords(data, appIntroData);
                    if (appIntroData.RecordCount == -1 && data[nameof(AppIntroDTO.AppIntro)].Any())
                    {
                        appIntroData.AppIntro = MapAppIntroData(data[nameof(AppIntroDTO.AppIntro)]);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            appIntroData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Sync File data to server
    /// </summary>
    /// <param name="requestData">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status call</returns>
    public async Task SyncAppIntroToServerAsync(AppIntroDTO requestData, CancellationToken cancellationToken)
    {
        try
        {
            if (GenericMethods.IsListNotEmpty(requestData.AppIntros))
            {
                var httpData = new HttpServiceModel<AppIntroDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_APP_INTROS_ASYNC_PATH,
                    ContentToSend = requestData,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    }
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                requestData.ErrCode = httpData.ErrCode;
            }
        }
        catch (Exception ex)
        {
            requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void MapAppIntroRecords(JToken data, AppIntroDTO appIntroData)
    {
        appIntroData.AppIntros = data[nameof(appIntroData.AppIntros)].Any()
            ? (from dataItem in data[nameof(appIntroData.AppIntros)]
               select MapAppIntroData(dataItem)).ToList()
            : new List<AppIntroModel>();
    }

    private AppIntroModel MapAppIntroData(JToken dataItem)
    {
        return new AppIntroModel
        {
            IntroSlideID = GetDataItem<long>(dataItem, nameof(AppIntroModel.IntroSlideID)),
            SequenceNo = GetDataItem<int>(dataItem, nameof(AppIntroModel.SequenceNo)),
            ImageName = GetDataItem<string>(dataItem, nameof(AppIntroModel.ImageName)),
            HeaderText = GetDataItem<string>(dataItem, nameof(AppIntroModel.HeaderText)),
            SubHeaderText = GetDataItem<string>(dataItem, nameof(AppIntroModel.SubHeaderText)),
            IsActive = GetDataItem<bool>(dataItem, nameof(AppIntroModel.IsActive)),
            LanguageID = GetDataItem<byte>(dataItem, nameof(AppIntroModel.LanguageID)),
            LanguageName = GetDataItem<string>(dataItem, nameof(AppIntroModel.LanguageName))
        };
    }

    private async Task ImageMappingAsync(AppIntroDTO appIntroData)
    {
        if (GenericMethods.IsListNotEmpty(appIntroData.AppIntros))
        {
            foreach (AppIntroModel AppIntro in appIntroData.AppIntros)
            {
                AppIntro.ImageName = await GetImageAsBase64Async(AppIntro.ImageName).ConfigureAwait(true);

                //AppIntro.ImageBytes = await GetImageAsByteArrayAsync(AppIntro.ImageName).ConfigureAwait(true);
                //AppIntro.ImageName = ResetCdnLink(AppIntro.ImageBytes, AppIntro.ImageName);
            }
        }
    }
}
