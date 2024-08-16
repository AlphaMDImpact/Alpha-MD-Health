using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer;

public class MasterServiceBusinessLayer : BaseServiceBusinessLayer
{
    /// <summary>
    /// Master page service
    /// </summary>
    /// <param name="httpContext">Instance of HttpContext</param>
    public MasterServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
    {
    }

    /// <summary>
    /// Get master page data from DB
    /// </summary>
    /// <param name="languageID">Users selected language ID</param>
    /// <param name="selectedUserID">Selected user Id</param>
    /// <param name="organisationDomain">Client URl</param>
    /// <returns>Master page data with operation status</returns>
    public async Task<MasterDTO> GetMasterDataAsync(byte languageID, long selectedUserID, string organisationDomain, long userAccountID)
    {
        MasterDTO masterData = new MasterDTO { ErrCode = ErrorCode.OK };
        try
        {
            if (string.IsNullOrWhiteSpace(organisationDomain))
            {
                masterData.ErrCode = ErrorCode.InvalidData;
                return masterData;
            }
            masterData.AccountID = AccountID;
            // Get Domain based upon Url Path
            GetDomain(organisationDomain, masterData);
            if (masterData.ErrCode == ErrorCode.OK)
            {
                masterData.SelectedUserID = selectedUserID;
                masterData.LanguageID = languageID;

                masterData.Session = new SessionModel();
                MapHeadersInSession(_httpContext.Request.Headers, masterData.Session);
                await new MasterServiceDataLayer().GetMasterDataAsync(masterData, userAccountID).ConfigureAwait(false);

                masterData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources,
                    $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_PROFILE_GROUP}",
                    masterData.LanguageID > 0 ? masterData.LanguageID : (byte)1,
                    default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (masterData.ErrCode == ErrorCode.OK)
                {
                    if (GenericMethods.IsListNotEmpty(masterData.Resources))
                    {
                        await ReplaceSettingImageCdnLinkAsync(masterData);
                        masterData.Settings.AddRange((await GetDataFromCacheAsync(CachedDataType.Settings, $"{GroupConstants.RS_ENVIRONMENT_GROUP},{GroupConstants.RS_ORGANISATION_SETTINGS_GROUP},{GroupConstants.RS_COMMON_GROUP}", languageID, default, 0, 0, false).ConfigureAwait(false)).Settings);
                        masterData.Settings.AddRange((await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, $"{GroupConstants.RS_ORGANISATION_SETTINGS_GROUP}", languageID, default, 0, masterData.OrganisationID, false).ConfigureAwait(false)).Settings);
                        await new UserServiceBusinessLayer(_httpContext).ReplaceUserImageCdnLinkAsync(masterData.Users);
                        await ReplaceMenuGroupImageCdnLinkAsync(masterData);
                    }
                    else
                    {
                        masterData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            masterData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        finally
        {
            masterData.Session = null;
        }
        return masterData;
    }

    private async Task ReplaceMenuGroupImageCdnLinkAsync(MasterDTO masterData)
    {
        if (GenericMethods.IsListNotEmpty(masterData.MenuGroups))
        {
            BaseDTO cdnCacheData = new BaseDTO();
            foreach (var detail in masterData.MenuGroups)
            {
                if (!string.IsNullOrWhiteSpace(detail.Content))
                {
                    detail.Content = await ReplaceCDNLinkAsync(detail.Content, cdnCacheData);
                }
            }
        }
    }

    private void GetDomain(string orgainsationDomain, MasterDTO masterData)
    {
        Uri organisationUrl = new Uri(orgainsationDomain.ToLowerInvariant());
        if (organisationUrl.AbsolutePath == null || organisationUrl.AbsolutePath == Constants.SYMBOL_SLASH.ToString(CultureInfo.InvariantCulture))
        {
            masterData.OrganisationDomain = organisationUrl.Host.Trim();
        }
        else
        {
            string[] pathSplit = organisationUrl.AbsolutePath.Split(Constants.SYMBOL_SLASH);
            if (pathSplit.Count() > 2)
            {
                SetOganisationDomain(masterData, organisationUrl, pathSplit);
            }
            else
            {
                if (!organisationUrl.AbsolutePath.Contains(AppPermissions.StaticMessageView.ToString(), StringComparison.InvariantCultureIgnoreCase) && (organisationUrl.AbsolutePath.Contains(Constants.ORGANISATION_STRING_CONST, StringComparison.InvariantCulture)
                    && !organisationUrl.AbsolutePath.Contains(Constants.ORGANISATION_SETUP_STRING_CONST, StringComparison.InvariantCulture)))
                {
                    masterData.ErrCode = ErrorCode.NoDomainFound;
                }
                else
                {
                    masterData.OrganisationDomain = organisationUrl.Host.Trim();
                }
            }
        }
        if (masterData.OrganisationDomain == "localhost")
        {
            masterData.OrganisationDomain = UrlConstants.LOCAL_SERVICE_ORGANISATION_DOMAIN;
        }
    }

    private void SetOganisationDomain(MasterDTO masterData, Uri organisationUrl, string[] pathSplit)
    {
        if (pathSplit[1] == Constants.ORGANISATION_STRING_CONST)
        {
            masterData.OrganisationDomain = organisationUrl.AbsolutePath.Split(Constants.SYMBOL_SLASH)[2].Trim();
        }
        else
        {
            masterData.OrganisationDomain = organisationUrl.Host.Trim();
        }
    }
}