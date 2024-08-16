using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PatientFilesPage))]
[QueryProperty("RecordCount", "recordCount")]
[QueryProperty(nameof(DocumentID), "id")]
public class PatientFilesPage : BasePage
{
    private readonly PatientFilesView _filesListView;
    private int _recordCount;
    private string _documentID;

    public string RecordCount
    {
        get
        {
            return _recordCount.ToString(CultureInfo.InvariantCulture);
        }
        set
        {
            _recordCount = Convert.ToInt32(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    public string DocumentID
    {
        get
        {
            return _documentID;
        }
        set
        {
            _documentID = Uri.UnescapeDataString(value);
        }
    }

    public PatientFilesPage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _filesListView = new PatientFilesView(this, null) { Margin = new Thickness(0) };
        PageLayout.Add(_filesListView);
        PageLayout.Padding = 0;
        SetPageContent(false);
    }

    protected override async void OnAppearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            AppHelper.ShowBusyIndicator = true;
            await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
            base.OnAppearing();
            await LoadDataAsync().ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
    }

    protected override async void OnDisappearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            await _filesListView.UnloadUIAsync().ConfigureAwait(true);
            _documentID = string.Empty;
            base.OnDisappearing();
        }
    }

    /// <summary>
    /// Method to override to handle header item clicks
    /// </summary>
    /// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
    /// <param name="menuType">Menu position whether Left, Right</param>
    /// <param name="menuAction">Action type</param>
    public async override Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        if (menuAction == MenuAction.MenuActionAddKey)
        {
            await _filesListView.DocumentClickAsync(Guid.Empty, true).ConfigureAwait(true);
        }
        else
        {
            if (menuAction == MenuAction.MenuActionSaveKey && MobileConstants.IsTablet && await _filesListView.SaveDocumentAsync().ConfigureAwait(true))
            {
                _recordCount = Constants.ZERO_VALUE;
                _documentID = string.Empty;
                await LoadDataAsync().ConfigureAwait(true);
                ShowHideLeftRightHeader(MenuLocation.Right, false);
                MenuView titleView = new MenuView(MenuLocation.Header, string.Empty, ShellMasterPage.CurrentShell.CurrentPage.IsAddEditPage);
                await OverrideTitleViewAsync(titleView).ConfigureAwait(true);
            }
        }
    }

    private async Task LoadDataAsync()
    {
        _filesListView.Parameters = AddParameters(
                 CreateParameter(nameof(BaseDTO.RecordCount), _recordCount.ToString(CultureInfo.InvariantCulture)),
                 CreateParameter(nameof(FileModel.FileID), _documentID),
                 CreateParameter(nameof(BaseDTO.SelectedUserID), App._essentials.GetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)0).ToString(CultureInfo.InvariantCulture)));
        await _filesListView.LoadUIAsync(false).ConfigureAwait(true);
    }
}