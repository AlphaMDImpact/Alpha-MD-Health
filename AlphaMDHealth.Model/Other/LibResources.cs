namespace AlphaMDHealth.Model;

/// <summary>
/// class containing common resource methods
/// </summary>
public static class LibResources
{
    /// <summary>
    /// Gets Resource by key
    /// </summary>
    /// <param name="resources">resource Data</param>
    /// <param name="resourceKey">Resource key</param>
    /// <returns>Resource data for the given key</returns>
    public static ResourceModel GetResourceByKey(IEnumerable<ResourceModel> resources, string resourceKey)
    {
        if (resources != null)
        {
            return resources.FirstOrDefault(x => x.ResourceKey.Trim() == resourceKey);
        }
        return null;
    }

    /// <summary>
    /// Gets resource by key ID from available list of resources
    /// </summary>
    /// <param name="resources">resource Data</param>
    /// <param name="resourceKeyID">Resource ID</param>
    /// <returns>Resource data</returns>
    public static ResourceModel GetResourceByKeyID(IEnumerable<ResourceModel> resources, long resourceKeyID)
    {
        if (resources != null)
        {
            return resources.FirstOrDefault(r => r.ResourceKeyID == resourceKeyID);
        }
        return null;
    }

    /// <summary>
    /// Get Resource Value based upon key
    /// </summary>
    /// <param name="resources">resource Data</param>
    /// <param name="resourceKey">Key to fetch Resource Value</param>
    /// <param name="languageID">selected language id</param>
    /// <returns></returns>
    public static string GetResourceValueByKey(IEnumerable<ResourceModel> resources, string resourceKey, byte languageID = default)
    {
        if (resources != null)
        {
            return resources.FirstOrDefault(x => x.ResourceKey == resourceKey && IsLanguageMatched(languageID, x))?.ResourceValue ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Get Resource Value based upon key
    /// </summary>
    /// <param name="resources">resource Data</param>
    /// <param name="resourceKeyID">Key to fetch Resource Value</param>
    /// <param name="languageID">selected language id</param>
    /// <returns></returns>
    public static string GetResourceValueByKeyID(IEnumerable<ResourceModel> resources, int resourceKeyID, byte languageID = default)
    {
        if (resources != null)
        {
            return resources.FirstOrDefault(x => x.ResourceKeyID == resourceKeyID && IsLanguageMatched(languageID, x))?.ResourceValue ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Gets resource key id by key from available list of resources
    /// </summary>
    /// <param name="resources">resource Data</param>
    /// <param name="resourceKey">Key to fetch Resource key id</param>
    /// <returns>Resource data</returns>
    public static int GetResourceKeyIDByKey(IEnumerable<ResourceModel> resources, string resourceKey)
    {
        if (resources != null)
        {
            return resources.FirstOrDefault(r => r.ResourceKey?.Trim() == resourceKey?.Trim())?.ResourceKeyID ?? 0;
        }
        return 0;
    }

    /// <summary>
    /// Gets resource key by key id from available list of resources
    /// </summary>
    /// <param name="resources">resource Data</param>
    /// <param name="resourceKeyID">Key id to fetch Resource key</param>
    /// <returns>Resource data</returns>
    public static string GetResourceKeyByKeyID(IEnumerable<ResourceModel> resources, int resourceKeyID)
    {
        if (resources != null)
        {
            return resources.FirstOrDefault(r => r.ResourceKeyID == resourceKeyID)?.ResourceKey ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Get Resource keyDescription based upon key
    /// </summary>
    /// <param name="resources">resource Data</param>
    /// <param name="resourceKey">Key to fetch Resource Value</param>
    /// <param name="languageID">selected language id</param>
    /// <returns></returns>
    public static string GetResourceKeyDescByKey(IEnumerable<ResourceModel> resources, string resourceKey, byte languageID = default)
    {
        if (resources != null)
        {
            return resources.FirstOrDefault(x => x.ResourceKey == resourceKey && IsLanguageMatched(languageID, x))?.KeyDescription ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Gets resource key  by key from available list of resources
    /// </summary>
    /// <param name="resources">resource Data</param>
    /// <param name="resourceDesc">Resource desc</param>
    /// <returns>Resource key</returns>
    public static string GetResourceKeyByKeyDescription(IEnumerable<ResourceModel> resources, string resourceDesc)
    {
        if (resources != null)
        {
            return resources?.FirstOrDefault(r => r.KeyDescription?.Trim() == resourceDesc?.Trim())?.ResourceKey;
        }
        return null;
    }

    /// <summary>
    /// Get Resource Info based upon key
    /// </summary>
    /// <param name="resources">resource Data</param>
    /// <param name="resourceKey">Key to fetch Resource Value</param>
    /// <param name="languageID">selected language id</param>
    /// <returns></returns>
    public static string GetResourceInfoByKey(IEnumerable<ResourceModel> resources, string resourceKey, byte languageID = default)
    {
        if (resources != null)
        {
            return resources.FirstOrDefault(x => x.ResourceKey == resourceKey && IsLanguageMatched(languageID, x))?.InfoValue ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Get Resource placeholder based upon key
    /// </summary>
    /// <param name="resources">resource Data</param>
    /// <param name="resourceKey">Key to fetch Resource Value</param>
    /// <param name="languageID">selected language id</param>
    /// <returns></returns>
    public static string GetResourcePlaceHolderByKey(IEnumerable<ResourceModel> resources, string resourceKey, byte languageID = default)
    {
        if (resources != null)
        {
            return resources.FirstOrDefault(x => x.ResourceKey == resourceKey && IsLanguageMatched(languageID, x))?.PlaceHolderValue?.Trim() ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Gets resource group description by group id from available list of resources
    /// </summary>
    /// <param name="resources">configuration Data</param>
    /// <param name="groupID">Resource Group IDKey</param>
    /// <returns>Resource group description</returns>
    public static string GetResourceGroupDescByGroupID(IEnumerable<ResourceModel> resources, short groupID)
    {
        if (resources != null)
        {
            return resources.FirstOrDefault(r => r.GroupID == groupID)?.GroupDesc;
        }
        return string.Empty;
    }

    /// <summary>
    /// Get if a field is required based upon he given key
    /// </summary>
    /// <param name="resources">resource Data</param>
    /// <param name="resourceKey">Key to fetch is a field is required</param>
    /// <param name="languageID">selected language id</param>
    /// <returns></returns>
    public static bool IsRequired(IEnumerable<ResourceModel> resources, string resourceKey, byte languageID = default)
    {
        if (resources != null)
        {
            bool? minLength = resources.FirstOrDefault(x => x.ResourceKey == resourceKey && IsLanguageMatched(languageID, x))?.IsRequired;
            return minLength != null && minLength == true;
        }
        return false;
    }

    /// <summary>
    /// Get Min length value based on resource key
    /// </summary>
    /// <param name="resources">configuration Data</param>
    /// <param name="resourceKey">resource key</param>
    /// <param name="languageID">selected language id</param>
    /// <returns>Min Value</returns>
    public static double? GetMinLengthValueByKey(IEnumerable<ResourceModel> resources, string resourceKey, byte languageID = default)
    {
        if (resources != null)
        {
            return resources.FirstOrDefault(x => x.ResourceKey == resourceKey && IsLanguageMatched(languageID, x))?.MinLength;
        }
        return default;
    }

    /// <summary>
    /// Get Max length value based on resource key
    /// </summary>
    /// <param name="resources">configuration Data</param>
    /// <param name="resourceKey">resource key</param>
    /// <param name="languageID">selected language id</param>
    /// <returns>Max Value</returns>
    public static double? GetMaxLengthValueByKey(IEnumerable<ResourceModel> resources, string resourceKey, byte languageID = default)
    {
        if (resources != null)
        {
            return resources.FirstOrDefault(x => x.ResourceKey == resourceKey && IsLanguageMatched(languageID, x))?.MaxLength;
        }
        return default;
    }

    private static bool IsLanguageMatched(byte languageID, ResourceModel x)
    {
        return (languageID == default || x.LanguageID == languageID);
    }
}