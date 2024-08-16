using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public partial class ReadingDatabase : BaseDatabase
    {
        /// <summary>
        /// Gets Units data list 
        /// </summary>
        /// <param name="readingsData">Reference object to store units data</param>
        /// <returns>List of units data</returns>
        public async Task GetUnitsAsync(PatientReadingDTO readingsData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                GetUnits(readingsData, transaction);
                GetMeasurementUnitFilterOptions(readingsData, transaction);
            });
        }

        private void GetUnits(PatientReadingDTO readingsData, SQLiteConnection transaction)
        {
            readingsData.ReadingUnits = transaction.Query<UnitModel>(
                "SELECT A.UnitID, A.UnitIdentifier, A.UnitGroupID, A.IsBaseUnit, A.BaseConversionFactor, B.FullUnitName, B.ShortUnitName " +
                "FROM UnitModel A " +
                "JOIN UnitI18NModel B ON B.UnitID = A.UnitID AND B.LanguageID = ? AND A.IsActive = 1 AND B.IsActive = 1"
                , readingsData.LanguageID);
        }

        /// <summary>
        /// Save Units to local database
        /// </summary>
        /// <param name="readingData">Reference object of Unnits</param>
        /// <returns>operation status</returns>
        public async Task SaveUnitsAsync(PatientReadingDTO readingData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveReadingUnits(readingData, transaction);
                SaveReadingUnitI18N(readingData, transaction);
            }).ConfigureAwait(false);
        }

        private void SaveReadingUnits(PatientReadingDTO readingData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(readingData.ReadingUnits))
            {
                foreach (UnitModel unit in readingData.ReadingUnits)
                {
                    string saveQuery;
                    if (transaction.FindWithQuery<UnitModel>("SELECT 1 FROM UnitModel WHERE UnitID=?", unit.UnitID) == null)
                    {
                        saveQuery = "INSERT INTO UnitModel(UnitIdentifier, UnitGroupID, IsBaseUnit, BaseConversionFactor, IsActive, UnitID) VALUES(?, ?, ?, ?, ?, ?) ";
                    }
                    else
                    {
                        saveQuery = "UPDATE UnitModel SET UnitIdentifier=?, UnitGroupID=?, IsBaseUnit=?, BaseConversionFactor=?, IsActive=? WHERE UnitID=? ";
                    }
                    transaction.Execute(saveQuery, unit.UnitIdentifier, unit.UnitGroupID, unit.IsBaseUnit, unit.BaseConversionFactor, unit.IsActive, unit.UnitID);
                }
            }
        }

        private void SaveReadingUnitI18N(PatientReadingDTO readingData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(readingData.ReadingUnitsI18N))
            {
                foreach (UnitI18NModel unit in readingData.ReadingUnitsI18N)
                {
                    string saveQuery;
                    if (transaction.FindWithQuery<UnitI18NModel>(
                        "SELECT 1 FROM UnitI18NModel WHERE UnitID=? AND LanguageID=? ",
                        unit.UnitID, unit.LanguageID) == null)
                    {
                        saveQuery = "INSERT INTO UnitI18NModel(ShortUnitName, FullUnitName, IsActive, UnitID, LanguageID) VALUES(?, ?, ?, ?, ?) ";
                    }
                    else
                    {
                        saveQuery = "UPDATE UnitI18NModel SET ShortUnitName=?, FullUnitName=?, IsActive=? WHERE UnitID=? AND LanguageID=? ";
                    }
                    transaction.Execute(saveQuery, unit.ShortUnitName, unit.FullUnitName, unit.IsActive, unit.UnitID, unit.LanguageID);
                }
            }
        }
    }
}