using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Reading Popup page
/// </summary>
public class ReadingPopupPage : BasePopupPage
{
    private readonly PatientReadingView _readingView;

    /// <summary>
    /// on click event of Send Button
    /// </summary>
    public event EventHandler<EventArgs> OnSaveButtonClicked;

    /// <summary>
    /// Default constructor 
    /// </summary>
    /// <param name="readingCategoryID">reading Category ID</param>
    /// <param name="readingID">reading ID</param>
    /// <param name="patientReadingID">patient Reading ID</param>
    /// <param name="selectedUserID">selected User ID</param>
    public ReadingPopupPage(string readingCategoryID, short readingID, string patientReadingID, string selectedUserID) : base(new BasePage(PageLayoutType.EndToEndPageLayout, false))
    {
        //todo: CloseWhenBackgroundIsClicked = false;
        Padding = _parentPage.GetPopUpPagePadding(PopUpPageType.Long);
        _readingView = new PatientReadingView(_parentPage, _parentPage.AddParameters(
            _parentPage.CreateParameter(nameof(PatientReadingDTO.ReadingCategoryID), readingCategoryID),
            _parentPage.CreateParameter(nameof(PatientReadingUIModel.PatientTaskID), Constants.NUMBER_ZERO),
            _parentPage.CreateParameter(nameof(PatientReadingUIModel.ReadingID), readingID.ToString()),
            _parentPage.CreateParameter(nameof(PatientReadingUIModel.PatientReadingID), patientReadingID),
            _parentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), selectedUserID)));
        _parentPage.PageLayout.Margin = new Thickness((double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], 0);
        _parentPage.PageLayout.Add(_readingView, 0, 0);
    }

    /// <summary>
    /// Appearing page
    /// </summary>
    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        base.OnAppearing();
        _readingView.OnListRefresh += OnSaveButtonClicked;
        await _readingView.LoadUIAsync(false).ConfigureAwait(true);
        _parentPage.PageData = _readingView.ParentPage.PageData;
        SetTitle(_readingView.GetPageTitle());
        if (!_readingView.IsViewOnly)
        {
            DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
        }
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Disappearing page
    /// </summary>
    protected override async void OnDisappearing()
    {
        _readingView.OnListRefresh -= OnSaveButtonClicked;
        await _readingView.UnloadUIAsync().ConfigureAwait(true);
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
        AppHelper.ShowBusyIndicator = true;
        var result = await _readingView.OnSaveButtonClickedAsync().ConfigureAwait(true);
        if (result.Item1)
        {
            await Task.Delay(300).ConfigureAwait(true);
            OnSaveButtonClicked.Invoke(_readingView._readingData, new EventArgs());
        }
        AppHelper.ShowBusyIndicator = false;
    }
}