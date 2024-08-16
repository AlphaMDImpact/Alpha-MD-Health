using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class MedicalHistoryService : BaseService
{
    public MedicalHistoryService(IEssentials serviceEssentials) : base(serviceEssentials)
    {

    }

    /// <summary>
    /// Get Medical History
    /// </summary>
    /// <param name="medicalHistoryData">Reference object contains data</param>
    /// <param name="getAllData">Is All data Required or particular date data required</param>
    /// <param name="getAllReadings">Is All reading data Required</param>
    /// <returns></returns>
    public async Task SyncMedicalHistoryAsync(MedicalHistoryDTO medicalHistoryData, bool getAllData = false, bool getAllReadings = false)
    {
        try
        {
            medicalHistoryData.SelectedUserID = GetUserID();
            medicalHistoryData.DateTimeDifference = CalculateUtcAndLocalTimeDifferenceInSeconds();
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                PathWithoutBasePath = UrlConstants.GET_MEDICAL_HISTORY_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(medicalHistoryData.SelectedUserID, CultureInfo.InvariantCulture) },
                    { nameof(BaseDTO.FromDate), medicalHistoryData.FromDate },
                    { nameof(BaseDTO.ToDate), medicalHistoryData.ToDate },
                    { nameof(MedicalHistoryDTO.DateTimeDifference), Convert.ToString(medicalHistoryData.DateTimeDifference, CultureInfo.InvariantCulture) },
                    { nameof(MedicalHistoryViewModel.ShowAllData),Convert.ToString(getAllReadings)},
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            medicalHistoryData.ErrCode = httpData.ErrCode;
            if (medicalHistoryData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(medicalHistoryData, data);
                    SetResourcesAndSettings(medicalHistoryData);
                    medicalHistoryData.OrganisationContact = GetDataItem<string>(data, nameof(MedicalHistoryDTO.OrganisationContact));
                    medicalHistoryData.OrganisationAddress = GetDataItem<string>(data, nameof(MedicalHistoryDTO.OrganisationAddress));
                    if (getAllData)
                    {
                        await MapMedicalHistoryViews(data, nameof(MedicalHistoryDTO.AllMedicalHistoryViews), medicalHistoryData);
                        medicalHistoryData.MedicalHistory = new List<MedicalHistoryModel>
                        {
                            new MedicalHistoryModel { IsLoaded = true, MedicalHistoryViews = medicalHistoryData.AllMedicalHistoryViews?.Where(x => !x.ShowAllData)?.ToList() }
                        };
                    }
                    else
                    {
                        MapHistoryFromDateAndToDate(medicalHistoryData, data);
                        medicalHistoryData.AddHistoryFor = GetPickerSource(data, nameof(MedicalHistoryDTO.AddHistoryFor), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), -1, false, null);
                        await MapMedicalHistoryViews(data, nameof(MedicalHistoryDTO.AllMedicalHistoryViews), medicalHistoryData);
                        if (GenericMethods.IsListNotEmpty(medicalHistoryData.MedicalHistory))
                        {
                            var currenthistory = medicalHistoryData.MedicalHistory.FirstOrDefault(x => x.HistoryFromDate == medicalHistoryData.FromDate);
                            if (currenthistory == null)
                            {
                                currenthistory = medicalHistoryData.MedicalHistory.LastOrDefault();
                            }
                            currenthistory.MedicalHistoryViews = medicalHistoryData.AllMedicalHistoryViews?.Where(x => !x.ShowAllData)?.ToList();
                            currenthistory.IsLoaded = true;
                            medicalHistoryData.AllMedicalHistoryViews = medicalHistoryData.AllMedicalHistoryViews?.Where(x => x.ShowAllData)?.ToList();
                        }
                        if (GenericMethods.IsListNotEmpty(medicalHistoryData.AddHistoryFor))
                        {
                            medicalHistoryData.AddHistoryFor.ForEach(x => x.OptionText = x.OptionText?.Replace("{0}", string.Empty)?.Trim());
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            medicalHistoryData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void MapHistoryFromDateAndToDate(MedicalHistoryDTO medicalHistoryData, JToken data)
    {
        if (medicalHistoryData.FromDate == null)
        {
            medicalHistoryData.MedicalHistory = MapMedicalHistories(data, nameof(MedicalHistoryDTO.MedicalHistory));
            if (GenericMethods.IsListNotEmpty(medicalHistoryData.MedicalHistory))
            {
                var firstHistory = medicalHistoryData.MedicalHistory.FirstOrDefault();
                medicalHistoryData.FromDate = firstHistory?.HistoryFromDate;
                medicalHistoryData.ToDate = firstHistory?.HistoryToDate;
            }
        }
    }

    private List<MedicalHistoryModel> MapMedicalHistories(JToken data, string collectionName)
    {
        return data[collectionName]?.Count() > 0
            ? (from dataItem in data[collectionName]
               select MapMedicalHistory(dataItem)).ToList()
            : null;
    }

    private MedicalHistoryModel MapMedicalHistory(JToken dataItem)
    {
        return new MedicalHistoryModel
        {
            SequenceNo = GetDataItem<short>(dataItem, nameof(MedicalHistoryModel.SequenceNo)),
            HistoryDate = GetDataItem<DateTimeOffset>(dataItem, nameof(MedicalHistoryModel.HistoryDate)),
            HistoryFromDate = GetDataItem<string>(dataItem, nameof(MedicalHistoryModel.HistoryFromDate)),
            HistoryToDate = GetDataItem<string>(dataItem, nameof(MedicalHistoryModel.HistoryToDate))
        };
    }

    private async Task MapMedicalHistoryViews(JToken data, string collectionName, MedicalHistoryDTO medicalHistoryData)
    {
        if (data[collectionName]?.Count() > 0)
        {
            medicalHistoryData.AllMedicalHistoryViews = new List<MedicalHistoryViewModel>();
            await Task.WhenAll(from dataItem in data[collectionName]
                               select MapMedicalHistoryView(dataItem, medicalHistoryData)
            ).ConfigureAwait(false);
            medicalHistoryData.AllMedicalHistoryViews?.OrderBy(x => x.SequenceNo);
        }
        else
        {
            medicalHistoryData.AllMedicalHistoryViews = null;
        }
    }

    private async Task MapMedicalHistoryView(JToken dataItem, MedicalHistoryDTO medicalHistoryData)
    {
        var historyView = new MedicalHistoryViewModel
        {
            FeatureCode = GetDataItem<AppPermissions>(dataItem, nameof(MedicalHistoryViewModel.FeatureCode)),
            ErrorCode = GetDataItem<ErrorCode>(dataItem, nameof(MedicalHistoryViewModel.ErrorCode)),
            SequenceNo = GetDataItem<short>(dataItem, nameof(MedicalHistoryViewModel.SequenceNo)),
            ShowAllData = GetDataItem<bool>(dataItem, nameof(MedicalHistoryViewModel.ShowAllData)),
        };
        var viewJsonData = dataItem[nameof(MedicalHistoryViewModel.PageData)];
        if (viewJsonData.HasValues)
        {
            historyView.PageData = await MapViewsPageDataAsync(medicalHistoryData, historyView, viewJsonData.ToString());
        }
        medicalHistoryData.AllMedicalHistoryViews.Add(historyView);
    }

    private async Task<object> MapViewsPageDataAsync(MedicalHistoryDTO medicalHistoryData, MedicalHistoryViewModel historyView, string viewJsonData)
    {
        switch (historyView.FeatureCode)
        {
            case AppPermissions.PatientReadingsView:
                return await new ReadingService(_essentials).MapPatientReadingHistoryDataAsync(medicalHistoryData, historyView, viewJsonData);
            case AppPermissions.PatientProviderNotesView:
                return new QuestionnaireService(_essentials).MapPatientProviderNotesHistoryData(medicalHistoryData, historyView, viewJsonData);
            case AppPermissions.PatientTasksView:
                return await new PatientTaskService(_essentials).MapPatientTasksHistoryData(medicalHistoryData, historyView, viewJsonData);
            case AppPermissions.PatientFilesView:
                return new FileService(_essentials).MapFilesHistoryData(medicalHistoryData, historyView, viewJsonData);
            case AppPermissions.PatientMedicationsView:
            case AppPermissions.PrescriptionView:
                return new MedicationSevice(_essentials).MapMedicationsHistoryData(medicalHistoryData, historyView, viewJsonData);
            case AppPermissions.PatientEducationsView:
                return await new ContentPageService(_essentials).MapContentPagesHistoryData(medicalHistoryData, historyView, viewJsonData);
            case AppPermissions.PatientTrackersView:
                return new PatientTrackerService(_essentials).MapPatientTrackersHistoryData(medicalHistoryData, historyView, viewJsonData);
        }
        return null;
    }

    /// <summary>
    /// Get Data For Printing Medical History
    /// </summary>
    /// <param name="medicalHistories">Historical Data</param>
    /// <param name="OrganisationLogo">Organisation Logo</param>
    /// <param name="OrganisationName">Organisation Name</param>
    /// <param name="PatientDetails">Patient Details</param>
    /// <returns>HTML Data, ErrorCode</returns>
    public void GetPrintData(MedicalHistoryDTO medicalHistories, List<string> OrgDetails, List<string> PatientDetails, Dictionary<AppPermissions, string> keyValuePairs, bool isAllData)
    {
        try
        {
            string filename = PatientDetails[1];
            CreateMedicalHistoryReport(filename, OrgDetails, PatientDetails, keyValuePairs, medicalHistories, isAllData);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            medicalHistories.HtmlString = string.Empty;
            medicalHistories.ErrCode = ErrorCode.InvalidData;
        }
    }

    /// <summary>
    /// Create HTML Medical History Report
    /// </summary>
    /// <param name="FileName">Name of File</param>
    /// <param name="OrgDetail">Organisation Details</param>
    /// <param name="PatientDetails">Patients Details</param>
    /// <param name="keyValuePairs">Feature Details</param>
    /// <param name="medicalHistories">Medical History Data </param>
    /// <returns>Html string</returns>
    private void CreateMedicalHistoryReport(string FileName, List<string> OrgDetail, List<string> PatientDetails, Dictionary<AppPermissions, string> keyValuePairs, MedicalHistoryDTO medicalHistories, bool isAllData)
    {
        LibSettings.TryGetDateFormatSettings(medicalHistories.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        StringBuilder html = new StringBuilder();
        html.Append("<!DOCTYPE html><html><head><title>" + FileName + @"</title>");
        html.Append("<base href='" + UrlConstants.SERVICE_ORGANISATION_DOMAIN + @"'>
			<style>
            .col {
                flex-basis: 0;
                flex-grow: 1;
                max-width: 100%;
            }

            .margin-vertical-xs {
                margin-top: 10px !important;
                margin-bottom: 10px !important
            }
            .px-0 {
                padding-right: 0 !important;
            }
            .d-flex {
                display: flex !important;
            }
            .justify-content-center {
                justify-content: center !important;
            }

            .ltr .reading-row-cell {
                align-items: center;
                height: 80px;
                margin-left: -15px;
                margin-right: -15px;
                padding-left: 20px;
                padding-right: 15px;
            }
            .ltr .childdisplay img:not(:first-child) {
                margin-left: 5px !important
            }
            .row {
                display: flex;
                flex-wrap: wrap;
                margin-right: -15px;
                margin-left: -15px;
            }
            .pr-0,
            .px-0 {
                padding-right: 0 !important;
            }
            .pl-0,
            .px-0 {
                padding-left: 0 !important;
            }
            .mr-0,
            .mx-0 {
                margin-right: 0 !important;
            }
            .ml-0,
            .mx-0 {
                margin-left: 0 !important;
            }
            .avatar-circle {
                text-align: center;
                -moz-border-radius: 12px;
                object-fit: contain;
                fill: #ff0000;
            }
            .margin-vertical-xs {
                margin-top: 10px !important;
                margin-bottom: 10px !important
            }
            .w-available {
                width: 100%;
                width: 100%;
                width: -moz-available; /* WebKit-based browsers will ignore this. */
                width: -webkit-fill-available; /* Mozilla-based browsers will ignore this. */
                width: fill-available;
            }
            .badge-number {
                display: inline-block;
                text-align: center !important;
                border-radius: 11px !important;
                width: 20px !important;
                height: 20px !important;
                background-color: #F6861F;
                font-size: 12px;
                padding-top: 2px;
            }
            .margin-clearfix {
                margin: 0px !important;
            }
            .text-start {
                width: 100%;
                width: -moz-available; /* WebKit-based browsers will ignore this. */
                width: -webkit-fill-available; /* Mozilla-based browsers will ignore this. */
                width: fill-available;
                text-align: start;
            }
            .truncate {
                max-width: 50%;
                overflow: hidden !important;
                text-overflow: ellipsis !important;
            }
            .text-start {
                width: 100%;
                width: -moz-available; /* WebKit-based browsers will ignore this. */
                width: -webkit-fill-available; /* Mozilla-based browsers will ignore this. */
                width: fill-available;
                text-align: start;
            }
            .margin-top-md {
                margin-top: 20px !important;
            }
            .margin-xs {
                margin: 10px !important;
            }
            .padding-horizontal-sm {
                padding-left: 15px !important;
                padding-right: 15px !important;
            }
            hr.dotted { border-top: 3px dotted #bbb;}
			</style>
			</head>");
        html.Append("<body style='background-color:white !important'>");
        html.Append("<p><br /></p>");

        //Organisation Details
        CreateOrganisationSection(html, OrgDetail);
        html.Append("<hr style='width:100%;text-align:left;margin-left:0'/>");
        //PatientSection
        CreatePatientSection(html, PatientDetails);
        html.Append("<hr style='width:100%;text-align:left;margin-left:0'/>");
        html.Append("<br />");
        html.Append($"<span style='font-weight: bold;'>{PatientDetails[13]} {PatientDetails[14]}</span>");
        html.Append("<br />");
        html.Append("<br />");
        html.Append("<hr style='width:100%;text-align:left;margin-left:0'/>");
        foreach (var medicalHistory in medicalHistories.AllMedicalHistoryViews)
        {
            if (medicalHistory.FeatureCode == AppPermissions.PatientReadingsView && medicalHistory.PageData != null)
            {
                var view = (PatientReadingDTO)medicalHistory.PageData;
                if (view != null && GenericMethods.IsListNotEmpty(view.ReadingDTOs))
                {
                    html.Append("<div style='border: 1px solid;padding-left: 5px; padding-right: 5px'>");
                    html.Append($"<h2 style='padding-left: 10px'>{keyValuePairs[medicalHistory.FeatureCode]}</h2>");
                    var taskHeaders = new List<TableDataStructureModel>
                    {
                        new TableDataStructureModel{DataField=nameof(PatientReadingDTO.LatestValueString)},
                        new TableDataStructureModel{DataField=nameof(PatientReadingDTO.LatestValueDateText)}
                    };
                    foreach (var option in view.FilterOptions)
                    {
                        var optionData = view.ReadingDTOs.Where(x => x.ReadingCategoryID == option.OptionID);
                        if (optionData.Any())
                        {
                            html.Append(CreateTable<PatientReadingDTO>(optionData.ToList(), option.OptionText, false, taskHeaders, medicalHistories.Resources));
                            html.Append("<br />");
                        }
                    }
                    html.Append("</div>");
                }
            }
        }
        foreach (var medicalHistory in medicalHistories.MedicalHistory)
        {
            if (medicalHistory.MedicalHistoryViews != null && medicalHistory.MedicalHistoryViews.Count > 0)
            {
                html.Append("<div style='border: 1px solid;padding-left: 5px; padding-right: 5px'>");
                if (!isAllData)
                {
                    html.Append($"<h2 style='padding-left: 10px'>{GenericMethods.GetLocalDateTimeBasedOnCulture(medicalHistory.HistoryDate, DateTimeType.Date, dayFormat, monthFormat, yearFormat)}</h2>");
                }
                html.Append("<br />");

                foreach (var item in medicalHistory.MedicalHistoryViews.OrderBy(X => X.SequenceNo))
                {
                    if (item.FeatureCode == AppPermissions.PatientProviderNotesView)
                    {
                        var view = (PatientProviderNoteDTO)item.PageData;
                        if (view != null && GenericMethods.IsListNotEmpty(view.PatientProviderNotes))
                        {
                            List<ListModel> notesModel = new List<ListModel>();
                            view.Providers.ForEach(prov =>
                            {
                                notesModel.Add(new ListModel
                                {
                                    ImageName = string.Empty,
                                    MiddleSection = new List<string>() { prov.OptionText },
                                    EndSection = string.Empty
                                });
                            });

                            html.Append(CreateList(notesModel, keyValuePairs[item.FeatureCode], false, false));
                            html.Append("<br />");
                        }
                    }
                    else if (item.FeatureCode == AppPermissions.PrescriptionView)
                    {
                        var view = (PatientMedicationDTO)item.PageData;
                        if (view != null && GenericMethods.IsListNotEmpty(view.Medications))
                        {
                            List<ListModel> prescriptionModel = new List<ListModel>();
                            view.Medications.ForEach(medication =>
                            {
                                prescriptionModel.Add(new ListModel
                                {
                                    ImageName = string.Empty,
                                    MiddleSection = CreatePrescriptionData(medication),
                                    EndSection = string.Empty
                                });
                            });

                            html.Append(CreateList(prescriptionModel, keyValuePairs[item.FeatureCode], false, false));
                            html.Append("<br />");
                        }
                    }
                    else if (item.FeatureCode == AppPermissions.PatientTasksView)
                    {
                        var view = (ProgramDTO)item.PageData;
                        if (view != null && GenericMethods.IsListNotEmpty(view.Tasks))
                        {
                            List<TaskModel> taskData = view.Tasks;
                            var taskHeaders = new List<TableDataStructureModel>
                            {
                                new TableDataStructureModel{DataField=nameof(TaskModel.SelectedItemName),DataHeader=ResourceConstants.R_PROGRAM_NAME_KEY},
                                new TableDataStructureModel{DataField=nameof(TaskModel.TaskType),DataHeader=ResourceConstants.R_TASK_TYPE_KEY},
                                new TableDataStructureModel{DataField=nameof(TaskModel.Name),DataHeader=ResourceConstants.R_PROGRAM_TITLE_KEY},
                                new TableDataStructureModel{DataField=nameof(TaskModel.FromDateValue),DataHeader=ResourceConstants.R_START_DATE_KEY},
                                new TableDataStructureModel{DataField=nameof(TaskModel.ToDateValue),DataHeader=ResourceConstants.R_END_DATE_KEY},
                                new TableDataStructureModel{DataField=nameof(TaskModel.StatusValue),DataHeader=ResourceConstants.R_STATUS_KEY,}
                            };
                            html.Append(CreateTable<TaskModel>(taskData, keyValuePairs[item.FeatureCode], true, taskHeaders, medicalHistories.Resources));
                            html.Append("<br />");
                        }

                    }
                    else if (item.FeatureCode == AppPermissions.PatientFilesView)
                    {
                        var view = (FileDTO)item.PageData;
                        if (view != null && GenericMethods.IsListNotEmpty(view.Files))
                        {
                            List<ListModel> filesModel = new List<ListModel>();
                            view.Files.ForEach(file =>
                            {
                                filesModel.Add(new ListModel
                                {
                                    ImageName = file.FileImage,
                                    MiddleSection = new List<string>() { file.FileName, file.FormattedDate },
                                    EndSection = file.FormattedNumberOfFiles + (file.IsUnreadHeader ? "<div><label class='badge-number'>" + file.NumberOfFiles + "</label></div>" : "")
                                });
                            });

                            html.Append(CreateList(filesModel, keyValuePairs[item.FeatureCode], true, true));
                            html.Append("<br />");
                        }
                    }
                    else if (item.FeatureCode == AppPermissions.PatientEducationsView)
                    {
                        var view = (ContentPageDTO)item.PageData;
                        if (view != null && GenericMethods.IsListNotEmpty(view.PatientEducations))
                        {
                            List<PatientEducationModel> educationData = view.PatientEducations;
                            var educationHeaders = new List<TableDataStructureModel>
                            {
                                new TableDataStructureModel{DataField=nameof(PatientEducationModel.PageHeading),DataHeader=ResourceConstants.R_SELECT_EDUCATION_LABEL_KEY},
                                new TableDataStructureModel{DataField=nameof(PatientEducationModel.CategoryName),DataHeader=ResourceConstants.R_EDUCATION_CATEGORY_NAME_KEY},
                                new TableDataStructureModel{DataField=nameof(PatientEducationModel.ProgramName),DataHeader=ResourceConstants.R_PROGRAM_TITLE_KEY},
                                new TableDataStructureModel{DataField=nameof(PatientEducationModel.FromDateString),DataHeader=ResourceConstants.R_START_DATE_KEY},
                                new TableDataStructureModel{DataField=nameof(PatientEducationModel.ToDateString),DataHeader=ResourceConstants.R_END_DATE_KEY},
                                new TableDataStructureModel{DataField=nameof(PatientEducationModel.StatusValue),DataHeader=ResourceConstants.R_STATUS_KEY,}
                            };

                            html.Append(CreateTable<PatientEducationModel>(educationData, keyValuePairs[item.FeatureCode], true, educationHeaders, medicalHistories.Resources));
                            html.Append("<br />");
                        }
                    }
                    else if (item.FeatureCode == AppPermissions.PatientTrackersView)
                    {
                        var view = (TrackerDTO)item.PageData;
                        if (view != null && GenericMethods.IsListNotEmpty(view.PatientTrackers))
                        {
                            List<ListModel> trackersModel = new List<ListModel>();
                            view.PatientTrackers.ForEach(tracker =>
                            {
                                trackersModel.Add(new ListModel
                                {
                                    ImageName = tracker.ImageName != null
                                ? tracker.ImageName
                                : "../" + "../" + ImageConstants.I_DEFAULT_TRACKER_SVG,
                                    MiddleSection = new List<string>() { tracker.TrackerName, tracker.CurrentValueDisplayFormatString },
                                    EndSection = tracker.FromDateDisplayFormatString + "<br/>" + tracker.ToDateDisplayFormatString
                                });
                            });

                            html.Append(CreateList(trackersModel, keyValuePairs[item.FeatureCode], true, true));
                            html.Append("<br />");
                        }
                    }
                }
            }
            html.Append("</div>");
            html.Append("<br />");
            html.Append("<br />");
        }
        html.Append("</body></html>");
        medicalHistories.HtmlString = html.ToString();
        medicalHistories.ErrCode = ErrorCode.OK;
    }

    private List<string> CreatePrescriptionData(PatientMedicationModel medication)
    {
        StringBuilder frequency = new StringBuilder();
        string Notes = string.Empty;
        if (!string.IsNullOrWhiteSpace(medication.HowOftenString))
        {
            frequency.Append("<table style='width: 100%;'>");
            frequency.Append("<tbody>");
            frequency.Append("<tr>");

            frequency.Append("<td>" +
                    "<label class='lbl-primary-text-body-large-semi-bold text-start truncate' style='max-width: 500px; text-decoration-line:none;'>" +
                        $"{medication.HowOftenString}" +
                    "</label>" +
                "</td>");
            frequency.Append("</tr>");
            frequency.Append("</tbody>");
            frequency.Append("</table>");
        }

        if (GenericMethods.IsListNotEmpty(medication.FrequencyTypeOptions) || GenericMethods.IsListNotEmpty(medication.AdditionalNotesOptions))
        {
            frequency.Append("<table style='width: 100%;'>");
            frequency.Append("<tbody>");
            frequency.Append("<tr><td>");
            if (GenericMethods.IsListNotEmpty(medication.FrequencyTypeOptions))
            {
                frequency.Append("<div id={id} style='display:inline-block;width:10px;height: 10px;border-radius: 100%;background-color:#000'></div>");
                for (var i = 0; i < medication.FrequencyTypeOptions.Count; i++)
                {
                    var f = medication.FrequencyTypeOptions[i];
                    var id = $"{ResourceConstants.R_FREQUENCY_KEY}-{medication.PatientMedicationID}-{f.OptionID}radio";
                    if (i == 0)
                    {
                        frequency.Append("     ");
                    }
                    frequency.Append($"<label for='{id}'>" +
                                        $"{f.OptionText}" +
                                        "</label>");
                    if (i != (medication.FrequencyTypeOptions.Count - 1))
                    {
                        frequency.Append(" | ");
                    }
                }
                frequency.Append("<br/>");
            }
            if (GenericMethods.IsListNotEmpty(medication.AdditionalNotesOptions))
            {
                frequency.Append("<div id={id} style='display:inline-block;width:10px;height: 10px;border-radius: 100%;background-color:#000'></div>");
                for (var i = 0; i < medication.AdditionalNotesOptions.Count; i++)
                {
                    if (i == 0)
                    {
                        frequency.Append("     ");
                    }
                    var n = medication.AdditionalNotesOptions[i];
                    frequency.Append($"<label>" +
                                        $"{n.OptionText}" +
                                        "</label>");
                    if (i != (medication.AdditionalNotesOptions.Count - 1))
                    {
                        frequency.Append(" | ");
                    }
                }
            }
            frequency.Append("</td></tr>");
            frequency.Append("</tbody>");
            frequency.Append("</table>");
        }

        if (!string.IsNullOrWhiteSpace(medication.Note))
        {
            Notes = "<div class='row margin-clearfix ltr'>" +
                "<label style='width: -webkit-fill-available; font-size:13px; font- weight:300'>" +
                    $"{medication.Note}" +
                "</label>" +
            "</div>";
        }

        return new List<string>() { medication.ShortName, medication.FormattedDate, frequency.ToString() + " " + Notes };
    }

    /// <summary>
    /// Create HTML For Organisation Related Info
    /// </summary>
    /// <param name="html">Html string</param>
    /// <param name="OrgDetail">Organisation data</param>
    private void CreateOrganisationSection(StringBuilder html, List<string> OrgDetail)
    {
        if (OrgDetail != null && OrgDetail.Count == 4)
        {
            html.Append("<table style='width: 100%;'>");
            html.Append("<tbody>");
            html.Append("<tr>");
            if (!string.IsNullOrWhiteSpace(OrgDetail[0]))
            {
                html.Append(@"<td style='width: 10%;' rowspan='3'><div class='col margin-vertical-xs px-0'>
                    <div class='lbl-secondary-text-body-medium-regular d-flex justify-content-center  ltr'>
                        <div class='row px-0 mx-0 d-flex justify-content-center'>
                                <img src='" + OrgDetail[0] + @"' class='avatar-circle' style='height:60px; width:60px; border-radius:8px;'/>
                        </div>
                    </div>
                </div></td>");
            }
            else
            {
                string OrgInitial = OrgDetail[1].Substring(0, 1);
                html.Append(@"<td style='width: 10%;' rowspan='3'><div class='col margin-vertical-xs px-0'>
                <div class='lbl-secondary-text-body-medium-regular d-flex justify-content-center ltr'>
                    <div class='row px-0 mx-0 d-flex justify-content-center'>
                        <div class='avatar-circle d-flex' style='border: 1px solid;height:60px; width:60px; border-radius:8px;'>
                            <span class='w-available' style='padding-left: 10px;width: 100%;font-size: 50px;'>" + OrgInitial + @"</span>
                        </div>
                    </div>
                </div>
            </div></td>");
            }
            html.Append("<td style='font-weight: bold;width: 90%;'>" + OrgDetail[1] + "</td>");
            html.Append("</tr>");
            html.Append("<tr>");
            html.Append("<td style='width: 90%;'>" + OrgDetail[2] + "</td>");
            html.Append("</tr>");
            html.Append("<tr>");
            html.Append("<td style='width: 90%;'>" + OrgDetail[3] + "</td>");
            html.Append("</tr>");
            html.Append("</tbody>");
            html.Append("</table>");
        }
    }

    /// <summary>
    /// Create HTML For Patient Related Info
    /// </summary>
    /// <param name="html">Html string</param>
    /// <param name="PatientDetails">Patient data</param>
    private void CreatePatientSection(StringBuilder html, List<string> PatientDetails)
    {
        if (PatientDetails?.Count > 0)
        {
            html.Append("<table style='width: 100%;'>");
            html.Append("<tbody>");
            html.Append("<tr>");
            if (!string.IsNullOrWhiteSpace(PatientDetails[0]))
            {
                if (PatientDetails[0].Length < 5)
                {
                    html.Append(@"<td style='width: 10%;' rowspan='3'>
                                <div class='col margin-vertical-xs px-0'>
                                    <div class='lbl-secondary-text-body-medium-regular d-flex justify-content-center ltr'>
                                        <div class='row px-0 mx-0 d-flex justify-content-center'>
                                            <div style='border: 2px solid #ccc; border-radius: 10px;text-align: center; padding-top: 20px;width: 60px; height: 40px;'>
                                                " + PatientDetails[0] + @"
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </td>");
                }
                else
                {
                    html.Append(@"<td style='width: 10%;' rowspan='3'><div class='col margin-vertical-xs px-0'>
                                    <div class='lbl-secondary-text-body-medium-regular d-flex justify-content-center  ltr'>
                                        <div class='row px-0 mx-0 d-flex justify-content-center'>
                                                <img src='" + PatientDetails[0] + @"' class='avatar-circle' style='height:60px; width:60px; border-radius:8px;'/>
                                        </div>
                                    </div>
                                </div></td>");
                }
            }
            else
            {
                string[] initials = PatientDetails[1].Trim().Split(' ');
                string patInitial;
                if (initials.Length > 1)
                {
                    patInitial = initials[0].Substring(0, 1);
                    patInitial += initials[1].Substring(0, 1);
                }
                else
                {
                    patInitial = PatientDetails[1].Substring(0, 2);
                }
                html.Append(@"<td style='width: 10%;' rowspan='3'><div class='col margin-vertical-xs px-0'>
                <div class='lbl-secondary-text-body-medium-regular d-flex justify-content-center ltr'>
                    <div class='row px-0 mx-0 d-flex justify-content-center'>
                        <div class='avatar-circle d-flex' style='padding: 20px; border: 1.5px solid;border-radius: 8px'>
                            <span style='font-weight: bold'>" + patInitial + @"</span>
                        </div>
                    </div>
                </div>
            </div></td>");
            }
            html.Append($"<td style='font-weight: bold;width: 90%;'>{PatientDetails[1]} | {PatientDetails[2]}, {PatientDetails[3]}" + "</td>");
            html.Append("</tr>");
            html.Append("<tr>");
            html.Append($"<td style='width: 75.0000%;'>Email : <span style='font-weight: bold;'>{PatientDetails[4]}</span> Mobile : <span style='font-weight: bold;'>{PatientDetails[5]}</span></td>");
            html.Append("</tr>");
            html.Append("<tr>");
            html.Append("<td style='width: 35.0000%;'>");
            html.Append("<table style='width: inherit;'>");
            html.Append("<tr>");
            html.Append($"<td rowspan='2'><img src='{string.Concat("../" + ImageConstants.WEB_IMAGE_PATH, '/', ImageConstants.I_HEIGHT_ICON_SVG)}' /></td>");
            html.Append($"<td>{PatientDetails[10]}</td>");
            html.Append($"<td rowspan='2'><img src='{string.Concat("../" + ImageConstants.WEB_IMAGE_PATH, '/', ImageConstants.I_BLOOD_TYPE_SVG)}' /></td>");
            html.Append($"<td>{PatientDetails[12]}</td>");
            html.Append($"<td rowspan='2'><img src='{string.Concat("../" + ImageConstants.WEB_IMAGE_PATH, '/', ImageConstants.I_WEIGHT_ICON_SVG)}' /></td>");
            html.Append($"<td>{PatientDetails[11]}</td>");
            html.Append("</tr>");
            html.Append("<tr>");
            html.Append($"<td>{PatientDetails[7]}</td>");
            html.Append($"<td>{PatientDetails[8]}</td>");
            html.Append($"<td>{PatientDetails[9]}</td>");
            html.Append("</tr>");
            html.Append("</table>");
            html.Append("</td>");
            html.Append("</tr>");
            html.Append("</tbody>");
            html.Append("</table>");
        }
    }

    /// <summary>
    /// Create HTML List
    /// </summary>
    /// <param name="data">Data for List</param>
    /// <param name="sectionName">Section Name</param>
    /// <param name="HasImages">Show Images or not</param>
    /// <param name="HasEndSection">Show End Section or not to set width</param>
    /// <returns></returns>
    private string CreateList(IList<ListModel> data, string sectionName, bool HasImages, bool HasEndSection)
    {
        if (data.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<div style='border: 0.5px solid ;border-radius: 10px;'>");
            sb.Append($"<h3 style='padding-left: 10px'>{sectionName} ({data.Count})</h3>");
            sb.Append($"<table style='width: 100%;padding-left: 10px;padding-right: 10px'>");
            sb.Append("<tbody>");
            int i = 0;
            foreach (var item in data)
            {
                i++;
                string border = i == data.Count ? "none" : "0.5px dotted";
                sb.Append("<tr>");
                if (HasImages)
                {
                    sb.Append(@"<td style='width: 10%;border-bottom: " + border + @" ;'>
                                    <div class='lbl-secondary-text-body-medium-regular d-flex justify-content-center  ltr'>
                                            <div class='row px-0 mx-0 d-flex justify-content-center'>
                                                    <img src='" + item.ImageName + @"' class='avatar-circle' style='height:60px; width:60px; border-radius:30px;'/>
                                            </div>
                                        </div>
                                    </td>");
                }

                string width = "60%";
                if (!HasEndSection)
                {
                    width = "100%";
                }
                sb.Append($"<td style='width: {width};border-bottom: {border} ;'>");
                int j = 0;
                foreach (var middleitem in item.MiddleSection)
                {
                    if (j == 0)
                    {
                        sb.Append("<span style='font-weight: bold;'>" + middleitem.Trim() + "</span><br/>");
                    }
                    else
                    {
                        sb.Append(middleitem.Trim() + "<br/>");
                    }
                    j++;
                }
                sb.Append("</td>");
                if (HasEndSection)
                {
                    sb.Append("<td style='font-weight: bold;width: 30%;border-bottom: 0.5px dotted ;text-align: right'>" + item.EndSection + "</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("</div>");
            return sb.ToString();
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Create HTML table
    /// </summary>
    /// <typeparam name="T">Generic Data Type</typeparam>
    /// <param name="data">Data for Table</param>
    /// <param name="sectionName">Section Name</param>
    /// <param name="showHeaders">Show Headers</param>
    /// <param name="headers">Headers</param>
    /// <param name="resources">resources</param>
    /// <returns> HTML table</returns>
    private string CreateTable<T>(IList<T> data, string sectionName, bool showHeaders, List<TableDataStructureModel> headers, List<ResourceModel> resources)
    {
        if (data != null && data.Count > 0)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            StringBuilder sb = new StringBuilder();
            sb.Append($"<div style='border: 0.5px solid ;border-radius: 10px'><h3 style='padding-left: 10px'>{sectionName} ({data.Count})</h3>");
            sb.Append("<table style='width: 100%;padding-left: 10px;padding-right: 10px'>");
            if (showHeaders)
            {
                sb.Append("<tbody>");
                sb.Append("<tr>");
                foreach (var item in headers)
                {
                    for (int i = 0; i < props.Count; i++)
                    {
                        PropertyDescriptor prop = props[i];
                        if (item.DataField == prop.Name)
                        {
                            sb.Append("<td style='border-bottom: 0.5px dotted ;'><div style='text-align: left;'>" + LibResources.GetResourceValueByKey(resources, item.DataHeader) + "</div></td>");
                            break;
                        }
                    }
                }
                sb.Append("</tr>");
                sb.Append("</tbody>");
            }
            sb.Append("<tbody>");
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                sb.Append("<tr>");
                int j = 0;
                foreach (var header in headers)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (header.DataField == props[i].Name)
                        {
                            if (j == 0)
                            {
                                sb.Append($"<td style='border-bottom: 0.5px dotted ;'>" + props[i].GetValue(item) + "</td>");
                            }
                            else
                            {
                                sb.Append("<td style='font-weight: bold;border-bottom: 0.5px dotted ;'>" + props[i].GetValue(item) + "</td>");
                            }
                            j++;
                        }
                    }
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("</div>");
            return sb.ToString();
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Sync Medical Report Forwards from service
    /// </summary>
    /// <param name="medicalHistoryData">filter data</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>resouces, settings and operation status</returns>
    public async Task SyncMedicalReportForwardsFromServerAsync(MedicalHistoryDTO medicalHistoryData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_MEDICAL_REPORT_FORWARDS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            medicalHistoryData.ErrCode = httpData.ErrCode;
            if (medicalHistoryData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(medicalHistoryData, data);
                    medicalHistoryData.CountryCodes = new CountryService(_essentials).MapCountryCodes(data);
                }
            }
        }
        catch (Exception ex)
        {
            medicalHistoryData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Medical Report Forwards To Server
    /// </summary>
    /// <param name="medicalHistoryData">medical History Data</param>
    /// <param name="OrgDetails">Organisation Details</param>
    /// <param name="PatientDetails">Patient Details</param>
    /// <param name="keyValuePairs">Feature Data</param>
    /// <param name="cancellationToken">cancellation Token</param>
    /// <returns>Operation status call</returns>
    public async Task SyncMedicalReportForwardsToServerAsync(MedicalHistoryDTO medicalHistoryData, List<string> OrgDetails, List<string> PatientDetails, Dictionary<AppPermissions, string> keyValuePairs, CancellationToken cancellationToken)
    {
        try
        {
            await SyncMedicalHistoryAsync(medicalHistoryData, medicalHistoryData.IsActive, true);
            if (medicalHistoryData.ErrCode == ErrorCode.OK)
            {
                GetPrintData(medicalHistoryData, OrgDetails, PatientDetails, keyValuePairs, medicalHistoryData.IsActive);
                if (medicalHistoryData.ErrCode == ErrorCode.OK)
                {
                    //creating temp MedicalHistoryDTO object as medicalHistoryData of type MedicalHistoryDTO is heavy
                    MedicalHistoryDTO historyDTO = new MedicalHistoryDTO();
                    historyDTO.MedicalReportForwards = medicalHistoryData.MedicalReportForwards;
                    historyDTO.HtmlString = medicalHistoryData.HtmlString;
                    var httpData = new HttpServiceModel<MedicalHistoryDTO>
                    {
                        CancellationToken = cancellationToken,
                        PathWithoutBasePath = UrlConstants.SAVE_MEDICAL_REPORT_FORWARDS_ASYNC_PATH,
                        ContentToSend = historyDTO,
                        QueryParameters = new NameValueCollection
                        {
                            { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        },
                    };
                    await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                    medicalHistoryData.ErrCode = httpData.ErrCode;
                }
            }
        }
        catch (Exception ex)
        {
            medicalHistoryData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }
}