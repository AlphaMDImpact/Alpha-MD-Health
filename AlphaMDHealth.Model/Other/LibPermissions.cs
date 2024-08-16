namespace AlphaMDHealth.Model;

public static class LibPermissions
{
    /// <summary>
    /// Check if feature permission is given or not
    /// </summary>
    /// <param name="permissions">list of feature permissions</param>
    /// <param name="featureCode">name of feature to access</param>
    /// <returns>true if permission is given else false</returns>
    public static bool HasPermission(IEnumerable<OrganizationFeaturePermissionModel> permissions, string featureCode)
    {
        if (permissions?.Count() > 0)
        {
            return permissions.FirstOrDefault(x => x.FeatureCode == featureCode)?.HasPermission ?? false;
        }
        return false;
    }

    /// <summary>
    /// Check if feature permission is given or not
    /// </summary>
    /// <param name="permissions">list of feature permissions</param>
    /// <param name="featureCode">name of feature to access</param>
    /// <returns>true if permission is given else false</returns> 
    public static bool HasActivePermission(IEnumerable<OrganizationFeaturePermissionModel> permissions, string featureCode)
    {
        if (permissions?.Count() > 0)
        {
            return permissions.Any(x => x.FeatureCode == featureCode && x.IsActive);
        }
        return false;
    }

    /// <summary>
    /// Get feature permission text
    /// </summary>
    /// <param name="permissions">list of feature permissions</param>
    /// <param name="featureCode">name of feature to access</param>
    /// <returns>permission text</returns>
    public static string GetFeatureText(IEnumerable<OrganizationFeaturePermissionModel> permissions, string featureCode)
    {
        if (permissions?.Count() > 0)
        {
            return permissions.FirstOrDefault(x => x.FeatureCode == featureCode)?.FeatureText ?? string.Empty;
        }
        return string.Empty;
    }
}