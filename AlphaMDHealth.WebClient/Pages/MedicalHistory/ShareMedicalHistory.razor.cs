using System.Globalization;
using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ShareMedicalHistory : BasePage
{
    private readonly MedicalHistoryDTO _medicationData = new MedicalHistoryDTO { MedicalReportForwards = new MedicalReportForwards() };
    private string _doctorName;
    private string _mobileNumber;
    private string _emailAddress;

    [Parameter]
    public MedicalHistoryDTO MedicalHistoryData { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await SendServiceRequestAsync(new MedicalHistoryService(AppState.webEssentials).SyncMedicalReportForwardsFromServerAsync(_medicationData, CancellationToken.None), _medicationData).ConfigureAwait(true);
        if (_medicationData.ErrCode == ErrorCode.OK)
        {
            _isDataFetched = true;
        }
        StateHasChanged();
    }

    /// <summary>
    /// it Share PDF file
    /// </summary>
    /// <returns> Share pdf</returns>
    private async Task OnSendButtonClickedAsync()
    {
        if (IsValid())
        {
            //This will be used for creating HTML
            _medicationData.OrganisationAddress = MedicalHistoryData.OrganisationAddress;
            _medicationData.OrganisationContact = MedicalHistoryData.OrganisationContact;
            _medicationData.MedicalHistory = MedicalHistoryData.MedicalHistory;
            _medicationData.IsActive = MedicalHistoryData.IsActive;
            _medicationData.FromDate = MedicalHistoryData.FromDate;
            _medicationData.ToDate = MedicalHistoryData.ToDate;
            //This will be used for Saving data in Db
            _medicationData.MedicalReportForwards.IsActive = true;
            _medicationData.MedicalReportForwards.DoctorName = _doctorName;
            _medicationData.MedicalReportForwards.EmailID = _emailAddress;
            _medicationData.MedicalReportForwards.MobileNo = _mobileNumber;
            _medicationData.MedicalReportForwards.PatientID = MedicalHistoryData.SelectedUserID;
            if (MedicalHistoryData.FromDate != GenericMethods.GetDefaultDateTime.ToString("o"))
            {
                _medicationData.MedicalReportForwards.ReportForDate = DateTimeOffset.Parse(MedicalHistoryData.FromDate);
            }
            await SaveMedicalReportForwardsAsync().ConfigureAwait(true);
        }
    }

    private async Task SaveMedicalReportForwardsAsync()
    {
        _medicationData.ErrCode = ErrorCode.OK;
        List<string> OrgDetails = new List<string>
        {
            AppState.MasterData.Settings.Find(x => x.SettingKey == SettingsConstants.S_LOGO_KEY).SettingValue,
            AppState.MasterData.OrganisationName,
            _medicationData.OrganisationAddress,
            _medicationData.OrganisationContact
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

        //Appstate not accessible in service so fetching featuretext from here
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

        await SendServiceRequestAsync(new MedicalHistoryService(AppState.webEssentials).SyncMedicalReportForwardsToServerAsync(_medicationData, OrgDetails, PatientDetails, features, CancellationToken.None), _medicationData).ConfigureAwait(true);
        if (_medicationData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_medicationData.ErrCode.ToString());
        }
        else
        {
            Error = _medicationData.ErrCode.ToString();
        }
    }

    private string GetValueToShow(double value)
    {
        return value.Equals(0) ? Constants.SYMBOL_DOUBLE_HYPHEN : value.ToString(CultureInfo.InvariantCulture);
    }

    private async Task OnCanceledClickAsync()
    {
        await OnClose.InvokeAsync(null);
    }
}