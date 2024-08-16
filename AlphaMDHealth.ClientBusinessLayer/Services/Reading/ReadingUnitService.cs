using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading units implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    /// <summary>
    /// Convert Reading Units
    /// </summary>
    /// <param name="readings">Readings data to Convert</param>
    /// <returns>Readings data in Converted unit</returns>
    public async Task ConvertReadingUnitsAsync(PatientReadingDTO readings)
    {
        MapUnitsDataIntoUnitConverter(readings);
        if (GenericMethods.IsListNotEmpty(readings.ListData))
        {
            foreach (var reading in readings.ListData)
            {
                var readingType = readings.ChartMetaData.FirstOrDefault(x => x.ReadingID == reading.ReadingID);
                if (readingType != null)
                {
                    await CallUnitConverterAsync(readingType, reading, true).ConfigureAwait(false);
                }
            }
        }
        else
        {
            readings.ListData = readings.ListData ?? new List<PatientReadingUIModel>() { new PatientReadingUIModel() };
        }
    }

    private void MapUnitsDataIntoUnitConverter(PatientReadingDTO readings)
    {
        if (!MobileConstants.IsMobilePlatform
            && (!GenericMethods.IsListNotEmpty(UnitConverter._unitData.UserAccountSettings)
            || !GenericMethods.IsListNotEmpty(UnitConverter._unitData.ReadingUnits)))
        {
            UnitConverter._unitData.UserAccountSettings = readings.UserAccountSettings;
            UnitConverter._unitData.ReadingUnits = readings.ReadingUnits;
        }
    }

    /// <summary>
    /// Maps json object into Model and saves data into DB
    /// </summary>
    /// <param name="result">Object which holds Operation status</param>
    /// <param name="data">json object to fetch data from it</param>
    /// <returns>Operation status with record count</returns>
    internal async Task MapAndSaveUnits(DataSyncModel result, JToken data)
    {
        try
        {
            PatientReadingDTO readingData = new PatientReadingDTO();
            if (result.SyncFor == DataSyncFor.Units.ToString())
            {
                readingData.ReadingUnits = MapReadingUnits(data, nameof(DataSyncDTO.Units));
            }
            else
            {
                readingData.ReadingUnitsI18N = MapReadingUnitsI18N(data, nameof(DataSyncDTO.UnitsI18N));
            }
            if (GenericMethods.IsListNotEmpty(readingData.ReadingUnits) || GenericMethods.IsListNotEmpty(readingData.ReadingUnitsI18N))
            {
                await _readingDB.SaveUnitsAsync(readingData).ConfigureAwait(false);
                result.RecordCount = 1;
            }
            result.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    private List<UnitModel> MapReadingUnits(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select new UnitModel
               {
                   UnitID = (byte)dataItem[nameof(UnitModel.UnitID)],
                   UnitIdentifier = (string)dataItem[nameof(UnitModel.UnitIdentifier)],
                   UnitGroupID = (int)dataItem[nameof(UnitModel.UnitGroupID)],
                   IsBaseUnit = (bool)dataItem[nameof(UnitModel.IsBaseUnit)],
                   BaseConversionFactor = (float)dataItem[nameof(UnitModel.BaseConversionFactor)],
                   IsActive = (bool)dataItem[nameof(UnitModel.IsActive)],
                   ShortUnitName = (string)dataItem[nameof(UnitModel.ShortUnitName)],
               }).ToList()
            : null;
    }

    private List<UnitI18NModel> MapReadingUnitsI18N(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0) ?
                   (from dataItem in data[collectionName]
                    select new UnitI18NModel
                    {
                        UnitID = (int)dataItem[nameof(UnitI18NModel.UnitID)],
                        FullUnitName = (string)dataItem[nameof(UnitI18NModel.FullUnitName)],
                        ShortUnitName = (string)dataItem[nameof(UnitI18NModel.ShortUnitName)],
                        LanguageID = (byte)dataItem[nameof(UnitI18NModel.LanguageID)],
                        IsActive = (bool)dataItem[nameof(UnitI18NModel.IsActive)]
                    }).ToList() : null;
    }

    #region Unit Conversions

    private async Task ConvertMetaDataUnitAsync(PatientReadingDTO readingData)
    {
        if (GenericMethods.IsListNotEmpty(readingData.ChartMetaData))
        {
            MapUnitsDataIntoUnitConverter(readingData);
            foreach (var metaData in readingData.ChartMetaData)
            {
                // Fetch Convert target ranges to user units
                metaData.TargetMinValue = await ConvertMetaDataUnitAsync(metaData, metaData.TargetMinValue);
                metaData.TargetMaxValue = await ConvertMetaDataUnitAsync(metaData, metaData.TargetMaxValue);
                // Convert ranges to user units
                metaData.AbsoluteMinValue = await ConvertMetaDataUnitAsync(metaData, metaData.AbsoluteMinValue);
                metaData.AbsoluteMaxValue = await ConvertMetaDataUnitAsync(metaData, metaData.AbsoluteMaxValue);
                metaData.NormalMinValue = await ConvertMetaDataUnitAsync(metaData, metaData.NormalMinValue);
                metaData.NormalMaxValue = await ConvertMetaDataUnitAsync(metaData, metaData.NormalMaxValue);

                if (metaData.ReadingValue.HasValue)
                {
                    metaData.ReadingValue = await ConvertMetaDataUnitAsync(metaData, metaData.ReadingValue);
                }
            }
        }
    }

    private async Task<double?> ConvertMetaDataUnitAsync(ReadingMetadataUIModel metaData, double? valueToConvert)
    {
        var convertedUnitData = await UnitConverter.ConvertToUserUnitAsync(valueToConvert, metaData.BaseUnitIdentifier, metaData.ReadingID, metaData.DigitsAfterDecimalPoint).ConfigureAwait(true);
        metaData.UnitIdentifier = convertedUnitData.Key;
        metaData.Unit = convertedUnitData.Value.Key;
        return convertedUnitData.Value.Value;
    }

    private async Task CallUnitConverterAsync(ReadingMetadataUIModel metadata, PatientReadingUIModel reading, bool isBaseUnit)
    {
        var unitData = isBaseUnit
            ? await UnitConverter.ConvertToUserUnitAsync(reading.ReadingValue, metadata.BaseUnitIdentifier, metadata?.ReadingID ?? 0, metadata.DigitsAfterDecimalPoint).ConfigureAwait(true)
            : await UnitConverter.ConvertToBaseUnitAsync(reading.ReadingValue, reading.UnitIdentifier).ConfigureAwait(true);
        reading.UnitIdentifier = unitData.Key;
        reading.Unit = unitData.Value.Key;
        reading.ReadingValue = unitData.Value.Value;
        if (isBaseUnit)
        {
            metadata.UnitIdentifier = reading.UnitIdentifier;
            metadata.Unit = reading.Unit;
        }
    }

    private async Task CallUnitConverterAsync(ReadingMetadataUIModel metadata, PatientReadingModel reading, bool isBaseUnit)
    {
        var unitData = isBaseUnit
            ? await UnitConverter.ConvertToUserUnitAsync(reading.ReadingValue, metadata.BaseUnitIdentifier, metadata?.ReadingID ?? 0, metadata.DigitsAfterDecimalPoint).ConfigureAwait(true)
            : await UnitConverter.ConvertToBaseUnitAsync(reading.ReadingValue, reading.UnitIdentifier).ConfigureAwait(true);
        reading.UnitIdentifier = unitData.Key;
        //reading.Unit = unitData.Value.Key;
        reading.ReadingValue = unitData.Value.Value;
        if (isBaseUnit)
        {
            metadata.UnitIdentifier = reading.UnitIdentifier;
            //metadata.Unit = reading.Unit;
        }
    }

    #endregion
}
