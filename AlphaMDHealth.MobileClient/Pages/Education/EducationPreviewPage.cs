using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration("EducationPreviewPage")]
[QueryProperty("PageID", "id")]
[QueryProperty("PatientTaskID", "identifier")]
public class EducationPreviewPage : BasePage
{
    private string _pageID;
    private long _patientTaskID;
    private readonly EducationPreview _educationView;

    /// <summary>
    /// Page ID of the education selected
    /// </summary>
    public string PageID
    {
        get
        {
            return _pageID;
        }
        set => _pageID = Uri.UnescapeDataString(value);
    }

    /// <summary>
    /// Patient task ID of task type education
    /// </summary>
    public string PatientTaskID
    {
        get
        {
            return _patientTaskID.ToString();
        }
        set => _patientTaskID = string.IsNullOrWhiteSpace(Uri.UnescapeDataString(value)) ? 0 : Convert.ToInt64(Uri.UnescapeDataString(value));
    }

    public EducationPreviewPage() : base(PageLayoutType.MastersContentPageLayout, true)
    {
        _educationView = new EducationPreview(this, null);
        Content = _educationView;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        await _educationView.LoadUIDataAsync(_pageID, _patientTaskID).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _educationView.UnloadUIAsync().ConfigureAwait(true);
    }
}