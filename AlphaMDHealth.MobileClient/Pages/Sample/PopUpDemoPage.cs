using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class PopUpDemoPage : BasePopupPage
{
	public PopUpDemoPage(BasePage page, object parameters) : base(new BasePage())
    { 
        var demoContent = new VerticalStackLayout
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            Children = {
                new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Welcome to .NET MAUI!"
                }
            },
            BackgroundColor = Color.FromArgb("#ff8080")

        };
        _parentPage.PageLayout.Add(demoContent, 0, 0);
    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        SetTitle("PopUpDemoPage");
        await new AuthService(App._essentials).GetAccountDataAsync(_parentPage.PageData).ConfigureAwait(true);
        var demoService = new ControlDemoService(App._essentials);
        demoService.GetControlDemoPageResources(_parentPage.PageData);
        if(DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
            DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
            OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        }
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        //if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true) && await _patientTrackerDetailsView.SaveTrackerAsync().ConfigureAwait(true))
        //{
            await ClosePopupAsync().ConfigureAwait(true);
        //}
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
    }
}