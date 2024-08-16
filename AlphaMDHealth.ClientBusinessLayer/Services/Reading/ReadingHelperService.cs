using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading resources implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    /// <summary>
    /// Is reading source type is of device or health app
    /// </summary>
    /// <param name="readingSourceType">source of the reading</param>
    /// <returns>true if source is device or health, else false</returns>
    public bool IsDeviceOrHealthAppReading(string readingSourceType)
    {
        return readingSourceType == ReadingSource.Device.ToString()
            || readingSourceType == ReadingSource.HealthKit.ToString()
            || readingSourceType == ReadingSource.GoogleFit.ToString();
    }

    /// <summary>
    /// Checks Add/Edit page view only
    /// </summary>
    /// <param name="readingData">readings page data</param>
    /// <param name="isPatientPage">is patient page</param>
    /// <returns>Flag representing Add/Edit page view only or not</returns>
    public bool IsAddEditPageViewOnly(PatientReadingDTO readingData, bool isPatientPage)
    {
        return IsDeviceOrHealthAppReading(readingData.ListData[0].ReadingSourceType)
            || !readingData.ChartMetaData[0].IsActive
            || (isPatientPage
                ? !(IsAddedByProvider(readingData) && readingData.ChartMetaData[0].AllowManualAdd
                    && readingData.ListData[0].AddedByID == LoggedInAccountID)
                : !(!IsAddedByProvider(readingData) && readingData.ChartMetaData[0].AllowManualAdd));
    }

    /// <summary>
    /// Checks Add operation is allowed or not
    /// </summary>
    /// <param name="metadata">readings metadata</param>
    /// <param name="isPatientPage">is patient login</param>
    /// <returns>Flag representing Add operation is allowed or not</returns>
    public bool IsAddAllowed(List<ReadingMetadataUIModel> metadata, bool isPatientPage)
    {
        string allowedRoles = GetAllowedRoles(isPatientPage);
        return GenericMethods.IsListNotEmpty(metadata)
            && metadata.Any(x => x.IsActive && x.AllowManualAdd && allowedRoles.Contains(x.ValueAddedBy.ToString()));
    }

    /// <summary>
    /// Checks Manual Add operation is allowed or not
    /// </summary>
    /// <param name="readingData">readings page data</param>
    /// <param name="isPatientLogin">is patient login</param>
    /// <returns>Flag representing Manual Add operation is allowed or not</returns>
    public bool IsManualAddAllowed(PatientReadingDTO readingData, bool isPatientLogin)
    {
        string allowedRoles = GetAllowedRoles(isPatientLogin);
        return allowedRoles.Contains(readingData.ChartMetaData[0].ValueAddedBy.ToString())
            && readingData.ChartMetaData[0].AllowManualAdd;
    }

    /// <summary>
    /// Checks Delete operation is allowed or not
    /// </summary>
    /// <param name="readingData">readings page data</param>
    /// <param name="isPatientPage">is patient page</param>
    /// <returns>Flag representing Delete operation is allowed or not</returns>
    //public bool IsDeleteAllowed(PatientReadingDTO readingData, bool isPatientPage)
    //{
    //    return !IsDeviceOrHealthAppReading(readingData.ListData[0].ReadingSourceType)
    //        && readingData.ChartMetaData[0].AllowDelete
    //        && (isPatientPage
    //            ? (IsAddedByProvider(readingData) && readingData.ListData[0].AddedByID == LoggedInAccountID)
    //            : !IsAddedByProvider(readingData));
    //}

    public bool IsEditAllowed(PatientReadingDTO readingData, bool IsPatientLogin)
    {
        if (LibPermissions.HasPermission(readingData.FeaturePermissions, AppPermissions.PatientReadingAddEdit.ToString())
            && GenericMethods.IsListNotEmpty(readingData.ChartMetaData)
            && GenericMethods.IsListNotEmpty(readingData.ListData))
        {
            var existingData = readingData.ListData[0];
            var metadata = readingData.ChartMetaData[0];
            return metadata.AllowManualAdd
                && IsAddedByAllowed(metadata, IsPatientLogin)
                && (readingData.PatientReadingID == Guid.Empty
                    || (existingData.ReadingSourceType == ReadingSource.ProviderManual.ToString()
                        && existingData.AddedByID == _essentials.GetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, Constants.CONSTANT_ZERO)));
        }
        return false;
    }

    public bool IsDeleteAllowed(PatientReadingDTO readingData, bool IsPatientLogin)
    {
        if (LibPermissions.HasPermission(readingData.FeaturePermissions, AppPermissions.PatientReadingDelete.ToString())
            && GenericMethods.IsListNotEmpty(readingData.ChartMetaData)
            && GenericMethods.IsListNotEmpty(readingData.ListData))
        {
            var existingData = readingData.ListData[0];
            var metadata = readingData.ChartMetaData[0];
            if (existingData.PatientReadingID != Guid.Empty
                && metadata.AllowDelete
                && IsAddedByAllowed(metadata, IsPatientLogin)
                && !IsDeviceOrHealthAppReading(existingData.ReadingSourceType)
                && existingData.ReadingSourceType == ReadingSource.ProviderManual.ToString()
                && existingData.AddedByID == _essentials.GetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, Constants.CONSTANT_ZERO))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsAddedByAllowed(ReadingMetadataUIModel metadata, bool IsPatientLogin)
    {
        return (IsPatientLogin)
            ? metadata.ValueAddedBy == (short)ReadingAddedBy.PatientKey || metadata.ValueAddedBy == (short)ReadingAddedBy.BothKey
            : metadata.ValueAddedBy == (short)ReadingAddedBy.ProviderKey || metadata.ValueAddedBy == (short)ReadingAddedBy.BothKey;
    }

    /// <summary>
    /// Fetch food name options
    /// </summary>
    /// <returns>List of food options to search or enter manually</returns>
    public List<OptionModel> GetFoodEntryOptions()
    {
        return new List<OptionModel>
        {
            new OptionModel {
                OptionID = 1,
                GroupName = ResourceConstants.R_MANUAL_TEXT_KEY,
                OptionText = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_MANUAL_TEXT_KEY),
                IsSelected = true
            },
            new OptionModel {
                OptionID = 2,
                GroupName = ResourceConstants.R_SEARCH_FOOD_TEXT_KEY,
                OptionText = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_SEARCH_FOOD_TEXT_KEY),
                IsSelected = false
            },
        };
    }

    /// <summary>
    /// Fetch child readings for selected id
    /// </summary>
    /// <param name="readingData">metadata to search</param>
    /// <param name="readingID">reading id to find its childs</param>
    /// <returns>KList of child metadata</returns>
    public List<ReadingMetadataUIModel> GetChildReadings(PatientReadingDTO readingData, long readingID)
    {
        return readingData.ChartMetaData?.Where(x => x.ReadingParentID == readingID || x.ReadingID == readingID)?.OrderBy(x => x.SequenceNo)?.ToList();
    }

}