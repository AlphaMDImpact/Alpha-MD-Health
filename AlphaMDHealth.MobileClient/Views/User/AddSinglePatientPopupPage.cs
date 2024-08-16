using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class AddSinglePatientPopupPage : BasePopupPage
{
    internal ProfileView _profileView;
    public static bool IsPopupClick { get; set; }

    public AddSinglePatientPopupPage(BasePage page, object parameters) : base(new BasePage())
    {

        _profileView = new ProfileView(page, parameters);
        ScrollView content = new ScrollView { Content = _profileView };
        _parentPage.PageLayout.Add(content, 0, 0);
    }

    protected async override void OnAppearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            base.OnAppearing();
            await _profileView.LoadUIAsync(false).ConfigureAwait(true);
            _parentPage.PageData = _profileView.ParentPage.PageData;
            _profileView.OnSaveSuccess += ProfileView_OnSaveSuccess;

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

    private async void ProfileView_OnSaveSuccess(object sender, EventArgs e)
    {
        var data = sender as UserDTO;
        if (data.ErrCode == ErrorCode.OK)
        {
            //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
            await new BasePage().SyncDataWithServerAsync(Pages.PatientsPage, ServiceSyncGroups.RSSyncFromServerGroup, DataSyncFor.Users, DataSyncFor.Users.ToString(), default).ConfigureAwait(true);
            _profileView.RefreshList();
        }
        else
        {
            _parentPage.DisplayOperationStatus(data.ErrorDescription);
        }

    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        await _profileView.OnSaveButtonClickedAsync().ConfigureAwait(true);
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
        // await SyncUsersFromServerAsync(userData, CancellationToken.None).ConfigureAwait(false)
    }

    protected override async void OnDisappearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
            OnRightHeaderClickedEvent -= OnRightHeaderClicked;
            _profileView.OnSaveSuccess -= ProfileView_OnSaveSuccess;
            IsPopupClick = false;
            await _profileView.UnloadUIAsync().ConfigureAwait(true);

            base.OnDisappearing();
        }
    }
}