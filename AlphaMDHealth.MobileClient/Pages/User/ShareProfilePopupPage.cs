using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class ShareProfilePopupPage : BasePopupPage
{
    internal ShareProfileView _shareProfileView;
    public bool IsPopupClick { get; set; }
    public event EventHandler<EventArgs> OnSaveButtonClicked;

    public ShareProfilePopupPage(BasePage page, object parameters) : base(new BasePage())
    {
        _shareProfileView = new ShareProfileView(page, parameters);
        ScrollView content = new ScrollView { Content = _shareProfileView };
        _parentPage.PageLayout.Add(content, 0, 0);
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await _shareProfileView.LoadUIAsync(false).ConfigureAwait(true);
        _parentPage.PageData = _shareProfileView.ParentPage.PageData;
        _shareProfileView.OnSaveSuccess += ProfileView_OnSaveSuccess;
        SetTitle(string.Format(CultureInfo.InvariantCulture,
       _parentPage.GetFeatureValueByCode(AppPermissions.ShareProfileAddEdit.ToString()),
       _parentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY)));
        if (_parentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.ShareProfileAddEdit.ToString()))
        {
            DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        AppHelper.ShowBusyIndicator = false;
    }

    private async void ProfileView_OnSaveSuccess(object sender, EventArgs e)
    {
        var data = sender as UserDTO;
        if (data.ErrCode == ErrorCode.OK)
        {
            _parentPage.DisplayOperationStatus(data.ErrorDescription,true);
            OnSaveButtonClicked?.Invoke(ErrorCode.OK.ToString(), new EventArgs());
            //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
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
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;
            await _shareProfileView.OnSaveButtonClickedAsync().ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
    }
    protected override async void OnDisappearing()
    {
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        _shareProfileView.OnSaveSuccess -= ProfileView_OnSaveSuccess;
        IsPopupClick = false;
        await _shareProfileView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }
}