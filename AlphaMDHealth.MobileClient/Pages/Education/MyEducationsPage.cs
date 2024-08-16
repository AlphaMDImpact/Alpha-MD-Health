using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Education list page
/// </summary>
[RouteRegistration(nameof(MyEducationsPage))]
[QueryProperty("RecordCount", "recordCount")]
[QueryProperty("EducationCategoryID", "id")]
[QueryProperty("EducationID", "identifier")]
public class MyEducationsPage : BasePage
{
    private readonly EducationsView _educationListView;
    private string _category;
    private string _recordCount;
    private string _educationID;

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
    /// Education Category ID
    /// </summary>
    public string EducationCategoryID
    {
        get
        {
            return _category;
        }
        set
        {
            _category = Uri.UnescapeDataString(value);
            if (_category.Equals(0))
            {
                DataMethod();
            }
        }
    }

    /// <summary>
    /// Education ID
    /// </summary>
    public string EducationID
    {
        get
        {
            return _educationID;
        }
        set
        {
            _educationID = Uri.UnescapeDataString(value);
        }
    }

    /// <summary>
    /// Education list page constructor
    /// </summary>
    public MyEducationsPage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _educationListView = new EducationsView(this, null) { Margin = new Thickness(0) };
        Content = _educationListView;
    }

    /// <summary>
    /// Educations page on appearing event
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        _educationListView.Parameters = AddParameters(
            CreateParameter(nameof(BaseDTO.RecordCount), _recordCount),
            CreateParameter(nameof(ContentPageModel.EducationCategoryID), _category),
            CreateParameter(nameof(ContentPageModel.PageID), _educationID));
        await _educationListView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Educations page on appearing event
    /// </summary>
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _educationListView.UnloadUIAsync().ConfigureAwait(false);
    }

    public void DataMethod()
    {
        Task.Run(async () => { await _educationListView.LoadUIAsync(false).ConfigureAwait(true); });
    }
}