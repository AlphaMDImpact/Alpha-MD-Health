namespace AlphaMDHealth.WebClient;

public partial class LandingPage : BasePage
{
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _isDataFetched = true;
    }
}