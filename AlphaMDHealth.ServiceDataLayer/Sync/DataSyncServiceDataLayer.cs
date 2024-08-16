using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer;

public class DataSyncServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Gets master data 
    /// </summary>
    /// <param name="syncData">Data sync info reference object which contains last synced data and holds output data with operations status</param>
    /// <returns>Operation status with master data in case of success</returns>
    public async Task GetMobileDataAsync(DataSyncDTO syncData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        var tbl = ToDataSyncTable(syncData);
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), syncData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(DataSyncDTO.SyncBatch)), syncData.SyncBatch, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), tbl.AsTableValuedParameter());
        parameter.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), syncData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(DataSyncDTO.PatientID)), syncData.PatientID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.LanguageID)), syncData.LanguageID, DbType.Byte, ParameterDirection.Input);
        MapCommonSPParameters(syncData, parameter, null);
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_MOBILE_DATA, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            if (syncData.DataSyncForRecords?.Count > 0)
            {
                foreach (DataSyncModel item in syncData.DataSyncForRecords)
                {
                    switch (item.SyncFor.ToEnum<DataSyncFor>())
                    {
                        case DataSyncFor.AppIntros:
                            syncData.AppIntros = await MapTableDataAsync<AppIntroModel>(DataSyncFor.AppIntros, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.AppointmentDetails:
                            syncData.AppointmentDetails = await MapTableDataAsync<ContentDetailModel>(DataSyncFor.AppointmentDetails, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Appointments:
                            await MapAppointmentData(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.BillingItems:
                            syncData.BillingItems = await MapTableDataAsync<BillItemModel>(DataSyncFor.BillingItems, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.BillingItemsI18N:
                            syncData.BillingItemsI18N = await MapTableDataAsync<BillingItemsI18NModel>(DataSyncFor.BillingItemsI18N, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Chats:
                            await MapChatData(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Consents:
                            syncData.Consents = await MapTableDataAsync<ConsentModel>(DataSyncFor.Consents, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Countries:
                            syncData.CountryCodes = await MapTableDataAsync<CountryModel>(DataSyncFor.Countries, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.DashboardConfigurations:
                            await MapDashboardConfigurationData(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.EducationCategories:
                            await MapEducationCategoryData(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.EducationDetails:
                            syncData.EducationPageDetails = await MapTableDataAsync<ContentDetailModel>(DataSyncFor.EducationDetails, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Educations:
                            await MapEducationData(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.FeatureRelations:
                            syncData.FeatureRelations = await MapTableDataAsync<FeatureRelationModel>(DataSyncFor.FeatureRelations, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Features:
                            syncData.Features = await MapTableDataAsync<OrganizationFeaturePermissionModel>(DataSyncFor.Features, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.FileCategories:
                            syncData.FileCategories = await MapTableDataAsync<FileCategoryModel>(DataSyncFor.FileCategories, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.FileCategoryDetails:
                            syncData.FileCategoryDetails = await MapTableDataAsync<FileCategoryDetailModel>(DataSyncFor.FileCategoryDetails, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Files:
                            await MapFileData(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.InstructionI18N:
                            syncData.InstructionI18N = await MapTableDataAsync<InstructionI18NModel>(DataSyncFor.InstructionI18N, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Instructions:
                            syncData.Instructions = await MapTableDataAsync<InstructionModel>(DataSyncFor.Instructions, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Languages:
                            syncData.Languages = await MapTableDataAsync<LanguageModel>(DataSyncFor.Languages, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Medicines:
                            syncData.Medicines = await MapTableDataAsync<MedicineModel>(DataSyncFor.Medicines, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.MenuGroups:
                            syncData.MenuGroups = await MapTableDataAsync<MenuGroupModel>(DataSyncFor.MenuGroups, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Menus:
                            await MapMenuData(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.OrganisationBranchesDepartments:
                            syncData.Organisations = await MapTableDataAsync<OrganisationModel>(DataSyncFor.OrganisationBranchesDepartments, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Pages:
                            await MapPageData(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.PatientBills:
                            await MapPatientBills(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.PatientContactDetails:
                            syncData.PatientContactDetails = await MapTableDataAsync<ContactDetailModel>(DataSyncFor.PatientContactDetails, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.PatientContacts:
                            syncData.PatientContacts = await MapTableDataAsync<ContactModel>(DataSyncFor.PatientContacts, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.PatientMedications:
                            await MapPatientMedicationData(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.PatientPrograms:
                            syncData.PatientPrograms = await MapTableDataAsync<PatientProgramModel>(DataSyncFor.PatientPrograms, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.PatientProviderNotes:
                            syncData.PatientProviderNotes = await MapTableDataAsync<PatientProviderNoteModel>(DataSyncFor.PatientProviderNotes, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.PatientsSharedPrograms:
                            syncData.PatientsSharedPrograms = await MapTableDataAsync<PatientsSharedProgramsModel>(DataSyncFor.PatientsSharedPrograms, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.PatientTasks:
                            syncData.PatientTasks = await MapTableDataAsync<TaskModel>(DataSyncFor.PatientTasks, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.PatientTrackers:
                            await MapPatientTrackers(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.PaymentModes:
                            syncData.BillPaymentModes = await MapTableDataAsync<BillPaymentModel>(DataSyncFor.PaymentModes, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.PaymentModesI18N:
                            syncData.PaymentModeI18N = await MapTableDataAsync<PaymentModeI18NModel>(DataSyncFor.PaymentModesI18N, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Professions:
                            syncData.Professions = await MapTableDataAsync<UserProfessionModel>(DataSyncFor.Professions, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.ProgramBillingItems:
                            syncData.ProgramBillingItems = await MapTableDataAsync<ProgramBillingModel>(DataSyncFor.ProgramBillingItems, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.ProgramCaregivers:
                            syncData.ProgramCaregivers = await MapTableDataAsync<CaregiverModel>(DataSyncFor.ProgramCaregivers, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.ProgramNotes:
                            syncData.ProgramNotes = await MapTableDataAsync<ProgramNoteModel>(DataSyncFor.ProgramNotes, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.ProgramNotesI18N:
                            syncData.ProgramNotesI18N = await MapTableDataAsync<ProgramNoteI18NModel>(DataSyncFor.ProgramNotesI18N, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Programs:
                            syncData.Programs = await MapTableDataAsync<ProgramModel>(DataSyncFor.Programs, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.ProgramTrackers:
                            syncData.ProgramTrackers = await MapTableDataAsync<ProgramTrackerModel>(DataSyncFor.ProgramTrackers, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.QuestionnaireI18N:
                            await MapQuestionnaireDetailData(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Questionnaires:
                            await MapQuestionnaireData(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.ReadingMasters:
                            syncData.ReadingMasters = await MapTableDataAsync<ReadingMasterModel>(DataSyncFor.ReadingMasters, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Readings:
                            await MapReadingData(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Resources:
                            syncData.Resources = await MapTableDataAsync<ResourceModel>(DataSyncFor.Resources, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Roles:
                            syncData.Roles = await MapTableDataAsync<UserRolesModel>(DataSyncFor.Roles, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Settings:
                            syncData.Settings = await MapTableDataAsync<SettingModel>(DataSyncFor.Settings, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Trackers:
                            await MapTrackers(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.TrackersI18N:
                            await MapTrackersI18N(syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Units:
                            syncData.Units = await MapTableDataAsync<UnitModel>(DataSyncFor.Units, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.UnitsI18N:
                            syncData.UnitsI18N = await MapTableDataAsync<UnitI18NModel>(DataSyncFor.UnitsI18N, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.UserAccountSettings:
                            syncData.UserAccountSettings = await MapTableDataAsync<UserAccountSettingsModel>(DataSyncFor.UserAccountSettings, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.UserConsents:
                            syncData.UserConsents = await MapTableDataAsync<UserConsentModel>(DataSyncFor.UserConsents, syncData, result).ConfigureAwait(false);
                            break;
                        case DataSyncFor.Users:
                            await MapUserData(syncData, result).ConfigureAwait(false);
                            break;
                    }
                }
            }
            if (!result.IsConsumed)
            {
                syncData.DataSyncForRecords = (await result.ReadAsync<DataSyncModel>().ConfigureAwait(false))?.ToList();
            }
        }
        syncData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
    }

    private async Task MapAppointmentData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.Appointments = await MapTableDataAsync<AppointmentModel>(DataSyncFor.Appointments, syncData, result).ConfigureAwait(false);
        syncData.AppointmentParticipants = await MapTableDataAsync<ParticipantsModel>(DataSyncFor.Appointments, syncData, result).ConfigureAwait(false);
        syncData.ExternalParticipants = await MapTableDataAsync<ParticipantsModel>(DataSyncFor.Appointments, syncData, result).ConfigureAwait(false);
    }

    private async Task MapChatData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.Chats = await MapTableDataAsync<ChatModel>(DataSyncFor.Chats, syncData, result).ConfigureAwait(false);
        syncData.ChatDetails = await MapTableDataAsync<ChatDetailModel>(DataSyncFor.Chats, syncData, result).ConfigureAwait(false);
    }

    private async Task MapDashboardConfigurationData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.DashboardConfigurations = await MapTableDataAsync<ConfigureDashboardModel>(DataSyncFor.DashboardConfigurations, syncData, result).ConfigureAwait(false);
        syncData.DashboardConfigurationParameters = await MapTableDataAsync<SystemFeatureParameterModel>(DataSyncFor.DashboardConfigurations, syncData, result).ConfigureAwait(false);
    }

    private async Task MapEducationCategoryData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.EducationCategory = await MapTableDataAsync<EductaionCatergoryModel>(DataSyncFor.EducationCategories, syncData, result).ConfigureAwait(false);
        syncData.EducationCategoryDetails = await MapTableDataAsync<EducationCategoryDetailModel>(DataSyncFor.EducationCategories, syncData, result).ConfigureAwait(false);
    }

    private async Task MapEducationData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        //syncData.EducationPageDetails = await MapTableDataAsync<ContentDetailModel>(DataSyncFor.EducationDetails, syncData, result).ConfigureAwait(false);
        syncData.EducationPages = await MapTableDataAsync<ContentPageModel>(DataSyncFor.Educations, syncData, result).ConfigureAwait(false);
        syncData.PatientEducations = await MapTableDataAsync<PatientEducationModel>(DataSyncFor.Educations, syncData, result).ConfigureAwait(false);
    }

    private async Task MapFileData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.Files = await MapTableDataAsync<FileModel>(DataSyncFor.Files, syncData, result).ConfigureAwait(false);
        syncData.FileDocuments = await MapTableDataAsync<FileDocumentModel>(DataSyncFor.Files, syncData, result).ConfigureAwait(false);
    }

    private async Task MapMenuData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.Menus = await MapTableDataAsync<MenuModel>(DataSyncFor.Menus, syncData, result).ConfigureAwait(false);
        syncData.MenuNodes = await MapTableDataAsync<MobileMenuNodeModel>(DataSyncFor.Menus, syncData, result).ConfigureAwait(false);
    }

    private async Task MapPageData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.Pages = await MapTableDataAsync<ContentPageModel>(DataSyncFor.Pages, syncData, result).ConfigureAwait(false);
        syncData.PagesDetails = await MapTableDataAsync<ContentDetailModel>(DataSyncFor.Pages, syncData, result).ConfigureAwait(false);
    }

    private async Task MapPatientBills(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.PatientBills = await MapTableDataAsync<PatientBillModel>(DataSyncFor.PatientBills, syncData, result).ConfigureAwait(false);
        syncData.PatientBillItems = await MapTableDataAsync<PatientBillItemModel>(DataSyncFor.PatientBills, syncData, result).ConfigureAwait(false);
    }

    private async Task MapPatientMedicationData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.PatientMedications = await MapTableDataAsync<PatientMedicationModel>(DataSyncFor.PatientMedications, syncData, result).ConfigureAwait(false);
        syncData.MedicationReminders = await MapTableDataAsync<MedicationReminderModel>(DataSyncFor.PatientMedications, syncData, result).ConfigureAwait(false);
    }

    private async Task MapPatientTrackers(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.PatientTrackers = await MapTableDataAsync<PatientTrackersModel>(DataSyncFor.PatientTrackers, syncData, result).ConfigureAwait(false);
        syncData.PatientTrackerValues = await MapTableDataAsync<PatientTrackersValuesModel>(DataSyncFor.PatientTrackers, syncData, result).ConfigureAwait(false);
    }

    private async Task MapQuestionnaireDetailData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.QuestionnaireDetails = await MapTableDataAsync<QuestionnaireDetailsModel>(DataSyncFor.QuestionnaireI18N, syncData, result).ConfigureAwait(false);
        syncData.QuestionnaireQuestionDetails = await MapTableDataAsync<QuestionnaireQuestionDetailsModel>(DataSyncFor.QuestionnaireI18N, syncData, result).ConfigureAwait(false);
        syncData.QuestionnaireQuestionOptionDetails = await MapTableDataAsync<QuestionnaireQuestionOptionDetailsModel>(DataSyncFor.QuestionnaireI18N, syncData, result).ConfigureAwait(false);
        syncData.QuestionnaireRecommendations = await MapTableDataAsync<QuestionnaireSubscaleRangeDetailsModel>(DataSyncFor.QuestionnaireI18N, syncData, result).ConfigureAwait(false);
    }

    private async Task MapQuestionnaireData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.Questionnaires = await MapTableDataAsync<QuestionnaireModel>(DataSyncFor.Questionnaires, syncData, result).ConfigureAwait(false);
        syncData.QuestionnaireQuestions = await MapTableDataAsync<QuestionnaireQuestionModel>(DataSyncFor.Questionnaires, syncData, result).ConfigureAwait(false);
        syncData.QuestionnaireQuestionOptions = await MapTableDataAsync<QuestionnaireQuestionOptionModel>(DataSyncFor.Questionnaires, syncData, result).ConfigureAwait(false);
        syncData.QuestionnaireSubscales = await MapTableDataAsync<QuestionnaireSubscaleModel>(DataSyncFor.Questionnaires, syncData, result).ConfigureAwait(false);
        syncData.QuestionnaireSubscaleRanges = await MapTableDataAsync<QuestionnaireSubscaleRangesModel>(DataSyncFor.Questionnaires, syncData, result).ConfigureAwait(false);
        syncData.QuestionConditions = await MapTableDataAsync<QuestionConditionModel>(DataSyncFor.Questionnaires, syncData, result).ConfigureAwait(false);
        syncData.QuestionScores = await MapTableDataAsync<QuestionScoreModel>(DataSyncFor.Questionnaires, syncData, result).ConfigureAwait(false);
        syncData.PatientQuestionnaireQuestionAnswers = await MapTableDataAsync<PatientQuestionnaireQuestionAnswersModel>(DataSyncFor.Questionnaires, syncData, result).ConfigureAwait(false);
        syncData.PatientQuestionnaireScores = await MapTableDataAsync<PatientQuestionnaireScoresModel>(DataSyncFor.Questionnaires, syncData, result).ConfigureAwait(false);
    }

    private async Task MapReadingData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.Readings = await MapTableDataAsync<ReadingModel>(DataSyncFor.Readings, syncData, result).ConfigureAwait(false);
        syncData.ReadingRanges = await MapTableDataAsync<ReadingRangeModel>(DataSyncFor.Readings, syncData, result).ConfigureAwait(false);
        syncData.PatientReadingTargets = await MapTableDataAsync<ReadingTargetModel>(DataSyncFor.Readings, syncData, result).ConfigureAwait(false);
        //syncData.ReadingDevices = await MapTableDataAsync<ReadingDeviceModel>(DataSyncFor.Readings, syncData, result).ConfigureAwait(false);
        syncData.PatientReadingDevices = await MapTableDataAsync<PatientDeviceModel>(DataSyncFor.Readings, syncData, result).ConfigureAwait(false);
        syncData.PatientReadings = await MapTableDataAsync<PatientReadingModel>(DataSyncFor.Readings, syncData, result).ConfigureAwait(false);
    }

    private async Task MapTrackers(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.Trackers = await MapTableDataAsync<TrackersModel>(DataSyncFor.Trackers, syncData, result).ConfigureAwait(false);
        syncData.TrackerRanges = await MapTableDataAsync<TrackerRangeModel>(DataSyncFor.Trackers, syncData, result).ConfigureAwait(false);
    }

    private async Task MapTrackersI18N(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.TrackerRangesI18N = await MapTableDataAsync<TrackerRangesI18N>(DataSyncFor.TrackersI18N, syncData, result).ConfigureAwait(false);
        syncData.TrackersI18N = await MapTableDataAsync<TrackersI18NModel>(DataSyncFor.TrackersI18N, syncData, result).ConfigureAwait(false);
    }

    private async Task MapUserData(DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        syncData.Users = await MapTableDataAsync<UserModel>(DataSyncFor.Users, syncData, result).ConfigureAwait(false);
        syncData.UserRelations = await MapTableDataAsync<UserRelationModel>(DataSyncFor.Users, syncData, result).ConfigureAwait(false);
    }

    /// <summary>
    /// Converts List of dataSync to Datatable
    /// </summary>
    /// <param name="dataSyncForRecords">List of dataSync data to be mapped in DataTable</param>
    /// <returns>DataSync DataTable</returns>
    private DataTable ToDataSyncTable(DataSyncDTO syncData)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
                {
                    new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(long)),
                    new DataColumn(nameof(DataSyncModel.SyncFor), typeof(string)),
                    new DataColumn(nameof(DataSyncModel.SyncFromServerDateTime), typeof(DateTimeOffset)),
                    new DataColumn(nameof(DataSyncModel.ErrCode), typeof(long)),
                }
        };
        if (syncData.DataSyncForRecords?.Count > 0)
        {
            int tempID = 0;
            foreach (DataSyncModel item in syncData.DataSyncForRecords)
            {
                tempID++;
                dataTable.Rows.Add(tempID, item.SyncFor, item.SyncFromServerDateTime, 0);
            }
        }
        return dataTable;
    }

    private async Task<List<T>> MapTableDataAsync<T>(DataSyncFor syncFor, DataSyncDTO syncData, SqlMapper.GridReader result)
    {
        return (!result.IsConsumed && syncData.DataSyncForRecords.Any(x => x.SyncFor == syncFor.ToString()))
            ? (await result.ReadAsync<T>().ConfigureAwait(false))?.ToList()
            : null;
    }
}