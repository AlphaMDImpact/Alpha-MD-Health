using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class DataSyncService : BaseService
{
    public DataSyncService(IEssentials serviceEssentails):base(serviceEssentails) {
    }

    /// <summary>
    /// Call Sync services based on Service sync types
    /// </summary>
    /// <param name="result">Object for storing operation status</param>
    /// <param name="syncMethod">To decide if data will be synced from server or synced to server</param>
    /// <param name="syncFor">Table for which data need be synced</param> 
    /// <param name="cancellationToken">Cancellation token for cancel current service call when required</param>
    /// <returns>returns Operation status</returns>
    public async Task SyncDataAsync(BaseDTO result, ServiceSyncGroups syncMethod, DataSyncFor syncFor, CancellationToken cancellationToken)
    {
        try
        {
            if (MobileConstants.CheckInternet)
            {
                switch (syncMethod)
                {
                    case ServiceSyncGroups.RSSyncFromServerGroup:
                        await SyncFromServerAsync(result, syncFor, cancellationToken).ConfigureAwait(false);
                        break;
                    case ServiceSyncGroups.RSSyncToServerGroup:
                        await SyncToServerAsync(result, syncFor, cancellationToken).ConfigureAwait(false);
                        break;
                    case ServiceSyncGroups.RSSyncFromDeviceGroup:
                        //todo:
                        //if (_essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0) == (int)RoleName.Patient)
                        //{
                        //    if (!_essentials.GetPreferenceValue(StorageConstants.PR_IS_SYNCING_FROM_FITNESS_APP, false))
                        //    {
                        //        _essentials.SetPreferenceValue(StorageConstants.PR_IS_SYNCING_FROM_FITNESS_APP, true);
                        //        await SyncDataFromHealthAppAsync(result, syncFor, -1, null, cancellationToken).ConfigureAwait(false);
                        //        _essentials.SetPreferenceValue(StorageConstants.PR_IS_SYNCING_FROM_FITNESS_APP, false);
                        //    }
                        //    else
                        //    {
                        //        result.ErrCode = ErrorCode.SyncInProgress;
                        //    }
                        //}
                        //else
                        {
                            result.ErrCode = ErrorCode.OK;
                        }
                        break;
                    default:
                        //will use for future implementation
                        break;
                }
            }
            else
            {
                result.ErrCode = ErrorCode.NoInternetConnection;
            }
        }
        catch (Exception ex)
        {
            result.ErrCode = syncMethod == ServiceSyncGroups.RSSyncFromServerGroup ? ErrorCode.ErrorWhileRetrievingRecords : ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Initializes DataSync date for the given batch
    /// </summary>
    /// <param name="syncData">Data Sync batch for which times are to be reset</param>
    /// <returns>DataSync result as reference</returns>
    public async Task ResetSyncFromAsync(DataSyncDTO syncData)
    {
        DateTimeOffset dateTimeOffset = GenericMethods.GetUtcDateTime;
        syncData.DataSyncForRecords = (from dataSyncFor in syncData.DataSyncFor select CreateDataSyncModel(dateTimeOffset, dataSyncFor)).ToList();
        using (DataSyncDatabase dataSyncForDB = new DataSyncDatabase())
        {
            await dataSyncForDB.SaveDataSyncInfoAsync(syncData).ConfigureAwait(false);
        }
    }

    #region Sync Data From Server

    private async Task SyncFromServerAsync(BaseDTO result, DataSyncFor syncFor, CancellationToken cancellationToken)
    {
        if (IsRequestValid(result, syncFor))
        {
            DataSyncDTO syncData = new DataSyncDTO { SyncBatch = syncFor.ToString(), DataSyncFor = result.Response.Split(Constants.SYMBOL_COMMA), PatientID = result.SelectedUserID };
            await new DataSyncDatabase().GetDataSyncForAsync(syncData).ConfigureAwait(false);
            if (GenericMethods.IsListNotEmpty(syncData.DataSyncForRecords))
            {
                DateTimeOffset serverDateTimeUTC = GenericMethods.GetUtcDateTime;
                await SyncMobileDataAsync(syncData, cancellationToken).ConfigureAwait(false);
                result.ErrCode = syncData.ErrCode;
                result.RecordCount = syncData.RecordCount;
                if (GenericMethods.IsListNotEmpty(syncData.DataSyncForRecords))
                {
                    if (result.ErrCode == ErrorCode.OK)
                    {
                        await Task.WhenAll(
                            from dataSyncRecord in syncData.DataSyncForRecords
                            where dataSyncRecord.ErrCode == ErrorCode.OK && dataSyncRecord.RecordCount > 0
                            select new DataSyncDatabase().UpdateDateSyncedFromServerAsync(dataSyncRecord.SyncFor, serverDateTimeUTC, result.SelectedUserID)
                        ).ConfigureAwait(false);
                    }
                    if (syncData.DataSyncForRecords.Any(x => x.SyncFor == DataSyncFor.PatientMedications.ToString() && x.RecordCount < 1))
                    {
                        _ = new MedicationSevice(_essentials).RegisterNotificationAsync();
                    }
                }
            }
            else
            {
                if (syncData.PatientID > 0)
                {
                    DateTimeOffset dateTimeOffset = GenericMethods.GetUtcDateTime;
                    syncData.DataSyncForRecords = (from syncForString in syncData.DataSyncFor
                                                   select CreateDataSyncModel(dateTimeOffset, syncForString)).ToList();
                    await new DataSyncDatabase().SaveDataSyncInfoAsync(syncData).ConfigureAwait(false);
                    await SyncFromServerAsync(result, syncFor, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    private bool IsRequestValid(BaseDTO result, DataSyncFor syncFor)
    {
        if (string.IsNullOrWhiteSpace(result.Response))
        {
            if (syncFor == DataSyncFor.MasterData)
            {
                result.Response = $"{DataSyncFor.Countries},{DataSyncFor.Languages},{DataSyncFor.Settings}";
            }
            else if (syncFor == DataSyncFor.MasterDataI18N)
            {
                result.Response = $"{DataSyncFor.AppIntros},{DataSyncFor.Features},{DataSyncFor.Pages},{DataSyncFor.Resources}";
            }
            else if (syncFor == DataSyncFor.Medicines)
            {
                result.Response = syncFor.ToString();
            }
            else
            {
                result.ErrCode = ErrorCode.InvalidData;
                return false;
            }
        }
        return true;
    }

    private async Task SyncMobileDataAsync(DataSyncDTO syncData, CancellationToken cancellationToken)
    {
        syncData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
        if (await CheckAndCancelDataSyncAsync(syncData).ConfigureAwait(false))
        {
            var httpData = new HttpServiceModel<DataSyncDTO>
            {
                CancellationToken = cancellationToken,
                AuthType = syncData.AccountID > 0
                    ? AuthorizationType.Bearer
                    : AuthorizationType.Basic,
                PathWithoutBasePath = syncData.AccountID > 0
                    ? UrlConstants.GET_MOBILE_USER_DATA_ASYNC_PATH
                    : UrlConstants.GET_MOBILE_MASTER_DATA_ASYNC_PATH,
                ContentToSend = syncData,
                QueryParameters = new NameValueCollection{
                    { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(GetLoginUserID(), CultureInfo.InvariantCulture) }
                }
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            syncData.ErrCode = httpData.ErrCode;
            syncData.Response = httpData.Response;
            await MapMobileDataServiceResponseAsync(syncData).ConfigureAwait(false);
        }
    }

    private async Task<bool> CheckAndCancelDataSyncAsync(DataSyncDTO syncData)
    {
        List<DataSyncFor> syncToCancel = new List<DataSyncFor>();
        foreach (DataSyncModel s in syncData.DataSyncForRecords)
        {
            GenericMethods.LogData($"SyncMobileDataAsync() Start; syncFor:{s.SyncFor}; Patient:{syncData.PatientID}; AccountID:{syncData.AccountID}; SyncBatch:{syncData.SyncBatch}; LastModifiedOn:{s.SyncFromServerDateTime}");
            if (s.SyncFor == DataSyncFor.Medicines.ToString())
            {
                await new MedicineService(_essentials).CheckAndAssignDefaultMedicineDataAsync(s, syncData.PatientID, syncToCancel);
            }
        }
        if (GenericMethods.IsListNotEmpty(syncToCancel))
        {
            syncData.DataSyncForRecords.RemoveAll(x => syncToCancel.Any(y => y == x.SyncFor.ToEnum<DataSyncFor>()));
        }
        return GenericMethods.IsListNotEmpty(syncData.DataSyncForRecords);
    }

    private async Task MapMobileDataServiceResponseAsync(DataSyncDTO syncData)
    {
        if (syncData.ErrCode == ErrorCode.OK)
        {
            JToken data = JToken.Parse(syncData.Response);
            if (data != null && data.HasValues)
            {
                await Task.WhenAll(from item in syncData.DataSyncForRecords select MapSyncedDataAsync(data, item)).ConfigureAwait(false);
                syncData.ErrCode = syncData.DataSyncForRecords.FirstOrDefault(x => x.ErrCode != ErrorCode.OK)?.ErrCode ?? ErrorCode.OK;
                syncData.RecordCount = syncData.DataSyncForRecords.FirstOrDefault(x => x.ErrCode == ErrorCode.OK && x.RecordCount > 0)?.RecordCount ?? 0;
            }
            await CheckConfigurationsAsync(syncData).ConfigureAwait(false);
        }
    }

    private async Task MapSyncedDataAsync(JToken data, DataSyncModel syncFor)
    {
        switch (syncFor.SyncFor.ToEnum<DataSyncFor>())
        {
            case DataSyncFor.Settings:
                await new SettingService(_essentials).MapAndSaveSettingsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Languages:
                await new LanguageService(_essentials).MapAndSaveLanguagesAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Countries:
                await new CountryService(_essentials).MapAndSaveCountriesAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Resources:
                await new ResourceService(_essentials).MapAndSaveResourcesAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Features:
                await new FeatureService(_essentials).MapAndSaveFeaturesAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.FeatureRelations:
                await new FeatureService(_essentials).MapAndSaveFeatureRelationsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Consents:
                await new ConsentService(_essentials).MapAndSaveConsentsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.UserConsents:
                await new ConsentService(_essentials).MapAndSaveUserConsentsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientPrograms:
                await new PatientProgramService(_essentials).MapAndSavePatientProgramsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Professions:
                await new ProfessionService(_essentials).MapAndSaveProfessionsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientTasks:
                await new PatientTaskService(_essentials).MapAndSaveTasksAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Programs:
                await new PatientProgramService(_essentials).MapAndSaveProgramsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Roles:
                await new RoleService(_essentials).MapAndSaveRolessAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.EducationCategories:
                await new EducationCategoryService(_essentials).MapAndSaveEducationCategoryAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Pages:
                await new ContentPageService(_essentials).MapAndSavePagesAsync(syncFor, data, nameof(DataSyncDTO.Pages), nameof(DataSyncDTO.PagesDetails), null).ConfigureAwait(false);
                break;
            case DataSyncFor.EducationDetails:
                await new ContentPageService(_essentials).MapAndSavePagesAsync(syncFor, data, nameof(DataSyncDTO.EducationPages), nameof(DataSyncDTO.EducationPageDetails), nameof(DataSyncDTO.PatientEducations)).ConfigureAwait(false);
                break;
            case DataSyncFor.Educations:
                await new ContentPageService(_essentials).MapAndSavePagesAsync(syncFor, data, nameof(DataSyncDTO.EducationPages), nameof(DataSyncDTO.EducationPageDetails), nameof(DataSyncDTO.PatientEducations)).ConfigureAwait(false);
                break;
            case DataSyncFor.Menus:
                await new MenuService(_essentials).MapAndSaveMenusAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.MenuGroups:
                await new MenuService(_essentials).MapAndSaveMenuGroupsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Users:
                await new UserService(_essentials).MapAndSaveUsersAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.DashboardConfigurations:
                await new DashboardService(_essentials).MapAndSaveDashboardConfigurationsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Chats:
                await new ChatService(_essentials).MapAndSaveChatsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Appointments:
                await new AppointmentService(_essentials).MapAndSaveAppointmentsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.AppointmentDetails:
                await new AppointmentService(_essentials).MapAndSaveAppointmentDetailsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Files:
                await new FileService(_essentials).MapAndSaveDocumentsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.FileCategories:
                await new FileCategoryService(_essentials).MapAndSaveFileCategoriesAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.FileCategoryDetails:
                await new FileCategoryService(_essentials).MapAndSaveFileCategoryDetailsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.OrganisationBranchesDepartments:
                await new OrganisationService(_essentials).MapAndSaveOrganisationAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Questionnaires:
                await new QuestionnaireService(_essentials).MapAndSaveQuestionnaireAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.QuestionnaireI18N:
                await new QuestionnaireService(_essentials).MapAndSaveQuestionnaireI18NAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Units:
            case DataSyncFor.UnitsI18N:
                await new ReadingService(_essentials).MapAndSaveUnits(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Readings:
                await new ReadingService(_essentials).MapAndSaveReadingsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.UserAccountSettings:
                await new UserAccountSettingService(_essentials).MapAndSaveUserAccountSettingsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientMedications:
                await new MedicationSevice(_essentials).MapAndSaveMedicationsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Medicines:
                await new MedicineService(_essentials).MapAndSaveMedicinesAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.AppIntros:
                await new AppIntroService(_essentials).MapAndSaveAppIntrosAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Instructions:
                await new InstructionService(_essentials).MapAndSaveInstructionsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.InstructionI18N:
                await new InstructionService(_essentials).MapAndSaveInstructionI18NAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientProviderNotes:
                await new QuestionnaireService(_essentials).MapAndSavePatientProviderNotesAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientContacts:
                await new ContactsService(_essentials).MapAndSavePatientContactsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientContactDetails:
                await new ContactsService(_essentials).MapAndSavePatientContactDetailsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientTrackers:
                await new PatientTrackerService(_essentials).MapAndSavePatientTrackersAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.Trackers:
                await new TrackerService(_essentials).MapAndSaveTrackersAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.TrackersI18N:
                await new TrackerService(_essentials).MapAndSaveTrackersI18NAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.ProgramTrackers:
                await new TrackerService(_essentials).MapAndSaveProgramTrackersAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.ProgramNotes:
                await new QuestionnaireService(_essentials).MapAndSaveProgramNotesAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.ProgramNotesI18N:
                await new QuestionnaireService(_essentials).MapAndSaveProgramNotes18NAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.ProgramCaregivers:
                await new PatientProgramService(_essentials).MapAndSaveProgramCaregiversAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.PaymentModes:
            case DataSyncFor.PaymentModesI18N:
                await new PatientBillService(_essentials).MapAndSavePaymentModes(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientBills:
                await new PatientBillService(_essentials).MapAndSavePatientBillsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.BillingItems:
                await new PatientBillService(_essentials).MapAndSaveBillingItemsAsync(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.BillingItemsI18N:
                await new PatientBillService(_essentials).MapAndSaveBillingItemsI18N(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.ProgramBillingItems:
                await new PatientBillService(_essentials).MapAndSaveProgramBillingItems(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientsSharedPrograms:
                await new PatientProgramService(_essentials).MapAndSavePatientsSharedPrograms(syncFor, data).ConfigureAwait(false);
                break;
            case DataSyncFor.ReadingMasters:
                await new ReadingService(_essentials).MapAndSaveReadingMasterData(syncFor, data).ConfigureAwait(false);
                break;
            default:
                //// will use for future implementation
                break;
        }
        GenericMethods.LogData($"MapSyncedDataAsync() END; syncFor:{syncFor.SyncFor}; Patient:{syncFor.PatientID}; ErrorCode:{syncFor.ErrCode}; RecordCount:{syncFor.RecordCount}; LastModifiedOn:{syncFor.SyncFromServerDateTime}");
    }

    private async Task CheckConfigurationsAsync(DataSyncDTO syncData)
    {
        if (syncData.ErrCode == ErrorCode.OK && syncData.PatientID == default)
        {
            string checkFor;
            if (syncData.DataSyncForRecords.Any(x => x.SyncFor == DataSyncFor.Settings.ToString() && x.SyncFor == DataSyncFor.Resources.ToString()))
            {
                checkFor = DataSyncFor.MasterData.ToString();
            }
            else if (syncData.DataSyncForRecords.Any(x => x.SyncFor == DataSyncFor.Settings.ToString()))
            {
                checkFor = DataSyncFor.Settings.ToString();
            }
            else if (syncData.DataSyncForRecords.Any(x => x.SyncFor == DataSyncFor.Resources.ToString()))
            {
                checkFor = DataSyncFor.Resources.ToString();
            }
            else
            {
                checkFor = string.Empty;
            }
            if (!string.IsNullOrWhiteSpace(checkFor))
            {
                syncData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                await new CommonDatabase().VerifyConfigAsync(syncData, checkFor, Constants.CONFIG_SETTINGS_COUNT, Constants.CONFIG_RESOURCES_COUNT).ConfigureAwait(false);
            }
        }
    }

    #endregion

    #region Sync Data To Server

    private async Task SyncToServerAsync(BaseDTO result, DataSyncFor syncFor, CancellationToken cancellationToken)
    {
        //todo:
        //DateTimeOffset currentUTCDateTime = LibGenericMethods.GetUtcDateTime;
        //string waitTime = await new SettingsService().GetSettingsValueByKeyAsync(SettingsConstants.S_SYNC_TO_SERVER_WAIT_TIME_MINUTES_KEY).ConfigureAwait(false);
        //DataSyncModel syncData = await new DataSyncLibDatabase().GetDataSyncForAsync(
        //	syncFor.ToString(),
        //	currentUTCDateTime,
        //	string.IsNullOrWhiteSpace(waitTime) ? Constants.SYNC_TO_SERVER_DEFAULT_WAIT_TIME_MINUTES : Convert.ToInt32(waitTime, CultureInfo.CurrentCulture),
        //	result.SelectedUserID
        //).ConfigureAwait(false);
        //result.ErrCode = syncData.ErrCode;
        //if (syncData.ErrCode == ErrorCode.OK)
        //{
        //	await CallSyncToServicesAsync(result, syncFor, cancellationToken).ConfigureAwait(false);
        //	//Update Operation status in dataSync Model
        //	if (result.ErrCode == ErrorCode.OK)
        //	{
        //		syncData.SyncToStatus = SyncStatus.Done;
        //		syncData.SyncToServerDateTime = currentUTCDateTime;
        //	}
        //	else
        //	{
        //		syncData.SyncToStatus = SyncStatus.Failed;
        //	}
        //}
        ////Update dataSyncDataBase
        //await new DataSyncLibDatabase().UpdateDateSyncedToServerAsync(syncData).ConfigureAwait(false);
    }

    private async Task CallSyncToServicesAsync(BaseDTO result, DataSyncFor syncFor, CancellationToken cancellationToken)
    {
        BaseDTO responseData;
        switch (syncFor)
        {
            case DataSyncFor.ErrorLogs:
                responseData = result;
                await new ErrorLogService(_essentials).SyncErrorLogsToServerAsync(responseData, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.Chats:
                responseData = new ChatDTO();
                await new ChatService(_essentials).SyncChatDetailsToServerAsync(responseData as ChatDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.UserConsents:
                responseData = new ConsentDTO();
                await new ConsentService(_essentials).SyncConsentsToServerAsync(responseData as ConsentDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.Files:
                responseData = new FileDTO();
                await new FileService(_essentials).SyncFilesToServerAsync(responseData as FileDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientPrograms:
                responseData = new PatientProgramDTO();
                await new PatientProgramService(_essentials).SyncPatientProgramsToServerAsync(responseData as PatientProgramDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.Users:
                responseData = new UserDTO { User = new UserModel { IsActive = true } };
                await new UserService(_essentials).SyncUserToServerAsync(responseData as UserDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.Questionnaires:
                responseData = new QuestionnaireDTO();
                await new QuestionnaireService(_essentials).SyncQuestionnaireResultsToServerAsync(responseData as QuestionnaireDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientTasks:
                responseData = new ProgramDTO();
                await new PatientTaskService(_essentials).SyncPatientTaskStatusToServerAsync(responseData as ProgramDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientReadings:
                responseData = new PatientReadingDTO { RecordCount = -11 };
                await new ReadingService(_essentials).SyncPatientReadingsToServerAsync(responseData as PatientReadingDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.UserAccountSettings:
                responseData = new UserAccountSettingDTO();
                await new UserAccountSettingService(_essentials).SyncUserAccountSettingsToServerAsync(responseData as UserAccountSettingDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientTargets:
                responseData = new PatientReadingDTO();
                await new ReadingService(_essentials).SyncUserReadingTargetsToServerAsync(responseData as PatientReadingDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientMedications:
                responseData = new PatientMedicationDTO();
                await new MedicationSevice(_essentials).SyncMedicationsToServerAsync(responseData as PatientMedicationDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.Educations:
                responseData = new ContentPageDTO();
                await new ContentPageService(_essentials).SyncEducationStatusToServerAsync(responseData as ContentPageDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientContacts:
                responseData = new ContactDTO();
                await new ContactsService(_essentials).SyncContactsToServerAsync(responseData as ContactDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientBills:
                responseData = new BillingItemDTO();
                await new PatientBillService(_essentials).SyncPatientBillToServerAsync(responseData as BillingItemDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientTrackers:
                responseData = new TrackerDTO();
                await new PatientTrackerService(_essentials).SyncPatientTrackersToServerAsync(responseData as TrackerDTO, cancellationToken).ConfigureAwait(false);
                await new PatientTrackerService(_essentials).SyncPatientTrackerValuesToServerAsync(responseData as TrackerDTO, cancellationToken).ConfigureAwait(false);
                break;
            case DataSyncFor.PatientProviderNotes:
                responseData = new PatientProviderNoteDTO();
                await new QuestionnaireService(_essentials).SyncPatientProviderNoteToServerAsync(responseData as PatientProviderNoteDTO, cancellationToken).ConfigureAwait(false);
                break;
            default:
                responseData = result;
                break;
        }
        result.ErrCode = responseData.ErrCode;
        result.RecordCount = responseData.RecordCount;
    }

    #endregion
}