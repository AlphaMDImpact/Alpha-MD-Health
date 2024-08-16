using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient Tracker Popup Page
/// </summary>
public class PatientTrackerPopUpPage : BasePopupPage
{
    private readonly Guid _patientTrackerID = Guid.Empty;
    internal PatientTrackerAddEdit _patientTrackerView;

    /// <summary>
    /// event to display operation status
    /// </summary>
    public event EventHandler<EventArgs> OnDisplayStatus;
    public event EventHandler<EventArgs> OnSaveButtonClicked;


    /// <summary>
    /// Default constructor of PatientMedicationPopupPage
    /// </summary>
    public PatientTrackerPopUpPage(Guid patienttrackerId) : base(new BasePage())
    {
        _patientTrackerID = patienttrackerId;
        //todo: CloseWhenBackgroundIsClicked = false;
        _patientTrackerView = new PatientTrackerAddEdit(_parentPage, null);
    }

    /// <summary>
    ///  Even which will trigger when page is becoming visible
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _patientTrackerView.Parameters = _parentPage.AddParameters(_parentPage.CreateParameter(nameof(PatientTrackersModel.PatientTrackerID), _patientTrackerID.ToString()));
        await _patientTrackerView.LoadUIAsync(false).ConfigureAwait(true);
        _parentPage.PageData = _patientTrackerView.ParentPage.PageData;
        _patientTrackerView.OnSaveSuccess += _patientTrackerView_OnSaveSuccess;
        _patientTrackerView.OnCloseSuccess += _patientTrackerView_OnCloseSuccess;
        SetTitle(_parentPage.GetFeatureValueByCode(AppPermissions.PatientTrackerAddEdit.ToString()));
        if (!_patientTrackerView.IsReadOnly && _parentPage.CheckFeaturePermissionByCode(AppPermissions.PatientTrackerAddEdit.ToString()))
        {
            DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        _patientTrackerView.OnSaveSuccess += DisplayErrorMessage;
        AppHelper.ShowBusyIndicator = false;
    }

    private void _patientTrackerView_OnCloseSuccess(object sender, EventArgs e)
    {
        OnSaveButtonClicked?.Invoke(default, new EventArgs());
    }

    private void _patientTrackerView_OnSaveSuccess(object sender, EventArgs e)
    {
        OnSaveButtonClicked?.Invoke((string)sender, new EventArgs());
    }

    /// <summary>
    /// Even which will trigger when page disappears
    /// </summary>
    protected override async void OnDisappearing()
    {
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        _patientTrackerView.OnSaveSuccess -= DisplayErrorMessage;
        await _patientTrackerView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await _patientTrackerView.InvokeAndClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        await _patientTrackerView.SaveTrackerAsync().ConfigureAwait(true);
    }

    private void DisplayErrorMessage(object sender, EventArgs e)
    {
        if (e == EventArgs.Empty)
        {
            _parentPage.DisplayOperationStatus(((string)sender), false);
        }
        else
        {
            OnDisplayStatus?.Invoke(sender, e);
        }
    }
}