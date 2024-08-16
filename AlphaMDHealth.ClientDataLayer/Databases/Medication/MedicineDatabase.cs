using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AlphaMDHealth.ClientDataLayer
{
    public class MedicineDatabase : BaseDatabase
    {
        /// <summary>
        /// Get medicines from db
        /// </summary>
        /// <param name="medicationData">Reference object to return medicines data</param>
        /// <returns>Operation status with list of medicines in reference object</returns>
        public async Task GetMedicinesAsync(PatientMedicationDTO medicationData, string searchText)
        {
            medicationData.Medicines = await SqlConnection.QueryAsync<MedicineModel>(
                $"SELECT * FROM MedicineModel WHERE IsActive = 1 AND (ShortName LIKE '%{searchText}%' OR FullName LIKE '%{searchText}%')"
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Save Medicines
        /// </summary>
        /// <param name="medicationData">Reference object to hold patient medication data</param>
        /// <returns>Operation status</returns>
        public async Task SaveMedicinesAsync(PatientMedicationDTO medicationData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveMedicines(medicationData, transaction);
            }).ConfigureAwait(false);
        }

        private void SaveMedicines(PatientMedicationDTO medicationData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(medicationData.Medicines))
            {
                foreach (var medicine in medicationData.Medicines)
                {
                    var existingMedicine = transaction.FindWithQuery<MedicineModel>("SELECT * FROM MedicineModel WHERE ShortName = ?", medicine.ShortName);
                    if (existingMedicine == null)
                    {
                        transaction.Execute("INSERT INTO MedicineModel (ShortName, FullName, UnitIdentifier, IsActive) VALUES (?, ?, ?, ?)",
                           medicine.ShortName, medicine.FullName, medicine.UnitIdentifier, medicine.IsActive);
                    }
                    else
                    {
                        transaction.Execute("UPDATE MedicineModel SET FullName = ?, UnitIdentifier = ? , IsActive = ? WHERE ShortName = ?",
                           medicine.FullName, medicine.UnitIdentifier, medicine.IsActive, medicine.ShortName);
                    }
                }
            }
        }

        public async Task<bool> SaveDefaultMedicinesAsync()
        {
            //string str = string.Empty;
            //try
            //{
            Assembly assembly = typeof(MedicineDatabase).GetTypeInfo().Assembly;
            var filePath = $"{assembly.GetName().Name}{Constants.DOT_SEPARATOR}{string.Format(Constants.MEDICINE_DATA_FILE_NAME, nameof(MedicineModel))}";
            using (var stream = assembly.GetManifestResourceStream(filePath))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    await SqlConnection.RunInTransactionAsync(transaction =>
                    {
                        transaction.Execute("DELETE FROM MedicineModel");
                        foreach (string query in Regex.Split(reader.ReadToEnd(), Environment.NewLine))
                        {
                            //str = query;
                            if (!string.IsNullOrWhiteSpace(query))
                                transaction.Execute(query);
                        }
                    }).ConfigureAwait(false);
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine($"++++++++++SaveDefaultMedicinesAsync() query:{str}");
            //    throw ex;
            //    return false;
            //}
            return true;
        }
    }
}