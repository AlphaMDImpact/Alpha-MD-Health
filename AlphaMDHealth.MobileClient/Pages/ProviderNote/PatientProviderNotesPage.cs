using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Provider Notes list page
/// </summary>
[RouteRegistration(nameof(PatientProviderNotesPage))]
[QueryProperty("RecordCount", "recordCount")]
[QueryProperty(nameof(PatientProviderNoteModel.ProviderNoteID), "id")]
public class PatientProviderNotesPage : BasePage
{
    private readonly PatientProviderNotesView _patientProviderNoteView;
    private string _providerNoteID;
    private string _recordCount;

    /// <summary>
    /// Record count 
    /// </summary>
    public string RecordCount
    {
        get
        {
            return _recordCount;
        }
        set
        {
            _recordCount = Uri.UnescapeDataString(value);
        }
    }

    /// <summary>
    /// Patient Provider Note ID 
    /// </summary>
    public string ProviderNoteID
    {
        get
        {
            return _providerNoteID;
        }
        set
        {
            _providerNoteID = Uri.UnescapeDataString(value);
        }
    }

    public PatientProviderNotesPage()
    {
        _patientProviderNoteView = new PatientProviderNotesView(this, null);
        Content = _patientProviderNoteView;
    }

    /// <summary>
    /// Provider notes page on appearing event
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        _patientProviderNoteView.Parameters = AddParameters(
            CreateParameter(nameof(BaseDTO.RecordCount), _recordCount),
            CreateParameter(nameof(PatientProviderNoteModel.ProviderNoteID), _providerNoteID));
        await _patientProviderNoteView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Provider notes page on appearing event
    /// </summary>
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _patientProviderNoteView.UnloadUIAsync().ConfigureAwait(false);
    }
}