using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class EducationPreviewPopupPage : BasePopupPage
{
    private readonly StaticMessageView _educationView;
    private readonly long _contentPageID;
    private readonly double padding = (double)App.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING] / 2;

    public EducationPreviewPopupPage(long contentPageID)
    {
        _contentPageID = contentPageID;
        _parentPage.AddScrollView = false;
        _educationView = new StaticMessageView(_parentPage, null)
        {
            VerticalOptions = LayoutOptions.StartAndExpand,
            LoadWebViewInPopup = true,
            Margin = new Thickness(padding, 0, padding, padding)
        };
        _parentPage.PageLayout.Add(_educationView, 0, 0);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _educationView.Parameters = _parentPage.AddParameters(
            _parentPage.CreateParameter(nameof(StaticMessageView.Key), _contentPageID.ToString(CultureInfo.InvariantCulture)),
            _parentPage.CreateParameter(nameof(StaticMessageView.MessageType), PageType.ContentPage.ToString()),
            _parentPage.CreateParameter(nameof(StaticMessageView.GroupName), GroupConstants.RS_MENU_PAGE_GROUP)
        );
        await _educationView.LoadUIAsync(false).ConfigureAwait(true);
        _parentPage.PageLayout.Add(_educationView, 0, 0);
        _parentPage.PageData = _educationView.ParentPage.PageData;
        if (_educationView.ParentPage.PageData.ErrCode == ErrorCode.OK)
        {
            SetTitle(_educationView.ParentPage.PageData.Resources[0].KeyDescription);
            DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
            OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        }
        AppHelper.ShowBusyIndicator = false;
    }

    protected override void OnDisappearing()
    {
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        base.OnDisappearing();
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
    }
}