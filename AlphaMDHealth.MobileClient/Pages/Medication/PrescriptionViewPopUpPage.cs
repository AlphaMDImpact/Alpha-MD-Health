using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Prescription View Popup Page
/// </summary>
public class PrescriptionViewPopUpPage : BasePopupPage
{
    private readonly PrescriptionView _prescriptionView;
    private Guid _patientMedicationID = Guid.Empty;

    /// <summary>
    /// Default constructor of PatientMedicationPopupPage
    /// </summary>
    public PrescriptionViewPopUpPage(Guid patientMedicationID) : base(new BasePage())
    {
        _patientMedicationID = patientMedicationID;
        _prescriptionView = new PrescriptionView(_parentPage, _parentPage.AddParameters(
            _parentPage.CreateParameter(nameof(PatientMedicationModel.PatientMedicationID),
            Convert.ToString(_patientMedicationID, CultureInfo.InvariantCulture))));
        ScrollView content= new ScrollView { Content = _prescriptionView };
        _parentPage.PageLayout.Add(content, 0, 0);
    }

    /// <summary>
    ///  Even which will trigger when page is becoming visible
    /// </summary>
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        _prescriptionView.Parameters = _parentPage.AddParameters(
            _parentPage.CreateParameter(nameof(PatientMedicationModel.PatientMedicationID),
            Convert.ToString(_patientMedicationID, CultureInfo.InvariantCulture))
        );
        await _prescriptionView.LoadUIAsync(false).ConfigureAwait(true);
        _parentPage.PageData = _prescriptionView.ParentPage.PageData;
        SetTitle(_parentPage.GetFeatureValueByCode(AppPermissions.PrescriptionView.ToString()));
        if (_parentPage.CheckFeaturePermissionByCode(AppPermissions.PrescriptionShare.ToString()))
        {
            DisplayRightHeader(ResourceConstants.R_MENU_ACTION_SHARE_KEY);
            OnRightHeaderClickedEvent += PrescriptionViewPopUpPage_OnRightHeaderClickedEvent;
        }
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += PrescriptionViewPopUpPage_OnLeftHeaderClickedEvent;
    }

    private async void PrescriptionViewPopUpPage_OnLeftHeaderClickedEvent(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void PrescriptionViewPopUpPage_OnRightHeaderClickedEvent(object sender, EventArgs e)
    {
        await _prescriptionView.ShareButtonClicked().ConfigureAwait(true);
    }

    /// <summary>
    /// Even which will trigger when page disappears
    /// </summary>
    protected override async void OnDisappearing()
    {
        OnLeftHeaderClickedEvent -= PrescriptionViewPopUpPage_OnLeftHeaderClickedEvent;
        OnRightHeaderClickedEvent -= PrescriptionViewPopUpPage_OnRightHeaderClickedEvent;
        await _prescriptionView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }
}