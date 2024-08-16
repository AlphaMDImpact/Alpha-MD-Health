using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientsBulkUploadPopupPage : BasePopupPage
{
    internal PatientBulkUploadView _patientBulkUploadView;
    public static bool IsPopupClick { get; set; }

    public PatientsBulkUploadPopupPage(BasePage page, object parameters) : base(new BasePage())
    {
        _patientBulkUploadView = new PatientBulkUploadView(page, parameters);
        _parentPage.PageLayout.Add(_patientBulkUploadView, 0, 0);
    }

    protected async override void OnAppearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            base.OnAppearing();
            _patientBulkUploadView.OnSaveSuccess += BulkPatients_OnSaveSuccess;
            await _patientBulkUploadView.LoadUIAsync(false).ConfigureAwait(true);
            _parentPage.PageData = _patientBulkUploadView.ParentPage.PageData;
            SetTitle(string.Format(CultureInfo.InvariantCulture,
           _parentPage.GetFeatureValueByCode(Utility.AppPermissions.PatientsView.ToString()),
           _parentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY)));
            if (_parentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientAddEdit.ToString()))
            {
                DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
                OnRightHeaderClickedEvent += OnRightHeaderClicked;
            }
            DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
            OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
            AppHelper.ShowBusyIndicator = false;
        }
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void BulkPatients_OnSaveSuccess(object sender, EventArgs e)
    {
        var data = sender as UserDTO;
        switch (data.ErrCode)
        {
            case ErrorCode.OK:
                await ClosePopupAsync().ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
                _patientBulkUploadView.RefreshList();
                break;
            case ErrorCode.BulkUploadDataEntryStatus:
                _parentPage.DisplayOperationStatus(data.ErrorDescription);
                await new BasePage().SyncDataWithServerAsync(Pages.PatientsPage, ServiceSyncGroups.RSSyncFromServerGroup, DataSyncFor.Users, DataSyncFor.Users.ToString(), default).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
                _patientBulkUploadView.RefreshList();
                break;
            default:
                _parentPage.DisplayOperationStatus(data.ErrorDescription);
                break;
        }
        AppHelper.ShowBusyIndicator = false;
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;
            await _patientBulkUploadView.OnSaveButtonClickedAsync().ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
    }

    protected override async void OnDisappearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
            OnRightHeaderClickedEvent -= OnRightHeaderClicked;
            IsPopupClick = false;
            await _patientBulkUploadView.UnloadUIAsync().ConfigureAwait(true);
            base.OnDisappearing();
        }
    }
}