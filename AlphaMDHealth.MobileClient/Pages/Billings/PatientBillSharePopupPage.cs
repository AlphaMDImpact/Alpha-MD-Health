using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientBillSharePopupPage : BasePopupPage
{
    private readonly PatientBillDetailsView _patientBillDetailsView;

    public PatientBillSharePopupPage(BasePage page, object parameters) : base(new BasePage())
    {
        _patientBillDetailsView = new PatientBillDetailsView(page, parameters);
        ScrollView content = new ScrollView { Content = _patientBillDetailsView };
        _parentPage.PageLayout.Add(content, 0, 0);
    }

    protected async override void OnAppearing()
    {
        await _patientBillDetailsView.LoadUIAsync(false).ConfigureAwait(true);
        _parentPage.PageData = _patientBillDetailsView.ParentPage.PageData;
        _patientBillDetailsView.OnSaveSuccess += PatientBillingAddEdit_OnSaveSuccess;

        SetTitle(string.Format(CultureInfo.InvariantCulture,
       _parentPage.GetFeatureValueByCode(Utility.AppPermissions.PatientBillDetailsView.ToString()),
       _parentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY)));
        if (_parentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientBillShare.ToString()))
        {
            DisplayRightHeader(MenuAction.MenuActionShareKey.ToString());
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        AppHelper.ShowBusyIndicator = false;
    }

    private async void PatientBillingAddEdit_OnSaveSuccess(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        await _patientBillDetailsView.ShareButtonClicked().ConfigureAwait(true);
    }

    protected override async void OnDisappearing()
    {
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        await _patientBillDetailsView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }
}