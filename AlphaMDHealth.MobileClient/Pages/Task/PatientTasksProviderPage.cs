using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PatientTasksProviderPage))]
[QueryProperty(nameof(RecordCount), "recordCount")]
public class PatientTasksProviderPage : BasePage
{
    private readonly PatientTasksView _patientTasksListView;
    private string _recordCount;

    /// <summary>
    /// No of pateint tasks to be displayed
    /// </summary>
    public string RecordCount
    {
        get
        {
            return _recordCount.ToString(CultureInfo.InvariantCulture);
        }
        set
        {
            _recordCount = Uri.UnescapeDataString(value);
        }
    }

    public PatientTasksProviderPage()
    {
        _patientTasksListView = new PatientTasksView(this, null);
        AddRowColumnDefinition(GridLength.Star, 1, true);
        PageLayout.Add(_patientTasksListView, 0, 0);
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _patientTasksListView.Parameters = AddParameters(CreateParameter(nameof(BaseDTO.RecordCount), _recordCount));
        await _patientTasksListView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _patientTasksListView.UnloadUIAsync().ConfigureAwait(false);
    }
}