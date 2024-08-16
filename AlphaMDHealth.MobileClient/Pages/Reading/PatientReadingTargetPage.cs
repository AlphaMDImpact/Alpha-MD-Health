using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient Reading TargetPage
/// </summary>
public class PatientReadingTargetPage : BasePopupPage
{
    private short _readingID;
    private readonly PatientReadingTargetView _patientReadingTargetView;

    /// <summary>
    /// on click event of Send Button
    /// </summary>
    public event EventHandler<EventArgs> OnSaveButtonClicked;

    /// <summary>
    /// Reading ID
    /// </summary>
    public short ReadingID { get; set; }

    /// <summary>
    /// constructor for PatientReading Target Page
    /// </summary>
    /// <param name="readingID"></param>
    public PatientReadingTargetPage(short readingID)
    {
        _readingID = readingID;
        //todo: CloseWhenBackgroundIsClicked = false;
        _patientReadingTargetView = new PatientReadingTargetView(_parentPage, null);
    }

    /// <summary>
    /// OnAppearing
    /// </summary>
    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _patientReadingTargetView.Parameters = _parentPage.AddParameters(_parentPage.CreateParameter(nameof(PatientReadingTargetPage.ReadingID), _readingID.ToString()));
        await _patientReadingTargetView.LoadUIAsync(true).ConfigureAwait(true);
        SetTitle(_parentPage.GetResourceValueByKey(ResourceConstants.R_SET_TARGET_KEY));
        DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// OnDisappearing
    /// </summary>
    protected override async void OnDisappearing()
    {
        await _patientReadingTargetView.UnloadUIAsync().ConfigureAwait(true);
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        base.OnDisappearing();
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        if (await _patientReadingTargetView.OnSaveButtonClickedAsync().ConfigureAwait(true))
        {
            OnSaveButtonClicked.Invoke(_patientReadingTargetView._readingData, new EventArgs());
            await ClosePopupAsync().ConfigureAwait(true);
        }
    }
}