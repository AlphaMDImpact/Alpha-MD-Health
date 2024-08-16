using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientDataLayer;

/// <summary>
/// represents resources database module
/// </summary>
public class ResourceDatabase : BaseDatabase
{
    /// <summary>
    /// insert or update record in the database
    /// </summary>
    /// <param name="resourceData">Object with modified resources and refence for operation status</param>
    /// <returns>Operation Status</returns>
    public async Task SaveResourcesAsync(BaseDTO resourceData)
    {
        await SqlConnection.RunInTransactionAsync(transaction =>
        {
            if (resourceData.LastModifiedON == null)
            {
                //When Last sync datetime is default datetime, clear all existing records from DB and insert new one for avoid duplicate record issue as each server will contains different ServerID(PrimaryKey) and InsertOrReplace() function update records based on PrimaryKey
                transaction.Execute("DELETE FROM ResourceModel");
            }
            if (GenericMethods.IsListNotEmpty(resourceData.Resources))
            {
                foreach (ResourceModel resource in resourceData.Resources)
                {
                    if (resource.IsActive)
                    {
                        transaction.InsertOrReplace(resource);
                    }
                    else
                    {
                        transaction.Execute($"DELETE FROM ResourceModel WHERE ResourceID = ?", resource.ResourceID);
                    }
                }
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves Resources from database for given group list
    /// </summary>
    /// <param name="resourceData">Object to hold resource data with operation status</param>
    /// <param name="getCommonResources">Bool to decided whether common resources are to be fetched or not</param>
    /// <param name="groupList">List of groups for which resources has to be retrieved</param>
    /// <returns>List of all resources by given grouplist</returns>
    public async Task GetResourcesAsync(BaseDTO resourceData, bool getCommonResources, params string[] groupList)
    {
        List<string> group = groupList?.ToList() ?? new List<string>();
        if (getCommonResources)
        {
            group.Add(GroupConstants.RS_COMMON_GROUP);
        }
        resourceData.Resources = await SqlConnection.QueryAsync<ResourceModel>(
            $"SELECT * FROM ResourceModel WHERE LanguageID = ? AND GroupName IN ('{string.Join("','", group)}')", resourceData.LanguageID
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves Resources from database
    /// </summary>
    /// <param name="resourceData">Object to hold resource data with operation status</param>
    /// <returns>List of all resources</returns>
    public async Task GetResourcesAsync(BaseDTO resourceData)
    {
        resourceData.Resources = await SqlConnection.QueryAsync<ResourceModel>(
            "SELECT * FROM ResourceModel WHERE IsActive = 1"
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves resources for given resource key
    /// </summary>
    /// <param name="resourceKey">Key for which resources to be retrieved</param>
    /// <returns>Resource model for requested key</returns>
    public async Task<ResourceModel> GetResourceAsync(string resourceKey, byte languageID)
    {
        return (await SqlConnection.QueryAsync<ResourceModel>(
            $"SELECT * FROM ResourceModel WHERE ResourceKey = ? AND LanguageID = ?", resourceKey, languageID
        ).ConfigureAwait(false)).FirstOrDefault();
    }

    /// <summary>
    /// Gets static resource count 
    /// </summary>
    /// <returns>no of static resources available in DB</returns>
    public async Task<int> GetStaticResourcesCountAsync(byte languageID)
    {
        var resourceCount = await SqlConnection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM ResourceModel WHERE IsDynamic = 0 AND LanguageID = ?", languageID).ConfigureAwait(false);
        //LibGenericMethods.LogData($"**********|GetStaticResourcesCountAsync().resourceCount : {resourceCount}|**********");
        return resourceCount;
    }
}