using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(CaregiverPage))]
[QueryProperty(nameof(CaregiverModel.PatientCareGiverID), "id")]
public class CaregiverPage : BasePage
{
    private readonly CaregiverView _caregiverView;
    private int _patientCaregiverID;

    /// <summary>
    /// PatientCaregiverid
    /// </summary>
    public string PatientCareGiverID
    {
        get { return _patientCaregiverID.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _patientCaregiverID = Convert.ToInt32(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    public CaregiverPage()
    {
        _caregiverView = new CaregiverView(this, null);
        SetPageContent(true);
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _caregiverView.Parameters = AddParameters(CreateParameter(nameof(CaregiverModel.PatientCareGiverID), PatientCareGiverID));
        await _caregiverView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        await _caregiverView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }
}
