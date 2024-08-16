using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class AppointmentViewPopupPage : BasePopupPage
{
    private readonly string _appointmentID;
    private readonly AppointmentView _appointmentView;

    /// <summary>
    /// on click event of Edit Button
    /// </summary>
    public event EventHandler<EventArgs> OnEditButtonClicked;

    /// <summary>
    /// View to display Individual Appointment as Popup Based on appointmentID
    /// </summary>
    /// <param name="appointmentID">ID of appointment</param>
    public AppointmentViewPopupPage(string appointmentID) : base(new BasePage())
    {
        _appointmentID = appointmentID;
        _appointmentView = new AppointmentView(_parentPage, null);
        _parentPage.PageLayout.Add(_appointmentView, 0, 0);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _appointmentView.LoadUIDataAsync(true, _appointmentID).ConfigureAwait(true);
        _parentPage.PageData = _appointmentView.ParentPage.PageData;
        SetTitle(_appointmentView._appointmentHeader);
        if (_appointmentView.CheckAppointmentPermission(AppPermissions.AppointmentAddEdit.ToString()))
        {
            DisplayRightHeader(ResourceConstants.R_EDIT_ACTION_KEY);
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        _appointmentView.OnVideoCallJoin += OnStartedVideoCall;
        AppHelper.ShowBusyIndicator = false;
    }

    protected override void OnDisappearing()
    {
        if (_appointmentView.IsEditClicked)
        {
            OnEditButtonClicked.Invoke(_appointmentID, new EventArgs());
        }
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        base.OnDisappearing();
        _appointmentView.UnLoadUIData();
    }

    protected async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    protected async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            OnRightHeaderClickedEvent -= OnRightHeaderClicked;
            _appointmentView.IsEditClicked = true;
            await ClosePopupAsync().ConfigureAwait(true);
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
    }

    private async void OnStartedVideoCall(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }
}
