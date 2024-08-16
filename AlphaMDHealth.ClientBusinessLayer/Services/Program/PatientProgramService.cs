using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class PatientProgramService : BaseService
    {
        public PatientProgramService(IEssentials essentials = null):base(essentials) 
        { }
        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveProgramsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                var programData = new PatientProgramDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    Programs = new ProgramService(_essentials).MapPrograms(data, nameof(DataSyncDTO.Programs)),
                };
                if (GenericMethods.IsListNotEmpty(programData.Programs))
                {
                    await new ProgramDatabase().SaveProgramsAsync(programData).ConfigureAwait(false);
                    result.RecordCount = programData.Programs.Count;
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
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSavePatientProgramsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                var programData = new PatientProgramDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    PatientPrograms = MapPatientPrograms(data, nameof(DataSyncDTO.PatientPrograms))
                };
                if (GenericMethods.IsListNotEmpty(programData.PatientPrograms))
                {
                    await new ProgramDatabase().SavePatientProgramsAsync(programData).ConfigureAwait(false);
                    result.RecordCount = programData.PatientPrograms.Count;
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
        /// Map programs data
        /// </summary>
        /// <param name="data">programs json data</param>
        /// <param name="collectionName">collection name</param>
        /// <returns>Program list</returns>
        private List<PatientProgramModel> MapPatientPrograms(JToken data, string collectionName)
        {
            return data[collectionName].Any()
                ? (from dataItem in data[collectionName]
                   select MapPatientProgram(dataItem)).ToList()
                : null;
        }

        /// <summary>
        /// Map program data
        /// </summary>
        /// <param name="dataItem">program json object</param>
        /// <returns>Program data</returns>
        private PatientProgramModel MapPatientProgram(JToken dataItem)
        {
            return new PatientProgramModel
            {
                PatientProgramID = GetDataItem<long>(dataItem, nameof(PatientProgramModel.PatientProgramID)),
                ProgramID = GetDataItem<long>(dataItem, nameof(PatientProgramModel.ProgramID)),
                PatientID = GetDataItem<long>(dataItem, nameof(PatientProgramModel.PatientID)),
                ProgramGroupIdentifier = GetDataItem<string>(dataItem, nameof(PatientProgramModel.ProgramGroupIdentifier)),
                ProgramEntryPoint = GetDataItem<string>(dataItem, nameof(PatientProgramModel.ProgramEntryPoint)),
                EntryTypeID = GetDataItem<int>(dataItem, nameof(PatientProgramModel.EntryTypeID)),
                EntryDate = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientProgramModel.EntryDate)),
                EntryPoint = GetDataItem<string>(dataItem, nameof(PatientProgramModel.EntryPoint)),
                TrackerID = GetDataItem<short>(dataItem, nameof(PatientProgramModel.TrackerID)),
                Name = GetDataItem<string>(dataItem, nameof(PatientProgramModel.Name)),
                IsActive = GetDataItem<bool>(dataItem, nameof(PatientProgramModel.IsActive)),
                IsSynced = true,
                AddedON = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientProgramModel.AddedON)),
                LastModifiedON = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientProgramModel.LastModifiedON)),
            };
        }

        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveProgramCaregiversAsync(DataSyncModel result, JToken data)
        {
            try
            {
                var programData = new ProgramDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    ProgramCareGivers = MapProgramCaregivers(data, nameof(DataSyncDTO.ProgramCaregivers))
                };
                if (GenericMethods.IsListNotEmpty(programData.ProgramCareGivers))
                {
                    await new CaregiverDatabase().SaveProgramCaregiversAsync(programData).ConfigureAwait(false);
                    result.RecordCount = programData.ProgramCareGivers.Count;
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
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSavePatientsSharedPrograms(DataSyncModel result, JToken data)
        {
            try
            {
                var programData = new ProgramDTO
                {
                    PatientsSharedPrograms = MapPatientsSharedPrograms(data, nameof(DataSyncDTO.PatientsSharedPrograms))
                };
                if (GenericMethods.IsListNotEmpty(programData.PatientsSharedPrograms))
                {
                    await new ProgramDatabase().SavePatientsSharedProgramsAsync(programData).ConfigureAwait(false);
                    result.RecordCount = programData.PatientsSharedPrograms.Count;
                }
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        private List<PatientsSharedProgramsModel> MapPatientsSharedPrograms(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
              ? (from dataItem in data[collectionName]
                 select new PatientsSharedProgramsModel
                 {
                     PatientProgramID = (long)dataItem[nameof(PatientsSharedProgramsModel.PatientProgramID)],
                     PatientCareGiverID = (long)dataItem[nameof(PatientsSharedProgramsModel.PatientCareGiverID)],
                     IsActive = (bool)dataItem[nameof(BillPaymentModel.IsActive)],
                     LastModifiedON = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientsSharedProgramsModel.LastModifiedON))
                 }).ToList()
              : null;
        }

        /// <summary>
        /// Sync Progrm details to server
        /// </summary>
        /// <param name="programData">Program data to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncPatientProgramsToServerAsync(PatientProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                await new ProgramDatabase().GetPatientProgramsToSyncWithServerAsync(programData).ConfigureAwait(false);
                if (!GenericMethods.IsListNotEmpty(programData.PatientPrograms))
                {
                    return;
                }
                programData.RecordCount = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0) == (int)RoleName.Patient ? -2 : -1;
                foreach (var program in programData.PatientPrograms)
                {
                    programData.PatientProgram = program;
                    programData.CreatedByID = program.PatientProgramID;
                    await SavePatientProgramsToServerAsync(programData, cancellationToken);
                    program.ErrCode = programData.ErrCode;
                    if (IsProgramSynced(program))
                    {
                        program.IsSynced = true;
                        program.PatientProgramID = programData.PatientProgram.PatientProgramID;
                        await new ProgramDatabase().UpdatePatientProgramIDAsync(programData.CreatedByID, programData.PatientProgram.PatientProgramID).ConfigureAwait(false);
                    }
                }
                programData.RecordCount = programData.PatientPrograms.Count(x => x.IsSynced);
                programData.ErrCode = programData.PatientPrograms.FirstOrDefault(x => !IsProgramSynced(x))?.ErrCode ?? ErrorCode.OK;
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private bool IsProgramSynced(PatientProgramModel program)
        {
            return program.ErrCode == ErrorCode.OK || program.ErrCode == ErrorCode.DuplicateData;
        }

        /// <summary>
        /// Save patient program data 
        /// </summary>
        /// <param name="programData">program data to save in server</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SavePatientProgramsToServerAsync(PatientProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                if (programData.PatientProgram == null)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return;
                }
                var httpData = new HttpServiceModel<PatientProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    ContentToSend = programData,
                    PathWithoutBasePath = UrlConstants.SAVE_PATIENT_PROGRAMS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        {Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(_essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0), CultureInfo.InvariantCulture)}
                    }
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (MobileConstants.IsMobilePlatform
                    && (programData.ErrCode == ErrorCode.OK || programData.ErrCode == ErrorCode.DuplicateData)
                   )
                {
                    MapPatientProgramID(programData, httpData);
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapPatientProgramID(PatientProgramDTO programData, HttpServiceModel<PatientProgramDTO> httpData)
        {
            JToken data = JToken.Parse(httpData.Response);
            if (data?.HasValues == true)
            {
                JToken patientProgramJData = data[nameof(programData.PatientProgram)];
                if (patientProgramJData.HasValues)
                {
                    programData.PatientProgram.PatientProgramID = (long)patientProgramJData[nameof(PatientProgramModel.PatientProgramID)];
                }
            }
        }

        /// <summary>
        /// Check if any program is selected 
        /// </summary>
        /// <returns>Is Program Selected Boolean Value</returns>
        public async Task<int> IsProgramSelectionRequiredAsync()
        {
            try
            {
                // Step 1: Check PatientProgramsView is allowed
                // Step 2: Check Logedin user is Patient
                // Step 3: Check AllowPatientToSelectProgramKey setting is true
                await GetFeaturesAsync(AppPermissions.PatientProgramAddEdit.ToString()).ConfigureAwait(true);
                if (CheckFeaturePermissionByCode(AppPermissions.PatientProgramAddEdit.ToString())
                    && IsPatientAllowedForProgramSelectionAsync())
                {
                    // Step 4: Check Programs are configured in Organization
                    PatientProgramDTO programData = new PatientProgramDTO
                    {
                        IsPatientAllowedForProgramSelection = true,
                        RecordCount = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0) == (int)RoleName.Patient ? -2 : -1
                    };

                    programData.SelectedUserID = programData.RecordCount == -2
                        ? GetLoginUserID()
                        : _essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);

                    await new ProgramDatabase().GetPatientProgramsAsync(programData).ConfigureAwait(false);
                    if (GenericMethods.IsListNotEmpty(programData.Programs))
                    {
                        // Step 5: Check program is selected or not and also check user can select program AllowSelfRegistration
                        if (!programData.Programs.Any(x => x.IsActive) && programData.Programs.Any(x => x.AllowSelfRegistration))
                        {
                            return 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                return 2;
            }
            return 0;
        }

        private bool IsPatientAllowedForProgramSelectionAsync()
        {
            // Step 1: Check Logedin user is Patient
            if (_essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0) == (int)RoleName.Patient)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get patient programs data
        /// </summary>
        /// <param name="programData">patient programs data</param>
        /// <returns>patient programs data with operation status</returns>
        public async Task GetPatientProgramsAsync(PatientProgramDTO programData)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    programData.IsPatientAllowedForProgramSelection = await IsAddEditAllowedAsync(programData.RecordCount).ConfigureAwait(false);
                    await Task.WhenAll(
                        GetResourcesAsync(GroupConstants.RS_PROGRAMS_GROUP, GroupConstants.RS_PATIENT_PROGRAM_END_POINT_TYPE_GROUP),
                        GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                        GetFeaturesAsync(AppPermissions.PatientProgramsView.ToString(), AppPermissions.PatientProgramAddEdit.ToString()),
                        new ProgramDatabase().GetPatientProgramsAsync(programData)
                    ).ConfigureAwait(false);
                    programData.EndPointTypes = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_PATIENT_PROGRAM_END_POINT_TYPE_GROUP, "", false);
                    programData.ErrCode = ErrorCode.OK;
                }
                else
                {
                    await GetPatientProgramsAsync(programData, CancellationToken.None).ConfigureAwait(false);
                }
                if (programData.ErrCode == ErrorCode.OK && programData.RecordCount != -2)
                {
                    MapFormattedData(programData);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        private async Task<bool> IsAddEditAllowedAsync(long recordCount)
        {
            switch (recordCount)
            {
                case -2:
                    return true;
                case -1:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Sync Patient programs from service
        /// </summary>
        /// <param name="programData">programData reference to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Patient Program Data received from server</returns>
        private async Task GetPatientProgramsAsync(PatientProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PATIENT_PROGRAMS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(_essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0), CultureInfo.InvariantCulture) },
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(programData.RecordCount, CultureInfo.InvariantCulture) },
                        { nameof(PatientProgramModel.PatientProgramID), Convert.ToString(programData.PatientProgram.PatientProgramID, CultureInfo.InvariantCulture) }
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        SetPageResources(programData.Resources);
                        MapPrograms(data, programData);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// Save patient programs in database
        /// </summary>
        /// <param name="programData">Patient Programs Data</param>
        /// <returns>Operation Status</returns>
        public async Task SaveAssignPatientProgramsAsync(PatientProgramDTO programData)
        {
            programData.PatientPrograms = new List<PatientProgramModel> { programData.PatientProgram };
            try
            {
                await new ProgramDatabase().SavePatientProgramDataAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Save patient programs data in database
        /// </summary>
        /// <param name="programData">patient programs data</param>
        /// <returns>operation status</returns>
        public async Task SavePatientProgramsAsync(PatientProgramDTO programData)
        {
            try
            {
                programData.PatientPrograms = programData.PatientPrograms.Where(x => !x.IsSynced).ToList();
                if (programData.PatientPrograms?.Count > 0)
                {
                    await new ProgramDatabase().SavePatientProgramsAsync(programData).ConfigureAwait(false);
                }
                programData.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapFormattedData(PatientProgramDTO programData)
        {
            if (!MobileConstants.IsMobilePlatform)
            {
                SetResourcesAndSettings(programData);
            }
            LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
            if (GenericMethods.IsListNotEmpty(programData.Programs))
            {
                programData.Programs.ForEach(program =>
                {
                    program.ProgramImage = ImageConstants.I_PROGRAMS_LIST_ICON;
                    program.AddedOnString = string.Format(CultureInfo.InvariantCulture
                        , GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(program.AddedOn)
                        , DateTimeType.Date, dayFormat, monthFormat, yearFormat));
                });
                //if (MobileConstants.IsMobilePlatform)
                //{
                //    GetResourcesAsync(GroupConstants.RS_PROGRAMS_GROUP).ConfigureAwait(false);
                //}
            }
            //ChangeResourcesValue();
        }

        //private void ChangeResourcesValue()
        //{
        //    ResourceModel PatientProgramDateResource = LibResources.GetResourceByKey(PageData?.Resources, ResourceConstants.R_PATIENT_PROGRAM_DATE_KEY);
        //    ResourceModel PatientProgramDayResource = LibResources.GetResourceByKey(PageData?.Resources, ResourceConstants.R_PATIENT_PROGRAM_DAYS_KEY);
        //    ResourceModel PatientProgramTrackerResource = LibResources.GetResourceByKey(PageData?.Resources, ResourceConstants.R_PATIENT_PROGRAM_TRACKER_KEY);

        //    PageData.Resources.Remove(PatientProgramDateResource);
        //    PageData.Resources.Remove(PatientProgramDayResource);
        //    PageData.Resources.Remove(PatientProgramTrackerResource);

        //    PatientProgramDateResource.IsRequired = false;
        //    PatientProgramDayResource.IsRequired = false;
        //    PatientProgramTrackerResource.IsRequired = false;

        //    PageData.Resources.Add(PatientProgramDateResource);
        //    PageData.Resources.Add(PatientProgramDayResource);
        //    PageData.Resources.Add(PatientProgramTrackerResource);
        //}

        private void MapPrograms(JToken data, PatientProgramDTO patientProgramData)
        {
            if (patientProgramData.PatientProgram?.PatientProgramID > 0)
            {
                var patientProgram = data[nameof(ProgramDTO.PatientProgram)];
                if (patientProgram.HasValues)
                {
                    patientProgramData.PatientProgram = MapPatientProgram(patientProgram);
                }
            }
            if (patientProgramData.RecordCount == -1)
            {
                patientProgramData.OrganizationPrograms = data[nameof(ProgramDTO.Items)].Any()
                    ? (from dataItem in data[nameof(ProgramDTO.Items)]
                       select new OptionModel
                       {
                           OptionID = (long)dataItem[nameof(OptionModel.OptionID)],
                           OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                           SequenceNo = (long)dataItem[nameof(OptionModel.SequenceNo)],
                           IsSelected = (bool)dataItem[nameof(OptionModel.IsSelected)],
                       }).ToList()
                    : new List<OptionModel>();
                patientProgramData.EndPointTypes = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_PATIENT_PROGRAM_END_POINT_TYPE_GROUP,"", false, patientProgramData.PatientProgram?.EntryTypeID??-1);
                patientProgramData.TrackerTypes = (data[nameof(ProgramDTO.TrackerTypes)].Any())
                    ? (from dataItem in data[nameof(ProgramDTO.TrackerTypes)]
                       select new OptionModel
                       {
                           OptionID = (long)dataItem[nameof(OptionModel.OptionID)],
                           OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                           ParentOptionID = (long)dataItem[nameof(OptionModel.ParentOptionID)],
                       }).ToList()
                    : new List<OptionModel>();
               
            }
            else
            {
                patientProgramData.Programs = (data[nameof(patientProgramData.Programs)].Any())
                    ? (from dataItem in data[nameof(PatientProgramDTO.Programs)]
                       select new ProgramModel
                       {
                           PatientProgramID = (long)dataItem[nameof(ProgramModel.PatientProgramID)],
                           ProgramID = (long)dataItem[nameof(ProgramModel.ProgramID)],
                           PatientID = (long)dataItem[nameof(ProgramModel.PatientID)],
                           ProgramEntryPoint = (string)dataItem[nameof(ProgramModel.ProgramEntryPoint)],
                           ProgramGroupIdentifier = (string)dataItem[nameof(ProgramModel.ProgramGroupIdentifier)],
                           Name = (string)dataItem[nameof(ProgramModel.Name)],
                           AddedOnString = (string)dataItem[nameof(ProgramModel.AddedOnString)],
                           IsActive = (bool)dataItem[nameof(ProgramModel.IsActive)],
                           AddedOn = (DateTimeOffset)dataItem[nameof(ProgramModel.AddedOn)],
                           IsSynced = true
                       }).ToList() 
                    : new List<ProgramModel>();
            }
            patientProgramData.ErrCode = (ErrorCode)(int)data[nameof(BranchDTO.ErrCode)];
        }

        private List<CaregiverModel> MapProgramCaregivers(JToken data, string collectionName)
        {
            return data[collectionName].Any()
                ? (from dataItem in data[collectionName]
                   select new UserService(_essentials).MapProgramCaregiver(dataItem)).ToList()
                : null;
        }

        /// <summary>
        /// Delete PatientProgram from database
        /// </summary>
        /// <param name="fileData">Object containing PatientProgramID to delete</param>
        /// <returns>Operation status</returns>
        public async Task<ErrorCode> DeletePatientProgramAsync(PatientProgramDTO patientProgramData)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    await new ProgramDatabase().DeletePatientProgramAsync(patientProgramData.PatientProgram.PatientProgramID).ConfigureAwait(false);
                }
                return ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                return ErrorCode.ErrorWhileDeletingRecords;
            }
        }
    }
}