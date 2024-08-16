using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading charts implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    /// <summary>
    /// Generate Chart Control with given data
    /// </summary>
    /// <param name="readingsData">reference Data</param>
    /// <param name="isSummaryView">is Summary View</param>
    /// <returns>Returns chart data</returns>
    public Task GenerateChartDataAsync(PatientReadingDTO readingsData, bool isSummaryView)
    {
        readingsData.SummaryDataOptions = new List<OptionModel>();
        var laterstReading = readingsData.ListData?.FirstOrDefault();
        var latestValue = laterstReading?.ReadingValue;
        ReadingMetadataUIModel metadata = readingsData.ChartMetaData.FirstOrDefault(x => x.ReadingID == laterstReading?.ReadingID);
        long readingID;
        var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
        readingsData.SummaryDataOptions.Add(new OptionModel
        {
            GroupName = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_READING_CURRENT_HEADING_KEY),
            OptionText = latestValue == null ? Constants.SYMBOL_DOUBLE_HYPHEN : (!string.IsNullOrWhiteSpace(GetSecuredValueAsync(nameof(TempSessionModel.TokenIdentifier)).Result)) ?$"{ GetLatestValue(readingsData, readingsData.ChartMetaData, out readingID) } { metadata?.Unit }" : $" {Constants.COLON_KEY} {GetLatestValue(readingsData, readingsData.ChartMetaData, out readingID)} {metadata?.Unit} ",
            OptionID = LibResources.GetResourceKeyIDByKey(PageData?.Resources,ResourceConstants.R_READING_CURRENT_HEADING_KEY),
            ParentOptionText = metadata?.Unit
        });
        MapChartDataAndMetadata(readingsData, isSummaryView);
        if (metadata?.TargetMaxValue > 0)
        {
            SetTargetLatestValue(readingsData, metadata);
        }
        return Task.CompletedTask;
    }

    private void SetTargetLatestValue(PatientReadingDTO readingsData, ReadingMetadataUIModel metadata)
    {
        var minTarget = "";
        var maxTarget = "";
        var targetValue = "";
        if (metadata.ReadingParentID == ResourceConstants.R_BLOOD_PRESSURE_KEY_ID)
        {
            var sysTarget = readingsData.ChartMetaData?.FirstOrDefault(x => x.ReadingID == ResourceConstants.R_BP_SYSTOLIC_KEY_ID);
            var diaTarget = readingsData.ChartMetaData?.FirstOrDefault(x => x.ReadingID == ResourceConstants.R_BP_DIASTOLIC_KEY_ID);
            minTarget = sysTarget?.TargetMinValue == sysTarget?.TargetMaxValue ? $"{sysTarget?.TargetMinValue}" : $"{sysTarget?.TargetMinValue}-{sysTarget?.TargetMaxValue}";
            maxTarget = diaTarget?.TargetMinValue == diaTarget?.TargetMaxValue ? $"{diaTarget?.TargetMinValue}" : $"{diaTarget?.TargetMinValue}-{diaTarget?.TargetMaxValue}";
            targetValue = $"{minTarget}/{maxTarget}";
        }
        else
        {
            minTarget = Convert.ToString(metadata.TargetMinValue, CultureInfo.InvariantCulture);
            maxTarget = Convert.ToString(metadata.TargetMaxValue, CultureInfo.InvariantCulture);
            targetValue = minTarget == maxTarget ? minTarget : $"{minTarget}-{maxTarget}";
        }
        readingsData.SummaryDataOptions.Add(new OptionModel
        {
            GroupName = (!string.IsNullOrWhiteSpace(GetSecuredValueAsync(nameof(TempSessionModel.TokenIdentifier)).Result)) ? $" {LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_GOAL_VALUE_KEY)}" : $"{Constants.PIPE_SEPERATOR_WITH_SPACE}{LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_GOAL_VALUE_KEY)}",
            OptionText = (!string.IsNullOrWhiteSpace(GetSecuredValueAsync(nameof(TempSessionModel.TokenIdentifier)).Result)) ? $"{targetValue} {metadata.Unit}" : $"{Constants.COLON_KEY} {targetValue} {metadata.Unit}",
            OptionID = LibResources.GetResourceKeyIDByKey(PageData?.Resources, ResourceConstants.R_READING_CURRENT_HEADING_KEY),
            ParentOptionText = metadata.Unit
        });
    }

    private void MapChartDataAndMetadata(PatientReadingDTO readingsData, bool isSummaryView)
    {
        LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        MapPatientReadingDetailsListUIData(readingsData, dayFormat, monthFormat, yearFormat, readingsData.ChartMetaData);
        if (!isSummaryView && GenericMethods.IsListNotEmpty(readingsData.ListData))
        {
            readingsData.ListData = readingsData.ListData.OrderByDescending(x => x.ReadingDateTime.Value).ThenBy(x => x.SequenceNo).ToList();
            if ((short)readingsData.FilterOptions.FirstOrDefault(x => x.IsSelected).ParentOptionID == -1)
            {
                AdjustStartEndDates(readingsData);
            }
        }
    }
}
