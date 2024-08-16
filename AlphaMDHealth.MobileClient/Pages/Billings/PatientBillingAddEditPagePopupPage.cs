using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientBillingAddEditPagePopupPage : BasePopupPage
{
    /// <summary>
    /// on click event of Send Button
    /// </summary>
    public event EventHandler<EventArgs> OnSaveButtonClicked;
    internal PatientBillingView _patientBillingView;

    public PatientBillingAddEditPagePopupPage(BasePage page, object parameters) :  base(new BasePage())
    {
        _patientBillingView = new PatientBillingView(page, parameters);
        ScrollView content = new ScrollView { Content = _patientBillingView };
        _parentPage.PageLayout.Add(content, 0, 0);
    }

    protected async override void OnAppearing()
    {
        await _patientBillingView.LoadUIAsync(false).ConfigureAwait(true);
        _parentPage.PageData = _patientBillingView.ParentPage.PageData;
        _patientBillingView.OnSaveSuccess += PatientBillingAddEdit_OnSaveSuccess;

        SetTitle(string.Format(CultureInfo.InvariantCulture,
       _parentPage.GetFeatureValueByCode(Utility.AppPermissions.PatientBillAddEdit.ToString()),
       _parentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY)));
        if (_parentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientBillAddEdit.ToString()) && !_patientBillingView._isProgramDeleted)
        {
            DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        AppHelper.ShowBusyIndicator = false;
    }

    private void PatientBillingAddEdit_OnSaveSuccess(object sender, EventArgs e)
    {
        OnSaveButtonClicked?.Invoke((string)sender, new EventArgs());
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        //todo:
        //await Navigation.PopPopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        await _patientBillingView.OnSaveButtonClickedAsync().ConfigureAwait(true);
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
    }

    protected override async void OnDisappearing()
    {
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        await _patientBillingView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }
}