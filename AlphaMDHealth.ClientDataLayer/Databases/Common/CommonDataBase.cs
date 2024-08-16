using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    /// <summary>
    /// Represents common database module
    /// </summary>
    public class CommonDatabase : BaseDatabase
    {
        private Type[] GetMasterTableTypes()
        {
            return new Type[] {
                typeof(SettingModel),
                typeof(LanguageModel),
                typeof(ResourceModel),
                typeof(CountryModel),
                typeof(OrganizationFeaturePermissionModel),
                typeof(AppIntroModel)
                //typeof(MedicineModel)
            };
        }

        private Type[] GetUserTableTypes()
        {
            return new Type[] {
                typeof(ConfigureDashboardModel),
                typeof(SystemFeatureParameterModel),
                typeof(UserModel),
                typeof(UserRelationModel),
                typeof(MenuModel),
                typeof(MobileMenuNodeModel),
                typeof(MenuGroupModel),
                typeof(ContentDetailModel),
                typeof(ContentPageModel),
                typeof(PatientEducationModel),
                typeof(ChatModel),
                typeof(ChatDetailModel),
                typeof(AppointmentModel),
                typeof(AppointmentDetailModel),
                typeof(ParticipantsModel),
                typeof(EductaionCatergoryModel),
                typeof(EducationCategoryDetailModel),
                typeof(FileModel),
                typeof(FileDocumentModel),
                typeof(FileCategoryModel),
                typeof(FileCategoryDetailModel),
                typeof(ConsentModel),
                typeof(PatientProgramModel),
                typeof(ProgramModel),
                typeof(OrganisationModel),
                typeof(TaskModel),
                typeof(UserConsentModel),
                typeof(UserProfessionModel),
                typeof(QuestionnaireModel),
                typeof(QuestionnaireDetailsModel),
                typeof(QuestionnaireQuestionModel),
                typeof(QuestionnaireQuestionDetailsModel),
                typeof(QuestionnaireQuestionOptionModel),
                typeof(QuestionnaireQuestionOptionDetailsModel),
                typeof(QuestionConditionModel),
                typeof(QuestionScoreModel),
                typeof(QuestionnaireSubscaleModel),
                typeof(QuestionnaireSubscaleRangesModel),
                typeof(QuestionnaireSubscaleRangeDetailsModel),
                typeof(PatientQuestionnaireQuestionAnswersModel),
                typeof(PatientQuestionnaireScoresModel),
                typeof(UserRolesModel),
                typeof(UnitModel),
                typeof(UnitI18NModel),
                typeof(ReadingModel),
                typeof(ReadingRangeModel),
                typeof(ReadingTargetModel),
                typeof(PatientReadingModel),
                typeof(UserHealthPermissionRequestModel),
                typeof(UserAccountSettingsModel),
                typeof(PatientDeviceModel),
                typeof(ReadingDeviceModel),
                typeof(PatientMedicationModel),
                typeof(MedicationReminderModel),
                typeof(LocalNotificationModel),
                typeof(InstructionModel),
                typeof(InstructionI18NModel),
                typeof(ContactModel),
                typeof(ContactDetailModel),
                typeof(PatientTrackersModel),
                typeof(TrackersModel),
                typeof(TrackerRangeModel),
                typeof(TrackerRangesI18N),
                typeof(ProgramTrackerModel),
                typeof(PatientBillModel),
                typeof(PatientBillItemModel),
                typeof(BillItemModel),
                typeof(BillingItemsI18NModel),
                typeof(ProgramBillingModel),
                typeof(ProgramNoteModel),
                typeof(ProgramNoteI18NModel),
                typeof(CaregiverModel),
                typeof(PatientProviderNoteModel),
                typeof(TrackersI18NModel),
                typeof(PatientsSharedProgramsModel),
                typeof(ReadingMasterModel)
            };
        }

        private Type[] GetAllTableTypes()
        {
            return new Type[] {
                typeof(SettingModel),
                typeof(ResourceModel),
                typeof(ErrorLogModel),
                typeof(DataSyncModel),
                typeof(SyncConfigurationModel),
                typeof(LanguageModel),
                typeof(CountryModel),
                typeof(OrganizationFeaturePermissionModel),
                typeof(ConfigureDashboardModel),
                typeof(SystemFeatureParameterModel),
                typeof(UserModel),
                typeof(UserRelationModel),
                typeof(MenuModel),
                typeof(MobileMenuNodeModel),
                typeof(MenuGroupModel),
                typeof(ContentDetailModel),
                typeof(FeatureRelationModel),
                typeof(ChatModel),
                typeof(ChatDetailModel),
                typeof(AppointmentModel),
                typeof(AppointmentDetailModel),
                typeof(ContentPageModel),
                typeof(PatientEducationModel),
                typeof(ParticipantsModel),
                typeof(EductaionCatergoryModel),
                typeof(EducationCategoryDetailModel),
                typeof(FileModel),
                typeof(FileDocumentModel),
                typeof(FileCategoryModel),
                typeof(FileCategoryDetailModel),
                typeof(ConsentModel),
                typeof(PatientProgramModel),
                typeof(ProgramModel),
                typeof(OrganisationModel),
                typeof(TaskModel),
                typeof(UserConsentModel),
                typeof(UserProfessionModel),
                typeof(QuestionnaireModel),
                typeof(QuestionnaireDetailsModel),
                typeof(QuestionnaireQuestionModel),
                typeof(QuestionnaireQuestionDetailsModel),
                typeof(QuestionnaireQuestionOptionModel),
                typeof(QuestionnaireQuestionOptionDetailsModel),
                typeof(QuestionConditionModel),
                typeof(QuestionScoreModel),
                typeof(QuestionnaireSubscaleModel),
                typeof(QuestionnaireSubscaleRangesModel),
                typeof(QuestionnaireSubscaleRangeDetailsModel),
                typeof(PatientQuestionnaireQuestionAnswersModel),
                typeof(PatientQuestionnaireScoresModel),
                typeof(UserRolesModel),
                typeof(UnitModel),
                typeof(UnitI18NModel),
                typeof(ReadingModel),
                typeof(ReadingRangeModel),
                typeof(ReadingTargetModel),
                typeof(PatientReadingModel),
                typeof(UserHealthPermissionRequestModel),
                typeof(UserAccountSettingsModel),
                typeof(PatientDeviceModel),
                typeof(ReadingDeviceModel),
                typeof(MedicineModel),
                typeof(PatientMedicationModel),
                typeof(MedicationReminderModel),
                typeof(LocalNotificationModel),
                typeof(AppIntroModel),
                typeof(InstructionModel),
                typeof(InstructionI18NModel),
                typeof(ContactModel),
                typeof(ContactDetailModel),
                typeof(ProgramNoteModel),
                typeof(ProgramNoteI18NModel),
                typeof(PatientProviderNoteModel),
                typeof(CaregiverModel),
                typeof(PatientTrackersModel),
                typeof(TrackersModel),
                typeof(TrackerRangeModel),
                typeof(TrackerRangesI18N),
                typeof(ProgramTrackerModel),
                typeof(PatientTrackersValuesModel),
                typeof(BillPaymentModel),
                typeof(PaymentModeI18NModel),
                typeof(PatientBillModel),
                typeof(PatientBillItemModel),
                typeof(BillItemModel),
                typeof(BillingItemsI18NModel),
                typeof(ProgramBillingModel),
                typeof(TrackersI18NModel),
                typeof(PatientsSharedProgramsModel),
                typeof(ReadingMasterModel)
            };
        }

        /// <summary>
        /// Gets the Sqlite Database connection
        /// </summary>
        /// <param name="types">database model types</param>
        /// <returns>returns after initializing connection and tables</returns>
        public async Task InitializeDatabaseAsync()
        {
            if (SqlConnection == null)
            {
                SqlConnection = new SQLiteAsyncConnection(LocalDBConstants.DatabasePath, LocalDBConstants.Flags);
                //var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), LibConstants.SQLITE_DB_NAME);
                //SQLiteOpenFlags openFlags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex;
                //SqlConnection = new SQLiteAsyncConnection(databasePath, openFlags);

                await SqlConnection.CreateTablesAsync(types: GetAllTableTypes()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Cleanups all data for current user, asynchronously.
        /// </summary>
        /// <returns>operation status</returns>
        public async Task CleanUserDataAsync()
        {
            await CleanTablesAsync(GetUserTableTypes()).ConfigureAwait(false);
        }

        /// <summary>
        /// Cleanups all master data of Current Environment, asynchronously.
        /// </summary>
        /// <returns>operation status</returns>
        public async Task CleanMasterDataAsync()
        {
            await CleanTablesAsync(GetMasterTableTypes()).ConfigureAwait(false);
        }

        /// <summary>
        /// Cleanups all data from the given tables
        /// </summary>
        /// <returns>operation status</returns>
        public async Task CleanTablesAsync(params Type[] types)
        {
            await Task.WhenAll(
                from type in types
                select SqlConnection.DeleteAllAsync(new TableMapping(type))
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Verify config data count with the count required by the app to run
        /// </summary>
        /// <param name="configData">Config data with record count. Also used to send errorCode as response</param>
        /// <param name="configType">Type of config data</param>
        /// <param name="appSettingCount">Expected setting count in client app</param>
        /// <param name="appResourceCount">Expected resource count in client app</param>
        /// <returns>Error code</returns>
        public async Task VerifyConfigAsync(BaseDTO configData, string configType, int appSettingCount, int appResourceCount)
        {
            if (configData.ErrCode == ErrorCode.OK)
            {
                bool isMatched = false;
                switch (configType)
                {
                    case "Settings":
                        isMatched = appSettingCount == (await new SettingLibDatabase().GetStaticSettingsCountAsync().ConfigureAwait(false));
                        break;
                    case "Resources":
                        isMatched = appResourceCount == (await new ResourceDatabase().GetStaticResourcesCountAsync(configData.LanguageID).ConfigureAwait(false));
                        break;
                    case "MasterData":
                        isMatched = appSettingCount == (await new SettingLibDatabase().GetStaticSettingsCountAsync().ConfigureAwait(false))
                                    && appResourceCount == (await new ResourceDatabase().GetStaticResourcesCountAsync(configData.LanguageID).ConfigureAwait(false));
                        break;
                    default:
                        //will use for future implementation
                        break;
                }
                if (!isMatched)
                {
                    //configData.ErrCode = ErrorCode.RecordCountMismatch;
                }
            }
        }

    }
}