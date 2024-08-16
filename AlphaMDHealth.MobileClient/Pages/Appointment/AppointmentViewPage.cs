using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(AppointmentViewPage))]
[QueryProperty("AppointmentID", "appointmentID")]
public class AppointmentViewPage : BasePage
{
    private readonly AppointmentView _appointmentView;
    private string _appointmentID;

    public string AppointmentID
    {
        get
        {
            return _appointmentID;
        }
        set => _appointmentID = Uri.UnescapeDataString(value);
    }

    public AppointmentViewPage()
    {
        _appointmentView = new AppointmentView(this, null);
        PageLayout.Add(_appointmentView);
        SetPageContent(false);
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        await _appointmentView.LoadUIDataAsync(false, _appointmentID).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _appointmentView.UnLoadUIData();
    }
}
