using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;
    public class OrganisationTagService : BaseService
{
    public OrganisationTagService(IEssentials serviceEssentials) : base(serviceEssentials)
    {

    }
    /// <summary>
    /// Sync organisationTags and page recourses from service 
    /// </summary>
    /// <param name="organisationTagData">organisationTag DTO to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>organisationTags received from server in organisationTag data and operation status</returns>
    public async Task SyncOrganisationTagsFromServerAsync(OrganisationTagDTO organisationTagData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_ORGANISATION_TAGS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(organisationTagData.RecordCount, CultureInfo.InvariantCulture) },
                    { Constants.SE_ORGANISATION_TAG_ID_QUERY_KEY, Convert.ToString(organisationTagData.OrganisationTag.OrganisationTagID, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            organisationTagData.ErrCode = httpData.ErrCode;
            if (organisationTagData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(organisationTagData, data);
                    organisationTagData.Response = null;
                    organisationTagData.OrganisationTags = MapOrganisationTags(data, nameof(OrganisationTagDTO.OrganisationTags));
                    organisationTagData.ErrCode = (ErrorCode)(int)data[nameof(OrganisationTagDTO.ErrCode)];
                }
            }
        }
        catch (Exception ex)
        {
            organisationTagData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Reason Data to server
    /// </summary>
    /// <param name="organisationTagData">object to return operation status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation status call</returns>
    public async Task SyncOrganisationTagToServerAsync(OrganisationTagDTO organisationTagData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<OrganisationTagDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_ORGANISATION_TAG_ASYNC_PATH,
                ContentToSend = organisationTagData,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            organisationTagData.ErrCode = httpData.ErrCode;
            organisationTagData.Response = httpData.Response;
        }
        catch (Exception ex)
        {
            organisationTagData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    private List<OrganisationTagModel> MapOrganisationTags(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select MapOrganisationTags(dataItem)).ToList()
            : null;
    }

    public OrganisationTagModel MapOrganisationTags(JToken dataItem)
    {
        return new OrganisationTagModel
        {
            OrganisationTagID = GetDataItem<long>(dataItem, nameof(OrganisationTagModel.OrganisationTagID)),
            TagText = GetDataItem<string>(dataItem, nameof(OrganisationTagModel.TagText)),
            IsActive = GetDataItem<bool>(dataItem, nameof(OrganisationTagModel.IsActive)),
            TagDescription = GetDataItem<string>(dataItem, nameof(OrganisationTagModel.TagDescription)),
            LanguageID = GetDataItem<byte>(dataItem, nameof(OrganisationTagModel.LanguageID)),
            LanguageName = GetDataItem<string>(dataItem, nameof(OrganisationTagModel.LanguageName)),
        };
    }
}

