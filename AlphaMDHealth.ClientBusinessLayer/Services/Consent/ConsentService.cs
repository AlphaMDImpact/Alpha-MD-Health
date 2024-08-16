using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public class ConsentService : BaseService
{
    public ConsentService(IEssentials essentials) : base(essentials)
    {

    }
    /// <summary>
    /// Maps json object into Model and saves data into DB
    /// </summary>
    /// <param name="result">Object which holds Operation status</param>
    /// <param name="data">json object to fetch data from it</param>
    /// <returns>Operation status with record count</returns>
    internal async Task MapAndSaveConsentsAsync(DataSyncModel result, JToken data)
    {
        try
        {
            var consentData = new ConsentDTO
            {
                LastModifiedON = result.SyncFromServerDateTime,
                Consents = MapConsents(data, nameof(DataSyncDTO.Consents)),
            };
            if (GenericMethods.IsListNotEmpty(consentData.Consents))
            {
                await new ConsentDatabase().SaveConsentsAsync(consentData).ConfigureAwait(false);
                result.RecordCount = consentData.Consents?.Count ?? 0;
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
    /// Maps json object into Model and saves data into DB
    /// </summary>
    /// <param name="result">Object which holds Operation status</param>
    /// <param name="data">json object to fetch data from it</param>
    /// <returns>Operation status with record count</returns>
    internal async Task MapAndSaveUserConsentsAsync(DataSyncModel result, JToken data)
    {
        try
        {
            var consentData = new ConsentDTO
            {
                LastModifiedON = result.SyncFromServerDateTime,
                UserConsents = MapUserConsents(data, nameof(DataSyncDTO.UserConsents))
            };
            if (GenericMethods.IsListNotEmpty(consentData.UserConsents))
            {
                await new ConsentDatabase().SaveUserConsentsAsync(consentData).ConfigureAwait(false);
                result.RecordCount = consentData.UserConsents?.Count ?? 0;
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
    /// Determine whether Consent approval is still pending.
    /// </summary>
    /// <returns>ture if pending else false</returns>
    public async Task<int> IsConsentRequiredAsync()
    {
        try
        {
            var consentData = new ConsentDTO
            {
                LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0)
            };
            await Task.WhenAll(
                 GetResourcesAsync(GroupConstants.RS_PLATFORM_TYPE_GROUP),
                 new ConsentDatabase().GetUserConsentsAsync(consentData)
             ).ConfigureAwait(true);
            if (GenericMethods.IsListNotEmpty(consentData.Consents))
            {
                GetPlatformSpecificConsents(consentData);
                if (consentData.Consents.Any(x => (x.IsRequired && !x.IsAccepted)))
                {
                    return 1;
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            return 2;
        }
        return 0;
    }

    /// <summary>
    /// Get inform consent data based on ID
    /// </summary>
    /// <param name="consentData">consent data</param>
    /// <returns>Consent details with operation status</returns>
    public async Task GetConsentsAsync(ConsentDTO consentData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                consentData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                await Task.WhenAll(
                    GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_CONSENT_GROUP, GroupConstants.RS_PLATFORM_TYPE_GROUP),
                    GetFeaturesAsync(AppPermissions.UserConsentsView.ToString()),
                    new ConsentDatabase().GetUserConsentsAsync(consentData)
                ).ConfigureAwait(true);
                if (GenericMethods.IsListNotEmpty(consentData.Consents))
                {
                    string acceptHtmlString = GetPlatformSpecificString();
                    consentData.Consents.ForEach(x =>
                    {
                        x.Accepted = x.IsAccepted ? acceptHtmlString : string.Empty;
                        x.ConsentName = (x.IsRequired ? Constants.ASTERISK : string.Empty) + x.ConsentName;
                    });
                }
            }
            else
            {
                await SyncConsentsFromServerAsync(consentData).ConfigureAwait(false);
                if (GenericMethods.IsListNotEmpty(consentData.Consents))
                {
                    consentData.Consents.ForEach(x =>
                    {
                        x.Accepted = x.IsAccepted ? LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_ACCEPTED_STATUS_KEY) : string.Empty;
                        x.Accepted = string.IsNullOrEmpty(x.Accepted) ? string.Empty : GetStatus(true);
                        x.Description = x.ConsentName;
                        x.ConsentName = (x.IsRequired ? Constants.ASTERISK : string.Empty) + x.ConsentName;

                    });
                }
            }
            if (consentData.RecordCount == -3)
            {
                GetPlatformSpecificConsents(consentData);
            }
        }
        catch (Exception ex)
        {
            consentData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }
    private string GetStatus(bool isActiveStatus)
    {
        string currentStatus = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_ACCEPTED_STATUS_KEY);
        string statusStyle;
        if (isActiveStatus)
        {
            statusStyle = Constants.ACCEPT_COLOR;
        }
        else
        {
            statusStyle = string.Empty;
        }
        return string.IsNullOrEmpty(statusStyle)
            ? string.Empty
            : $"<label style ='{statusStyle}'><b>{currentStatus}</b></label>";

    }
    private void GetPlatformSpecificConsents(ConsentDTO consentData)
    {
        List<ConsentModel> consents = new List<ConsentModel> { };
        if (GenericMethods.IsListNotEmpty(consentData.Consents))
        {
            foreach (var consent in consentData.Consents)
            {
                if (!string.IsNullOrEmpty(consent.ConsentFor))
                {
                    if (MobileConstants.IsMobilePlatform)
                    {
                        //todo:
                        if (MobileConstants.IsAndroidPlatform && consent.ConsentFor.Contains(LibResources.GetResourceKeyIDByKey(PageData?.Resources, ResourceConstants.R_ANDROID_PLATFORM_KEY).ToString()))
                        {
                            consents.Add(consent);
                        }
                        if (MobileConstants.IsIosPlatform && consent.ConsentFor.Contains(LibResources.GetResourceKeyIDByKey(PageData?.Resources, ResourceConstants.R_IOS_PLATFORM_KEY).ToString()))
                        {
                            consents.Add(consent);
                        }
                    }
                    else if (consent.ConsentFor.Contains(LibResources.GetResourceKeyIDByKey(PageData?.Resources, ResourceConstants.R_WEB_PLATFORM_KEY).ToString()))
                    {
                        consents.Add(consent);
                    }
                }
            }
        }
        consentData.Consents = consents;
    }

    private string GetPlatformSpecificString()
    {
        return GenericMethods.GetPlatformSpecificValue<string>(Constants.DEFAULT_FONT_HTML.Replace("{0}", LibResources.GetResourceByKey(PageData?.Resources, ResourceConstants.R_ACCEPT_KEY).InfoValue), LibResources.GetResourceByKey(PageData?.Resources, ResourceConstants.R_ACCEPT_KEY).InfoValue, LibResources.GetResourceByKey(PageData?.Resources, ResourceConstants.R_ACCEPT_KEY).InfoValue);
    }

    /// <summary>
    /// Save inform consent data in database
    /// </summary>
    /// <param name="consentData">consent data</param>
    /// <returns>operation status</returns>
    public async Task SaveConsentAsync(ConsentDTO consentData)
    {
        try
        {
            await new ConsentDatabase().UpdateUserConsentAsync(consentData, false).ConfigureAwait(false);
            consentData.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            consentData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Consents data to server
    /// </summary>
    /// <param name="consentData">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status</returns>
    public async Task SyncConsentsToServerAsync(ConsentDTO consentData, CancellationToken cancellationToken)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await new ConsentDatabase().GetConsentDataForServerSyncAsync(consentData).ConfigureAwait(false);
                if (!GenericMethods.IsListNotEmpty(consentData.Consents))
                {
                    return;
                }
            }
            var httpData = new HttpServiceModel<ConsentDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = consentData.RecordCount == -1 ? UrlConstants.SAVE_CONSENT_ASYNC : UrlConstants.SAVE_USER_CONSENT_ASYNC,
                QueryParameters = consentData.RecordCount == -1
                    ? new NameValueCollection { { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() }, }
                    : default,
                ContentToSend = consentData
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            consentData.ErrCode = httpData.ErrCode;
            if (MobileConstants.IsMobilePlatform && consentData.ErrCode == ErrorCode.OK)
            {
                consentData.Consents.ForEach(item => item.IsSynced = true);
                await new ConsentDatabase().UpdateUserConsentAsync(consentData, true).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            consentData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    private async Task SyncConsentsFromServerAsync(ConsentDTO consentData)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                PathWithoutBasePath = consentData.IsActive
                    ? UrlConstants.GET_CONSENTS_ASYNC
                    : UrlConstants.GET_USER_CONSENTS_ASYNC,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_CONSENT_ID_QUERY_KEY, Convert.ToString(consentData?.Consent?.ConsentID, CultureInfo.InvariantCulture) },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(consentData?.RecordCount, CultureInfo.InvariantCulture) }
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            consentData.ErrCode = httpData.ErrCode;
            if (consentData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(consentData, data);
                    MapConsentsData(data, consentData);
                }
            }
        }
        catch (Exception ex)
        {
            consentData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void MapConsentsData(JToken data, ConsentDTO consentData)
    {
        SetPageResources(consentData.Resources);
        consentData.Consents = MapConsents(data, nameof(consentData.Consents));
        if (consentData.RecordCount == -1)
        {
            consentData.Consent = consentData.Consents?.FirstOrDefault() ?? new ConsentModel();
            consentData.Pages = (from dataItem in data[nameof(ConsentDTO.Pages)]
                                 select new OptionModel
                                 {
                                     OptionID = (long)dataItem[nameof(OptionModel.OptionID)],
                                     OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                                     IsSelected = consentData.Consent.PageID == (long)dataItem[nameof(OptionModel.OptionID)],
                                 }).ToList();
            //AddPlaceHolder(consentData.Pages);
            consentData.Roles = (from dataItem in data[nameof(ConsentDTO.Roles)]
                                 select new OptionModel
                                 {
                                     OptionID = (long)dataItem[nameof(OptionModel.OptionID)],
                                     OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                                     IsSelected = (bool)dataItem[nameof(OptionModel.IsSelected)],
                                 }).ToList();

            GetConsentPlatforms(consentData);
            // use below mapping 
            //consentData.Pages = GetPickerSource(data, nameof(ConsentDTO.Pages), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), consentData.Consent?.PageID ?? -1, true, null);
            //consentData.Roles = GetPickerSource(data, nameof(ConsentDTO.Roles), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), consentData.Consent?.RoleID ?? -1, true, null);
        }
        if (consentData.RecordCount == -12)
        {
            consentData.UserConsents = MapUserConsents(data, nameof(consentData.UserConsents));
        }
    }

    private void GetConsentPlatforms(ConsentDTO consent)
    {
        long[] selectedIDs;
        if (string.IsNullOrWhiteSpace(consent.Consent?.ConsentFor))
        {
            selectedIDs = new long[] { -1 };
        }
        else
        {
            selectedIDs = consent.Consent.ConsentFor?.Split(Constants.SYMBOL_PIPE_SEPERATOR)?.Select(x => Convert.ToInt64(x))?.ToArray();
        }
        consent.ConsentPlatforms = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_PLATFORM_TYPE_GROUP, string.Empty, false, selectedIDs);
        consent.ConsentPlatforms = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_PLATFORM_TYPE_GROUP, string.Empty, false);
    }

    private List<ConsentModel> MapConsents(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select new ConsentModel
               {
                   ConsentID = GetDataItem<long>(dataItem, nameof(ConsentModel.ConsentID)),
                   SequenceNo = GetDataItem<byte>(dataItem, nameof(ConsentModel.SequenceNo)),
                   IsRequired = GetDataItem<bool>(dataItem, nameof(ConsentModel.IsRequired)),
                   ConsentName = GetDataItem<string>(dataItem, nameof(ConsentModel.ConsentName)),
                   Description = GetDataItem<string>(dataItem, nameof(ConsentModel.Description)),
                   PageID = GetDataItem<long>(dataItem, nameof(ConsentModel.PageID)),
                   RoleName = GetDataItem<string>(dataItem, nameof(ConsentModel.RoleName)),
                   IsAccepted = GetDataItem<bool>(dataItem, nameof(ConsentModel.IsAccepted)),
                   IsActive = GetDataItem<bool>(dataItem, nameof(ConsentModel.IsActive)),
                   ConsentFor = GetDataItem<string>(dataItem, nameof(ConsentModel.ConsentFor)),
                   AcceptedOn = GetDataItem<DateTimeOffset>(dataItem, nameof(ConsentModel.AcceptedOn)),
                   IsSynced = true
               }).ToList()
            : null;
    }

    private List<UserConsentModel> MapUserConsents(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select new UserConsentModel
               {
                   ConsentID = GetDataItem<long>(dataItem, nameof(UserConsentModel.ConsentID)),
                   IsAccepted = GetDataItem<bool>(dataItem, nameof(UserConsentModel.IsAccepted)),
                   AcceptedOn = GetDataItem<DateTimeOffset>(dataItem, nameof(UserConsentModel.AcceptedOn)),
                   IsSynced = true
               }).ToList()
            : null;
    }
}