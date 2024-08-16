using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer;
public class OrganisationTagServiceBusinessLayer : BaseServiceBusinessLayer
{
    /// <summary>
    /// OrganisationTag Service
    /// </summary>
    /// <param name="httpContext">Instance of HttpContext</param>
    public OrganisationTagServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
    {

    }

    /// <summary>
    /// Get Reasons
    /// </summary>
    /// <param name="languageID">User's language ID</param>
    /// <param name="permissionAtLevelID">level at which permission is required</param>
    /// <param name="organisationTagID">ID of OrganisationTagID</param>
    /// <param name="recordCount">Record Count</param>
    /// <returns>Returns list of OrganisationTag(s)</returns>
    public async Task<OrganisationTagDTO> GetOrganisationTagsAsync(byte languageID, long permissionAtLevelID, long organisationTagID, long recordCount)
    {
        OrganisationTagDTO organisationTagData = new OrganisationTagDTO();
        try
        {
            if (permissionAtLevelID < 1 || languageID < 1)
            {
                organisationTagData.ErrCode = ErrorCode.InvalidData;
                return organisationTagData;
            }
            organisationTagData.AccountID = AccountID;
            if (organisationTagData.AccountID < 1)
            {
                organisationTagData.ErrCode = ErrorCode.Unauthorized;
                return organisationTagData;
            }
            organisationTagData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            if (await GetConfigurationDataAsync(organisationTagData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_TAG_GROUP}").ConfigureAwait(false))
            {
                organisationTagData.AccountID = AccountID;
                organisationTagData.PermissionAtLevelID = permissionAtLevelID;
                organisationTagData.LanguageID = languageID;
                organisationTagData.RecordCount = recordCount;
                organisationTagData.OrganisationTag = new OrganisationTagModel
                {
                    OrganisationTagID = organisationTagID
                };
                organisationTagData.FeatureFor = FeatureFor;
                await new OrganisationTagServiceDataLayer().GetOrganisationTagsAsync(organisationTagData).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            organisationTagData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        return organisationTagData;
    }

    private async Task<bool> GetConfigurationDataAsync(BaseDTO baseDTO, byte languageID, string groupNames)
    {
        baseDTO.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, groupNames, languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
        if (baseDTO.Resources != null)
        {
            return true;
        }
        baseDTO.ErrCode = ErrorCode.InvalidData;
        return false;
    }

    /// <summary>
    /// Save Organisation Tag
    /// </summary>
    /// <param name="organisationTagData">Data to be saved</param>
    /// <param name="permissionAtLevelID">level at which permission is required</param>
    /// <returns>Operation status</returns>
    public async Task<BaseDTO> SaveOrganisationTagAsync(OrganisationTagDTO organisationTagData, long permissionAtLevelID)
    {
        try
        {
            if (permissionAtLevelID < 1 || AccountID < 1 || organisationTagData.OrganisationTag == null)
            {
                organisationTagData.ErrCode = ErrorCode.InvalidData;
                return organisationTagData;
            }
            if (organisationTagData.IsActive)
            {
                if (!GenericMethods.IsListNotEmpty(organisationTagData.OrganisationTags))
                {
                    organisationTagData.ErrCode = ErrorCode.InvalidData;
                    return organisationTagData;
                }
                organisationTagData.LanguageID = 1;
                if (await GetSettingsResourcesAsync(organisationTagData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_TAG_GROUP}").ConfigureAwait(false))
                {
                    if (!await ValidateDataAsync(organisationTagData.OrganisationTags, organisationTagData.Resources))
                    {
                        organisationTagData.ErrCode = ErrorCode.InvalidData;
                        return organisationTagData;
                    }
                }
                else
                {
                    return organisationTagData;
                }
            }
            organisationTagData.AccountID = AccountID;
            organisationTagData.PermissionAtLevelID = permissionAtLevelID;
            organisationTagData.FeatureFor = FeatureFor;
            await new OrganisationTagServiceDataLayer().SaveOrganisationTagAsync(organisationTagData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            organisationTagData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return organisationTagData;
    }
}


