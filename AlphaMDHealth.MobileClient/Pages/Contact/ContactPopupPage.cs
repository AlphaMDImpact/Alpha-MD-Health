using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class ContactPopupPage : BasePopupPage
{
    private readonly string _contactID;
    internal PatientContactView _contactView;

    public static bool IsPopupClick { get; set; }

    public ContactPopupPage(string contactID) : base(new BasePage())
    {
        _contactID = contactID;
        //todo: CloseWhenBackgroundIsClicked = false;
        Padding = _parentPage.GetPopUpPagePadding(PopUpPageType.Long);
        _contactView = new PatientContactView(_parentPage, _parentPage.AddParameters(
            _parentPage.CreateParameter(nameof(ContactModel.ContactID), Convert.ToString(_contactID, CultureInfo.InvariantCulture)))
        );
        _parentPage.PageLayout.Add(_contactView, 0, 0);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _contactView.Parameters = _parentPage.AddParameters(
            _parentPage.CreateParameter(nameof(ContactModel.ContactID), Convert.ToString(_contactID, CultureInfo.InvariantCulture))
        );
        await _contactView.LoadUIAsync(false).ConfigureAwait(true);
        _parentPage.PageData = _contactView.ParentPage.PageData;
        SetTitle(string.Format(CultureInfo.InvariantCulture,
            _parentPage.GetFeatureValueByCode(AppPermissions.PatientContactAddEdit.ToString()),
            _parentPage.GetResourceValueByKey(_contactID == Guid.Empty.ToString()
                ? ResourceConstants.R_ADD_ACTION_KEY
                : ResourceConstants.R_EDIT_ACTION_KEY)
        ));
        if (_parentPage.CheckFeaturePermissionByCode(AppPermissions.PatientContactAddEdit.ToString()))
        {
            DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        IsPopupClick = false;
        await _contactView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        await _contactView.OnSaveActionClicked().ConfigureAwait(true);
    }
}