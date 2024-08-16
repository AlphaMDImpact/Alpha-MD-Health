using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    /// <summary>
    /// Medication service 
    /// </summary>
    public partial class MedicationSevice : BaseService
    {
        #region public methods

        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveMedicationsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                var medicationData = new PatientMedicationDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    Medications = MapMedications(data, nameof(DataSyncDTO.PatientMedications)),
                    Reminders = MapMedicationRemiders(data, nameof(DataSyncDTO.MedicationReminders))
                };
                if (GenericMethods.IsListNotEmpty(medicationData.Medications) || GenericMethods.IsListNotEmpty(medicationData.Reminders))
                {
                    await SaveMedicationsAsync(medicationData).ConfigureAwait(false);
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

        /// <summary>
        /// Sync medication from service
        /// </summary>
        /// <param name="medicationData">Reference object to hold medication data</param>
        /// <returns>Operation status with medication data received from server</returns>
        private async Task SyncMedicationsFromServerAsync(PatientMedicationDTO medicationData)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    PathWithoutBasePath = UrlConstants.GET_MEDICATIONS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { nameof(PatientMedicationModel.ProgramID),Convert.ToString(medicationData.Medication?.ProgramID, CultureInfo.InvariantCulture) },
                        { nameof(PatientMedicationModel.ProgramMedicationID),Convert.ToString(medicationData.Medication?.ProgramMedicationID, CultureInfo.InvariantCulture) },
                        { nameof(PatientMedicationModel.PatientMedicationID), Convert.ToString(medicationData.Medication?.PatientMedicationID ?? Guid.Empty, CultureInfo.InvariantCulture) },
                        { nameof(PatientMedicationDTO.SelectedUserID), Convert.ToString(medicationData.SelectedUserID, CultureInfo.InvariantCulture) },
                        { nameof(PatientMedicationDTO.RecordCount), Convert.ToString(medicationData.RecordCount, CultureInfo.InvariantCulture) },
                        { nameof(BaseDTO.FromDate), medicationData.FromDate },
                        { nameof(BaseDTO.ToDate), medicationData.ToDate },
                        { nameof(PatientMedicationDTO.IsMedicalHistory), Convert.ToString(medicationData.IsMedicalHistory, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                medicationData.ErrCode = httpData.ErrCode;
                if (medicationData.ErrCode == ErrorCode.OK)
                {
                    MapGetMedicationsServiceResponse(medicationData, httpData.Response);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                medicationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        private void MapGetMedicationsServiceResponse(PatientMedicationDTO medicationData, string jsonResponse)
        {
            JToken data = JToken.Parse(jsonResponse);
            if (data != null && data.HasValues)
            {
                MapCommonData(medicationData, data);
                if (!MobileConstants.IsMobilePlatform)
                {
                    SetPageResources(medicationData.Resources);
                }
                MapMedicationData(data, medicationData);
            }
        }

        internal object MapMedicationsHistoryData(MedicalHistoryDTO medicalHistoryData, MedicalHistoryViewModel historyView, string jsonResponse)
        {
            PatientMedicationDTO medicationData = new PatientMedicationDTO
            {
                IsMedicalHistory = true,
                FromDate = medicalHistoryData.FromDate,
                ToDate = medicalHistoryData.ToDate,
                RecordCount = medicalHistoryData.RecordCount,
                ErrCode = historyView.ErrorCode
            };
            if (historyView.FeatureCode == AppPermissions.PrescriptionView)
            {
                medicationData.IsPrescriptionView = true;
                medicationData.RecordCount = -2;
            }
            MapGetMedicationsServiceResponse(medicationData, jsonResponse);
            medicationData.FeaturePermissions = medicalHistoryData.FeaturePermissions;
            GetMedicationsUIData(medicationData);
            historyView.HasData = GenericMethods.IsListNotEmpty(medicationData.Medications);
            return medicationData;
        }

        /// <summary>
        /// Sync medication data to server
        /// </summary>
        /// <param name="medicationData">object to return operation status</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status</returns>
        public async Task SyncMedicationsToServerAsync(PatientMedicationDTO medicationData, CancellationToken cancellationToken)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    await new MedicationDatabase().GetPatientMedicationForSyncAsync(medicationData).ConfigureAwait(false);
                }
                if (GenericMethods.IsListNotEmpty(medicationData.Medications) || GenericMethods.IsListNotEmpty(medicationData.Reminders))
                {
                    //var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
                    //medicationData.IsActive = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
                    var httpData = new HttpServiceModel<PatientMedicationDTO>
                    {
                        CancellationToken = cancellationToken,
                        PathWithoutBasePath = UrlConstants.SAVE_MEDICATION_ASYNC_PATH,
                        ContentToSend = medicationData,
                        QueryParameters = new NameValueCollection
                        {
                            { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        }
                    };
                    await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                    medicationData.ErrCode = httpData.ErrCode;
                    if (medicationData.ErrCode == ErrorCode.OK)
                    {
                        JToken data = JToken.Parse(httpData.Response);
                        if (data?.HasValues == true)
                        {
                            medicationData.SaveMedications = MapSaveResponse(data, nameof(PatientMedicationDTO.SaveMedications));
                            await SaveMedicationsSyncResultAsync(medicationData, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                medicationData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Maps medications data
        /// </summary>
        /// <param name="data">Json data to map</param>
        /// <param name="collectionName">Collection name in json</param>
        /// <returns>List of medication model</returns>
        internal List<PatientMedicationModel> MapMedications(JToken data, string collectionName)
        {
            return data[collectionName].Any()
               ? (from dataItem in data[collectionName]
                  select MapMedication(dataItem)).ToList()
               : new List<PatientMedicationModel>();
        }

        #endregion

        #region Private Methods

        private async Task SaveMedicationsSyncResultAsync(PatientMedicationDTO medicationData, CancellationToken cancellationToken)
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await new MedicationDatabase().UpdateMedicationsSyncStatusAsync(medicationData).ConfigureAwait(false);
            }
            else
            {
                // Map error result to main object
                medicationData.ErrCode = medicationData.SaveMedications?.FirstOrDefault(x => x.ErrCode != ErrorCode.OK)?.ErrCode ?? ErrorCode.OK;
                if (medicationData.Medication?.ProgramID > 1)
                {
                    if (medicationData.ErrCode == ErrorCode.OK)
                    {
                        medicationData.Medication.ProgramMedicationID = medicationData.SaveMedications.FirstOrDefault().ID;
                    }
                    return;
                }
                if (medicationData.ErrCode == ErrorCode.DuplicateGuid)
                {
                    medicationData.Medications.FirstOrDefault().PatientMedicationID = GenericMethods.GenerateGuid();
                    foreach (var reminder in medicationData.Reminders)
                    {
                        reminder.PatientMedicationID = medicationData.Medications.FirstOrDefault().PatientMedicationID;
                    }
                }
            }
            if (medicationData.ErrCode == ErrorCode.DuplicateGuid)
            {
                medicationData.ErrCode = ErrorCode.OK;
                await SyncMedicationsToServerAsync(medicationData, cancellationToken).ConfigureAwait(false);
            }
            medicationData.RecordCount = medicationData.Medications?.Count ?? 0;
        }

        private void MapMedicationData(JToken data, PatientMedicationDTO medicationData)
        {
            medicationData.Medications = MapMedications(data, nameof(PatientMedicationDTO.Medications));
            if (medicationData.RecordCount == -1)
            {
                if (GenericMethods.IsListNotEmpty(medicationData.Medications))
                {
                    medicationData.Medication = medicationData.Medications.FirstOrDefault();
                    medicationData.Medication.StartDate = _essentials.ConvertToLocalTime(medicationData.Medication.StartDate.Value);
                    medicationData.Medication.EndDate = _essentials.ConvertToLocalTime(medicationData.Medication.EndDate.Value);
                    medicationData.Medications.Clear();
                }
                medicationData.Reminders = MapMedicationRemiders(data, nameof(PatientMedicationDTO.Reminders));

                medicationData.UnitOptions = GetPickerSource(data, nameof(PatientMedicationDTO.UnitOptions), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), 0, true, nameof(OptionModel.ParentOptionID), nameof(OptionModel.ParentOptionText));
                var unit = medicationData.UnitOptions?.FirstOrDefault(x => x.GroupName == medicationData.Medication?.UnitIdentifier);
                if (unit != null)
                {
                    unit.IsSelected = true;
                }
            }
        }

        private PatientMedicationModel MapMedication(JToken dataItem)
        {
            return new PatientMedicationModel
            {
                ProgramID = GetDataItem<long>(dataItem, nameof(PatientMedicationModel.ProgramID)),
                ProgramMedicationID = GetDataItem<long>(dataItem, nameof(PatientMedicationModel.ProgramMedicationID)),
                PatientMedicationID = GetDataItem<Guid>(dataItem, nameof(PatientMedicationModel.PatientMedicationID)),
                Doses = GetDataItem<decimal>(dataItem, nameof(PatientMedicationModel.Doses)),
                Frequency = GetDataItem<String>(dataItem, nameof(PatientMedicationModel.Frequency)),
                HowOften = GetDataItem<int>(dataItem, nameof(PatientMedicationModel.HowOften)),
                AfterDays = GetDataItem<byte>(dataItem, nameof(PatientMedicationModel.AfterDays)),
                AssignAfterDays = GetDataItem<short>(dataItem, nameof(PatientMedicationModel.AssignAfterDays)),
                AssignForDays = GetDataItem<short>(dataItem, nameof(PatientMedicationModel.AssignForDays)),
                IsActive = GetDataItem<bool>(dataItem, nameof(PatientMedicationModel.IsActive)),
                IsReadOnly = GetDataItem<bool>(dataItem, nameof(PatientMedicationModel.IsReadOnly)),
                UnitIdentifier = GetDataItem<string>(dataItem, nameof(PatientMedicationModel.UnitIdentifier)),
                ShortName = GetDataItem<string>(dataItem, nameof(PatientMedicationModel.ShortName)),
                FullName = GetDataItem<string>(dataItem, nameof(PatientMedicationModel.FullName)),
                StartDate = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientMedicationModel.StartDate)),
                EndDate = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientMedicationModel.EndDate)),
                Note = GetDataItem<string>(dataItem, nameof(PatientMedicationModel.Note)),
                AdditionalNotes = GetDataItem<string>(dataItem, nameof(PatientMedicationModel.AdditionalNotes)),
                IsCritical = GetDataItem<bool>(dataItem, nameof(PatientMedicationModel.IsCritical)),
                Reminder = GetDataItem<bool>(dataItem, nameof(PatientMedicationModel.Reminder)),
                PatientID = GetDataItem<long>(dataItem, nameof(PatientMedicationModel.PatientID)),
                AddedByID = GetDataItem<string>(dataItem, nameof(PatientMedicationModel.AddedByID)),
                LastModifiedByID = GetDataItem<long>(dataItem, nameof(PatientMedicationModel.LastModifiedByID)),
                AddedOn = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientMedicationModel.AddedOn)),
                AddedByName = GetDataItem<string>(dataItem, nameof(PatientMedicationModel.AddedByName)),
                ProgramName = GetDataItem<string>(dataItem, nameof(PatientMedicationModel.ProgramName)),
                ProgramColor = GetDataItem<string>(dataItem, nameof(PatientMedicationModel.ProgramColor)),
                ShortUnitName = GetDataItem<string>(dataItem, nameof(PatientMedicationModel.ShortUnitName)),
                IsSynced = true
            };
        }

        private List<MedicationReminderModel> MapMedicationRemiders(JToken data, string collectionName)
        {
            return data[collectionName].Any()
               ? (from dataItem in data[collectionName]
                  select MapMedicationReminder(dataItem)).ToList()
               : new List<MedicationReminderModel>();
        }

        private MedicationReminderModel MapMedicationReminder(JToken dataItem)
        {
            return new MedicationReminderModel
            {
                PatientMedicationID = GetDataItem<Guid>(dataItem, nameof(MedicationReminderModel.PatientMedicationID)),
                IsActive = GetDataItem<bool>(dataItem, nameof(MedicationReminderModel.IsActive)),
                ReminderDateTime = GetDataItem<DateTimeOffset>(dataItem, nameof(MedicationReminderModel.ReminderDateTime)),
                IsSynced = true
            };
        }

        #endregion
    }
}