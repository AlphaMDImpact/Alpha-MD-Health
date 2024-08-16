using System.Collections.Specialized;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Medicine client business layer logical service
/// </summary>
public partial class MedicineService : BaseService
{
    public MedicineService(IEssentials essentials) : base(essentials)
    {

    }
    /// <summary>
    /// Maps json object into Model and saves data into DB
    /// </summary>
    /// <param name="result">Object which holds Operation status</param>
    /// <param name="data">json object to fetch data from it</param>
    /// <returns>Operation status with record count</returns>
    internal async Task MapAndSaveMedicinesAsync(DataSyncModel result, JToken data)
    {
        try
        {
            var medicationData = new PatientMedicationDTO
            {
                LastModifiedON = result.SyncFromServerDateTime,
                Medicines = MapMedicines(data, nameof(DataSyncDTO.Medicines))
            };
            if (GenericMethods.IsListNotEmpty(medicationData.Medicines))
            {
                await new MedicineDatabase().SaveMedicinesAsync(medicationData).ConfigureAwait(false);
                result.RecordCount += medicationData.Medicines.Count;
            }
            result.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Sync medicines from service
    /// </summary>
    /// <param name="medicationData">Reference object to hold medication data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="searchText">Search text</param>
    /// <returns>Operation status with medicine data received from server</returns>
    private async Task SyncMedicinesFromServerAsync(PatientMedicationDTO medicationData, CancellationToken cancellationToken, string searchText)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_MEDICINES_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { nameof(PatientMedicationModel.ShortName), searchText },
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            medicationData.ErrCode = httpData.ErrCode;
            if (medicationData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    medicationData.Medicines = MapMedicines(data, nameof(PatientMedicationDTO.Medicines));
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            medicationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Get madicine based on searched text
    /// </summary>
    /// <param name="medicationData">Reference object to return medicine data</param>
    /// <param name="searchText">test to search medicine</param>
    /// <returns>Operation status with searched medicines in reference object</returns>
    public async Task SearchMedicineAsync(PatientMedicationDTO medicationData, string searchText)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await new MedicineDatabase().GetMedicinesAsync(medicationData, searchText).ConfigureAwait(false);
                medicationData.Medicines = medicationData.Medicines ?? new List<MedicineModel>();
            }
            else
            {
                await SyncMedicinesFromServerAsync(medicationData, CancellationToken.None, searchText).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            medicationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Check Medicine default data is saved or not, if not execute the default medicine data script to save it
    /// </summary>
    /// <param name="dataSync">data sync model</param>
    /// <param name="selectedUserID">selected user id</param>
    /// <param name="syncToCancel">sync to cancel in case of error</param>
    /// <returns>operation status</returns>
    internal async Task CheckAndAssignDefaultMedicineDataAsync(DataSyncModel dataSync, long selectedUserID, List<DataSyncFor> syncToCancel)
    {
        try
        {
            if (!dataSync.SyncFromServerDateTime.HasValue
                || (dataSync.SyncFromServerDateTime.HasValue && dataSync.SyncFromServerDateTime.Value < Constants.DEFAULT_MEDICINE_DATETIME))
            {
                if (!_essentials.GetPreferenceValue(StorageConstants.IS_MEDICINE_MASTER_UPLOADING_KEY, false))
                {
                    _essentials.SetPreferenceValue(StorageConstants.IS_MEDICINE_MASTER_UPLOADING_KEY, true);
                    if (await new MedicineDatabase().SaveDefaultMedicinesAsync().ConfigureAwait(false))
                    {
                        dataSync.SyncFromServerDateTime = Constants.DEFAULT_MEDICINE_DATETIME;
                        dataSync.RecordCount = 1;
                        await new DataSyncDatabase().UpdateDateSyncedFromServerAsync(dataSync.SyncFor, dataSync.SyncFromServerDateTime.Value, selectedUserID).ConfigureAwait(false);
                    }
                    else
                    {
                        _essentials.SetPreferenceValue(StorageConstants.IS_MEDICINE_MASTER_UPLOADING_KEY, false);
                        syncToCancel.Add(dataSync.SyncFor.ToEnum<DataSyncFor>());
                    }
                }
                else
                {
                    syncToCancel.Add(dataSync.SyncFor.ToEnum<DataSyncFor>());
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            _essentials.SetPreferenceValue(StorageConstants.IS_MEDICINE_MASTER_UPLOADING_KEY, false);
            syncToCancel.Add(dataSync.SyncFor.ToEnum<DataSyncFor>());
        }
    }

    private List<MedicineModel> MapMedicines(JToken data, string collectionName)
    {
        return data[collectionName].Any()
           ? (from dataItem in data[collectionName]
              select MapMedicine(dataItem)).ToList()
           : new List<MedicineModel>();
    }

    private MedicineModel MapMedicine(JToken dataItem)
    {
        return new MedicineModel
        {
            UnitIdentifier = GetDataItem<string>(dataItem, nameof(MedicineModel.UnitIdentifier)),
            ShortName = GetDataItem<string>(dataItem, nameof(MedicineModel.ShortName)),
            FullName = GetDataItem<string>(dataItem, nameof(MedicineModel.FullName)),
            IsActive = GetDataItem<bool>(dataItem, nameof(MedicineModel.IsActive))
        };
    }
}