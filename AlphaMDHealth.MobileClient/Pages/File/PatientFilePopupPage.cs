using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientFilePopupPage : BasePopupPage
{
    private readonly string _fileID;
    internal PatientFileView _fileView;

    public static bool IsPopupClick { get; set; }

    public PatientFilePopupPage(string appointmentID) : base(new BasePage())
    {
        _fileID = appointmentID;
        //todo: CloseWhenBackgroundIsClicked = false;
        _fileView = new PatientFileView(_parentPage, _parentPage.AddParameters(_parentPage.CreateParameter(nameof(FileModel.FileID), Convert.ToString(_fileID, CultureInfo.InvariantCulture))));
        _parentPage.PageLayout.Add(_fileView, 0, 0);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _fileView.Parameters = _parentPage.AddParameters(_parentPage.CreateParameter(nameof(FileModel.FileID), Convert.ToString(_fileID, CultureInfo.InvariantCulture)));
        await _fileView.LoadUIAsync(true).ConfigureAwait(true);
        _parentPage.PageData = _fileView._parentPage.PageData;
        SetTitle(_parentPage.GetFeatureValueByCode(AppPermissions.PatientFileAddEdit.ToString()));
        if (_parentPage.CheckFeaturePermissionByCode(AppPermissions.PatientFileAddEdit.ToString()))
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
        await _fileView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            await _fileView.SaveDocumentDataAsync().ConfigureAwait(true);
        }
    }
}