using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PatientProgramsPage))]
[QueryProperty(nameof(RecordCount), "recordCount")]
public class PatientProgramsPage : BasePage
{
    private readonly PatientProgramsView _programsListView;
    private int _recordCount;

    /// <summary>
    /// Record Count 
    /// </summary>
    public string RecordCount
    {
        get { return _recordCount.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _recordCount = Convert.ToInt32(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    public PatientProgramsPage()
    {
        _programsListView = new PatientProgramsView(this, null) { Margin = new Thickness(0) };
        AddRowColumnDefinition(GridLength.Star, 1, true);
        PageLayout.Add(_programsListView, 0, 0);
    }

    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _programsListView.Parameters = AddParameters(CreateParameter(nameof(BaseDTO.RecordCount), _recordCount.ToString(CultureInfo.InvariantCulture)));
        await _programsListView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        await _programsListView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }
}