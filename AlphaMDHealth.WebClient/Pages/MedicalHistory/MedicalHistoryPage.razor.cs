using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.JSInterop;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class MedicalHistoryPage : BasePage
{
    private MedicalHistoryService _service;
    private MedicalHistoryDTO _medicalHistoryData;
    private MedicalHistoryDTO _shareMedicalHistoryData;
    private List<ButtonActionModel> _buttons;
    private AppPermissions _selectedFeature;
    private bool _isPrintAllowed = false;
    private bool _isShareAllowed = false;

    private List<OptionModel> _accordianOptions;
    private OptionModel selectedView = null;
    private string _selectedOption;

    protected override async Task OnInitializedAsync()
    {
        await GetPageDataAsync();
        _isDataFetched = true;
        StateHasChanged();
    }

    private async Task GetPageDataAsync()
    {
        _service = new MedicalHistoryService(AppState.webEssentials);
        _medicalHistoryData = new MedicalHistoryDTO() { RecordCount = 0 };
        await SendServiceRequestAsync(_service.SyncMedicalHistoryAsync(_medicalHistoryData), _medicalHistoryData).ConfigureAwait(true);
        _isPrintAllowed = LibPermissions.HasPermission(_medicalHistoryData.FeaturePermissions, AppPermissions.MedicalHistoryPrint.ToString());
        _isShareAllowed = LibPermissions.HasPermission(_medicalHistoryData.FeaturePermissions, AppPermissions.MedicalHistoryShare.ToString());
        GetAccordianOptions();
        await OnAccordianContentChanged(_accordianOptions.FirstOrDefault()?.OptionID.ToString() ?? string.Empty);
    }

    private List<SystemFeatureParameterModel> GetViewParameters(MedicalHistoryViewModel view, MedicalHistoryModel historyData)
    {
        var parameters = ParameterMethods.AddParameters(ParameterMethods.CreateParameter("IsMedicalHistory", true.ToString()));
        if (!view.ShowAllData && historyData != null)
        {

            parameters.Add(ParameterMethods.CreateParameter(nameof(BaseDTO.FromDate), historyData?.HistoryFromDate.ToString() ?? null));
            parameters.Add(ParameterMethods.CreateParameter(nameof(BaseDTO.ToDate), historyData?.HistoryToDate.ToString() ?? null));
        }
        return parameters;
    }

    private List<OptionModel> GetHistoryOptions(MedicalHistoryModel history)
    {
        List<OptionModel> options = new List<OptionModel>();
        if (GenericMethods.IsListNotEmpty(history?.MedicalHistoryViews) && history.MedicalHistoryViews.Any(x => !x.ShowAllData && x.HasData))
        {
            for (var i = 0; i < history?.MedicalHistoryViews.Count; i++)
            {
                var view = history.MedicalHistoryViews[i];
                options.Add(new OptionModel
                {
                    OptionID = i,
                    GroupName = view.FeatureCode.ToString(),
                    OptionText = LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, view.FeatureCode.ToString()),
                });
            }
        }
        return options;
    }

    private List<OptionModel> GetHistoryFixedOptions()
    {
        List<OptionModel> options = new List<OptionModel>();

        //Render Medical history views for all date range(MedicalHistoryFixedFeatures)
        if (GenericMethods.IsListNotEmpty(_medicalHistoryData?.AllMedicalHistoryViews))
        {
            for (var i = 0; i < _medicalHistoryData?.AllMedicalHistoryViews.Count; i++)
            {
                var view = _medicalHistoryData?.AllMedicalHistoryViews[i];
                if (view.ShowAllData && view.HasData)
                {
                    options.Add(new OptionModel
                    {
                        OptionID = i,
                        GroupName = view.FeatureCode.ToString(),
                        OptionText = LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, view.FeatureCode.ToString()),
                        IsDisabled = false,
                    });
                }
            }
        }

        return options;
    }

    private void GetAccordianOptions()
    {
        _accordianOptions = GetHistoryFixedOptions();
        if (GenericMethods.IsListNotEmpty(_medicalHistoryData?.MedicalHistory))
        {
            LibSettings.TryGetDateFormatSettings(AppState.MasterData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
            foreach (var item in _medicalHistoryData.MedicalHistory)
            {
                _accordianOptions.Add(new OptionModel
                {
                    OptionID = (_accordianOptions.LastOrDefault()?.OptionID ?? 0) + 1,
                    ParentOptionText = item.HistoryFromDate,
                    OptionText = GenericMethods.GetLocalDateTimeBasedOnCulture(item.HistoryDate, DateTimeType.Date, dayFormat, monthFormat, yearFormat),
                    IsDisabled = !item.IsLoaded
                });
            }
        }
    }

    private async Task OnAccordianContentChanged(object optionID)
    {
        if (!string.IsNullOrEmpty(optionID as string))
        {
            _selectedOption = optionID as string;
        }
        selectedView = _accordianOptions.FirstOrDefault(x => x.OptionID.ToString() == _selectedOption);
        if (!string.IsNullOrWhiteSpace(selectedView?.ParentOptionText))
        {
            var currenthistory = _medicalHistoryData.MedicalHistory.FirstOrDefault(x => x.HistoryFromDate == selectedView.ParentOptionText);
            if (currenthistory == null)
            {
                _medicalHistoryData.ToDate = _medicalHistoryData.FromDate;
                _medicalHistoryData.FromDate = GenericMethods.GetDefaultDateTime.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                _medicalHistoryData.ToDate = currenthistory.HistoryToDate;
                _medicalHistoryData.FromDate = currenthistory.HistoryFromDate;
            }
            bool changeState = false;
            if (!(currenthistory?.IsLoaded ?? false))
            {
                await SendServiceRequestAsync(_service.SyncMedicalHistoryAsync(_medicalHistoryData), _medicalHistoryData).ConfigureAwait(true);
                changeState = true;
            }

            _medicalHistoryData.MedicalHistory?.ForEach(x =>
            {
                if (x.IsLoaded && !x.MedicalHistoryViews.Any(y => y.HasData))
                {
                    var toRemove = _accordianOptions?.FirstOrDefault(z => z.ParentOptionText == x.HistoryFromDate);
                    if (toRemove != null)
                    {
                        _accordianOptions.Remove(toRemove);
                        changeState = true;
                    }
                }
            });
            if (changeState)
            {
                StateHasChanged();
            }
            selectedView.IsDisabled = false;
        }
    }

    private async void OnRefreshClicked()
    {
        await GetPageDataAsync();
        _isDataFetched = true;
        StateHasChanged();
    }

    private Task OnAddFeatureSelectionChanged(object e)
    {
        ShowDetailPage = true;
        var model = _medicalHistoryData.AddHistoryFor.Find(x => x.OptionID == Convert.ToInt64(e));
        _selectedFeature = model?.GroupName?.ToEnum<AppPermissions>() ?? default;
        return Task.CompletedTask;
    }

    private void AddEditClosedReadingEventCallback(string readingData)
    {
        ShowDetailPage = false;
        ClearSelection();
        if (!string.IsNullOrEmpty(readingData))
        {
            string[] values = readingData.Split('|');
            AddEditClosedWithIsDataUpdatedEventCallback(values[0]);
        }
        else
        {
            ClearSelection();
        }
    }

    private void AddEditClosedWithIsDataUpdatedEventCallback(string isDataUpdated)
    {
        ShowDetailPage = false;
        ClearSelection();
        if (isDataUpdated == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = isDataUpdated;
            OnRefreshClicked();
        }
        else if (!string.IsNullOrWhiteSpace(isDataUpdated))
        {
            Error = isDataUpdated;
        }
    }

    private void ShareAddEditClosedEventCallback(string response)
    {
        ShowDetailPage = false;
        ClearSelection();
        if (response == ErrorCode.OK.ToString())
        {
            Success = response;
        }
        else
        {
            Error = response;
        }
    }

    private void ClearSelection()
    {
        _selectedFeature = default;
        Success = Success = string.Empty;
    }

    private void OnShareClicked(MedicalHistoryModel medicalHistory)
    {
        _shareMedicalHistoryData = new MedicalHistoryDTO();
        _shareMedicalHistoryData.OrganisationAddress = _medicalHistoryData.OrganisationAddress;
        _shareMedicalHistoryData.OrganisationContact = _medicalHistoryData.OrganisationContact;
        _shareMedicalHistoryData.SelectedUserID = _medicalHistoryData.SelectedUserID;
        if (medicalHistory.HistoryFromDate == null || medicalHistory.HistoryToDate == null)
        {
            _shareMedicalHistoryData.IsActive = true;
            _shareMedicalHistoryData.FromDate = GenericMethods.GetDefaultDateTime.ToString("o");
            _shareMedicalHistoryData.ToDate = GenericMethods.GetUtcDateTime.ToString("o");
        }
        else
        {
            _shareMedicalHistoryData.IsActive = false;
            _shareMedicalHistoryData.MedicalHistory = _medicalHistoryData.MedicalHistory.Where(x => x.HistoryFromDate == medicalHistory.HistoryFromDate).ToList();
            _shareMedicalHistoryData.FromDate = medicalHistory.HistoryFromDate;
            _shareMedicalHistoryData.ToDate = medicalHistory.HistoryToDate;
        }
        _selectedFeature = AppPermissions.MedicalHistoryShare;
        ShowDetailPage = true;
        StateHasChanged();
    }

    private static string GetValueToShow(double value)
    {
        return value.Equals(0) ? Constants.SYMBOL_DOUBLE_HYPHEN : value.ToString(CultureInfo.InvariantCulture);
    }

    private async void OnPrintClicked(MedicalHistoryModel medicalHistory)
    {
        bool isAllData = false;
        MedicalHistoryDTO medicalHistoryData = new MedicalHistoryDTO();
        medicalHistoryData.OrganisationAddress = _medicalHistoryData.OrganisationAddress;
        medicalHistoryData.OrganisationContact = _medicalHistoryData.OrganisationContact;
        if (medicalHistory.HistoryFromDate == null && medicalHistory.HistoryToDate == null)
        {
            isAllData = true;
            medicalHistoryData.FromDate = GenericMethods.GetDefaultDateTime.ToString("o");
            medicalHistoryData.ToDate = GenericMethods.GetUtcDateTime.ToString("o");
        }
        else
        {
            medicalHistoryData.MedicalHistory = _medicalHistoryData.MedicalHistory.Where(x => x.HistoryFromDate == medicalHistory.HistoryFromDate).ToList();
            medicalHistoryData.FromDate = medicalHistory.HistoryFromDate;
            medicalHistoryData.ToDate = medicalHistory.HistoryToDate;
        }
        await SendServiceRequestAsync(_service.SyncMedicalHistoryAsync(medicalHistoryData, isAllData, true), medicalHistoryData).ConfigureAwait(true);
        if (medicalHistoryData.ErrCode == ErrorCode.OK)
        {
            //Appstate not accessible in service so fetching from here
            List<string> OrgDetails = new List<string>
            {
                AppState.MasterData.Settings.Find(x => x.SettingKey == SettingsConstants.S_LOGO_KEY).SettingValue,
                AppState.MasterData.OrganisationName,
                medicalHistoryData.OrganisationAddress,
                medicalHistoryData.OrganisationContact
            };
            var PatientDetails = new List<string>
            {
                AppState.UserDetails.User.ImageName,
                string.Concat(AppState.UserDetails.User.FirstName, Constants.STRING_SPACE, AppState.UserDetails.User.LastName),
                AppState.UserDetails.User.UserAge.ToString(),
                LibResources.GetResourceValueByKey(AppState.UserDetails.Resources, AppState.UserDetails.User.GenderID),
                AppState.UserDetails.User.EmailId,
                AppState.UserDetails.User.PhoneNo,
                AppState.UserDetails.User.SocialSecurityNo,
                GetValueToShow(AppState.UserDetails.User.Height),
                AppState.UserDetails.Resources.FirstOrDefault(x => x.ResourceKeyID == AppState.UserDetails.User.BloodGroupID && x.LanguageID == AppState.SelectedLanguageID)?.ResourceValue ?? Constants.SYMBOL_DOUBLE_HYPHEN ,
                GetValueToShow(AppState.UserDetails.User.Weight),
                LibResources.GetResourceValueByKey(AppState.UserDetails.Resources, ResourceConstants.R_HEIGHT_KEY),
                LibResources.GetResourceValueByKey(AppState.UserDetails.Resources, ResourceConstants.R_WEIGHT_KEY),
                LibResources.GetResourceValueByKey(AppState.UserDetails.Resources, ResourceConstants.R_BLOOD_GROUP_KEY),
                LibResources.GetResourceValueByKey(AppState.UserDetails.Resources, ResourceConstants.R_PATIENT_KEY),
                LibResources.GetResourceValueByKey(AppState.UserDetails.Resources, ResourceConstants.R_HISTORY_TASK_KEY),
            };

            Dictionary<AppPermissions, string> features = new Dictionary<AppPermissions, string>()
            {
                {AppPermissions.PatientReadingsView,LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.PatientReadingsView.ToString())},
                {AppPermissions.PatientProviderNotesView,LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.PatientProviderNotesView.ToString())},
                {AppPermissions.PrescriptionView,LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.PrescriptionView.ToString())},
                {AppPermissions.PatientTasksView,LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.PatientTasksView.ToString())},
                {AppPermissions.PatientFilesView,LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.PatientFilesView.ToString())},
                {AppPermissions.PatientEducationsView,LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.PatientEducationsView.ToString())},
                {AppPermissions.PatientTrackersView,LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.PatientTrackersView.ToString())},
            };
            _service.GetPrintData(medicalHistoryData, OrgDetails, PatientDetails, features, isAllData);
            if (medicalHistoryData.ErrCode == ErrorCode.OK)
            {
                await JSRuntime.InvokeVoidAsync("printData", medicalHistoryData.HtmlString);
            }
        }
    }
}