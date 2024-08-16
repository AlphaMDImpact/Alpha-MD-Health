using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient Medication Popup Page
/// </summary>
public class PatientMedicationPopupPage : BasePopupPage
{
    private readonly string _medicationID;
    internal PatientMedicationView _medicationView;

    /// <summary>
    /// event to display operation status
    /// </summary>
    public event EventHandler<EventArgs> OnDisplayStatus;

    /// <summary>
    /// Default constructor of PatientMedicationPopupPage
    /// </summary>
    public PatientMedicationPopupPage(string medicationId) : base(new BasePage())
    {
        _medicationID = medicationId;
        //todo: CloseWhenBackgroundIsClicked = false;
        _medicationView = new PatientMedicationView(_parentPage, null);
    }

    /// <summary>
    ///  Even which will trigger when page is becoming visible
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _medicationView.Parameters = _parentPage.AddParameters(_parentPage.CreateParameter(nameof(PatientMedicationModel.PatientMedicationID), _medicationID));
        await _medicationView.LoadUIAsync(false).ConfigureAwait(true);
        _parentPage.PageData = _medicationView.ParentPage.PageData;
        SetTitle(_parentPage.GetFeatureValueByCode(AppPermissions.PatientMedicationAddEdit.ToString()));
        if (!_medicationView.IsReadOnly && _parentPage.CheckFeaturePermissionByCode(AppPermissions.PatientMedicationAddEdit.ToString()))
        {
            DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        _medicationView.OnDisplayStatus += DisplayErrorMessage;
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Even which will trigger when page disappears
    /// </summary>
    protected override async void OnDisappearing()
    {
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        _medicationView.OnDisplayStatus -= DisplayErrorMessage;
        await _medicationView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true) && await _medicationView.SaveMedicationAsync().ConfigureAwait(true))
        {
            await ClosePopupAsync().ConfigureAwait(true);
        }
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
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