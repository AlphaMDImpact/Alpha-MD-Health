using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class LinkedUserPopUpPage : BasePopupPage
{
    internal ProfileView _profileView;
    public bool ShowDeleteButton { get; set; }
    public event EventHandler<EventArgs> OnSaveButtonClicked;
  
    public LinkedUserPopUpPage(BasePage page, object parameters) : base(new BasePage())
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

            SetTitle(string.Format(CultureInfo.InvariantCulture
                , _parentPage.GetFeatureValueByCode(AppPermissions.LinkedUserAddEdit.ToString())
                , _parentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY))
            );
            if (ShowDeleteButton && _parentPage.CheckFeaturePermissionByCode(AppPermissions.LinkedUserDelete.ToString()))
            {
                DisplayBottomButton(ResourceConstants.R_DELETE_ACTION_KEY, FieldTypes.DeleteTransparentExButtonControl);
                OnBottomButtonClickedEvent += DeleteButtonClicked;
            }
            if (_parentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.LinkedUserAddEdit.ToString()))
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
        AppHelper.ShowBusyIndicator = true;
        var data = sender as UserDTO;
        if (data.ErrCode == ErrorCode.OK)
        {
            await new BasePage().SyncDataWithServerAsync(Pages.PatientsPage, ServiceSyncGroups.RSSyncFromServerGroup, DataSyncFor.Users, DataSyncFor.Users.ToString(), default).ConfigureAwait(true);
            //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
            OnSaveButtonClicked?.Invoke(sender, new EventArgs());
        }
        else
        {
            AppHelper.ShowBusyIndicator = false;
            _parentPage.DisplayOperationStatus(data.ErrorDescription);
        }
        _profileView.IsSaveInProgress = false;
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        if (!_profileView.IsSaveInProgress)
        {
            await ClosePopupAsync().ConfigureAwait(true);
        }
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        _profileView.IsSaveInProgress = true;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        AppHelper.ShowBusyIndicator = true;
        await _profileView.OnSaveButtonClickedAsync().ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
    }

    private async void DeleteButtonClicked(object sender, EventArgs e)
    {
        await _parentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnMessgeViewActionClicked, true, true, false).ConfigureAwait(true);
    }

    private async void OnMessgeViewActionClicked(object sender, int e)
    {
        OnBottomButtonClickedEvent -= DeleteButtonClicked;
        _parentPage.OnClosePupupAction(sender, e);
        if (e == 1)
        {
            await _profileView.OnDeleteButtonClickedAsync().ConfigureAwait(true);
        }
        OnBottomButtonClickedEvent += DeleteButtonClicked;
    }
    protected override async void OnDisappearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
            OnRightHeaderClickedEvent -= OnRightHeaderClicked;
            _profileView.OnSaveSuccess -= ProfileView_OnSaveSuccess;
            await _profileView.UnloadUIAsync().ConfigureAwait(true);
            base.OnDisappearing();
        }
    }
}