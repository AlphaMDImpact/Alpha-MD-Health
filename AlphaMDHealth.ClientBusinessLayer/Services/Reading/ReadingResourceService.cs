using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading resources implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    private bool ShouldMapReadingResource(bool isReadingAdd, ReadingMetadataUIModel reading)
    {
        return !isReadingAdd 
            || (MobileConstants.IsMobilePlatform && reading.AllowManualAdd)
            || (!MobileConstants.IsMobilePlatform && (reading.AllowManualAdd
            && (reading.ValueAddedBy == (short)ReadingAddedBy.ProviderKey || reading.ValueAddedBy == (short)ReadingAddedBy.BothKey)));
    }

    private void UpdateResourceMinAndMaxLength(ReadingMetadataUIModel metadata, ResourceModel resource)
    {
        if (metadata.AbsoluteMinValue.HasValue)
        {
            resource.MinLength = metadata.AbsoluteMinValue.Value;
        }
        if (metadata.AbsoluteMaxValue.HasValue)
        {
            resource.MaxLength = metadata.AbsoluteMaxValue.Value;
        }
    }

    private void UpdateReadingResources(PatientReadingDTO readingData, bool isAdd)
    {
        // Create a list to store the indices of items to be added
        List<int> indicesToAdd = new List<int>();

        // Iterate over the collection indices
        for (int i = 0; i < readingData.ChartMetaData.Count; i++)
        {
            ReadingMetadataUIModel metadata = readingData.ChartMetaData[i];
            var resource = LibResources.GetResourceByKeyID(PageData?.Resources, metadata.ReadingID);
            if (!string.IsNullOrWhiteSpace(metadata.Unit))
            {
                resource.ResourceValue = $"{resource.ResourceValue} ({metadata.Unit}) ";
            }
            if (ShouldMapReadingResource(isAdd, metadata))
            {
                UpdateResourceMinAndMaxLength(metadata, resource);
                indicesToAdd.Add(i); // Add the index to the list
            }
        }

        // Iterate over the list of indices to add and call CreatePatientReadingUIModel
        foreach (int index in indicesToAdd)
        {
            CreatePatientReadingUIModel(readingData, readingData.ChartMetaData[index]);
        }
    }
}