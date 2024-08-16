using System.Globalization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading BMI implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    #region BMI Calculations

    private async Task CalculateBmiAsync(PatientReadingDTO readingsData)
    {
        List<PatientReadingUIModel> weightData = readingsData.ListData.Where(x => x.ReadingID == ResourceConstants.R_WEIGHT_KEY_ID).ToList();
        if (GenericMethods.IsListNotEmpty(weightData))
        {
            var newReadingsData = new PatientReadingDTO
            {
                SelectedUserID = readingsData.SelectedUserID,
                LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0),
                ListData = weightData,
                ReadingIDs = $"{ResourceConstants.R_BMI_KEY_ID},{ResourceConstants.R_HEIGHT_KEY_ID}",
                ErrCode = ErrorCode.OK,
            };
            // Get BMI metadata
            // Get Latest Height value to calculate BMI 
            // Get Existing BMI data in case of Edit Weight to update latest BMI Value
            await _readingDB.GetDataToCalculateBmiAsync(newReadingsData).ConfigureAwait(false);
            if (newReadingsData.ErrCode == ErrorCode.OK)
            {
                var bmiMetaData = newReadingsData.ChartMetaData.FirstOrDefault(x => x.ReadingID == ResourceConstants.R_BMI_KEY_ID);
                var heightMetaData = newReadingsData.ChartMetaData.FirstOrDefault(x => x.ReadingID == ResourceConstants.R_HEIGHT_KEY_ID);
                foreach (var reading in weightData)
                {
                    CreateBmiReading(readingsData, newReadingsData, bmiMetaData, heightMetaData, reading);
                }
            }
        }
    }

    private void CreateBmiReading(PatientReadingDTO readingsData, PatientReadingDTO newReadingsData, ReadingMetadataUIModel bmiMetaData, ReadingMetadataUIModel heightMetaData, PatientReadingUIModel reading)
    {
        var heightData = newReadingsData.ListData.Where(x => x.ReadingID == heightMetaData.ReadingID && x.ReadingDateTime <= reading.ReadingDateTime)?.OrderByDescending(x => x.ReadingDateTime).FirstOrDefault();
        if (heightData != null)
        {
            PatientReadingUIModel bmiReading = null;
            var existingWeight = newReadingsData.ListData.FirstOrDefault(x => x.ReadingID == reading.ReadingID && x.PatientReadingID == reading.PatientReadingID);
            if (existingWeight != null)
            {
                bmiReading = newReadingsData.ListData.FirstOrDefault(x => x.ReadingID == bmiMetaData.ReadingID && x.ReadingDateTime == existingWeight.ReadingDateTime);
            }
            bmiReading = bmiReading ?? new PatientReadingUIModel
            {
                PatientReadingID = GenericMethods.GenerateGuid(),
                AddedByID = Convert.ToString(reading.AddedByID, CultureInfo.InvariantCulture),
                ReadingID = bmiMetaData.ReadingID,
                UserID = reading.UserID,
            };
            bmiReading.ReadingSourceType = reading.ReadingSourceType;
            bmiReading.ReadingNotes = reading.ReadingNotes;
            bmiReading.IsActive = reading.IsActive;
            bmiReading.ReadingDateTime = reading.ReadingDateTime;
            bmiReading.ReadingValue = CalculateBmiValue(reading.ReadingValue.Value, heightData.ReadingValue.Value, bmiMetaData.DigitsAfterDecimalPoint);
            readingsData.ListData.Add(bmiReading);
        }
    }

    /// <summary>
    /// Calculates BMI based on height and weight
    /// </summary>
    /// <param name="weightInKg">Weight reading in Base Unit (Kg)</param>
    /// <param name="heightInCm">Height reading in Base Unit (cm)</param>
    /// <param name="digitsAfterDecimal">decimal precisions to display</param>
    /// <returns>BMI value in Base Unit (kg/m2)</returns>
    private double CalculateBmiValue(double weightInKg, double heightInCm, byte digitsAfterDecimal)
    {
        //// When weight is 16.9 kg and height is 105.4 cm then BMI will be :  
        //// (16.9 kg / 105.4 cm / 105.4 cm ) x 10000 = 15.2 kg/m2
        double mainValue = weightInKg / heightInCm / heightInCm;
        double value = mainValue * 10000;
        var finalValue = Math.Round(value, digitsAfterDecimal);
        return finalValue;
    }

    #endregion
}
