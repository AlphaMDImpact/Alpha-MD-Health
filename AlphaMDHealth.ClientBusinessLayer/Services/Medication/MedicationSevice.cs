using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer
{
    /// <summary>
    /// Medication service 
    /// </summary>
    public partial class MedicationSevice : BaseService
    {
        #region Public Methods

        /// <summary>
        /// Get list of patient medications
        /// </summary>
        /// <param name="medicationData">Reference object to return patient medication data</param>
        /// <param name="selectedTabKey">Selected tab name to retrieve the particular list</param>
        /// <returns>Operation status with patient medications in reference object</returns>
        public async Task GetMedicationsAsync(PatientMedicationDTO medicationData, string selectedTabKey = null)
        {
            try
            {
                medicationData.SelectedUserID = GetUserID();
                if (MobileConstants.IsMobilePlatform)
                {
                    medicationData.AddedBy = _essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0).ToString();
                    medicationData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                    medicationData.OrganisationID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, 0);
                    medicationData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                    List<Task> tasks = new List<Task> {
                         GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, GroupConstants.RS_MEDICATION_GROUP
                            , GroupConstants.RS_ORGANISATION_THEMES_STYLES_GROUP, GroupConstants.RS_USER_DEGREES_GROUPS),
                         GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_MEDICATION_GROUP, GroupConstants.RS_ORGANISATION_READINGS_PAGE_GROUP
                            , GroupConstants.RS_MEDICATION_FREQUENCY_TYPE_GROUP, GroupConstants.RS_MEDICATION_NOTES_GROUP, GroupConstants.RS_MEDICATION_FREQUENCY_GROUP
                            , GroupConstants.RS_GENDER_TYPE_GROUP, GroupConstants.RS_PROGRAMS_GROUP, GroupConstants.RS_USER_DEGREES_GROUPS, GroupConstants.RS_MENU_ACTION_GROUP),
                         GetFeaturesAsync(AppPermissions.PatientMedicationsView.ToString(), AppPermissions.PatientMedicationAddEdit.ToString(), AppPermissions.PrescriptionView.ToString(), AppPermissions.PrescriptionShare.ToString())
                    };
                    switch (medicationData.RecordCount)
                    {
                        case -2:
                            tasks.Add(new MedicationDatabase().GetPrescriptionAsync(medicationData));
                            break;
                        case -1:
                            tasks.Add(new MedicationDatabase().GetPatientMedicationAsync(medicationData));
                            break;
                        default:
                            tasks.Add(new MedicationDatabase().GetPatientMedicationsAsync(medicationData, selectedTabKey));
                            break;
                    }
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                    if (medicationData.RecordCount == 0 || medicationData.RecordCount == -2 || medicationData.RecordCount > 0)
                    {
                        foreach (var medication in medicationData.Medications)
                        {
                            medication.StartDate = _essentials.ConvertToLocalTime(medication.StartDate.Value);
                        }
                        var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
                        if (selectedTabKey == ResourceConstants.R_OPEN_TASK_KEY && (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker))
                        {
                            medicationData.Medications = medicationData.Medications.Where(x => x.StartDate.Value.Date <= GenericMethods.GetUtcDateTime.Date).ToList();
                        }
                        medicationData.Medications = medicationData.Medications.OrderByDescending(x => x.StartDate.Value.Date).ToList();
                    }
                }
                else
                {
                    await SyncMedicationsFromServerAsync(medicationData).ConfigureAwait(false);
                }
                GetMedicationsUIData(medicationData);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                medicationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// Get Data from Printing Bill
        /// </summary>
        /// <param name="patientBillData">Billing Data</param>
        /// <param name="OrganisationLogo">Organisation Logo</param>
        /// <param name="OrganisationName">Organisation Name</param>
        /// <returns>HTML Data, ErrorCode and FileName</returns>
        public (string, ErrorCode, string) GetPrintData(PatientMedicationDTO medicationData, string OrganisationLogo, string OrganisationName)
        {
            try
            {
                List<string> OrgDetails = new List<string>
                {
                    OrganisationLogo,
                    OrganisationName,
                    medicationData.Medication.OrganisationAddress,
                    medicationData.Medication.OrganisationContact
                };

                List<string> PatientAndCaregiverDetail = new List<string>
                {
                     medicationData.Medication.PatientName,
                     medicationData.Caregiver.FirstName,
                     medicationData.Medication.DoctorNameDisplayString
                };

                string filename = medicationData.Medication.PatientMedicationID.ToString();
                return (new HtmlDataService(_essentials).CreateHtml(filename, OrgDetails, PatientAndCaregiverDetail, medicationData), ErrorCode.OK, filename);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                return (string.Empty, ErrorCode.InvalidData, string.Empty);
            }
        }

        private void MapUserDegreesOptionsData(PatientMedicationDTO medicationData)
        {
            medicationData.UserDegrees = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_USER_DEGREES_GROUPS, string.Empty, false
                , GetSelectedIDs(medicationData.Caregiver.Degrees)).Where(y => y.IsSelected)?.ToList();
            medicationData.Caregiver.Degrees = string.Join(",", medicationData.UserDegrees?.Select(x => x.OptionText));
        }

        private List<OptionModel> MapFrequencyOptionsData(PatientMedicationModel medication)
        {
            return MapResourcesIntoOptionsByKeyID(GroupConstants.RS_MEDICATION_FREQUENCY_GROUP, string.Empty, false
                , medication?.HowOften ?? -1);
        }

        public List<OptionModel> MapFrequencyTypeOptionsData(PatientMedicationModel medication)
        {
            return MapResourcesIntoOptionsByKeyID(GroupConstants.RS_MEDICATION_FREQUENCY_TYPE_GROUP, string.Empty, false
                , GetSelectedIDs(medication?.Frequency));
        }

        private List<OptionModel> MapAdditionalNotesOptionsData(PatientMedicationModel medication)
        {
            return MapResourcesIntoOptionsByKeyID(GroupConstants.RS_MEDICATION_NOTES_GROUP, string.Empty, false
                , GetSelectedIDs(medication?.AdditionalNotes));
        }

        private long[] GetSelectedIDs(string ids)
        {
            long[] selectedIDs = new long[] { -1 };
            if (!string.IsNullOrWhiteSpace(ids))
            {
                selectedIDs = ids?.Split(Constants.SYMBOL_PIPE_SEPERATOR)?.Select(x => Convert.ToInt64(x))?.ToArray();
            }
            if (selectedIDs == null || selectedIDs.Count() < 1)
            {
                selectedIDs = new long[] { -1 };
            }
            return selectedIDs;
        }

        /// <summary>
        /// Save medication data
        /// </summary>
        /// <param name="medicationData">Reference object holding medication data</param>
        /// <returns>Operation status</returns>
        public async Task SaveMedicationAsync(PatientMedicationDTO medicationData)
        {
            try
            {
                medicationData.Medication.IsSynced = false;
                medicationData.Medication.AddedOn = GenericMethods.GetUtcDateTime;
                //Iupdating data patient medication or provider added medication
                if (medicationData.Medication.PatientMedicationID != Guid.Empty)
                {
                    medicationData.Medication.AddedByID = GetLoginUserID().ToString();
                    medicationData.Medication.LastModifiedByID = GetLoginUserID();
                }
                if (medicationData.Medication.ProgramID < 1 && medicationData.Medication.PatientMedicationID == Guid.Empty)
                {
                    medicationData.Medication.PatientID = GetUserID();
                    medicationData.Medication.PatientMedicationID = GenericMethods.GenerateGuid();
                    medicationData.Medication.IsActive = true;
                    medicationData.Medication.AddedByID = GetLoginUserID().ToString();
                    medicationData.Medication.LastModifiedByID = GetLoginUserID();
                }
                medicationData.Medications = new List<PatientMedicationModel> { medicationData.Medication };
                if (GenericMethods.IsListNotEmpty(medicationData.Reminders))
                {
                    foreach (var reminder in medicationData.Reminders)
                    {
                        reminder.PatientMedicationID = medicationData.Medication.PatientMedicationID;
                    }
                }
                if (MobileConstants.IsMobilePlatform)
                {
                    await SaveMedicationsAsync(medicationData).ConfigureAwait(false);
                }
                else
                {
                    await SyncMedicationsToServerAsync(medicationData, CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                medicationData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
        }

        #endregion

        #region Private Methods

        private async Task GetResourcesAndSettingsAsync()
        {
            if (!GenericMethods.IsListNotEmpty(PageData.Resources)
                || !GenericMethods.IsListNotEmpty(PageData.Settings))
            {
                await Task.WhenAll(
                   GetSettingsAsync(GroupConstants.RS_MEDICATION_GROUP, GroupConstants.RS_COMMON_GROUP),
                   GetResourcesAsync(GroupConstants.RS_MEDICATION_GROUP, GroupConstants.RS_MEDICATION_FREQUENCY_GROUP)
                ).ConfigureAwait(false);
            }
        }

        private void GetMedicationsUIData(PatientMedicationDTO medicationData)
        {
            if (!MobileConstants.IsMobilePlatform)
            {
                SetResourcesAndSettings(medicationData);
            }
            if (medicationData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(medicationData.Medications))
            {
                LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                foreach (var medication in medicationData.Medications)
                {
                    medication.FullName = string.IsNullOrWhiteSpace(medication.FullName) ? medication.ShortName : medication.FullName;
                    medication.LeftDefaultIcon = MobileConstants.IsMobilePlatform ? ImageConstants.I_MEDICATION_LIST_ICON_SVG : ImageConstants.I_MEDICATION_LIST_WEB_ICON_SVG;
                    medication.LeftSourceIcon = ImageConstants.I_MEDICATION_MOBILE_ICON_SVG;
                    medication.FromDateString = GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(medication.StartDate.Value), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                    medication.FormattedDate = medication.EndDateString = GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(medication.EndDate.Value), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                    medication.EndDateShortFormat =$"End date - {Convert.ToDateTime(medication.EndDateString).ToString("dd MMM yyyy")}";
                    medication.MedicationReminderImage = medication.Reminder ? GetMedicationReminderHtmlString() : string.Empty;
                    //todo: needs to verify
                    if (medicationData.RecordCount > -1)
                    {
                        if (MobileConstants.IsMobilePlatform)
                        {
                            if (medication.IsCritical && !medication.Reminder && medication.LastModifiedByID.ToString() != medicationData.AddedBy)
                            {
                                medication.Reminder = medication.IsCritical;
                            }
                        }

                        medication.MedicationDosesString = $"{medication.Doses} {medication.ShortUnitName}";
                        string key;
                        if (_essentials.ConvertToLocalTime(medication.EndDate.Value).Date < (_essentials.ConvertToLocalTime(DateTime.Now).Date))
                        {
                            key = ResourceConstants.R_HISTORY_TASK_KEY;
                            medication.MedicationStatusColorString = StyleConstants.ERROR_COLOR;
                        }
                        else
                        {
                            key = ResourceConstants.R_OPEN_TASK_KEY;
                            medication.MedicationStatusColorString = StyleConstants.SUCCESS_COLOR;
                        }

                        if (MobileConstants.IsMobilePlatform)
                        {
                            medication.IsCriticalStatusString = medication.IsCritical ? "    " + LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_CRITICAL_STATUS_KEY) : "";
                            medication.IsCriticalStatusColorString = medication.IsCritical ? StyleConstants.ERROR_COLOR : "";
                            medication.MedicationStatusString = LibResources.GetResourceValueByKey(PageData?.Resources, key);
                        }
                        else
                        {
                            medication.MedicationStatusString = LibResources.GetResourceValueByKey(PageData?.Resources, key);
                            medication.MedicationStatusColorString = LibResources.GetResourceValueByKey(PageData?.Resources, key) == LibResources.GetResourceValueByKey(PageData.Resources, ResourceConstants.R_HISTORY_TASK_KEY)
                                ? FieldTypes.DangerBadgeControl.ToString() : FieldTypes.SuccessBadgeControl.ToString();
                        }
                    }

                    if (medicationData.IsPrescriptionView)
                    {
                        medication.ShortName = $"{medication.UnitIdentifier} {Constants.DASH_INDICATOR} {medication.ShortName} {Constants.PIPE_SEPERATOR_WITH_SPACE} " +
                            $"{LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DOSES_KEY)} {Constants.COLON_KEY} {medication.Doses} {medication.ShortUnitName}";
                        medication.FormattedDate = string.Format(CultureInfo.InvariantCulture
                            , LibResources.GetResourceValueByKey(PageData.Resources, ResourceConstants.R_MEDICATION_DURATION_KEY)
                            , medication.FromDateString
                            , medication.EndDateString
                            , Math.Ceiling(((TimeSpan)(medication.EndDate - medication.StartDate)).TotalDays));
                        medication.FrequencyOptions = MapFrequencyOptionsData(medication)?.Where(x => x.IsSelected)?.ToList();
                        medication.HowOftenString = medication.FrequencyOptions?.FirstOrDefault()?.OptionText;
                        medication.FrequencyTypeOptions = MapFrequencyTypeOptionsData(medication)?.Where(x => x.IsSelected)?.ToList();
                        medication.AdditionalNotesOptions = MapAdditionalNotesOptionsData(medication)?.Where(x => x.IsSelected)?.ToList();
                        medication.AdditionalNotesOptionString = medication.AdditionalNotesOptions.Select(x => x.OptionText).ToList();
                        medication.AdditionalNotesOptionSelect = MapAdditionalNotesOptionsSelect(medication);

                        if (MobileConstants.IsMobilePlatform)
                        {
                            medication.FrequencyOptionsString = medication.FrequencyTypeOptions?.Select(x => x.OptionText).ToList();
                            medication.steps = medication.FrequencyOptionsString.Count();
                            medicationData.LastModifiedBy = GetInitials(medicationData.Organisation?.OrganisationName);
                            medicationData.AddedBy = LibSettings.GetSettingValueByKey(PageData?.Settings, SettingsConstants.S_LOGO_KEY);
                            if (medication.Dob.HasValue)
                            {
                                medication.Dob = _essentials.ConvertToLocalTime(medication.Dob.Value);
                                medicationData.Medication.Age = DateTime.Now.Subtract(medication.Dob.Value.Date).Days / 365;
                            }
                            else
                            {
                                medicationData.Medication.Age = 0;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(medication.HowOftenString))
                            {
                                medication.Frequency = "<div class='row margin-clearfix ltr'>" +
                                        "<label class='lbl-primary-text-body-large-semi-bold text-start truncate' style='max-width: 500px; text-decoration-line:none;'>" +
                                            $"{medication.HowOftenString}" +
                                        "</label>" +
                                    "</div>";
                            }

                            if (GenericMethods.IsListNotEmpty(medication.FrequencyTypeOptions))
                            {
                                medication.Frequency += "<div class='row margin-clearfix ltr' >";
                                for (var i = 0; i < medication.FrequencyTypeOptions.Count; i++)
                                {
                                    var f = medication.FrequencyTypeOptions[i];
                                    var id = $"{ResourceConstants.R_FREQUENCY_KEY}-{medication.PatientMedicationID}-{f.OptionID}radio";

                                    var seperatorstyle = i != medication.FrequencyTypeOptions.Count - 1
                                        ? "border-top: 1px solid var(--separator-n-disabled-color);top: 3px;"
                                        : "top: 4px;";

                                    medication.Frequency += "<div class='col px-2 px-2 pr-2 text-start'>" +
                                        "<div style='position: inherit; z-index: 1004;'>" +
                                            $"<input type='radio' id='{id}' style='z-index: 1005;' value='1' checked disabled />" +
                                            "<label class='lbl-primary-text-body-large-regular'/>" +
                                        "</div>" +
                                        $"<div class='timeline-horizontal-line' style='position: inherit;{seperatorstyle}left: 13px !important;margin-bottom: 20px !important;display: block;width: 100%;height: 0px !important;'></div>" +
                                        $"<label for='{id}' class='lbl-primary-text-body-large-regular margin-top-md' style='z-index: 1001;top: -20px;position: relative;'>" +
                                        $"{f.OptionText}" +
                                        "</label>" +
                                    "</div>";
                                }
                                medication.Frequency += "</div>";
                            }

                            if (GenericMethods.IsListNotEmpty(medication.AdditionalNotesOptions))
                            {
                                medication.Frequency += "<div class='row padding-horizontal-s ltr' >" +
                                    string.Join("",
                                        from n in medication.AdditionalNotesOptions
                                        let id = $"{ResourceConstants.R_NOTE_TEXT_KEY}-{medication.PatientMedicationID}-{n.OptionID}checkbox"
                                        select (
                                            "<div class='col px-2 margin-xs text-start'>" +
                                                $"<input type='checkbox' id='{id}' class=' ltr' checked disabled />" +
                                                $"<label for='{id}'class='lbl-primary-text-body-large-regular ltr'>{n.OptionText}</label>" +
                                            "</div>"
                                        )
                                    ) +
                                "</div>";
                            }
                            if (!string.IsNullOrWhiteSpace(medication.Note))
                            {
                                medication.Notes = "<div class='row margin-clearfix ltr'>" +
                                    "<label style='width: -webkit - fill - available; font - size:13px; font - weight:300'>" +
                                        $"{medication.Note}" +
                                    "</label>" +
                                "</div>";
                            }
                        }
                    }
                }

                if (medicationData.IsPrescriptionView && MobileConstants.IsMobilePlatform)
                {
                    medicationData.Medication.OrganisationDetail = GetOrganisationDetails(medicationData.Medication?.OrganisationAddress, medicationData.Medication?.OrganisationContact);
                    var medicationWithEarliestStartDate = medicationData.Medications.OrderBy(x => x.AddedOn).FirstOrDefault();
                    if (medicationData.Caregiver != null)
                    {
                        MapUserDegreesOptionsData(medicationData);
                        medicationData.Caregiver.FirstName = string.Format(CultureInfo.InvariantCulture
                                                    , LibResources.GetResourceValueByKey(PageData.Resources, ResourceConstants.R_COMMON_BY_Key)
                                                    , medicationData.Caregiver.FirstName);
                        string timestr = medicationWithEarliestStartDate.AddedOn.ToLocalTime().TimeOfDay.ToString();
                        DateTime parsedDateTime = DateTime.ParseExact(timestr, "HH:mm:ss.fffffff", null);
                        string convertedTime12Hour = parsedDateTime.ToString("h:mm tt");
                        medicationData.Caregiver.DateStyle = $"{GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(medicationWithEarliestStartDate.AddedOn.Date), DateTimeType.Date, dayFormat, monthFormat, yearFormat)} " +
                            $"{convertedTime12Hour}";
                        medicationData.Medication.DoctorNameDisplayString = $"{medicationData.Caregiver.Department}" +
                        $"{(!string.IsNullOrEmpty(medicationData.Caregiver.Degrees) ? $"{Constants.PIPE_SEPERATOR_WITH_SPACE} {medicationData.Caregiver.Degrees}" : "")}";
                    }
                    if (medicationData.Medication != null)
                    {
                        medicationData.Medication.PatientName = $"{medicationData.Medication.FirstName}" +
                            $" {medicationData.Medication.LastName}" +
                            $" {Constants.PIPE_SEPERATOR_WITH_SPACE}" +
                            $" {medicationData.Medication.Age}" +
                            $" {Constants.COMMA_SEPARATOR}" +
                            $" {LibResources.GetResourceValueByKey(PageData?.Resources, medicationData.Medication.Gender)} ";
                    }
                }

                var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
                if (roleID != (int)RoleName.Patient && roleID != (int)RoleName.CareTaker)
                {
                    //SortPatientMedications(medicationData);
                    GenericMethods.SortByDate(medicationData.Medications, x => x.StartDate, x => x.EndDate);
                }
            }
            if (medicationData.RecordCount == -1)
            {
                medicationData.FrequencyOptions = MapFrequencyOptionsData(medicationData.Medication);
                medicationData.FrequencyTypeOptions = MapFrequencyTypeOptionsData(medicationData.Medication);
                medicationData.AdditionalNotesOptions = MapAdditionalNotesOptionsData(medicationData.Medication);
            }
            else if (MobileConstants.IsMobilePlatform && medicationData.RecordCount == -2)
            {
                medicationData.Medications = medicationData.Medications?.Where(x => x.StartDate.Value.Date <= GenericMethods.GetUtcDateTime.Date).ToList();
                medicationData.Medications = medicationData.Medications?.OrderByDescending(x => x.AddedOn).ThenBy(c => c.AddedOn).ToList();
            }
        }

        private List<OptionSelectModel> MapAdditionalNotesOptionsSelect(PatientMedicationModel medicationData)
        {
            return medicationData.AdditionalNotesOptions.Select(filter => new OptionSelectModel
            {
                OptionID = filter.OptionID,
                Value = filter.OptionID.ToString(),
                DisplayText = filter.OptionText,
                IsSelected = filter.IsSelected
            }).ToList();
        }

        public string GetMedicationReminderHtmlString()
        {
            return $@"<img src=""{ImageConstants.I_MOBILE_MEDICATION_REMINDER_ICON_SVG}"" width=""20"">";
        }

        private void SortPatientMedications(PatientMedicationDTO medicationData)
        {
            if (GenericMethods.IsListNotEmpty(medicationData.Medications))
            {
                List<PatientMedicationModel> openMedications = new List<PatientMedicationModel>();
                List<PatientMedicationModel> historyMedications = new List<PatientMedicationModel>();
                foreach (var categoryGroup in medicationData.Medications.GroupBy(x => x.MedicationStatusString).ToList())
                {
                    if (GenericMethods.IsListNotEmpty(categoryGroup.ToList()) && categoryGroup.Key == LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_OPEN_TASK_KEY))
                    {
                        openMedications.AddRange(categoryGroup.ToList());
                    }
                    else
                    {
                        historyMedications.AddRange(categoryGroup.ToList());
                    }
                }
                var open = openMedications.OrderBy(x => x.EndDate).ToList();
                var history = historyMedications.OrderByDescending(x => x.EndDate).ToList();
                medicationData.Medications = new List<PatientMedicationModel>();
                medicationData.Medications.AddRange(open);
                medicationData.Medications.AddRange(history);
            }
        }

        private async Task SaveMedicationsAsync(PatientMedicationDTO medicationData)
        {
            var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
            var isPatient = roleID == (int)RoleName.Patient;// || roleID == (int)RoleName.CareTaker;
            if (isPatient)
            {
                await GetResourcesAndSettingsAsync().ConfigureAwait(false);
                GenerateNotificationData(medicationData);
            }
            await new MedicationDatabase().SavePatientMedicationDataAsync(medicationData).ConfigureAwait(false);
            if (isPatient && medicationData.ErrCode == ErrorCode.OK)
            {
                await RegisterNotificationAsync().ConfigureAwait(false);
            }
        }

        #endregion
    }
}