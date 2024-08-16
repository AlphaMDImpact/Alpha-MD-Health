using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Scheduler;

namespace AlphaMDHealth.MobileClient;

public class CalenderDemoPage : BasePage
{
    private readonly AmhCalendarControl _myCalandarControl;
    private readonly AmhButtonControl _backButton;

    public CalenderDemoPage() : base(PageLayoutType.MastersContentPageLayout, false)
    {
        //SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.FillAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        AddRowColumnDefinition(new GridLength(1, GridUnitType.Auto), 2, true);

        _myCalandarControl = new AmhCalendarControl(FieldTypes.CalendarControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY,
            HeightRequest = -1,
        };
        AddView(_myCalandarControl);

        _backButton = new AmhButtonControl(FieldTypes.DeleteButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_backButton);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await new AuthService(App._essentials).GetAccountDataAsync(PageData).ConfigureAwait(true);
        var demoService = new ControlDemoService(App._essentials);
        demoService.GetControlDemoPageResources(PageData);

        var CustomeEvents = demoService.GetCalanderData();
        _myCalandarControl.OnValueChanged += OnAppointmentTapped;
        _myCalandarControl.PageResources = PageData;
        _myCalandarControl.Options = CustomeEvents;

        _backButton.PageResources = PageData;
        _backButton.OnValueChanged += OnBackButtonClicked;

        AppHelper.ShowBusyIndicator = false;
    }

    protected override void OnDisappearing()
    {
        _myCalandarControl.OnValueChanged -= OnAppointmentTapped;
        _backButton.OnValueChanged -= OnBackButtonClicked;
        base.OnDisappearing();
    }

    private void OnAppointmentTapped(object sender, EventArgs e)
    {
        if (sender != null && sender is AppointmentInfo)
        {
            AppointmentItem appointment = (sender as AppointmentInfo).Appointment;
            int id = Convert.ToInt32(appointment.Id);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await App.Current.MainPage.DisplayAlert("Date Selection", $"You selected a date with ID: {id}", "OK");
            });
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await App.Current.MainPage.DisplayAlert("Date Selection", $"No appointment associated with the selected date.", "OK");
            });
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new ControlDemoPage()).ConfigureAwait(false);
    }

    private void AddView(View view)
    {
        int index = PageLayout.Children?.Count() ?? 0;
        PageLayout.Add(view, 0, index);
    }
}