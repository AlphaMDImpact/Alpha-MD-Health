using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Units converter to convert data from one unit to another
/// </summary>
public class UnitConverterService : BaseService
{
    public UnitConverterService(IEssentials serviceEssentials) : base(serviceEssentials)
    {

    }
    internal readonly PatientReadingDTO _unitData = new PatientReadingDTO { ReadingUnits = new List<UnitModel>() };
    bool isLoading;

    /// <summary>
    /// Converts Unit data to base units
    /// </summary>
    /// <param name="value">value to convert</param>
    /// <param name="unitIdentifier">unit of current value</param>
    /// <returns>Converted value with unit</returns>
    public async Task<KeyValuePair<string, KeyValuePair<string, double?>>> ConvertToBaseUnitAsync(double? value, string unitIdentifier)
    {
        UnitModel convertedUnit = null;
        if (await CheckUnitsMasterDataAsync().ConfigureAwait(true))
        {
            //// Check when received Unit is BaseUnit, conversion is not required
            convertedUnit = _unitData.ReadingUnits.Find(x => x.UnitIdentifier == unitIdentifier);
            if (convertedUnit != null && !convertedUnit.IsBaseUnit)
            {
                //// Fetch BaseUnit metadata
                UnitModel baseUnit = _unitData.ReadingUnits.Find(x => x.IsBaseUnit && x.UnitGroupID == convertedUnit.UnitGroupID);
                if (baseUnit != null)
                {
                    value = value == null ? value : ConvertValue(value.Value, convertedUnit, baseUnit);
                    convertedUnit = baseUnit;
                }
            }
        }
        convertedUnit = convertedUnit ?? new UnitModel { UnitIdentifier = unitIdentifier, ShortUnitName = unitIdentifier };
        return new KeyValuePair<string, KeyValuePair<string, double?>>(convertedUnit.UnitIdentifier, new KeyValuePair<string, double?>(convertedUnit.ShortUnitName, value));
    }

    /// <summary>
    /// Converts Unit data to user units
    /// </summary>
    /// <param name="value">value to convert</param>
    /// <param name="unitIdentifier">unit of current value</param>
    /// <param name="readingID">reading id</param>
    /// <param name="digitsAfterDecimal">decimal precisions to display</param>
    /// <returns>Converted value with unit</returns>
    public async Task<KeyValuePair<string, KeyValuePair<string, double?>>> ConvertToUserUnitAsync(double? value, string unitIdentifier, short readingID, byte digitsAfterDecimal)
    {
        UnitModel convertedUnit = null;
        if (await CheckUnitsMasterDataAsync().ConfigureAwait(true))
        {
            convertedUnit = _unitData.ReadingUnits.Find(x => x.UnitIdentifier == unitIdentifier);
            if (convertedUnit != null)
            {
                //// If received unit is not BaseUnit, then Convert data to Base Unit Then Convert to User Unit, Otherwise directly convert to UserUnit
                var readingTypeData = _unitData.UserAccountSettings?.Find(x => x.SettingTypeID == readingID);
                if (readingTypeData != null)
                {
                    byte userUnitID = Convert.ToByte(readingTypeData.SettingValue, CultureInfo.InvariantCulture);
                    UnitModel toUnit = _unitData.ReadingUnits.Find(x => x.UnitID == userUnitID);
                    //// If Received Unit is UserUnit, Conversion is not needed
                    if (toUnit != null && convertedUnit.UnitIdentifier != toUnit.UnitIdentifier)
                    {
                        value = value == null ? value : ConvertValue(value.Value, convertedUnit, toUnit);
                        convertedUnit = toUnit;
                    }
                }
            }
        }
        convertedUnit = convertedUnit ?? new UnitModel { UnitIdentifier = unitIdentifier, ShortUnitName = unitIdentifier };
        return new KeyValuePair<string, KeyValuePair<string, double?>>(convertedUnit.UnitIdentifier,
            new KeyValuePair<string, double?>(convertedUnit.ShortUnitName,
            value.HasValue
                ? Math.Round(value.Value, digitsAfterDecimal)
                : value));
    }

    private async Task<bool> CheckUnitsMasterDataAsync()
    {
        await Task.Run(() =>
        {
            while (isLoading)
            {
                //Wait here unitl units data is not loded
            }
        });
        if (GenericMethods.IsListNotEmpty(_unitData.ReadingUnits))
        {
            return true;
        }
        if (MobileConstants.IsMobilePlatform)
        {
            isLoading = true;
            _unitData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            await new ReadingDatabase().GetUnitsAsync(_unitData).ConfigureAwait(false);
            isLoading = false;
        }
        return GenericMethods.IsListNotEmpty(_unitData.ReadingUnits);
    }

    private double ConvertValue(double value, UnitModel fromUnit, UnitModel toUnit)
    {
        double convertedValue = value;
        if (fromUnit.UnitIdentifier == ReadingUnit.Fahrenheit.ToString())
        {
            convertedValue -= 32;
        }
        if (fromUnit.IsBaseUnit)
        {
            convertedValue *= toUnit.BaseConversionFactor;
        }
        else if (toUnit.IsBaseUnit)
        {
            convertedValue /= fromUnit.BaseConversionFactor;
        }
        else
        {
            // Neither unit is base unit
            ////convertedValue = (value / fromUnit.BaseConversionFactor) * toUnit.BaseConversionFactor;
        }
        if (fromUnit.UnitIdentifier == ReadingUnit.Celsius.ToString())
        {
            convertedValue += 32;
        }
        return convertedValue;
    }
}
