using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientTrackerDetailPopupPage : BasePopupPage
{
    private readonly PatientTrackerDetailView _patientTrackerDetailsView;
    private Guid _patienttrackerID = Guid.Empty;
    public event EventHandler<EventArgs> OnSaveButtonClicked;


    /// <summary>
    /// User Id of the selected conversation
    /// </summary>
    public string PatientTrackerID
    {
        get { return Convert.ToString(_patienttrackerID, CultureInfo.InvariantCulture); }
        set { _patienttrackerID = string.IsNullOrWhiteSpace(value) ? Guid.Empty : new Guid(Convert.ToString(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture)); }
    }

    public PatientTrackerDetailPopupPage(Guid patienttrackerId) : base(new BasePage())
    {
        _patienttrackerID = patienttrackerId;
        _patientTrackerDetailsView = new PatientTrackerDetailView(_parentPage, null);
    }

    /// <summary>
    ///  Even which will trigger when page is becoming visible
    /// </summary>
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        _patientTrackerDetailsView.Parameters = _parentPage.AddParameters(
            _parentPage.CreateParameter(nameof(PatientTrackersModel.PatientTrackerID),
            Convert.ToString(_patienttrackerID, CultureInfo.InvariantCulture))
        );
        await _patientTrackerDetailsView.LoadUIAsync(false).ConfigureAwait(true);
        _parentPage.PageData = _patientTrackerDetailsView.ParentPage.PageData;
        _patientTrackerDetailsView.OnSaveSuccess += _patientTrackerDetailView_OnSaveSuccess; ;
        SetTitle(_parentPage.GetFeatureValueByCode(AppPermissions.PatientTrackerDetailView.ToString()));
        if (_parentPage.CheckFeaturePermissionByCode(AppPermissions.PatientTrackerDetailView.ToString()))
        {
            DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        _patientTrackerDetailsView.OnSaveSuccess += DisplayErrorMessage;
    }

    private void _patientTrackerDetailView_OnSaveSuccess(object sender, EventArgs e)
    {
        OnSaveButtonClicked?.Invoke((string)sender, new EventArgs());
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true) && await _patientTrackerDetailsView.SaveTrackerAsync().ConfigureAwait(true))
        {
            await ClosePopupAsync().ConfigureAwait(true);
        }
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
    }


    protected override async void OnDisappearing()
    {
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        _patientTrackerDetailsView.OnSaveSuccess -= DisplayErrorMessage;
        await _patientTrackerDetailsView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }

    private void DisplayErrorMessage(object sender, EventArgs e)
    {
        if (e == EventArgs.Empty)
        {
            _parentPage.DisplayOperationStatus(((string)sender), false);
        }
        else
        {
            OnSaveButtonClicked?.Invoke(sender, e);
        }
    }
}