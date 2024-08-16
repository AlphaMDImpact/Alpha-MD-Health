using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PatientFilePage))]
[QueryProperty(nameof(DocumentID), "id")]
public class PatientFilePage : BasePage
{
    private readonly PatientFileView _fileView;
    private string _documentID;

    /// <summary>
    /// User Id of the selected conversation
    /// </summary>
    public string DocumentID
    {
        get
        {
            return _documentID;
        }
        set => _documentID = Uri.UnescapeDataString(value);
    }

    public PatientFilePage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _fileView = new PatientFileView(this, null);
        HideFooter(true);
        PageLayout.Add(_fileView);
        SetPageContent(false);
    }
    
    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _fileView.Parameters = AddParameters(CreateParameter(nameof(FileModel.FileID), Convert.ToString(_documentID, CultureInfo.InvariantCulture)));
        await _fileView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        await _fileView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }

    /// <summary>
    /// Method to override to handle header item clicks
    /// </summary>
    /// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
    /// <param name="menuType">Menu position whether Left, Right</param>
    /// <param name="menuAction">Action type</param>
    public async override Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        if (menuAction == MenuAction.MenuActionSaveKey)
        {
            await _fileView.SaveDocumentAsync().ConfigureAwait(true);
        }
    }
}