using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(CaregiversPage))]
[QueryProperty(nameof(RecordCount), "recordCount")]
[QueryProperty(nameof(CaregiverModel.PatientCareGiverID), "id")]
public class CaregiversPage : BasePage
{
    private readonly CaregiversView _caregiversListView;
    private int _recordCount;
    private int _patientCaregiverID;

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


    /// <summary>
    /// Patient caregiver ID
    /// </summary>
    public string PatientCareGiverID
    {
        get
        {
            return _patientCaregiverID.ToString(CultureInfo.InstalledUICulture);
        }
        set
        {
            _patientCaregiverID = Convert.ToInt32(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    public CaregiversPage() : base(PageLayoutType.EndToEndPageLayout, false)
    {
        //IsSplitView = MobileConstants.IsTablet;
        _caregiversListView = new CaregiversView(this, null) { Margin = new Thickness(0) };
        PageLayout.Add(_caregiversListView, 0, 0);
        PageLayout.Padding = 0;
        SetPageContent(false);
        
    }

    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _caregiversListView.Parameters = AddParameters(
            CreateParameter(nameof(BaseDTO.RecordCount), _recordCount.ToString(CultureInfo.InvariantCulture)),
            CreateParameter(nameof(CaregiverModel.PatientCareGiverID), PatientCareGiverID)
        );
        await _caregiversListView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        await _caregiversListView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }
}