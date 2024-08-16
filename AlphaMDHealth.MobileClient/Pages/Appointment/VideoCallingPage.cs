using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(VideoCallingPage))]
[QueryProperty(nameof(AppointmentID), "id")]
public class VideoCallingPage : BasePage
{
    private readonly AppointmentDTO _appointmentData = new AppointmentDTO { Appointment = new AppointmentModel() };
    private readonly VideoCallingControl _videoCallingControl;
    private bool _isPagePopped;

    /// <summary>
    /// Appointment ID
    /// </summary>
    public string AppointmentID
    {
        get { return _appointmentData.Appointment.AppointmentID.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _appointmentData.Appointment.AppointmentID = Convert.ToInt64(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }
    public VideoCallingPage(string appointmentID) : this()
    {
        _appointmentData.Appointment.AppointmentID = Convert.ToInt64(appointmentID, CultureInfo.InvariantCulture);
    }

    public VideoCallingPage()
    {
        HideFooter(true);
        //todo:
        //_appointmentData.Appointment= (AppointmentModel)Application.Current.Properties[StorageConstants.PR_SELECTED_VIDEO_SERVICE_KEY];
        _videoCallingControl = new VideoCallingControl(_appointmentData.Appointment.ServiceType);
        Content = _videoCallingControl;
    }


    protected async override void OnAppearing()
    {
        base.OnAppearing();
        DeviceDisplay.KeepScreenOn = true;
        Shell.SetNavBarIsVisible(this, false);
        await Task.Delay(500).ConfigureAwait(true);
        //todo:
        //await _videoCallingControl.VideoView.OnLoadAsync().ConfigureAwait(true);
        _videoCallingControl.OnDisconnectCall += OnCallDisconnect;
        await _videoCallingControl.LoadUI(_appointmentData.Appointment).ConfigureAwait(true);
    }
    
    protected override bool OnBackButtonPressed()
    {
        if (!_isPagePopped)
        {
            _isPagePopped = true;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _videoCallingControl.DisconnectCallAsync().ConfigureAwait(true);
            });
            return true;
        }
        return false;
    }

    protected override async void OnDisappearing()
    {
        DeviceDisplay.KeepScreenOn = false;
        _videoCallingControl.OnDisconnectCall -= OnCallDisconnect;
        await _videoCallingControl.UnloadUIAsync().ConfigureAwait(true);
        Shell.SetNavBarIsVisible(this, true);
        HideFooter(false);
        base.OnDisappearing();
    }

    private async void OnCallDisconnect(object sender, EventArgs e)
    {
        AppointmentDTO appointmentDTO = new AppointmentDTO
        {
            Appointment = new AppointmentModel
            {
                AppointmentID = _appointmentData.Appointment.AppointmentID,
                AppointmentStatusID = ResourceConstants.R_COMPLETED_STATUS_KEY,
                AccountID = App._essentials.GetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, (long)0)
            }
        };
        await new AppointmentService(App._essentials).UpdateAppointmentAsync(appointmentDTO, new CancellationToken()).ConfigureAwait(true);
        _isPagePopped = true;
        await ShellMasterPage.CurrentShell.CurrentPage.PopPageAsync(true).ConfigureAwait(false);
    }
}

